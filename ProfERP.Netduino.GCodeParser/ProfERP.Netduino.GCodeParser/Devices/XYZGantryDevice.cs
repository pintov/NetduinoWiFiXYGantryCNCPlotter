using System;
using Microsoft.SPOT;

namespace ProfERP.Netduino.GCodeParser
{

    public class XYZGantryDevice : IDevice
    {
        private float StepsPerMmX = 26.666666f;
        private float StepsPerMmY = 26.666666f;
        private float StepsPerMmZ = 26.666666f;

        private const bool PositiveAxisDirection = true;
        private const bool NegativeAxisDirection = !PositiveAxisDirection;

        private float _currentX;
        private float _currentY;
        private float _currentZ;

        private IDeviceDelegateOneStep oneStepX;
        private IDeviceDelegateOneStep oneStepY;
        private IDeviceDelegateOneStep oneStepZ;

        private IDeviceDelegateStop startX;
        private IDeviceDelegateStop startY;
        private IDeviceDelegateStop startZ;

        private IDeviceDelegateStop stopX;
        private IDeviceDelegateStop stopY;
        private IDeviceDelegateStop stopZ;

        public XYZGantryDevice()
        {
        }
        public void StartAllAxes()
        {
            startX();
            startY();
            startZ();
        }

        public void StopAllAxes()
        {
            stopX();
            stopY();
            stopZ();
        }


        public void SetPosition(float x, float y, float z)
        {
            _currentX = x;
            _currentY = y;
            _currentZ = z;
        }

        public void MoveAxes(int dx, int dy, int dz = 0)
        {
            int dirz = dz > 0 ? 1 : -1;
            dz = Abs(dz);
            for (int i = 0; i < dz; ++i)
                oneStepZ(dirz);

            // Uses Bresenham's line algorithm for X and Y

            int dirx = dx > 0 ? 1 : -1;
            int diry = dy > 0 ? 1 : -1;

            dx = Abs(dx);
            dy = Abs(dy);

            int over = 0;
            if (dx > dy)
            {
                over = dx / 2;
                for (int i = 0; i < dx; ++i)
                {
                    oneStepX(dirx);
                    over += dy;
                    if (over >= dx)
                    {
                        over -= dx;
                        oneStepY(diry);
                    }
                }
            }
            else
            {
                over = dy / 2;
                for (int i = 0; i < dy; ++i)
                {
                    oneStepY(diry);
                    over += dx;
                    if (over >= dy)
                    {
                        over -= dy;
                        oneStepX(dirx);
                    }
                }
            }
        }

        public void Calibrate(float x, float y, float z)
        {
            if (x > float.MinValue) StepsPerMmX = x;
            if (y > float.MinValue) StepsPerMmY = y;
            if (z > float.MinValue) StepsPerMmZ = z;

            Print(GetCalibration());
        }

        public string GetCalibration()
        {
            return "CALIBRATION"
                + " X:" + StepsPerMmX
                + " Y:" + StepsPerMmY
                + " Z:" + StepsPerMmZ;
        }

        public void MoveAbsoluteLinear(float x, float y, float z)
        {
            float xRelative = x - _currentX;
            float yRelative = y - _currentY;
            float zRelative = z - _currentZ;

            int xRelativeSteps = (int)(xRelative * StepsPerMmX);
            int yRelativeSteps = (int)(yRelative * StepsPerMmY);
            int zRelativeSteps = (int)(zRelative * StepsPerMmZ);

            MoveAxes(xRelativeSteps, yRelativeSteps, zRelativeSteps);

            _currentX = x;
            _currentY = y;
            _currentZ = z;
        }

        public void MoveArc(float x, float y, float z, float i, float j, float k, float radius, bool clockwise, DistanceMode distanceMode)
        {
            // Only XY plane supported for now.

            Point start = new Point(_currentX * StepsPerMmX, _currentY * StepsPerMmY), end;
            ArcInterpolation arc = null;

            if (radius == float.MinValue)
            {
                // Center format arc.

                if (i == float.MinValue && j == float.MinValue)
                    throw new Exception("G2/3: I and J are missing");

                if (i == float.MinValue) i = 0;
                if (j == float.MinValue) j = 0;
                if (x == float.MinValue) x = _currentX;
                if (y == float.MinValue) y = _currentY;
                if (z == float.MinValue) z = _currentZ;

                Point center = new Point((_currentX + i) * StepsPerMmX, (_currentY + j) * StepsPerMmY);
                end = new Point(x * StepsPerMmX, y * StepsPerMmY);

                arc = new ArcInterpolation(start, center, end, clockwise);
            }
            else
            {
                // Radius format arc
                // XYZ are the endpoint. R is the radius. 
                if (x == float.MinValue && y == float.MinValue)
                    throw new Exception("G2/3: X and Y are missing");

                if (x == float.MinValue) x = _currentX;
                if (y == float.MinValue) y = _currentY;

                if (distanceMode == DistanceMode.Absolute)
                {
                    end = new Point(x * StepsPerMmX, y * StepsPerMmY);
                }
                else
                {
                    end = new Point((_currentX + x) * StepsPerMmX, (_currentY + y) * StepsPerMmY);
                }

                arc = new ArcInterpolation(start, end, radius, clockwise);
            }


            if (arc == null)
                throw new Exception("G2/3: could not find an arc solution");
            
            const float ArcStep = 0.05f;  // Make sure it's a "multiple" of 1.0f.

            int currentStepsX = (int)start.X;
            int currentStepsY = (int)start.Y;

            for (float t = ArcStep; t <= 1.0 + (ArcStep / 2); t += ArcStep)
            {
                Point target = arc.GetArcPoint(t);

                // Only XY supported
                int targetX = (int)target.X;
                int targetY = (int)target.Y;


                int dx = (int)target.X - currentStepsX;
                int dy = (int)target.Y - currentStepsY;

                MoveAxes(dx, dy);

                currentStepsX += dx;
                currentStepsY += dy;
            }

            _currentX = currentStepsX / StepsPerMmX;
            _currentY = currentStepsY / StepsPerMmY;
        }

        private void Print(string s)
        {
            Debug.Print(s);
        }

        private int Abs(int val)
        {
            return (val < 0) ? -val : val;
        }

        public float GetCurrentX()
        {
            return _currentX;
        }

        public float GetCurrentY()
        {
            return _currentY;
        }

        public float GetCurrentZ()
        {
            return _currentZ;
        }

        public void SetStepX(IDeviceDelegateOneStep oneStep)
        {
            oneStepX = oneStep;
        }

        public void SetStepY(IDeviceDelegateOneStep oneStep)
        {
            oneStepY = oneStep;
        }

        public void SetStepZ(IDeviceDelegateOneStep oneStep)
        {
            oneStepZ = oneStep;
        }
        public void SetStartX(IDeviceDelegateStop start)
        {
            startX = start;
        }
        public void SetStartY(IDeviceDelegateStop start)
        {
            startY = start;
        }
        public void SetStartZ(IDeviceDelegateStop start)
        {
            startZ = start;
        }

        public void SetStopX(IDeviceDelegateStop stop)
        {
            stopX = stop;
        }
        public void SetStopY(IDeviceDelegateStop stop)
        {
            stopY = stop;
        }
        public void SetStopZ(IDeviceDelegateStop stop)
        {
            stopZ = stop;
        }
    }
}
