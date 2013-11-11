using System;
using System.Collections;
using Sketch;
using Featurefy;
using MathNet.Numerics.LinearAlgebra;

namespace Segment
{
	/// <summary>
	/// Goal:
	/// Segment is the class that puts everything together after each substroke has been labeled.
	/// That is, it figures out that these group of substrokes labeled as 'and' is a single 
	/// (or multiple) 'and' gate(s). Segment also has the ability to reinterpret labeled substrokes.
	/// Since each substroke has a probability associated with it, educated guesses can be made
	/// appropriately.  In addition to labeling components, Segment figures out how everything is 
	/// interconnected.  That is, it can construct a truth table for a given logic diagram. 
	///  
	///  So far, Segment is far from being done.  All the updateCC functions are based on semi-random
	///  values and untested with a Matching belief.  That is, Belief needs to be updated before any
	///  really testing or results can be done.
	/// </summary>
	public class Segment
	{
		#region INTERNALS 

		/// <summary>
		/// The labeled Sketch to Segment
		/// </summary>
		private Sketch.Sketch sketch;

		/// <summary>
		/// The labeled Substrokes (belongs to sketch)
		/// </summary>
		private Substroke[] substrokes;

		/// <summary>
		/// An ArrayList of Component
		/// </summary>
		private ArrayList components;

		/// <summary>
		/// If a label is greater than BELIEFTHRESHOLD, do not change it
		/// </summary>
		//private static double BELIEFTHRESHOLD = 0.95; // NEVER USED ATM

		/// <summary>
		/// This is an ArrayList of ConnectedComponents of the sketch
		/// </summary>
		private ArrayList connectedComponents;
		
		/// <summary>
		/// This is an adjacency matrix telling whether a ConnectedComponentn is adjacent to a Wire
		/// </summary>
		private Matrix connectedComponentsAdjacentWires;

		/// <summary>
		/// This is an adjacency matrix telling whether a ConnectedComponent is adjacent (through a wire) to another component
		/// </summary>
		private Matrix connectedComponentsAdjacentComponents;

		/// <summary>
		/// Whether to include unlabeled strokes in the calculations
		/// </summary>
		private bool useUnlabeled;

		private const bool USE_UNLABELED_DEFAULT = false;

		private static Random random = new Random(DateTime.Now.Millisecond);

		#endregion

		#region CONSTRUCTORS

		/// <summary>
		/// Constructor (note: modifies sketch)
		/// </summary>
		/// <param name="sketch"></param>
		public Segment(Sketch.Sketch sketch)
		{
			this.connectedComponents = null;
			this.sketch = sketch;
			this.substrokes = this.sketch.Substrokes;

			this.useUnlabeled = USE_UNLABELED_DEFAULT;

			this.initializeSegments();
		}


		#endregion

		#region INITIAL SEGMENTATION

		/// <summary>
		/// Adds each of the substrokes into an initial grouping strictly by labels
		/// </summary>
		private void initializeSegments()
		{		
			this.createComponents();
			this.extractCC();
		//	this.calculateCCAdjacentWires();
		//	this.calculateCCAdjacentComponents();
		//	this.showCCBelief();
		//	this.updateAllCC();
		}

		
		private void createComponents()
		{
			ArrayList labels = new ArrayList(this.sketch.LabelStrings);
			// ex.: LabelStrings = ["Wire", "Gate"]
	
			// Create a component for every label
			// ex.: [c("Wire"), c("Gate")]
			this.components = new ArrayList();
			for(int i = 0; i < labels.Count; ++i)
			{
				if(useUnlabeled || !((string)labels[i]).Equals("unlabeled"))
					this.components.Add(new Component((string)labels[i]));
			}

			// Group all substrokes into the constructed components based on their parent shape's label
			for(int i = 0; i < this.substrokes.Length; ++i)
			{
				string label = this.substrokes[i].FirstLabel;
				if(useUnlabeled || !label.Equals("unlabeled"))
					((Component)this.components[labels.IndexOf(label)]).addSubstroke(this.substrokes[i]);
			}

			// Calculate all the compenents internals (connected matrix, adjacency matrix, etc)
			for(int i = 0; i < this.components.Count; ++i)
				((Component)this.components[i]).calculateAll();	
		}

	
		/// <summary>
		/// Extract the connected components and store it.
		/// The ConnectedComponents are each stored in their respective Components
		/// </summary>
		private void extractCC()
		{
			this.connectedComponents = new ArrayList();
			Component component;
			for(int i = 0; i < this.components.Count; ++i)
			{
				component = (Component)this.components[i];
				for(int j = 0; j < component.ConnectedComponents.Count; ++j)
					connectedComponents.Add(component.ConnectedComponents[j]);
			}
		}

	
		/// <summary>
		/// Step through the ConnectedComponents and determine which ones are adjacent to wire components
		/// </summary>
		private void calculateCCAdjacentWires()
		{
			if(this.connectedComponents == null)
				this.extractCC();

			int length = this.connectedComponents.Count;

			//Create an n x n matrix where n is the number of connected components
			this.connectedComponentsAdjacentWires = new Matrix(length, length);

			//Loop through pairwise components and set it to 1.0 if something is adjacent to a wire
			for(int i = 0; i < length - 1; ++i)
				for(int j = i + 1; j < length; ++j)
					if(ConnectedComponent.isAdjacentWires((ConnectedComponent)this.connectedComponents[i], (ConnectedComponent)this.connectedComponents[j]))
					{
						this.connectedComponentsAdjacentWires[i, j] = 1.0;
						this.connectedComponentsAdjacentWires[j, i] = 1.0;
					}			
		}

		
		/// <summary>
		/// Compute whether ConnectedComponents are Adjacent through a Wire ConnectedComponent
		/// </summary>
		private void calculateCCAdjacentComponents()
		{
			if(this.connectedComponents == null)
				this.extractCC();

			if(this.connectedComponentsAdjacentWires == null)
				this.calculateCCAdjacentWires();

			int length = this.connectedComponents.Count;

			//Create an n x n matrix where n is the number of connected components
			this.connectedComponentsAdjacentComponents = new Matrix(length, length);

			
			//ConnectedComponent I
			for(int i = 0; i < length - 1; ++i)
			{
				//If it is a wire, don't bother (we want a non wire ConnectedComponent)
				if(((ConnectedComponent)this.connectedComponents[i]).Label.ToLower().Equals("wire"))
					continue;

				//ConnectedComponent J
				for(int j = i + 1; j < length; ++j)
				{
					//If it is a wire, don't bother (we want a non wire ConnectedComponent
					if(((ConnectedComponent)this.connectedComponents[j]).Label.ToLower().Equals("wire"))
						continue;
					
					//WIRE
					for(int wire = 0; wire < length; ++wire)
					{
						//If it is not a wire, don't bother (we WANT a wire ConnectedComponent)
						if(!((ConnectedComponent)this.connectedComponents[wire]).Label.ToLower().Equals("wire"))
							continue;

						//If I ~ WIRE, J ~ WIRE, then I ~~ J (we will call it adjacent)
						if(this.connectedComponentsAdjacentWires[i, wire] == 1.0 && this.connectedComponentsAdjacentWires[j, wire] == 1.0)
						{
							this.connectedComponentsAdjacentComponents[i, j] = 1.0;
							this.connectedComponentsAdjacentComponents[j, i] = 1.0;
						}
						/*else
						{						
							this.componentsAdjacent[i, j] = 0.0;
							this.componentsAdjacent[j, i] = 0.0;
						}*/
					}
				
				}
			}			
		}


		#endregion

		#region BELIEF SEGMENTATION

		private string getLabel(int index)
		{
			return ((ConnectedComponent)this.ConnectedComponents[index]).Label;
		}

	
		private double getBelief(int index)
		{
			return Belief.belief(((ConnectedComponent)this.ConnectedComponents[index]));
		}

		
		/// <summary>
		/// Show every ConnectedComponent
		/// </summary>
		private void showCCBelief()
		{
			for(int i = 0; i < this.connectedComponents.Count; ++i)
				Console.WriteLine(this.getLabel(i) + i + ": " + this.getBelief(i));
		}

		
		private void updateAllCC()
		{
			Console.Write("Updating");
			//Do stuff

			int length = this.connectedComponents.Count;
			for(int i = 0; i < length; ++i)
			{
				this.updateCC(i);
				Console.Write(".");
			}
			Console.WriteLine("Single Done!");

			length = this.connectedComponents.Count;
			for(int i = 0; i < length - 1; ++i)
			{
				for(int j = i + 1; j < length; ++j)
				{
					this.updateCCPairwise(i, j);
					Console.Write(".");
				}
				Console.WriteLine();
			}

					
			this.calculateCCAdjacentWires();
			this.calculateCCAdjacentComponents();			
		}

		
		private void updateCC(int index)
		{
			//Component c1 = this.updateCCThreshold((ConnectedComponent)this.connectedComponents[index]);
			//Component c2 = this.updateCCTemporal((ConnectedComponent)this.connectedComponents[index]);
			//Component c3 = this.updateCCRemove((ConnectedComponent)this.connectedComponents[index]);
			/*
			Component c4 = this.updateCCRemoveWorst((ConnectedComponent)this.connectedComponents[index]);
			this.connectedComponents.RemoveAt(index);
			this.connectedComponents.Insert(index, c4.ConnectedComponents[0]);
			this.connectedComponents.Add(c4.ConnectedComponents[1]);
			*/
		}

		private void updateCCPairwise(int i, int j)
		{
			ConnectedComponent cc1 = (ConnectedComponent)this.connectedComponents[i];
			ConnectedComponent cc2 = (ConnectedComponent)this.connectedComponents[j];

			double cc1B = cc1.belief();
			double cc2B = cc2.belief();

			//Need to create global thresholds for these values... 
			if((cc1.Substrokes.Count == 1 || cc2.Substrokes.Count == 1) && (cc1B < 0.9 || cc2B < 0.9))
			{

				Component c = new Component(cc1.Label);
				c.ConnectedComponents.Add(cc1);
				c.ConnectedComponents.Add(cc2);

				if(this.isBetter(c, cc1) > 0.0 && this.isBetter(c, cc2) > 0.0)
					Console.Write("*");
			}			
		}

		
		/// <summary>
		/// Try to get a better Belief by making Component.THRESHOLD smaller
		/// WARNING: Component.THRESHOLD is changed... make sure to change back
		/// </summary>
		private Component updateCCThreshold(ConnectedComponent cc)
		{
			Component c = new Component((Substroke[])cc.Substrokes.ToArray(typeof(Substroke)), (string)cc.Label.Clone());

			double originalThreshold = Component.DISTANCETHRESHOLD;

			const int TRIES = 10;

			for(int i = 0; i < TRIES; ++i)
			{
				//Make the threshold 90% of what it used to be
				Component.DISTANCETHRESHOLD *= 0.9;

				c.calculateAll();
				if(this.isBetter(c, cc) > 0)
					break;
			}		

			Component.DISTANCETHRESHOLD = originalThreshold;

			return c;
		}

		
		/// <summary>
		/// Try to get a better Belief by dividing the CC based on time
		/// </summary>
		/// <returns></returns>
		private Component updateCCTemporal(ConnectedComponent cc)
		{
			ConnectedComponent cc1 = cc.Clone();
			ConnectedComponent cc2 = new ConnectedComponent((string)cc.Label.Clone());

			ConnectedComponent bestCC1 = null;
			ConnectedComponent bestCC2 = null;

			double best = double.NegativeInfinity;

			//double max;
			double average;
			double cc1B;
			double cc2B;

			for(int i = 0; i < cc.Substrokes.Count - 1; ++i)
			{
				cc1.removeSubstroke((Substroke)cc.Substrokes[i]);
				cc2.addSubstroke((Substroke)cc.Substrokes[i], 0.9); //FIX BELIEF

				cc1B = cc1.belief();
				cc2B = cc2.belief();

				//max = cc1B > cc2B ? cc1B : cc2B;
				average = (cc1B + cc2B) / 2;

				//if(max > best)
				if(average > best)
				{
					//best = max;
					best = average;
					bestCC1 = (ConnectedComponent)cc1.Clone();
					bestCC2 = (ConnectedComponent)cc2.Clone();
				}
			}

			Component toReturn = new Component((string)cc.Label.Clone());

			toReturn.addCC(bestCC1);
			toReturn.addCC(bestCC2);

			return toReturn;
		}

		/// <summary>
		/// Removes a single substroke such that it maximizes the 'bigger' CC's belief
		/// </summary>
		/// <param name="cc"></param>
		/// <returns></returns>
		private Component updateCCRemove(ConnectedComponent cc)
		{
			ConnectedComponent cc1 = cc.Clone();
			ConnectedComponent cc2 = new ConnectedComponent((string)cc.Label.Clone());

			ConnectedComponent bestCC1 = null;
			ConnectedComponent bestCC2 = null;

			double best = double.NegativeInfinity;

			double max;
			double cc1B;
			double cc2B;

			for(int i = 0; i < cc.Substrokes.Count; ++i)
			{
				cc1.removeSubstroke((Substroke)cc.Substrokes[i]);
				cc2.addSubstroke((Substroke)cc.Substrokes[i], 0.9); //FIX BELIEF

				cc1B = cc1.belief();
				cc2B = cc2.belief();


				max = cc1B > cc2B ? cc1B : cc2B;

				if(max > best)
				{
					best = max;
					bestCC1 = (ConnectedComponent)cc1.Clone();
					bestCC2 = (ConnectedComponent)cc2.Clone();
				}

				cc1.addSubstroke((Substroke)cc.Substrokes[i], 0.9);
				cc2.removeSubstroke((Substroke)cc.Substrokes[i]);
			}

			Component toReturn = new Component((string)cc.Label.Clone());

			toReturn.addCC(bestCC1);
			toReturn.addCC(bestCC2);

			return toReturn;
		}

		/// <summary>
		/// Removes the substroke with the worst NaiveBelief
		/// </summary>
		/// <param name="cc"></param>
		/// <returns></returns>
		private Component updateCCRemoveWorst(ConnectedComponent cc)
		{
			double worst = double.PositiveInfinity;
			double belief;
			Substroke toRemove = (Substroke)cc.Substrokes[0]; //should be null, but cant test it because all belief is 0.0
			for(int i = 0; i < cc.Substrokes.Count; ++i)
			{
				belief = ((Substroke)cc.Substrokes[i]).GetFirstBelief();
				if(belief > 0.0 && belief < worst)
				{
					worst = belief;
					toRemove = (Substroke)cc.Substrokes[i];
				}				
			}

			ConnectedComponent cc1 = cc.Clone();
			ConnectedComponent cc2 = new ConnectedComponent((string)cc.Label.Clone());

			cc1.removeSubstroke(toRemove);
			cc2.addSubstroke(toRemove, 0.9); //Fix this

			Component toReturn = new Component((string)cc.Label.Clone());

			toReturn.addCC(cc1);
			toReturn.addCC(cc2);

			return toReturn;
		}

		
		/// <summary>
		/// Computes the Belief of a Component's ConnectedComponent vs a ConnetedComponent...
		/// </summary>
		/// <param name="c"></param>
		/// <param name="cc"></param>
		/// <returns></returns>
		private double isBetter(Component c, ConnectedComponent cc)
		{
			double[] cBeliefs = c.belief();
			double ccBelief = cc.belief();

			//NEED GOOD FUNCTION HERE...
			double average = 0.0;
			for(int i = 0; i < cBeliefs.Length; ++i)
				average += cBeliefs[i];
			average /= cBeliefs.Length;

			return average - ccBelief;
		}

		
		#endregion

		public void showConnectedComponentAdjacency()
		{
			int length = this.connectedComponents.Count;

			if(this.connectedComponentsAdjacentComponents == null)
				this.calculateCCAdjacentComponents();
			
			for(int i = 0; i < length - 1; ++i)
			{
				for(int j = i + 1; j < length; ++j)
				{
					if(this.connectedComponentsAdjacentComponents[i, j] == 1.0)
					{
						Console.WriteLine(((ConnectedComponent)this.connectedComponents[i]).Label + i + " <-> " + ((ConnectedComponent)this.connectedComponents[j]).Label + j);
					}
				}
			}

		}

	
		/// <summary>
		/// Give each connected componet a random color so it may be viewed
		/// </summary>
		public void randomColorConnectedComponents()
		{
			
			//Give all the connected components random colors so we may view them
			//for(int i = 0; i < this.components.Count; ++i)
			{
				//Component component = (Component)this.components[i];
				//for(int j = 0; j < component.ConnectedComponents.Count; ++j)
				for(int j = 0; j < this.connectedComponents.Count; ++j)	
				{				
					int randomInt = random.Next(int.MinValue, int.MaxValue);

					ConnectedComponent cc = (ConnectedComponent)this.connectedComponents[j];
					for(int k = 0; k < cc.Substrokes.Count; ++k)
					{
						((Substroke)cc.Substrokes[k]).XmlAttrs.Color = randomInt;
					}
				}
			}	
		}

		
		/// <summary>
		/// Write the Segmented version to an xml file
		/// </summary>
		/// <param name="filename"></param>
		public void writeXML(string filename)
		{
			(new ConverterXML.MakeXML(this.sketch)).WriteXML(filename);
		}

		
		public ArrayList ConnectedComponents
		{
			get
			{
				if(this.connectedComponents == null)
					this.extractCC();
				return this.connectedComponents;
			}
		}

		public int numConnectedComponents()
		{
			return this.connectedComponents.Count; 
		}
	}
}
