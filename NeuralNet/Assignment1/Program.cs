// Aaron Wolin
// CS 152

using System;
using System.IO;
using System.Collections;

using NeuralNets;

namespace Percep
{
	/// <summary>
	/// Summary description for Assignment 1.
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
				Console.WriteLine("*** Usage: Percep.exe [-n <double>] [-l <int>] [-t <int>] -f (<input file>.in | <input stream>)");
				Console.WriteLine("***  -n <double>: Flag and floating point value for the learning rate. Defaults to 1.");
				Console.WriteLine("***  -l <int>: Flag and int specifying the max number of training epochs. Defaults to 100.");
				Console.WriteLine("***  -t <int>: Flag and int for trace output. A positive int corresponds to detailed output. Defaults to 0");
				Console.WriteLine("***  -f <input file>.in: Any file with the valid perceptron format specified, with the file extension .in");
				Console.WriteLine("***  -f <input stream>: Input stream of values consistent with the Assignment 1 guidelines.");

				return;
			}
			
			// Default learning rate
			double n = 1;

			// Default training epoch limit
			int l = 100;

			// Default trace value
			int t = 0;


			// Check for a learning rate argument
			if (argArray.Contains("-n"))
			{
				int index = argArray.IndexOf("-n");
				
				// Should probably throw an exception if a double isn't here
				n = Convert.ToDouble(argArray[index + 1]);
			}

			// Check for an epoch limit argument
			if (argArray.Contains("-l"))
			{
				int index = argArray.IndexOf("-l");
				
				// Should probably throw an exception if an int isn't here
				l = Convert.ToInt32(argArray[index + 1]);
			}

			// Check for a trace argument
			if (argArray.Contains("-t"))
			{
				int index = argArray.IndexOf("-t");

				// Should probably throw an exception if an int isn't here
				t = Convert.ToInt32(argArray[index + 1]);
			}

			// Make sure that the input char '<' is present, and then collect the input
			if (argArray.Contains("-f"))
			{
				int index = argArray.IndexOf("-f");

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
					// Here we assume that a list of values was passed after '<', such as "< 1 2 0 1 1 0 1 ..."
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
					d_array.Add(Convert.ToInt32(input[i]));

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


				// Generate the perceptron and train
				Perceptron percep = new Perceptron(x_array, d_array, weights);
				percep.Train(n, l, t);
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
