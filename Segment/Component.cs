using System;
using System.Collections;
using Sketch;
using Featurefy;
using MathNet.Numerics.LinearAlgebra;

namespace Segment
{
	/// <summary>
	/// The Component class hold an array of substrokes that all may or may not be connected.
	/// By partitioning, the Component computes the connected components for everything in it.
	/// </summary>
	public class Component
	{		
		#region INTERNALS

		/// <summary>
		/// Empirical threshold for segmenting based on distance
		/// </summary>
		internal static double DISTANCETHRESHOLD = .05;

		/// <summary>
		/// Empirical threshold for segmenting based on time
		/// </summary>
		//internal static double TIMETHRESHOLD = 1000;

	
		/// <summary>
		/// Purely speed enhancement. This gets some costly loops to go through like += POINTCOARSNESS
		/// </summary>
		private static int POINTCOARSNESS = 1;

		/// <summary>
		/// Average distance between substroke i and j
		/// </summary>
		private Matrix average;
		
		/// <summary>
		/// Minimum distance between substroke i and j
		/// </summary>
		private Matrix minimum;
		
		/// <summary>
		/// Maximum distance between substroke i and j
		/// </summary>
		private Matrix maximum;

		/// <summary>
		/// 1.0 at [i, j] if the substrokes i and j are close enough
		/// </summary>
		private Matrix adjacency;

		/// <summary>
		/// 1.0 at [i, j] if the there is a path between i and j
		/// </summary>
		private Matrix connected;

		/// <summary>
		/// An array of ConnectedComponents
		/// </summary>
		private ArrayList connectedComponents;

		/// <summary>
		/// The substrokes
		/// </summary>
		private Substroke[] substrokes;

		private string label;

		#endregion

		#region CONSTRUCTORS

		/// <summary>
		/// Constructor
		/// </summary>
		public Component(string label)
		{
			//Do not modify this to null... connectedComponents must be initialized as an ArrayList
			this.connectedComponents = new ArrayList();

			this.average = null;
			this.minimum = null;
			this.maximum = null;
			this.adjacency = null;
			this.connected = null;

			this.label = label;
		}


		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="substrokes"></param>
		public Component(Substroke[] substrokes, string label) : this(label)
		{
			this.substrokes = substrokes;
		}


		#endregion

		#region CHANGE ADD 

		/// <summary>
		/// Ignore the old substrokes and use these substrokes as the new ones
		/// </summary>
		/// <param name="substrokes">Substrokes to use</param>
		public void changeSubstrokes(Substroke[] substrokes)
		{
			this.substrokes = substrokes;
		}


		/// <summary>
		/// Ignore the old substrokes and use these substrokes as the new ones 
		/// </summary>
		/// <param name="substrokes">Substrokes to use</param>
		public void changeSubstrokes(ArrayList substrokes)
		{
			this.changeSubstrokes((Substroke[])substrokes.ToArray(typeof(Substroke)));
		}


		/// <summary>
		/// Add a single substroke 
		/// </summary>
		/// <param name="substroke">The substroke to add</param>
		public void addSubstroke(Substroke substroke)
		{
			ArrayList al = new ArrayList();
			if(this.substrokes != null)
				al = new ArrayList(this.substrokes);
			al.Add(substroke);
			this.changeSubstrokes(al);
		}
		
		/// <summary>
		/// Do not call any calculate things after this...
		/// </summary>
		/// <param name="cc"></param>
		public void addCC(ConnectedComponent cc)
		{
			this.connectedComponents.Add(cc);
		}


		#endregion

		/// <summary>
		/// Calculate the three distance matrices (max, min, avg) as well as the connected matrix. 
		/// </summary>
		public void calculateAll()
		{
			if(this.substrokes != null)
			{
				this.calculateDistances();
				this.calculateConnected();
				this.initialPartition();
			}
		}

		
		#region PARTIONING

		/// <summary>
		/// Partition up the Substrokes into connected components.
		/// It works by finding the rows in the connected components Matrix that
		/// are the same.  All the rows that are the same are in the same component.
		/// </summary>
		private void initialPartition()
		{
			this.connectedComponents = new ArrayList();

			//Initially, nothing has been used
			Hashtable used = new Hashtable(this.substrokes.Length);
			for(int i = 0; i < this.substrokes.Length; ++i)
				used.Add(i, false);
			
			int cLength = 0;

			//Go through every row
			for(int i = 0; i < this.connected.RowCount; ++i)
			{			
				//Make sure we haven't used it in a component
				if(!(bool)used[i])
				{
					//Here is the row
					double[] key = this.getRow(this.connected, i);

					//Get the other rows that have the same key (i.e. are also connected, k_n)
					int[] same = this.getNodesWithSameRow(key);

					//Add a new part
					this.connectedComponents.Add(new ConnectedComponent(this.label));
					++cLength;

					//Add each of the same to parts
					for(int j = 0; j < same.Length; ++j)
					{
						((ConnectedComponent)this.connectedComponents[cLength - 1]).addSubstroke(this.substrokes[same[j]], 0.9);
						used[same[j]] = true;
					}
				}
			}
		}

		#endregion

		public double[] belief()
		{
			return Belief.belief(this);
		}

		#region ROW OPS

		private int[] getNodesWithSameRow(double[] row)
		{
			ArrayList rows = new ArrayList();
			for(int i = 0; i < this.connected.RowCount; ++i)
			{
				bool add = true;
				for(int j = 0; j < row.Length; ++j)
					if(row[j] != this.connected[i, j])
					{
						add = false;
						break;
					}

				if(add)
					rows.Add(i);
			}
			return (int[])rows.ToArray(typeof(int));
		}

		
		private double[] getRow(Matrix matrix, int row)
		{
			double[] r = new double[matrix.ColumnCount];
			for(int i = 0; i < r.Length; ++i)
				r[i] = matrix[row, i];
			return r;
		}

	
		#endregion

		#region CONNECTEDNESS

		/// <summary>
		/// Calculate the connected matrix
		/// </summary>
		private void calculateConnected()
		{
			int length = this.substrokes.Length;
			this.connected = new Matrix(length, length);
			for(int i = 0; i < length; ++i)
			{
				for(int j = i; j < length; ++j)
				{
					//If they are connected
					if(isConnected(i, j))
					{
						this.connected[i, j] = 1.0;
						this.connected[j, i] = 1.0;
					}
					else
					{
						//this.connected[i, j] = 0.0;
						//this.connected[j, i] = 0.0;
					}
				}
			}					
		}

		
		private bool isConnected(int start, int goal)
		{
			if(depthFirstSearch(start, goal).Count > 0)
				return true;
			else
				return false;
		}

		
		/// <summary>
		/// Depth first search returns the Stack representing the path from the start to the goal.
		/// The stack is empty if there is no path.
		/// </summary>
		/// <param name="start"></param>
		/// <param name="goal"></param>
		/// <returns></returns>
		private Stack depthFirstSearch(int start, int goal)
		{
			Hashtable visited = new Hashtable();
			for(int i = 0; i < this.substrokes.Length; ++i)
				visited.Add(i, false);
			visited[start] = true;
		
			Stack stack = new Stack(this.substrokes.Length);
			stack.Push(start);	
		
			while(stack.Count > 0)
			{
				int node = (int)stack.Peek();
				if(node == goal)
					return stack;
				else
				{
					int neighbor = this.getUnvisitedNeighbor(visited, node);
					if(neighbor == -1)
						stack.Pop();
					else
					{
						visited[neighbor] = true;
						stack.Push(neighbor);
					}
				}
			}
			return stack;
		}

		
		private int getUnvisitedNeighbor(Hashtable visited, int node)
		{
			ArrayList neighbors = this.getNeighbors(node);
			for(int i = 0; i < neighbors.Count; ++i)
			{
				if(!(bool)visited[neighbors[i]])
					return (int)neighbors[i];
			}
			return -1;
		}

		
		private ArrayList getNeighbors(int node)
		{
			ArrayList neighbors = new ArrayList();
			for(int i = 0; i < this.substrokes.Length; ++i)
			{
				if(this.adjacency[node, i] == 1.0)
					neighbors.Add(i);
			}
			return neighbors;
		}


		#endregion
		
		#region DISTANCES

		/// <summary>
		/// 
		/// </summary>
		/// <param name="i"></param>
		/// <param name="j"></param>
		/// <returns></returns>
		private bool isAdjacent(int i, int j)
		{
			
			Featurefy.ArcLength arc1 = new ArcLength(this.substrokes[i].Points);
			Featurefy.ArcLength arc2 = new ArcLength(this.substrokes[j].Points);	
			
			//ulong time1 = (ulong)this.substrokes[i].XmlAttrs.Time;
			//ulong time2 = (ulong)this.substrokes[j].XmlAttrs.Time;

			
			return (this.minimum[i, j]) / (arc1.Diagonal + arc2.Diagonal + 1) < Component.DISTANCETHRESHOLD;// || time2 - time1 < Component.TIMETHRESHOLD;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="a"></param>
		/// <param name="b"></param>
		/// <returns></returns>
		public static bool isAdjacent(Substroke a, Substroke b)
		{				
			
			Featurefy.ArcLength arc1 = new ArcLength(a.Points);
			Featurefy.ArcLength arc2 = new ArcLength(b.Points);
			
			double min = Component.minimumDistance(a, b);
			//double avg = Component.averageDistance(a, b);

			//ulong time1 = (ulong)a.XmlAttrs.Time;
			//ulong time2 = (ulong)b.XmlAttrs.Time;
			
			return min / (arc1.Diagonal + arc2.Diagonal + 1) < Component.DISTANCETHRESHOLD;// || time2 - time1 < Component.TIMETHRESHOLD;
		}

		/// <summary>
		/// Calculate average, minimum, maximum, and adjacency
		/// </summary>
		private void calculateDistances()
		{
			int length = this.substrokes.Length;
			this.average = new Matrix(length, length);
			this.minimum = new Matrix(length, length);
			this.maximum = new Matrix(length, length);
			this.adjacency = new Matrix(length, length);
			
			int i;
			int j;

			double avg;
			double[] minMax;
			double min;
			double max;

			//Find all the distances
			for(i = 0; i < length - 1; ++i)
			{
				for(j = i + 1; j < length; ++j)
				{
					avg = Component.averageDistance(this.substrokes[i], this.substrokes[j]);

					minMax = Component.minMaxDistance(this.substrokes[i], this.substrokes[j]);
					min = minMax[0];
					max = minMax[1];

					this.average[i, j] = avg;
					this.average[j, i] = avg;
					this.minimum[i, j] = min;
					this.minimum[j, i] = min;
					this.maximum[i, j] = max;
					this.maximum[j, i] = max;
				}
			}

			//Find the distance ajacency
			for(i = 0; i < length - 1; ++i)
			{
				for(j = i + 1; j < length; ++j)
				{
					//If they are adjacent
					if(this.isAdjacent(i, j))
					{
						this.adjacency[i, j] = 1.0;
						this.adjacency[j, i] = 1.0;
					}
					else
					{
						//this.adjacencyDistance[i, j] = 0.0;  //????
						//this.adjacencyDistance[j, i] = 0.0;
					}
				}
			}
		}

		
		/// <summary>
		/// Compute the power distance
		/// </summary>
		/// <param name="x1"></param>
		/// <param name="y1"></param>
		/// <param name="x2"></param>
		/// <param name="y2"></param>
		/// <param name="p"></param>
		/// <param name="r"></param>
		/// <returns></returns>
		internal static double powerDistance(double x1, double y1, double x2, double y2, double p, double r)
		{
			return Math.Pow(Math.Pow(x1 - x2, p) + Math.Pow(y1 - y2, p), 1.0 / r);
		}

		internal static double euclideanDistance(double x1, double y1, double x2, double y2)
		{
			return Math.Sqrt((x1 - x2)*(x1 - x2) + (y1 - y2)*(y1 - y2));
		}

		
		/// <summary>
		/// Get the average distance between two substrokes
		/// </summary>
		/// <param name="a"></param>
		/// <param name="b"></param>
		/// <returns></returns>
		public static double averageDistance(Substroke a, Substroke b)
		{
			Point aAvg = (new Spatial(a.Points)).AveragePoint;
			Point bAvg = (new Spatial(b.Points)).AveragePoint;

			/*
			float ax = aAvg.X;
			float ay = aAvg.Y;
			float bx = bAvg.X;
			float by = bAvg.Y;

			return Component.euclideanDistance(ax, ay, bx, by);
			*/
			return Component.euclideanDistance(aAvg.X, aAvg.Y, bAvg.X, bAvg.Y);
		}

		
		public static double minimumDistance(Substroke a, Substroke b)
		{
			Point[] aPoints = a.Points;
			Point[] bPoints = b.Points;

			double min = double.MaxValue;
			double dist;

			//If either are both wires, only compare endpoints
			if(a.FirstLabel.ToLower().Equals("wire") || b.FirstLabel.ToLower().Equals("wire"))
			{
				//a[0] vs b[]
				int i = 0;
				for(int j = 0; j < bPoints.Length; j += Component.POINTCOARSNESS)
				{
					dist = Component.euclideanDistance(aPoints[i].X, aPoints[i].Y, bPoints[j].X, bPoints[j].Y);
					if(dist < min)
						min = dist;
				}

				//a[last] vs b[]
				i = aPoints.Length - 1;
				for(int j = 0; j < bPoints.Length; j += Component.POINTCOARSNESS)
				{
					dist = Component.euclideanDistance(aPoints[i].X, aPoints[i].Y, bPoints[j].X, bPoints[j].Y);
					if(dist < min)
						min = dist;
				}

				//a[] vs b[0]
				i = 0;
				for(int j = 0; j < aPoints.Length; j += Component.POINTCOARSNESS)
				{
					dist = Component.euclideanDistance(aPoints[j].X, aPoints[j].Y, bPoints[i].X, bPoints[i].Y);
					if(dist < min)
						min = dist;
				}

				//a[] vs b[last]
				i = bPoints.Length - 1;
				for(int j = 0; j < aPoints.Length; j += Component.POINTCOARSNESS)
				{
					dist = Component.euclideanDistance(aPoints[j].X, aPoints[j].Y, bPoints[i].X, bPoints[i].Y);
					if(dist < min)
						min = dist;
				}				
			}
			else
			{
				for(int i = 0; i < aPoints.Length; i += Component.POINTCOARSNESS)
				{
					for(int j = 0; j < bPoints.Length; j += Component.POINTCOARSNESS)
					{
						dist = Component.euclideanDistance(aPoints[i].X, aPoints[i].Y, bPoints[j].X, bPoints[j].Y);
						if(dist < min)
							min = dist;
					}
				}
			}

			return min;
		}

	
		public static double maximumDistance(Substroke a, Substroke b)
		{
			Point[] aPoints = a.Points;
			Point[] bPoints = b.Points;

			double max = double.MinValue;
			double dist;

			//If they are both wires, only compare endpoints
			if(a.FirstLabel.ToLower().Equals("wire") || b.FirstLabel.ToLower().Equals("wire"))
			{
				//a[0] vs b[]
				int i = 0;
				for(int j = 0; j < bPoints.Length; j += Component.POINTCOARSNESS)
				{
					dist = Component.euclideanDistance(aPoints[i].X, aPoints[i].Y, bPoints[j].X, bPoints[j].Y);
					if(dist > max)
						max = dist;
				}

				//a[last] vs b[]
				i = aPoints.Length - 1;
				for(int j = 0; j < bPoints.Length; j += Component.POINTCOARSNESS)
				{
					dist = Component.euclideanDistance(aPoints[i].X, aPoints[i].Y, bPoints[j].X, bPoints[j].Y);
					if(dist > max)
						max = dist;
				}

				//a[] vs b[0]
				i = 0;
				for(int j = 0; j < aPoints.Length; j += Component.POINTCOARSNESS)
				{
					dist = Component.euclideanDistance(aPoints[j].X, aPoints[j].Y, bPoints[i].X, bPoints[i].Y);
					if(dist > max)
						max = dist;
				}

				//a[] vs b[last]
				i = bPoints.Length - 1;
				for(int j = 0; j < aPoints.Length; j += Component.POINTCOARSNESS)
				{
					dist = Component.euclideanDistance(aPoints[j].X, aPoints[j].Y, bPoints[i].X, bPoints[i].Y);
					if(dist > max)
						max = dist;
				}				
			}
			else
			{
				for(int i = 0; i < aPoints.Length; i += Component.POINTCOARSNESS)
				{
					for(int j = 0; j < bPoints.Length; j += Component.POINTCOARSNESS)
					{
						dist = Component.euclideanDistance(aPoints[i].X, aPoints[i].Y, bPoints[j].X, bPoints[j].Y);
						if(dist > max)
							max = dist;
					}
				}
			}

			return max;
		}

		
		public static double[] minMaxDistance(Substroke a, Substroke b)
		{
			Point[] aPoints = a.Points;
			Point[] bPoints = b.Points;

			double min = double.MaxValue;
			double max = double.MinValue;
			double dist;

			//If either is a wire only compare endpoints
			if(a.FirstLabel.ToLower().Equals("wire") || b.FirstLabel.ToLower().Equals("wire"))
			{
				//a[0] vs b[]
				int i = 0;
				for(int j = 0; j < bPoints.Length; j += Component.POINTCOARSNESS)
				{
					dist = Component.euclideanDistance(aPoints[i].X, aPoints[i].Y, bPoints[j].X, bPoints[j].Y);
					if(dist > max)
						max = dist;
					if(dist < min)
						min = dist;
				}

				//a[last] vs b[]
				i = aPoints.Length - 1;
				for(int j = 0; j < bPoints.Length; j += Component.POINTCOARSNESS)
				{
					dist = Component.euclideanDistance(aPoints[i].X, aPoints[i].Y, bPoints[j].X, bPoints[j].Y);
					if(dist > max)
						max = dist;
					if(dist < min)
						min = dist;
				}

				//a[] vs b[0]
				i = 0;
				for(int j = 0; j < aPoints.Length; j += Component.POINTCOARSNESS)
				{
					dist = Component.euclideanDistance(aPoints[j].X, aPoints[j].Y, bPoints[i].X, bPoints[i].Y);
					if(dist > max)
						max = dist;
					if(dist < min)
						min = dist;
				}

				//a[] vs b[last]
				i = bPoints.Length - 1;
				for(int j = 0; j < aPoints.Length; j += Component.POINTCOARSNESS)
				{
					dist = Component.euclideanDistance(aPoints[j].X, aPoints[j].Y, bPoints[i].X, bPoints[i].Y);
					if(dist > max)
						max = dist;
					if(dist < min)
						min = dist;
				}				
			}
			else
			{
				for(int i = 0; i < aPoints.Length; i += Component.POINTCOARSNESS)
				{
					for(int j = 0; j < bPoints.Length; j += Component.POINTCOARSNESS)
					{
						dist = Component.euclideanDistance(aPoints[i].X, aPoints[i].Y, bPoints[j].X, bPoints[j].Y);
						if(dist > max)
							max = dist;
						
						if(dist < min)
							min = dist;
					}
				}
			}

			return new double[] { min, max };
		}
		
		
		public double averageDistance(int a, int b)
		{
			if(this.average == null)
				this.calculateDistances();

			return this.average[a, b];
		}
		
		
		public double minimumDistance(int a, int b)
		{
			if(this.minimum == null)
				this.calculateDistances();

			return this.minimum[a, b];
		}

		
		public double maximumDistance(int a, int b)
		{
			if(this.maximum == null)
				this.calculateDistances();

			return this.maximum[a, b];
		}

		
		public double[] normalizedDistances(int a, int b)
		{
			double avg = this.averageDistance(a, b);
			double min = this.minimumDistance(a, b);
			double max = this.maximumDistance(a, b);
			min /= (avg + 1);
			max /= (avg + 1);

			return new double[] { avg, min, max };
		}


		#endregion

		#region GET

		public ArrayList ConnectedComponents
		{
			get
			{
				if(this.connectedComponents == null)
					this.calculateAll();
				return this.connectedComponents;
			}
		}	

		#endregion
	}
}
