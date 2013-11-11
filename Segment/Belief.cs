using System;

namespace Segment
{
	/// <summary>
	/// Belief can calculate the belief that something is a specific object
	/// such as an 'and', or a 'wire'.
	/// </summary>
	public class Belief
	{
		public Belief()
		{
		}

		/* TODO! Need to create the following 3 functions!!!! Important for segmentation.
		 * 
		 * 
		 * 
		 * 
		 */ 

		/// <summary>
		/// Computes the percentage of what it believes is the best label and 
		/// </summary>
		/// <param name="cc"></param>
		/// <param name="label"></param>
		/// <returns></returns>
		internal static double beliefPercentage(ConnectedComponent cc, out string label)
		{
			return Belief.nThBeliefPercentage(cc, 0, out label);
		}

		/// <summary>
		/// Computes the percentage of what it believes is the nth best label (0 is best)
		/// </summary>
		/// <param name="cc"></param>
		/// <param name="nth"></param>
		/// <param name="label"></param>
		/// <returns></returns>
		internal static double nThBeliefPercentage(ConnectedComponent cc, uint nth, out string label)
		{
			label = (string)cc.Label.Clone();
			return 0.95;
		}

		/// <summary>
		/// Computes the percentage that cc is a label
		/// </summary>
		/// <param name="cc"></param>
		/// <param name="label"></param>
		/// <returns></returns>
		internal static double labelBeliefPercentage(ConnectedComponent cc, string label)
		{
			return 0.95;
		}

		internal static double naiveBelief(ConnectedComponent cc)
		{
			return cc.NaiveBelief;
		}

		internal static double belief(ConnectedComponent cc)
		{
			//return Belief.belief(c, 0.1, 0.9);
			return Belief.belief(cc, 1.0, 0.0);
		}
		
		internal static double belief(ConnectedComponent cc, double naiveWeight, double matchingWeight)
		{
			return naiveWeight * cc.NaiveBelief + matchingWeight * cc.MatchingBelief;
		}

		internal static double[] belief(Component c)
		{
			//return Belief.belief(c, 0.1, 0.9);
			return Belief.belief(c, 1.0, 0.0);
		}
		
		internal static double[] belief(Component c, double naiveWeight, double matchingWeight)
		{
			double[] beliefs = new double[c.ConnectedComponents.Count];

			ConnectedComponent cc;
			for(int i = 0; i < c.ConnectedComponents.Count; ++i)
			{
				cc = (ConnectedComponent)c.ConnectedComponents[i];
				beliefs[i] = Belief.belief(cc);
			}

			return beliefs;
		}

	}
}
