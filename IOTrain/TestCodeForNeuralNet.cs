/*		#region TESTING

		public void Test(ArrayList input, ArrayList desired)
		{
			int size = input.Count;
			double averageMSE = 0.0;
			double[] mseArray = new double[size];
			int numberoferrors = 0;
			int errorsfromrounding = 0;
			int errorsfromdesired = 0;

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

				Console.WriteLine("Input " + i + ": " + bpOutput[0] + "," + bpOutput[1] + "," + bpOutput[2] + "/" + curr_d[0] + "," + curr_d[1] + "," + curr_d[2] 
					+ ", MSE = " + mse.ToString("#0.000000"));
				
				int inpdist = (int)Math.Round(bpOutput[0]);
				int outdist = (int)Math.Round(bpOutput[1]);
				int intdist = (int)Math.Round(bpOutput[2]);
				Console.WriteLine("inputdist: {0}, output dist: {1}, internal dist: {2}",inpdist,outdist,intdist);
				int output;
				if (inpdist == 1 && outdist != 1 && intdist != 1)
					output = 1;
				else if (inpdist != 1 && outdist == 1 && intdist != 1)
					output = 2;
				else if (inpdist != 1 && outdist != 1 && intdist == 1)
					output = 3;
				else
				{
					Console.WriteLine("******************************");
					Console.WriteLine("Two or more outputs from the neurons rounded to the same value");
					Console.WriteLine("******************************");
					output = 4;
					errorsfromrounding ++;
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
					Console.WriteLine("******************************");
					Console.WriteLine("Two or more desired inputs have same value");
					Console.WriteLine("******************************");
					expoutput = 5;
					errorsfromdesired ++;
				}
					

				Console.WriteLine("Output: {0}, Expected Output: {1}",output,expoutput);

				if (output != expoutput)
				{
					Console.WriteLine("*********Error in this vector*********\n");
					numberoferrors ++;
				}

				averageMSE += mse;
				mseArray[i] = mse;
			}

			averageMSE /= size;
			Array.Sort(mseArray);

			Console.WriteLine();
			Console.WriteLine("Average MSE = " + averageMSE.ToString("#0.000000"));
			Console.WriteLine("Median MSE = " + mseArray[size/2].ToString("#0.000000"));
			Console.WriteLine("Min MSE = " + mseArray[0].ToString("#0.000000"));
			Console.WriteLine("Max MSE = " + mseArray[size - 1].ToString("#0.000000"));
			Console.WriteLine("Total number of errors: " + numberoferrors);
			Console.WriteLine("Number of errors from rounding: " + errorsfromrounding);
			Console.WriteLine("Number of errors from desired inputs: " + errorsfromdesired);
			Console.ReadLine();
		}

		#endregion
*/