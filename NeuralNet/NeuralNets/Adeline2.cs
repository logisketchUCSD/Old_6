// Aaron Wolin
// CS 152

using System;
using System.Collections;

namespace NeuralNets
{
	// Type of Adeline function to use
	public enum FunctionType
	{
		Linear = 0, Logsig = 1, Tansig = 2, Hardlim = 3
	}

	/// <summary>
	/// Summary description for Adeline.
	/// </summary>
	public class Adeline2
	{
		#region INTERNALS

		/// <summary>
		/// The weights for the perceptron.
		/// </summary>
		private ArrayList weights;

		/// <summary>
		/// The type of function to use with the Adeline
		/// </summary>
		private FunctionType fnType;

		/// <summary>
		/// Current input of the epoch
		/// </summary>
		private ArrayList currInput;

		#endregion

		#region CONSTRUCTOR

		/// <summary>
		/// Initializes a Adeline object
		/// </summary>
		public Adeline2(ArrayList weights, FunctionType fn)
		{
			this.fnType  = fn;
			this.weights = weights;
		}

		#endregion

		#region METHODS

		/// <summary>
		/// Tests the given weights against an ArrayList (vector) x.
		/// Returns an output, (w^T)x.
		/// </summary>
		/// <param name="x_array">Input vector</param>
		/// <param name="training">Are we running to train?</param>
		/// <returns>An output value for the product of the weight and x vectors</returns>
		public double Run(ArrayList x_array, bool training)
		{
			ArrayList x = (ArrayList)x_array.Clone();
			
			double result = 0.0;

			while (x.Count < weights.Count)
			{
				x.Add(1.0);
			}

			currInput = x;
		
			// Take the current wx product for the current neuron (row of weights)
			double product = 0.0;
			for (int i = 0; i < x.Count; i++)
				product += (Convert.ToDouble(x[i]) * Convert.ToDouble(this.weights[i]));
		
			// Output our result	
			switch (fnType)
			{
				case FunctionType.Linear:
					result = product;
					break;
				case FunctionType.Logsig:
					result = logsig(product);
					break;
				case FunctionType.Tansig:
					result = tansig(product);
					break;
				case FunctionType.Hardlim:
					if (training)
						result = logsig(product);
					else
						result = hardlim(product);
					break;
				default:
					break;
			}
			
			return result;
		}


		/// <summary>
		/// Updates all of the weights by the delta amounts we pass to the adeline
		/// </summary>
		/// <param name="delta_weights">The weight increments to update</param>
		public void UpdateWeights(ArrayList delta_weights)
		{
			for (int i = 0; i < delta_weights.Count; i++)
				weights[i] = (double)weights[i] + (double)delta_weights[i];
		}
		

		/// <summary>
		/// Log sig function, returning 1 / 1 + e^(-u).
		/// </summary>
		/// <param name="u">Value to pass to the log sig function</param>
		/// <returns>The logsig(u)</returns>
		private double logsig(double u)
		{
			return (1.0 / (1.0 + Math.Pow(Math.E, -u)));
		}


		/// <summary>
		/// Tan sig function, returning 2/(1+exp(-2*n)) - 1
		/// </summary>
		/// <param name="u">Value to pass to the tan sig function</param>
		/// <returns>The tansig(u)</returns>
		private double tansig(double u)
		{
			return (2.0 / (1.0 + Math.Exp(-2.0 * u))) - 1.0;
		}


		/// <summary>
		/// Hardlim function, returns 1 if greater than 0, otherwise 0.
		/// </summary>
		/// <param name="u">Value to pass to the hardlim</param>
		/// <returns>The hardlim(u)</returns>
		private double hardlim(double u)
		{
			if (u >= 0)
				return 1;
			else
				return 0;
		}

		#endregion

		#region GETTERS

		/// <summary>
		/// Returns the weights associated with the Adeline
		/// </summary>
		/// <returns>Weights</returns>
		public ArrayList Weights()
		{
			return this.weights;
		}

		/// <summary>
		/// Returns the function type of the adeline
		/// </summary>
		/// <returns>The function type of the adeline</returns>
		public FunctionType GetFunctionType()
		{
			return this.fnType;
		}


		/// <summary>
		/// Returns what was input into the adeline
		/// </summary>
		/// <returns>The previous input into the adeline</returns>
		public ArrayList Input()
		{
			return this.currInput;
		}

		#endregion
	}
}
