
namespace ProfERP.Netduino.AdafruitMotorShield
{
    public enum PinState
    {
        LOW = 0,
        HIGH = 1,
    }

    public enum Pin
    {
        M1pwr = (byte)8,
        M1in1 = (byte)10,
        M1in2 = (byte)9,

        M2pwr = (byte)13,
        M2in1 = (byte)11,
        M2in2 = (byte)12,

        M3pwr = (byte)2,
        M3in1 = (byte)4,
        M3in2 = (byte)3,

        M4pwr = (byte)7,
        M4in1 = (byte)5,
        M4in2 = (byte)6,
    }

    public enum Command
    {
        FORWARD = 1,
        BACKWARD = 2,
        BRAKE = 3,
        RELEASE = 4,           
    }

    public enum Direction
    {
        FORWARD = (sbyte)1,
        BACKWARD = (sbyte)-1,
    }

    public enum OperationMode
    {
        FullStep,
        HalfStep,
        MicroStep,
    }

}
