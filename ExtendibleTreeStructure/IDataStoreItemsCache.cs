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
    /// Cache of instances of <typeparamref name="TDataStoreItemWrapper"/> loaded for data store items (instances of <see cref="IDataStoreItem"/>,
    /// in specific data store).
    /// </summary>
    public interface IDataStoreItemsCache<TNonCopyDataStoreItem, TDataStoreItemWrapper>
        where TNonCopyDataStoreItem : class, INonCopyDataStoreItem
        where TDataStoreItemWrapper : class, IDataStoreItemWrapper<TNonCopyDataStoreItem>
    {
        /// <summary>
        /// Data storeId.
        /// </summary>
        long DataStoreId { get; }

        /// <summary>
        /// Tries to retrieve an instance of <see cref="TDataStoreItemWrapper"/> with data store item Id=<paramref name="dataStoreItemId"/>.
        /// </summary>
        /// <param name="dataStoreItemId">Data store item Id.</param>
        /// <param name="dataStoreItemWrapper">Retrieved instance of <typeparamref name="TDataStoreItemWrapper"/>.</param>
        /// <returns>
        /// Returns true, if data store item wrapper was loaded into cache in data store with Id=<see cref="DataStoreId"/>.
        /// Returns false otherwise.
        /// </returns>
        bool TryGetDataStoreItem(long dataStoreItemId, out TDataStoreItemWrapper? dataStoreItemWrapper);

        /// <summary>
        /// List of data store item wrappers for top level data store items (i.e., data store items <see cref="IDataStoreItem"/> that do not implement
        /// <see cref="ICanHaveParent"/> or implement <see cref="ICanHaveParent"/> but have null value for <see cref="ICanHaveParent.ParentId"/>).
        /// The list is sorted based on the values of <see cref="IDataStoreItem.Priority"/>.
        /// </summary>
        IReadOnlyList<TDataStoreItemWrapper> TopLevelDataStoreItemWrappers { get; }
    }
}