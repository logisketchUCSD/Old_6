using System;
using System.Collections.Generic;
using System.Text;

namespace Svm
{
    /// <summary>
    /// Class to classify partial gates
    /// </summary>
    public class ClassifyPartialGate : ClassifySVM
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="modelFile"></param>
        public ClassifyPartialGate(string modelFile)
            : base(modelFile) { }

        /// <summary>
        /// Get the partial gate associated with the prediction
        /// </summary>
        /// <param name="predict"></param>
        /// <returns></returns>
        public override string predictToString(int predict)
        {
            switch (predict)
            {
                case 1:
                    return "BACKLINE";

                case 2:
                    return "BACKARC";

                case 3:
                    return "FRONTARC";

                case 4:
                    return "BUBBLE";

                case 5:
                    return "NONPARTIAL";

                default:
                    return "UNKNOWN";
            }
        }
    }
}
