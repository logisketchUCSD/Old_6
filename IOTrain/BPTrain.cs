using System;
using System.IO;
using System.Collections;

using NeuralNets;

namespace IOTrain
{
	/// <summary>b
	/// Summary description for Program.
	/// </summary>
	class BackProp
	{
		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		[STAThread]
		public static void BPTrain(string[] args)
		{
			ArrayList argArray = new ArrayList(args);
			int numArgs = argArray.Count;
			
			// Output usage if nothing was passed as an argument
			if (numArgs == 0)
			{
				Console.WriteLine("*** Usage: BackProp.exe [-n <double>] [-e <double>] [-l <int>] [-f <int>]");
				Console.WriteLine("***                    [-t <int>] -i (<input file>.in | <input stream>)");
				Console.WriteLine("***");
				Console.WriteLine("***  -n <double>: Float for the learning rate. (Default = 0.01)");
				Console.WriteLine("***  -e <double>: Float indicating the MSE goal. (Default = 0.0001)");
				Console.WriteLine("***  -l <int>: Max number of training epochs. (Default = 500)");
				Console.WriteLine("***  -m <int>: Training mode (Default = 0 on-line, 1 batch)");
				Console.WriteLine("***  -t <int>: Trace output. A positive int corresponds to detailed output. (Default = 0)");
				Console.WriteLine("***  -nn <input file>.nn: Valid neural network file.");
				Console.WriteLine("***  -i <input file>.in: Valid *.in file");
				Console.WriteLine("***  -i <input stream>: Input stream of values in the proper Assign4 specification.");

				return;
			}
			
			// Default learning rate
			double n = 0.01;

			// Default MSE goal
			double e = 0.000001;

			// Default training epoch limit
			int l = 50000;

			// Default function type
			int f = 0;

			// Default training mode
			int m = 0;
			TrainingMode trainingMode = TrainingMode.Online;

			// Default trace value
			int t = 0;

			// Neural Network to use
			ArrayList neuralNet = new ArrayList();

			// Check for a learning rate argument
			if (argArray.Contains("-n"))
			{
				int index = argArray.IndexOf("-n");
				
				// Should probably throw an exception if a double isn't here
				n = Convert.ToDouble(argArray[index + 1]);
			}

			// Check for the MSE goal
			if (argArray.Contains("-e"))
			{
				int index = argArray.IndexOf("-e");

				// Should probably throw an exception if a double isn't here
				e = Convert.ToDouble(argArray[index + 1]);
			}

			// Check for an epoch limit argument
			if (argArray.Contains("-l"))
			{
				int index = argArray.IndexOf("-l");
				
				// Should probably throw an exception if an int isn't here
				l = Convert.ToInt32(argArray[index + 1]);
			}

			// Check for an epoch limit argument
			if (argArray.Contains("-m"))
			{
				int index = argArray.IndexOf("-m");
				
				// Should probably throw an exception if an int isn't here
				m = Convert.ToInt32(argArray[index + 1]);

				if (m == 0)
					trainingMode = TrainingMode.Online;
				else if (m == 1)
					trainingMode = TrainingMode.Batch;
			}

			// Check for a function type
			if (argArray.Contains("-f"))
			{
				int index = argArray.IndexOf("-f");

				// Should probably throw an exception if an int isn't here
				f = Convert.ToInt32(argArray[index + 1]);
			}

			// Check for a trace argument
			if (argArray.Contains("-t"))
			{
				int index = argArray.IndexOf("-t");

				// Should probably throw an exception if an int isn't here
				t = Convert.ToInt32(argArray[index + 1]);
			}

			// Check for a neural network file or stream

			if (argArray.Contains("-nn"))
			{
				int index = argArray.IndexOf("-nn");

				string filepath = (string)argArray[index + 1];
				
				// Parse the file or input stream
				string[] input = parseFile(filepath, ".nn");
				Console.WriteLine("{0}",input);
				neuralNet = new ArrayList(input);
			}

			// Collect the input, if there is any
			if (argArray.Contains("-i"))
			{
				int index = argArray.IndexOf("-i");

				string filepath = (string)argArray[index + 1];
				string file_input = "";
				string[] input;
				
				// Set the defaults
				int output_dimension = 1;
				int input_dimension = 0;

				// Parse the file or input stream
				if (filepath.ToLower().EndsWith(".in"))
				{
					input = parseFile(filepath, ".in");
				}
				else
				{
					file_input = filepath;

					// Remove whitespace from the string
					input = file_input.Split(' ');

					// More removing of whitespace
					ArrayList tmp_input = new ArrayList();
					foreach (string s in input)
					{
						if (s != "")
							tmp_input.Add(s);
					}

					input = (string[])tmp_input.ToArray(typeof(string));
				}

				
					
				// Get the output and input dimensions
				output_dimension = Convert.ToInt32(input[0]);
				input_dimension  = Convert.ToInt32(input[1]);

				// Initialize the input and desired sets
				ArrayList x_array = new ArrayList();
				ArrayList d_array = new ArrayList();

				// Fill the corresponding x and d ArrayLists
				for (int i = 2; i < input.Length; i += (input_dimension + output_dimension))
				{
					ArrayList curr_d = new ArrayList();

					for (int j= i; j<i + output_dimension; j++)
					{
						curr_d.Add(Convert.ToDouble(input[j]));
					}

					d_array.Add(curr_d);

					ArrayList curr_x = new ArrayList();
					
					for (int k = i + output_dimension; k < i + input_dimension + output_dimension; k++)
					{
						
						if (k < input.Length)
							curr_x.Add(Convert.ToDouble(input[k]));
						else
						{
							Console.WriteLine("Last sample did not have enough values, aborting.");
							return;
						}
					}
				
					x_array.Add(curr_x);
				}

				Console.WriteLine("count of darray: " + d_array.Count);
				Console.WriteLine("count of xarray: " + x_array.Count);

				// Test debugging...
				if (argArray.Contains("-test"))
				{
					int ind = argArray.IndexOf("-test");
					string path = (string)argArray[ind + 1];
				
					NeuralNets.BackProp backpropnet = new NeuralNets.BackProp(path);	
				
					Console.WriteLine("Testing neural network on input samples");
					backpropnet.Test(x_array, d_array);
					return;
				}

				

				// Doing this just to make testing with Keller's code easier
				//x_array.Reverse();
				//d_array.Reverse();

				// Repeat out any input flags
				if (f == 0)
					Console.WriteLine("Function is linear");
				else
					Console.WriteLine("Function is logistic");

				Console.WriteLine("Learning rate is " + n.ToString("#0.0000"));
				Console.WriteLine("MSE goal is " + e.ToString("0.0000"));

				// Initial display of the samples and desireds
				Console.WriteLine("Input dimension (not counting bias): " + input_dimension);
				Console.WriteLine("Output dimension: " + output_dimension);
				Console.WriteLine("Samples (with desired output first) are:");
				for (int i = 0; i < x_array.Count; i++)
				{
					ArrayList darr = (ArrayList)d_array[i];
					if (darr.Count == 1)
						Console.Write("Desired: " + (double)((ArrayList)d_array[i])[0] + ", Inputs: ");
					else
						Console.Write("Desired: " + (double)((ArrayList)d_array[i])[0] + ", " + (double)((ArrayList)d_array[i])[1] + ", " + (double)((ArrayList)d_array[i])[2] + ", Inputs: ");

					foreach (double val in (ArrayList)x_array[i])
						Console.Write(val + " ");
					Console.Write("\n");
				}

				// Initialize our neural net
				ArrayList network = generateNetwork(neuralNet, x_array);

				// Set up our back-propagation
				NeuralNets.BackProp bpNetwork = new NeuralNets.BackProp(network, x_array, d_array);	
				
				// Train
				bpNetwork.Train(n, l, e, trainingMode, t);

				// Save network weights
				bpNetwork.Save("ioweights");
			}
			else
			{
				Console.WriteLine("Error, no input was given!");
				return;
			}
	
			Console.WriteLine("Press ENTER to exit...");
			Console.ReadLine();
		}


		/// <summary>
		/// Parses a file at the given filepath.
		/// </summary>
		/// <param name="filepath">Filepath of the file</param>
		/// <param name="extension">Extension of the file</param>
		/// <returns>The parsed, string array of data</returns>
		static string[] parseFile(string filepath, string extension)
		{
			string file_input = "";
			
			// If we are getting input from a file
			if (filepath.ToLower().EndsWith(extension))
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
			}
			else
			{
				Console.WriteLine("Extension does not match!");
				return null;
			}
			
			// Remove whitespace from the string
			string[] input = file_input.Split(' ');

			// More removing of whitespace
			ArrayList tmp_input = new ArrayList();
			foreach (string s in input)
			{
				if (s != "")
					tmp_input.Add(s);
			}

			input = (string[])tmp_input.ToArray(typeof(string));

			return input;
		}


		/// <summary>
		/// Generate our neural network
		/// </summary>
		/// <param name="neuralNet">Framework to build our neural net from</param>
		/// <param name="x_array">Initial inputs into the network</param>
		static ArrayList generateNetwork(ArrayList neuralNet, ArrayList x_array)
		{
			ArrayList layers = new ArrayList();
			
			// Get the weight vector size for each neuron in the layer
			int wLength = ((ArrayList)x_array[0]).Count + 1;

			for (int i = 0; i < neuralNet.Count; i += 2)
			{
				ArrayList currLayer = new ArrayList();

				// Get the function type for the neurons in the current layer
				string neuron = (neuralNet[i] as String);
				FunctionType fnType = FunctionType.Linear;

				if (neuron.Equals("linear"))
					fnType = FunctionType.Linear;
				else if (neuron.Equals("logsig"))
					fnType = FunctionType.Logsig;
				else if (neuron.Equals("tansig"))
					fnType = FunctionType.Tansig;
				else if (neuron.Equals("hardlim"))
					fnType = FunctionType.Hardlim;
							
				// Add each neuron into our layer
				int numNeurons = Convert.ToInt32(neuralNet[i+1]);
				for (int k = 0; k < numNeurons; k++)
				{
					currLayer.Add(new Adeline2(new ArrayList(generateWeights(wLength)), fnType));
				}

				// Set the weight vector length for the next layer
				wLength = numNeurons + 1;
				
				// Add the current layer into the network
				layers.Add(currLayer);
			}

			return layers;
		}



		/// <summary>
		/// Generates a weights vector
		/// </summary>
		/// <param name="length">Length of the weight vector</param>
		/// <returns>The weight vector</returns>
		static double[] generateWeights(int length)
		{
			double[] weights = new double[length];
			Random RandomClass = new Random();

			for (int i = 0; i < weights.Length; i++)
				weights[i] = (RandomClass.NextDouble() * 0.1) * Math.Pow(-1.0, (RandomClass.Next() % 2));

			return weights;
		}
	}
}
