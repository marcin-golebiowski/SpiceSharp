﻿using System;
using System.Collections.Generic;

namespace SpiceSharp.IntegrationMethods
{
    /// <summary>
    /// A class that implements a history with an array.
    /// </summary>
    /// <typeparam name="T">The base value type.</typeparam>
    /// <seealso cref="History{T}" />
    public class ArrayHistory<T> : History<T>
    {
        /// <summary>
        /// Gets or sets the current value.
        /// </summary>
        /// <value>
        /// The current value.
        /// </value>
        public override T Current
        {
            get => _history[0];
            set => _history[0] = value;
        }

        /// <summary>
        /// Gets the value at the specified index.
        /// </summary>
        /// <value>
        /// The value at the specified index.
        /// </value>
        /// <param name="index">The index.</param>
        /// <returns>
        /// The value at the specified index.
        /// </returns>
        /// <exception cref="ArgumentException">Invalid index {0}".FormatString(index)</exception>
        public override T this[int index]
        {
            get
            {
                if (index < 0 || index >= Length)
                    throw new ArgumentException("Invalid index {0}".FormatString(index));
                return _history[index];
            }
        }

        /// <summary>
        /// Gets all points in the history.
        /// </summary>
        /// <value>
        /// The points.
        /// </value>
        protected override IEnumerable<T> Points => _history;

        /// <summary>
        /// Timesteps in history
        /// </summary>
        private readonly T[] _history;

        /// <summary>
        /// Initializes a new instance of the <see cref="ArrayHistory{T}"/> class.
        /// </summary>
        /// <param name="length">The number of points to store.</param>
        public ArrayHistory(int length)
            : base(length)
        {
            _history = new T[length];
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ArrayHistory{T}"/> class.
        /// </summary>
        /// <param name="length">The number of points to store.</param>
        /// <param name="defaultValue">The default value.</param>
        public ArrayHistory(int length, T defaultValue)
            : base(length)
        {
            _history = new T[length];
            for (var i = 0; i < length; i++)
                _history[i] = defaultValue;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ArrayHistory{T}"/> class.
        /// </summary>
        /// <param name="length">The number of points to store.</param>
        /// <param name="generator">The function that generates the initial values.</param>
        /// <exception cref="ArgumentNullException">generator</exception>
        public ArrayHistory(int length, Func<int, T> generator)
            : base(length)
        {
            if (generator == null)
                throw new ArgumentNullException(nameof(generator));

            for (var i = 0; i < length; i++)
                _history[i] = generator(i);
        }

        /// <summary>
        /// Cycles through history.
        /// </summary>
        public override void Cycle()
        {
            var tmp = _history[Length - 1];
            for (var i = Length - 1; i > 0; i--)
                _history[i] = _history[i - 1];
            _history[0] = tmp;
        }

        /// <summary>
        /// Store a new value in the history.
        /// </summary>
        /// <param name="newValue">The new value.</param>
        public override void Store(T newValue)
        {
            // Shift the history
            for (var i = Length - 1; i > 0; i--)
                _history[i] = _history[i - 1];
            _history[0] = newValue;
        }

        /// <summary>
        /// Clear the whole history with the same value.
        /// </summary>
        /// <param name="value">The value to be cleared with.</param>
        public override void Clear(T value)
        {
            for (var i = 0; i < Length; i++)
                _history[i] = value;
        }

        /// <summary>
        /// Clear the history using a function by index.
        /// </summary>
        /// <param name="generator">The function generating the values.</param>
        /// <exception cref="ArgumentNullException">generator</exception>
        public override void Clear(Func<int, T> generator)
        {
            if (generator == null)
                throw new ArgumentNullException(nameof(generator));

            for (var i = 0; i < Length; i++)
                _history[i] = generator(i);
        }
    }
}
