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
using System;
using System.Collections.Generic;

namespace ExtendibleTreeStructure.Tests.SuccessfulCacheBuildTests
{
    [TestFixture]
    internal class SuccessfulCacheBuildTests
    {

        [SetUp]
        public void TestInitialize()
        {
            TestHelpers.RegisterLogger();
        }

        [Test]
        public void SimpleDataStoresCacheLoadDemoTest()
        {
            var menuDataStores = new List<IDataStore<IMenuObject>>();

            menuDataStores.Add(new DataStore<IMenuObject>(MenuIds.SharedMenuObjects,
                new List<IMenuObject>
                {
                    new MenuItemCollection(MenuIds.SharedToolsCollection),
                    new MenuItemData(CommandIds.Projects, MenuIds.SharedToolsCollection),
                    new MenuItemData(CommandIds.Errors, MenuIds.SharedToolsCollection),
                }));

            menuDataStores.Add(new DataStore<IMenuObject>(MenuIds.NoFileSelectedMenuObjects,
                new List<IMenuObject>
                {
                    new MenuBarData(MenuIds.NoFileSelectedDefaultMenuBar),
                    new MenuBarItemData(CommandIds.ViewMenuBarItem, MenuIds.NoFileSelectedDefaultMenuBar),

                    new MenuItemData(CommandIds.ToolsSubmenuItem, CommandIds.ViewMenuBarItem),

                    // Copy SharedToolsCollection from SharedMenuObjects data store and add the copy to CommandIds.ToolsSubmenuItem as a new child.
                    new CopyMenuObject(MenuIds.SharedMenuObjects, MenuIds.SharedToolsCollection,
                        CommandIds.ToolsSubmenuItem),

                    // Add new child CommandIds.Git to child CommandIds.Projects of MenuIds.SharedToolsCollection copied from data store
                    // MenuIds.SharedToolsCollection.
                    new MenuItemData(CommandIds.Git, CommandIds.Projects)
                }));

            menuDataStores.Add(new DataStore<IMenuObject>(MenuIds.TextFileMenuObjects,
                new List<IMenuObject>
                {
                    // Copy the menu bar NoFileSelectedDefaultMenuBar from MenuIds.NoFileSelectedMenuObjects data store
                    // and add new children top copied data store item.
                    new CopyMenuObject(MenuIds.NoFileSelectedMenuObjects, MenuIds.NoFileSelectedDefaultMenuBar,
                        null),

                    // Add child CommandIds.RecentLocations to CommandIds.Projects, which is copied from MenuIds.NoFileSelectedMenuObjects
                    // as a child of MenuIds.SharedToolsCollection.
                    new MenuItemData(CommandIds.RecentLocations, CommandIds.Projects)
                    {
                        // CommandIds.RecentLocations has a priority 0 and will appear before 
                        // other siblings in Project (default value is null) 
                        Priority = 0
                    },

                    // Add new menu bar item under menu bar MenuIds.NoFileSelectedDefaultMenuBar copied from data store 
                    // MenuIds.NoFileSelectedMenuObjects.
                    new MenuBarItemData(CommandIds.BuildMenuBarItem, MenuIds.NoFileSelectedDefaultMenuBar),
                    new MenuItemData(CommandIds.RecentFiles, CommandIds.BuildMenuBarItem),

                    // This item will result in error message being logged and item not being added
                    // to MenuIds.NoFileSelectedDefaultMenuBar, since menu bars can have only menu bar items as children,
                    // according to logic we provided in implementation MenuDataObjectWrapperFactory (private class below) of interface 
                    // IDataStoreItemWrapperFactory<INonCopyMenuObject, IMenuDataObjectWrapper>.
                    new MenuItemData(CommandIds.SaveToCloud, MenuIds.NoFileSelectedDefaultMenuBar)
                }));

            var loadedDataStoresCache = new TestDataStoresCache(menuDataStores, new MenuDataObjectWrapperFactory());

            loadedDataStoresCache.DataStoresCacheLoadMessageEvent += (sender, e) =>
            {
                var loggedMessage = e.LoggedMessage;

                Console.WriteLine(string.Format("[Data Store Id:{0}, Data Store Item: [{1}], MessageType: {2}, MessageCategory: {3}]{4}{4}\t\tMessage:{5}{4}",
                    loggedMessage.DataStoreId, loggedMessage.DataStoreItem?.GetDisplayValue(loggedMessage.DataStoreId, false),
                    loggedMessage.MessageType,
                    loggedMessage.MessageCategory,
                    Environment.NewLine,
                    loggedMessage.Message));
            };

            loadedDataStoresCache.Initialize();

            //var visualizedCacheFilePath = Path.Combine(Path.GetDirectoryName(typeof(TestHelpers).Assembly.Location)!,
            //    @"SuccessfulCacheBuildTests\TestFiles\SimpleDataStoresCacheLoadDemo.processed.xml");

            //var visualizeTestDataStoresCache = TestHelpers.SaveDataStoreCache(loadedDataStoresCache, visualizedCacheFilePath);

            Console.WriteLine(TestHelpers.VisualizeTestDataStoresCache(loadedDataStoresCache));
        }

        private class MenuDataObjectWrapperFactory : IDataStoreItemWrapperFactory<INonCopyMenuObject, MenuDataObjectWrapper>
        {
            public CreateDataStoreItemWrapperResult<INonCopyMenuObject, MenuDataObjectWrapper> Create(long dataStoreId, INonCopyMenuObject dataStoreItem, MenuDataObjectWrapper? parent = null)
            {
                if (parent?.DataStoreItem is IMenuBarData)
                {
                    if (dataStoreItem is not IMenuBarItemData)
                        return new CreateDataStoreItemWrapperResult<INonCopyMenuObject, MenuDataObjectWrapper>(
                            null, new LoggedMessage(
                                MessageType.InvalidChildDataStoreItem,
                                String.Format("[{0}] cannot be used as a child for [{1}].",
                                    dataStoreItem.GetDisplayValue(dataStoreId, true),
                                    parent.DataStoreItem.GetDisplayValue(dataStoreId, false)),
                                dataStoreId, dataStoreItem, MessageCategory.Error));
                }

                return new CreateDataStoreItemWrapperResult<INonCopyMenuObject, MenuDataObjectWrapper>(
                    new MenuDataObjectWrapper(dataStoreId, dataStoreItem,  parent), null);
            }
        }

        [Test]
        public void TestSuccessfulCacheLoad()
        {
            // TODO: finish
            //TestHelpers.LoadTestDataStoresCacheForSuccessTests(@"SuccessfulCacheBuildTests\TestFiles\SuccessfulCacheBuildTests.xml", null);
        }


        [Test]
        public void CopyDataStoreItemTest()
        {
            var expectedDataStoresCache = new ExpectedDataStoresCache();

            expectedDataStoresCache.AddDataStoreItemsCache(MenuIds.SharedMenuObjects)
                .ContinueWithMenuItemCollection(MenuIds.RecentObjectsCollection, null, true)
                .AddChildMenuItemAndContinue(CommandIds.RecentFiles)
                .AddChildMenuItemAndContinue(CommandIds.RecentlyChangedFiles);

            expectedDataStoresCache.AddDataStoreItemsCache(MenuIds.SharedMenuObjects2)
                .ContinueWithMenuBarItem(CommandIds.ViewMenuBarItem)
                .ContinueWithChildMenuItemCollection(MenuIds.RecentObjectsCollection, null, true)
                .ContinueWithChildMenuItem(CommandIds.RecentFiles)
                .AddChildMenuItemAndContinue(CommandIds.RecentSubmissions)
                .MoveToParent()
                .AddChildMenuItemAndContinue(CommandIds.RecentlyChangedFiles)
                .AddChildMenuItemAndContinue(CommandIds.RecentChanges);

            TestHelpers.LoadTestDataStoresCacheForSuccessTests(@"SuccessfulCacheBuildTests\TestFiles\CopyDataStoreItem.xml", expectedDataStoresCache);
        }

        [Test]
        public void CopyingCopyOfDataStoreItemTest()
        {
            var expectedDataStoresCache = new ExpectedDataStoresCache();

            expectedDataStoresCache.AddDataStoreItemsCache(MenuIds.SharedMenuObjects)
                .ContinueWithMenuItemCollection(MenuIds.RecentObjectsCollection, null, true)
                .AddChildMenuItemAndContinue(CommandIds.RecentFiles)
                .AddChildMenuItemAndContinue(CommandIds.RecentlyChangedFiles);

            expectedDataStoresCache.AddDataStoreItemsCache(MenuIds.SharedMenuObjects2)
                .ContinueWithMenuBarItem(CommandIds.ViewMenuBarItem)
                .ContinueWithChildMenuItemCollection(MenuIds.RecentObjectsCollection, null, true)
                .AddChildMenuItemAndContinue(CommandIds.RecentFiles)
                .AddChildMenuItemAndContinue(CommandIds.RecentlyChangedFiles)
                .AddChildMenuItemAndContinue(CommandIds.RecentChanges);

            expectedDataStoresCache.AddDataStoreItemsCache(MenuIds.NoFileSelectedMenuObjects)
                .ContinueWithMenuBar(MenuIds.NoFileSelectedDefaultMenuBar)
                .ContinueWithChildMenuBarItem(CommandIds.FileMenuBarItem)
                .ContinueWithChildMenuItemCollection(MenuIds.RecentObjectsCollection, null, true)
                .AddChildMenuItemAndContinue(CommandIds.RecentFiles)
                .AddChildMenuItemAndContinue(CommandIds.RecentlyChangedFiles)
                .ContinueWithChildMenuItem(CommandIds.RecentChanges)
                .AddChildMenuItemAndContinue(CommandIds.Errors)
                .MoveToParent()
                .AddChildMenuItemAndContinue(CommandIds.RecentLocations)
                .AddChildMenuItemAndContinue(CommandIds.RecentSubmissions);


            TestHelpers.LoadTestDataStoresCacheForSuccessTests(
                @"SuccessfulCacheBuildTests\TestFiles\CopyingCopyOfDataStoreItem.xml", expectedDataStoresCache);
        }

        [Test]
        public void DataStoresReferencingEachOtherTest()
        {
            // This should be changed to failure scenario
            var expectedDataStoresCache = new ExpectedDataStoresCache();

            expectedDataStoresCache.AddDataStoreItemsCache(MenuIds.SharedMenuObjects)
                .ContinueWithMenuItemCollection(MenuIds.RecentObjectsCollection, null, true)
                .AddChildMenuItemAndContinue(CommandIds.RecentFiles)
                .ContinueWithChildMenuItem(CommandIds.RecentlyChangedFiles)
                .ContinueWithChildMenuItem(CommandIds.SaveToCloud)
                .ContinueWithChildMenuItem(CommandIds.SaveFile);

            expectedDataStoresCache.AddDataStoreItemsCache(MenuIds.SharedMenuObjects2)
                .ContinueWithMenuBarItem(CommandIds.ViewMenuBarItem)
                .ContinueWithChildMenuItemCollection(MenuIds.RecentObjectsCollection, null, true)
                .AddChildMenuItemAndContinue(CommandIds.RecentFiles)
                .ContinueWithChildMenuItem(CommandIds.RecentlyChangedFiles)
                .ContinueWithChildMenuItem(CommandIds.SaveToCloud, null, false)
                .ContinueWithChildMenuItem(CommandIds.SaveFile, null, false)
                .ExpectedMenuDataStoreItemsCache.GetExpectedMenuObjectWrapper(MenuIds.RecentObjectsCollection)
                .AddChildMenuItemAndContinue(CommandIds.RecentChanges)
                .ExpectedMenuDataStoreItemsCache.ContinueWithMenuItem(CommandIds.SaveToCloud)
                .ContinueWithChildMenuItem(CommandIds.SaveFile);

            TestHelpers.LoadTestDataStoresCacheForSuccessTests(
                @"SuccessfulCacheBuildTests\TestFiles\DataStoresReferencingEachOther.xml", expectedDataStoresCache);
        }

        [Test]
        public void IgnoreSomeDataStoreItemsTest()
        {
            var expectedDataStoresCache = new ExpectedDataStoresCache();

            expectedDataStoresCache.AddDataStoreItemsCache(MenuIds.SharedMenuObjects)
                .ContinueWithMenuBarItem(CommandIds.RecentlyChangedFiles)
                .AddChildMenuItemAndContinue(CommandIds.RecentFiles)
                .ExpectedMenuDataStoreItemsCache
                .ContinueWithMenuItem(CommandIds.RecentLocations)
                .AddChildMenuItemAndContinue(CommandIds.EncodingUtf8);

            expectedDataStoresCache.AddDataStoreItemsCache(MenuIds.SharedMenuObjects2)
                .ContinueWithMenuBarItem(CommandIds.Projects)
                .AddChildMenuItemAndContinue(CommandIds.EnterPresentationMode)

                .ExpectedMenuDataStoreItemsCache
                .ContinueWithMenuBarItem(CommandIds.BuildMenuBarItem)
                .ContinueWithChildMenuItem(CommandIds.RecentLocations)
                .AddChildMenuItemAndContinue(CommandIds.SaveToCloud);

            TestHelpers.LoadTestDataStoresCacheForSuccessTests(
                @"SuccessfulCacheBuildTests\TestFiles\IgnoreSomeDataStoreItems.xml", expectedDataStoresCache,
                true, menuDataStores =>
                {
                    return new TestDataStoresCache(menuDataStores,
                        new TestMenuDataObjectWrapperFactory(
                            (dataStoreId, dataStoreItem, parent) =>
                            {
                                if (dataStoreItem.Id == CommandIds.FindFile)
                                    return (null, null);

                                switch (dataStoreId)
                                {
                                    case MenuIds.SharedMenuObjects:
                                        switch (dataStoreItem.Id)
                                        {
                                            case CommandIds.Projects:
                                                return (null, new LoggedMessage(MessageLogging.MessageType.Custom,
                                                    $"Projects is hidden in data store with Id={dataStoreId}.",
                                                    dataStoreId, dataStoreItem, MessageLogging.MessageCategory.Info));

                                            case CommandIds.SaveToCloud:
                                                return (null, null);
                                        }

                                        break;

                                    case MenuIds.SharedMenuObjects2:
                                        switch (dataStoreItem.Id)
                                        {
                                            case CommandIds.Git:
                                            case CommandIds.EncodingUtf8:
                                            case CommandIds.RecentlyChangedFiles:
                                                return (null, null);
                                        }

                                        break;
                                }

                                return (new MenuDataObjectWrapper(dataStoreId, dataStoreItem, parent), null);
                            }));

                }, loggedMessages =>
                {

                    Assert.AreEqual(11, loggedMessages.Count);

                    TestHelpers.ValidateLoggedMessage(new LoggedMessage(MessageType.Custom,
                        "Projects is hidden in data store with Id=", MenuIds.SharedMenuObjects,
                        new MenuBarItemData(CommandIds.Projects), MessageCategory.Info), loggedMessages[0]);

                    var dataStoreItemWillBeIgnoredMessage = "will be ignored";

                    TestHelpers.ValidateLoggedMessage(new LoggedMessage(MessageType.DataStoreItemIsIgnored,
                        dataStoreItemWillBeIgnoredMessage, MenuIds.SharedMenuObjects,
                        new MenuItemData(CommandIds.SaveToCloud, CommandIds.RecentLocations), MessageCategory.Info), loggedMessages[1]);

                    TestHelpers.ValidateLoggedMessage(new LoggedMessage(MessageType.DataStoreItemIsIgnored,
                        dataStoreItemWillBeIgnoredMessage, MenuIds.SharedMenuObjects,
                        new MenuItemData(CommandIds.FindFile, CommandIds.RecentLocations), MessageCategory.Info), loggedMessages[2]);

                    TestHelpers.ValidateLoggedMessage(new LoggedMessage(MessageType.DataStoreItemIsIgnored,
                        dataStoreItemWillBeIgnoredMessage, MenuIds.SharedMenuObjects,
                        new MenuItemData(CommandIds.SaveToCloud), MessageCategory.Info), loggedMessages[3]);

                    TestHelpers.ValidateLoggedMessage(new LoggedMessage(MessageType.DataStoreItemIsIgnored,
                        dataStoreItemWillBeIgnoredMessage, MenuIds.SharedMenuObjects,
                        new MenuItemData(CommandIds.FindFile), MessageCategory.Info), loggedMessages[4]);

                    TestHelpers.ValidateLoggedMessage(new LoggedMessage(MessageType.DataStoreItemIsIgnored,
                        dataStoreItemWillBeIgnoredMessage, MenuIds.SharedMenuObjects2,
                        new MenuItemData(CommandIds.Git, CommandIds.Projects), MessageCategory.Info), loggedMessages[5]);

                    TestHelpers.ValidateLoggedMessage(new LoggedMessage(MessageType.DataStoreItemIsIgnored,
                        dataStoreItemWillBeIgnoredMessage, MenuIds.SharedMenuObjects2,
                        new MenuBarItemData(CommandIds.RecentlyChangedFiles), MessageCategory.Info), loggedMessages[6]);

                    TestHelpers.ValidateLoggedMessage(new LoggedMessage(MessageType.DataStoreItemIsIgnored,
                        dataStoreItemWillBeIgnoredMessage, MenuIds.SharedMenuObjects2,
                        new MenuItemData(CommandIds.EncodingUtf8, CommandIds.RecentLocations), MessageCategory.Info), loggedMessages[7]);

                    TestHelpers.ValidateLoggedMessage(new LoggedMessage(MessageType.DataStoreItemIsIgnored,
                        dataStoreItemWillBeIgnoredMessage, MenuIds.SharedMenuObjects2,
                        new MenuItemData(CommandIds.FindFile, CommandIds.RecentLocations), MessageCategory.Info), loggedMessages[8]);

                    TestHelpers.ValidateLoggedMessage(new LoggedMessage(MessageType.DataStoreItemIsIgnored,
                        dataStoreItemWillBeIgnoredMessage, MenuIds.SharedMenuObjects2,
                        new MenuItemData(CommandIds.EncodingUtf8, CommandIds.RecentLocations), MessageCategory.Info), loggedMessages[9]);

                    TestHelpers.ValidateLoggedMessage(new LoggedMessage(MessageType.DataStoreItemIsIgnored,
                        dataStoreItemWillBeIgnoredMessage, MenuIds.SharedMenuObjects2,
                        new MenuItemData(CommandIds.FindFile, CommandIds.RecentLocations), MessageCategory.Info), loggedMessages[10]);
                }, false);
        }

        [Test]
        public void SortingDataStoreItemsTest()
        {
            var expectedDataStoresCache = new ExpectedDataStoresCache();

            expectedDataStoresCache.AddDataStoreItemsCache(MenuIds.SharedMenuObjects)
                .AddMenuItemAndContinue(CommandIds.Errors, 10)
                .AddMenuItemAndContinue(CommandIds.SaveFileAs, 15)
                .AddMenuItemAndContinue(CommandIds.BuildSolution, 20)
                .ContinueWithMenuItemCollection(MenuIds.RecentObjectsCollection, null, true)
                .AddChildMenuItemAndContinue(CommandIds.RecentlyChangedFiles, 10)
                .AddChildMenuItemAndContinue(CommandIds.RecentLocations, 20)
                .AddChildMenuItemAndContinue(CommandIds.RecentFiles)
                .ExpectedMenuDataStoreItemsCache
                .AddMenuItemAndContinue(CommandIds.EncodingUtf8)
                .AddMenuItemAndContinue(CommandIds.SaveFile)
                .AddMenuItemAndContinue(CommandIds.Projects);

            expectedDataStoresCache.AddDataStoreItemsCache(MenuIds.SharedMenuObjects2)
                .AddMenuBarItemAndContinue(CommandIds.FileMenuBarItem, 2)
                .AddMenuItemAndContinue(CommandIds.BuildSolution, 5)
                .AddMenuItemAndContinue(CommandIds.Errors, 30)
                .ContinueWithMenuBarItem(CommandIds.ViewMenuBarItem)
                .ContinueWithChildMenuItemCollection(MenuIds.RecentObjectsCollection, null, true)


                .AddChildMenuItemAndContinue(CommandIds.RecentChanges, 3) // Priority is 3
                .AddChildMenuItemAndContinue(CommandIds.SaveFileAs, 4) // Priority is 4
                .AddChildMenuItemAndContinue(CommandIds.RecentlyChangedFiles, 10) // Priority is 10
                .AddChildMenuItemAndContinue(CommandIds.EncodingUtf8, 12) // Priority is 12
                .AddChildMenuItemAndContinue(CommandIds.RecentLocations, 20) // Priority is 20
                .AddChildMenuItemAndContinue(CommandIds.RecentFiles) // Priority is null
                .AddChildMenuItemAndContinue(CommandIds.RecentSubmissions) // Priority is null
                .AddChildMenuItemAndContinue(CommandIds.SaveFile)
                .ExpectedMenuDataStoreItemsCache
                .AddMenuItemAndContinue(CommandIds.Projects);

            TestHelpers.LoadTestDataStoresCacheForSuccessTests(@"SuccessfulCacheBuildTests\TestFiles\SortingDataStoreItems.xml", expectedDataStoresCache);
        }

        [Test]
        public void CopyDataStoreItemMultipleTimesInDataStoreTest()
        {
            var expectedDataStoresCache = new ExpectedDataStoresCache();

            expectedDataStoresCache.AddDataStoreItemsCache(MenuIds.SharedMenuObjects)
                .ContinueWithMenuBarItem(CommandIds.ViewMenuBarItem)
                .ContinueWithChildMenuItemCollection(MenuIds.RecentObjectsGroup)
                .AddChildMenuItemAndContinue(CommandIds.RecentlyChangedFiles)
                .AddChildMenuItemAndContinue(CommandIds.RecentLocations);

            expectedDataStoresCache.AddDataStoreItemsCache(MenuIds.SharedMenuObjects2)
                .ContinueWithMenuItemCollection(MenuIds.RecentObjectsGroup)
                .AddChildMenuItemAndContinue(CommandIds.RecentlyChangedFiles)
                .ContinueWithChildMenuItem(CommandIds.RecentLocations)
                .AddChildMenuItemAndContinue(CommandIds.RecentSubmissions)
                .MoveToParent(1)
                .ContinueWithChildMenuItem(CommandIds.RecentFiles)
                .AddChildMenuItemAndContinue(CommandIds.Errors)

                .ExpectedMenuDataStoreItemsCache
                .ContinueWithMenuBarItem(CommandIds.FileMenuBarItem)
                .ContinueWithChildMenuItemCollection(MenuIds.RecentObjectsGroupCopy1)
                .AddChildMenuItemAndContinue(CommandIds.RecentlyChangedFiles)
                .ContinueWithChildMenuItem(CommandIds.RecentLocations)
                .AddChildMenuItemAndContinue(CommandIds.RecentSubmissions)
                .MoveToParent(1)
                .ContinueWithChildMenuItem(CommandIds.RecentFiles)
                .AddChildMenuItemAndContinue(CommandIds.Errors)
                // Go up to FileMenuBarItem
                .MoveToParent(2)
                .ContinueWithChildMenuItemCollection(MenuIds.RecentObjectsGroupCopy2)
                .AddChildMenuItemAndContinue(CommandIds.RecentlyChangedFiles)
                .ContinueWithChildMenuItem(CommandIds.RecentLocations)
                .AddChildMenuItemAndContinue(CommandIds.RecentSubmissions)
                .MoveToParent(1)
                .ContinueWithChildMenuItem(CommandIds.RecentFiles)
                .AddChildMenuItemAndContinue(CommandIds.Errors)

                .ExpectedMenuDataStoreItemsCache
                .ContinueWithMenuBarItem(CommandIds.BuildMenuBarItem)
                .ContinueWithChildMenuItemCollection(MenuIds.RecentObjectsGroupCopy3)
                .AddChildMenuItemAndContinue(CommandIds.RecentlyChangedFiles)
                .ContinueWithChildMenuItem(CommandIds.RecentLocations)
                .AddChildMenuItemAndContinue(CommandIds.RecentSubmissions)
                .MoveToParent(1)
                .ContinueWithChildMenuItem(CommandIds.RecentFiles)
                .AddChildMenuItemAndContinue(CommandIds.Errors);

            TestHelpers.LoadTestDataStoresCacheForSuccessTests(
                @"SuccessfulCacheBuildTests\TestFiles\CopyDataStoreItemMultipleTimesInDataStore.xml", expectedDataStoresCache);
        }
    }
}