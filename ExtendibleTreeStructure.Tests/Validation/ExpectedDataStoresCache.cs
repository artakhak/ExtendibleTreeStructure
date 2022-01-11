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

using NUnit.Framework;
using System.Collections.Generic;
using ExtendibleTreeStructure.Tests.MenuItems;

namespace ExtendibleTreeStructure.Tests.Validation
{
    public class ExpectedDataStoresCache
    {
        private readonly Dictionary<long, ExpectedMenuDataStoreItemsCache> _dataStoreIdToDataStore = new();

        private readonly List<ExpectedMenuDataStoreItemsCache> _dataStoreItemsCacheList = new();

        public ExpectedMenuDataStoreItemsCache GetExpectedMenuDataStoreItemsCache(long dataStoreId)
        {
            if (_dataStoreIdToDataStore.TryGetValue(dataStoreId, out var dataStoreItemsCache))
                return dataStoreItemsCache;

            throw new System.Exception($"No data store with Id={dataStoreId}.");
        }

        public ExpectedMenuObjectWrapper GetExpectedMenuObjectWrapper(long menuDataStoreId, long menuObjectId)
        {
            return GetExpectedMenuDataStoreItemsCache(menuDataStoreId).GetExpectedMenuObjectWrapper(menuObjectId);
        }

        public INonCopyMenuObject GetExpectedMenuObject(long menuDataStoreId, long menuObjectId)
        {
            return GetExpectedMenuObjectWrapper(menuDataStoreId, menuObjectId).NonCopyMenuObject;
        }

        public ExpectedMenuDataStoreItemsCache AddDataStoreItemsCache(long dataStoreId)
        {
            Assert.IsFalse(_dataStoreIdToDataStore.ContainsKey(dataStoreId));
            var expectedMenuDataStoreItemsCache = new ExpectedMenuDataStoreItemsCache(dataStoreId);

            _dataStoreIdToDataStore[dataStoreId] = expectedMenuDataStoreItemsCache;
            _dataStoreItemsCacheList.Add(expectedMenuDataStoreItemsCache);

            return expectedMenuDataStoreItemsCache;
        }

        public void AssertEqualTo(TestDataStoresCache testDataStoresCache)
        {
            Assert.AreEqual(_dataStoreItemsCacheList.Count, testDataStoresCache.DataStoreItemsCacheList.Count);

            for (var i = 0; i < _dataStoreItemsCacheList.Count; ++i)
            {
                var dataStoreItemsCache = _dataStoreItemsCacheList[i];
                Assert.IsTrue(testDataStoresCache.TryGetDataStore(dataStoreItemsCache.DataStoreId, out var dataStoreItemsCache2));

                dataStoreItemsCache.AssertEqualTo(dataStoreItemsCache2!);
                dataStoreItemsCache.AssertEqualTo(testDataStoresCache.DataStoreItemsCacheList[i]);
            }
        }
    }
}