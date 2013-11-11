/*
* MarkovRandomField.java
*
* Michael Ross
*
* 3/18/02
*
*/
namespace mit.ai.mrf
{
	using System;
	using System.Collections;
    using System.Collections.Generic;
	using System.Runtime.Serialization;

	/// <summary>A class for specifying Markov random fields and for finding their
	/// MAP estimates by belief propagation. 
	/// </summary>
	[Serializable]
	public class MarkovRandomField //: System.Runtime.Serialization.ISerializable
	{
		// Replacement for the mit.debug file
		public const bool DEBUG = false;

		//UPGRADE_NOTE: Field 'EnclosingInstance' was added to class 'BeliefPropagationThread' to access its enclosing instance. 'ms-help://MS.VSCC.2003/commoner/redir/redirect.htm?keyword="jlca1019"'
		//UPGRADE_NOTE: Local class 'BeliefPropagationThread' in method 'passMessages' was converted to  a nested class. 'ms-help://MS.VSCC.2003/commoner/redir/redirect.htm?keyword="jlca1022"'
		public class BeliefPropagationThread:SupportClass.ThreadClass
		{
			private void InitBlock(int iterations, bool[] converged, int bpType, bool[] finished, double stepSize, 
				bool testConvergence, double[] delta, double convergenceLimit, MarkovRandomField enclosingInstance)
			{
				this.iterations = iterations;
				this.converged = converged;
				this.bpType = bpType;
				this.finished = finished;
				this.stepSize = stepSize;
				this.testConvergence = testConvergence;
				this.delta = delta;
				this.convergenceLimit = convergenceLimit;
				this.enclosingInstance = enclosingInstance;
			}
			
			//UPGRADE_NOTE: Final variable iterations was copied into class BeliefPropagationThread. 'ms-help://MS.VSCC.2003/commoner/redir/redirect.htm?keyword="jlca1023"'
			private int iterations;
			//UPGRADE_NOTE: Final variable converged was copied into class BeliefPropagationThread. 'ms-help://MS.VSCC.2003/commoner/redir/redirect.htm?keyword="jlca1023"'
			private bool[] converged;
			//UPGRADE_NOTE: Final variable bpType was copied into class BeliefPropagationThread. 'ms-help://MS.VSCC.2003/commoner/redir/redirect.htm?keyword="jlca1023"'
			private int bpType;
			//UPGRADE_NOTE: Final variable finished was copied into class BeliefPropagationThread. 'ms-help://MS.VSCC.2003/commoner/redir/redirect.htm?keyword="jlca1023"'
			private bool[] finished;
			//UPGRADE_NOTE: Final variable stepSize was copied into class BeliefPropagationThread. 'ms-help://MS.VSCC.2003/commoner/redir/redirect.htm?keyword="jlca1023"'
			private double stepSize;
			//UPGRADE_NOTE: Final variable testConvergence was copied into class BeliefPropagationThread. 'ms-help://MS.VSCC.2003/commoner/redir/redirect.htm?keyword="jlca1023"'
			private bool testConvergence;
			//UPGRADE_NOTE: Final variable delta was copied into class BeliefPropagationThread. 'ms-help://MS.VSCC.2003/commoner/redir/redirect.htm?keyword="jlca1023"'
			private double[] delta;
			//UPGRADE_NOTE: Final variable convergenceLimit was copied into class BeliefPropagationThread. 'ms-help://MS.VSCC.2003/commoner/redir/redirect.htm?keyword="jlca1023"'
			private double convergenceLimit;

            
			private MarkovRandomField enclosingInstance;
			public MarkovRandomField Enclosing_Instance
			{
				get
				{
					return enclosingInstance;
				}
				
			}
			private int which;
			private int mod;

			public BeliefPropagationThread(int iterations, bool[] converged, int bpType, bool[] finished, 
				double stepSize, bool testConvergence, double[] delta, double convergenceLimit, 
				MarkovRandomField enclosingInstance, int which, int mod):base()
			{
				InitBlock(iterations, converged, bpType, finished, stepSize, testConvergence, delta, convergenceLimit, enclosingInstance);
				Name = "BeliefPropagationThread-" + which + " of " + mod;
				this.which = which;
				this.mod = mod;
				return ;
			}
			
			//UPGRADE_TODO: The equivalent of method 'java.lang.Thread.run' is not an override method. 'ms-help://MS.VSCC.2003/commoner/redir/redirect.htm?keyword="jlca1143"'
			override public void  Run()
			{
				bool DEBUG = false;

				for (int iter = 0; iter < iterations && !converged[which]; iter++)
				{
					if (which == 0 && DEBUG)
					{
						System.Console.Out.WriteLine("BP iteration " + iter);
					}
					
					for (int i = which; i < Enclosing_Instance.nodes.Length; i += mod)
					{
						if (bpType == mit.ai.mrf.MarkovRandomField.MAX_PRODUCT)
						{
							Enclosing_Instance.nodes[i].passMPMessages();
						}
						else if (bpType == mit.ai.mrf.MarkovRandomField.SUM_PRODUCT)
						{
							Enclosing_Instance.nodes[i].passSPMessages();
						}
						else
						{
							//throw new UnsupportedOperationException();
						}
					}
					
					lock (finished)
					{
						finished[which] = true;
						bool allFinished = true;
						for (int t = 0; t < finished.Length; t++)
						{
							if (!finished[t])
							{
								allFinished = false;
								break;
							}
						}
						
						if (allFinished)
						{
							System.Threading.Monitor.PulseAll(finished);
							for (int i = 0; i < finished.Length; i++)
								finished[i] = false;
						}
						else
						{
							try
							{
								System.Threading.Monitor.Wait(finished);
							}
							catch (System.Threading.ThreadInterruptedException x)
							{
								throw new System.SystemException("Error in BP thread.");
							}
						}
					}
					
					for (int i = which; i < Enclosing_Instance.nodes.Length; i += mod)
					{
						if (stepSize == 1)
						{
							Enclosing_Instance.nodes[i].update();
						}
						else
						{
							Enclosing_Instance.nodes[i].update(stepSize);
						}
					}
					
					for (int i = which; i < Enclosing_Instance.nodes.Length; i += mod)
					{
						Enclosing_Instance.nodes[i].rescale();
					}
					
					if (testConvergence)
					{
						delta[which] = 0;
						converged[which] = true;
						for (int i = which; i < Enclosing_Instance.nodes.Length; i += mod)
						{
							delta[which] += Enclosing_Instance.nodes[i].delta();
							converged[which] = converged[which] && (delta[which] <= convergenceLimit);
						}
					}
					
					lock (finished)
					{
						finished[which] = true;
						bool allFinished = true;
						
						for (int t = 0; t < finished.Length; t++)
						{
							if (!finished[t])
							{
								allFinished = false;
								break;
							}
						}
						
						if (allFinished)
						{
							if (testConvergence && DEBUG)
							{
								double deltaSum = 0;
								for (int t = 0; t < delta.Length; t++)
								{
									deltaSum += delta[t];
								}
								
								System.Console.Out.WriteLine("Delta: " + deltaSum);
							}
							
							for (int t = 0; t < converged.Length; t++)
							{
								if (!converged[t])
								{
									for (int i = 0; i < converged.Length; i++)
										converged[i] = false;

									break;
								}
							}

							System.Threading.Monitor.PulseAll(finished);
							
							for (int i = 0; i < finished.Length; i++)
								finished[i] = false;
						}
						else
						{
							try
							{
								System.Threading.Monitor.Wait(finished);
							}
							catch (System.Threading.ThreadInterruptedException x)
							{
								throw new System.SystemException("Error in BP thread.");
							}
						}
					}
				}
			}
		}
		/// <summary>Access MRFNodes in the MarkovRandomField. 
		/// </summary>
		virtual public MRFNode[] Nodes
		{
			get
			{
				return nodes;
			}
			
		}
		internal const long serialVersionUID = 1L;
		private static int MAX_PRODUCT = 0;
		private static int SUM_PRODUCT = 1;
		private MRFNode[] nodes;
        private Hashtable configurationTable;

		
		
		/// <summary>
		/// Create an MRF with initial nodes
		/// </summary>
		/// <param name="initNodes">MRF's nodes</param>
		public MarkovRandomField(MRFNode[] initNodes)
		{
			nodes = initNodes;
			return;
		}

        public Hashtable ConfigTable
        {
            get
            {
                if (configurationTable == null)
                    configurationTable = makeConfigurationTable();
                return configurationTable;
            }
        }
		
		/// <summary>Calculate exact beliefs for a state. 
		/// </summary>
		public virtual double probability(HashIndex[] vals)
		{
			return configurationValue(vals) / partitionValue();
		}

        /// <summary>
        /// Calculate the marginal probability for the given node and the specified assignment to the node using exact inference by summing out 
        /// </summary>
        /// <param name="node"> The node in question</param>
        /// <param name="val">The value for the node</param>
        /// <returns></returns>
        public virtual double exactMarginal(int node, int valIndex)
        {
            if ( configurationTable == null)
            {
                configurationTable = makeConfigurationTable();
            }

            // Sum out the probabilities for all of the values of all of the nodes except for the one in question. Hold that fixed.
            double total = 0;
            
            int[] assignIndex = new int[nodes.Length];
            for (int i = 0; i < assignIndex.Length; i++)
                assignIndex[i] = 0;

            assignIndex[node] = valIndex;

            HashIndex[] assignment = new HashIndex[nodes.Length];

            bool done = false;
            while (!done)
            {
                for (int n = 0; n < nodes.Length; n++)
                {
                        assignment[n] = nodes[n].PossibleScenes[assignIndex[n]];
                }

                String index = arrayToString(assignIndex);
                total += (double)configurationTable[index];

                bool flipped = false;
                for (int n = 0; n < assignIndex.Length && !flipped; n++)
                {
                    if (n == node)  // Never change the node in question
                    {
                        continue;
                    }
                    int maxLength = nodes[n].PossibleScenes.Length;
                    if (assignIndex[n] == maxLength - 1)
                    {
                        assignIndex[n] = 0;
                    }
                    else
                    {
                        assignIndex[n]++;
                        flipped = true;
                    }
                }

                if (!flipped)
                {
                    done = true;
                }
            }

            return total / partitionValueFromTable(configurationTable);


        }

        /// <summary>
        /// Same as exactMarginal above, except we take two nodes and two assignments.
        /// Assumes that the given nodes are adjacent
        /// </summary>
        /// <param name="node"> The node in question</param>
        /// <param name="val">The value for the node</param>
        /// <returns></returns>
        public virtual double exactMarginalEdge(int n1, int v1, int n2, int v2)
        {
            if (configurationTable == null)
            {
                configurationTable = makeConfigurationTable();
            }

            // Sum out the probabilities for all of the values of all of the nodes except for the two in question. Hold those fixed.
            double total = 0;

            int[] assignIndex = new int[nodes.Length];
            for (int i = 0; i < assignIndex.Length; i++)
                assignIndex[i] = 0;

            assignIndex[n1] = v1;
            assignIndex[n2] = v2;

            HashIndex[] assignment = new HashIndex[nodes.Length];

            bool done = false;
            while (!done)
            {
                for (int n = 0; n < nodes.Length; n++)
                {
                    assignment[n] = nodes[n].PossibleScenes[assignIndex[n]];
                }

                String index = arrayToString(assignIndex);
                total += (double)configurationTable[index];

                bool flipped = false;
                for (int n = 0; n < assignIndex.Length && !flipped; n++)
                {
                    if (n == n1 || n == n2)  // Never change the nodes in question
                    {
                        continue;
                    }
                    int maxLength = nodes[n].PossibleScenes.Length;
                    if (assignIndex[n] == maxLength - 1)
                    {
                        assignIndex[n] = 0;
                    }
                    else
                    {
                        assignIndex[n]++;
                        flipped = true;
                    }
                }

                if (!flipped)
                {
                    done = true;
                }
            }

            return total / partitionValueFromTable(configurationTable);


        }

        private String arrayToString(int[] iArray)
        {
            String ret = "";
            foreach (int x in iArray)
            {
                ret += x.ToString();
            }
            return ret;
        }

        /// <summary>
        /// Compute all of the configuration values for all possible variable assignments and store them in a big table
        /// </summary>
        /// <returns></returns>
        public virtual Hashtable makeConfigurationTable()
        {
            Hashtable configTable = new Hashtable();
            int[] assignIndex = new int[nodes.Length];
            for (int i = 0; i < assignIndex.Length; i++)
                assignIndex[i] = 0;

            HashIndex[] assignment = new HashIndex[nodes.Length];

            bool done = false;
            while (!done)
            {
                for (int n = 0; n < nodes.Length; n++)
                {
                    assignment[n] = nodes[n].PossibleScenes[assignIndex[n]];
                }

                double cval = configurationValue(assignment);
                String index = arrayToString(assignIndex);
                configTable.Add( index, configurationValue(assignment));

                bool flipped = false;
                for (int n = 0; n < assignIndex.Length && !flipped; n++)
                {
                    int maxLength = nodes[n].PossibleScenes.Length;
                    if (assignIndex[n] == maxLength - 1)
                    {
                        assignIndex[n] = 0;
                    }
                    else
                    {
                        assignIndex[n]++;
                        flipped = true;
                    }
                }

                if (!flipped)
                {
                    done = true;
                }
            }

            return configTable;
        }

        public virtual double partitionValueFromTable(Hashtable configTable)
        {
            double partition = 0;
            foreach (String assignment in configTable.Keys)
                partition += (double)configTable[assignment];

            return partition;
        }

        public double partitionValueFromTable()
        {
            return partitionValueFromTable(configurationTable);
        }

		/// <summary>
		/// Calculate the partition function's value for the MRF.
		/// I believe this is the Z value
		/// </summary>
		public virtual double partitionValue()
		{
			double partition = 0;
			
			int[] assignIndex = new int[nodes.Length];
			for (int i = 0; i < assignIndex.Length; i++)
				assignIndex[i] = 0;
			
			HashIndex[] assignment = new HashIndex[nodes.Length];
			
			bool done = false;
			while (!done)
			{
				for (int n = 0; n < nodes.Length; n++)
				{
					assignment[n] = nodes[n].PossibleScenes[assignIndex[n]];
				}
				
				partition += configurationValue(assignment);
				
				bool flipped = false;
				for (int n = 0; n < assignIndex.Length && !flipped; n++)
				{
					int maxLength = nodes[n].PossibleScenes.Length;
					if (assignIndex[n] == maxLength - 1)
					{
						assignIndex[n] = 0;
					}
					else
					{
						assignIndex[n]++;
						flipped = true;
					}
				}
				
				if (!flipped)
				{
					done = true;
				}
			}
			
			return partition;
		}
		
		/// <summary>Create an array of all the indexes of the neighbors of each nodes. 
		/// </summary>
		private int[][] findNeighborIndices()
		{
			int[][] neighInd = new int[nodes.Length][];
			for (int n = 0; n < nodes.Length; n++)
			{
				ArrayList neighbors = nodes[n].Neighbors;
				neighInd[n] = new int[neighbors.Count];
				for (int ni = 0; ni < neighbors.Count; ni++)
				{
					int location = 0;
					while (nodes[location] != neighbors[ni])
					{
						location++;
					}
					neighInd[n][ni] = location;
				}
			}
			return neighInd;
		}
		
		/// <summary>Use Gibbs sampling to compute a sample marginal distribution for a
		/// particular node. 
		/// </summary>
		public virtual double[] computeSampleDistribution(int node, int numSamples)
		{
			if (numSamples > System.Int32.MaxValue)
			{
				return null;
			}
			
			int[] counts = new int[nodes[node].PossibleScenes.Length];
			int[] initAssignment = randomAssignment();
			int remainingSamples = numSamples;
			while (remainingSamples > 0)
			{
				int takenSamples = System.Math.Min(remainingSamples, 10000);
				int[][] samples = generateGibbsSampleIndices(initAssignment, takenSamples);
				for (int s = 0; s < samples.Length; s++)
				{
					counts[samples[s][node]]++;
				}
				Array.Copy(samples[samples.Length - 1], 0, initAssignment, 0, nodes.Length);
				remainingSamples -= takenSamples;
			}
			
			double[] distribution = new double[counts.Length];
			for (int s = 0; s < counts.Length; s++)
			{
				distribution[s] = counts[s] / ((double) numSamples);
			}
			
			return distribution;
		}
		
		/// <summary>Use Gibbs sampling to compute the sample mode for a particular
		/// node. 
		/// </summary>
		public virtual HashIndex computeSampleMode(int node, int numSamples)
		{
			double[] distribution = computeSampleDistribution(node, numSamples);
			HashIndex[] possibleScenes = nodes[node].PossibleScenes;
			HashIndex maxScene = null;
			double maxValue = System.Double.NegativeInfinity;
			for (int s = 0; s < distribution.Length; s++)
			{
				if (distribution[s] > maxValue)
				{
					maxScene = possibleScenes[s];
					maxValue = distribution[s];
				}
			}
			return maxScene;
		}
		
		/// <summary>Convert samples into HashIndex representation. 
		/// </summary>
		private HashIndex[][] translateSamples(int[][] samples)
		{
			HashIndex[][] samplesHI = new HashIndex[samples.Length][];
			for (int i = 0; i < samples.Length; i++)
			{
				samplesHI[i] = new HashIndex[samples[0].Length];
			}
			HashIndex[][] possibleScenes = new HashIndex[nodes.Length][];
			for (int n = 0; n < nodes.Length; n++)
			{
				possibleScenes[n] = nodes[n].PossibleScenes;
			}
			
			for (int i = 0; i < samplesHI.Length; i++)
			{
				for (int j = 0; j < samplesHI[0].Length; j++)
				{
					samplesHI[i][j] = possibleScenes[j][samples[i][j]];
				}
			}
			
			return samplesHI;
		}
		
		/// <summary>Generate a series of Gibbs samples, starting from a random initial
		/// assignment. 
		/// </summary>
		public virtual HashIndex[][] generateGibbsSamples(int numSamples)
		{
			return translateSamples(generateGibbsSampleIndices(numSamples));
		}
		
		/// <summary>Generate a series of Gibbs samples, starting from a random initial
		/// assignment, in index format. 
		/// </summary>
		public virtual int[][] generateGibbsSampleIndices(int numSamples)
		{
			return generateGibbsSampleIndices(randomAssignment(), numSamples);
		}
		
		/// <summary>Generate a uniformly random assignment to the MRF. 
		/// </summary>
		public virtual int[] randomAssignment()
		{
			int[] assignment = new int[nodes.Length];
			for (int n = 0; n < assignment.Length; n++)
			{
				//UPGRADE_WARNING: Narrowing conversions may produce unexpected results in C#. 'ms-help://MS.VSCC.2003/commoner/redir/redirect.htm?keyword="jlca1042"'
				assignment[n] = (int) (SupportClass.Random.NextDouble() * nodes[n].PossibleScenes.Length);
			}
			return assignment;
		}
		
		/// <summary>Generate a series of Gibbs samples, given an initial
		/// assignment. 
		/// </summary>
		private int[][] generateGibbsSampleIndices(int[] initAssignment, int numSamples)
		{
			int[][] neighInd = findNeighborIndices();
			int[][] neighValues = new int[nodes.Length][];
			for (int n = 0; n < neighValues.Length; n++)
			{
				neighValues[n] = new int[neighInd.Length];
			}
			
			int[] assignment = new int[nodes.Length];
			Array.Copy(initAssignment, 0, assignment, 0, initAssignment.Length);
			int[][] samples = new int[numSamples][];
			for (int i = 0; i < numSamples; i++)
			{
				samples[i] = new int[assignment.Length];
			}
			
			for (int s = 0; s < numSamples; s++)
			{
				//UPGRADE_WARNING: Narrowing conversions may produce unexpected results in C#. 'ms-help://MS.VSCC.2003/commoner/redir/redirect.htm?keyword="jlca1042"'
				int n = (int) (SupportClass.Random.NextDouble() * nodes.Length);
				for (int nn = 0; nn < neighInd[n].Length; nn++)
				{
					neighValues[n][nn] = assignment[neighInd[n][nn]];
				}
				int sampleIndex = drawNextGibbsSample(n, neighInd[n], neighValues[n]);
				assignment[n] = sampleIndex;
				Array.Copy(assignment, 0, samples[s], 0, assignment.Length);
			}
			
			return samples;
		}
		
		/// <summary>Return the index of the next Gibbs sample at a particular node n,
		/// with a given set of neighbor indices and values. 
		/// </summary>
		private int drawNextGibbsSample(int n, int[] neighbors, int[] neighValues)
		{
			int numValues = nodes[n].PossibleScenes.Length;
			double[] distribution = new double[numValues];
			double norm = 0;
			
			for (int v = 0; v < numValues; v++)
			{
				distribution[v] = nodes[n].localMatch(v);
				for (int nn = 0; nn < neighbors.Length; nn++)
				{
					distribution[v] *= nodes[n].neighborMatch(nodes[neighbors[nn]], v, neighValues[nn]);
				}
				norm += distribution[v];
			}
			
			for (int v = 0; v < numValues; v++)
			{
				distribution[v] /= norm;
			}
			
			double uniform = SupportClass.Random.NextDouble();
			double sum = 0;
			for (int v = 0; v < numValues; v++)
			{
				sum += distribution[v];
				if (sum > uniform)
				{
					return v;
				}
			}
			
			return numValues - 1;
		}
		
		/// <summary>Log of the configuration value. 
		/// </summary>
		public virtual double logConfigurationValue(HashIndex[] values)
		{
			int[] assignment = new int[nodes.Length];
			for (int i = 0; i < assignment.Length; i++)
				assignment[i] = -1;
						
			for (int n = 0; n < nodes.Length; n++)
			{
				HashIndex[] scenes = nodes[n].PossibleScenes;
				for (int s = 0; s < scenes.Length; s++)
				{
					if (scenes[s] == values[n])
					{
						assignment[n] = s;
					}
				}
			}
			
			int[][] neighInd = findNeighborIndices();
			
			double sum = 0;
			for (int n = 0; n < nodes.Length; n++)
			{
				if (DEBUG)
				{
					System.Console.Out.Write(n + " " + nodes[n].localMatch(assignment[n]));
				}
				sum += System.Math.Log(nodes[n].localMatch(assignment[n]));
				ArrayList neighbors = nodes[n].Neighbors;
				for (int ni = 0; ni < neighbors.Count; ni++)
				{
					MRFNode neigh = (MRFNode) neighbors[ni];
					
					if (n > neighInd[n][ni])
					{
						if (DEBUG)
						{
							System.Console.Out.Write(" " + nodes[n].neighborMatch(neigh, assignment[n], assignment[neighInd[n][ni]]));
						}
						sum += System.Math.Log(nodes[n].neighborMatch(neigh, assignment[n], assignment[neighInd[n][ni]]));
					}
				}
				
				if (DEBUG)
				{
					System.Console.Out.WriteLine();
				}
			}
			
			return sum;
		}
		
		/// <summary>Configuration value should be divided by partition function
		/// to produce probability. 
		/// </summary>
		public virtual double configurationValue(HashIndex[] values)
		{
			return System.Math.Exp(logConfigurationValue(values));
		}
		
		public virtual HashIndex[] icm(HashIndex[] startValues)
		{
			return icm(startValues, - 1);
		}
		
		/// <summary>Improve the configuration through the iterative conditional modes
		/// method. 
		/// </summary>
		public virtual HashIndex[] icm(HashIndex[] startValues, int maxFlips)
		{
			int[][] neighInd = new int[nodes.Length][];
			for (int n = 0; n < nodes.Length; n++)
			{
				ArrayList neighbors = nodes[n].Neighbors;
				neighInd[n] = new int[neighbors.Count];
				for (int ni = 0; ni < neighbors.Count; ni++)
				{
					int location = 0;
					while (nodes[location] != neighbors[ni])
					{
						location++;
					}
					neighInd[n][ni] = location;
				}
			}
			
			HashIndex[] icmValues = new HashIndex[startValues.Length];
			Array.Copy(startValues, 0, icmValues, 0, startValues.Length);
			
			int[] icmIndices = new int[icmValues.Length];
			
			for (int i = 0; i < icmIndices.Length; i++)
			{
				HashIndex[] vals = nodes[i].PossibleScenes;
				
				for (int v = 0; v < vals.Length; v++)
				{
					if (vals[v] == icmValues[i])
					{
						icmIndices[i] = v;
						break;
					}
				}
			}
			
			bool change = true;
			double bestScore = logConfigurationValue(icmValues);
			int flips = 0;
			while (change && (maxFlips == - 1 || flips < maxFlips))
			{
				change = false;
				
				for (int i = 0; i < icmValues.Length; i++)
				{
					HashIndex[] possibleValues = nodes[i].PossibleScenes;
					ArrayList neighbors = nodes[i].Neighbors;
					for (int v = 0; v < possibleValues.Length; v++)
					{
						double changeScore = bestScore - System.Math.Log(nodes[i].localMatch(icmIndices[i])) + System.Math.Log(nodes[i].localMatch(v));
						
						for (int n = 0; n < neighInd[i].Length; n++)
						{
							MRFNode neigh = (MRFNode) neighbors[n];
							changeScore = changeScore - System.Math.Log(nodes[i].neighborMatch(neigh, icmIndices[i], icmIndices[neighInd[i][n]])) + System.Math.Log(nodes[i].neighborMatch(neigh, v, icmIndices[neighInd[i][n]]));
						}
						
						if (changeScore > bestScore && icmIndices[i] != v)
						{
							if (DEBUG)
							{
								System.Console.Out.WriteLine(icmValues[i] + " (" + bestScore + ") -> " + possibleValues[v] + "( " + changeScore + ")");
							}
							
							bestScore = changeScore;
							icmValues[i] = possibleValues[v];
							icmIndices[i] = v;
							change = true;
							flips++;
						}
					}
				}
			}
			
			return icmValues;
		}
		
		/// <summary>Scan the nodes, arbitrarily break the first MAP estimate tie,
		/// fixing the node to the first tied value. Return index if a tie
		/// is found and broken, return -1 if no ties found to be
		/// broken. 
		/// </summary>
		public virtual int breakFirstTie()
		{
			for (int i = 0; i < nodes.Length; i++)
			{
				if (nodes[i].existMAPTie())
				{
					if (DEBUG)
					{
						System.Console.Out.WriteLine("Tie broken at " + i);
						
						System.Console.Out.WriteLine("Possible choices:");
						
						int mapInd = nodes[i].mapEstimateIndex();
						HashIndex[] possibleScenes = nodes[i].PossibleScenes;
						double[] beliefs = nodes[i].beliefs();
						for (int p = 0; p < possibleScenes.Length; p++)
						{
							if (beliefs[p] == beliefs[mapInd])
							{
								System.Console.Out.WriteLine(possibleScenes[p]);
							}
						}
						
						System.Console.Out.WriteLine("Selected:");
					}
					
					nodes[i].setMAPValue();
					
					if (DEBUG)
					{
						System.Console.Out.WriteLine(nodes[i].mapEstimate());
					}
					
					return i;
				}
			}
			
			return - 1;
		}
		
		/// <summary>Unfix any fixed nodes. 
		/// </summary>
		public virtual void  unsetValues()
		{
			for (int i = 0; i < nodes.Length; i++)
			{
				nodes[i].unsetValue();
			}
			
			return ;
		}
		
		/// <summary>Reset belief propagation messages to starting state. 
		/// </summary>
		public virtual void  resetMessages()
		{
			for (int i = 0; i < nodes.Length; i++)
			{
				nodes[i].resetMessages();
			}
			
			return ;
		}
		
		/// <summary>Use belief propagation to infer the beliefs for the given node. 
		/// </summary>
		public virtual double[] bpInferBeliefs(int nodeindex, int iterations)
		{
			resetMessages();
			passSPMessages(iterations);
			return nodes[nodeindex].beliefs();
		}
		
		/// <summary>Use belief propagation to infer the beliefs for the given pair
		/// of neighboring nodes. 
		/// </summary>
		public virtual double[][] bpInferBelifs(int nodeindexA, int nodeindexB, int iterations)
		{
			resetMessages();
			passSPMessages(iterations);
			return MRFNode.beliefs(nodes[nodeindexA], nodes[nodeindexB]);
		}
		
		public virtual double[][] beliefs()
		{
			double[][] out_Renamed = new double[nodes.Length][];
			
			for (int i = 0; i < nodes.Length; i++)
			{
				out_Renamed[i] = nodes[i].beliefs();
			}
			
			return out_Renamed;
		}
		
		public virtual int[] mapEstimateIndices()
		{
			int[] out_Renamed = new int[nodes.Length];
			for (int i = 0; i < nodes.Length; i++)
			{
				out_Renamed[i] = nodes[i].mapEstimateIndex();
			}
			return out_Renamed;
		}
		
		public virtual HashIndex[] mapEstimate()
		{
			HashIndex[] out_Renamed = new HashIndex[nodes.Length];
			for (int i = 0; i < nodes.Length; i++)
			{
				out_Renamed[i] = nodes[i].mapEstimate();
			}
			return out_Renamed;
		}
		
		/// <summary>Sum-product belief propagation until convergence. 
		/// </summary>
		public virtual bool sumProductBeliefPropagate(int maxIter)
		{
			return passMessages(SUM_PRODUCT, maxIter, true, 0);
		}
		
		/// <summary>Max-product belief propagation until convergence. 
		/// </summary>
		public virtual bool maxProductBeliefPropagate(int maxIter)
		{
			return passMessages(MAX_PRODUCT, maxIter, true, 0);
		}
		
		/// <summary>Perform sum-product belief propagation among the nodes. 
		/// </summary>
		public virtual bool passSPMessages(int iterations)
		{
			return passMessages(SUM_PRODUCT, iterations);
		}
		
		/// <summary>Perform belief propagation among the nodes. 
		/// </summary>
		public virtual bool passMPMessages(int iterations)
		{
			return passMessages(MAX_PRODUCT, iterations);
		}
		
		/// <summary>BP until convergence. If there is a tie, break it and
		/// rerun. If convergence fails, fix a node and rerun. 
		/// </summary>
		public virtual void  maxProductBeliefPropagateFix(int iterations)
		{
			bool converged = false;
			int tieIndex = 0;
			//int fixIndex = 0;
			int[] fixIndices = new int[nodes.Length];
			double maxlogval = System.Double.NegativeInfinity;
			int[] maxmapind = null;
			while ((tieIndex != - 1 || !converged))
			{
				resetMessages();
				converged = maxProductBeliefPropagate(iterations);
				
				double logval = logConfigurationValue(mapEstimate());
				
				if (logval < maxlogval)
				{
					/*
					nodes[fixIndex].unsetValue();
					fixIndex++;
					*/
					for (int i = 0; i < fixIndices.Length && fixIndices[i] != - 1; i++)
					{
						nodes[fixIndices[i]].unsetValue();
					}
				}
				else
				{
					maxlogval = logval;
					int[] newindices = mapEstimateIndices();
					
					if (!Array.Equals(maxmapind, newindices))
					{
						maxmapind = newindices;
					}
				}
				
				tieIndex = breakFirstTie();
				System.Console.Out.WriteLine("converged: " + converged + " tie: " + tieIndex + " score: " + logval);
				
				if (tieIndex == - 1 && !converged)
				{
					int loc = 0;
					for (int i = 0; i < nodes.Length; i++)
					{
						if (SupportClass.Random.NextDouble() < 0.10 && !nodes[i].Set)
						{
							fixIndices[loc] = i;
							
							System.Console.Out.WriteLine("setting: " + fixIndices[loc]);
							
							if (fixIndices[loc] == 0)
							{
								nodes[fixIndices[loc]].Value = maxmapind[fixIndices[loc]];
							}
							else
							{
								int[] vals = new int[2];
								vals[0] = (maxmapind[fixIndices[loc]] / 2) * 2;
								vals[1] = (maxmapind[fixIndices[loc]] / 2) * 2 + 1;
								nodes[fixIndices[loc]].Values = vals;
							}
							
							loc++;
						}
					}
					fixIndices[loc] = - 1;
					
					/*
					for ( ; fixIndex < nodes.length; fixIndex++)
					{
					if (!nodes[fixIndex].isSet())
					{
					if (fixIndex == 0)
					{
					nodes[fixIndex].setValue(maxmapind[fixIndex]);
					}
					else
					{
					int[] vals = new int[2];
					vals[0] = (maxmapind[fixIndex] / 2) * 2;
					vals[1] = (maxmapind[fixIndex] / 2) * 2 + 1;
					nodes[fixIndex].setValues(vals);
					}
					
					System.out.println("fixed: " + fixIndex);
					break;
					}
					}
					*/
					
					/*
					double maxval = Double.NEGATIVE_INFINITY;
					int maxind = -1;
					int[] mapind = mapEstimateIndices();
					for (int i = 0; i < nodes.length; i++)
					{
					double val = 0;
					if (!nodes[i].isSet()
					&& (val = nodes[i].beliefs()[mapind[i]]) > maxval)
					{
					maxind = i;
					maxval = val;
					}
					}
					
					nodes[maxind].setMAPValue();
					System.out.println("fixed: " + maxind);
					fixIndex = maxind;
					*/
				}
			}
			return ;
		}
		
		/// <summary>Repeatedly break belief propagation ties and rerun
		/// the inference until no ties remain. 
		/// </summary>
		public virtual void  maxProductBeliefPropagateTieBreak(int iterations)
		{
			int tieIndex = 0;
			while (tieIndex != - 1)
			{
				resetMessages();
				maxProductBeliefPropagate(iterations);
				tieIndex = breakFirstTie();
			}
			return ;
		}
		
		/// <summary>Perform specified iterations of max-product belief
		/// propagation, break ties, and repeat until no ties remain. 
		/// </summary>
		public virtual void  passMPMessagesTieBreak(int iterations)
		{
			int tieIndex = 0;
			while (tieIndex != - 1)
			{
				resetMessages();
				passMPMessages(iterations);
				tieIndex = breakFirstTie();
			}
			return ;
		}
		
		/// <summary>Repeatedly break belief propagation ties and rerun the
		/// inference until no ties remain. Then continue to fix untied
		/// nodes and rerun inference. 
		/// </summary>
		public virtual void  maxProductBeliefPropagateTieBreakFixAll(int iterations)
		{
			maxProductBeliefPropagateTieBreak(iterations);
			
			for (int i = 0; i < nodes.Length; i++)
			{
				if (!nodes[i].Set)
				{
					nodes[i].setMAPValue();
					resetMessages();
					maxProductBeliefPropagate(iterations);
				}
			}
			
			return ;
		}
		
		/// <summary>Perform specified iterations of max-product belief
		/// propagation, break ties, and repeat until no ties remain. Then
		/// fix all nodes and rerun BP after each fix. 
		/// </summary>
		public virtual bool passMPMessagesTieBreakFixAll(int iterations)
		{
			passMPMessagesTieBreak(iterations);
			
			for (int i = 0; i < nodes.Length; i++)
			{
				if (!nodes[i].Set)
				{
					nodes[i].setMAPValue();
					resetMessages();
					passMPMessages(iterations);
				}
			}
			
			return true;
		}
		
		/// <summary>Perform belief propagation for a fixed number of iterations. 
		/// </summary>
		public virtual bool passMessages(int bpType, int iterations)
		{
			return passMessages(bpType, iterations, false, 0.0001);
		}
		
		/// <summary>Perform belief propagation. 
		/// </summary>
		public virtual bool passMessages(int bpType, int iterations, bool testConvergence, double convergenceLimit)
		{
			return passMessages(bpType, iterations, testConvergence, convergenceLimit, 1);
		}
		
		/// <summary>Perform belief propagation. 
		/// </summary>
		public virtual bool passMessages(int bpType, int iterations, bool testConvergence, double convergenceLimit, double stepSize)
		{
			//UPGRADE_ISSUE: Method 'java.lang.Integer.getInteger' was not converted. 'ms-help://MS.VSCC.2003/commoner/redir/redirect.htm?keyword="jlca1000_javalangIntegergetInteger_javalangString_int"'
			int numBPThreads = 1; //Integer.getInteger("mit.ai.mrf.MarkovRandomField.numBPThreads", 2);
			//UPGRADE_NOTE: Final was removed from the declaration of 'finished '. 'ms-help://MS.VSCC.2003/commoner/redir/redirect.htm?keyword="jlca1003"'
			bool[] finished = new bool[numBPThreads];
			//UPGRADE_NOTE: Final was removed from the declaration of 'converged '. 'ms-help://MS.VSCC.2003/commoner/redir/redirect.htm?keyword="jlca1003"'
			bool[] converged = new bool[numBPThreads];
			//UPGRADE_NOTE: Final was removed from the declaration of 'delta '. 'ms-help://MS.VSCC.2003/commoner/redir/redirect.htm?keyword="jlca1003"'
			double[] delta = new double[numBPThreads];
			
			testConvergence = true;			

			BeliefPropagationThread[] threads = new BeliefPropagationThread[numBPThreads];
			for (int t = 0; t < threads.Length; t++)
			{
				threads[t] = new BeliefPropagationThread(iterations, converged, bpType, finished, stepSize, testConvergence, delta, convergenceLimit, this, t, threads.Length);
				threads[t].Start();
			}
			
			try
			{
				for (int t = 0; t < threads.Length; t++)
				{
					threads[t].Join();
				}
			}
			catch (System.Threading.ThreadInterruptedException x)
			{
				throw new System.SystemException("Thread error in Possible Scenes.");
			}
			
			if (testConvergence)
			{
				for (int t = 0; t < converged.Length; t++)
				{
					if (!converged[t])
					{
						return false;
					}
				}
				
				return true;
			}
			
			return false;
		}
	}
}