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

namespace ExtendibleTreeStructure.MessageLogging
{
    /// <summary>
    /// Logged message type.
    /// </summary>
    public enum MessageType
    {
        /// <summary>
        /// Custom error initiated by 
        /// </summary>
        Custom = 1,

        /// <summary>
        /// Multiple occurrences of data store wit the same value for <see cref="IDataStore{TDataStoreItem}.DataStoreId"/>.
        /// Data store Ids should be unique.
        /// </summary>
        DataStoreAppearsMultipleTimes,

        /// <summary>
        /// Multiple occurrences of data store item with the same value for <see cref="IDataStoreItem.Id"/> within the same data store.
        /// </summary>
        DataStoreItemAppearsMultipleTimesInSameDataStore,

        /// <summary>
        /// Parent Id specified in data store item <see cref="ICanHaveParent.ParentId"/> property is invalid because either no data store item with
        /// <see cref="ICanHaveParent.ParentId"/> exists in the data store that owns the data store item, or because the parent data store item
        /// has errors and were removed..
        /// </summary>
        InvalidDataStoreItemParent,

        /// <summary>
        /// The value specified in <see cref="ICopyDataStoreItem.ReferencedDataStoreId"/> and <see cref="ICopyDataStoreItem.ReferencedDataStoreItemId"/>
        /// reference to an invalid data store item.
        /// </summary>
        InvalidCopiedDataStoreItem,

        /// <summary>
        /// Data store item specified in <see cref="ICopyDataStoreItem"/> failed to get copied. Normally happens because of other errors, such as circular
        /// references, invalid implementation of <see cref="IDataStoreItem"/> referenced by <see cref="ICopyDataStoreItem"/>, etc.
        /// </summary>
        FailedToCopyDataStoreItem,

        /// <summary>
        /// This is a non-error message for the situation when dat store item is intentionally per result of virtual method
        /// ignored by returning null in <see cref="DataStoresCache{TDataStoreItem, TNonCopyDataStoreItem, TDataStoreItemWrapper}"/>.ConvertDataStoreItemWrapper() method.
        /// </summary>
        DataStoreItemIsIgnored,

        /// <summary>
        /// An error that data store item cannot be a child of data store item specified in <see cref="ICanHaveParent.ParentId"/>
        /// per result of virtual method
        /// <see cref="DataStoresCache{TDataStoreItem, TNonCopyDataStoreItem, TDataStoreItemWrapper}"/>.IsValidChildDataStoreItem(childDataStoreItem, parentDataStoreItem).
        /// </summary>
        InvalidChildDataStoreItem,

        /// <summary>
        /// Data store item type is of invalid type 
        /// </summary>
        InvalidDataStoreItemType,

        /// <summary>
        /// Copied data store item does not implement valid interface. 
        /// </summary>
        CopiedDataStoreItemDoesNotImplementInterface,

        /// <summary>
        /// An error message for situation when copy data store item <see cref="ICopyDataStoreItem"/> references itself
        /// via properties <see cref="ICopyDataStoreItem.ReferencedDataStoreId"/> and <see cref="ICopyDataStoreItem.ReferencedDataStoreItemId"/>
        /// </summary>
        CopyDataStoreItemReferencesItself,

        /// <summary>
        /// An error message for situation when data store item is an instance of <see cref="ICanHaveParent"/> and <see cref="ICanHaveParent.ParentId"/>
        /// is equal to <see cref="IDataStoreItem.Id"/> 
        /// </summary>
        ParentIdReferencesItself,

        /// <summary>
        /// An error message for situation when data store items reference each other that result in circular references.
        /// Logged error message <see cref="ICircularReferencesErrorMessage"/> in this case provides more details in this case.
        /// </summary>
        CircularReferences,

        /// <summary>
        /// Ann error indicating an issue in code, rather than a configuration error.
        /// </summary>
        ImplementationError
    }
}