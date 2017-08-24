﻿using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SpiceSharp;
using SpiceSharp.Components;
using SpiceSharp.Simulations;

namespace SpiceSharpTest.Models.Transistors
{
    [TestClass]
    public class SpiceSharpMOS1Test
    {
        /**
         * Note to self:
         * SmartSpice uses extended models, or propriety models for mosfet parasitic capacitances for LEVEL=1,2,3. If they are not specified by the model,
         * use CAPMOD=1 to use the legacy parasitic capacitance calculations!
         **/

        /// <summary>
        /// Get the test model
        /// </summary>
        private MOS1Model TestModel
        {
            get
            {
                /* Model part of the ntd20n06 (ONSemi)
                 * M1 9 7 8 8 MM L=100u W=100u
                 * .MODEL MM NMOS LEVEL=1 IS=1e-32
                 * +VTO=3.03646 LAMBDA=0 KP=5.28747
                 * +CGSO=6.5761e-06 CGDO=1e-11 */
                MOS1Model model = new MOS1Model("ntd20n06");
                model.Set("is", 1e-32);
                model.Set("vto", 3.03646);
                model.Set("lambda", 0);
                model.Set("kp", 5.28747);
                model.Set("cgso", 6.5761e-06);
                model.Set("cgdo", 1e-11);
                return model;
            }
        }

        [TestMethod]
        public void TestMOS1_DC()
        {
            // Simulation results from SmartSpice
            // GMIN = 1e-12
            // vds,vgs: 0->5V in 0.5V steps
            double[] reference = new double[]
            {
                0.000000000000000e+000, 1.000000000000000e-012, 2.000000000000000e-012, 3.000000000000000e-012, 4.000000000000000e-012, 5.000000000000000e-012, 6.000000000000000e-012, 7.000000000000000e-012, 8.000000000000000e-012, 9.000000000000000e-012, 9.999999999999999e-012,
                0.000000000000000e+000, 1.000000000000000e-012, 2.000000000000000e-012, 3.000000000000000e-012, 4.000000000000000e-012, 5.000000000000000e-012, 6.000000000000000e-012, 7.000000000000000e-012, 8.000000000000000e-012, 9.000000000000000e-012, 9.999999999999999e-012,
                0.000000000000000e+000, 1.000000000000000e-012, 2.000000000000000e-012, 3.000000000000000e-012, 4.000000000000000e-012, 5.000000000000000e-012, 6.000000000000000e-012, 7.000000000000000e-012, 8.000000000000000e-012, 9.000000000000000e-012, 9.999999999999999e-012,
                0.000000000000000e+000, 1.000000000000000e-012, 2.000000000000000e-012, 3.000000000000000e-012, 4.000000000000000e-012, 5.000000000000000e-012, 6.000000000000000e-012, 7.000000000000000e-012, 8.000000000000000e-012, 9.000000000000000e-012, 9.999999999999999e-012,
                0.000000000000000e+000, 1.000000000000000e-012, 2.000000000000000e-012, 3.000000000000000e-012, 4.000000000000000e-012, 5.000000000000000e-012, 6.000000000000000e-012, 7.000000000000000e-012, 8.000000000000000e-012, 9.000000000000000e-012, 9.999999999999999e-012,
                0.000000000000000e+000, 1.000000000000000e-012, 2.000000000000000e-012, 3.000000000000000e-012, 4.000000000000000e-012, 5.000000000000000e-012, 6.000000000000000e-012, 7.000000000000000e-012, 8.000000000000000e-012, 9.000000000000000e-012, 9.999999999999999e-012,
                0.000000000000000e+000, 1.000000000000000e-012, 2.000000000000000e-012, 3.000000000000000e-012, 4.000000000000000e-012, 5.000000000000000e-012, 6.000000000000000e-012, 7.000000000000000e-012, 8.000000000000000e-012, 9.000000000000000e-012, 9.999999999999999e-012,
                0.000000000000000e+000, 5.680575723785252e-001, 5.680575723795250e-001, 5.680575723805251e-001, 5.680575723815250e-001, 5.680575723825251e-001, 5.680575723835252e-001, 5.680575723845250e-001, 5.680575723855251e-001, 5.680575723865250e-001, 5.680575723875251e-001,
                0.000000000000000e+000, 1.886410671900999e+000, 2.454468244279524e+000, 2.454468244280524e+000, 2.454468244281524e+000, 2.454468244282523e+000, 2.454468244283524e+000, 2.454468244284524e+000, 2.454468244285524e+000, 2.454468244286524e+000, 2.454468244287524e+000,
                0.000000000000000e+000, 3.208278171900999e+000, 5.094688843801998e+000, 5.662746416180523e+000, 5.662746416181523e+000, 5.662746416182523e+000, 5.662746416183524e+000, 5.662746416184524e+000, 5.662746416185524e+000, 5.662746416186524e+000, 5.662746416187522e+000,
                0.000000000000000e+000, 4.530145671900999e+000, 7.738423843801998e+000, 9.624834515702997e+000, 1.019289208808152e+001, 1.019289208808252e+001, 1.019289208808352e+001, 1.019289208808452e+001, 1.019289208808552e+001, 1.019289208808652e+001, 1.019289208808752e+001
            };

            // Build the circuit
            Circuit ckt = new Circuit();

            MOS1 m = new MOS1("M1");
            m.SetModel(TestModel);
            m.Set("w", 100e-6);
            m.Set("l", 100e-6);
            m.Connect("D", "G", "GND", "GND");
            ckt.Objects.Add(
                new Voltagesource("V1", "G", "GND", 0.0),
                new Voltagesource("V2", "D", "GND", 0.0),
                new Resistor("Rgmin", "D", "GND", 1e12), // To match SmartSpice
                m);

            // Simulate the circuit
            DC dc = new DC("TestMOS1_DC");
            dc.Sweeps.Add(new DC.Sweep("V1", 0.0, 5.0, 0.5));
            dc.Sweeps.Add(new DC.Sweep("V2", 0.0, 5.0, 0.5));
            int index = 0;
            dc.OnExportSimulationData += (object sender, SimulationData data) =>
            {
                double vgs = dc.Sweeps[0].CurrentValue;
                double vds = dc.Sweeps[1].CurrentValue;

                double expected = reference[index];
                double actual = -data.Ask("V2", "i");

                double tol = Math.Max(Math.Abs(expected), Math.Abs(actual)) * 1e-6 + 1e-12;

                Assert.AreEqual(expected, actual, tol);
                index = index + 1;
            };
            ckt.Simulate(dc);
        }

        [TestMethod]
        public void TestMOS1_DC_Default()
        {
            // Simulation data by LTSpiceXVII
            // Please note that LTSpice uses a different models for diodes in all models, including MOS1,
            // so our tolerance will be a little bit more relaxed
            // GMIN = 1e-12
            // vds,vgs: 0->5V in 0.5V steps
            double[] reference = new double[]
            {
                0.000000e+000, -1.010000e-012, -2.010000e-012, -3.010000e-012, -4.010000e-012, -5.010000e-012, -6.010000e-012, -7.010000e-012, -8.010000e-012, -9.010000e-012, -1.001000e-011,
                0.000000e+000, -2.500001e-006, -2.500002e-006, -2.500003e-006, -2.500004e-006, -2.500005e-006, -2.500006e-006, -2.500007e-006, -2.500008e-006, -2.500009e-006, -2.500010e-006,
                0.000000e+000, -7.500001e-006, -1.000000e-005, -1.000000e-005, -1.000000e-005, -1.000001e-005, -1.000001e-005, -1.000001e-005, -1.000001e-005, -1.000001e-005, -1.000001e-005,
                0.000000e+000, -1.250000e-005, -2.000000e-005, -2.250000e-005, -2.250000e-005, -2.250001e-005, -2.250001e-005, -2.250001e-005, -2.250001e-005, -2.250001e-005, -2.250001e-005,
                0.000000e+000, -1.750000e-005, -3.000000e-005, -3.750000e-005, -4.000000e-005, -4.000001e-005, -4.000001e-005, -4.000001e-005, -4.000001e-005, -4.000001e-005, -4.000001e-005,
                0.000000e+000, -2.250000e-005, -4.000000e-005, -5.250000e-005, -6.000001e-005, -6.250000e-005, -6.250000e-005, -6.250001e-005, -6.250001e-005, -6.250001e-005, -6.250001e-005,
                0.000000e+000, -2.750000e-005, -5.000000e-005, -6.750000e-005, -8.000001e-005, -8.750000e-005, -9.000001e-005, -9.000001e-005, -9.000001e-005, -9.000001e-005, -9.000001e-005,
                0.000000e+000, -3.250000e-005, -6.000000e-005, -8.250000e-005, -1.000000e-004, -1.125000e-004, -1.200000e-004, -1.225000e-004, -1.225000e-004, -1.225000e-004, -1.225000e-004,
                0.000000e+000, -3.750000e-005, -7.000000e-005, -9.750000e-005, -1.200000e-004, -1.375000e-004, -1.500000e-004, -1.575000e-004, -1.600000e-004, -1.600000e-004, -1.600000e-004,
                0.000000e+000, -4.250000e-005, -8.000001e-005, -1.125000e-004, -1.400000e-004, -1.625000e-004, -1.800000e-004, -1.925000e-004, -2.000000e-004, -2.025000e-004, -2.025000e-004,
                0.000000e+000, -4.750000e-005, -9.000000e-005, -1.275000e-004, -1.600000e-004, -1.875000e-004, -2.100000e-004, -2.275000e-004, -2.400000e-004, -2.475000e-004, -2.500000e-004
            };

            // Build the circuit
            Circuit ckt = new Circuit();
            MOS1 m = new MOS1("M1");
            m.SetModel(new MOS1Model("DefaultModel"));
            m.Connect("D", "G", "0", "0");
            ckt.Objects.Add(
                new Voltagesource("V1", "G", "0", 0.0),
                new Voltagesource("V2", "D", "0", 0.0),
                m);

            // Build the simulation
            DC dc = new DC("TestMOS1_DC_Default");
            dc.Sweeps.Add(new DC.Sweep("V1", 0.0, 5.0, 0.5));
            dc.Sweeps.Add(new DC.Sweep("V2", 0.0, 5.0, 0.5));
            int index = 0;
            dc.OnExportSimulationData += (object sender, SimulationData data) =>
            {
                double vgs = dc.Sweeps[0].CurrentValue;
                double vds = dc.Sweeps[1].CurrentValue;
                double expected = reference[index];
                double actual = data.Ask("V2", "i");
                double tol = Math.Max(Math.Abs(expected), Math.Abs(actual)) * 1e-6 + 1e-10; // Absolute tolerance weaker because BS/BD diode model is different
                Assert.AreEqual(actual, expected, tol);
                index++;
            };
            ckt.Simulate(dc);
        }

        [TestMethod]
        public void TestMOS1_AC()
        {
            // Simulation from SmartSpice
            double[] reference_db = new double[]
            {
                4.416233896274324e+001, 4.416233896248351e+001, 4.416233896183108e+001, 4.416233896019226e+001, 4.416233895607573e+001,
                4.416233894573548e+001, 4.416233891976192e+001, 4.416233885451931e+001, 4.416233869063727e+001, 4.416233827898424e+001,
                4.416233724495874e+001, 4.416233464760518e+001, 4.416232812335490e+001, 4.416231173522237e+001, 4.416227057036741e+001,
                4.416216717064771e+001, 4.416190745315265e+001, 4.416125514079855e+001, 4.415961703826970e+001, 4.415550503395419e+001,
                4.414519328601954e+001, 4.411939882594997e+001, 4.405527385764408e+001, 4.389825555042088e+001, 4.352720062742411e+001,
                4.271480983675309e+001, 4.116587743874943e+001, 3.872674389043524e+001, 3.554733682171911e+001, 3.192268608819617e+001,
                2.808163643564533e+001, 2.414657100678447e+001, 2.017269458249036e+001, 1.618313850727109e+001, 1.218730344081383e+001,
                8.188962975894842e+000
            };
            double[] reference_ph = new double[]
            {
                1.799996396377713e+002, 1.799994288643570e+002, 1.799990948110075e+002, 1.799985653721280e+002, 1.799977262680527e+002,
                1.799963963777181e+002, 1.799942886435886e+002, 1.799909481101492e+002, 1.799856537215769e+002, 1.799772626817085e+002,
                1.799639637818850e+002, 1.799428864546132e+002, 1.799094811760466e+002, 1.798565375125780e+002, 1.797726279986953e+002,
                1.796396425228570e+002, 1.794288832724440e+002, 1.790948863044717e+002, 1.785656718229811e+002, 1.777274604821202e+002,
                1.764011181038848e+002, 1.743074481922301e+002, 1.710223103863751e+002, 1.659427331356018e+002, 1.583547689940415e+002,
                1.478320663378927e+002, 1.350910765273782e+002, 1.223321978134144e+002, 1.117698800929852e+002, 1.041421840128158e+002,
                9.903194674660426e+001, 9.572519309762384e+001, 9.361624354771799e+001, 9.227818666377719e+001, 9.142943351372914e+001,
                9.088860904886528e+001
            };

            // Make the circuit
            Circuit ckt = new Circuit();

            MOS1 m = new MOS1("M1");
            m.SetModel(TestModel);
            m.Set("w", 100e-6);
            m.Set("l", 100e-6);
            m.Connect("OUT", "IN", "GND", "GND");
            Voltagesource vsrc;
            ckt.Objects.Add(
                vsrc = new Voltagesource("V1", "IN", "GND", 3.067),
                new Voltagesource("V2", "VDD", "GND", 5.0),
                new Resistor("R1", "VDD", "OUT", 1e3),
                new Resistor("Rgmin", "OUT", "GND", 1e12), // To match SmartSpice
                new Capacitor("C1", "OUT", "GND", 1e-12),
                m);
            vsrc.Set("acmag", 1.0);

            // Make the simulation
            AC ac = new AC("TestMOS1_AC");
            ac.StartFreq = 1e3;
            ac.StopFreq = 10e9;
            ac.NumberSteps = 5;
            ac.StepType = AC.StepTypes.Decade;
            int index = 0;
            ac.OnExportSimulationData += (object sender, SimulationData data) =>
            {
                double frequency = data.GetFrequency();

                double expected = reference_db[index];
                double actual = data.GetDb("OUT");
                double tol = Math.Max(Math.Abs(expected), Math.Abs(actual)) * 1e-6 + 1e-12;
                Assert.AreEqual(expected, actual, tol);

                expected = reference_ph[index];
                actual = data.GetPhase("OUT");
                tol = Math.Max(Math.Abs(expected), Math.Abs(actual)) * 1e-6 + 1e-12;
                Assert.AreEqual(expected, actual, tol);

                index++;
            };
            ckt.Simulate(ac);
        }
    }
}
