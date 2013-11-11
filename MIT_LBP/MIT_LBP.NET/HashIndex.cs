/*
* HashIndex.java
*
* Michael Ross
*
* 11/14/01
*
*/
namespace mit.ai.mrf
{
	using System;
	
	/// <summary>Representation of the data for the Markov random field data
	/// structures. 
	/// </summary>
	public interface HashIndex
	{
		/// <summary>Override Object.equals(Object) so HashMap can function correctly. 
		/// </summary>
		//bool equals(System.Object anotherIndex);
		/// <summary>Override Object.hashCode() so HashMap can function correctly. 
		/// </summary>
		//int hashCode();
	}


	// Bullshit HashIndex implementation. I think all I want is the number of labels...?
	public class Labels : HashIndex
	{
		/*bool equals(System.Object anotherIndex)
		{
			return true;
		}

		int hashcode()
		{
			return 0;
		}*/
	}
}