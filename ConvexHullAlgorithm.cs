using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Threading.Tasks;
using Emgu.CV;
using Emgu.CV.Structure;
using Emgu.CV.Util;

namespace Projekt_edytora_graficznego
{
    public class ConvexHullAlgorithm
    {
        // funkcja znajdująca punkt P0
        public static Point FindStartingPoint(List<Point> points)
        {
            //Na razie najmniejszym punktem będzie pierwszy punkt w tablicy
            Point currentMinPoint = points[0];
            // iterowanie po wszystkich punktach
            foreach (var point in points)
            {
                if (point.Y < currentMinPoint.Y)
                {
                    // zapamiętywany jest punkt o mniejszej współrzędnej Y
                    currentMinPoint = point;
                }
                else if (point.Y == currentMinPoint.Y)
                {
                    // jeśli Y są równe, brany jest pod uwagę ten, który ma mniejsze X
                    if (point.X < currentMinPoint.X)
                    {
                        currentMinPoint = point;
                    }
                }
            }
            // znaleziony punkt P0 zwracamy
            return currentMinPoint;
        }

        // funkcja zwracająca posortowane punkty z odrzuceniem duplikatów
        public static List<Point> SortPoints(List<Point> points, Point p0)
        {
            // punkty sortowane według kąta do P0
            var sortedPoints = new List<Point>(points);
            sortedPoints.Sort((a, b) =>
            {
                double angleA = Math.Atan2(a.Y - p0.Y, a.X - p0.X);
                double angleB = Math.Atan2(b.Y - p0.Y, b.X - p0.X);
                return angleA.CompareTo(angleB);
            });

            var result = new List<Point>();
            foreach (var point in sortedPoints)
            {
                if (result.Count < 2)
                {
                    result.Add(point);
                }
                else
                {
                    var lastPoint = result[result.Count - 1];
                    var lastAngle = Math.Atan2(lastPoint.Y - p0.Y, lastPoint.X - p0.X);
                    var currentAngle = Math.Atan2(point.Y - p0.Y, point.X - p0.X);

                    if (Math.Abs(lastAngle - currentAngle) < Double.Epsilon)
                    {
                        var lastDistance = Math.Abs(lastPoint.X - p0.X) + Math.Abs(lastPoint.Y - p0.Y);
                        var currentDistance = Math.Abs(point.X - p0.X) + Math.Abs(point.Y - p0.Y);
                        if (currentDistance > lastDistance)
                        {
                            result[result.Count - 1] = point;
                        }
                    }
                    else
                    {
                        result.Add(point);
                    }
                }
            }
            return result;
        }

        public static int Ccw(Point a, Point b, Point c)
        {
            return (b.X - a.X) * (c.Y - a.Y) - (c.X - a.X) * (b.Y - a.Y);
        }

        public static Point NextToTop(Stack<Point> stack)
        {
            Point top = stack.Pop();
            Point nextToTop = stack.Peek();
            stack.Push(top);
            return nextToTop;
        }

        public static Point Top(Stack<Point> stack)
        {
            return stack.Peek();
        }

        public static List<Point> GrahamScan(List<Point> points)
        {
            if (points.Count < 2)
            {
                return new List<Point>();
            }

            Point p0 = FindStartingPoint(points);
            List<Point> sortedPoints = SortPoints(points, p0);

            Stack<Point> stack = new Stack<Point>();
            foreach (var point in sortedPoints)
            {
                while (stack.Count > 1 && Ccw(NextToTop(stack), Top(stack), point) <= 0)
                {
                    stack.Pop();
                }
                stack.Push(point);
            }
            return new List<Point>(stack);
        }
    }
}
