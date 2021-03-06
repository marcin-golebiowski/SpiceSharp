﻿using System;
using System.Collections.Generic;
using SpiceSharp.Behaviors;

namespace SpiceSharp.Simulations
{
    /// <summary>
    /// This class can export real properties.
    /// </summary>
    /// <seealso cref="Export{T}" />
    public class RealPropertyExport : Export<double>
    {
        /// <summary>
        /// Gets the identifier of the entity.
        /// </summary>
        /// <value>
        /// The identifier of the entity.
        /// </value>
        public string EntityName { get; }

        /// <summary>
        /// Gets the name of the property.
        /// </summary>
        /// <value>
        /// The property name.
        /// </value>
        public string PropertyName { get; }

        /// <summary>
        /// Gets the comparer for finding the parameter.
        /// </summary>
        /// <value>
        /// The comparer.
        /// </value>
        public IEqualityComparer<string> Comparer { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="RealPropertyExport"/> class.
        /// </summary>
        /// <param name="simulation">The simulation.</param>
        /// <param name="entityName">The identifier of the entity.</param>
        /// <param name="propertyName">The name of the property.</param>
        /// <param name="comparer">The <see cref="IEqualityComparer{T}" /> implementation to use when comparing parameter names, or <c>null</c> to use the default <see cref="EqualityComparer{T}"/>.</param>
        /// <exception cref="ArgumentNullException">
        /// entityName
        /// or
        /// propertyName
        /// </exception>
        public RealPropertyExport(Simulation simulation, string entityName, string propertyName, IEqualityComparer<string> comparer = null)
            : base(simulation)
        {
            EntityName = entityName ?? throw new ArgumentNullException(nameof(entityName));
            PropertyName = propertyName ?? throw new ArgumentNullException(nameof(propertyName));
            Comparer = comparer ?? EqualityComparer<string>.Default;
        }

        /// <summary>
        /// Initializes the export.
        /// </summary>
        /// <param name="sender">The object (simulation) sending the event.</param>
        /// <param name="e">The <see cref="T:System.EventArgs" /> instance containing the event data.</param>
        /// <exception cref="ArgumentNullException">e</exception>
        protected override void Initialize(object sender, EventArgs e)
        {
            if (e == null)
                throw new ArgumentNullException(nameof(e));
            var simulation = (Simulation) sender;
            var eb = simulation.EntityBehaviors[EntityName];

            // Get the necessary behavior in order:
            // 1) First try transient analysis
            if (eb.TryGetValue(typeof(BaseTransientBehavior), out var behavior))
                Extractor = behavior.CreateGetter(Simulation, PropertyName, Comparer);

            // 2) Second, try the load behavior
            if (Extractor == null)
            {
                if (eb.TryGetValue(typeof(BaseLoadBehavior), out behavior))
                    Extractor = behavior.CreateGetter(Simulation, PropertyName, Comparer);
            }

            // 3) Thirdly, check temperature behavior
            if (Extractor == null)
            {
                if (eb.TryGetValue(typeof(BaseTemperatureBehavior), out behavior))
                    Extractor = behavior.CreateGetter(Simulation, PropertyName, Comparer);
            }

            // 4) Check parameter sets
            if (Extractor == null)
            {
                // Get all parameter sets associated with the entity
                var ps = simulation.EntityParameters[EntityName];
                foreach (var p in ps.Values)
                {
                    Extractor = p.CreateGetter<double>(PropertyName, Comparer);
                    if (Extractor != null)
                        break;
                }
            }
        }
    }
}
