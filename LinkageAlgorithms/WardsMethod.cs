namespace Clustering.LinkageAlgorithms
{
    /// <summary>
    /// Conservative algoritm that doesn't minimize distances between clusters
    /// but tries to keep increasment of heterogenity within the clusters small
    /// </summary>
    public class WardsMethod : ILinkage
    {
        /// <summary>
        /// Gets the name of the metric
        /// </summary>
        public string Name
        {
            get { return "Ward's Method"; }
        }

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
        public double GetDistance(int p, int q, int r, double[,] distMatrix, BinaryTreeNode[] nodes)
        {
            // only the lower half of the (symmetric) distance matrix gets filled for performance
            // reasons, so make sure that column index is smaller than row index
            double d_rp = p < r ? distMatrix[p, r] : distMatrix[r, p];
            double d_rq = q < r ? distMatrix[q, r] : distMatrix[r, q];
            double d_pq = distMatrix[p, q];

            int n_p = nodes[p].LeafCount;
            int n_q = nodes[q].LeafCount;
            int n_r = nodes[r].LeafCount;

            return ((n_p + n_r) * d_rp + (n_q + n_r) * d_rq - n_r * d_pq ) / (n_p + n_q + n_r);
        }
    }
}
