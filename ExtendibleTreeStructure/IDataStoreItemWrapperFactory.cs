namespace ExtendibleTreeStructure;

/// <summary>
/// A factory for creating instances of <typeparamref name="TDataStoreItemWrapper"/> of type <see cref="DataStoreItemWrapper{TNonCopyDataStoreItem, TDataStoreItemWrapper}"/>.
/// </summary>
/// <typeparam name="TNonCopyDataStoreItem">A generic type of <see cref="INonCopyDataStoreItem"/>.</typeparam>
/// <typeparam name="TDataStoreItemWrapper">A generic type of <see cref="DataStoreItemWrapper{TNonCopyDataStoreItem, TDataStoreItemWrapper}"/>.</typeparam>
public interface IDataStoreItemWrapperFactory<TNonCopyDataStoreItem, TDataStoreItemWrapper>
    where TNonCopyDataStoreItem : class, IDataStoreItem, INonCopyDataStoreItem, IDataStoreItemCanBeCopiedWithNullParent<TNonCopyDataStoreItem>
    where TDataStoreItemWrapper : DataStoreItemWrapper<TNonCopyDataStoreItem, TDataStoreItemWrapper>
{
    /// <summary>
    /// Creates a new instance of <see cref="CreateDataStoreItemWrapperResult{TNonCopyDataStoreItem, TDataStoreItemWrapper}"/> with an instance of created
    /// object of type <typeparamref name="TDataStoreItemWrapper"/> in property <see cref="CreateDataStoreItemWrapperResult{TNonCopyDataStoreItem, TDataStoreItemWrapper}.CreatedDataStoreItemWrapper"/>.
    /// </summary>
    /// <param name="dataStoreId">Data store Id of the data store that owns the data store item <paramref name="dataStoreItem"/>.</param>
    /// <param name="dataStoreItem">An instance of <typeparamref name="TNonCopyDataStoreItem"/> used to create the new instance of <typeparamref name="TDataStoreItemWrapper"/>.</param>
    /// <param name="parent">Parent to which the created instance of <typeparamref name="TDataStoreItemWrapper"/> isa added.</param>
    /// <returns>Returns an instance of <see cref="CreateDataStoreItemWrapperResult{TNonCopyDataStoreItem, TDataStoreItemWrapper}"/>.</returns>
    CreateDataStoreItemWrapperResult<TNonCopyDataStoreItem, TDataStoreItemWrapper> Create(long dataStoreId,
        TNonCopyDataStoreItem dataStoreItem, TDataStoreItemWrapper? parent = null);
}