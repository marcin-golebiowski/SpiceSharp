﻿using System;
using SpiceSharp.Behaviors;
using SpiceSharp.Simulations;

namespace SpiceSharp.Components.ResistorBehaviors
{
    /// <summary>
    /// Temperature behavior for a <see cref="Resistor"/>
    /// </summary>
    public class TemperatureBehavior : BaseTemperatureBehavior
    {
        /// <summary>
        /// Gets or sets the random generator for resistors
        /// </summary>
        public static Random Generator { get; set; } = new Random();

        /// <summary>
        /// Necessary parameters and behaviors
        /// </summary>
        private ModelBaseParameters _mbp;
        private BaseParameters _bp;
        private double _original = double.NaN;

        /// <summary>
        /// Gets the default conductance for this model
        /// </summary>
        public double Conductance { get; protected set; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">Name</param>
        public TemperatureBehavior(string name) : base(name) { }

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
            _bp = provider.GetParameterSet<BaseParameters>();
            provider.TryGetParameterSet("model", out _mbp);
        }

        /// <summary>
        /// Unsetup the behavior
        /// </summary>
        /// <param name="simulation">Simulation</param>
        public override void Unsetup(Simulation simulation)
        {
            base.Unsetup(simulation);

            // Revert the parameter value
            if (!double.IsNaN(_original))
                _bp.Resistance.RawValue = _original;

            // Clear references
            _bp = null;
            _mbp = null;
        }

        /// <summary>
        /// Execute behavior
        /// </summary>
        /// <param name="simulation">Base simulation</param>
        public override void Temperature(BaseSimulation simulation)
        {
			if (simulation == null)
				throw new ArgumentNullException(nameof(simulation));

            double factor;
            double resist = _bp.Resistance;

            // Default Value Processing for Resistor Instance
            if (!_bp.Temperature.Given)
                _bp.Temperature.RawValue = simulation.RealState.Temperature;
            if (!_bp.Width.Given)
                _bp.Width.RawValue = _mbp?.DefaultWidth ?? 0.0;

            if (_mbp != null)
            {
                if (!_bp.Resistance.Given)
                {
                    if (_mbp.SheetResistance.Given && _mbp.SheetResistance > 0 && _bp.Length > 0)
                        resist = _mbp.SheetResistance * (_bp.Length - _mbp.Narrow) / (_bp.Width - _mbp.Narrow);
                    else
                    {
                        CircuitWarning.Warning(this, "{0}: resistance=0, set to 1000".FormatString(Name));
                        resist = 1000;
                    }
                }

                var difference = _bp.Temperature - _mbp.NominalTemperature;
                factor = 1.0 + _mbp.TemperatureCoefficient1 * difference + _mbp.TemperatureCoefficient2 * difference * difference;
            }
            else
            {
                factor = 1.0;
            }

            if (resist < 1e-12)
                resist = 1e-12;
            Conductance = 1.0 / (resist * factor);
        }
    }
}
