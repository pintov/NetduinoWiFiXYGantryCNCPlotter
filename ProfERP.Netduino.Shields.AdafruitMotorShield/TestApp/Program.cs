using System;
using Microsoft.SPOT;
using ProfERP.Netduino.AdafruitMotorShield;

namespace TestApp
{
    public class Program
    {
        public static void Main()
        {
            Debug.Print("test app run");

            var sh = new AdafruitMotorShield();
            var st1 = sh.GetStepper(1);
            st1.PerformSteps(1000, Direction.FORWARD);
            st1.PerformSteps(1000, Direction.BACKWARD);
            st1.PerformSteps(1000, Direction.FORWARD);
            st1.PerformSteps(1000, Direction.BACKWARD);
            st1.PerformSteps(1000, Direction.FORWARD);
            st1.PerformSteps(1000, Direction.BACKWARD);
            //var st2 = sh.GetStepper(2);
            //st2.PerformSteps(100, Direction.FORWARD);


        }
    }
}
