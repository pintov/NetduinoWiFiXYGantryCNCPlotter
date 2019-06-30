using System;

namespace ProfERP.Netduino.GCodeParser
{
    public enum MachineState
    {
        Stopped,
        G0_RapidMove, 
        G1_LinearMove,
        G2_ArcMove, 
        G3_ArcMoveCCW,
        G92_SetPosition,
        D21_ManualMode,
        D92_Calibration
    }

    public enum MachinePlane { XY, XZ, YZ }
    public enum DistanceMode { Absolute, Relative }

    public class GCodeMachine
    {

        private const float ArcStep = 0.1f;  // Make sure it's a "multiple" of 1.0f.

        private MachineState _state = MachineState.Stopped;
        private MachinePlane _plane = MachinePlane.XY;
        private DistanceMode _distanceMode = DistanceMode.Absolute;

        private IDevice _device;

        public GCodeMachine(IDevice device)
        {
            _device = device;
        }

        public void SetState(MachineState state)
        {
            _state = state;
        }
        internal void Start()
        {
            _device.StartAllAxes();
        }

        internal void Stop()
        {
            _device.StopAllAxes();
        }

        public void SetPlane(MachinePlane plane)
        {
            _plane = plane;
        }

        public void SetMode(DistanceMode mode)
        {
            _distanceMode = mode;
        }

        public void Move(float x, float y, float z, float i, float j, float k, float r)
        {
            switch (_state)
            { 
                case MachineState.G0_RapidMove:
                case MachineState.G1_LinearMove:
                    LinearMove(x, y, z);
                    break;
                case MachineState.G2_ArcMove:
                    ArcMove2(x, y, z, i, j, k, r, true);
                    break;
                case MachineState.G3_ArcMoveCCW:
                    ArcMove2(x, y, z, i, j, k, r, false);
                    break;
                case MachineState.G92_SetPosition:
                    _device.SetPosition(x, y, z);
                    break;
                case MachineState.D92_Calibration:
                    _device.Calibrate(x, y, z);
                    break;
                default:
                    //Logger.Error("Axis coords received, but machine is not in the right state");
                    break;
            }
        }

        private void LinearMove(float x, float y, float z)
        {
            if (_distanceMode == DistanceMode.Absolute)
            {
                if (x == float.MinValue) x = _device.GetCurrentX();
                if (y == float.MinValue) y = _device.GetCurrentY();
                if (z == float.MinValue) z = _device.GetCurrentZ();

                _device.MoveAbsoluteLinear(x, y, z);
            }
            else
            {
                float absX = _device.GetCurrentX();
                float absY = _device.GetCurrentY();
                float absZ = _device.GetCurrentZ();

                if (x != float.MinValue) absX += x;
                if (y != float.MinValue) absY += y;
                if (z != float.MinValue) absZ += z;

                _device.MoveAbsoluteLinear(absX, absY, absZ);
            }
        }
        private void ArcMove2(float x, float y, float z, float i, float j, float k, float radius, bool clockwise)
        {
            _device.MoveArc(x, y, z, i, j, k, radius, clockwise, _distanceMode);
        }
        
        private void ArcMove(float x, float y, float z, float i, float j, float k, float radius, bool clockwise)
        {

           // Only XY plane supported for now.

           Point start = new Point(_device.GetCurrentX(), _device.GetCurrentY()), end;
           ArcInterpolation arc = null;

            if (radius == float.MinValue)
            {
                // Center format arc.

                if (i == float.MinValue && j == float.MinValue) Error("G2/3: I and J are missing");

                if (i == float.MinValue) i = 0;
                if (j == float.MinValue) j = 0;
                if (x == float.MinValue) x = _device.GetCurrentX();
                if (y == float.MinValue) y = _device.GetCurrentY();
                if (z == float.MinValue) z = _device.GetCurrentZ();

                Point center = new Point(_device.GetCurrentX() + i, _device.GetCurrentY() + j);
                end = new Point(x, y);

                arc = new ArcInterpolation(start, center, end, clockwise);
            }
            else
            {
                // Radius format arc
                // XYZ are the endpoint. R is the radius. 
                if (x == float.MinValue && y == float.MinValue) Error("G2/3: X and Y are missing");

                if (x == float.MinValue) x = _device.GetCurrentX();
                if (y == float.MinValue) y = _device.GetCurrentY();

                if (_distanceMode == DistanceMode.Absolute)
                {
                    end = new Point(x, y);
                }
                else
                {
                    end = new Point(_device.GetCurrentX() + x, _device.GetCurrentY() + y);
                }

                arc = new ArcInterpolation(start, end, radius, clockwise);
            }


            if (arc == null) Error("G2/3: could not find an arc solution");

            for (float t = ArcStep; t <= 1.0 + (ArcStep / 2); t += ArcStep)
            {
                Point target = arc.GetArcPoint(t);

                // Only XY supported
                _device.MoveAbsoluteLinear(target.X, target.Y, _device.GetCurrentZ());
            }
        }

        public static void Error(string msg, params object[] args)
        {
            throw new Exception(msg);
        }
    }
}
