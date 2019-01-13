using System;
using System.Linq;

namespace Clustering.DistanceAlgorithms
{
    /// <summary>
    /// Class to calculate the pearson correlation (similarity in profile) between v1 and v2
    /// </summary>
    public class PearsonCorrelation : ISimilarity
    {
        /// <summary>
        /// Gets the name of the distance method
        /// </summary>
        public string Name
        {
            get { return "Pearson Correlation"; }
        }

        /// <summary>
        /// Gets the 2 minus Pearson correlation between the two vectors so that
        /// GetSimilarity(v, v) will return 0, while two anti-correlating vectors
        /// will return 2.
        /// </summary>
        public double GetSimilarity(double[] v1, double[] v2)
        {
            double v1_ = v1.Sum() / v1.Length;
            double v2_ = v2.Sum() / v1.Length;

            double cov = 0;

            double s1 = 0;
            double s2 = 0;

            double d1;
            double d2;

            for (int i = 0; i < v1.Length; i++)
            {
                d1 = v1[i] - v1_;
                d2 = v2[i] - v2_;

                cov += d1 * d2;

                s1 += d1 * d1;
                s2 += d2 * d2;
            }

            return 2 - cov / Math.Sqrt(s1 * s2);
        }
    }
}
