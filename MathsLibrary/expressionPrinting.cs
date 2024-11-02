using FinalFormsApp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MathsLibrary
{
    public partial class Expression : ICloneable, MatrixOperand, ILatexPrintable
    {
        public string BasicToString()
        {
            if (isLeaf)
            {
                if (isDouble && ToDouble() < 0)
                {
                    return $"({value.ToString()})";
                }
                return value.ToString();
            }
            string output = "";
            if (Utils.functions.ContainsKey(op.ToString()))
            {
                string toReturn = "";
                if (children.Count == 1)
                {
                    toReturn = $"{op.ToString()}({children[0].BasicToString()})";
                }
                else if (op.Symbol == "log")
                {
                    toReturn = $"log({children[0].BasicToString()}, {children[1].BasicToString()})";
                }
                else
                {
                    throw new Exception("no known function");
                }
                return toReturn;
            }
            for (int i = 0; i < children.Count; i++)
            {
                if (!children[i].isLeaf && children[i].op.Precedence < op.Precedence)
                {
                    if (op.Symbol != children[i].op.Symbol || op.Associative)
                    {
                        output += "(";
                        output += children[i].BasicToString();
                        output += ")";
                    }
                }

                if (i == children.Count - 1 ||
                    (op.Symbol == "*" && children[i + 1].isVariable))
                {
                    continue;
                }
                else { output += op.ToString(); }
            }
            output = output.Replace("+-", "-");
            output = output.Replace(")*(", ")(");
            output = output.Replace("*(", "(");
            output = output.Replace(")*", ")");
            output = output.Replace("-1*", "-");


            for (int i = 0; i < output.Length; i++)
            {
                if (output[i] == '*' && i < output.Length - 1 && char.IsLetter(output[i + 1]))
                {
                    output = output.Remove(i, 1);
                }
            }

            return output;
        }
        public override string ToString()
        {
            if (isLeaf)
            {
                return value.ToString();
            }
            if (Equals(new Expression("i")))
            {
                return "i";
            }
            string output = "";
            if (Utils.functions.ContainsKey(op.ToString()))
            {
                string toReturn = "";
                if (children.Count == 1)
                {
                    toReturn = $"{op.ToString()}({children[0].ToString()})";
                }
                else if (op.Symbol == "log")
                {
                    if (children[0].ToString() == "10")
                    {
                        toReturn = $"lg({children[1].ToString()})";
                    }
                    else if (children[0].ToString() == "e")
                    {
                        toReturn = $"ln({children[1].ToString()})";
                    }
                    else
                    {
                        toReturn = $"log({children[0].ToString()}, {children[1].ToString()})";
                    }
                }
                else
                {
                    throw new Exception("no known function");
                }
                return toReturn;
            }
            bool hat = true;
            for (int i = 0; i < children.Count; i++)
            {
                if (!children[i].isLeaf && children[i].op.Precedence <= op.Precedence)
                {
                    if (op.Symbol != children[i].op.Symbol || op.Associative)
                    {
                        output += "(";
                    }
                }
                if (op.Symbol == "^")
                {
                    if (i == 0)
                    {
                        if (children[1].isInt && 0 < children[1].ToDouble() && children[1].ToDouble() < 10)
                        {
                            hat = false;
                        }
                        
                        string toAdd = children[i].ToString();
                        if (toAdd[0] == '-')
                        {
                            toAdd = "(" + toAdd + ")";
                        }
                        output += toAdd;
                    }
                    else if (!hat)
                    {
                        output += new string(children[i].ToString().Select(x => Utils.SuperscriptDigits[x - '0']).ToArray());
                    }
                    else
                    {
                        output += children[i].ToString();
                    }
                }
                else
                {
                    string toAdd = children[i].ToString();
                    if (toAdd[0] == '-')
                    {
                        toAdd = "(" + toAdd + ")";
                    }
                    output += toAdd;

                }
                if (!children[i].isLeaf && children[i].op.Precedence < op.Precedence)
                {
                    if (op.Symbol != children[i].op.Symbol || op.Associative)
                    {
                        output += ")";
                    }
                }

                if (i == children.Count - 1 ||
                    (op.Symbol == "*" && children[i + 1].isVariable) ||
                    hat == false)
                {
                    continue;
                }
                else { output += op.ToString(); }
            }                      


            for (int i = 0; i < output.Length; i++)
            {
                if (output[i] == '*' && i < output.Length - 1 && char.IsLetter(output[i + 1]))
                {
                    output = output.Remove(i, 1);
                }
            }

            output = output.Replace("+-", "-");
            output = output.Replace(")*(", ")(");
            output = output.Replace("*(", "(");
            output = output.Replace(")*", ")");
            output = output.Replace("-1*", "-");

            return output;
        }
        public void MakePretty()
        {
            Expression copy;
            do
            {
                copy = Clone() as Expression;
                if (isLeaf)
                {
                    return;
                }
                SinOverCosToTan();
                for (int i = 0; i < children.Count; i++)
                {
                    Expression child = children[i];
                    child.MakePretty();
                }
                if (op.Commutative)
                {
                    children = SortCommutativeOperators(children, op.Symbol);
                }
            } while (!Equals(copy));
            
        }
        private int CompareCommutativeOrder(Expression other, string parentOp)
        {
            int toReturn = 0;
            // assume operator is +
            // 1 is switch, -1 is don't switch, 0 is no preference
            if (isDouble && ((Number)value).value < 0 && parentOp == "*")
            {
                return -1;
            }
            else if (other.isDouble && ((Number)other.value).value < 0 && parentOp == "*")
            {
                return 1;
            }
            else if (isLeaf && other.isLeaf)
            {
                if ((isVariable || isConstant) && (other.isVariable || other.isConstant))
                {
                    toReturn = value.ToString().CompareTo(other.value.ToString());
                }
                else if (isVariable || isConstant)
                {       
                    // x*2 => 2x
                    // x + 2 => x + 2
                    if (parentOp == "+")
                    {
                        toReturn = -1;
                    }
                    else if (parentOp == "*")
                    {
                        toReturn = 1;
                    }
                    else
                    {
                        toReturn = -1;
                    }
                }
                else if (other.isVariable || other.isConstant)
                {
                    if (parentOp == "+")
                    {
                        toReturn = 1;
                    }
                    else if (parentOp == "*")
                    {
                        toReturn = -1;
                    }
                    else
                    {
                        toReturn = -1;
                    }
                }                
            }
            else if (!isLeaf && op.Symbol == "*" && children.Count(c =>c.isNumeric && c.ToDouble() < 0) == 1)
            {
                return 1;
            }
            else if (!other.isLeaf && other.op.Symbol == "*" && other.children.Count(c =>c.isDouble && c.ToDouble() < 0) == 1)
            {
                return -1;
            }
            else if (isLeaf)
            {
                if (parentOp == "+")
                {
                    toReturn = 1;
                }
                else if (parentOp == "*")
                {
                    toReturn = -1;
                }
                else
                {
                    toReturn = -1;
                }
            }
            else if (other.isLeaf)
            {
                if (parentOp == "+")
                {
                    toReturn = -1;
                }
                else if (parentOp == "*")
                {
                    toReturn = 1;
                }
                else
                {
                    toReturn = 1;
                }
            }            
            else if (isNumeric && !other.isNumeric)
            {
                return -1;
            }
            else if (!isNumeric && other.isNumeric)
            {
                return 1;
            }            
            else if (ToString().Length > other.ToString().Length)
            {
                if (parentOp == "+")
                {
                    toReturn = -1;
                }
                else if (parentOp == "*")
                {
                    toReturn = 1;
                }
                else
                {
                    toReturn = -1;
                }
            }
            else
            {
                return -1;
            }

            return toReturn;
        }
        private static List<Expression> SortCommutativeOperators(List<Expression> list, string parentOp)
        {
            //bubble sort
            for (int j = list.Count - 1; j > 0; j--)
            {
                for (int i = 1; i <= j; i++)
                {
                    if (list[i - 1].CompareCommutativeOrder(list[i], parentOp) > 0)
                    {
                        Expression temp = list[i - 1];
                        list[i - 1] = list[i];
                        list[i] = temp;
                    }
                }
            }

            return list;
        }
        public string ToLatexWithoutChangingOrder()
        {
            if (isLeaf)
            {
                return value.ToLatex();
            }
            string latex = "";
            if (op.Symbol == "/")
            {
                latex += "\\frac";
            }
            else if (op.Symbol == "^" && children[1].ToString() == "1/2")
            {
                if (children[0]==-1)
                {
                    latex += "i";
                }
                else
                {

                    latex = "\\sqrt{" + children[0].ToLatex() + "}";                   
                }
                latex.Replace(")*(", ")(");

                return latex;

            }
            else if (Utils.functions.ContainsKey(op.Symbol))
            {
                if (op.Symbol == "log")
                {
                    if (children[0] == 10)
                    {
                        latex += op.latex + "{(" + children[1].ToLatexWithoutChangingOrder() + ")}";
                    }
                    else if (children[0].ToString() == "e")
                    {
                        latex += "\\ln{(" + children[1].ToLatexWithoutChangingOrder() + ")}";
                    }
                    else
                    {
                        latex += "\\log_{" + children[0].ToLatexWithoutChangingOrder() + "}{(" + children[1].ToLatexWithoutChangingOrder() + ")}";
                    }
                }
                else if (op.OperandNumber == 1)
                {
                    latex += op.latex + "{(" + children[0].ToLatexWithoutChangingOrder() + ")}";
                }
                else
                {
                    throw new NotImplementedException();
                }
                return latex;
            }

            for (int i = 0; i < children.Count; i++)
            {
                string latexChild = children[i].ToLatexWithoutChangingOrder();
                if (latexChild == "-1" && op.Symbol == "*" && i != children.Count - 1)
                {
                    latexChild = "-";
                    latex += latexChild;
                    continue;
                }
                if (!children[i].isLeaf)
                {
                    if (children[i].op.Precedence == op.Precedence)
                    {
                        if (children[i].op.Symbol == op.Symbol) { }
                        else if (i == 0) { }
                        else
                        {
                            latexChild = "(" + latexChild + ")";
                        }
                    }
                    else if (children[i].op.Precedence < op.Precedence)
                    {                                               
                        if (op.Symbol == "^")
                        {
                            if (i == 0)
                            {
                                latexChild = "(" + latexChild + ")";
                            }
                        }
                        else if (op.Symbol != "/")
                        {
                            latexChild = "(" + latexChild + ")";
                        }

                    }                    
                }                
                

                if (op.Symbol == "*" &&
                    i != 0 &&
                    latexChild[0] != '-' &&
                    latex[latex.Length - 1] != '-' &&
                    latex.Substring(latex.Length - op.latex.Length) == op.latex &&
                    (!char.IsNumber(latexChild.Replace("{", "")[0]) || latexChild[0] == '('))
                {
                    //delete the last operator
                    latex = latex.Substring(0, latex.Length - op.latex.Length);
                }         
                if (op.Symbol == "^" && i != children.Count-1 && latexChild[0] == '-')
                {
                    latexChild = "(" + latexChild + ")";
                }
                if (Utils.functions.ContainsKey(op.Symbol) || op.Symbol == "^" || op.Symbol == "/")
                {
                    latex += "{" + latexChild + "}";
                }
                else
                {
                    latex += latexChild;
                }


                bool dontAddOperator = false;
                if (i == children.Count - 1 ||
                    op.Symbol == "/")
                {
                    dontAddOperator = true;
                }
                if (!dontAddOperator)
                {
                    latex += op.latex;
                }
            }

            latex = latex.Replace(")*(", ")(");
            latex = latex.Replace("*(", "(");
            latex = latex.Replace(")*", ")");
            latex = latex.Replace("-1*", "-");
            latex = latex.Replace("+-", "-");

            return latex;
        }
        public string ToLatex()
        {
            MakePretty();
            return ToLatexWithoutChangingOrder();

        }
    }
}
