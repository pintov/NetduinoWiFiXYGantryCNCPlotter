
using System.Threading;

namespace ProfERP.Netduino.AdafruitMotorShield
{
    public class AdafruitStepperMotor
    {
        private AdafruitMotorShield _ms;
        private AdafruitMotorHBridge phaseA;
        private AdafruitMotorHBridge phaseB;
        private sbyte phaseIndex;

        private double[] DutyCycleA;
        private double[] DutyCycleB;
        private sbyte maxIndex;

        public AdafruitStepperMotor(byte num, AdafruitMotorShield ms)
        {
            _ms = ms;

            if (num == 0)
            {
                phaseA = new AdafruitMotorHBridge(_ms, Pin.M1pwr, Pin.M1in1, Pin.M1in2);
                phaseB = new AdafruitMotorHBridge(_ms, Pin.M2pwr, Pin.M2in1, Pin.M2in2);
            }
            else if (num == 1)
            {
                phaseA = new AdafruitMotorHBridge(_ms, Pin.M3pwr, Pin.M3in1, Pin.M3in2);
                phaseB = new AdafruitMotorHBridge(_ms, Pin.M4pwr, Pin.M4in1, Pin.M4in2);
            }

            SetOperationMode(OperationMode.FullStep);
        }

        public void SetOperationMode(OperationMode operationMode)
        {
            switch (operationMode)
            {
                case OperationMode.FullStep:
                    DutyCycleA = new[] { +1.0, -1.0, -1.0, +1.0 };
                    DutyCycleB = new[] { +1.0, +1.0, -1.0, -1.0 };
                    break;
                case OperationMode.HalfStep:
                    DutyCycleA = new[] { +1.0, +1.0, +0.0, -1.0, -1.0, -1.0, +0.0, +1.0 };
                    DutyCycleB = new[] { +0.0, +1.0, +1.0, +1.0, +0.0, -1.0, -1.0, -1.0 };
                    break;
                case OperationMode.MicroStep:
                    DutyCycleA = new[] { +1.0, +1.0, +0.5, +0.5, +0.0, -0.5, -0.5, -1.0, -1.0, -1.0, -0.5, -0.5, +0.0, +0.5, +0.5, +1.0 };
                    DutyCycleB = new[] { +0.0, +0.5, +0.5, +1.0, +1.0, +1.0, +0.5, +0.5, +0.0, -0.5, -0.5, -1.0, -1.0, -1.0, -0.5, -0.5 };
                    break;
            }
            maxIndex = (sbyte)(DutyCycleA.Length - 1);
            phaseIndex = -1;
            PerformSteps(1, Direction.FORWARD);
            PerformSteps(1, Direction.BACKWARD);
        }

        public void PerformSteps(ushort steps, Direction direction)
        {
            while (steps-- > 0)
            {
                PerformStep(direction);
                Thread.Sleep(1);
            }
            ReleaseHoldingTorque();
        }

        public void PerformStep(Direction direction)
        {
            phaseIndex += (sbyte)direction;
            
            if (phaseIndex > maxIndex)
                phaseIndex = 0;

            if (phaseIndex < 0)
                phaseIndex = maxIndex;

            phaseA.SetOutputPowerAndPolarity(DutyCycleA[phaseIndex]);
            phaseB.SetOutputPowerAndPolarity(DutyCycleB[phaseIndex]);
        }

        public void ReleaseHoldingTorque()
        {
            phaseA.ReleaseHoldingTorque();
            phaseB.ReleaseHoldingTorque();
        }
    }
}
