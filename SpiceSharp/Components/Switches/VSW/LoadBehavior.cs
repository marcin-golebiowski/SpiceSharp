﻿using System;
using SpiceSharp.Algebra;
using SpiceSharp.Behaviors;
using SpiceSharp.Simulations;

namespace SpiceSharp.Components.VoltageSwitchBehaviors
{
    /// <summary>
    /// Load behavior for a <see cref="VoltageSwitch"/>
    /// </summary>
    public class LoadBehavior : BaseLoadBehavior, IConnectedBehavior
    {
        /// <summary>
        /// Necessary behaviors
        /// </summary>
        private BaseParameters _bp;
        private ModelLoadBehavior _modelload;
        private ModelBaseParameters _mbp;

        /// <summary>
        /// Gets or sets the previous state
        /// </summary>
        public bool PreviousState { get; set; }

        /// <summary>
        /// Flag for using the previous state or not
        /// </summary>
        public bool UseOldState { get; set; }

        /// <summary>
        /// The current state
        /// </summary>
        public bool CurrentState { get; protected set; }

        /// <summary>
        /// Gets the current conductance
        /// </summary>
        public double Cond { get; protected set; }

        /// <summary>
        /// Nodes
        /// </summary>
        private int _posNode, _negNode, _contPosNode, _contNegNode;
        protected MatrixElement<double> PosPosPtr { get; private set; }
        protected MatrixElement<double> NegPosPtr { get; private set; }
        protected MatrixElement<double> PosNegPtr { get; private set; }
        protected MatrixElement<double> NegNegPtr { get; private set; }

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
            _mbp = provider.GetParameterSet<ModelBaseParameters>("model");

            // Get behaviors
            _modelload = provider.GetBehavior<ModelLoadBehavior>("model");
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
            if (solver == null)
                throw new ArgumentNullException(nameof(solver));

            PosPosPtr = solver.GetMatrixElement(_posNode, _posNode);
            PosNegPtr = solver.GetMatrixElement(_posNode, _negNode);
            NegPosPtr = solver.GetMatrixElement(_negNode, _posNode);
            NegNegPtr = solver.GetMatrixElement(_negNode, _negNode);
        }
        
        /// <summary>
        /// Execute behavior
        /// </summary>
        /// <param name="simulation">Base simulation</param>
        public override void Load(BaseSimulation simulation)
        {
            if (simulation == null)
                throw new ArgumentNullException(nameof(simulation));

            bool currentState;
            var state = simulation.RealState;

            if (state.Init == InitializationModes.Fix || state.Init == InitializationModes.Junction)
            {
                if (_bp.ZeroState)
                {
                    // Switch specified "on"
                    CurrentState = true;
                    currentState = true;
                }
                else
                {
                    // Switch specified "off"
                    CurrentState = false;
                    currentState = false;
                }
            }
            else
            {
                // First iteration after a new timepoint!
                var vCtrl = state.Solution[_contPosNode] - state.Solution[_contNegNode];
                if (UseOldState)
                {
                    // Calculate the current state
                    if (vCtrl > _mbp.Threshold + _mbp.Hysteresis)
                        currentState = true;
                    else if (vCtrl < _mbp.Threshold - _mbp.Hysteresis)
                        currentState = false;
                    else
                        currentState = PreviousState;
                    CurrentState = currentState;
                    UseOldState = false;
                }
                else
                {
                    // Continue from last iteration
                    PreviousState = CurrentState;

                    // Calculate the current state
                    if (vCtrl > _mbp.Threshold + _mbp.Hysteresis)
                    {
                        CurrentState = true;
                        currentState = true;
                    }
                    else if (vCtrl < _mbp.Threshold - _mbp.Hysteresis)
                    {
                        CurrentState = false;
                        currentState = false;
                    }
                    else
                        currentState = PreviousState;

                    // Ensure one more iteration
                    if (currentState != PreviousState)
                        state.IsConvergent = false;
                }
            }

            var gNow = currentState ? _modelload.OnConductance : _modelload.OffConductance;
            Cond = gNow;

            // Load the Y-matrix
            PosPosPtr.Value += gNow;
            PosNegPtr.Value -= gNow;
            NegPosPtr.Value -= gNow;
            NegNegPtr.Value += gNow;
        }
    }
}
