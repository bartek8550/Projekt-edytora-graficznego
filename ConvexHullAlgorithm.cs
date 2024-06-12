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
        // Metoda do znalezienia punktu startowego
        public static Point FindStartingPoint(List<Point> points)
        {
            return points.Aggregate((currentMinPoint, point) => point.Y < currentMinPoint.Y || (point.Y == currentMinPoint.Y && point.X < currentMinPoint.X)
                ? point : currentMinPoint);
        }

        // Metoda do obliczania kątów i sortowania punktów
        public static List<Point> SortPointsByAngle(List<Point> points, Point p0)
        {
            return points
                .Select(point => new { Point = point, Angle = Math.Atan2(point.Y - p0.Y, point.X - p0.X) })
                .OrderBy(p => p.Angle)
                .Select(p => p.Point)
                .ToList();
        }

        // Metoda zwracająca listę posortowanych punktów bez duplikatów 
        public static List<Point> SortPoints(List<Point> points, Point p0)
        {
            //Wykonanie metody do sortowania punktów po kącie
            var sortedPoints = SortPointsByAngle(points, p0);
            //Sprawdzenie punktów o takim samym koncie i ustawienie ich w kolejności w zależności od odległości od punktu P0
            return sortedPoints
                .Aggregate(new List<Point>(), (result, point) => {
                    if (result.Count < 2)
                    {
                        result.Add(point);
                    }
                    else
                    {
                        //Pobranie ostatniego punktu z listy
                        var lastPoint = result.Last();
                        //Policzenie kątów dla sprawdzanych punktów
                        var lastAngle = Math.Atan2(lastPoint.Y - p0.Y, lastPoint.X - p0.X);
                        var currentAngle = Math.Atan2(point.Y - p0.Y, point.X - p0.X);
                        //Sprawdzenie czy punkty mają taki sam kąt albo kąt jest mniejszy od błędu pomiaru
                        if (Math.Abs(lastAngle - currentAngle) < Double.Epsilon)
                        {
                            //Wykorzystanie metryki Manhattan do zbadania odległości między punktami, a punktem P0
                            var lastDistance = Math.Abs(lastPoint.X - p0.X) + Math.Abs(lastPoint.Y - p0.Y);
                            var currentDistance = Math.Abs(point.X - p0.X) + Math.Abs(point.Y - p0.Y);

                            /*
                             * Jeśli dystans aktualnie sprawdzanego punktu jest większy od ostatniego punktu 
                             * to aktualnie sprawdzany punkt zastępuje ostatni punkt 
                            */
                            if (currentDistance > lastDistance)
                            {
                                result[result.Count - 1] = point;
                            }
                        }
                        //Jeśli kąty sprawdzanych punktów są większe to dodajemy punkt od razu do listy
                        else
                        {
                            result.Add(point);
                        }
                    }
                    return result;
                });
        }

        public static int Ccw(Point a, Point b, Point c)
        {
            //Wyprowadzony wzór na podwójne pole trójkąta
            return (b.X - a.X) * (c.Y - a.Y) - (c.X - a.X) * (b.Y - a.Y);
        }

        public static List<Point> GrahamScan(List<Point> points)
        {
            //Znalezienie punktu startowego
            Point p0 = FindStartingPoint(points);
            //sortowanie punktów
            List<Point> sortedPoints = SortPoints(points, p0);
            //Tworzenie nowego stosu
            Stack<Point> stack = new Stack<Point>();
            //Pętla po wszystkich punktach
            foreach (var point in sortedPoints)
            {
                //Sprawdzenie czy na stosie jest więcej niż dwa elementy
                //jeśli tak to sprawdzamy czy Ccw jest mniejsze bądź równe 0 bo wtedy oznacza to, że jest skręt w prawo
                //należy wtedy usunąć ostatni element ze stosu 
                //jeśli nie to dodajemy punkt do stosu
                while (stack.Count > 1 && Ccw(stack.ElementAt(1), stack.ElementAt(0), point) <= 0) {
                    stack.Pop();
                }
                stack.Push(point);
            }
            //Zwrócenie Listy punktów do stworzenia otoczki wypukłej 
            return new List<Point>(stack);
        }
    }
}
