using ExtendibleTreeStructure.MessageLogging;
using ExtendibleTreeStructure.Tests.MenuItems;

namespace ExtendibleTreeStructure.Tests;

public delegate (MenuDataObjectWrapper? menuDataObjectWrapper, LoggedMessage? loggedMessage) CreateMenuDataObjectWrapperDelegate(long dataStoreId, INonCopyMenuObject dataStoreItem,
    MenuDataObjectWrapper? parent);