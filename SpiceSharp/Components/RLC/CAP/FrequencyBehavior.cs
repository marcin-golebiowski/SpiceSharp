﻿using System;
using System.Numerics;
using SpiceSharp.Algebra;
using SpiceSharp.Attributes;
using SpiceSharp.Behaviors;
using SpiceSharp.Simulations;

namespace SpiceSharp.Components.CapacitorBehaviors
{
    /// <summary>
    /// AC behavior for <see cref="Capacitor"/>
    /// </summary>
    public class FrequencyBehavior : BaseFrequencyBehavior, IConnectedBehavior
    {
        /// <summary>
        /// Necessary paramters and behaviors
        /// </summary>
        private BaseParameters _bp;

        /// <summary>
        /// Nodes
        /// </summary>
        private int _posNode, _negNode;
        protected MatrixElement<Complex> PosPosPtr { get; private set; }
        protected MatrixElement<Complex> NegNegPtr { get; private set; }
        protected MatrixElement<Complex> PosNegPtr { get; private set; }
        protected MatrixElement<Complex> NegPosPtr { get; private set; }

        [ParameterName("v"), ParameterInfo("Capacitor voltage")]
        public Complex GetVoltage(ComplexSimulationState state)
        {
            if (state == null)
                throw new ArgumentNullException(nameof(state));
            return state.Solution[_posNode] - state.Solution[_negNode];
        }
        [ParameterName("i"), ParameterName("c"), ParameterInfo("Capacitor current")]
        public Complex GetCurrent(ComplexSimulationState state)
        {
            if (state == null)
                throw new ArgumentNullException(nameof(state));
            var conductance = state.Laplace * _bp.Capacitance.Value;
            return (state.Solution[_posNode] - state.Solution[_negNode]) * conductance;
        }
        [ParameterName("p"), ParameterInfo("Capacitor power")]
        public Complex GetPower(ComplexSimulationState state)
        {
            if (state == null)
                throw new ArgumentNullException(nameof(state));
            var conductance = state.Laplace * _bp.Capacitance.Value;
            var voltage = state.Solution[_posNode] - state.Solution[_negNode];
            return voltage * Complex.Conjugate(voltage * conductance);
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">Name</param>
        public FrequencyBehavior(string name) : base(name) { }

        /// <summary>
        /// Setup the behavior
        /// </summary>
        /// <param name="simulation">Simulation</param>
        /// <param name="provider">Data provider</param>
        public override void Setup(Simulation simulation, SetupDataProvider provider)
        {
            if (provider == null)
                throw new ArgumentNullException(nameof(provider));

            // Get parameters
            _bp = provider.GetParameterSet<BaseParameters>();
        }
        
        /// <summary>
        /// Connect behavior
        /// </summary>
        /// <param name="pins"></param>
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
        /// Gets matrix pointers
        /// </summary>
        /// <param name="solver">The matrix</param>
        public override void GetEquationPointers(Solver<Complex> solver)
        {
			if (solver == null)
				throw new ArgumentNullException(nameof(solver));

            // Get matrix pointers
            PosPosPtr = solver.GetMatrixElement(_posNode, _posNode);
            NegNegPtr = solver.GetMatrixElement(_negNode, _negNode);
            NegPosPtr = solver.GetMatrixElement(_negNode, _posNode);
            PosNegPtr = solver.GetMatrixElement(_posNode, _negNode);
        }
        
        /// <summary>
        /// Execute behavior for AC analysis
        /// </summary>
        /// <param name="simulation">Frequency-based simulation</param>
        public override void Load(FrequencySimulation simulation)
        {
			if (simulation == null)
				throw new ArgumentNullException(nameof(simulation));

            var state = simulation.ComplexState;
            var val = state.Laplace * _bp.Capacitance.Value;

            // Load the Y-matrix
            PosPosPtr.Value += val;
            NegNegPtr.Value += val;
            PosNegPtr.Value -= val;
            NegPosPtr.Value -= val;
        }
    }
}
