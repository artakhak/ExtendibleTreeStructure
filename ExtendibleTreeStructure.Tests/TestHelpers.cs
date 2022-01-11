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
using log4net;
using log4net.Config;
using log4net.Repository.Hierarchy;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Xml;
using System.Xml.Schema;

namespace ExtendibleTreeStructure.Tests
{
    public static class TestHelpers
    {
        private static readonly Dictionary<long, (Type constantsType, string memberName)> ConstantValueToConstantMemberData = new Dictionary<long, (Type constantsType, string memberName)>();

        // TODO: _logger will be replaced with LogHelper.Context.Log when dependency to package OROptimizer.Shared is added
        private static readonly ILog Logger = RegisterLogger();

        static TestHelpers()
        {

            var constantTypes = new Type[] {typeof(MenuIds), typeof(CommandIds)};

            foreach (var constantType in constantTypes)
            {
                foreach (var fieldData in constantType.GetFields())
                {
                    if (fieldData.FieldType == typeof(long) && fieldData.IsPublic && fieldData.IsStatic)
                    {
                        var fieldValue = fieldData.GetValue(null);
                        if (fieldValue == null)
                            continue;

                        var fieldLongValue = (long) fieldValue;

                        if (ConstantValueToConstantMemberData.ContainsKey(fieldLongValue))
                            continue;

                        ConstantValueToConstantMemberData[fieldLongValue] = (constantType, fieldData.Name);
                    }
                }
            }
        }

        // TODO: Get rid off this method and replace with RegisterLogger() below, when dependency to package OROptimizer.Shared is added
        public static ILog RegisterLogger()
        {
            var log4netConfig = new XmlDocument();
            log4netConfig.Load(File.OpenRead("ExtendibleTreeStructure.Tests.log4net.config"));

            var repo = LogManager.CreateRepository(
                Assembly.GetEntryAssembly(), typeof(Hierarchy));

            XmlConfigurator.Configure(repo, log4netConfig["log4net"]);

            return LogManager.GetLogger(typeof(TestHelpers));
        }

        // TODO: Uncomment this method and remove the similar  rid off this method above when dependency to package OROptimizer.Shared is added
        //public static void RegisterLogger()
        //{
        //    LogHelper.RemoveContext();
        //    LogHelper.RegisterContext(new Log4NetHelperContext("ExtendibleTreeStructure.Tests.log4net.config"));
        //}

        public static IReadOnlyList<IDataStore<IMenuObject>> LoadMenuDataStores(string xmlFileRelativePath)
        {
            var menuDataStores = new List<IDataStore<IMenuObject>>();

            var schemaSet = new XmlSchemaSet();

            XmlDocument xmlDocument;
            using (var stream = LoadFileTextSteam("MenuDataStoresSchema.xsd"))
            {
                var xmlSchema = XmlSchema.Read(stream, (sender, e) =>
                {
                    if (e.Exception != null)
                        throw e.Exception;

                    if (e.Severity == XmlSeverityType.Error)
                        throw new Exception($"Error in XML Schema : 'resourceName'. Schema load message: {e.Message}");
                });

                if (xmlSchema == null)
                    throw new Exception("Xml  schema failed to load.");

                schemaSet.Add(xmlSchema);

                // Load XML file
                xmlDocument = new XmlDocument {Schemas = schemaSet};
            }

            using (var stream = LoadFileTextSteam(xmlFileRelativePath))
                xmlDocument.Load(stream);

            foreach (var childNode in xmlDocument.SelectNodes("/menuDataStores/menuDataStore")!)
            {
                if (childNode is not XmlElement menuDataStoreElement)
                    continue;

                var menuStoreObjects = new List<IMenuObject>();

                foreach (var menuObjectNode in menuDataStoreElement.ChildNodes)
                {
                    if (menuObjectNode is not XmlElement xmlElement)
                        continue;

                    IMenuObject menuObject;
                    switch (xmlElement.Name)
                    {
                        case "menuBar":
                            menuObject = new MenuBarData(
                                GetRequiredIdAttributeValue(xmlElement));
                            break;

                        case "menuBarItem":
                            menuObject = new MenuBarItemData(
                                GetCommandIdAttributeValue(xmlElement),
                                GetOptionalIdAttributeValue(xmlElement, "parentMenuBarId"));
                            break;

                        case "menuItem":
                            menuObject = new MenuItemData(
                                GetCommandIdAttributeValue(xmlElement),
                                GetOptionalIdAttributeValue(xmlElement, "parentId"));
                            break;

                        case "menuItemCollection":
                            menuObject = new MenuItemCollection(
                                GetRequiredIdAttributeValue(xmlElement),
                                GetOptionalIdAttributeValue(xmlElement, "parentId"),
                                GetAttributeValue(xmlElement, "usesMenuSeparator",
                                    bool.Parse, false));

                            break;

                        case "copyMenuObject":
                            var menuId = GetOptionalIdAttributeValue(xmlElement);

                            if (menuId == null)
                                menuObject = new CopyMenuObject(
                                    GetRequiredIdAttributeValue(xmlElement, "referencedMenuDataStoreId"),
                                    GetRequiredIdAttributeValue(xmlElement, "referencedMenuObjectId"),
                                    GetOptionalIdAttributeValue(xmlElement, "parentId"));
                            else
                                menuObject = new CopyMenuObject(menuId.Value,
                                    GetRequiredIdAttributeValue(xmlElement, "referencedMenuDataStoreId"),
                                    GetRequiredIdAttributeValue(xmlElement, "referencedMenuObjectId"),
                                    GetOptionalIdAttributeValue(xmlElement, "parentId"));
                            break;

                        default:
                            throw new Exception($"Invalid element '{xmlElement.Name}'.");
                    }

                    var priorityValue = xmlElement.GetAttribute("priority");

                    if (!string.IsNullOrWhiteSpace(priorityValue))
                        menuObject.Priority = int.Parse(priorityValue);

                    menuStoreObjects.Add(menuObject);
                }

                menuDataStores.Add(new DataStore<IMenuObject>(
                    GetRequiredIdAttributeValue(menuDataStoreElement, "id"), menuStoreObjects));
            }

            return menuDataStores;
        }

        private static long GetCommandIdAttributeValue(XmlElement xmlElement)
        {
            return GetConstantValue(typeof(CommandIds),
                GetAttributeValue(xmlElement, "commandId", true) ?? throw new Exception());
        }

        private static long GetRequiredIdAttributeValue(XmlElement xmlElement)
        {
            return GetConstantValue(new[] {typeof(MenuIds), typeof(CommandIds)},
                GetAttributeValue(xmlElement, "id", true) ?? throw new Exception());
        }


        private static long? GetOptionalIdAttributeValue(XmlElement xmlElement)
        {
            var constantName = GetAttributeValue(xmlElement, "id", false);

            if (constantName == null)
                return null;

            return GetConstantValue(new[] {typeof(MenuIds), typeof(CommandIds)}, constantName);
        }

        private static long GetRequiredIdAttributeValue(XmlElement xmlElement, string attributeName)
        {
            var attributeData = GetIdAttributeData(xmlElement, attributeName, true);
            return GetConstantValue(attributeData.constantsType ?? typeof(MenuIds), attributeData.constantName ?? throw new Exception());
        }

        private static long? GetOptionalIdAttributeValue(XmlElement xmlElement, string attributeName)
        {
            var attributeData = GetIdAttributeData(xmlElement, attributeName, false);

            if (attributeData.constantName == null)
                return null;

            return GetConstantValue(attributeData.constantsType ?? typeof(MenuIds), attributeData.constantName);
        }

        public static string GetConstantNameForLogs(long id)
        {
            if (!ConstantValueToConstantMemberData.TryGetValue(id, out var fieldData))
                throw new Exception($"No field data found for Id={id}.");

            var constantNameStrBldr = new StringBuilder();

            if (fieldData.constantsType == typeof(MenuIds))
                constantNameStrBldr.Append(nameof(MenuIds));
            else if (fieldData.constantsType == typeof(CommandIds))
                constantNameStrBldr.Append(nameof(CommandIds));
            else
                constantNameStrBldr.Append(fieldData.constantsType.FullName);

            constantNameStrBldr.Append($".{fieldData.memberName}");
            return constantNameStrBldr.ToString();
        }

        private static (Type? constantsType, string? constantName) GetIdAttributeData(XmlElement xmlElement, string attributeName, bool isRequired)
        {
            var attributeValue = GetAttributeValue(xmlElement, attributeName, isRequired);

            if (attributeValue == null)
                return (null, null);

            var lastIndexOfPeriod = attributeValue.LastIndexOf('.');

            if (lastIndexOfPeriod < 0)
                return (null, attributeValue);

            if (lastIndexOfPeriod == 0 || lastIndexOfPeriod == attributeValue.Length - 1)
                throw new Exception($"Invalid value '{attributeValue}' in attribute '{attributeName}'.");

            var typeFullName = attributeValue.Substring(0, lastIndexOfPeriod);

            Type? constantsType = typeof(TestHelpers).Assembly.GetType(attributeValue.Substring(0, lastIndexOfPeriod));

            if (constantsType == null)
                throw new Exception($"No type found with full name '{typeFullName}'.");

            return (constantsType, attributeValue.Substring(lastIndexOfPeriod + 1));
        }

        public static long GetConstantValue(Type constantsType, string constantName)
        {
            return GetConstantValue(new[] {constantsType}, constantName);
        }

        public static long GetConstantValue(Type[] constantsTypes, string constantName)
        {
            foreach (var constantsType in constantsTypes)
            {
                var fieldInfo = constantsType.GetField(constantName,
                    System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);

                if (fieldInfo == null || fieldInfo.FieldType != typeof(long))
                    continue;

                var value = fieldInfo.GetValue(null);

                if (value == null)
                    throw new Exception();

                return (long) value;
            }

            throw new Exception($"No constant with name '{constantName}' was found in types '{string.Join(',', constantsTypes.Select(x => x.FullName))}'.");
        }

        private static T GetAttributeValue<T>(XmlElement xmlElement, string attributeName, Func<string, T> convertToT, T defaultValue)
        {
            var attribute = xmlElement.Attributes[attributeName];
            if (attribute?.Value == null)
                return defaultValue;

            return convertToT(attribute.Value);
        }

        private static string? GetAttributeValue(XmlElement xmlElement, string attributeName, bool isRequired)
        {
            var attribute = xmlElement.Attributes[attributeName];
            if (attribute?.Value == null)
            {
                if (isRequired)
                    throw new Exception($"No attribute with name '{attributeName}' was found in element '{xmlElement.Name}'.");

                return null;
            }

            return attribute.Value;
        }

        private static StringReader LoadFileTextSteam(string fileRelativePath)
        {
            var menuDataStoresFilePath = Path.Combine(Path.GetDirectoryName(typeof(TestHelpers).Assembly.Location)!, fileRelativePath);

            using (var streamReader = new StreamReader(menuDataStoresFilePath))
            {
                return new StringReader(streamReader.ReadToEnd());
            }
        }

        public static TestDataStoresCache LoadTestDataStoresCacheForSuccessTests(string xmlFileRelativePath,
            ExpectedDataStoresCache? expectedDataStoresCache,
            bool saveVisualizeCache = true,
            Func<IReadOnlyList<IDataStore<IMenuObject>>, TestDataStoresCache>? createTestDataStoresCache = null,
            Action<IReadOnlyList<ILoggedMessage>>? processLoggedMessagesOnLoaded = null,
            bool allDataStoreItemsAreIncluded = true)
        {
            var testDataStoresCache = LoadTestDataStoresCache(xmlFileRelativePath, expectedDataStoresCache,
                saveVisualizeCache, allDataStoreItemsAreIncluded,
                createTestDataStoresCache,
                loggedMessages =>
                {
                    Assert.AreEqual(0, loggedMessages.Count(x => x.MessageCategory == MessageCategory.Error));

                    processLoggedMessagesOnLoaded?.Invoke(loggedMessages);
                });

            return testDataStoresCache;
        }

        public static TestDataStoresCache LoadTestDataStoresCacheForErrorTests(string xmlFileRelativePath,
            ExpectedDataStoresCache? expectedDataStoresCache,
            int numberOfExpectedErrors,
            Action<IReadOnlyList<ILoggedMessage>> processLoggedMessagesOnLoaded,
            bool saveVisualizeCache = true,
            Func<IReadOnlyList<IDataStore<IMenuObject>>, TestDataStoresCache>? createTestDataStoresCache = null)
        {
            void ProcessedLoggedMessagesOnLoaded(IReadOnlyList<ILoggedMessage> loggedMessages)
            {
                Assert.AreEqual(numberOfExpectedErrors, loggedMessages.Count(x => x.MessageCategory == MessageCategory.Error));
                processLoggedMessagesOnLoaded(loggedMessages);
            }

            return LoadTestDataStoresCache(xmlFileRelativePath, expectedDataStoresCache,
                saveVisualizeCache, false,
                createTestDataStoresCache,
                ProcessedLoggedMessagesOnLoaded);
        }

        private static TestDataStoresCache LoadTestDataStoresCache(string xmlFileRelativePath,
            ExpectedDataStoresCache? expectedDataStoresCache,
            bool saveVisualizeCache,
            bool allDataStoreItemsAreIncluded,

            Func<IReadOnlyList<IDataStore<IMenuObject>>, TestDataStoresCache>? createTestDataStoresCache = null,
            Action<IReadOnlyList<ILoggedMessage>>? processLoggedMessagesOnLoaded = null)
        {
            string? visualizedMenuDataStoresFilePath = null;
            if (saveVisualizeCache)
            {
                if (!xmlFileRelativePath.EndsWith(".xml"))
                    throw new Exception("The xml file extension is missing.");

                visualizedMenuDataStoresFilePath = Path.Combine(Path.GetDirectoryName(typeof(TestHelpers).Assembly.Location)!,
                    $"{xmlFileRelativePath.Substring(0, xmlFileRelativePath.Length - 4)}.processed.xml");
            }

            return LoadTestDataStoresCache(LoadMenuDataStores(xmlFileRelativePath), expectedDataStoresCache, visualizedMenuDataStoresFilePath,
                allDataStoreItemsAreIncluded, createTestDataStoresCache,
                processLoggedMessagesOnLoaded);
        }

        public static TestDataStoresCache LoadTestDataStoresCache(IReadOnlyList<IDataStore<IMenuObject>> menuDataStores,
            ExpectedDataStoresCache? expectedDataStoresCache,
            string? visualizedMenuDataStoresFilePath,
            bool allDataStoreItemsAreIncluded,
            Func<IReadOnlyList<IDataStore<IMenuObject>>, TestDataStoresCache>? createTestDataStoresCache = null,
            Action<IReadOnlyList<ILoggedMessage>>? processLoggedMessagesOnLoaded = null)
        {
            TestDataStoresCache testDataStoresCache;
            if (createTestDataStoresCache != null)
                testDataStoresCache = createTestDataStoresCache(menuDataStores);
            else
                testDataStoresCache = new TestDataStoresCache(menuDataStores);

            List<ILoggedMessage> loggedMessages = new List<ILoggedMessage>();

            testDataStoresCache.DataStoresCacheLoadMessageEvent += (_, e) =>
            {
                var loggedMessage = e.LoggedMessage;
                loggedMessages.Add(loggedMessage);

                var parentId = (loggedMessage.DataStoreItem as ICanHaveParent)?.ParentId;

                log4net.GlobalContext.Properties["context"] = string.Format("[Data Store Id:{0}, Data Store Item (Id:{1}{2}), MessageType:{3}]",
                    GetConstantNameForLogs(e.LoggedMessage.DataStoreId),
                    loggedMessage.DataStoreItem == null ? "null" : TestHelpers.GetConstantNameForLogs(loggedMessage.DataStoreItem.Id),
                    (parentId != null ? $", ParentId:{TestHelpers.GetConstantNameForLogs(parentId.Value)}" : String.Empty),
                    e.LoggedMessage.MessageType);

                var message = e.LoggedMessage.Message;

                switch (e.LoggedMessage.MessageCategory)
                {
                    case MessageCategory.Debug:
                        Logger.Debug(message);
                        break;
                    case MessageCategory.Info:
                        Logger.Info(message);
                        break;
                    case MessageCategory.Warn:
                        Logger.Warn(message);
                        break;

                    case MessageCategory.Error:
                        Logger.Error(message);
                        break;
                }
            };

            testDataStoresCache.Initialize();

            if (allDataStoreItemsAreIncluded)
            {
                foreach (var dataStore in menuDataStores)
                {
                    Assert.IsTrue(testDataStoresCache.TryGetDataStore(dataStore.DataStoreId, out var dataStoreItemsCache));

                    foreach (var menuObject in dataStore.Items)
                    {
                        if (dataStoreItemsCache == null || !dataStoreItemsCache.TryGetDataStoreItem(menuObject.Id, out var menuDataObjectWrapper) ||
                            menuDataObjectWrapper == null)
                            throw new Exception();

                        Assert.IsNull(menuDataObjectWrapper.Parent);

                        Assert.IsTrue(menuDataObjectWrapper.DataStoreItem is not ICanHaveParent canHaveParent || canHaveParent.ParentId == null);
                        Assert.IsTrue(menuDataObjectWrapper.DataStoreId == dataStore.DataStoreId &&
                                      menuDataObjectWrapper.DataStoreItem.Id == menuObject.Id);

                        if (expectedDataStoresCache != null)
                        {
                            var expectedMenuDataStoreItemsCache = expectedDataStoresCache.GetExpectedMenuDataStoreItemsCache(dataStore.DataStoreId);

                            ExpectedMenuObjectWrapper? expectedMenuObjectWrapper = null;
                            foreach (var dataStoreItemWrapper in expectedMenuDataStoreItemsCache.AllDataStoreItemWrappers)
                            {
                                if (dataStoreItemWrapper.NonCopyMenuObject.Id == menuDataObjectWrapper.DataStoreItem.Id &&
                                    menuDataObjectWrapper.DataStoreItem.GetType() == dataStoreItemWrapper.NonCopyMenuObject.GetType())
                                {
                                    expectedMenuObjectWrapper = dataStoreItemWrapper;
                                    break;
                                }
                            }

                            if (expectedMenuObjectWrapper == null)
                                throw new Exception();

                            expectedMenuObjectWrapper.AssertEqualTo(menuDataObjectWrapper, true);
                        }
                    }
                }
            }

            if (visualizedMenuDataStoresFilePath != null)
            {
                SaveDataStoreCache(testDataStoresCache, visualizedMenuDataStoresFilePath);
            }

            processLoggedMessagesOnLoaded?.Invoke(loggedMessages);

            if (expectedDataStoresCache != null)
                expectedDataStoresCache.AssertEqualTo(testDataStoresCache);

            return testDataStoresCache;
        }

        public static string SaveDataStoreCache(IDataStoresCache<INonCopyMenuObject, MenuDataObjectWrapper> dataStoresCache,
            string visualizedMenuDataStoresFilePath)
        {
            using (var streamWriter = new StreamWriter(visualizedMenuDataStoresFilePath, false))
            {
                var visualizedDataStoresCache = VisualizeTestDataStoresCache(dataStoresCache);
                streamWriter.Write(visualizedDataStoresCache);
                return visualizedDataStoresCache;
            }
        }

        public static string VisualizeTestDataStoresCache(IDataStoresCache<INonCopyMenuObject, MenuDataObjectWrapper> dataStoresCache)
        {
            var generatedXml = new StringBuilder();
            generatedXml.AppendLine("<?xml version=\"1.0\" encoding=\"utf - 8\" ?>");
            generatedXml.AppendLine("<menuDataStores>");

            foreach (var dataStoreItemsCache in dataStoresCache.DataStoreItemsCacheList)
            {
                generatedXml.AppendLine($"\t<menuDataStore id=\"{ConstantValueToConstantMemberData[dataStoreItemsCache.DataStoreId].memberName}\">");

                foreach (var topLevelDataStoreItemWrapper in dataStoreItemsCache.TopLevelDataStoreItemWrappers)
                {
                    VisualizeDataStoreItemWrapper(topLevelDataStoreItemWrapper, generatedXml, 2);
                }

                generatedXml.AppendLine("\t</menuDataStore>");
            }

            generatedXml.AppendLine("</menuDataStores>");
            return generatedXml.ToString();
        }

        public static (Type constantsType, string memberName) GetIdAttributeFieldData(long id)
        {
            if (!ConstantValueToConstantMemberData.TryGetValue(id, out var fieldData))
                throw new Exception($"No field data found for Id={id}.");

            return fieldData;
        }

        private static void VisualizeDataStoreItemWrapper(IDataStoreItemWrapper<INonCopyMenuObject> dataStoreItemWrapper, StringBuilder generatedXml, int level)
        {
            var indent = new String('\t', level);

            generatedXml.Append(indent);

            generatedXml.Append("<");

            string elementName;
            string parentIdAttributeName = "parentId";

            if (dataStoreItemWrapper.DataStoreItem is IMenuBarData menuBarData)
            {
                elementName = "menuBar";
                generatedXml.Append($"{elementName} id=\"{ConstantValueToConstantMemberData[menuBarData.Id].memberName}\"");
            }
            else if (dataStoreItemWrapper.DataStoreItem is IMenuBarItemData menuBarItemData)
            {
                parentIdAttributeName = "parentMenuBarId";

                elementName = "menuBarItem";
                generatedXml.Append($"{elementName} commandId=\"{ConstantValueToConstantMemberData[menuBarItemData.CommandId].memberName}\"");
            }
            else if (dataStoreItemWrapper.DataStoreItem is IMenuItemData menuItemData)
            {
                elementName = "menuItem";
                generatedXml.Append($"{elementName} commandId=\"{ConstantValueToConstantMemberData[menuItemData.CommandId].memberName}\"");
            }
            else if (dataStoreItemWrapper.DataStoreItem is IMenuItemCollection menuItemCollection)
            {
                elementName = "menuItemCollection";
                generatedXml.Append($"{elementName} id=\"{ConstantValueToConstantMemberData[menuItemCollection.Id].memberName}\"");

                if (menuItemCollection.UsesMenuSeparator)
                    generatedXml.Append(" usesMenuSeparator=\"true\"");
            }
            else if (dataStoreItemWrapper.DataStoreItem is MenuObjectWithNoCloneWithNewParent)
            {
                elementName = "menuObjectWithNoCloneWithNewParent";
                generatedXml.Append($"{elementName} id=\"{ConstantValueToConstantMemberData[dataStoreItemWrapper.DataStoreItem.Id].memberName}\"");
            }
            else
            {
                throw new Exception($"Invalid type '{dataStoreItemWrapper.DataStoreItem.GetType()}'");
            }

            if (dataStoreItemWrapper.DataStoreItem is ICanHaveParent canHaveParent && canHaveParent.ParentId != null)
            {
                var parentIdMemberData = ConstantValueToConstantMemberData[canHaveParent.ParentId.Value];
                generatedXml.Append($" {parentIdAttributeName}=\"{parentIdMemberData.constantsType.FullName}.{parentIdMemberData.memberName}\"");
            }

            if (dataStoreItemWrapper.DataStoreItem.Priority != null)
            {
                generatedXml.Append($" priority=\"{dataStoreItemWrapper.DataStoreItem.Priority}\"");
            }

            if (dataStoreItemWrapper.Children.Count > 0)
            {
                generatedXml.AppendLine(">");
                foreach (var childDataStoreItemWrapper in dataStoreItemWrapper.Children)
                {
                    VisualizeDataStoreItemWrapper(childDataStoreItemWrapper, generatedXml, level + 1);
                }

                generatedXml.AppendLine($"{indent}</{elementName}>");
            }
            else
            {
                generatedXml.AppendLine(" />");
            }
        }

        public static void ValidateLoggedMessages(IReadOnlyList<ILoggedMessage> expectedLoggedMessages,
            IReadOnlyList<ILoggedMessage> actualLoggedMessages)
        {
            Assert.AreEqual(expectedLoggedMessages.Count, actualLoggedMessages.Count);

            for (var i = 0; i < expectedLoggedMessages.Count; ++i)
                ValidateLoggedMessage(expectedLoggedMessages[i], actualLoggedMessages[i]);
        }

        public static void ValidateLoggedMessage(ILoggedMessage expectedLoggedMessage, ILoggedMessage actualLoggedMessage)
        {
            Assert.AreEqual(expectedLoggedMessage.MessageCategory, actualLoggedMessage.MessageCategory);
            Assert.AreEqual(expectedLoggedMessage.MessageType, actualLoggedMessage.MessageType);
            Assert.AreEqual(expectedLoggedMessage.DataStoreId, actualLoggedMessage.DataStoreId);

            Assert.AreEqual(expectedLoggedMessage.DataStoreItem, actualLoggedMessage.DataStoreItem);

            Assert.IsNotEmpty(actualLoggedMessage.Message);
            Assert.IsTrue(expectedLoggedMessage.Message.Length == 0 || actualLoggedMessage.Message.Contains(expectedLoggedMessage.Message));
        }
    }
}