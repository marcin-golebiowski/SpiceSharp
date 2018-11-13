﻿using System;
using SpiceSharp.Algebra;
using SpiceSharp.Attributes;
using SpiceSharp.Behaviors;
using SpiceSharp.Simulations;

namespace SpiceSharp.Components.VoltageSourceBehaviors
{
    /// <summary>
    /// General behavior for <see cref="VoltageSource"/>
    /// </summary>
    public class LoadBehavior : BaseLoadBehavior, IConnectedBehavior
    {
        /// <summary>
        /// Necessary behaviors and parameters
        /// </summary>
        private CommonBehaviors.IndependentBaseParameters _bp;

        /// <summary>
        /// Device methods and properties
        /// </summary>
        [ParameterName("i"), ParameterInfo("Voltage source current")]
        public double GetCurrent(BaseSimulationState state)
        {
            if (state == null)
                throw new ArgumentNullException(nameof(state));

            return state.Solution[BranchEq];
        }
        [ParameterName("p"), ParameterInfo("Instantaneous power")]
        public double GetPower(BaseSimulationState state)
        {
            if (state == null)
                throw new ArgumentNullException(nameof(state));

            return (state.Solution[_posNode] - state.Solution[_negNode]) * -state.Solution[BranchEq];
        }
        [ParameterName("v"), ParameterInfo("Instantaneous voltage")]
        public double Voltage { get; protected set; }

        /// <summary>
        /// Nodes
        /// </summary>
        private int _posNode, _negNode;
        public int BranchEq { get; protected set; }
        protected MatrixElement<double> PosBranchPtr { get; private set; }
        protected MatrixElement<double> NegBranchPtr { get; private set; }
        protected MatrixElement<double> BranchPosPtr { get; private set; }
        protected MatrixElement<double> BranchNegPtr { get; private set; }
        protected VectorElement<double> BranchPtr { get; private set; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">Name</param>
        public LoadBehavior(string name) : base(name) { }

        /// <summary>
        /// Setup the behavior
        /// </summary>
        /// <param name="simulation">Simulation</param>
        /// <param name="provider">Data provider</param>
        public override void Setup(Simulation simulation, SetupDataProvider provider)
        {
            base.Setup(simulation, provider);
            if (provider == null)
                throw new ArgumentNullException(nameof(provider));

            // Get parameters
            _bp = provider.GetParameterSet<CommonBehaviors.IndependentBaseParameters>();

            // Setup the waveform
            _bp.Waveform?.Setup();

            // Calculate the voltage source's complex value
            if (!_bp.DcValue.Given)
            {
                // No DC value: either have a transient value or none
                CircuitWarning.Warning(this,
                    _bp.Waveform != null
                        ? "{0}: No DC value, transient time 0 value used".FormatString(Name)
                        : "{0}: No value, DC 0 assumed".FormatString(Name));
            }
        }

        /// <summary>
        /// Connect the load behavior
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

            // Get matrix elements
            PosBranchPtr = solver.GetMatrixElement(_posNode, BranchEq);
            BranchPosPtr = solver.GetMatrixElement(BranchEq, _posNode);
            NegBranchPtr = solver.GetMatrixElement(_negNode, BranchEq);
            BranchNegPtr = solver.GetMatrixElement(BranchEq, _negNode);

            // Get rhs elements
            BranchPtr = solver.GetRhsElement(BranchEq);
        }
        
        /// <summary>
        /// Execute behavior
        /// </summary>
        /// <param name="simulation">Base simulation</param>
        public override void Load(BaseSimulation simulation)
        {
            if (simulation == null)
                throw new ArgumentNullException(nameof(simulation));

            var state = simulation.RealState;
            double value;

            PosBranchPtr.Value += 1;
            BranchPosPtr.Value += 1;
            NegBranchPtr.Value -= 1;
            BranchNegPtr.Value -= 1;

            if (simulation is TimeSimulation ts)
            {
                var time = ts.Method.Time;

                // Use the waveform if possible
                if (_bp.Waveform != null)
                    value = _bp.Waveform.Value;
                else
                    value = _bp.DcValue * state.SourceFactor;
            }
            else
            {
                value = _bp.DcValue * state.SourceFactor;
            }
            BranchPtr.Value += value;
        }
    }
}
