﻿using SpiceSharp.Simulations.Base;
using System;

namespace SpiceSharp.Simulations
{
    /// <summary>
    /// This class can export real properties.
    /// </summary>
    /// <seealso cref="Export{S, T}" />
    public class RealPropertyExport : Export<IEventfulSimulation, double>
    {
        /// <summary>
        /// Gets the path to the entity.
        /// </summary>
        public Reference Entity { get; }

        /// <summary>
        /// Gets property name.
        /// </summary>
        public string PropertyName { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="RealPropertyExport"/> class.
        /// </summary>
        /// <param name="simulation">The simulation.</param>
        /// <param name="entity">The path to the entity (can be a string or string array).</param>
        /// <param name="propertyName">The name of the property.</param>
        public RealPropertyExport(IEventfulSimulation simulation, Reference entity, string propertyName)
            : base(simulation)
        {
            if (entity.Length == 0)
                throw new ArgumentException(Properties.Resources.References_IsEmptyReference, nameof(entity));
            Entity = entity;
            PropertyName = propertyName.ThrowIfNull(nameof(propertyName));
        }

        /// <inheritdoc />
        protected override void Initialize(object sender, EventArgs e)
        {
            var behaviors = Entity.GetContainer(Simulation);
            Extractor = behaviors.CreatePropertyGetter<double>(PropertyName);
        }

        /// <summary>
        /// Converts the export to a string.
        /// </summary>
        /// <returns>Returns the export represented as a string.</returns>
        public override string ToString()
            => "@{0}[{1}]".FormatString(Entity, PropertyName);
    }
}
