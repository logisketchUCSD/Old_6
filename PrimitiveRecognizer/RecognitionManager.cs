using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using Sketch;

namespace PrimitiveRecognizer
{
    class RecognitionManager
    {
        private PrimitiveClassifier.PrimitiveClassifier classifier;
        private PrimitiveGrouper.TimeGrouper grouper1;
        private PrimitiveGrouper.PrimitiveGrouper grouper2;
        private SketchPanelLib.SketchPanel parentPanel;

        public RecognitionManager(SketchPanelLib.SketchPanel panel)
        {
            this.parentPanel = panel;
            this.classifier = new PrimitiveClassifier.PrimitiveClassifier();
            this.grouper1 = new PrimitiveGrouper.TimeGrouper();
            this.grouper2 = new PrimitiveGrouper.PrimitiveGrouper();
        }

        public void classifySketch()
        {
            //Fragmenter.Fragment.fragmentSketch(parentPanel.Sketch);
            //parentPanel.loadSketch(parentPanel.Sketch, true, true);
            classifier.classifySketch(parentPanel.Sketch);
        }

        public void groupSketch()
        {
            classifySketch();
            parentPanel.Sketch.clearShapes();
            grouper1.groupSketch(parentPanel.Sketch);
            grouper2.groupSketch(parentPanel.Sketch);
        }


        public void testClassifier(string fromDirectory)
        {
            Dictionary<string, Dictionary<string, int>> testResults = new Dictionary<string, Dictionary<string, int>>();
            string[] filepaths = System.IO.Directory.GetFiles(fromDirectory);
            foreach (string filepath in filepaths)
            {
                if (!System.IO.File.Exists(filepath))
                {
                    MessageBox.Show("Error: target file does not exist");
                }

                parentPanel.InkSketch.LoadSketch(filepath);
                Sketch.Sketch temp = parentPanel.Sketch.Clone();
                classifier.classifySketch(parentPanel.Sketch);
                evaluateClasses(temp, parentPanel.Sketch, ref testResults);
            }
        }

        private void evaluateClasses(Sketch.Sketch trueSketch, Sketch.Sketch ourSketch, ref Dictionary<string, Dictionary<string, int>> testresults)
        {
            foreach (Substroke trueSub in trueSketch.Substrokes)
            {
                string classification = "None";
                foreach (Shape parentShape in trueSub.ParentShapes)
                    if (parentShape.Label.Contains("Arc") || parentShape.Label.Contains("Line") || parentShape.Label == "Wire"
                        || parentShape.Label == "Label" || parentShape.Label == "NOTBUBBLE")
                        classification = parentShape.Label;
                if (classification == "None")
                    continue;
                if (!testresults.ContainsKey(classification))
                    testresults.Add(classification, new Dictionary<string, int>());
                Substroke ourSub = ourSketch.GetSubstroke(trueSub.Id);
                if (!testresults[classification].ContainsKey(ourSub.XmlAttrs.Classification))
                    testresults[classification].Add(ourSub.XmlAttrs.Classification, 1);
                else
                    testresults[classification][ourSub.XmlAttrs.Classification]++;

            }
        }

        public void testGrouper(string fromDirectory)
        {
            Dictionary<string, Dictionary<string, int>> testResults = new Dictionary<string, Dictionary<string, int>>();
            string[] filepaths = System.IO.Directory.GetFiles(fromDirectory);
            foreach (string filepath in filepaths)
            {
                if (!System.IO.File.Exists(filepath))
                {
                    MessageBox.Show("Error: target file does not exist");
                }

                parentPanel.InkSketch.LoadSketch(filepath);
                Sketch.Sketch temp = parentPanel.Sketch.Clone();
                groupSketch();
                evaluateGroups(parentPanel.Sketch, temp, ref testResults);
            }
        }

        public void testGrouper2(string fromDirectory)
        {
            int totalMatch = 0;
            int correctMatch = 0;

            string[] filepaths = System.IO.Directory.GetFiles(fromDirectory);
            foreach (string filepath in filepaths)
            {
                if (!System.IO.File.Exists(filepath))
                {
                    MessageBox.Show("Error: target file does not exist");
                }

                parentPanel.InkSketch.LoadSketch(filepath);
                Sketch.Sketch temp = parentPanel.Sketch.Clone();
                groupSketch();
                evaluateGroups2(parentPanel.Sketch, temp, ref correctMatch, ref totalMatch);
            }
        }

        private void evaluateGroups2(Sketch.Sketch ourSketch, Sketch.Sketch trueSketch, ref int correctMatch, ref int totalMatch)
        {
            foreach(Sketch.Shape shape in ourSketch.Shapes)
                for(int i = 0; i < shape.SubstrokesL.Count; i++)
                    for (int j = i + 1; j < shape.SubstrokesL.Count; j++)
                    {
                        totalMatch++;
                        if (trueSketch.GetSubstroke(shape.Substrokes[i].Id).ParentShapes[0] == trueSketch.GetSubstroke(shape.Substrokes[j].Id).ParentShapes[0])
                            correctMatch++;
                    }
        }

        private void evaluateGroups(Sketch.Sketch ourSketch, Sketch.Sketch trueSketch, ref Dictionary<string, Dictionary<string, int>> results)
        {
            Dictionary<string, int> dict = new Dictionary<string, int>();
            dict.Add("Total Shapes", 0);
            dict.Add("Correct Match", 0);
            dict.Add("Shapes with extra strokes", 0);
            dict.Add("Split Shapes", 0);
            dict.Add("Skewed Shapes", 0);

            foreach (Sketch.Shape trueShape in trueSketch.Shapes)
            {
                if (!results.ContainsKey(trueShape.Label))
                    results.Add(trueShape.Label, new Dictionary<string, int>(dict));
                results[trueShape.Label]["Total Shapes"]++;

                if (trueShape.SubstrokesL.Count == 1)
                {
                    if (ourSketch.GetSubstroke(trueShape.Substrokes[0].Id).ParentShapes.Count > 0)
                    {
                        Shape ourShape = ourSketch.GetSubstroke(trueShape.Substrokes[0].Id).ParentShapes[0];
                        if (ourShape.SubstrokesL.Count > 1)
                        {
                            results[trueShape.Label]["Shapes with extra strokes"]++;
                            continue;
                        }
                    }
                    results[trueShape.Label]["Correct Match"]++;
                    continue;
                }

                bool gotResult = false;
                foreach (Sketch.Shape ourShape in ourSketch.Shapes)
                {
                    bool hit = false;
                    List<Substroke> trueSubs = trueShape.SubstrokesL;
                    List<Substroke> ourSubs = ourShape.SubstrokesL;
                    foreach (Sketch.Substroke sub in ourShape.Substrokes)
                    {
                        if (trueSubs.Contains(trueSketch.GetSubstroke(sub.Id)))
                        {
                            hit = true;
                            trueSubs.Remove(trueSketch.GetSubstroke(sub.Id));
                            ourSubs.Remove(sub);
                        }
                    }

                    // All of the substrokes matched exactly.
                    if (hit && trueSubs.Count == 0 && ourSubs.Count == 0)
                    {
                        results[trueShape.Label]["Correct Match"]++;
                        gotResult = true;
                        break;
                    }
                    // We used up all of the true substrokes, but not all of the substrokes in our shape.
                    if (hit && trueSubs.Count == 0)
                    {
                        results[trueShape.Label]["Shapes with extra strokes"]++;
                        gotResult = true;
                        break;
                    }
                    // Not all of the substrokes were matched and there were extra strokes.
                    if (hit && trueSubs.Count != 0 && ourSubs.Count != 0)
                    {
                        results[trueShape.Label]["Skewed Shapes"]++;
                        gotResult = true;
                        break;
                    }
                }
                // None of the other options worked- means that it was split
                if(!gotResult)
                    results[trueShape.Label]["Split Shapes"]++;
            }
        }

    }
}
