using System;
using Microsoft.SPOT;
using Math = System.Math;

namespace ProfERP.Netduino.AdafruitMotorShield
{
    public class AdafruitMotorHBridge
    {
        private AdafruitMotorShield ms;
        private Pin pwr;
        private Pin in1;
        private Pin in2;

        private double magnitude;

        public AdafruitMotorHBridge(AdafruitMotorShield _ms, Pin _pwr, Pin _in1, Pin _in2)
        {
            ms = _ms;
            pwr = _pwr;
            in1 = _in1;
            in2 = _in2;
            ReleaseHoldingTorque();
        }

        public void ReleaseHoldingTorque()
        {
            ms.SetPin(pwr, PinState.LOW);
            ms.SetPin(in1, PinState.LOW);
            ms.SetPin(in2, PinState.LOW);
            magnitude = 0;
        }

        internal void SetOutputPowerAndPolarity(double duty)
        {
            if (duty > 1.0 || duty < -1.0)
                throw new ArgumentOutOfRangeException("duty", "-1.0 to 1.0 inclusive");
            
            var newPolarity = (duty >= 0.0);
            var newMagnitude = Math.Abs(duty);

            if (newPolarity)
                Forward();
            else
                Reverse();

            if (magnitude != newMagnitude)
                SetMagnitude(newMagnitude);

            magnitude = newMagnitude;
        }


        void Forward()
        {
            ms.SetPin(in2, PinState.LOW);
            ms.SetPin(in1, PinState.HIGH);
        }

        void Reverse()
        {
            ms.SetPin(in1, PinState.LOW);
            ms.SetPin(in2, PinState.HIGH);
        }

        private void SetMagnitude(double magnitude)
        {
            if (magnitude == 1.0)
            {
                ms.SetPin(pwr, PinState.HIGH);
                return;
            }
            if (magnitude == 0.0)
            {
                ms.SetPin(pwr, PinState.LOW);
                return;
            }

            ushort onCount = 0;
            var offCount = (ushort)Math.Floor(4096 * magnitude);
            if (offCount <= onCount)
                offCount = (ushort)(onCount + 1);

            ms.SetRegisterValues(pwr, onCount, offCount);
        }
    }

}

    

