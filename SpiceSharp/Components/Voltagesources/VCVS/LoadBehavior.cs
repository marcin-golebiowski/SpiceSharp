﻿using System;
using SpiceSharp.Algebra;
using SpiceSharp.Attributes;
using SpiceSharp.Behaviors;
using SpiceSharp.Simulations;

namespace SpiceSharp.Components.VoltageControlledVoltageSourceBehaviors
{
    /// <summary>
    /// General behavior for a <see cref="VoltageControlledVoltageSource"/>
    /// </summary>
    public class LoadBehavior : BaseLoadBehavior, IConnectedBehavior
    {
        /// <summary>
        /// Necessary behaviors and parameters
        /// </summary>
        private BaseParameters _bp;

        /// <summary>
        /// Device methods and properties
        /// </summary>
        [ParameterName("i"), ParameterInfo("Output current")]
        public double GetCurrent(BaseSimulationState state)
        {
            if (state == null)
                throw new ArgumentNullException(nameof(state));

            return state.Solution[BranchEq];
        }
        [ParameterName("v"), ParameterInfo("Output current")]
        public double GetVoltage(BaseSimulationState state)
        {
            if (state == null)
                throw new ArgumentNullException(nameof(state));

            return state.Solution[_posNode] - state.Solution[_negNode];
        }
        [ParameterName("p"), ParameterInfo("Power")]
        public double GetPower(BaseSimulationState state)
        { 
            if (state == null)
                throw new ArgumentNullException(nameof(state));

            return state.Solution[BranchEq] * (state.Solution[_posNode] - state.Solution[_negNode]);
        }

        /// <summary>
        /// Nodes
        /// </summary>
        private int _posNode, _negNode, _contPosNode, _contNegNode;
        public int BranchEq { get; private set; }
        protected MatrixElement<double> PosBranchPtr { get; private set; }
        protected MatrixElement<double> NegBranchPtr { get; private set; }
        protected MatrixElement<double> BranchPosPtr { get; private set; }
        protected MatrixElement<double> BranchNegPtr { get; private set; }
        protected MatrixElement<double> BranchControlPosPtr { get; private set; }
        protected MatrixElement<double> BranchControlNegPtr { get; private set; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">Name</param>
        public LoadBehavior(string name) : base(name) { }

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
        /// <param name="variables">Variables</param>
        /// <param name="solver">Solver</param>
        public override void GetEquationPointers(VariableSet variables, Solver<double> solver)
        {
            if (variables == null)
                throw new ArgumentNullException(nameof(variables));
            if (solver == null)
                throw new ArgumentNullException(nameof(solver));

            BranchEq = variables.Create(Name.Combine("branch"), VariableType.Current).Index;
            PosBranchPtr = solver.GetMatrixElement(_posNode, BranchEq);
            NegBranchPtr = solver.GetMatrixElement(_negNode, BranchEq);
            BranchPosPtr = solver.GetMatrixElement(BranchEq, _posNode);
            BranchNegPtr = solver.GetMatrixElement(BranchEq, _negNode);
            BranchControlPosPtr = solver.GetMatrixElement(BranchEq, _contPosNode);
            BranchControlNegPtr = solver.GetMatrixElement(BranchEq, _contNegNode);
        }
        
        /// <summary>
        /// Execute behavior
        /// </summary>
        /// <param name="simulation">Base simulation</param>
        public override void Load(BaseSimulation simulation)
        {
            PosBranchPtr.Value += 1;
            BranchPosPtr.Value += 1;
            NegBranchPtr.Value -= 1;
            BranchNegPtr.Value -= 1;
            BranchControlPosPtr.Value -= _bp.Coefficient;
            BranchControlNegPtr.Value += _bp.Coefficient;
        }
    }
}
