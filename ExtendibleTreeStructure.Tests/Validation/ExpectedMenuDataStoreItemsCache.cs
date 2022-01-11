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
using ExtendibleTreeStructure.Tests.MenuItems;
using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;

namespace ExtendibleTreeStructure.Tests.Validation
{

    public class ExpectedMenuDataStoreItemsCache
    {
        private readonly List<ExpectedMenuObjectWrapper> _topLevelDataStoreItemWrappers = new();
        private readonly List<ExpectedMenuObjectWrapper> _allDataStoreItemWrappers = new();

        public ExpectedMenuDataStoreItemsCache(long dataStoreId)
        {
            DataStoreId = dataStoreId;
        }

        public long DataStoreId { get; }

        public ExpectedMenuObjectWrapper GetExpectedMenuObjectWrapper(long menuObjectId)
        {
            var expectedMenuObjectWrappers = _allDataStoreItemWrappers.Where(x => x.NonCopyMenuObject.Id == menuObjectId).ToList();
            Assert.AreEqual(1, expectedMenuObjectWrappers.Count);
            return expectedMenuObjectWrappers[0];
        }

        public IReadOnlyList<ExpectedMenuObjectWrapper> TopLevelDataStoreItemWrappers => _topLevelDataStoreItemWrappers;

        public IReadOnlyList<ExpectedMenuObjectWrapper> AllDataStoreItemWrappers => _allDataStoreItemWrappers;

        public ExpectedMenuObjectWrapper ContinueWithMenuBar(long id, int? priority = null)
        {
            var expectedMenuObjectWrapper = new ExpectedMenuObjectWrapper(this, new MenuBarData(id)
            {
                Priority = priority
            });

            AddMenuObjectWrapper(expectedMenuObjectWrapper);

            return expectedMenuObjectWrapper;
        }

        public ExpectedMenuDataStoreItemsCache AddMenuBarAndContinue(long id, int? priority = null)
        {
            _ = ContinueWithMenuBar(id, priority);
            return this;
        }

        public ExpectedMenuObjectWrapper ContinueWithMenuBarItem(long id, int? priority = null, long? parentId = null)
        {
            var expectedMenuObjectWrapper = new ExpectedMenuObjectWrapper(this, new MenuBarItemData(id, parentId)
            {
                Priority = priority
            });

            AddMenuObjectWrapper(expectedMenuObjectWrapper);

            return expectedMenuObjectWrapper;
        }

        public ExpectedMenuDataStoreItemsCache AddMenuBarItemAndContinue(long id, int? priority = null, long? parentId = null)
        {
            _ = ContinueWithMenuBarItem(id, priority, parentId);
            return this;
        }

        public ExpectedMenuObjectWrapper ContinueWithMenuItem(long id, int? priority = null, long? parentId = null)
        {
            var expectedMenuObjectWrapper = new ExpectedMenuObjectWrapper(this, new MenuItemData(id, parentId)
            {
                Priority = priority
            });

            AddMenuObjectWrapper(expectedMenuObjectWrapper);

            return expectedMenuObjectWrapper;
        }

        public ExpectedMenuDataStoreItemsCache AddMenuItemAndContinue(long id, int? priority = null, long? parentId = null)
        {
            _ = ContinueWithMenuItem(id, priority, parentId);
            return this;
        }

        public ExpectedMenuObjectWrapper ContinueWithMenuItemCollection(long id, int? priority = null,
            bool usesMenuSeparator = false, long? parentId = null)
        {
            var expectedMenuObjectWrapper = new ExpectedMenuObjectWrapper(this, new MenuItemCollection(id, parentId, usesMenuSeparator)
            {
                Priority = priority
            });

            AddMenuObjectWrapper(expectedMenuObjectWrapper);

            return expectedMenuObjectWrapper;
        }

        public ExpectedMenuDataStoreItemsCache AddMenuItemCollectionAndContinue(long id, int? priority = null,
            bool usesMenuSeparator = false, long? parentId = null)
        {
            _ = ContinueWithMenuItemCollection(id, priority, usesMenuSeparator, parentId);
            return this;
        }

        private void AddMenuObjectWrapper(ExpectedMenuObjectWrapper expectedMenuObjectWrapper)
        {
            _allDataStoreItemWrappers.Add(expectedMenuObjectWrapper);

            if (expectedMenuObjectWrapper.NonCopyMenuObject is not ICanHaveParent canHaveParent || canHaveParent.ParentId == null)
            {
                Assert.IsFalse(_topLevelDataStoreItemWrappers.Any(x => x.NonCopyMenuObject.Id == expectedMenuObjectWrapper.NonCopyMenuObject.Id));
                _topLevelDataStoreItemWrappers.Add(expectedMenuObjectWrapper);
            }
        }

        [Obsolete("This method is added only to be called from ExpectedMenuObjectWrapper.")]
        public void AddAddMenuObjectWrapperToCache(ExpectedMenuObjectWrapper expectedMenuObjectWrapper)
        {
            AddMenuObjectWrapper(expectedMenuObjectWrapper);
        }

        public void AssertEqualTo(IDataStoreItemsCache<INonCopyMenuObject, MenuDataObjectWrapper> dataStoreItemsCache)
        {
            Assert.AreEqual(this.DataStoreId, dataStoreItemsCache.DataStoreId);

            Assert.AreEqual(TopLevelDataStoreItemWrappers.Count, dataStoreItemsCache.TopLevelDataStoreItemWrappers.Count);
            for (var i = 0; i < TopLevelDataStoreItemWrappers.Count; ++i)
            {
                var expectedTopLevelItemWrapper = TopLevelDataStoreItemWrappers[i];
                var actualTopLevelItemWrapper = dataStoreItemsCache.TopLevelDataStoreItemWrappers[i];
                expectedTopLevelItemWrapper.AssertEqualTo(actualTopLevelItemWrapper);
                Assert.Contains(expectedTopLevelItemWrapper, _allDataStoreItemWrappers);
            }
        }
    }
}

