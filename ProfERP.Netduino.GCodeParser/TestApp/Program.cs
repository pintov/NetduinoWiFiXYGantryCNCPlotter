using System;
using Microsoft.SPOT;
using ProfERP.Netduino.GCodeParser;

namespace TestApp
{
    public class Program
    {
        public static void Main()
        {
            var device = new XYZGantryDevice();
            device.SetStepX((steps) => { });
            device.SetStepY((steps) => { });
            device.SetStepZ((steps) => { });
            device.SetStopX(() => { });
            device.SetStopX(() => { });
            device.SetStopX(() => { });

            var machine = new GCodeMachine(device);

            var parser = new GCodeParser(machine);

            //parser.ParseLine("G01 X1 Y1");

            parser.ParseLine("G02 X1 Y0 I1 J0");

        }
    }
}
