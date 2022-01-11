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

using System.Collections.Generic;

namespace ExtendibleTreeStructure
{

    /// <summary>
    /// A wrapper for <see cref="INonCopyDataStoreItem"/>. 
    /// </summary>
    public class DataStoreItemWrapper<TNonCopyDataStoreItem> : IDataStoreItemWrapper<TNonCopyDataStoreItem>
        where TNonCopyDataStoreItem : class, INonCopyDataStoreItem
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="dataStoreItem">Wrapped data store item.</param>
        /// <param name="dataStoreId">Data store Id of the data store that owns the data store item.</param>
        /// <param name="parent">Parent data store item wrapper.</param>
        public DataStoreItemWrapper(TNonCopyDataStoreItem dataStoreItem, long dataStoreId, IDataStoreItemWrapper<TNonCopyDataStoreItem>? parent = null)
        {
            DataStoreItem = dataStoreItem;
            Parent = parent;
            DataStoreId = dataStoreId;
        }


        /// <inheritdoc />
        public TNonCopyDataStoreItem DataStoreItem { get; }

        /// <inheritdoc />
        public IDataStoreItemWrapper<TNonCopyDataStoreItem>? Parent { get; }

        /// <inheritdoc />
        public long DataStoreId { get; }

        /// <inheritdoc />
        public List<IDataStoreItemWrapper<TNonCopyDataStoreItem>> Children { get; } = new();
    }
}