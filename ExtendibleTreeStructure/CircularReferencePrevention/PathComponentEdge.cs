// This software is part of the ExtendibleTreeStructure library
// Copyright © 2018 ExtendibleTreeStructure Contributors
// http://oroptimizer.com
//
// Permission is hereby granted, free of charge, to any person
// obtaining a copy of this software and associated documentation
// files (the "Software"), to deal in the Software without
// restriction, including without limitation the rights to use,
// copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the
// Software is furnished to do so, subject to the following
// conditions:
//
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES
// OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT
// HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
// WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
// FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR
// OTHER DEALINGS IN THE SOFTWARE.

namespace ExtendibleTreeStructure.CircularReferencePrevention
{

    /// <summary>
    /// Circular path edge. 
    /// </summary>
    public class PathComponentEdge
    {
        /// <summary>
        /// Constructor.
        /// </summary> 
        public PathComponentEdge(PathComponentNode startingNode, PathComponentNode endingNode, DataStoreItemsRelationship startingEndingNodesRelationship)
        {
            StartingNode = startingNode;
            EndingNode = endingNode;
            StartingEndingNodesRelationship = startingEndingNodesRelationship;
        }

        /// <summary>
        /// Starting node.
        /// </summary>
        public PathComponentNode StartingNode { get; }

        /// <summary>
        /// Ending node.
        /// </summary>
        public PathComponentNode EndingNode { get; }

        /// <summary>
        /// Relationship between <see cref="StartingNode"/> and <see cref="EndingNode"/>.
        /// For example if <see cref="StartingEndingNodesRelationship"/> is <see cref="DataStoreItemsRelationship.Copied"/>,
        /// then <see cref="StartingNode"/> is expected to be an instance of <see cref="ICopyDataStoreItem"/> that references
        /// data store item in <see cref="EndingNode"/>.
        /// Another example is when <see cref="StartingEndingNodesRelationship"/> is <see cref="DataStoreItemsRelationship.Child"/>,
        /// In this case <see cref="EndingNode"/> is expected to be an instance of <see cref="ICanHaveParent"/> and
        /// <see cref="EndingNode"/>.<see cref="ICanHaveParent.ParentId"/> is expected to be equal to <see cref="StartingNode"/>.<see cref="IDataStoreItem.Id"/>.
        /// </summary>
        public DataStoreItemsRelationship StartingEndingNodesRelationship { get; }

        /// <inheritdoc />
        public override bool Equals(object? obj)
        {
            if (obj is not PathComponentEdge pathComponentEdge)
                return false;

            return this.StartingNode.Equals(pathComponentEdge.StartingNode) && this.EndingNode.Equals(pathComponentEdge.EndingNode) &&
                   this.StartingEndingNodesRelationship == pathComponentEdge.StartingEndingNodesRelationship;
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            // TODO: Improve to use other members too. For now does not matter.
            return this.StartingNode.DataStoreItem.Id.GetHashCode();
        }
    }
}