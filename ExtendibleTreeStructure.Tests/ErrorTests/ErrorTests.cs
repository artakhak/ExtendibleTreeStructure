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

using ExtendibleTreeStructure.MessageLogging;
using ExtendibleTreeStructure.Tests.MenuItems;
using ExtendibleTreeStructure.Tests.Validation;
using NUnit.Framework;
using System.Collections.Generic;

namespace ExtendibleTreeStructure.Tests.ErrorTests
{
    [TestFixture]
    internal class ErrorTests
    {
        [SetUp]
        public void TestInitialize()
        {
            TestHelpers.RegisterLogger();
        }

        [Test]
        public void InvalidParentIdsTest()
        {
            var expectedDataStoresCache = new ExpectedDataStoresCache();

            expectedDataStoresCache.AddDataStoreItemsCache(MenuIds.SharedMenuObjects)
                .ContinueWithMenuItem(CommandIds.RecentFiles)
                .AddChildMenuItemAndContinue(CommandIds.RecentLocations);

            expectedDataStoresCache.AddDataStoreItemsCache(MenuIds.SharedMenuObjects2)
                .AddMenuBarAndContinue(MenuIds.VisibleCommandBars);

            var dataStoresCache = TestHelpers.LoadTestDataStoresCacheForErrorTests(
                @"ErrorTests\TestFiles\InvalidParentIds.xml", expectedDataStoresCache, 2,
                loggedMessages =>
                {
                    TestHelpers.ValidateLoggedMessages(new List<ILoggedMessage>
                    {
                        new LoggedMessage(MessageType.InvalidDataStoreItemParent, "references an invalid or missing 'ParentId', or the parent had errors.",
                            MenuIds.SharedMenuObjects2,
                            new MenuItemData(CommandIds.Errors, CommandIds.EnterPresentationMode), MessageCategory.Error),

                        new LoggedMessage(MessageType.InvalidChildDataStoreItem, "cannot be used as a child for",
                            MenuIds.SharedMenuObjects2,
                            new MenuItemData(CommandIds.RecentFiles, MenuIds.VisibleCommandBars), MessageCategory.Error)

                    }, loggedMessages);
                },
                true);

            // Even though parent ids are invalid for some neu item, these menu items are still added 
            // to dictionary in dataStoresCache (without paernt data) and can be references using the TryGetDataStoreItem() method.
            Assert.IsTrue(dataStoresCache.TryGetDataStoreItem(MenuIds.SharedMenuObjects, CommandIds.RecentFiles, out _));

            Assert.IsTrue(dataStoresCache.TryGetDataStoreItem(MenuIds.SharedMenuObjects, CommandIds.RecentLocations, out _));

            Assert.IsTrue(dataStoresCache.TryGetDataStoreItem(MenuIds.SharedMenuObjects2, MenuIds.VisibleCommandBars, out _));

            Assert.IsTrue(dataStoresCache.TryGetDataStoreItem(MenuIds.SharedMenuObjects2, CommandIds.RecentFiles, out _));
            Assert.IsTrue(dataStoresCache.TryGetDataStoreItem(MenuIds.SharedMenuObjects2, CommandIds.Errors, out _));
            Assert.IsTrue(dataStoresCache.TryGetDataStoreItem(MenuIds.SharedMenuObjects2, CommandIds.Projects, out _));
        }

        [Test]
        public void InvalidChildDataStoreItemsTest()
        {
            var expectedDataStoresCache = new ExpectedDataStoresCache();

            expectedDataStoresCache.AddDataStoreItemsCache(MenuIds.SharedMenuObjects)
                .ContinueWithMenuBar(MenuIds.CSharpFileMenuObjectsMenuBar)
                .AddChildMenuBarItemAndContinue(CommandIds.Projects);

            expectedDataStoresCache.AddDataStoreItemsCache(MenuIds.SharedMenuObjects2)
                .ContinueWithMenuBar(MenuIds.CSharpFileMenuObjectsMenuBar)
                .AddChildMenuBarItemAndContinue(CommandIds.Projects)
                .AddChildMenuBarItemAndContinue(CommandIds.FileMenuBarItem);

            expectedDataStoresCache.AddDataStoreItemsCache(MenuIds.NoFileSelectedMenuObjects)
                .ContinueWithMenuBar(MenuIds.CSharpFileMenuObjectsMenuBar)
                .AddChildMenuBarItemAndContinue(CommandIds.Projects)
                .AddChildMenuBarItemAndContinue(CommandIds.FileMenuBarItem)
                .AddChildMenuBarItemAndContinue(CommandIds.BuildSolution);

            var dataStoresCache = TestHelpers.LoadTestDataStoresCacheForErrorTests(
                @"ErrorTests\TestFiles\InvalidChildDataStoreItems.xml", expectedDataStoresCache, 3,
                loggedMessages =>
                {
                    var invalidChildDataStoreItemMessage = "cannot be used as a child for";
                    TestHelpers.ValidateLoggedMessages(new List<ILoggedMessage>
                    {
                        new LoggedMessage(MessageType.InvalidChildDataStoreItem, invalidChildDataStoreItemMessage,
                            MenuIds.SharedMenuObjects,
                            new MenuItemData(CommandIds.Git, MenuIds.CSharpFileMenuObjectsMenuBar), MessageCategory.Error),
                        new LoggedMessage(MessageType.InvalidChildDataStoreItem, invalidChildDataStoreItemMessage,
                            MenuIds.SharedMenuObjects2,
                            new MenuItemData(CommandIds.EnterPresentationMode, MenuIds.CSharpFileMenuObjectsMenuBar), MessageCategory.Error),
                        new LoggedMessage(MessageType.InvalidChildDataStoreItem, invalidChildDataStoreItemMessage,
                            MenuIds.NoFileSelectedMenuObjects,
                            new MenuItemData(CommandIds.SaveFile, MenuIds.CSharpFileMenuObjectsMenuBar), MessageCategory.Error)
                    }, loggedMessages);

                });

            // Even though CommandIds.Git, CommandIds.EnterPresentationMode, and CommandIds.SaveFile are not added under 
            // MenuIds.CSharpFileMenuObjectsMenuBar in data stores MenuIds.SharedMenuObjects, MenuIds.SharedMenuObjects2, and 
            // MenuIds.NoFileSelectedMenuObjects (which is validated by expectedDataStoresCache), these data store items 
            // are added to dictionary in dataStoresCache and can be references using the TryGetDataStoreItem() method.
            Assert.IsTrue(dataStoresCache.TryGetDataStoreItem(MenuIds.SharedMenuObjects, CommandIds.Git, out _));
            Assert.IsTrue(dataStoresCache.TryGetDataStoreItem(MenuIds.SharedMenuObjects2, CommandIds.EnterPresentationMode, out _));
            Assert.IsTrue(dataStoresCache.TryGetDataStoreItem(MenuIds.NoFileSelectedMenuObjects, CommandIds.SaveFile, out _));
        }

        [Test]
        public void InvalidTopLevelDataStoreItemsTest()
        {
            var expectedDataStoresCache = new ExpectedDataStoresCache();

            expectedDataStoresCache.AddDataStoreItemsCache(MenuIds.SharedMenuObjects)
                .ContinueWithMenuItem(CommandIds.Projects)
                .AddChildMenuItemAndContinue(CommandIds.Errors);

            expectedDataStoresCache.AddDataStoreItemsCache(MenuIds.SharedMenuObjects2)
                .ContinueWithMenuItem(CommandIds.Projects)
                .AddChildMenuItemAndContinue(CommandIds.Errors);

            expectedDataStoresCache.AddDataStoreItemsCache(MenuIds.NoFileSelectedMenuObjects)
                .ContinueWithMenuBarItem(CommandIds.FileMenuBarItem)
                .ContinueWithChildMenuItem(CommandIds.Git)
                .AddChildMenuItemAndContinue(CommandIds.RecentSubmissions);

            var dataStoresCache = TestHelpers.LoadTestDataStoresCacheForErrorTests(
                @"ErrorTests\TestFiles\InvalidTopLevelDataStoreItems.xml", expectedDataStoresCache, 3,
                loggedMessages =>
                {
                    var errorMessage = "cannot be used as top level data store item.";
                    TestHelpers.ValidateLoggedMessages(new List<ILoggedMessage>
                    {
                        new LoggedMessage(MessageType.Custom, errorMessage,
                            MenuIds.SharedMenuObjects,
                            new MenuItemData(CommandIds.Git), MessageCategory.Error),
                        new LoggedMessage(MessageType.Custom, errorMessage,
                            MenuIds.SharedMenuObjects2,
                            new MenuItemData(CommandIds.Git), MessageCategory.Error),
                        new LoggedMessage(MessageType.Custom, errorMessage,
                            MenuIds.NoFileSelectedMenuObjects,
                            new MenuItemData(CommandIds.Git), MessageCategory.Error)

                    }, loggedMessages);

                },
                true,
                dataStores =>
                {
                    return new TestDataStoresCache(dataStores,
                        new TestMenuDataObjectWrapperFactory(
                            (dataStoreId, dataStoreItem, parent) =>
                            {
                                if (dataStoreItem.Id == CommandIds.Git &&
                                    (dataStoreItem is not ICanHaveParent canHaveParent || canHaveParent.ParentId == null))
                                {
                                    return (null, new LoggedMessage(MessageType.Custom,
                                        $"[{dataStoreItem.GetDisplayValue(dataStoreItem.Id)}] with type '{dataStoreItem.GetType()}' cannot be used as top level data store item.",
                                        dataStoreId, dataStoreItem, MessageCategory.Error));
                                }

                                return (new MenuDataObjectWrapper(dataStoreId, dataStoreItem, parent), null);
                            }));
                });

            Assert.IsFalse(dataStoresCache.TryGetDataStoreItem(MenuIds.SharedMenuObjects, CommandIds.Git, out _));
            Assert.IsFalse(dataStoresCache.TryGetDataStoreItem(MenuIds.SharedMenuObjects2, CommandIds.Git, out _));
            Assert.IsFalse(dataStoresCache.TryGetDataStoreItem(MenuIds.NoFileSelectedMenuObjects, CommandIds.Git, out _));
        }


        [Test]
        public void InvalidCopiedDataStoreItemTest()
        {
            var expectedDataStoresCache = new ExpectedDataStoresCache();

            expectedDataStoresCache.AddDataStoreItemsCache(MenuIds.SharedMenuObjects)
                .ContinueWithMenuItem(CommandIds.Projects)
                .AddChildMenuItemAndContinue(CommandIds.Errors)
                .ExpectedMenuDataStoreItemsCache
                .ContinueWithMenuItem(CommandIds.SaveFile)
                .AddChildMenuItemAndContinue(CommandIds.SaveFileAs);

            expectedDataStoresCache.AddDataStoreItemsCache(MenuIds.SharedMenuObjects2)
                .ContinueWithMenuBarItem(CommandIds.BuildMenuBarItem);

            expectedDataStoresCache.AddDataStoreItemsCache(MenuIds.NoFileSelectedMenuObjects)
                .ContinueWithMenuBarItem(CommandIds.BuildMenuBarItem);

            TestHelpers.LoadTestDataStoresCacheForErrorTests(
                @"ErrorTests\TestFiles\InvalidCopiedDataStoreItem.xml", expectedDataStoresCache, 6,
                loggedMessages =>
                {
                    var invalidCopiedDataStoreMessage = "Data store with Id=";
                    var invalidCopiedDataStoreItemMessage = "Data store item with Id=";
                    var failedToCopyDataStoreItemErrorMessage = "Failed to copy data store item referenced by";

                    TestHelpers.ValidateLoggedMessages(new List<ILoggedMessage>
                    {
                        new LoggedMessage(MessageType.InvalidCopiedDataStoreItem, invalidCopiedDataStoreMessage,
                            MenuIds.SharedMenuObjects2,
                            new CopyMenuObject(MenuIds.RecentObjectsGroup, CommandIds.Projects, null), MessageCategory.Error),

                        new LoggedMessage(MessageType.InvalidCopiedDataStoreItem, invalidCopiedDataStoreItemMessage,
                            MenuIds.SharedMenuObjects2,
                            new CopyMenuObject(MenuIds.SharedMenuObjects, CommandIds.EncodingUtf8, null), MessageCategory.Error),

                        new LoggedMessage(MessageType.InvalidCopiedDataStoreItem, invalidCopiedDataStoreMessage,
                            MenuIds.SharedMenuObjects2,
                            new CopyMenuObject(MenuIds.RecentObjectsGroup, CommandIds.BuildProject,
                                CommandIds.BuildMenuBarItem), MessageCategory.Error),
                        new LoggedMessage(MessageType.InvalidCopiedDataStoreItem, invalidCopiedDataStoreItemMessage,
                            MenuIds.SharedMenuObjects2,
                            new CopyMenuObject(MenuIds.SharedMenuObjects, CommandIds.BuildSolution,
                                CommandIds.BuildMenuBarItem), MessageCategory.Error),

                        new LoggedMessage(MessageType.FailedToCopyDataStoreItem, failedToCopyDataStoreItemErrorMessage,
                            MenuIds.NoFileSelectedMenuObjects,
                            new CopyMenuObject(MenuIds.SharedMenuObjects2, CommandIds.EncodingUtf8, null),
                            MessageCategory.Error),

                        new LoggedMessage(MessageType.FailedToCopyDataStoreItem, failedToCopyDataStoreItemErrorMessage,
                            MenuIds.NoFileSelectedMenuObjects,
                            new CopyMenuObject(MenuIds.SharedMenuObjects2, CommandIds.BuildSolution,
                                CommandIds.BuildMenuBarItem), MessageCategory.Error)

                    }, loggedMessages);

                });
        }

        [Test]
        public void CustomErrorsTest()
        {
            var expectedDataStoresCache = new ExpectedDataStoresCache();

            expectedDataStoresCache.AddDataStoreItemsCache(MenuIds.SharedMenuObjects);
            expectedDataStoresCache.AddDataStoreItemsCache(MenuIds.SharedMenuObjects2)
                .ContinueWithMenuItem(CommandIds.Errors);

            string GetExpectedErrorMessage(long commandId) => $"{TestHelpers.GetConstantNameForLogs(commandId)} is invalid.";

            TestHelpers.LoadTestDataStoresCacheForErrorTests(
                @"ErrorTests\TestFiles\CustomErrors.xml", expectedDataStoresCache, 5,
                loggedMessages =>
                {
                    TestHelpers.ValidateLoggedMessages(new List<ILoggedMessage>
                    {
                        new LoggedMessage(MessageType.Custom, GetExpectedErrorMessage(CommandIds.Projects),
                            MenuIds.SharedMenuObjects,
                            new MenuItemData(CommandIds.Projects), MessageCategory.Error),

                        new LoggedMessage(MessageType.Custom, GetExpectedErrorMessage(CommandIds.RecentFiles),
                            MenuIds.SharedMenuObjects,
                            new MenuItemData(CommandIds.RecentFiles), MessageCategory.Error),

                        new LoggedMessage(MessageType.Custom, GetExpectedErrorMessage(CommandIds.Projects),
                            MenuIds.SharedMenuObjects2,
                            new MenuItemData(CommandIds.Projects), MessageCategory.Error),

                        new LoggedMessage(MessageType.Custom, GetExpectedErrorMessage(CommandIds.RecentFiles),
                            MenuIds.SharedMenuObjects2,
                            new MenuItemData(CommandIds.RecentFiles, CommandIds.Errors), MessageCategory.Error),

                        new LoggedMessage(MessageType.Custom, GetExpectedErrorMessage(CommandIds.RecentFiles),
                            MenuIds.SharedMenuObjects2,
                            new MenuItemData(CommandIds.RecentFiles), MessageCategory.Error)
                    }, loggedMessages);
                },
                true,
                menuDataStores =>
                {
                    return new TestDataStoresCache(menuDataStores,
                        new TestMenuDataObjectWrapperFactory(
                            (dataStoreId, dataStoreItem, parent) =>
                            {
                                switch (dataStoreItem.Id)
                                {
                                    case CommandIds.Projects:
                                    case CommandIds.RecentFiles:
                                        return (null, new LoggedMessage(MessageType.Custom,
                                            GetExpectedErrorMessage(dataStoreItem.Id),
                                            dataStoreId, dataStoreItem, MessageCategory.Error));
                                }

                                return (new MenuDataObjectWrapper(dataStoreId, dataStoreItem, parent), null);
                            }));
                });
        }

        [Test]
        public void DuplicateDataStoreIdsTest()
        {
            var expectedDataStoresCache = new ExpectedDataStoresCache();

            expectedDataStoresCache.AddDataStoreItemsCache(MenuIds.SharedMenuObjects)
                .ContinueWithMenuItem(CommandIds.Errors);

            expectedDataStoresCache.AddDataStoreItemsCache(MenuIds.SharedMenuObjects2)
                .ContinueWithMenuItem(CommandIds.Projects)
                .AddChildMenuItemAndContinue(CommandIds.Errors);

            TestHelpers.LoadTestDataStoresCacheForErrorTests(
                @"ErrorTests\TestFiles\DuplicateDataStoreIds.xml", expectedDataStoresCache, 1,
                loggedMessages =>
                {
                    TestHelpers.ValidateLoggedMessages(new List<ILoggedMessage>
                    {
                        new LoggedMessage(MessageType.DataStoreAppearsMultipleTimes, "appears multiple times.",
                            MenuIds.SharedMenuObjects, null, MessageCategory.Error)

                    }, loggedMessages);
                });
        }

        [Test]
        public void DuplicateDataStoreItemIdsInSameDataStoreTest()
        {
            var expectedDataStoresCache = new ExpectedDataStoresCache();

            expectedDataStoresCache.AddDataStoreItemsCache(MenuIds.SharedMenuObjects)
                .ContinueWithMenuItem(CommandIds.Projects)
                .AddChildMenuItemAndContinue(CommandIds.Errors);

            expectedDataStoresCache.AddDataStoreItemsCache(MenuIds.SharedMenuObjects2)
                .ContinueWithMenuItem(CommandIds.Projects)
                .AddChildMenuItemAndContinue(CommandIds.Errors);

            TestHelpers.LoadTestDataStoresCacheForErrorTests(
                @"ErrorTests\TestFiles\DuplicateDataStoreItemIdsInSameDataStore.xml", expectedDataStoresCache, 1,
                loggedMessages =>
                {
                    TestHelpers.ValidateLoggedMessages(new List<ILoggedMessage>
                    {
                        new LoggedMessage(MessageType.DataStoreItemAppearsMultipleTimesInSameDataStore, "uses an Id that was already used in this data store.",
                            MenuIds.SharedMenuObjects,
                            new MenuItemData(CommandIds.Projects), MessageCategory.Error)

                    }, loggedMessages);
                });
        }

        [Test]
        public void ParentIdReferencesItselfTest()
        {
            var expectedDataStoresCache = new ExpectedDataStoresCache();

            expectedDataStoresCache.AddDataStoreItemsCache(MenuIds.SharedMenuObjects);

            expectedDataStoresCache.AddDataStoreItemsCache(MenuIds.SharedMenuObjects2)
                .ContinueWithMenuItem(CommandIds.Projects);

            TestHelpers.LoadTestDataStoresCacheForErrorTests(
                @"ErrorTests\TestFiles\ParentIdReferencesItself.xml", expectedDataStoresCache, 2,
                loggedMessages =>
                {
                    TestHelpers.ValidateLoggedMessages(new List<ILoggedMessage>
                    {
                        new LoggedMessage(MessageType.ParentIdReferencesItself, "will be removed since it references itself via ParentId property",
                            MenuIds.SharedMenuObjects,
                            new MenuItemData(CommandIds.Errors, CommandIds.Errors), MessageCategory.Error),
                        new LoggedMessage(MessageType.InvalidCopiedDataStoreItem, "Data store item with Id",
                            MenuIds.SharedMenuObjects2,
                            new CopyMenuObject(MenuIds.SharedMenuObjects,
                                CommandIds.Errors, CommandIds.Projects), MessageCategory.Error)

                    }, loggedMessages);
                });
        }

        [Test]
        public void CopyDataStoreItemReferencesItselfTest()
        {
            var expectedDataStoresCache = new ExpectedDataStoresCache();

            expectedDataStoresCache.AddDataStoreItemsCache(MenuIds.SharedMenuObjects)
                .AddMenuItemAndContinue(CommandIds.Errors)
                .AddMenuItemAndContinue(CommandIds.ErrorsCopy);

            expectedDataStoresCache.AddDataStoreItemsCache(MenuIds.SharedMenuObjects2);

            TestHelpers.LoadTestDataStoresCacheForErrorTests(
                @"ErrorTests\TestFiles\CopyDataStoreItemReferencesItself.xml",
                expectedDataStoresCache, 3,
                loggedMessages =>
                {
                    TestHelpers.ValidateLoggedMessages(new List<ILoggedMessage>
                    {
                        new LoggedMessage(MessageType.CopyDataStoreItemReferencesItself, " references itself.",
                            MenuIds.SharedMenuObjects,
                            new CopyMenuObject(MenuIds.SharedMenuObjects, CommandIds.Projects, null), MessageCategory.Error),

                        new LoggedMessage(MessageType.InvalidCopiedDataStoreItem, " not found in data store with Id=",
                            MenuIds.SharedMenuObjects2,
                            new CopyMenuObject(MenuIds.SharedMenuObjects, CommandIds.Projects, null), MessageCategory.Error),

                        new LoggedMessage(MessageType.InvalidDataStoreItemParent, "references an invalid or missing 'ParentId', or the parent had errors.",
                            MenuIds.SharedMenuObjects,
                            new MenuItemData(CommandIds.BuildSolution, CommandIds.Projects), MessageCategory.Error)

                    }, loggedMessages);
                });
        }

        [Test]
        public void DataStoreItemCannotBeCopiedWithNewParentTest()
        {
            List<IDataStore<IMenuObject>> dataStores = new List<IDataStore<IMenuObject>>();

            var dataStoreItems1 = new List<IMenuObject>();
            dataStoreItems1.Add(new MenuObjectWithNoCloneWithNewParent(CommandIds.Errors));

            IDataStore<IMenuObject> dataStore1 = new DataStore<IMenuObject>(MenuIds.SharedMenuObjects, dataStoreItems1);
            dataStores.Add(dataStore1);

            var dataStoreItems2 = new List<IMenuObject>
            {
                new MenuItemData(CommandIds.Projects),
                new CopyMenuObject(MenuIds.SharedMenuObjects, CommandIds.Errors, CommandIds.Projects)
            };

            IDataStore<IMenuObject> dataStore2 = new DataStore<IMenuObject>(MenuIds.SharedMenuObjects2, dataStoreItems2);
            dataStores.Add(dataStore2);

            var dataStoresCache = TestHelpers.LoadTestDataStoresCache(dataStores, null,
                @"ErrorTests\TestFiles\DataStoreItemCannotBeCopiedWithNewParent.processed.xml",
                false,
                null,
                loggedMessages =>
                {
                    Assert.AreEqual(1, loggedMessages.Count);

                    TestHelpers.ValidateLoggedMessage(new LoggedMessage(MessageType.CopiedDataStoreItemDoesNotImplementInterface,
                            " since the data store item does not implement interface",
                            MenuIds.SharedMenuObjects2,
                            new CopyMenuObject(MenuIds.SharedMenuObjects, CommandIds.Errors, CommandIds.Projects), MessageCategory.Error),
                        loggedMessages[0]);
                });

            Assert.IsTrue(dataStoresCache.TryGetDataStoreItem(MenuIds.SharedMenuObjects, CommandIds.Errors, out var dataStoreItemWrapper));
            Assert.AreSame(dataStoreItemWrapper!.DataStoreItem, dataStoreItems1[0]);

            Assert.IsTrue(dataStoresCache.TryGetDataStoreItem(MenuIds.SharedMenuObjects2, CommandIds.Projects, out var dataStoreItemWrapper2));
            Assert.AreSame(dataStoreItemWrapper2!.DataStoreItem, dataStoreItems2[0]);

            Assert.AreEqual(0, dataStoreItemWrapper2.Children.Count);

            Assert.IsFalse(dataStoresCache.TryGetDataStoreItem(MenuIds.SharedMenuObjects2, CommandIds.Errors, out _));
        }

        [Test]
        public void CanCopyOnlyItemsInDataStoreTest()
        {
            var expectedDataStoresCache = new ExpectedDataStoresCache();

            expectedDataStoresCache.AddDataStoreItemsCache(MenuIds.SharedMenuObjects)
                .ContinueWithMenuItemCollection(MenuIds.RecentObjectsGroup)
                .AddChildMenuItemAndContinue(CommandIds.RecentFiles)
                .AddChildMenuItemAndContinue(CommandIds.RecentlyChangedFiles)
                .AddChildMenuItemAndContinue(CommandIds.RecentLocations);

            expectedDataStoresCache.AddDataStoreItemsCache(MenuIds.SharedMenuObjects2)
                .ContinueWithMenuItem(CommandIds.ToolsSubmenuItem)
                .ContinueWithChildMenuItemCollection(MenuIds.RecentObjectsGroup)
                .ContinueWithChildMenuItem(CommandIds.RecentFiles)
                .AddChildMenuItemAndContinue(CommandIds.RecentChanges)

                .MoveToParent(1)
                .ContinueWithChildMenuItem(CommandIds.RecentlyChangedFiles)
                .AddChildMenuItemAndContinue(CommandIds.RecentSubmissions)
                .MoveToParent(1)
                .AddChildMenuItemAndContinue(CommandIds.RecentLocations)

                .ExpectedMenuDataStoreItemsCache
                .ContinueWithMenuItem(CommandIds.RecentFiles)
                .AddChildMenuItemAndContinue(CommandIds.RecentChanges);

            expectedDataStoresCache.AddDataStoreItemsCache(MenuIds.NoFileSelectedMenuObjects)
                .ContinueWithMenuItemCollection(MenuIds.RecentObjectsGroup)
                .ContinueWithChildMenuItem(CommandIds.RecentFiles)
                .AddChildMenuItemAndContinue(CommandIds.RecentChanges)

                .MoveToParent(1)
                .ContinueWithChildMenuItem(CommandIds.RecentlyChangedFiles)
                .AddChildMenuItemAndContinue(CommandIds.RecentSubmissions)
                .MoveToParent(1)
                .AddChildMenuItemAndContinue(CommandIds.RecentLocations);

            var dataStoresCache = TestHelpers.LoadTestDataStoresCacheForErrorTests(
                @"ErrorTests\TestFiles\CanCopyOnlyItemsInDataStore.xml", expectedDataStoresCache, 2,
                loggedMessages =>
                {
                    var invalidCopiedDataStoreItemError = "not found in data store with Id=";
                    TestHelpers.ValidateLoggedMessages(new List<ILoggedMessage>
                    {
                        new LoggedMessage(MessageType.InvalidCopiedDataStoreItem, invalidCopiedDataStoreItemError,
                            MenuIds.SharedMenuObjects2,
                            new CopyMenuObject(CommandIds.RecentlyChangedFilesCopy,
                                MenuIds.SharedMenuObjects2,
                                CommandIds.RecentlyChangedFiles, null), MessageCategory.Error),

                        new LoggedMessage(MessageType.InvalidCopiedDataStoreItem, invalidCopiedDataStoreItemError,
                            MenuIds.NoFileSelectedMenuObjects,
                            new CopyMenuObject(
                                MenuIds.SharedMenuObjects2,
                                CommandIds.RecentLocations, null), MessageCategory.Error)
                    }, loggedMessages);
                });

            // Children of items copied from different data store cannot be referenced using TryGetDataStoreItem(). Only items 
            // defined in data store (including the copy data store items) can be referenced
            Assert.IsFalse(dataStoresCache.TryGetDataStoreItem(MenuIds.SharedMenuObjects2, CommandIds.RecentlyChangedFiles, out _));
            Assert.IsFalse(dataStoresCache.TryGetDataStoreItem(MenuIds.SharedMenuObjects2, CommandIds.RecentLocations, out _));
        }
    }
}