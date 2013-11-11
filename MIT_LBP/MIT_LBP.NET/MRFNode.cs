/*
* MRFNode.java
*
* Michael Ross
*
* 4/5/02
*
*/
namespace mit.ai.mrf
{
	using System;
	using System.Collections;
	using System.Runtime.Serialization;

	/// <summary>Element of a Markov random field, holding neighborhood information. 
	/// </summary>
	[Serializable]
	public class MRFNode //: System.Runtime.Serialization.ISerializable
	{
		/// <summary>Return access to the messages list. 
		/// </summary>
		virtual public ArrayList Messages
		{
			get
			{
				return msgs;
			}		
		}

		/// <summary>Return access to the neighbor list. 
		/// </summary>
		virtual public ArrayList Neighbors
		{
			get
			{
				return neighbors;
			}
			
		}
		
		/// <summary>Return array of possible scenes. 
		/// </summary>
		virtual public HashIndex[] PossibleScenes
		{
			get
			{
				return possibleScenes;
			}
			
			set
			{
				possibleScenes = value;
				localCompats = new double[possibleScenes.Length];
				fullLocalCompats = localCompats;
				
				for (int i = 0; i < localCompats.Length; i++)
					localCompats[i] = 1;
				
				return;
			}
		}
		
		/// <summary>Check if value is set. 
		/// </summary>
		virtual public bool Set
		{
			get
			{
				return set;
			}	
		}

        /// <summary>Checks if the node is an evidenceNode. 
        /// </summary>
        virtual public bool EvidenceNode
        {
            get 
            {
                return evidenceNode;
            }

            set 
            {
                evidenceNode = value;
            }

        }


		/// <summary>Fix the node to a particular subset of values. 
		/// </summary>
		virtual public int[] Values
		{
			set
			{
				if (set)
				{
					unsetValue();
				}
				set = true;
				fullLocalCompats = localCompats;
				localCompats = new double[possibleScenes.Length];
				
				for (int i = 0; i < value.Length; i++)
				{
					localCompats[value[i]] = fullLocalCompats[value[i]];
				}
				
				return ;
			}	
		}

		/// <summary>Fix the node to a particular value. 
		/// </summary>
		virtual public int Value
		{
			set
			{
				if (set)
				{
					unsetValue();
				}
				set = true;
				fullLocalCompats = localCompats;
				localCompats = new double[possibleScenes.Length];
				localCompats[value] = fullLocalCompats[value];
				
				return ;
			}
		}

		virtual public double[] LocalCompatibilities
		{
			get
			{
				return localCompats;
			}	
		}

		virtual public double[] LocalMatches
		{
			set
			{
				localCompats = value;
				fullLocalCompats = localCompats;
				return ;
			}	
		}
		
		internal const long serialVersionUID = 7622821961596096962L;
		protected internal const int MAX_PRODUCT = 0;
		protected internal const int SUM_PRODUCT = 1;
		protected internal HashIndex[] possibleScenes; /// <summary>Scenes to consider. 
		/// </summary>
		protected internal double[] localCompats; /// <summary>Local data compatibility. 
		/// </summary>
		protected internal ArrayList neighbors; /// <summary>Neighbors. 
		/// </summary>
		protected internal ArrayList selfIndices; /// <summary>Who am I to them? 
		/// </summary>
		protected internal ArrayList msgs; /// <summary>Messages from neighbors. 
		/// </summary>
		protected internal ArrayList newMsgs; /// <summary>Storage for updated messages. 
		/// </summary>
		protected internal Hashtable neighCompats; /// <summary>Hammersley-Clifford compatibilities. 
		/// </summary>
		protected internal double[][] neighMsgs; /// <summary>A cache used in BP. 
		/// </summary>
		protected internal double[] fullLocalCompats; /// <summary>Storage used in fixing. 
		/// </summary>
		protected internal bool set; /// <summary>Indicate if this node's value has been fixed. 
		/// </summary>
		private ArrayList neighCompatScratch;
        /// <summary>Indicates if this node is an evidence node. 
        /// </summary>
        protected internal bool evidenceNode;

		/// <summary>Construct a new dataless node with no set scenes or compats. 
		/// </summary>
		public MRFNode()
		{
			neighCompats = new Hashtable();
			neighbors = new ArrayList();
			selfIndices = new ArrayList();
			msgs = new ArrayList();
			newMsgs = new ArrayList();
			neighMsgs = new double[0][];

            set = false;
            evidenceNode = false;

			neighCompatScratch = new ArrayList();
			return ;
		}
		
		/// <summary>Construct a new node with scenes, but default compats. 
		/// </summary>
		public MRFNode(HashIndex[] initScenes):this()
		{
			PossibleScenes = initScenes;
		}
		
		/// <summary>Construct a new dataless node with preset localCompats. 
		/// </summary>
		public MRFNode(HashIndex[] initScenes, double[] initLocalCompats):this()
		{
			setPossibleScenes(initScenes, initLocalCompats);
			return ;
		}
		
		/// <summary>Detect if there are currently tied MAP estimate values. 
		/// </summary>
		public virtual bool existMAPTie()
		{
			double mapval = System.Double.NegativeInfinity;
			bool ties = false;
			
			for (int arg = 0; arg < possibleScenes.Length; arg++)
			{
				double curmapval = localCompats[arg];
				
				for (int m = 0; m < msgs.Count; m++)
				{
					curmapval *= ((double[]) msgs[m])[arg];
				}
				
				if (curmapval > mapval)
				{
					mapval = curmapval;
					ties = false;
				}
				else if (curmapval == mapval)
				{
					ties = true;
				}
			}
			return ties;
		}
		
		
		/// <summary>Fix the node to the current MAP estimate (uses the first max
		/// valued estimate discovered in case of a tie). 
		/// </summary>
		public virtual void setMAPValue()
		{
			Value = mapEstimateIndex();
			return ;
		}
		

		/// <summary>Restore the node to it's original values and compatibilities. 
		/// </summary>
		public virtual void unsetValue()
		{
			if (set)
			{
				localCompats = fullLocalCompats;
			}
			set = false;
			return ;
		}
		
		
		/// <summary>Add a neighbor. 
		/// </summary>
		public virtual void addNeighbor(MRFNode newNeighbor)
		{
			if (!neighbors.Contains(newNeighbor))
			{
				neighbors.Add(newNeighbor);
				double[] nmsgs = new double[possibleScenes.Length];
				double[] nnewMsgs = new double[possibleScenes.Length];
				
				for (int i = 0; i < nmsgs.Length; i++)
					nmsgs[i] = 1.0;
				
				for (int i = 0; i < nnewMsgs.Length; i++)
					nnewMsgs[i] = 1.0;
				
				msgs.Add(nmsgs);
				newMsgs.Add(nnewMsgs);
				
				newNeighbor.neighbors.Add(this);
				double[] n2msgs = new double[newNeighbor.possibleScenes.Length];
				double[] n2newmsgs = new double[newNeighbor.possibleScenes.Length];
				
				for (int i = 0; i < n2msgs.Length; i++)
					n2msgs[i] = 1.0;

				for (int i = 0; i < n2newmsgs.Length; i++)
					n2newmsgs[i] = 1.0;
				
				newNeighbor.msgs.Add(n2msgs);
				newNeighbor.newMsgs.Add(n2newmsgs);
				
				selfIndices.Add(newNeighbor.neighbors.Count - 1);
				newNeighbor.selfIndices.Add(neighbors.Count - 1);
				
				neighCompatScratch.Add(new double[newNeighbor.possibleScenes.Length]);
				newNeighbor.neighCompatScratch.Add(new double[possibleScenes.Length]);
				
				double[][] compats = new double[possibleScenes.Length][];
				for (int i = 0; i < possibleScenes.Length; i++)
				{
					compats[i] = new double[newNeighbor.possibleScenes.Length];
				}
				for (int i = 0; i < compats.Length; i++)
				{
					for (int k = 0; k < compats[i].Length; k++)
						compats[i][k] = 1.0;
				}
				neighCompats.Add(newNeighbor, compats);
			}
			
			return;
		}
		

		/// <summary>Add a neighbor. 
		/// </summary>
		public virtual void addNeighbor(MRFNode newNeighbor, double[][] edgePot)
		{
			if (!neighbors.Contains(newNeighbor))
			{
				neighbors.Add(newNeighbor);
				double[] nmsgs = new double[possibleScenes.Length];
				double[] nnewMsgs = new double[possibleScenes.Length];
				
				for (int i = 0; i < nmsgs.Length; i++)
					nmsgs[i] = 1.0;

				for (int i = 0; i < nnewMsgs.Length; i++)
					nnewMsgs[i] = 1.0;

				msgs.Add(nmsgs);
				newMsgs.Add(nnewMsgs);
				
				newNeighbor.neighbors.Add(this);
				double[] n2msgs = new double[newNeighbor.possibleScenes.Length];
				double[] n2newmsgs = new double[newNeighbor.possibleScenes.Length];
				
				for (int i = 0; i < n2msgs.Length; i++)
					n2msgs[i] = 1.0;

				for (int i = 0; i < n2newmsgs.Length; i++)
					n2newmsgs[i] = 1.0;
				
				newNeighbor.msgs.Add(n2msgs);
				newNeighbor.newMsgs.Add(n2newmsgs);
				
				selfIndices.Add(newNeighbor.neighbors.Count - 1);
				newNeighbor.selfIndices.Add(neighbors.Count - 1);
				
				neighCompatScratch.Add(new double[newNeighbor.possibleScenes.Length]);
				newNeighbor.neighCompatScratch.Add(new double[possibleScenes.Length]);
				
				neighCompats.Add(newNeighbor, edgePot);
			}
			
			return;
		}


		/// <summary>Remove a neighbor. 
		/// </summary>
		public virtual void  removeNeighbor(MRFNode neigh)
		{
			if (neighbors.Contains(neigh))
			{
				int nind = neighbors.IndexOf(neigh);
				neighbors.Remove(nind);
				selfIndices.Remove(nind);
				msgs.Remove(nind);
				newMsgs.Remove(nind);
				
				int rind = neigh.neighbors.IndexOf(this);
				neigh.neighbors.Remove(rind);
				neigh.selfIndices.Remove(rind);
				neigh.msgs.Remove(rind);
				neigh.newMsgs.Remove(rind);
				
				if (neighCompats.ContainsKey(neigh))
				{
					neighCompats.Remove(neigh);
				}
				else
				{
					neigh.neighCompats.Remove(this);
				}
			}
			
			return ;
		}
		
		/// <summary>Reset all messages to 1. 
		/// </summary>
		public virtual void  resetMessages()
		{
			for (int i = 0; i < neighbors.Count; i++)
			{
				double[] tmpMsgs = (double[])msgs[i];
				for (int k = 0; k < tmpMsgs.Length; k++)
					tmpMsgs[k] = 1.0;

				msgs[i] = tmpMsgs;

				double[] tmpNewMsgs = (double[])newMsgs[i];
				for (int k = 0; k < tmpNewMsgs.Length; k++)
					tmpNewMsgs[k] = 1.0;

				newMsgs[i] = tmpNewMsgs;
			}
			return;
		}
		
		/// <summary>Return true if this node neighbors the argument node. 
		/// </summary>
		public virtual bool isNeighbor(MRFNode potentialNeighbor)
		{
			return neighbors.Contains(potentialNeighbor);
		}
		
		/// <summary>Find the index of a particular neighbor. 
		/// </summary>
		public virtual int neighborIndex(MRFNode neighbor)
		{
			return neighbors.IndexOf(neighbor);
		}
		
		/// <summary>Return the beliefs (approximate pdf) for a pair of neighboring
		/// nodes. 
		/// </summary>
		public static double[][] beliefs(MRFNode nodeA, MRFNode nodeB)
		{
			if (!nodeA.isNeighbor(nodeB))
			{
				throw new System.ArgumentException("Nodes are not neighbors.");
			}
			int ani = nodeB.neighborIndex(nodeA);
			int bni = nodeA.neighborIndex(nodeB);
			double norm = 0;
			double[][] bvals = new double[nodeA.possibleScenes.Length][];
			for (int i = 0; i < nodeA.possibleScenes.Length; i++)
			{
				bvals[i] = new double[nodeB.possibleScenes.Length];
			}
			for (int a = 0; a < bvals.Length; a++)
			{
				for (int b = 0; b < bvals[0].Length; b++)
				{
					bvals[a][b] = nodeA.localCompats[a] * nodeB.localCompats[b] * nodeA.neighborMatch(nodeB, a, b);
					
					for (int m = 0; m < nodeA.msgs.Count; m++)
					{
						if (m != bni)
						{
							bvals[a][b] *= ((double[]) nodeA.msgs[m])[a];
						}
					}
					for (int m = 0; m < nodeB.msgs.Count; m++)
					{
						if (m != ani)
						{
							bvals[a][b] *= ((double[]) nodeB.msgs[m])[b];
						}
					}
					norm += bvals[a][b];
				}
			}
			
			for (int a = 0; a < bvals.Length; a++)
			{
				for (int b = 0; b < bvals[0].Length; b++)
				{
					if (norm != 0)
						bvals[a][b] /= norm;
					else
						continue;
				}
			}
			
			return bvals;
		}
		
		/// <summary>Return the beliefs (approximate pdf). 
		/// </summary>
		public virtual double[] beliefs()
		{
			double[] bvals = new double[possibleScenes.Length];
			double norm = 0;
			for (int arg = 0; arg < bvals.Length; arg++)
			{
				bvals[arg] = localCompats[arg];
				
				for (int m = 0; m < msgs.Count; m++)
				{
					bvals[arg] *= ((double[]) msgs[m])[arg];
				}
				
				norm += bvals[arg];
			}
			for (int arg = 0; arg < bvals.Length; arg++)
			{
				if (norm != 0)
					bvals[arg] /= norm;
				else
					continue;
			}
			return bvals;
		}

		/// <summary>Return the beliefs (approximate pdf) for a pair of neighboring
		/// nodes. 
		/// </summary>
		public virtual double[][] beliefs(MRFNode neighbor)
		{
			if (!this.isNeighbor(neighbor))
			{
				throw new System.ArgumentException("Nodes are not neighbors.");
			}
			
			int ani = neighbor.neighborIndex(this);
			int bni = this.neighborIndex(neighbor);
			double norm = 0;
			
			double[][] bvals = new double[this.possibleScenes.Length][];
			for (int i = 0; i < this.possibleScenes.Length; i++)
			{
				bvals[i] = new double[neighbor.possibleScenes.Length];
			}

			for (int a = 0; a < bvals.Length; a++)
			{
				for (int b = 0; b < bvals[0].Length; b++)
				{
					bvals[a][b] = this.localCompats[a] * neighbor.localCompats[b] * this.neighborMatch(neighbor, a, b);
					
					for (int m = 0; m < this.msgs.Count; m++)
					{
						if (m != bni)
						{
							bvals[a][b] *= ((double[])this.msgs[m])[a];
						}
					}
					for (int m = 0; m < neighbor.msgs.Count; m++)
					{
						if (m != ani)
						{
							bvals[a][b] *= ((double[])neighbor.msgs[m])[b];
						}
					}
					norm += bvals[a][b];
				}
			}
			
			for (int a = 0; a < bvals.Length; a++)
			{
				for (int b = 0; b < bvals[0].Length; b++)
				{
					if (norm != 0)
						bvals[a][b] /= norm;
					else
						continue;
				}
			}
			
			return bvals;
		}

		
		/// <summary>Compute the index of the MAP scene estimate. 
		/// </summary>
		public virtual int mapEstimateIndex()
		{
			double mapval = System.Double.NegativeInfinity;
			int mapArg = 0;
			for (int arg = 0; arg < possibleScenes.Length; arg++)
			{
				double curmapval = localCompats[arg];
				
				for (int m = 0; m < msgs.Count; m++)
				{
					curmapval *= ((double[]) msgs[m])[arg];
				}
				
				if (curmapval > mapval)
				{
					mapval = curmapval;
					mapArg = arg;
				}
			}
			return mapArg;
		}
		
		/// <summary>Compute the MAP scene estimate. 
		/// </summary>
		public virtual HashIndex mapEstimate()
		{
			return PossibleScenes[mapEstimateIndex()];
		}
		
		/// <summary>Update with a linear combination of the old and new messages. 
		/// </summary>
		public virtual void  update(double step)
		{
			if (step > 1)
				step = 1;
			if (step < 0)
				step = 0;
			
			int numNeighbors = msgs.Count;
			
			for (int i = 0; i < numNeighbors; i++)
			{
				double[] vals = (double[]) msgs[i];
				double[] nvals = (double[]) newMsgs[i];
				
				for (int v = 0; v < vals.Length; v++)
				{
					nvals[v] = System.Math.Exp(step * System.Math.Log(nvals[v]) + (1 - step) * System.Math.Log(vals[v]));
					
				}
				
			}
			
			update();
			
			return ;
		}
		
		/// <summary>Swap the updated messages with the old ones. 
		/// </summary>
		public virtual void  update()
		{
			ArrayList tempMsgs = msgs;
			msgs = newMsgs;
			newMsgs = tempMsgs;
			return ;
		}
		
		/// <summary>Check convergence by comparing the two sets of messages. 
		/// </summary>
		public virtual double delta()
		{
			double diff = 0;
			for (int m = 0; m < msgs.Count; m++)
			{
				double[] newMessage = (double[]) msgs[m];
				double[] oldMessage = (double[]) newMsgs[m];
				
				for (int e = 0; e < newMessage.Length; e++)
				{
					diff += System.Math.Abs(newMessage[e] - oldMessage[e]);
				}
			}
			return diff;
		}
		
		/// <summary>Rescale the messages to avoid precision problems. 
		/// </summary>
		public virtual void  rescale()
		{
			int numNeighbors = msgs.Count;
			for (int i = 0; i < numNeighbors; i++)
			{
				double[] messages = (double[]) msgs[i];
				double max = System.Double.NegativeInfinity;
				for (int m = 0; m < messages.Length; m++)
				{
					if (!System.Double.IsNaN(messages[m]) && !System.Double.IsInfinity(messages[m]))
					{
						max = System.Math.Max(max, messages[m]);
					}			
					else
					{
						throw new System.ArithmeticException("NaN/Inf detected");
					}
				}
				if (max != 0)
				{
					for (int m = 0; m < messages.Length; m++)
					{
						messages[m] /= max;
					}
				}
				else
				{
					throw new System.ArithmeticException("Zero scale detected");
				}
			}
			
			return ;
		}
		
		/// <summary>Iterate sum-product belief propagation for this node. 
		/// </summary>
		public virtual void  passSPMessages()
		{
			passMessages(SUM_PRODUCT);
			return ;
		}
		
		/// <summary>Iterate max-product belief propagation for this node. 
		/// </summary>
		public virtual void  passMPMessages()
		{
			//passMessages(MAX_PRODUCT);
			
			passLogMessages();
			return ;
		}
		
		/// <summary>Iterate belief propagation for this node. 
		/// </summary>
		protected internal virtual void  passMessages(int bpType)
		{
			int neighborsSize = neighbors.Count;
			for (int neighInd = 0; neighInd < neighborsSize; neighInd++)
			{
				MRFNode neighNode = (MRFNode) neighbors[neighInd];
				int selfInd = ((System.Int32) selfIndices[neighInd]);
				
				neighMsgs = (double[][])neighNode.msgs.ToArray(typeof (double[]));
				
				bool revCompat = false;
                // Edge potentials.
				double[][] ncompats = (double[][]) neighCompats[neighNode];

                double[] ncMult = (double[]) neighCompatScratch[neighInd];

				for (int ncand = 0; ncand < neighNode.possibleScenes.Length; ncand++)
				{
                    //Site potentials of the neighbor.
					ncMult[ncand] = neighNode.localCompats[ncand];

                    //if (ncMult[ncand] != 0)
					if (ncMult[ncand] != 0 && !neighNode.EvidenceNode)
					{
						for (int m = 0; m < neighMsgs.Length && neighMsgs[m] != null; m++)
						{
							if (m != selfInd)
							{
								ncMult[ncand] *= neighMsgs[m][ncand];
							}
						}
					}
				}
				
				if (ncompats == null)
				{
					ncompats = (double[][]) neighNode.neighCompats[this];
					revCompat = true;
				}
				
				double[] newMsgsNeigh = (double[]) newMsgs[neighInd];
				
				for (int cand = 0; cand < possibleScenes.Length; cand++)
				{
					double maxVal = System.Double.NegativeInfinity;
					double sumVal = 0;
					
					for (int neighCand = 0; neighCand < neighNode.possibleScenes.Length; neighCand++)
					{
						double curVal = ncMult[neighCand];
						
						if (!revCompat)
						{
							curVal *= ncompats[cand][neighCand];
						}
						else
						{
							curVal *= ncompats[neighCand][cand];
						}
						
						if (bpType == MAX_PRODUCT && curVal > maxVal)
						{
							maxVal = curVal;
						}
						else if (bpType == SUM_PRODUCT)
						{
   
							if (curVal == 0) 
							{
								System.Console.WriteLine( "precision problems?" );
							}
							sumVal += curVal;
						}
					}
					
					if (bpType == MAX_PRODUCT)
					{
						newMsgsNeigh[cand] = maxVal;
					}
					else if (bpType == SUM_PRODUCT)
					{
						newMsgsNeigh[cand] = sumVal;
						//if (sumVal == 0) 
						//{
						//	System.Console.WriteLine( "precision problems?" );
						//}
					}
					else
					{
						throw new System.ArgumentException("bpType: " + bpType + " is unknown.");
					}
				}
			}
			
			return;
		}


		/// <summary>Iterate belief propagation for this node. 
		/// </summary>
		protected internal virtual void  passLogMessages()
		{
          
			int neighborsSize = neighbors.Count;
			for (int neighInd = 0; neighInd < neighborsSize; neighInd++)
			{

                MRFNode neighNode = (MRFNode) neighbors[neighInd];
				int selfInd = ((System.Int32) selfIndices[neighInd]);
				
				neighMsgs = (double[][])neighNode.msgs.ToArray(typeof (double[]));
				
				bool revCompat = false;
				
                // Edge potential.
				double[][] logNCompats = (double[][]) neighCompats[neighNode];
				
				if (logNCompats != null)
				{
					foreach (double[] neigh in logNCompats)
						for (int i = 0; i < neigh.Length; i++)
							neigh[i] = Math.Log(neigh[i]);
				}
				

				double[] logNCMult = (double[]) neighCompatScratch[neighInd];
                
				for (int i = 0; i < logNCMult.Length; i++)
					logNCMult[i] = Math.Log(logNCMult[i]);

                for (int ncand = 0; ncand < neighNode.possibleScenes.Length; ncand++)
				{
                    // Getting the site potential of the neighbouring node.
                    // Uncomment the commented code in the "if" statement if evidence nodes are present.
                    //if (!neighNode.evidenceNode)
                        logNCMult[ncand] = Math.Log(neighNode.localCompats[ncand]);
                    //else
                    //{
                    //    if (ncand == 1) logNCMult[ncand] = 0;
                    //    if (ncand != 1) logNCMult[ncand] = Double.NegativeInfinity;
                    //}

                    // Uncomment the line below in case of evidence nodes and comment the one below.
                    //if (!Double.IsInfinity(logNCMult[ncand]) && !neighNode.evidenceNode)
                    if (!Double.IsInfinity(logNCMult[ncand]))
                    {
						for (int m = 0; m < neighMsgs.Length && neighMsgs[m] != null; m++)
						{
                            
							if (m != selfInd)
							{
								logNCMult[ncand] += Math.Log(neighMsgs[m][ncand]);
							}
						}
					}
				}
				
				if (logNCompats == null)
				{
					logNCompats = (double[][]) neighNode.neighCompats[this];
					
					if (logNCompats != null)
					{
						foreach (double[] neigh in logNCompats)
							for (int i = 0; i < neigh.Length; i++)
								neigh[i] = Math.Log(neigh[i]);
					}
					
					revCompat = true;
				}
				
				double[] newMsgsNeigh = (double[]) newMsgs[neighInd];

				for (int cand = 0; cand < possibleScenes.Length; cand++)
				{
					double maxVal = System.Double.NegativeInfinity;
					
					for (int neighCand = 0; neighCand < neighNode.possibleScenes.Length; neighCand++)
					{
						double curVal = logNCMult[neighCand];
						
						if (!revCompat)
						{
							curVal += logNCompats[cand][neighCand];
						}
						else
						{
							curVal += logNCompats[neighCand][cand];
						}
						
						if (curVal > maxVal)
						{
							maxVal = curVal;
						}
					}

					newMsgsNeigh[cand] = Math.Exp(maxVal);

				}


				if (logNCompats != null)
				{
					for (int i = 0; i < logNCompats.Length; i++)
					{
						double[] neighs = logNCompats[i];

						for (int k = 0; k < neighs.Length; k++)
							logNCompats[i][k] = Math.Exp(logNCompats[i][k]);
					}
				}
				
				for (int i = 0; i < logNCMult.Length; i++)
					logNCMult[i] = Math.Exp(logNCMult[i]);


			}

			return;
		}

		
		/// <summary>Represent the data for this node as a String. 
		/// </summary>
		public override System.String ToString()
		{
			System.String out_Renamed = new System.String("".ToCharArray());
			out_Renamed += "Node:\n";
			HashIndex[] scenes = PossibleScenes;
			double[] compats = LocalCompatibilities;
			for (int i = 0; i < scenes.Length; i++)
			{
				out_Renamed += "[" + i + "]\t" + scenes[i] + " localCompat: " + compats[i] + "\n";
			}
			
			return out_Renamed;
		}
		
		public virtual double localMatch(int val)
		{
			return localCompats[val];
		}
		
		
		public virtual double[][] getNeighborCompatibilities(MRFNode neigh)
		{
			double[][] compatMatrix = new double[PossibleScenes.Length][];
			for (int i = 0; i < PossibleScenes.Length; i++)
			{
				compatMatrix[i] = new double[neigh.PossibleScenes.Length];
			}
			
			for (int i = 0; i < PossibleScenes.Length; i++)
			{
				for (int j = 0; j < neigh.PossibleScenes.Length; j++)
				{
					compatMatrix[i][j] = neighborMatch(neigh, i, j);
				}
			}
			
			return compatMatrix;
		}
		
		public virtual double neighborMatch(MRFNode neigh, int thisVal, int neighVal)
		{
			double[][] compat = (double[][]) neighCompats[neigh];
			
			if (compat == null)
			{
				compat = (double[][]) neigh.neighCompats[this];
				if (compat == null)
				{
					throw new System.ArgumentException("These nodes are not neighbors.");
				}
				else
				{
					return compat[neighVal][thisVal];
				}
			}
			else
			{
				return compat[thisVal][neighVal];
			}
		}
		
		public virtual void  setNeighborMatch(MRFNode neigh, int thisVal, int neighVal, double matchval)
		{
			double[][] compat = (double[][]) neighCompats[neigh];
			
			if (compat == null)
			{
				compat = (double[][]) neigh.neighCompats[this];
				if (compat == null)
				{
					throw new System.ArgumentException("These nodes are not neighbors.");
				}
				else
				{
					compat[neighVal][thisVal] = matchval;
				}
			}
			else
			{
				compat[thisVal][neighVal] = matchval;
			}
			
			return;
		}
		
		public virtual void  setLocalMatch(int thisVal, double matchVal)
		{
			localCompats[thisVal] = matchVal;
			return;
		}
		
		
		
		public virtual void  setPossibleScenes(HashIndex[] initPossibleScenes, double[] matchValues)
		{
			possibleScenes = initPossibleScenes;
			localCompats = matchValues;
			fullLocalCompats = localCompats;
			return;
		}
	}
}