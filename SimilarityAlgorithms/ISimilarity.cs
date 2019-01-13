namespace Clustering.DistanceAlgorithms
{
    /// <summary>
    /// Interface for classes that provide a method to calculate
    /// a distance between two elements
    /// </summary>
    public interface ISimilarity
    {
        /// <summary>
        /// Gets the name of the metric
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Gets the similarity (distance or correlation) between the two vectors
        /// </summary>
        double GetSimilarity(double[] v1, double[] v2);
    }
}
