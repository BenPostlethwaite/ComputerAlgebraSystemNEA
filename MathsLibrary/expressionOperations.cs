using FinalFormsApp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace MathsLibrary
{
    public partial class Expression : ICloneable, MatrixOperand, ILatexPrintable
    {
        public static Expression operator +(Expression a, Expression b)
        {
            Expression toReturn = new Expression(Utils.operators["+"], new List<Expression>() { a, b });
            return toReturn.Clone() as Expression;
        }
        public static Expression operator -(Expression a, Expression b)
        {
            Expression toReturn = new Expression(Utils.operators["-"], new List<Expression>() { a, b });
            return toReturn.Clone() as Expression;
        }
        public static Expression operator *(Expression a, Expression b)
        {
            Expression toReturn = new Expression(Utils.operators["*"], new List<Expression>() { a, b });
            return toReturn.Clone() as Expression;
        }
        public static Expression operator /(Expression a, Expression b)
        {
            Expression toReturn = new Expression(Utils.operators["/"], new List<Expression>() { a, b });
            return toReturn.Clone() as Expression;
        }
        public static Expression operator ^(Expression a, Expression b)
        {
            Expression toReturn = new Expression(Utils.operators["^"], new List<Expression>() { a, b });
            return toReturn.Clone() as Expression;
        }
        public static Expression Log(Expression a, Expression b)
        {
            return new Expression(Utils.functions["log"], new List<Expression>() { a, b });
        }
        public static Expression Sin(Expression a)
        {
            return new Expression(Utils.functions["sin"], new List<Expression>() { a });
        }
        public static Expression Cos(Expression a)
        {
            return new Expression(Utils.functions["cos"], new List<Expression>() { a });
        }
        public static Expression Tan(Expression a)
        {
            return new Expression(Utils.functions["tan"], new List<Expression>() { a });
        }
        public static Expression Arcsin(Expression a)
        {
            return new Expression(Utils.functions["arcsin"], new List<Expression>() { a });
        }
        public static Expression Arccos(Expression a)
        {
            return new Expression(Utils.functions["arccos"], new List<Expression>() { a });
        }
        public static Expression Arctan(Expression a)
        {
            return new Expression(Utils.functions["arctan"], new List<Expression>() { a });
        }
        public static Expression operator +(Expression a, double b)
        => a + new Expression(b);
        public static Expression operator -(Expression a, double b)
        => a - new Expression(b);
        public static Expression operator *(Expression a, double b)
        => a * new Expression(b);
        public static Expression operator /(Expression a, double b)
        => a / new Expression(b);
        public static Expression operator ^(Expression a, double b)
        => a ^ new Expression(b);
        public static Expression Log(Expression a, double b)
            => Log(a, new Expression(b.ToString()));
        public static Expression Sin(double a) => Sin(new Expression(a));
        public static Expression Cos(double a) => Cos(new Expression(a));
        public static Expression Tan(double a) => Tan(new Expression(a));
        public static Expression Arcsin(double a) => Arcsin(new Expression(a));
        public static Expression Arccos(double a) => Arccos(new Expression(a));
        public static Expression Arctan(double a) => Arctan(new Expression(a));
        public static Expression operator +(double a, Expression b)
        => new Expression(a) + b;
        public static Expression operator -(double a, Expression b)
        => new Expression(a) - b;
        public static Expression operator *(double a, Expression b)
        => new Expression(a) * b;
        public static Expression operator /(double a, Expression b)
        => new Expression(a) / b;
        public static Expression operator ^(double a, Expression b)
        => new Expression(a) ^ b;
        public static Expression Log(double a, Expression b)
            => Log(new Expression(a), b);
        public static Expression operator -(Expression a)
        => -1 * a;

        public override bool Equals(object obj)
        {
            if (obj == null)
            {
                return false;
            }
            try
            {
                obj = (Expression)obj;
            }
            catch
            {
                return false;
            }
            Expression other = (Expression)obj;

            if (this.isLeaf && other.isLeaf)
            {
                if (!value.Equals(other.value))
                {
                    return false;
                }
                else { return true; }
            }
            else if (isLeaf || other.isLeaf)
            {
                return false;
            }
            else if (!op.Equals(other.op))
            {
                return false;
            }
            else if (children.Count != other.children.Count)
            {
                return false;
            }
            else if (op.Commutative)
            {                
                Expression otherCopy = other.Clone() as Expression;
                foreach (Expression child in children)
                {
                    if (otherCopy.children.Contains(child))
                    {
                        otherCopy.children.Remove(child);
                        continue;
                    }
                    else { return false; }
                }
                if (otherCopy.children.Count > 0)
                {
                    return false;
                }
            }
            else
            {
                if (children.Count != other.children.Count)
                {
                    return false;
                }
                for (int i = 0; i < children.Count; i++)
                {
                    if (!children[i].Equals(other.children[i]))
                    {
                        return false;
                    }
                }
            }
            return true;
        }
        public static bool operator ==(Expression a, Expression b)
        {
            return a.Equals(b);
        }
        public static bool operator ==(Expression a, double b)
        {
            return (a.isLeaf && a.isNumeric && a.ToDouble() == b);
        }
        public static bool operator ==(double a, Expression b)
        {
            return b == a;
        }
        public static bool operator !=(Expression a, double b)
        {
            return !(a == b);
        }
        public static bool operator !=(Expression a, Expression b)
        {
            return !a.Equals(b);
        }
        public static bool operator !=(double a, Expression b)
        {
            return b != a;
        }
    }
    public interface iHasNumericValue
    {
        public double value { get; }
    }
    public class Number : Operand, iHasNumericValue, IComparable<Number>, IComparable<double>
    {
        public double value {  get; set; }
        public bool IsPositive => value >= 0;
        public bool IsNegative => value < 0;
        public bool isNatural => value > 0;
        public List<int> primeFactors
        {
            get
            {
                List<int> factors = new List<int>();
                int n = (int)value;
                if (n == 0)
                {
                    factors.Add(0);
                    return factors;
                }
                else if (n == 1)
                {
                    factors.Add(1);
                    return factors;
                }
                if (n < 0)
                {
                    n = -n;
                    factors.Add(-1);
                }
                for (int i = 2; i <= n / i; i++)
                {
                    while (n % i == 0)
                    {
                        factors.Add(i);
                        n /= i;
                    }
                }
                if (n > 1)
                {
                    factors.Add(n);
                }
                return factors;
            }
        }
        public Number(double value)
        {
            if (value > 1000000000 || value < -1000000000)
            {
                throw new Exception("Number too large");
            }
            this.value = value;
        }
        public int CompareTo(Number other)
        {
            return value.CompareTo(other.value);
        }
        public int CompareTo(double other)
        {
            return value.CompareTo(other);
        }
        public override string ToString()
        {
            return value.ToString();
        }
        public string ToLatex() => value.ToString();
        public override bool Equals(object obj)
        {
            if (obj is Number)
            {
                return value == ((Number)obj).value;
            }
            return false;
        }
    }
    public class Variable : Operand
    {
        public string name;
        public Variable(string name)
        {
            this.name = name;
        }
        public override string ToString()
        {
            return name;
        }
        public string ToLatex() => name;
        public override bool Equals(object obj)
        {
            if (obj is Variable)
            {
                return name == ((Variable)obj).name;
            }
            return false;
        }
    }
    public abstract class Constant : iHasNumericValue, Operand
    {
        public virtual string name { get; }
        public virtual string symbol { get; }
        public virtual double value { get; }
        public string ToString()
        {
            return symbol;
        }
        public virtual string ToLatex() { return ""; }
        public new bool Equals(object obj)
        {
            return obj is Constant && name == ((Constant)obj).name;
        }
    }
    public class E: Constant
    {
        public override string name => "e";
        public override string symbol => "e";
        public override double value => Math.E;
        public override string ToLatex() => "e";
    }
    public class Pi : Constant
    {
        public override string name => "pi";
        public override string symbol => "π";
        public override double value { get => Math.PI; }
        public override string ToLatex()
        {
            return "{\\pi}";
        }
    }
    public interface Operand
    {
        public string ToString();
        public new string ToLatex();
        public bool Equals(object obj);
    }
}
