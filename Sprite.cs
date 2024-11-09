using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EulerProject252
{
    internal abstract class Sprite
    {
        internal Sprite() { }

        internal abstract float[] Vertices();

        internal virtual float boundFloat(float value, float min, float max)
        {

            if (value > max)
            {
                return max;
            }
            else if (value < min)
            {
                return min;
            }
            else
            {
                return value;
            }
        }
    }
}
