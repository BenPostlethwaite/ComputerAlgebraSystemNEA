using FinalFormsApp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using static System.Formats.Asn1.AsnWriter;

namespace MathsLibrary
{
    public partial class Expression : ICloneable, MatrixOperand, ILatexPrintable
    {
        private void CombinePowerStack()
        {
            // eg: (x^a)^b -> x^(a*b)

            if (isLeaf || op.Symbol != "^")
            {
                return;
            }
            
            else if (!children[0].isLeaf && children[0].op.Symbol == "^")
            {
                CloneFrom(children[0].children[0] ^ (children[0].children[1] * children[1]));
            }
        }
        public void CombineMultiplicationTerms()
        {
            //eg: x^2*3^n*x^3 -> x^5*3^n
            if (isLeaf || op.Symbol != "*")
            {
                return;
            }



            int constant = 1;
            List<Expression> otherChildren = new List<Expression>();

            for (int i = 0; i < children.Count; i++)
            {
                if (children[i].isDouble)
                {
                    constant *= (int)children[i].ToDouble();
                }
                else
                {
                    otherChildren.Add(children[i]);
                }
            }

            children = otherChildren;


            List<(Expression remaining, Expression coefficient)> terms = new List<(Expression, Expression)>();
            for (int c = 0; c < children.Count; c++)
            {
                Expression child = children[c];
                if (child.isLeaf)
                {
                    if (terms.Any(t => t.remaining.Equals(child)))
                    {
                        (Expression remaining, Expression coefficient) match = terms.Where(t => t.remaining.Equals(child)).First();
                        match.coefficient += 1;
                        match.coefficient.GeneralSimplify();
                        for (int i = 0; i < terms.Count; i++)
                        {
                            if (terms[i].remaining.Equals(child))
                            {
                                terms[i] = match;
                                break;
                            }
                        }
                    }
                    else
                    {
                        terms.Add((child, new Expression(1)));
                    }
                }
                else if (child.op.Symbol == "^")
                {
                    //when to combine?
                    // x^2 * x^3 -> x^5
                    // 2^x * 2^4x -> 2^(5x)
                    // 2^x * 2^4 -> 2^x * 2^4


                    Expression coefficient = child.children[1].Clone() as Expression;
                    Expression remaining = child.children[0].Clone() as Expression;

                    if (terms.Any(t => t.remaining.Equals(remaining)))
                    {
                        (Expression remaining, Expression coefficient) match = terms.Where(t => t.remaining.Equals(remaining)).FirstOrDefault();
                        match.coefficient += coefficient;
                        match.coefficient.ExpandAndSimplify();

                        for (int i = 0; i < terms.Count; i++)
                        {
                            if (terms[i].remaining.Equals(remaining))
                            {
                                terms[i] = match;
                            }
                        }
                    }
                    else
                    {
                        terms.Add((remaining, coefficient));
                    }
                }
                else
                {
                    if (terms.Any(t => t.remaining.Equals(child)))
                    {
                        for (int i = 0; i < terms.Count; i++)
                        {
                            if (terms[i].remaining.Equals(child))
                            {
                                terms[i] = (terms[i].remaining, terms[i].coefficient + 1);
                                terms[i].coefficient.GeneralSimplify();
                                break;
                            }
                        }
                    }
                    else
                    {
                        terms.Add((child, new Expression(1)));
                    }
                }
            }
            children = new List<Expression>();

            foreach (var term in terms)
            {
                if (term.coefficient.isLeaf && term.coefficient == 1)
                {
                    children.Add(term.remaining);
                }
                else
                {
                    children.Add(term.remaining ^ term.coefficient);
                }
            }
            if (constant != 1)
            {
                children.Add(new Expression(constant));
            }
            if (children.Count == 0)
            {
                CloneFrom(new Expression(constant));
            }
            else if (children.Count == 1)
            {
                CloneFrom(children[0]);
            }        
            CombineAssociativeOperators();

        }                
        public void SplitPowers()
        {         


            //eg 2^(3/2) -> 2 * 2^(1/2)
            if (!IsOp("^"))
            {
                return;
            }

            if (children[0].isDouble)
            {
                children[0].ConstantsToPrimeFactors();

                //test if base is prime
                if (children[0].isLeaf && children[1].IsOp("/"))
                {
                    children[1].FractionToMixedNumber();
                    if (children[1].IsOp("+"))
                    {
                        Expression newExpression = new Expression(Utils.operators["*"]);
                        for (int i = 0; i < children[1].children.Count; i++)
                        {
                            newExpression.children.Add(children[0] ^ children[1].children[i]);
                            newExpression.children[i].ExpandAndSimplify();
                        }                        
                        CloneFrom(newExpression);
                        //2^(3/2) -> 2 * 2^(1/2)    
                        return;
                    }
                }                
            }          
                
            //eg: (2x)^2 -> 2^2x^2
            if (children[0].IsOp("*"))
            {
                List<Expression> notSeperated = new List<Expression>();
                List<Expression> seperated = new List<Expression>();
                for (int i = 0; i < children[0].children.Count; i++)
                {
                    Expression child = children[0].children[i] ^ children[1];
                    Expression imaginary = new Expression("i", "infix");
                    child.Simplify();                    
                    if (child.isLeaf ||!child.isNumeric || child.op.Symbol != "^" || child == imaginary)
                    {
                        seperated.Add(child);
                    }
                    else
                    {
                        notSeperated.Add(child);
                    }
                }
                Expression toReturn = new Expression(Utils.operators["*"], new List<Expression>());

                toReturn.children.AddRange(seperated);

                if (notSeperated.Count > 0)
                {
                    Expression notSeperatedExpression;
                    if (notSeperated.Count == 1)
                    {
                        notSeperatedExpression = notSeperated[0];
                    }
                    else
                    {
                        notSeperatedExpression = new Expression(Utils.operators["*"], notSeperated);
                        notSeperatedExpression.CombineMultiplicationTerms();
                    }
                    toReturn.children.Add(notSeperatedExpression);
                    toReturn.CombineAssociativeOperators();
                }
                if (toReturn.children.Count == 1)
                {
                    CloneFrom(toReturn.children[0]);
                }
                else
                {
                    CloneFrom(toReturn);
                }
                return;
            }
        }
        public void CombineSameExponants()
        {
            if (!IsOp("*"))
            {
                return;
            }

            List<Expression> dontCombine = new List<Expression>();
            List<(Expression exponant, Expression baseExpression)> terms = new List<(Expression, Expression)>();
            Expression imaginary = new Expression("i", "infix");
            for (int i = 0; i < children.Count; i++)
            {
                //eg: 
                if (!children[i].IsOp("^") ||
                    !children[i].children[0].isDouble ||
                    children[i].Equals(imaginary))
                {
                    dontCombine.Add(children[i]);
                }
                else
                {
                    bool found = false;
                    for (int j = 0; j < terms.Count; j++)
                    {
                        Expression exponent = children[i].children[1];
                        Expression baseExpression = new Expression(Utils.operators["*"], new List<Expression>());
                        if (terms[j].exponant.Equals(exponent))
                        {
                            terms[j] = (terms[j].exponant, terms[j].baseExpression * children[i].children[0]);
                            found = true;
                            break;
                        }
                    }
                    if (!found)
                    {
                        terms.Add((children[i].children[1], children[i].children[0]));
                    }
                }
            }

            List<Expression> newChildren = new List<Expression>();
            newChildren.AddRange(dontCombine);
            foreach ((Expression exponant, Expression baseExpression) in terms)
            {
                baseExpression.Simplify();
                if (exponant.Equals(new Expression(1)))
                {
                    if (baseExpression.Equals(new Expression(1)))
                    {
                        continue;
                    }
                    else
                    {
                        newChildren.Add(baseExpression);
                    }
                }
                else
                {
                    newChildren.Add(baseExpression ^ exponant);
                }
            }
            if (newChildren.Count == 1)
            {
                CloneFrom(newChildren[0]);
            }
            else
            {
                CloneFrom(new Expression(Utils.operators["*"], newChildren));
            }
        }
        private void ChangeNegativePowersToFractions()
        {
            if (!IsOp("^"))
            {
                return;
            }
            Expression copy = Clone() as Expression;
            if (copy.children[1] == -1)
            {
                CloneFrom(1 / copy.children[0]);
                return;
            }
            copy.children[1].Factorise();
            copy.children[1].ConstantsToPrimeFactors();
            copy.children[1].CombineAssociativeOperators();
            if (!copy.children[1].IsOp("*"))
            {
                return;
            }
            for (int i = 0; i < copy.children[1].children.Count; i++)
            {
                if (copy.children[1].children[i] == -1)
                {
                    copy.children[1].children.RemoveAt(i);
                    if (copy.children[1].children.Count == 1)
                    {
                        copy.children[1] = copy.children[1].children[0];
                    }
                    CloneFrom(1 / copy);
                    return;
                }
            }
        }
    }
}
