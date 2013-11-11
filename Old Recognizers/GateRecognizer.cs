using System;
using System.Collections.Generic;
using System.Text;
using SymbolRec;
using SymbolRec.Image;
using System.Windows.Forms;
using System.IO;

namespace OldRecognizers
{
    /// <summary>
    /// Recognize Gates ( AND, NAND, NOR, NOT, OR )
    /// </summary>
    public class GateRecognizer : IRecognizer
    {
        /// <summary>
        /// SVM Classifier used to classify substrokes
        /// </summary>
        private Svm.ClassifyGate classify;        

        private static readonly string path = Path.GetDirectoryName(Application.ExecutablePath);
       
        /// <summary>
        /// Default Constructor
        /// Beware, hardcoded the size of the .amat files to be 32x32
        /// </summary>
        public GateRecognizer()
            : this(path + @"\data\gate.model", 
            new string[] { 
                path + @"\data\and.amat", 
                path + @"\data\nand.amat", 
                path + @"\data\nor.amat", 
                path + @"\data\not.amat", 
                path + @"\data\or.amat" }, 32, 32) { }

        /// <summary>
        /// Constructor
        /// </summary>
        public GateRecognizer(string modelFile, string[] definitions, int width, int height) 
        {
            //Clear the old matches
            SymbolRec.Image.DefinitionImage.ClearMatches();
            
            int i, len = definitions.Length;
            for (i = 0; i < len; ++i)
                SymbolRec.Image.DefinitionImage.AddMatch(new SymbolRec.Image.DefinitionImage(width, height, definitions[i]));

            classify = new Svm.ClassifyGate(modelFile);
        }

        /// <summary>
        /// Recognize a list of substrokes
        /// </summary>
        /// <param name="substrokes">Substrokes to recognize</param>
        /// <returns>Sorted recognition results</returns>
        public Results Recognize(Sketch.Substroke[] substrokes)
        {
            //Get the predictions and probabilities
            Substrokes subs = new Substrokes(substrokes);
            DefinitionImage di = new DefinitionImage(32, 32, subs);

            double[] probs;
            string[] labels;

            classify.predict(di.toNodes(), out probs, out labels);


            //Add it to the results
            int i, len = probs.Length;
            Results r = new Results(len);
            for (i = 0; i < len; ++i)
                r.Add(labels[i], probs[i]);

            //Sort :)
            r.Sort();

            return r;
        }
    }
}
