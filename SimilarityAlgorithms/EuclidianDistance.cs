namespace Clustering.DistanceAlgorithms
{
    /// <summary>
    /// Class to calculate the squared euclidian distance between v1 and v2
    /// </summary>
    public class EuclidianDistance : ISimilarity
    {
        /// <summary>
        /// Gets the name of the distance method
        /// </summary>
        public string Name
        {
            get { return "Euclidian Distance"; }
        }

        /// <summary>
        /// Gets the euclidian distance between the two vectors
        /// </summary>
        public double GetSimilarity(double[] v1, double[] v2)
        {
            double res = 0;

            for (int i = 0; i < v1.Length; i++)
                res += (v1[i] - v2[i]) * (v1[i] - v2[i]);

            return res;
        }
    }
}
