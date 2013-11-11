/**
 * File: LoopyBP.cs
 * 
 * Author: Max Pflueger.
 * Harvey Mudd College, Claremont, CA 91711.
 * Sketchers 2006.
 */

using System;
using System.Collections; 
using System.Collections.Specialized;
using System.Data;
using System.Diagnostics;
using System.Reflection;
using System.Resources;
using System.Runtime.InteropServices;
using System.IO;
using System.Text;
using System.Threading;

using mxComplexity = System.Int32;
using mit.ai.mrf;

namespace LoopyBP
{
	/// <summary>
	/// This class should provide access to the matlab function bploopyUse.
	/// </summary>
	public class LoopyBP
	{
		/******* DLL functions ********/
		[DllImport(@"libbploopyUse.dll")]
		private static extern void libbploopyUseInitialize();
		[DllImport(@"libbploopyUse.dll")]
		private static extern void libbploopyUseTerminate();
		[DllImport(@"libbploopyUse.dll")]
		private static extern void mlfBploopyUse(int num, ref IntPtr belBP, ref IntPtr belE, ref IntPtr logZ, [In]IntPtr G, [In]IntPtr pot, [In]IntPtr localEv);
		[DllImport(@"mclmcrrt72.dll")]
		private static extern IntPtr mxCreateDoubleScalar([In]double value);
		[DllImport(@"mclmcrrt72.dll")]
		private static extern IntPtr mxGetPr([In]IntPtr mxArray);
		[DllImport(@"mclmcrrt72.dll")]
		private static extern bool mclInitializeApplication(string options, int count);
		[DllImport(@"mclmcrrt72.dll")]
		private static extern void mclTerminateApplication();
		[DllImport(@"mclmcrrt72.dll")]
		private static extern IntPtr mxCreateDoubleMatrix([In]int m, [In]int n, [In]mxComplexity ComplexFlag);
		[DllImport(@"mclmcrrt72.dll")]
		private static extern IntPtr mxCreateCellMatrix([In]int m, [In]int n);
		[DllImport(@"mclmcrrt72.dll")]
		private static extern void mxSetCell([In]IntPtr array_ptr, [In]int index, [In]IntPtr value);
		[DllImport(@"mclmcrrt72.dll")]
		private static extern IntPtr mxGetCell([In]IntPtr array_ptr, [In]int index);


		/*
		[STAThread]
		/// <summary>
		/// For now this should demonstrate our ability to interface with the Matlab loopyBP DLL
		/// </summary>
		static void Main(string[] args)
		{

			double[] ans = new double[16];

			//***** Initialize arguments *****
			// Pretend this data came from our CRF
			//  The adjacency matrix
			int[][] GMat = new int[4][];
			for(int i=0; i<4; i++)
				GMat[i] = new int[4];
			GMat[0][0] = 0; GMat[0][1] = 1; GMat[0][2] = 1; GMat[0][3] = 1;
			GMat[1][0] = 1; GMat[1][1] = 0; GMat[1][2] = 0; GMat[1][3] = 1;
			GMat[2][0] = 1; GMat[2][1] = 0; GMat[2][2] = 0; GMat[2][3] = 0;
			GMat[3][0] = 1; GMat[3][1] = 1; GMat[3][2] = 0; GMat[3][3] = 0;

			//  The potential function (will be the same for all nodes)
			double[][] pot1 = new double[4][];
			for(int i=0; i<4; i++)
				pot1[i] = new double[4];
			pot1[0][0] = 0.1899; pot1[0][1] = 0.0007; pot1[0][2] = 0.0042; pot1[0][3] = 0.0161;
			pot1[1][0] = 0.1824; pot1[1][1] = 0.0270; pot1[1][2] = 0.0089; pot1[1][3] = 0.0086;
			pot1[2][0] = 0.0365; pot1[2][1] = 0.1435; pot1[2][2] = 0.0086; pot1[2][3] = 0.0001;
			pot1[3][0] = 0.1733; pot1[3][1] = 0.00002; pot1[3][2] = 0.0791; pot1[3][3] = 0.1210;
			double[][][] pot = new double[4][][];
			for (int i=0; i<4; i++)
				pot[i] = pot1;

			//  the local environment data
			double[][] localEvDATA = new double[4][];
			for(int i=0; i<4; i++)
				localEvDATA[i] = new double[4];
			localEvDATA[0][0] = 0.5828; localEvDATA[0][1] = 0.4329; localEvDATA[0][2] = 0.5298; localEvDATA[0][3] = 0.7833;
			localEvDATA[1][0] = 0.4235; localEvDATA[1][1] = 0.2259; localEvDATA[1][2] = 0.6405; localEvDATA[1][3] = 0.6808;
			localEvDATA[2][0] = 0.5155; localEvDATA[2][1] = 0.5798; localEvDATA[2][2] = 0.2091; localEvDATA[2][3] = 0.4611;
			localEvDATA[3][0] = 0.3340; localEvDATA[3][1] = 0.7604; localEvDATA[3][2] = 0.3798; localEvDATA[3][3] = 0.5678;

			//create a place for the output
			double[] belBP = new double[16];
			double[] belE = new double[4*16];
			double logZ = 0.0;

			//***** Call C# method *****
			loopyBPUse(ref belBP, ref belE, ref logZ, GMat, pot, localEvDATA);

			
			// Print output
			Console.WriteLine("The node potential values are:");
			for(int i=0; i<4; i++)
			{
				Console.WriteLine(belBP[0*4+i].ToString("g7") + "\t" + belBP[1*4+i].ToString("g7") + "\t" + belBP[2*4+i].ToString("g7") + "\t" + belBP[3*4+i].ToString("g7"));
			}

			Console.WriteLine("\nedge potentials: ");
			for(int i=0; i<4; i++)
			{
				Console.WriteLine(belE[0*4+i].ToString("g7") + "\t" + belE[1*4+i].ToString("g7") + "\t" + belE[2*4+i].ToString("g7") + "\t" + belE[3*4+i].ToString("g7"));
			}

			Console.WriteLine("\nlogZ: " + logZ.ToString("g7"));
		}
		*/

		
		/// <summary>
		/// loopyBPUse uses the Matlab .dll's to perform loopy belief propagation on the specified graph.
		/// </summary>
		/// <param name="bel">nodewise beliefs are placed here</param>
		/// <param name="belE">edgewise beliefs are placed here</param>
		/// <param name="logZ">the (positive) log of Z is placed here</param>
		/// <param name="G">input adjacency matrix</param>
		/// <param name="pot">input edge potentials</param>
		/// <param name="localEv">input site potentials</param>
		/// <param name="nlabels">Keeps track of the number of labels without needing to do calls that could break on empty datasets</param>
		/// <param name="loopyBPInitialized">Keeps track of initalization so mclInitalizedApplication is not called more than once</param>
		/// <returns>Returns 0 is successful, and less than 0 if otherwise.</returns>
		public static int loopyBPUse(ref double[] bel, ref double[] belE, ref double logZ, 
			[In]int[][] G, [In]double[][][] pot, [In]double[][] localEv, 
			int nlabels, bool loopyBPInitialized)
		{
			// Initialize libraries
			bool ret;
			// This needs to be conditional on the first use of this function
			if(!loopyBPInitialized)
			{
				ret= mclInitializeApplication("NULL",0);
				libbploopyUseInitialize();
			}

			// **** create GMat ****
			if (G.Length != G[0].Length) //this could be a jagged array, in which case this test is not sufficient
			{
				//ERROR: The adjacency matrix 'G' is not square
				Console.WriteLine("ERROR - loopyBPUse: The adjacency matrix \'G\' is not square.\n\tIts actual dimensions are "+ 
					G.Length + " x "+ G[1].Length + ".\n" +
					"Aborting.");
				return -1;
			}

			int nnodes = G.Length; //store the number of nodes, we will need it a lot

			//find the number of edges
			int nedges = 0;
			for (int i=0; i<nnodes; i++) 
			{
				for ( int j=i+1; j<nnodes; j++) 
				{
					if (G[i][j] != 0) nedges++; //if there is an edge increment
				}
			}

			IntPtr GMat = mxCreateDoubleMatrix(nnodes, nnodes, 0); // the 0 means real valued
			unsafe //livin' on the edge
			{
				double* GPr = (double*)mxGetPr(GMat);
				for(int i=0; i<nnodes; i++)
				{
					for(int j=0; j<nnodes; j++)
					{
						*(GPr + i*nnodes + j) = G[i][j]; //this might be transposed, but for the adjacency matrix it should not matter
					}
				}
			}

			// **** create potMat ****
			if (pot.Length != nedges)
			{
				//ERROR: pot does not match the number of edges
				Console.WriteLine("ERROR - loopyBPUse: The size of the edge potential matrix \'pot\' does not match the number of edges.\n" +
					"\tpot specifies {0} nodes, it should be {1}.\nAborting.", pot.Length, nedges);
				return -1;
			}
			
			/*if (pot[0].Length!=pot[0][0].Length)  // FIXME: is there a way of doing this that doesn't assume that there is an edge?
			{
				//ERROR: pot nlabels dimensions do not match
				Console.WriteLine("ERROR - loopyBPUse: The dimensions for nlabels in the potential matrix \'pot\' are inconsistent.\n" +
					"\tThey should be the same. They are {0} and {1}.\nAborting.", pot[1].Length, pot[1][1].Length);
				return -1;
			}

			//store the number of possible labels, we will need it a lot
			int nlabels = pot[0].Length;  // FIXME: is there a way of doing this that doesn't assume that there is an edge?*/
			
		
			// Pass in nlabels as an argument
			IntPtr potMat = mxCreateCellMatrix(1, nedges);
			for(int i=0; i<nedges; i++)
				//create a matrix of potentials for each edge
				mxSetCell(potMat, i, mxCreateDoubleMatrix(nlabels, nlabels, 0)); // the 0 means real valued
			unsafe
			{
				for(int k=0; k<nedges; k++)  // CHANGED HERE from nnodes -> nedges
				{
					double* potPr = (double*)mxGetPr(mxGetCell(potMat, k));
					//the following loop is transposing a matrix, 
					//  so if this matrix is large hardware optimization may be significant
					for(int i=0; i<nlabels; i++)
					{
						for(int j=0; j<nlabels; j++)
						{
							*(potPr + j*nlabels + i) = pot[k][i][j]; //Matlab matricies are columnwise
						}
					}
				}
			}

			// **** create localEvMat ****
			if (localEv[0].Length != nnodes)
			{
				//ERROR: localEv does not match the number of nodes
				Console.WriteLine("ERROR - loopyBPUse: The dimension of the site potential matrix \'localEv\' does not match the number of nodes.\n" +
					"\tlocalEv dimension is {0}, and should be {1}.\nAborting.", localEv.Length, nnodes);
				return -1;
			}
			if (localEv.Length != nlabels)
			{
				//ERROR: localEv does not match the number of labels
				Console.WriteLine("ERROR - loopyBPUse: The dimension of the site potential matrix \'localEv\' does not match the number of labels.\n" +
					"\tlocalEv dimension is {0}, and should be {1}.\nAborting.", localEv[1].Length,nlabels);
				return -1;
			}

			IntPtr localEvMat = mxCreateCellMatrix(1, nnodes);
			for(int i=0; i<nnodes; i++)
				// create a column vector of potentials for each node
				mxSetCell(localEvMat, i, mxCreateDoubleMatrix(nlabels, 1, 0)); //the 0 means real valued
			unsafe
			{
				for(int k=0; k<nnodes; k++)
				{
					double* localEvPr = (double*)mxGetPr(mxGetCell(localEvMat, k));
					for(int i=0; i<nlabels; i++)
					{
						*(localEvPr+i) = localEv[i][k]; //remember that localEvDATA is in columns associated to nodes
					}
				}
			}
			
			//create a place for the output
			IntPtr belBPMat = IntPtr.Zero;
			IntPtr belEMat = IntPtr.Zero;
			IntPtr logZMat = IntPtr.Zero;


			// ***** Call library function *****
			mlfBploopyUse(3, ref belBPMat, ref belEMat, ref logZMat, GMat, potMat, localEvMat);

			// Get return values in a sensible format
			//bel = new double[nnodes*nlabels];
			/*for(int i=0; i < nnodes*nlabels; i++)
			{
				Marshal.Copy(mxGetPr(belBPMat), bel[i], i, 1);
			}*/
			
			Marshal.Copy(mxGetPr(belBPMat), bel, 0, nnodes*nlabels);

			//belE = new double[nedges*(nlabels * nlabels)];
			/*for(int i=0; i < nedges*(nlabels*nlabels); i++)
			{
				Marshal.Copy(mxGetPr(belEMat), belE[i], i, 1);
			}*/
		
			Marshal.Copy(mxGetPr(belEMat), belE, 0, nedges*(nlabels*nlabels));

			double[] logZPtr = new double[1];
			Marshal.Copy(mxGetPr(logZMat), logZPtr, 0, 1);
			logZ = logZPtr[0];

			// Terminate ze libraries <-- Imagine the previous in a Schwarzenegger voice
			//libbploopyUseTerminate(); //I probably should not comment this line out, but if I don't, Console.WriteLine throws an exception
			//mclTerminateApplication();
			
			
			/* File output for debugging */
			string FILE_NAME = "EdgeBels-Matlab.txt";
			StreamWriter Tex = new StreamWriter(FILE_NAME, true);
			
			Tex.Write( "*********************************\n" );
			for (int i = 0; i < nnodes; i++) 
			{
				Tex.Write( "Node {0}: [", i );
				for (int j = 0; j < nlabels; j++ ) 
				{
					Tex.Write(bel[i*nlabels+j] + " ");
				}
				Tex.Write( "]" + Tex.NewLine );
			}

			/*for (int e = 0; e < nedges; e++)
			{
				if (e == 0)
					Tex.Write(e + ": [");
				else
					Tex.Write("]" + Tex.NewLine + e + ": [");
				
				for (int a = 0; a < nlabels; a++)
				{
					for (int b = 0; b < nlabels; b++)
					{
						Tex.Write(belE[(nlabels * (e + (b * nedges))) + a]);

						if (b+1 < nlabels)
							Tex.Write(" ");
					}

					if (a+1 < nlabels)
						Tex.Write("]" + Tex.NewLine + "   [");
					else 
						Tex.Write("]" + Tex.NewLine);
				}
			}
			*/
			Tex.Close();

			return 0;
		}


        private static MarkovRandomField makeMRF(double[][] sitePot, double[][][] interPot, int[][] adjMat, int numEdges, int numLabels, bool[] isEvidenceNode)
        {
            // Initialize the nodes for the MRF
            MRFNode[] initNodes = new MRFNode[adjMat[0].Length];
            mit.ai.mrf.Labels[] initScenes;

            for (int i = 0; i < initNodes.Length; i++)
            {
                initScenes = new mit.ai.mrf.Labels[numLabels]; // This initializes the array...

                for (int sc = 0; sc < numLabels; sc++)   // ...but we need to make sure each scene in the array is initialized.
                    initScenes[sc] = new mit.ai.mrf.Labels();

                initNodes[i] = new MRFNode(initScenes, sitePot[i]);
                if (isEvidenceNode[i])
                    initNodes[i].EvidenceNode = true;
            }

            // Add all the neighbors in the nodes, based off the Adjacency Matrix
            for (int r = 0; r < initNodes.Length; r++)
            {
                for (int c = 0; c < initNodes.Length; c++)
                {
                    if (adjMat[c][r] > 0 && c != r)
                    {
                        initNodes[r].addNeighbor(initNodes[c], interPot[adjMat[c][r] - 1]);
                    }
                }
            }

            // Create the MRF
            MarkovRandomField mrf = new MarkovRandomField(initNodes);
            return mrf;
        }

		/// <summary>
		/// MIT LoopyBP call
		/// </summary>
		/// <returns>Returns 0 always. I have no check for whether or not it was successful.</returns>
		public static int loopyBPUse_MIT([In]double[][] sitePot, [In]double[][][] interPot,
			[In]int[][] adjMat, int numEdges, int numLabels, int maxIterations,
			ref double[][] siteBels, ref double[][][] edgeBels, bool[] isEvidenceNode)
		{
            // Initialize the nodes for the MRF
            MRFNode[] initNodes = new MRFNode[adjMat[0].Length];
            mit.ai.mrf.Labels[] initScenes;

            for (int i = 0; i < initNodes.Length; i++)
            {
                initScenes = new mit.ai.mrf.Labels[numLabels]; // This initializes the array...

                for (int sc = 0; sc < numLabels; sc++)   // ...but we need to make sure each scene in the array is initialized.
                    initScenes[sc] = new mit.ai.mrf.Labels();

                initNodes[i] = new MRFNode(initScenes, sitePot[i]);
                if (isEvidenceNode[i])
                    initNodes[i].EvidenceNode = true;
            }

            // Add all the neighbors in the nodes, based off the Adjacency Matrix
            for (int r = 0; r < initNodes.Length; r++)
            {
                for (int c = 0; c < initNodes.Length; c++)
                {
                    if (adjMat[c][r] > 0 && c != r)
                    {
                        initNodes[r].addNeighbor(initNodes[c], interPot[adjMat[c][r] - 1]);
                    }
                }
            }

            // Create the MRF
            MarkovRandomField mrf = new MarkovRandomField(initNodes);
			mrf.resetMessages();

           
			// Pass messages (perform LoopyBP)
			// What's a good number of iterations?
			//bool converged = mrf.passSPMessages(maxIterations);
			
			// I think Sum Product was having precision problems.  Try max product?
			bool converged = mrf.passMPMessages(maxIterations);
            //Console.WriteLine("Loopy BP converged = " + converged);

			// Get the sitePotential beliefs
			siteBels = mrf.beliefs();

            // DEBUGGING: Check to see if the site beliefs match the exact marginals
            // This process will be SLOW
            /*for (int i = 0; i < siteBels.Length; i++)
            {
                for (int j = 0; j < siteBels[i].Length; j++)
                {
                    double exactMarg = mrf.exactMarginal(i, j);
                    Console.WriteLine("Node {0}, Val {1}:\tLBP: {2}\tExact: {3}", i, j, siteBels[i][j], exactMarg);
                }
            }*/
			
			// Get the edgePotential beliefs
			int e = 0;

			
			/*string FILE_NAME = "EdgeBels-MIT.txt";
			
			StreamWriter Tex = new StreamWriter(FILE_NAME, true);
			Tex.Write( "*********************************\n" );

			for (int i = 0; i < siteBels.Length; i++) 
			{
				Tex.Write( "Node {0}: [", i );
				for (int j = 0; j < siteBels[i].Length; j++ ) 
				{
					Tex.Write(siteBels[i][j] + " ");
				}
				Tex.Write( Tex.NewLine );
			}
			*/
			// This goes through each edge only once in the adjacency matrix by looping through
			// the top triangle of the matrix. If there's an edge between nodes, we find the belief.
			for (int i = 0; i < mrf.Nodes.Length; i++)
			{

				for (int j = i + 1; j < mrf.Nodes.Length; j++)
				{
					if (adjMat[i][j] > 0)
					{
						// Set the edge belief, and increase the number of edges
						edgeBels[e++] = mrf.Nodes[i].beliefs(mrf.Nodes[j]);
					
						/* File output for debugging */
						/*Tex.Write((e-1) + ": ");
						
						for (int k = 0; k < edgeBels[e-1].Length; k++)
						{
							if (k == 0)
								Tex.Write("[");
							else
								Tex.Write("   [");

							for (int l = 0; l < edgeBels[e-1][k].Length; l++)
							{
								Tex.Write(edgeBels[e-1][k][l]);

								if (l < edgeBels[e-1][k].Length - 1)
									Tex.Write(", ");
							}

							Tex.Write("]" + Tex.NewLine);
						}*/
					}
				}
			}

			//Tex.Close();

			return 0;
		}


        /// <summary>
        /// MIT Exact Inference call
        /// </summary>
        /// <returns>Returns the partition value</returns>
        public static double useMITExact([In]double[][] sitePot, [In]double[][][] interPot,
            [In]int[][] adjMat, int numEdges, int numLabels, ref double[][] siteBels,
            ref double[][][] edgeBels, ref Hashtable configTable, bool[] isEvidenceNode)
        {
            // Initialize the nodes for the MRF
            MRFNode[] initNodes = new MRFNode[adjMat[0].Length];
            mit.ai.mrf.Labels[] initScenes;

            for (int i = 0; i < initNodes.Length; i++)
            {
                initScenes = new mit.ai.mrf.Labels[numLabels]; // This initializes the array...

                for (int sc = 0; sc < numLabels; sc++)   // ...but we need to make sure each scene in the array is initialized.
                    initScenes[sc] = new mit.ai.mrf.Labels();

                initNodes[i] = new MRFNode(initScenes, sitePot[i]);
                if (isEvidenceNode[i])
                    initNodes[i].EvidenceNode = true;
            }

            // Add all the neighbors in the nodes, based off the Adjacency Matrix
            for (int r = 0; r < initNodes.Length; r++)
            {
                for (int c = 0; c < initNodes.Length; c++)
                {
                    if (adjMat[c][r] > 0 && c != r)
                    {
                        initNodes[r].addNeighbor(initNodes[c], interPot[adjMat[c][r] - 1]);
                    }
                }
            }

            // Create the MRF
            MarkovRandomField mrf = new MarkovRandomField(initNodes);

            double[][] outBeliefs = new double[mrf.Nodes.Length][];
            double[][][] outEdgeBeliefs = new double[numEdges][][];

            // calculate site beliefs
            for (int i = 0; i < mrf.Nodes.Length; ++i)
            {
                outBeliefs[i] = new double[numLabels];
                for (int a = 0; a < numLabels; ++a)
                {
                    outBeliefs[i][a] = mrf.exactMarginal(i, a);
                }
            }

            // calculate edge beliefs
            int e = 0; // edge counter
            for (int i = 0; i < mrf.Nodes.Length; ++i)
            {
                // "int j = i + 1" because we don't want to check symmetric combinations
                for (int j = i + 1; j < mrf.Nodes.Length; ++j)
                {
                    if (adjMat[i][j] > 0)
                    {
                        double[][] bvals = new double[numLabels][];

                        for (int a = 0; a < numLabels; ++a)
                        {
                            bvals[a] = new double[numLabels];
                            for (int b = 0; b < numLabels; ++b)
                            {
                                bvals[a][b] = mrf.exactMarginalEdge(i, a, j, b);
                            }
                        }
                        
                        outEdgeBeliefs[e++] = bvals;
                    }
                }
            }

            siteBels = outBeliefs;
            edgeBels = outEdgeBeliefs;
            configTable = mrf.ConfigTable;

            return mrf.partitionValueFromTable();
        }
	}
}