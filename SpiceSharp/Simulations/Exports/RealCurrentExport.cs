﻿using SpiceSharp.Behaviors;
using SpiceSharp.Components;
using SpiceSharp.Components.Subcircuits;
using System;
using System.Collections.Generic;

namespace SpiceSharp.Simulations
{
    /// <summary>
    /// This class can export real currents.
    /// </summary>
    /// <seealso cref="Export{S, T}" />
    public class RealCurrentExport : Export<IBiasingSimulation, double>
    {
        /// <summary>
        /// Gets the name of the voltage source.
        /// </summary>
        public IReadOnlyList<string> SourcePath { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="RealCurrentExport"/> class.
        /// </summary>
        /// <param name="simulation">The simulation.</param>
        /// <param name="source">The source name.</param>
        public RealCurrentExport(IBiasingSimulation simulation, string source)
            : base(simulation)
        {
            source.ThrowIfNull(nameof(source));
            SourcePath = new[] { source };
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RealCurrentExport"/> class.
        /// </summary>
        /// <param name="simulation">The simulation.</param>
        /// <param name="sourcePath">The path to the source that defines a current.</param>
        public RealCurrentExport(IBiasingSimulation simulation, string[] sourcePath)
            : base(simulation)
        {
            if (sourcePath == null || sourcePath.Length == 0)
                throw new ArgumentNullException(nameof(sourcePath), "sourcePath cannot be null or empty");
            SourcePath = new List<string>(sourcePath);
        }

        /// <summary>
        /// Initializes the export.
        /// </summary>
        /// <param name="sender">The object (simulation) sending the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected override void Initialize(object sender, EventArgs e)
        {
            var behaviors = new HashSet<IBehavior>();
            foreach (var behavior in Simulation.EntityBehaviors[SourcePath[0]])
                behaviors.Add(behavior);

            for (int i = 1; i < SourcePath.Count; i++)
            {
                string nextComponentName = SourcePath[i];

                var subBehaviors = new HashSet<IBehavior>();
                foreach (var behavior in behaviors)
                {
                    if (behavior is EntitiesBehavior entitiesBehavior &&
                        entitiesBehavior.LocalBehaviors.TryGetBehaviors(SourcePath[i], out var behaviorContainer))
                    {
                        foreach (var subBehavior in behaviorContainer)
                            subBehaviors.Add(subBehavior);
                    }
                }

                // If we didn't find any new behaviors, then that means that the last level was not a subcircuit
                if (subBehaviors.Count == 0)
                    throw new SpiceSharpException($"Entity {SourcePath[i - 1]} is not a subcircuit.");
                behaviors = subBehaviors;
            }

            // Find a branched behavior for the current
            foreach (var behavior in behaviors)
            {
                if (behavior is IBranchedBehavior<double> branchedBehavior)
                {
                    var branch = branchedBehavior.Branch;
                    Extractor = () => branch.Value;
                }
            }
        }
    }
}
