﻿using System;
using SpiceSharp.Algebra.Solve;

// ReSharper disable once CheckNamespace
namespace SpiceSharp.Algebra
{
    /// <summary>
    /// A base class for linear systems that can be solved.
    /// </summary>
    /// <typeparam name="T">The base value type.</typeparam>
    public abstract class Solver<T> : SparseLinearSystem<T> where T : IFormattable, IEquatable<T>
    {
        /// <summary>
        /// Number of fill-ins in the matrix generated by the solver.
        /// </summary>
        /// <remarks>
        /// Fill-ins are elements that were auto-generated as a consequence
        /// of the solver trying to solve the matrix. To save memory, this
        /// number should remain small.
        /// </remarks>
        public int Fillins { get; private set; }

        /// <summary>
        /// Gets or sets a flag that reordering is required.
        /// </summary>
        public bool NeedsReordering { get; set; }

        /// <summary>
        /// Gets whether or not the solver is factored.
        /// </summary>
        public bool IsFactored { get; protected set; }

        /// <summary>
        /// Gets the pivot strategy used.
        /// </summary>
        public PivotStrategy<T> Strategy { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="Solver{T}"/> class.
        /// </summary>
        /// <param name="strategy">The pivot strategy that needs to be used.</param>
        protected Solver(PivotStrategy<T> strategy)
        {
            NeedsReordering = true;
            Strategy = strategy;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Solver{T}"/> class.
        /// </summary>
        /// <param name="strategy">The pivot strategy that needs to be used.</param>
        /// <param name="size">The number of equations/variables.</param>
        protected Solver(PivotStrategy<T> strategy, int size)
            : base(size)
        {
            NeedsReordering = true;
            Strategy = strategy;
        }

        /// <summary>
        /// Solve the system of equations.
        /// </summary>
        /// <param name="solution">The solution vector that will hold the solution to the set of equations.</param>
        public abstract void Solve(Vector<T> solution);

        /// <summary>
        /// Solve the transposed problem.
        /// </summary>
        /// <param name="solution">The solution vector that will hold the solution to the transposed set of equations.</param>
        public abstract void SolveTransposed(Vector<T> solution);

        /// <summary>
        /// Factor the matrix.
        /// </summary>
        /// <returns>True if factoring was successful.</returns>
        public abstract bool Factor();

        /// <summary>
        /// Order and factor the matrix.
        /// </summary>
        public abstract void OrderAndFactor();

        /// <summary>
        /// Move a chosen pivot to the diagonal.
        /// </summary>
        /// <param name="pivot">The pivot element.</param>
        /// <param name="step">The current step of factoring.</param>
        public void MovePivot(MatrixElement<T> pivot, int step)
        {
            if (pivot == null)
                throw new ArgumentNullException(nameof(pivot));
            Strategy.MovePivot(Matrix, Rhs, pivot, step);

            // Move the pivot in the matrix
            var row = pivot.Row;
            var column = pivot.Column;
            if (row != step)
                SwapRows(row, step);
            if (column != step)
                SwapColumns(column, step);

            Strategy.Update(Matrix, pivot, step);
        }

        /// <summary>
        /// Create a fill-in element.
        /// </summary>
        /// <param name="row">The row index.</param>
        /// <param name="column">The column index.</param>
        /// <returns>The created element.</returns>
        protected virtual MatrixElement<T> CreateFillin(int row, int column)
        {
            var result = Matrix.GetElement(row, column);
            Fillins++;
            return result;
        }

        /// <summary>
        /// Clear the system of linear equations.
        /// </summary>
        public override void Clear()
        {
            base.Clear();
            IsFactored = false;
        }
    }
}
