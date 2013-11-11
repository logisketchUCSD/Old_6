using System;
using System.Collections;
using ConverterXML;

namespace IOTrain
{
	/// <summary>
	/// Summary description for DetermineIO.
	/// </summary>
	public class DetermineIO
	{

		public static int SetIO(Wire wire, Wire[] wires, Circuit circuit, Gate[] gates, Label[] labels,
			double maxdistwire, double maxdistgate, double maxdistlabel,
			NeuralNets.BackProp bpNetwork)
		{
			#region FEATURE EXTRACTION

			// Find the distance of the wire endpoints from the perimeter lines
			
			double[] perimvalues = Wire.WireDistToPerim(wire,circuit);
			wire.P1toleftperim = perimvalues[0];
			wire.P1torightperim = perimvalues[1];
			wire.P1totopperim = perimvalues[2];
			wire.P1tobotperim = perimvalues[3];
			wire.P2toleftperim = perimvalues[4];
			wire.P2torightperim = perimvalues[5];
			wire.P2totopperim = perimvalues[6];
			wire.P2tobotperim = perimvalues[7];

			// Find the distance of the wire endpoints from the from the bounding lines for each
			// gate, and choose the smallest value
			
			
			double[] w2gdists = Wire.DistWiretoGate(wire,gates);
			wire.P1mindistgate = w2gdists[0];
			wire.P2mindistgate = w2gdists[1];

			// Find the closest distance to other wires; check the distance of both ends of the wire
			// to the ends of all other wires and three intermediate points in each wire
			
			
			double[] wiredists = Wire.DistWireToWire(wire,wires);
			wire.P1mindistwire = wiredists[0];
			wire.P2mindistwire = wiredists[1];

			// Find the closest distance to labels; check the distance of every point of the
			// wire and label

			
			double mindistl = Wire.DistToLabel(wire,labels);
			wire.mindistlabel = mindistl;

			#endregion FEATURE EXTRACTION

			#region DATA NORMALIZATION

			double circuitwidth = circuit.BottomRightX-circuit.TopLeftX;
			double circuitheight = circuit.BottomRightY-circuit.TopLeftY;

			wire.mindistlabel = wire.mindistlabel/maxdistlabel;
			wire.P1mindistgate = wire.P1mindistgate/maxdistgate;
			wire.P1mindistwire = wire.P1mindistwire/maxdistwire;
			wire.P2mindistgate = wire.P2mindistgate/maxdistgate;
			wire.P2mindistwire = wire.P2mindistwire/maxdistwire;

			wire.P1tobotperim = wire.P1tobotperim/circuitheight;
			wire.P1toleftperim = wire.P1toleftperim/circuitwidth;
			wire.P1torightperim = wire.P1torightperim/circuitwidth;
			wire.P1totopperim = wire.P1totopperim/circuitheight;

			wire.P2tobotperim = wire.P2tobotperim/circuitheight;
			wire.P2toleftperim = wire.P2toleftperim/circuitwidth;
			wire.P2torightperim = wire.P2torightperim/circuitwidth;
			wire.P2totopperim = wire.P2totopperim/circuitheight;
			
			double[] input = new double[]{wire.P1toleftperim,
											 wire.P1torightperim,wire.P1totopperim,wire.P1tobotperim,wire.P2toleftperim,
											 wire.P2torightperim,wire.P2totopperim,wire.P2tobotperim,
											 wire.P1mindistgate,wire.P1mindistwire,wire.P2mindistgate,
											 wire.P2mindistwire,wire.mindistlabel};
			ArrayList inputAL = new ArrayList(input);

			#endregion DATA NORMALIZATION

			double[] outputs = bpNetwork.Run(inputAL);

			// Based on the output of the neural net, it selects 1 if
			// the output corresponding to an input wire is closer to one
			// than the other two possibilities.  If it is not, then it selects 2
			// if the output corresponding to an output wire is closer to one than
			// the internal wire possibility, and if both of these conditions are false
			// it chooses 3 for internal wire

			int inpdist = (int)Math.Round(outputs[0]);
			int outdist = (int)Math.Round(outputs[1]);
			int intdist = (int)Math.Round(outputs[2]);
			int output;
			if (inpdist == 1 && outdist != 1 && intdist != 1)
				output = 1;
			else if (inpdist != 1 && outdist == 1 && intdist != 1)
				output = 2;
			else
				output = 3;
			
			return output;
		}

		public static double[] NormParams(Wire[] wires, Circuit circuit)
		{
				
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

			double[] dists = new double[]{maxdistwire,maxdistgate,maxdistlabel};
			return dists;
		}


		
	}
}
