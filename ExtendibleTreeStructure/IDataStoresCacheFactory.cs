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

using System;
using System.Collections.Generic;
using ExtendibleTreeStructure.MessageLogging;

namespace ExtendibleTreeStructure
{
    /// <summary>
    /// A factory for loading data stores cache.
    /// </summary>
    public interface IDataStoresCacheFactory<in TDataStoreItem, TNonCopyDataStoreItem, TDataStoreItemWrapper>
        where TNonCopyDataStoreItem : class, TDataStoreItem, INonCopyDataStoreItem, IDataStoreItemCanBeCopiedWithNullParent<TNonCopyDataStoreItem>
        where TDataStoreItem : class, IDataStoreItem
        where TDataStoreItemWrapper : class, IDataStoreItemWrapper<TNonCopyDataStoreItem>
    {
        /// <summary>
        /// Creates and loads data store items cache into <see cref="IDataStoreItemsCache{TNonCopyDataStoreItem, TDataStoreItemWrapper}"/> using
        /// data stores specified in parameter <paramref name="dataStores"/>.
        /// </summary>
        /// <param name="dataStores">Data stores to load.</param>
        /// <param name="convertDataStoreItemWrapper">A converter that converts an instance of <typeparamref name="TDataStoreItemWrapper"/> 
        /// into a tuple of two objects. The first one is an instance of <see cref="TDataStoreItemWrapper"/>, that will be added to cache. If the first value is null, data store item wrapper will not be added to cache.
        /// The second parameter is an instance of <see cref="ILoggedMessage"/> (which can be null as well), which will be logged by <see cref="IDataStoresCacheFactory{TDataStoreItem, TNonCopyDataStoreItem, TDataStoreItemWrapper}"/>.
        /// </param>
        /// <param name="onMessageLogged">A delegate that handles logged messages (writes the logged message to console or a log file, etc.).
        ///  The logged message can be an instance of <see cref="ILoggedMessage"/> or any subclass of <see cref="ILoggedMessage"/> (e.g., <see cref="ICircularReferencesErrorMessage"/>).
        /// </param>
        IDataStoresCache<TNonCopyDataStoreItem, TDataStoreItemWrapper> LoadDataStoresCache(
            IReadOnlyList<IDataStore<TDataStoreItem>> dataStores,
            ConvertDataStoreItemWrapperDelegate<TNonCopyDataStoreItem, TDataStoreItemWrapper> convertDataStoreItemWrapper,
            Action<ILoggedMessage> onMessageLogged);
    }

    /// <inheritdoc />
    public class DataStoresCacheFactory<TDataStoreItem, TNonCopyDataStoreItem, TDataStoreItemWrapper> : IDataStoresCacheFactory<TDataStoreItem, TNonCopyDataStoreItem, TDataStoreItemWrapper>
        where TNonCopyDataStoreItem : class, TDataStoreItem, INonCopyDataStoreItem, IDataStoreItemCanBeCopiedWithNullParent<TNonCopyDataStoreItem>
        where TDataStoreItem : class, IDataStoreItem
        where TDataStoreItemWrapper : class, IDataStoreItemWrapper<TNonCopyDataStoreItem>
    {
        /// <inheritdoc />
        public IDataStoresCache<TNonCopyDataStoreItem, TDataStoreItemWrapper> LoadDataStoresCache(IReadOnlyList<IDataStore<TDataStoreItem>> dataStores,
            ConvertDataStoreItemWrapperDelegate<TNonCopyDataStoreItem, TDataStoreItemWrapper> convertDataStoreItemWrapper,
            Action<ILoggedMessage> onMessageLogged)
        {
            var dataStoresCache = new DataStoresCache<TDataStoreItem, TNonCopyDataStoreItem, TDataStoreItemWrapper>(dataStores, convertDataStoreItemWrapper);
            dataStoresCache.DataStoresCacheLoadMessageEvent += (sender, e) => { onMessageLogged(e.LoggedMessage); };

            dataStoresCache.Initialize();
            return dataStoresCache;
        }
    }
}