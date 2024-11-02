using FinalFormsApp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace MathsLibrary
{
    public partial class Expression : ICloneable, MatrixOperand, ILatexPrintable
    {
        public bool isLeaf => children.Count == 0;
        public bool isVariable => isLeaf && value is Variable;
        public bool isDouble => isLeaf && value is Number;
        public bool isConstant => isLeaf && value is Constant;
        public bool isInt => isDouble && ToDouble() % 1 == 0;
        public bool isNatural => isInt && ToDouble() > 0;
        public bool isNumeric
        {
            get
            {
                if (isLeaf)
                {
                    return value is iHasNumericValue;
                }
                else
                {
                    return children.All(child => child.isNumeric);
                }
            }
        }

        public bool? isPositive
        {
            get
            {
                Expression copy = Clone() as Expression;

                if (copy.isDouble)
                {
                    return copy.ToDouble() > 0;
                }
                else if (isLeaf)
                {
                    //is variable
                    return true;
                }
                else if (copy.op.Symbol == "+")
                {
                    if (children.All(c => c.isPositive == true))
                    {
                        return true;
                    }
                    else if (children.All(c => c.isPositive == false))
                    {
                        return false;
                    }
                    else
                    {
                        return null;
                    }
                }
                else if (copy.op.Symbol == "-")
                {
                    for (int i = 1; i < copy.children.Count; i++)
                    {
                        if (copy.children[i].isPositive != true)
                        {
                            return null;
                        }
                    }
                    return true;
                }
                else if (copy.op.Symbol == "/")
                {
                    if (copy.children[0].isPositive == null || copy.children[1].isPositive == null)
                    {
                        return null;
                    }
                    return copy.children[0].isPositive == copy.children[1].isPositive;
                }
                else if (copy.op.Symbol == "log")
                {
                    return null; ;
                }
                else if (copy.op.Symbol == "*")
                {
                    copy.GeneralSimplify();
                    copy.ConstantsToPrimeFactors();
                    copy.CombineAssociativeOperators();
                    if (copy.children.Contains(new Expression(-1)))
                    {
                        return false;
                    }
                    else
                    {
                        return true;
                    }
                }
                else if (copy.op.Symbol == "^")
                {
                    if (copy.children[0].isPositive == true)
                    {
                        return true;
                    }

                    {
                        return null;
                    }
                }
                else
                {
                    throw new Exception("unknown operation");
                }
            }
        }
        public bool IsOp(string symbol)
        {
            return !isLeaf && op.Symbol == symbol;
        }
        public double ToDouble()
        {                        
            if (isLeaf)
            {
                if (value is iHasNumericValue)
                {
                    return (value as iHasNumericValue).value;
                }
                else
                {
                    throw new Exception("variable has no numeric value");
                }
            }
            else
            {
                List<double> listValues = new List<double>();

                for (int i = 0; i < children.Count(); i++)
                {
                    try
                    {
                        listValues.Add(children[i].ToDouble());
                    }
                    catch (Exception e)
                    {
                        throw e;
                    }
                }                

                return op.Evaluate(listValues);
            }
        }        
        public Expression GetCoefficient(Expression variable)
        {
            if (isLeaf)
            {
                if (Equals(variable))
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
                if (op.Symbol == "*")
                {
                    if (children.Any(c => c == variable))
                    {
                        List<Expression> coefficientChildren = children.Where(c => c != variable).ToList();
                        Expression coefficient = new Expression();
                        coefficient.op = Utils.operators["*"];
                        coefficient.children = coefficientChildren;
                        if (coefficient.children.Count() == 1)
                        {
                            coefficient = coefficient.children[0];
                        }
                        return coefficient;
                    }
                    else
                    {
                        return new Expression(0);
                    }
                }
                else if (op.Symbol == "+")
                {
                    if (children.Any(c => c.ContainsVariable(variable)))
                    {
                        Expression toReturn = new Expression(0);
                        foreach (Expression child in children)
                        {
                            if (child.ContainsVariable(variable))
                            {
                                toReturn += child.GetCoefficient(variable);
                            }
                        }
                        toReturn.Simplify();
                        return toReturn;
                    }
                    else
                    {
                        return new Expression(0);
                    }
                }
                else
                {
                    throw new Exception("Not implemented");
                }
            }
        }
        public Expression GetPolynomialDegree(Expression variable)
        {
            if (!ContainsVariable(variable))
            {
                return new Expression(0);
            }
            if (isLeaf)
            {
                return new Expression(1);
            }
            if (op.Symbol == "^")
            {
                if (children[0] == variable)
                {
                    return children[1];
                }
                else
                {
                    throw new Exception("not polynomial");
                }
            }
            return new Expression(0);
        }
        public (Expression coefficient, Expression degree) GetPolynomialDegreeAndCoefficient(Expression variable)
        {
            if (!ContainsVariable(variable))
            {
                return (this, new Expression(0));
            }
            if (isLeaf)
            {
                return (new Expression(1), new Expression(1));
            }
            if (op.Symbol == "^")
            {
                if (children[0] == variable)
                {
                    if (!children[1].isNumeric)
                    {
                        throw new Exception("not polynomial");
                    }
                    return (new Expression(1), children[1]);
                }
                else
                {
                    throw new Exception("not polynomial");
                }
            }
            if (IsOp("*"))
            {
                List<Expression> variableChildren = children.Where(c => c.ContainsVariable(variable)).ToList();
                if (variableChildren.Count() == 1)
                {
                    Expression exponent = variableChildren[0].GetPolynomialDegree(variable);
                    if (exponent != 0)
                    {
                        List<Expression> coefficientchildren = children.Where(c => !c.ContainsVariable(variable)).ToList();
                        Expression coefficient = new Expression(Utils.operators["*"], coefficientchildren);
                        if (coefficient.children.Count() == 1)
                        {
                            coefficient = coefficient.children[0];
                        }
                        return (coefficient, exponent);
                    }
                    else
                    {
                        return (this, new Expression(0));
                    }
                }
                else
                {
                    throw new NotImplementedException("not polynomial");
                }
            }
            else
            {
                throw new Exception("error");
            }
        }
        public Dictionary<int,Expression> getPolynomialInfo(Expression variable)
        {
            Dictionary<int, Expression> toReturn = new Dictionary<int, Expression>();            
            //eg: 2x^3+3x
            if (IsOp("+"))
            {
                foreach (Expression child in children)
                {
                    Dictionary<int, Expression> childCoefficients = child.getPolynomialInfo(variable);
                    foreach (KeyValuePair<int, Expression> coefficient in childCoefficients)
                    {
                        if (toReturn.ContainsKey(coefficient.Key))
                        {
                            toReturn[coefficient.Key] += coefficient.Value;
                        }
                        else
                        {
                            toReturn.Add(coefficient.Key, coefficient.Value);
                        }
                    }
                }                
            }
            else if (IsOp("/"))
            {
                SeparateFraction();
                if (IsOp("/"))
                {
                    CloneFrom(children[0] * (1 / children[1]));
                    CombineAssociativeOperators();                    
                }
                return getPolynomialInfo(variable);                
            }
            else
            {
                try
                {
                    (Expression coefficient, Expression degree) = GetPolynomialDegreeAndCoefficient(variable);
                    int intDegree = (int)degree.ToDouble();
                    if (toReturn.ContainsKey(intDegree))
                    {
                        toReturn[intDegree] += coefficient;
                    }
                    else
                    {
                        toReturn.Add(intDegree, coefficient);
                    }
                }
                catch (Exception e)
                {
                    throw e;
                }
            }
            return toReturn;
        }
        public bool ContainsVariable(Expression var)
        {
            if (isLeaf)
            {
                return Equals(var);
            }
            foreach (Expression child in children)
            {
                if (child.ContainsVariable(var))
                {
                    return true;
                }
            }
            return false;
        }
        public bool ContainsVariable(string var)
        {
            return ContainsVariable(new Expression(var));
        }
    }
}
