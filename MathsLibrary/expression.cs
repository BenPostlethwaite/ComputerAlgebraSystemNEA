using FinalFormsApp;
using System.ComponentModel.Design;
using System.Globalization;
using System.Linq.Expressions;
using System.Reflection;

namespace MathsLibrary
{
    public partial class Expression : ICloneable, MatrixOperand, ILatexPrintable
    {
        public void Substitute(string variable, double subIn)
        {
            Expression var = new Expression(variable, "infix");
            Expression sub = new Expression(subIn.ToString());
            Substitute(var, sub);
        }
        public void Substitute(string variable, string subIn)
        {
            Expression var = new Expression(variable, "infix");
            Expression sub = new Expression(subIn, "infix");
            Substitute(var, sub);
        }
        public void Substitute(string variable, Expression subIn)
        {
            Expression var = new Expression(variable);
            Substitute(var, subIn);
        }
        public void Substitute(Expression variable, double subIn)
        {
            Expression sub = new Expression(subIn.ToString());
            Substitute(variable, sub);
        }
        public void Substitute(Expression variable, Expression subIn)
        {
            if (Equals(variable))
            {
                CloneFrom(subIn);
            }
            else
            {
                foreach (Expression child in children)
                {
                    child.Substitute(variable, subIn);
                }
            }
        }
        public void ExpandAndSimplifyOneLayer()
        {
            try
            {
                SimplifyOneLevel();

                Distribute();
                BinomialExpansion();
            }
            catch (Exception e)
            {
                throw e;
            }

        }
        public void ExpandAndSimplify()
        {
            try
            {
                Expression newRoot;
                do
                {
                    newRoot = (Expression)Clone();
                    CombineAssociativeOperators();
                    for (int i = 0; i < children.Count; i++)
                    {
                        if (!children[i].isLeaf)
                        {
                            Expression child = children[i];
                            child.ExpandAndSimplify();
                        }
                    }
                    CombineAssociativeOperators();
                    ExpandAndSimplifyOneLayer();

                } while (!newRoot.Equals(this));
            }
            catch (Exception e)
            {
                throw e;
            }

        }
        private void SimplifyOneLevel()
        {
            Expression old;
            try
            {
                do
                {
                    old = (Expression)Clone();

                    if (isLeaf) { return; }
                    CombineAssociativeOperators();
                    CombineAdditionTerms();
                    ChangeNegativePowersToFractions();
                    TanToSinOverCos();

                    SimplifyLogs();
                    SimplifyTrig();
                    SimplifyTrigExactValues();

                    CombineNumericTerms();
                    CombineMultiplicationTerms();

                    SplitPowers();
                    CombinePowerStack();

                    CombineSameExponants();

                    GeneralSimplify();

                } while (!old.Equals(this));
            }
            catch (Exception e)
            {
                throw e;
            }

        }
        public void Simplify()
        {
            try
            {
                Expression newRoot;
                do
                {
                    newRoot = (Expression)Clone();
                    CombineAssociativeOperators();
                    for (int i = 0; i < children.Count; i++)
                    {
                        if (!children[i].isLeaf)
                        {
                            Expression child = children[i];
                            child.Simplify();
                        }
                    }

                    CombineAssociativeOperators();
                    SimplifyOneLevel();
                } while (!newRoot.Equals(this));
            }
            catch (Exception e)
            {
                throw e;
            }

        }
        private void CombineNumericTerms()
        {
            //eg: 2 + x + 4 => 6 + x
            //   2 * x * 4 => 8 * x
            if (isLeaf || !op.Commutative)
            {
                return;
            }
            List<Expression> numericChildren = children.Where(x => x.isDouble).ToList();
            List<Expression> remainingChildren = children.Where(x => !x.isDouble).ToList();
            if (numericChildren.Count <= 1)
            {
                return;
            }
            Expression coefficient = new Expression(op, numericChildren);
            coefficient.GeneralSimplify();
            if (remainingChildren.Count == 0)
            {
                CloneFrom(coefficient);
                return;
            }
            if (coefficient.value.ToString() == "0" && op.ToString() == "*")
            {
                value = new Number(0);
                children = new List<Expression>();
                return;
            }
            children = new List<Expression>();
            if (op.ToString() != "*" || coefficient.value.ToString() != "1")
            {
                children.Add(coefficient);
            }
            children.AddRange(remainingChildren);
            if (children.Count == 1)
            {
                CloneFrom(children[0]);
            }
        }
        private void MultiplyFractions()
        {
            List<Expression> numerators = children.Where(c => !c.isLeaf && c.op.Symbol == "/").Select(c => c.children[0]).ToList();
            List<Expression> denominators = children.Where(c => !c.isLeaf && c.op.Symbol == "/").Select(c => c.children[1]).ToList();
            List<Expression> notFraction = children.Where(c => c.isLeaf || (c.op.Symbol != "/")).ToList();
            List<Expression> newChildren = new List<Expression>();
            numerators.AddRange(notFraction);

            Expression newNumerator = new Expression(Utils.operators["*"], numerators);
            Expression newDenominator = new Expression(Utils.operators["*"], denominators);

            if (newNumerator.children.Count == 1)
            {
                newNumerator = newNumerator.children[0];
            }
            else if (newNumerator.children.Count == 0)
            {
                newNumerator = new Expression("1");
            }
            if (newDenominator.children.Count == 1)
            {
                newDenominator = newDenominator.children[0];
            }
            else if (newDenominator.children.Count == 0)
            {
                CloneFrom(newNumerator);
                return;
            }

            CloneFrom(newNumerator / newDenominator);
            return;
        }
        public void GeneralSimplify()
        {
            if (isLeaf)
            {
                return;
            }
            if (Utils.functions.ContainsKey(op.Symbol))
            {
                return;
            }
            if (op.Symbol.ToString() == "-")
            {
                if (children.Count == 1)
                {
                    value = null;
                    op = Utils.operators["*"];
                    children.Insert(0, new Expression("-1"));
                }
                else
                {
                    value = null;
                    op = Utils.operators["+"];

                    for (int i = 1; i < children.Count; i++)
                    {
                        Expression newChild;
                        if (children[i].IsOp("+"))
                        {
                            for (int j = 0; j < children[i].children.Count; j++)
                            {
                                newChild = -1 * children[i].children[j];
                                newChild.CombineAssociativeOperators();
                                newChild.GeneralSimplify();
                                children[i].children[j] = newChild;
                            }
                        }
                        else
                        {
                            newChild = -1 * children[i];
                            newChild.CombineAssociativeOperators();
                            newChild.GeneralSimplify();
                            children[i] = newChild;
                        }
                    }
                }
            }
            if (op.Symbol == "/" && children.Count == 2)
            {
                if (!children[1].isLeaf && children[1].op.Symbol == "/")
                {
                    Expression newChild = children[0] * children[1].children[1];
                    newChild.GeneralSimplify();
                    children[0] = newChild;
                    children[1] = children[1].children[0];
                }
                else if (!children[0].isLeaf && children[0].op.Symbol == "/")
                {
                    Expression newChild = children[0].children[1] * children[1];
                    newChild.GeneralSimplify();
                    children[0] = children[0].children[0];
                    children[1] = newChild;
                }
                if (children[0] == new Expression("0"))
                {
                    CloneFrom(new Expression("0"));

                }
                else
                {
                    SimplifyFraction();
                }
                return;
            }
            if (children.Count == 0)
            {
                return;
            }
            if (op.Commutative)
            {
                if (children.All(x => x.isDouble))
                {
                    int intValue = (int)op.Evaluate(children.Select(x => x.ToDouble()).ToList());
                    value = new Number(intValue);
                    op = null;
                    children = new List<Expression>();
                    return;
                }
                else
                {
                    CombineNumericTerms();
                }
            }
            if (isLeaf)
            {
                return;
            }
            if (op.Symbol == "^")
            {
                for (int i = 1; i < children.Count; i++)
                {
                    if (children[i].isLeaf && children[i].ToString() == "0")
                    {
                        value = new Number(1);
                        op = null;
                        children = new List<Expression>();
                        return;
                    }
                    else if (children[i].isLeaf && children[i].ToString() == "1")
                    {
                        children.RemoveAt(i);
                        if (children.Count == 1)
                        {
                            op = children[0].op;
                            value = children[0].value;
                            children = children[0].children;
                            return;
                        }
                    }
                }
                if (!children[0].isLeaf && children[0].op.Symbol == "/")
                {
                    children[0].children[0] ^= children[1];
                    children[0].children[1] ^= children[1];
                    CloneFrom(children[0]);
                    return;
                }
                if (children[0].ToString() == "0")
                {
                    value = new Number(0);
                    op = null;
                    children = new List<Expression>();
                    return;
                }
                if (children[0].ToString() == "1")
                {
                    value = new Number(1);
                    op = null;
                    children = new List<Expression>();
                    return;
                }
            }
            if (op.OperandNumber >= 2 && children.Count == 1)
            {
                CloneFrom(children[0]);
                return;
            }
            if (op.Symbol == "*")
            {
                children.RemoveAll(c => c.isLeaf && c.value.ToString() == "1");
                if (children.Any(c => c.isLeaf && c.value.ToString() == "0"))
                {
                    value = new Number(0);
                    children = new List<Expression>();
                    return;
                }
                if (children.Any(c => !c.isLeaf && c.op.Symbol == "/"))
                {
                    MultiplyFractions();
                }
            }
            else if (op.Symbol.ToString() == "+")
            {
                children.RemoveAll(c => c == 0);
                if (children.Count == 0)
                {
                    op = null;
                    value = new Number(0);
                    children = new List<Expression>();
                    return;
                }
                else if (children.Count == 1)
                {
                    CloneFrom(children[0]);
                }
                else if (children.Any(c => !c.isLeaf && c.op.Symbol == "/"))
                {
                    AddFractions();
                }
            }
            if (isNumeric)
            {
                double evaluation = ToDouble();
                if (evaluation % 1 == 0)
                {
                    op = null;
                    try
                    {
                        value = new Number(evaluation);
                    }
                    catch (Exception e)
                    {
                        throw e;
                    }
                    children = new List<Expression>();
                    return;
                }
            }

        }
        public void SimplifyLogs()
        {
            if (isLeaf)
            {
                return;
            }
            if (op.Symbol == "^")
            {
                //a^b
                if (children[1].isLeaf)
                {
                    return;
                }
                if (children[1].op.Symbol == "*")
                {
                    //a^(bc)
                    Expression e = new Expression("e");
                    List<Expression> logChildren = children[1].children.Where(c => c.IsOp("log")).ToList();
                    List<Expression> nonLogChildren = children[1].children.Where(c => !c.IsOp("log")).ToList();

                    if (logChildren.Count == 0)
                    {
                        return;
                    }
                    if (children[0].ToString() != "e")
                    {
                        //a^(b*ln(c)) => e^(b*ln(c)*ln(a))
                        CloneFrom(e ^ (children[1] * Log(e, children[0])));
                        CombineAssociativeOperators();
                        SimplifyLogs();
                        return;
                    }
                    // a^(ln(b)*ln(c))
                    if (logChildren.Count != 1)
                    {
                        return;
                    }
                    Expression nonLogChildrenExpression = new Expression(Utils.operators["*"], nonLogChildren);
                    if (nonLogChildrenExpression.children.Count == 1)
                    {
                        nonLogChildrenExpression = nonLogChildrenExpression.children[0];
                    }
                    // a^(b*ln(c)) => c^b
                    CloneFrom(logChildren[0].children[1] ^ nonLogChildrenExpression);
                    return;
                }
                else if (children[1].op.Symbol == "log")
                {
                    if (children[1].children[0] == children[0])
                    {
                        CloneFrom(children[1].children[1]);
                        return;
                    }
                    Expression e = new Expression("e");
                    Expression log = children[1];
                    Expression expressionBase = children[0];

                    // a^(ln(b))
                    if (expressionBase != e)
                    {
                        bool logNumeric = log.isNumeric;
                        bool baseNumeric = expressionBase.isNumeric;

                        if (logNumeric && baseNumeric)
                        {
                            CloneFrom(e ^ (log * Log(e, expressionBase)));
                        }
                        else if (baseNumeric && !logNumeric)
                        {
                            CloneFrom(log.children[1] ^ Log(e, expressionBase));
                        }
                        else if (!baseNumeric && logNumeric)
                        {
                            return;
                        }
                        else
                        {
                            return;
                        }
                    }
                }
                else if (children[1].op.Symbol == "/")
                {
                    // a^(b/c)
                    Expression e = new Expression("e");
                    if (children[1].children[1].IsOp("log") &&
                        children[0] == children[1].children[1].children[1])
                    {
                        // a^(b/ln(a)) = e^(b*ln(a)/ln(a)) = e^b
                        CloneFrom(e ^ children[1].children[0]);
                        return;
                    }                    
                    Expression denominator = children[1].children[1].Clone() as Expression;
                    Expression copy = Clone() as Expression;
                    copy.children[1] = copy.children[1].children[0];
                    copy.Simplify();
                    if (copy.IsOp("^"))
                    {
                        return;
                    }
                    else
                    {
                        CloneFrom(copy ^ (1 / denominator));
                    }
                    return;
                }
                else if (children[1].op.Symbol == "+")
                {
                    Expression baseExpression = children[0];
                    Expression copy = new Expression(Utils.operators["*"], new List<Expression>());

                    foreach (Expression child in children[1].children)
                    {
                        Expression part = baseExpression ^ child;
                        part.Simplify();
                        copy.children.Add(part);
                    }
                    CloneFrom(copy);
                    return;                    
                }
            }
            else if (op.Symbol == "/")
            {
                if (children[0].isLeaf || children[1].isLeaf)
                {
                    return;
                }
                if (children[0].op.Symbol == "log" && children[1].op.Symbol == "log")
                {
                    if (children[0].children[0] == children[1].children[0])
                    {
                        Expression e = new Expression("e");
                        children[0].children[0] = e;
                        children[1].children[0] = e;
                    }
                }
            }
            else if (op.Symbol == "log")
            {
                if (children[0] == children[1])
                {
                    CloneFrom(new Expression(1));
                    return;
                }
                Expression e = new Expression("e");
                if (!children[0].isLeaf || children[0].value.ToString() != "e")
                {
                    CloneFrom(Log(e, children[1]) / Log(e, children[0]));
                }
                else if (!children[1].isLeaf && children[1].op.Symbol == "^")
                {
                    CloneFrom(children[1].children[1] * Log(children[0], children[1].children[0]));
                }
                else if (children[1].isLeaf)
                {
                    if (children[1] == 1)
                    {
                        CloneFrom(new Expression(0));
                        return;
                    }
                    Expression copy = children[1].Clone() as Expression;
                    children[1].ConstantsToPrimeFactors();

                    if (children[1] != copy)
                    {
                        SimplifyLogs();
                    }
                }
                else if (children[1].op.Symbol == "*")
                {
                    Expression toCloneFrom = new Expression(Utils.operators["+"], new List<Expression>());
                    for (int i = 0; i < children[1].children.Count; i++)
                    {
                        toCloneFrom.children.Add(Log(children[0], children[1].children[i]));
                    }
                    if (toCloneFrom.children.Count == 1)
                    {
                        CloneFrom(toCloneFrom.children[0]);
                    }
                    else
                    {
                        CloneFrom(toCloneFrom);
                    }
                }
                else if (children[1].op.Symbol == "/")
                {
                    Expression toCloneFrom = new Expression(Utils.operators["-"], new List<Expression>());
                    for (int i = 0; i < children[1].children.Count; i++)
                    {
                        toCloneFrom.children.Add(Log(children[0], children[1].children[i]));
                    }
                    if (toCloneFrom.children.Count == 1)
                    {
                        CloneFrom(toCloneFrom.children[0]);
                    }
                    else
                    {
                        CloneFrom(toCloneFrom);
                    }
                    GeneralSimplify();
                }
            }
        }
        public void CombineAssociativeOperators()
        {
            for (int i = 0; i < children.Count; i++)
            {
                children[i].CombineAssociativeOperators();
            }
            if (isLeaf || !op.Associative)
            {
                return;
            }
            List<Expression> newChildren = new List<Expression>();
            for (int i = 0; i < children.Count; i++)
            {
                Expression child = children[i];
                if ((!child.isLeaf) && child.op.Equals(op))
                {
                    newChildren.AddRange(child.children);
                }
                else
                {
                    newChildren.Add(child);
                }
            }
            children = newChildren;
            return;
        }
        public void Distribute()
        {
            if (!IsOp("*"))
            {
                return;
            }
            if (children.All(c => c.isLeaf))
            {
                return;
            }
            for (int i = 0; i < children.Count; i++)
            {
                if (children[i].isLeaf || children[i].op.Symbol != "+")
                {
                    children[i] = new Expression(Utils.operators["+"], new List<Expression>() { children[i] });
                }
            }
            List<List<Expression>> expressions = new List<List<Expression>>();
            foreach (Expression child in children)
            {
                expressions.Add(child.children);
            }
            List<List<Expression>> combinations = Utils.GetCombinations(expressions);
            value = null;
            op = Utils.operators["+"];
            children = new List<Expression>();
            foreach (List<Expression> combination in combinations)
            {
                children.Add(new Expression(Utils.operators["*"], combination));
            }
            if (children.Count == 1)
            {
                CloneFrom(children[0]);
            }
            return;
        }
        public void BinomialExpansion()
        {
            if (!IsOp("^"))
            {
                return;
            }
            if (!children[0].IsOp("+"))
            {
                return;
            }
            while (children.Count > 2)
            {
                Expression child1 = new Expression(op, new List<Expression>() { children[0], children[1] });
                children.RemoveRange(0, 2);
                children.Insert(0, child1);
            }
            if (children[0].isLeaf)
            {
                return;
            }
            while (children[0].children.Count > 2)
            {
                Expression child1 = new Expression(children[0].op, new List<Expression>() { children[0].children[0], children[0].children[1] });
                children[0].children.RemoveRange(0, 2);
                children[0].children.Insert(0, child1);
            }
            Expression baseExpression = children[0];
            Expression exponent = children[1];

            //binomial expansion
            if (!exponent.isNatural)
            {
                return;
            }
            int intExponant = (int)exponent.ToDouble();
            value = null;
            op = Utils.operators["+"];
            children = new List<Expression>();
            for (int i = 0; i <= intExponant; i++)
            {
                Expression newChild = new Expression(Utils.operators["*"], new List<Expression>());

                Expression coefficient = new Expression(Utils.NcR(intExponant, i));
                Expression part1 = baseExpression.children[0] ^ new Expression(i);
                Expression part2 = baseExpression.children[1] ^ new Expression(intExponant - i);

                newChild.children = new List<Expression>() { coefficient, part1, part2 };
                children.Add(newChild);
            }
        }
        public void CombineAdditionTerms()
        {
            // eg: 2x + 3x -> 5x                      
            if (isLeaf || op.Symbol != "+")
            {
                return;
            }

            List<(Expression remaining, Expression coefficient)> terms = new List<(Expression, Expression)>();
            for (int c = 0; c < children.Count; c++)
            {
                Expression child = children[c];
                if (child.isLeaf)
                {
                    if (terms.Any(t => t.remaining.Equals(child)))
                    {
                        (Expression remaining, Expression coefficient) match = terms.Where(t => t.remaining.Equals(child)).First();
                        match.coefficient = new Expression(Utils.operators["+"], new List<Expression>() { match.coefficient, new Expression("1") });
                        match.coefficient.GeneralSimplify();
                        for (int i = 0; i < terms.Count; i++)
                        {
                            if (terms[i].remaining.Equals(child))
                            {
                                terms[i] = match;
                            }
                        }
                    }
                    else
                    {
                        terms.Add((child, new Expression("1")));
                    }
                }
                else if (child.op.Symbol == "*")
                {
                    List<Expression> coefficientChildren = new List<Expression>();
                    List<Expression> remainingChildren = new List<Expression>();
                    for (int i = 0; i < child.children.Count; i++)
                    {
                        if (child.children[i].isDouble)
                        {
                            coefficientChildren.Add(child.children[i]);
                        }
                        else
                        {
                            remainingChildren.Add(child.children[i]);
                        }
                    }
                    Expression coefficient = new Expression(Utils.operators["*"], coefficientChildren);
                    Expression remaining = new Expression(Utils.operators["*"], remainingChildren);
                    if (remaining.children.Count == 0)
                    {
                        remaining = new Expression("1");
                    }
                    else if (remaining.children.Count == 1)
                    {
                        remaining = remaining.children[0];
                    }
                    if (coefficient.children.Count == 0)
                    {
                        coefficient = new Expression("1");
                    }
                    else if (coefficient.children.Count == 1)
                    {
                        coefficient = coefficient.children[0];
                    }


                    remaining.GeneralSimplify();
                    coefficient.GeneralSimplify();

                    if (terms.Any(t => t.remaining.Equals(remaining)))
                    {
                        (Expression remaining, Expression coefficient) match = terms.Where(t => t.remaining.Equals(remaining)).FirstOrDefault();
                        if (match.coefficient.isLeaf)
                        {
                            match.coefficient = new Expression(Utils.operators["+"], new List<Expression>() { match.coefficient, coefficient });
                        }
                        else
                        {
                            match.coefficient += coefficient;
                        }
                        match.coefficient.GeneralSimplify();
                        for (int i = 0; i < terms.Count; i++)
                        {
                            if (terms[i].remaining.Equals(remaining))
                            {
                                terms[i] = match;
                                break;
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
                        int matchIndex = terms.FindIndex(t => t.remaining.Equals(child));
                        terms[matchIndex] = (terms[matchIndex].remaining, terms[matchIndex].coefficient + 1);
                        terms[matchIndex].coefficient.GeneralSimplify();
                    }
                    else
                    {
                        terms.Add((child, new Expression("1")));
                    }
                }
            }
            children = new List<Expression>();
            foreach (var term in terms)
            {
                if (term.coefficient.isLeaf && term.coefficient.value.ToString() == "1")
                {
                    children.Add(term.remaining);
                }
                else if (term.remaining.isLeaf && term.remaining.value.ToString() == "1")
                {
                    children.Add(term.coefficient);
                }
                else
                {
                    Expression newChild = term.remaining * term.coefficient;
                    newChild.CombineAssociativeOperators();
                    children.Add(newChild);
                }
            }
            if (children.Count == 1)
            {
                CloneFrom(children[0]);
            }
        }
        public void CloneFrom(Expression expression)
        {
            op = expression.op;
            value = expression.value;
            children = new List<Expression>();
            foreach (Expression child in expression.children)
            {
                children.Add(child.Clone() as Expression);
            }
        }
        public static List<Expression> CommonFactors(List<Expression> list)
        {
            // returns common factors, and divides all expressions in list by common factors


            Expression part1 = list[0].Clone() as Expression;
            Expression part2 = list[1].Clone() as Expression;


            //do factors 2 at a time;
            if (list.Count > 2)
            {
                List<Expression> part2List = list.GetRange(1, list.Count - 1);
                part2 = new Expression(Utils.operators["+"], part2List);
            }

            part1.Simplify();
            part2.Simplify();


            List<Expression> commonFactors = new List<Expression>();


            if (part1.children.Count > 1)
            {
                if (part1.op.Symbol == "^")
                {
                    part1.children[0].Factorise();
                }
                else
                {
                    part1.Factorise();
                }
            }
            part1.ConstantsToPrimeFactors();
            for (int i = 0; i < part1.children.Count; i++)
            {
                part1.children[i].CombineMultiplicationTerms();
                part1.children[i].SplitPowers();
            }
            part1.ConstantsToPrimeFactors();
            if (part1.IsOp("/"))
            {
                part1 = part1.children[0] * (1 / part1.children[1]);
            }
            part1.CombineAssociativeOperators();

            if (part2.children.Count > 1)
            {
                if (part2.op.Symbol == "^")
                {
                    part2.children[0].Factorise();
                }
                else
                {
                    part2.Factorise();
                }
            }
            part2.ConstantsToPrimeFactors();

            for (int i = 0; i < part2.children.Count; i++)
            {
                part2.children[i].CombineMultiplicationTerms();
                part2.children[i].SplitPowers();
            }
            part2.ConstantsToPrimeFactors();
            if (part2.IsOp("/"))
            {
                part2 = part2.children[0] * (1 / part2.children[1]);
            }
            part2.CombineAssociativeOperators();

            if (!part1.IsOp("*"))
            {
                part1 = new Expression(Utils.operators["*"], new List<Expression>() { part1 });
            }

            if (!part2.IsOp("*"))
            {
                part2 = new Expression(Utils.operators["*"], new List<Expression>() { part2 });
            }

            for (int i = 0; i < part1.children.Count; i++)
            {
                if (!part1.children[i].IsOp("^"))
                {
                    part1.children[i] = part1.children[i] ^ 1;
                }
                Expression part1Child = part1.children[i];
                Expression part1Base = part1Child.children[0];
                Expression part1Exponent = part1Child.children[1];

                for (int j = 0; j < part2.children.Count; j++)
                {
                    if (!part2.children[j].IsOp("^"))
                    {
                        part2.children[j] = part2.children[j] ^ 1;
                    }

                    Expression part2Child = part2.children[j];
                    Expression part2Base = part2Child.children[0];
                    Expression part2Exponent = part2Child.children[1];

                    if (!part1Base.Equals(part2Base))
                    {
                        continue;
                    }

                    Expression exponantDiffernce = part1Exponent - part2Exponent;
                    exponantDiffernce.ExpandAndSimplify();
                    Expression factor;
                    bool? exponantDifferenceIsPositive = exponantDiffernce.isPositive;
                    if (exponantDifferenceIsPositive == true)
                    {
                        factor = part1Base ^ part2Exponent;
                    }
                    else if (exponantDifferenceIsPositive == false)
                    {
                        factor = part1Base ^ part1Exponent;
                    }
                    else
                    {
                        continue;
                    }

                    part1Child.children[1] -= factor.children[1];
                    part2Child.children[1] -= factor.children[1];

                    part1Child.children[1].ExpandAndSimplify();
                    if (part1Child.children[1] == 0)
                    {
                        part1.children.RemoveAt(i);
                        i--;
                    }
                    part2Child.children[1].ExpandAndSimplify();
                    if (part2Child.children[1] == 0)
                    {
                        part2.children.RemoveAt(j);
                        j--;
                    }

                    factor.Simplify();
                    commonFactors.Add(factor);

                    break;
                }
            }
            if (commonFactors.Count == 0)
            {
                return commonFactors;
            }

            if (part1.children.Count == 0)
            {
                part1 = new Expression(1);
            }
            else if (part1.children.Count == 1)
            {
                part1 = part1.children[0];
            }

            if (part2.children.Count == 0)
            {
                part2 = new Expression(1);
            }
            else if (part2.children.Count == 1)
            {
                part2 = part2.children[0];
            }

            part1.Simplify();
            part2.Simplify();

            list.Clear();

            list.Add(part1);
            list.Add(part2);

            return commonFactors;
        }
        public void FractionToMixedNumber()
        {
            if (isLeaf || op.Symbol != "/")
            {
                return;
            }
            if (isNumeric && children[0].isLeaf && children[1].isLeaf)
            {
                double whole = Math.Floor(children[0].ToDouble() / children[1].ToDouble());
                double remainder = children[0].ToDouble() % children[1].ToDouble();

                Expression wholeExpression = new Expression(whole);
                Expression remainderExpression = remainder / children[1];

                if (whole == 0)
                {
                    return;
                }
                Expression mixedNumber = wholeExpression + remainderExpression;
                CloneFrom(mixedNumber);
            }
        }
        private void FactoriseOnce()
        {
            //eg: 6x+9xy => 3x(2+3y)
            //or: 6x+9xy+3x^2 => 3x(2+3y+x)
            try
            {
                if (isLeaf)
                {
                    return;
                }
                for (int i = 0; i < children.Count; i++)
                {
                    children[i].Factorise();
                }
                CombineAssociativeOperators();
                if (op.OperandNumber != 1 && children.Count == 1)
                {
                    children = children[0].children;
                }
                Simplify();

                if (!IsOp("+"))
                {
                    return;
                }


                List<Expression> factors = Expression.CommonFactors(children);
                CombineAssociativeOperators();
                Simplify();


                Expression factorsExpression = new Expression(Utils.operators["*"], factors);
                if (factorsExpression.children.Count == 0)
                {
                    // eg: 
                    if (IsOp("+"))
                    {
                        Expression old = Clone() as Expression;
                        for (int i = 0; i < children.Count; i++)
                        {
                            children[i].ExpandAndSimplifyOneLayer();
                        }
                        CombineAssociativeOperators();
                        if (!Equals(old))
                        {
                            Factorise();
                        }
                    }
                    return;
                }

                else if (factorsExpression.children.Count == 1)
                {
                    factorsExpression = factorsExpression.children[0];
                }

                CloneFrom(factorsExpression * this);
            }
            catch (Exception e)
            {
                throw e;
            }

        }
        public void Factorise()
        {
            try
            {
                Expression old;
                do
                {
                    old = Clone() as Expression;
                    FactoriseOnce();
                } while (!Equals(old));
            }
            catch (Exception e)
            {
                throw e;
            }

        }
        public void ConstantsToPrimeFactors()
        {
            foreach (Expression child in children)
            {
                child.ConstantsToPrimeFactors();
            }
            if (!isDouble)
            {
                return;
            }
            List<int> primeFactors = (value as Number).primeFactors;
            Dictionary<int, int> primeFactorsDictionary = new Dictionary<int, int>();
            foreach (int factor in primeFactors)
            {
                if (primeFactorsDictionary.ContainsKey(factor))
                {
                    primeFactorsDictionary[factor]++;
                }
                else
                {
                    primeFactorsDictionary.Add(factor, 1);
                }
            }
            op = Utils.operators["*"];
            children = new List<Expression>();
            foreach (var item in primeFactorsDictionary)
            {
                if (item.Value == 1)
                {
                    children.Add(new Expression(item.Key));
                }
                else
                {
                    children.Add(new Expression(item.Key) ^ new Expression(item.Value));
                }
            }
            if (children.Count == 1)
            {
                CloneFrom(children[0]);
            }
        }
        private void AddFractions()
        {
            if (isLeaf || op.Symbol != "+")
            {
                throw new Exception("function called for wrong operator");
            }

            List<Expression> numerators = new List<Expression>();
            List<Expression> denominators = new List<Expression>();
            foreach (Expression child in children)
            {
                if (child.IsOp("/"))
                {
                    numerators.Add(child.children[0]);
                    denominators.Add(child.children[1]);
                }
                else
                {
                    numerators.Add(child);
                    denominators.Add(new Expression(1));
                }
            }
            //get common denominator
            Expression commonDenominator;
            if (denominators.All(d => denominators[0].Equals(d)))
            {
                commonDenominator = denominators[0];
            }
            else
            {
                commonDenominator = new Expression(Utils.operators["*"], new List<Expression>(denominators));
                if (commonDenominator.children.Count == 1)
                {
                    commonDenominator = commonDenominator.children[0];
                }
                commonDenominator.Simplify();

                //multiply each numerator by the common denominator divided by its denominator
                for (int i = 0; i < numerators.Count; i++)
                {
                    Expression numerator = numerators[i];
                    Expression denominator = denominators[i];

                    Expression factor = commonDenominator.Clone() as Expression;
                    factor = factor / denominator;
                    factor.Simplify();
                    if (factor != 1)
                    {
                        numerators[i] = numerator * factor;
                        numerators[i].Simplify();
                    }

                }
            }



            //add numerators
            Expression numeratorSum = new Expression(Utils.operators["+"], numerators);
            numeratorSum.Simplify();

            //set this expression to the fraction with the common denominator and the sum of the numerators
            CloneFrom(numeratorSum / commonDenominator);
        }
        public void PartialFactorise(Expression var)
        {
            if (isLeaf)
            {
                return;
            }
            for (int i = 0; i < children.Count; i++)
            {
                children[i].PartialFactorise(var);
            }
            if (op.Symbol == "+")
            {
                List<Expression> varChildren = children.Where(x => x.ContainsVariable(var)).ToList();
                List<Expression> nonVarChildren = children.Where(x => !x.ContainsVariable(var)).ToList();

                Expression varChildrenExpression;
                Expression nonVarChildrenExpression;

                if (varChildren.Count == 0)
                {
                    return;
                }
                else if (varChildren.Count == 1)
                {
                    varChildrenExpression = varChildren[0];
                }
                else
                {
                    varChildrenExpression = new Expression(op = Utils.operators["+"], varChildren);
                    varChildrenExpression.Factorise();
                }
                if (nonVarChildren.Count == 0)
                {
                    CloneFrom(varChildrenExpression);
                    return;
                }
                else if (nonVarChildren.Count == 1)
                {
                    nonVarChildrenExpression = nonVarChildren[0];
                }
                else
                {
                    nonVarChildrenExpression = new Expression(op = Utils.operators["+"], nonVarChildren);
                }

                CloneFrom(varChildrenExpression + nonVarChildrenExpression);

            }
        }
        public Expression Differentiate(string var)
        {
            return Differentiate(new Expression(var));
        }
        public Expression Differentiate(Expression var)
        {
            Expression copy = Clone() as Expression;
            if (copy.isLeaf)
            {
                if (copy == var)
                {
                    return new Expression(1);
                }
                else
                {
                    return new Expression(0);
                }
            }
            else
            {
                Expression toReturn = op.Differentiate(this, var);
                toReturn.Factorise();
                return toReturn;
            }           
        }        
        private void SimplifyFraction()
        {
            if (!IsOp("/"))
            {
                return;
            }
            else if (children[1] == new Expression(1))
            {
                CloneFrom(children[0]);
                return;
            }
            else if (children[1] == new Expression(-1))
            {
                CloneFrom(-children[0]);
                return;
            }

            Expression copy = Clone() as Expression;
            copy.ConstantsToPrimeFactors();

            if (copy.children[1].IsOp("*"))
            {
                for (int i = 0; i < copy.children[1].children.Count; i++)
                {
                    if (copy.children[1].children[i] == -1)
                    {
                        copy.children[0] *= -1;
                        copy.children[1].children.RemoveAt(i);
                        i--;
                        if (copy.children[1].children.Count == 1)
                        {
                            copy.children[1] = copy.children[1].children[0];
                        }
                    }
                }
            }

            Expression numerator = copy.children[0];
            Expression denominator = copy.children[1];

            numerator.Simplify();
            denominator.Simplify();



            if (Expression.CommonFactors(copy.children).Count() == 0)
            {
                CloneFrom(copy);
            }

            else if (copy.children[1] == 1)
            {
                CloneFrom(copy.children[0]);
                return;
            }
            else
            {
                CloneFrom(copy);
            }
            //RationaliseDenominator();

        }
        private List<Expression> GetMultiplesOfExpression(Expression expression)
        {
            List<Expression> toReturn = new List<Expression>();

            if (Equals(expression))
            {
                return new List<Expression>() { Clone() as Expression };
            }
            if (isLeaf)
            {
                return new List<Expression>();
            }
            else if (op.Symbol == "+")
            {
                foreach (var child in children)
                {
                    toReturn.AddRange(child.GetMultiplesOfExpression(expression));
                }
                return toReturn;
            }
            else if (op.Symbol == "*")
            {
                for (int i = 0; i < children.Count; i++)
                {
                    Expression child = children[i];

                    if (!child.IsOp("^"))
                    {
                        continue;
                    }
                    else if (!child.Equals(expression))
                    {
                        continue;
                    }
                    else
                    {
                        toReturn.Add(Clone() as Expression);
                    }
                }
                return toReturn;
            }
            else
            {
                return new List<Expression>();
            }
        }
        private List<Expression> GetTrigExpressions()
        {
            if (isLeaf)
            {
                return new List<Expression>();
            }
            else if (op.Symbol == "sin" || op.Symbol == "cos")
            {
                return new List<Expression>() { Clone() as Expression };
            }
            List<Expression> toReturn = new List<Expression>();
            foreach (var child in children)
            {
                toReturn.AddRange(child.GetTrigExpressions());
            }
            return toReturn;

        }
        private void TanToSinOverCos()
        {
            if (IsOp("tan"))
            {
                Expression sin = Sin(children[0]);
                Expression cos = Cos(children[0]);
                CloneFrom(sin / cos);
            }
        }
        public void SinOverCosToTan()
        {
            if (!IsOp("/"))
            {
                return;
            }
            if (children[0].IsOp("sin") && children[1].IsOp("cos"))
            {
                if (children[0].children[0] == children[1].children[0])
                {
                    CloneFrom(Tan(children[0].children[0]));
                    return;
                }
            }
            else if (children[0].IsOp("cos") && children[1].IsOp("sin"))
            {
                if (children[0].children[0] == children[1].children[0])
                {
                    CloneFrom(1 / Tan(children[0].children[0]));
                    return;
                }
            }
            if (!children[0].IsOp("*"))
            {
                children[0] = new Expression(Utils.operators["*"], new List<Expression>() { children[0] });
            }
            if (!children[1].IsOp("*"))
            {
                children[1] = new Expression(Utils.operators["*"], new List<Expression>() { children[1] });
            }
            for (int i = 0; i < children[0].children.Count; i++)
            {
                for (int j = 0; j < children[1].children.Count; j++)
                {
                    if (children[0].children[i].IsOp("sin") && children[1].children[j].IsOp("cos"))
                    {
                        if (children[0].children[i].children[0] == children[1].children[j].children[0])
                        {
                            children[0].children[i] = Tan(children[0].children[i].children[0]);
                            children[1].children.RemoveAt(j);
                            if (children[1].children.Count == 1)
                            {
                                children[1] = children[1].children[0];
                            }
                            else if (children[1].children.Count == 0)
                            {
                                children[1] = new Expression(1);
                            }
                            if (children[0].children.Count == 1)
                            {
                                children[0] = children[0].children[0];
                            }
                            else if (children[0].children.Count == 0)
                            {
                                children[0] = new Expression(1);
                            }
                            return;
                        }
                    }
                    else if (children[0].children[i].IsOp("cos") && children[1].children[j].IsOp("sin"))
                    {
                        if (children[0].children[i].children[0] == children[1].children[j].children[0])
                        {
                            children[1].children[j] = Tan(children[0].children[i].children[0]);
                            children[0].children.RemoveAt(i);
                            if (children[1].children.Count == 1)
                            {
                                children[1] = children[1].children[0];
                            }
                            else if (children[1].children.Count == 0)
                            {
                                children[1] = new Expression(1);
                            }
                            if (children[0].children.Count == 1)
                            {
                                children[0] = children[0].children[0];
                            }
                            else if (children[0].children.Count == 0)
                            {
                                children[0] = new Expression(1);
                            }
                            return;
                        }
                    }
                    else if (children[0].children[i].IsOp("^") && children[1].children[j].IsOp("^"))
                    {
                        Expression exponantTop = children[0].children[i].children[1];
                        Expression exponantBottom = children[1].children[j].children[1];

                        Expression baseTop = children[0].children[i].children[0];
                        Expression baseBottom = children[1].children[j].children[0];

                        if (exponantTop != exponantBottom)
                        {
                            continue;
                        }
                        if (baseTop.IsOp("sin") && baseBottom.IsOp("cos"))
                        {
                            if (baseTop.children[0] == baseBottom.children[0])
                            {
                                children[0].children[i] = Tan(baseTop.children[0]) ^ exponantTop;
                                children[1].children.RemoveAt(j);
                                if (children[1].children.Count == 1)
                                {
                                    children[1] = children[1].children[0];
                                }
                                else if (children[1].children.Count == 0)
                                {
                                    children[1] = new Expression(1);
                                }
                                if (children[0].children.Count == 1)
                                {
                                    children[0] = children[0].children[0];
                                }
                                else if (children[0].children.Count == 0)
                                {
                                    children[0] = new Expression(1);
                                }
                                return;
                            }
                        }
                        else if ((baseTop.IsOp("sin") && baseBottom.IsOp("cos"))
                            || (baseTop.IsOp("cos") && baseBottom.IsOp("sin")))
                        {
                            if (baseTop.children[0] == baseBottom.children[0])
                            {
                                Expression tanExpression = baseTop / baseBottom;
                                tanExpression.SinOverCosToTan();
                                CloneFrom(tanExpression ^ exponantTop);
                                return;
                            }
                        }
                    }
                }
            }
            if (children[0].children.Count == 1)
            {
                children[0] = children[0].children[0];
            }
            if (children[1].children.Count == 1)
            {
                children[1] = children[1].children[0];
            }
            if (children[1] == 1)
            {
                CloneFrom(children[0]);
            }

        }
        private void SimplifyTrig()
        {
            if (isLeaf)
            {
                return;
            }
            List<Expression> insideTrig = GetTrigExpressions().Select(x => x.children[0]).ToList();
            List<Expression> alreadyTested = new List<Expression>();

            if (op.Symbol != "+")
            {
                return;
            }
            foreach (Expression insideExpression in insideTrig)
            {
                if (alreadyTested.Contains(insideExpression))
                {
                    continue;
                }
                List<Expression> sinSquareds = GetMultiplesOfExpression(Sin(insideExpression) ^ 2);
                List<Expression> cosSquareds = GetMultiplesOfExpression(Cos(insideExpression) ^ 2);


                if (sinSquareds.Count == 1 && cosSquareds.Count == 0)
                {
                    if (children.Count == 2)
                    {
                        if (Equals((Sin(insideExpression) ^ 2) + -1))
                        {
                            CloneFrom(-(Cos(insideExpression) ^ 2));
                            return;
                        }
                        else if (Equals(1 + -(Sin(insideExpression) ^ 2)))
                        {
                            CloneFrom(Cos(insideExpression) ^ 2);
                            return;
                        }
                    }
                    return;
                }
                else if (cosSquareds.Count == 1 && sinSquareds.Count == 0)
                {
                    if (children.Count == 2)
                    {
                        if (Equals((Cos(insideExpression) ^ 2) + (-1)))
                        {
                            CloneFrom(-(Sin(insideExpression) ^ 2));
                            return;
                        }
                        else if (Equals(1 + -(Cos(insideExpression) ^ 2)))
                        {
                            CloneFrom(Sin(insideExpression) ^ 2);
                            return;
                        }
                    }
                    return;
                }

                else if (sinSquareds.Count != 1 || cosSquareds.Count != 1)
                {
                    //throw new Exception("More than one sin^2 or cos^2");
                    continue;
                }


                Expression sinCoefficient;
                if (sinSquareds[0].op.Symbol == "*")
                {
                    sinCoefficient = sinSquareds[0] / (Sin(insideExpression) ^ 2);
                    sinCoefficient.ExpandAndSimplify();
                }
                else
                {
                    sinCoefficient = new Expression(1);
                }
                Expression cosCoefficient;
                if (cosSquareds[0].op.Symbol == "*")
                {
                    cosCoefficient = cosSquareds[0] / (Cos(insideExpression) ^ 2);
                    cosCoefficient.ExpandAndSimplify();
                }
                else
                {
                    cosCoefficient = new Expression(1);
                }

                if (sinCoefficient.Equals(cosCoefficient))
                {
                    for (int i = 0; i < children.Count; i++)
                    {
                        if (children[i].Equals(sinSquareds[0]) || children[i].Equals(cosSquareds[0]))
                        {
                            children.RemoveAt(i);
                            i--;
                        }
                    }
                    children.Add(sinCoefficient);
                    if (children.Count == 1)
                    {
                        CloneFrom(children[0]); 
                    }
                }

                alreadyTested.Add(insideExpression);
            }
        }
        private void SimplifyTrigExactValues()
        {
            if (isLeaf)
            {
                return;
            }
            if (op.Symbol == "sin")
            {
                //sin(-x) = -sin(x)
                Expression child = children[0].Clone() as Expression;
                List<Expression> factors = CommonFactors(new List<Expression>() { new Expression(-1), children[0].Clone() as Expression });
                if (factors.Count > 0)
                {
                    children[0] = -children[0];
                    children[0].ExpandAndSimplifyOneLayer();
                    CloneFrom(-this);
                    return;
                }


                for (int i = 0; i < Utils.sinExactValues.Count; i++)
                {
                    if (children[0] == Utils.sinExactValues[i].Item1)
                    {
                        CloneFrom(Utils.sinExactValues[i].Item2);
                        return;

                    }
                }
            }
            else if (op.Symbol == "cos")
            {
                //cos(-x) = cos(x)
                List<Expression> factors = CommonFactors(new List<Expression>() { new Expression(-1), children[0].Clone() as Expression });
                if (factors.Count > 0)
                {
                    children[0] = -children[0];
                    children[0].ExpandAndSimplifyOneLayer();
                    return;
                }

                for (int i = 0; i < Utils.cosExactValues.Count; i++)
                {
                    if (children[0] == Utils.cosExactValues[i].Item1)
                    {
                        CloneFrom(Utils.cosExactValues[i].Item2);
                        return;
                    }
                }

                //if (children[0].isNumeric)
            }
            else if (op.Symbol == "tan")
            {
                //tan(-x) = -tan(x)
                Expression child = children[0].Clone() as Expression;
                List<Expression> factors = CommonFactors(new List<Expression>() { new Expression(-1), children[0].Clone() as Expression });
                if (factors.Count > 0)
                {
                    children[0] = -children[0];
                    children[0].ExpandAndSimplifyOneLayer();
                    CloneFrom(-this);
                    return;
                }

                for (int i = 0; i < Utils.sinExactValues.Count; i++)
                {
                    Expression exactValue = Utils.sinExactValues[i].Item1;
                    if (children[0] == exactValue)
                    {
                        CloneFrom(Utils.sinExactValues[i].Item2 / Utils.cosExactValues[i].Item2);
                        Simplify();
                        return;
                    }
                }
            }
            else if (op.Symbol == "arcsin")
            {
                //arcsin(-x) = -arcsin(x)
                Expression child = children[0].Clone() as Expression;
                List<Expression> factors = CommonFactors(new List<Expression>() { new Expression(-1), children[0].Clone() as Expression });
                if (factors.Count > 0)
                {
                    children[0] = -children[0];
                    children[0].ExpandAndSimplifyOneLayer();
                    CloneFrom(-this);
                    return;
                }

                for (int i = 0; i < Utils.sinExactValues.Count; i++)
                {
                    if (children[0] == Utils.sinExactValues[i].Item2)
                    {
                        CloneFrom(Utils.sinExactValues[i].Item1);
                        return;
                    }
                }
            }
            else if (op.Symbol == "arccos")
            {
                for (int i = 0; i < Utils.cosExactValues.Count; i++)
                {
                    if (children[0] == Utils.cosExactValues[i].Item2)
                    {
                        CloneFrom(Utils.cosExactValues[i].Item1);
                        return;

                    }
                }
            }
            else if (op.Symbol == "arctan")
            {
                for (int i = 0; i < Utils.sinExactValues.Count; i++)
                {
                    Expression exactValue = Utils.sinExactValues[i].Item2 / Utils.cosExactValues[i].Item2;
                    exactValue.Simplify();
                    if (children[0] == exactValue)
                    {
                        CloneFrom(exactValue);
                        return;
                    }
                }
            }
        }
        public void SeparateFraction()
        {
            if (isLeaf || op.Symbol != "/")
            {
                return;
                //throw new Exception("function called for wrong operator");
            }

            Expression numerator = children[0];
            Expression denominator = children[1];
            if (!numerator.IsOp("+"))
            {
                return;
            }
            List<Expression> newChildren = new List<Expression>();
            for (int i = 0; i < numerator.children.Count; i++)
            {
                Expression newNumerator = numerator.children[i];
                Expression newDenominator = denominator.Clone() as Expression;
                newChildren.Add(newNumerator / newDenominator);
            }
            children = newChildren;
            op = Utils.operators["+"];
        }
    }
}