using Sketch;

namespace OldRecognizers
{
    /// <summary>
    /// The interface that all Recognizers should implement
    /// </summary>
    public interface IRecognizer
    {
        /// <summary>
        /// Recognize a list of substrokes
        /// </summary>
        /// <param name="substrokes">Substrokes to recognize</param>
        /// <returns>The results of recognition</returns>
        Results Recognize(Substroke[] substrokes);
    }
}
