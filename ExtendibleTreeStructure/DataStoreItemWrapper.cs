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

using System.Collections.Generic;

namespace ExtendibleTreeStructure;

/// <summary>
/// A wrapper for <see cref="INonCopyDataStoreItem"/>. 
/// </summary>
/// <typeparam name="TNonCopyDataStoreItem">Generic type parameter for a type of <see cref="INonCopyDataStoreItem"/>.</typeparam>
/// <typeparam name="TDataStoreItemWrapper">Generic type parameter used for type of <see cref="Parent"/> and items in <see cref="Children"/>.</typeparam>
public class DataStoreItemWrapper<TNonCopyDataStoreItem, TDataStoreItemWrapper> 
    where TNonCopyDataStoreItem : class, INonCopyDataStoreItem
    where TDataStoreItemWrapper : DataStoreItemWrapper<TNonCopyDataStoreItem, TDataStoreItemWrapper>
{
    private readonly List<TDataStoreItemWrapper> _children = new();

    /// <summary>
    /// Constructor.
    /// </summary>
    /// <param name="dataStoreId">Data store Id of the data store that owns the data store item.</param>
    /// <param name="dataStoreItem">Wrapped data store item.</param>
    /// <param name="parent">Parent data store item wrapper.</param>
    public DataStoreItemWrapper(long dataStoreId, TNonCopyDataStoreItem dataStoreItem, TDataStoreItemWrapper? parent = null)
    {
        DataStoreId = dataStoreId;
        DataStoreItem = dataStoreItem;
        Parent = parent;
    }

    /// <summary>
    /// Data store item.
    /// </summary>
    public TNonCopyDataStoreItem DataStoreItem { get; }

    /// <summary>
    /// Parent ata store item wrapper.
    /// </summary>
    public TDataStoreItemWrapper? Parent { get; }

    /// <summary>
    /// Data store Id of the data store that owns the data store item.
    /// </summary>
    public long DataStoreId { get; }

    /// <summary>
    /// Children of data store item wrapper. Children are sorted based on the values of <see cref="IDataStoreItem.Priority"/>.
    /// </summary>
    public IReadOnlyList<TDataStoreItemWrapper> Children => _children;

    /// <summary>
    /// Initializes the value of <see cref="Children"/>. 
    /// </summary>
    /// <param name="children">Child data store item wrappers</param>
   
    internal void SetChildren(IReadOnlyList<TDataStoreItemWrapper> children)
    {
        _children.AddRange(children);
    }
}