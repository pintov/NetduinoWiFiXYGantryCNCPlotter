
namespace ProfERP.Netduino.AdafruitMotorShield
{
    public class AdafruitDCMotor
    {
        private AdafruitMotorShield ms;
        private byte motornum;
        private Pin PWMpin;
        private Pin IN1pin;
        private Pin IN2pin;

        public AdafruitDCMotor(byte num, AdafruitMotorShield adafruit_MotorShield)
        {
            motornum = num;
            ms = adafruit_MotorShield;

            if (num == 0)
            {
                PWMpin = Pin.M1pwr;
                IN2pin = Pin.M1in2;
                IN1pin = Pin.M1in1;
            }
            else if (num == 1)
            {
                PWMpin = Pin.M2pwr;
                IN2pin = Pin.M2in2;
                IN1pin = Pin.M2in1;
            }
            else if (num == 2)
            {
                PWMpin = Pin.M3pwr;
                IN2pin = Pin.M3in2;
                IN1pin = Pin.M3in1;
            }
            else if (num == 3)
            {
                PWMpin = Pin.M4pwr;
                IN2pin = Pin.M4in2;
                IN1pin = Pin.M4in1;
            }
        }

        public void run(Command cmd)
        {
            switch (cmd)
            {
                case Command.FORWARD:
                    ms.SetPin(IN2pin, PinState.LOW);  // take low first to avoid 'break'
                    ms.SetPin(IN1pin, PinState.HIGH);
                    break;
                case Command.BACKWARD:
                    ms.SetPin(IN1pin, PinState.LOW);  // take low first to avoid 'break'
                    ms.SetPin(IN2pin, PinState.HIGH);
                    break;
                case Command.RELEASE:
                    ms.SetPin(IN1pin, PinState.LOW);
                    ms.SetPin(IN2pin, PinState.LOW);
                    break;
            }
        }

        public void setSpeed(ushort speed)
        {
            ms.setPWM(PWMpin, (ushort)(speed * 16));
        }
    }
}