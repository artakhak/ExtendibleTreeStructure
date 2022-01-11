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
    /// Circular path node. 
    /// </summary>
    public class PathComponentNode
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        public PathComponentNode(IDataStoreItem dataStoreItem, long dataStoreId)
        {
            DataStoreItem = dataStoreItem;
            DataStoreId = dataStoreId;
        }

        /// <summary>
        /// Id of the data store that owns the <see cref="DataStoreItem"/>.
        /// </summary>
        public long DataStoreId { get; }

        /// <summary>
        /// Data store item in circular path.
        /// </summary>
        public IDataStoreItem DataStoreItem { get; }

        /// <inheritdoc />
        public override bool Equals(object? obj)
        {
            if (obj is not PathComponentNode pathComponentNode)
                return false;

            return pathComponentNode.DataStoreItem.Id == DataStoreItem.Id && pathComponentNode.DataStoreId == DataStoreId;
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            return this.DataStoreItem.Id.GetHashCode();
        }
    }
}
