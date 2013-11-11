using System;
using System.Collections.Generic;
using System.Text;
using SymbolRec;
using SymbolRec.Image;

namespace OldRecognizers
{
    /// <summary>
    /// Recognize Partial Gates ( BACKLINE, BACKARC, FRONTLINE, BUBBLE )
    /// </summary>
    public class PartialGateRecognizer : IRecognizer
    {
        /// <summary>
        /// SVM Classifier used to classify substrokes
        /// </summary>
        private Svm.ClassifyPartialGate classify;

        /// <summary>
        /// Default Constructor
        /// </summary>
        public PartialGateRecognizer()
            : this("data/partial.model", 
            new string[] { "data/backline.amat", "data/backarc.amat", "data/frontarc.amat", "data/bubble.amat" }, 
            32, 32) { }

        /// <summary>
        /// Constructor
        /// </summary>
        public PartialGateRecognizer(string modelFile, string[] definitions, int width, int height)
        {
            SymbolRec.Image.DefinitionImage.ClearMatches();
            int i, len = definitions.Length;
            for (i = 0; i < len; ++i)
                SymbolRec.Image.DefinitionImage.AddMatch(new SymbolRec.Image.DefinitionImage(width, height, definitions[i]));
                     
            classify = new Svm.ClassifyPartialGate(modelFile);
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
