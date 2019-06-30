
namespace ProfERP.Netduino.GCodeParser
{
    public struct Point
    {
        public float X;
        public float Y;

        public Point(float x, float y)
        {
            X = x;
            Y = y;
        }
    }

    public class ArcInterpolation
    {
        internal Point Start, Center, End;
        internal float Alpha, Beta, Gamma;
        internal float Radius;
        internal bool Clockwise;

        public ArcInterpolation(Point start, Point end, float radius, bool clockwise)
        {
            // PhlatScript uses the radius format, so we need to support it. 

            Start = start;
            End = end;
            Radius = radius;
            Clockwise = !clockwise;

            // Calculate center. Best explanation found here: 
            // http://mathforum.org/library/drmath/view/53027.html

            float xs = Start.X;
            float ys = Start.Y;
            float xe = End.X;
            float ye = End.Y;
            float r = Radius;

            // Distance between start and end
            float q = (float)System.Math.Sqrt((xe - xs) * (xe - xs) + (ye - ys) * (ye - ys));

            // middle ploint between both points
            float xc = (xs + xe) / 2;
            float yc = (ys + ye) / 2;

            if (!Clockwise)
            {
                Center = new Point(
                    (float)(xc - System.Math.Sqrt(r * r - (q / 2) * (q / 2)) * (ys - ye) / q),
                    (float)(yc - System.Math.Sqrt(r * r - (q / 2) * (q / 2)) * (xe - xs) / q)
                    );
            }
            else
            {
                Center = new Point(
                    (float)(xc + System.Math.Sqrt(r * r - (q / 2) * (q / 2)) * (ys - ye) / q),
                    (float)(yc + System.Math.Sqrt(r * r - (q / 2) * (q / 2)) * (xe - xs) / q)
                    );
            }

            const string E_NO_ARC_CENTER = "Could not find a suitable center for arc";

            if (Center.X == float.MinValue) GCodeMachine.Error(E_NO_ARC_CENTER);
            if (Center.Y == float.MinValue) GCodeMachine.Error(E_NO_ARC_CENTER);

            Initialize();
        }

        public ArcInterpolation(Point origin, Point center, Point end, bool clockwise)
        {
            Start = origin;
            Center = center;
            End = end;
            Clockwise = clockwise;

            Initialize();
        }

        private void Initialize()
        {
            // Radius from start to center. 

            const float twoPi = (float)(2 * System.Math.PI);

            float oox = Start.X - Center.X;
            float ooy = Start.Y - Center.Y;

            float eex = End.X - Center.X;
            float eey = End.Y - Center.Y;

            Radius = (float)System.Math.Sqrt(oox * oox + ooy * ooy);

            // Alpha angle: start with X axis

            Alpha = (float)System.Math.Atan2(ooy, oox);

            // Beta angle: end with X axis

            Beta = (float)System.Math.Atan2(eey, eex);

            if (Alpha < 0 && Beta > 0)
            {
                if (Clockwise)
                {
                    Gamma = Beta - (Alpha + twoPi);
                }
                else
                {
                    Gamma = Beta - Alpha;
                }
            }
            else if (Alpha > 0 && Beta < 0)
            {
                Gamma = (Beta + twoPi) - Alpha;
                if (!Clockwise)
                    Gamma = -Gamma;
            }
            else
            {
                if (Beta > Alpha)
                {
                    if (Clockwise)
                    {
                        Gamma = Alpha - Beta;
                    }
                    else
                    {
                        Gamma = Beta - Alpha;
                    }
                }
                else
                {
                    if (Clockwise)
                    {
                        Gamma = Beta - Alpha;
                    }
                    else
                    {
                        Gamma = twoPi - (Alpha - Beta);
                    }
                }
            }

        }

        public Point GetArcPoint(float t)
        {
            float Delta = Gamma * t + Alpha;

            float x = Radius * (float)System.Math.Cos(Delta);
            float y = Radius * (float)System.Math.Sin(Delta);

            Point result = new Point(Center.X + x, Center.Y + y);

            return result;
        }
    }
}
