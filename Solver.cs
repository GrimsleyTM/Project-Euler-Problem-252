using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Characteristics;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Numerics;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace EulerProject252
{
    [MemoryDiagnoser]
    public class Solver
    {
        DataSet dataSet;

        // For use by BenchmarkDotNet
        public Solver()
        {
            DataSet dataSet = new DataSet(size: 50);
            this.dataSet = dataSet;
        }

        public Solver(DataSet dataSet)
        {
            this.dataSet = dataSet;
        }

        [Benchmark(Description = "Full Solve, find largest convex shape")]
        public double Solve()
        {
            Point[] points = dataSet.points;

            List<(int pointAIndex, int pointBIndex, int pointCIndex, int areaDoubled)> triangles = new List<(int, int, int, int)>();

            for (int pointAIndex = 0; pointAIndex < points.Length - 2; pointAIndex++)
            {
                for (int pointBIndex = pointAIndex + 1; pointBIndex < points.Length - 1; pointBIndex++)
                {
                    for (int pointCIndex = pointBIndex + 1; pointCIndex < points.Length; pointCIndex++)
                    {
                        (bool enclosesPoint, int areaDoubled) = TriangleEnclosesPoint(points, pointAIndex, pointBIndex, pointCIndex);

                        if (!enclosesPoint && areaDoubled > 0)
                        {
                            triangles.Add((pointAIndex, pointBIndex, pointCIndex, areaDoubled));
                        }
                    }
                }
            }

            triangles.Sort(delegate ((int x, int y, int z, int a) t1, (int x, int y, int z, int a) t2)
            {
                if (t1.a > t2.a) return -1;
                else if (t1.a == t2.a) return 0;
                else return 1;
            });

            int largestAreaDoubled = 0;
            Point[] orderedPointsOfLargestShape;

            List<(Point, float)[]> shapes = new List<(Point, float)[]>();

            for (int i = 0; i < triangles.Count; i++)
            {
                var largestShape = LargestShapeWithTriangle(points, (triangles[i].pointAIndex, triangles[i].pointBIndex
                    , triangles[i].pointCIndex, triangles[i].areaDoubled));

                if (largestShape.areaDoubled > largestAreaDoubled)
                {
                    (Point point, float size)[] shape = new (Point, float)[largestShape.orderedPoints.Length];
                    for (int j = 0; j < shape.Length; j++)
                    {
                        shape[j].point = largestShape.orderedPoints[j];
                        if (shape[j].point == points[triangles[i].pointAIndex]
                            || shape[j].point == points[triangles[i].pointBIndex]
                            || shape[j].point == points[triangles[i].pointCIndex])
                        {
                            shape[j].size = 0.007f;
                        }
                        else
                            shape[j].size = 0.005f;
                    }

                    shapes.Add(shape);

                    largestAreaDoubled = largestShape.areaDoubled;
                    orderedPointsOfLargestShape = largestShape.orderedPoints;
                    //Console.WriteLine("New Largest Area: " + largestAreaDoubled.ToString() + "@ Triangle Index: " + i.ToString());
                    //for (int j = 0; j < largestShape.orderedPoints.Length; j++)
                    //{
                    //    Console.Write("(" + largestShape.orderedPoints[j].X.ToString() + ", "
                    //        + largestShape.orderedPoints[j].Y.ToString() + "); ");
                    //}
                    //Console.WriteLine();
                }
            }

            return largestAreaDoubled / 2d;
        }

        private int SideOfLine(Point segmentPointA, Point segmentPointB, Point testPoint)
        {
            Point B = new Point(segmentPointB.X - segmentPointA.X, segmentPointB.Y - segmentPointA.Y);
            Point T = new Point(testPoint.X - segmentPointA.X, testPoint.Y - segmentPointA.Y);

            // The follow is an optimized version of:
            // First, check if B.Y is zero; if it is,
            //  return 1 if T.Y is less than zero,
            //  return 0 if T.Y is zero
            //  return -1 if T.Y is greater than zero
            // If B.Y is not zero, calculate the inverse slope of segment AB (B.X/B.Y) times T.Y.
            // Conceptually, this gives you the X value of a point on the line-through-AB with the same
            // Y value as T.Y. Then, subtract that value from T.X, which tells you how far the point T is
            // from the line-through-AB along the x-axis
            //  return 1 if this x-distance value is greater than zero,
            //  return 0 if the x-distance value is zero
            //  return -1 if the x-distance value is less than zero

            int tempValue = T.X * B.Y - T.Y * B.X;
            if (B.Y < 0)
            {
                tempValue = -1 * tempValue;
            }
            
            if (tempValue > 0)
                return 1;
            else if (tempValue == 0)
                return 0;
            else
                return -1;
        }

        // Returns true if the triangle defined by the three input indicies, which are indicies of the points[] array,
        // contains any of the other points in the points[] array. Returns false if there are no enclosed points.
        // Points lying along any one of the line-segments that makes up the triangle do not count as enclosed points.
        // Function also returns double the area of each triangle without an enclosed point. It is double the area
        // so the value can be stored in an integer without rounding (see the shoelace theorem).
        // Function assumes none of the three input points have the same coordinates.
        private (bool hasEnclosedPoint, int areaDoubled) TriangleEnclosesPoint(Point[] points,
            int trianglePointAIndex, int trianglePointBIndex, int trianglePointCIndex)
        {
            Point A = points[trianglePointAIndex];
            Point B = points[trianglePointBIndex];
            Point C = points[trianglePointCIndex];

            if(SideOfLine(A, B, C) == 0)
            {
                return (false, 0);
            }

            byte[] enclosedPointFlag = new byte[points.Length];

            for (int i = 0; i < enclosedPointFlag.Length; i++)
            {
                enclosedPointFlag[i] = 1;
            }
            enclosedPointFlag[trianglePointAIndex] = 0;
            enclosedPointFlag[trianglePointBIndex] = 0;
            enclosedPointFlag[trianglePointCIndex] = 0;

            (Point min, Point max) = BoundingBox(A, B, C);

            for (int i = 0; i < points.Length; i++)
            {
                if (points[i].X < min.X || points[i].X > max.X
                    || points[i].Y < min.Y || points[i].Y > max.Y)
                {
                    enclosedPointFlag[i] = 0;
                }
                else if (SideOfLine(A, B, C) != SideOfLine(A, B, points[i]))
                {
                    enclosedPointFlag[i] = 0;
                }
                else if (SideOfLine(B, C, A) != SideOfLine(B, C, points[i]))
                {
                    enclosedPointFlag[i] = 0;
                }
                else if (SideOfLine(A, C, B) != SideOfLine(A, C, points[i]))
                {
                    enclosedPointFlag[i] = 0;
                }
            }

            if(ArrayContainsValue(enclosedPointFlag, 1))
            {
                return (true, 0);
            }

            return (false, areaOfTriangleDoubled(A, B, C));
        }

        private int areaOfTriangleDoubled(Point A, Point B, Point C)
        {
            return Math.Abs(A.X * B.Y + B.X * C.Y + C.X * A.Y - B.X * A.Y - C.X * B.Y - A.X * C.Y);
        }

        // Returns the bottom-left and top-right cartesian coordinates of an x-y aligned box
        // with minimum area that encloses the triangle defined by points A, B, & C.
        private (Point min, Point max) BoundingBox(Point A, Point B, Point C)
        {
            Point min = new Point(int.MinValue, int.MinValue);
            Point max = new Point(int.MaxValue, int.MaxValue);

            if(A.X > B.X)
            {
                max.X = A.X;
                min.X = B.X;
            }
            else
            {
                max.X = B.X;
                min.X = A.X;
            }

            if (A.Y > B.Y)
            {
                max.Y = A.Y;
                min.Y = B.Y;
            }
            else
            {
                max.Y = B.Y;
                min.Y = A.Y;
            }
            
            if (C.X > max.X)
            {
                max.X = C.X;
            }
            else if (C.X < min.X)
            {
                min.X = C.X;
            }

            if (C.Y > max.Y)
            {
                max.Y = C.Y;
            }
            else if (C.Y < min.Y)
            {
                min.Y = C.Y;
            }

            return (min, max);
        }
        
        private (Point[] orderedPoints, int areaDoubled) LargestShapeWithTriangle(Point[] points,
            (int pointAIndex, int pointBIndex, int pointCIndex, int areaDoubled) triangle)
        {
            byte[] pointBeyondLineABFlags = new byte[points.Length];
            FlagPtsOnSideOppositeToRefPt(pointBeyondLineABFlags, points, triangle.pointAIndex, triangle.pointBIndex, points[triangle.pointCIndex]);
            pointBeyondLineABFlags[triangle.pointAIndex] = 1;
            pointBeyondLineABFlags[triangle.pointBIndex] = 1;
            pointBeyondLineABFlags[triangle.pointCIndex] = 1;

            byte[] pointBeyondLineBCFlags = new byte[points.Length];
            FlagPtsOnSideOppositeToRefPt(pointBeyondLineBCFlags, points, triangle.pointBIndex, triangle.pointCIndex, points[triangle.pointAIndex]);
            pointBeyondLineBCFlags[triangle.pointAIndex] = 1;
            pointBeyondLineBCFlags[triangle.pointBIndex] = 1;
            pointBeyondLineBCFlags[triangle.pointCIndex] = 1;

            byte[] pointBeyondLineCAFlags = new byte[points.Length];
            FlagPtsOnSideOppositeToRefPt(pointBeyondLineCAFlags, points, triangle.pointCIndex, triangle.pointAIndex, points[triangle.pointBIndex]);
            pointBeyondLineCAFlags[triangle.pointAIndex] = 1;
            pointBeyondLineCAFlags[triangle.pointBIndex] = 1;
            pointBeyondLineCAFlags[triangle.pointCIndex] = 1;

            var sideABPoints = ValidPointsBeyondSideAB(points, pointBeyondLineBCFlags, pointBeyondLineCAFlags
                , (triangle.pointAIndex, triangle.pointBIndex, triangle.pointCIndex));
            var sideBCPoints = ValidPointsBeyondSideAB(points, pointBeyondLineCAFlags, pointBeyondLineABFlags
                , (triangle.pointBIndex, triangle.pointCIndex, triangle.pointAIndex));
            var sideCAPoints = ValidPointsBeyondSideAB(points, pointBeyondLineABFlags, pointBeyondLineBCFlags
                , (triangle.pointCIndex, triangle.pointAIndex, triangle.pointBIndex));

            var sideABShapes = ConvexShapesWithSideAB(points, sideABPoints, points[triangle.pointAIndex], points[triangle.pointBIndex]);
            var sideBCShapes = ConvexShapesWithSideAB(points, sideBCPoints, points[triangle.pointBIndex], points[triangle.pointCIndex]);
            var sideCAShapes = ConvexShapesWithSideAB(points, sideCAPoints, points[triangle.pointCIndex], points[triangle.pointAIndex]);

            var shapes = ValidShapeCombinations(points, triangle.pointAIndex, triangle.pointBIndex, triangle.pointCIndex
                , sideABShapes, sideBCShapes, sideCAShapes);

            shapes.Sort(delegate (Shape a, Shape b)
            {
                if (a.areaDoubled > b.areaDoubled) return -1;
                else if (a.areaDoubled == b.areaDoubled) return 0;
                else return 1;
            });

            return (shapes[0].orderedPoints, shapes[0].areaDoubled);
        }

        private List<Shape> ConvexShapesWithSideAB(Point[] points, Point[] trialPoints, Point pointA, Point pointB)
        {
            List<int[]> pointIndexSets = ValidPointIndiciesOrders(trialPoints, pointA, pointB);

            List<Shape> shapes = new List<Shape>();
            foreach (int[] pointIndexSet in pointIndexSets)
            {
                Point[] orderedPoints = new Point[pointIndexSet.Length + 2];

                orderedPoints[0] = pointA;
                for (int i = 1; i < orderedPoints.Length - 1; i++)
                {
                    orderedPoints[i] = trialPoints[pointIndexSet[i - 1]];
                }
                orderedPoints[orderedPoints.Length - 1] = pointB;

                if (IsShapeConvex(orderedPoints)
                    && !ShapeEnclosesPoint(points, orderedPoints))
                {
                    shapes.Add(new Shape(orderedPoints));
                }
            }

            return shapes;
        }

        // Returns true if the shape defined by an ordered list of points is a convex polygon.
        // If the reference point is one of the points that describes the shape, the concavity of segments
        // containing the reference point will not be checked.
        private bool IsShapeConvex(Point[] orderedPoints)
        {
            for (int i = 0; i < orderedPoints.Length - 2; i++)
            {
                byte[] pointFlags = new byte[orderedPoints.Length];
                FlagPtsOnSideOppositeToRefPt(pointFlags, orderedPoints, i, i + 1, orderedPoints[i + 2]);

                if(ArrayContainsValue(pointFlags, 1))
                {
                    return false;
                }
            }

            return true;
        }

        private List<int[]> ValidPointIndiciesOrders(Point[] points, Point pointA, Point pointB)
        {
            List<int[]> listOfShapeIndicies = new List<int[]>();
            for (int i = 0; i < points.Length; i++)
            {
                List<int> firstIndex = new List<int>();
                firstIndex.Add(i);
                byte[] pointFlags = new byte[points.Length];
                listOfShapeIndicies.AddRange(NestedCulledLoopMarch(firstIndex, points, pointFlags, pointA, pointB));
            }

            return listOfShapeIndicies;
        }

        private List<int[]> NestedCulledLoopMarch(List<int> lowerLevelIndicies, Point[] points, byte[] culledPointFlags
            , Point pointA, Point pointB)
        {
            List<int[]> listOfShapeIndicies = new List<int[]>();

            int[] shapeIndicies = new int[lowerLevelIndicies.Count];
            for (int i = 0; i < shapeIndicies.Length; i++)
            {
                shapeIndicies[i] = lowerLevelIndicies[i];
            }
            listOfShapeIndicies.Add(shapeIndicies);


            int currentPointIndex = lowerLevelIndicies[lowerLevelIndicies.Count - 1];
            culledPointFlags[currentPointIndex] = 1;

            if (ArrayContainsValue(culledPointFlags, 0))
            {
                Point linePointA;
                Point linePointB;

                int depthCount = lowerLevelIndicies.Count;
                if (depthCount == 1)
                {
                    linePointA = pointA;
                }
                else
                {
                    linePointA = points[lowerLevelIndicies[depthCount - 2]];
                }
                linePointB = points[lowerLevelIndicies[depthCount - 1]];

                int newSegmentSideRef = -1 * SideOfLine(linePointA, linePointB, pointB);
                int rootSegmentSideRef = -1 * SideOfLine(pointA, linePointB, pointB);
                for (int i = 0; i < points.Length; i++)
                {
                    int newSegmentSide = SideOfLine(linePointA, linePointB, points[i]);
                    int rootSegmentSide = SideOfLine(pointA, linePointB, points[i]);

                    if (newSegmentSide == newSegmentSideRef
                        || rootSegmentSide == rootSegmentSideRef)
                    {
                        culledPointFlags[i] = 1;
                    }
                }
            }

            for (int i = 0; i < points.Length; i++)
            {
                if (culledPointFlags[i] == 0)
                {
                    lowerLevelIndicies.Add(i);
                    listOfShapeIndicies.AddRange(NestedCulledLoopMarch(lowerLevelIndicies, points, culledPointFlags, pointA, pointB));
                }
            }

            return listOfShapeIndicies;
        }

        private bool ArrayContainsValue(byte[] flagArray, byte testValue)
        {
            for (int i = 0; i < flagArray.Length; i++)
            {
                if (flagArray[i] == testValue)
                {
                    return true;
                }
            }
            return false;
        }

        private Point[] ValidPointsBeyondSideAB(Point[] points, byte[] pointBeyondLineBCFlags
            , byte[] pointBeyondLineCAFlags, (int pointAIndex, int pointBIndex, int pointCIndex) triangle)
        {
            List<(int index, int distSq)> sideABPointList = new List<(int, int)>();
            for (int i = 0; i < points.Length; i++)
            {
                if (pointBeyondLineBCFlags[i] == 0
                    && pointBeyondLineCAFlags[i] == 0)
                {
                    int avgAB_X = (points[triangle.pointAIndex].X + points[triangle.pointBIndex].X) / 2;
                    int avgAB_Y = (points[triangle.pointAIndex].Y + points[triangle.pointBIndex].Y) / 2;
                    int distSquared = (points[i].X - avgAB_X) * (points[i].X - avgAB_X) + (points[i].Y - avgAB_Y) * (points[i].Y - avgAB_Y);
                    sideABPointList.Add((i, distSquared));
                }
            }

            sideABPointList.Sort(delegate ((int i, int d) p1, (int i, int d) p2)
            {
                if (p1.d < p2.d) return -1;
                else if (p1.d == p2.d) return 0;
                else return 1;
            });

            Point[] sideABPoints = new Point[sideABPointList.Count + 3];

            int sideABPointsIndex = 0;

            for (; sideABPointsIndex < sideABPointList.Count; sideABPointsIndex++)
            {
                sideABPoints[sideABPointsIndex] = new Point(points[sideABPointList[sideABPointsIndex].index].X, points[sideABPointList[sideABPointsIndex].index].Y);
            }

            byte[] sideABPointFlags = new byte[sideABPoints.Length];

            sideABPointFlags[sideABPointsIndex] = 1;
            sideABPoints[sideABPointsIndex++] = new Point(points[triangle.pointAIndex].X, points[triangle.pointAIndex].Y);
            sideABPointFlags[sideABPointsIndex] = 1;
            sideABPoints[sideABPointsIndex++] = new Point(points[triangle.pointBIndex].X, points[triangle.pointBIndex].Y);
            sideABPointFlags[sideABPointsIndex] = 1;
            sideABPoints[sideABPointsIndex++] = new Point(points[triangle.pointCIndex].X, points[triangle.pointCIndex].Y);

            for (int i = 0; i < sideABPointList.Count; i++)
            {
                if (sideABPointFlags[i] == 0)
                {
                    FlagPtsInShadowOfRefPt(sideABPointFlags, sideABPoints, sideABPointsIndex - 3, sideABPointsIndex - 2, i);
                }
            }

            int sideABPointCount = 0;

            foreach (int flag in sideABPointFlags)
            {
                if (flag == 0)
                    sideABPointCount++;
            }

            int sideABPAIIndex = 0;
            Point[] finalSideABPoints = new Point[sideABPointCount];

            for (int i = 0; i < sideABPointList.Count; i++)
            {
                if (sideABPointFlags[i] == 0)
                {
                    finalSideABPoints[sideABPAIIndex++] = sideABPoints[i];
                }
            }

            return finalSideABPoints;
        }

        // Given three specific points (A, B, and a reference point, R); and an array of other points which
        // includes A, B, and R. This function populates integer array "pointFlags" with values indicating which
        // points in are between the lines AR and BR; and more distant from each point A and B than point R is.
        // This target zone is a cone starting at point R, extending away from points A and B, and with sides AR and BR.
        // If the point points[i] is in the target zone, the value of pointFlags[i] will be 1; otherwise,
        // the value will be 0.
        private void FlagPtsInShadowOfRefPt(byte[] pointFlags, Point[] points, int pointAIndex, int pointBIndex, int referencePointIndex)
        {
            if (pointFlags.Length != points.Length)
                throw new ArgumentException("The pointsFlags array must have the same number of elements as the points array.");

            byte[] refABoundaryExclusions = new byte[points.Length];
            byte[] refBBoundaryExclusions = new byte[points.Length];
            FlagPtsOnSideOppositeToRefPt(refABoundaryExclusions, points, pointAIndex, referencePointIndex, points[pointBIndex]);
            FlagPtsOnSideOppositeToRefPt(refBBoundaryExclusions, points, pointBIndex, referencePointIndex, points[pointAIndex]);

            for (int i = 0; i < points.Length; i++)
            {
                if (pointFlags[i] == 0
                    && refABoundaryExclusions[i] == 1 && refBBoundaryExclusions[i] == 1)
                {
                    pointFlags[i] = 1;
                }
            }
        }

        // If the reference point lies on the comparison line, AB, nothing will be flagged.
        // Points on the line, AB, are not flagged.
        private void FlagPtsOnSideOppositeToRefPt(byte[] pointFlags, Point[] points, int pointAIndex, int pointBIndex, Point referencePoint)
        {
            if (pointFlags.Length != points.Length)
                throw new ArgumentException("The pointsFlags array must have the same number of elements as the points array.");

            int referenceSide = -1 * SideOfLine(points[pointAIndex], points[pointBIndex], referencePoint);
            if (referenceSide == 0)
            {
                return;
            }

            int side;

            for (int i = 0; i < points.Length; i++)
            {
                side = SideOfLine(points[pointAIndex], points[pointBIndex], points[i]);
                if (side == referenceSide)
                {
                    pointFlags[i] = 1;
                }
            }
        }

        // Returns true if the shape defined by orderedShapePoints contains any of the other points in the points[] array.
        // Returns false if there are no enclosed points. Points lying along any one of the lines that make up the sides
        // of the shape do not count as enclosed points.
        // Function assumes none of the points have the same coordinates
        private bool ShapeEnclosesPoint(Point[] points, Point[] orderedShapePoints)
        {
            byte[] enclosedPointFlag = new byte[points.Length];

            for (int i = 0; i < enclosedPointFlag.Length; i++)
            {
                enclosedPointFlag[i] = 1;
                for (int j = 0; j < orderedShapePoints.Length; j++)
                {
                    if (points[i] == orderedShapePoints[j])
                    {
                        enclosedPointFlag[i] = 0;
                        break;
                    }
                }
            }

            for (int i = 0; i < points.Length; i++)
            {
                int ptBIndex = 1;
                int refPtIndex = 2;
                for (int ptAIndex = 0; ptAIndex < orderedShapePoints.Length; ptAIndex++, ptBIndex++, refPtIndex++)
                {
                    if (ptBIndex >= orderedShapePoints.Length)
                        ptBIndex -= orderedShapePoints.Length;
                    if (refPtIndex >= orderedShapePoints.Length)
                        refPtIndex -= orderedShapePoints.Length;

                    Point refPoint = orderedShapePoints[refPtIndex];

                    int referenceSide = SideOfLine(orderedShapePoints[ptAIndex], orderedShapePoints[ptBIndex], orderedShapePoints[refPtIndex]);

                    if (referenceSide == 0)
                    {
                        continue;
                    }

                    if (referenceSide != SideOfLine(orderedShapePoints[ptAIndex], orderedShapePoints[ptBIndex], points[i]))
                    {
                        enclosedPointFlag[i] = 0;
                        break;
                    }
                }
            }

            if (ArrayContainsValue(enclosedPointFlag, 1))
            {
                return true;
            }

            return false;
        }

        private List<Shape> ValidShapeCombinations(Point[] points, int pointAIndex, int pointBIndex, int pointCIndex
            , List<Shape> sideABShapes, List<Shape> sideBCShapes, List<Shape> sideCAShapes)
        {
            List<Shape> shapes = new List<Shape>();

            for (int i = 0; i < sideABShapes.Count + 1; i++)
            {
                for (int j = 0; j < sideBCShapes.Count + 1; j++)
                {
                    for (int k = 0; k < sideCAShapes.Count + 1; k++)
                    {
                        int numSideABPoints = (i == sideABShapes.Count) ? 2 : sideABShapes[i].orderedPoints.Length;
                        int numSideBCPoints = (j == sideBCShapes.Count) ? 2 : sideBCShapes[j].orderedPoints.Length;
                        int numSideCAPoints = (k == sideCAShapes.Count) ? 2 : sideCAShapes[k].orderedPoints.Length;

                        int numPointsInShape = numSideABPoints + numSideBCPoints + numSideCAPoints - 3;

                        Point[] orderedPoints = new Point[numPointsInShape];

                        int orderedPointsIndex = 0;
                        if (i == sideABShapes.Count)
                        {
                            orderedPoints[orderedPointsIndex++] = points[pointAIndex];
                        }
                        else
                        {
                            for (int a = 0; a < numSideABPoints - 1; a++)
                            {
                                orderedPoints[orderedPointsIndex++] = sideABShapes[i].orderedPoints[a];
                            }
                        }
                        if (j == sideBCShapes.Count)
                        {
                            orderedPoints[orderedPointsIndex++] = points[pointBIndex];
                        }
                        else
                        {
                            for (int a = 0; a < numSideBCPoints - 1; a++)
                            {
                                orderedPoints[orderedPointsIndex++] = sideBCShapes[j].orderedPoints[a];
                            }
                        }
                        if (k == sideCAShapes.Count)
                        {
                            orderedPoints[orderedPointsIndex++] = points[pointCIndex];
                        }
                        else
                        {
                            for (int a = 0; a < numSideCAPoints - 1; a++)
                            {
                                orderedPoints[orderedPointsIndex++] = sideCAShapes[k].orderedPoints[a];
                            }
                        }
                        if (IsShapeConvex(orderedPoints))
                        {
                            shapes.Add(new Shape(orderedPoints));
                        }
                    }
                }
            }

            return shapes;
        }

    }
}
