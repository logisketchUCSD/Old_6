using System;
using System.Collections;
using System.IO;
using MathNet.Numerics.LinearAlgebra;
using System.Collections.Generic;

namespace NeuralNets
{
	public enum TrainingMode
	{
		Online = 0, Batch = 1
	}

	/// <summary>
	/// Summary description for BackProp.
	/// </summary>
	public class BackProp
	{
		#region INTERNALS

        /// <summary>
        /// Prints debug statements iff debug == true.
        /// </summary>
        private bool debug = false;

		/// <summary>
		/// Our network of layers, each containing an ArrayList of neurons
		/// </summary>
		private ArrayList layers;

		/// <summary>
		/// The inputs
		/// </summary>
		private ArrayList x_array;

		/// <summary>
		/// The desired outputs
		/// </summary>
		private ArrayList d_array;

		/// <summary>
		/// The extension type of a BackProp file
		/// </summary>
		private const string EXT = ".bp";

		#endregion

		#region CONSTRUCTOR
		
		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="filepath">Initialize a BackProp network from a file</param>
		public BackProp(string filepath)
		{
			// Initialize all of the internals to nothing
			this.layers  = new ArrayList();
			this.x_array = new ArrayList();
			this.d_array = new ArrayList();
		
			// Load in the actual network from the file given
			Load(filepath);
		}
		
		
		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="layers">Network of layers</param>
		/// <param name="x_array">Input to network</param>
		/// <param name="d_array">Desired outputs</param>
		public BackProp(ArrayList layers, ArrayList x_array, ArrayList d_array)
		{
			this.layers = layers;
			this.x_array = x_array;
			this.d_array = d_array;
		}

		#endregion

		#region METHODS
		
		/// <summary>
		/// Run the network (with the current weights) on a single input array.
		/// </summary>
		/// <param name="x">The input array</param>
		/// <returns>The output array</returns>
		public double[] Run(ArrayList x)
		{
			// Local variable storing the outputs from each layer
			ArrayList outputs = new ArrayList();

			// Run x through all of the layers
			for (int i = 0; i < layers.Count; i++)
			{
				ArrayList currLayer = (ArrayList)(layers[i]);
				ArrayList layerOutputs = new ArrayList();	
	
				// Run x through each layer
				for (int k = 0; k < currLayer.Count; k++)
				{
					ArrayList currInputs = new ArrayList();

					// Clone x for the first layer, so we can keep the old x around incase
					if (i == 0)
						currInputs = (ArrayList)x.Clone();
					else
						currInputs = (ArrayList)outputs[i-1];
						
					// Run x through each neuron
					layerOutputs.Add( ((Adeline2)currLayer[k]).Run(currInputs, false) );
				}

				// Add the outputs from each layer to a variable (to keep track of everything)
				outputs.Add(layerOutputs);
			}
				
			// Get the last layer's output
			double[] bpOutput = (double[])((ArrayList)(outputs[outputs.Count - 1])).ToArray(typeof(double));

			return bpOutput;
		}

		#endregion

		#region TRAINING
		
		/// <summary>
		/// Train the multi-layer neural net
		/// </summary>
		/// <param name="learning_rate">The learning rate</param>
		/// <param name="epoch_threshold">The maximum number of epochs to run</param>
		/// <param name="mode">Training mode, on-line or batch</param>
		public void Train(double learning_rate, int epoch_threshold, double mse_goal, 
			TrainingMode mode, int trace)
		{
			int num_inputs = x_array.Count;
			int num_errors = 1;
			int num_epochs = 0;
			double mse = Double.PositiveInfinity;
			
			// Train the network until we have no errors, or the number of epochs has been reached
			while (num_epochs < epoch_threshold && mse > mse_goal)
			{
				num_errors = Epoch(learning_rate, mode, out mse, trace);
				
				int percent = (num_errors * 100) / num_inputs;

				if (debug && ( (trace == 0 && (num_epochs < 1000 || num_epochs % 100 == 0 || mse < mse_goal)) 
					|| (trace > 0) ) )
				{
					Console.Write("MSE in epoch " + num_epochs + ": ");
					Console.Write(mse.ToString("#0.000000"));
					Console.Write(", " + num_errors + "/" + num_inputs + " wrong (" + percent + "%)");
					Console.WriteLine();

					if (trace > 0)
						Console.WriteLine();
				}
				
				if (trace > 1)
				{
					for (int i = 0; i < layers.Count; i++)
					{
						if (debug) Console.WriteLine("Layer " + (i+1) + ":");
						ArrayList currLayer = (ArrayList)layers[i];

						for (int n = 0; n < currLayer.Count; n++)
						{
							if (debug) Console.Write("Neuron " + (n+1) + " weights: ");

							Adeline2 currNeuron = (Adeline2)currLayer[n];
							foreach (double w in currNeuron.Weights())
							{
								if (debug) Console.Write(w + " ");
							}

							if (debug) Console.WriteLine();
						}		
					
						if (debug) Console.WriteLine();
					}
				}

				++num_epochs;
			}

			// Output that the weights converged if they did
			if (debug && (num_errors == 0 || mse < mse_goal) && num_epochs <= epoch_threshold)
			{
				Console.WriteLine("Weights converged in " + (num_epochs - 1) + " epochs.");
				Console.WriteLine();
			}
			else if (debug)
			{
				Console.WriteLine("Weights did not converge after " + num_epochs + " epochs.");
				Console.WriteLine();
			}

			// Output the final weights for the neurons
			for (int i = 0; i < layers.Count; i++)
			{
				if (debug) Console.WriteLine("Layer " + (i+1) + ":");
				ArrayList currLayer = (ArrayList)layers[i];

				for (int n = 0; n < currLayer.Count; n++)
				{
					if (debug) Console.Write("Neuron " + (n+1) + " weights: ");

					Adeline2 currNeuron = (Adeline2)currLayer[n];
					foreach (double w in currNeuron.Weights())
					{
						if (debug) Console.Write(w + " ");
					}

					if (debug) Console.WriteLine();
				}		
					
				if (debug) Console.WriteLine();
			}
		}


		/// <summary>
		/// Run through the entire set of inputs once
		/// </summary>
		/// <param name="learning_rate">Learning rate for the epoch</param>
		/// <param name="mode">Training mode, on-line or batch</param>
		/// <param name="mse">The Mean-Squared Error of the epoch</param>
		/// <returns>The number of errors in the epoch</returns>
		private int Epoch(double learning_rate, TrainingMode mode, out double mse, int trace)
		{
			int inputNum = 0;
			mse = 0.0;

			// Keep a temp variable of batch outputs, but only use this if we're 
			// doing batch training
			ArrayList batchDeltas = null;

			// Go through all of our inputs
			foreach (ArrayList x in x_array)
			{
				ArrayList outputs = new ArrayList();

				for (int i = 0; i < layers.Count; i++)
				{
					ArrayList currLayer = (ArrayList)(layers[i]);
					ArrayList layerOutputs = new ArrayList();

					for (int k = 0; k < currLayer.Count; k++)
					{
						ArrayList currInputs = new ArrayList();

						if (i == 0)
							currInputs = (ArrayList)x.Clone();
						else
							currInputs = (ArrayList)outputs[i-1];
						
						layerOutputs.Add( ((Adeline2)currLayer[k]).Run(currInputs, true) );
					}	
					
					outputs.Add(layerOutputs);
				}

				// Calculate the error involved
				ArrayList finalOut = (ArrayList)outputs[outputs.Count - 1];
				ArrayList e = new ArrayList();
				ArrayList curr_d = (ArrayList)d_array[inputNum];
				
				for (int i = 0; i < curr_d.Count; i++)
				{
					e.Add((double)curr_d[i] - (double)finalOut[i]);
				}

				// Calculate the sensitivies
				ArrayList sensitivities = CalcSensitivities(outputs, e);
				
				// Calculate the weight changes
				ArrayList deltaWs = CalcDeltas(learning_rate, x, outputs, sensitivities);
				
				// Update the weights immediately if we are doing online training
				if (mode == TrainingMode.Online)
				{
					UpdateWeights(deltaWs);
				}

				// Otherwise if we're doing batch training
				else if (mode == TrainingMode.Batch)
				{
					if (batchDeltas == null)
						batchDeltas = (ArrayList)deltaWs.Clone();

					else
					{
						// Go through the layers
						for (int i = 0; i < layers.Count; i++)
						{
							ArrayList curr_weights = (ArrayList)batchDeltas[i];
							ArrayList curr_deltas  = (ArrayList)deltaWs[i];

							// Go through the neurons
							int numNeurons = curr_weights.Count;
							for (int n = 0; n < numNeurons; n++)
							{
								ArrayList neuronWs = (ArrayList)curr_weights[n];
								ArrayList neuronDWs = (ArrayList)curr_deltas[n];

								// Update the neuron's weights
								for (int w = 0; w < neuronWs.Count; w++)
									neuronWs[w] = (double)neuronWs[w] + (double)neuronDWs[w];
							}
						}
					}
				}

				inputNum++;
			}

			// Finally update the weights if we've done batch training
			if (mode == TrainingMode.Batch)
			{
				// Go through the layers
				for (int i = 0; i < layers.Count; i++)
				{
					ArrayList curr_weights = (ArrayList)batchDeltas[i];
					
					// Go through the neurons
					int numNeurons = curr_weights.Count;
					for (int n = 0; n < numNeurons; n++)
					{
						ArrayList neuronWs = (ArrayList)curr_weights[n];
					
						// Update the neuron's weights
						for (int w = 0; w < neuronWs.Count; w++)
							neuronWs[w] = ((double)neuronWs[w] / (double)x_array.Count);
					}
				}

				UpdateWeights(batchDeltas);
			}

			// The number of errors for the epoch
			int num_errors = 0;
			inputNum = 0;

			// Trace info
			if (trace > 0 && debug)
			{
				Console.WriteLine("*********************************************");
				Console.Write("Step MSEs: ");
			}
				
			// Run the classifier to see how many errors we have for the epoch
			foreach (ArrayList x in x_array)
			{
				// Run the current BackProp network on the input given
				// Returns the final output of the network
				double[] finalOutput = Run(x);

				// Classifier (for discrete data sets). Not using right now.
				/*int classifier;

				if (finalOutput[0] > 0.5)
					classifier = 1;
				else
					classifier = 0;*/			

				// Calculate the MSE
				double stepMSE = 0.0;
				
				ArrayList e = new ArrayList();
				ArrayList curr_d = (ArrayList)d_array[inputNum];
				for (int i = 0; i < curr_d.Count; i++)
				{
					double err = (double)curr_d[i] - finalOutput[i];
					stepMSE += err * err;
				}

				//if (Convert.ToDouble(curr_d[0]) - classifier != 0.0)
				//	num_errors++;
				
				if ( Math.Abs((double)curr_d[0] - finalOutput[0]) > 0.005)
					num_errors++;
				
				mse += (stepMSE / curr_d.Count);

				inputNum++;
			
				// Output each step's MSE
				if (trace > 0 && debug)
				{
					Console.Write(stepMSE.ToString("#0.000") + " ");
				}
			}

			// Final MSE to return via out
			mse /= x_array.Count;			

			if (trace > 0 && debug)
				Console.WriteLine();

			return num_errors;
		}


		/// <summary>
		/// Calculates the sensitivities for each layer
		/// </summary>
		/// <param name="outputs">The outputs of all of the layers</param>
		/// <param name="e">The desired - actual outputs of the actual layer</param>
		/// <returns>An ArrayList of sensitivities</returns>
		private ArrayList CalcSensitivities(ArrayList outputs, ArrayList e)
		{
			ArrayList sensitivities = new ArrayList();

			// Update the last sensitivity first
			ArrayList lastOutput = (ArrayList)outputs[outputs.Count - 1];
			ArrayList currSensitivities = new ArrayList();
				
			// Calculate s^L
			ArrayList finalLayer = (ArrayList)layers[layers.Count - 1];
			ArrayList deriv = Deriv(((Adeline2)finalLayer[0]).GetFunctionType(), lastOutput);
			for (int i = 0; i < lastOutput.Count; i++)
			{
				currSensitivities.Add(-2 * (double)e[i] * (double)deriv[i]);
			}
			sensitivities.Add(currSensitivities.Clone());

			// Calculate all s^(l-1)
			for (int i = outputs.Count - 2; i >= 0; i--)
			{
				currSensitivities.Clear();
				ArrayList currLayer = (ArrayList)layers[i];
				ArrayList currOutput = (ArrayList)outputs[i];

				ArrayList followingLayer = (ArrayList)layers[i+1];
				Adeline2 neuron = (Adeline2)followingLayer[0];
				
				// Create a matrix of the weights in w^l, MINUS THE BIASES
				Matrix lp1Weights = new Matrix(followingLayer.Count, neuron.Weights().Count - 1);
				for (int r = 0; r < lp1Weights.RowCount; r++)
				{
					neuron = (Adeline2)followingLayer[r];
					ArrayList weights = neuron.Weights();

					for (int c = 0; c < lp1Weights.ColumnCount; c++)
						lp1Weights[r,c] = (double)weights[c];
				}

				// Create a matrix for the sensitivities in s^l
				ArrayList tmpSens = (ArrayList)(sensitivities[0]);
				Matrix lp1Sensitivities = new Matrix((double[])tmpSens.ToArray(typeof(double)), tmpSens.Count);
					
				// Do a (w^l)^T * s^l calculation
				lp1Weights.Transpose();
				Matrix prod = lp1Weights * lp1Sensitivities;
				
				// Calculate each row of the sensitivities vector by multiplying the previous
				// matrix product with the derivative calculation from the neuron
				Adeline2 currNeuron = (Adeline2)currLayer[0];
				deriv = Deriv(currNeuron.GetFunctionType(), currOutput);
				for (int k = 0; k < currOutput.Count; k++)
				{
					currSensitivities.Add( (double)deriv[k] * prod[k,0] );
				}
				
				sensitivities.Insert(0, currSensitivities.Clone());
			}

			return sensitivities;
		}


		/// <summary>
		/// The derivative of whatever function we're using in the Adeline.
		/// </summary>
		/// <param name="u">The value to pass to the deriv, f'</param>
		/// <returns>f'(u)</returns>
		private ArrayList Deriv(FunctionType fnType, ArrayList u)
		{
			ArrayList result = new ArrayList();

			// Derivative of Linear
			if (fnType == FunctionType.Linear)
			{
				for (int i = 0; i < u.Count; i++)
					result.Add(1.0);
			}

				// Derivative of Logsig
			else if (fnType == FunctionType.Logsig)
			{
				for (int i = 0; i < u.Count; i++)
					result.Add( ((1.0 - (double)u[i]) * (double)u[i]) );
			}

				// Derivative of Tansig
			else if (fnType == FunctionType.Tansig)
			{
				for (int i = 0; i < u.Count; i++)
					result.Add( 1 - Math.Pow((double)u[i], 2.0) );
			}

				// Derivative of Hardlim
			else if (fnType == FunctionType.Hardlim)
			{
				for (int i = 0; i < u.Count; i++)
					result.Add( ((1.0 - (double)u[i]) * (double)u[i]) );
			}

			return result;
		}


		/// <summary>
		/// Calculate all the deltas and store them in an ArrayList that mimics the network.
		/// This function is necessary for batch training...
		/// </summary>
		/// <param name="learning_rate">The learning rate</param>
		/// <param name="x">The input to the network</param>
		/// <param name="outputs">The outputs of each network</param>
		/// <param name="sensitivities">The sensitivities for each layer</param>
		/// <returns>All the delta w's</returns>
		private ArrayList CalcDeltas(double learning_rate, ArrayList x, ArrayList outputs, ArrayList sensitivities)
		{
			ArrayList deltaWs = new ArrayList();

			// Go through the layers
			for (int i = 0; i < layers.Count; i++)
			{
				deltaWs.Add(new ArrayList());
				
				ArrayList currLayer = (ArrayList)layers[i];
				ArrayList prevOutputs;
				if (i == 0)
					prevOutputs = (ArrayList)x.Clone();
				else
					prevOutputs = (ArrayList)outputs[i-1];
					
				// Go through the neurons in each layer
				int numNeurons = currLayer.Count;
				for (int n = 0; n < numNeurons; n++)
				{
					// Go through the weights in each neuron
					ArrayList delta_weights = new ArrayList();
					ArrayList curr_weights = ((Adeline2)currLayer[n]).Weights();

					// Tack on any 1.0's we might have added directly into the neuron
					while (prevOutputs.Count < curr_weights.Count)
						prevOutputs.Add(1.0);

					for (int k = 0; k < prevOutputs.Count; k++)
					{
						double s = (double)((ArrayList)sensitivities[i])[n];
						delta_weights.Add(-1 * learning_rate * s * (double)prevOutputs[k]);
					}

					((ArrayList)deltaWs[i]).Add(delta_weights);
				}
			}

			return deltaWs;
		}
		
		
		/// <summary>
		/// Updates the weights of the network
		/// </summary>
		/// <param name="learning_rate">The learning rate</param>
		/// <param name="x">The input to the network</param>
		/// <param name="outputs">The outputs of each network</param>
		/// <param name="sensitivities">The sensitivities for each layer</param>
		private void UpdateWeights(double learning_rate, ArrayList x, ArrayList outputs, ArrayList sensitivities)
		{
			// Go through the layers
			for (int i = 0; i < layers.Count; i++)
			{
				ArrayList currLayer = (ArrayList)layers[i];
                ArrayList prevOutputs;
				if (i == 0)
					prevOutputs = (ArrayList)x.Clone();
				else
					prevOutputs = (ArrayList)outputs[i-1];
					
				// Go through the neurons in each layer
				int numNeurons = currLayer.Count;
				for (int n = 0; n < numNeurons; n++)
				{
					// Go through the weights in each neuron
					ArrayList delta_weights = new ArrayList();
					ArrayList curr_weights = ((Adeline2)currLayer[n]).Weights();

					// Tack on any 1.0's we might have added directly into the neuron
					while (prevOutputs.Count < curr_weights.Count)
						prevOutputs.Add(1.0);

					for (int k = 0; k < prevOutputs.Count; k++)
					{
						double s = (double)((ArrayList)sensitivities[i])[n];
						delta_weights.Add(-1 * learning_rate * s * (double)prevOutputs[k]);
					}

					// Update the neuron's weights
					((Adeline2)currLayer[n]).UpdateWeights(delta_weights);
				}
			}
		}


		/// <summary>
		/// Updates the weights of the network
		/// </summary>
		/// <param name="deltaWs">The deltas we need to update</param>
		private void UpdateWeights(ArrayList deltaWs)
		{
			// Go through the layers
			for (int i = 0; i < layers.Count; i++)
			{
				ArrayList curr_layer = (ArrayList)layers[i];
				ArrayList curr_weights = (ArrayList)deltaWs[i];
				
				// Go through the neurons
				int numNeurons = curr_layer.Count;
				for (int n = 0; n < numNeurons; n++)
				{
					ArrayList neuronWs = (ArrayList)curr_weights[n];

					// Update the neuron's weights
					((Adeline2)curr_layer[n]).UpdateWeights((ArrayList)neuronWs.Clone());
				}
			}
		}
	
		#endregion

		#region TESTING

        public void Test(ArrayList input, ArrayList desired)
        {
            int size = input.Count;
            double averageMSE = 0.0;
            double[] mseArray = new double[size];
            int numberoferrors = 0;
            int errorsfromrounding = 0;
            int errorsfromdesired = 0;
            int inputerrors = 0;
            int internalerrors = 0;
            int outputerrors = 0;

            for (int i = 0; i < size; i++)
            {
                double[] bpOutput = Run((ArrayList)input[i]);

                double mse = 0;

                ArrayList curr_d = (ArrayList)desired[i];
                for (int k = 0; k < curr_d.Count; k++)
                {
                    double err = (double)curr_d[k] - bpOutput[k];
                    mse += err * err;
                }

                mse /= curr_d.Count;

                if (debug) Console.WriteLine("Input " + i + ": " + bpOutput[0] + "," + bpOutput[1] + "," + bpOutput[2] + "/" + curr_d[0] + "," + curr_d[1] + "," + curr_d[2]
                    + ", MSE = " + mse.ToString("#0.000000"));

                int inpdist = (int)Math.Round(bpOutput[0]);
                int outdist = (int)Math.Round(bpOutput[1]);
                int intdist = (int)Math.Round(bpOutput[2]);
                if (debug) Console.WriteLine("inputdist: {0}, output dist: {1}, internal dist: {2}", inpdist, outdist, intdist);
                int output;
                if (inpdist == 1 && outdist != 1 && intdist != 1)
                    output = 1;
                else if (inpdist != 1 && outdist == 1 && intdist != 1)
                    output = 2;
                else if (inpdist != 1 && outdist != 1 && intdist == 1)
                    output = 3;
                else
                {
                    if (debug)
                    {
                        Console.WriteLine("******************************");
                        Console.WriteLine("Two or more outputs from the neurons rounded to the same value");
                        Console.WriteLine("******************************");
                    }
                    output = 4;
                    errorsfromrounding++;
                }

                int expoutput;
                double currd0 = (double)curr_d[0];
                double currd1 = (double)curr_d[1];
                double currd2 = (double)curr_d[2];
                if ((int)currd0 == 1 && (int)currd1 == 0 && (int)currd2 == 0)
                    expoutput = 1;
                else if ((int)currd0 == 0 && (int)currd1 == 1 && (int)currd2 == 0)
                    expoutput = 2;
                else if ((int)currd0 == 0 && (int)currd1 == 0 && (int)currd2 == 1)
                    expoutput = 3;
                else
                {
                    if (debug)
                    {
                        Console.WriteLine("******************************");
                        Console.WriteLine("Two or more desired inputs have same value");
                        Console.WriteLine("******************************");
                    }
                    expoutput = 5;
                    errorsfromdesired++;
                }


                if (debug) Console.WriteLine("Output: {0}, Expected Output: {1}", output, expoutput);

                if (output != expoutput)
                {
                    if (debug) Console.WriteLine("*********Error in this vector*********\n");
                    numberoferrors++;
                    if (expoutput == 1)
                        inputerrors++;
                    else if (expoutput == 2)
                        outputerrors++;
                    else if (expoutput == 3)
                        internalerrors++;
                }

                averageMSE += mse;
                mseArray[i] = mse;
            }

            averageMSE /= size;
            Array.Sort(mseArray);

            if (debug)
            {
                Console.WriteLine();
                Console.WriteLine("Average MSE = " + averageMSE.ToString("#0.000000"));
                Console.WriteLine("Median MSE = " + mseArray[size / 2].ToString("#0.000000"));
                Console.WriteLine("Min MSE = " + mseArray[0].ToString("#0.000000"));
                Console.WriteLine("Max MSE = " + mseArray[size - 1].ToString("#0.000000"));
                Console.WriteLine("Total number of errors: " + numberoferrors);
                Console.WriteLine("Number of errors from rounding: " + errorsfromrounding);
                Console.WriteLine("Number of errors from desired inputs: " + errorsfromdesired);
                Console.WriteLine("Input errors: {0}, Internal errors: {1}, Output errors: {2}", inputerrors, internalerrors, outputerrors);
                Console.ReadLine();
            }
        }

		#endregion

		#region SAVE/LOAD

		/// <summary>
		/// Saves the back propagation network in the file format:
		/// 
		/// # Layers
		/// # Neurons FnType # Weights weights...
		/// # Neurons FnType # Weights weights...
		/// 
		/// Uses an extension defined as an internal const
		/// </summary>
		/// <param name="filename"></param>
        public void Save(string filename)
        {
            FileInfo t = new FileInfo(filename + EXT);
            StreamWriter Tex = t.CreateText();

            // Number of layers
            Tex.WriteLine(layers.Count + " ");
            Tex.WriteLine();

            for (int i = 0; i < layers.Count; i++)
            {
                ArrayList currLayer = (ArrayList)layers[i];

                // Number of neurons in the layer
                Tex.Write(currLayer.Count + " ");

                for (int n = 0; n < currLayer.Count; n++)
                {
                    Adeline2 currNeuron = (Adeline2)currLayer[n];

                    if (n == 0)
                    {
                        // Function type of the neurons
                        Tex.Write(((Adeline2)currLayer[0]).GetFunctionType());
                        Tex.Write(" ");

                        // Number of weights
                        Tex.Write(currNeuron.Weights().Count);
                        Tex.Write(" ");
                    }

                    foreach (double w in currNeuron.Weights())
                    {
                        Tex.Write(w + " ");
                    }

                    Tex.WriteLine();
                }

                Tex.WriteLine();
            }

            Tex.Close();

            if (debug)
            {
                Console.WriteLine();
                Console.WriteLine("Outputted backprop network to the file: " + filename + EXT);
            }
        }


		/// <summary>
		/// Loads a backpropagation network from a file. The file format is specified in
		/// the header comment of the Save method.
		/// </summary>
		/// <param name="filename"></param>
		public void Load(string filepath)
		{
			string file_input = "";
			
			if (filepath.ToLower().EndsWith(EXT))
			{
				TextReader tr = new StreamReader(filepath);
					
				string curr_line;
				while ((curr_line = tr.ReadLine()) != null)
				{
					file_input += curr_line + " ";
				}

				// Remove any ending whitespace added onto the string
				while (file_input[file_input.Length - 1] == ' ')
					file_input = file_input.Substring(0, file_input.Length - 1);

				tr.Close();

				// Remove whitespace from the string
				char[] sepr = new char[2] {' ', '\t'};
				string[] input = file_input.Split(sepr);

				// More removing of whitespace
				ArrayList tmp_input = new ArrayList();
				foreach (string s in input)
				{
					if (s != "")
						tmp_input.Add(s);
				}

				input = (string[])tmp_input.ToArray(typeof(string));

				// BP initialization
				this.layers.Clear();

				int llength = Convert.ToInt32(input[0]);
				int index = 0;

				// Cycle through the layers
				for (int i = 0; i < llength; i++)
				{
					// Number of neurons
					int nlength = Convert.ToInt32(input[index + 1]);
					
					// Type of neuron
					string neuronType = input[index + 2];
					FunctionType fnType = FunctionType.Linear;
					if (neuronType.Equals("Linear"))
						fnType = FunctionType.Linear;
					else if (neuronType.Equals("Logsig"))
						fnType = FunctionType.Logsig;
					else if (neuronType.Equals("Tansig"))
						fnType = FunctionType.Tansig;
					else if (neuronType.Equals("Hardlim"))
						fnType = FunctionType.Hardlim;
				
					// Weight length
					int wlength = Convert.ToInt32(input[index + 3]);
					
					ArrayList currLayer = new ArrayList();
					
					// Initialize each neuron in the layer
					for (int n = 0; n < nlength; n++)
					{
						ArrayList weights = new ArrayList();
						
						for (int w = 0; w < wlength; w++)
							weights.Add(Convert.ToDouble(input[index + 4 + (n * wlength) + w]));

						currLayer.Add(new Adeline2(weights, fnType));
					}

					this.layers.Add(currLayer);
					
					index += (nlength * wlength) + 3;
				}

			}
			else if (debug)
			{
				Console.WriteLine("\nError! Incorrect file extension\n");
			}
		}

		#endregion
	}	
}
