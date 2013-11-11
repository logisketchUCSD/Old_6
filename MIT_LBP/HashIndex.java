/*
 * HashIndex.java
 *
 * Michael Ross
 *
 * 11/14/01
 *
 */

package mit.ai.mrf;

import java.awt.image.BufferedImage;

/** Representation of the data for the Markov random field data
    structures. */
public interface HashIndex
{
    /** Override Object.equals(Object) so HashMap can function correctly. */
    public boolean equals(Object anotherIndex);
    /** Override Object.hashCode() so HashMap can function correctly. */
    public int hashCode();
    /** Create a buffered image representing the value. */
    public BufferedImage toBufferedImage();
}
