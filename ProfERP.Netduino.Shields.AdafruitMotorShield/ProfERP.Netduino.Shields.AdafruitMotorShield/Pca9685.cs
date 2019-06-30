using System;
using Netduino.Foundation.Communications;
using System.Threading;

namespace ProfERP.Netduino.AdafruitMotorShield
{
    internal class Pca9685
    {
        public const byte AllCallBit = 0;
        public const byte Channel0OnLow = 0x06;
        public const byte PrescaleRegister = 0xFE;
        public const int ClockRateKhz = 100;
    }

    class Pca9685PwmController
    {
        private const byte PCA9685_MODE1 = Pca9685.AllCallBit;
        private const byte PCA9685_PRESCALE = Pca9685.PrescaleRegister;

        private byte _i2caddr;
        private I2CBus i2c;

        public Pca9685PwmController(byte addr = 0x40)
        {
            _i2caddr = addr;
            i2c = new I2CBus(_i2caddr, Pca9685.ClockRateKhz);
            reset();
        }

        public void reset()
        {
            write8(PCA9685_MODE1, 0x0);
        }

        public void setPWMFreq(double freq)
        {
            // See PCA9685 data sheet, pp.24 for details on calculating the prescale value.

            freq *= 0.9;  // Correct for overshoot in the frequency setting (see issue #11).
            double prescaleval = 25000000;
            prescaleval /= 4096;
            prescaleval /= System.Math.Round(freq);
            prescaleval -= 1;

            if (prescaleval < 3.0 || prescaleval > 255.0)
                throw new ArgumentOutOfRangeException("frequencyHz", "range 24 Hz to 1743 Hz");

            byte prescale = (byte)(prescaleval + 0.5);

            byte oldmode = read8(PCA9685_MODE1);
            byte newmode = (byte)(oldmode & 0x7F);
            newmode = (byte)(newmode | 0x10); // sleep
            write8(PCA9685_MODE1, newmode); // go to sleep
            write8(PCA9685_PRESCALE, prescale); // set the prescaler
            write8(PCA9685_MODE1, oldmode);
            Thread.Sleep(5);
            write8(PCA9685_MODE1, (byte)(oldmode | 0xa1));  
        }

        public void SetRegisterValues(Pin pin, ushort on, ushort off)
        {
            byte[] data = new byte[5];
            data[0] = (byte)(Pca9685.Channel0OnLow + 4 * (byte)pin);
            data[1] = (byte)on;
            data[2] = (byte)(on >> 8);
            data[3] = (byte)(off);
            data[4] = (byte)(off >> 8);
            i2c.WriteBytes(data);
        }


        private void write8(byte addr, byte d)
        {
            i2c.WriteRegister(addr, d);
        }

        private byte read8(byte addr)
        {
            return i2c.ReadRegister(addr);
        }


    }
}


//# ifndef _Adafruit_MS_PWMServoDriver_H
//#define _Adafruit_MS_PWMServoDriver_H

//#if ARDUINO >= 100
//# include "Arduino.h"
//#else
//# include "WProgram.h"
//#endif


//#define PCA9685_SUBADR1 0x2
//#define PCA9685_SUBADR2 0x3
//#define PCA9685_SUBADR3 0x4




//#define LED0_ON_H 0x7
//#define LED0_OFF_L 0x8
//#define LED0_OFF_H 0x9

//#define ALLLED_ON_L 0xFA
//#define ALLLED_ON_H 0xFB
//#define ALLLED_OFF_L 0xFC
//#define ALLLED_OFF_H 0xFD


//class Pca9685PwmController
//{
//    public:
//  Pca9685PwmController(byte addr = 0x40);
//    void begin(void);
//    void reset(void);
//    void setPWMFreq(float freq);
//    void SetRegisterValues(byte pin, ushort on, ushort off);

//    private:
//    byte _i2caddr;
//    byte read8(byte addr);
//    void write8(byte addr, byte d);
//};

//#endif

//# include <Pca9685PwmController.h>
//# include <Wire.h>
//#if defined(ARDUINO_SAM_DUE)
//#define i2c Wire1
//#else
//#define i2c Wire
//#endif


