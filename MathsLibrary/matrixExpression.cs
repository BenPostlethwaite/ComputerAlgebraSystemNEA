using MathsLibrary;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FinalFormsApp
{
    //public class MatrixExpression : ILatexPrintable
    //{
    //    public bool isLeaf
    //    {
    //        get
    //        {
    //            return children.Count == 0;
    //        }
    //    }
    //    List<MatrixExpression> children;
    //    MatrixOperand value;
    //    MatrixOperator op;

    //    public MatrixExpression(List<MatrixExpression> children, MatrixOperator op)
    //    {
    //        this.children = children;
    //        this.op = op;
    //    }
    //    public MatrixExpression(MatrixOperand value)
    //    {
    //        children = new List<MatrixExpression>();
    //        this.value = value;
    //    }
    //    public void ExpandAndSimplify()
    //    {
    //        MatrixOperand newValue = Evaluate();
    //        children = new List<MatrixExpression>();
    //        value = newValue;
    //    }
    //    public MatrixOperand Evaluate()
    //    {
    //        if (isLeaf)
    //        {
    //            return value;
    //        }
    //        else
    //        {
    //            List<MatrixOperand> evaluatedChildren = new List<MatrixOperand>();
    //            for (int i = 0; i < children.Count; i++)
    //            {
    //                evaluatedChildren.Add(children[i].Evaluate());
    //            }
    //            MatrixOperand toReturn = op.Calculate(evaluatedChildren);
    //            toReturn.ExpandAndSimplify();
    //            return toReturn;
    //        }
    //    }
    //    public string ToLatex()
    //    {
    //        if (isLeaf)
    //        {
    //            return value.ToLatex();
    //        }
    //        else if (op.Symbol == "||")
    //        {
    //            MatrixExpression child = children[0];
    //            string childLatex = child.ToLatex();
    //            childLatex = childLatex.Replace("pmatrix", "vmatrix");
    //            return childLatex;
    //        }
    //        else if (op.Symbol == "^-1")
    //        {
    //            MatrixExpression child = children[0];
    //            string childLatex = child.ToLatex();
                
    //            return childLatex + op.LatexSymbol;
    //        }
    //        else
    //        {
    //            string latex = "";
    //            for (int i = 0; i < children.Count; i++)
    //            {
    //                latex += children[i].ToLatex();
    //                if (i != children.Count-1)
    //                {
    //                    latex += op.LatexSymbol;
    //                }
    //            }
    //            return latex;
    //        }
    //    }
    //}

    
    //public interface MatrixOperator
    //{
    //    public int precedence { get; }
    //    public bool associative { get; }
    //    public bool commutative { get; }
    //    public string Symbol { get; }
    //    public string LatexSymbol { get; }
    //    public MatrixOperand Calculate(List<MatrixOperand> operands);
    //}
    //public class DotProduct : MatrixOperator
    //{        
    //    public int precedence { get { return 1; } }
    //    public bool associative { get { return false; } }
    //    public bool commutative { get { return true; } }
    //    public string Symbol { get { return "."; } }
    //    public string LatexSymbol { get { return @"\cdot"; } }
    //    public MatrixOperand Calculate(List<MatrixOperand> operands)
    //    {
    //        if (operands.Count != 2)
    //        {
    //            throw new Exception("Invalid number of operands");
    //        }
    //        else if (!operands.All(o => o.GetType() == typeof(Matrix)))
    //        {
    //            throw new Exception("Invalid type of operands");
    //        }
    //        Matrix a = operands[0] as Matrix;
    //        Matrix b = operands[1] as Matrix;

    //        return Matrix.DotProduct(a,b);
    //    }
    //}
    //public class CrossProduct : MatrixOperator
    //{
    //    public int precedence { get { return 1; } }
    //    public bool associative { get { return false; } }
    //    public bool commutative { get { return false; } }
    //    public string Symbol { get { return "x"; } }
    //    public string LatexSymbol { get { return @"\times"; } }
    //    public MatrixOperand Calculate(List<MatrixOperand> operands)
    //    {
    //        if (operands.Count != 2)
    //        {
    //            throw new Exception("Invalid number of operands");
    //        }
    //        else if (!operands.All(o => o.GetType() == typeof(Matrix)))
    //        {
    //            throw new Exception("Invalid type of operands");
    //        }
    //        Matrix a = operands[0] as Matrix;
    //        Matrix b = operands[1] as Matrix;
    //        return Matrix.CrossProduct(a, b);

    //    }
    //}
    //public class Multiply : MatrixOperator
    //{
    //    public int precedence { get { return 2; } }
    //    public bool associative { get { return true; } }
    //    public bool commutative { get { return true; } }
    //    public string Symbol { get { return "*"; } }
    //    public string LatexSymbol { get { return @"\cdot"; } }
    //    public MatrixOperand Calculate(List<MatrixOperand> operands)
    //    {        
    //        if (operands.Count != 2)
    //        {
    //            throw new Exception("Invalid number of operands");
    //        }
    //        MatrixOperand a = operands[0];
    //        MatrixOperand b = operands[1];
    //        if (a.GetType() == typeof(Matrix))
    //        {
    //            if (b.GetType() == typeof(Matrix))
    //            {
    //                return (a as Matrix) * (b as Matrix);
    //            }
    //            else if (b.GetType() == typeof(Expression))
    //            {
    //                return (a as Matrix) * (b as Expression);
    //            }
    //            else
    //            {
    //                throw new Exception("Invalid type for multiplication");
    //            }
    //        }
    //        else if (a.GetType() == typeof(Expression))
    //        {
    //            if (b.GetType() == typeof(Matrix))
    //            {
    //                return (a as Expression) * (b as Matrix);
    //            }                
    //            else
    //            {
    //                throw new Exception("Invalid type for multiplication");
    //            }
    //        }
    //        else
    //        {
    //            throw new Exception("Invalid type for multiplication");
    //        }
    //    }
    //}        
    //public class Add : MatrixOperator
    //{
    //    public int precedence { get { return 1; } }
    //    public bool associative { get { return true; } }
    //    public bool commutative { get { return true; } }
    //    public string Symbol { get { return "+"; } }
    //    public string LatexSymbol { get { return "+"; } }
    //    public MatrixOperand Calculate(List<MatrixOperand> operands)
    //    {
    //        if (operands.Count != 2)
    //        {
    //            throw new Exception("Invalid number of operands");
    //        }
    //        MatrixOperand a = operands[0];
    //        MatrixOperand b = operands[1];
    //        if (a.GetType() == typeof(Matrix) && b.GetType() == typeof(Matrix))
    //        {
    //            return (a as Matrix) + (b as Matrix);
    //        }            
    //        else
    //        {
    //            throw new Exception("Invalid type for addition");
    //        }
    //    }
        
    //}
    //public class Subtract: MatrixOperator
    //{
    //    public int precedence { get { return 1; } }
    //    public bool associative { get { return false; } }
    //    public bool commutative { get { return false; } }
    //    public string Symbol { get { return "-"; } }
    //    public string LatexSymbol { get { return "-"; } }
    //    public MatrixOperand Calculate(List<MatrixOperand> operands)
    //    {
    //        if (operands.Count != 2)
    //        {
    //            throw new Exception("Invalid number of operands");
    //        }
    //        MatrixOperand a = operands[0];
    //        MatrixOperand b = operands[1];
    //        if (a.GetType() == typeof(Matrix) && b.GetType() == typeof(Matrix))
    //        {
    //            return (a as Matrix) - (b as Matrix);
    //        }
    //        else
    //        {
    //            throw new Exception("Invalid type for subtraction");
    //        }
    //    }    
    //}
    //public class Determinant : MatrixOperator
    //{
    //    public int precedence { get { return 2; } }
    //    public bool associative { get { return false; } }
    //    public bool commutative { get { return false; } }
    //    public string Symbol { get { return "||"; } }
    //    public string LatexSymbol { get { return @"\begin{vmatrix}"; } }

    //    public MatrixOperand Calculate(List<MatrixOperand> operands)
    //    {
    //        if (operands.Count != 1)
    //        {
    //            throw new Exception("Invalid number of operands");
    //        }
    //        MatrixOperand a = operands[0];
    //        if (a.GetType() == typeof(Matrix))
    //        {
    //            return (a as Matrix).Determinant;
    //        }
    //        else
    //        {
    //            throw new Exception("Invalid type for determinant");
    //        }
    //    }
    //}
    //public class Inverse
    //{
    //    public int precedence { get { return 2; } }
    //    public bool associative { get { return false; } }
    //    public bool commutative { get { return false; } }
    //    public string Symbol { get { return "^-1"; } }
    //    public string LatexSymbol { get { return @"^{-1}"; } }
    //    public MatrixOperand Calculate(List<MatrixOperand> operands)
    //    {
    //        if (operands.Count != 1)
    //        {
    //            throw new Exception("Invalid number of operands");
    //        }
    //        MatrixOperand a = operands[0];
    //        if (a.GetType() == typeof(Matrix))
    //        {
    //            return (a as Matrix).Inverse;
    //        }
    //        else
    //        {
    //            throw new Exception("Invalid type for inverse");
    //        }
    //    }
    //}
}
