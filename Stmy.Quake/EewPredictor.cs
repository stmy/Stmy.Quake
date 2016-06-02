using System;
using static System.Math;

namespace Stmy.Quake.Resources
{
    public class EewPredictor
    {
        public static PredictionResult Predict(
            Earthquake quake, 
            double observerLat, 
            double observerLon, 
            double amplifier)
        {
            var result = new PredictionResult();
            
            const double a = 6370.291; // Radius of the Earth in km

            var φe = ToRadian(quake.Latitude);
            var λe = ToRadian(quake.Longitude);
            var φx = ToRadian(observerLat);
            var λx = ToRadian(observerLon);
            φe = φe - (11.55 / 60.0) * PI / 180.0 * Sin(2.0 * φe);
            φx = φx - (11.55 / 60.0) * PI / 180.0 * Sin(2.0 * φx);

            // Calculate hypocentral distance S0
            var Ae = Cos(φe) * Cos(λe);
            var Ax = Cos(φx) * Cos(λx);
            var Be = Cos(φe) * Sin(λe);
            var Bx = Cos(φx) * Sin(λx);
            var Ce = Sin(φe);
            var Cx = Sin(φx);
            var θL = Asin(Sqrt((Pow(Ae - Ax, 2) + Pow(Be - Bx, 2) + Pow(Ce - Cx, 2))) / 2.0) * 2.0;
            var L0 = θL * a;

            var rd = (a - quake.Depth) / a;
            Ae *= rd; Be *= rd; Ce *= rd;
            var ΔS = Sqrt(Pow(Ae - Ax, 2) + Pow(Be - Bx, 2) + Pow(Ce - Cx, 2));
            var S0 = ΔS * a;

            // Predict minimun fault distance X
            var Mw = quake.Magnitude - 0.171;
            var L = Max(Pow(10, 0.5 * Mw - 1.85), 3);
            var X = Max(S0 - L / 2.0, 3.0);

            // Predict velocity and JMA intensity at the observer
            var PGV600 = Pow(10, 0.58 * Mw + 0.0038 * quake.Depth - 1.29 - Log10(X - 0.002810 * Pow(10, 0.50 * Mw)) - 0.002 * X);
            var PGV = amplifier * PGV600;

            result.Intensity = 2.68 + 1.72 * Log10(PGV);

            // Predict arrival times
            var t = JMA2001.GetTime(quake.Depth, L0);
            result.PrimaryWaveArrival = quake.OriginTime.AddSeconds(t.Item1);
            result.SecondaryWaveArrival = quake.OriginTime.AddSeconds(t.Item2);

            return result;
        }

        static double ToRadian(double degree)
        {
            return PI * degree / 180.0;
        }

        public class PredictionResult
        {
            public double Intensity { get; set; }
            public DateTime PrimaryWaveArrival { get; set; }
            public DateTime SecondaryWaveArrival { get; set; }
        }
    }
}
