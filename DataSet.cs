using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EulerProject252
{
    public class DataSet
    {
        public Point[] points;
        public int minAxisValue;
        public int maxAxisValue;

        // This class generates a set of points according to the rules set out in Euler Project Problem 252.
        public DataSet(int seed = 290797, int divisor = 50515093, int maxValue = 1000, int size = 500)
        {
            // The optimizations in the Solver class require that the maxValue squared does not overflow the int type.
            int sqrtOfMaxInt = (int)Math.Sqrt(int.MaxValue);
            if (maxValue > sqrtOfMaxInt)
            {
                throw new ArgumentException("maxValue argument cannot be greater than " + sqrtOfMaxInt.ToString() + ".");
            }

            minAxisValue = -1 * maxValue;
            maxAxisValue = maxValue - 1;

            int numOfPoints = size;
            points = new Point[numOfPoints];

            int s_n = seed;

            for (int i = 0; i < numOfPoints; i++)
            {
                s_n = (int)(((long)s_n * s_n) % divisor);
                points[i].X = (s_n % (maxValue * 2)) - maxValue;

                s_n = (int)(((long)s_n * s_n) % divisor);
                points[i].Y = (s_n % (maxValue * 2)) - maxValue;
            }

        }
    }
}
