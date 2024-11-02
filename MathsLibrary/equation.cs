using MathsLibrary;

namespace MathsLibrary
{
    public class Equation : ICloneable, ILatexPrintable
    {
        public Expression left;
        public Expression right;
        public Equation(Expression left, Expression right)
        {
            this.left = left;
            this.right = right;
        }
        public Equation(string equation)
        {
            string[] split = equation.Split('=');
            left = new Expression(split[0], "infix");
            right = new Expression(split[1], "infix");
        }
        public object Clone()
        {
            return new Equation((Expression)left.Clone(), (Expression)right.Clone());
        }
        public override bool Equals(object other)
        {
            if (other is not Equation)
            {
                return false;
            }
            Expression otherLeft = ((Equation)other).left;
            Expression otherRight = ((Equation)other).right;

            return (left.Equals(otherLeft) && right.Equals(otherRight));
        }
        public void ExpandAndSimplify()
        {
            left.ExpandAndSimplify();
            right.ExpandAndSimplify();

            if (right != 0)
            {
                left = new Expression(Utils.operators["-"], new List<Expression> { left, right });
                right = new Expression(0);

                left.ExpandAndSimplify();
            }            
        }
        public void Simplify()
        {
            left.Simplify();
            right.Simplify();

            if (right != 0)
            {
                left = new Expression(Utils.operators["-"], new List<Expression> { left, right });
                right = new Expression(0);

                left.Simplify();
            }
        }
        public void Substitute(string variable, double subIn)
        {
            left.Substitute(variable, subIn);
            right.Substitute(variable, subIn);
        }
        public void Substitute(string variable, string subIn)
        {
            left.Substitute(variable, subIn);
            right.Substitute(variable, subIn);
        }
        public void Substitute(string variable, Expression subIn)
        {
            left.Substitute(variable, subIn);
            right.Substitute(variable, subIn);
        }
        public void Substitute(Expression oldExpression, Expression newExpression)
        {
            left.Substitute(oldExpression, newExpression);
            right.Substitute(oldExpression, newExpression);
        }
        public bool ContainsVariable(string variable)
        {
            return (left.ContainsVariable(variable) || right.ContainsVariable(variable));
        }
        public bool ContainsVariable(Expression variable)
        {
            return (left.ContainsVariable(variable) || right.ContainsVariable(variable));
        }        
        public static List<SolutionSet> SolveSimultaneous(List<Equation> equations, List<string> variables)
        {            
           List<SolutionSet> solutionSets = new List<SolutionSet>();
            for (int i = 0; i < equations.Count; i++)
            {
                if (equations[i].right != 0)
                {
                    equations[i].left -= equations[i].right;
                    equations[i].right = new Expression(0);
                }                
            }

            for (int v = 0; v < variables.Count; v++)
            {
                int i = 0;
                for (i = 0; i < equations.Count; i++)
                {
                    if (equations[i].left.ContainsVariable(variables[v]))
                    {
                        List<Equation> thisVarSolutions;
                        try
                        {
                            thisVarSolutions = equations[i].Solve(variables[v]);                            
                        }
                        catch (Exception exception)
                        {
                            throw exception;
                        }
                        if (thisVarSolutions.Count == 1)
                        {
                            equations[i] = thisVarSolutions[0];
                            for (int j = 0; j < equations.Count; j++)
                            {
                                if (j != i)
                                {
                                    equations[j].Substitute(variables[v], thisVarSolutions[0].right);
                                    equations[j].left.ExpandAndSimplify();
                                    equations[j].right.ExpandAndSimplify();
                                }
                            }
                        }
                        else if (thisVarSolutions.Count > 1)
                        {
                            for (int s = 0; s < thisVarSolutions.Count; s++)
                            {
                                List<Equation> subEquations = new List<Equation>();
                                for (int subIndex = 0; subIndex < equations.Count; subIndex++)
                                {
                                    if (subIndex == i)
                                    {
                                        subEquations.Add(thisVarSolutions[s]);
                                    }
                                    else
                                    {
                                        subEquations.Add((Equation)equations[subIndex].Clone());
                                    }
                                }
                                List<SolutionSet> subSolutions = SolveSimultaneous(subEquations, variables);  
                                
                                
                                
                                foreach (SolutionSet subSolution in subSolutions)
                                {
                                    if (!solutionSets.Contains(subSolution))
                                    {
                                        solutionSets.Add(subSolution);
                                    }
                                }
                            }        
                            
                            return solutionSets;
                        }
                        else
                        {
                            throw new Exception("error");
                        }
                        break;
                    }
                }
                if (i == equations.Count)
                {
                    //variable is not in any of the equations and therefore has no restrictions
                    equations.Add(new Equation(new Expression(variables[v]), new Expression("ℝ")));
                    continue;
                }
            }
            SolutionSet solutionSet = new SolutionSet(new List<Equation>());
            for (int i = 0; i < equations.Count; i++)
            {
                for (int j = 0; j < variables.Count; j++)
                {
                    if (equations[i].left.ToString() == variables[j])
                    {
                        solutionSet.equations.Add(equations[i]);
                        break;
                    }
                }
            }
            if (!solutionSets.Contains(solutionSet))
            {
                solutionSets.Add(solutionSet);
            }
            return solutionSets;
        }
        public List<Equation> Solve(Expression var)
        {
            if (!ContainsVariable(var))
            {
                if (left.isNumeric && right.isNumeric)
                {
                    left.ExpandAndSimplify();
                    right.ExpandAndSimplify();
                    if (left != right)
                    {
                        return new List<Equation>();
                    }
                }
                return new List<Equation>() { new Equation(var, new Expression("ℝ")) };
            }
            if (right != 0)
            {
                left = left - right;
                right = new Expression(0);
            }            
            left.Factorise();
            if (left.IsOp("*"))
            {
                List<Equation> solutions = new List<Equation>();
                for (int i = 0; i < left.children.Count; i++)
                {
                    Equation eq = new Equation(left.children[i].Clone() as Expression, right.Clone() as Expression);
                    if (eq.ContainsVariable(var))
                    {
                        List<Equation> solutionPart = eq.Solve(var);
                        solutions.AddRange(solutionPart);
                    }
                }
                if (solutions.Count != 0)
                {
                    return solutions;
                }
            }
            else if (left.IsOp("/"))
            {
                Equation asymptoteEquation = new Equation(left.children[1].Clone() as Expression, new Expression(0));
                Equation topEquation = new Equation(left.children[0].Clone() as Expression, right.Clone() as Expression);

                List<Equation> asymptotes = asymptoteEquation.Solve(var);
                List<Equation> topSolution = topEquation.Solve(var);
                topSolution.RemoveAll(x => asymptotes.Any(y => x.Equals(y)));
                return topSolution;
            }


            Simplify();
            Equation copy;
            do
            {
                copy = Clone() as Equation;
                left.PartialFactorise(var);

                if (left.isLeaf)
                {
                    if (left.Equals(var) && !right.ContainsVariable(var))
                    {
                        right.ExpandAndSimplify();
                        return new List<Equation>() { this };
                    }
                    else
                    {
                        throw new Exception("error has occured");
                    }
                }                
                else if (left.op.Commutative)
                {
                    for (int i = 0; i < left.children.Count; i++)
                    {
                        if (!left.children[i].ContainsVariable(var))
                        {
                            right = new Expression(left.op.Inverse, new List<Expression> { right, left.children[i] });
                            right.Simplify();
                            left.children.RemoveAt(i);
                            i--;
                        }
                    }
                    if (left.children.Count == 1)
                    {
                        left = left.children[0];
                    }
                }
                else if (left.op.Symbol == "^")
                {
                    //eg: x^2 = 4, (2x)^2=4
                    if (left.children[0].ContainsVariable(var) && !left.children[1].ContainsVariable(var))
                    {
                        if (left.children[1].isLeaf)
                        {
                            //eg: x^4 = 1+i
                            if (left.children[1].isNatural)
                            {
                                int degree = (int)left.children[1].ToDouble();
                                List<Expression> rootsOfUnity = new List<Expression>();
                                List<Equation> equations = new List<Equation>();
                                List<Equation> solutions = new List<Equation>();

                                Expression pi = new Expression("π", "infix");
                                Expression i = new Expression("i", "infix");
                                for (int k = 0; k < degree; k++)
                                {
                                    Expression argument = 2 * pi * k / degree;
                                    if (k > degree/2)
                                    {
                                        argument = argument-(2 * pi);                                        
                                    }
                                    argument.Simplify();
                                    Expression rootOfUnity = Expression.Cos(argument) + i * Expression.Sin(argument);
                                    rootOfUnity.ExpandAndSimplify();
                                    rootsOfUnity.Add(rootOfUnity);
                                }
                                for (int k = 0; k < degree; k++)
                                {
                                    equations.Add(new Equation(left.children[0], rootsOfUnity[k] * (right^(new Expression(1)/degree))));
                                }
                                for (int k = 0; k < degree; k++)
                                {
                                    List<Equation> subSolutions = equations[k].Solve(var);
                                    foreach (Equation subSolution in subSolutions)
                                    {
                                        bool inSolutions = false;
                                        for (int solutionNo = 0; solutionNo < solutions.Count; solutionNo++)
                                        {
                                            if (solutions[solutionNo].Equals(subSolution))
                                            {
                                                inSolutions = true;
                                                break;
                                            }
                                        }
                                        if (!inSolutions)
                                        {
                                            solutions.Add(subSolution);
                                        }
                                    }
                                }
                                return solutions;
                            }
                            // or x^a = 4 -> x = 4^(1/a)
                            else
                            {
                                right = right ^ (1 / left.children[1]);
                                left = left.children[0];
                            }
                        }
                        //eg: x^(3/2) = 4 -> x^3 = 4                        
                        else if (left.children[1].IsOp("/"))
                        {
                            right = right ^ left.children[1].children[1];
                            left = left.children[0] ^ left.children[1].children[0];
                        }
                        //eg: x^sin(a) -> x = 4^(1/sin(a))
                        else
                        {
                            right = right ^ (1 / left.children[1]);
                            left = left.children[0];
                        }                        
                        
                    }
                    //eg: 2^sin(x) = 3 -> sin(x) = log(2,3)           
                    else if (!left.children[0].ContainsVariable(var) && left.children[1].ContainsVariable(var))
                    {

                        right = Expression.Log(left.children[0], right);
                        left = left.children[1];
                    }                    
                }               
                else if (left.op.Symbol == "log")
                {
                    if (left.children[0].ContainsVariable(var) && !left.children[1].ContainsVariable(var))
                    {
                        left.Simplify();
                    }
                    //eg 
                    else if (!left.children[0].ContainsVariable(var) && left.children[1].ContainsVariable(var))
                    {
                        right = left.children[0] ^ right;
                        left = left.children[1];
                    }                    
                }
                else if (left.op.Inverse == null)
                {
                    break;
                    //throw new Exception("no inverse");
                }
                else if (Utils.functions.ContainsKey(left.op.Symbol))
                {
                    if (left.op is ITrigFunction)
                    {
                        List<Equation> inverses = (left.op as ITrigFunction).GetInverse(this);
                        SolutionSet toReturn = new SolutionSet(new List<Equation>());

                        foreach (Equation inverse in inverses)
                        {
                            List<Equation> solutions = inverse.Solve(var);
                            foreach (Equation solution in solutions)
                            {
                                if (!toReturn.Contains(solution))
                                {
                                    toReturn.equations.Add(solution);
                                }
                            }
                        }
                        return toReturn.equations;
                    }

                    right = new Expression(left.op.Inverse, new List<Expression> { right });
                    right.Simplify();
                    left = left.children[0];
                }
                else if (left.children[0].ContainsVariable(var) && !left.children[1].ContainsVariable(var))
                {
                    //eg x+2 = 3 -> x = 3-2
                    right = new Expression(left.op.Inverse, new List<Expression> { right, left.children[1] });
                    right.Simplify();
                    left = left.children[0];
                }
                else if (!left.children[0].ContainsVariable(var) && left.children[1].ContainsVariable(var))
                {
                    //eg 2+x = 3 -> 2 = 3-x -> 3-x = 2
                    right = new Expression(left.op.Inverse, new List<Expression> { right, left.children[1] });
                    right.Simplify();
                    left = left.children[0];
                    Expression temp = left;
                    left = right;
                    right = temp;
                }
                else if (left.op.Symbol == "/")
                {                    
                    if (left.children[0].IsOp("+"))
                    {
                        left.SeparateFraction();
                        foreach (Expression child in left.children)
                        {
                            child.Simplify();
                        }
                    }     
                    else if (left.children[0].IsOp("*"))
                    {
                        for (int i = 0; i < left.children[0].children.Count; i++)
                        {
                            if (!left.children[0].children[i].ContainsVariable(var))
                            {
                                right = right / left.children[0].children[i];
                                right.Simplify();
                                left.children[0].children.RemoveAt(i);
                                if (left.children[0].children.Count == 1)
                                {
                                    left.children[0] = left.children[0].children[0];
                                }
                                break;
                            }
                        }
                    }
                    else if (left.children[1].IsOp("*"))
                    {
                        for (int i = 0; i < left.children[1].children.Count; i++)
                        {
                            if (!left.children[1].children[i].ContainsVariable(var))
                            {
                                right = right * left.children[1].children[i];
                                right.Simplify();
                                left.children[1].children.RemoveAt(i);
                                if (left.children[1].children.Count == 1)
                                {
                                    left.children[1] = left.children[1].children[0];
                                }
                                i--;
                            }
                        }
                    }
                    else
                    {
                        left.SinOverCosToTan();                        
                    }
                }                
                else
                {
                    break;
                    //throw new Exception("no progress made, cannot solve");
                }
            } while (!Equals(copy));

            try
            {                
                ExpandAndSimplify();
                Dictionary<int, Expression> info = left.getPolynomialInfo(var);
                

                if (info.ContainsKey(2) && info.Keys.All(x => 0<=x && x<=2))
                {                    
                    for (int i = 0; i <= 2; i++)
                    {
                        if (!info.ContainsKey(i))
                        {
                            info.Add(i, new Expression(0));
                        }
                    }        

                    return SolveQuadratic(info, var);
                }
                else
                {
                    throw new Exception("cannot solve polynomial of degree greater than 2");
                }
            }
            catch (Exception exception)
            {
                throw exception;
            }
        }
        public List<Equation> Solve(string var)
        {
            return Solve(new Expression(var));
        }
        private List<Equation> SolveQuadratic(Dictionary<int, Expression> info, Expression var)
        {            
            
            List<Equation> quadraticEquations = Utils.GetQuadraticEquations(var, info);
            List<Equation> solutions = new List<Equation>();
            foreach (Equation quadraticEquation in quadraticEquations)
            {
                Equation quadraticSolution = quadraticEquation.Solve(var)[0];
                if (!solutions.Contains(quadraticSolution))
                {
                    solutions.Add(quadraticSolution);
                }
            }
            return solutions;
        }
        public override string ToString()
        {
            return left.ToString() + "=" + right.ToString();
        }
        public string ToLatex()
        {
            string latexString = "";
            string leftLatex = left.ToLatex();
            string rightLatex = right.ToLatex();
            if (rightLatex == "ℝ")
            {
                rightLatex = @"\text{any real number}";
            }
            latexString += leftLatex + "=" + rightLatex;
            return latexString;
        }
    }
}

public class SolutionSet
{
    public List<Equation> equations;
    public SolutionSet(List<Equation> solutionSet)
    {
        this.equations = solutionSet;
    }
    public bool Contains(Equation equation)
    {
        bool contains = false;
        for (int i = 0; i < equations.Count; i++)
        {
            if (equations[i].Equals(equation))
            {
                contains = true;
                break;
            }
        }
        return contains;
    }
    public override bool Equals(object obj)
    {
        if (!(obj is SolutionSet))
        {
            return false;
        }        

        SolutionSet other = obj as SolutionSet;

        bool equals = true;
        if (equations.Count != other.equations.Count)
        {
            equals = false;
        }
        else
        {
            for (int i = 0; i < equations.Count; i++)
            {
                bool inOther = false;
                for (int j = 0; j < other.equations.Count; j++)
                {
                    if (equations[i].Equals(other.equations[j]))
                    {
                        other.equations.RemoveAt(j);
                        inOther = true;
                        break;
                    }
                }
                if (!inOther)
                {
                    equals = false;
                    break;
                }
            }
        }
        return equals;
    }
}
