using System;
using System.Collections.Generic;
using System.Text;
using Sketch;

namespace OldRecognizers
{
    public class FeatureFunctions
    {
        /// <summary>
        /// Feature function to get the minium distance from the left or right of the sketch
        /// </summary>
        /// <param name="callingNode"></param>
        /// <param name="input"></param>
        /// <returns>1 if close to edge, -1 if far away</returns>
        static public double distFromLR(Featurefy.FeatureStroke fragFeat, double[] boundBox)
        {
            double fromL = fragFeat.Spatial.UpperLeft.X - boundBox[0];
            double fromR = boundBox[2] - fragFeat.Spatial.LowerRight.X;
            double dist = fromL;
            if (fromR < dist)
            {
                dist = fromR;
            }
            double scale = 30;
            return tfLow(dist, (boundBox[2] - boundBox[0]) / 4, scale);
        }

        /// <summary>
        /// Feature function to get the minium distance from the left or right of the sketch
        /// </summary>
        /// <param name="callingNode"></param>
        /// <param name="input"></param>
        /// <returns>1 if close to edge, -1 if far away</returns>
        static public double distFromTB(Featurefy.FeatureStroke fragFeat, double[] boundBox)
        {
            double fromTop = fragFeat.Spatial.UpperLeft.Y - boundBox[1];
            double fromBot = boundBox[3] - fragFeat.Spatial.LowerRight.Y;
            double dist = fromTop;
            if (fromBot < dist)
            {
                dist = fromBot;
            }
            double scale = 30;
            return tfLow(dist, (boundBox[3] - boundBox[1]) / 4, scale);
        }


        /// <summary>
        /// Returns the bounding box of the whole sketch [leftx, topy, rightx, bottomy]
        /// </summary>
        /// <param name="nodes"></param>
        /// <returns>A list [leftx, topy, rightx, bottomy] specifying the bounding box of the sketch</returns>
        public static double[] bbox(Sketch.Sketch sketch)
        {
            Sketch.Stroke[] strokes = sketch.Strokes;

            double topy = Double.PositiveInfinity;
            double leftx = Double.PositiveInfinity;
            double boty = 0;
            double rightx = 0;

            for (int i = 0; i < strokes.Length; i++)
            {
                Featurefy.FeatureStroke fragFeat = new Featurefy.FeatureStroke(strokes[i]);

                if (fragFeat.Spatial.UpperLeft.X < leftx)
                {
                    leftx = fragFeat.Spatial.UpperLeft.X;
                }
                if (fragFeat.Spatial.UpperLeft.Y < topy)
                {
                    topy = fragFeat.Spatial.UpperLeft.Y;
                }
                if (fragFeat.Spatial.LowerRight.X > rightx)
                {
                    rightx = fragFeat.Spatial.LowerRight.X;
                }
                if (fragFeat.Spatial.UpperLeft.Y > boty)
                {
                    boty = fragFeat.Spatial.UpperLeft.Y;
                }
            }
            return new double[4] { leftx, topy, rightx, boty };
        }

        static public double squareInkDensityHigh(Featurefy.FeatureStroke fragFeat)
        {
            double density = fragFeat.ArcLength.InkDensity;
            double scale = 100;
            return tfHigh(density, 24, scale);
            //return CreateGraph.tfHigh(density, parameter, scale);
        }


        static public double squareInkDensityLow(Featurefy.FeatureStroke fragFeat)
        {
            double density = fragFeat.ArcLength.InkDensity;
            double scale = 90;
            return tfLow(density, 5, scale);
        }


        /// <summary>
        /// Determines whether this Node falls into the category of short
        /// </summary>
        /// <param name="callingNode">Node that the feature function is being evaluated around</param>
        /// <param name="input">The set of all stroke data for the graph</param>
        /// <returns>1 if this Node is short, -1 otherwise</returns>
        static public double arcLengthShort(Featurefy.FeatureStroke fragFeat)
        {
            //ARBITRARY VALUE!!!!
            // Data analysis should be performed to determine what a good threshold is for this
            // NOTE: Aaron changed this value from 1000 to 300
            double scale = 30;
            return tfLow(fragFeat.ArcLength.TotalLength, 300, scale);
        }

        /// <summary>
        /// Determines whether this Node falls into the category of long
        /// </summary>
        /// <param name="callingNode">Node that the feature function is being evaluated around</param>
        /// <param name="input">The set of all stroke data for the graph</param>
        /// <returns>1 if this Node is long, -1 otherwise</returns>
        static public double arcLengthLong(Featurefy.FeatureStroke fragFeat)
        {
            //ARBITRARY VALUE!!!!
            // Data analysis should be performed to determine what a good threshold is for this
            // NOTE: Aaron changed this value from 2000 to 2000
            double scale = 30;
            return tfHigh(fragFeat.ArcLength.TotalLength, 2000, scale);
        }

        /// <summary>
        /// Determines if the two ends of this stroke are far apart
        /// </summary>
        /// <param name="callingNode">Node to evaluate on</param>
        /// <param name="input">The set of all stroke data in the graph</param>
        /// <returns>1 if far apart, -1 otherwise</returns>
        static public double distBetweenEndsLarge(Featurefy.FeatureStroke fragFeat)
        {
            Point p1 = fragFeat.Spatial.FirstPoint;
            Point p2 = fragFeat.Spatial.LastPoint;

            double u = p1.X;
            double v = p1.Y;
            double p = p2.X;
            double q = p2.Y;

            double dist = Math.Sqrt((u - p) * (u - p) + (v - q) * (v - q));

            // transfer at dist > 70% of arclength
            if (fragFeat.ArcLength.TotalLength == 0.0)
            {
                //This stroke has a length of zero, so this feature is meaningless
                return 0.0;
            }
            double scale = 30;
            return tfHigh(dist, (fragFeat.ArcLength.TotalLength * 0.7), scale);
        }

        /// <summary>
        /// Determines if the two endso of this stroke are close together
        /// </summary>
        /// <param name="callingNode">Node to evaluate on</param>
        /// <param name="input">The set of all stroke data in the graph</param>
        /// <returns></returns>
        static public double distBetweenEndsSmall(Featurefy.FeatureStroke fragFeat)
        {
            Point p1 = fragFeat.Spatial.FirstPoint;
            Point p2 = fragFeat.Spatial.LastPoint;

            double u = p1.X;
            double v = p1.Y;
            double p = p2.X;
            double q = p2.Y;

            double dist = Math.Sqrt((u - p) * (u - p) + (v - q) * (v - q));

            // transfer at dist < 20% of arclength
            if (fragFeat.ArcLength.TotalLength == 0.0)
            {
                //This stroke has a length of zero, so this feature is meaningless
                return 0.0;
            }

            double scale = 30;
            return tfLow(dist, (fragFeat.ArcLength.TotalLength * 0.2), scale);
        }

        /// <summary>
        /// This function find the total angle (in radians) that a stroke turns over its length.
        /// </summary>
        /// <param name="callingNode">Node that the feature function is being evaluated on.</param>
        /// <param name="input">The set of all stroke data for the graph</param>
        /// <returns>Angle turned by stroke in radians.</returns>
        static private double turning(Featurefy.FeatureStroke fragFeat)
        {
            // End point window so we don't count initial hooks
            const int WINDOW = 5;

            double sumDeltaAngle = 0.0;

            // Make sure we have something relevant to compute
            if (fragFeat.Points.Length < (WINDOW * 2) + 1)
                return sumDeltaAngle;

            // Make room for the data, and initialize to our first case
            double prevX = (fragFeat.Points[WINDOW]).X;
            double prevY = (fragFeat.Points[WINDOW]).Y;
            double X = (fragFeat.Points[WINDOW + 1]).X;
            double Y = (fragFeat.Points[WINDOW + 1]).Y;

            // The change in X and Y
            double delX = X - prevX;
            double delY = Y - prevY;

            // ah-Ha, the angle
            double prevDirection = Math.Atan2(delY, delX);

            // Make some space we will need
            double newDirection;
            double deltaAngle;

            int length = fragFeat.Points.Length - WINDOW;
            for (int i = WINDOW + 2; i < length; i++)
            {
                // Update the previous values
                prevX = X;
                prevY = Y;

                // Grab the new values
                X = (fragFeat.Points[i]).X;
                Y = (fragFeat.Points[i]).Y;

                // Find the new deltas
                delX = X - prevX;
                delY = Y - prevY;

                // Find the new direction
                newDirection = Math.Atan2(delY, delX);

                // Find the change from the previous dirction
                deltaAngle = newDirection - prevDirection;

                // Not so fast, we're not done yet
                // deltaAngle has to be in the range +pi to -pi
                deltaAngle = (deltaAngle % (2 * Math.PI));
                if (deltaAngle > Math.PI)
                {
                    deltaAngle -= (2 * Math.PI);
                }

                // And finally add it to the sum
                sumDeltaAngle += deltaAngle;

                // Some bookkeeping
                prevDirection = newDirection;
            }

            return Math.Abs(sumDeltaAngle);
        }

        /// <summary>
        /// Determines if the stroke underwent no net angle change
        /// </summary>
        /// <param name="callingNode">Node that the feature function is being evaluated on</param>
        /// <param name="input">The set of all stroke data for the graph</param>
        /// <returns>1 if turned ~0 deg, -1 otherwise</returns>
        static public double turningZero(Featurefy.FeatureStroke fragFeat)
        {
            //angle turned is near zero?
            double deltaAngle = turning(fragFeat);

            //ARBITRARY VALUE!!!
            // NOTE: Aaron changed value from 17.5 (0.305) to 20
            double scale = 30;
            return tfLow(deltaAngle, 0.349, scale); //approx 17.5 degrees
        }

        /// <summary>
        /// Determines if the stroke underwent a small angle change
        /// </summary>
        /// <param name="callingNode">Node that the feature function is being evaluated on</param>
        /// <param name="input">The set of all stroke data for the graph</param>
        /// <returns>1 if turned small angle, -1 otherwise</returns>
        static public double turningSmall(Featurefy.FeatureStroke fragFeat)
        {
            //angle turned is small
            double deltaAngle = turning(fragFeat);

            //ARBITRARY VALUE!!!
            // band is 17.5 (0.305) to 217.5 degrees (3.80)
            // NOTE: Aaron changed this value to 20 - 180
            double scale = 30;
            double upperLimit = tfLow(deltaAngle, 3.14, scale); // <217.5 degrees
            double lowerLimit = tfHigh(deltaAngle, 0.349, scale); // >17.5 degrees
            return upperLimit * lowerLimit; //multiply them to create a band of approx 1
        }

        /// <summary>
        /// Determines if the stroke underwent approx 1 full rotation (217.5 to 450 degrees)
        /// </summary>
        /// <param name="callingNode">Node that the feature function is being evaluated on</param>
        /// <param name="input">The set of all stroke data for the graph</param>
        /// <returns>1 if turned one rotation, -1 otherwise</returns>
        static public double turning360(Featurefy.FeatureStroke fragFeat)
        {
            //angle turned is near one revolution
            double deltaAngle = turning(fragFeat);

            //ARBITRARY VALUE!!!
            // band is 217.5 (3.80) to 450 (7.85) degrees
            // NOTE: Aaron changed this value to 290 - 430
            double scale = 30;
            double upperLimit = tfLow(deltaAngle, 7.50, scale); // <450 degrees
            double lowerLimit = tfHigh(deltaAngle, 5.06, scale); // >217.5 degrees
            return upperLimit * lowerLimit; //multiply them to create a band of approx 1
        }

        /// <summary>
        /// Determines if the stroke underwent a large amount of turning (>450 degrees)
        /// </summary>
        /// <param name="callingNode">Node that the feature function is being evaluated on</param>
        /// <param name="input">The set of all stroke data for the graph</param>
        /// <returns>1 if turned large amount, -1 otherwise</returns>
        static public double turningLarge(Featurefy.FeatureStroke fragFeat)
        {
            //angle turned is large
            double deltaAngle = turning(fragFeat);

            //ARBITRARY VALUE!!!
            // NOTE: Aaron changed this value to > 430
            double scale = 30;
            return tfHigh(deltaAngle, 7.50, scale); //approx 450 degrees
        }

        /// <summary>
        /// Creates a smooth transfer of the output from 1 to -1 as the input crosses the threshold
        /// </summary>
        /// <param name="input">The value to create the transfer on</param>
        /// <param name="threshold">The point around which a transfer is made</param>
        /// <param name="scale">The value by which we scale the function. The higher the value, 
        ///                     the higher the slope</param>>
        /// <returns>asymptotic from 1 to -1</returns>
        public static double tfLow(double input, double threshold, double scale)
        {
            //arctan provides the smooth transfer function I desire
            //arctan goes from -1.2 to +1.2 as the input goes from -3 to +3
            //the input will be scaled to map a change of 10% threshold to 0-3
            //Note: the default value of slope is 30.0
            return (-1.0 * Math.Atan((input - threshold) / threshold * scale)) / (Math.PI / 2);
        }

        /// <summary>
        /// Creates a smooth transfer of the output from -1 to 1 as the input crosses the threshold
        /// </summary>
        /// <param name="input">The value to create the transfer on</param>
        /// <param name="threshold">The point around which a transfer is made</param>
        /// <param name="scale">The value by which we scale the function. The higher the value, 
        ///                     the higher the slope</param>>
        /// <returns>asymptotic from -1 to 1</returns>
        public static double tfHigh(double input, double threshold, double scale)
        {
            //arctan provides the smooth transfer function I desire
            //arctan goes from -1.2 to +1.2 as the input goes from -3 to +3
            //the input will be scaled to map a change of 10% threshold to 0-3
            //Note: the default value of scale is 30.0
            return (1.0 * Math.Atan((input - threshold) / threshold * scale)) / (Math.PI / 2);
        }

    }
}
