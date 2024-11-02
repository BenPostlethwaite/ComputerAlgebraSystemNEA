using FinalFormsApp;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Numerics;

namespace MathsLibrary
{
    public class Matrix : ICloneable, MatrixOperand, ILatexPrintable
    {
        private Expression[,] matrix;
        private bool determinatUpdated = false;
        private bool inverseUpdated = false;
        public bool hasNumericValue
        {
            get
            {
                foreach (Expression e in matrix)
                {
                    if (!e.isNumeric)
                    {
                        return false;
                    }
                }
                return true;
            }
        }
        public Expression this[int ColNo, int rowNo]
        {
            get => matrix[ColNo,rowNo];
            set
            {
                matrix[ColNo, rowNo] = value;                                    
                determinatUpdated = false;
                inverseUpdated = false;
            }
        }
        public bool isSquare { get => matrix.GetLength(0) == matrix.GetLength(1); }
        public int colCount { get => matrix.GetLength(0); }
        public int rowCount { get => matrix.GetLength(1); }
        public bool isVector { get => colCount == 1 || rowCount == 1; }
        public string ToLatex()
        {
            string[,] latexParts = new string[colCount, rowCount];
            for (int i = 0; i < colCount; i++)
            {
                for (int j = 0; j < rowCount; j++)
                {
                    latexParts[i, j] = matrix[i, j].ToLatexWithoutChangingOrder();
                }
            }


            string toReturn = @"\begin{pmatrix}";

            for (int j = 0; j < rowCount; j++)
            {
                for (int i = 0; i < colCount; i++)
                {
                    toReturn += latexParts[i, j];
                    if (i != colCount - 1)
                    {
                        toReturn += " & & & & ";
                    }
                }
                if (j != rowCount - 1)
                {
                    toReturn += @" \\\\\\\\ ";
                }
            }
            toReturn += @"\end{pmatrix}";
            return toReturn;
        }
        public Matrix(int collumns, int rows)
        {
            matrix = new Expression[collumns, rows];
            for (int i = 0; i < collumns; i++)
            {
                for (int j = 0; j < rows; j++)
                {
                    matrix[i, j] = new Expression(0);
                }
            }
        }
        public Matrix(Expression[,] matrix)
        {
            this.matrix = matrix;
        }
        public static Matrix operator +(Matrix a, Matrix b)
        {
            Matrix toReturn = new Matrix(a.colCount, a.rowCount);
            if (a.rowCount != b.rowCount || a.colCount != b.colCount)
            {
                throw new Exception("Matrices must be the same size to add them");
            }
            for (int x = 0; x < a.colCount; x++)
            {
                for (int y = 0; y < a.rowCount; y++)
                {
                    toReturn[x, y] = a[x, y] + b[x, y];
                }
            }
            toReturn.ExpandAndSimplify();
            return toReturn;
        }
        public static Matrix operator -(Matrix a, Matrix b)
        {
            if (a.rowCount != b.rowCount || a.colCount != b.colCount)
            {
                throw new Exception("Matrices must be the same size to subtract them");
            }
            Matrix toReturn = new Matrix(a.colCount, a.rowCount);
            for (int x = 0; x < a.colCount; x++)
            {
                for (int y = 0; y < a.rowCount; y++)
                {
                    toReturn[x, y] = a[x, y] - b[x, y];
                }
            }
            return toReturn;
        }
        public static Matrix operator *(Matrix a, Matrix b)
        {
            Matrix toReturn = new Matrix(b.colCount, a.rowCount);
            if (a.colCount != b.rowCount)
            {
                throw new Exception("Matrices are not the right size to multiply.");
            }
            for (int x = 0; x < toReturn.colCount; x++)
            {
                for (int y = 0; y < toReturn.rowCount; y++)
                {
                    Expression sum = new Expression(0);
                    for (int i = 0; i < a.colCount; i++)
                    {
                        sum += a[i, y] * b[x, i];
                    }
                    toReturn[x, y] = sum;
                }
            }
            return toReturn;
        }
        public static Matrix operator *(Matrix a, Expression b)
        {
            Matrix toReturn = new Matrix(a.colCount, a.rowCount);
            for (int x = 0; x < toReturn.colCount; x++)
            {
                for (int y = 0; y < toReturn.rowCount; y++)
                {
                    toReturn[x, y] = a[x, y] * b;
                }
            }
            return toReturn;
        }
        public static Matrix operator *(Expression a, Matrix b)
        {
            return b * a;
        }
        public static Expression DotProduct(Matrix a, Matrix b)
        {
            if (a.colCount != b.colCount || a.rowCount != b.rowCount)
            {
                throw new Exception("Matrices must be the same size to dot product them");
            }
            if (!a.isVector || !b.isVector)
            {
                throw new Exception("Can only dot product vectors");
            }
            Expression toReturn = new Expression(0);
            for (int col = 0; col < a.colCount; col++)
            {
                for (int row = 0; row < a.rowCount; row++)
                {
                    toReturn += a[col, row] * b[col, row];
                }
            }
            return toReturn;
        }
        public static Matrix CrossProduct(Matrix a, Matrix b)
        {
            if (!a.isVector || !b.isVector)
            {
                throw new Exception("Can only cross product vectors");
            }
            if (a.colCount != b.colCount || a.rowCount != b.rowCount)
            {
                throw new Exception("Vectors must be the same size to cross product them");
            }
            if (!(a.colCount == 3 && a.rowCount == 1) && !(a.colCount == 1 && a.rowCount == 3))
            {
                throw new Exception("Vectors must be 3 dimensional to cross product them");
            }
            Matrix toReturn;
            if (a.colCount == 1)
            {
                toReturn = new Matrix(1, 3);

                toReturn[0, 0] = (a[0, 1] * b[0, 2]) - (a[0, 2] * b[0, 1]);
                toReturn[0, 1] = (a[0, 2] * b[0, 0]) - (a[0, 0] * b[0, 2]);
                toReturn[0, 2] = (a[0, 0] * b[0, 1]) - (a[0, 1] * b[0, 0]);
                
            }
            else
            {
                toReturn = new Matrix(3, 1);
                toReturn[0, 0] = (a[1, 0] * b[2, 0]) - (a[2, 0] * b[1, 0]);
                toReturn[1, 0] = (a[2, 0] * b[0, 0]) - (a[0, 0] * b[2, 0]);
                toReturn[2, 0] = (a[0, 0] * b[1, 0]) - (a[1, 0] * b[0, 0]);
            }
            return toReturn;
        }
        public void ExpandAndSimplify()
        {
            foreach (Expression e in matrix)
            {
                e.ExpandAndSimplify();
                e.MakePretty();
            }
        }
        public Expression Cofactor(int row, int column)
        {
            Expression toReturn = Math.Pow(-1, row + column) * Minor(row, column);
            toReturn.Simplify();
            return toReturn;
        }
        private Expression Minor(int row, int column)
        {
            Matrix result = new Matrix(colCount - 1, rowCount - 1);
            int r = 0;
            for (int i = 0; i < colCount; i++)
            {
                if (i == row)
                {
                    continue;
                }
                int c = 0;
                for (int j = 0; j < rowCount; j++)
                {
                    if (j == column)
                    {
                        continue;
                    }
                    result[r, c] = matrix[i, j];
                    c++;
                }
                r++;
            }
            Expression toReturn = result.Determinant;
            return toReturn;
        }
        private static T[,] Transpose<T>(T[,] input)
        {
            T[,] result = new T[input.GetLength(1), input.GetLength(0)];
            for (int i = 0; i < input.GetLength(0); i++)
            {
                for (int j = 0; j < input.GetLength(1); j++)
                {
                    result[j, i] = input[i, j];
                }
            }
            return result;  
        }
        private void UpdateDeterminant()
        {
            if (!isSquare)
            {
                Determinant = null;
            }
            else if (matrix.GetLength(0) == 1)
            {
                Determinant = matrix[0, 0].Clone() as Expression;
            }
            else if (matrix.GetLength(0) == 2)
            {
                Expression toReturn = (matrix[0, 0] * matrix[1, 1] - matrix[0, 1] * matrix[1, 0]).Clone() as Expression;
                toReturn.Factorise();
                Determinant = toReturn;
            }
            else
            {
                Expression result = new Expression(0);
                for (int i = 0; i < matrix.GetLength(0); i++)
                {
                    result += (matrix[0, i] * Cofactor(0, i)).Clone() as Expression;
                }
                result.Factorise();
                Determinant = result;
            }
        }
        private Expression _determinant;
        public Expression Determinant
        {
            private set => _determinant = value;
            get
            {
                if (determinatUpdated == false)
                {
                    UpdateDeterminant();
                }
                return _determinant;
            }
        }
        private void UpdateInverse()
        {
            if (!isSquare)
            {
                Inverse = null;
            }
            else if (Determinant == new Expression(0))
            {
                Inverse = null;
            }
            else if (matrix.GetLength(0) == 1)
            {
                Matrix toReturn = new Matrix(1, 1);
                toReturn[0, 0] = matrix[0, 0] / Determinant;
                toReturn.ExpandAndSimplify();
                Inverse = toReturn;
            }
            else
            {
                Expression[,] result = new Expression[matrix.GetLength(0), matrix.GetLength(1)];
                for (int i = 0; i < matrix.GetLength(0); i++)
                {
                    for (int j = 0; j < matrix.GetLength(1); j++)
                    {
                        result[i, j] = Cofactor(i, j) / Determinant;
                    }
                }
                Matrix toReturn = new Matrix(Transpose(result));
                toReturn.ExpandAndSimplify();
                Inverse = toReturn;
            }            
        }
        private Matrix _inverse;
        public Matrix Inverse
        {
            private set => _inverse = value;
            get
            {
                if (determinatUpdated == false)
                {
                    UpdateDeterminant();
                    determinatUpdated = true;
                }
                if (inverseUpdated == false)
                {
                    UpdateInverse();
                    inverseUpdated = true;
                }
                return _inverse;
            }
        }
        public static List<Equation> CreateSimultaneousEquations(Matrix a, Matrix b)
        {
            if (a.rowCount != b.rowCount)
            {
                throw new Exception("Matrices must have the same number of rows");
            }
            if (a.colCount != b.colCount)
            {
                throw new Exception("Matrices must have the same number of columns");
            }

            List<Equation> toReturn = new List<Equation>();
            for (int i = 0; i < a.colCount; i++)
            {
                for (int j = 0; j < a.rowCount; j++)
                {
                    toReturn.Add(new Equation(a[i, j], b[i, j]));
                }
            }
            return toReturn;
        }
        public object Clone()
        {
            Matrix toReturn = new Matrix(matrix.GetLength(0), matrix.GetLength(1));
            for (int i = 0; i < matrix.GetLength(0); i++)
            {
                for (int j = 0; j < matrix.GetLength(1); j++)
                {
                    toReturn[i, j] = matrix[i, j].Clone() as Expression;
                }
            }
            return toReturn;
        }
        public override bool Equals(object? obj)
        {
            Matrix other = obj as Matrix;
            if (other == null)
            {
                return false;
            }
            if (this.colCount != other.colCount)
            {
                return false;
            }
            if (this.rowCount != other.rowCount)
            {
                return false;
            }
            for (int col = 0; col < colCount; col++)
            {
                for (int row = 0; row < rowCount; row++)
                {
                    if (this[col, row] != other[col, row])
                    {
                        return false;
                    }
                }
            }
            return true;
        }
        public List<Expression> GetEigenValues()
        {
            Matrix m = Clone() as Matrix;
            if (!m.isSquare)
            {
                throw new Exception("Matrix must be square");
            }
            List<Expression> toReturn = new List<Expression>();
            for (int i = 0; i < m.colCount; i++)
            {
                m[i, i] -= new Expression("igenValue");
            }

            Expression det = m.Determinant;
            Equation equation = new Equation(det, new Expression(0));
            List<Equation> results = equation.Solve("igenValue");
            
            foreach (Equation e in results)
            {
                if (e.left.ToString() == "igenValue" && !toReturn.Contains(e.right))
                {
                    toReturn.Add(e.right);
                }
            }
            return toReturn;

        }
        public List<(Matrix eigenVector, Expression eigenValue)> GetEigenVectors(List<Expression> eigenValues)
        {            
            List<(Matrix, Expression)> toReturn = new List<(Matrix, Expression)>();
            foreach (Expression eigenValue in eigenValues)
            {
                List<Matrix> eigenVectors;
                try
                {
                    eigenVectors = GetEigenVector(eigenValue.Clone() as Expression);
                }
                catch (Exception exception)
                {
                    throw exception;
                }


                for (int i = 0; i < eigenVectors.Count; i++)
                {
                    if (eigenVectors[i] != null)
                    {
                        toReturn.Add((eigenVectors[i], eigenValue));
                    }
                }
            }
            return toReturn;
        }
        public List<Matrix> GetEigenVector(Expression eigenValue)
        {
            Matrix m = Clone() as Matrix;       
            List<Matrix> toReturn = new List<Matrix>();
            Matrix zero = new Matrix(1, m.rowCount);
            Matrix varVector = new Matrix(1, colCount);

            List<string> variables = new List<string>();
            for (int i = 0; i < m.colCount; i++)
            {
                string variable = ("v" + i);
                variables.Add(variable);
                varVector[0, i] = new Expression(variable);
                m[i, i] -= eigenValue;

                zero[0, i] = new Expression(0);
            }

            m.ExpandAndSimplify();
            Matrix left = m * varVector;            
            left.ExpandAndSimplify();
            
            List<Equation> equations = CreateSimultaneousEquations(left, zero);

            List<SolutionSet> solvedEquationsSets = Equation.SolveSimultaneous(equations, variables);


            for (int i = 0; i < solvedEquationsSets.Count; i++)
            {
                Matrix eigenVector = new Matrix(1, colCount);
                SolutionSet solvedEquations = solvedEquationsSets[i];
                if (solvedEquations.equations.All(solvedEquations => solvedEquations.right.ToString() == "ℝ"))
                {
                    //All lines are invarient
                    throw new Exception("Infinite invarient lines");
                }
                else if (solvedEquations.equations.Where(solvedEquation => solvedEquation.right.ToString() == "ℝ").Count() == 1 &&
                    solvedEquations.equations.Where(solvedEquation => solvedEquation.right.ToString() != "ℝ").All(eq => eq.right == 0 || !eq.right.isNumeric))
                {
                    // vector is either in the format (0,ℝ,0,...) or (a,ℝ,b,...) where it is assumed a and b are equal to 0
                    foreach (Equation eq in solvedEquations.equations)
                    {
                        if (eq.right.ToString() == "ℝ")
                        {
                            eq.right = new Expression(1);
                        }
                    }
                    solvedEquations.equations = Equation.SolveSimultaneous(solvedEquations.equations, variables)[0].equations;
                }                  
                foreach (Equation eq in solvedEquations.equations)
                {
                    for (int j = 0; j < varVector.rowCount; j++)
                    {
                        if (eq.left.Equals(varVector[0, j]))
                        {
                            eigenVector[0, j] = eq.right;
                        }
                    }
                }
                toReturn.Add(eigenVector);
            }
            
            return toReturn;
        }        
    }
    public interface MatrixOperand: ILatexPrintable
    {
        public void ExpandAndSimplify();
        public string ToLatex();
    }
}