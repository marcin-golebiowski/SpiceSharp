﻿using System;
using System.Numerics;
using SpiceSharp.Algebra;
using SpiceSharp.Attributes;
using SpiceSharp.Behaviors;
using SpiceSharp.Simulations;

namespace SpiceSharp.Components.VoltageControlledVoltageSourceBehaviors
{
    /// <summary>
    /// AC behavior for a <see cref="VoltageControlledVoltageSource"/>
    /// </summary>
    public class FrequencyBehavior : BaseFrequencyBehavior, IConnectedBehavior
    {
        /// <summary>
        /// Necessary behaviors
        /// </summary>
        private BaseParameters _bp;
        private LoadBehavior _load;

        /// <summary>
        /// Nodes
        /// </summary>
        private int _posNode, _negNode, _contPosNode, _contNegNode, _branchEq;
        protected MatrixElement<Complex> PosBranchPtr { get; private set; }
        protected MatrixElement<Complex> NegBranchPtr { get; private set; }
        protected MatrixElement<Complex> BranchPosPtr { get; private set; }
        protected MatrixElement<Complex> BranchNegPtr { get; private set; }
        protected MatrixElement<Complex> BranchControlPosPtr { get; private set; }
        protected MatrixElement<Complex> BranchControlNegPtr { get; private set; }

        /// <summary>
        /// Device methods and properties
        /// </summary>
        [ParameterName("v"), ParameterInfo("Complex voltage")]
        public Complex GetVoltage(ComplexSimulationState state)
        {
			if (state == null)
				throw new ArgumentNullException(nameof(state));

            return state.Solution[_posNode] - state.Solution[_negNode];
        }
        [ParameterName("i"), ParameterName("c"), ParameterInfo("Complex current")]
        public Complex GetCurrent(ComplexSimulationState state)
        {
			if (state == null)
				throw new ArgumentNullException(nameof(state));

            return state.Solution[_branchEq];
        }
        [ParameterName("p"), ParameterInfo("Complex power")]
        public Complex GetPower(ComplexSimulationState state)
        {
			if (state == null)
				throw new ArgumentNullException(nameof(state));

            var v = state.Solution[_posNode] - state.Solution[_negNode];
            var i = state.Solution[_branchEq];
            return -v * Complex.Conjugate(i);
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">Name</param>
        public FrequencyBehavior(string name) : base(name) { }

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

            // Get behaviors
            _load = provider.GetBehavior<LoadBehavior>();
        }

        /// <summary>
        /// Connect
        /// </summary>
        /// <param name="pins">Pins</param>
        public void Connect(params int[] pins)
        {
            if (pins == null)
                throw new ArgumentNullException(nameof(pins));
            if (pins.Length != 4)
                throw new CircuitException("Pin count mismatch: 4 pins expected, {0} given".FormatString(pins.Length));
            _posNode = pins[0];
            _negNode = pins[1];
            _contPosNode = pins[2];
            _contNegNode = pins[3];
        }

        /// <summary>
        /// Gets matrix pointers
        /// </summary>
        /// <param name="solver">Solver</param>
        public override void GetEquationPointers(Solver<Complex> solver)
        {
			if (solver == null)
				throw new ArgumentNullException(nameof(solver));

            _branchEq = _load.BranchEq;
            PosBranchPtr = solver.GetMatrixElement(_posNode, _branchEq);
            NegBranchPtr = solver.GetMatrixElement(_negNode, _branchEq);
            BranchPosPtr = solver.GetMatrixElement(_branchEq, _posNode);
            BranchNegPtr = solver.GetMatrixElement(_branchEq, _negNode);
            BranchControlPosPtr = solver.GetMatrixElement(_branchEq, _contPosNode);
            BranchControlNegPtr = solver.GetMatrixElement(_branchEq, _contNegNode);
        }
        
        /// <summary>
        /// Execute behavior for AC analysis
        /// </summary>
        /// <param name="simulation">Frequency-based simulation</param>
        public override void Load(FrequencySimulation simulation)
        {
			if (simulation == null)
				throw new ArgumentNullException(nameof(simulation));

            // Load Y-matrix
            PosBranchPtr.Value += 1.0;
            BranchPosPtr.Value += 1.0;
            NegBranchPtr.Value -= 1.0;
            BranchNegPtr.Value -= 1.0;
            BranchControlPosPtr.Value -= _bp.Coefficient.Value;
            BranchControlNegPtr.Value += _bp.Coefficient.Value;
        }
    }
}
