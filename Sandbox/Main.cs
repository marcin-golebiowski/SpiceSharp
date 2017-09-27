﻿using System;
using System.IO;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;
using SpiceSharp;
using SpiceSharp.Components;
using SpiceSharp.Simulations;
using SpiceSharp.Parameters;
using SpiceSharp.Parser.Readers;
using SpiceSharp.Designer;
using SpiceSharp.Diagnostics;
using MathNet.Numerics.Interpolation;

namespace Sandbox
{
    public partial class Main : Form
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public Main()
        {
            InitializeComponent();

            double[] reft = new double[]
            {
                0.000000000000000e+00, 1.000000000000000e-11, 2.000000000000000e-11, 4.000000000000000e-11, 8.000000000000001e-11, 1.600000000000000e-10, 3.200000000000000e-10, 6.400000000000001e-10, 1.280000000000000e-09, 2.560000000000000e-09, 5.120000000000001e-09, 1.024000000000000e-08, 2.048000000000000e-08, 4.096000000000000e-08, 8.192000000000001e-08, 1.638400000000000e-07, 3.276800000000000e-07, 5.276800000000000e-07, 7.276800000000000e-07, 9.276800000000000e-07, 1.000000000000000e-06, 1.000100000000000e-06, 1.000300000000000e-06, 1.000700000000000e-06, 1.001000000000000e-06, 1.001080000000000e-06, 1.001240000000000e-06, 1.001560000000000e-06, 1.002200000000000e-06, 1.003480000000000e-06, 1.006040000000000e-06, 1.011160000000001e-06, 1.021400000000002e-06, 1.041880000000003e-06, 1.082840000000006e-06, 1.164760000000012e-06, 1.328600000000025e-06, 1.528600000000025e-06, 1.728600000000025e-06, 1.928600000000025e-06, 2.128600000000025e-06, 2.328600000000025e-06, 2.528600000000024e-06, 2.728600000000024e-06, 2.928600000000024e-06, 3.001000000000000e-06, 3.021000000000000e-06, 3.061000000000000e-06, 3.141000000000000e-06, 3.301000000000000e-06, 3.501000000000000e-06, 3.520999999999999e-06, 3.560999999999999e-06, 3.641000000000000e-06, 3.801000000000000e-06, 4.000999999999999e-06, 4.200999999999999e-06, 4.400999999999999e-06, 4.600999999999999e-06, 4.800999999999999e-06, 5.000999999999998e-06, 5.200999999999998e-06, 5.400999999999998e-06, 5.600999999999998e-06, 5.800999999999997e-06, 6.000999999999997e-06, 6.200999999999997e-06, 6.400999999999997e-06, 6.600999999999997e-06, 6.800999999999996e-06, 7.000000000000000e-06, 7.000100000000000e-06, 7.000300000000000e-06, 7.000700000000000e-06, 7.001000000000000e-06, 7.001079999999999e-06, 7.001239999999999e-06, 7.001559999999999e-06, 7.002199999999999e-06, 7.003479999999999e-06, 7.006039999999998e-06, 7.011159999999996e-06, 7.021399999999993e-06, 7.041879999999985e-06, 7.082839999999971e-06, 7.164759999999943e-06, 7.328599999999885e-06, 7.528599999999885e-06, 7.728599999999886e-06, 7.928599999999885e-06, 8.128599999999885e-06, 8.328599999999885e-06, 8.528599999999885e-06, 8.728599999999885e-06, 8.928599999999884e-06, 9.000999999999999e-06, 9.020999999999999e-06, 9.060999999999998e-06, 9.140999999999998e-06, 9.300999999999998e-06, 9.500999999999998e-06, 9.520999999999997e-06, 9.560999999999997e-06, 9.640999999999996e-06, 9.800999999999996e-06, 9.999999999999999e-06
            };
            double[] refv = new double[]
            {
                1.799999451299451e+00, 1.799999453635131e+00, 1.799999450956929e+00, 1.799999450956926e+00, 1.799999450956929e+00, 1.799999450956926e+00, 1.799999450956929e+00, 1.799999450956926e+00, 1.799999450956929e+00, 1.799999450956926e+00, 1.799999450956929e+00, 1.799999450956926e+00, 1.799999450956929e+00, 1.799999450956926e+00, 1.799999450956929e+00, 1.799999450956926e+00, 1.799999450956929e+00, 1.799999450956926e+00, 1.799999450956929e+00, 1.799999450956926e+00, 1.799999450956929e+00, 1.799999389971110e+00, 1.799999183568271e+00, 7.448095922434256e-02, -7.448046003580762e-02, 2.495942674587899e-07, 2.495942674587899e-07, 2.495942674587899e-07, 2.495942674587899e-07, 2.495942674587899e-07, 2.495942674587899e-07, 2.495942674587899e-07, 2.495942674587899e-07, 2.495942674587899e-07, 2.495942674587899e-07, 2.495942674587899e-07, 2.495942674587899e-07, 2.495942674587899e-07, 2.495942674587899e-07, 2.495942674587899e-07, 2.495942674587899e-07, 2.495942674587899e-07, 2.495942674587899e-07, 2.495942674587899e-07, 2.495942674587899e-07, 2.495942674587899e-07, 2.495942674587899e-07, 2.495942674587899e-07, 2.495942674587899e-07, 1.799998636447238e+00, 1.800000994575628e+00, 1.799999453635142e+00, 1.799999453635142e+00, 1.799999450956928e+00, 1.799999450956927e+00, 1.799999450956928e+00, 1.799999450956927e+00, 1.799999450956928e+00, 1.799999450956927e+00, 1.799999450956928e+00, 1.799999450956927e+00, 1.799999450956928e+00, 1.799999450956927e+00, 1.799999450956928e+00, 1.799999450956927e+00, 1.799999450956928e+00, 1.799999450956927e+00, 1.799999450956928e+00, 1.799999450956927e+00, 1.799999450956928e+00, 1.799999450956927e+00, 1.799999389971109e+00, 1.799999183568270e+00, 7.448095922456173e-02, -7.448046003602679e-02, 2.495942674587899e-07, 2.495942674587899e-07, 2.495942674587899e-07, 2.495942674587899e-07, 2.495942674587899e-07, 2.495942674587899e-07, 2.495942674587899e-07, 2.495942674587899e-07, 2.495942674587899e-07, 2.495942674587899e-07, 2.495942674587899e-07, 2.495942674587899e-07, 2.495942674587899e-07, 2.495942674587899e-07, 2.495942674587899e-07, 2.495942674587899e-07, 2.495942674587899e-07, 2.495942674587899e-07, 2.495942674587899e-07, 2.495942674587899e-07, 2.495942674587899e-07, 2.495942674587899e-07, 2.495942674587899e-07, 2.495942674587899e-07, 1.799998636447239e+00, 1.800000994575627e+00, 1.799999453635142e+00, 1.799999453635142e+00, 1.799999450956928e+00, 1.799999450956927e+00, 1.799999450956928e+00
            };

            // Parse the netlist
            NetlistReader nr = new NetlistReader();
            string netlist = string.Join(Environment.NewLine,
                "M1 out in vdd vdd DMOS L = 1u W = 1u",
                ".MODEL DMOS pmos(LEVEL = 3 VTO = -0.7 KP = 3.8E+1 THETA = .25 VMAX = 3.5E5)",
                "V1 in 0 PULSE(0 1.8 1u 1n 0.5u 2u 6u)",
                "Vsupply vdd 0 1.8",
                "R1 out 0 100k",
                ".SAVE V(out)",
                ".tran 1n 10u"
            );
            nr.Parse(new MemoryStream(Encoding.UTF8.GetBytes(netlist)));

            // Execute the simulation
            Series output = chMain.Series.Add("Output");
            output.ChartType = SeriesChartType.FastPoint;
            Series chref = chMain.Series.Add("Spice 3f5");
            chref.ChartType = SeriesChartType.FastPoint;

            int index = 0;
            nr.Netlist.OnExportSimulationData += (object sender, SimulationData data) =>
            {
                output.Points.AddXY(data.GetTime(), nr.Netlist.Exports[0].Extract(data));
                chref.Points.AddXY(reft[index], refv[index]);
                index++;
            };
            nr.Netlist.Simulate();

            chMain.ChartAreas[0].AxisX.RoundAxisValues();
        }
    }
}
