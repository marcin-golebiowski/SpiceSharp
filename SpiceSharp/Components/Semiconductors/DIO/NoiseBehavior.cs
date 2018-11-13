﻿using System;
using SpiceSharp.Behaviors;
using SpiceSharp.Components.NoiseSources;
using SpiceSharp.Simulations;

namespace SpiceSharp.Components.DiodeBehaviors
{
    /// <summary>
    /// Noise behavior for <see cref="Diode"/>
    /// </summary>
    public class NoiseBehavior : BaseNoiseBehavior, IConnectedBehavior
    {
        /// <summary>
        /// Necessary behaviors
        /// </summary>
        private LoadBehavior _load;
        private BaseParameters _bp;
        private ModelNoiseParameters _mnp;
        private ModelTemperatureBehavior _modeltemp;

        /// <summary>
        /// Nodes
        /// </summary>
        private int _posNode, _negNode, _posPrimeNode;

        /// <summary>
        /// Noise sources by their index
        /// </summary>
        private const int RsNoise = 0;
        private const int IdNoise = 1;
        private const int FlickerNoise = 2;

        /// <summary>
        /// Noise generators
        /// </summary>
        public ComponentNoise DiodeNoise { get; } = new ComponentNoise(
            new NoiseThermal("rs", 0, 1),
            new NoiseShot("id", 1, 2),
            new NoiseGain("1overf", 1, 2));

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">Name</param>
        public NoiseBehavior(string name) : base(name) { }

        /// <summary>
        /// Setup behavior
        /// </summary>
        /// <param name="simulation">Simulation</param>
        /// <param name="provider">Data provider</param>
        public override void Setup(Simulation simulation, SetupDataProvider provider)
        {
            base.Setup(simulation, provider);
            if (provider == null)
                throw new ArgumentNullException(nameof(provider));

            // Get parameters
            _bp = provider.GetParameterSet<BaseParameters>();
            _mnp = provider.GetParameterSet<ModelNoiseParameters>("model");

            // Get behaviors
            _load = provider.GetBehavior<LoadBehavior>();
            _modeltemp = provider.GetBehavior<ModelTemperatureBehavior>("model");
        }

        /// <summary>
        /// Connect the noise source
        /// </summary>
        public override void ConnectNoise()
        {
            // Get extra equations
            _posPrimeNode = _load.PosPrimeNode;

            // Connect noise sources
            DiodeNoise.Setup(_posNode, _posPrimeNode, _negNode);
        }
        
        /// <summary>
        /// Connect the behavior
        /// </summary>
        /// <param name="pins">Pins</param>
        public void Connect(params int[] pins)
        {
            if (pins == null)
                throw new ArgumentNullException(nameof(pins));
            if (pins.Length != 2)
                throw new CircuitException("Pin count mismatch: 2 pins expected, {0} given".FormatString(pins.Length));
            _posNode = pins[0];
            _negNode = pins[1];
        }

        /// <summary>
        /// Noise calculations
        /// </summary>
        /// <param name="simulation">Noise simulation</param>
        public override void Noise(Noise simulation)
        {
            if (simulation == null)
                throw new ArgumentNullException(nameof(simulation));
            var noise = simulation.NoiseState;

            // Set noise parameters
            DiodeNoise.Generators[RsNoise].SetCoefficients(_modeltemp.Conductance * _bp.Area);
            DiodeNoise.Generators[IdNoise].SetCoefficients(_load.Current);
            DiodeNoise.Generators[FlickerNoise].SetCoefficients(_mnp.FlickerNoiseCoefficient * Math.Exp(_mnp.FlickerNoiseExponent 
                * Math.Log(Math.Max(Math.Abs(_load.Current), 1e-38))) / noise.Frequency);

            // Evaluate noise
            DiodeNoise.Evaluate(simulation);
        }
    }
}
