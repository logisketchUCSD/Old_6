/*
 * MRFNode.java
 *
 * Michael Ross
 *
 * 4/5/02
 *
 */

package mit.ai.mrf;

import java.io.*;
import java.util.*;

/** Element of a Markov random field, holding neighborhood information. */
public class MRFNode implements Serializable
{
    static final long serialVersionUID = 7622821961596096962L;
    protected static final int MAX_PRODUCT = 0;
    protected static final int SUM_PRODUCT = 1;
    protected HashIndex[] possibleScenes; /** Scenes to consider. */
    protected double[] localCompats; /** Local data compatibility. */
    protected ArrayList neighbors; /** Neighbors. */
    protected ArrayList selfIndices; /** Who am I to them? */
    protected ArrayList msgs; /** Messages from neighbors. */
    protected ArrayList newMsgs; /** Storage for updated messages. */
    protected HashMap neighCompats; /** Hammersley-Clifford compatibilities. */
    protected double[][] neighMsgs; /** A cache used in BP. */
    protected double[] fullLocalCompats; /** Storage used in fixing. */
    protected boolean set; /** Indicate if this node's value has been fixed. */
    private ArrayList neighCompatScratch;

    /** Construct a new dataless node with no set scenes or compats. */
    public MRFNode()
    {
	neighCompats = new HashMap();
	neighbors = new ArrayList();
	selfIndices = new ArrayList();
	msgs = new ArrayList();
	newMsgs = new ArrayList();
	neighMsgs = new double[0][];
	set = false;
	neighCompatScratch = new ArrayList();
	return;
    }

    /** Construct a new node with scenes, but default compats. */
    public MRFNode(HashIndex[] initScenes)
    {
	this();
	setPossibleScenes(initScenes);
    }

    /** Construct a new dataless node with preset localCompats. */
    public MRFNode(HashIndex[] initScenes, double[] initLocalCompats)
    {
	this();
	setPossibleScenes(initScenes, initLocalCompats);
	return;
    }

    /** Return access to the messages list. */
    public ArrayList getMessages()
    {
	return msgs;
    }

    /** Return access to the neighbor list. */
    public ArrayList getNeighbors()
    {
	return neighbors;
    }

    /** Return array of possible scenes. */
    public HashIndex[] getPossibleScenes()
    {
	return possibleScenes;
    }

    /** Detect if there are currently tied MAP estimate values. */
    public boolean existMAPTie()
    {
	double mapval = Double.NEGATIVE_INFINITY;
	boolean ties = false;

	for (int arg = 0; arg < possibleScenes.length; arg++)
	{
	    double curmapval = localCompats[arg];

	    for (int m = 0; m < msgs.size(); m++)
	    {
		curmapval *= ((double[]) msgs.get(m))[arg];
	    }

	    if (curmapval > mapval)
	    {
		mapval = curmapval;
		ties = false;
	    }
	    else if (curmapval == mapval)
	    {
		ties = true;
	    }	    
	}
	return ties;
    }

    /** Check if value is set. */
    public boolean isSet()
    {
	return set;
    }

    /** Fix the node to the current MAP estimate (uses the first max
	valued estimate discovered in case of a tie). */
    public void setMAPValue()
    {
	setValue(mapEstimateIndex());
	return;
    }

    /** Restore the node to it's original values and compatibilities. */
    public void unsetValue()
    {
	if (set)
	{
	    localCompats = fullLocalCompats;
	}
	set = false;
	return;
    }

    /** Fix the node to a particular subset of values. */
    public void setValues(int[] indices)
    {
	if (set)
	{
	    unsetValue();
	}
	set = true;
	fullLocalCompats = localCompats;
	localCompats = new double[possibleScenes.length];
	
	for (int i = 0; i < indices.length; i++)
	{
	    localCompats[indices[i]] = fullLocalCompats[indices[i]];
	}

	return;
    }

    /** Fix the node to a particular value. */
    public void setValue(int index)
    {
	if (set)
	{
	    unsetValue();
	}
	set = true;
	fullLocalCompats = localCompats;
	localCompats = new double[possibleScenes.length];
	localCompats[index] = fullLocalCompats[index];

	return;
    }

    /** Add a neighbor. */
    public void addNeighbor(MRFNode newNeighbor)
    {
	if (!neighbors.contains(newNeighbor))
	{
	    neighbors.add(newNeighbor);
	    double[] nmsgs = new double[possibleScenes.length];
	    double[] nnewMsgs = new double[possibleScenes.length];
	    Arrays.fill(nmsgs, 1);
	    Arrays.fill(nnewMsgs, 1);
	    msgs.add(nmsgs);
	    newMsgs.add(nnewMsgs);

	    newNeighbor.neighbors.add(this);
	    double[] n2msgs = new double[newNeighbor.possibleScenes.length];
	    double[] n2newmsgs = new double[newNeighbor.possibleScenes.length];
	    Arrays.fill(n2msgs, 1);
	    Arrays.fill(n2newmsgs, 1);
	    newNeighbor.msgs.add(n2msgs);
	    newNeighbor.newMsgs.add(n2newmsgs);

	    selfIndices.add(new Integer(newNeighbor.neighbors.size() - 1));
	    newNeighbor.selfIndices.add(new Integer(neighbors.size() - 1));

	    neighCompatScratch.add
		(new double[newNeighbor.possibleScenes.length]);
	    newNeighbor.neighCompatScratch.add
		(new double[possibleScenes.length]);

	    double[][] compats = new double[possibleScenes.length]
		[newNeighbor.possibleScenes.length];
	    for (int i = 0; i < compats.length; i++)
	    {
		Arrays.fill(compats[i], 1);
	    }
	    neighCompats.put(newNeighbor, compats);
	}

	return;
    }

    /** Remove a neighbor. */
    public void removeNeighbor(MRFNode neigh)
    {
	if (neighbors.contains(neigh))
	{
	    int nind = neighbors.indexOf(neigh);
	    neighbors.remove(nind);
	    selfIndices.remove(nind);
	    msgs.remove(nind);
	    newMsgs.remove(nind);

	    int rind = neigh.neighbors.indexOf(this);
	    neigh.neighbors.remove(rind);
	    neigh.selfIndices.remove(rind);
	    neigh.msgs.remove(rind);
	    neigh.newMsgs.remove(rind);
	 
	    if (neighCompats.containsKey(neigh))
	    {
		neighCompats.remove(neigh);
	    }
	    else
	    {
		neigh.neighCompats.remove(this);
	    }
	}

	return;
    }

    /** Reset all messages to 1. */
    public void resetMessages()
    {
	for (int i = 0; i < neighbors.size(); i++)
	{
	    Arrays.fill((double[]) msgs.get(i), 1);
	    Arrays.fill((double[]) newMsgs.get(i), 1);
	}
	return;
    }

    /** Return true if this node neighbors the argument node. */
    public boolean isNeighbor(MRFNode potentialNeighbor)
    {
	return neighbors.contains(potentialNeighbor);
    }
    
    /** Find the index of a particular neighbor. */
    public int neighborIndex(MRFNode neighbor)
    {
	return neighbors.indexOf(neighbor);
    }

    /** Return the beliefs (approximate pdf) for a pair of neighboring
	nodes. */
    public static double[][] beliefs(MRFNode nodeA, MRFNode nodeB)
    {
	if (!nodeA.isNeighbor(nodeB))
	{
	    throw new IllegalArgumentException("Nodes are not neighbors.");
	}
	int ani = nodeB.neighborIndex(nodeA);
	int bni = nodeA.neighborIndex(nodeB);
	double norm = 0;
	double[][] bvals = new double[nodeA.possibleScenes.length]
	    [nodeB.possibleScenes.length];
	for (int a = 0; a < bvals.length; a++)
	{
	    for (int b = 0; b < bvals[0].length; b++)
	    {
		bvals[a][b] = nodeA.localCompats[a] * nodeB.localCompats[b]
		    * nodeA.neighborMatch(nodeB, a, b);

		for (int m = 0; m < nodeA.msgs.size(); m++)
		{
		    if (m != bni)
		    {
			bvals[a][b] *= ((double[]) nodeA.msgs.get(m))[a];
		    }
		}
		for (int m = 0; m < nodeB.msgs.size(); m++)
		{
		    if (m != ani)
		    {
			bvals[a][b] *= ((double[]) nodeB.msgs.get(m))[b];
		    }
		}
		norm += bvals[a][b];
	    }
	}

	for (int a = 0; a < bvals.length; a++)
	{
	    for (int b = 0; b < bvals[0].length; b++)
	    {
		bvals[a][b] /= norm;
	    }
	}

	return bvals;
    }

    /** Return the beliefs (approximate pdf). */
    public double[] beliefs()
    {
	double[] bvals = new double[possibleScenes.length];
	double norm = 0;
	for (int arg = 0; arg < bvals.length; arg++)
	{
	    bvals[arg] = localCompats[arg];

	    for (int m = 0; m < msgs.size(); m++)
	    {
		bvals[arg] *= ((double[]) msgs.get(m))[arg]; 
	    }

	    norm += bvals[arg];
	}
	for (int arg = 0; arg < bvals.length; arg++)
	{
	    bvals[arg] /= norm;
	}
	return bvals;
    }

    /** Compute the index of the MAP scene estimate. */
    public int mapEstimateIndex()
    {
	double mapval = Double.NEGATIVE_INFINITY;
	int mapArg = 0;
	for (int arg = 0; arg < possibleScenes.length; arg++)
	{
	    double curmapval = localCompats[arg];

	    for (int m = 0; m < msgs.size(); m++)
	    {
		curmapval *= ((double[]) msgs.get(m))[arg];
	    }

	    if (curmapval > mapval)
	    {
		mapval = curmapval;
		mapArg = arg;
	    }
	}
	return mapArg;
    }

    /** Compute the MAP scene estimate. */
    public HashIndex mapEstimate()
    {
	return getPossibleScenes()[mapEstimateIndex()];
    }

    /** Update with a linear combination of the old and new messages. */
    public void update(double step)
    {
	if (step > 1) step = 1;
	if (step < 0) step = 0;

	int numNeighbors = msgs.size();

	for (int i = 0; i < numNeighbors; i++)
	{
	    double[] vals = (double[]) msgs.get(i);
	    double[] nvals = (double[]) newMsgs.get(i);

	    for (int v = 0; v < vals.length; v++)
	    {
		nvals[v] = Math.exp(step * Math.log(nvals[v])
				    + (1 - step) * Math.log(vals[v]));
	    }
	}

	update();

	return;
    }

    /** Swap the updated messages with the old ones. */
    public void update()
    {
	ArrayList tempMsgs = msgs;
	msgs = newMsgs;
	newMsgs = tempMsgs;
	return;
    }

    /** Check convergence by comparing the two sets of messages. */
    public double delta()
    {
	double diff = 0;
	for (int m = 0; m < msgs.size(); m++)
	{
	    double[] newMessage = (double[]) msgs.get(m);
	    double[] oldMessage = (double[]) newMsgs.get(m);
		    
	    for (int e = 0; e < newMessage.length; e++)
	    {
		diff += Math.abs(newMessage[e] - oldMessage[e]);
	    }
	}
	return diff;
    }

    /** Rescale the messages to avoid precision problems. */
    public void rescale()
    {
	int numNeighbors = msgs.size();
	for (int i = 0; i < numNeighbors; i++)
	{
	    double[] messages = (double[]) msgs.get(i);
	    double max = Double.NEGATIVE_INFINITY;
	    for (int m = 0; m < messages.length; m++)
	    {
		if (!Double.isNaN(messages[m])
		    && !Double.isInfinite(messages[m]))
		{
		    max = Math.max(max, messages[m]);
		}
		else
		{
		    throw new ArithmeticException("NaN/Inf detected");
		}
	    }
	    if (max != 0)
	    {
		for (int m = 0; m < messages.length; m++)
		{
		    messages[m] /= max;
		}
	    }
	    else
	    {
		throw new ArithmeticException("Zero scale detected");
	    }
	}
	
	return;
    }

    /** Iterate sum-product belief propagation for this node. */
    public void passSPMessages()
    {
	passMessages(SUM_PRODUCT);
	return;
    }

    /** Iterate max-product belief propagation for this node. */
    public void passMPMessages()
    {
	passMessages(MAX_PRODUCT);
	return;
    }

    /** Iterate belief propagation for this node. */
    protected void passMessages(final int bpType)
    {
	int neighborsSize = neighbors.size();
	for (int neighInd = 0; neighInd < neighborsSize; neighInd++)
	{
	    MRFNode neighNode = (MRFNode) neighbors.get(neighInd);
	    int selfInd = ((Integer) selfIndices.get(neighInd)).intValue();

	    neighMsgs = (double[][]) neighNode.msgs.toArray(neighMsgs);

	    boolean revCompat = false;
	    double[][] ncompats = (double[][]) neighCompats.get(neighNode);

	    double[] ncMult = (double[]) neighCompatScratch.get(neighInd);
	    for (int ncand = 0; ncand < neighNode.possibleScenes.length;
		 ncand++)
	    {
		ncMult[ncand] = neighNode.localCompats[ncand];
		if (ncMult[ncand] != 0)
		{
		    for (int m = 0; m < neighMsgs.length
			     && neighMsgs[m] != null; m++)
		    {
			if (m != selfInd)
			{
			    ncMult[ncand] *= neighMsgs[m][ncand];
			}
		    }
		}
	    }

	    if (ncompats == null)
	    {
		ncompats = (double[][]) neighNode.neighCompats.get(this);
		revCompat = true;
	    }
	    double[] newMsgsNeigh = (double[]) newMsgs.get(neighInd);

	    for (int cand = 0; cand < possibleScenes.length; cand++)
	    {	
		double maxVal = Double.NEGATIVE_INFINITY;
		double sumVal = 0;

		for (int neighCand = 0;
		     neighCand < neighNode.possibleScenes.length; neighCand++)
		{
		    double curVal = ncMult[neighCand];
		    
		    if (!revCompat)
		    {
			curVal *= ncompats[cand][neighCand];
		    }
		    else
		    {
			curVal *= ncompats[neighCand][cand];
		    }

		    if (bpType == MAX_PRODUCT && curVal > maxVal)
		    {
			maxVal = curVal;
		    }
		    else if (bpType == SUM_PRODUCT)
		    {
			sumVal += curVal;
		    }
		}
		
		if (bpType == MAX_PRODUCT)
		{
		    newMsgsNeigh[cand] = maxVal;
		}
		else if (bpType == SUM_PRODUCT)
		{
		    newMsgsNeigh[cand] = sumVal;
		}
		else
		{
		    throw new IllegalArgumentException("bpType: " + bpType
						       + " is unknown.");
		}
	    }
	}

	return;
    }
  
    /** Represent the data for this node as a String. */
    public String toString()
    {
	String out = new String();
	out += "Node:\n";
	HashIndex[] scenes = getPossibleScenes();
	double[] compats = getLocalCompatibilities();
	for (int i = 0; i < scenes.length; i++)
	{
	    out += "[" + i + "]\t" + scenes[i] + " localCompat: "
		+ compats[i] + "\n";
	}

	return out;
    }

    public double localMatch(int val)
    {
	return localCompats[val];
    }
    
    public double[] getLocalCompatibilities()
    {
	return localCompats;
    }

    public double[][] getNeighborCompatibilities(MRFNode neigh)
    {
	double[][] compatMatrix = new double[getPossibleScenes().length]
	    [neigh.getPossibleScenes().length];
	
	for (int i = 0; i < getPossibleScenes().length; i++)
	{
	    for (int j = 0; j < neigh.getPossibleScenes().length; j++)
	    {
		compatMatrix[i][j] = neighborMatch(neigh, i, j);
	    }
	}

	return compatMatrix;
    }

    public double neighborMatch(MRFNode neigh, int thisVal, int neighVal)
    {
	double[][] compat = (double[][]) neighCompats.get(neigh);

	if (compat == null)
	{
	    compat = (double[][]) neigh.neighCompats.get(this);
	    if (compat == null)
	    {
		throw new IllegalArgumentException
		    ("These nodes are not neighbors.");
	    }
	    else
	    {
		return compat[neighVal][thisVal];
	    }
	}
	else
	{
	    return compat[thisVal][neighVal];
	}
    }

    public void setNeighborMatch(MRFNode neigh, int thisVal, int neighVal,
				 double matchval)
    {
	double[][] compat = (double[][]) neighCompats.get(neigh);
	
	if (compat == null)
	{
	    compat = (double[][]) neigh.neighCompats.get(this);
	    if (compat == null)
	    {
		throw new IllegalArgumentException
		    ("These nodes are not neighbors.");
	    }
	    else
	    {
		compat[neighVal][thisVal] = matchval;
	    }
	}
	else
	{
	    compat[thisVal][neighVal] = matchval;
	}

	return;
    }

    public void setLocalMatch(int thisVal, double matchVal)
    {
	localCompats[thisVal] = matchVal;
	return;
    }

    public void setLocalMatches(double[] matchValues)
    {
	localCompats = matchValues;
	fullLocalCompats = localCompats;
	return;
    }

    public void setPossibleScenes(HashIndex[] initPossibleScenes)
    {
	possibleScenes = initPossibleScenes;
	localCompats = new double[possibleScenes.length];
	fullLocalCompats = localCompats;
	Arrays.fill(localCompats, 1);
	return;
    }

    public void setPossibleScenes(HashIndex[] initPossibleScenes,
				  double[] matchValues)
    {
	possibleScenes = initPossibleScenes;
	localCompats = matchValues;
	fullLocalCompats = localCompats;
	return;
    }
}
