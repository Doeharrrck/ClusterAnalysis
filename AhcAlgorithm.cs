using Clustering.DistanceAlgorithms;
using Clustering.LinkageAlgorithms;
using System;
using System.Linq;


namespace Clustering
{
    /// <summary>
    /// Algorithm to cluster data given by <see cref="DataMatrix"/> object where each column represents an element
    /// of the set of elements to be clustered and each row represents a feature of the elements.
    /// 
    /// Use as following:
    ///  - create an instance
    ///  - you can optionally change the <see cref="DistanceMode"/> or the <see cref="LinkageMode"/> of the 
    ///    algorithm in the constructor or by setting the properties
    ///  - provide the data using the <see cref="SetData(DataMatrix)"/> method
    ///  - run the algorithm with the <see cref="RunAhc"/> method
    ///  
    /// The output is a binary tree. The <see cref="Tree"/> property returns the root node
    /// The <see cref="Permutation"/> method returns the permutation of the original objects
    /// </summary>
    public class AhcAlgorithm
    {
        private DataMatrix _rawData;
        private DataMatrix _distanceMatrix;
        private DataMatrix _tempDistanceMatrix;

        private int[] _permutation = new int[0];
        private BinaryTreeNode[] _nodeArray = new BinaryTreeNode[0];
        private int _nodeIdx = 0;


        /// <summary>
        /// Constructor
        /// </summary>
        public AhcAlgorithm()
        {
            DistanceMode = new EuclidianDistance();
            LinkageMode = new SingleLinkage();
        }

        /// <summary>
        /// Constructor
        /// </summary>
        public AhcAlgorithm(ISimilarity distanceMode, ILinkage linkageMode)
        {
            DistanceMode = distanceMode;
            LinkageMode = linkageMode;
        }


        #region Public Properties

        /// <summary>
        /// Gets or sets the distance matrix that quantifies
        /// the differences between clusters / elements
        /// </summary>
        public ISimilarity DistanceMode
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the linkage mode that determines
        /// in which way the clusters are merged
        /// </summary>
        public ILinkage LinkageMode
        {
            get;
            set;
        }

        /// <summary>
        /// The <see cref="RunAhc"/> method will print debug information
        /// to the console if this property is set to:
        ///  0: nothing is printed
        ///  1: merge info
        ///  2: merge info + distance matrices
        /// </summary>
        public int Verbosity
        {
            get;
            set;
        }

        /// <summary>
        /// Gets the root node of the binary tree after the 
        /// AHC algorithm was executed on the data
        /// </summary>
        public BinaryTreeNode Tree
        {
            get
            {
                if (_nodeArray == null)
                    throw new InvalidOperationException("No tree data available");

                if (_nodeArray.Length != 1)
                    throw new InvalidOperationException("Algorithm didn't finish successfully");

                return _nodeArray[0];
            }
        }

        /// <summary>
        /// Gets the permutation of the leaves after the AHC algorithm was executed on the data
        /// </summary>
        public int[] Permutation
        {
            get { return _permutation; }
            private set { _permutation = value; }
        }

        #endregion


        #region Public Methods

        /// <summary>
        /// Sets the raw data
        /// </summary>
        public void SetData(DataMatrix data)
        {
            _rawData = data;
        }

        /// <summary>
        /// Runs the clustering algorithm after the data have been set using the <see cref="SetData(DataMatrix)"/> method
        /// </summary>
        /// <exception cref="InvalidOperationException">Exception is thrown or configuration is invalid</exception>
        public void RunAhc()
        {
            if (_rawData == null)
                throw new InvalidOperationException("No raw data set");

            if (LinkageMode is WardsMethod && !(DistanceMode is EuclidianDistance))
                throw new InvalidOperationException("Ward's method can only run with Euclidian distance");

            if (DistanceMode is PearsonCorrelation && _rawData.Data.GetLength(0) < 2)
                throw new InvalidOperationException("Pearson correlation requires that the elements have at least two features");

            _distanceMatrix = CalculateDistanceMatrix(_rawData);
            _tempDistanceMatrix = _distanceMatrix;

            _nodeIdx = 0;
            _nodeArray = CreateLeafNodeArray(_tempDistanceMatrix);

            while (_tempDistanceMatrix.Data.GetLength(0) > 1)
                MergeClosestClusters();

            _permutation = BuildPermutation();
        }

        /// <summary>
        /// Brings the leaves of the binary tree into the linear order consistent with the tree
        /// sturcture in which adjacent elements are most close together by flipping cluster
        /// nodes. The <see cref="Permutation"/> property is updated after this is done. 
        /// </summary>
        public void SortLeaves()
        {
            DataMatrix dist = CalculateDistanceMatrix(_rawData);

            for (int j = 1; j < _distanceMatrix.Data.GetLength(0); j++)
                for (int i = 0; i < j; i++)
                    dist[j, i] = dist[i, j];

            dist = dist.GetRowAndColumnPermutatedMatrix(Permutation);

            GetOptimalOrdering(Tree, dist);

            _permutation = BuildPermutation();
        }

        #endregion


        #region Private

        /// <summary>
        /// Calculates the distance matrix for the given data matrix
        /// where each column represents an element and each row a
        /// feature / property of the elements.
        /// </summary>
        private DataMatrix CalculateDistanceMatrix(DataMatrix data)
        {
            int dataCount = data.Data.GetLength(0);
            int featureCount = data.Data.GetLength(1);

            DataMatrix dist = new DataMatrix(data.ElementNames, data.ElementNames);

            for (int j = 1; j < dataCount; j++)
            {
                for (int i = 0; i < j; i++)
                {
                    double[] v1 = new double[featureCount];
                    double[] v2 = new double[featureCount];

                    for (int k = 0; k < featureCount; k++)
                    {
                        v1[k] = data[i, k];
                        v2[k] = data[j, k];
                    }

                    dist[i, j] = DistanceMode.GetSimilarity(v1, v2);
                }
            }

            if (Verbosity > 1)
                PrintMatrix(dist, 3, DistanceMode.Name + " Distance Matrix");

            return dist;
        }

        /// <summary>
        /// Searches the closest pair of clusters from the current temporary distance matrix
        /// </summary>
        private void MergeClosestClusters()
        {
            int c1 = -1;
            int c2 = -1;

            double d = FindClosestClusters(ref c1, ref c2);

            DataMatrix newDistanceMatrix = UpdateDistanceMatrix(c1, c2);

            if (Verbosity > 0)
                Console.WriteLine("Merged " + _tempDistanceMatrix.ElementNames[c1] + " with " + _tempDistanceMatrix.ElementNames[c2]);

            if(Verbosity > 1)
                PrintMatrix(newDistanceMatrix, 3, "Temp Matrix " + (_rawData.Data.GetLength(0) - newDistanceMatrix.Data.GetLength(0)));

            _tempDistanceMatrix = newDistanceMatrix;
            _nodeArray = UpdateNodeArray(_tempDistanceMatrix, c1, c2, d);
        }

        /// <summary>
        /// Merges the clusters that correspond to the columns of the current temporary distance matrix
        /// </summary>
        /// <param name="c1">column index of first cluster</param>
        /// <param name="c2">column index of second cluster</param>
        /// <returns>updated distance matrix</returns>
        private DataMatrix UpdateDistanceMatrix(int c1, int c2)
        {
            int oldClusterCount = _tempDistanceMatrix.Data.GetLength(0);
            int newClusterCount = oldClusterCount - 1;

            int n = 1;

            string[] newClusterNames = new string[newClusterCount];
            newClusterNames[0] = _tempDistanceMatrix.ElementNames[c1] + "," + _tempDistanceMatrix.ElementNames[c2];

            for (int i = 0; i < oldClusterCount; i++)
            {
                if (i != c1 && i != c2)
                    newClusterNames[n++] = _tempDistanceMatrix.ElementNames[i];
            }

            DataMatrix newDistanceMatrix = new DataMatrix(newClusterNames, newClusterNames);

            int m = 1;

            for (int j = 0; j < oldClusterCount; j++)
            {
                if (j != c1 && j != c2)
                    newDistanceMatrix[0, m++] = LinkageMode.GetDistance(c1, c2, j, _tempDistanceMatrix.Data, _nodeArray);
            }

            m = 1;

            for (int j = 0; j < oldClusterCount; j++)
            {
                n = 1;

                for (int i = 0; i < j; i++)
                {
                    if (i != c1 && i != c2 && j != c1 && j != c2)
                        newDistanceMatrix[n, m] = _tempDistanceMatrix.Data[i, j];

                    if (i != c1 && i != c2)
                        n++;
                }

                if (j != c1 && j != c2)
                    m++;
            }

            return newDistanceMatrix;
        }

        /// <summary>
        /// Gets the indices of the two closest clusters, i.e the column index and
        /// row index of the smallest element of the temporary distance matrix. This
        /// method also returns this minimum distance value
        /// </summary>
        private double FindClosestClusters(ref int c1, ref int c2)
        {
            double min = double.MaxValue;
            int clusterCount = _tempDistanceMatrix.Data.GetLength(1);

            for (int j = 1; j < clusterCount; j++)
            {
                for (int i = 0; i < j; i++)
                {
                    if (_tempDistanceMatrix[i, j] < min)
                    {
                        c1 = i;
                        c2 = j;

                        min = _tempDistanceMatrix[i, j];
                    }
                }
            }

            return min;
        }

        /// <summary>
        /// Creates the array of leaf nodes with the distance matrix of the original elements
        /// </summary>
        /// <param name="distanceMatrix">distance matrix of the original elements</param>
        /// <returns>array with length equal to the number of elements</returns>
        private BinaryTreeNode[] CreateLeafNodeArray(DataMatrix distanceMatrix)
        {
            int elementCount = distanceMatrix.Data.GetLength(0);
            BinaryTreeNode[] newNodeArray = new BinaryTreeNode[elementCount];

            for (int i = 0; i < elementCount; i++)
                newNodeArray[i] = new BinaryTreeNode(_nodeIdx++, _rawData.ElementNames[i]);

            return newNodeArray;
        }

        /// <summary>
        /// Updates the array of nodes after a merge operation with the new distance matrix
        /// </summary>
        /// <param name="distanceMatrix">temporary distance matrix between the clusters after merge</param>
        /// <param name="c1">index of the first cluster that was merged</param>
        /// <param name="c2">index of the second cluster that was merged</param>
        /// <param name="d">distance between the clusters that were merged</param>
        /// <returns>array with length equal to the current number of clusters</returns>
        private BinaryTreeNode[] UpdateNodeArray(DataMatrix distanceMatrix, int c1, int c2, double d)
        {
            int clusterCount = distanceMatrix.Data.GetLength(0);
            BinaryTreeNode[] newNodeArray = new BinaryTreeNode[clusterCount];

            newNodeArray[0] = new BinaryTreeNode(_nodeIdx++, _nodeArray[c1], _nodeArray[c2], d);

            int n = 1;

            for (int i = 0; i < clusterCount + 1; i++)
            {
                if (i != c1 && i != c2)
                    newNodeArray[n++] = _nodeArray[i];
            }

            return newNodeArray;
        }

        /// <summary>
        /// Builds the permutation of the original elements that was generated during clustering from the tree
        /// Output like 3, 1, 0, ... means that the originally first element is on position 3 now and so on.
        /// </summary>
        private int[] BuildPermutation()
        {
            if (_nodeArray == null)
                throw new InvalidOperationException("No tree data available -> Cannot build permutation");

            if (_nodeArray.Length != 1)
                throw new InvalidOperationException("Algorithm didn't finish successfully");

            int i = 0;
            int[] permuationArray = new int[_rawData.ElementCount];

            FillPermutationArray(permuationArray, ref i, Tree);

            return permuationArray;
        }

        /// <summary>
        /// Fills the permutation array iteratively
        /// </summary>
        private void FillPermutationArray(int[] permuationArray, ref int i, BinaryTreeNode node)
        {
            if (node.IsLeaf)
            {
                permuationArray[node.Id] = i++;
            }
            else
            {
                FillPermutationArray(permuationArray, ref i, node.LeftChild);
                FillPermutationArray(permuationArray, ref i, node.RightChild);
            }
        }

        /// <summary>
        /// Prints the matrix to the console
        /// </summary>
        private void PrintMatrix(DataMatrix m, int precission, string name = "")
        {
            int columnCount = m.Data.GetLength(0);
            int rowCount = m.Data.GetLength(1);

            if (!string.IsNullOrEmpty(name))
                Console.WriteLine(name + Environment.NewLine);

            Console.Write(string.Empty.PadLeft(30));

            for (int j = 0; j < columnCount; j++)
                Console.Write(m.ElementNames[j].PadLeft(20));

            Console.Write(Environment.NewLine);

            for (int j = 0; j < rowCount; j++)
            {
                Console.Write(m.FeatureNames[j].PadLeft(30));

                for (int i = 0; i < columnCount; i++)
                {
                    Console.Write(m.Data[i, j].ToString("F" + precission).PadLeft(20));
                }

                Console.Write(Environment.NewLine);
            }

            Console.Write(Environment.NewLine);
        }

        /// <summary>
        /// Iteratively flips the nodes of the tree and checks for the optimum distance
        /// </summary>
        private double GetOptimalOrdering(BinaryTreeNode node, DataMatrix distanceMatrix)
        {
            if (node.LeafCount == 1)
                return 0;

            if (node.LeafCount == 2)
                return distanceMatrix[Permutation[node.LeftChild.Id], Permutation[node.RightChild.Id]];

            double ml = GetOptimalOrdering(node.LeftChild, distanceMatrix);
            double mr = GetOptimalOrdering(node.RightChild, distanceMatrix);

            // array to store distance values between subclusters of the node:
            // 0: no child flipped
            // 1: left child node flipped
            // 2: both child nodes flipped
            // 3: right child node flipped
            double[] mvalues = new double[4];

            node.LeftChild.Flip();
            mvalues[1] = ml + mr + distanceMatrix[Permutation[node.LeftChild.RightLeaf.Id], Permutation[node.RightChild.LeftLeaf.Id]];
            node.RightChild.Flip();
            mvalues[2] = ml + mr + distanceMatrix[Permutation[node.LeftChild.RightLeaf.Id], Permutation[node.RightChild.LeftLeaf.Id]];
            node.LeftChild.Flip();
            mvalues[3] = ml + mr + distanceMatrix[Permutation[node.LeftChild.RightLeaf.Id], Permutation[node.RightChild.LeftLeaf.Id]];
            node.RightChild.Flip();
            mvalues[0] = ml + mr + distanceMatrix[Permutation[node.LeftChild.RightLeaf.Id], Permutation[node.RightChild.LeftLeaf.Id]];

            double min = mvalues.Min();
            int minIndex = Array.IndexOf(mvalues, min);

            switch (minIndex)
            {
                case 1:
                    node.LeftChild.Flip();
                    break;
                case 2:
                    node.LeftChild.Flip();
                    node.RightChild.Flip();
                    break;
                case 3:
                    node.RightChild.Flip();
                    break;
                default:
                    break;
            }

            return min;
        }

        #endregion
    }
}
