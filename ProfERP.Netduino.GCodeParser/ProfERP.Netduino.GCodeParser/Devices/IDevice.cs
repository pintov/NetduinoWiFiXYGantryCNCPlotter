namespace ProfERP.Netduino.GCodeParser
{
    public delegate void IDeviceDelegateOneStep(int steps);
    public delegate void IDeviceDelegateStop();

    public interface IDevice
    {
        void Calibrate(float x, float y, float z);
        string GetCalibration();
        void MoveAbsoluteLinear(float x, float y, float z);
        void MoveArc(float x, float y, float z, float i, float j, float k, float radius, bool clockwise, DistanceMode distanceMode);
        void MoveAxes(int dx, int dy, int dz = 0);
        void SetPosition(float x, float y, float z);
        void StartAllAxes();
        void StopAllAxes();
        float GetCurrentX();
        float GetCurrentY();
        float GetCurrentZ();

        void SetStepX(IDeviceDelegateOneStep oneStep);
        void SetStepY(IDeviceDelegateOneStep oneStep);
        void SetStepZ(IDeviceDelegateOneStep oneStep);
        void SetStopX(IDeviceDelegateStop stop);
        void SetStopY(IDeviceDelegateStop stop);
        void SetStopZ(IDeviceDelegateStop stop);

    }
}