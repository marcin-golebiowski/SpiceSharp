﻿using System;
using System.Numerics;
using SpiceSharp.Behaviors;
using SpiceSharp.Sparse;
using SpiceSharp.Simulations;

namespace SpiceSharp.Components.CapacitorBehaviors
{
    /// <summary>
    /// AC behavior for <see cref="Components.Capacitor"/>
    /// </summary>
    public class FrequencyBehavior : Behaviors.FrequencyBehavior, IConnectedBehavior
    {
        /// <summary>
        /// Necessary paramters and behaviors
        /// </summary>
        BaseParameters bp;

        /// <summary>
        /// Nodes
        /// </summary>
        int CAPposNode, CAPnegNode;
        MatrixElement CAPposPosptr;
        MatrixElement CAPnegNegptr;
        MatrixElement CAPposNegptr;
        MatrixElement CAPnegPosptr;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">Name</param>
        public FrequencyBehavior(Identifier name) : base(name) { }

        /// <summary>
        /// Export methods for AC behavior
        /// </summary>
        /// <param name="property">Property</param>
        /// <returns></returns>
        public override Func<State, Complex> CreateAcExport(string property)
        {
            switch (property)
            {
                case "v": return (State state) => new Complex(state.Solution[CAPposNode] - state.Solution[CAPnegNode], state.iSolution[CAPposNode] - state.iSolution[CAPnegNode]);
                case "i": return (State state) =>
                {
                    Complex voltage = new Complex(state.Solution[CAPposNode] - state.Solution[CAPnegNode], state.iSolution[CAPposNode] - state.iSolution[CAPnegNode]);
                    return state.Laplace * bp.CAPcapac.Value * voltage;
                };
                case "p": return (State state) =>
                {
                    Complex voltage = new Complex(state.Solution[CAPposNode] - state.Solution[CAPnegNode], state.iSolution[CAPposNode] - state.iSolution[CAPnegNode]);
                    Complex current = state.Laplace * bp.CAPcapac.Value * voltage;
                    return voltage * Complex.Conjugate(current);
                };
                default: return null;
            }
        }

        /// <summary>
        /// Setup the behavior
        /// </summary>
        /// <param name="provider">Data provider</param>
        public override void Setup(SetupDataProvider provider)
        {
            if (provider == null)
                throw new ArgumentNullException(nameof(provider));

            // Get parameters
            bp = provider.GetParameterSet<BaseParameters>(0);
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
                throw new Diagnostics.CircuitException($"Pin count mismatch: 2 pins expected, {pins.Length} given");
            CAPposNode = pins[0];
            CAPnegNode = pins[1];
        }

        /// <summary>
        /// Get matrix pointers
        /// </summary>
        /// <param name="matrix">The matrix</param>
        public override void GetMatrixPointers(Matrix matrix)
        {
			if (matrix == null)
				throw new ArgumentNullException(nameof(matrix));


            CAPposPosptr = matrix.GetElement(CAPposNode, CAPposNode);
            CAPnegNegptr = matrix.GetElement(CAPnegNode, CAPnegNode);
            CAPnegPosptr = matrix.GetElement(CAPnegNode, CAPposNode);
            CAPposNegptr = matrix.GetElement(CAPposNode, CAPnegNode);
        }
        
        /// <summary>
        /// Execute behavior for AC analysis
        /// </summary>
        /// <param name="sim">Frequency-based simulation</param>
        public override void Load(FrequencySimulation sim)
        {
			if (sim == null)
				throw new ArgumentNullException(nameof(sim));

            var state = sim.State;
            var val = state.Laplace * bp.CAPcapac.Value;

            // Load the matrix
            CAPposPosptr.Add(val);
            CAPnegNegptr.Add(val);
            CAPposNegptr.Sub(val);
            CAPnegPosptr.Sub(val);
        }
    }
}
