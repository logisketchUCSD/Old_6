using System;
using System.Collections.Generic;
using System.Text;

using Sketch;
using MathNet.Numerics;

namespace OldRecognizers
{
    /// <summary>
    /// Implementation of the $N recognizer
    /// </summary>
    public class NDollar
    {
        private int bb_size;
        private int num_points;
        Dictionary<string, List<Substroke>> templates;

        private const double SEARCH_ANGLE = Math.PI / 4.0; // search +- this angle
        private const double SEARCH_INCREMENT = Math.PI / 90.0; // resolution of the golden section search
        private const double ONE_D_RATIO = 0.3;
        private const int I = 12;  // initial angle = angle between Point-0 and Point-I
        private const double PHI = Math.PI / 6.0; // maximum difference between start angles

        #region constructors
        /// <summary>
        /// Defaults for bounding box: 250x250
        /// Defaults for points per stroke: 96
        /// Defaults taken from http://depts.washington.edu/aimgroup/proj/dollar/ndollar.js
        /// </summary>
        public NDollar()
            : this(250, 96) { }

        public NDollar(int bb, int num)
        {
            bb_size = bb;
            num_points = num;
            templates = new Dictionary<string, List<Substroke>>();
        }
        #endregion

        #region Training
        /// <summary>
        /// Add one example of a class to the templates
        /// </summary>
        /// <param name="Class">name of the class</param>
        /// <param name="example">training example</param>
        public void addExample(string Class, Shape example)
        {
            addExampleWorker(Class, example.SubstrokesL);
        }

        public void addExample(string Class, Substroke example)
        {
            addExampleWorker(Class, new List<Substroke>(new Substroke[] { example }));
        }

        /// <summary>
        /// this function does the work for the public overloaded addExample
        /// </summary>
        /// <param name="Class"></param>
        /// <param name="example"></param>
        private void addExampleWorker(string Class, List<Substroke> example)
        {
            string ClassL = Class.ToLower();
            if (!templates.ContainsKey(ClassL))
            {
                templates.Add(ClassL, new List<Substroke>());
            }

            List<List<Substroke>> allPossibilities = new List<List<Substroke>>();
            generatePermutations(example, new List<Substroke>(), ref allPossibilities);
            foreach (List<Substroke> possibility in allPossibilities)
            {
                templates[ClassL].Add(StepTwo(Resample(possibility)));
            }

        }
        /// <summary>
        /// Add multiple examples of a class to the templates
        /// </summary>
        /// <param name="Class">number of the class</param>
        /// <param name="examples">list of example substrokes</param>
        public void addExamples(string Class, List<Substroke> examples)
        {
            foreach (Substroke s in examples) addExample(Class, s);
        }

        public void addExamples(string Class, List<Shape> examples)
        {
            foreach (Shape example in examples) addExample(Class, example);
        }

        /// <summary>
        /// Add preprocessed examples to a class -- this is useful when you 
        /// want to create 2 recognizers with the same (or partially overlapping)
        /// training sets.  Just copy the templates out of one and into the other.
        /// </summary>
        /// <param name="Class"></param>
        /// <param name="examples"></param>
        public void addProcessedExamples(string Class, List<Substroke> examples)
        {
            if (!templates.ContainsKey(Class))
                templates.Add(Class, new List<Substroke>());
            if (examples != null)
                templates[Class].AddRange(examples);
            else
                Console.WriteLine("warning: nothing added this time for " + Class);
        }

        /// <summary>
        /// Get the processed examples for a class.  For use with addProcessedExamples.
        /// </summary>
        /// <param name="Class"></param>
        /// <returns></returns>
        public List<Substroke> getProcessedExamples(string Class)
        {
            if (templates.ContainsKey(Class)) return templates[Class];
            return null;
        }
        #endregion

        #region Helpers
        /// <summary>
        /// generate all permutations of STROKES.  Store result in PERMUTATIONS.
        /// </summary>
        /// <param name="strokes"></param>
        /// <param name="permutations"></param>
        void generatePermutations(List<Substroke> strokes, List<Substroke> current, ref List<List<Substroke>> permutations)
        {
            if (strokes.Count == 0)
            {
                // there is weird pointer stuff going on here.  If we just use
                // current here, then all entries in permutations will be the
                // same pointer.  However, it is ok if every instance of any
                // substroke points to the same object, because they are not
                // modified in the course of recognition.
                List<Substroke> toAdd = new List<Substroke>();
                toAdd.AddRange(current);
                permutations.Add(toAdd);
                return;
            }

            for (int i = 0; i < strokes.Count; ++i)
            {
                Substroke s = strokes[i];
                strokes.RemoveAt(i);
                current.Add(s);
                generatePermutations(strokes, current, ref permutations);
                current.Remove(s);

                Substroke sr = s.GetReversed();
                current.Add(sr);
                generatePermutations(strokes, current, ref permutations);
                current.Remove(sr);


                strokes.Insert(i, s);
            }
        }
        #endregion

        #region Step 1: Resample

        /// <summary>
        /// Resample a group of substrokes to contain num_points and put all those points in 
        /// into one Substroke.
        /// </summary>
        /// <param name="ls"></param>
        /// <returns></returns>
        public Substroke Resample(List<Substroke> ls)
        {
            ulong counter = 0;
            Substroke res = new Substroke();
            res.XmlAttrs.Id = Guid.NewGuid();
            res.XmlAttrs.Name = "substroke";
            res.XmlAttrs.Type = "substroke";
            res.XmlAttrs.Time = (ulong)DateTime.Now.Ticks;

            foreach (Substroke s in ls)
            {
                foreach (Point p in s.PointsL)
                {
                    ++counter;
                    p.Time = counter;
                    res.AddPoint(p);
                }
            }

            res.ResampleInPlace(num_points);

            return res;
        }

        #endregion

        #region Steps 2 & 3: center, rotate, scale
        /// <summary>
        /// move centroid to origin, rotate to indicative angle, scale to correct bounding box size
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        public Substroke StepTwo(Substroke s)
        {
            if (s.XmlAttrs.Width == null || s.XmlAttrs.Height == null) s.UpdateAttributes();
            double[] c = s.Centroid;
            double cx = c[0];
            double cy = c[1];
            double theta = -Math.Atan2(cy - s.PointsL[0].Y, cx - s.PointsL[0].X);

            double cosT = Math.Cos(theta);
            double sinT = Math.Sin(theta);

            double XLen = s.XmlAttrs.Width.Value;  // bounding box width
            double YLen = s.XmlAttrs.Height.Value; // bounding box height
            double Sx, Sy; // scale factors

            if (Math.Min(XLen / YLen, YLen / XLen) <= ONE_D_RATIO)
            {
                Sx = Sy = bb_size / Math.Max(XLen, YLen);
            }
            else
            {
                Sx = bb_size / XLen;
                Sy = bb_size / YLen;
            }

            List<Point> spc = new List<Point>();

            foreach (Point sp in s.PointsL)
            {
                Point n = new Point();
                // this computation comes from affine transforms.
                // first translate centroid to origin, then rotate theta radians around origin, then scale
                n.X = (float)(Sx * (sp.X * cosT - sp.Y * sinT + cy * sinT - cx * cosT));
                n.Y = (float)(Sy * (sp.X * sinT + sp.Y * cosT - cx * sinT - cy * cosT));
                n.XmlAttrs.Id = Guid.NewGuid();
                n.XmlAttrs.Time = (ulong)DateTime.Now.Ticks;
                spc.Add(n);
            }

            Substroke res = new Substroke(spc, new XmlStructs.XmlShapeAttrs(true));
            res.XmlAttrs.Name = "substroke";
            res.XmlAttrs.Type = "substroke";
            res.XmlAttrs.Time = (ulong)DateTime.Now.Ticks;

            return res;
        }

        #endregion

        #region Golden Section Search
        /// <summary>
        /// Finds the distance between two substrokes at the angle where it is 
        /// minimized.
        /// </summary>
        /// <param name="test">Substroke A</param>
        /// <param name="template">Substroke B</param>
        /// <param name="t0">Minimum angle</param>
        /// <param name="t1">Maximum angle</param>
        /// <param name="ti">Angle increment (search resolution)</param>
        /// <returns></returns>
        public static double distanceAtBestAngle
            (Substroke test, Substroke template, double t0, double t1, double ti)
        {
            double phi = 0.5 * (Math.Sqrt(5) - 1);

            double x1 = phi * t0 + (1 - phi) * t1;
            double f1 = DistanceAtAngle(test, template, x1);

            double x2 = (1 - phi) * t0 + phi * t1;
            double f2 = DistanceAtAngle(test, template, x2);

            while (Math.Abs(t0 - t1) > ti)
            {
                if (f1 < f2)
                {
                    t1 = x2;
                    x2 = x1;
                    f2 = f1;
                    x1 = phi * t0 + (1 - phi) * t1;
                    f1 = DistanceAtAngle(test, template, x1);
                }
                else
                {
                    t0 = x1;
                    x1 = x2;
                    f1 = f2;
                    x2 = (1 - phi) * t0 + phi * t1;
                    f2 = DistanceAtAngle(test, template, x2);
                }
            }

            return Math.Min(f1, f2);
        }

        /// <summary>
        /// Rotates Substroke A and calculates distance to Substroke B.
        /// </summary>
        /// <param name="test">Substroke A</param>
        /// <param name="template">Substroke B</param>
        /// <param name="angle">angle to rotate in radians</param>
        /// <returns></returns>
        private static double DistanceAtAngle(Substroke test, Substroke template, double angle)
        {
            double[] c = test.Centroid;
            List<Point> rotated = test.cloneRotate(angle, (float)c[0], (float)c[1]).PointsL;
            double res = 0d;
            for (int i = 0; i < rotated.Count; ++i)
            {
                res += rotated[i].distance(template.PointsL[i]);
            }
            return res / rotated.Count;
        }
        #endregion

        #region Recognition
        /// <summary>
        /// Classify given substroke
        /// </summary>
        /// <param name="test"></param>
        /// <returns></returns>
        public string classify(Substroke test)
        {
            Substroke stepped = StepTwo(test.Resample(num_points));
            return doSearch(stepped);
        }
        public string classify(Shape test)
        {
            Substroke stepped = StepTwo(Resample(test.SubstrokesL));
            return doSearch(stepped);
        }
        public string classify(Shape test, out double score)
        {
            Substroke stepped = StepTwo(Resample(test.SubstrokesL));
            return doSearch(stepped, out score);
        }

        /// <summary>
        /// Do the actual search here
        /// </summary>
        /// <param name="test">processed substroke</param>
        /// <returns></returns>
        private string doSearch(Substroke test)
        {
            double minD = double.MaxValue;
            string minC = null;
            Point testIA = initialAngle(test);

            foreach (string Class in templates.Keys)
            {
                foreach (Substroke template in templates[Class])
                {
                    if (initialAngleDistance(testIA, initialAngle(template)) > PHI) 
                        continue;
                    double testD =
                        distanceAtBestAngle(test, template, -(SEARCH_ANGLE), SEARCH_ANGLE, SEARCH_INCREMENT);
                    if (testD < minD)
                    {
                        minD = testD;
                        minC = Class;
                    }
                }
            }
            return minC;
        }

        /// <summary>
        /// Calculate the intial angle of a stroke.  Uses the global
        /// constant I to determine how far to look.
        /// </summary>
        /// <param name="stroke"></param>
        /// <returns></returns>
        private Point initialAngle(Substroke stroke)
        {
            double qx = stroke.PointsL[I].X - stroke.PointsL[0].X;
            double qy = stroke.PointsL[I].Y - stroke.PointsL[0].Y;
            double x = qx / Math.Sqrt(qx * qx + qy * qy);
            double y = qy / Math.Sqrt(qx * qx + qy * qy);

            return new Point((float)x, (float)y);
        }

        /// <summary>
        /// Calculate the angle between the initial angles of 
        /// two strokes.
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        private double initialAngleDistance(Point a, Point b)
        {
            double test = a.X * b.X + a.Y * b.Y;

            // sometimes these two cases can happen because of rounding error.
            if (test < -1.0)
                return Math.PI;
            if (test > 1.0)
                return 0;
            return Math.Acos(test);
        }

        private string doSearch(Substroke test, out double score)
        {
            double minD = double.MaxValue;
            string minC = null;
            double b = 0.0;
            double half_Diagonal = 0.5 * Math.Sqrt(2.0 * Math.Pow((double)bb_size, 2.0));

            foreach (string Class in templates.Keys)
            {
                foreach (Substroke template in templates[Class])
                {
                    double testD =
                        distanceAtBestAngle(test, template, -(SEARCH_ANGLE), SEARCH_ANGLE, SEARCH_INCREMENT);
                    if (testD < minD)
                    {
                        minD = testD;
                        minC = Class;
                        b = 1.0 - testD / half_Diagonal;
                    }
                }
            }

            score = b;
            return minC;
        }
        #endregion
    }
}
