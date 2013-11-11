using System;
using System.Collections;
using Sketch;

namespace Segment
{
	/// <summary>
	/// Summary description for ConnectedComponent.
	/// </summary>
	public class ConnectedComponent
	{
		#region INTERNALS

		/// <summary>
		/// Substrokes representing the ConnectedComponent
		/// </summary>
		private ArrayList substrokes;

		/// <summary>
		/// Doubles array representing our belief in the corresponding stroke
		/// </summary>
		private ArrayList substrokesBelief;

		/// <summary>
		/// The label associated with this ConnectedComponent
		/// </summary>
		private string label;

		/// <summary>
		/// The product of the substrokesBelief
		/// </summary>
		private double naiveBelief;


		private double matchingBelief;

		#endregion

		#region CONSTRUCTORS

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="label">Label of belief</param>
		public ConnectedComponent(string label)
		{
			this.substrokes = new ArrayList();
			this.substrokesBelief = new ArrayList();
			this.naiveBelief = -1.0;
			this.matchingBelief = -1.0;
			this.label = label;
		}


		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="substrokes"></param>
		/// <param name="label"></param>
		/// <param name="percentage"></param>
		public ConnectedComponent(ArrayList substrokes, ArrayList substrokesBeliefs, string label) : this(label)
		{
			this.substrokes = substrokes;
			this.substrokesBelief = substrokesBeliefs;
		}

		
		#endregion

		#region ADD/REMOVE

		/// <summary>
		/// Add a Substroke
		/// </summary>
		/// <param name="substroke"></param>
		public void addSubstroke(Substroke substroke, double belief)
		{
			this.substrokes.Add(substroke);
			this.substrokesBelief.Add(belief);

			this.naiveBelief = -1.0;
			this.matchingBelief = -1.0;
		}

		
		/// <summary>
		/// Add Substrokes
		/// </summary>
		/// <param name="substrokes"></param>
		public void addSubstrokes(Substroke[] substrokes, double[] beliefs)
		{
			for(int i = 0; i < substrokes.Length; ++i)
				this.addSubstroke(substrokes[i], beliefs[i]);			
		}

		
		/// <summary>
		/// Add Substrokes
		/// </summary>
		/// <param name="substrokes"></param>
		public void addSubstrokes(ArrayList substrokes, ArrayList beliefs)
		{
			for(int i = 0; i < substrokes.Count; ++i)
				this.addSubstroke((Substroke)substrokes[i], (double)beliefs[i]);
		}

		
		/// <summary>
		/// Remove the given Substroke from the CC
		/// </summary>
		/// <param name="substroke"></param>
		public void removeSubstroke(Substroke substroke)
		{
			this.removeSubstroke(this.substrokes.IndexOf(substroke));
		}

		
		/// <summary>
		/// Remove the Substroke from the given index from the CC
		/// </summary>
		/// <param name="index"></param>
		public void removeSubstroke(int index)
		{

			this.substrokes.RemoveAt(index);
			this.substrokesBelief.RemoveAt(index);
			
			this.naiveBelief = -1.0;
			this.matchingBelief = -1.0;
		}
		
	
		#endregion

		#region ADJACENCY

		/// <summary>
		/// Computes whether two ConnectedComponents (at least one is a wire) are adjacent.
		/// It does that by checking each substroke in one against the other. 
		/// </summary>
		/// <param name="a"></param>
		/// <param name="b"></param>
		/// <returns></returns>
		public static bool isAdjacentWires(ConnectedComponent a, ConnectedComponent b)
		{
			//A or B must be a wire!
			if(!a.label.ToLower().Equals("wire") && !b.label.ToLower().Equals("wire"))
				return false;

			//Loop through A and B, return true if any is found to be adjacent
			for(int i = 0; i < a.substrokes.Count; ++i)
				for(int j = 0; j < b.substrokes.Count; ++j)
					if(Component.isAdjacent((Substroke)a.substrokes[i], (Substroke)b.substrokes[j]))
						return true;			
			
			return false;
		}

		
		/// <summary>
		/// Compute whether cutting at index would break the ConnectedComponent.
		/// </summary>
		/// <param name="index"></param>
		/// <returns></returns>
		public bool isCutSubstroke(int index, out Component toTest)
		{
			//To test this, we will use a Component!
			//We will add everything but index.
			//If the # of ConnectedComponents is still
			//1, then it doesn't break the ConnectedComponent!

			toTest = new Component(this.label);
			for(int i = 0; i < this.substrokes.Count; ++i)
			{
				if(i == index)
					continue;
				else
					toTest.addSubstroke((Substroke)this.substrokes[i]);
			}

			toTest.calculateAll();
			
			if(toTest.ConnectedComponents.Count > 1)
				return true;
			else
				return false;
		}

		#endregion

		public override string ToString()
		{
			string toReturn = label + "\n";
			for(int i = 0; i < label.Length; ++i)
				toReturn += "-";
			toReturn += "\n";
			for(int i = 0; i < substrokes.Count; ++i)
				toReturn += substrokesBelief[i] + "\n";
			toReturn += "Naive:" + this.NaiveBelief + "\n";
			toReturn += "Matching: " + this.MatchingBelief;
			return toReturn;
		}

		
		#region BELIEF

		/// <summary>
		/// Calculates the naiveBelief by multiplying the percentage of each label
		/// </summary>
		private void calculateNaiveBelief()
		{
			this.naiveBelief = 1.0;
			for(int i = 0; i < substrokesBelief.Count; ++i)
				naiveBelief *= (double)substrokesBelief[i];
		}

		
		/// <summary>
		/// Calculates the matchingBelief by using Belief
		/// </summary>
		private void calculateMatchingBelief()
		{
			this.matchingBelief = Belief.labelBeliefPercentage(this, this.label);
		}

		public double belief()
		{
			return Belief.belief(this);
		}


		#endregion

		#region GETTERS

		/// <summary>
		/// Get the Naive Belief 
		/// </summary>
		public double NaiveBelief
		{
			get
			{
				if(this.naiveBelief == -1.0)
					this.calculateNaiveBelief();
				return this.naiveBelief;
			}
		}

		/// <summary>
		/// Get the Matching Belief
		/// </summary>
		public double MatchingBelief
		{
			get
			{
				if(this.matchingBelief == -1.0)
					this.calculateMatchingBelief();
				return this.matchingBelief;
			}
		}
		
		public ArrayList Substrokes
		{
			get
			{
				return this.substrokes;
			}
		}

		
		public string Label
		{
			get
			{
				return this.label;
			}
		}

		
		#endregion

		#region OTHER

		public ConnectedComponent Clone()
		{
			return new ConnectedComponent((ArrayList)this.substrokes.Clone(), (ArrayList)this.substrokesBelief.Clone(), (string)this.label.Clone());
		}

		#endregion

		#region OLD DISTANCE CODE

		/*
		public static double minimumDistance(ConnectedComponent a, ConnectedComponent b, out Substroke minA, out Substroke minB)
		{
			double min = double.MaxValue;
			double dist;
			for(int i = 0; i < a.substrokes.Count; ++i)
			{
				for(int j = 0; j < b.substrokes.Count; ++j)
				{
					dist = Component.minimumDistance((Substroke)a.substrokes[i], (Substroke)b.substrokes[i]);
					if(dist < min)
						min = dist;
				}
			}
			return min;
		}

		public static double maximumDistance(ConnectedComponent a, ConnectedComponent b)
		{
			double max = double.MinValue;
			double dist;
			for(int i = 0; i < a.substrokes.Count; ++i)
			{
				for(int j = 0; j < b.substrokes.Count; ++j)
				{
					dist = Component.minimumDistance((Substroke)a.substrokes[i], (Substroke)b.substrokes[i]);
					if(dist > max)
						max = dist;
				}
			}
			return max;
		}

		public static double[] minMaxDistance(ConnectedComponent a, ConnectedComponent b)
		{
			double min = double.MaxValue;
			double max = double.MinValue;
			double dist;
			for(int i = 0; i < a.substrokes.Count; ++i)
			{
				for(int j = 0; j < b.substrokes.Count; ++j)
				{
					dist = Component.minimumDistance((Substroke)a.substrokes[i], (Substroke)b.substrokes[i]);
					if(dist > max)
						max = dist;
					if(dist < min)
						min = dist;
				}
			}
			return new double[] { min, max };
		}
		*/

		#endregion
	}
}
