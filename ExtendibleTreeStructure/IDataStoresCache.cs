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

using System;
using System.Collections.Generic;
using ExtendibleTreeStructure.CircularReferencePrevention;
using ExtendibleTreeStructure.MessageLogging;
using System.Text;

namespace ExtendibleTreeStructure;

/// <summary>
///Contains data on loaded data store caches (instances of <see cref="IDataStoreItemsCache{TNonCopyDataStoreItem, TDataStoreItemWrapper}"/>.
/// </summary>
/// <typeparam name="TNonCopyDataStoreItem"></typeparam>
/// <typeparam name="TDataStoreItemWrapper"></typeparam>
public interface IDataStoresCache<TNonCopyDataStoreItem, TDataStoreItemWrapper>
    where TNonCopyDataStoreItem : class, INonCopyDataStoreItem
    where TDataStoreItemWrapper : DataStoreItemWrapper<TNonCopyDataStoreItem, TDataStoreItemWrapper>
{
    /// <summary>
    /// Tries to retrieve an data store items cache for data store Id=<paramref name="dataStoreId"/>.
    /// </summary>
    /// <param name="dataStoreId">Data store Id.</param>
    /// <param name="dataStoreItemsCache">Retrieved data store items cache for data store Id=<paramref name="dataStoreId"/>.
    /// The value can be null.</param>
    /// <returns>Returns true, if data store items cache for data store Id=<paramref name="dataStoreId"/> was retrieved. Returns false otherwise.</returns>
    bool TryGetDataStore(long dataStoreId, out IDataStoreItemsCache<TNonCopyDataStoreItem, TDataStoreItemWrapper>? dataStoreItemsCache);

    /// <summary>
    /// List of data store caches of type <see cref="IDataStoreItemsCache{TNonCopyDataStoreItem, TDataStoreItemWrapper}"/>.
    /// </summary>
    IReadOnlyList<IDataStoreItemsCache<TNonCopyDataStoreItem, TDataStoreItemWrapper>> DataStoreItemsCacheList { get; }

    /// <summary>
    /// An  event fired when new message is available. Check the values of members <see cref="ILoggedMessage.MessageType"/> and <see cref="ILoggedMessage.Message"/>
    /// to properly handle the message (i.e., log the message, etc).
    /// </summary>
    event EventHandler<LoggedMessageEventArgs> DataStoresCacheLoadMessageEvent;

    /// <summary>
    /// Call this once to initialize the cache.
    /// </summary>
    void Initialize();
}

/// <summary>
/// Extension methods for <see cref="IDataStoreItemsCache{TNonCopyDataStoreItem, TDataStoreItemWrapper}"/>
/// </summary>
public static class DataStoresCacheExtensions
{
    /// <summary>
    /// Tries to retrieve a data store item wrapper in data store with Id=<paramref name="dataStoreId"/> with data store item Id equal to <paramref name="dataStoreItemId"/>.
    /// </summary>
    public static bool TryGetDataStoreItem<TNonCopyDataStoreItem, TDataStoreItemWrapper>(this IDataStoresCache<TNonCopyDataStoreItem, TDataStoreItemWrapper> dataStoresCache, long dataStoreId, long dataStoreItemId,
        out DataStoreItemWrapper<TNonCopyDataStoreItem, TDataStoreItemWrapper>? dataStoreItemWrapper)
        where TNonCopyDataStoreItem : class, INonCopyDataStoreItem
        where TDataStoreItemWrapper : DataStoreItemWrapper<TNonCopyDataStoreItem, TDataStoreItemWrapper>
    {
        if (dataStoresCache.TryGetDataStore(dataStoreId, out var dataStoreData) &&
            dataStoreData!.TryGetDataStoreItem(dataStoreItemId, out var dataStoreItemWrapperLocal))
        {
            dataStoreItemWrapper = dataStoreItemWrapperLocal;
            return true;
        }

        dataStoreItemWrapper = null;
        return false;
    }
}

/// <inheritdoc />
public class DataStoresCache<TDataStoreItem, TNonCopyDataStoreItem, TDataStoreItemWrapper> : IDataStoresCache<TNonCopyDataStoreItem, TDataStoreItemWrapper>
    where TNonCopyDataStoreItem : class, TDataStoreItem, INonCopyDataStoreItem, IDataStoreItemCanBeCopiedWithNullParent<TNonCopyDataStoreItem>
    where TDataStoreItem : class, IDataStoreItem
    where TDataStoreItemWrapper : DataStoreItemWrapper<TNonCopyDataStoreItem, TDataStoreItemWrapper> 

{
    private readonly IReadOnlyList<IDataStore<TDataStoreItem>> _dataStores;
    private readonly IDataStoreItemWrapperFactory<TNonCopyDataStoreItem, TDataStoreItemWrapper> _dataStoreItemWrapperFactory;

    private readonly Dictionary<long, IDataStoreItemsCache<TNonCopyDataStoreItem, TDataStoreItemWrapper>> _dataStoreIdToDataStoreItemsCache = new();
    private readonly List<IDataStoreItemsCache<TNonCopyDataStoreItem, TDataStoreItemWrapper>> _dataStoreItemsCacheList = new();
    private readonly List<List<PathComponentEdge>> _loggedCircularReferencePaths = new();

    /// <summary>
    /// Constructor.
    /// </summary>
    /// <param name="dataStores">Data stores to load.</param>
    /// <param name="dataStoreItemWrapperFactory">A factory for creating instances of <typeparamref name="TDataStoreItemWrapper"/> of type <see cref="DataStoreItemWrapper{TNonCopyDataStoreItem}"/>.
    /// </param>
    public DataStoresCache(IReadOnlyList<IDataStore<TDataStoreItem>> dataStores,
        IDataStoreItemWrapperFactory<TNonCopyDataStoreItem, TDataStoreItemWrapper> dataStoreItemWrapperFactory)
    {
        _dataStores = dataStores;
        _dataStoreItemWrapperFactory = dataStoreItemWrapperFactory;
    }

    /// <inheritdoc />
    public void Initialize()
    {
        var dataStoreIdToDataStoreData = new Dictionary<long, DataStoreDataInProgress>();
        var dataStoreItemsCacheList = new List<DataStoreItemsCache>();

        #region Original initiaization

        foreach (var dataStore in _dataStores)
        {
            if (_dataStoreIdToDataStoreItemsCache.ContainsKey(dataStore.DataStoreId))
            {
                LogMessage(new LoggedMessage(MessageType.DataStoreAppearsMultipleTimes, $"Data store with Id={dataStore.DataStoreId} appears multiple times.",
                    dataStore.DataStoreId, null, MessageCategory.Error));
                continue;
            }

            var dataStoreItemsCache = new DataStoreItemsCache(dataStore.DataStoreId);

            _dataStoreIdToDataStoreItemsCache[dataStore.DataStoreId] = dataStoreItemsCache;
            _dataStoreItemsCacheList.Add(dataStoreItemsCache);
            dataStoreItemsCacheList.Add(dataStoreItemsCache);

            var dataStoreData = new DataStoreDataInProgress(dataStore);
            dataStoreIdToDataStoreData[dataStoreData.DataStore.DataStoreId] = dataStoreData;

            foreach (var dataStoreItem in dataStore.Items)
            {
                if (dataStoreItem is ICanHaveParent canHaveParent && canHaveParent.ParentId == dataStoreItem.Id)
                {
                    LogMessage(new LoggedMessage(MessageType.ParentIdReferencesItself,
                        $"[{dataStoreItem.GetDisplayValue(dataStoreData.DataStore.DataStoreId, true)}] will be removed since it references itself via {nameof(ICanHaveParent.ParentId)} property.",
                        dataStoreData.DataStore.DataStoreId, dataStoreItem, MessageCategory.Error));
                    continue;
                }

                if (dataStoreItem is ICopyDataStoreItem copyDataStoreItem &&
                    copyDataStoreItem.ReferencedDataStoreItemId == copyDataStoreItem.Id && copyDataStoreItem.ReferencedDataStoreId == dataStoreData.DataStore.DataStoreId)
                {
                    LogMessage(new LoggedMessage(MessageType.CopyDataStoreItemReferencesItself,
                        $"[{copyDataStoreItem.GetDisplayValue(dataStoreData.DataStore.DataStoreId, true)}] references itself.",
                        dataStoreData.DataStore.DataStoreId, dataStoreItem, MessageCategory.Error));
                    continue;
                }

                if (dataStoreData.DataStoreItemIdToDataStoreItem.ContainsKey(dataStoreItem.Id))
                {
                    LogMessage(new LoggedMessage(MessageType.DataStoreItemAppearsMultipleTimesInSameDataStore,
                        $"[{dataStoreItem.GetDisplayValue(dataStore.DataStoreId, true)}] uses an Id that was already used in this data store.",
                        dataStore.DataStoreId, dataStoreItem, MessageCategory.Error));

                    continue;
                }

                dataStoreItem.Initialize();

                dataStoreData.DataStoreItemIdToDataStoreItem[dataStoreItem.Id] = new DataStoreItemData(dataStoreItem);
                dataStoreData.AllDataStoreItems.Add(dataStoreItem);
            }
        }

        #endregion

        #region Initialize child/parent relationships.

        foreach (var dataStoreData in dataStoreIdToDataStoreData.Values)
        {
            foreach (var dataStoreItemData in dataStoreData.DataStoreItemIdToDataStoreItem.Values)
            {
                var dataStoreItem = dataStoreItemData.DataStoreItem;

                if (dataStoreItem is ICanHaveParent { ParentId: { } } canHaveParent)
                {
                    if (!dataStoreData.DataStoreItemIdToChildren.TryGetValue(canHaveParent.ParentId.Value, out var childDataStoreItems))
                    {
                        childDataStoreItems = new List<TDataStoreItem>();
                        dataStoreData.DataStoreItemIdToChildren[canHaveParent.ParentId.Value] = childDataStoreItems;
                    }

                    childDataStoreItems.Add(dataStoreItem);
                }
            }
        }

        #endregion

        #region Create data store item wrappers

        foreach (var dataStoreData in dataStoreIdToDataStoreData.Values)
        {
            foreach (var dataStoreItem in dataStoreData.AllDataStoreItems)
            {
                var currentDataStoreItemsPath = new List<PathComponentEdgeLocal>();
                AddPathComponentEdge(currentDataStoreItemsPath, DataStoreItemsRelationship.Child,
                    dataStoreItem, dataStoreData.DataStore.DataStoreId);

                // All data store items we create here have null parents. 
                // The same data item wrapper in data store might be created multiple times, if its is 
                // added as a child to many parents (if the parent Id appears multiple times, which can happen if we copy some data item with children
                // multiple times, and use some child id as parent).
                // Therefore, since we might have multiple copies of the same data store item id (because of ICanHaveParent.ParentId matching multiple parents)
                // we do not want to add all those copies to the dictionary DataStoreDataInProgress.DataStoreItemIdToDataStoreItemWrapper.
                // Instead, we make another copy here without any parent. DataStoreItemIdToDataStoreItemWrapper is used either for 
                // referencing the data store items from other data stores, or by referencing it via IDataStoreItemsCache.TryGetDataStoreItem(),
                // and in both these cases we are not interested in the parent (whe copying, the parent is null-ed out, or changed to a different parent,
                // and when retrieving the data store item IDataStoreItemsCache.TryGetDataStoreItem(), parent does not matter (otherwise, we would just
                // retrieve the parent of the data store item).
                var dataStoreItemWrapper = CreateDataStoreItemWrapperInProgress(dataStoreItem, dataStoreData, currentDataStoreItemsPath, dataStoreIdToDataStoreData);

                if (dataStoreItemWrapper != null)
                    dataStoreData.DataStoreItemIdToDataStoreItemWrapper[dataStoreItem.Id] = dataStoreItemWrapper;
            }
        }

        #endregion

        #region Sort top level data wrappers and convert to TDataStoreItemWrapper

        foreach (var dataStoreItemsCache in dataStoreItemsCacheList)
        {
            var dataStoreDataSrc = dataStoreIdToDataStoreData[dataStoreItemsCache.DataStoreId];

            foreach (var dataStoreItemData in dataStoreDataSrc.DataStoreItemIdToDataStoreItem.Values)
            {
                var dataStoreItem = dataStoreItemData.DataStoreItem;

                if (dataStoreItem is ICanHaveParent canHaveParent && canHaveParent.ParentId != null &&
                    !dataStoreItemData.WasAddedToSomeParent &&
                    !(dataStoreDataSrc.GetPerformedExclusionCheckResult(dataStoreItem.Id, DataStoreItemCheckType.CopiedDataStoreItemDoesNotImplementInterface) == true ||
                      dataStoreDataSrc.GetPerformedExclusionCheckResult(dataStoreItem.Id, DataStoreItemCheckType.InvalidCopiedDataStore) == true ||
                      dataStoreDataSrc.GetPerformedExclusionCheckResult(dataStoreItem.Id, DataStoreItemCheckType.InvalidCopiedDataStoreItem) == true ||
                      dataStoreDataSrc.GetPerformedExclusionCheckResult(dataStoreItem.Id, DataStoreItemCheckType.FailedToCopyDataStoreItem) == true))
                {
                    LogMessage(new LoggedMessage(MessageType.InvalidDataStoreItemParent,
                        $"[{dataStoreItem.GetDisplayValue(dataStoreDataSrc.DataStore.DataStoreId, true)}] references an invalid or missing '{nameof(ICanHaveParent.ParentId)}', or the parent had errors.",
                        dataStoreDataSrc.DataStore.DataStoreId, dataStoreItem, MessageCategory.Error));
                }
            }

            var topLevelDataStoreItemWrappers = new List<TDataStoreItemWrapper>(dataStoreDataSrc.AllDataStoreItems.Count);

            foreach (var dataStoreItem in dataStoreDataSrc.AllDataStoreItems)
            {
                if (!dataStoreDataSrc.DataStoreItemIdToDataStoreItemWrapper.TryGetValue(dataStoreItem.Id, out var dataStoreItemWrapperInProgress))
                {
                    // Already logged.
                    continue;
                }

                var isTopLevelDataStoreItem = IsTopLevelDataStoreItem(dataStoreItemWrapperInProgress.DataStoreItem);

                if (!isTopLevelDataStoreItem)
                {
                    var dataStoreItemWrapperWithNullParent = new DataStoreItemWrapperInProgress(
                        dataStoreItemWrapperInProgress.DataStoreItem.CloneWithNullParent(dataStoreItemWrapperInProgress.DataStoreItem.Id), dataStoreItemWrapperInProgress.DataStoreId);

                    foreach (var child in dataStoreItemWrapperInProgress.Children)
                    {
                        dataStoreItemWrapperWithNullParent.Children.Add(child);
                        child.Parent = dataStoreItemWrapperInProgress;

                        if (dataStoreDataSrc.DataStoreItemIdToDataStoreItem.TryGetValue(child.DataStoreItem.Id, out var childDataStoreItemData))
                            childDataStoreItemData.WasAddedToSomeParent = true;
                    }

                    dataStoreItemWrapperInProgress = dataStoreItemWrapperWithNullParent;
                }
                //else
                //{
                //    // We do not do validations for top level items, since any item can be used as top level item in some shared data store just to be able to 
                //    // copy the data store item to other data store and use it as a child data store item.
                //    // Therefore doing a check IsValidTopLevelDataStoreItem(dataStoreItemWrapperInProgress.DataStoreItem) might be confusing.
                //    // To prevent data store items, do a validation in ExtendibleTreeStructure.IDataStoreItemWrapperFactory.Create().
                //    if (!IsValidTopLevelDataStoreItem(dataStoreItemWrapperInProgress.DataStoreItem))
                //    {
                //        LogMessage(new LoggedMessage(
                //            MessageType.InvalidChildDataStoreItem,
                //            String.Format("[{0}] cannot be used as top level data store.",
                //                dataStoreItem.GetDisplayValue(dataStoreDataSrc.DataStore.DataStoreId, true)),
                //            dataStoreDataSrc.DataStore.DataStoreId, dataStoreItem, MessageCategory.Error));
                //        continue;
                //    }
                //}


                var createdDataStoreItemWrapper = CreateDataStoreItemWrapper(dataStoreItemWrapperInProgress, null, dataStoreItemsCache, dataStoreDataSrc);

                if (createdDataStoreItemWrapper != null)
                {
                    if (isTopLevelDataStoreItem)
                        topLevelDataStoreItemWrappers.Add(createdDataStoreItemWrapper);

                    dataStoreItemsCache.AddDataStoreItemWrapper(createdDataStoreItemWrapper);
                }
            }

            topLevelDataStoreItemWrappers.Sort((x, y) =>
            {
                if (x == y || x.DataStoreItem.Priority == y.DataStoreItem.Priority)
                    return 0;

                if (x.DataStoreItem.Priority == null)
                    return y.DataStoreItem.Priority == null ? -1 : 1;

                // If we got here, x.DataStoreItem.Priority is not null
                if (y.DataStoreItem.Priority == null)
                    return -1;

                return x.DataStoreItem.Priority.Value <= y.DataStoreItem.Priority.Value ? -1 : 1;
            });

            foreach (var topLevelDataStoreItemWrapper in topLevelDataStoreItemWrappers)
                dataStoreItemsCache.AddTopLevelDataStoreItemWrapper(topLevelDataStoreItemWrapper);
        }

        #endregion

        CleanupOnTreeOnCacheLoaded();
    }

    /// <summary>
    /// Override this method to cleanup any state data used during loading of the cache, that is not needed after the cache is loaded.
    /// </summary>
    protected virtual void CleanupOnTreeOnCacheLoaded()
    {
        _loggedCircularReferencePaths.Clear();
    }

    private TDataStoreItemWrapper? CreateDataStoreItemWrapper(DataStoreItemWrapperInProgress dataStoreItemWrapperInProgress,
        TDataStoreItemWrapper? parentDataStoreItemWrapper,
        DataStoreItemsCache dataStoreItemsCache,
        DataStoreDataInProgress dataStoreData)
    {
        var dataStoreItem = dataStoreItemWrapperInProgress.DataStoreItem;
        var childDataStoreItemWrappers = new List<TDataStoreItemWrapper>();

        var createResult = _dataStoreItemWrapperFactory.Create(dataStoreData.DataStore.DataStoreId, dataStoreItem, parentDataStoreItemWrapper);

        if (createResult.CreatedDataStoreItemWrapper == null)
        {
            this.LogMessage(createResult.MessageToLog ?? new LoggedMessage(
                MessageType.DataStoreItemIsIgnored,
                $"[{dataStoreItem.GetDisplayValue(dataStoreItemsCache.DataStoreId, true)}] will be ignored.",
                dataStoreItemsCache.DataStoreId, dataStoreItem, MessageCategory.Info));
            return null;
        }

        if (createResult.MessageToLog != null)
            LogMessage(createResult.MessageToLog);

        var createdDataStoreItemWrapper = createResult.CreatedDataStoreItemWrapper;

        if (dataStoreItemWrapperInProgress.Children.Count > 0)
        {
            foreach (var childDataStoreItemWrapperInProgress in dataStoreItemWrapperInProgress.Children)
            {
                if (!CheckIsValidChild(dataStoreData, childDataStoreItemWrapperInProgress.DataStoreItem,
                        dataStoreItemWrapperInProgress.DataStoreItem))
                    continue;

                var childDataStoreItemWrapper = CreateDataStoreItemWrapper(childDataStoreItemWrapperInProgress, createdDataStoreItemWrapper,
                    dataStoreItemsCache, dataStoreData);

                if (childDataStoreItemWrapper == null)
                    continue;

                childDataStoreItemWrappers.Add(childDataStoreItemWrapper);
            }

            childDataStoreItemWrappers.Sort((x, y) =>
            {
                if (x == y || x.DataStoreItem.Priority == y.DataStoreItem.Priority)
                    return 0;

                if (x.DataStoreItem.Priority == null)
                    return y.DataStoreItem.Priority == null ? -1 : 1;

                // If we got here, x.DataStoreItem.Priority is not null
                if (y.DataStoreItem.Priority == null)
                    return -1;

                return x.DataStoreItem.Priority.Value <= y.DataStoreItem.Priority.Value ? -1 : 1;
            });
        }

        createResult.CreatedDataStoreItemWrapper.SetChildren(childDataStoreItemWrappers);

        return createdDataStoreItemWrapper;
    }

    private bool IsTopLevelDataStoreItem(TDataStoreItem dataStoreItem)
    {
        return dataStoreItem is not ICanHaveParent canHaveParent || canHaveParent.ParentId == null;
    }

    private bool CheckIsValidChild(DataStoreDataInProgress dataStoreData, TNonCopyDataStoreItem childDataStoreItem, TNonCopyDataStoreItem parentDataStoreItem)
    {
        if (!IsValidChildDataStoreItem(childDataStoreItem, parentDataStoreItem))
        {
            if (dataStoreData.DataStoreItemIdToDataStoreItem.ContainsKey(childDataStoreItem.Id))
            {
                // If DataStoreItemIdToDataStoreItem does not contain childDataStoreItem.Id,
                // then child data store item was copied from some other data store and the error will be reported in that data store.
                var dataStoreId = dataStoreData.DataStore.DataStoreId;
                LogMessage(new LoggedMessage(
                    MessageType.InvalidChildDataStoreItem,
                    String.Format("[{0}] cannot be used as a child for [{1}].",
                        childDataStoreItem.GetDisplayValue(dataStoreId, true),
                        parentDataStoreItem.GetDisplayValue(dataStoreId, false)),
                    dataStoreId, childDataStoreItem, MessageCategory.Error));
            }

            return false;
        }

        return true;
    }

    ///// <summary>
    ///// If the returned value is false, data store item will not be added to cache and an error log message <see cref="ILoggedMessage"/> will be logged.
    ///// </summary>
    ///// <param name="dataStoreItem">Data store item to check.</param>
    //protected virtual bool IsValidTopLevelDataStoreItem(TNonCopyDataStoreItem dataStoreItem)
    //{
    //    return true;
    //}

    /// <summary>
    /// If the returned value is false, data store item <paramref name="childDataStoreItem"/> will not be added to cache. 
    /// </summary>
    /// <param name="childDataStoreItem">Child data store item wrapper to check.</param>
    /// <param name="parentDataStoreItem">Parent data store item Id to check.</param>
    protected virtual bool IsValidChildDataStoreItem(TNonCopyDataStoreItem childDataStoreItem, TNonCopyDataStoreItem parentDataStoreItem)
    {
        return true;
    }

    private DataStoreItemWrapperInProgress? CopyDataStoreItem(DataStoreDataInProgress dataStoreData,
        ICopyDataStoreItem copyDataStoreItem,
        List<PathComponentEdgeLocal> currentDataStoreItemsPath,
        Dictionary<long, DataStoreDataInProgress> dataStoreIdToDataStoreData)
    {
        DataStoreDataInProgress referencedDataStoreData;

        if (dataStoreData.DataStore.DataStoreId == copyDataStoreItem.ReferencedDataStoreId)
        {
            referencedDataStoreData = dataStoreData;
        }
        else
        {
            if (dataStoreData.GetOrSetPerformedExclusionCheckResult(copyDataStoreItem.Id,
                    DataStoreItemCheckType.InvalidCopiedDataStore, () =>
                    {
                        if (!dataStoreIdToDataStoreData.ContainsKey(copyDataStoreItem.ReferencedDataStoreId))
                        {
                            this.LogMessage(new LoggedMessage(MessageType.InvalidCopiedDataStoreItem,
                                String.Format("Data store with Id={0} referenced by [{1}] not found.",
                                    copyDataStoreItem.ReferencedDataStoreId,
                                    copyDataStoreItem.GetDisplayValue(dataStoreData.DataStore.DataStoreId, false)),
                                dataStoreData.DataStore.DataStoreId, copyDataStoreItem, MessageCategory.Error));
                            return true;
                        }

                        return false;
                    }))
                return null;

            referencedDataStoreData = dataStoreIdToDataStoreData[copyDataStoreItem.ReferencedDataStoreId];
        }

        if (dataStoreData.GetOrSetPerformedExclusionCheckResult(copyDataStoreItem.Id, DataStoreItemCheckType.InvalidCopiedDataStoreItem,
                () =>
                {
                    if (!referencedDataStoreData.DataStoreItemIdToDataStoreItem.ContainsKey(copyDataStoreItem.ReferencedDataStoreItemId))
                    {
                        this.LogMessage(new LoggedMessage(MessageType.InvalidCopiedDataStoreItem,
                            String.Format("Data store item with Id={0} referenced by [{1}] not found in data store with Id={2}.",
                                copyDataStoreItem.ReferencedDataStoreItemId,
                                copyDataStoreItem.GetDisplayValue(dataStoreData.DataStore.DataStoreId, false),
                                copyDataStoreItem.ReferencedDataStoreId),
                            dataStoreData.DataStore.DataStoreId, copyDataStoreItem, MessageCategory.Error));
                        return true;
                    }

                    return false;
                }))
            return null;

        var dataStoreItemToCopy = referencedDataStoreData.DataStoreItemIdToDataStoreItem[copyDataStoreItem.ReferencedDataStoreItemId].DataStoreItem;

        DataStoreItemWrapperInProgress? copiedDataStoreItemWrapper = null;
        try
        {
            if (CheckForCircularReferences(currentDataStoreItemsPath, DataStoreItemsRelationship.Copied,
                    dataStoreItemToCopy, referencedDataStoreData))
                return null;

            var addedComponentEdge = AddPathComponentEdge(currentDataStoreItemsPath, DataStoreItemsRelationship.Copied,
                dataStoreItemToCopy, referencedDataStoreData.DataStore.DataStoreId);

            copiedDataStoreItemWrapper = CreateDataStoreItemWrapperInProgress(dataStoreItemToCopy, referencedDataStoreData, currentDataStoreItemsPath, dataStoreIdToDataStoreData);

            RemovePathComponentEdge(currentDataStoreItemsPath, addedComponentEdge);

            if (copiedDataStoreItemWrapper == null)
                return null;

            TNonCopyDataStoreItem copiedDataStoreItem;

            if (copyDataStoreItem.ParentId != null)
            {
                if (dataStoreData.GetOrSetPerformedExclusionCheckResult(copiedDataStoreItemWrapper.DataStoreItem.Id, DataStoreItemCheckType.CopiedDataStoreItemDoesNotImplementInterface,
                        () =>
                        {
                            if (copiedDataStoreItemWrapper.DataStoreItem is not (ICanHaveParent and IDataStoreItemCanBeCopiedWithNewParent<TNonCopyDataStoreItem>))
                            {
                                LogMessage(new LoggedMessage(MessageType.CopiedDataStoreItemDoesNotImplementInterface,
                                    String.Format("Cannot copy [{0}] with new parent Id={1} since the data store item does not implement interface '{2}'. Copied data store item type is '{3}'.",
                                        copiedDataStoreItemWrapper.DataStoreItem.GetDisplayValue(copiedDataStoreItemWrapper.DataStoreId, false),
                                        copyDataStoreItem.ParentId,
                                        typeof(IDataStoreItemCanBeCopiedWithNewParent<TNonCopyDataStoreItem>), copiedDataStoreItemWrapper.DataStoreItem.GetType()),
                                    dataStoreData.DataStore.DataStoreId, copyDataStoreItem, MessageCategory.Error));
                                return true;
                            }

                            return false;
                        }))
                    return null;

                copiedDataStoreItem = ((IDataStoreItemCanBeCopiedWithNewParent<TNonCopyDataStoreItem>)copiedDataStoreItemWrapper.DataStoreItem).CloneWithNewParent(
                    copyDataStoreItem.Id, copyDataStoreItem.ParentId.Value);
            }
            else
            {
                copiedDataStoreItem = copiedDataStoreItemWrapper.DataStoreItem.CloneWithNullParent(copyDataStoreItem.Id);
            }

            copiedDataStoreItemWrapper.OnCopiedToNewDataStore(copiedDataStoreItem, dataStoreData.DataStore.DataStoreId);

            if (copyDataStoreItem.Priority != null)
                copiedDataStoreItemWrapper.DataStoreItem.Priority = copyDataStoreItem.Priority;

            ConfigureCopiedDataStoreItemWrapperInNewDataStore(copiedDataStoreItemWrapper, dataStoreData,
                currentDataStoreItemsPath, dataStoreIdToDataStoreData);

        }
        finally
        {
            if (copiedDataStoreItemWrapper == null)
            {
                if (dataStoreData.GetPerformedExclusionCheckResult(copyDataStoreItem.Id, DataStoreItemCheckType.FailedToCopyDataStoreItem) != true)
                {
                    dataStoreData.OnExclusionCheckPerformed(copyDataStoreItem.Id, DataStoreItemCheckType.FailedToCopyDataStoreItem, true);

                    this.LogMessage(new LoggedMessage(MessageType.FailedToCopyDataStoreItem,
                        String.Format("Failed to copy data store item referenced by [{0}]. This might be a result of other errors logged earlier.",
                            copyDataStoreItem.GetDisplayValue(dataStoreData.DataStore.DataStoreId, false)),
                        dataStoreData.DataStore.DataStoreId, copyDataStoreItem, MessageCategory.Error));
                }
            }
        }

        return copiedDataStoreItemWrapper;
    }

    private void ConfigureCopiedDataStoreItemWrapperInNewDataStore(DataStoreItemWrapperInProgress dataStoreItemWrapper,
        DataStoreDataInProgress dataStoreDataWhereCopied,
        List<PathComponentEdgeLocal> currentDataStoreItemsPath,
        Dictionary<long, DataStoreDataInProgress> dataStoreIdToDataStoreData)
    {
        // After child data store items in referenced data store are copied, lets add the children in new data store
        AddChildrenInCurrentDataStore(dataStoreItemWrapper, dataStoreDataWhereCopied, currentDataStoreItemsPath, dataStoreIdToDataStoreData);

        foreach (var childDataStoreItemWrapper in dataStoreItemWrapper.Children)
        {
            var childDataStoreItem = childDataStoreItemWrapper.DataStoreItem;
            TNonCopyDataStoreItem modifiedChildDataStoreItem = childDataStoreItem;

            if (childDataStoreItem is ICanHaveParent canHaveParent)
            {
                // This might happen if ICopyDataStoreItem Id is different from the copied data store item (see the test case 
                // ExtendibleTreeStructure.Tests.SuccessfulCacheBuildTests.SuccessfulCacheBuildTests.CopyDataStoreItemMultipleTimesInDataStoreTest())
                if (canHaveParent.ParentId != dataStoreItemWrapper.DataStoreItem.Id)
                {
                    if (childDataStoreItem is IDataStoreItemCanBeCopiedWithNewParent<TNonCopyDataStoreItem> cloneableWithNewParent)
                    {
                        modifiedChildDataStoreItem = cloneableWithNewParent.CloneWithNewParent(childDataStoreItem.Id, dataStoreItemWrapper.DataStoreItem.Id);
                    }
                    else
                    {
                        LogMessage(new LoggedMessage(MessageType.CopiedDataStoreItemDoesNotImplementInterface,
                            String.Format("[{0}] copied under [{1}] does not implement interface '{2}'. Copied data store item type is '{3}'",
                                childDataStoreItem.GetDisplayValue(dataStoreDataWhereCopied.DataStore.DataStoreId, false),
                                dataStoreItemWrapper.DataStoreItem.GetDisplayValue(dataStoreDataWhereCopied.DataStore.DataStoreId, false),
                                typeof(IDataStoreItemCanBeCopiedWithNewParent<TNonCopyDataStoreItem>), childDataStoreItem.GetType()),
                            dataStoreDataWhereCopied.DataStore.DataStoreId, childDataStoreItem, MessageCategory.Error));
                    }
                }
            }
            else
            {
                LogMessage(new LoggedMessage(MessageType.ImplementationError,
                    String.Format("Child data store item wrapper item in [{0}] of [{1}] does not implement interface '{2}'.",
                        childDataStoreItem.GetDisplayValue(dataStoreDataWhereCopied.DataStore.DataStoreId, false),
                        dataStoreItemWrapper.DataStoreItem.GetDisplayValue(dataStoreDataWhereCopied.DataStore.DataStoreId, false),
                        typeof(ICanHaveParent)),
                    dataStoreDataWhereCopied.DataStore.DataStoreId, childDataStoreItem, MessageCategory.Error));
            }

            childDataStoreItemWrapper.OnCopiedToNewDataStore(modifiedChildDataStoreItem, dataStoreDataWhereCopied.DataStore.DataStoreId);

            ConfigureCopiedDataStoreItemWrapperInNewDataStore(childDataStoreItemWrapper,
                dataStoreDataWhereCopied,
                currentDataStoreItemsPath, dataStoreIdToDataStoreData);
        }
    }

    private DataStoreItemWrapperInProgress? CreateDataStoreItemWrapperInProgress(TDataStoreItem dataStoreItem,
        DataStoreDataInProgress dataStoreDataWhereToCreate,
        List<PathComponentEdgeLocal> currentDataStoreItemsPath,
        Dictionary<long, DataStoreDataInProgress> dataStoreIdToDataStoreData)
    {
        DataStoreItemWrapperInProgress dataStoreItemWrapper;

        if (dataStoreItem is ICopyDataStoreItem copyDataStoreItem)
            return CopyDataStoreItem(dataStoreDataWhereToCreate, copyDataStoreItem, currentDataStoreItemsPath, dataStoreIdToDataStoreData);

        if (dataStoreItem is TNonCopyDataStoreItem nonCopyDataStoreItem)
        {
            dataStoreItemWrapper = new DataStoreItemWrapperInProgress(nonCopyDataStoreItem, dataStoreDataWhereToCreate.DataStore.DataStoreId);
        }
        else
        {
            LogMessage(new LoggedMessage(MessageType.InvalidDataStoreItemType,
                String.Format("[{0}] is of type '{1}' which does not implement either '{2}' or '{3}'.",
                    dataStoreItem.GetDisplayValue(dataStoreDataWhereToCreate.DataStore.DataStoreId, true),
                    dataStoreItem.GetType(), typeof(ICopyDataStoreItem), typeof(TNonCopyDataStoreItem)),
                dataStoreDataWhereToCreate.DataStore.DataStoreId, dataStoreItem, MessageCategory.Error));
            return null;
        }

        AddChildrenInCurrentDataStore(dataStoreItemWrapper, dataStoreDataWhereToCreate, currentDataStoreItemsPath, dataStoreIdToDataStoreData);
        return dataStoreItemWrapper;
    }

    private bool CheckForCircularReferences(List<PathComponentEdgeLocal> currentDataStoreItemsPath,
        DataStoreItemsRelationship newItemRelationshipWithLastItemInPath,
        TDataStoreItem addedDataStoreItem, DataStoreDataInProgress dataStoreData)
    {
        if (currentDataStoreItemsPath.Count <= 1)
        {
            return false;
        }

        long dataStoreId = dataStoreData.DataStore.DataStoreId;

        for (var i = currentDataStoreItemsPath.Count - 1; i >= 1; --i)
        {
            var currentEdge = currentDataStoreItemsPath[i];

            var startingNode = currentEdge.StartingNode;

            if (startingNode == null)
            {
                LogMessage(new LoggedMessage(MessageType.ImplementationError, $"Value of '{nameof(startingNode)}' can be null only for the first item in {nameof(currentDataStoreItemsPath)}. This is a bug in this class.",
                    dataStoreId, addedDataStoreItem, MessageCategory.Error));
                return false;
            }

            if (startingNode.DataStoreItem.Id == addedDataStoreItem.Id && startingNode.DataStoreId == dataStoreId)
            {
                List<PathComponentEdge> circularReferencesPath = new();

                for (var nodeInd = i; nodeInd < currentDataStoreItemsPath.Count; ++nodeInd)
                {
                    PathComponentEdgeLocal cyclingItemsPathEdge = currentDataStoreItemsPath[nodeInd];

                    circularReferencesPath.Add(new PathComponentEdge(cyclingItemsPathEdge.StartingNode!, cyclingItemsPathEdge.EndingNode,
                        cyclingItemsPathEdge.StartingEndingNodesRelationship));
                }

                circularReferencesPath.Add(new PathComponentEdge(currentDataStoreItemsPath[^1].EndingNode,
                    new PathComponentNode(addedDataStoreItem, dataStoreId), newItemRelationshipWithLastItemInPath));

                foreach (var loggedCircularReferencePath in _loggedCircularReferencePaths)
                {
                    if (loggedCircularReferencePath.Count == circularReferencesPath.Count)
                    {
                        // Example of positive match in loggedCircularReferencePath and circularReferencesPath
                        // Example  of path in loggedCircularReferencePath : x1->x2->x3->x4
                        // Example of path in circularReferencesPath : x3->x4->x1->x2

                        // Lets try to match the new path circularReferencesPath with loggedCircularReferencePath, by starting comparison
                        // of edges from index 0 (x1 in loggedCircularReferencePath with x3 in circularReferencesPath),
                        // then at index 1 (x1 in loggedCircularReferencePath with x4 in circularReferencesPath)
                        // the at index 2 (x1 in loggedCircularReferencePath with x1 in circularReferencesPath)
                        // until we find a match, or try all circularReferencesPath.Count possibilities, and do not find any match)
                        for (var startIndexInNewPath = 0; startIndexInNewPath < circularReferencesPath.Count; ++startIndexInNewPath)
                        {
                            var indexInNewPath = startIndexInNewPath;

                            var circularPathWasLogged = true;

                            for (var edgeInd = 0; edgeInd < loggedCircularReferencePath.Count; ++edgeInd)
                            {
                                if (!loggedCircularReferencePath[edgeInd].Equals(circularReferencesPath[indexInNewPath]))
                                {
                                    circularPathWasLogged = false;
                                    break;
                                }

                                if (indexInNewPath == circularReferencesPath.Count - 1)
                                    indexInNewPath = 0;
                                else
                                    ++indexInNewPath;
                            }

                            if (circularPathWasLogged)
                                return true;
                        }
                    }
                }

                _loggedCircularReferencePaths.Add(circularReferencesPath);

                var errorMessage = new StringBuilder();
                errorMessage.AppendLine($"{addedDataStoreItem.GetDisplayValue(dataStoreId, true)} results in the following circular references:");

                foreach (var edge in circularReferencesPath)
                {
                    string relationshipDescription;

                    switch (edge.StartingEndingNodesRelationship)
                    {
                        case DataStoreItemsRelationship.Child:
                            relationshipDescription = "going to child";
                            break;
                        case DataStoreItemsRelationship.Copied:
                            relationshipDescription = "going to copied item";
                            break;
                        default:
                            LogMessage(new LoggedMessage(MessageType.ImplementationError,
                                $"Invalid value '{edge.StartingEndingNodesRelationship}' in {typeof(DataStoreItemsRelationship)}.",
                                dataStoreId, addedDataStoreItem, MessageCategory.Error));
                            relationshipDescription = "references";
                            break;
                    }

                    errorMessage.AppendLine(string.Format("\t\t[{0}] {1} [{2}]",
                        edge.StartingNode.DataStoreItem.GetDisplayValue(edge.StartingNode.DataStoreId, false),
                        relationshipDescription,
                        edge.EndingNode.DataStoreItem.GetDisplayValue(edge.EndingNode.DataStoreId, false)));
                }

                LogMessage(new CircularReferencesErrorMessage(errorMessage.ToString(), circularReferencesPath,
                    dataStoreId, addedDataStoreItem));

                return true;
            }
        }

        return false;
    }

    private PathComponentEdgeLocal AddPathComponentEdge(List<PathComponentEdgeLocal> currentDataStoreItemsPath,
        DataStoreItemsRelationship newItemRelationshipWithLastItemInPath,
        TDataStoreItem addedDataStoreItem, long addedDataStoreId)
    {
        var startingNode = currentDataStoreItemsPath.Count == 0 ? null : currentDataStoreItemsPath[^1].EndingNode;
        var addedPathComponentEdge = new PathComponentEdgeLocal(startingNode,
            new PathComponentNode(addedDataStoreItem, addedDataStoreId), newItemRelationshipWithLastItemInPath);

        currentDataStoreItemsPath.Add(addedPathComponentEdge);
        return addedPathComponentEdge;
    }

    private void RemovePathComponentEdge(List<PathComponentEdgeLocal> currentDataStoreItemsPath, PathComponentEdgeLocal removedComponentEdge)
    {
        if (currentDataStoreItemsPath.Count == 0 || !currentDataStoreItemsPath[^1].Equals(removedComponentEdge))
        {
            LogMessage(new LoggedMessage(MessageType.ImplementationError, "Invalid state. This is indicative of a bug in this class.",
                removedComponentEdge.EndingNode.DataStoreId, null, MessageCategory.Error));
            return;
        }

        currentDataStoreItemsPath.RemoveAt(currentDataStoreItemsPath.Count - 1);
    }

    private void AddChildrenInCurrentDataStore(DataStoreItemWrapperInProgress dataStoreItemWrapper, DataStoreDataInProgress dataStoreData,
        List<PathComponentEdgeLocal> currentDataStoreItemsPath,
        Dictionary<long, DataStoreDataInProgress> dataStoreIdToDataStoreData)
    {
        // We might get here twice if two data stores reference each other (see the test
        // ExtendibleTreeStructure.Tests.SuccessfulCacheBuildTests.DataStoresReferencingEachOtherTest).
        // To avoid adding the same child multiple times, lets check if children in data store were added
        // in dataStoreItemWrapper.DataStoresInWhichChildrenWereAdded.
        if (dataStoreData.DataStoreItemIdToChildren.TryGetValue(dataStoreItemWrapper.DataStoreItem.Id, out var childDataStoreItems) &&
            !dataStoreItemWrapper.DataStoresInWhichChildrenWereAdded.Contains(dataStoreData.DataStore.DataStoreId))
        {

            dataStoreItemWrapper.DataStoresInWhichChildrenWereAdded.Add(dataStoreData.DataStore.DataStoreId);

            foreach (var childDataStoreItem in childDataStoreItems)
            {
                if (CheckForCircularReferences(currentDataStoreItemsPath, DataStoreItemsRelationship.Child,
                        childDataStoreItem, dataStoreData))
                    continue;

                var addedComponentEdge = AddPathComponentEdge(currentDataStoreItemsPath, DataStoreItemsRelationship.Child,
                    childDataStoreItem, dataStoreData.DataStore.DataStoreId);


                var childDtaStoreItemWrapper = CreateDataStoreItemWrapperInProgress(childDataStoreItem, dataStoreData, currentDataStoreItemsPath, dataStoreIdToDataStoreData);

                try
                {
                    if (childDtaStoreItemWrapper != null)
                    {
                        dataStoreItemWrapper.Children.Add(childDtaStoreItemWrapper);
                        childDtaStoreItemWrapper.Parent = dataStoreItemWrapper;

                        if (dataStoreData.DataStoreItemIdToDataStoreItem.TryGetValue(childDtaStoreItemWrapper.DataStoreItem.Id, out var childDataStoreItemData))
                            childDataStoreItemData.WasAddedToSomeParent = true;
                    }
                }
                finally
                {
                    RemovePathComponentEdge(currentDataStoreItemsPath, addedComponentEdge);
                }
            }
        }
    }

    /// <inheritdoc />
    public bool TryGetDataStore(long dataStoreId, out IDataStoreItemsCache<TNonCopyDataStoreItem, TDataStoreItemWrapper>? dataStoreItemsCache)
    {
        if (_dataStoreIdToDataStoreItemsCache.TryGetValue(dataStoreId, out var dataStoreItemsCacheLocal))
        {
            dataStoreItemsCache = dataStoreItemsCacheLocal;
            return true;
        }

        dataStoreItemsCache = null;
        return false;
    }

    /// <inheritdoc />
    public IReadOnlyList<IDataStoreItemsCache<TNonCopyDataStoreItem, TDataStoreItemWrapper>> DataStoreItemsCacheList => _dataStoreItemsCacheList;

    /// <inheritdoc />
    public event EventHandler<LoggedMessageEventArgs>? DataStoresCacheLoadMessageEvent;

    private void LogMessage(ILoggedMessage loggedMessage)
    {
        DataStoresCacheLoadMessageEvent?.Invoke(this, new LoggedMessageEventArgs(loggedMessage));
    }

    private class DataStoreItemsCache : IDataStoreItemsCache<TNonCopyDataStoreItem, TDataStoreItemWrapper>
    {
        private readonly List<TDataStoreItemWrapper> _topLevelDataStoreItemWrappers = new();
        private readonly Dictionary<long, TDataStoreItemWrapper> _dataStoreItemIdToDataStoreItemWrapper = new();

        public DataStoreItemsCache(long dataStoreId)
        {
            DataStoreId = dataStoreId;
        }

        public long DataStoreId { get; }

        public bool TryGetDataStoreItem(long dataStoreItemId, out TDataStoreItemWrapper? dataStoreItemWrapper)
        {
            dataStoreItemWrapper = null;
            if (!_dataStoreItemIdToDataStoreItemWrapper.TryGetValue(dataStoreItemId, out var dataStoreItemWrapper2))
                return false;

            dataStoreItemWrapper = dataStoreItemWrapper2;
            return true;
        }

        public IReadOnlyList<TDataStoreItemWrapper> TopLevelDataStoreItemWrappers => _topLevelDataStoreItemWrappers;

        public void AddDataStoreItemWrapper(TDataStoreItemWrapper dataStoreItemWrapper)
        {
            _dataStoreItemIdToDataStoreItemWrapper[dataStoreItemWrapper.DataStoreItem.Id] = dataStoreItemWrapper;
        }

        public void AddTopLevelDataStoreItemWrapper(TDataStoreItemWrapper topLevelDataStoreItemWrapper)
        {
            _topLevelDataStoreItemWrappers.Add(topLevelDataStoreItemWrapper);
        }
    }

    private class PathComponentEdgeLocal
    {
        public PathComponentEdgeLocal(PathComponentNode? startingNode, PathComponentNode endingNode,
            DataStoreItemsRelationship startingEndingNodesRelationship)
        {
            StartingNode = startingNode;
            EndingNode = endingNode;
            StartingEndingNodesRelationship = startingEndingNodesRelationship;
        }

        public PathComponentNode? StartingNode { get; }
        public PathComponentNode EndingNode { get; }

        public DataStoreItemsRelationship StartingEndingNodesRelationship { get; }

        public override bool Equals(object? obj)
        {
            if (obj is not PathComponentEdgeLocal pathComponentEdge)
                return false;

            return (this.StartingNode == null && pathComponentEdge.StartingNode == null ||
                    this.StartingNode != null && StartingNode.Equals(pathComponentEdge.StartingNode)) &&
                   this.EndingNode.Equals(pathComponentEdge.EndingNode) && this.StartingEndingNodesRelationship == pathComponentEdge.StartingEndingNodesRelationship;
        }

        public override int GetHashCode()
        {
            // TODO: Improve this to use other members too in has calculation.
            // For now the code doe not use this class instances as dictionary keys.
            return EndingNode.GetHashCode();
        }
    }

    private class DataStoreDataInProgress
    {
        private readonly Dictionary<long, Dictionary<DataStoreItemCheckType, bool>> _dataStoreItemIdToExclusionCheckResults = new();

        public DataStoreDataInProgress(IDataStore<TDataStoreItem> dataStore)
        {
            DataStore = dataStore;
        }

        public void OnExclusionCheckPerformed(long dataStoreItemId, DataStoreItemCheckType dataStoreItemCheckType, bool excludeDataStoreItem)
        {
            if (!_dataStoreItemIdToExclusionCheckResults.TryGetValue(dataStoreItemId, out var exclusionCheckResults))
            {
                exclusionCheckResults = new Dictionary<DataStoreItemCheckType, bool>();
                _dataStoreItemIdToExclusionCheckResults[dataStoreItemId] = exclusionCheckResults;
            }

            exclusionCheckResults[dataStoreItemCheckType] = excludeDataStoreItem;
        }

        public bool? GetPerformedExclusionCheckResult(long dataStoreItemId, DataStoreItemCheckType dataStoreItemCheckType)
        {
            return _dataStoreItemIdToExclusionCheckResults.TryGetValue(dataStoreItemId, out var exclusionCheckResults) &&
                   exclusionCheckResults.TryGetValue(dataStoreItemCheckType, out var value)
                ? value
                : null;
        }

        public bool GetOrSetPerformedExclusionCheckResult(long dataStoreItemId, DataStoreItemCheckType dataStoreItemCheckType,
            Func<bool> getPerformedExclusionCheckResult)
        {
            var checkResult = GetPerformedExclusionCheckResult(dataStoreItemId, dataStoreItemCheckType);

            if (checkResult != null)
                return checkResult.Value;

            var newCheckResult = getPerformedExclusionCheckResult();

            OnExclusionCheckPerformed(dataStoreItemId, dataStoreItemCheckType, newCheckResult);

            return newCheckResult;
        }

        public IDataStore<TDataStoreItem> DataStore { get; }

        public Dictionary<long, DataStoreItemWrapperInProgress> DataStoreItemIdToDataStoreItemWrapper { get; } = new();

        public Dictionary<long, DataStoreItemData> DataStoreItemIdToDataStoreItem { get; } = new();

        public List<TDataStoreItem> AllDataStoreItems { get; } = new();

        public Dictionary<long, List<TDataStoreItem>> DataStoreItemIdToChildren { get; } = new();
    }

    private class DataStoreItemData
    {
        public DataStoreItemData(TDataStoreItem dataStoreItem)
        {
            DataStoreItem = dataStoreItem;
        }

        public TDataStoreItem DataStoreItem { get; }

        public bool WasAddedToSomeParent { get; set; }
    }

    private enum DataStoreItemCheckType
    {
        InvalidCopiedDataStore,
        InvalidCopiedDataStoreItem,
        FailedToCopyDataStoreItem,
        CopiedDataStoreItemDoesNotImplementInterface
    }

    private class DataStoreItemWrapperInProgress
    {
        public DataStoreItemWrapperInProgress(TNonCopyDataStoreItem dataStoreItem, long dataStoreId)
        {
            DataStoreItem = dataStoreItem;
            DataStoreId = dataStoreId;
        }

        public void OnCopiedToNewDataStore(TNonCopyDataStoreItem dataStoreItem, long dataStoreId)
        {
            DataStoreItem = dataStoreItem;
            DataStoreId = dataStoreId;
        }

        public HashSet<long> DataStoresInWhichChildrenWereAdded { get; } = new();

        public long DataStoreId { get; private set; }

        public TNonCopyDataStoreItem DataStoreItem { get; private set; }

        public DataStoreItemWrapperInProgress? Parent { get; set; }

        public List<DataStoreItemWrapperInProgress> Children { get; } = new ();
    }
}