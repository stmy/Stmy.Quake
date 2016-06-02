using System;
using System.Collections.Generic;

namespace Stmy.Quake
{
    public static class JMA2001
    {
        static C5.TreeSet<double> depthGrid;
        static C5.TreeSet<double> distanceGrid;
        static C5.TreeDictionary<double, C5.TreeDictionary<double, int>> valueIndex;
        static Value[] values;

        static double minDepth;
        static double maxDepth;
        static double minDistance;
        static double maxDistance;

        static JMA2001()
        {
            InitializeTable();
        }

        private static void InitializeTable()
        {
            depthGrid = new C5.TreeSet<double>();
            distanceGrid = new C5.TreeSet<double>();
            valueIndex = new C5.TreeDictionary<double, C5.TreeDictionary<double, int>>();
            var valueList = new List<Value>();

            var index = 0;
            var lines = Properties.Resources.Tjma2001.Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var line in lines)
            {
                var primary = double.Parse(line.Substring(2, 8));
                var secondary = double.Parse(line.Substring(13, 8));
                var depth = double.Parse(line.Substring(22, 3));
                var distance = double.Parse(line.Substring(26, 6));

                if (!depthGrid.Contains(depth)) depthGrid.Add(depth);
                if (!distanceGrid.Contains(distance)) distanceGrid.Add(distance);

                var value = new Value
                {
                    Depth = depth,
                    Distance = distance,
                    Primary = primary,
                    Secondary = secondary
                };

                if (!valueIndex.Contains(depth)) valueIndex.Add(depth, new C5.TreeDictionary<double, int>());
                valueIndex[depth].Add(distance, index);

                valueList.Add(value);

                index++;
            }

            values = valueList.ToArray();

            minDepth = depthGrid.FindMin();
            maxDepth = depthGrid.FindMax();
            minDistance = distanceGrid.FindMin();
            maxDistance = distanceGrid.FindMax();
        }

        private static Tuple<double, double> GetNearbyDepths(double depth)
        {
            if (depth < minDepth || depth > maxDepth)
            {
                throw new ArgumentOutOfRangeException();
            }

            var lower = depthGrid.WeakPredecessor(depth);
            var higher = depthGrid.Successor(depth);

            return new Tuple<double, double>(lower, higher);
        }

        private static Tuple<double, double> GetNearbyDistances(double distance)
        {
            if (distance < minDistance || distance > maxDistance)
            {
                throw new ArgumentOutOfRangeException();
            }

            var lower = distanceGrid.WeakPredecessor(distance);
            var higher = distanceGrid.Successor(distance);

            return new Tuple<double, double>(lower, higher);
        }

        private static Value[] GetNearbyValues(double depth, double distance)
        {
            var nearbyDepths = GetNearbyDepths(depth);
            var nearbyDistances = GetNearbyDistances(distance);

            var result = new Value[4];
            result[0] = values[valueIndex[nearbyDepths.Item1][nearbyDistances.Item1]];
            result[1] = values[valueIndex[nearbyDepths.Item1][nearbyDistances.Item2]];
            result[2] = values[valueIndex[nearbyDepths.Item2][nearbyDistances.Item1]];
            result[3] = values[valueIndex[nearbyDepths.Item2][nearbyDistances.Item2]];

            return result;
        }

        public static Tuple<double, double> GetTime(double depth, double distance)
        {
            var nearby = GetNearbyValues(depth, distance);
            var result = GetTimeInterpolated(depth, distance, nearby);
            return new Tuple<double, double>(result.Primary, result.Secondary);
        }

        private static Value GetTimeInterpolated(double depth, double distance, Value[] values)
        {
            // x = distance, y = depth
            var x = distance;
            var y = depth;
            var x1 = values[0].Distance;
            var y1 = values[0].Depth;
            var x2 = values[3].Distance;
            var y2 = values[3].Depth;
            var dxdy = (x2 - x1) * (y2 - y1);

            // Get interpolated value by bilinear formula
            var primary = (x2 - x) * (y2 - y) / dxdy * values[0].Primary
                        + (x - x1) * (y2 - y) / dxdy * values[1].Primary
                        + (x2 - x) * (y - y1) / dxdy * values[2].Primary
                        + (x - x1) * (y - y1) / dxdy * values[3].Primary;
            var secondary = (x2 - x) * (y2 - y) / dxdy * values[0].Secondary
                          + (x - x1) * (y2 - y) / dxdy * values[1].Secondary
                          + (x2 - x) * (y - y1) / dxdy * values[2].Secondary
                          + (x - x1) * (y - y1) / dxdy * values[3].Secondary;

            return new Value
            {
                Depth = depth,
                Distance = distance,
                Primary = primary,
                Secondary = secondary
            };
        }

        class Value
        {
            public double Depth { get; set; }
            public double Distance { get; set; }
            public double Primary { get; set; }
            public double Secondary { get; set; }
        }
    }
}
