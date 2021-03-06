﻿using System;
using System.Globalization;
using NUnit.Framework;
using SpiceSharp;
using SpiceSharp.Components;
using SpiceSharp.IntegrationMethods;
using SpiceSharp.Simulations;

namespace SpiceSharpTest.Simulations
{
    [TestFixture]
    public class TransientTests
    {
        [Test]
        public void When_RCFilterConstantTransient_Expect_Reference()
        {
            // Create the circuit
            var ckt = new Circuit(
                new VoltageSource("V1", "in", "0", 10.0),
                new Resistor("R1", "in", "out", 10),
                new Capacitor("C1", "out", "0", 20)
            );

            // Create the transient analysis
            var tran = new Transient("tran 1", 1.0, 10.0);
            tran.Configurations.Get<TimeConfiguration>().InitTime = 0.0;
            tran.Configurations.Get<TimeConfiguration>().Method = new Gear();
            tran.ExportSimulationData += (sender, args) =>
            {
                Assert.AreEqual(10.0, args.GetVoltage("out"), 1e-9);
            };
            tran.Run(ckt);

            // Let's run the simulation twice to check if it is consistent
            try
            {
                tran.Run(ckt);
            }
            catch (Exception)
            {
                throw new Exception(@"Cannot run transient analysis twice");
            }
        }

        [Test]
        public void When_RCFilterConstantTransientGear_Expect_Reference()
        {
            // Create the circuit
            var ckt = new Circuit(
                new VoltageSource("V1", "in", "0", 10.0),
                new Resistor("R1", "in", "out", 10),
                new Capacitor("C1", "out", "0", 20));

            // Create the transient analysis
            var tran = new Transient("tran 1", 1.0, 10.0);
            tran.ExportSimulationData += (sender, args) => { Assert.AreEqual(args.GetVoltage("out"), 10.0, 1e-12); };
            tran.Run(ckt);

            // Let's run the simulation twice to check if it is consistent
            try
            {
                tran.Run(ckt);
            }
            catch (Exception)
            {
                throw new Exception(@"Cannot run transient analysis twice");
            }
        }

        [Test]
        public void When_FloatingRTransient_Expect_Reference()
        {
            // Create the circuit
            var ckt = new Circuit(
                new VoltageSource("V1", "in", "0", 10.0),
                new Resistor("R1", "in", "out", 10.0)
            );

            // Create the transient analysis
            var tran = new Transient("Tran 1", 1e-6, 10.0);
            tran.ExportSimulationData += (sender, args) =>
            {
                Assert.AreEqual(args.GetVoltage("out"), 10.0, 1e-12);
            };
            tran.Run(ckt);
        }

        [Test]
        public void When_LazyLoadExport_Expect_Reference()
        {
            // Create the circuit
            var ckt = new Circuit(
                new VoltageSource("V1", "in", "0", 10),
                new Resistor("R1", "in", "0", 1e3));

            // Create the transient analysis
            var tran = new Transient("Tran 1", 1e-6, 10.0);
            Export<double> export = null;
            tran.ExportSimulationData += (sender, args) =>
            {
                // If the time > 5.0 then start exporting our stuff
                if (args.Time > 5.0)
                {
                    if (export == null)
                        export = new RealPropertyExport((Simulation) sender, "R1", "i");
                    Assert.AreEqual(10.0 / 1e3, export.Value, 1e-12);
                }
            };
            tran.Run(ckt);
        }

        [Test]
        public void When_FixedEuler_Expect_NoException()
        {
            // Create a circuit with a nonlinear component
            var ckt = new Circuit(
                new VoltageSource("V1", "in", "0", new Pulse(0, 5, 1e-6, 1e-6, 1e-6, 1e-5, 2e-5)),
                new NonlinearResistor("NLR1", "in", "out"),
                new Capacitor("C1", "out", "0", 1.0e-9)
                );
            ckt.Entities["NLR1"].SetParameter("a", 100.0);
            ckt.Entities["NLR1"].SetParameter("b", 0.7);

            // Create a transient analysis using Backward Euler with fixed timesteps
            var tran = new Transient("tran", 1e-7, 10e-5);
            tran.Configurations.Get<TimeConfiguration>().Method = new FixedEuler();
            tran.Run(ckt);
        }
    }
}
