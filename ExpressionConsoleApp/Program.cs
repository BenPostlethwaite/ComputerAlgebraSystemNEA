using MathsLibrary;

class Program
{
    static void Main()
    {
        Expression expression = new Expression("x^(1+1/y^2)", "infix");
        expression.ExpandAndSimplify();
        expression.MakePretty();
        Console.WriteLine(expression.ToString());
    }
}