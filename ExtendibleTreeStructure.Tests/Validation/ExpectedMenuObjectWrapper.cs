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
using NUnit.Framework;

namespace ExtendibleTreeStructure.Tests.Validation
{
    public class ExpectedMenuObjectWrapper
    {
        public ExpectedMenuObjectWrapper(ExpectedMenuDataStoreItemsCache expectedMenuDataStoreItemsCache,
            INonCopyMenuObject nonCopyMenuObject, ExpectedMenuObjectWrapper? parent = null)
        {
            ExpectedMenuDataStoreItemsCache = expectedMenuDataStoreItemsCache;
            NonCopyMenuObject = nonCopyMenuObject;
            Parent = parent;

            if (parent != null)
            {
                this.Parent = parent;
                parent.Children.Add(this);
            }
        }

        public ExpectedMenuDataStoreItemsCache ExpectedMenuDataStoreItemsCache { get; }
        public INonCopyMenuObject NonCopyMenuObject { get; }
        public List<ExpectedMenuObjectWrapper> Children { get; } = new();

        public ExpectedMenuObjectWrapper? Parent { get; }

        public ExpectedMenuObjectWrapper ContinueWithChildMenuBarItem(long commandId, int? priority = null, bool addToCache = true)
        {
            var expectedMenuObjectWrapper = new ExpectedMenuObjectWrapper(ExpectedMenuDataStoreItemsCache, new MenuBarItemData(commandId, NonCopyMenuObject.Id)
            {
                Priority = priority

            }, this);

            if (addToCache)
#pragma warning disable CS0618 // Type or member is obsolete
                this.ExpectedMenuDataStoreItemsCache.AddAddMenuObjectWrapperToCache(expectedMenuObjectWrapper);
#pragma warning restore CS0618 // Type or member is obsolete

            return expectedMenuObjectWrapper;
        }

        public ExpectedMenuObjectWrapper AddChildMenuBarItemAndContinue(long commandId, int? priority = null, bool addToCache = true)
        {
            _ = ContinueWithChildMenuBarItem(commandId, priority, addToCache);
            return this;
        }

        public ExpectedMenuObjectWrapper ContinueWithChildMenuItem(long commandId, int? priority = null, bool addToCache = true)
        {
            var expectedMenuObjectWrapper = new ExpectedMenuObjectWrapper(ExpectedMenuDataStoreItemsCache, new MenuItemData(commandId, NonCopyMenuObject.Id)
            {
                Priority = priority
            }, this);

            if (addToCache)
#pragma warning disable CS0618 // Type or member is obsolete
                this.ExpectedMenuDataStoreItemsCache.AddAddMenuObjectWrapperToCache(expectedMenuObjectWrapper);
#pragma warning restore CS0618 // Type or member is obsolete

            return expectedMenuObjectWrapper;
        }

        public ExpectedMenuObjectWrapper AddChildMenuItemAndContinue(long commandId, int? priority = null, bool addToCache = true)
        {
            _ = ContinueWithChildMenuItem(commandId, priority, addToCache);
            return this;
        }

        public ExpectedMenuObjectWrapper ContinueWithChildMenuItemCollection(long commandId,
            int? priority = null,
            bool usesMenuSeparator = false, bool addToCache = true)
        {
            var expectedMenuObjectWrapper = new ExpectedMenuObjectWrapper(ExpectedMenuDataStoreItemsCache,
                new MenuItemCollection(commandId, NonCopyMenuObject.Id, usesMenuSeparator)
                {
                    Priority = priority
                }, this);

            if (addToCache)
#pragma warning disable CS0618 // Type or member is obsolete
                this.ExpectedMenuDataStoreItemsCache.AddAddMenuObjectWrapperToCache(expectedMenuObjectWrapper);
#pragma warning restore CS0618 // Type or member is obsolete

            return expectedMenuObjectWrapper;
        }

        public ExpectedMenuObjectWrapper AddChildMenuItemCollectionAndContinue(long commandId, int? priority = null, bool addToCache = true)
        {
            _ = ContinueWithChildMenuItemCollection(commandId, priority, addToCache);
            return this;
        }

        public ExpectedMenuObjectWrapper MoveToParent(int numberOfParentsToMoveUp = 1)
        {
            ExpectedMenuObjectWrapper? currentParent = this.Parent;

            Assert.IsNotNull(currentParent);

            var currentNumberOfLevels = 1;

            while (currentParent != null && currentNumberOfLevels++ < numberOfParentsToMoveUp)
                currentParent = currentParent.Parent;

            if (currentParent == null)
                throw new System.Exception("Failed to move to parent.");

            return currentParent;
        }

        public void AssertEqualTo(MenuDataObjectWrapper dataStoreItemWrapper, bool isParentNulledOut = false)
        {
            Assert.AreEqual(NonCopyMenuObject.GetType(), dataStoreItemWrapper.DataStoreItem.GetType());

            dataStoreItemWrapper.DataStoreItem.IsParentNulledOut = isParentNulledOut;
            Assert.IsTrue(NonCopyMenuObject.Equals(dataStoreItemWrapper.DataStoreItem));
            Assert.AreEqual(this.ExpectedMenuDataStoreItemsCache.DataStoreId, dataStoreItemWrapper.DataStoreId);

            if (this.NonCopyMenuObject is ICanHaveParent canHaveParent1)
            {
                Assert.IsInstanceOf<ICanHaveParent>(dataStoreItemWrapper.DataStoreItem);

                var canHaveParent2 = (ICanHaveParent) dataStoreItemWrapper.DataStoreItem;

                var parentIsNull = isParentNulledOut;

                if (!isParentNulledOut)
                {
                    Assert.AreEqual(canHaveParent1.ParentId, canHaveParent2.ParentId);

                    if (this.Parent != null)
                    {
                        Assert.IsNotNull(dataStoreItemWrapper.Parent);
                        Assert.AreEqual(this.Parent.NonCopyMenuObject.Id, dataStoreItemWrapper.Parent!.DataStoreItem.Id);
                    }
                    else
                    {
                        parentIsNull = true;
                    }
                }

                if (parentIsNull)
                {
                    Assert.IsNull(dataStoreItemWrapper.Parent);
                    Assert.IsNull(canHaveParent2.ParentId);
                }
            }
            else
            {
                Assert.IsNull(dataStoreItemWrapper.Parent);
                Assert.IsNotInstanceOf<ICanHaveParent>(dataStoreItemWrapper.DataStoreItem);
            }

            Assert.AreEqual(Children.Count, dataStoreItemWrapper.Children.Count);

            for (var i = 0; i < Children.Count; ++i)
            {
                this.Children[i].AssertEqualTo(dataStoreItemWrapper.Children[i]);
            }
        }

        public override string ToString()
        {
            return $"Expected wrapper for {this.NonCopyMenuObject}, DataToreId={this.ExpectedMenuDataStoreItemsCache.DataStoreId}, {base.ToString()}";
        }
    }
}