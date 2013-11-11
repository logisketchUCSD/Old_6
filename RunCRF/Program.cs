/**
 * File: Program.cs
 * 
 * Authors: Aaron Wolin, Devin Smith, Jason Fennell, and Max Pflueger (Sketchers 2006).
 *          Code expanded by Anton Bakalov (Sketchers 2007).
 *          Harvey Mudd College, Claremont, CA 91711.
 * 
 * Use at your own risk.  This code is not maintained and not guaranteed to work.
 * We take no responsibility for any harm this code may cause.
 */
using CRF;
using TrainCRF;
using Sketch;
using System;
using System.IO;
using ConverterXML;
using Fragmenter;
using Featurefy;
using LabelMapper;

using System.Windows.Forms;
using System.Collections;
using System.Collections.Generic;

// For saving TrainCRF objects to minimize my workload
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

// Command line code. See printHelp() and/or the wiki for complete documentation.
//
//-t -n 2 -l wire-gate-labels.txt -o wgTest.tcrf -d "C:\Documents and Settings\Da Vinci\My Documents\Visual Studio Projects\branches\redesign\RunCRF\execution directory - Max\WireGateTraining\fragmentedXml"
//-t -n 3 -l 3-label-CRF.txt -o 3-label-test.tcrf -d "C:\Documents and Settings\Da Vinci\My Documents\Visual Studio Projects\branches\redesign\RunCRF\execution directory - Max\3-Label Training"

//RunCRF.exe -p2 -t -ft -n 2 -np2 2 -l lnlDomainCRF.txt -lp2 wgDomainCRF.txt -o Sample.tcrf -d "C:\Documents and Settings\Student\My Documents\Trunk2\Code\Recognition\RunCRF\TrainingSwitchedUnFrag"
//RunCRF.exe -p2 -fl -c Sample.tcrf Sample.tcrf.p2 -np2 2 -l lnlDomainCRF.txt -lp2 wgDomainCRF.txt -o A1.xml INPUT1.xml
//RunCRF.exe -p2 -fl -c Sample.tcrf Sample.tcrf.p2 -np2 2 -l lnlDomainCRF.txt -lp2 wgDomainCRF.txt -d INPUT_MULTIPASS
//
//RunCRF.exe -t -ft -n 2 -l lnlDomainCRF.txt -o Sample.tcrf -d "C:\Documents and Settings\Student\My Documents\Trunk2\Code\Recognitnion\RunCRF\TrainingSwitchedUnFrag\New Folder"
//RunCRF.exe -fl -c Sample.tcrf -l lnlDomainCRF.txt -o A1.xml INPUT1.xml
//
// 

namespace RunCRF
{
	class Program
	{
        public enum Criterion { toGateNongate, toWireLabel, toLabelNonlabel, toWireGate, toWireNonwire, toGateLabel }

        private static bool mapDomain = false;
        private static string labelMapFile = "";

		/// <summary>
		/// the CRF program: uses the CRF to do sketch recognition
		/// </summary>
		/// <param name="args">program arguments, see printHelp or "RunCRF -help" to know what to do</param>
		public static void Main(string[] args)
		{
            // To see why we subscribe to the UnhandledException event see the comment about 
            // the function OnUnhandledException
            AppDomain.CurrentDomain.UnhandledException += 
                new UnhandledExceptionEventHandler(OnUnhandledException);

            DateTime startTime = DateTime.Now;

            Console.WriteLine("");
			Console.WriteLine("STARTING PROGRAM: {0}", startTime);
            
            #region INTERNALS
            bool trainCRF = false;
			bool newCRF = false;
			bool loadCRF = false;
			bool randCRF = false;
            bool fragTraining = false; // Fragmenting the trading data
            bool fragLabeling = false; // Fragmenting the data we are doing inference/labeling on
			int numLabels = 0;
			int numLabelsP2 = 0;
			string inputFile = "";    // This is for input of a saved CRF
            string inputFile2 = "";
			string outputFile = "";   // Output of progam is saved here
			string labelFile = "";	  // Label file to tell program how to translate from text to int labels and back
			string labelFileP2 = "";  // 2-pass label file

			bool multiPassCRF = false;

			CRF.CRF crf = null;
            CRF.CRF crf2 = null;
			CRF.CRF mpCRF = null;
			List<string> dataFiles = new List<string>();
            #endregion

            #region COMMAND LINE PARSING
			// Do we want to use serialized FSXML files?
			bool useSerialized = false;

			// start at 1 to skip the name of the executable
			for(int i = 0; i < args.Length; i++)
			{
				switch(args[i].ToLower())
				{
                    // For a desctiption of each flag, see printHelp();
					case "-h":
					case "-help":
					case "--help":
						printHelp();
                        return;
                    case "-p2":
                        multiPassCRF = true;
                        break;
                    case "-ft":
                        fragTraining = true;
                        break;
                    case "-fl":
                        fragLabeling = true;
                        break;
                    case "-c":
                        loadCRF = true;
						i++;
						#region error check
						if((loadCRF) && ((newCRF) || (randCRF)))
						{
							Console.WriteLine("ERROR: You cannot have more than one of -c, -r, or -n options on at the same time");
							return;
						}						
						if(i>=args.Length)
						{
							Console.WriteLine("ERROR: -c must be followed by the location of your CRF file.");
							return;
						}
						#endregion
						inputFile = args[i];
                        if (multiPassCRF)
                        {
                            i++;
                            inputFile2 = args[i];
                        }
                        break;
					case "-l":
						i++;
						#region error check
						if(i >= args.Length)
						{
							Console.WriteLine("ERROR: -l must be followed by the location of your your label file that tells you what ints correspond to what labels.");
							return;
						}
						#endregion
						labelFile = args[i];
						break;
					case "-lp2":
						i++;
						#region error check
						if(i >= args.Length)
						{
							Console.WriteLine("ERROR: -lp2 must be followed by the location of your your label file that tells you what ints correspond to what labels.");
							return;
						}
						#endregion
						labelFileP2 = args[i];
						break;
					case "-o":
						i++;
						#region error check
						if(i>=args.Length)
						{
							Console.WriteLine("ERROR: -o must be followed by the location of your output file.");
							return;
						}
						#endregion
						outputFile = args[i];
						break;
					case "-t":
						trainCRF = true;
						break;
					case "-n": // Make a new CRF
						newCRF = true;
						#region error check
						if((newCRF) && ((loadCRF) || (randCRF)))
						{
							Console.WriteLine("ERROR: You cannot have more than one of -c, -r, or -n options on at the same time");
							return;
						}
						i++;
						if(i >= args.Length)
						{
							Console.WriteLine("ERROR: -n must be followed by the number of labels that the CRF should use for classification.");
							return;
						}
						#endregion
						numLabels = Convert.ToInt32(args[i]);
						break;
					case "-np2":
						i++;
						#region error check
						if(i >= args.Length)
						{
							Console.WriteLine("ERROR: -np2 must be followed by the location of your your label file that tells you what ints correspond to what labels.");
							return;
						}
						#endregion
						numLabelsP2 = Convert.ToInt32(args[i]);
						break;
					case "-r": // Create a random CRF 
						randCRF = true;
                        #region error check
                        if ((randCRF) && ((loadCRF) || (newCRF)))
						{
							Console.WriteLine("ERROR: You cannot have more than one of -c, -r, or -n options on at the same time");
							return;
                        }
                        #endregion
                        i++;
                        #region error check
                        if (i >= args.Length)
						{
							Console.WriteLine("ERROR: -r must be followed by the number of labels the CRF can use.");
							return;
						}
						numLabels = Convert.ToInt32(args[i]);
                        #endregion
                        break;
					case "-d": // Load files from a directory
						i++;
						#region error check
						if(i >= args.Length)
						{
							Console.WriteLine("ERROR: -d must be followed by the location of your files.");
							return;
						}
						#endregion
						string dir_path = args[i];
						foreach (string file in Directory.GetFiles(dir_path, "*.xml")) 
						{
							dataFiles.Add(file);
						}
						break;
                    case "-map": // load a domain map
                        i++;
                        if (i >= args.Length)
                        {
                            Console.WriteLine("ERROR: -map must be followed by the path of your domain map file.");
                            return;
                        }
                        labelMapFile = args[i];
                        mapDomain = true;
                        break;
					case "-s": // Try to use serialized FSXML files whenever possible
						useSerialized = true;
						break;
					default:  // This is the last argument, which is the XML file we will be loading from.
						dataFiles.Add(args[i]);
						break;
				}
			}
			#endregion 

			#region ERROR CHECK
			if(!(newCRF) && !(loadCRF)  && !(randCRF))
			{
				Console.WriteLine("ERROR: You must either load a CRF, create a new one using -n, or create one using -r");
				return;
			}
			if(dataFiles.Count == 0  && !(randCRF))
			{
				Console.WriteLine("ERROR: You must give a data file for the CRF to work with");
				return;
			}
			#endregion

            #region INITIALIZING INTERNALS
            if (newCRF)
			{
                /*
                 * Stage number tells us the stage of recognition. For example, in the 
                 * Gate vs. Nongate and Wire vs. Label recognition, stage 1 corresponds to
                 * Gate vs. Nongate. This is necessary since we are using different combinations
                 * of functions for each stage of recognition.
                 */
                CRF.SiteFeatures.setStageNumber(1);
                CRF.InteractionFeatures.setStageNumber(1);

				crf = new CRF.CRF(numLabels, false);
			}

			if (loadCRF)
			{
                // See the comment at the beginning of the region INITIALIZING INTERNALS.
                CRF.SiteFeatures.setStageNumber(1);
                CRF.InteractionFeatures.setStageNumber(1);

				crf = new CRF.CRF(inputFile, false);

                if (multiPassCRF)
                {
                    // See the comment at the beginning of the region INITIALIZING INTERNALS.
                    CRF.SiteFeatures.setStageNumber(2);
                    CRF.InteractionFeatures.setStageNumber(2);

                    crf2 = new CRF.CRF(inputFile2, false);
                }
            }

			if (randCRF)
			{
                // See the comment at the beginning of the region INITIALIZING INTERNALS.
                CRF.SiteFeatures.setStageNumber(1);
                CRF.InteractionFeatures.setStageNumber(1);

				crf = new CRF.CRF(numLabels, false);

				crf.saveCRF(outputFile);
				return;
			}

			if (multiPassCRF)
			{
                // See the comment at the beginning of the region INITIALIZING INTERNALS.
                CRF.SiteFeatures.setStageNumber(2);
                CRF.InteractionFeatures.setStageNumber(2);

				mpCRF = new CRF.CRF(numLabelsP2, false);
			}

			// Load from the label to index file so that we know which labels correspond to which ints
			// The format of this file is pairs of lines:
			//     string - text form of the label.
			//     int - stands for this label
			// There will be numLabels pairs of these lines in the file.  An example file would be
			//     wire
			//     0
			//     gate
			//     1
			StreamReader labelReader = new StreamReader(labelFile);
			System.Collections.Hashtable stringToIntTable = new System.Collections.Hashtable(numLabels);
			System.Collections.Hashtable intToStringTable = new System.Collections.Hashtable(numLabels);
			for (int i = 0; i < crf.numLabels; i++)
			{
                string textLabel = labelReader.ReadLine();
				int intLabel = Convert.ToInt32(labelReader.ReadLine());

				stringToIntTable.Add(textLabel, intLabel);
				intToStringTable.Add(intLabel, textLabel);
			}
            string testEndOfFile = labelReader.ReadLine();
            if (testEndOfFile != null)
            {
                Console.WriteLine("Possible Error. The label to index file (domainIndex) contains more label pairs than the file that takes ??? to labels (labelMap)");
            }

			// Initialize the Hashtables for a multi-pass CRF
			System.Collections.Hashtable stringToIntTableP2 = new System.Collections.Hashtable(numLabels);
			System.Collections.Hashtable intToStringTableP2 = new System.Collections.Hashtable(numLabels);

            if (multiPassCRF)
			{
				StreamReader labelReaderP2 = new StreamReader(labelFileP2);
				for (int i = 0; i < numLabelsP2; i++)
				{
					string textLabel = labelReaderP2.ReadLine();
					int intLabel = Convert.ToInt32(labelReaderP2.ReadLine());

					stringToIntTableP2.Add(textLabel, intLabel);
					intToStringTableP2.Add(intLabel, textLabel);
				}
            }
            #endregion


            if (trainCRF)
            {
                #region TRAINING
                // Allow for list of input files to be passed in a .txt file.
				if (dataFiles.Count == 1)
				{
					if(((string)dataFiles[0]).EndsWith(".txt"))
					{
						// File that contains a list of files to train on.
						StreamReader sr = new StreamReader((string)dataFiles[0]);
						dataFiles.Clear();
						
						while(sr.Peek() >= 0)
						{
							string tmp = sr.ReadLine();
							if(tmp.Equals(""))
							{
								continue;
							}
							dataFiles.Add(tmp);
						}	
					}
				}

				// Get our stroke data from the XML input file and load it into our CRF.
				int[][] inputLabels = new int[dataFiles.Count][];
				int[][] inputLabelsP2 = new int[dataFiles.Count][];

				Sketch.Substroke[][] inputFrags = new Sketch.Substroke[dataFiles.Count][];
				Sketch.Substroke[][] inputFragsP2 = new Sketch.Substroke[dataFiles.Count][];

				FeatureSketch[] fs = new FeatureSketch[dataFiles.Count];

				for (int i = 0; i < dataFiles.Count; i++)
				{
                    string fileName = dataFiles[i];

					string fsFileName;
					if (fragTraining)
						fsFileName = fileName.Replace(Files.FUtil.Extension(Files.Filetype.XML), ".fragged" + Files.FUtil.Extension(Files.Filetype.FEATURESKETCH));
					else
						fsFileName = fileName.Replace(Files.FUtil.Extension(Files.Filetype.XML), Files.FUtil.Extension(Files.Filetype.FEATURESKETCH));
					if (useSerialized && File.Exists(fsFileName))
						fs[i] = initVars(1, fsFileName, fragTraining, stringToIntTable, 
								ref inputFrags, i, ref inputLabels);
					else
						fs[i] = initVars(1, fileName, fragTraining, stringToIntTable, 
							ref inputFrags, i, ref inputLabels);

                    if (multiPassCRF)
                    {
                        fs[i] = initVars(2, fileName, fragTraining, stringToIntTableP2, 
                            ref inputFragsP2, i, ref inputLabelsP2);
                    }

				}

                Console.WriteLine("");
                string trainInfo = "TRAINING CRF.";
                if (multiPassCRF) { trainInfo += " STAGE 1."; }
                Console.WriteLine(trainInfo);

                // See the comment at the beginning of the region INITIALIZING INTERNALS.
                CRF.SiteFeatures.setStageNumber(1);
                CRF.InteractionFeatures.setStageNumber(1);

				TrainCRF.TrainCRF tCRF = new TrainCRF.TrainCRF(inputFrags, fs, inputLabels, crf);
                crf.loadParameters(tCRF.train());				
				crf.saveCRF(outputFile);

                if (multiPassCRF)
                {
                    Console.WriteLine("");
                    Console.WriteLine("TRAINING CRF. STAGE 2.");

                    // See the comment at the beginning of the region INITIALIZING INTERNALS.
                    CRF.SiteFeatures.setStageNumber(2);
                    CRF.InteractionFeatures.setStageNumber(2);

                    TrainCRF.TrainCRF tmpCRF = new TrainCRF.TrainCRF(inputFragsP2, fs, inputLabelsP2, mpCRF);
                    mpCRF.loadParameters(tmpCRF.train());
                    mpCRF.saveCRF(outputFile + ".p2");
                }
                #endregion
            }
			else
            {
                #region LABELING
                // Run the CRF to do labeling
				
				// Get our stroke data from the XML input file and load it into CRF
                for (int index = 0; index < dataFiles.Count; ++index)
                {
                    string xmlFilename = (string)dataFiles[index];
                    Console.WriteLine("");
                    Console.WriteLine("Doing inference on file: {0}", xmlFilename);

					string fsFile;
					if (fragLabeling)
						fsFile = xmlFilename.Replace(Files.FUtil.Extension(Files.Filetype.XML), Files.FUtil.Extension(Files.Filetype.FEATURESKETCH));
					else
						fsFile = xmlFilename.Replace(Files.FUtil.Extension(Files.Filetype.XML), ".fragged" + Files.FUtil.Extension(Files.Filetype.FEATURESKETCH));

					FeatureSketch fs;
					Sketch.Sketch sketchHolder;
					bool usedSerialized = false;
					if (useSerialized && File.Exists(fsFile))
					{
						fs = FeatureSketch.readFromFile(fsFile);
						sketchHolder = fs.Sketch;
						usedSerialized = true;
					}
					else
					{
						ConverterXML.ReadXML xmlHolder = new ConverterXML.ReadXML(xmlFilename);
						sketchHolder = xmlHolder.Sketch;
						fs = new FeatureSketch(ref sketchHolder);
					}

                    // Remove the labels if the file has alredy been labeled.
                    cleanUpSketch(ref sketchHolder, "AND", "OR", "NOT", "NAND", "NOR", "XOR", "XNOR",
                                                     "Wire", "Other", "Label", "Nonlabel", "Nongate",
                                                     "Nonwire", "BUBBLE", "Gate");
                    //debug
                    //(new ConverterXML.MakeXML(sketchHolder)).WriteXML("OnlyGates.xml");

                    // Fragmenting the sketch. Not if it's cached, though.
                    if (fragLabeling && usedSerialized == false)
                        Fragment.fragmentSketch(sketchHolder);

                    // See the comment at the beginning of the region INITIALIZING INTERNALS.
                    CRF.SiteFeatures.setStageNumber(1);
                    CRF.InteractionFeatures.setStageNumber(1);
                    doLabeling(1, crf, ref fs, intToStringTable, multiPassCRF);

                    if (multiPassCRF)
                    {
                        // In case of the following multipassCRF: Wire vs. Nonwire and then Gate vs. Label
                        //cleanUpSketch(ref sketchHolder, "Nonwire");

                        // In case of the following multipassCRF: Label vs. Nonlabel and then Wire vs. Gate
                        //cleanUpSketch(ref sketchHolder, "Nonlabel");

                        // In case of the following multipassCRF: Gate vs. Nongate and then Wire vs. Label
                        cleanUpSketch(ref sketchHolder, "Nongate");

                        // See the comment at the beginning of the region INITIALIZING INTERNALS.
                        CRF.SiteFeatures.setStageNumber(2);
                        CRF.InteractionFeatures.setStageNumber(2);
                        doLabeling(2, crf2, ref fs, intToStringTableP2, multiPassCRF);
                    }

                    ConverterXML.MakeXML xmlOut = new ConverterXML.MakeXML(sketchHolder);

                    if (outputFile.Length != 0)
                        xmlOut.WriteXML(outputFile);
                    else
                        xmlOut.WriteXML(xmlFilename.Remove(xmlFilename.Length - 4, 4) + ".LABELED.xml");
                }
                #endregion

            }
            DateTime endTime = DateTime.Now;

            Console.WriteLine("");
            Console.WriteLine("ENDING PROGRAM: {0}", endTime);
            TimeSpan timeElapsed = endTime - startTime;
            Console.WriteLine();
            Console.WriteLine("Time elapsed: {0}", timeElapsed);
            //Console.ReadLine();
			return;

        }

        #region TRAINING. HELPER FUNCTIONS.
        /// <summary>
        /// Finds inputFrags and inputLabels which are passed to TrainCRF.
        /// </summary>
        /// <param name="flag">1 Gate vs. Nongate, 2 Wire vs. Label.</param>
        /// <param name="fileName">File from the training set.</param>
        /// <param name="fragTraining">If true, then we fragment the data before training.</param>
        /// <param name="stringToIntTable">Hashtable returning the int representation of an element.</param>
        /// <param name="inputFrags">2D array of substrokes.</param>
        /// <param name="numFile">Index of fileName.</param>
        /// <param name="inputLabels">2D array of labels represented by an int.</param>
        static FeatureSketch initVars(int flag, string fileName, bool fragTraining, System.Collections.Hashtable stringToIntTable,
            ref Sketch.Substroke[][] inputFrags, int numFile, ref int[][] inputLabels)
        {
			Sketch.Sketch sketch;
			FeatureSketch fs;
			bool isXml = Files.FUtil.FileType(fileName) == Files.Filetype.XML;
			if (isXml)
			{
				ConverterXML.ReadXML xmlHolder = new ConverterXML.ReadXML(fileName);
				sketch = xmlHolder.Sketch;
				if (fragTraining)
					Fragment.fragmentSketch(sketch);
				fs = new FeatureSketch(ref sketch);
			}
			else
			{
				fs = FeatureSketch.readFromFile(fileName);
				sketch = fs.Sketch;
			}

            if (mapDomain)
            {
                LabelMapper.LabelMapper mapper = new LabelMapper.LabelMapper(labelMapFile);
                mapper.translateSketch(ref sketch);
            }
            //changeTypes(ref sketch, Criterion.toGateNongate);
            //changeTypes(ref sketch, "toWireLabel");
            //(new ConverterXML.MakeXML(sketch)).WriteXML("ToWireLabel.xml");

            // In case of the following multipassCRF: Gate vs. Nongate and then Wire vs. Label
            //if (flag == 1) changeTypes(ref sketch, Criterion.toGateNongate);
            //if (flag == 2) changeTypes(ref sketch, Criterion.toWireLabel);

            // In case of the following multipassCRF: Label vs. Nonlabel and then Wire vs. Gate
            //if (flag == 1) changeTypes(ref sketch, Criterion.toLabelNonlabel);
            //if (flag == 2) changeTypes(ref sketch, Criterion.toWireGate);

            // In case of the following multipassCRF: Wire vs. Nonwire and then Gate vs. Label
            //if (flag == 1) changeTypes(ref sketch, Criterion.toWireNonwire);
            //if (flag == 2) changeTypes(ref sketch, Criterion.toGateLabel);
            
			// Only fragment if we're using XML files. FSXML files have been pre-fragmented

            List<Substroke> substrokesAList = new List<Substroke>(sketch.Substrokes);
			substrokesAList.RemoveAll(delegate(Substroke s)
			{
				return (!stringToIntTable.ContainsKey(s.FirstLabel));
			});

			List<Substroke> toremove = sketch.SubstrokesL.FindAll(delegate(Substroke s)
			{
				return (!stringToIntTable.ContainsKey(s.FirstLabel));
			});
			if (toremove.Count > 0)	fs.RemoveSubstrokes(toremove);

			inputFrags[numFile] = substrokesAList.ToArray();
            inputLabels[numFile] = new int[inputFrags[numFile].Length];

            for (int j = 0; j < inputFrags[numFile].Length; j++)
            {
                string firstLabel = inputFrags[numFile][j].FirstLabel;
                int labelId;

                if (stringToIntTable.ContainsKey(firstLabel))
                    labelId = (int)stringToIntTable[firstLabel];

                // We have a problem, and the program should crash...
                else
                    labelId = (int)stringToIntTable["CRASH NOW"];

                inputLabels[numFile][j] = labelId;
            }
			return fs;
        }

        #endregion

        #region LABELING. HELPER FUNCTIONS.
        /// <summary>
        /// Does the labeling of a sketch.
        /// </summary>
        /// <param name="runNum">1 for Gate vs. Nongate stage, 2 for Wire vs. Label stage</param>
        /// <param name="crf">CRF.CRF used.</param>
        /// <param name="sketchHolder">Sketch to be labeled.</param>
        /// <param name="intToStringTable">Hashtable which returns a label given an int.</param>
        static void doLabeling(int runNum, CRF.CRF crf, ref FeatureSketch fs,
                               System.Collections.Hashtable intToStringTable, bool multiPassCRF)
        {
            string info = "";
            ArrayList substrokesNonlabel = new ArrayList();
            if (runNum == 1 && multiPassCRF) info = "Stage: Gate vs. Nongate";
            if (runNum == 2 && multiPassCRF) info = "Stage: Wire vs. Label";

            Console.WriteLine("");
            Console.WriteLine("Initializing graph.");

            //debug
            //foreach(Sketch.Substroke deb in sketchHolder.Substrokes)
            //    Console.WriteLine("Label: {0}", deb.GetFirstLabel());


            //changeTypes(ref sketchHolder, "toWireLabel");

			Sketch.Sketch sketchHolder = fs.Sketch;

            if (runNum == 1)
            {
                crf.initGraph(ref fs);
            }
            if (runNum == 2)
            {
                Substroke[] substrokesAll = sketchHolder.Substrokes;
                for (int i = 0; i < substrokesAll.Length; ++i)
                {
                    // In case of the following multipassCRF: Wire vs. Nonwire and then Gate vs. Label
                    //string typeOfElement = "Wire";

                    // In case of the following multipassCRF: Label vs. Nonlabel and then Wire vs. Gate
                    //string typeOfElement = "Label";
                    
                    // In case of the following multipassCRF: Gate vs. Nongate and then Wire vs. Label
                    string typeOfElement = "Gate";

                    if (!substrokesAll[i].Labels[0].Equals(typeOfElement))
                        substrokesNonlabel.Add(substrokesAll[i]);
                }
                crf.initGraph((Substroke[])substrokesNonlabel.ToArray(typeof(Substroke)),ref fs);
            }

            Console.WriteLine("Calculating features. ", info);
            crf.calculateFeatures();

            Console.WriteLine("Doing inference.      ", info);
            crf.infer();

            Console.WriteLine("Finding labels.       ", info);
            int[] outIntLabels;
            double[] outProbLabels;
            crf.findLabels(out outIntLabels, out outProbLabels);

            for (int i = 0; i < outIntLabels.Length; i++)
            {
                if (runNum == 1)
                {
                    sketchHolder.AddLabel(sketchHolder.Substrokes[i], (string)intToStringTable[outIntLabels[i]], outProbLabels[i]);
                }
                if (runNum == 2)
                    sketchHolder.AddLabel((Substroke)substrokesNonlabel[i], (string)intToStringTable[outIntLabels[i]], outProbLabels[i]);

            }

        }
        #endregion

        #region HELPER FUNCTIONS.
        /// <summary>
		/// Prints out help information on using RunCRF.
		/// </summary>
		public static void printHelp()
		{
            Console.WriteLine(@"
******************************************************
* USAGE (note that all commands should be one line): *
******************************************************
The command for inference using an ordinary CRF should look like:
RunCRF.exe -fl -c tcrfGeneratedDuringTraining.tcrf -l labelFile.txt 
           -d directoryOfFilesToBeLabeled
The labeled files will be created in directoryOfFilesToBeLabeled 
with a .LABELED extension. 

An alternative command to the one above is:
RunCRF.exe -fl -c tcrfGeneratedDuringTraining.tcrf -l labelFile.txt 
           -o outputFile.xml inputFile.xml

The command for inference using a multipass CRF should look like:
RunCRF.exe -p2 -fl -c firstStage.tcrf secondStage.tcrf 
           -np2 numberOfLabels -l labelFileFirstStage.txt 
           -lp2 labelFileSecondStage.txt -d directoryOfFilesToBeLabeled

The command for training using an ordinary CRF should look like:
RunCRF.exe -t -ft -n numberOfLabels -l labelFile.txt -o outputFile.tcrf 
           -d firectoryOfFilesForTraining

The command for training using a multipass CRF should look like:
RunCRF.exe -p2 -t -ft -n numberOfLabels -np2 numberOfLabels 
           -l labelFileFirstStage.txt -lp2 labelFileSecondStage.txt 
           -o outputFile.tcrf -d firectoryOfFilesForTraining 

*****************************
* DESCTIPTION OF EACH FLAG: *
*****************************

-fl  specifies that we are running fragmented labeling

-c   tcrfGeneratedDuringTraining.tcrf specifies the location of the tcrf which 
     we are loading; tcrfGeneratedDuringTraining.tcrf - the crf you obtained 
     from training, the .tcrf indicates trained crf, but it is really just a 
     text file that keeps track of parameters.

-l   labelFile.txt loads the labelFile.txt (you need the path to the file) 
     which is used to translate between numerical and english representations 
     of labels (like 0=wire, 1=gate). See the file Wire_Label_Domain_CRF.txt 
     in Latest_Labeled_Data3, for an example.

-d   directoryOfFilesToBeLabeled as it suggests specifies the location of the 
     directory containing the files we want to label.

-p2  specifies that we are in multipass mode.

-n   specifies the number of classes

-np2 specifies the number of classes during the second stage of the multipass 
     recognition (e.g. for wire-label we have 2)

-lp2 specifies the location of the labelFile used during the second stage of 
     the multipass recognition.

-t   flag specifices that the CRF is going to run training

-ft  fragments the input sketches before training

-o   specifies the name of the tcrf

outputFile.xml - the final result of classification, in MIT XML format

inputfile.txt  - the unclassified MIT XML that you want to classify.

-r   create a random CRF

-S   use serialized, pre-computed FSXML files whenever possible

numberOfLabels - this is an integer >= 2 that indicates the number of 
     different ways an individual stroke can be classified. It is 2 for 
     wire/gate
");
		}


        /// <summary>
        /// At the beginning of Main we subscribe to the UnhandledException event with an 
        /// UnhandledExceptionEventHandler delegate. This allows us to easily restart the 
        /// program if an exception is thrown due to division by infinity in a thread 
        /// in the Loopy Belief Propagation code.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public static void OnUnhandledException(Object sender, UnhandledExceptionEventArgs e)
        {
            
            Exception ex = (Exception)e.ExceptionObject;
            string message = "The program is restarting due to bad initial values in the " +
                             "calculation of the Conjugate Gradient Descent. This causes " +
                             "division by infinity during rescaling of the messages in the " +
                             "implementation of the Loopy Belief Propagation algorithm.";

            /*
            MessageBox.Show(message);
            */
            if (ex is ArithmeticException)
            {
                Console.WriteLine("");
                Console.WriteLine(message);
                Application.Restart();
                System.Environment.Exit(-1);
            }
            else 
            {
                Console.WriteLine("HOUSTON, WE HAVE A PROBLEM:");
                //Console.WriteLine(ex.ToString());
            }
            
        }
        /// <summary>
        /// Removes shapes with given types from Sketch.Sketch.
        /// </summary>
        /// <param name="sketchHolder">Sketch to be changed.</param>
        /// <param name="typesArray">Types which we want to remove.</param>
        static void cleanUpSketch(ref Sketch.Sketch sketchHolder, params string[] typesArray)
        {
            ArrayList typesAList = ArrayList.Adapter(typesArray);

            foreach (Sketch.Shape shape in sketchHolder.Shapes)
            {
                String type = shape.XmlAttrs.Type.ToString();

                if (typesAList.Contains(type))
                {
                    sketchHolder.RemoveShape(shape);
                }
            }
        }

        /// <summary>
        /// Changes types in Sketch.Sketch.
        /// *Edit: Try using the LabelMapper instead.*
        /// </summary>
        /// <param name="sketchHolder">Sketch to be changed.</param>
        /// <param name="criterion">Flag specifying what changes we want to make.</param>
        static void changeTypes(ref Sketch.Sketch sketchHolder, Criterion criterion)
        {
            foreach (Shape shape in sketchHolder.Shapes)
            {
                string type = shape.XmlAttrs.Type.ToString();

                switch(criterion)
                {
                    case Criterion.toGateNongate:
                        if (!type.Equals("Gate"))
                            shape.XmlAttrs.Type = "Nongate";
                    break;

                    case Criterion.toWireLabel:
                        if (!(type.Equals("Wire") || type.Equals("Label")))
                            sketchHolder.RemoveSubstrokes(new List<Substroke>(shape.Substrokes));
                    break;

                    case Criterion.toLabelNonlabel:
                        if (!type.Equals("Label"))
                            shape.XmlAttrs.Type = "Nonlabel";
                    break;      

                    case Criterion.toWireGate:
                        if (type.Equals("AND") || type.Equals("OR") || type.Equals("NOT") ||
                            type.Equals("NAND") || type.Equals("NOR") || type.Equals("XOR") ||
                            type.Equals("XNOR"))
                        {
                            shape.XmlAttrs.Type = "Gate";
                        }
                        else if (!(type.Equals("Wire") || type.Equals("Gate"))) // Need "Gate" since some sketches
                                                                                // are labeled wire-gate-label
                        {
                            sketchHolder.RemoveSubstrokes(new List<Substroke>(shape.Substrokes));
                        }
                    break;

                    case Criterion.toWireNonwire:
                        if (!type.Equals("Wire"))
                            shape.XmlAttrs.Type = "Nonwire";
                    break;

                    case Criterion.toGateLabel:
                        if (!(type.Equals("Gate") || type.Equals("Label")))
                            sketchHolder.RemoveSubstrokes(new List<Substroke>(shape.Substrokes));  
                    break;

                    default:
                        Console.WriteLine("Unknown criterion in the function changeTypes().");
                    break;
                }
            }
        }
        #endregion
    }
}
