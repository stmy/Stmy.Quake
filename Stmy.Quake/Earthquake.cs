using System;

namespace Stmy.Quake
{
    public class Earthquake
    {
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public double Depth { get; set; }
        public double Magnitude { get; set; }
        public DateTime OriginTime { get; set; }
    }
}
