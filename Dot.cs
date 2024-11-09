using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EulerProject252
{
    internal class Dot : Sprite
    {
        private float x;
        private float y;
        private float size;

        internal Dot(float x, float y, float size)
        {
            this.x = boundFloat(x, -1.0f, 1.0f);
            this.y = boundFloat(y, -1.0f, 1.0f);
            this.size = boundFloat(size, -1.0f, 1.0f);
        }

        internal override float[] Vertices()
        {
            float[] dot = new float[48] {
                0.0f, 0.0f,
                -1.0f, 0.0f,
                -0.707106781186f, 0.707106781186f,  // Q2 Lower Triangle
                0.0f, 0.0f,
                0.0f, 1.0f,
                -0.707106781186f, 0.707106781186f,  // Q2 Upper Triangle
                0.0f, 0.0f,
                0.0f, 1.0f,
                0.707106781186f, 0.707106781186f,   // Q1 Upper Triangle
                0.0f, 0.0f,
                1.0f, 0.0f,
                0.707106781186f, 0.707106781186f,   // Q1 Lower Triangle
                0.0f, 0.0f,
                -1.0f, 0.0f,
                -0.707106781186f, -0.707106781186f,  // Q4 Lower Triangle
                0.0f, 0.0f,
                0.0f, -1.0f,
                -0.707106781186f, -0.707106781186f,  // Q4 Upper Triangle
                0.0f, 0.0f,
                0.0f, -1.0f,
                0.707106781186f, -0.707106781186f,   // Q3 Upper Triangle
                0.0f, 0.0f,
                1.0f, 0.0f,
                0.707106781186f, -0.707106781186f,   // Q3 Lower Triangle
            };

            for (int i = 0; i < dot.Length; i++)
            {
                dot[i] *= size;
                if (i%2 == 0)
                {
                    dot[i] += x;
                }
                else
                {
                    dot[i] += y;
                }
            }

            return dot;
        }
    }
}
