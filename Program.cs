using BenchmarkDotNet.Running;
using BenchmarkDotNet.Attributes;
using System;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Toolchains.InProcess.NoEmit;
using System.Data;
using System.Drawing;

namespace EulerProject252 // Note: actual namespace depends on the project name.
{
    public class Program
    {
        static void Main(string[] args)
        {
            DataSet dataSet = new DataSet(size: 500);
            Solver solver = new Solver(dataSet);
            Console.WriteLine("Largest Area: " + solver.Solve().ToString());


            //List<(Point, float)[]> shapes = solver.Solve();
            //foreach (var shape in shapes)
            //{
            //    ShowVisualization(new DataSet(size: 500), shape);
            //}

            //RunBenchmarkDotNet();

            //ShowVisualization(new DataSet(size: 500));

        }

        static void ShowVisualization(DataSet dataSet, (Point, float)[] specialPoints)
        {
            using (VisualizationWindow visualization = new VisualizationWindow(dataSet, specialPoints, 960, 960, "Convex Holes Visualization"))
            {
                visualization.Run();
            }
        }

        static void ShowVisualization(DataSet dataSet)
        {
            (Point, float)[]  specialPoints = new (Point, float)[]
            {
                (new Point(128, -889), 0.005f),
                (new Point(254, -866), 0.005f),
                (new Point(720, -780), 0.005f),
                (new Point(964, -726), 0.005f),
                (new Point(343, -756), 0.005f),
                (new Point(-28, -781), 0.005f),
                (new Point(-330, -809), 0.005f),
                (new Point(-571, -847), 0.005f),
                (new Point(-218, -878), 0.005f)
            };

            using (VisualizationWindow visualization = new VisualizationWindow(dataSet, specialPoints, 960, 960, "Convex Holes Visualization"))
            {
                visualization.Run();
            }
        }

        static void RunBenchmarkDotNet()
        {
            var config = DefaultConfig.Instance
                .AddJob(Job
                    .MediumRun
                    .WithLaunchCount(1)
                    );
            //            .WithToolchain(InProcessNoEmitToolchain.Instance));
            var summary = BenchmarkRunner.Run(typeof(Program).Assembly, config);
        }

    }

}