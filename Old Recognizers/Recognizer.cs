using System;
using System.Text;
using Sketch;

namespace OldRecognizers
{
    /// <summary>
    /// The algorithms available for gate-level recognition
    /// </summary>
    public enum Algorithm { GATE, PARTIAL_GATE, WGL, CONGEAL, JOSHUA };

    /// <summary>
    /// Generic recognizer class. Wraps all other recognizers and allows users to select recognizer type at run-time.
    /// </summary>
    public class Recognizer : IRecognizer
    {
        private IRecognizer recognizer;

        // This doesn't do much yet. WGL looks like it could use a lot more work.
        private static string DefaultWGLModelFile = "";

        
        public Recognizer() 
        {
            // Added by Ben Pollard, this constructor should only be used by the
            // OldRecognizersAdaptor.
            
        }
        
        
        public Recognizer(Algorithm algorithm)
            : this(algorithm, new object[] { })
        {
            // Nothing to do here, this is just a wraper
        }

        public Recognizer(Algorithm algorithm, object[] args)
        {
            switch (algorithm)
            {
                case Algorithm.WGL:
                    recognizer = new WGLRecognizer(DefaultWGLModelFile);
                    break;
                case Algorithm.PARTIAL_GATE:
                    recognizer = new PartialGateRecognizer();
                    break;
                case Algorithm.GATE:
                    recognizer = new GateRecognizer();
                    break;
                case Algorithm.CONGEAL:
                    recognizer = new CongealRecognizer();
                    Console.WriteLine("Made a new CongealRecognizer: " + recognizer.ToString());
                    break;
                default:
                    recognizer = new GateRecognizer();
                    break;
            }
        }

        /// <summary>
        /// Performs recognition
        /// </summary>
        /// <param name="substrokes">An array of substrokes to recognize</param>
        /// <returns>Results in the Recognizers.Results format</returns>
        virtual public Results Recognize(Substroke[] substrokes)
        {
            if (recognizer == null)
            {
                Console.WriteLine("Error: recognizer not initalized");
                return new OldRecognizers.Results();
            }
            else return recognizer.Recognize(substrokes);
        }

        virtual public String getTypeName()
        {
            return recognizer.ToString();
        }

    }
}