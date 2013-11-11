using System;
using System.Collections.Generic;
using System.Text;
using Sketch;

namespace PrimitiveClassifier
{
    public class PrimitiveClassifier
    {
        public PrimitiveClassifier()
        {
            // Do nothing for now
        }

        public void classifySketch(Sketch.Sketch sketch)
        {
            foreach (Substroke sub in sketch.Substrokes)
                sub.XmlAttrs.Classification = classifySubstroke(sub);
        }

        private string classifySubstroke(Substroke sub)
        {
            LineSegment lineFit = new LineSegment(sub.Points);
            ArcSegment arcFit = new ArcSegment(sub.Points);

            if (Math.Abs(arcFit.SweepAngle) > 300 && arcFit.Score > .96)
                return ("Circle");
            if(lineFit.Score > .97)
                return ("Line");
            if(arcFit.Score > .98)
                return ("Arc");
            return ("Other");
        }
    }
}
