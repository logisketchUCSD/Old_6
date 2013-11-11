/**
 * File: Circuit.cs
 *
 * Authors: Matthew Weiner and Howard Chen
 * Harvey Mudd College, Claremont, CA 91711.
 * Sketchers 2007.
 * 
 * Use at your own risk.  This code is not maintained and not guaranteed to work.
 * We take no responsibility for any harm this code may cause.
 */

using System;

namespace IOTrain
{
	/// <summary>
	/// Summary description for Circuit.
	/// </summary>
	public class Circuit
    {
        #region INTERNALS

        public double TopLeftX;
		public double TopLeftY;
		public double BottomRightX;
		public double BottomRightY;
		public int WireCount;
		public int GateCount;
		public int LabelCount;
		public double WireTLAvg;
		public double WireBRAvg;
		public int inputcount;
		public int outputcount;
		private double diagslope;
		private double diagyint;

        #endregion INTERNALS

        #region CONSTRUCTOR

        public Circuit(Sketch.Shape[] shapes)
		{
			double[] coords = CircuitPerim(shapes);
			TopLeftX = coords[0];
			TopLeftY = coords[1];
			BottomRightX = coords[2];
			BottomRightY = coords[3];
			Console.WriteLine("{0}, {1}, {2}, {3}", TopLeftX, TopLeftY, BottomRightX,BottomRightY);
			int[] counts = ShapeCount(shapes);
			WireCount = counts[0];
			GateCount = counts[1];
			LabelCount = counts[2];
			Console.WriteLine("{0}, {1}, {2}",WireCount,GateCount,LabelCount);
        }

        #endregion CONSTRUCTOR

        #region METHODS

        public static double[] CircuitPerim(Sketch.Shape[] shapes)
		{
			double minX = Double.PositiveInfinity;
			double maxX = Double.NegativeInfinity;

			double minY = Double.PositiveInfinity;
			double maxY = Double.NegativeInfinity;

			foreach(Sketch.Shape shape in shapes)
			{
				string type = (string)shape.XmlAttrs.Type;
				if (!(type.Equals("Other") || type.Equals("Label") || type.Equals("Text")))
				{
					double pminX = Double.PositiveInfinity;
					double pminY = Double.PositiveInfinity;
					double pmaxX = Double.NegativeInfinity;
					double pmaxY = Double.NegativeInfinity;

					Sketch.Substroke[] ssubs = shape.Substrokes;
					Sketch.Point[][] points = new Sketch.Point[ssubs.Length][];

					for (int i=0; i<ssubs.Length; i++)
					{
						points[i] = ssubs[i].Points;
					}

					for (int i=0; i<ssubs.Length; i++)
					{
			
						foreach (Sketch.Point p in points[i])
						{
							pminX = Math.Min(pminX,Convert.ToDouble(p.X));
							pminY = Math.Min(pminY,Convert.ToDouble(p.Y));
							pmaxX = Math.Max(pmaxX,Convert.ToDouble(p.X));
							pmaxY = Math.Max(pmaxY,Convert.ToDouble(p.Y));
						}
					}

					minX = Math.Min(pminX,minX);
					minY = Math.Min(pminY,minY);
					maxX = Math.Max(maxX,pmaxX);
					maxY = Math.Max(maxY,pmaxY);
					//maxY = Math.Max(maxY, Convert.ToDouble(shape.XmlAttrs.Y) + Convert.ToDouble(shape.XmlAttrs.Height));
					Console.WriteLine("{0}: Xmin: {1}, Ymin: {2}",type,pminX,pminY);
				}
			}
			double[] coords = new double[]{minX,minY,maxX,maxY};
			//Console.WriteLine("TopLeft: ({0},{1}), BottomRight: ({2},{3})",minX,minY,maxX,maxY);
			return coords;
		}

		public static int[] ShapeCount(Sketch.Shape[] shapes)
		{
			int wirecount = 0;
			int gatecount = 0;
			int labelcount = 0;
			for (int y = 0; y < shapes.Length; y++)
			{
				string shapetype = Convert.ToString(shapes[y].XmlAttrs.Type);
				if (shapetype.Equals("Wire"))
				{
					wirecount = wirecount+1;
				}
				else if (!(shapetype.Equals("Wire")) && !(shapetype.Equals("Label")) && !(shapetype.Equals("Other")))
				{
					gatecount = gatecount+1;
				}
				else if (shapetype.Equals("Label"))
				{
					labelcount = labelcount+1;
				}
			}
			
			int[] counts = new int[]{wirecount,gatecount,labelcount};
			return counts;
        }

        #endregion METHODS

    }
}

