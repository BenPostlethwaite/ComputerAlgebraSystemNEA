using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Linq;
using System.Net.Http.Headers;
using System.Reflection.Metadata;
using System.Runtime.CompilerServices;
using System.Security.AccessControl;

namespace MathsLibrary
{
    public static class Utils
    {        
        public static Dictionary<string, IOperator> operators = new Dictionary<string, IOperator>
        {
            { "+", new Add() },
            { "-", new Subtract() },
            { "*", new Multiply() },
            { "/", new Divide() },
            { "^", new Power() },
        };
        public static Dictionary<string, IOperator> functions = new Dictionary<string, IOperator>
        {
            //arcsin and arccos must be above sin and cos due to the way the parser works
            {"log", new Log()},
            {"arcsin", new ArcSin() },
            {"arccos", new ArcCos() },
            {"arctan", new ArcTan() },
            {"sin", new Sin() },
            {"cos", new Cos() },
             {"tan", new Tan() }
        };       
        public static Dictionary<string, Equation> suvats = new Dictionary<string, Equation>()
        {
            { "atuv", new Equation("v=u+at") },
            { "asuv", new Equation("v^2=u^2+2as")},
            { "astu", new Equation("s=ut+(1/2)at^2") },
            { "astv", new Equation("s=vt-(1/2)at^2")},
            { "stuv", new Equation("s=(1/2)(v+u)t") }
        };
        public static List<(Expression, Expression)> sinExactValues = new List<(Expression, Expression)>()
        {
            (new Expression(0), new Expression(0)),
            (new Expression("π/6", "infix"), new Expression("1/2", "infix")),
            (new Expression("π/4", "infix"), new Expression("1/(2^(1/2))", "infix")),
            (new Expression("π/3", "infix"), new Expression("3^(1/2)/2", "infix")),
            (new Expression("π/2", "infix"), new Expression(1)),
            (new Expression("2π/3", "infix"), new Expression("3^(1/2)/2", "infix")),
            (new Expression("3π/4", "infix"), new Expression("1/(2^(1/2))", "infix")),
            (new Expression("5π/6", "infix"), new Expression("1/2", "infix")),
            (new Expression("π", "infix"), new Expression("0", "infix"))
        };
        public static List<(Expression, Expression)> cosExactValues = new List<(Expression, Expression)>()
        {
            ( new Expression(0), new Expression(1)),
            (new Expression("π/6", "infix"), new Expression("3^(1/2)/2", "infix") ),
            (new Expression("π/4", "infix"), new Expression("1/(2^(1/2))", "infix")),
            (new Expression("π/3", "infix"), new Expression("1/2", "infix")),
            (new Expression("π/2", "infix"), new Expression(0)),
            (new Expression("2π/3", "infix"), new Expression("-1/2", "infix")),
            (new Expression("3π/4", "infix"), new Expression("-1/(2^(1/2))", "infix")),
            (new Expression("5π/6", "infix"), new Expression("-3^(1/2)/2", "infix")),
            (new Expression("π", "infix"), new Expression("-1", "infix"))
        };
        public static Expression pi = new Expression("π", "infix");
        public static Expression i = new Expression("i", "infix");
        public static List<Equation> SolveSuvat(char unknown, Dictionary<char, Expression> known)
        {
            List<char> variables = known.Keys.ToList();
            variables.Add(unknown);
            for (int i = 0; i < variables.Count; i++)
            {
                variables[i] = char.ToLower(variables[i]);
            }

            variables.Sort();

            if (variables.Count != 4)
            {
                throw new ArgumentException("Incorrect number of variables");
            }
            Equation suvat = suvats[string.Join("", variables)].Clone() as Equation;

            foreach (var pair in known)
            {
                suvat.Substitute(pair.Key.ToString(), pair.Value);
            }
            return suvat.Solve(unknown.ToString());
            
        }
        public static List<Equation> GetQuadraticEquations(Expression var, Dictionary<int, Expression> info)
        {
            Expression a = info[2];
            Expression b = info[1];
            Expression c = info[0];

            List<Equation> toReturn = new List<Equation>();
            Expression descriminent = (b^2) - (4 * a * c);
            Expression right1 = (-b + (descriminent^(new Expression("1/2", "infix")))) / (2 * a);
            Expression right2 = (-b - (descriminent^(new Expression("1/2", "infix")))) / (2 * a);
            toReturn.Add(new Equation(var, right1));
            toReturn.Add(new Equation(var, right2));
            return toReturn;
        }
        public static int NcR(int n, int r)
        {
            if (r > n || r < 0)
            {
                return 0;
            }
            if (r == 0 || r == n)
            {
                return 1;
            }
            return NcR(n - 1, r - 1) + NcR(n - 1, r);
        }
        public static List<List<T>> GetCombinations<T>(List<List<T>> list)
        {
            //find every combination of the elements in the list
            List<List<T>> combinations = new List<List<T>>();
            //base case: if there is only one list in the list, return the list
            if (list.Count == 1)
            {
                foreach (T element in list[0])
                {
                    combinations.Add(new List<T>() { element });
                }
                return combinations;
            }
            // recursively find the combinations of the rest of the list
            else
            {
                List<List<T>> subCombinations = GetCombinations(list.GetRange(1, list.Count - 1));
                foreach (T element in list[0])
                {
                    foreach (List<T> subCombination in subCombinations)
                    {
                        List<T> combination = new List<T>();
                        combination.Add(element);
                        combination.AddRange(subCombination);
                        combinations.Add(combination);
                    }
                }
                return combinations;
            }
        }     
        public static bool IsOperator(string s)
        {
           return operators.ContainsKey(s);
        }
        public static bool IsFunction(string s)
        {
            return functions.ContainsKey(s);
        }
        public static bool IsAssociative(string s)
            => operators[s].Associative;
        public static int GetPrecedence(string s)
        {
           return operators[s].Precedence;
        }
        public const string SuperscriptDigits =
    "\u2070\u00b9\u00b2\u00b3\u2074\u2075\u2076\u2077\u2078\u2079";
        
    }
    public interface ILatexPrintable
    {
        public string ToLatex();
    }
    public interface IOperator
    {
        public string Symbol { get; }
        public string latex { get; }
        public bool Commutative { get; }
        public bool Associative { get; }
        public int Precedence { get; }
        public int OperandNumber { get; }
        public IOperator Inverse { get; }
        public string ToString();
        public double Evaluate(List<double> operands);
        public bool Equals(object other);
        public Expression Differentiate(Expression expression, Expression var);
    }
    public interface ITrigFunction
    {
        public List<Equation> GetInverse(Equation equation);
    }
    public class Add : IOperator
    {
        public string Symbol { get { return "+"; } }
        public string latex { get { return "+"; } }
        public bool Commutative { get { return true; } }
        public bool Associative { get { return true;  }}
        public int Precedence { get { return 1; } }
        public int OperandNumber { get { return 2; } }
        public IOperator Inverse { get { return new Subtract(); } }
        public override string ToString() { return Symbol; }
        public double Evaluate(List<double> operands)
        {
            double sum = 0;
            foreach (double operand in operands)
            {
                sum += operand;
            }
            return sum;
        }
        public override bool Equals(object other)
        {
            return other is Add;
        }
        public Expression Differentiate(Expression expression, Expression var)
        {
            Expression copy = expression.Clone() as Expression;
            for (int i = 0; i < copy.children.Count(); i++)
            {
                copy.children[i] = copy.children[i].Differentiate(var);
            }
            return copy;
        }
    }
    public class Subtract : IOperator
    {
        public string Symbol { get { return "-"; } }
        public string latex { get { return "-"; } }
        public bool Commutative { get { return false; } }
        public bool Associative { get { return false; } }
        public int Precedence { get { return 1; } }
        public int OperandNumber { get { return 2; } }
        public IOperator Inverse { get { return new Add(); } }
        public override string ToString() { return Symbol; }
        public double Evaluate(List<double> operands)
        {
            double result = operands[0];
            for (int i = 1; i < operands.Count; i++)
            {
                result -= operands[i];
            }
            return result;
        }
        public override bool Equals(object other)
        {
            return other is Subtract;
        }
        public Expression Differentiate(Expression expression, Expression var)
        {
            Expression copy = expression.Clone() as Expression;
            for (int i = 0; i < copy.children.Count(); i++)
            {
                copy.children[i] = copy.children[i].Differentiate(var);
            }
            return copy;
        }
    }
    public class Multiply : IOperator
    {
        public string Symbol { get { return "*"; } }
        public string latex { get { return @"\cdot"; } }
        public bool Commutative { get { return true; } }
        public bool Associative { get { return true; } }
        public int Precedence { get { return 2; } }
        public int OperandNumber { get { return 2; } }
        public IOperator Inverse { get { return new Divide(); } }
        public override string ToString() { return Symbol; }
        public double Evaluate(List<double> operands)
        {
            double result = 1;
            foreach (double operand in operands)
            {
                result *= operand;
            }
            return result;
        }
        public override bool Equals(object other)
        {
            return other is Multiply;
        }
        public Expression Differentiate(Expression expression, Expression variable)
        {
            Expression copy = expression.Clone() as Expression;
            Expression toReturn = new Expression(new Add(), new List<Expression>());

            List<Expression> operandDerivatives = expression.children.Select(operand => operand.Differentiate(variable)).ToList();
            for (int i = 0; i < expression.children.Count; i++)
            {
                Expression part = new Expression(this, new List<Expression> { operandDerivatives[i] });
                for (int j = 0; j < operandDerivatives.Count; j++)
                {
                    if (i == j)
                    {
                        continue;
                    }
                    part.children.Add(expression.children[j]);
                }
                toReturn.children.Add(part);
            }
            return toReturn;
        }
    }
    public class Divide : IOperator
    {
        public string Symbol { get { return "/"; } }
        public string latex { get { return @"\frac"; } }
        public bool Commutative { get { return false; } }
        public bool Associative { get { return false; } }
        public int Precedence { get { return 3; } }
        public int OperandNumber { get { return 2; } }
        public IOperator Inverse { get { return new Multiply(); } }
        public override string ToString() { return Symbol; }
        public double Evaluate(List<double> operands)
        {
            double result = operands[0];
            for (int i = 1; i < operands.Count; i++)
            {
                result /= operands[i];
            }
            return result;
        }
        public override bool Equals(object other)
        {
            return other is Divide;
        }
        public Expression Differentiate(Expression expression, Expression var)
        {
            Expression copy = expression.Clone() as Expression;
            if (copy.children.Count() != 2)
            {
                throw new Exception("Incorrect number of operands");
            }

            //quotient rule
            Expression numerator = copy.children[0];
            Expression denominator = copy.children[1];
            Expression numeratorDerivative = numerator.Differentiate(var);
            Expression denominatorDerivative = denominator.Differentiate(var);
            Expression newNumerator = (numeratorDerivative * denominator) - (numerator * denominatorDerivative);
            Expression newDenominator = denominator * denominator;

            return newNumerator / newDenominator;
        }
    }
    public class Power : IOperator
    {
        public string Symbol { get { return "^"; } }
        public string latex { get { return "^"; } }
        public bool Commutative { get { return false; } }
        public bool Associative { get { return false; } }
        public int Precedence { get { return 4; } }
        public int OperandNumber { get { return 2; } }
        public IOperator Inverse { get { return null; } }
        public override string ToString() { return Symbol; }
        public double Evaluate(List<double> operands)
        {
            double result = operands[0];
            for (int i = 1; i < operands.Count; i++)
            {
                result = Math.Pow(result, operands[i]);
            }
            return result;
        }
        public override bool Equals(object other)
        {
            return other is Power;
        }
        public Expression Differentiate(Expression expression, Expression var)
        {
            Expression copy = expression.Clone() as Expression;
            if (copy.children.Count() != 2)
            {
                throw new Exception("Incorrect number of operands");
            }

            Expression expressionBase = copy.children[0];
            Expression exponent = copy.children[1];


            Expression e = new Expression("e");
            if (copy.children[0] == e)
            {
                //(e^f(x)) => e^f(x) * f'(x)
                Expression f = exponent;
                Expression fDerivative = f.Differentiate(var);
                return copy * fDerivative;
            }
            else
            {
                Expression newExponant = copy.children[1] * Expression.Log(e, expressionBase);
                copy = e ^ newExponant;
                return copy.Differentiate(var);
            }
        }

    }
    public class Log : IOperator
    {
        public string Symbol { get { return "log"; } }
        public string latex { get { return @"\log"; } }
        public bool Commutative { get { return false; } }
        public bool Associative { get { return false; } }
        public int Precedence { get { return 5; } }
        public int OperandNumber { get { return 2; } }
        public IOperator Inverse { get { return null; } }
        public override string ToString() { return Symbol; }
        public double Evaluate(List<double> operands)
        {
            if (operands.Count != 2)
            {
                throw new ArgumentException("Log takes exactly two arguments");
            }
            return Math.Log(operands[1], operands[0]);
        }
        public override bool Equals(object other)
        {
            return other is Log;
        }
        public Expression Differentiate(Expression expression, Expression var)
        {
            Expression copy = expression.Clone() as Expression;
            Expression argument = copy.children[1];
            Expression logBase = copy.children[0];
            Expression e = new Expression("e");

            if (logBase == e)
            {
                return argument.Differentiate(var) / copy.children[1];
            }
            else
            {
                copy = Expression.Log(e, argument) / Expression.Log(e, logBase);
                return copy.Differentiate(var);
            }
        }
    }
    public class Sin : IOperator, ITrigFunction
    {
        public string Symbol { get { return "sin"; } }
        public string latex { get { return @"\sin"; } }
        public bool Commutative { get { return false; } }
        public bool Associative { get { return false; } }
        public int Precedence { get { return 5; } }
        public int OperandNumber { get { return 1; } }
        public IOperator Inverse { get { return new ArcSin(); } }
        public List<Equation> GetInverse(Equation equationInput)
        {
            Expression n1 = new Expression("n_{1}");

            List<Equation> toReturn = new List<Equation>();
            Expression right1 = Expression.Arcsin(equationInput.right) 
                + (new Expression("2*π", "infix") * n1);

            Expression right2 = (new Expression("2*π", "infix") * n1)
                + new Expression("π") - Expression.Arcsin(equationInput.right);


            right1.Simplify();
            right2.Simplify();

            toReturn.Add(new Equation(equationInput.left.children[0], right1));
            toReturn.Add(new Equation(equationInput.left.children[0], right2));
            return toReturn;

        }
        public override string ToString() { return Symbol; }
        public double Evaluate(List<double> operands)
        {
            if (operands.Count != 1)
            {
                throw new ArgumentException("Sin takes exactly one argument");
            }
            double toReturn = Math.Sin(operands[0]);
            //round so trig of pi works
            toReturn = Math.Round(toReturn, 10);
            return toReturn;
        }
        public override bool Equals(object other)
        {
            return other is Sin;
        }
        public Expression Differentiate(Expression expression, Expression var)
        {
            Expression copy = expression.Clone() as Expression;
            Expression child = copy.children[0];

            return child.Differentiate(var) * Expression.Cos(child);
        }
    }
    public class Cos : IOperator, ITrigFunction
    {
        public string Symbol { get { return "cos"; } }
        public string latex { get { return @"\cos"; } }
        public bool Commutative { get { return false; } }
        public bool Associative { get { return false; } }
        public int Precedence { get { return 5; } }
        public int OperandNumber { get { return 1; } }
        public IOperator Inverse { get { return new ArcCos(); } }

        public List<Equation> GetInverse(Equation equationInput)
        {
            Expression n1 = new Expression("n_{1}");

            List<Equation> toReturn = new List<Equation>();
            Expression right1 = Expression.Arccos(equationInput.right)
            + (new Expression("2*π", "infix") * n1);

            Expression right2 = (new Expression("2*π", "infix") * n1)
            - Expression.Arccos(equationInput.right);


            right1.Simplify();
            right2.Simplify();

            toReturn.Add(new Equation(equationInput.left.children[0], right1));
            toReturn.Add(new Equation(equationInput.left.children[0], right2));
            return toReturn;
        }
        public override string ToString() { return Symbol; }
        public double Evaluate(List<double> operands)
        {
            if (operands.Count != 1)
            {
                throw new ArgumentException("Cos takes exactly one argument");
            }
            double toReturn = Math.Sin(operands[0]);
            //round so trig of pi works
            toReturn = Math.Round(toReturn, 10);
            return toReturn;
        }
        public override bool Equals(object other)
        {
            return other is Cos;
        }
        public Expression Differentiate(Expression expression, Expression var)
        {
            Expression copy = expression.Clone() as Expression;
            Expression child = copy.children[0];

            return child.Differentiate(var) * -Expression.Sin(child);
        }
    }
    public class Tan : IOperator, ITrigFunction
    {
        public string Symbol { get { return "tan"; } }
        public string latex { get { return @"\tan"; } }
        public bool Commutative { get { return false; } }
        public bool Associative { get { return false; } }
        public int Precedence { get { return 5; } }
        public int OperandNumber { get { return 1; } }
        public IOperator Inverse { get { return new ArcTan(); } }
        public List<Equation> GetInverse(Equation equationInput)
        {
            Expression n1 = new Expression("n_{1}");

            List<Equation> toReturn = new List<Equation>();
            Expression right1 = Expression.Arctan(equationInput.right)
            + (new Expression("π", "infix") * n1);


            right1.Simplify();

            toReturn.Add(new Equation(equationInput.left.children[0], right1));
            return toReturn;
        }
        public override string ToString() { return Symbol; }
        public double Evaluate(List<double> operands)
        {
            if (operands.Count != 1)
            {
                throw new ArgumentException("Tan takes exactly one argument");
            }
            return Math.Tan(operands[0]);
        }
        public override bool Equals(object other)
        {
            return other is Tan;
        }
        public Expression Differentiate(Expression expression, Expression var)
        {
            Expression child = expression.children[0];
            return child.Differentiate(var) / (Expression.Cos(var) ^ 2);
        }
    }
    public class ArcSin : IOperator
    {
        public string Symbol { get { return "arcsin"; } }
        public string latex { get { return @"\arcsin"; } }
        public bool Commutative { get { return false; } }
        public bool Associative { get { return false; } }
        public int Precedence { get { return 5; } }
        public int OperandNumber { get { return 1; } }
        public IOperator Inverse { get { return new Sin(); } }
        public override string ToString() { return Symbol; }
        public double Evaluate(List<double> operands)
        {
            if (operands.Count != 1)
            {
                throw new ArgumentException("ArcSin takes exactly one argument");
            }
            return Math.Asin(operands[0]);
        }
        public override bool Equals(object other)
        {
            return other is ArcSin;
        }
        public Expression Differentiate(Expression expression, Expression var)
        {
            Expression copy = expression.Clone() as Expression;
            Expression u = copy.children[0];
            return u.Differentiate(var) / ((1 - (u ^ 2)) ^ new Expression("1 / 2", "infix"));
        }
       
    }
    public class ArcCos : IOperator
    {
        public string Symbol { get { return "arccos"; } }
        public string latex { get { return @"\arccos"; } }
        public bool Commutative { get { return false; } }
        public bool Associative { get { return false; } }
        public int Precedence { get { return 5; } }
        public int OperandNumber { get { return 1; } }
        public IOperator Inverse { get { return new Cos(); } }
        public override string ToString() { return Symbol; }
        public double Evaluate(List<double> operands)
        {
            if (operands.Count != 1)
            {
                throw new ArgumentException("ArcCos takes exactly one argument");
            }
            return Math.Acos(operands[0]);
        }
        public override bool Equals(object other)
        {
            return other is ArcCos;
        }
        public Expression Differentiate(Expression expression, Expression var)
        {
            Expression copy = expression.Clone() as Expression;
            Expression u = copy.children[0];
            return -u.Differentiate(var) / ((1 - (u ^ 2)) ^ new Expression("1 / 2", "infix"));
        }
    }
    public class ArcTan : IOperator
    {
        public string Symbol { get { return "arctan"; } }
        public string latex { get { return @"\arctan"; } }
        public bool Commutative { get { return false; } }
        public bool Associative { get { return false; } }
        public int Precedence { get { return 5; } }
        public int OperandNumber { get { return 1; } }       
        public IOperator Inverse { get { return new Tan(); } }
        public override string ToString() { return Symbol; }
        public double Evaluate(List<double> operands)
        {
            if (operands.Count != 1)
            {
                throw new ArgumentException("ArcTan takes exactly one argument");
            }
            return Math.Atan(operands[0]);
        }
        public override bool Equals(object other)
        {
            return other is ArcTan;
        }
        public Expression Differentiate(Expression expression, Expression var)
        {
            Expression copy = expression.Clone() as Expression;
            Expression u = copy.children[0];
            return u.Differentiate(var) / ((u ^ 2 + 1) ^ new Expression("1 / 2", "infix"));
        }
    }
}

