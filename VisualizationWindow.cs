using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace EulerProject252
{
    internal class VisualizationWindow : GameWindow
    {
        private DataSet dataSet;
        int VertexBufferObject = 0;
        int VertexArrayObject = 0;
        Shader shader;
        float[] vertices;
        const float defaultPointSize = 0.003f;

        public VisualizationWindow(DataSet dataSet, (Point point, float size)[] specialPoints, int width
            , int height, string title) : base(GameWindowSettings.Default, new NativeWindowSettings() {
                ClientSize = (width, height), NumberOfSamples = 8, Title = title })
        {
            this.dataSet = dataSet;

            int maxAxisExtent;
            if (Math.Abs(dataSet.minAxisValue) > dataSet.maxAxisValue)
            {
                maxAxisExtent = Math.Abs(dataSet.minAxisValue);
            }
            else
            {
                maxAxisExtent = dataSet.maxAxisValue;
            }

            int numOfPoints = dataSet.points.Length;
            vertices = new float[numOfPoints * 48];

            float[] pointSize = new float[numOfPoints];
            for (int i = 0; i < numOfPoints; i++)
            {
                pointSize[i] = defaultPointSize;
            }

            foreach (var pointData in specialPoints)
            {
                for (int i = 0; i < numOfPoints; i++)
                {
                    if (dataSet.points[i].X == pointData.point.X
                        && dataSet.points[i].Y == pointData.point.Y)
                    {
                        pointSize[i] = pointData.size;
                    }
                }
            }
            
            for (int i = 0; i < numOfPoints; i++)
            {
                float xLoc = (float)dataSet.points[i].X / (float)dataSet.maxAxisValue;
                float yLoc = (float)dataSet.points[i].Y / (float)dataSet.maxAxisValue;

                new Dot(xLoc, yLoc, pointSize[i]).Vertices().CopyTo(vertices, i*48);
            }

            shader = new Shader("../../../Shaders/shader.vert", "../../../Shaders/shader.frag");

        }

        protected override void OnLoad()
        {
            base.OnLoad();

            GL.ClearColor(1.0f, 1.0f, 1.0f, 1.0f);

            VertexArrayObject = GL.GenVertexArray();
            GL.BindVertexArray(VertexArrayObject);

            VertexBufferObject = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, VertexBufferObject);
            GL.BufferData(BufferTarget.ArrayBuffer, vertices.Length * sizeof(float), vertices, BufferUsageHint.StaticDraw);

            GL.VertexAttribPointer(0, 2, VertexAttribPointerType.Float, false, 2 * sizeof(float), 0);
            GL.EnableVertexAttribArray(0);
        }

        protected override void OnUnload()
        {
            base.OnUnload();

            shader.Dispose();
        }

        protected override void OnRenderFrame(FrameEventArgs args)
        {
            base.OnRenderFrame(args);

            GL.Clear(ClearBufferMask.ColorBufferBit);

            shader.Use();
            GL.BindVertexArray(VertexArrayObject);
            GL.DrawArrays(PrimitiveType.Triangles, 0, vertices.Length / 2);

            SwapBuffers();
        }

        protected override void OnFramebufferResize(FramebufferResizeEventArgs e)
        {
            base.OnFramebufferResize(e);

            GL.Viewport(0, 0, e.Width, e.Height);
        }

    }
}
