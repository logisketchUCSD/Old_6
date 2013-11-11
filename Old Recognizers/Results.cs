using System;
using System.Collections.Generic;
using Sketch;

namespace OldRecognizers
{
    /// <summary>
    /// Class that stores the results after recognition has been performed
    /// </summary>
    public class Results
    {
        #region INTERNALS

        /// <summary>
        /// Stores the doubley sorted list
        /// </summary>
        private PairedList.PairedList<string, double> m_list;

        /// <summary>
        /// Indicates whether the PairedList has been sorted
        /// </summary>
        private bool sorted;

		/// <summary>
		/// Image of the recognition state
		/// </summary>
		private SymbolRec.Image.Image m_bitmap = null;
		private int DWIDTH = 32;
		private int DHEIGHT = 32;

		/// <summary>
		/// Extra informations
		/// </summary>
		private List<string> infos;

        #endregion

        #region CONSTRUCTORS

        /// <summary>
        /// Default Constructor
        /// </summary>
        public Results()
            : this(6) { }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="capacity">Initial capacity</param>
        public Results(int capacity)
        {
            m_list = new PairedList.PairedList<string, double>(capacity);
            sorted = false;
			infos = new List<string>();
        }

        #endregion

        #region MODIFIERS

        /// <summary>
        /// Add the following pair to the PairedList
        /// </summary>
        /// <param name="label">Type to add</param>
        /// <param name="probability">Corresponding probability</param>
        public void Add(string label, double probability)
        {
            m_list.Add(label, probability);
            sorted = false;
        }

		public void Add(string label, double probability, SymbolRec.Image.Image bitmap)
		{
			Add(label, probability);
			m_bitmap = bitmap;
		}

        /// <summary>
        /// Sort the PairedList (bidirectional)
        /// </summary>
        internal void Sort()
        {
            m_list.Sort();
            sorted = true;
        }

		/// <summary>
		/// Add an informational stirng
		/// </summary>
		/// <param name="info"></param>
		public void AddInfoString(string info)
		{
			infos.Add(info);
		}

        #endregion

        #region GETTERS

        /// <summary>
        /// Get the probability associated with a specific label
        /// </summary>
        /// <param name="label">Type to find</param>
        /// <returns>The probability of label</returns>
        public double this[string label]
        {
            get
            {
                List<PairedList.Pair<string, double>> labelList = LabelList;
                int i, len = labelList.Count;
                for (i = 0; i < len; ++i)
                    if (labelList[i].ItemA.Equals(label))
                        return labelList[i].ItemB;

                return 0.0;
            }
        }

        /// <summary>
        /// Return the label sorted list
        /// </summary>
        public List<PairedList.Pair<string, double>> LabelList
        {
            get
            {
                if (!sorted)
                    Sort();

                return m_list.ListA;
            }
        }

        /// <summary>
        /// Return the probability sorted list
        /// </summary>
        public List<PairedList.Pair<double, string>> ProbabilityList
        {
            get
            {
                if (!sorted)
                    Sort();

                return m_list.ListB;
            }
        }

        /// <summary>
        /// Returns the highest probability
        /// </summary>
        /// <returns></returns>
        public double BestMeasure
        {
            get
            {
                if (m_list.ListA.Count == 0)
                    return 0.0;
                else
                {
                    List<PairedList.Pair<double, string>> probabilityList = ProbabilityList;
                    return probabilityList[probabilityList.Count - 1].ItemA;
                }
            }
        }

        /// <summary>
        /// Returns the label with the highest probability
        /// </summary>
        /// <returns>Type with height probability</returns>
        public string BestLabel
        {
            get
            {
                if (m_list.ListA.Count == 0)
                    return "UNKNOWN";
                else
                {
                    List<PairedList.Pair<double, string>> probabilityList = ProbabilityList;
                    return probabilityList[probabilityList.Count - 1].ItemB;
                }
            }
        }

		/// <summary>
		/// Get the best pair
		/// </summary>
        public PairedList.Pair<double, string> BestPair
        {
            get
            {
                if (m_list.ListA.Count == 0)
                    return null;
                else
                {
                    List<PairedList.Pair<double, string>> probabilityList = ProbabilityList;
                    return probabilityList[probabilityList.Count - 1];
                }
            }
        }
		
		/// <summary>
		/// A bitmap of the current recognition state. Useful for transforming image-based recognizers.
		/// </summary>
		public SymbolRec.Image.Image Bitmap
		{
			get
			{
				if (m_bitmap == null)
					return new SymbolRec.Image.Image(DWIDTH, DHEIGHT);
				else
					return m_bitmap;
			}
			set
			{
				m_bitmap = value;
			}
		}

        #endregion

        #region MISC

        /// <summary>
        /// Returns the string representation, sorted by labels
        /// </summary>
        /// <returns>String representation</returns>
        public override string ToString()
        {
            List<PairedList.Pair<string, double>> labelList = LabelList;

            int i, len = labelList.Count;
            string status = "";
            string format = "{0:0%}";

            for (i = 0; i < len; ++i)
            {
                status += labelList[i].ItemA + ":" + String.Format(format, labelList[i].ItemB) + " ";
            }

            return status;
        }


        /// <summary>
        /// Turn the results into a color for visualization purposes
        /// </summary>
        /// <returns>Color associated with the results</returns>
        public System.Drawing.Color ToColor()
        {
            System.Drawing.Color c;
            double r = 0.0, b = 0.0, g = 0.0, prob;

            List<PairedList.Pair<string, double>> labelList = LabelList;
            int i, len = labelList.Count;
            for (i = 0; i < len; ++i)
            {
                c = Colorizer.LabelToColor(labelList[i].ItemA);
                prob = labelList[i].ItemB;

                r += c.R * prob;
                g += c.G * prob;
                b += c.B * prob;
            }

            return System.Drawing.Color.FromArgb((int)r, (int)g, (int)b);
        }

		/// <summary>
		/// Informational strings
		/// </summary>
		public List<string> Infos
		{
			get
			{
				return infos;
			}
		}

        #endregion
    }
}
