using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EulerProject252
{
    internal class Line : Sprite
    {
        private float x1;
        private float y1;
        private float x2;
        private float y2;
        private float width;

        internal Line(float x1, float y1, float x2, float y2, float width)
        {
            this.x1 = boundFloat(x1, -1.0f, 1.0f);
            this.y1 = boundFloat(y1, -1.0f, 1.0f);
            this.x2 = boundFloat(x2, -1.0f, 1.0f);
            this.y2 = boundFloat(y2, -1.0f, 1.0f);
            this.width = boundFloat(width, -0.1f, 0.1f);
        }

        internal override float[] Vertices()
        {
            float xDelta = x2 - x1;
            float yDelta = y2 - y1;

            float deltaLen = (float)Math.Sqrt((double)(xDelta * xDelta + yDelta * yDelta));

            float normalizedPerpendicularX = (-1 * yDelta) / deltaLen;
            float normalizedPerpendicularY = xDelta / deltaLen;

            float perpX = normalizedPerpendicularX * width;
            float perpY = normalizedPerpendicularY * width;


            float[] line = new float[12] {
                x1 + perpX, y1 + perpY,
                x2 + perpX, y2 + perpY,
                x1 - perpX, y1 - perpY,  // 1st Triangle
                x1 - perpX, y1 - perpY,
                x2 - perpX, y2 - perpY,
                x2 + perpX, y2 + perpY  // 2nd Triangle
            };

            return line;
        }
    }
}
