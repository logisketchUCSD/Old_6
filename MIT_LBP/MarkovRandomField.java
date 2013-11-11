/*
 * MarkovRandomField.java
 *
 * Michael Ross
 *
 * 3/18/02
 *
 */

package mit.ai.mrf;

import java.util.*;
import java.io.*;

/** A class for specifying Markov random fields and for finding their
    MAP estimates by belief propagation. */
public class MarkovRandomField implements Serializable
{
    static final long serialVersionUID = 1L;
    private static int MAX_PRODUCT = 0;
    private static int SUM_PRODUCT = 1;
    private MRFNode[] nodes;

    public MarkovRandomField(MRFNode[] initNodes)
    {
	nodes = initNodes;
	return;
    }

    /** Access MRFNodes in the MarkovRandomField. */
    public MRFNode[] getNodes()
    {
	return nodes;
    }

    /** Calculate exact beliefs for a state. */
    public double probability(HashIndex[] vals)
    {
	return configurationValue(vals) / partitionValue();
    }

    /** Calculate the partition function's value for the MRF. */
    public double partitionValue()
    {
	double partition = 0;
	
	int[] assignIndex = new int[nodes.length];
	Arrays.fill(assignIndex, 0);
	HashIndex[] assignment = new HashIndex[nodes.length];

	boolean done = false;
	while (!done)
	{
	    for (int n = 0; n < nodes.length; n++)
	    {
		assignment[n] = nodes[n].getPossibleScenes()[assignIndex[n]];
	    }

	    partition += configurationValue(assignment);

	    boolean flipped = false;
	    for (int n = 0; n < assignIndex.length && !flipped; n++)
	    {
		int maxLength = nodes[n].getPossibleScenes().length;
		if (assignIndex[n] == maxLength - 1)
		{
		    assignIndex[n] = 0;
		}
		else
		{
		    assignIndex[n]++;
		    flipped = true;
		}
	    }

	    if (!flipped)
	    {
		done = true;
	    }
	}

	return partition;
    }

    /** Create an array of all the indexes of the neighbors of each nodes. */
    private int[][] findNeighborIndices()
    {
	int[][] neighInd = new int[nodes.length][];
	for (int n = 0; n < nodes.length; n++)
	{
	    ArrayList neighbors = nodes[n].getNeighbors();
	    neighInd[n] = new int[neighbors.size()];
	    for (int ni = 0; ni < neighbors.size(); ni++)
	    {
		int location = 0;
		while (nodes[location] != neighbors.get(ni))
		{
		    location++;
		}
		neighInd[n][ni] = location;
	    }
	}
	return neighInd;
    }

    /** Use Gibbs sampling to compute a sample marginal distribution for a
	particular node. */
    public double[] computeSampleDistribution(int node, int numSamples)
    {
	if (numSamples > Integer.MAX_VALUE)
	{
	    return null;
	}

	int[] counts = new int[nodes[node].getPossibleScenes().length];
	int[] initAssignment = randomAssignment();
	int remainingSamples = numSamples;
	while (remainingSamples > 0)
	{
	    int takenSamples = Math.min(remainingSamples, 10000);
	    int[][] samples = generateGibbsSampleIndices(initAssignment,
							 takenSamples);
	    for (int s = 0; s < samples.length; s++)
	    {
		counts[samples[s][node]]++;
	    }
	    System.arraycopy(samples[samples.length - 1], 0, initAssignment, 0,
			     nodes.length);
	    remainingSamples -= takenSamples;
	}

	double[] distribution = new double[counts.length];
	for (int s = 0; s < counts.length; s++)
	{
	    distribution[s] = counts[s] / ((double) numSamples);
	}

	return distribution;
    }

    /** Use Gibbs sampling to compute the sample mode for a particular
	node. */
    public HashIndex computeSampleMode(int node, int numSamples)
    {
	double[] distribution = computeSampleDistribution(node, numSamples);
	HashIndex[] possibleScenes = nodes[node].getPossibleScenes();
	HashIndex maxScene = null;
	double maxValue = Double.NEGATIVE_INFINITY; 
	for (int s = 0; s < distribution.length; s++)
	{
	    if (distribution[s] > maxValue)
	    {
		maxScene = possibleScenes[s];
		maxValue = distribution[s];
	    }
	}
	return maxScene;
    }

    /** Convert samples into HashIndex representation. */
    private HashIndex[][] translateSamples(int[][] samples)
    {
	HashIndex[][] samplesHI = new HashIndex[samples.length]
	    [samples[0].length];
	HashIndex[][] possibleScenes = new HashIndex[nodes.length][];
	for (int n = 0; n < nodes.length; n++)
	{
	    possibleScenes[n] = nodes[n].getPossibleScenes();
	}

	for (int i = 0; i < samplesHI.length; i++)
	{
	    for (int j = 0; j < samplesHI[0].length; j++)
	    {
		samplesHI[i][j] = possibleScenes[j][samples[i][j]];
	    }
	}

	return samplesHI;
    }

    /** Generate a series of Gibbs samples, starting from a random initial
	assignment. */
    public HashIndex[][] generateGibbsSamples(int numSamples)
    {
	return translateSamples(generateGibbsSampleIndices(numSamples));
    }

    /** Generate a series of Gibbs samples, starting from a random initial
	assignment, in index format. */
    public int[][] generateGibbsSampleIndices(int numSamples)
    {	
	return generateGibbsSampleIndices(randomAssignment(), numSamples);
    }

    /** Generate a uniformly random assignment to the MRF. */
    public int[] randomAssignment()
    {
	int[] assignment = new int[nodes.length];
	for (int n = 0; n < assignment.length; n++)
	{
	    assignment[n] = (int) (Math.random()
				   * nodes[n].getPossibleScenes().length);
	}
	return assignment;
    }

    /** Generate a series of Gibbs samples, given an initial
	assignment. */
    private int[][] generateGibbsSampleIndices(int[] initAssignment,
					       int numSamples)
    {
	int[][] neighInd = findNeighborIndices();
	int[][] neighValues = new int[nodes.length][];
	for (int n = 0; n < neighValues.length; n++)
	{
	    neighValues[n] = new int[neighInd.length];
	}

	int[] assignment = new int[nodes.length];
	System.arraycopy(initAssignment, 0, assignment, 0,
			 initAssignment.length);
	int[][] samples = new int[numSamples][assignment.length];

	for (int s = 0; s < numSamples; s++)
	{
	    int n = (int) (Math.random() * nodes.length);
	    for (int nn = 0; nn < neighInd[n].length; nn++)
	    {
		neighValues[n][nn] = assignment[neighInd[n][nn]];
	    }
	    int sampleIndex = drawNextGibbsSample(n, neighInd[n],
						  neighValues[n]);
	    assignment[n] = sampleIndex;
	    System.arraycopy(assignment, 0, samples[s], 0,
			     assignment.length);
	}

	return samples;
    }

    /** Return the index of the next Gibbs sample at a particular node n,
	with a given set of neighbor indices and values. */
    private int drawNextGibbsSample(int n, int[] neighbors, int[] neighValues)
    {
	int numValues = nodes[n].getPossibleScenes().length;
	double[] distribution = new double[numValues];
	double norm = 0;
	
	for (int v = 0; v < numValues; v++)
	{
	    distribution[v] = nodes[n].localMatch(v);
	    for (int nn = 0; nn < neighbors.length; nn++)
	    {
		distribution[v] *= nodes[n].neighborMatch
		    (nodes[neighbors[nn]], v, neighValues[nn]);
	    }
	    norm += distribution[v];
	}
	
	for (int v = 0; v < numValues; v++)
	{
	    distribution[v] /= norm;
	}

	double uniform = Math.random();
	double sum = 0;
	for (int v = 0; v < numValues; v++)
	{
	    sum += distribution[v];
	    if (sum > uniform)
	    {
		return v;
	    }
	}

	return numValues - 1;
    }

    /** Log of the configuration value. */
    public double logConfigurationValue(HashIndex[] values)
    {
	int[] assignment = new int[nodes.length];
	Arrays.fill(assignment, -1);
	for (int n = 0; n < nodes.length; n++)
	{
	    HashIndex[] scenes = nodes[n].getPossibleScenes();
	    for (int s = 0; s < scenes.length; s++)
	    {
		if (scenes[s] == values[n])
		{
		    assignment[n] = s;
		}
	    }
	}

	int[][] neighInd = findNeighborIndices();

	double sum = 0;
	for (int n = 0; n < nodes.length; n++)
	{
	    if (Boolean.getBoolean("mit.ai.mrf.MarkovRandomField.debug"))
	    {
		System.out.print(n + " " + nodes[n].localMatch(assignment[n]));
	    }
	    sum += Math.log(nodes[n].localMatch(assignment[n]));
	    ArrayList neighbors = nodes[n].getNeighbors();
	    for (int ni = 0; ni < neighbors.size(); ni++)
	    {
		MRFNode neigh = (MRFNode) neighbors.get(ni);

		if (n > neighInd[n][ni])
		{
		    if (Boolean.getBoolean
			("mit.ai.mrf.MarkovRandomField.debug"))
		    {
			System.out.print(" " + nodes[n].neighborMatch
					 (neigh, assignment[n],
					  assignment[neighInd[n][ni]]));
		    }
		    sum += Math.log(nodes[n].neighborMatch
				    (neigh, assignment[n],
				     assignment[neighInd[n][ni]]));
		}
	    }
	    if (Boolean.getBoolean("mit.ai.mrf.MarkovRandomField.debug"))
	    {
		System.out.println();
	    }
	}

	return sum;
    }

    /** Configuration value should be divided by partition function
	to produce probability. */
    public double configurationValue(HashIndex[] values)
    {
	return Math.exp(logConfigurationValue(values));
    }

    public HashIndex[] icm(HashIndex[] startValues)
    {
	return icm(startValues, -1);
    }

    /** Improve the configuration through the iterative conditional modes
	method. */
    public HashIndex[] icm(HashIndex[] startValues, int maxFlips)
    {
	int[][] neighInd = new int[nodes.length][];
	for (int n = 0; n < nodes.length; n++)
	{
	    ArrayList neighbors = nodes[n].getNeighbors();
	    neighInd[n] = new int[neighbors.size()];
	    for (int ni = 0; ni < neighbors.size(); ni++)
	    {
		int location = 0;
		while (nodes[location] != neighbors.get(ni))
		{
		    location++;
		}
		neighInd[n][ni] = location;
	    }
	}

	HashIndex[] icmValues = new HashIndex[startValues.length];
	System.arraycopy(startValues, 0, icmValues, 0, startValues.length);

	int[] icmIndices = new int[icmValues.length];

	for (int i = 0; i < icmIndices.length; i++)
	{
	    HashIndex[] vals = nodes[i].getPossibleScenes();
	   
	    for (int v = 0; v < vals.length; v++)
	    {
		if (vals[v] == icmValues[i])
		{    
		    icmIndices[i] = v;
		    break;
		}
	    }
	}
	
	boolean change = true;
	double bestScore = logConfigurationValue(icmValues);
	int flips = 0;
	while (change && (maxFlips == -1 || flips < maxFlips))
	{
	    change = false;

	    for (int i = 0; i < icmValues.length; i++)
	    {
		HashIndex[] possibleValues = nodes[i].getPossibleScenes();
		ArrayList neighbors = nodes[i].getNeighbors();
		for (int v = 0; v < possibleValues.length; v++)
		{
		    double changeScore = bestScore
			- Math.log(nodes[i].localMatch(icmIndices[i]))
			+ Math.log(nodes[i].localMatch(v));

		    for (int n = 0; n < neighInd[i].length; n++)
		    {
			MRFNode neigh = (MRFNode) neighbors.get(n);
			changeScore = changeScore
			    - Math.log(nodes[i].neighborMatch
				       (neigh, icmIndices[i],
					icmIndices[neighInd[i][n]]))
			    + Math.log(nodes[i].neighborMatch
				       (neigh, v,
					icmIndices[neighInd[i][n]]));
		    }

		    if (changeScore > bestScore && icmIndices[i] != v)
		    {			
			if (Boolean.getBoolean
			    ("mit.ai.mrf.MarkovRandomField.debug"))
			{
			    System.out.println(icmValues[i] + " (" + bestScore
					       + ") -> " + possibleValues[v]
					       + "( " + changeScore + ")");
			}

			bestScore = changeScore;
			icmValues[i] = possibleValues[v];
			icmIndices[i] = v;
			change = true;
			flips++;
		    }
		}
	    }
	}

	return icmValues;
    }

    /** Scan the nodes, arbitrarily break the first MAP estimate tie,
	fixing the node to the first tied value. Return index if a tie
	is found and broken, return -1 if no ties found to be
	broken. */
    public int breakFirstTie()
    {
	for (int i = 0; i < nodes.length; i++)
	{
	    if (nodes[i].existMAPTie())
	    {
		if (Boolean.getBoolean("mit.ai.mrf.MarkovRandomField.debug"))
		{
		    System.out.println("Tie broken at " + i);

		    System.out.println("Possible choices:");
	       
		    int mapInd = nodes[i].mapEstimateIndex();
		    HashIndex[] possibleScenes = nodes[i].getPossibleScenes();
		    double[] beliefs = nodes[i].beliefs();
		    for (int p = 0; p < possibleScenes.length; p++)
		    {
			if (beliefs[p] == beliefs[mapInd])
			{
			    System.out.println(possibleScenes[p]);
			}
		    }
		    
		    System.out.println("Selected:");
		}
		
		nodes[i].setMAPValue();

		if (Boolean.getBoolean("mit.ai.mrf.MarkovRandomField.debug"))
		{
		    System.out.println(nodes[i].mapEstimate());
		}

		return i;
	    }
	}

	return -1;
    }

    /** Unfix any fixed nodes. */
    public void unsetValues()
    {
	for (int i = 0; i < nodes.length; i++)
	{
	    nodes[i].unsetValue();
	}

	return;
    }

    /** Reset belief propagation messages to starting state. */
    public void resetMessages()
    {
	for (int i = 0; i < nodes.length; i++)
	{
	    nodes[i].resetMessages();
	}
	
	return;
    }

    /** Use belief propagation to infer the beliefs for the given node. */
    public double[] bpInferBeliefs(int nodeindex, int iterations)
    {
	resetMessages();
	passSPMessages(iterations);
	return nodes[nodeindex].beliefs();
    }
    
    /** Use belief propagation to infer the beliefs for the given pair
     * of neighboring nodes. */
    public double[][] bpInferBelifs(int nodeindexA, int nodeindexB,
				    int iterations)
    {
	resetMessages();
	passSPMessages(iterations);
	return MRFNode.beliefs(nodes[nodeindexA], nodes[nodeindexB]);
    }

    public double[][] beliefs()
    {
	double[][] out = new double[nodes.length][];

	for (int i = 0; i < nodes.length; i++)
	{
	    out[i] = nodes[i].beliefs();
	}

	return out;
    }

    public int[] mapEstimateIndices()
    {
	int[] out = new int[nodes.length];
	for (int i = 0; i < nodes.length; i++)
	{
	    out[i] = nodes[i].mapEstimateIndex();
	}
	return out;
    }

    public HashIndex[] mapEstimate()
    {
	HashIndex[] out = new HashIndex[nodes.length];
	for (int i = 0; i < nodes.length; i++)
	{
	    out[i] = nodes[i].mapEstimate();
	}
	return out;
    }

    /** Sum-product belief propagation until convergence. */
    public boolean sumProductBeliefPropagate(int maxIter)
    {
	return passMessages(SUM_PRODUCT, maxIter, true, 0);
    }

    /** Max-product belief propagation until convergence. */
    public boolean maxProductBeliefPropagate(int maxIter)
    {
	return passMessages(MAX_PRODUCT, maxIter, true, 0);
    }

    /** Perform sum-product belief propagation among the nodes. */
    public void passSPMessages(int iterations)
    {
	passMessages(SUM_PRODUCT, iterations);
	return;
    }

    /** Perform belief propagation among the nodes. */
    public void passMPMessages(int iterations)
    {
	passMessages(MAX_PRODUCT, iterations);
	return;
    }

    /** BP until convergence. If there is a tie, break it and
	rerun. If convergence fails, fix a node and rerun. */
    public void maxProductBeliefPropagateFix(int iterations)
    {
	boolean converged = false;
	int tieIndex = 0;
	//int fixIndex = 0;
	int[] fixIndices = new int[nodes.length];
	double maxlogval = Double.NEGATIVE_INFINITY;
	int[] maxmapind = null;
	while ((tieIndex != -1 || !converged) /*&& fixIndex < nodes.length*/)
	{
	    resetMessages();
	    converged = maxProductBeliefPropagate(iterations);
	    
	    double logval = logConfigurationValue(mapEstimate());

	    if (logval < maxlogval)
	    {
		/*
		nodes[fixIndex].unsetValue();
		fixIndex++;
		*/
		for (int i = 0; i < fixIndices.length && fixIndices[i] != -1;
		     i++)
		{
		    nodes[fixIndices[i]].unsetValue();
		}
	    }
	    else
	    {
		maxlogval = logval;
		int[] newindices = mapEstimateIndices();
		
		if (!Arrays.equals(maxmapind, newindices))
		{
		    maxmapind = newindices;		    
		}
	    }

	    tieIndex = breakFirstTie();
	    System.out.println("converged: " + converged + " tie: "
			       + tieIndex + " score: "
			       + logval);

	    if (tieIndex == -1 && !converged)
	    {
		int loc = 0;
		for (int i = 0; i < nodes.length; i++)
		{
		    if (Math.random() < 0.10 && !nodes[i].isSet())
		    {
			fixIndices[loc] = i;
		    
			System.out.println("setting: " + fixIndices[loc]);
			
			if (fixIndices[loc] == 0)
			{
			    nodes[fixIndices[loc]].setValue
				(maxmapind[fixIndices[loc]]);
			}
			else
			{
			    int[] vals = new int[2];
			    vals[0] = (maxmapind[fixIndices[loc]] / 2) * 2;
			    vals[1] = (maxmapind[fixIndices[loc]] / 2) * 2 + 1;
			    nodes[fixIndices[loc]].setValues(vals);
			}

			loc++;
		    }
		}
		fixIndices[loc] = -1;

		/*
		for ( ; fixIndex < nodes.length; fixIndex++)
		{
		    if (!nodes[fixIndex].isSet())
		    {
			if (fixIndex == 0)
			{
			    nodes[fixIndex].setValue(maxmapind[fixIndex]);
			}
			else
			{
			    int[] vals = new int[2];
			    vals[0] = (maxmapind[fixIndex] / 2) * 2;
			    vals[1] = (maxmapind[fixIndex] / 2) * 2 + 1;
			    nodes[fixIndex].setValues(vals);
			}

			System.out.println("fixed: " + fixIndex);
			break;
		    }
		}
		*/

		/*
		double maxval = Double.NEGATIVE_INFINITY;
		int maxind = -1;
		int[] mapind = mapEstimateIndices();
		for (int i = 0; i < nodes.length; i++)
		{
		    double val = 0;
		    if (!nodes[i].isSet()
			&& (val = nodes[i].beliefs()[mapind[i]]) > maxval)
		    {
			maxind = i;
			maxval = val;
		    }
		}

		nodes[maxind].setMAPValue();
		System.out.println("fixed: " + maxind);
		fixIndex = maxind;
		*/
	    }
	}
	return;
    }

    /** Repeatedly break belief propagation ties and rerun
	the inference until no ties remain. */
    public void maxProductBeliefPropagateTieBreak(int iterations)
    {
	int tieIndex = 0;
	while (tieIndex != -1)
	{
	    resetMessages();
	    maxProductBeliefPropagate(iterations);
	    tieIndex = breakFirstTie();
	}
	return;
    }

    /** Perform specified iterations of max-product belief
	propagation, break ties, and repeat until no ties remain. */
    public void passMPMessagesTieBreak(int iterations)
    {
	int tieIndex = 0;
	while (tieIndex != -1)
	{
	    resetMessages();
	    passMPMessages(iterations);
	    tieIndex = breakFirstTie();
	}
	return;
    }

    /** Repeatedly break belief propagation ties and rerun the
	inference until no ties remain. Then continue to fix untied
	nodes and rerun inference. */
    public void maxProductBeliefPropagateTieBreakFixAll(int iterations)
    {
	maxProductBeliefPropagateTieBreak(iterations);

	for (int i = 0; i < nodes.length; i++)
	{
	    if (!nodes[i].isSet())
	    {
		nodes[i].setMAPValue();
		resetMessages();
		maxProductBeliefPropagate(iterations);
	    }
	}

	return;
    }

    /** Perform specified iterations of max-product belief
	propagation, break ties, and repeat until no ties remain. Then
	fix all nodes and rerun BP after each fix. */
    public void passMPMessagesTieBreakFixAll(int iterations)
    {
	passMPMessagesTieBreak(iterations);

	for (int i = 0; i < nodes.length; i++)
	{
	    if (!nodes[i].isSet())
	    {
		nodes[i].setMAPValue();
		resetMessages();
		passMPMessages(iterations);
	    }
	}

	return;
    }

    /** Perform belief propagation for a fixed number of iterations. */
    public void passMessages(final int bpType, final int iterations)
    {
	passMessages(bpType, iterations, false, 0);
    }

    /** Perform belief propagation. */
    public boolean passMessages(final int bpType, final int iterations,
				final boolean testConvergence,
				final double convergenceLimit)
    {
	return passMessages(bpType, iterations, testConvergence,
			    convergenceLimit, 1);
    }

    /** Perform belief propagation. */
    public boolean passMessages(final int bpType, final int iterations,
				final boolean testConvergence,
				final double convergenceLimit,
				final double stepSize)
    {	
	int numBPThreads = Integer.getInteger
	    ("mit.ai.mrf.MarkovRandomField.numBPThreads", 2).intValue();
	final boolean[] finished = new boolean[numBPThreads];
	final boolean[] converged = new boolean[numBPThreads];
	final double[] delta = new double[numBPThreads];

	class BeliefPropagationThread extends Thread
	{
	    private int which;
	    private int mod;

	    public BeliefPropagationThread(int which, int mod)
	    {
		super("BeliefPropagationThread-" + which + " of " + mod);
		this.which = which;
		this.mod = mod;
		return;
	    }
	    
	    public void run()
	    {
		for (int iter = 0; iter < iterations && !converged[which];
		     iter++)
		{
		    if (which == 0 && Boolean.getBoolean
			("mit.ai.mrf.MarkovRandomField.debug"))
		    {
			System.out.println("BP iteration " + iter);
		    }
		    
		    for (int i = which; i < nodes.length; i += mod)
		    {
			if (bpType == MAX_PRODUCT)
			{
			    nodes[i].passMPMessages();
			}
			else if (bpType == SUM_PRODUCT)
			{
			    nodes[i].passSPMessages();
			}
			else
			{
			    throw new UnsupportedOperationException();
			}
		    }

		    synchronized (finished)
		    {
			finished[which] = true;
			boolean allFinished = true;
			for (int t = 0; t < finished.length; t++)
			{
			    if (!finished[t])
			    {
				allFinished = false;
				break;
			    }
			}

			if (allFinished)
			{
			    finished.notifyAll();
			    Arrays.fill(finished, false);
			}
			else
			{
			    try
			    {
				finished.wait();
			    }
			    catch (InterruptedException x)
			    {
				throw new RuntimeException
				    ("Error in BP thread.");
			    }
			}
		    }

		    for (int i = which; i < nodes.length; i += mod)
		    {
			if (stepSize == 1)
			{
			    nodes[i].update();
			}
			else
			{
			    nodes[i].update(stepSize);
			}
		    }
		    
		    for (int i = which; i < nodes.length; i += mod)
		    {
			nodes[i].rescale();
		    }

		    if (testConvergence)
		    {
			delta[which] = 0;
			converged[which] = true;
			for (int i = which; i < nodes.length; i += mod)
			{
			    delta[which] += nodes[i].delta();
			    converged[which] = converged[which]
				&& delta[which] <= convergenceLimit;
			}
		    }

		    synchronized (finished)
		    {
			finished[which] = true;
			boolean allFinished = true;
			for (int t = 0; t < finished.length; t++)
			{
			    if (!finished[t])
			    {
				allFinished = false;
				break;
			    }
			}

			if (allFinished)
			{
			    if (testConvergence && Boolean.getBoolean
				("mit.ai.mrf.MarkovRandomField.debug"))
			    {
				double deltaSum = 0;
				for (int t = 0; t < delta.length; t++)
				{
				    deltaSum += delta[t];
				}

				System.out.println("Delta: " + deltaSum);
			    }

			    for (int t = 0; t < converged.length; t++)
			    {
				if (!converged[t])
				{
				    Arrays.fill(converged, false);
				    break;
				}
			    }			    
			    finished.notifyAll();
			    Arrays.fill(finished, false);
			}
			else
			{
			    try
			    {
				finished.wait();
			    }
			    catch (InterruptedException x)
			    {
				throw new RuntimeException
				    ("Error in BP thread.");
			    }
			}
		    }
		}
	    }
	}

	BeliefPropagationThread[] threads =
	    new BeliefPropagationThread[numBPThreads];
	for (int t = 0; t < threads.length; t++)
	{
	    threads[t] = new BeliefPropagationThread(t, threads.length);
	    threads[t].start();
	}

	try
	{
	    for (int t = 0; t < threads.length; t++)
	    {
		threads[t].join();
	    }
	}
	catch (InterruptedException x)
	{
	    throw new RuntimeException("Thread error in Possible Scenes.");
	}

	if (testConvergence)
	{
	    for (int t = 0; t < converged.length; t++)
	    {
		if (!converged[t])
		{
		    return false;
		}
	    }

	    return true;
	}
	
	return false;
    }
}
