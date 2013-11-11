/**
 * File: Gate.cs
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
	/// Summary description for Class1.
	/// </summary>
	public class Gate
	{
		#region INTERNALS

		/// <summary>
		/// internal name of the gate
		/// </summary>
		private string Name;

		/// <summary>
		/// Leftmost coordinate
		/// </summary>
		private int Xcoord;

		/// <summary>
		/// Topmost coordinate
		/// </summary>
		private int Ycoord;

		/// <summary>
		/// Height of gate
		/// </summary>
		private int Height;

		/// <summary>
		/// Width of gate
		/// </summary>
		private int Width;
		
		/// <summary>
		/// ID for gate; used to make sure we do not have duplicate gates
		/// </summary>
		private string Id;

		/// <summary>
		/// Input Wires to the gate
		/// </summary>
		public ArrayList Inputs;

		/// <summary>
		/// Output Wires from the gate
		/// </summary>
		public ArrayList Outputs;

		/// <summary>
		/// Type of Gate (i.e AND, NOR, etc.)
		/// </summary>
		private string Type;

		public Sketch.Substroke[] substrokes;

		#endregion INTERNALS

		#region CONSTRUCTORS

		public Gate(Sketch.Shape gate)
		{
			this.Xcoord = Convert.ToInt32(gate.XmlAttrs.X);
			this.Ycoord = Convert.ToInt32(gate.XmlAttrs.Y);
			this.Width = Convert.ToInt32(gate.XmlAttrs.Width);
			this.Height = Convert.ToInt32(gate.XmlAttrs.Height);
			this.Id = Convert.ToString(gate.XmlAttrs.Id);
			this.Type = Convert.ToString(gate.XmlAttrs.Type);
			this.substrokes = gate.Substrokes;
		}

		#endregion CONSTRUCTORS

		#region METHODS



		#endregion METHODS


		#region GETTERS AND SETTERS

		public string Gatename
		{
			get
			{
				return this.Name;
			}

			set
			{
				this.Name = value;
			}
		}
		
		public int X
		{
			get
			{
				return this.Xcoord;
			}

			set
			{
				this.Xcoord = value;
			}
		}

		public int Y
		{
			get
			{
				return this.Ycoord;
			}

			set
			{
				this.Ycoord = value;
			}
		}

		public int High
		{
			get
			{
				return this.Height;
			}

			set
			{
				this.Height = value;
			}
		}

		public int Wide
		{
			get
			{
				return this.Width;
			}

			set
			{
				this.Width = value;
			}
		}

		public string ID
		{
			get
			{
				return this.Id;
			}

			set
			{
				this.Id = value;
			}
		}

		public string Gatetype
		{
			get
			{
				return this.Type;
			}

			set
			{
				this.Type = value;
			}
		}

		#endregion GETTERS AND SETTERS
	}
}
