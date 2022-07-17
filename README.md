## This is a high level overview of ExtendibleTreeStructure package. For more details look at the code docs and the tests in ExtendibleTreeStructure.Tests.

## Overview

**ExtendibleTreeStructure** is a .NET package that allows easy configuration of tree structures that need to be copied to some other trees with new parents and children.
The package allows providing as an input collections (called data stores in subsequent text) of simple objects (called data store items in subsequent text) that have minimum state (id, parent id, priority, id and data store id of copied data store item, etc). 
The classes in this package construct proper tree structures for each data store. 

Good example of application of **ExtendibleTreeStructure** package is when we want to configure menu structure of a project (say in xml file). 
Lets say some part of the menu structure is common for all file types (e.g., File Menu with Exit, Save, SaveAs menu items, Edit menu with search, etc).
Also, lets suppose that the project supports C# and image file types, that support some additional functionality, and require new menu items on top of common (shared) menu structure.
In this scenario we can provide a menu data store with shared data store items. 
Then we can define two more data stores for these two file types, with additional data store items (menu items) specific to these file types.
In these new data stores we can copy the data store items in shared data store while spcifying new parent id (or no parent at all). Say we can copy File menu bar to data store for C# file type, and add new data store item for 'Compile' with parent id equal to id of File menu bar data store item.
In other words, we specify copy data store items (data store items that specify the referenced data store item data id), with new parent id (or no parent id at all). Also, we can add new data store items with parent Ids equal to ids of children of copied data store items (i.e., add new children to children of copied data store items).

The classes in this package will create tree structures in all the specified data stores with proper parent/children relationship, and will make sure the referenced (copied) data store items are copied under new parents (or are copied as top level data store items).
Also, the package logs any errors, and prevents circular references that might otherwise result via parent/reference relationships.

The following is some vocabulary we use in this document to describe the package

- Data store item: simple structure that has an Id, Priority and might have parent tree item Id (see interfaces
  **ExtendibleTreeStructure.IDataStoreItem**, **ExtendibleTreeStructure.INonCopyDataStoreItem**, **ExtendibleTreeStructure.ICopyDataStoreItem**, and **ExtendibleTreeStructure.ICanHaveParent**).
- Data store: collection of data store items (see **ExtendibleTreeStructure.IDataStore&lt;TDataStoreItem&gt;**)
- Data store item wrapper: A wrapper object of type **ExtendibleTreeStructure.DataStoreItemWrapper&lt;TNonCopyDataStoreItem, TDataStoreItemWrapper&gt;** constructed from data store items by **ExtendibleTreeStructure.IDataStoresCache&lt;TNonCopyDataStoreItem, TDataStoreItemWrapper&gt;**. **DataStoreItemWrapper&lt;TNonCopyDataStoreItem, TDataStoreItemWrapper&gt** has a reference to data store item, parent data store item wrapper, and child data store item wrappers.
- Data stores cache (see **ExtendibleTreeStructure.IDataStoresCache&lt;TNonCopyDataStoreItem, TDataStoreItemWrapper&gt;** ) that loads a cache of data store item wrappers (i.e., **ExtendibleTreeStructure.IDataStoresCache&lt;TNonCopyDataStoreItem, TDataStoreItemWrapper&gt;**) from data stores. Use the default implementation **ExtendibleTreeStructure.DataStoresCache<TDataStoreItem, TNonCopyDataStoreItem, TDataStoreItemWrapper>**) or its subclass. See an implementation **ExtendibleTreeStructure.Tests.TestDataStoresCache** for an example.
- When data **ExtendibleTreeStructure.IDataStoresCache&lt;TNonCopyDataStoreItem, TDataStoreItemWrapper&gt;** is loaded, any errors are logged with details of data store items that are sources of errors. Also, the prevents circular references and logs the paths in circular references (see examples below).  

Below is an example of constructing data stores (see collection menuDataStores in this code), and then using this collection of data stores to construct an instance of **ExtendibleTreeStructure.IDataStoresCache&lt;TNonCopyDataStoreItem, TDataStoreItemWrapper&gt;** (i.e., **TestDataStoresCache** in this example).

- The last line in the example below visualizes the **ExtendibleTreeStructure.IDataStoresCache&lt;TNonCopyDataStoreItem, TDataStoreItemWrapper&gt;** (i.e., **ExtendibleTreeStructure.Tests.TestDataStoresCache**).

## Example of constructing a cache that contains tree structures from data stores. See test method **SimpleDataStoresCacheLoadDemoTest** in class **ExtendibleTreeStructure.Tests.SuccessfulCacheBuildTests.SuccessfulCacheBuildTests** for the complete code.

```C#

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
```

## Visualized instance of **ExtendibleTreeStructure.IDataStoresCache&lt;TNonCopyDataStoreItem, TDataStoreItemWrapper&gt;** (**loadedDataStoresCache** in code above).
The xml file below is the visualized text for loadedDataStoresCache variable in C# code above 

```XML
<?xml version="1.0" encoding="utf-8" ?>
<menuDataStores>
	<menuDataStore id="SharedMenuObjects">
		<menuItemCollection id="SharedToolsCollection">
			<menuItem commandId="Projects" parentId="ExtendibleTreeStructure.Tests.MenuIds.SharedToolsCollection" />
			<menuItem commandId="Errors" parentId="ExtendibleTreeStructure.Tests.MenuIds.SharedToolsCollection" />
		</menuItemCollection>
	</menuDataStore>
	<menuDataStore id="NoFileSelectedMenuObjects">
		<menuBar id="NoFileSelectedDefaultMenuBar">
			<menuBarItem commandId="ViewMenuBarItem" parentMenuBarId="ExtendibleTreeStructure.Tests.MenuIds.NoFileSelectedDefaultMenuBar">
				<menuItem commandId="ToolsSubmenuItem" parentId="ExtendibleTreeStructure.Tests.CommandIds.ViewMenuBarItem">
					<menuItemCollection id="SharedToolsCollection" parentId="ExtendibleTreeStructure.Tests.CommandIds.ToolsSubmenuItem">
						<menuItem commandId="Projects" parentId="ExtendibleTreeStructure.Tests.MenuIds.SharedToolsCollection">
							<menuItem commandId="Git" parentId="ExtendibleTreeStructure.Tests.CommandIds.Projects" />
						</menuItem>
						<menuItem commandId="Errors" parentId="ExtendibleTreeStructure.Tests.MenuIds.SharedToolsCollection" />
					</menuItemCollection>
				</menuItem>
			</menuBarItem>
		</menuBar>
	</menuDataStore>
	<menuDataStore id="TextFileMenuObjects">
		<menuBar id="NoFileSelectedDefaultMenuBar">
			<menuBarItem commandId="ViewMenuBarItem" parentMenuBarId="ExtendibleTreeStructure.Tests.MenuIds.NoFileSelectedDefaultMenuBar">
				<menuItem commandId="ToolsSubmenuItem" parentId="ExtendibleTreeStructure.Tests.CommandIds.ViewMenuBarItem">
					<menuItemCollection id="SharedToolsCollection" parentId="ExtendibleTreeStructure.Tests.CommandIds.ToolsSubmenuItem">
						<menuItem commandId="Projects" parentId="ExtendibleTreeStructure.Tests.MenuIds.SharedToolsCollection">
							<menuItem commandId="RecentLocations" parentId="ExtendibleTreeStructure.Tests.CommandIds.Projects" priority="0" />
							<menuItem commandId="Git" parentId="ExtendibleTreeStructure.Tests.CommandIds.Projects" />
						</menuItem>
						<menuItem commandId="Errors" parentId="ExtendibleTreeStructure.Tests.MenuIds.SharedToolsCollection" />
					</menuItemCollection>
				</menuItem>
			</menuBarItem>
			<menuBarItem commandId="BuildMenuBarItem" parentMenuBarId="ExtendibleTreeStructure.Tests.MenuIds.NoFileSelectedDefaultMenuBar">
				<menuItem commandId="RecentFiles" parentId="ExtendibleTreeStructure.Tests.CommandIds.BuildMenuBarItem" />
			</menuBarItem>
		</menuBar>
	</menuDataStore>
</menuDataStores>
```

## Messages logged by code above
[Data Store Id:637754484146601333, Data Store Item: [menu object (Id:CommandIds.SaveToCloud), (ParentId:MenuIds.NoFileSelectedDefaultMenuBar), (DataStoreId:MenuIds.TextFileMenuObjects)], MessageType: InvalidChildDataStoreItem, MessageCategory: Error]

		Message:[Menu object (Id:CommandIds.SaveToCloud), (ParentId:MenuIds.NoFileSelectedDefaultMenuBar), (DataStoreId:MenuIds.TextFileMenuObjects)] cannot be used as a child 
               for [menu object (Id:MenuIds.NoFileSelectedDefaultMenuBar), (DataStoreId:MenuIds.TextFileMenuObjects)].
## Some more examples
- The examples below show data stores as xml files (the code parses the xml file into collection of **IDataStore&lt;TDataStoreItem&gt;** objects), then an instance of **IDataStoreItemsCache&lt;TNonCopyDataStoreItem, TDataStoreItemWrapper&gt;** loaded from the data stores in xml file visualized into another xml file.

## Simple example demonstrating copying a data store items from a different data store, and extending the copied data store with new child data store items (pretty similar to example above).

- Input data stores
```XML
<?xml version="1.0" encoding="utf-8" ?>
<menuDataStores
	xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance'
	xsi:noNamespaceSchemaLocation="http://oroptimizer.com/ExtendibleTreeStructure/TestsXmlSchemas/MenuDataStoresSchema.xsd">
	<menuDataStore id="SharedMenuObjects">
		<menuItemCollection id="RecentObjectsCollection" usesMenuSeparator="true" />

		<menuItem commandId="RecentFiles" parentId="RecentObjectsCollection" />
		<menuItem commandId="RecentlyChangedFiles" parentId="RecentObjectsCollection" />
	</menuDataStore>

	<menuDataStore id="SharedMenuObjects2">
		<menuBarItem commandId="ViewMenuBarItem"/>

		<!--1. Copy RecentObjectsCollection item from SharedMenuObjects (along with its children) data store as a child 
		of ViewMenuBarItem in this store 
		-->
		<copyMenuObject referencedMenuDataStoreId="SharedMenuObjects" referencedMenuObjectId="RecentObjectsCollection"
		                parentId="ExtendibleTreeStructure.Tests.CommandIds.ViewMenuBarItem" />

		<!--2. Add new children to copied RecentObjectsCollection-->
		<menuItem commandId="RecentChanges" parentId="RecentObjectsCollection" />
		<!--3. Add RecentSubmissions as a child to RecentFiles that is a child of copied RecentObjectsCollection-->
		<menuItem commandId="RecentSubmissions" parentId="ExtendibleTreeStructure.Tests.CommandIds.RecentFiles" />
	</menuDataStore>
</menuDataStores>
```
- Loaded **ExtendibleTreeStructure.IDataStoresCache&lt;TNonCopyDataStoreItem, TDataStoreItemWrapper&gt;**
```XML
<?xml version="1.0" encoding="utf-8" ?>
<menuDataStores>
	<menuDataStore id="SharedMenuObjects">
		<menuItemCollection id="RecentObjectsCollection" usesMenuSeparator="true">
			<menuItem commandId="RecentFiles" parentId="ExtendibleTreeStructure.Tests.MenuIds.RecentObjectsCollection" />
			<menuItem commandId="RecentlyChangedFiles" parentId="ExtendibleTreeStructure.Tests.MenuIds.RecentObjectsCollection" />
		</menuItemCollection>
	</menuDataStore>
	<menuDataStore id="SharedMenuObjects2">
		<menuBarItem commandId="ViewMenuBarItem">
			<menuItemCollection id="RecentObjectsCollection" usesMenuSeparator="true" parentId="ExtendibleTreeStructure.Tests.CommandIds.ViewMenuBarItem">
				<menuItem commandId="RecentFiles" parentId="ExtendibleTreeStructure.Tests.MenuIds.RecentObjectsCollection">
					<menuItem commandId="RecentSubmissions" parentId="ExtendibleTreeStructure.Tests.CommandIds.RecentFiles" />
				</menuItem>
				<menuItem commandId="RecentlyChangedFiles" parentId="ExtendibleTreeStructure.Tests.MenuIds.RecentObjectsCollection" />
				<menuItem commandId="RecentChanges" parentId="ExtendibleTreeStructure.Tests.MenuIds.RecentObjectsCollection" />
			</menuItemCollection>
		</menuBarItem>
	</menuDataStore>
</menuDataStores>
```

## Copying a copy of data store item.

- Input data stores
```XML
<?xml version="1.0" encoding="utf-8" ?>
<menuDataStores
	xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance'
	xsi:noNamespaceSchemaLocation="http://oroptimizer.com/ExtendibleTreeStructure/TestsXmlSchemas/MenuDataStoresSchema.xsd">
	<menuDataStore id="SharedMenuObjects">
		<menuItemCollection id="RecentObjectsCollection" usesMenuSeparator="true" />

		<menuItem commandId="RecentFiles" parentId="RecentObjectsCollection" />
		<menuItem commandId="RecentlyChangedFiles" parentId="RecentObjectsCollection" />
	</menuDataStore>

	<menuDataStore id="SharedMenuObjects2">
		<menuBarItem commandId="ViewMenuBarItem"/>

		<!--1. Copy RecentObjectsCollection item from SharedMenuObjects (along with its children) data store as a child 
		of ViewMenuBarItem in this store 
		-->
		<copyMenuObject referencedMenuDataStoreId="SharedMenuObjects" referencedMenuObjectId="RecentObjectsCollection"
		                parentId="ExtendibleTreeStructure.Tests.CommandIds.ViewMenuBarItem" />

		<!--2. Add new children to copied RecentObjectsCollection-->
		<menuItem commandId="RecentChanges" parentId="RecentObjectsCollection" />
	</menuDataStore>

	<menuDataStore id="NoFileSelectedMenuObjects">
		<menuBar id="NoFileSelectedDefaultMenuBar" />
		<menuBarItem commandId="FileMenuBarItem" parentMenuBarId="NoFileSelectedDefaultMenuBar"/>

		<!--Copy RecentObjectsCollection from SharedMenuObjects2 data store (which in turn was copied from SharedMenuObjects2 into SharedMenuObjects2)
		as a child of FileMenuBarItem-->
		<copyMenuObject referencedMenuDataStoreId="SharedMenuObjects2" referencedMenuObjectId="RecentObjectsCollection"
		                parentId="ExtendibleTreeStructure.Tests.CommandIds.FileMenuBarItem" />

		<!--2. Add new children to copied RecentObjectsCollection-->
		<menuItem commandId="RecentLocations" parentId="RecentObjectsCollection" />
		<menuItem commandId="RecentSubmissions" parentId="RecentObjectsCollection" />

		<!--3. 'Errors' menu item will be added as a child to RecentChanges that is a child of 
			RecentObjectsCollection in SharedMenuObjects2. Since RecentObjectsCollection and its children are copied from SharedMenuObjects2
			we can add new children to RecentObjectsCollection as well as any of its children (or children of children, etc).
		-->
		<menuItem commandId="Errors" parentId="ExtendibleTreeStructure.Tests.CommandIds.RecentChanges" />
	</menuDataStore>
</menuDataStores>
```
- Loaded **ExtendibleTreeStructure.IDataStoresCache&lt;TNonCopyDataStoreItem, TDataStoreItemWrapper&gt;**

```XML
<?xml version="1.0" encoding="utf-8" ?>
<menuDataStores>
	<menuDataStore id="SharedMenuObjects">
		<menuItemCollection id="RecentObjectsCollection" usesMenuSeparator="true">
			<menuItem commandId="RecentFiles" parentId="ExtendibleTreeStructure.Tests.MenuIds.RecentObjectsCollection" />
			<menuItem commandId="RecentlyChangedFiles" parentId="ExtendibleTreeStructure.Tests.MenuIds.RecentObjectsCollection" />
		</menuItemCollection>
	</menuDataStore>
	<menuDataStore id="SharedMenuObjects2">
		<menuBarItem commandId="ViewMenuBarItem">
			<menuItemCollection id="RecentObjectsCollection" usesMenuSeparator="true" parentId="ExtendibleTreeStructure.Tests.CommandIds.ViewMenuBarItem">
				<menuItem commandId="RecentFiles" parentId="ExtendibleTreeStructure.Tests.MenuIds.RecentObjectsCollection" />
				<menuItem commandId="RecentlyChangedFiles" parentId="ExtendibleTreeStructure.Tests.MenuIds.RecentObjectsCollection" />
				<menuItem commandId="RecentChanges" parentId="ExtendibleTreeStructure.Tests.MenuIds.RecentObjectsCollection" />
			</menuItemCollection>
		</menuBarItem>
	</menuDataStore>
	<menuDataStore id="NoFileSelectedMenuObjects">
		<menuBar id="NoFileSelectedDefaultMenuBar">
			<menuBarItem commandId="FileMenuBarItem" parentMenuBarId="ExtendibleTreeStructure.Tests.MenuIds.NoFileSelectedDefaultMenuBar">
				<menuItemCollection id="RecentObjectsCollection" usesMenuSeparator="true" parentId="ExtendibleTreeStructure.Tests.CommandIds.FileMenuBarItem">
					<menuItem commandId="RecentFiles" parentId="ExtendibleTreeStructure.Tests.MenuIds.RecentObjectsCollection" />
					<menuItem commandId="RecentlyChangedFiles" parentId="ExtendibleTreeStructure.Tests.MenuIds.RecentObjectsCollection" />
					<menuItem commandId="RecentChanges" parentId="ExtendibleTreeStructure.Tests.MenuIds.RecentObjectsCollection">
						<menuItem commandId="Errors" parentId="ExtendibleTreeStructure.Tests.CommandIds.RecentChanges" />
					</menuItem>
					<menuItem commandId="RecentLocations" parentId="ExtendibleTreeStructure.Tests.MenuIds.RecentObjectsCollection" />
					<menuItem commandId="RecentSubmissions" parentId="ExtendibleTreeStructure.Tests.MenuIds.RecentObjectsCollection" />
				</menuItemCollection>
			</menuBarItem>
		</menuBar>
	</menuDataStore>
</menuDataStores>
```

## Sorting data store items in parent data store using the value of Priority property of **ExtendibleTreeStructure.IDataStoreItem.**.

- Input data stores
```XML
<?xml version="1.0" encoding="utf-8" ?>
<menuDataStores
	xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance'
	xsi:noNamespaceSchemaLocation="http://oroptimizer.com/ExtendibleTreeStructure/TestsXmlSchemas/MenuDataStoresSchema.xsd">
	<!--Default value for priority is (int.MaxValue/2)-->
	<menuDataStore id="SharedMenuObjects">
		<!--This will be added as the last top level item in SharedMenuObjects.-->
		<menuItemCollection id="RecentObjectsCollection" usesMenuSeparator="true" />

		<menuItem commandId="RecentFiles" parentId="RecentObjectsCollection" />
		<menuItem commandId="RecentLocations" parentId="RecentObjectsCollection" priority="20" />
		<menuItem commandId="RecentlyChangedFiles" parentId="RecentObjectsCollection" priority="10" />

		<!--EncodingUtf8 will be copied under RecentObjectsCollection with priority set in SharedMenuObjects2-->
		<menuItem commandId="EncodingUtf8"/>
		<!--SaveFile will be copied under RecentObjectsCollection without changing the priority (which is null by default) in SharedMenuObjects2-->
		<menuItem commandId="SaveFile" />
		<!--SaveFileAs will be copied under RecentObjectsCollection with different priority in SharedMenuObjects2-->
		<menuItem commandId="SaveFileAs" priority="15"/>

		<!--Projects will have priority null, since the value is not explicitly set.
		Therefore, this item will appear after the items with set priorities.
		-->
		<menuItem commandId="Projects" />
		<!--This will be added as the first top level item in SharedMenuObjects.-->
		<menuItem commandId="Errors" priority="10"/>
		<menuItem commandId="BuildSolution" priority="20"/>
	</menuDataStore>

	<menuDataStore id="SharedMenuObjects2">
		<menuBarItem commandId="ViewMenuBarItem"/>

		<!--1. Copy RecentObjectsCollection item from SharedMenuObjects (along with its children) data store as a child 
		of ViewMenuBarItem in this store 
		-->
		<copyMenuObject referencedMenuDataStoreId="SharedMenuObjects" referencedMenuObjectId="RecentObjectsCollection"
		                parentId="ExtendibleTreeStructure.Tests.CommandIds.ViewMenuBarItem" />

		<!--2. Add new children to copied RecentObjectsCollection-->
		<!--This will be added as the first child in copied RecentObjectsCollection-->
		<menuItem commandId="RecentChanges" parentId="RecentObjectsCollection" priority="3" />
		<!--This will be added as the last child in RecentObjectsCollection-->
		<menuItem commandId="RecentSubmissions" parentId="RecentObjectsCollection" />

		<copyMenuObject referencedMenuDataStoreId="SharedMenuObjects" 
		                referencedMenuObjectId="ExtendibleTreeStructure.Tests.CommandIds.EncodingUtf8" 
		                parentId="RecentObjectsCollection" priority="12"/>
		<copyMenuObject referencedMenuDataStoreId="SharedMenuObjects"
		                referencedMenuObjectId="ExtendibleTreeStructure.Tests.CommandIds.SaveFile"
		                parentId="RecentObjectsCollection"/>
		<copyMenuObject referencedMenuDataStoreId="SharedMenuObjects"
		                referencedMenuObjectId="ExtendibleTreeStructure.Tests.CommandIds.SaveFileAs"
		                parentId="RecentObjectsCollection" priority="4"/>

		<!--if no value is specified for Priority, copied value will be used.-->
		<copyMenuObject referencedMenuDataStoreId="SharedMenuObjects" 
		                referencedMenuObjectId="ExtendibleTreeStructure.Tests.CommandIds.Projects"/>
		<copyMenuObject referencedMenuDataStoreId="SharedMenuObjects" 
		                referencedMenuObjectId="ExtendibleTreeStructure.Tests.CommandIds.Errors"
		                priority="30" />

		<!--The value of priority as copied from SharedMenuObjects is 20, however we change it to 5, therefore, this item
			will be the second top level item (the first will be FileMenuBarItem wit Priority=2)
		-->
		<copyMenuObject referencedMenuDataStoreId="SharedMenuObjects" 
		                referencedMenuObjectId="ExtendibleTreeStructure.Tests.CommandIds.BuildSolution"
		                priority="5"/>

		<!--This will be added as the first top level item.-->
		<menuBarItem commandId="FileMenuBarItem" priority="2"/>
	</menuDataStore>
</menuDataStores>
```
- Loaded **ExtendibleTreeStructure.IDataStoresCache&lt;TNonCopyDataStoreItem, TDataStoreItemWrapper&gt;**
```XML
<?xml version="1.0" encoding="utf-8" ?>
<menuDataStores>
	<menuDataStore id="SharedMenuObjects">
		<menuItem commandId="Errors" priority="10" />
		<menuItem commandId="SaveFileAs" priority="15" />
		<menuItem commandId="BuildSolution" priority="20" />
		<menuItemCollection id="RecentObjectsCollection" usesMenuSeparator="true">
			<menuItem commandId="RecentlyChangedFiles" parentId="ExtendibleTreeStructure.Tests.MenuIds.RecentObjectsCollection" priority="10" />
			<menuItem commandId="RecentLocations" parentId="ExtendibleTreeStructure.Tests.MenuIds.RecentObjectsCollection" priority="20" />
			<menuItem commandId="RecentFiles" parentId="ExtendibleTreeStructure.Tests.MenuIds.RecentObjectsCollection" />
		</menuItemCollection>
		<menuItem commandId="EncodingUtf8" />
		<menuItem commandId="SaveFile" />
		<menuItem commandId="Projects" />
	</menuDataStore>
	<menuDataStore id="SharedMenuObjects2">
		<menuBarItem commandId="FileMenuBarItem" priority="2" />
		<menuItem commandId="BuildSolution" priority="5" />
		<menuItem commandId="Errors" priority="30" />
		<menuBarItem commandId="ViewMenuBarItem">
			<menuItemCollection id="RecentObjectsCollection" usesMenuSeparator="true" parentId="ExtendibleTreeStructure.Tests.CommandIds.ViewMenuBarItem">
				<menuItem commandId="RecentChanges" parentId="ExtendibleTreeStructure.Tests.MenuIds.RecentObjectsCollection" priority="3" />
				<menuItem commandId="SaveFileAs" parentId="ExtendibleTreeStructure.Tests.MenuIds.RecentObjectsCollection" priority="4" />
				<menuItem commandId="RecentlyChangedFiles" parentId="ExtendibleTreeStructure.Tests.MenuIds.RecentObjectsCollection" priority="10" />
				<menuItem commandId="EncodingUtf8" parentId="ExtendibleTreeStructure.Tests.MenuIds.RecentObjectsCollection" priority="12" />
				<menuItem commandId="RecentLocations" parentId="ExtendibleTreeStructure.Tests.MenuIds.RecentObjectsCollection" priority="20" />
				<menuItem commandId="RecentFiles" parentId="ExtendibleTreeStructure.Tests.MenuIds.RecentObjectsCollection" />
				<menuItem commandId="RecentSubmissions" parentId="ExtendibleTreeStructure.Tests.MenuIds.RecentObjectsCollection" />
				<menuItem commandId="SaveFile" parentId="ExtendibleTreeStructure.Tests.MenuIds.RecentObjectsCollection" />
			</menuItemCollection>
		</menuBarItem>
		<menuItem commandId="Projects" />
	</menuDataStore>
</menuDataStores>
```

## Copy data store item multiple times in data store.
- Input data stores
```XML
<?xml version="1.0" encoding="utf-8" ?>
<menuDataStores
	xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance'
	xsi:noNamespaceSchemaLocation="http://oroptimizer.com/ExtendibleTreeStructure/TestsXmlSchemas/MenuDataStoresSchema.xsd">
	<menuDataStore id="SharedMenuObjects">
		<menuBarItem commandId="ViewMenuBarItem"/>
		<menuItemCollection id="RecentObjectsGroup" parentId="ExtendibleTreeStructure.Tests.CommandIds.ViewMenuBarItem"/>
		<menuItem commandId="RecentlyChangedFiles" parentId="ExtendibleTreeStructure.Tests.MenuIds.RecentObjectsGroup"/>
		<menuItem commandId="RecentLocations" parentId="ExtendibleTreeStructure.Tests.MenuIds.RecentObjectsGroup"/>
	</menuDataStore>

	<menuDataStore id="SharedMenuObjects2">

		<!--Copy RecentObjectsGroup from SharedMenuObjects with the same id (Id value will be the same as the Id in copied 
		data store item in SharedMenuObjects, since we do not explicitly set the Id) in this data store as a top level data store 
		item (no parent Id specified).
		-->
		<copyMenuObject referencedMenuDataStoreId="SharedMenuObjects"
		                referencedMenuObjectId="ExtendibleTreeStructure.Tests.MenuIds.RecentObjectsGroup"/>

		<!--Add RecentFiles as a child to RecentObjectsGroup copied from SharedMenuObjects.
			Note, RecentFiles, along with its child Errors will appear under all copies of RecentObjectsGroup.
		-->
		<menuItem commandId="RecentFiles" parentId="ExtendibleTreeStructure.Tests.MenuIds.RecentObjectsGroup"/>
		<menuItem commandId="Errors" parentId="ExtendibleTreeStructure.Tests.CommandIds.RecentFiles"/>

		<menuBarItem commandId="FileMenuBarItem"/>
		
		<!---Copy RecentObjectsGroup in SharedMenuObjects2 data store (which is a copy of RecentObjectsGroup in SharedMenuObjects2 with new child RecentFiles
		added in this data store) and add it as a child of FileMenuBarItem in this data store twice (once with new Id=RecentObjectsGroupCopy1,
		and another time with new Id=RecentObjectsGroupCopy2).
		Note, since the ids of data store items specified in data store should be unique, we need to change the id oc copied object
		if we copy the data store item more than once. However, we need to worry for Id being unique only for data store items specified in 
		copyMenuObject, and not of children of copied dat store items. For example RecentObjectsGroup has children with ids RecentlyChangedFiles
		and RecentLocations, which are copied multiple times.
		-->
		<copyMenuObject referencedMenuDataStoreId="SharedMenuObjects2"
		                referencedMenuObjectId="ExtendibleTreeStructure.Tests.MenuIds.RecentObjectsGroup"
						id="RecentObjectsGroupCopy1"
		                parentId="ExtendibleTreeStructure.Tests.CommandIds.FileMenuBarItem"/>
		
		<copyMenuObject referencedMenuDataStoreId="SharedMenuObjects2"
		                referencedMenuObjectId="ExtendibleTreeStructure.Tests.MenuIds.RecentObjectsGroup"
		                id="RecentObjectsGroupCopy2"
		                parentId="ExtendibleTreeStructure.Tests.CommandIds.FileMenuBarItem"/>

		<menuBarItem commandId="BuildMenuBarItem"/>

		<!---Make another copy of RecentObjectsGroup in SharedMenuObjects2 data store (which is a copy of RecentObjectsGroup in 
		SharedMenuObjects2 with new child RecentFiles
		added in this data store) and add it as a child of BuildMenuBarItem in this data store with new Id=RecentObjectsGroupCopy3.
		-->
		<copyMenuObject referencedMenuDataStoreId="SharedMenuObjects2"
		                referencedMenuObjectId="ExtendibleTreeStructure.Tests.MenuIds.RecentObjectsGroup"
		                id="RecentObjectsGroupCopy3"
		                parentId="ExtendibleTreeStructure.Tests.CommandIds.BuildMenuBarItem"/>

		<!--Add RecentSubmissions as a child of RecentLocations.
		Note, RecentLocations is not defined in this data store, but is a child of RecentObjectsGroup in SharedMenuObjects data store.
		Since RecentObjectsGroup is copied to this data store multiple times (as top level data store item, a well as a child of  
		FileMenuBarItem and BuildMenuBarItem), the children (including RecentSubmissions) is copied multiple times too.
		As a result, RecentSubmissions will be added as a child under all copies of RecentLocations in this data store).		
		-->
		<menuItem commandId="RecentSubmissions" parentId="ExtendibleTreeStructure.Tests.CommandIds.RecentLocations"/>
	</menuDataStore>
</menuDataStores>
```

- Loaded **ExtendibleTreeStructure.IDataStoresCache&lt;TNonCopyDataStoreItem, TDataStoreItemWrapper&gt;**
```XML
<?xml version="1.0" encoding="utf-8" ?>
<menuDataStores>
	<menuDataStore id="SharedMenuObjects">
		<menuBarItem commandId="ViewMenuBarItem">
			<menuItemCollection id="RecentObjectsGroup" parentId="ExtendibleTreeStructure.Tests.CommandIds.ViewMenuBarItem">
				<menuItem commandId="RecentlyChangedFiles" parentId="ExtendibleTreeStructure.Tests.MenuIds.RecentObjectsGroup" />
				<menuItem commandId="RecentLocations" parentId="ExtendibleTreeStructure.Tests.MenuIds.RecentObjectsGroup" />
			</menuItemCollection>
		</menuBarItem>
	</menuDataStore>
	<menuDataStore id="SharedMenuObjects2">
		<menuItemCollection id="RecentObjectsGroup">
			<menuItem commandId="RecentlyChangedFiles" parentId="ExtendibleTreeStructure.Tests.MenuIds.RecentObjectsGroup" />
			<menuItem commandId="RecentLocations" parentId="ExtendibleTreeStructure.Tests.MenuIds.RecentObjectsGroup">
				<menuItem commandId="RecentSubmissions" parentId="ExtendibleTreeStructure.Tests.CommandIds.RecentLocations" />
			</menuItem>
			<menuItem commandId="RecentFiles" parentId="ExtendibleTreeStructure.Tests.MenuIds.RecentObjectsGroup">
				<menuItem commandId="Errors" parentId="ExtendibleTreeStructure.Tests.CommandIds.RecentFiles" />
			</menuItem>
		</menuItemCollection>
		<menuBarItem commandId="FileMenuBarItem">
			<menuItemCollection id="RecentObjectsGroupCopy1" parentId="ExtendibleTreeStructure.Tests.CommandIds.FileMenuBarItem">
				<menuItem commandId="RecentlyChangedFiles" parentId="ExtendibleTreeStructure.Tests.MenuIds.RecentObjectsGroupCopy1" />
				<menuItem commandId="RecentLocations" parentId="ExtendibleTreeStructure.Tests.MenuIds.RecentObjectsGroupCopy1">
					<menuItem commandId="RecentSubmissions" parentId="ExtendibleTreeStructure.Tests.CommandIds.RecentLocations" />
				</menuItem>
				<menuItem commandId="RecentFiles" parentId="ExtendibleTreeStructure.Tests.MenuIds.RecentObjectsGroupCopy1">
					<menuItem commandId="Errors" parentId="ExtendibleTreeStructure.Tests.CommandIds.RecentFiles" />
				</menuItem>
			</menuItemCollection>
			<menuItemCollection id="RecentObjectsGroupCopy2" parentId="ExtendibleTreeStructure.Tests.CommandIds.FileMenuBarItem">
				<menuItem commandId="RecentlyChangedFiles" parentId="ExtendibleTreeStructure.Tests.MenuIds.RecentObjectsGroupCopy2" />
				<menuItem commandId="RecentLocations" parentId="ExtendibleTreeStructure.Tests.MenuIds.RecentObjectsGroupCopy2">
					<menuItem commandId="RecentSubmissions" parentId="ExtendibleTreeStructure.Tests.CommandIds.RecentLocations" />
				</menuItem>
				<menuItem commandId="RecentFiles" parentId="ExtendibleTreeStructure.Tests.MenuIds.RecentObjectsGroupCopy2">
					<menuItem commandId="Errors" parentId="ExtendibleTreeStructure.Tests.CommandIds.RecentFiles" />
				</menuItem>
			</menuItemCollection>
		</menuBarItem>
		<menuBarItem commandId="BuildMenuBarItem">
			<menuItemCollection id="RecentObjectsGroupCopy3" parentId="ExtendibleTreeStructure.Tests.CommandIds.BuildMenuBarItem">
				<menuItem commandId="RecentlyChangedFiles" parentId="ExtendibleTreeStructure.Tests.MenuIds.RecentObjectsGroupCopy3" />
				<menuItem commandId="RecentLocations" parentId="ExtendibleTreeStructure.Tests.MenuIds.RecentObjectsGroupCopy3">
					<menuItem commandId="RecentSubmissions" parentId="ExtendibleTreeStructure.Tests.CommandIds.RecentLocations" />
				</menuItem>
				<menuItem commandId="RecentFiles" parentId="ExtendibleTreeStructure.Tests.MenuIds.RecentObjectsGroupCopy3">
					<menuItem commandId="Errors" parentId="ExtendibleTreeStructure.Tests.CommandIds.RecentFiles" />
				</menuItem>
			</menuItemCollection>
		</menuBarItem>
	</menuDataStore>
</menuDataStores>
```

## Data stores referencing each other.

- Input data stores
```XML
<?xml version="1.0" encoding="utf-8" ?>
<menuDataStores
	xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance'
	xsi:noNamespaceSchemaLocation="http://oroptimizer.com/ExtendibleTreeStructure/TestsXmlSchemas/MenuDataStoresSchema.xsd">
	<menuDataStore id="SharedMenuObjects">
		<menuItemCollection id="RecentObjectsCollection" usesMenuSeparator="true" />

		<menuItem commandId="RecentFiles" parentId="RecentObjectsCollection" />
		<menuItem commandId="RecentlyChangedFiles" parentId="RecentObjectsCollection" />
		<copyMenuObject referencedMenuDataStoreId="SharedMenuObjects2" 
		                referencedMenuObjectId="ExtendibleTreeStructure.Tests.CommandIds.SaveToCloud"
						parentId="ExtendibleTreeStructure.Tests.CommandIds.RecentlyChangedFiles" />
	</menuDataStore>

	<menuDataStore id="SharedMenuObjects2">
		<menuBarItem commandId="ViewMenuBarItem"/>

		<!--1. Copy RecentObjectsCollection item from SharedMenuObjects (along with its children) data store as a child 
		of ViewMenuBarItem in this store 
		-->
		<copyMenuObject referencedMenuDataStoreId="SharedMenuObjects" referencedMenuObjectId="RecentObjectsCollection"
		                parentId="ExtendibleTreeStructure.Tests.CommandIds.ViewMenuBarItem" />

		<!--2. Add new children to copied RecentObjectsCollection-->
		<menuItem commandId="RecentChanges" parentId="RecentObjectsCollection" />

		<menuItem commandId="SaveToCloud"/>
		<menuItem commandId="SaveFile" parentId="ExtendibleTreeStructure.Tests.CommandIds.SaveToCloud"/>
	</menuDataStore>
</menuDataStores>
```

- Loaded **ExtendibleTreeStructure.IDataStoresCache&lt;TNonCopyDataStoreItem, TDataStoreItemWrapper&gt;**
```XML
<?xml version="1.0" encoding="utf-8" ?>
<menuDataStores>
	<menuDataStore id="SharedMenuObjects">
		<menuItemCollection id="RecentObjectsCollection" usesMenuSeparator="true">
			<menuItem commandId="RecentFiles" parentId="ExtendibleTreeStructure.Tests.MenuIds.RecentObjectsCollection" />
			<menuItem commandId="RecentlyChangedFiles" parentId="ExtendibleTreeStructure.Tests.MenuIds.RecentObjectsCollection">
				<menuItem commandId="SaveToCloud" parentId="ExtendibleTreeStructure.Tests.CommandIds.RecentlyChangedFiles">
					<menuItem commandId="SaveFile" parentId="ExtendibleTreeStructure.Tests.CommandIds.SaveToCloud" />
				</menuItem>
			</menuItem>
		</menuItemCollection>
	</menuDataStore>
	<menuDataStore id="SharedMenuObjects2">
		<menuBarItem commandId="ViewMenuBarItem">
			<menuItemCollection id="RecentObjectsCollection" usesMenuSeparator="true" parentId="ExtendibleTreeStructure.Tests.CommandIds.ViewMenuBarItem">
				<menuItem commandId="RecentFiles" parentId="ExtendibleTreeStructure.Tests.MenuIds.RecentObjectsCollection" />
				<menuItem commandId="RecentlyChangedFiles" parentId="ExtendibleTreeStructure.Tests.MenuIds.RecentObjectsCollection">
					<menuItem commandId="SaveToCloud" parentId="ExtendibleTreeStructure.Tests.CommandIds.RecentlyChangedFiles">
						<menuItem commandId="SaveFile" parentId="ExtendibleTreeStructure.Tests.CommandIds.SaveToCloud" />
					</menuItem>
				</menuItem>
				<menuItem commandId="RecentChanges" parentId="ExtendibleTreeStructure.Tests.MenuIds.RecentObjectsCollection" />
			</menuItemCollection>
		</menuBarItem>
		<menuItem commandId="SaveToCloud">
			<menuItem commandId="SaveFile" parentId="ExtendibleTreeStructure.Tests.CommandIds.SaveToCloud" />
		</menuItem>
	</menuDataStore>
</menuDataStores>
```

## Circular references (prevention and error logging).

- Input data stores
```XML
<?xml version="1.0" encoding="utf-8" ?>
<menuDataStores
	xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance'
	xsi:noNamespaceSchemaLocation="http://oroptimizer.com/ExtendibleTreeStructure/TestsXmlSchemas/MenuDataStoresSchema.xsd">
	<!--ToolsSubmenuItem in SharedMenuObjects goes to child RecentFiles
	   RecentFiles goes to child Projects in SharedMenuObjects
	   Projects in SharedMenuObjects goes to copied menu object Projects in SharedMenuObjects2
	   Projects in SharedMenuObjects2 goes to child ToolsSubmenuItem in SharedMenuObjects2
	   ToolsSubmenuItem in SharedMenuObjects2 goes to copied menu object ToolsSubmenuItem in SharedMenuObjects
	   ToolsSubmenuItem in SharedMenuObjects goes to child RecentFiles, which is the menu object we started with
	   -->
	<menuDataStore id="SharedMenuObjects">

		<menuItem commandId="ToolsSubmenuItem" />

		<menuItem commandId="RecentSubmissions" parentId="ExtendibleTreeStructure.Tests.CommandIds.ToolsSubmenuItem" />
		
		<menuItem commandId="RecentFiles" parentId="ExtendibleTreeStructure.Tests.CommandIds.ToolsSubmenuItem" />

		<copyMenuObject referencedMenuDataStoreId="SharedMenuObjects2" 
		                referencedMenuObjectId="ExtendibleTreeStructure.Tests.CommandIds.Projects"
		                parentId="ExtendibleTreeStructure.Tests.CommandIds.RecentFiles"/>
		
		<menuItem commandId="RecentChanges" parentId="ExtendibleTreeStructure.Tests.CommandIds.ToolsSubmenuItem" />
	</menuDataStore>

	<menuDataStore id="SharedMenuObjects2">
		<menuItem commandId="Projects"/>
		<copyMenuObject referencedMenuDataStoreId="SharedMenuObjects" 
		                referencedMenuObjectId="ExtendibleTreeStructure.Tests.CommandIds.ToolsSubmenuItem" 
		                parentId="ExtendibleTreeStructure.Tests.CommandIds.Projects"/>
	</menuDataStore>
</menuDataStores>
```
- Loaded **ExtendibleTreeStructure.IDataStoresCache&lt;TNonCopyDataStoreItem, TDataStoreItemWrapper&gt;**
```XML
<?xml version="1.0" encoding="utf-8" ?>
<menuDataStores>
	<menuDataStore id="SharedMenuObjects">
		<menuItem commandId="ToolsSubmenuItem">
			<menuItem commandId="RecentSubmissions" parentId="ExtendibleTreeStructure.Tests.CommandIds.ToolsSubmenuItem" />
			<menuItem commandId="RecentFiles" parentId="ExtendibleTreeStructure.Tests.CommandIds.ToolsSubmenuItem">
				<menuItem commandId="Projects" parentId="ExtendibleTreeStructure.Tests.CommandIds.RecentFiles" />
			</menuItem>
			<menuItem commandId="RecentChanges" parentId="ExtendibleTreeStructure.Tests.CommandIds.ToolsSubmenuItem" />
		</menuItem>
	</menuDataStore>
	<menuDataStore id="SharedMenuObjects2">
		<menuItem commandId="Projects">
			<menuItem commandId="ToolsSubmenuItem" parentId="ExtendibleTreeStructure.Tests.CommandIds.Projects">
				<menuItem commandId="RecentSubmissions" parentId="ExtendibleTreeStructure.Tests.CommandIds.ToolsSubmenuItem" />
				<menuItem commandId="RecentFiles" parentId="ExtendibleTreeStructure.Tests.CommandIds.ToolsSubmenuItem" />
				<menuItem commandId="RecentChanges" parentId="ExtendibleTreeStructure.Tests.CommandIds.ToolsSubmenuItem" />
			</menuItem>
		</menuItem>
	</menuDataStore>
</menuDataStores>
```
- Logged error messages.
When circular references are logged, the logged message is of type **ExtendibleTreeStructure.MessageLogging.ICircularReferencesErrorMessage** that extends **ExtendibleTreeStructure.MessageLogging.ILoggedMessage** and has a property **CircularReferencesPath** of type **IReadOnlyList&lt;PathComponentEdge&gt;** with more details on data store items that cause the circular references. Below, is the message that is logged for the data stores above (using the data in **ExtendibleTreeStructure.MessageLogging.ICircularReferencesErrorMessage**).

ERROR : [Data Store Id:MenuIds.SharedMenuObjects, Data Store Item (Id:CommandIds.ToolsSubmenuItem), MessageType:CircularReferences]
Message: Menu object (Id:CommandIds.ToolsSubmenuItem), (DataStoreId:MenuIds.SharedMenuObjects) results in the following circular references:
- [menu object (Id:CommandIds.ToolsSubmenuItem), (DataStoreId:MenuIds.SharedMenuObjects)] going to child [menu object (Id:CommandIds.RecentFiles), (ParentId:CommandIds.ToolsSubmenuItem), (DataStoreId:MenuIds.SharedMenuObjects)]
- [menu object (Id:CommandIds.RecentFiles), (ParentId:CommandIds.ToolsSubmenuItem), (DataStoreId:MenuIds.SharedMenuObjects)] going to child [copy menu object: (Id:CommandIds.Projects), (ParentId:CommandIds.RecentFiles), (DataStoreId:MenuIds.SharedMenuObjects), (ReferencedDataStoreId:MenuIds.SharedMenuObjects2), (ReferencedDataStoreItemId:CommandIds.Projects)]
- [copy menu object: (Id:CommandIds.Projects), (ParentId:CommandIds.RecentFiles), (DataStoreId:MenuIds.SharedMenuObjects), (ReferencedDataStoreId:MenuIds.SharedMenuObjects2), (ReferencedDataStoreItemId:CommandIds.Projects)] going to copied item [menu object (Id:CommandIds.Projects), (DataStoreId:MenuIds.SharedMenuObjects2)]
- [menu object (Id:CommandIds.Projects), (DataStoreId:MenuIds.SharedMenuObjects2)] going to child [copy menu object: (Id:CommandIds.ToolsSubmenuItem), (ParentId:CommandIds.Projects), (DataStoreId:MenuIds.SharedMenuObjects2), (ReferencedDataStoreId:MenuIds.SharedMenuObjects), (ReferencedDataStoreItemId:CommandIds.ToolsSubmenuItem)]
- [copy menu object: (Id:CommandIds.ToolsSubmenuItem), (ParentId:CommandIds.Projects), (DataStoreId:MenuIds.SharedMenuObjects2), (ReferencedDataStoreId:MenuIds.SharedMenuObjects), (ReferencedDataStoreItemId:CommandIds.ToolsSubmenuItem)] going to copied item [menu object (Id:CommandIds.ToolsSubmenuItem), (DataStoreId:MenuIds.SharedMenuObjects)]


ERROR : [Data Store Id:MenuIds.SharedMenuObjects2, Data Store Item (Id:CommandIds.ToolsSubmenuItem, ParentId:CommandIds.Projects), MessageType:FailedToCopyDataStoreItem]
Message: Failed to copy data store item referenced by [copy menu object: (Id:CommandIds.ToolsSubmenuItem), (ParentId:CommandIds.Projects), (DataStoreId:MenuIds.SharedMenuObjects2), (ReferencedDataStoreId:MenuIds.SharedMenuObjects), (ReferencedDataStoreItemId:CommandIds.ToolsSubmenuItem)]. This might be a result of other errors logged earlier.

ERROR : [Data Store Id:MenuIds.SharedMenuObjects, Data Store Item (Id:CommandIds.Projects, ParentId:CommandIds.RecentFiles), MessageType:FailedToCopyDataStoreItem]
Message: Failed to copy data store item referenced by [copy menu object: (Id:CommandIds.Projects), (ParentId:CommandIds.RecentFiles), (DataStoreId:MenuIds.SharedMenuObjects), (ReferencedDataStoreId:MenuIds.SharedMenuObjects2), (ReferencedDataStoreItemId:CommandIds.Projects)]. This might be a result of other errors logged earlier.