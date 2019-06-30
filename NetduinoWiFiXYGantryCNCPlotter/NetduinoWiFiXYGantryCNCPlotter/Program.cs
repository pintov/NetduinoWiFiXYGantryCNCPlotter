using System;
using Microsoft.SPOT;
using System.Threading;
using Maple;
using ProfERP.Netduino.AdafruitMotorShield;
using ProfERP.Netduino.GCodeParser;
using Netduino.Foundation.Servos;
using SecretLabs.NETMF.Hardware.Netduino;

namespace NetduinoWiFiXYGantryCNCPlotter
{
    public static partial class Program
    {
        public static GCodeParser parser;

        public static Settings settings;

        public static void Main()
        {
            if (!NetworkUtils.InitializeNetwork())
                return;

            InitializeHttpJsonServer();

            InitializeGCodeParser();

            SleepAndDebugPrintAlive();
        }

        private static void InitializeGCodeParser()
        {
            settings = new Settings();
            settings.OneStepZ_Angle_Positive = 30;
            settings.OneStepZ_Angle_Negative = 15;

            var servo = new Servo(
                PWMChannels.PWM_PIN_D10, 
                new ServoConfig(0, 180, 500, 2500, 50));
            servo.RotateTo(15);
            Thread.Sleep(500);
            servo.RotateTo(35);

            var motorShield = new AdafruitMotorShield();
            IDeviceDelegateOneStep oneStepX = PrepareOneStepDelegate(motorShield, 1, OperationMode.FullStep);
            IDeviceDelegateOneStep oneStepY = PrepareOneStepDelegate(motorShield, 2, OperationMode.FullStep);
            IDeviceDelegateOneStep oneStepZ = (steps) => 
                {
                    var a = servo.Angle;

                    if (steps >= 0)
                        servo.RotateTo(settings.OneStepZ_Angle_Negative);
                    else
                        servo.RotateTo(settings.OneStepZ_Angle_Positive);

                    if (a != servo.Angle)
                        Thread.Sleep(500);
                };
            IDeviceDelegateStop startX = () => {  };
            IDeviceDelegateStop startY = () => {  };
            IDeviceDelegateStop startZ = () => {
                servo.RotateTo(settings.OneStepZ_Angle_Negative);
                Thread.Sleep(500);
                servo.RotateTo(settings.OneStepZ_Angle_Positive);
                Thread.Sleep(500);
                servo.RotateTo(settings.OneStepZ_Angle_Negative);
                Thread.Sleep(500);
                servo.RotateTo(settings.OneStepZ_Angle_Positive);
            };

            IDeviceDelegateStop stopX = () => { motorShield.GetStepper(2).ReleaseHoldingTorque(); };
            IDeviceDelegateStop stopY = () => { motorShield.GetStepper(1).ReleaseHoldingTorque(); };
            IDeviceDelegateStop stopZ = () => { servo.RotateTo(settings.OneStepZ_Angle_Negative); };

            var device = new XYZGantryDevice();
            device.SetStepX(oneStepX);
            device.SetStepY(oneStepY);
            device.SetStepZ(oneStepZ);
            device.SetStartX(startX);
            device.SetStartY(startY);
            device.SetStartZ(startZ);
            device.SetStopX(stopX);
            device.SetStopY(stopY);
            device.SetStopZ(stopZ);

            device.Calibrate(25f, 25f, 1);


            var machine = new GCodeMachine(device);
            
            parser = new GCodeParser(machine);

        }

        private static IDeviceDelegateOneStep PrepareOneStepDelegate(AdafruitMotorShield ms, byte motorNum, OperationMode operationMode)
        {
            var stepper = ms.GetStepper(motorNum);
            stepper.SetOperationMode(operationMode);
            IDeviceDelegateOneStep moveRaw = (steps) => {
                if (steps >= 0)
                    stepper.PerformStep(Direction.FORWARD);
                else
                    stepper.PerformStep(Direction.BACKWARD);
            };
            return moveRaw;
        }



        private static void InitializeHttpJsonServer()
        {
            var server = new MapleServer();
            server.Start();
        }

        private static void SleepAndDebugPrintAlive()
        {
            while (true)
            {
                Thread.Sleep(5000);
                Debug.Print("Still alive. " + DateTime.Now.ToString());
            }
        }

        public class Settings
        {
            public int OneStepZ_Angle_Positive;
            public int OneStepZ_Angle_Negative;
            public int OneStepZ_Angle_Negative_MaxY;
        }
    }
}