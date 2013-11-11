using System;
using System.Collections.Generic;
using System.Text;

using Sketch;
using ConverterXML;
using Recognizers;


namespace DollarTester
{
    class Program
    {
        static void Main(string[] args)
        {

            DollarRecognizer dr = new DollarRecognizer();

            Sketch.Sketch and2 = new ConverterXML.ReadXML("c:\\and2.xml").Sketch;
            Sketch.Sketch and3 = new ConverterXML.ReadXML("c:\\and3.xml").Sketch;
            Sketch.Sketch and4 = new ConverterXML.ReadXML("c:\\and4.xml").Sketch;
            Sketch.Sketch and5 = new ConverterXML.ReadXML("c:\\and5.xml").Sketch;
            Sketch.Sketch or1 = new ConverterXML.ReadXML("c:\\or1.xml").Sketch;
            Sketch.Sketch or2 = new ConverterXML.ReadXML("c:\\or2.xml").Sketch;
            Sketch.Sketch or3 = new ConverterXML.ReadXML("c:\\or3.xml").Sketch;
            Sketch.Sketch or4 = new ConverterXML.ReadXML("c:\\or4.xml").Sketch;

            Sketch.Sketch and1 = new ConverterXML.ReadXML("c:\\and1.xml").Sketch;

            dr.addExample("and", and1.ShapesL[0]);
            dr.addExample("and", and2.ShapesL[0]);
            dr.addExample("and", and3.ShapesL[0]);
            dr.addExample("and", and4.ShapesL[0]);
            dr.addExample("and", and5.ShapesL[0]);
            dr.addExample("or", or1.ShapesL[0]);
            dr.addExample("or", or2.ShapesL[0]);
            //dr.addExample("or", or3.ShapesL[0]);
            dr.addExample("or", or4.ShapesL[0]);

            DateTime dt = DateTime.Now;
            string x = dr.classify(or3.ShapesL[0]);
            DateTime dt2 = DateTime.Now;
            Console.WriteLine(x);
            Console.WriteLine(dt2 - dt);
            Console.Read();

        }
    }
}
