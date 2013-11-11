using System;
using System.Collections.Generic;
using System.Text;
using Sketch;

namespace PrimitiveGrouper
{
    public class TimeGrouper
    {
        private double thresholdDistance;
        public TimeGrouper()
        {
            // Do nothing for now
        }
        public void groupSketch(Sketch.Sketch sketch)
        {
            computeThreshold(sketch);

            List<Substroke> subs = sketch.SubstrokesL;
            subs.Sort();

            int lastStart = 0;
            List<Substroke> currentGroup = new List<Substroke>();
            currentGroup.Add(subs[0]);
            Point endPoint1 = subs[0].Endpoints[0];
            Point endPoint2 = subs[0].Endpoints[1];
            for (int i = 1; i < subs.Count; i++)
            {
                bool close1 = close(endPoint1, subs[i].Endpoints[0]);
                bool close2 = close(endPoint1, subs[i].Endpoints[1]);
                bool close3 = close(endPoint2, subs[i].Endpoints[0]);
                bool close4 = close(endPoint2, subs[i].Endpoints[1]);

                if (close1 || close2 || close3 || close4) // Something is touching.
                {
                    currentGroup.Add(subs[i]);
                    if (close1 && close4 || close2 && close3) // Both endpoints are touching- complete loop.
                    {
                        makeShape(currentGroup, sketch);
                        currentGroup.Clear();
                        i++;
                        if (i < subs.Count)
                        {
                            lastStart = i;
                            currentGroup.Add(subs[i]);
                            endPoint1 = subs[i].Endpoints[0];
                            endPoint2 = subs[i].Endpoints[1];
                        }
                    }
                    else // Only one endpoint is touching- continue the group.
                    {
                        if (close1)
                            endPoint1 = subs[i].Endpoints[1];
                        else if (close2)
                            endPoint1 = subs[i].Endpoints[0];
                        else if (close3)
                            endPoint2 = subs[i].Endpoints[1];
                        else
                            endPoint2 = subs[i].Endpoints[0];
                    }

                }
                else // Nothing is touching. Start over at the next stroke.
                {
                    lastStart++;
                    i = lastStart;
                    currentGroup.Clear();
                    currentGroup.Add(subs[i]);
                    endPoint1 = subs[i].Endpoints[0];
                    endPoint2 = subs[i].Endpoints[1];
                }
            }
        }
        private void computeThreshold(Sketch.Sketch sketch)
        {
            double totaldistance = 0;
            int totalpoints = 0;
            foreach(Substroke sub in sketch.SubstrokesL)
                for (int i = 0; i < sub.Points.Length - 1; i++)
                {
                    totaldistance += sub.Points[i].distance(sub.Points[i + 1]);
                    totalpoints++;
                }
            thresholdDistance = totaldistance / totalpoints * 15;
        }
        private void computeThreshold(Substroke sub1)
        {
            if (sub1.XmlAttrs.Classification == "Line")
                thresholdDistance = new LineSegment(sub1.Points).LineLength / 5;
            else if (sub1.XmlAttrs.Classification == "Arc")
                thresholdDistance = new ArcSegment(sub1.Points).ArcLength / 7;
            else
                thresholdDistance = new Circle(sub1.Points).Circumference / 7;
        }

        private bool close(Point pt1, Point pt2)
        {
            if (pt1.distance(pt2) < thresholdDistance)
                return true;
            return false;
        }

        private void makeShape(List<Substroke> subs, Sketch.Sketch sketch)
        {
            Shape shape = new Shape();
            foreach(Substroke sub in subs)
                shape.AddSubstroke(sub);
            sketch.AddShapeByID(shape);
        }
    }

    public class PrimitiveGrouper
    {
        private double thresholdDistance;

        public PrimitiveGrouper()
        {
            // Nothing here for now.
        }

        public void groupSketch(Sketch.Sketch sketch)
        {
            computeThreshold(sketch);
            List<List<Substroke>> toMerge = new List<List<Substroke>>();
            foreach(Substroke sub1 in sketch.Substrokes)
                foreach(Substroke sub2 in sketch.Substrokes)
                    if (sub1 != sub2)
                        if (belongTogether(sub1, sub2))
                        {
                            List<Substroke> temp = new List<Substroke>();
                            temp.Add(sub1);
                            temp.Add(sub2);
                            toMerge.Add(temp);
                        }
            foreach (List<Substroke> strokePair in toMerge)
            {
                if (strokePair[0].ParentShapes.Count == 0)
                    if (strokePair[1].ParentShapes.Count == 0)
                    {
                        Shape shape = new Shape();
                        shape.AddSubstroke(strokePair[0]);
                        shape.AddSubstroke(strokePair[1]);
                        sketch.AddShapeByID(shape);
                    }
                    else
                        strokePair[1].ParentShapes[0].AddSubstrokeByID(strokePair[0]);
                else
                    if (strokePair[1].ParentShapes.Count == 0)
                        strokePair[0].ParentShapes[0].AddSubstrokeByID(strokePair[1]);
                    else
                        if(strokePair[0].ParentShapes[0] != strokePair[1].ParentShapes[0])
                            sketch.mergeShapes(strokePair[0].ParentShapes[0], strokePair[1].ParentShapes[0]);
            }
        }

        private void computeThreshold(Sketch.Sketch sketch)
        {
            double totaldistance = 0;
            int totalpoints = 0;
            foreach (Substroke sub in sketch.SubstrokesL)
                for (int i = 0; i < sub.Points.Length - 1; i++)
                {
                    totaldistance += sub.Points[i].distance(sub.Points[i + 1]);
                    totalpoints++;
                }
            thresholdDistance = totaldistance / totalpoints * 50;
        }

        private bool close(Point pt1, Point pt2)
        {
            if (pt1.distance(pt2) < thresholdDistance)
                return true;
            return false;
        }

        private bool belongTogether(Substroke sub1, Substroke sub2)
        {
            if (sub1.XmlAttrs.Classification == "Arc")
            {
                ArcSegment fit1 = new ArcSegment(sub1.Points);
                if (sub2.XmlAttrs.Classification == "Arc")
                {
                    ArcSegment fit2 = new ArcSegment(sub2.Points);
                    if (lineUpArcs(fit1, fit2))
                        return true;
                }
                else if (sub2.XmlAttrs.Classification == "Line")
                {
                    LineSegment fit2 = new LineSegment(sub2.Points);
                    if (lineUpArcLine(fit1, fit2))
                        return true;
                }
                else if (sub2.XmlAttrs.Classification == "Circle")
                {
                    Circle fit2 = new Circle(sub2.Points);
                    if (lineUpArcCircle(fit1, fit2))
                        return true;
                }
            }
            else if (sub1.XmlAttrs.Classification == "Circle")
            {
                Circle fit1 = new Circle(sub1.Points);
                if (lineUpCircleOther(fit1, sub2))
                    return true;
            }
            return false;
        }

        private bool lineUpArcs(ArcSegment fit1, ArcSegment fit2)
        {
            bool close1 = close(fit1.Points[0], fit2.Points[0]) && close(fit1.Points[fit1.Points.Length - 1], fit2.Points[fit2.Points.Length - 1]);
            bool close2 = close(fit1.Points[0], fit2.Points[fit2.Points.Length - 1]) && close(fit1.Points[fit1.Points.Length - 1], fit2.Points[0]);

            // Either the arcs touch at the endpoints OR they are the paralell arcs of the XOR
            if (!(close1 || close2 || Math.Abs(fit1.SweepAngle - fit2.SweepAngle) < 30))
                return false;

            // Check that they are facing the same way.
            double centerAngle1 = ((fit1.StartAngle + fit1.SweepAngle / 2 + 180) % 180) / 360 * 2 * Math.PI;
            double centerAngle2 = ((fit2.StartAngle + fit2.SweepAngle / 2 + 180) % 180) / 360 * 2 * Math.PI;

            if (!(Math.Abs(centerAngle1 - centerAngle2) < Math.PI / 6 || Math.Abs(centerAngle1 - centerAngle2) > 5 * Math.PI /6))
                return false;

            // Make sure that they are close to on the same axis.
            float y = fit1.CenterPoint.Y + (float) Math.Tan(centerAngle1) * (fit2.CenterPoint.X - fit1.CenterPoint.X);

            Point pt1 = new Point(fit2.CenterPoint.X, fit2.CenterPoint.Y);
            Point pt2 = new Point(fit2.CenterPoint.X, y);

            double length = Math.Max(fit1.ArcLength, fit2.ArcLength);

            if (pt1.distance(pt2) > length / 6)
                return false;

            // Make sure they are not too far away from eachother.

            Point pt3 = fit1.Points[fit1.Points.Length / 2];
            Point pt4 = fit2.Points[fit2.Points.Length / 2];

            if (pt3.distance(pt2) > 3 * length / 4)
                return false;


            return true;
        }

        private bool lineUpArcLine(ArcSegment fit1, LineSegment fit2)
        {
            if (fit2.LineLength > fit1.ArcLength)
                return false;

            bool close1 = close(fit1.Points[0], fit2.StartPoint) && close(fit1.Points[fit1.Points.Length - 1], fit2.EndPoint);
            bool close2 = close(fit1.Points[0], fit2.EndPoint) && close(fit1.Points[fit1.Points.Length - 1], fit2.StartPoint);

            if (!(close1 || close2))
                return false;

            double centerAngle1 = ((fit1.StartAngle + fit1.SweepAngle / 2 + 180) % 180) / 360 * 2 * Math.PI;
            double centerAngle2 = Math.Atan(fit2.Slope) + Math.PI / 2;

            if (centerAngle2 < 0)
                centerAngle2 += Math.PI;
            else if (centerAngle2 > Math.PI)
                centerAngle2 -= Math.PI;

            if (!(Math.Abs(centerAngle1 - centerAngle2) < Math.PI / 6 || Math.Abs(centerAngle1 - centerAngle2) > 5 * Math.PI / 6 ))
                return false;

            float y = fit1.CenterPoint.Y + (float)Math.Tan(centerAngle1) * ((fit2.StartPoint.X + fit2.EndPoint.X) / 2 - fit1.CenterPoint.X);

            Point pt2 = new Point((fit2.StartPoint.X + fit2.EndPoint.X) / 2, (fit2.StartPoint.Y + fit2.EndPoint.Y) / 2);
            Point pt3 = new Point(pt2.X, y);

            if (pt3.distance(pt2) < fit2.LineLength/4)
                return true;

            return false;
        }

        private bool lineUpArcCircle(ArcSegment fit1, Circle fit2)
        {
            double centerAngle1 = ((fit1.StartAngle + fit1.SweepAngle / 2 + 180) % 180) / 360 * 2 * Math.PI;

            float y = fit1.CenterPoint.Y + (float)Math.Tan(centerAngle1) * (fit2.Center.X - fit1.CenterPoint.X);

            Point pt1 = new Point(fit1.CenterPoint.X, fit1.CenterPoint.Y);
            Point pt2 = fit2.Center;
            Point pt3 = new Point(pt2.X, y);

            if (Math.Abs(pt1.distance(pt2) - fit1.Radius - fit2.Radius) < 2 * fit2.Radius && pt2.distance(pt3) < 2 * fit2.Radius)
                return true;

            return false;
        }

        private bool lineUpCircleOther(Circle fit1, Substroke sub2)
        {
            if (fit1.Points.Length > sub2.Points.Length)
                return false;

            if (sub2.Points[sub2.Points.Length / 2].distance(fit1.Center) > 2 * fit1.Radius)
                return false;

            return true;
        }
    }
}
