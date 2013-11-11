using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using Sketch;
using ConverterXML;

namespace ScaleFamilyTreeSketches
{
    class Program
    {
        static void Main(string[] args)
        {
            float ConversionFactor = 20f;

            string[] files = Directory.GetFiles(args[0], "*.labeled.xml");

            foreach (string file in files)
            {
                Sketch.Sketch sketch = new ReadXML(file).Sketch;
                Dictionary<Point, Point> pointLookup = new Dictionary<Point, Point>();

                string fileShort = Path.GetFileName(file);

                Console.WriteLine(fileShort + ": " + sketch.Points.Length + " Points");

                int n = 0;
                foreach (Sketch.Substroke stroke in sketch.SubstrokesL)
                {
                    for (int i = 0; i < stroke.PointsL.Count; i++)
                    {
                        if (n % 1000 == 0)
                            Console.WriteLine("\t" + n + " @ " + DateTime.Now.ToLocalTime().ToString());
                        stroke.PointsL[i].X = stroke.PointsL[i].X * ConversionFactor;
                        stroke.PointsL[i].Y = stroke.PointsL[i].Y * ConversionFactor;
                        n++;
                    }
                }

                MakeXML xml = new MakeXML(sketch);
                xml.WriteXML(file.Replace(fileShort, "\\scaled\\" + fileShort));
            }
        }
    }
}
