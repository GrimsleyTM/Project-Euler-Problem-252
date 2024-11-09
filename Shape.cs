using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EulerProject252
{
    public struct Shape
    {
        public Point[] orderedPoints;
        public int areaDoubled;

        public Shape(Point[] orderedPoints)
        {
            this.orderedPoints = orderedPoints;
            areaDoubled = 0;
            areaDoubled = CalcAreaDoubled();
        }

        private int CalcAreaDoubled()
        {
            int runningSum = 0;
            for (int i = 0; i < orderedPoints.Length; i++)
            {
                int j = (i == orderedPoints.Length - 1) ? 0 : i + 1;
                runningSum += orderedPoints[i].X * orderedPoints[j].Y
                    - orderedPoints[j].X * orderedPoints[i].Y;
            }

            return runningSum;
        }
    }
}
