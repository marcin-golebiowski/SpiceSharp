﻿using System;
using System.Collections.ObjectModel;

namespace SpiceSharp.Algebra.Solve
{
    /// <summary>
    /// A search strategy based on methods outlined by Markowitz.
    /// </summary>
    /// <typeparam name="T">The base value type.</typeparam>
    /// <seealso cref="SpiceSharp.Algebra.Solve.PivotStrategy{T}" />
    public class Markowitz<T> : PivotStrategy<T> where T : IFormattable, IEquatable<T>
    {
        /// <summary>
        /// Markowitz numbers
        /// </summary>
        private int[] _markowitzRow;
        private int[] _markowitzColumn;
        private int[] _markowitzProduct;

        /// <summary>
        /// The maximum Markowitz count that will not result in Int32 overflow when squared
        /// Markowitz counts are capped at this quantity.
        /// </summary>
        private const int MaxMarkowitzCount = 46340;

        /// <summary>
        /// Gets the Markowitz row counts.
        /// </summary>
        public int RowCount(int row) => _markowitzRow[row];

        /// <summary>
        /// Gets the Markowitz column counts.
        /// </summary>
        public int ColumnCount(int column) => _markowitzColumn[column];

        /// <summary>
        /// Gets the Markowitz products.
        /// </summary>
        public int Product(int index) => _markowitzProduct[index];

        /// <summary>
        /// Gets the number of singletons.
        /// </summary>
        public int Singletons { get; private set; }

        /// <summary>
        /// Gets the magnitude method.
        /// </summary>
        public Func<T, double> Magnitude { get; private set; }

        /// <summary>
        /// Gets the strategies used for finding a pivot.
        /// </summary>
        public Collection<MarkowitzSearchStrategy<T>> Strategies { get; } = new Collection<MarkowitzSearchStrategy<T>>();

        /// <summary>
        /// Initializes a new instance of the <see cref="Markowitz{T}"/> class.
        /// </summary>
        public Markowitz()
        {
            // Register default strategies
            Strategies.Add(new MarkowitzSingleton<T>());
            Strategies.Add(new MarkowitzQuickDiagonal<T>());
            Strategies.Add(new MarkowitzDiagonal<T>());
            Strategies.Add(new MarkowitzEntireMatrix<T>());
        }

        /// <summary>
        /// This method will check whether or not a pivot element is valid or not.
        /// It checks for the submatrix right/below of the pivot.
        /// </summary>
        /// <param name="pivot">The pivot candidate.</param>
        /// <returns>
        /// True if the pivot can be used.
        /// </returns>
        /// <exception cref="ArgumentNullException">pivot</exception>
        public override bool IsValidPivot(MatrixElement<T> pivot)
        {
            if (pivot == null)
                throw new ArgumentNullException(nameof(pivot));

            // Get the magnitude of the current pivot
            var magnitude = Magnitude(pivot.Value);

            // Search for the largest element below the pivot
            var element = pivot.Below;
            var largest = 0.0;
            while (element != null)
            {
                largest = Math.Max(largest, Magnitude(element.Value));
                element = element.Below;
            }

            // Check the validity
            if (largest * RelativePivotThreshold < magnitude)
                return true;
            return false;
        }

        /// <summary>
        /// Initializes the pivot searching algorithm.
        /// </summary>
        /// <param name="matrix">The matrix.</param>
        /// <exception cref="ArgumentNullException">matrix</exception>
        public void Initialize(Matrix<T> matrix)
        {
            if (matrix == null)
                throw new ArgumentNullException(nameof(matrix));

            // Allocate arrays
            _markowitzRow = new int[matrix.Size + 1];
            _markowitzColumn = new int[matrix.Size + 1];
            _markowitzProduct = new int[matrix.Size + 2];
        }

        /// <summary>
        /// Count the Markowitz numbers.
        /// </summary>
        /// <param name="matrix">The matrix.</param>
        /// <param name="rhs">The right-hand side vector.</param>
        /// <param name="step">The elimination step.</param>
        private void Count(SparseMatrix<T> matrix, SparseVector<T> rhs, int step)
        {
            MatrixElement<T> element;

            // Get the first element in the vector
            var rhsElement = rhs.First;

            // Generate Markowitz row count
            for (var i = matrix.Size; i >= step; i--)
            {
                // Set count to -1 initially to remove count due to pivot element
                var count = -1;
                element = matrix.GetFirstInRow(i);
                while (element != null && element.Column < step)
                    element = element.Right;
                while (element != null)
                {
                    count++;
                    element = element.Right;
                }

                // Include elements on the Rhs vector
                while (rhsElement != null && rhsElement.Index < step)
                    rhsElement = rhsElement.Below;
                if (rhsElement != null && rhsElement.Index == i)
                    count++;
                
                _markowitzRow[i] = Math.Min(count, MaxMarkowitzCount);
            }
            
            // Generate Markowitz column count
            for (var i = step; i <= matrix.Size; i++)
            {
                // Set count to -1 initially to remove count due to pivot element
                var count = -1;
                element = matrix.GetFirstInColumn(i);
                while (element != null && element.Row < step)
                    element = element.Below;
                while (element != null)
                {
                    count++;
                    element = element.Below;
                }
                _markowitzColumn[i] = Math.Min(count, MaxMarkowitzCount);
            }
        }

        /// <summary>
        /// Calculate the Markowitz products.
        /// </summary>
        /// <param name="matrix">The matrix.</param>
        /// <param name="step">The elimination step.</param>
        private void Products(SparseMatrix<T> matrix, int step)
        {
            Singletons = 0;
            var size = matrix.Size;
            for (var i = step; i <= size; i++)
            {
                // UpdateMarkowitzProduct(i);
                _markowitzProduct[i] = _markowitzRow[i] * _markowitzColumn[i];
                if (_markowitzProduct[i] == 0)
                    Singletons++;
            }
        }

        /// <summary>
        /// Setup the pivot strategy.
        /// </summary>
        /// <param name="matrix">The matrix.</param>
        /// <param name="rhs">The right-hand side vector.</param>
        /// <param name="eliminationStep">The current elimination step.</param>
        /// <param name="magnitude">The method used to determine the magnitude of an element.</param>
        /// <exception cref="ArgumentNullException">matrix</exception>
        public override void Setup(SparseMatrix<T> matrix, SparseVector<T> rhs, int eliminationStep, Func<T, double> magnitude)
        {
            if (matrix == null)
                throw new ArgumentNullException(nameof(matrix));

            Magnitude = magnitude;

            // Initialize Markowitz row, column and product vectors if necessary
            if (_markowitzRow == null || _markowitzRow.Length != matrix.Size + 1)
                Initialize(matrix);

            Count(matrix, rhs, eliminationStep);
            Products(matrix, eliminationStep);
        }

        /// <summary>
        /// Move the pivot to the diagonal for this elimination step.
        /// </summary>
        /// <param name="matrix">The matrix.</param>
        /// <param name="rhs">The right-hand side vector.</param>
        /// <param name="pivot">The pivot element.</param>
        /// <param name="eliminationStep">The elimination step.</param>
        /// <exception cref="ArgumentNullException">pivot</exception>
        /// <remarks>
        /// This is done by swapping the rows and columns of the diagonal and that of the pivot.
        /// </remarks>
        public override void MovePivot(SparseMatrix<T> matrix, SparseVector<T> rhs, MatrixElement<T> pivot, int eliminationStep)
        {
            // If we haven't setup, just skip
            if (_markowitzProduct == null)
                return;
            if (pivot == null)
                throw new ArgumentNullException(nameof(pivot));
            int oldProduct;

            var row = pivot.Row;
            var column = pivot.Column;

            // If the pivot is a singleton, then we just consumed it
            if (_markowitzProduct[pivot.Row] == 0 || _markowitzProduct[pivot.Column] == 0)
                Singletons--;

            // Exchange rows
            if (pivot.Row != eliminationStep)
            {
                // Swap row Markowitz numbers
                var tmp = _markowitzRow[row];
                _markowitzRow[row] = _markowitzRow[eliminationStep];
                _markowitzRow[eliminationStep] = tmp;

                // Update the Markowitz product
                oldProduct = _markowitzProduct[row];
                _markowitzProduct[row] = _markowitzRow[row] * _markowitzColumn[row];
                if (oldProduct == 0)
                {
                    if (_markowitzProduct[row] != 0)
                        Singletons--;
                }
                else
                {
                    if (_markowitzProduct[row] == 0)
                        Singletons++;
                }
            }

            // Exchange columns
            if (column != eliminationStep)
            {
                // Swap column Markowitz numbers
                var tmp = _markowitzColumn[column];
                _markowitzColumn[column] = _markowitzColumn[eliminationStep];
                _markowitzColumn[eliminationStep] = tmp;

                // Update the Markowitz product
                oldProduct = _markowitzProduct[column];
                _markowitzProduct[column] = _markowitzRow[column] * _markowitzColumn[column];
                if (oldProduct == 0)
                {
                    if (_markowitzProduct[column] != 0)
                        Singletons--;
                }
                else
                {
                    if (_markowitzProduct[column] == 0)
                        Singletons++;
                }
            }

            // Also update the moved pivot
            oldProduct = _markowitzProduct[eliminationStep];
            _markowitzProduct[eliminationStep] = _markowitzRow[eliminationStep] * _markowitzColumn[eliminationStep];
            if (oldProduct == 0)
            {
                if (_markowitzProduct[eliminationStep] != 0)
                    Singletons--;
            }
            else
            {
                if (_markowitzProduct[eliminationStep] == 0)
                    Singletons++;
            }
        }

        /// <summary>
        /// Update the strategy after the pivot was moved.
        /// </summary>
        /// <param name="matrix">The matrix.</param>
        /// <param name="pivot">The pivot element.</param>
        /// <param name="eliminationStep">The elimination step.</param>
        /// <exception cref="ArgumentNullException">pivot</exception>
        public override void Update(SparseMatrix<T> matrix, MatrixElement<T> pivot, int eliminationStep)
        {
            // If we haven't setup, just skip
            if (_markowitzProduct == null)
                return;
            if (pivot == null)
                throw new ArgumentNullException(nameof(pivot));

            // Go through all elements below the pivot. If they exist, then we can subtract 1 from the Markowitz row vector!
            for (var column = pivot.Below; column != null; column = column.Below)
            {
                var row = column.Row;
                
                // Update the Markowitz product
                _markowitzProduct[row] -= _markowitzColumn[row];
                --_markowitzRow[row];

                // If we reached 0, then the row just turned to a singleton row
                if (_markowitzRow[row] == 0)
                    Singletons++;
            }

            // go through all elements right of the pivot. For every element, we can subtract 1 from the Markowitz column vector!
            for (var row = pivot.Right; row != null; row = row.Right)
            {
                var column = row.Column;
                
                // Update the Markowitz product
                _markowitzProduct[column] -= _markowitzRow[column];
                --_markowitzColumn[column];

                // If we reached 0, then the column just turned to a singleton column
                // This only adds a singleton if the row wasn't detected as a singleton row first
                if (_markowitzColumn[column] == 0 && _markowitzRow[column] != 0)
                    Singletons++;
            }
        }

        /// <summary>
        /// Find a pivot in the matrix.
        /// </summary>
        /// <param name="matrix">The matrix.</param>
        /// <param name="eliminationStep">The current elimination step.</param>
        /// <returns></returns>
        /// <remarks>
        /// The pivot should be searched for in the submatrix towards the right and down of the
        /// current diagonal at row/column <paramref name="eliminationStep" />. This pivot element
        /// will be moved to the diagonal for this elimination step.
        /// </remarks>
        public override MatrixElement<T> FindPivot(SparseMatrix<T> matrix, int eliminationStep)
        {
            foreach (var strategy in Strategies)
            {
                var chosen = strategy.FindPivot(this, matrix, eliminationStep);
                if (chosen != null)
                    return chosen;
            }
            return null;
        }
    }
}
