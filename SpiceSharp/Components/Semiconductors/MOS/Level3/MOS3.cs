﻿using SpiceSharp.Attributes;
using SpiceSharp.Components.MosfetBehaviors.Level3;

namespace SpiceSharp.Components
{
    /// <summary>
    /// A MOS3 Mosfet
    /// Level 3, a semi-empirical model(see reference for level 3).
    /// </summary>
    [Pin(0, "Drain"), Pin(1, "Gate"), Pin(2, "Source"), Pin(3, "Bulk"), Connected(0, 2), Connected(0, 3)]
    public class MOS3 : Component
    {
        /// <summary>
        /// Set the model for the MOS3 model
        /// </summary>
        public void SetModel(MOS3Model model) => Model = model;

        /// <summary>
        /// Nodes
        /// </summary>
        [PropertyName("dnode"), PropertyInfo("Number of drain node")]
        public int DrainNode { get; internal set; }
        [PropertyName("gnode"), PropertyInfo("Number of gate node")]
        public int GateNode { get; internal set; }
        [PropertyName("snode"), PropertyInfo("Number of source node")]
        public int SourceNode { get; internal set; }
        [PropertyName("bnode"), PropertyInfo("Number of bulk node")]
        public int BulkNode { get; internal set; }

        /// <summary>
        /// Constants
        /// </summary>
        public const int MOS3PinCount = 4;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">The name of the device</param>
        public MOS3(Identifier name) : base(name, MOS3PinCount)
        {
            // Add parameters
            Parameters.Add(new BaseParameters());

            // Add factories
            AddFactory(typeof(TemperatureBehavior), () => new TemperatureBehavior(Name));
            AddFactory(typeof(LoadBehavior), () => new LoadBehavior(Name));
            AddFactory(typeof(FrequencyBehavior), () => new FrequencyBehavior(Name));
            AddFactory(typeof(TransientBehavior), () => new TransientBehavior(Name));
            AddFactory(typeof(NoiseBehavior), () => new NoiseBehavior(Name));
        }

        /// <summary>
        /// Setup the device
        /// </summary>
        /// <param name="circuit">The circuit</param>
        public override void Setup(Circuit circuit)
        {
            // Allocate nodes
            var nodes = BindNodes(circuit);
            DrainNode = nodes[0].Index;
            GateNode = nodes[1].Index;
            SourceNode = nodes[2].Index;
            BulkNode = nodes[3].Index;
        }
    }
}
