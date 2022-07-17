using ExtendibleTreeStructure.Tests.MenuItems;

namespace ExtendibleTreeStructure.Tests;

public class TestMenuDataObjectWrapperFactory : IDataStoreItemWrapperFactory<INonCopyMenuObject, MenuDataObjectWrapper>
{
    private readonly CreateMenuDataObjectWrapperDelegate? _createMenuDataObjectWrapperDelegate;

    public TestMenuDataObjectWrapperFactory()
    {

    }

    public TestMenuDataObjectWrapperFactory(CreateMenuDataObjectWrapperDelegate createMenuDataObjectWrapperDelegate)
    {
        _createMenuDataObjectWrapperDelegate = createMenuDataObjectWrapperDelegate;
    }

    public CreateDataStoreItemWrapperResult<INonCopyMenuObject, MenuDataObjectWrapper> Create(long dataStoreId, INonCopyMenuObject dataStoreItem, MenuDataObjectWrapper? parent = null)
    {
        if (_createMenuDataObjectWrapperDelegate != null)
        {
            var result = _createMenuDataObjectWrapperDelegate(dataStoreId, dataStoreItem, parent);

            return new CreateDataStoreItemWrapperResult<INonCopyMenuObject, MenuDataObjectWrapper>(result.menuDataObjectWrapper, result.loggedMessage);
        }

        return new CreateDataStoreItemWrapperResult<INonCopyMenuObject, MenuDataObjectWrapper>(
            new MenuDataObjectWrapper(dataStoreId, dataStoreItem, parent), null);
    }
}