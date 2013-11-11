using System;
using System.Collections.Generic;
using System.Text;

namespace Svm
{
    /// <summary>
    /// Class to classify Wires, Gates, and Labels
    /// </summary>
    public class ClassifyWGL : ClassifySVM
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="modelFile"></param>
        public ClassifyWGL(string modelFile)
            : base(modelFile) { }

        /// <summary>
        /// Get the type associated with the prediction
        /// </summary>
        /// <param name="predict"></param>
        /// <returns></returns>
        public override string predictToString(int predict)
        {
            switch (predict)
            {
                case 1:
                    return "Wire";

                case 2:
                    return "Gate";

                case 3:
                    return "Label";

                case 4:
                    return "Nonwire";

                case 5:
                    return "Nongate";

                case 6:
                    return "Nonlabel";

                default:
                    return "Unknown";
            }
        }

        /// <summary>
        /// Gets the color associated with a label for visual purposes
        /// </summary>
        /// <param name="label">Type to match</param>
        /// <returns>Color that matches the label</returns>
        public static System.Drawing.Color labelToColor(string label)
        {
            switch (label)
            {
                case "Wire":
                    return System.Drawing.Color.Blue;
                case "Gate":
                    return System.Drawing.Color.Red;
                case "Label":
                    return System.Drawing.Color.Orange;
                case "Nonwire":
                    return System.Drawing.Color.Purple;
                case "Nongate":
                    return System.Drawing.Color.Green;
                case "Nonlabel":
                    return System.Drawing.Color.Indigo;
                default:
                    return System.Drawing.Color.Black;
            }
        }
    }
}
