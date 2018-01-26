﻿using SpiceSharp.Attributes;

namespace SpiceSharp.Components.VoltagesourceModel
{
    /// <summary>
    /// Parameters for a <see cref="Voltagesource"/>
    /// </summary>
    public class BaseParameters : ParameterSet
    {
        /// <summary>
        /// Parameters
        /// </summary>
        public Waveform VSRCwaveform { get; set; }
        [PropertyName("dc"), PropertyInfo("D.C. source value")]
        public Parameter VSRCdcValue { get; } = new Parameter();

        /// <summary>
        /// Constructor
        /// </summary>
        public BaseParameters()
        {
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="dc">DC value</param>
        public BaseParameters(double dc)
        {
            VSRCdcValue.Set(dc);
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="w">Waveform</param>
        public BaseParameters(Waveform w)
        {
            VSRCwaveform = w;
        }
    }
}
