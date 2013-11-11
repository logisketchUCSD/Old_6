// Aaron Wolin
// CS 152

using System;
using System.Collections;

namespace NeuralNets
{
	/// <summary>
	/// Summary description for Adeline.
	/// </summary>
	public class Adeline
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

		/// <summary>
		/// The type of function to use with the Adeline
		/// </summary>
		private FunctionType fnType;

		#endregion

		#region CONSTRUCTOR

		/// <summary>
		/// Constructor without a specified weights ArrayList. Calls the main constructor 
		/// with a null weights parameter.
		/// </summary>
		/// <param name="x_array">An ArrayList of x input sets, where each set is also 
		///		an ArrayList</param>
		/// <param name="d_array">An Arraylist of desired values for each x input set. 
		///		The index of d should correspond to the input set in x under the same index.</param>
		public Adeline(ArrayList x_array, ArrayList d_array)
			: this(x_array, d_array, null, FunctionType.Linear)
		{
			// Calls the main constructor
		}


		/// <summary>
		/// Initializes a Adeline object containing an array of x inputs, an array of 
		/// the desired value for each set of x inputs, and (possibly) a set of weights.
		/// 
		/// The x inputs are assumed to NOT have the -1 value for x_0 already attached.
		/// 
		/// If the weights are not included (weights == null) then we set the weights to 0.
		/// </summary>
		/// <param name="x_array">An ArrayList of x input sets, where each set is also an ArrayList</param>
		/// <param name="d_array">An Arraylist of desired values for each x input set. 
		///		The index of d should correspond to the input set in x under the same index.</param>
		/// <param name="weights">Weights for the perceptron. If nothing, set all weights to 0.</param>
		public Adeline(ArrayList x_array, ArrayList d_array, ArrayList weights, FunctionType fn)
		{
			this.x_array = x_array;
			this.d_array = d_array;
			this.fnType  = fn;
		
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
		/// Trains the Adeline by repeating epochs on the input set until no errors 
		///	are found.
		/// </summary>
		/// <param name="training_rate">The training rate indicating how the 
		///		training will converge</param>
		/// <param name="mse_goal">The MSE goal</param>
		/// <param name="epoch_threshold">The maximum number of epochs we 
		///		should run before we stop</param>
		/// <param name="trace">An integer, where postive indicates we want to 
		///		output detailed trace information</param>
		public void Train(double training_rate, double mse_goal, int epoch_threshold, int trace)
		{
			// Generate a new set of ArrayLists for the input data
			ArrayList x_training = (ArrayList)x_array.Clone();
			
			// Set to 1 so the while-loop will initially run
			int num_errors = 1;		
			
			// The MSE of an epoch
			double mse = 1.0;

			// Start counting the number of epochs at 0
			int num_epochs = 0;

			// Number of actual inputs
			int num_inputs = x_array.Count;
	
			// The weights we will have after training
			ArrayList trained_weights = (ArrayList)this.weights.Clone();
			
			// Repeat until we have converged (no errors) or we decide that we have run enough epochs
			while (num_errors > 0 && mse > mse_goal && num_epochs < epoch_threshold)
			{
				num_errors = Epoch(x_training, training_rate, mse_goal, trace, ref trained_weights, out mse);
				int percent = (num_errors * 100)/num_inputs;

				Console.Write("MSE in epoch " + (++num_epochs) + ": ");
				Console.Write(mse.ToString("#0.000000"));
				Console.Write(", " + num_errors + "/" + num_inputs + " wrong (" + percent + "%)\n");
				Console.WriteLine();
			}

			// Set our current weights to the ones we found after training
			// Do this only if we converged
			this.weights = trained_weights;
				
			Console.Write("Final weights: ");
			foreach (double w in this.weights)
				Console.Write(w.ToString("#0.000000") + " ");

			Console.Write("\n\n");
				
			if (num_errors == 0 && num_epochs <= epoch_threshold)
			{
				Console.WriteLine("Weights converged in " + num_epochs + " epochs.");
			}
		}


		/// <summary>
		/// Performs one training run on all of the vectors in an input set.
		/// </summary>
		/// <param name="x_training">The input set to train with</param>
		/// <param name="training_rate">The training rate indicating how the training 
		///		will converge</param>
		/// <param name="mse_goal">MSE goal</param>
		/// <param name="trace">An integer, where postive indicates we want to output 
		///		detailed trace information</param>
		/// <param name="w_training">The initial weights to train for this epoch</param>
		/// <param name="mse">The MSE to send out</param>
		/// <returns>An int specifying the number of errors in this epoch</returns>
		private int Epoch(ArrayList x_training, double training_rate, double mse_goal, int trace,
			ref ArrayList w_training, out double mse)
		{
			// Value to store the Mean Squared Error
			mse = 0.0;

			// Test the weights we have with each input set, 
			// and recalculate the weights if there were any errors
			for (int i = 0; i < x_training.Count; i++)
			{
				double desired = (double)d_array[i];
				double output = TestWeights((ArrayList)x_training[i], w_training);

				double error = desired - output;
				double errorSq = error * error;

				// Add the squared error to the sum
				mse += errorSq;

				double learning = Convert.ToDouble(error) * training_rate;
					
				// Recalculate the weights
				for (int k = 0; k < w_training.Count; k++)
				{
					ArrayList curr_x = (ArrayList)x_training[i];
					
					double new_weight = (double)w_training[k] + (learning * Convert.ToDouble(curr_x[k]));

					// Set the next set of weights
					w_training[k] = new_weight;
				}
			
				// Output more detailed trace information if the trace is positive
				if (trace > 0)
				{
					Console.Write("Step " + (i+1) + " weights: ");
					foreach (double w in w_training)
						Console.Write(w.ToString("#0.000000") + " ");
					Console.Write("\n");
				}
			}

			// The resulting Mean Squared Error (stored by sending it out)
			mse /= x_training.Count;
			
			// Keep track of how many input sets are incorrect for our final epoch weights
			int num_errors = 0;
			
			// Run the classifier to see how many errors we have for the epoch
			for (int i = 0; i < x_training.Count; i++)
			{
				double classifier;

				if (TestWeights((ArrayList)x_training[i], w_training) > 0.5) 
					classifier = 1;
				else
					classifier = 0;				

				if ((double)d_array[i] - classifier != 0.0)
					num_errors++;
			}

			// Return the number of errors, or incorrect input sets, we had
			return num_errors;
		}


		/// <summary>
		/// Tests the given weights against an ArrayList (vector) x.
		/// Returns an output, (w^T)x.
		/// </summary>
		/// <param name="x">Input vector</param>
		/// <param name="weights">Given weights</param>
		/// <returns>An output value for the product of the weight and x vectors</returns>
		private double TestWeights(ArrayList x, ArrayList weights)
		{
			double result = 0.0;
			
			for (int i = 0; i < x.Count; i++)
			{
				// Take the current (x_i)(w_i) product
				double currProduct = (Convert.ToDouble(x[i]) * Convert.ToDouble(weights[i]));

				if (fnType == FunctionType.Linear)
				{
					result += currProduct;
				}
				else if (fnType == FunctionType.Logsig)
				{
					result += logsig(currProduct);
				}

			}
			
			return result;
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
		/// The derivative of whatever function we're using in the Adeline.
		/// </summary>
		/// <param name="u">The value to pass to the deriv, f'</param>
		/// <returns>f'(u)</returns>
		private double deriv(double u)
		{
			if (fnType == FunctionType.Linear)
				return 1.0;

			else if (fnType == FunctionType.Logsig)
				return ((1.0 - u) * u);

			return 1.0;
		}

		#endregion
	}
}
