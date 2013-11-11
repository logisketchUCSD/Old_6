using System;
using System.Collections.Generic;
using System.Text;

using Sketch;
using ConverterXML;
using Recognizers;


namespace RubineTester
{
    class Program
    {
        static void Main(string[] args)
        {

            Rubine dr = new Rubine();

            Sketch.Sketch and2 = new ConverterXML.ReadXML("c:\\and2.xml").Sketch;
            Sketch.Sketch and3 = new ConverterXML.ReadXML("c:\\and3.xml").Sketch;
            Sketch.Sketch and4 = new ConverterXML.ReadXML("c:\\and4.xml").Sketch;
            Sketch.Sketch and5 = new ConverterXML.ReadXML("c:\\and5.xml").Sketch;
            Sketch.Sketch or1 = new ConverterXML.ReadXML("c:\\or1.xml").Sketch;
            Sketch.Sketch or2 = new ConverterXML.ReadXML("c:\\or2.xml").Sketch;
            Sketch.Sketch or3 = new ConverterXML.ReadXML("c:\\or3.xml").Sketch;
            Sketch.Sketch or4 = new ConverterXML.ReadXML("c:\\or4.xml").Sketch;

            Sketch.Sketch and1 = new ConverterXML.ReadXML("c:\\and1.xml").Sketch;

            Dictionary<string, List<Shape>> data = new Dictionary<string, List<Shape>>();
            data.Add("and", new List<Shape>());
            data.Add("or", new List<Shape>());

            //data["and"].Add(and1.ShapesL[0]);
            data["and"].Add(and2.ShapesL[0]);
            data["and"].Add(and3.ShapesL[0]);
            data["and"].Add(and4.ShapesL[0]);
            data["and"].Add(and5.ShapesL[0]);
            data["or"].Add(or1.ShapesL[0]);
            data["or"].Add(or2.ShapesL[0]);
            data["or"].Add(or3.ShapesL[0]);
            data["or"].Add(or4.ShapesL[0]);

            dr.train(data);

            Results x = dr.classify(and1.ShapesL[0]);
            Console.WriteLine(x);
            Console.Read();

        }
    }
}
