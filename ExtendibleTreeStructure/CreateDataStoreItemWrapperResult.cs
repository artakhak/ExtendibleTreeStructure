using ExtendibleTreeStructure.MessageLogging;

namespace ExtendibleTreeStructure;

/// <summary>
/// Result o creating an instance of <typeparamref name="TDataStoreItemWrapper"/> of type <see cref="DataStoreItemWrapper{TNonCopyDataStoreItem}"/>.
/// </summary>
public class CreateDataStoreItemWrapperResult<TNonCopyDataStoreItem, TDataStoreItemWrapper> 
    where TNonCopyDataStoreItem : class, IDataStoreItem, INonCopyDataStoreItem, IDataStoreItemCanBeCopiedWithNullParent<TNonCopyDataStoreItem>
    where TDataStoreItemWrapper : DataStoreItemWrapper<TNonCopyDataStoreItem, TDataStoreItemWrapper>
{
    /// <summary>
    /// Constructor.
    /// </summary>
    /// <param name="createdDataStoreItemWrapper">Created instance of <typeparamref name="TDataStoreItemWrapper"/>.</param>
    /// <param name="messageToLog">An instance of <see cref="ILoggedMessage"/>. This value can be null. Pass non-null for errors, warning, or info level messages.</param>
    public CreateDataStoreItemWrapperResult(TDataStoreItemWrapper? createdDataStoreItemWrapper, ILoggedMessage? messageToLog)
    {
        CreatedDataStoreItemWrapper = createdDataStoreItemWrapper;
        MessageToLog = messageToLog;
    }

    /// <summary>
    /// Created instance of <typeparamref name="TDataStoreItemWrapper"/>.
    /// </summary>
    public TDataStoreItemWrapper? CreatedDataStoreItemWrapper { get; }

    /// <summary>
    /// An instance of <see cref="ILoggedMessage"/>. This value can be null. Assign this property for errors, warning, or info level messages.
    /// </summary>
    public ILoggedMessage? MessageToLog { get; }
}