using System;
using System.Collections.Generic;
using System.Text;
using Sketch;

namespace OldRecognizers
{
    public class WGLRecognizer : IRecognizer
    {
        /// <summary>
        /// SVM Classifier used to classify substrokes
        /// </summary>
        private Svm.ClassifyWGL classify;

        public WGLRecognizer(string modelFile)
        {
            classify = new Svm.ClassifyWGL(modelFile);
        }

        public Results Recognize(Sketch.Substroke substroke, double[] boundBox, int numLabels)
        {
            string line = substrokeToLine(substroke, boundBox).Insert(0, "0").Remove(1, 1); ;

            double[] probs;
            string[] labels;
            classify.predict(line, out probs, out labels);
                        
            Results res = new Results(numLabels);
            int i, len = numLabels;
            for (i = 0; i < len; ++i)
                res.Add(labels[i], probs[i]);

            return res;
        }

        static private string substrokeToLine(Sketch.Substroke sub, double[] boundBox)
        {
            int category = -1;
            string label = sub.FirstLabel;
            
            if (!label.Equals("unlabeled"))
            switch(label)
            {
                case "Wire":
                    category = 1;
                    break;
                case "Gate":
                    category = 2;
                    break;
                case "Label":
                    category = 3;
                    break;
                case "Nonwire":
                    category = 4;
                    break;
                case "Nongate":
                    category = 5;
                    break;
                case "Nonlabel":
                    category = 6;
                    break;
                default:
                    Console.WriteLine("Heuston we have a problem: unknown label.");
                    Console.WriteLine(label);
                    break;
            }
         
            Featurefy.FeatureStroke fragFeat = new Featurefy.FeatureStroke(sub);

            string line = category.ToString();

            line += " 1:" + FeatureFunctions.arcLengthLong(fragFeat).ToString();
            line += " 2:" + FeatureFunctions.arcLengthLong(fragFeat).ToString();
            line += " 3:" + FeatureFunctions.distBetweenEndsLarge(fragFeat).ToString();
            line += " 4:" + FeatureFunctions.distBetweenEndsSmall(fragFeat).ToString();
            line += " 5:" + FeatureFunctions.turning360(fragFeat).ToString();
            line += " 6:" + FeatureFunctions.turningLarge(fragFeat).ToString();
            line += " 7:" + FeatureFunctions.turningSmall(fragFeat).ToString();
            line += " 8:" + FeatureFunctions.turningZero(fragFeat).ToString();
            line += " 9:" + FeatureFunctions.squareInkDensityHigh(fragFeat).ToString();
            line += " 10:" + FeatureFunctions.squareInkDensityLow(fragFeat).ToString();
            line += " 11:" + FeatureFunctions.distFromLR(fragFeat, boundBox).ToString();
            line += " 12:" + FeatureFunctions.distFromTB(fragFeat, boundBox).ToString();
            return line;
        }

        public Results Recognize(Substroke[] substrokes)
        {
            return null;//Recognize(substrokes[0]);
        }
	}
}
