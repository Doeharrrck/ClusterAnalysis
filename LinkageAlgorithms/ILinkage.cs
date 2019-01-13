namespace Clustering.LinkageAlgorithms
{
    /// <summary>
    /// Interface for classes that provide algorithms to calculate a distance between two clusters
    /// </summary>
    public interface ILinkage
    {
        /// <summary>
        /// Gets the name of the metric
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Gets the distance between the cluster r and the cluster 
        /// that was merged from p and q
        /// </summary>
        /// <param name="p">index of first child cluster of last cluster that was merged</param>
        /// <param name="q">index of second child cluster of last cluster that was merged</param>
        /// <param name="r">index of cluster which's distance to pq is to be calculated</param>
        /// <param name="distMatrix">current distance matrix</param>
        /// <param name="nodes">current cluster nodes</param>
        /// <returns>distance between r and pq</returns>
        double GetDistance(int p, int q, int r, double[,] distMatrix, BinaryTreeNode[] nodes);
    }
}
