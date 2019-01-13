using System;

namespace Clustering
{
    public class BinaryTreeNode
    {
        /// <summary>
        /// Constructor for non-leaf nodes
        /// </summary>
        /// <param name="id">index of the node (root is last)</param>
        /// <param name="leftChild">left child node</param>
        /// <param name="rightChild">right child node</param>
        /// <param name="distance">distance between the two children</param>
        public BinaryTreeNode(int id, BinaryTreeNode leftChild, BinaryTreeNode rightChild, double distance)
        {
            Id = id;
            Level = Math.Max(leftChild.Level, rightChild.Level) + 1;
            LeafCount = leftChild.LeafCount + rightChild.LeafCount;
            LeftChild = leftChild;
			LeftChild.Parent = this;
            RightChild = rightChild;
			RightChild.Parent = this;
            Name = LeftChild.Name + "," + RightChild.Name;
            ChildDistance = distance;
        }

        /// <summary>
        /// Constructor for leaf nodes
        /// </summary>
        /// <param name="id">index of the node (root is last)</param>
        public BinaryTreeNode(int id, string name)
        {
            Id = id;
            Level = 0;
            LeafCount = 1;
            Name = name;
        }

        /// <summary>
        /// Gets the id of the node. In a tree with n leafs the leafs are numbered 
        /// from 0 to n-1, so the root node will have the index 2n - 2.
        /// </summary>
        public int Id
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the level of the node. For leafs this property returns 0, for other
        /// nodes it returns the maximum of the children's level plus 1.
        /// </summary>
        public int Level
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the number of leafs among the descendants of this node 
        /// (or 1 if the node is a leaf itself)
        /// </summary>
        public int LeafCount
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the name of the node. For non-leaf nodes 
        /// it is composed from the leafs names
        /// </summary>
        public string Name
        {
            get;
            private set;
        }

        /// <summary>
        /// Returns true if this node doesn't have children
        /// </summary>
        public bool IsLeaf
        {
            get { return LeftChild == null && RightChild == null; }
        }

		/// <summary>
		/// Gets the parent node. Will return null for the root node
		/// </summary>
		public BinaryTreeNode Parent
		{
			get;
			private set;
		}

        /// <summary>
        /// Gets the left child of the node
        /// </summary>
        public BinaryTreeNode LeftChild
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the right child of the node
        /// </summary>
        public BinaryTreeNode RightChild
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the most left leaf of the node
        /// </summary>
        public BinaryTreeNode LeftLeaf
        {
            get
            {
                BinaryTreeNode leftLeaf = this;

                while (!leftLeaf.IsLeaf)
                    leftLeaf = leftLeaf.LeftChild;

                return leftLeaf;
            }
        }

        /// <summary>
        /// Gets the most right leaf of the node
        /// </summary>
        public BinaryTreeNode RightLeaf
        {
            get
            {
                BinaryTreeNode rigthLeaf = this;

                while (!rigthLeaf.IsLeaf)
                    rigthLeaf = rigthLeaf.RightChild;

                return rigthLeaf;
            }
        }

        /// <summary>
        /// Gets the distance between the two children
        /// </summary>
        public double ChildDistance
        {
            get;
            private set;
        }

        /// <summary>
        /// Swaps the left and the right child iteratively for all children
        /// </summary>
        public void Flip()
        {
            if(!IsLeaf)
            {
                BinaryTreeNode temp = LeftChild;
                LeftChild = RightChild;
                RightChild = temp;

                LeftChild.Flip();
                RightChild.Flip();
            }
        }
    }
}
