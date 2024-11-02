using FinalFormsApp;
using System.Net;

namespace MathsLibrary
{
    public partial class Expression : ICloneable, MatrixOperand, ILatexPrintable
    {
        public IOperator op;
        public Operand value;
        public List<Expression> children = new List<Expression>();
        public Expression(IOperator op, List<Expression> children = null)
        {
            this.op = op;
            if (children == null)
            {
                this.children = new List<Expression>();
            }
            else
            {
                this.children = children;
            }
        }
        public Expression(double number)
        {            
            value = new Number(number);            
        }
        public Expression(string expression = "", string type = "basic")
        {
            if (type == "basic")
            {
                if (double.TryParse(expression, out double number))
                {
                    if (number % 1 == 0)
                    {
                        try
                        {
                            value = new Number(number);
                        }
                        catch (Exception exception)
                        {
                            throw exception;
                        }
                    }
                    else
                    {
                        int lenAfterPoint = expression.Length - expression.IndexOf('.') - 1;
                        int denominator = (int)Math.Pow(10, lenAfterPoint);
                        int numerator = (int)(number * denominator);
                        CloneFrom(new Expression(numerator) / new Expression(denominator));
                    }
                }
                else if (expression == "e")
                {
                    value = new E();
                }
                else if (expression == "π")
                {
                    value = new Pi();
                }                
                else if (expression == "i")
                {
                    CloneFrom((-1)^(1/new Expression(2)));
                }
                else if (expression.Length == 1 && char.IsLetter(expression[0]))
                {
                    value = new Variable(expression);
                }
                else if (expression.Length > 1 && expression.All(c => char.IsLetter(c) || char.IsDigit(c)))
                {
                    value = new Variable(expression);
                }
                else
                {
                    //maybe this breaks stuff?
                    value = new Variable(expression);
                    //throw new Exception("invalid variable");
                }
            }
            else if (type == "postfix")
            {
                List<string> split = BasicFormat(expression);
                split.Reverse();
                CloneFrom(ExpressionFromRPN(split));
            }
            else if (type == "infix")
            {
                List<string> split = FormatInfix(expression);
                CloneFrom(ExpressionFromInfix(split));
            }
            else
            {
                throw new Exception("Invalid type");
            }
            CombineAssociativeOperators();
        }
        private static Expression ExpressionFromRPN(List<string> rpn)
        {
            Stack<Expression> stack = new Stack<Expression>();
            foreach (string s in rpn)
            {
                if (Utils.IsOperator(s))
                {
                    List<Expression> operands = new List<Expression>();
                    for (int i = 0; i < Utils.operators[s].OperandNumber; i++)
                    {
                        operands.Add(stack.Pop());
                    }
                    operands.Reverse();
                    stack.Push(new Expression(Utils.operators[s], operands));
                }
                else if (Utils.IsFunction(s))
                {
                    List<Expression> operands = new List<Expression>();
                    for (int i = 0; i < Utils.functions[s].OperandNumber; i++)
                    {
                        operands.Add(stack.Pop());
                    }
                    operands.Reverse();
                    stack.Push(new Expression(Utils.functions[s], operands));
                }
                else
                {
                    stack.Push(new Expression(s));
                }
            }
            if (stack.Count != 1)
            {
                throw new Exception("Invalid expression");
            }
            return stack.Pop();
        }
        private static Expression ExpressionFromInfix(List<string> split)
        {
            List<string> operators = Utils.operators.Keys.ToList();
            List<string> functions = Utils.functions.Keys.ToList();

            Stack<Expression> expressionStack = new Stack<Expression>();
            Stack<string> operatorStack = new Stack<string>();

            
            for (int i = 0; i < split.Count; i++)
            {
                string token = split[i];
                if (functions.Contains(token))
                {
                    operatorStack.Push(token);
                }
                else if (operators.Contains(token))
                {
                    if (i == 0 || (operators.Contains(split[i - 1]) || "({,".Contains(split[i - 1])))
                    {
                        if (token == "-")
                        {
                            split.RemoveAt(i);
                            split.Insert(i, "*");
                            split.Insert(i, "-1");
                            split.Insert(i, "(");


                            List<string> newSplit = split.GetRange(i, split.Count - i);
                            newSplit = CloseBracket(newSplit);
                            split.RemoveRange(i, split.Count - i);
                            split.AddRange(newSplit);
                            i--;
                            continue;
                        }
                        else if (token == "+")
                        {
                            continue;
                        }
                        else
                        {
                            throw new Exception("Invalid expression");
                        }
                    }

                    string o1 = token;
                    while (operatorStack.Count > 0 && operatorStack.Peek() != "(" &&
                        (Utils.GetPrecedence(operatorStack.Peek()) >= Utils.GetPrecedence(token)))
                    {
                        Expression right = expressionStack.Pop();
                        Expression left = expressionStack.Pop();
                        IOperator op = Utils.operators[operatorStack.Pop()];
                        Expression newExpression = new Expression(op, new List<Expression>() { left, right });
                        expressionStack.Push(newExpression);
                    }
                    operatorStack.Push(o1);
                }
                else if (token == ",")
                {
                    while (operatorStack.Peek() != "(")
                    {
                        Expression right = expressionStack.Pop();
                        Expression left = expressionStack.Pop();
                        IOperator op = Utils.operators[operatorStack.Pop()];
                        Expression newExpression = new Expression(op, new List<Expression>() { left, right });
                        expressionStack.Push(newExpression);
                    }
                    string top = operatorStack.Pop();
                    if (!Utils.functions.ContainsKey(operatorStack.Peek()))
                    {
                        throw new Exception("invalid use of ','");
                    }
                    operatorStack.Push(top);
                }
                else if (token == "(")
                {
                    operatorStack.Push(token);
                }
                else if (token == ")")
                {
                    while (operatorStack.Peek() != "(")
                    {
                        Expression right = expressionStack.Pop();
                        Expression left = expressionStack.Pop();
                        if (Utils.IsFunction(operatorStack.Peek()))
                        {
                            Expression newExpression = new Expression(Utils.functions[operatorStack.Pop()], new List<Expression>() { left, right });
                            expressionStack.Push(newExpression);
                        }
                        else if (Utils.IsOperator(operatorStack.Peek()))
                        {
                            IOperator op = Utils.operators[operatorStack.Pop()];
                            Expression newExpression = new Expression(op, new List<Expression>() { left, right });
                            expressionStack.Push(newExpression);
                        }
                        else
                        {
                            throw new Exception("Error");
                        }
                    }
                    operatorStack.Pop();
                    if (operatorStack.Count > 0 && functions.Contains(operatorStack.Peek()))
                    {
                        IOperator op;
                        Expression newExpression;
                        if (Utils.IsFunction(operatorStack.Peek()))
                        {
                            op = Utils.functions[operatorStack.Pop()];
                        }
                        else if (Utils.IsOperator(operatorStack.Peek()))
                        {
                            op = Utils.operators[operatorStack.Pop()];

                        }
                        else
                        {
                            throw new Exception("Error");
                        }

                        List<Expression> children = new List<Expression>();
                        for (int j = 0; j < op.OperandNumber; j++)
                        {
                            children.Add(expressionStack.Pop());
                        }
                        children.Reverse();
                        newExpression = new Expression(op, children);
                        expressionStack.Push(newExpression);
                    }

                }
                else if (token == "i")
                {
                    expressionStack.Push(new Expression(-1)^(new Expression(1)/new Expression(2)));
                }
                else
                {                    
                    expressionStack.Push(new Expression(token));
                }
            }
            while (operatorStack.Count != 0)
            {
                Expression right = expressionStack.Pop();
                Expression left = expressionStack.Pop();
                IOperator op = Utils.operators[operatorStack.Pop()];
                Expression newExpression = new Expression(op, new List<Expression>() { left, right });
                expressionStack.Push(newExpression);
            }
            return expressionStack.Pop();
        }
        private static List<string> FormatInfix(string expression)
        {
            List<string> split = BasicFormat(expression);
            List<string> newSplit = new List<string>();

            for (int i = 0; i < split.Count; i++)
            {
                bool containsFunction = false;
                string func = "";
                foreach (string f in Utils.functions.Keys)
                {
                    if (split[i].Contains(f))
                    {
                        func = f;
                        containsFunction = true;
                        break;
                    }
                }
                if (containsFunction && i != split.Count - 1 && split[i + 1] == "(")
                {
                    int start = split[i].IndexOf(func);
                    List<string> before = FormatInfix(split[i].Substring(0, start));

                    newSplit.AddRange(before);

                    int bracketCount = 0;
                    int j = i + 1;
                    int lastComma = i + 1;
                    while ((bracketCount != 0 || j == i + 1) &&
                        j < split.Count)
                    {
                        if (split[j] == "(")
                        {
                            bracketCount++;
                        }
                        else if (split[j] == ")")
                        {
                            bracketCount--;
                        }
                        else if (split[j] == "," && bracketCount == 1)
                        {
                            FormatFunctionArg(ref split, ref lastComma, ref j);
                        }
                        j++;
                    }
                    j--;
                    FormatFunctionArg(ref split, ref lastComma, ref j);

                    List<string> funcInstance = split.GetRange(i + 1, j - i);
                    funcInstance.Insert(0, func);
                    newSplit.AddRange(funcInstance);

                    i = j;
                    continue;
                }
                if (split[i].Length == 1)
                {
                    newSplit.Add(split[i]);
                    continue;
                }
                string newS = "";
                foreach (char c in split[i])
                {
                    if (char.IsLetter(c))
                    {
                        if (newS != "")
                        {
                            newSplit.Add(newS);
                            newS = "";
                        }
                        newSplit.Add(c.ToString());
                        newS = "";
                    }
                    else
                    {
                        newS += c;
                    }
                }
                if (newS != "")
                {
                    newSplit.Add(newS);
                }
            }
            split = newSplit;

            newSplit = new List<string>();
            if (split.Count > 0)
            {
                newSplit.Add(split[0]);
            }
            else
            {
                return newSplit;
            }

            for (int i = 1; i < split.Count; i++)
            {
                bool opLast = Utils.IsOperator(split[i - 1]);
                bool opThis = Utils.IsOperator(split[i]);

                bool funcLast = Utils.IsFunction(split[i - 1]);

                bool badSymbolLast = "(,".Contains(split[i - 1]);
                bool badSymbolThis = "),".Contains(split[i]);

                bool badLast = funcLast || opLast || badSymbolLast;
                bool badThis = opThis || badSymbolThis;


                if (!badLast && !badThis)
                {
                    newSplit.Add("*");
                }
                newSplit.Add(split[i]);
            }
            return newSplit;
        }
        private static List<string> BasicFormat(string expression)
        {

            foreach (string op in Utils.operators.Keys)
            {
                expression = expression.Replace(op, " " + op + " ");
            }
            foreach (string func in Utils.functions.Keys)
            {
                expression = expression.Replace(func+"(", " " + func + " ( ");
            }
            expression = expression.Replace("(", " ( ");
            expression = expression.Replace(")", " ) ");
            expression = expression.Replace("{", " { ");
            expression = expression.Replace("}", " } ");
            expression = expression.Replace(",", " , ");

            List<string> split = expression.Split(' ').Where(s => s != "").ToList();
            return split;
        }
        private static void FormatFunctionArg(ref List<string> split, ref int lastComma, ref int j)
        {
            List<string> arg = split.GetRange(lastComma + 1, j - lastComma - 1);
            int len1 = arg.Count;
            arg = FormatInfix(string.Join(" ", arg));
            int len2 = arg.Count;
            int diff = len2 - len1;
            split.RemoveRange(lastComma + 1, j - lastComma - 1);
            split.InsertRange(lastComma + 1, arg);
            j += diff;
            lastComma = j;
        }     
        private static List<string> CloseBracket(List<string> split)
        {
            int predecence = Utils.operators[split[2]].Precedence;
            int bracketCount = 1;
            for (int i = 3; i < split.Count; i++)
            {
                //eg ((-1) * (2+3)+3 => ((-1) * (2 + 3)) + 3
                if (bracketCount == 0)
                {
                    split.Insert(i, ")");
                    return split;
                }
                else if (split[i] == "(")
                {
                    bracketCount++;
                }
                else if (split[i] == ")")
                {
                    bracketCount--;
                }

                //eg: ((-1) * 2 + 3 => ((-1) * 2) + 3              
                if (bracketCount == 0 || (Utils.operators.Keys.Contains(split[i]) && Utils.operators[split[i]].Precedence <= predecence && bracketCount == 1))
                {
                    split.Insert(i, ")");
                    return split;
                }
            }

            //eg ((-1) * (2 + 3) => ((-1) * 2) + 3)
            if (bracketCount == 1)
            {
                split.Add(")");
                return split;
            }
            else
            {
                throw new Exception("Invalid expression");
            }

        }              

        public object Clone()
        {
            Expression clone;
            if (isLeaf)
            {
                clone = new Expression(value.ToString());
            }
            else
            {
                List<Expression> newChildren = new List<Expression>();
                foreach (Expression child in children)
                {
                    newChildren.Add(child.Clone() as Expression);
                }
                clone = new Expression(op, newChildren);

            }
            return clone;
        }
    }
}
