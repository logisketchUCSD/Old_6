/**
 * File: Label.cs
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

namespace IOTrain
{
	/// <summary>
	/// Summary description for Label.
	/// </summary>
	public class Label
	{
		#region INTERNALS

		public ArrayList LPointsAL;

		public object[] LPoints;

		public Sketch.Substroke[] substrokes;
		
		#endregion INTERNALS

		#region CONSTRUCTOR

		public Label(Sketch.Shape label)
		{
			this.LPointsAL = PointAdd(label);
			this.LPoints = LPointsAL.ToArray();
			this.substrokes = label.Substrokes;


		}

		#endregion CONSTRUCTOR

		#region METHODS

		public static ArrayList PointAdd(Sketch.Shape label)
		{
			ArrayList labelpoints = new ArrayList();;
			Sketch.Substroke[] lsubstrokes = label.Substrokes;
			for (int count=0; count<lsubstrokes.Length; count++)
			{
				Sketch.Point[] temppoints = lsubstrokes[count].Points;
				for (int i=0; i<temppoints.Length; i++)
				{
					labelpoints.Add(temppoints[i]);
				}
			}

			return labelpoints;
		
		}

		#endregion METHODS
	}
}
