﻿using System;
using SpiceSharp.Attributes;
using SpiceSharp.Simulations;

namespace SpiceSharp.Components
{
    /// <summary>
    /// This class describes a sine wave.
    /// </summary>
    /// <seealso cref="Waveform" />
    public class Sine : Waveform
    {
        /// <summary>
        /// Gets the offset.
        /// </summary>
        /// <value>
        /// The offset.
        /// </value>
        [ParameterName("vo"), ParameterInfo("The offset of the sine wave")]
        public GivenParameter<double> Offset { get; } = new GivenParameter<double>();

        /// <summary>
        /// Gets the amplitude of the sine wave.
        /// </summary>
        /// <value>
        /// The amplitude.
        /// </value>
        [ParameterName("va"), ParameterInfo("The amplitude of the sine wave")]
        public GivenParameter<double> Amplitude { get; } = new GivenParameter<double>();

        /// <summary>
        /// Gets the frequency of the sine wave in Hertz (Hz).
        /// </summary>
        /// <value>
        /// The frequency.
        /// </value>
        [ParameterName("freq"), ParameterInfo("The frequency in Hz")]
        public GivenParameter<double> Frequency { get; } = new GivenParameter<double>();

        /// <summary>
        /// Gets the delay of the sine wave in seconds.
        /// </summary>
        /// <value>
        /// The delay.
        /// </value>
        [ParameterName("td"), ParameterInfo("The delay in seconds")]
        public GivenParameter<double> Delay { get; } = new GivenParameter<double>();


        /// <summary>
        /// Gets the damping factor theta of the sinewave.
        /// </summary>
        /// <value>
        /// The damping factor.
        /// </value>
        [ParameterName("theta"), ParameterInfo("The damping factor")]
        public GivenParameter<double> Theta { get; } = new GivenParameter<double>();

        /// <summary>
        /// Private variables
        /// </summary>
        private double _vo, _va, _freq, _td, _theta;

        /// <summary>
        /// Initializes a new instance of the <see cref="Sine"/> class.
        /// </summary>
        public Sine()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Sine"/> class.
        /// </summary>
        /// <param name="offset">The offset.</param>
        /// <param name="amplitude">The amplitude.</param>
        /// <param name="frequency">The frequency.</param>
        /// <param name="delay">The delay.</param>
        /// <param name="theta">The theta.</param>
        public Sine(double offset, double amplitude, double frequency, double delay, double theta)
        {
            Offset.Value = offset;
            Amplitude.Value = amplitude;
            Frequency.Value = frequency;
            Delay.Value = delay;
            Theta.Value = theta;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Sine"/> class.
        /// </summary>
        /// <param name="offset">The offset.</param>
        /// <param name="amplitude">The amplitude.</param>
        /// <param name="frequency">The frequency.</param>
        public Sine(double offset, double amplitude, double frequency)
        {
            Offset.Value = offset;
            Amplitude.Value = amplitude;
            Frequency.Value = frequency;
        }

        /// <summary>
        /// Sets up the waveform.
        /// </summary>
        /// <exception cref="SpiceSharp.CircuitException">Invalid frequency {0}".FormatString(Frequency.Value)</exception>
        public override void Setup()
        {
            // Cache parameter values
            _vo = Offset;
            _va = Amplitude;
            _freq = Frequency * 2.0 * Math.PI;
            _td = Delay;
            _theta = Theta;

            // Some checks
            if (_freq < 0)
                throw new CircuitException("Invalid frequency {0}".FormatString(Frequency.Value));
        }

        /// <summary>
        /// Calculates the value of the waveform at a specific timepoint.
        /// </summary>
        /// <param name="time">The time point.</param>
        /// <returns>
        /// The value of the waveform.
        /// </returns>
        public override double At(double time)
        {
            time -= _td;

            // Calculate sine wave result (no offset)
            double result;
            if (time <= 0.0)
                result = 0.0;
            else
                result = _va * Math.Sin(_freq * time);

            // Modify with theta
            if (Theta.Given)
                result *= Math.Exp(-time * _theta);

            // Return result (with offset)
            return _vo + result;
        }

        /// <summary>
        /// Accepts the current timepoint.
        /// </summary>
        /// <param name="simulation">The time-based simulation</param>
        public override void Accept(TimeSimulation simulation)
        {
            // Do nothing
        }
    }
}
