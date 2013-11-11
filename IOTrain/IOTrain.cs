/**
 * File: CircuitRec.cs
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
using ZedGraph;
using System.IO;
using System.Windows.Forms;

namespace IOTrain
{
	/// <summary>
	/// Summary description for Class1.
	/// </summary>
	class IOTrain
	{
		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		[STAThread]
		static void Main(string[] args)
		{
			Console.Write("Type 'runbp' to run backpropagation algorithm on trainingdata.in: ");
			string run = Console.ReadLine();
			if (run.Equals("runbp"))
			{
                //To test the neural net, add in this argument to inputargs: "-test",.bp file to be tested in "",
                string[] inputargs = new string[]{"-i","../../../CircuitRec/Training Data 17/trainingdata17.in","-nn","quickest17.nn",
													 "-l","200000","-e",".0000001","-m","0","-n",".006"};
				BackProp.BPTrain(inputargs);
			}
			else
			{
				Console.Write("Input File Name (without .xml): ");
				string filename = Console.ReadLine();
				// Loads the sketch files;
				ConverterXML.ReadXML xml = new ConverterXML.ReadXML("../../data/Test Data/" + filename + ".xml");

				// Takes the loaded sketch file and places it into a temp Sketch variable
				Sketch.Sketch temp = new Sketch.Sketch(xml.Sketch);

				// Gets the shapes from the sketch file
				Sketch.Shape[] tempshape = temp.Shapes;

				#region FEATURE EXTRACTION

				// "Magic" numbers
				// Used for wire boundary marginvalue when detecting endpoints
				// Range of Values are [0, 1]
				double wire_marginvalue = 0.2;
				// Range is in inkspace
				double substroke_margin = 100;
			
				Circuit circuit = new Circuit(tempshape);
				Console.WriteLine("wirecount: {0}, gatecount: {1}",circuit.WireCount,circuit.GateCount);
				Wire[] wires = new Wire[circuit.WireCount];
				Gate[] gates = new Gate[circuit.GateCount];
				Label[] labels = new Label[circuit.LabelCount];
				int wirecount = 0;
				int gatecount = 0;
				int labelcount = 0;
				foreach (Sketch.Shape shape in tempshape)
				{
					string shapetype = Convert.ToString(shape.XmlAttrs.Type);
					if (shapetype.Equals("Wire"))
					{
						wires[wirecount] = new Wire(shape,wire_marginvalue,substroke_margin);
						wires[wirecount].Wirename = "wire" + Convert.ToString(wirecount);
						wirecount = wirecount+1;
					}
					if (!(shapetype.Equals("Wire")) && !(shapetype.Equals("Label")) && !(shapetype.Equals("Other")) && !(shapetype.Equals("Text")))
					{					
						gates[gatecount] = new Gate(shape);
						gates[gatecount].Gatename = "gate" + Convert.ToString(gatecount);
						gatecount = gatecount+1;
					}
					if ((shapetype.Equals("Label")))
					{
						labels[labelcount] = new Label(shape);
						labelcount++;
					}
				
				}
                
				// For Debug
				foreach(Gate g in gates)
					Console.WriteLine(g.Gatename + "  " + g.Gatetype);

				// Find the distance of the wire endpoints from the perimeter lines
			
				for (int count=0; count<wires.Length; count++)
				{
					double[] perimvalues = Wire.WireDistToPerim(wires[count],circuit);
					wires[count].P1toleftperim = perimvalues[0];
					wires[count].P1torightperim = perimvalues[1];
					wires[count].P1totopperim = perimvalues[2];
					wires[count].P1tobotperim = perimvalues[3];
					wires[count].P2toleftperim = perimvalues[4];
					wires[count].P2torightperim = perimvalues[5];
					wires[count].P2totopperim = perimvalues[6];
					wires[count].P2tobotperim = perimvalues[7];
				}

				// Find the distance of the wire endpoints from the from the bounding lines for each
				// gate, and choose the smallest value
			
				for (int count=0; count<wires.Length; count++)
				{
					double[] w2gdists = Wire.DistWiretoGate(wires[count],gates);
					wires[count].P1mindistgate = w2gdists[0];
					wires[count].P2mindistgate = w2gdists[1];
				}

				// Find the closest distance to other wires; check the distance of both ends of the wire
				// to the ends of all other wires and three intermediate points in each wire
			
				for (int count=0; count<wires.Length; count++)
				{
					double[] wiredists = Wire.DistWireToWire(wires[count],wires);
					wires[count].P1mindistwire = wiredists[0];
					wires[count].P2mindistwire = wiredists[1];
				}

				// Find the closest distance to labels; check the distance of every point of the
				// wire and label

				for (int count=0; count<wires.Length; count++)
				{
					double mindistl = Wire.DistToLabel(wires[count],labels);
					wires[count].mindistlabel = mindistl;
					//Console.WriteLine("Label Dist to {0}: {1}",wires[count].Wirename,wires[count].mindistlabel);
				}

				#endregion FEATURE EXTRACTION

				#region DATA NORMALIZATION
				
				double circuitwidth = circuit.BottomRightX-circuit.TopLeftX;
				double circuitheight = circuit.BottomRightY-circuit.TopLeftY;
				//Console.WriteLine(circuitwidth);
				
				double maxdistgate = Double.NegativeInfinity;
				double maxdistwire = Double.NegativeInfinity;
				double maxdistlabel = Double.NegativeInfinity;

				for (int i=0; i<wires.Length; i++)
				{
					double tempmaxdist = Math.Max(wires[i].P1mindistwire,wires[i].P2mindistwire);
					maxdistwire = Math.Max(maxdistwire,tempmaxdist);
					tempmaxdist = Math.Max(wires[i].P1mindistgate,wires[i].P2mindistgate);
					maxdistgate = Math.Max(maxdistgate,tempmaxdist);
					maxdistlabel = Math.Max(maxdistlabel,wires[i].mindistlabel);
				}

				//Console.WriteLine("Max dist for: Wire: {0}, Gate: {1}, Label: {2}",maxdistwire,maxdistgate,maxdistlabel);

                // Normalize the features.  If there are no labels, set mindistlabel to .5 so that no NaN's get into training data.
				for (int count=0; count<wires.Length; count++)
				{
					if (labelcount == 0)
					{
						wires[count].mindistlabel = .5;
					}
					else
					{
						wires[count].mindistlabel = wires[count].mindistlabel/maxdistlabel;
					}

                    // Normalize features
					wires[count].P1mindistgate = wires[count].P1mindistgate/maxdistgate;
					wires[count].P1mindistwire = wires[count].P1mindistwire/maxdistwire;
					wires[count].P2mindistgate = wires[count].P2mindistgate/maxdistgate;
					wires[count].P2mindistwire = wires[count].P2mindistwire/maxdistwire;

					wires[count].P1tobotperim = wires[count].P1tobotperim/circuitheight;
					wires[count].P1toleftperim = wires[count].P1toleftperim/circuitwidth;
					wires[count].P1torightperim = wires[count].P1torightperim/circuitwidth;
					wires[count].P1totopperim = wires[count].P1totopperim/circuitheight;

					wires[count].P2tobotperim = wires[count].P2tobotperim/circuitheight;
					wires[count].P2toleftperim = wires[count].P2toleftperim/circuitwidth;
					wires[count].P2torightperim = wires[count].P2torightperim/circuitwidth;
					wires[count].P2totopperim = wires[count].P2totopperim/circuitheight;

                    // Debug
                    Console.WriteLine("{0}", wires[count].Wirename);
                    Console.WriteLine("P1 Perimeter Distances: L={0}, R={1}, T={2}, B={3}",
                                      wires[count].P1tobotperim, wires[count].P1torightperim, wires[count].P1totopperim,
                                      wires[count].P1tobotperim);
                    Console.WriteLine("\nP2 Perimeter Distances: L={0}, R={1}, T={2}, B={3}",
                                      wires[count].P2tobotperim, wires[count].P2torightperim, wires[count].P2totopperim,
                                      wires[count].P2tobotperim);
                    Console.WriteLine("\nP1 Minimum Distances to: Wire={0}, Gate={1}",
                                      wires[count].P1mindistwire, wires[count].P1mindistgate);
                    Console.WriteLine("\nP2 Minimum Distances to: Wire={0}, Gate={1}",
                                      wires[count].P2mindistwire, wires[count].P2mindistgate);
                    Console.WriteLine("\nMinimum Distance to label: {0}", wires[count].mindistlabel);
                    Console.WriteLine("\n-----------------\n");
				}
                Console.ReadLine();
                

				#endregion DATA NORMALIZATION

				#region CREATE PLOTS

				// For Debug: Plots
				// Get points for wires, gates, and labels

				// Get Wire Points
				ArrayList wireptsAL = new ArrayList();
				ArrayList wirenamesAL = new ArrayList();
				for (int count=0; count<wires.Length; count++)
				{
					PointPairList wirepoints = new PointPairList();
					Sketch.Substroke[] substrokesgraph = wires[count].substrokes;
					Sketch.Point[][] pointsgraph = new Sketch.Point[substrokesgraph.Length][];
					for (int i=0; i<substrokesgraph.Length; i++)
					{
						pointsgraph[i] = substrokesgraph[i].Points;
				
						foreach (Sketch.Point p in pointsgraph[i])
						{
							wirepoints.Add(Convert.ToDouble(p.X),-Convert.ToDouble(p.Y),wires[count].Wirename);
						}
					}

					wireptsAL.Add(wirepoints);
					wirenamesAL.Add(wires[count].Wirename);
				}

				// Get Gate Points
				ArrayList gatepointsAL = new ArrayList();
				
				for (int count=0; count<gates.Length; count++)
				{
					PointPairList gatepoints = new PointPairList();
					Sketch.Substroke[] substrokesgraph = gates[count].substrokes;
					Sketch.Point[][] pointsgraph = new Sketch.Point[substrokesgraph.Length][];
					for (int i=0; i<substrokesgraph.Length; i++)
					{
						pointsgraph[i] = substrokesgraph[i].Points;
				
						foreach (Sketch.Point p in pointsgraph[i])
						{
							gatepoints.Add(Convert.ToDouble(p.X),-Convert.ToDouble(p.Y));
						}
					}
					gatepointsAL.Add(gatepoints);
				}
			
				// Get End Points
				ArrayList endpointsAL = new ArrayList();
				for (int count=0; count<wires.Length; count++)
				{
					PointPairList endpoints = new PointPairList();
					endpoints.Add(wires[count].P1.X,-wires[count].P1.Y);
					endpoints.Add(wires[count].P2.X,-wires[count].P2.Y);
					endpointsAL.Add(endpoints);
				}

				// Get Label Points
				ArrayList labelpointsAL = new ArrayList();
				for (int count=0; count<labels.Length; count++)
				{
					PointPairList labelpoints = new PointPairList();
					foreach (Sketch.Point point in labels[count].LPoints)
					{
						labelpoints.Add((double)point.X,-(double)point.Y);
					}
					labelpointsAL.Add(labelpoints);
				}


				//Used to output the graphical representation of the gates and wires
				Form1 instance = new Form1(wireptsAL,gatepointsAL,endpointsAL,labelpointsAL,filename,wirenamesAL,wires);
				Application.Run(instance);
				

				#endregion CREATE PLOTS

				#region WRITE TRAINING DATA
				// Write an input file for the backpropagation training (outputs have to be prepended later)
				FileStream inputfile = new FileStream(filename + ".in",FileMode.Create,FileAccess.ReadWrite);
				StreamWriter sw = new StreamWriter(inputfile);
				sw.WriteLine("3 13");
				for (int i=0; i<wires.Length; i++)
				{
					if (wires[i].P1.Time >= wires[i].P2.Time)
					{
						Console.WriteLine("{0} is a 1) input, 2) output, 3) internal: ",wires[i].Wirename);
						string io = Console.ReadLine();
						if (io.Equals("1"))
						{
							sw.WriteLine("1 0 0 {0} {1} {2} {3} {4} {5} {6} {7} {8} {9} {10} {11} {12}",wires[i].P1toleftperim,
								wires[i].P1torightperim,wires[i].P1totopperim,wires[i].P1tobotperim,wires[i].P2toleftperim,
								wires[i].P2torightperim,wires[i].P2totopperim,wires[i].P2tobotperim,
								wires[i].P1mindistgate,wires[i].P1mindistwire,wires[i].P2mindistgate,
								wires[i].P2mindistwire,wires[i].mindistlabel);
						}
						else if (io.Equals("2"))
						{
							sw.WriteLine("0 1 0 {0} {1} {2} {3} {4} {5} {6} {7} {8} {9} {10} {11} {12}",wires[i].P1toleftperim,
								wires[i].P1torightperim,wires[i].P1totopperim,wires[i].P1tobotperim,wires[i].P2toleftperim,
								wires[i].P2torightperim,wires[i].P2totopperim,wires[i].P2tobotperim,
								wires[i].P1mindistgate,wires[i].P1mindistwire,wires[i].P2mindistgate,
								wires[i].P2mindistwire,wires[i].mindistlabel);
						}
						else if (io.Equals("3"))
						{
							sw.WriteLine("0 0 1 {0} {1} {2} {3} {4} {5} {6} {7} {8} {9} {10} {11} {12}",wires[i].P1toleftperim,
								wires[i].P1torightperim,wires[i].P1totopperim,wires[i].P1tobotperim,wires[i].P2toleftperim,
								wires[i].P2torightperim,wires[i].P2totopperim,wires[i].P2tobotperim,
								wires[i].P1mindistgate,wires[i].P1mindistwire,wires[i].P2mindistgate,
								wires[i].P2mindistwire,wires[i].mindistlabel);
						}
						else
						{
							sw.WriteLine("x x x {0} {1} {2} {3} {4} {5} {6} {7} {8} {9} {10} {11} {12}",wires[i].P1toleftperim,
								wires[i].P1torightperim,wires[i].P1totopperim,wires[i].P1tobotperim,wires[i].P2toleftperim,
								wires[i].P2torightperim,wires[i].P2totopperim,wires[i].P2tobotperim,
								wires[i].P1mindistgate,wires[i].P1mindistwire,wires[i].P2mindistgate,
								wires[i].P2mindistwire,wires[i].mindistlabel);
						}
					}
					else
					{
						Console.WriteLine("{0} is a 1) input, 2) output, 3) internal: ",wires[i].Wirename);
						string io = Console.ReadLine();
						if (io.Equals("1"))
						{
							sw.WriteLine("1 0 0 {0} {1} {2} {3} {4} {5} {6} {7} {8} {9} {10} {11} {12}",wires[i].P2toleftperim,
								wires[i].P2torightperim,wires[i].P2totopperim,wires[i].P2tobotperim,wires[i].P1toleftperim,
								wires[i].P1torightperim,wires[i].P1totopperim,wires[i].P1tobotperim,
								wires[i].P2mindistgate,wires[i].P2mindistwire,wires[i].P1mindistgate,
								wires[i].P1mindistwire,wires[i].mindistlabel);
						}
						else if (io.Equals("2"))
						{
							sw.WriteLine("0 1 0 {0} {1} {2} {3} {4} {5} {6} {7} {8} {9} {10} {11} {12}",wires[i].P2toleftperim,
								wires[i].P2torightperim,wires[i].P2totopperim,wires[i].P2tobotperim,wires[i].P1toleftperim,
								wires[i].P1torightperim,wires[i].P1totopperim,wires[i].P1tobotperim,
								wires[i].P2mindistgate,wires[i].P2mindistwire,wires[i].P1mindistgate,
								wires[i].P1mindistwire,wires[i].mindistlabel);
						}
						else if (io.Equals("3"))
						{
							sw.WriteLine("0 0 1 {0} {1} {2} {3} {4} {5} {6} {7} {8} {9} {10} {11} {12}",wires[i].P2toleftperim,
								wires[i].P2torightperim,wires[i].P2totopperim,wires[i].P2tobotperim,wires[i].P1toleftperim,
								wires[i].P1torightperim,wires[i].P1totopperim,wires[i].P1tobotperim,
								wires[i].P2mindistgate,wires[i].P2mindistwire,wires[i].P1mindistgate,
								wires[i].P1mindistwire,wires[i].mindistlabel);
						}
						else
						{
							sw.WriteLine("x x x {0} {1} {2} {3} {4} {5} {6} {7} {8} {9} {10} {11} {12}",wires[i].P2toleftperim,
								wires[i].P2torightperim,wires[i].P2totopperim,wires[i].P2tobotperim,wires[i].P1toleftperim,
								wires[i].P1torightperim,wires[i].P1totopperim,wires[i].P1tobotperim,
								wires[i].P2mindistgate,wires[i].P2mindistwire,wires[i].P1mindistgate,
								wires[i].P1mindistwire,wires[i].mindistlabel);
						}
					}

				}
				sw.Close();
				/*
								//Should not be needed anymore since plot shows wire names in text box
								//and when you hover over the wire
				
								// Write a file to record the x and y coordinates and names of the wires
								FileStream file = new FileStream("props_" + filename + ".txt",FileMode.Create,FileAccess.ReadWrite);
								StreamWriter sw2 = new StreamWriter(file);
								for (int i=0; i<wires.Length; i++)
								{
									if (wires[i].P1.Time >= wires[i].P2.Time)
									{
										sw2.WriteLine("Name: {0} P1X: {1} P1Y: {2} P2X: {3} P2Y: {4}",wires[i].Wirename,
											wires[i].P1.X,wires[i].P1.Y,wires[i].P2.X,wires[i].P2.Y);
									}
									else
									{
										sw2.WriteLine("Name: {0} P1X: {1} P1Y: {2} P2X: {3} P2Y: {4}",wires[i].Wirename,
											wires[i].P2.X,wires[i].P2.Y,wires[i].P1.X,wires[i].P1.Y);
									}
								}
								sw2.Close();
				*/

				#endregion WRITE TRAINING DATA

				

			
			}
		}
		
		
	}
}
