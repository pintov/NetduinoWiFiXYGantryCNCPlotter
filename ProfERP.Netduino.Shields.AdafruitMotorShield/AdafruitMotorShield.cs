using System;

namespace ProfERP.Netduino.AdafruitMotorShield
{
    public class AdafruitMotorShield
    {
        private const byte LOW = 0;
        private const byte HIGH = 1;

        private Pca9685PwmController _pwm;
        private AdafruitDCMotor[] dcmotors = new AdafruitDCMotor[4];
        private AdafruitStepperMotor[] steppers = new AdafruitStepperMotor[2];

        public AdafruitMotorShield(byte addr = 0x60, double _freq = 1600)
        {
            _pwm = new Pca9685PwmController(addr);
            _pwm.setPWMFreq(_freq);
            for (byte i = 0; i < 16; i++)
                _pwm.SetRegisterValues((Pin)i, 0, 0);
        }

        public AdafruitDCMotor getMotor(byte num)
        {
            if (num < 1 | num > 4)
                throw new ArgumentOutOfRangeException("num", "range 1 to 4");

            num--;

            if (dcmotors[num] == null)
                dcmotors[num] = new AdafruitDCMotor(num, this);

            return dcmotors[num];
        }

        public void SetPin(Pin pin, PinState value)
        {
            if (value == PinState.LOW)
                _pwm.SetRegisterValues(pin, 0, 0);
            else
                _pwm.SetRegisterValues(pin, 4096, 0);
        }

        public void setPWM(Pin pin, ushort value)
        {
            if (value > 4095)
                _pwm.SetRegisterValues(pin, 4096, 0);
            else
                _pwm.SetRegisterValues(pin, 0, value);
        }

        public void SetRegisterValues(Pin pin, ushort on, ushort off)
        {
            _pwm.SetRegisterValues(pin, on, off);
        }

        public AdafruitStepperMotor GetStepper(byte num)
        {
            if (num < 1 | num > 2)
                throw new ArgumentOutOfRangeException("num", "range 1 to 2");

            num--;

            if (steppers[num] == null)
                steppers[num] = new AdafruitStepperMotor(num, this);

            return steppers[num];
        }

        public void ReleaseHoldingTorqueAllSteppers()
        {
            foreach(var st in steppers)
            {
                if (st == null)
                    continue;
                st.ReleaseHoldingTorque();
            }
        }

    }
}