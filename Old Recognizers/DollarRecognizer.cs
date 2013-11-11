using System;
using System.Collections.Generic;
using System.Text;

using Sketch;
using MathNet.Numerics;

namespace OldRecognizers
{
    /// <summary>
    /// Implementation of multi-stroke $1 recognizer.
    /// 
    /// Also has some extra stuff to do Hausdorff distance comparisons that can be ignored.
    /// </summary>
    public class DollarRecognizer
    {

        private int bb_size;
        private int num_points;
        Dictionary<string, List<Substroke>> templates;

        // constants for simulated annealing
        private const double STARTING_TEMPERATURE = 1;
        private const double EXPONENTIAL_DROPOFF_RATE = 10;

        // for golden section
        private const double SEARCH_ANGLE = Math.PI / 4.0; // search +- this angle
        private double SEARCH_INCREMENT; // for default see default constructor

        // constant for hungarian algorithm
        // used for checking double-equality
        private const double EPSILON = 1.0E-5;

        /// <summary>
        /// delegate used to describe a distance function between two substrokes
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public delegate double distance_function(Substroke a, Substroke b);
        private distance_function df; // for default see default constructor

        /// <summary>
        /// Defaults for bounding box: 64x64
        /// Defaults for points per stroke: 48
        /// </summary>
        public DollarRecognizer()
            : this(64, 48, Math.PI/20.0, new distance_function(SAdistance)) { }

        public DollarRecognizer(int bb, int num, double increment, distance_function d)
        {
            bb_size = bb;
            num_points = num;
            SEARCH_INCREMENT = increment;
            df = d;
            templates = new Dictionary<string, List<Substroke>>();
        }

        /// <summary>
        /// not really a copy constructor, just copy the parameters.
        /// </summary>
        /// <param name="other">DR to copy params from</param>
        public DollarRecognizer(DollarRecognizer other)
            : this(other.bb_size, other.num_points, other.SEARCH_INCREMENT, other.df)
        {
        }

        #region Step 1: Resample

        /// <summary>
        /// Resample a group of substrokes to contain num_points and put all those points in 
        /// into one Substroke.
        /// Note: they might be ordered funny, but it shouldn't matter to the $1 recognizer.
        /// </summary>
        /// <param name="ls"></param>
        /// <returns></returns>
        public Substroke Resample(List<Substroke> ls)
        {
            double totalLength = 0d;

            foreach (Substroke s in ls)
            {
                totalLength += s.SpatialLength;
            }

            Substroke res = new Substroke();
            res.XmlAttrs.Id = Guid.NewGuid();
            res.XmlAttrs.Name = "substroke";
            res.XmlAttrs.Type = "substroke";
            res.XmlAttrs.Time = (ulong)DateTime.Now.Ticks;

            List<Substroke> list = new List<Substroke>();
            double remainder = 0;
            int sum = 0;
            foreach (Substroke s in ls)
            {
                double x = (s.SpatialLength * num_points / totalLength);
                int xi = (int)x;
                remainder += x - xi;
                if (remainder > 1d - (1e-5))
                {
                    remainder = remainder - 1d;
                    xi++;
                }
                if (xi > 0)
                    list.Add(s.Resample(xi));
                sum += xi;
            }

            res.AddSubstrokes(list);
            if (res.PointsL.Count != num_points)
                throw new Exception("Resample failed");
            return res;
        }

        #endregion

        #region Steps 2 & 3: center, rotate, scale
        /// <summary>
        /// center, scale
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        public Substroke StepTwo(Substroke s)
        {
            if (s.XmlAttrs.Width == null || s.XmlAttrs.Height == null) s.UpdateAttributes();
            double[] c = s.Centroid;
            double cx = c[0];
            double cy = c[1];
            // we don't want to rotate for the multi-stroke
            double theta = 0d;

            double cosT = Math.Cos(theta);
            double sinT = Math.Sin(theta);

            double XLen = s.XmlAttrs.Width.Value;
            if (XLen == 0.0) XLen = 1;
            double Sx = bb_size / XLen;
            double YLen = s.XmlAttrs.Height.Value;
            if (YLen == 0.0) YLen = 1;
            double Sy = bb_size / YLen;

            List<Point> spc = new List<Point>();

            foreach (Point sp in s.PointsL)
            {
                Point n = new Point();
                // this computation comes from affine transforms.
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

        #region Training
        /// <summary>
        /// Add one example of a class to the templates
        /// </summary>
        /// <param name="Class">number of the class</param>
        /// <param name="example">example substroke</param>
        public void addExample(string Class, Substroke example)
        {
            string ClassL = Class.ToLower();
            if (!templates.ContainsKey(ClassL))
            {
                templates.Add(ClassL, new List<Substroke>());
            }

            templates[ClassL].Add(StepTwo(example.Resample(num_points)));
        }

        public void addExample(string Class, Shape example)
        {
            string ClassL = Class.ToLower();
            if (!templates.ContainsKey(ClassL))
            {
                templates.Add(ClassL, new List<Substroke>());
            }
            Substroke toadd = StepTwo(Resample(example.SubstrokesL));
            templates[ClassL].Add(toadd);
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
        /// Add preprocessed examples to a class
        /// </summary>
        /// <param name="Class"></param>
        /// <param name="examples"></param>
        public void addProcessedExamples(string Class, List<Substroke> examples)
        {
            Class = Class.ToLower();
            if (!templates.ContainsKey(Class))
                templates.Add(Class, new List<Substroke>());
            if (examples != null)
                templates[Class].AddRange(examples);
            else
                Console.WriteLine("warning: nothing added this time for " + Class);
        }

        /// <summary>
        /// Get the processed examples for a class
        /// </summary>
        /// <param name="Class"></param>
        /// <returns></returns>
        public List<Substroke> getProcessedExamples(string Class)
        {
            if (templates.ContainsKey(Class)) return templates[Class];
            return null;
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
        /// <param name="df">Distance function between 2 substrokes</param>
        /// <returns></returns>
        public double distanceAtBestAngle
            (Substroke test, Substroke template, double t0, double t1, double ti)
        {
            double no_rot = df(test, template);

            double phi = 0.5 * (Math.Sqrt(5) - 1);

            double x1 = phi * t0 + (1 - phi) * t1;
            double f1 = DistanceAtAngle(test, template, x1, df);

            double x2 = (1 - phi) * t0 + phi * t1;
            double f2 = DistanceAtAngle(test, template, x2, df);

            while (Math.Abs(t0 - t1) > ti)
            {
                if (f1 < f2)
                {
                    t1 = x2;
                    x2 = x1;
                    f2 = f1;
                    x1 = phi * t0 + (1 - phi) * t1;
                    f1 = DistanceAtAngle(test, template, x1, df);
                }
                else
                {
                    t0 = x1;
                    x1 = x2;
                    f1 = f2;
                    x2 = (1 - phi) * t0 + phi * t1;
                    f2 = DistanceAtAngle(test, template, x2, df);
                }
            }

            return Math.Min(f1, Math.Min(f2, no_rot));
        }

        /// <summary>
        /// Rotates Substroke A and calculates distance to Substroke B.
        /// </summary>
        /// <param name="test">Substroke A</param>
        /// <param name="template">Substroke B</param>
        /// <param name="angle">angle to rotate in radians</param>
        /// <param name="df">distance function between 2 substrokes</param>
        /// <returns></returns>
        private double DistanceAtAngle(Substroke test, Substroke template, double angle, distance_function df)
        {
            double[] c = test.Centroid;
            double res = df(test.cloneRotate(angle, (float)c[0], (float)c[1]), template);
            return res;
        }
        #endregion

        #region Various Hausdorff Distance Functions
        /// <summary>
        /// Hausdorff Distance
        /// </summary>
        /// <param name="test"></param>
        /// <param name="template"></param>
        /// <returns></returns>
        public static double HDdistance(Substroke test, Substroke template)
        {
            double max = double.MinValue;
            foreach (Point p in test.PointsL)
            {
                double min = double.MaxValue;
                foreach (Point p2 in template.PointsL)
                {
                    double d = p.distance(p2);
                    if (d < min) min = d;
                }
                if (min > max) max = min;
            }
            return max;
        }

        /// <summary>
        /// Modified Hausdorff Distance
        /// </summary>
        /// <param name="test"></param>
        /// <param name="template"></param>
        /// <returns></returns>
        public static double MHDdistance(Substroke test, Substroke template)
        {
            double sum = 0d;
            foreach (Point p in test.PointsL)
            {
                double min = double.MaxValue;
                foreach (Point p2 in template.PointsL)
                {
                    double d = p.distance(p2);
                    if (d < min) min = d;
                }
                sum += min;
            }
            return sum;
        }

        /// <summary>
        /// Partial Hausdorff Distance: ignore top 6%
        /// </summary>
        /// <param name="test"></param>
        /// <param name="template"></param>
        /// <returns></returns>
        public static double PHDdistance(Substroke test, Substroke template)
        {
            List<double> mins = new List<double>();

            foreach (Point p in test.PointsL)
            {
                double min = double.MaxValue;
                foreach (Point p2 in template.PointsL)
                {
                    double d = p.distance(p2);
                    if (d < min) min = d;
                }
                mins.Add(min);
            }

            mins.Sort();
            int n = (int)(0.94 * (double)mins.Count);
            return mins[n];
        }

        /// <summary>
        /// setup function specifically for use with the Hausdorff stuff in UCRDataAnalysis
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        public static Substroke setup(Shape s)
        {
            DollarRecognizer dr = new DollarRecognizer();
            return dr.StepTwo(dr.Resample(s.SubstrokesL));

        }
        #endregion

        #region Simulated Annealing
        /// <summary>
        /// Uses Simulated Annealing to approximate the minimum point-to-point
        /// distance between test and template, where points are matched 1-to-1
        /// </summary>
        /// <param name="test"></param>
        /// <param name="template"></param>
        /// <returns></returns>
        public static double SAdistance(Substroke test, Substroke template)
        {
            Dictionary<Point, List<KeyValuePair<Point, double>>> PtoP =
                new Dictionary<Point, List<KeyValuePair<Point, double>>>();

            // pre-calculate the distance from each point to each other point
            foreach (Point p in test.PointsL)
            {
                PtoP.Add(p, new List<KeyValuePair<Point, double>>());
                foreach (Point p2 in template.PointsL)
                {
                    PtoP[p].Add(new KeyValuePair<Point, double>(p2, p.distance(p2)));
                }

                PtoP[p].Sort(delegate(KeyValuePair<Point, double> a, KeyValuePair<Point, double> b)
                {
                    return a.Value.CompareTo(b.Value);
                });
            }
            double sa = simulateAnnealing(test, template, PtoP);
            return sa;
        }

        /// <summary>
        /// Takes steps towards finding the exact solution to the minimum 
        /// point-to-point distance between test and template.
        /// Takes 'n' steps and runs in O(N^n) where N is the number of points
        /// per substroke
        /// </summary>
        /// <param name="test"></param>
        /// <param name="template"></param>
        /// <param name="PtoP">distances from each point to each other point</param>
        /// <param name="n"></param>
        /// <returns></returns>
        private static double nthBest
            (Substroke test, Substroke template, Dictionary<Point, List<KeyValuePair<Point, double>>> PtoP, int n)
        {
            if (test.PointsL.Count == 0) return 0d;
            Point testP = test.PointsL[test.PointsL.Count - 1];
            test.PointsL.RemoveAt(test.PointsL.Count - 1);

            List<KeyValuePair<Point, double>> topN = new List<KeyValuePair<Point, double>>();

            foreach (KeyValuePair<Point, double> exists in PtoP[testP])
            {
                if (topN.Count >= n) break;
                if (template.PointsL.Contains(exists.Key))
                {
                    topN.Add(exists);
                }
            }

            double min = double.MaxValue;
            foreach (KeyValuePair<Point, double> p in topN)
            {
                template.PointsL.Remove(p.Key);
                double t = p.Value + nthBest(test, template, PtoP, n);
                template.PointsL.Add(p.Key);

                if (t < min) min = t;
            }

            test.PointsL.Add(testP);

            return min;

        }

        /// <summary>
        /// Run Simulated Annealing to find the minimum point-to-point distance
        /// between test and template.
        /// </summary>
        /// <param name="test"></param>
        /// <param name="template"></param>
        /// <param name="PtoP"></param>
        /// <returns></returns>
        private static double simulateAnnealing
            (Substroke test, Substroke template, Dictionary<Point, List<KeyValuePair<Point, double>>> PtoP)
        {
            Random r = new Random();
            int k = -1, kmax = 500;
            double emax = 50, e = 0;
            List<Point> state = new List<Point>();

            double best = double.MaxValue;

            //initialize state to be the greedy solution
            foreach (Point p1 in test.PointsL)
            {
                foreach (KeyValuePair<Point, double> kvp in PtoP[p1])
                {
                    if (template.PointsL.Contains(kvp.Key))
                    {
                        template.PointsL.Remove(kvp.Key); //temporarily destroy template
                        state.Add(kvp.Key);
                        break;
                    }
                }
            }

            // restore template to former glory!
            foreach (Point p in state)
            {
                template.PointsL.Add(p);
            }

            // calculate the current distance between test and template
            e = energy(test, state);

            // main SA loop
            while (++k < kmax && e > emax)
            {
                List<Point> stateNew = neighbor(state);
                double en = energy(test, stateNew);
                if (en < best) best = en;
                if (P(e, en, temp((double)k / (double)kmax)) > r.NextDouble())
                {
                    state = stateNew;
                    e = en;
                }
            }

            return best;
        }

        /// <summary>
        /// Calculates a "probability" that the state with en should be taken
        /// as the current state.  This function taken from Wikipedia article on SA
        /// </summary>
        /// <param name="e">Current state energy</param>
        /// <param name="en">Neighbor candidate state energy</param>
        /// <param name="T">Current temperature</param>
        /// <returns>probability en is better than e [0,1]</returns>
        private static double P(double e, double en, double T)
        {
            if (en < e) return 1d;
            return Math.Exp((e - en) / T);
        }

        /// <summary>
        /// Calculate the "energy" of a state, ie the distance using
        /// the matches defined by the state.
        /// </summary>
        /// <param name="s"></param>
        /// <param name="matches"></param>
        /// <returns></returns>
        private static double energy(Substroke s, List<Point> matches)
        {
            double res = 0d;

            for (int i = 0; i < matches.Count; i++)
            {
                res += s.PointsL[i].distance(matches[i]);
            }

            return res;
        }

        /// <summary>
        /// Temperature function given % completion.
        /// Exponential decay
        /// </summary>
        /// <param name="percent">% completion</param>
        /// <returns></returns>
        private static double temp(double percent)
        {
            return STARTING_TEMPERATURE * Math.Exp(-EXPONENTIAL_DROPOFF_RATE * percent);
        }

        /// <summary>
        /// Generate a neighbor of the current state.
        /// Pick 2 random numbers, swap their matches.
        /// 
        /// Could perhaps be made smarter:
        ///  - fewer iterations needed
        ///  - better convergence?
        /// </summary>
        /// <param name="current">current state</param>
        /// <returns>a new state</returns>
        private static List<Point> neighbor(List<Point> current)
        {
            List<Point> res = new List<Point>(current);
            Random r = new Random();
            int a = r.Next(current.Count);
            int b = r.Next(current.Count);
            while (a == b) b = r.Next(current.Count);

            Point temp = res[a];
            res[a] = res[b];
            res[b] = temp;

            return res;
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

            Shape t = new Shape();
            t.AddSubstroke(test);

            return classify(t);
        }

        public string classify(Shape test)
        {
            double minD = double.MaxValue;
            string minC = null;

            Substroke stepped = StepTwo(Resample(test.SubstrokesL));

            foreach (string Class in templates.Keys)
            {
                foreach (Substroke template in templates[Class])
                {
                    double testD = 
                        distanceAtBestAngle(stepped, template, -SEARCH_ANGLE, SEARCH_ANGLE, SEARCH_INCREMENT);
                    if (testD < minD)
                    {
                        minD = testD;
                        minC = Class;
                    }
                }
            }
            return minC;
        }
        #endregion

        #region Hungarian Algorithm
        /// <summary>
        /// Calculate the distance between test and template using the Hungarian Algorithm
        /// Solution to the Assignment Problem.  This code is adapted from:
        /// http://www.public.iastate.edu/~ddoty/HungarianAlgorithm.html
        /// 
        /// Assumes test and template have the same # of points.
        /// </summary>
        /// <param name="test"></param>
        /// <param name="template"></param>
        /// <returns></returns>
        public static double HADistance(Substroke test, Substroke template)
        {
            List<Point> testPs = test.PointsL, tempPs = template.PointsL;
            double[,] costs = new double[testPs.Count, testPs.Count];


            // possible masks are * and ', or nothing (' ')
            char[,] masks = new char[testPs.Count, testPs.Count];

            // rows and columns can be masked
            bool[] rowCov = new bool[testPs.Count];
            bool[] colCov = new bool[testPs.Count];

            // for step4, step5 interchange
            int z0_r=0, z0_c=0;

            for (int i = 0; i < testPs.Count; ++i)
            {
                rowCov[i] = false;
                colCov[i] = false;
                for (int j = 0; j < tempPs.Count; ++j)
                {
                    costs[i, j] = testPs[i].distance(tempPs[j]);
                    masks[i,j] = ' ';
                }
            }

            // main hungarian algorithm loop
            int stepNum = 1;
            bool done = false;
            while (!done)
            {
                switch (stepNum)
                {
                    case 1:
                        stepNum = HAStep1(ref costs);
                        break;
                    case 2:
                        stepNum = HAStep2(ref costs, ref masks, ref colCov);
                        break;
                    case 3:
                        stepNum = HAStep3(ref masks, ref colCov);
                        break;
                    case 4:
                        stepNum = HAStep4(ref costs, ref masks, ref rowCov, ref colCov,
                            ref z0_r, ref z0_c);
                        break;
                    case 5:
                        stepNum = HAStep5(ref costs, ref masks, ref rowCov, ref colCov,
                            z0_r, z0_c);
                        break;
                    case 6:
                        stepNum = HAStep6(ref costs, ref rowCov, ref colCov);
                        break;
                    case 7:
                        done = true;
                        break;

                }
            }

            double distance = 0.0;
            for (int i = 0; i < masks.GetLength(0); ++i)
            {
                for (int j = 0; j < masks.GetLength(1); ++j)
                {
                    if (masks[i, j] == '*')
                        distance += testPs[i].distance(tempPs[j]);
                }
            }
            return distance;
        }

        #region Steps 1-6
        /// <summary>
        /// For each row, find the smallest element and subtract it from every element in
        /// its row. Go to step 2.
        /// </summary>
        /// <param name="costs">cost matrix</param>
        /// <returns>2</returns>
        private static int HAStep1(ref double[,] costs)
        {
            for (int i = 0; i < costs.GetLength(0); ++i)
            {
                double minval = costs[i, 0];

                for (int j = 1; j < costs.GetLength(1); ++j)
                {
                    if (costs[i, j] < minval)
                        minval = costs[i, j];
                }

                for (int j = 1; j < costs.GetLength(1); ++j)
                {
                    costs[i, j] -= minval;
                }
            }

            return 2;
        }

        /// <summary>
        /// Find a zero (Z) in the resulting matrix.  If there is no starred zero in its
        /// row or column, star Z.  Repeat for each element in the matrix.
        /// Go to step 3.
        /// </summary>
        /// <param name="costs">cost matrix</param>
        /// <param name="masks">element masks</param>
        /// <param name="colCov">are columns covered?</param>
        /// <returns>3</returns>
        private static int HAStep2(ref double[,] costs, ref char[,] masks, ref bool[] colCov)
        {
            for (int i = 0; i < costs.GetLength(0); ++i)
            {
                for (int j = 1; j < costs.GetLength(1); ++j)
                {
                    if (Math.Abs(costs[i, j]) < EPSILON && !colCov[j])
                    {
                        masks[i, j] = '*';
                        colCov[j] = true;
                        break; // no more zeros can be starred in this row -- skip to next row.
                    }
                }
            }

            return 3;
        }

        /// <summary>
        /// Cover each column containing a starred 0.  If all columns are covered, 
        /// the starred zeros describe a complete set of unique assignments.
        /// In this case, go to DONE (7), otherwise to go step 4.
        /// 
        /// Actually, columns with starred 0's are already covered.  Just count and see
        /// if we are done or not.
        /// </summary>
        /// <param name="colCov">are columns covered?</param>
        /// <returns>next step, 4 or 7 (done)</returns>
        private static int HAStep3(ref char[,] masks, ref bool[] colCov)
        {
            int count = 0;

            for (int i = 0; i < masks.GetLength(0); ++i)
            {
                for (int j = 0; j < masks.GetLength(1); ++j)
                {
                    if (masks[i, j] == '*')
                        colCov[j] = true;
                }
            }

            foreach (bool covered in colCov)
                if (covered) ++count;

            if (count == colCov.Length)
                return 7;

            return 4;

        }

        /// <summary>
        /// Find a noncovered zero and prime it('). If there is no starred zero in the
        /// row containing this primed zero, go to step 5.
        /// otherwise, cover this row and uncover the column containing the starred zero.
        /// Continue in this manner until there are no uncovered zeros left.
        /// save the smallest uncovered value and go to step 6.
        /// </summary>
        /// <param name="costs"></param>
        /// <param name="masks"></param>
        /// <param name="rowCov"></param>
        /// <param name="colCov"></param>
        /// <returns></returns>
        private static int HAStep4(ref double[,] costs, ref char[,] masks,
            ref bool[] rowCov, ref bool[] colCov, ref int z0_r, ref int z0_c)
        {
            // we'll just return when we're done, so until then: loop
            int found = 0;
            while (true)
            {
                int row=-1, col=-1;
                if (!findZero(ref costs, ref rowCov, ref colCov, ref row, ref col))
                {
                    return 6;
                }

                ++found;

                masks[row, col] = '\'';

                if (findCharInRow(ref masks, '*', row, ref col))
                {
                    rowCov[row] = true;
                    colCov[col] = false;
                }
                else
                {
                    // step 5 wants this z0
                    z0_r = row;
                    z0_c = col;
                    return 5;
                }
            }
        }

        /// <summary>
        /// Construct a series of alternating primed and starred zeros as follows:
        /// let z0 represent the uncovered primed zero found in step 4.
        /// let z1 denote the starred zero in the column of z0 (if any).
        /// let z2 denote the primed zero in the erow of z1 (there will always be).
        /// Continue until the series terminates at a primed zero that has no starred
        /// zero in its column.  
        /// Unstar each starred zero of the series,
        /// star each primed zero of the series,
        /// erase all primes and uncover every line in the matrix.
        /// return to step 3.
        /// </summary>
        /// <param name="costs">cost matrix</param>
        /// <param name="masks">element masks</param>
        /// <param name="rowCov">covered rows</param>
        /// <param name="colCov">covered cols</param>
        /// <param name="z0_r">row of primed zero</param>
        /// <param name="z0_c">col of primed zero</param>
        /// <returns>3</returns>
        private static int HAStep5(ref double[,] costs, ref char[,] masks,
            ref bool[] rowCov, ref bool[] colCov, int z0_r, int z0_c)
        {
            int row=z0_r, col=z0_c;
            List<Element> series = new List<Element>();
            series.Add(new Element(z0_r, z0_c));

            // we'll just break when we're done.  until then: loop
            while (true)
            {
                if (findCharInCol(ref masks, '*', ref row, col))
                {
                    series.Add(new Element(row, col));
                }
                else
                {
                    break;
                }
                findCharInRow(ref masks, '\'', row, ref col);
                series.Add(new Element(row, col));
            }

            foreach (Element e in series)
            {
                // unstar stars
                if (masks[e.row, e.col] == '*')
                    masks[e.row, e.col] = ' ';
                // star primes (only primes and stars in masks)
                else
                    masks[e.row, e.col] = '*';
            }

            // clear covers and primes
            for (int i = 0; i < masks.GetLength(0); ++i)
            {
                rowCov[i] = false;
                colCov[i] = false;
                for (int j = 0; j < masks.GetLength(1); ++j)
                    if (masks[i, j] == '\'') masks[i, j] = ' ';
            }
            return 3;
        }

        /// <summary>
        /// Add the value found in step 4 to every element of each covered row and
        /// subtract it from every element of each uncovered column.
        /// return to step 4 without altering any stars, primes or covered lines.
        /// </summary>
        /// <param name="costs">cost matrix</param>
        /// <param name="rowCov">covered rows</param>
        /// <param name="colCov">covered columns</param>
        /// <returns>4</returns>
        private static int HAStep6(ref double[,] costs,
            ref bool[] rowCov, ref bool[] colCov)
        {
            double min = findSmallest(ref costs, ref rowCov, ref colCov);

             for (int i = 0; i < costs.GetLength(0); ++i)
                 for (int j = 0; j < costs.GetLength(1); ++j)
                 {
                     // NOT mutually exclusive
                     if (rowCov[i]) costs[i, j] += min;
                     if (!colCov[j]) costs[i, j] -= min;
                 }

             return 4;
         }
        #endregion

        #region HA helper functions
         /// <summary>
        /// Find the smallest element in costs
        /// </summary>
        /// <param name="costs">cost matrix</param>
        /// <returns></returns>
        private static double findSmallest(ref double[,] costs, ref bool[] rowCov, ref bool[] colCov)
        {
            double min = 1.0e100;
            for (int i = 0; i < costs.GetLength(0); ++i)
            {
                if (rowCov[i]) continue;
                for (int j = 0; j < costs.GetLength(1); ++j)
                {
                    if (!colCov[j] && costs[i, j] < min) min = costs[i, j];
                }
            }
            return min;
        }

        /// <summary>
        /// Find an (approximate) 0 in the matrix
        /// </summary>
        /// <param name="costs">matrix</param>
        /// <param name="row">row of 0</param>
        /// <param name="col">column of 0</param>
        /// <returns>true if a 0 is found</returns>
        private static bool findZero(ref double[,] costs, ref bool[] rowCov, ref bool[] colCov,
            ref int row, ref int col)
        {
            for (int i = 0; i < costs.GetLength(0); ++i)
            {
                if (rowCov[i]) continue;
                for (int j = 0; j < costs.GetLength(1); ++j)
                {
                    if (colCov[j]) continue;
                    if (Math.Abs(costs[i, j]) < EPSILON)
                    {
                        row = i;
                        col = j;
                        return true;
                    }
                }
            }

            return false;
        }

        /// <summary>
        /// Finds a masked element in the given row
        /// </summary>
        /// <param name="masks">element masks</param>
        /// <param name="toFind">mask to look for</param>
        /// <param name="row">row to look in</param>
        /// <param name="col">column with element</param>
        /// <returns>true if element is found</returns>
        private static bool findCharInRow(ref char[,] masks, char toFind,
            int row, ref int col )
        {
            for (int i = 0; i < masks.GetLength(1); ++i)
                if (masks[row, i] == toFind)
                {
                    col = i;
                    return true;
                }

            return false;
        }

        /// <summary>
        /// Finds a masked element in the given col
        /// </summary>
        /// <param name="masks">element masks</param>
        /// <param name="toFind">mask to look for</param>
        /// <param name="row">row with element</param>
        /// <param name="col">col to look in</param>
        /// <returns>true if element is found</returns>
        private static bool findCharInCol(ref char[,] masks, char toFind,
            ref int row, int col )
        {
            for (int i = 0; i < masks.GetLength(0); ++i)
                if (masks[i, col] == toFind)
                {
                    row = i;
                    return true;
                }

            return false;
        }

        /// <summary>
        /// Cheap little struct to hold a row,col value
        /// </summary>
        private struct Element
        {
            public int row, col;

            public Element(int r, int c)
            {
                row = r;
                col = c;
            }
        }
        #endregion
        #endregion
    }


}
