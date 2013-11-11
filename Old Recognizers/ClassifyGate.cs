using System;
using System.Collections.Generic;
using System.Text;

namespace Svm
{
    /// <summary>
    /// Class to classify gates
    /// </summary>
    public class ClassifyGate : ClassifySVM
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="modelFile"></param>
        public ClassifyGate(string modelFile)
            : base(modelFile) { }

        /// <summary>
        /// Get the gate associated with the prediction
        /// </summary>
        /// <param name="predict"></param>
        /// <returns></returns>
        public override string predictToString(int predict)
        {
            switch (predict)
            {
                case 1:
                    return "AND";
                    
                case 2:
                    return "NAND";
                    
                case 3:
                    return "NOR";
                    
                case 4:
                    return "NOT";
                    
                case 5:
                    return "OR";
                
                case 6:
                    return "UNKNOWN";
                
                default:
                    return "UNKNOWN";
            }
        }
        
    }
}
