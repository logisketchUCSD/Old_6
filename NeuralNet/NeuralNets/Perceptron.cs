// Aaron Wolin
// CS 152

using System;
using System.Collections;

namespace NeuralNets
{
	/// <summary>
	/// Summary description for Perceptron.
	/// </summary>
	public class Perceptron
	{
		#region INTERNALS

		/// <summary>
		/// The set of vectors given for the perceptron.
		/// </summary>
		private ArrayList x_array;

		/// <summary>
		/// The desired values for each vector in the input set.
		/// d_array[i] corresponds to the desired value of x_array[i].
		/// </summary>
		private ArrayList d_array;

		/// <summary>
		/// The weights for the perceptron.
		/// </summary>
		private ArrayList weights;

		#endregion

		#region CONSTRUCTOR

		/// <summary>
		/// Constructor without a specified weights ArrayList. Calls the main constructor with a null weights
		/// parameter.
		/// </summary>
		/// <param name="x_array">An ArrayList of x input sets, where each set is also an ArrayList</param>
		/// <param name="d_array">An Arraylist of desired values for each x input set. The index of d should correspond
		/// to the input set in x under the same index.</param>
		public Perceptron(ArrayList x_array, ArrayList d_array)
			: this(x_array, d_array, null)
		{
			// Calls the main constructor
		}


		/// <summary>
		/// Initializes a perceptron object containing an array of x inputs, an array of the desired value for each
		/// set of x inputs, and (possibly) a set of weights.
		/// 
		/// The x inputs are assumed to NOT have the -1 value for x_0 already attached.
		/// 
		/// If the weights are not included (weights == null) then we set the weights to 0.
		/// </summary>
		/// <param name="x_array">An ArrayList of x input sets, where each set is also an ArrayList</param>
		/// <param name="d_array">An Arraylist of desired values for each x input set. The index of d should correspond
		/// to the input set in x under the same index.</param>
		/// <param name="weights">Weights for the perceptron. If nothing, set all weights to 0.</param>
		public Perceptron(ArrayList x_array, ArrayList d_array, ArrayList weights)
		{
			this.x_array = x_array;
			this.d_array = d_array;
		
			// Initialize M+1 weights, where M is the number of elements per input set
			if (weights == null)
			{
				this.weights = new ArrayList();

				for (int i = 0; i < ((ArrayList)x_array[0]).Count; i++)
					this.weights.Add(0.0);
			}
			else
			{
				this.weights = weights;
			}
		}

		#endregion

		#region ADDERS

		/// <summary>
		/// Adds an input sample, with the corresponding x input and the desired d value, into the perceptron.
		/// 
		/// NOTE: Assumes that both of them will be added to the same (last) index of both ArrayLists.
		/// </summary>
		/// <param name="x">Input values</param>
		/// <param name="d">Desired value</param>
		public void AddInput(ArrayList x, int d)
		{
			x_array.Add(x);
			d_array.Add(d);
		}

		#endregion

		#region METHODS

		/// <summary>
		/// Trains the perceptron by repeating epochs on the input set until no errors are found.
		/// </summary>
		/// <param name="training_rate">The training rate indicating how the training will converge</param>
		/// <param name="epoch_threshold">The maximum number of epochs we should run before we stop</param>
		/// <param name="trace">An integer, where postive indicates we want to output detailed trace information</param>
		public void Train(double training_rate, int epoch_threshold, int trace)
		{
			// Generate a new set of ArrayLists for the input data
			ArrayList x_training = (ArrayList)x_array.Clone();
			
			// Set to 1 so the while-loop will initially run
			int num_errors = 1;		
			
			// Start counting the number of epochs at 0
			int num_epochs = 0;
	
			// The weights we will have after training
			ArrayList trained_weights = (ArrayList)this.weights.Clone();
			
			// Repeat until we have converged (no errors) or we decide that we have run enough epochs
			while (num_errors > 0 && num_epochs < epoch_threshold)
			{
				num_errors = Epoch(x_training, training_rate, ref trained_weights, trace);
				
				Console.WriteLine("Errors in epoch " + (++num_epochs) + ": " + num_errors);
				Console.WriteLine();
			} 

			// Set our current weights to the ones we found after training
			// Do this only if we converged
			if (num_errors == 0 && num_epochs <= epoch_threshold)
			{
				this.weights = trained_weights;
				
				Console.Write("Final weights: ");
				foreach (double w in this.weights)
					Console.Write(w.ToString("#0.00") + " ");

				Console.Write("\n\n");
				
				Console.WriteLine("Weights converged in " + num_epochs + " epochs.");
			}
		}


		/// <summary>
		/// Performs one training run on all of the vectors in an input set.
		/// </summary>
		/// <param name="x_training">The input set to train with</param>
		/// <param name="training_rate">The training rate indicating how the training will converge</param>
		/// <param name="w_training">The initial weights to train for this epoch</param>
		/// <param name="trace">An integer, where postive indicates we want to output detailed trace information</param>
		/// <returns>An int specifying the number of errors in this epoch</returns>
		private int Epoch(ArrayList x_training, double training_rate, ref ArrayList w_training, int trace)
		{
			// Keep track of how many input sets are incorrect in our current run
			int num_errors = 0;

			// Test the weights we have with each input set, and recalculate the weights if there were any errors
			for (int i = 0; i < x_training.Count; i++)
			{
				int desired = (int)d_array[i];
				int actual = Signum((ArrayList)x_training[i], w_training);

				int error = desired - actual;

				// If we found an error with our weights
				if (error != 0)
				{
					num_errors++;

					// Recalculate the weights
					for (int k = 0; k < w_training.Count; k++)
					{
						ArrayList curr_x = (ArrayList)x_training[i];
						double new_weight = (double)w_training[k] + 
							(Convert.ToDouble(error) * training_rate * Convert.ToDouble(curr_x[k]));

						// Set the next set of weights
						w_training[k] = new_weight;
					}
				}

				// Output more detailed trace information if the trace is positive
				if (trace > 0)
				{
					Console.Write("Step " + (i+1) + " weights: ");
					foreach (double w in w_training)
						Console.Write(w.ToString("#0.00") + " ");
					Console.Write("\n");
				}
			}

			// Return the number of errors, or incorrect input sets, we had
			return num_errors;
		}


		/// <summary>
		/// Tests the given weights against an ArrayList (vector) x.
		/// Returns 1 if wx > 0, 0 otherwise
		/// </summary>
		/// <param name="x">Input vector</param>
		/// <param name="weights">Given weights</param>
		/// <returns>Either 1 or 0, depending on if wx > 0</returns>
		private int Signum(ArrayList x, ArrayList weights)
		{
			double actual_sum = 0;
			
			// Take the dot product of x and w
			for (int i = 0; i < x.Count; i++)
			{
				actual_sum += Convert.ToDouble(x[i]) * Convert.ToDouble(weights[i]);
			}

			if (actual_sum > 0)
				return 1;
			else
				return 0;
		}

		#endregion
	}
}
