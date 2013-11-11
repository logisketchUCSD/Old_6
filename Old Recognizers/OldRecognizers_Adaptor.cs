using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sketch;

namespace OldRecognizers
{
    /// <summary>
    /// Allows the newer version of recognizers used in this project to be used with the old system,
    /// for example with TestRig.
    /// 
    /// Created by Ben Pollard, February 14, 2011
    /// </summary>
    public class OldRecognizers_Adaptor : OldRecognizers.Recognizer
    {
        RecognitionInterfaces.Recognizer core;
        private Dictionary<string, string> settings;
        private Featurefy.FeatureSketch currentFeatureSketch;

        /// <summary>
        /// Create an adaptor around a CombinationRecognizer.ComboRecognizer
        /// </summary>
        public OldRecognizers_Adaptor()
            : this(CombinationRecognizer.ComboRecognizer.LoadDefault())
        {
        }

        /// <summary>
        /// Create an adaptor for an arbitrary recognizer
        /// </summary>
        /// <param name="r">the recognizer this adaptor will contain</param>
        public OldRecognizers_Adaptor(RecognitionInterfaces.Recognizer r)
        {
            settings = Files.SettingsReader.readSettings(AppDomain.CurrentDomain.BaseDirectory + "settings.txt");
            core = r;
            currentFeatureSketch = new Featurefy.FeatureSketch();
        }

        override public OldRecognizers.Results Recognize(Substroke[] substrokes)
        {
            Console.WriteLine("Warning: Constructing a shape without XML attributes!");

            Shape newShape = new Shape(new List<Substroke>(substrokes),
                new Sketch.XmlStructs.XmlShapeAttrs(true));

            return RecognizeShape(newShape);

        }

        override public string getTypeName()
        {
            return core.GetType().ToString();
        }

        public void setFeatureSketch(Sketch.Sketch s)
        {
            currentFeatureSketch = Featurefy.FeatureSketch.MakeFeatureSketch(s, settings);
        }

        public OldRecognizers.Results RecognizeShape(Shape s)
        {
            if (currentFeatureSketch == null)
            {
                Console.WriteLine("Warning: Performing recognition without a FeatureSketch.");
                currentFeatureSketch = new Featurefy.FeatureSketch();
            }
            core.recognize(s, currentFeatureSketch);  //Note: FeatureSketch is unused

            OldRecognizers.Results res = new OldRecognizers.Results();
            Dictionary<string, double> altTypes = s.AlternateTypes;
            if (altTypes == null)
            {
                return res;
            }

            foreach (KeyValuePair<string, double> pair in altTypes)
            {
                res.Add(pair.Key, pair.Value);
            }

            return res;
        }

        public void LearnFromExample(Shape s)
        {
            core.learnFromExample(s);
        }

    }
}
