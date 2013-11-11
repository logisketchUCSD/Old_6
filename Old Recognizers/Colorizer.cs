/*
 * File: Colorizer.cs
 *
 * Author: James Brown
 * Harvey Mudd College, Claremont, CA 91711.
 * Sketchers 2008.
 * 
 * Use at your own risk.  This code is not maintained and not guaranteed to work.
 * We take no responsibility for any harm this code may cause.
 */

using System.Drawing;

namespace OldRecognizers
{
	/// <summary>
	/// This static utility class provides a handy way to associate labels with colors
	/// </summary>
	public static class Colorizer
	{
		/// <summary>
		/// Converts a label to a color
		/// </summary>
		/// <param name="label">The label to get the associated color for</param>
		/// <returns>A System.Drawing.Color object representing the colorization of the provided label. Black for unknown</returns>
		public static Color LabelToColor(string label)
		{
			switch (label.ToLower())
			{
				case "and":
					return Color.Red;
				case "nand":
					return Color.Blue;
				case "nor":
					return Color.Orange;
				case "not":
					return Color.Green;
				case "or":
					return Color.Pink;
				case "backline":
					return Color.Red;
				case "backarc":
					return Color.Blue;
				case "frontarc":
					return Color.Orange;
				case "bubble":
					return Color.Green;
				case "wire":
					return Color.Blue;
                case "mesh":
                    return Color.DarkBlue;
				case "gate":
					return Color.Red;
				case "label":
					return Color.Orange;
				case "nonwire":
					return Color.Purple;
				case "nongate":
					return Color.Green;
				case "nonlabel":
					return Color.Indigo;
				case "unknown":
				case "unlabeled":
				default:
					return Color.Black;
			}
		}
	}
}
