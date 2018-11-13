﻿using System;
using SpiceSharp.Attributes;
using SpiceSharp.Behaviors;
using SpiceSharp.Simulations;
using SpiceSharp.Simulations.Behaviors;

namespace SpiceSharp.Components.ResistorBehaviors
{
    /// <summary>
    /// Temperature behavior for a <see cref="Resistor"/>
    /// </summary>
    public class TemperatureBehavior : ExportingBehavior, ITemperatureBehavior, IConnectedBehavior
    {
        /// <summary>
        /// Gets the model parameters.
        /// </summary>
        /// <value>
        /// The model parameters.
        /// </value>
        protected ModelBaseParameters ModelParameters { get; private set; }

        /// <summary>
        /// Gets the base parameters.
        /// </summary>
        /// <value>
        /// The base parameters.
        /// </value>
        protected BaseParameters BaseParameters { get; private set; }

        /// <summary>
        /// Gets the default conductance for this model
        /// </summary>
        [ParameterName("g"), ParameterInfo("The conductance of the resistor.")]
        public double Conductance { get; private set; }

        /// <summary>
        /// Nodes.
        /// </summary>
        protected int PosNode { get; private set; }
        protected int NegNode { get; private set; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">Name</param>
        public TemperatureBehavior(string name) : base(name) { }

        /// <summary>
        /// Connect the behavior to nodes
        /// </summary>
        /// <param name="pins">Pins</param>
        public void Connect(params int[] pins)
        {
            if (pins == null)
                throw new ArgumentNullException(nameof(pins));
            if (pins.Length != 2)
                throw new CircuitException("Pin count mismatch: 2 pins expected, {0} given".FormatString(pins.Length));
            PosNode = pins[0];
            NegNode = pins[1];
        }

        /// <summary>
        /// Setup the behavior
        /// </summary>
        /// <param name="simulation">The simulation</param>
        /// <param name="provider">The setup data provider</param>
        public override void Setup(Simulation simulation, SetupDataProvider provider)
        {
			if (provider == null)
				throw new ArgumentNullException(nameof(provider));

            // Get parameters
            BaseParameters = provider.GetParameterSet<BaseParameters>();
            provider.TryGetParameterSet("model", out ModelBaseParameters mbp);
            ModelParameters = mbp;
        }
        
        /// <summary>
        /// Execute behavior
        /// </summary>
        /// <param name="simulation">Base simulation</param>
        public void Temperature(BaseSimulation simulation)
        {
			if (simulation == null)
				throw new ArgumentNullException(nameof(simulation));

            double factor;
            double resistance = BaseParameters.Resistance;

            // Default Value Processing for Resistor Instance
            if (!BaseParameters.Temperature.Given)
                BaseParameters.Temperature.RawValue = simulation.RealState.Temperature;
            if (!BaseParameters.Width.Given)
                BaseParameters.Width.RawValue = ModelParameters?.DefaultWidth ?? 0.0;

            if (ModelParameters != null)
            {
                if (!BaseParameters.Resistance.Given)
                {
                    if (ModelParameters.SheetResistance.Given && ModelParameters.SheetResistance > 0 && BaseParameters.Length > 0)
                        resistance = ModelParameters.SheetResistance * (BaseParameters.Length - ModelParameters.Narrow) / (BaseParameters.Width - ModelParameters.Narrow);
                    else
                    {
                        CircuitWarning.Warning(this, "{0}: resistance=0, set to 1000".FormatString(Name));
                        resistance = 1000;
                    }
                }

                var difference = BaseParameters.Temperature - ModelParameters.NominalTemperature;

                if (ModelParameters.ExponentialCoefficient.Given)
                    factor = Math.Pow(1.01, ModelParameters.ExponentialCoefficient * difference);
                else
                    factor = 1.0 + ModelParameters.TemperatureCoefficient1 * difference + ModelParameters.TemperatureCoefficient2 * difference * difference;
            }
            else
            {
                factor = 1.0;
            }

            if (resistance < 1e-12)
                resistance = 1e-12;

            Conductance = 1.0 / (resistance * factor);
        }
    }
}
