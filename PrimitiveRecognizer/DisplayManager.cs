using System;
using System.Collections.Generic;
using System.Text;
using SketchPanelLib;
using Sketch;

namespace PrimitiveRecognizer
{
    class DisplayManager
    {
        private SketchPanel parentPanel;

        public DisplayManager(SketchPanel panel)
        {
            parentPanel = panel;
        }

        public void DisplayClassification()
        {
            System.Drawing.RectangleF Rect = parentPanel.InkPicture.Ink.GetBoundingBox();
            using (System.Drawing.Graphics g = parentPanel.InkPicture.CreateGraphics())
            {
                foreach (Substroke sub in parentPanel.Sketch.Substrokes)
                {
                    Microsoft.Ink.Stroke stroke = parentPanel.InkSketch.GetInkStrokeBySubstrokeId(sub.Id);
                    Primitive fit;
                    if (sub.XmlAttrs.Classification == "Line")
                    {
                        fit = new LineSegment(sub.Points);
                        stroke.DrawingAttributes.Color = System.Drawing.Color.Blue;
                    }
                    else if (sub.XmlAttrs.Classification == "Arc")
                    {
                        fit = new ArcSegment(sub.Points);
                        stroke.DrawingAttributes.Color = System.Drawing.Color.Red;
                    }
                    else if (sub.XmlAttrs.Classification == "Circle")
                    {
                        fit = new Circle(sub.Points);
                        stroke.DrawingAttributes.Color = System.Drawing.Color.Green;
                    }
                    else
                    {
                        stroke.DrawingAttributes.Color = System.Drawing.Color.Black;
                        continue;
                    }
                    fit.draw(g, parentPanel.InkPicture.Renderer, Rect.Top, Rect.Left, 1);
                }
            }

            parentPanel.Invalidate();
            parentPanel.Refresh();
        }

        public void DisplayGroups()
        {
            foreach (Substroke sub in parentPanel.Sketch.Substrokes)
                parentPanel.InkSketch.GetInkStrokeBySubstrokeId(sub.Id).DrawingAttributes.Color = System.Drawing.Color.Black;
            Random random = new Random();
            foreach (Shape shape in parentPanel.Sketch.Shapes)
            {
                System.Drawing.Color shapecolor = System.Drawing.Color.FromArgb(random.Next());
                foreach (Substroke sub in shape.Substrokes)
                {
                    Microsoft.Ink.Stroke inkStroke = parentPanel.InkSketch.GetInkStrokeBySubstrokeId(sub.Id);
                    inkStroke.DrawingAttributes.Color = shapecolor;
                }
            }

            parentPanel.Invalidate();
            parentPanel.Refresh();
        }
    }
}
