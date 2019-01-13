using System;
using System.Collections.Generic;
using System.Linq;

namespace Clustering
{
    /// <summary>
    /// Data matrix to describe a raw set of data for clustering. Each column corresponds to a
    /// data element, each row to a property (feature) to distinguish them.
    /// </summary>
    public class DataMatrix
    {
        private readonly string[] _elementNames;
        private readonly string[] _featureNames;
        private readonly double[,] _data;


        /// <summary>
        /// Constructor
        /// </summary>
        public DataMatrix(int n, int m)
        {
            _elementNames = Enumerable.Range(1, n).Select(i => "E" + i.ToString()).ToArray();
            _featureNames = Enumerable.Range(1, m).Select(i => "F" + i.ToString()).ToArray();

            _data = new double[n, m];
        }

        /// <summary>
        /// Constructor
        /// </summary>
        public DataMatrix(string[] elementNames, string[] featureNames)
        {
            _elementNames = elementNames;
            _featureNames = featureNames;

            _data = new double[elementNames.Length, featureNames.Length];
        }


        /// <summary>
        /// Gets the data: first index is element (column), second is feature (row)
        /// </summary>
        public double[,] Data
        {
            get { return _data; }
        }

        /// <summary>
        /// Gets the number of elements, i.e. the number of columns
        /// </summary>
        public int ElementCount
        {
            get { return _elementNames != null ? _elementNames.Length : 0; }
        }

        /// <summary>
        /// Gets the names of the columns (elements)
        /// </summary>
        public string[] ElementNames
        {
            get { return _elementNames; }
        }

        /// <summary>
        /// Gets the number of features/properties of each element, i.e. the number of rows
        /// </summary>
        public int FeatureCount
        {
            get { return _featureNames != null ? _featureNames.Length : 0; }
        }

        /// <summary>
        /// Gets the names of the rows (features)
        /// </summary>
        public string [] FeatureNames
        {
            get { return _featureNames; }
        }

        /// <summary>
        /// Returns true if the number of elements equals the number of the
        /// elements' features
        /// </summary>
        public bool IsSquare
        {
            get { return ElementCount == FeatureCount; }
        }

        /// <summary>
        /// Gets or set the element at the given position in the matrix
        /// </summary>
        /// <param name="n">colum index</param>
        /// <param name="m">row index</param>
        /// <returns>element in column n and row m</returns>
        /// <exception cref="IndexOutOfRangeException">if n or m is too large or negative</exception>
        public double this[int n, int m]
        {
            get { return _data[n, m]; }
            set { _data[n, m] = value; }
        }

        /// <summary>
        /// Gets the column of the matrix given by the index
        /// </summary>
        /// <param name="index">element index, i.e. column index</param>
        /// <returns>column vector</returns>
        /// <exception cref="IndexOutOfRangeException">if n or m is too large or negative</exception>
        /// 
        public IEnumerable<double> GetElement(int index)
        {
            for (int col = 0; col < FeatureCount; col++)
                yield return _data[index, col];
        }

        /// <summary>
        /// Returns a matrix with the same elements where the rows and columns
        /// are reorded using the given permutation
        /// </summary>
        public DataMatrix GetRowAndColumnPermutatedMatrix(int[] permutation)
        {
            if (!IsSquare)
                throw new InvalidOperationException("Cannot run row-and-column permutation on a " + ElementCount + "x" + FeatureCount + "-matrix");

            if (permutation.Length != ElementCount)
                throw new ArgumentException("Cannot apply permutation of length " + permutation.Length + " on a square matrix with dimension " + ElementCount);

            string[] reorderedElementNames = new string[ElementCount];

            for (int col = 0; col < ElementCount; col++)
                reorderedElementNames[permutation[col]] = _elementNames[col];

            string[] reorderedFeatureNames = new string[FeatureCount];

            for (int row = 0; row < FeatureCount; row++)
                reorderedFeatureNames[permutation[row]] = _featureNames[row];

            DataMatrix m = new DataMatrix(reorderedElementNames, reorderedFeatureNames);

            for (int col = 0; col < ElementCount; col++)
            {
                for (int row = 0; row < FeatureCount; row++)
                {
                    m[permutation[col], permutation[row]] = _data[col, row];
                }
            }

            return m;
        }

        /// <summary>
        /// Returns a matrix with the same elements where the columns
        /// are reorded using the given permutation
        /// </summary>
        public DataMatrix GetColumnPermutatedMatrix(int[] permutation)
        {
            if (permutation.Length != ElementCount)
                throw new ArgumentException("Cannot apply permutation of length " + permutation.Length + " on a matrix with " + ElementCount + " columns");

            string[] reorderedElementNames = new string[ElementCount];

            for (int col = 0; col < ElementCount; col++)
                reorderedElementNames[permutation[col]] = _elementNames[col];

            DataMatrix m = new DataMatrix(reorderedElementNames, _featureNames);

            for (int col = 0; col < ElementCount; col++)
            {
                for (int row = 0; row < FeatureCount; row++)
                {
                    m[permutation[col], row] = _data[col, row];
                }
            }

            return m;
        }

        /// <summary>
        /// Returns a matrix with the same elements where the rows
        /// are reorded using the given permutation
        /// </summary>
        public DataMatrix GetRowPermutatedMatrix(int[] permutation)
        {
            if (permutation.Length != FeatureCount)
                throw new ArgumentException("Cannot apply permutation of length " + permutation.Length + " on a matrix with " + FeatureCount + " rows");

            string[] reorderedFeatureNames = new string[FeatureCount];

            for (int i = 0; i < FeatureCount; i++)
                reorderedFeatureNames[permutation[i]] = _featureNames[i];

            DataMatrix m = new DataMatrix(_elementNames, reorderedFeatureNames);

            for (int col = 0; col < ElementCount; col++)
            {
                for (int row = 0; row < FeatureCount; row++)
                {
                    m[col, permutation[row]] = _data[col, row];
                }
            }

            return m;
        }

        /// <summary>
        /// Populates the matrix with random elements between 0 and 1
        /// </summary>
        public void PopulateRandom()
        {
            Random r = new Random();

            for (int col = 0; col < ElementCount; col++)
            {
                for (int row = 0; row < FeatureCount; row++)
                {
                    _data[col, row] = r.NextDouble();
                }
            }
        }


    }
}
