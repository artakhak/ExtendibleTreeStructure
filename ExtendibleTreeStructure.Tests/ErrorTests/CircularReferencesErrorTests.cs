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

using ExtendibleTreeStructure.CircularReferencePrevention;
using ExtendibleTreeStructure.MessageLogging;
using ExtendibleTreeStructure.Tests.MenuItems;
using ExtendibleTreeStructure.Tests.Validation;
using NUnit.Framework;
using System;
using System.Collections.Generic;

namespace ExtendibleTreeStructure.Tests.ErrorTests
{
    [TestFixture]
    internal class CircularReferencesErrorTests
    {
        [SetUp]
        public void TestInitialize()
        {
            TestHelpers.RegisterLogger();
        }

        [Test]
        public void ChildParentReferencingEachOther1Test()
        {
            var expectedDataStoresCache = new ExpectedDataStoresCache();

            var toolsSubmenuItem = new MenuItemData(CommandIds.ToolsSubmenuItem, CommandIds.RecentlyChangedFiles);
            var recentlyChangedFilesMenuItem = new MenuItemData(CommandIds.RecentlyChangedFiles, CommandIds.ToolsSubmenuItem);

            expectedDataStoresCache.AddDataStoreItemsCache(MenuIds.SharedMenuObjects);

            TestHelpers.LoadTestDataStoresCacheForErrorTests(
                @"ErrorTests\TestFiles\ChildParentReferencingEachOther1.xml", expectedDataStoresCache, 1,
                loggedMessages =>
                {
                    ValidateCircularReferencesErrorMessage(loggedMessages[0],
                        MenuIds.SharedMenuObjects,
                        toolsSubmenuItem,
                        new List<PathComponentEdge>
                        {
                            new PathComponentEdge(
                                new PathComponentNode(toolsSubmenuItem, MenuIds.SharedMenuObjects),
                                new PathComponentNode(recentlyChangedFilesMenuItem, MenuIds.SharedMenuObjects),
                                DataStoreItemsRelationship.Child),

                            new PathComponentEdge(
                                new PathComponentNode(recentlyChangedFilesMenuItem, MenuIds.SharedMenuObjects),
                                new PathComponentNode(toolsSubmenuItem, MenuIds.SharedMenuObjects),
                                DataStoreItemsRelationship.Child)
                        });
                },
                true);
        }

        [Test]
        public void ChildParentReferencingEachOther2Test()
        {
            var expectedDataStoresCache = new ExpectedDataStoresCache();

            expectedDataStoresCache.AddDataStoreItemsCache(MenuIds.SharedMenuObjects);

            var toolsSubmenuItem = new MenuItemData(CommandIds.ToolsSubmenuItem, CommandIds.RecentlyChangedFiles);
            var recentlyChangedFilesMenuItem = new MenuItemData(CommandIds.RecentlyChangedFiles, CommandIds.ToolsSubmenuItem);

            TestHelpers.LoadTestDataStoresCacheForErrorTests(
                @"ErrorTests\TestFiles\ChildParentReferencingEachOther2.xml", expectedDataStoresCache, 1,
                loggedMessages =>
                {
                    ValidateCircularReferencesErrorMessage(loggedMessages[0],
                        MenuIds.SharedMenuObjects,
                        recentlyChangedFilesMenuItem,
                        new List<PathComponentEdge>
                        {
                            new PathComponentEdge(
                                new PathComponentNode(recentlyChangedFilesMenuItem, MenuIds.SharedMenuObjects),
                                new PathComponentNode(toolsSubmenuItem, MenuIds.SharedMenuObjects),
                                DataStoreItemsRelationship.Child),
                            new PathComponentEdge(
                                new PathComponentNode(toolsSubmenuItem, MenuIds.SharedMenuObjects),
                                new PathComponentNode(recentlyChangedFilesMenuItem, MenuIds.SharedMenuObjects),
                                DataStoreItemsRelationship.Child)

                        });
                },
                true);
        }

        [Test]
        public void CircularCopyItemReferencesTest()
        {
            var expectedDataStoresCache = new ExpectedDataStoresCache();

            expectedDataStoresCache.AddDataStoreItemsCache(MenuIds.SharedMenuObjects)
                .ContinueWithMenuItem(CommandIds.ToolsSubmenuItem)
                .AddChildMenuItemAndContinue(CommandIds.RecentFiles)
                .AddChildMenuItemAndContinue(CommandIds.RecentSubmissions);

            expectedDataStoresCache.AddDataStoreItemsCache(MenuIds.SharedMenuObjects2)
                .AddMenuBarItemAndContinue(CommandIds.FileMenuBarItem);

            TestHelpers.LoadTestDataStoresCacheForErrorTests(
                @"ErrorTests\TestFiles\CircularCopyItemReferences.xml", expectedDataStoresCache, 3,
                loggedMessages =>
                {
                    var copyMenuObject1 = new CopyMenuObject(MenuIds.SharedMenuObjects2, CommandIds.RecentlyChangedFiles, CommandIds.ToolsSubmenuItem);
                    var copyMenuObject2 = new CopyMenuObject(MenuIds.SharedMenuObjects, CommandIds.RecentlyChangedFiles, CommandIds.FileMenuBarItem);

                    var failedToCopyErrorMessage = "Failed to copy data store item referenced by";

                    ValidateCircularReferencesErrorMessage(loggedMessages[0],
                        MenuIds.SharedMenuObjects,
                        copyMenuObject1,
                        new[]
                        {
                            new PathComponentEdge(
                                new PathComponentNode(copyMenuObject1, MenuIds.SharedMenuObjects),
                                new PathComponentNode(copyMenuObject2, MenuIds.SharedMenuObjects2),
                                DataStoreItemsRelationship.Copied),

                            new PathComponentEdge(
                                new PathComponentNode(copyMenuObject2, MenuIds.SharedMenuObjects2),
                                new PathComponentNode(copyMenuObject1, MenuIds.SharedMenuObjects),
                                DataStoreItemsRelationship.Copied)
                        });

                    TestHelpers.ValidateLoggedMessage(new LoggedMessage(MessageType.FailedToCopyDataStoreItem, failedToCopyErrorMessage,
                        MenuIds.SharedMenuObjects2, copyMenuObject2, MessageCategory.Error), loggedMessages[1]);

                    TestHelpers.ValidateLoggedMessage(new LoggedMessage(MessageType.FailedToCopyDataStoreItem, failedToCopyErrorMessage,
                        MenuIds.SharedMenuObjects, copyMenuObject1, MessageCategory.Error), loggedMessages[2]);
                },
                true);
        }

        [Test]
        public void TransitiveChildAndCopyCircularReferencesTest()
        {
            var expectedDataStoresCache = new ExpectedDataStoresCache();

            expectedDataStoresCache.AddDataStoreItemsCache(MenuIds.SharedMenuObjects)
                .ContinueWithMenuItem(CommandIds.ToolsSubmenuItem)
                .AddChildMenuItemAndContinue(CommandIds.RecentSubmissions)
                .ContinueWithChildMenuItem(CommandIds.RecentFiles)
                .AddChildMenuItemAndContinue(CommandIds.Projects)
                .MoveToParent(1)
                .AddChildMenuItemAndContinue(CommandIds.RecentChanges);

            expectedDataStoresCache.AddDataStoreItemsCache(MenuIds.SharedMenuObjects2)
                .ContinueWithMenuItem(CommandIds.Projects)
                .ContinueWithChildMenuItem(CommandIds.ToolsSubmenuItem)
                .AddChildMenuItemAndContinue(CommandIds.RecentSubmissions)
                .AddChildMenuItemAndContinue(CommandIds.RecentFiles)
                .AddChildMenuItemAndContinue(CommandIds.RecentChanges);

            TestHelpers.LoadTestDataStoresCacheForErrorTests(
                @"ErrorTests\TestFiles\TransitiveChildAndCopyCircularReferences.xml", expectedDataStoresCache, 3,
                loggedMessages =>
                {
                    var toolsSubmenuItem = new MenuItemData(CommandIds.ToolsSubmenuItem);
                    var recentFilesItem = new MenuItemData(CommandIds.RecentFiles, CommandIds.ToolsSubmenuItem);
                    var projectsCopyMenuItem = new CopyMenuObject(MenuIds.SharedMenuObjects2, CommandIds.Projects, CommandIds.RecentFiles);
                    var projectsMenuItemInStore2 = new MenuItemData(CommandIds.Projects);
                    var toolsSubmenuCopyMenuItem = new CopyMenuObject(MenuIds.SharedMenuObjects, CommandIds.ToolsSubmenuItem, CommandIds.Projects);

                    ValidateCircularReferencesErrorMessage(loggedMessages[0],
                        MenuIds.SharedMenuObjects,
                        toolsSubmenuItem,
                        new[]
                        {

                            new PathComponentEdge(new PathComponentNode(toolsSubmenuItem, MenuIds.SharedMenuObjects),
                                new PathComponentNode(recentFilesItem, MenuIds.SharedMenuObjects), DataStoreItemsRelationship.Child),

                            new PathComponentEdge(new PathComponentNode(recentFilesItem, MenuIds.SharedMenuObjects),
                                new PathComponentNode(projectsCopyMenuItem, MenuIds.SharedMenuObjects), DataStoreItemsRelationship.Child),

                            new PathComponentEdge(new PathComponentNode(projectsCopyMenuItem, MenuIds.SharedMenuObjects),
                                new PathComponentNode(projectsMenuItemInStore2, MenuIds.SharedMenuObjects2), DataStoreItemsRelationship.Copied),

                            new PathComponentEdge(new PathComponentNode(projectsMenuItemInStore2, MenuIds.SharedMenuObjects2),
                                new PathComponentNode(toolsSubmenuCopyMenuItem, MenuIds.SharedMenuObjects2), DataStoreItemsRelationship.Child),

                            new PathComponentEdge(new PathComponentNode(toolsSubmenuCopyMenuItem, MenuIds.SharedMenuObjects2),
                                new PathComponentNode(toolsSubmenuItem, MenuIds.SharedMenuObjects), DataStoreItemsRelationship.Copied)

                        });

                    var failedToCopyDataStoreItemMessage = "Failed to copy data store item referenced by";

                    TestHelpers.ValidateLoggedMessage(
                        new LoggedMessage(MessageType.FailedToCopyDataStoreItem,
                            failedToCopyDataStoreItemMessage,
                            MenuIds.SharedMenuObjects2, toolsSubmenuCopyMenuItem, MessageCategory.Error),
                        loggedMessages[1]);

                    TestHelpers.ValidateLoggedMessage(
                        new LoggedMessage(MessageType.FailedToCopyDataStoreItem,
                            failedToCopyDataStoreItemMessage,
                            MenuIds.SharedMenuObjects, projectsCopyMenuItem, MessageCategory.Error),
                        loggedMessages[2]);

                },
                true);
        }

        private void ValidateCircularReferencesErrorMessage(
            ILoggedMessage loggedMessage,
            long expectedDataStoreId,
            IDataStoreItem expectedDataStoreItem,
            IReadOnlyList<PathComponentEdge> expectedCircularReferencesPath)
        {
            if (loggedMessage is not ICircularReferencesErrorMessage actualCircularReferencesErrorMessage)
                throw new Exception($"Expected an instance of '{typeof(ICircularReferencesErrorMessage)}'.");

            Assert.AreEqual(expectedDataStoreId, actualCircularReferencesErrorMessage.DataStoreId);
            Assert.AreEqual(actualCircularReferencesErrorMessage.MessageCategory, MessageCategory.Error);
            Assert.AreEqual(actualCircularReferencesErrorMessage.MessageType, MessageType.CircularReferences);

            Assert.IsTrue(expectedDataStoreItem.Equals(actualCircularReferencesErrorMessage.DataStoreItem));

            Assert.IsTrue(actualCircularReferencesErrorMessage.Message.Contains("results in the following circular references:"));

            Assert.AreEqual(expectedCircularReferencesPath.Count, actualCircularReferencesErrorMessage.CircularReferencesPath.Count);

            for (var i = 0; i < expectedCircularReferencesPath.Count; ++i)
            {
                var expectedEdge = expectedCircularReferencesPath[i];
                var actualEdge = actualCircularReferencesErrorMessage.CircularReferencesPath[i];

                Assert.IsTrue(expectedEdge.Equals(actualEdge));
            }
        }
    }
}