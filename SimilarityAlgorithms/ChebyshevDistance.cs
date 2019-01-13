using System;

namespace Clustering.DistanceAlgorithms
{
	/// <summary>
	/// Class to calculate the city-block distance between v1 and v2
	/// </summary>
	public class ChebyshevDistance : ISimilarity
	{
		/// <summary>
		/// Gets the name of the distance method
		/// </summary>
		public string Name
		{
			get { return "Chebyshef Distance"; }
		}

		/// <summary>
		/// Gets the chebyshef(maximum norm)
		/// distance between the two vectors
		/// </summary>
		public double GetSimilarity(double[] v1, double[] v2)
		{
			double res = 0;

			for (int i = 0; i < v1.Length; i++)
			{
				double d = Math.Abs(v1[i] - v2[i]);
				                    
				if(d > res)
					res = d;
			}

			return res;
		}
	}
}
