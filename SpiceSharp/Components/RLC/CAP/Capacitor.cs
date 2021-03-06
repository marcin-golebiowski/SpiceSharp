﻿using SpiceSharp.Attributes;
using SpiceSharp.Components.CapacitorBehaviors;

namespace SpiceSharp.Components
{
    /// <summary>
    /// A capacitor
    /// </summary>
    [Pin(0, "C+"), Pin(1, "C-"), Connected]
    public class Capacitor : Component
    {
        /// <summary>
        /// Set the model for the capacitor
        /// </summary>
        /// <param name="model"></param>
        public void SetModel(CapacitorModel model) => Model = model;

        /// <summary>
        /// Constants
        /// </summary>
        [ParameterName("pincount"), ParameterInfo("Number of pins")]
		public const int CapacitorPinCount = 2;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name"></param>
        public Capacitor(string name) : base(name, CapacitorPinCount)
        {
            // Register parameters
            ParameterSets.Add(new BaseParameters());

            // Register factories
            Behaviors.Add(typeof(TransientBehavior), () => new TransientBehavior(Name));
            Behaviors.Add(typeof(FrequencyBehavior), () => new FrequencyBehavior(Name));
            Behaviors.Add(typeof(TemperatureBehavior), () => new TemperatureBehavior(Name));
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">The name of the capacitor</param>
        /// <param name="pos">The positive node</param>
        /// <param name="neg">The negative node</param>
        /// <param name="cap">The capacitance</param>
        public Capacitor(string name, string pos, string neg, double cap) 
            : base(name, CapacitorPinCount)
        {
            // Register parameters
            ParameterSets.Add(new BaseParameters(cap));

            // Register factories
            Behaviors.Add(typeof(TransientBehavior), () => new TransientBehavior(Name));
            Behaviors.Add(typeof(FrequencyBehavior), () => new FrequencyBehavior(Name));
            Behaviors.Add(typeof(TemperatureBehavior), () => new TemperatureBehavior(Name));

            // Connect
            Connect(pos, neg);
        }
    }
}
