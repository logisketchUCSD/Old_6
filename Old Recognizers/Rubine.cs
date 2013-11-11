using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

using Sketch;
using Featurefy;
using MathNet.Numerics.LinearAlgebra;
using ConverterXML;

namespace OldRecognizers
{
    [Serializable]
    public class Rubine
    {
        // number of features
        private const int NFEATS = 13;

        private double[][] _weights;
        private List<string> classes;


        public Rubine()
        {
        }

        /// <summary>
        /// attempt to classify the given shape
        /// </summary>
        /// <param name="test"></param>
        /// <returns></returns>
        public Results classify(Shape test)
        {
            Results r = new Results();
            double[] feats = features(test);
            double[] v = new double[_weights.Length];

            for (int c = 0; c < _weights.Length; c++)
            {
                v[c] = _weights[c][NFEATS];
                for (int i = 0; i < NFEATS; i++)
                    v[c] += _weights[c][i] * feats[i];
            }

            for (int i = 0; i < _weights.Length; i++)
            {
                double p = 0d;
                for (int j = 0; j < _weights.Length; j++)
                {
                    if (i==j)continue;
                    p += Math.Exp(v[j] - v[i]);
                }
                r.Add(classes[i], 1d / p);
            }

            return r;
        }

        /// <summary>
        /// Train the rubine classifier.  Method straight out of paper as are variable names.
        /// </summary>
        /// <param name="data">Dictionary mapping string class names to lists of example shapes for each class</param>
        public void train(Dictionary<string, List<Shape>> data)
        {
            List<string> keys = new List<string>(data.Keys);

            double[][][] f = new double[keys.Count][][];
            double[][] fbar = new double[keys.Count][];
            Matrix[] covar = new Matrix[keys.Count];
            int nexamples = 0;

            for (int c = 0; c < keys.Count; c++)
            {
                List<Shape> examples = data[keys[c]];
                f[c] = new double[examples.Count][];
                fbar[c] = new double[NFEATS];
                covar[c] = new Matrix(NFEATS, NFEATS);
                nexamples += examples.Count;

                for (int e = 0; e < examples.Count; e++)
                {
                    f[c][e] = features(glue(examples[e]));

                    for (int i = 0; i < NFEATS; i++)
                    {
                        fbar[c][i] += f[c][e][i];
                    }
                }
                for (int i = 0; i < NFEATS; i++)
                    fbar[c][i] /= examples.Count;

                for (int i = 0; i < NFEATS; i++)
                    for (int j = 0; j < NFEATS; j++)
                        for (int e = 0; e < examples.Count; e++)
                            covar[c][i,j] += (f[c][e][i]-fbar[c][i])*(f[c][e][j]-fbar[c][j]);
            }

            Matrix common_covar = new Matrix(NFEATS,NFEATS);
            for (int i = 0; i < NFEATS; i++)
                for (int j = 0; j < NFEATS; j++)
                    for (int c = 0; c < keys.Count; c++)
                        common_covar[i,j] += (covar[c][i,j])/(nexamples-keys.Count);

            Matrix common_inverse = common_covar.Inverse();

            double[][] weights = new double[keys.Count][];
            for (int c = 0; c < keys.Count; c++)
            {
                weights[c] = new double[NFEATS+1];

                for (int j = 0; j < NFEATS; j++)
                    for (int i = 0; i < NFEATS; i++)
                        weights[c][j] += common_inverse[i, j] * fbar[c][i];

                for (int i = 0; i < NFEATS; i++)
                    weights[c][NFEATS] += weights[c][i] * fbar[c][i];
                weights[c][NFEATS] *= -0.5;
            }
            classes = keys;
            _weights = weights;
        }

        /// <summary>
        /// calculate the rubine features:
        /// 
        /// alpha: angle made between line from first to third point and the horizontal axis
        /// beta: angle made between line from first to last point and horizontal axis
        /// 
        /// f1: cos(alpha)
        /// f2: sin(alpha)
        /// f3: bounding box diagonal length
        /// f4: bounding box diagonal angle with horizontal
        /// f5: distance from first to last point
        /// f6: cos(beta)
        /// f7: sin(beta)
        /// f8: total length of stroke
        /// f9: sum of angles
        /// f10: sum of |angles|
        /// f11: sum of (angles)^2
        /// f12: max speed
        /// fNFEATS: total time spent drawing stroke
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        private double[] features(Shape s)
        {
            s.UpdateAttributes();
            FeatureStroke[] fs = new FeatureStroke[s.SubstrokesL.Count];
            for (int i = 0; i < s.SubstrokesL.Count; i++)
                fs[i] = new FeatureStroke(s.SubstrokesL[0]);

            double[] res = new double[NFEATS];

            Point temp = fs[0].Points[0].Clone();
            Point topRight = new Point(s.XmlAttrs.X.Value+s.XmlAttrs.Width.Value, s.XmlAttrs.Y.Value);
            Point botLeft = new Point(s.XmlAttrs.X.Value, s.XmlAttrs.Y.Value+s.XmlAttrs.Height.Value);
            Point botRight = new Point(s.XmlAttrs.X.Value+s.XmlAttrs.Width.Value,
                s.XmlAttrs.Y.Value+s.XmlAttrs.Height.Value);
            Point first = fs[0].Points[0];
            Point last = fs[fs.Length - 1].Points[fs[fs.Length - 1].Points.Length - 1];
            temp.X += 100;
            double alpha = double.NaN;
            int fix = 2;
            while (double.IsNaN(alpha) && fs[0].Points.Length > fix)
                alpha = Curvature.cosineRuleAngle(temp, first, fs[0].Points[fix++]);
            if (double.IsNaN(alpha)) alpha = 0;
            double beta = Curvature.cosineRuleAngle
                (temp, first, last);

            res[0] = Math.Cos(alpha);
            res[1] = Math.Sin(alpha);
            res[2] = botLeft.distance(topRight);
            res[3] = Curvature.cosineRuleAngle(botRight, botLeft, topRight);
            res[4] = first.distance(last);
            res[5] = Math.Cos(beta);
            res[6] = Math.Sin(beta);

            res[7] = 0d;
            foreach (Substroke sub in s.SubstrokesL)
                res[7] += sub.SpatialLength;

            res[8] = 0d;
            foreach (FeatureStroke fsub in fs)
                res[8] += fsub.Curvature.TotalAngle;

            res[9] = 0d;
            foreach (FeatureStroke fsub in fs)
                res[9] += fsub.Curvature.TotalAbsAngle;

            res[10] = 0d;
            foreach (FeatureStroke fsub in fs)
                res[10] += fsub.Curvature.TotalSquaredAngle;

            res[11] = double.MinValue;
            foreach (FeatureStroke fsub in fs)
                if (fsub.Speed.MaximumSpeed > res[11])
                    res[11] = fsub.Speed.MaximumSpeed;

            res[12] = last.Time - first.Time;

            return res;
        }

        /// <summary>
        /// calculate the rubine features:
        /// 
        /// alpha: angle made between line from first to third point and the horizontal axis
        /// beta: angle made between line from first to last point and horizontal axis
        /// 
        /// f1: cos(alpha)
        /// f2: sin(alpha)
        /// f3: bounding box diagonal length
        /// f4: bounding box diagonal angle with horizontal
        /// f5: distance from first to last point
        /// f6: cos(beta)
        /// f7: sin(beta)
        /// f8: total length of stroke
        /// f9: sum of angles
        /// f10: sum of |angles|
        /// f11: sum of (angles)^2
        /// f12: max speed
        /// fNFEATS: total time spent drawing stroke
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        private double[] features(Substroke s)
        {
            s.UpdateAttributes();
            FeatureStroke fs = new FeatureStroke(s);

            double[] res = new double[NFEATS];

            Point temp = fs.Points[0].Clone();
            Point topRight = new Point(s.XmlAttrs.X.Value + s.XmlAttrs.Width.Value, s.XmlAttrs.Y.Value);
            Point botLeft = new Point(s.XmlAttrs.X.Value, s.XmlAttrs.Y.Value + s.XmlAttrs.Height.Value);
            Point botRight = new Point(s.XmlAttrs.X.Value + s.XmlAttrs.Width.Value,
                s.XmlAttrs.Y.Value + s.XmlAttrs.Height.Value);
            Point first = fs.Points[0];
            Point last = fs.Points[fs.Points.Length - 1];
            temp.X += 100;
            double alpha = double.NaN;
            int fix = 2;
            while (double.IsNaN(alpha))
                alpha = Curvature.cosineRuleAngle(temp, first, fs.Points[fix++]);
            double beta = Curvature.cosineRuleAngle
                (temp, first, last);

            res[0] = Math.Cos(alpha);
            res[1] = Math.Sin(alpha);
            res[2] = botLeft.distance(topRight);
            res[3] = Curvature.cosineRuleAngle(botRight, botLeft, topRight);
            res[4] = first.distance(last);
            res[5] = Math.Cos(beta);
            res[6] = Math.Sin(beta);

            res[7] = s.SpatialLength;

            res[8] = fs.Curvature.TotalAngle;

            res[9] = fs.Curvature.TotalAbsAngle;

            res[10] = fs.Curvature.TotalSquaredAngle;

            res[11] = fs.Speed.MaximumSpeed;

            res[12] = last.Time - first.Time;

            return res;
        }

        /// <summary>
        /// Glue a Shape together into one substroke.
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        public Substroke glue(Shape s)
        {
            List<Substroke> ssubs = s.SubstrokesL;

            List<Substroke> copy = new List<Substroke>();
            foreach (Substroke sub in ssubs)
                copy.Add(sub.Clone());
            copy.Sort();

            Substroke res = copy[0];
            copy.RemoveAt(0);

            ulong tstd = 100;

            while (copy.Count > 0)
            {
                int[] _ = closest(res, copy);
                switch (_[1])
                {
                    case 0:
                        offset(copy[_[0]], 
                            res.PointsL[res.PointsL.Count-1].Time+tstd - copy[_[0]].PointsL[0].Time);
                        res.AddSubstroke(copy[_[0]]);
                        break;
                    case 1:
                        ulong tdiff = copy[_[0]].PointsL[copy[_[0]].PointsL.Count-1].Time - res.PointsL[0].Time;
                        tdiff += tstd;
                        offset(res, tdiff);
                        res.AddSubstroke(copy[_[0]]);
                        break;
                    case 2:
                        Substroke X = reverse(copy[_[0]]);
                        offset(res, X.PointsL[X.PointsL.Count-1].Time+tstd - res.PointsL[0].Time);
                        res.AddSubstroke(X);
                        break;
                    case 3:
                        Substroke Y = reverse(copy[_[0]]);
                        offset(Y,
                               res.PointsL[res.PointsL.Count - 1].Time + tstd - Y.PointsL[0].Time);
                        res.AddSubstroke(Y);
                        break;
                }
                copy.RemoveAt(_[0]);
            }

            return res;
        }

        /// <summary>
        /// find the closest substroke in b to a
        /// 4 cases:
        /// 1 - a's last point to b's first point
        /// 2 - b's last point to a's first point
        /// 3 - a's first point to b's first point
        /// 3 - a's last point to b's last point
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        private int[] closest(Substroke a, List<Substroke> b)
        {
            Point a1 = a.PointsL[0];
            Point a2 = a.PointsL[a.PointsL.Count - 1];

            double res = double.MaxValue;
            int ind = 0;
            int type = -1;

            for (int i = 0; i < b.Count; i++)
            {
                Point b1 = b[i].PointsL[0];
                Point b2 = b[i].PointsL[b[i].PointsL.Count - 1];

                double a2b1 = a2.distance(b1);
                if (a2b1 < res)
                {
                    res = a2b1;
                    ind = i;
                    type = 0;
                }

                double b2a1 = b2.distance(a1);
                if (b2a1 < res)
                {
                    res = b2a1;
                    ind = i;
                    type = 1;
                }

                double a1b1 = a1.distance(b1);
                if (a1b1 < res)
                {
                    res = a1b1;
                    ind = i;
                    type = 2;
                }

                double a2b2 = a2.distance(b2);
                if (a2b2 < res)
                {
                    res = a2b2;
                    ind = i;
                    type = 3;
                }
            }

            return new int[] { ind, type };
        }

        /// <summary>
        /// Reverse the time order of the points in a substroke
        /// </summary>
        /// <param name="x"></param>
        /// <returns></returns>
        private Substroke reverse(Substroke x)
        {
            Substroke res = x.Clone();

            for (int i = 0; i < x.PointsL.Count/2; i++)
            {
                res.PointsL[i].Time = x.PointsL[x.PointsL.Count - (i + 1)].Time;
                res.PointsL[res.PointsL.Count - (i + 1)].Time = x.PointsL[i].Time;
            }

            res.UpdateAttributes();

            return res;
        }

        /// <summary>
        /// Offset the timestamps of all points in a substroke
        /// </summary>
        /// <param name="x"></param>
        /// <param name="time"></param>
        private void offset(Substroke x, ulong time)
        {
            for (int i = 0; i < x.PointsL.Count; i++)
                x.PointsL[i].Time = x.PointsL[i].Time + time;
        }

        #region Serialization
        /// <summary>
        /// Load a trained Rubine classifier from a file.
        /// </summary>
        /// <param name="filename">filename to load from</param>
        /// <returns></returns>
        public static Rubine Load(string filename)
        {
            FileStream fs = new FileStream(filename, FileMode.Open);
            BinaryFormatter bf = new BinaryFormatter();
            Rubine d = (Rubine)bf.Deserialize(fs);
            fs.Close();
            return d;
        }

        /// <summary>
        /// Safe a trained Rubine classifier to a file.
        /// </summary>
        /// <param name="filename">filename to save to</param>
        public void Save(string filename)
        {
            FileStream fs = new FileStream(filename, FileMode.Create);
            BinaryFormatter bf = new BinaryFormatter();
            bf.Serialize(fs, this);
            fs.Close();
        }
        #endregion

    }
}
