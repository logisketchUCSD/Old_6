/*
 * File: CongealRecognizer.cs, previously Classify.cs
 *
 * Author: Unknown, Marty Field, and James Brown
 * Harvey Mudd College, Claremont, CA 91711.
 * Sketchers 2008.
 * 
 * Use at your own risk.  This code is not maintained and not guaranteed to work.
 * We take no responsibility for any harm this code may cause.
 */

using System;
using System.Threading;
using System.Drawing;
using System.IO;
using System.Text;
using System.Collections.Generic;
using Congeal;

namespace OldRecognizers
{
	/// <summary>
	/// This class implements recognition using the Congealer. The meat of the work is
	/// actually done by the Congeal.Designation class, which you can find in the 
	/// Recognition\Congeal directory.
	/// </summary>
	public class CongealRecognizer : Recognizer
	{
		#region Internals

        private List<Designation> _symbolTypes;
		private int _numThreads;
		bool loaded = false;

        #endregion

        #region Constructors

		/// <summary>
		/// Creates a new CongealRecognizer. This loads files with the .m4a extension
		/// from the directory that the CongealRecognizer.dll file is in. An exception 
		/// will be thrown if no such files are found
		/// </summary>
        public CongealRecognizer()
        {
			loaded = false;
        }

		/// <summary>
		/// Creates a new CongealRecognizer with the specified designations
		/// </summary>
		/// <param name="designations">The different classes we might recognize</param>
        public CongealRecognizer(List<Designation> designations)
        {
            _symbolTypes = designations;
			initThreads();
			loaded = true;
        }

		/// <summary>
		/// Creates a new CongealRecognizer with the specified designations
		/// </summary>
		/// <param name="designations">The different classes we might recognize</param>
		public CongealRecognizer(Designation[] designations)
			: this(new List<Designation>(designations))
		{
			// Nothing to do here
		}

		/// <summary>
		/// Do threading setup.
		/// </summary>
		private void initThreads()
		{
			/*
			 * Only use multi-threaded code in RELEASE mode, because
			 * the Visual Studio debugger doesn't properly display
			 * multi-threaded code (sometimes the stack traces get confused,
			 * and you can't view most stack variables). This might be fixed
			 * in Visual Studio 2008, so if you're using Visual Studio 2008,
			 * you might disable this test.
			 */
			#if DEBUG
				_numThreads = 1;
			#else
				_numThreads = Math.Min(_symbolTypes.Count, Environment.ProcessorCount + 1);
			#endif
		}
		#endregion

		/// <summary>
		/// Load the files in and initialize the recognizer. This operation
		/// has to be delayed to prevent missing files from making TestRig barf
		/// when it constructs a CongealRecognizer that it's never going to use.
		/// </summary>
		private void load()
		{
			List<Designation> ld = new List<Designation>();

			string fullname = System.Reflection.Assembly.GetExecutingAssembly().Location;
			string path = System.IO.Path.GetDirectoryName(fullname);

            string[] dirFiles = System.IO.Directory.GetFiles(
                path, "*.m4a"); //+ Files.FUtil.Extension(Files.Filetype.CONGEALER_TRAINING_DATA), SearchOption.TopDirectoryOnly);

			foreach (string file in dirFiles)
			{
				ld.Add(Designation.LoadDesignation(file, dirFiles.Length));
			}

			if (ld.Count == 0)
				throw new Exception("No trained data files were provided. Please ensure that they are in the same directory as Congeal.dll");

			_symbolTypes = ld;

			initThreads();

			loaded = true;
		}

		/// <summary>
		/// Performs recognition on the provided array of substrokes
		/// </summary>
		/// <param name="strokes">The strokes to attempt to find a match for</param>
		/// <returns>The result of the recognition</returns>
		public override Results Recognize(Sketch.Substroke[] strokes)
        {
			if (!loaded)
				load();

			Sketch.Shape shape = new Sketch.Shape(new List<Sketch.Substroke>(strokes), new Sketch.XmlStructs.XmlShapeAttrs(true));
			shape.UpdateAttributes();

			if (shape.Centroid[0] == double.NaN)
				throw new Exception("NaN found!");

			if (shape == null)
				throw new Exception("Null shape detected.");

			/* This code is multi-threaded using a ThreadPool. For more information on how
			 * this particular sort of threading works, see the Microsoft documentation at 
			 * http://msdn.microsoft.com/en-us/library/3dasc8as(VS.80).aspx
			 */
			ThreadPool.SetMaxThreads(_numThreads, _numThreads);

			List<ManualResetEvent> events = new List<ManualResetEvent>(_numThreads);
			List<CongealRecognizerThread> threads = new List<CongealRecognizerThread>(_numThreads);

            foreach (Designation d in _symbolTypes)
            { 
				ManualResetEvent m = new ManualResetEvent(false);
				CongealRecognizerThread c = new CongealRecognizerThread(d, ref shape, m);
				events.Add(m);
				threads.Add(c);
				ThreadPool.QueueUserWorkItem(c.ThreadPoolCallback);
            }

			// Wait for computation to finish
			WaitHandle.WaitAll(events.ToArray());

			Results r = new Results();
			
			double minTanimoto = Double.PositiveInfinity, minYule = Double.PositiveInfinity, minHausdorff=Double.PositiveInfinity, minModifiedHausdorff = Double.PositiveInfinity;//, minEntropy = Double.PositiveInfinity;
			double maxTanimoto = 0, maxYule = 0, maxHausdorff = 0, maxModifiedHausdorff = 0;//, maxEntropy = 0;
			foreach (CongealRecognizerThread t in threads)
			{
				if (t.Metrics["Tanimoto"] > maxTanimoto)
					maxTanimoto = t.Metrics["Tanimoto"];
				if (t.Metrics["Tanimoto"] < minTanimoto)
					minTanimoto = t.Metrics["Tanimoto"];
				if (t.Metrics["Yule"] > maxYule)
					maxYule = t.Metrics["Yule"];
				if (t.Metrics["Yule"] < minYule)
					minYule = t.Metrics["Yule"];
				if (t.Metrics["Hausdorff"] > maxHausdorff)
					maxHausdorff = t.Metrics["Hausdorff"];
				if (t.Metrics["Hausdorff"] < minHausdorff)
					minHausdorff = t.Metrics["Hausdorff"];
				if (t.Metrics["ModifiedHausdorff"] > maxModifiedHausdorff)
					maxModifiedHausdorff = t.Metrics["ModifiedHausdorff"];
				if (t.Metrics["ModifiedHausdorff"] < minModifiedHausdorff)
					minModifiedHausdorff = t.Metrics["ModifiedHausdorff"];
				/* Entropy is disabled as a metric because it's silly!
				if (t.Metrics["Entropy"] > maxEntropy)
					maxEntropy = t.Metrics["Entropy"];
				if (t.Metrics["Entropy"] < minEntropy)
					minEntropy = t.Metrics["Entropy"];*/
			}
			double maxscore = 0;
			string best = "", secondbest = "";
			foreach (CongealRecognizerThread t in threads)
			{
				double yule = (t.Metrics["Yule"] - minYule) / (maxYule - minYule);
				double tanimoto = (t.Metrics["Tanimoto"] - minTanimoto) / (maxTanimoto - minTanimoto);
				double hausdorff = (t.Metrics["Hausdorff"] - minHausdorff) / (maxHausdorff - minHausdorff);
				double modifiedhausdorff = (t.Metrics["ModifiedHausdorff"] - minModifiedHausdorff) / (maxModifiedHausdorff - minModifiedHausdorff);
				//double entropy = (t.Metrics["Entropy"] - minEntropy) / (maxEntropy - minEntropy);
				//Console.WriteLine("Designation {4}\tY: {0:0.000}, T:{1:0.000}, H:{2:0.000}, MH:{3:0.000}", yule, tanimoto, hausdorff, modifiedhausdorff, t.Name);
				double distance = (yule + tanimoto + hausdorff + modifiedhausdorff)/4;
				double score = 1 - distance;
				if (score > maxscore)
				{
					maxscore = score;
					secondbest = best;
					best = String.Format("\t\t{0} - Y: {1:0.000}, T:{2:0.000}, H:{3:0.000}, MH:{4:0.000}, Score:{5:0.000}", t.Name, yule, tanimoto, hausdorff, modifiedhausdorff, score);
				}
				r.Add(t.Name, score , t.Bitmap);
			}
			r.AddInfoString(String.Format("\tShape labeled as {0}", r.BestLabel));
			r.AddInfoString(best);
			r.AddInfoString(secondbest);
            return r;
        }
	}

	/// <summary>
	/// This class is used for the ThreadPool. It is a very simply wrapper around Designation
	/// that takes so many lines because of this bloody language.
	/// </summary>
	internal class CongealRecognizerThread
	{
		#region Internals

		private Designation _d;
		private Sketch.Shape _shape;
		private ManualResetEvent _doneEvent;
		private Dictionary<string, double> _metrics;
		private SymbolRec.Image.Image _bitmap;

		#endregion

		#region Constructors

		/// <summary>
		/// Create a new CongealRecognizerThread
		/// </summary>
		/// <param name="d">The designation to create from</param>
		/// <param name="shape">The shape to recognize</param>
		/// <param name="doneEvent">An event which will be raised when recognition is completed</param>
		public CongealRecognizerThread(Designation d, ref Sketch.Shape shape, ManualResetEvent doneEvent)
		{
			_d = d;
			_shape = shape;
			if (_shape.Centroid[0] == double.NaN)
				throw new Exception("I hate this language.");
			_doneEvent = doneEvent;
		}

		#endregion

		#region Threading

		/// <summary>
		/// This is used for threading interaction
		/// </summary>
		/// <param name="threadInformation">Unused</param>
		public void ThreadPoolCallback(Object threadInformation)
		{
			_metrics = _d.classify(_shape, "", classifyMetric.ALL, out _bitmap);
			_doneEvent.Set();
		}

		#endregion

		#region Accessors

		/// <summary>
		/// Information about subresults from the congealer
		/// </summary>
		public Dictionary<string, double> Metrics
		{
			get
			{
				return _metrics;
			}
		}

		/// <summary>
		/// The name of the associated designation
		/// </summary>
		public string Name
		{
			get
			{
				return _d.Name;
			}
		}

		/// <summary>
		/// The post-congealing bitmap
		/// </summary>
		public SymbolRec.Image.Image Bitmap
		{
			get { 
				return _bitmap; 
			}
		}

		#endregion
	}
}
