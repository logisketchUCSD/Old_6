using System;
using System.IO;
using System.Collections;

using NeuralNets;

namespace Adeline
{
	/// <summary>
	/// Summary description for Program.
	/// </summary>
	class Program
	{
		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		[STAThread]
		static void Main(string[] args)
		{
			ArrayList argArray = new ArrayList(args);
			int numArgs = argArray.Count;
			
			// Output usage if nothing was passed as an argument
			if (numArgs == 0)
			{
				Console.WriteLine("*** Usage: Adeline.exe [-n <double>] [-e <double>] [-l <int>] [-f <int>]");
				Console.WriteLine("***                    [-t <int>] -i (<input file>.in | <input stream>)");
				Console.WriteLine("***");
				Console.WriteLine("***  -n <double>: Float for the learning rate. (Default = 0.01)");
				Console.WriteLine("***  -e <double>: Float indicating the MSE goal. (Default = 0.0001)");
				Console.WriteLine("***  -l <int>: Max number of training epochs. (Default = 500)");
				Console.WriteLine("***  -t <int>: Trace output. A positive int corresponds to detailed output. (Default = 0)");
				Console.WriteLine("***  -f <int>: Transfer function type, 0 ordinary, 1 logistic. (Default = 0)");
				Console.WriteLine("***  -i <input file>.in: Valid Adeline *.in file");
				Console.WriteLine("***  -i <input stream>: Input stream of values in the proper Assign2 specification.");

				return;
			}
			
			// Default learning rate
			double n = 0.01;

			// Default MSE goal
			double e = 0.0001;

			// Default training epoch limit
			int l = 500;

			// Default function type
			int f = 0;

			// Default trace value
			int t = 0;

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

			// Collect the input, if there is any
			if (argArray.Contains("-i"))
			{
				int index = argArray.IndexOf("-i");

				string filename = (string)argArray[index + 1];
				string file_input = "";

				// Set the defaults
				int output_dimension = 1;
				int input_dimension = 0;

				// If we are getting input from a file
				if (filename.ToLower().EndsWith(".in"))
				{
					TextReader tr = new StreamReader(filename);
					
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
					// Here we assume that a list of values was passed
					// Such as "-i 1 2 0 1 1 0 1 ..."
					file_input = filename;
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
					
				// Get the output and input dimensions
				output_dimension = Convert.ToInt32(input[0]);
				input_dimension  = Convert.ToInt32(input[1]);

				// Initialize the input and desired sets
				ArrayList x_array = new ArrayList();
				ArrayList d_array = new ArrayList();

				// Fill the corresponding x and d ArrayLists
				for (int i = 2; i < input.Length; i += (input_dimension + 1))
				{
					d_array.Add(Convert.ToDouble(input[i]));

					ArrayList curr_x = new ArrayList();
					curr_x.Add(-1.0);

					for (int k = i + 1; k <= i + input_dimension; k++)
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

				//x_array.Reverse();
				//d_array.Reverse();

				// Repeat out any input flags
				if (f == 0)
					Console.WriteLine("Function is linear");
				else
					Console.WriteLine("Function is logistic");

				Console.WriteLine("Learning rate is " + n.ToString("#0.0000"));
				Console.WriteLine("MSE goal is " + e.ToString("0.0000"));

				// Initial display of the samples, desireds, and weights
				Console.WriteLine("Input dimension (not counting bias): " + input_dimension);
				Console.WriteLine("Samples (with desired output first) are:");
				for (int i = 0; i < x_array.Count; i++)
				{
					Console.Write("Desired: " + d_array[i] + ", Inputs: ");
					foreach (double val in (ArrayList)x_array[i])
						Console.Write(val + " ");
					Console.Write("\n");
				}
				
				Console.Write("Initial weights: ");
				ArrayList weights = new ArrayList();
				for (int i = 0; i < ((ArrayList)x_array[0]).Count; i++)
				{
					weights.Add(0.0);
					Console.Write("0.00 ");
				}
				Console.Write("\n\n");

				// Generate the Adeline and train
				NeuralNets.Adeline adeline = new NeuralNets.Adeline(x_array, d_array, weights);
				adeline.Train(n, e, l, t);
			}
			else
			{
				Console.WriteLine("Error, no input was given!");
				return;
			}


			Console.WriteLine("Press ENTER to exit...");
			Console.ReadLine();
		}
	}
}
