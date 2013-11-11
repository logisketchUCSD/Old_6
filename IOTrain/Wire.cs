/**
 * File: Wire.cs
 *
 * Authors: Matthew Weiner and Howard Chen
 * Harvey Mudd College, Claremont, CA 91711.
 * Sketchers 2007.
 * 
 * Use at your own risk.  This code is not maintained and not guaranteed to work.
 * We take no responsibility for any harm this code may cause.
 */

using System;
using System.Collections;
using ConverterXML;

namespace IOTrain
{
	/// <summary>
	/// Wire Class
	/// </summary>
	/// 
	public class Wire
	{
		#region INTERNALS
		
		/// <summary>
		/// identifies a wire as "input", "output", "internal", or "unclassified"
		/// </summary>
		private string Io;

		/// <summary>
		/// internal name of the wire
		/// </summary>
		private string Name;

		public double BottomRightX;

		public double BottomRightY;

		public double TopLeftX;

		public double TopLeftY;

		/// <summary>
		/// Height of wire
		/// </summary>
		private int Height;

		/// <summary>
		/// Width of wire
		/// </summary>
		private int Width;
		
		/// <summary>
		/// ID for wire; used to make sure we dont queue the same wire more than once
		/// </summary>
		private string Id;

		/// <summary>
		/// Start point of wire
		/// </summary>
		public Sketch.Point P1;

		/// <summary>
		/// End point of wire
		/// </summary>
		public Sketch.Point P2;

		public double P1disttoperim;

		public double P1closestside;

		public double P2disttoperim;

		public double P2closestside;

		public double P1mindistgate;

		public double P2mindistgate;

		public double mindistlabel;

		public double P1mindistwire;

		public double P2mindistwire;

		public Sketch.Substroke[] substrokes;

		public double P1torightperim;

		public double P1toleftperim;

		public double P1totopperim;

		public double P1tobotperim;

		public double P2torightperim;

		public double P2toleftperim;

		public double P2totopperim;

		public double P2tobotperim;

		public bool[] EndPtExist;
		public Sketch.Point[] EndPt;
		public bool[] EndPtConnect;
		public bool[] EndPtExist_1;
		public Sketch.Point[] EndPt_1;
		// index 0: TL (Also the default location for endpts on the left)
		// index 1: TR (Also the default location for endpts on the top)
		// index 2: BL (Also the default location for endpts on the bottom)
		// index 3: BR (Also the default location for endpts on the right)
		public bool[] EndPtExist_2;
		public Sketch.Point[] EndPt_2;
		public bool[] EndPtExist_3;
		public Sketch.Point[] EndPt_3;
		private double marginvalue;
		private int Orientation;
		public int substroke_count;
		private double substroke_margin;
		private String name;
		public ArrayList points;

		#endregion INTERNALS

		#region CONSTRUCTORS

		public Wire(Sketch.Shape wire, double marginvalue, double substroke_margin)
		{
			this.TopLeftX = Convert.ToInt32(wire.XmlAttrs.X);
			this.TopLeftY = Convert.ToInt32(wire.XmlAttrs.Y);
			this.BottomRightX = this.TopLeftX + this.Width;
			this.BottomRightY = this.TopLeftY + this.Height;

			this.Width = Convert.ToInt32(wire.XmlAttrs.Width);
			this.Height = Convert.ToInt32(wire.XmlAttrs.Height);
			this.Id = Convert.ToString(wire.XmlAttrs.Id);

			points = GetPoints(wire);
			EndPt = new Sketch.Point[4];
			EndPt_1 = new Sketch.Point[4];
			EndPt_2 = new Sketch.Point[4];
			EndPtExist = new bool[4];
			EndPtExist_1 = new bool[4];
			EndPtExist_2 = new bool[4];
			EndPtConnect = new bool[4];
			name = (String)wire.XmlAttrs.Type;

			// Various Algorithms for determining End Points
			DetermineEndPts1();					// Looks at the corners for endpoints
			DetermineEndPts2();					// Looks at the sides for endpoints
			DetermineEndPts3(wire);			// Looks at Substrokes for endpoints
			BestEndPts();

			this.P1 = EndPt[0];
			this.P2 = EndPt[1];

			this.substrokes = wire.Substrokes;

			//double[] coords = FindBound(substrokes);
			//this.TopLeftX = coords[0];
			//this.TopLeftY = coords[1];
			//this.BottomRightX = coords[2];
			//this.BottomRightY = coords[3];
		}

		
		#endregion CONSTRUCTORS

		#region METHODS

		// Finds the extreme X and Y coordinates for the wire

		public static double[] FindBound(Sketch.Substroke[] substrokes)
		{
			double pminX = Double.PositiveInfinity;
			double pminY = Double.PositiveInfinity;
			double pmaxX = Double.NegativeInfinity;
			double pmaxY = Double.NegativeInfinity;

			Sketch.Point[][] pointsb = new Sketch.Point[substrokes.Length][];
			for (int i=0; i<substrokes.Length; i++)
			{
				pointsb[i] = substrokes[i].Points;
			}

			for (int i=0; i<substrokes.Length; i++)
			{
				
				foreach (Sketch.Point p in pointsb[i])
				{
					pminX = Math.Min(pminX,Convert.ToDouble(p.X));
					pminY = Math.Min(pminY,Convert.ToDouble(p.Y));
					pmaxX = Math.Max(pmaxX,Convert.ToDouble(p.X));
					pmaxY = Math.Max(pmaxY,Convert.ToDouble(p.Y));
				}
			}

			double[] coords = new double[]{pminX,pminY,pmaxX,pmaxY};
			return coords;
		}

		public static double[] DistWireToWire(Wire wire, Wire[] wires)
		{
			double sPdist = Double.PositiveInfinity;
			double ePdist = Double.PositiveInfinity;
			// Cycle through each wire except the current one and find the minimum distance
			// from the endpoints to each point in the wire
			for (int j=0; j<wires.Length; j++)
			{
				if (!(wire.Wirename.Equals(wires[j].Wirename)))
				{
					Sketch.Substroke[] wsubstrokes = wires[j].substrokes;
					Sketch.Point[][] wpoints = new Sketch.Point[wsubstrokes.Length][];
					for (int i=0; i<wsubstrokes.Length; i++)
					{
						wpoints[i] = wsubstrokes[i].Points;
					}
					for (int i=0; i<wsubstrokes.Length; i++)
					{
						foreach (Sketch.Point p in wpoints[i])
						{
							double tempdistp1 = Math.Sqrt(Math.Pow(Convert.ToDouble(p.X)-wire.P1.X,2)+Math.Pow(Convert.ToDouble(p.Y)-wire.P1.Y,2));
							double tempdistp2 = Math.Sqrt(Math.Pow(Convert.ToDouble(p.X)-wire.P2.X,2)+Math.Pow(Convert.ToDouble(p.Y)-wire.P2.Y,2));
							sPdist = Math.Min(sPdist,tempdistp1);
							ePdist = Math.Min(ePdist,tempdistp2);
						}
					}
				}
			}
			Console.WriteLine("sPdist " + sPdist + " ePdist " + ePdist);
			double[] mindists = new double[] {sPdist,ePdist};
			return mindists;
		}

		public static double[] DistWiretoGate(Wire currentwire, Gate[] gates)
		{
			double sPdist = Double.PositiveInfinity;
			double ePdist = Double.PositiveInfinity;
			// Cycle through each gate except the current one and find the minimum distance
			// from the endpoints to each point in the gate
			for (int j=0; j<gates.Length; j++)
			{
				
				
				Sketch.Substroke[] gsubstrokes = gates[j].substrokes;
				Sketch.Point[][] gpoints = new Sketch.Point[gsubstrokes.Length][];
				for (int i=0; i<gsubstrokes.Length; i++)
				{
					gpoints[i] = gsubstrokes[i].Points;
				}
				for (int i=0; i<gsubstrokes.Length; i++)
				{
					foreach (Sketch.Point p in gpoints[i])
					{
						double tempdistp1 = Math.Sqrt(Math.Pow(Convert.ToDouble(p.X)-currentwire.P1.X,2)+Math.Pow(Convert.ToDouble(p.Y)-currentwire.P1.Y,2));
						double tempdistp2 = Math.Sqrt(Math.Pow(Convert.ToDouble(p.X)-currentwire.P2.X,2)+Math.Pow(Convert.ToDouble(p.Y)-currentwire.P2.Y,2));
						sPdist = Math.Min(sPdist,tempdistp1);
						ePdist = Math.Min(ePdist,tempdistp2);
					}
				}
			}
			
			Console.WriteLine("sPdist " + sPdist + " ePdist " + ePdist);
			double[] mindists = new double[] {sPdist,ePdist};
			return mindists;
		}

		public static double[] WireDistToPerim(Wire wire, Circuit circuit)
		{
			double P1toleft = Math.Abs(wire.P1.X-circuit.TopLeftX);
			double P1toright = Math.Abs(wire.P1.X-circuit.BottomRightX);
			double P1totop = Math.Abs(wire.P1.Y-circuit.TopLeftY);
			double P1tobottom = Math.Abs(wire.P1.Y-circuit.BottomRightY);

			double P2toleft = Math.Abs(wire.P2.X-circuit.TopLeftX);
			double P2toright = Math.Abs(wire.P2.X-circuit.BottomRightX);
			double P2totop = Math.Abs(wire.P2.Y-circuit.TopLeftY);
			double P2tobottom = Math.Abs(wire.P2.Y-circuit.BottomRightY);

			double[] perimvalues = new double[8]{P1toleft,P1toright,P1totop,P1tobottom,P2toleft,
													P2toright,P2totop,P2tobottom};

			Console.WriteLine("{0}: P1left: {1}, P1right: {2}, P1top: {3}, P1bot: {4}, P2left: {5}, P2right: {6}, P2top: {7}, P2bot: {8}",
				wire.Wirename,P1toleft,P1toright,P1totop,P1tobottom,P2toleft,P2toright,P2totop,P2tobottom);
			return perimvalues;
			/*
						// Not used now because other features were selected
						double[] perimvalues = new double[4];
						// First do this for the first end point of the wire
						double P1toleft = Math.Abs(wire.P1.X-circuit.TopLeftX);
						double P1toright = Math.Abs(wire.P1.X-circuit.BottomRightX);
						double P1totop = Math.Abs(wire.P1.Y-circuit.TopLeftY);
						double P1tobottom = Math.Abs(wire.P1.Y-circuit.BottomRightY);

						if (P1toleft <= P1toright && P1toleft <= P1totop && P1toleft <= P1tobottom)
						{
							perimvalues[0] = P1toleft;
							perimvalues[1] = 1;
						}
						else if (P1totop <= P1toright && P1totop <= P1tobottom)
						{
							perimvalues[0] = P1totop;
							perimvalues[1] = 2;
						}
						else if (P1toright <= P1tobottom)
						{
							perimvalues[0] = P1toright;
							perimvalues[1] = 3;
						}
						else
						{
							perimvalues[0] = P1tobottom;
							perimvalues[1] = 4;
						}

						// Next, do this for the second end point
						double P2toleft = Math.Abs(wire.P2.X-circuit.TopLeftX);
						double P2toright = Math.Abs(wire.P2.X-circuit.BottomRightX);
						double P2totop = Math.Abs(wire.P2.Y-circuit.TopLeftY);
						double P2tobottom = Math.Abs(wire.P2.Y-circuit.BottomRightY);

						if (P2toleft <= P2toright && P2toleft <= P2totop && P2toleft <= P2tobottom)
						{
							perimvalues[2] = P2toleft;
							perimvalues[3] = 1;
						}
						else if (P2totop <= P2toright && P2totop <= P2tobottom)
						{
							perimvalues[2] = P2totop;
							perimvalues[3] = 2;
						}
						else if (P2toright <= P2tobottom)
						{
							perimvalues[2] = P2toright;
							perimvalues[3] = 3;
						}
						else
						{
							perimvalues[2] = P2tobottom;
							perimvalues[3] = 4;
						}
						return perimvalues;
			*/
		}

		public static double DistToLabel(Wire wire, Label[] labels)
		{
			double p1tol = Double.PositiveInfinity;
			double p2tol = Double.PositiveInfinity;
			for (int countl=0; countl<labels.Length; countl++)
			{
				foreach (object point in labels[countl].LPoints)
				{
					Sketch.Point lpoint = (Sketch.Point)point;
					double p1localdist = Math.Sqrt(Math.Pow(wire.P1.X-(double)lpoint.X,2)+Math.Pow(wire.P1.Y-(double)lpoint.Y,2));
					double p2localdist = Math.Sqrt(Math.Pow(wire.P2.X-(double)lpoint.X,2)+Math.Pow(wire.P2.Y-(double)lpoint.Y,2));
					p1tol = Math.Min(p1tol,p1localdist);
					p2tol = Math.Min(p2tol,p2localdist);
				}
					
			}

			double mindistl = Math.Min(p1tol,p2tol);
			return mindistl;
			
		}



		#region Howard's Algorithms

		/// <summary>
		/// Used to get the points within a Shape.
		/// </summary>
		/// <param name="temp"></param>
		/// <returns></returns>
		public ArrayList GetPoints(Sketch.Shape temp)
		{
			//Creates an Arraylist to store all the points
			ArrayList datalist = new ArrayList();

			// Goes through the entire Shape and locates all the points within it
			foreach(Sketch.Substroke substrokedata in temp.Substrokes)
				foreach(Sketch.Point pointdata in substrokedata.PointsL)
					datalist.Add(pointdata);

			return datalist;		
		}

		public void DetermineEndPts3(Sketch.Shape temp)
		{
			int index = 0;
			ArrayList UIDlist = new ArrayList();
			ArrayList pointlist = new ArrayList();
			ArrayList pointlist_remain = new ArrayList();

			// Gets the UIDs of the Start and End points of SubStrokes
			foreach(Sketch.Substroke substrokedata in temp.Substrokes)
			{
				this.substroke_count++;
				UIDlist.Add(Convert.ToString(substrokedata.XmlAttrs.Start));
				UIDlist.Add(Convert.ToString(substrokedata.XmlAttrs.End));				
			}

			// Matches each UID to a specific Point
			foreach(String data in UIDlist)
				foreach(Sketch.Point point in this.points)
					if (data.Equals(Convert.ToString(point.XmlAttrs.Id)))
						pointlist.Add(point);

			// Determines if two Endpoints are too close to each other, then eliminate both of them.
			// (Helps identifies internal endpts)
			foreach(Sketch.Point cpoint in pointlist)
			{
				if (intersectcount((double)cpoint.X-substroke_margin, (double)cpoint.Y-substroke_margin,
					(double)cpoint.X+substroke_margin, (double)cpoint.Y+substroke_margin,
					pointlist) == 1)
					pointlist_remain.Add(cpoint);
			}

			this.EndPt_3 = new Sketch.Point[pointlist_remain.Count];
			this.EndPtExist_3 = new bool[pointlist_remain.Count];

			foreach( Sketch.Point point in pointlist_remain )
			{
				this.EndPt_3[index] = point;
				this.EndPtExist_3[index] = true;
				index++;
			}
		}

		/// <summary>
		/// Used to determine if a Point already exists within an ArrayList.
		/// </summary>
		/// <param name="datalist"></param>
		/// <param name="newPoint"></param>
		/// <returns></returns>
		private bool exists(ArrayList datalist, Sketch.Point newPoint)
		{
			foreach(Sketch.Point savedpoint in datalist)
				if (savedpoint.X == newPoint.X && savedpoint.Y == newPoint.Y)
					return true;

			return false;
		}

		private void FindBoundaries(Sketch.Shape wire)
		{
			// TopLeftX = Convert.ToInt16(wire.XmlAttrs.X);
			// TopLeftY = Convert.ToInt16(wire.XmlAttrs.Y);

			TopLeftX = Int16.MaxValue;
			TopLeftY = Int16.MaxValue;
			BottomRightX = Int16.MinValue;
			BottomRightY = Int16.MinValue;

			foreach( Sketch.Point point in this.points )
			{
				if ( point.X < TopLeftX )
					TopLeftX = Convert.ToInt16(point.X);

				if ( point.Y < TopLeftY )
					TopLeftY = Convert.ToInt16(point.Y);

				if ( point.X > BottomRightX )
					BottomRightX = Convert.ToInt16(point.X);
				
				if ( point.Y > BottomRightY )
					BottomRightY = Convert.ToInt16(point.Y);
			}

			//BottomRightX = TopLeftX + Convert.ToInt16(wire.XmlAttrs.Width);
			//	BottomRightY = TopLeftY + Convert.ToInt16(wire.XmlAttrs.Height);
		}

		private void DetermineEndPts1()
		{
			int intCount;
			ArrayList intPts;

			Console.WriteLine("Algorithm 1");
			Console.WriteLine(this.name + " aka " + this.ID);
	
			// Use 10% bounding box margin
			double marginX = (double)(BottomRightX - TopLeftX)*marginvalue; 
			double marginY = (double)(BottomRightY - TopLeftY)*marginvalue;
 
			#region Algorithm 1

			// Create TopLeft Bounding Box
			intCount = intersectcount((double)TopLeftX, (double)TopLeftY, 
				(double)TopLeftX+marginX, (double)TopLeftY+marginY, points, out intPts);

			Console.WriteLine("TL Bounding Box ({0},{1}) - ({2},{3})", 
				TopLeftX, TopLeftY, TopLeftX+marginX, TopLeftY+marginY);

			if (intCount > 0)
			{
				Sketch.Point cPoint = null;
				double cPointX = TopLeftX + marginX;
				double cPointY = TopLeftY + marginY;
				double cDistance = Math.Sqrt(Math.Pow((cPointX-TopLeftX),2)+Math.Pow((cPointY-TopLeftY),2));

				foreach(Sketch.Point point in intPts)
				{
					// Console.WriteLine("({0},{1})", Convert.ToString(point.X), Convert.ToString(point.Y));

					double Distance = Math.Sqrt(Math.Pow((point.X-TopLeftX),2)+Math.Pow((point.Y-TopLeftY),2));

					//if (point.X <= cPointX | point.Y <= cPointY)
					if ( Distance < cDistance )
					{
						cPoint = point;
						cPointX = Convert.ToInt16(point.X);
						cPointY = Convert.ToInt16(point.Y);
						cDistance = Distance;
					}
				}				
				EndPt_1[0] = cPoint;
				EndPtExist_1[0] = true;
				
			}


			// Look at TopRight Bounding Box
			intCount = intersectcount((double)BottomRightX-marginX, (double)TopLeftY, 
				(double)BottomRightX, (double)TopLeftY+marginY, points, out intPts);

			Console.WriteLine("TR Bounding Box ({0},{1}) - ({2},{3})", 
				BottomRightX-marginX, TopLeftY, BottomRightX, TopLeftY+marginY);

			if (intCount > 0)
			{
				Sketch.Point cPoint = null;
				double cPointX = BottomRightX - marginX;
				double cPointY = TopLeftY + marginY;
				double cDistance = Math.Sqrt(Math.Pow((cPointX-BottomRightX),2)+Math.Pow((cPointY-TopLeftY),2));

				foreach(Sketch.Point point in intPts)
				{
					double Distance = Math.Sqrt(Math.Pow((point.X-BottomRightX),2)+Math.Pow((point.Y-TopLeftY),2));

					// if (point.X >= cPointX | point.Y <= cPointY)
					if ( Distance < cDistance )
					{
						cPoint = point;
						cPointX = Convert.ToInt16(point.X);
						cPointY = Convert.ToInt16(point.Y);
						cDistance = Distance;
					}
				}
				
				EndPt_1[1] = cPoint;
				EndPtExist_1[1] = true;
			}

			// Look at Bottom Left Bounding Box
			intCount = intersectcount((double)TopLeftX, (double)BottomRightY-marginY, 
				(double)TopLeftX+marginX, (double)BottomRightY, points, out intPts);

			Console.WriteLine("BL Bounding Box ({0},{1}) - ({2},{3})", 
				(double)TopLeftX, (double)BottomRightY-marginY, 
				(double)TopLeftX+marginX, (double)BottomRightY);

			if (intCount > 0)
			{
				Sketch.Point cPoint = null;
				double cPointX = TopLeftX + marginX;
				double cPointY = BottomRightY - marginY;
				double cDistance = Math.Sqrt(Math.Pow((cPointX-TopLeftX),2)+Math.Pow((cPointY-BottomRightY),2));

				foreach(Sketch.Point point in intPts)
				{
					double Distance = Math.Sqrt(Math.Pow((point.X-TopLeftX),2)+Math.Pow((point.Y-BottomRightY),2));
					
					// if (point.X <= cPointX | point.Y >= cPointY)
					if ( Distance < cDistance )
					{
						cPoint = point;
						cPointX = Convert.ToInt16(point.X);
						cPointY = Convert.ToInt16(point.Y);
						cDistance = Distance;
					}
				}
				
				EndPt_1[2] = cPoint;
				EndPtExist_1[2] = true;
			}

			// Look at Bottom Right Bounding Box
			intCount = intersectcount((double)BottomRightX-marginX, (double)BottomRightY-marginY, 
				(double)BottomRightX, (double)BottomRightY, points, out intPts);

			Console.WriteLine("BR Bounding Box ({0},{1}) - ({2},{3})", 
				(double)BottomRightX-marginX, (double)BottomRightY-marginY, 
				(double)BottomRightX, (double)BottomRightY);

			if (intCount > 0)
			{
				Sketch.Point cPoint = null;
				double cPointX = BottomRightX - marginX;
				double cPointY = BottomRightY - marginY;
				double cDistance = Math.Sqrt(Math.Pow((cPointX-BottomRightX),2)+Math.Pow((cPointY-BottomRightY),2));

				foreach(Sketch.Point point in intPts)
				{
					double Distance = Math.Sqrt(Math.Pow((point.X-BottomRightX),2)+Math.Pow((point.Y-BottomRightY),2));

					// if (point.X >= cPointX | point.Y >= cPointY)
					if ( Distance < cDistance )
					{
						cPoint = point;
						cPointX = Convert.ToInt16(point.X);
						cPointY = Convert.ToInt16(point.Y);
						cDistance = Distance;
					}
				}
				
				EndPt_1[3] = cPoint;
				EndPtExist_1[3] = true;
			}

			#endregion			
		}


		private void DetermineEndPts2()
		{
			int intCount;
			ArrayList intPts;

			Console.WriteLine("\nAlgorithm 2");
			Console.WriteLine(this.name + " aka " + this.ID);

			// Use 10% bounding box margin
			double marginX = (double)(BottomRightX - TopLeftX)*marginvalue; 
			double marginY = (double)(BottomRightY - TopLeftY)*marginvalue;

			#region Algorithm 2
			// If the wire is LR, look for end-pt on left-side and right-side
			if (Orientation == 1)
			{
				ArrayList x_values = new ArrayList();
				double avg_dist_x = 0;

				// Look for the end pt on the left side
				intCount = intersectcount((double)TopLeftX, (double)TopLeftY, 
					(double)TopLeftX+marginX, (double)BottomRightY, points, out intPts);

				// Take all the intersecting points and find the average X value
				foreach ( Sketch.Point point in intPts )
					x_values.Add(Convert.ToDouble(point.X));

				if (x_values.Count > 1 )
				{
					x_values.Sort();
					double previous_pt = Convert.ToDouble(x_values[x_values.Count - 1]);
                    
					for( int index = x_values.Count - 2; index >= 0; index-- )
					{
						avg_dist_x += previous_pt - Convert.ToDouble(x_values[index]);
						previous_pt = Convert.ToDouble(x_values[index]);
					}

					avg_dist_x = avg_dist_x / x_values.Count;
				}

				Console.WriteLine("avg_dist_x: {0}", avg_dist_x);

				Console.WriteLine("Left Bounding Box ({0},{1}) - ({2},{3})", 
					(double)TopLeftX, (double)TopLeftY, 
					(double)TopLeftX+marginX, (double)BottomRightY);

				if (intCount > 0)
				{
					Sketch.Point cPoint = null;
					double cPointX = TopLeftX + marginX;
					double cPointY = -100;

					foreach(Sketch.Point point in intPts)
					{
						if (point.X <= cPointX)
						{
							cPoint = point;
							cPointX = Convert.ToInt16(point.X);
							cPointY = Convert.ToInt16(point.Y);
						}
					}
			
					EndPt_2[0] = cPoint;
					EndPtExist_2[0] = true;
				}

				// Look for the end pt on the right side
				intCount = intersectcount((double)BottomRightX-marginX, (double)TopLeftY, 
					(double)BottomRightX, (double)BottomRightY, points, out intPts);

				Console.WriteLine("Right Bounding Box ({0},{1}) - ({2},{3})", 
					(double)BottomRightX-marginX, (double)TopLeftY, 
					(double)BottomRightX, (double)BottomRightY);

				if (intCount > 0)
				{
					Sketch.Point cPoint = null;
					double cPointX = BottomRightX - marginX;
					double cPointY = -100;

					foreach(Sketch.Point point in intPts)
					{
						if (point.X >= cPointX)
						{
							cPoint = point;
							cPointX = Convert.ToInt16(point.X);
							cPointY = Convert.ToInt16(point.Y);
						}
					}
		
					EndPt_2[2] = cPoint;
					EndPtExist_2[2] = true;
				}	
			}
			else // orientation = 2
			{
				double Total_Y = 0;
				double Avg_Y = 0;
				double Stdev_Y = 0;

				// Look at Top Bounding Box
				intCount = intersectcount((double)TopLeftX, (double)TopLeftY, 
					(double)BottomRightX, (double)TopLeftY+marginY, points, out intPts);

				// Take all the intersecting points and find the average X value
				foreach ( Sketch.Point point in intPts )
					Total_Y += Convert.ToDouble(point.Y);

				Avg_Y = Total_Y / intPts.Count;
				
				// Calculates the stdev
				foreach ( Sketch.Point point in intPts )
					Stdev_Y = Stdev_Y + Math.Pow((point.Y - Avg_Y),2);
				
				Stdev_Y = Math.Sqrt(Stdev_Y / intPts.Count );

				Console.WriteLine("Avg_Y: {0}, Stdev_Y: {1}", Avg_Y, Stdev_Y);


				Console.WriteLine("Top Bounding Box ({0},{1}) - ({2},{3})", 
					(double)TopLeftX, (double)TopLeftY, 
					(double)BottomRightX, (double)TopLeftY+marginY);

				if (intCount > 0)
				{
					Sketch.Point cPoint = null;
					double cPointX = -100;
					double cPointY = BottomRightY + marginY;

					foreach(Sketch.Point point in intPts)
					{
						if (point.Y <= cPointY)
						{
							cPoint = point;
							cPointX = Convert.ToInt16(point.X);
							cPointY = Convert.ToInt16(point.Y);
						}
					}
		
					EndPt_2[1] = cPoint;
					EndPtExist_2[1] = true;
				}

				// Look at Bottom Bounding Box
				intCount = intersectcount((double)TopLeftX, (double)BottomRightY-marginY, 
					(double)BottomRightX, (double)BottomRightY, points, out intPts);

				Console.WriteLine("BR Bounding Box ({0},{1}) - ({2},{3})", 
					(double)TopLeftX, (double)BottomRightY-marginY, 
					(double)BottomRightX, (double)BottomRightY);

				if (intCount > 0)
				{
					Sketch.Point cPoint = null;
					double cPointX = -100;
					double cPointY = BottomRightY - marginY;

					foreach(Sketch.Point point in intPts)
					{
						if (point.Y >= cPointY)
						{
							cPoint = point;
							cPointX = Convert.ToInt16(point.X);
							cPointY = Convert.ToInt16(point.Y);
						}
					}
		
					EndPt_2[3] = cPoint;
					EndPtExist_2[3] = true;
				}
			}
			#endregion

			Console.WriteLine();
		}

		private void BestEndPts()
		{
			// Using ArrayList to make it easier to perform certain operations
			ArrayList Collection = new ArrayList();
			ArrayList Occurrence = new ArrayList();

			// Flags for checking
			bool flag1 = true;
			bool flag2 = true;
			bool flag3 = true;
			int count = 0;

			// Flag to see if there are any matches
			int match_count = 0;

			// Check to make sure that there are at least 2 endpts from each algorithm
			// Checking algorithm 1, count number of trues
			for (int index = 0; index < EndPtExist_1.Length; index++)
				if (EndPtExist_1[index])
					count++;

			if (count < 2) flag1 = false;
			count = 0;

			// Checking algorithm 2
			for (int index = 0; index < EndPtExist_2.Length; index++)
				if (EndPtExist_2[index])
					count++;

			if (count < 2) flag2 = false;
			count = 0;

			// Checking algorithm 3
			for (int index = 0; index < EndPtExist_3.Length; index++)
				if (EndPtExist_3[index])
					count++;

			if (count < 2) flag3 = false;

			// Only if all algorithms have more than 2 endpts, then continue.
			if (flag1 & flag2 & flag3)
			{
				// Goes through and adds all the endpoints into a single list.
				for (int index = 0; index < EndPtExist_1.Length; index++)
					if (EndPtExist_1[index])
					{
						Collection.Add(EndPt_1[index]);
						Occurrence.Add(1);
					}

				// Starting from the second list, start counting occurrences
				for (int index = 0; index < EndPtExist_2.Length; index++)
				{
					if (EndPtExist_2[index])
					{
						int Repeat = Contain((Sketch.Point)EndPt_2[index], Collection);
					
						if (Repeat != -1)
						{
							// If the point already exists, then just increment Occurrence
							Occurrence[Repeat] = (int)Occurrence[Repeat] + 1;
						}
						else
						{
							// If the point does not exist, then add the point to the list
							Collection.Add(EndPt_2[index]);
							Occurrence.Add(1);
						}
					}
				}

				for (int index = 0; index < EndPtExist_3.Length; index++)
				{
					if (EndPtExist_3[index])
					{
						int Repeat = Contain((Sketch.Point)EndPt_3[index], Collection);

						if (Repeat != -1)
						{
							// If the point already exists, then just increment Occurrence
							Occurrence[Repeat] = (int)Occurrence[Repeat] + 1;
						}
						else
						{
							// If the point does not exist, then add the point to the list
							Collection.Add(EndPt_3[index]);
							Occurrence.Add(1);
						}
					}
				}

				// Now sort the ArrayList to find the top two occuring Endpoints.
				int[] Sorted_Occurrence = (int[])Occurrence.ToArray(typeof(int));
				Sketch.Point[] Sorted_Collection = (Sketch.Point[])Collection.ToArray(typeof(Sketch.Point));

				Array.Sort(Sorted_Occurrence, Sorted_Collection);

				for(int index = 0; index < Sorted_Collection.Length; index++)
				{
					Sketch.Point point = Sorted_Collection[index];
					Console.WriteLine("({0},{1}) - {2}", point.X, point.Y, Sorted_Occurrence[index].ToString());
				}

				// Go through the Occurrence ArrayList to see if there are any matches, if not
				// default to Algorithm 3
				foreach( int num in Occurrence)
					if (num > 1) match_count++;

				// Only choose the most occuring if there are more than 2 matches
				if (match_count > 1)
				{
					// Resizing, because we know for sure how long Endpt is.
					// We are only going to take the top two.
					this.EndPt = new Sketch.Point[2];
					this.EndPtExist = new bool[2];
					this.EndPtConnect = new bool[2];

					EndPt[0] = Sorted_Collection[Sorted_Collection.Length - 1];
					EndPt[1] = Sorted_Collection[Sorted_Collection.Length - 2];
					EndPtExist[0] = true;
					EndPtExist[1] = true;
				}
					// If there are no matches, default to Algorithm 3
				else
				{
					// Only makes sure that Algorithm 3 only gives the two extreme endpoints
					if (this.EndPt_3.Length > 2)
					{
						this.EndPt[0] = this.EndPt_3[0];
						this.EndPt[1] = this.EndPt_3[this.EndPt_3.Length-1];
						this.EndPtExist[0] = this.EndPtExist_3[0];
						this.EndPtExist[1] = this.EndPtExist_3[this.EndPt_3.Length-1];
						this.EndPtConnect = new bool[EndPt.Length];
					}
					else
					{
						this.EndPt = this.EndPt_3;
						this.EndPtExist = this.EndPtExist_3;
						this.EndPtConnect = new bool[EndPt.Length];
					}
				}
			}
			else
			{
				// Default to Algorithm 3 first
				if (flag3)
				{
					if (this.EndPt_3.Length > 2)
					{
						this.EndPt[0] = this.EndPt_3[0];
						this.EndPt[1] = this.EndPt_3[this.EndPt_3.Length-1];
						this.EndPtExist[0] = this.EndPtExist_3[0];
						this.EndPtExist[1] = this.EndPtExist_3[this.EndPt_3.Length-1];
						this.EndPtConnect = new bool[EndPt.Length];
					}
					else
					{
						this.EndPt = this.EndPt_3;
						this.EndPtExist = this.EndPtExist_3;
						this.EndPtConnect = new bool[EndPt.Length];
					}
				}
					// then defaults to Algorithm 1
				else if (flag1)
				{
					this.EndPt = this.EndPt_1;
					this.EndPtExist = this.EndPtExist_1;
					this.EndPtConnect = new bool[EndPt_1.Length];
				}
					// Finally uses Algorithm 2, but displays warning
				else // if (flag2)
				{
					Console.WriteLine("WARNING! EndPoints may not be accurate enough");
					this.EndPt = this.EndPt_2;
					this.EndPtExist = this.EndPtExist_2;
					this.EndPtConnect = new bool[EndPt_2.Length];
				}
			}			
		}

		private int Contain( Sketch.Point point, ArrayList list)
		{
			int index = 0;

			foreach(Sketch.Point cpoint in list)
			{
				if( cpoint.X == point.X & cpoint.Y == point.Y)
					return index;
				index++;
			}

			return -1;
		}

		private int intersectcount(double TL_X, double TL_Y, double BR_X, double BR_Y, 
			ArrayList points)
		{
			int pointcount = 0;

			foreach(Sketch.Point cPoint in points)
				if (cPoint.X <= BR_X & cPoint.X >= TL_X & cPoint.Y <= BR_Y & cPoint.Y >= TL_Y)
					pointcount++;
			
			return pointcount;
		}

		private int intersectcount(double TL_X, double TL_Y, double BR_X, double BR_Y, 
			ArrayList points, out ArrayList intersect)
		{
			int pointcount = 0;
			intersect = new ArrayList();

			foreach(Sketch.Point cPoint in points)
				if (cPoint.X <= BR_X & cPoint.X >= TL_X & cPoint.Y <= BR_Y & cPoint.Y >= TL_Y)
				{
					intersect.Add(cPoint);
					pointcount++;
				}
			
			return pointcount;
		}

		private void DetermineOrientation()
		{
			// LR if width > height, else TB
			if ( (BottomRightX - TopLeftX) > (BottomRightY - TopLeftY))
				Orientation = 1;
			else
				Orientation = 2;
		}

		#endregion Howard's Algorithms

		#endregion METHODS

		#region GETTERS & SETTERS

		public string Ioro
		{
			get
			{
				return this.Io;
			}

			set
			{
				this.Io = value;
			}
		}

		public string Wirename
		{
			get
			{
				return this.Name;
			}

			set
			{
				this.Name = value;
			}
		}
		
		public double X
		{
			get
			{
				return this.TopLeftX;
			}

			set
			{
				this.TopLeftX = value;
			}
		}

		public double Y
		{
			get
			{
				return this.TopLeftY;
			}

			set
			{
				this.TopLeftY = value;
			}
		}

		public int High
		{
			get
			{
				return this.Height;
			}

			set
			{
				this.Height = value;
			}
		}

		public int Wide
		{
			get
			{
				return this.Width;
			}

			set
			{
				this.Width = value;
			}
		}

		public string ID
		{
			get
			{
				return this.Id;
			}

			set
			{
				this.Id = value;
			}
		}
		#endregion GETTERS & SETTERS


		
	}
}
