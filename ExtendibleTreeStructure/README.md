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

The git repository https://github.com/artakhak/ExtendibleTreeStructure has more detailed documentation, along with the source code and tests project **ExtendibleTreeStructure.Tests** with good examples.