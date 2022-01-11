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
using ExtendibleTreeStructure.Tests.MenuItems;

namespace ExtendibleTreeStructure.Tests
{
    public class TestDataStoresCache : DataStoresCache<IMenuObject, INonCopyMenuObject, MenuDataObjectWrapper>
    {
        public delegate bool IsValidChildDataStoreItemDelegate(INonCopyMenuObject childDataStoreItem, INonCopyMenuObject parentDataStoreItem);

        public TestDataStoresCache(IReadOnlyList<IDataStore<IMenuObject>> dataStores) : base(dataStores,
            (dataStoreItemWrapper, parent) =>
                (new MenuDataObjectWrapper(dataStoreItemWrapper.DataStoreItem, dataStoreItemWrapper.DataStoreId, parent), null))
        {
            DataStores = dataStores;
        }

        public TestDataStoresCache(IReadOnlyList<IDataStore<IMenuObject>> dataStores,
            ConvertDataStoreItemWrapperDelegate<INonCopyMenuObject, MenuDataObjectWrapper> convertDataStoreItemWrapper) : base(dataStores, convertDataStoreItemWrapper)
        {
            DataStores = dataStores;
        }

        public IReadOnlyList<IDataStore<IMenuObject>> DataStores { get; }

        public IsValidChildDataStoreItemDelegate? IsValidChildDataStoreItemFunc { get; set; }

        protected override bool IsValidChildDataStoreItem(INonCopyMenuObject childDataStoreItem, INonCopyMenuObject parentDataStoreItem)
        {
            if (!base.IsValidChildDataStoreItem(childDataStoreItem, parentDataStoreItem))
                return false;

            if (parentDataStoreItem is MenuBarData)
                return childDataStoreItem is IMenuBarItemData;

            if (!(childDataStoreItem is IMenuItemData or IMenuItemCollection))
                return false;

            return IsValidChildDataStoreItemFunc?.Invoke(childDataStoreItem, parentDataStoreItem) ?? true;
        }
    }
}