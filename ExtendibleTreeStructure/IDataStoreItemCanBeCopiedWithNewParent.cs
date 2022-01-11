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

namespace ExtendibleTreeStructure
{
    /// <summary>
    /// An interface that should be implemented by subclasses and implementations of <see cref="IDataStoreItem"/> that should
    /// support being copied with new parent (either directly, when data store item is referenced by <see cref="ICopyDataStoreItem"/>,
    /// or indirectly, when data store item needs to be copied when parent data store item is copied).
    /// General rule is for all implementations of <see cref="IDataStoreItem"/> to implement also this interface, unless
    /// data store item can never have a parent (say menu bar can have no parent, so oes not need to implement this interface, however,
    /// menu bar items, menu items, submenu items should implement this interface).
    /// </summary>
    public interface IDataStoreItemCanBeCopiedWithNewParent<TNonCopyDataStoreItem> where TNonCopyDataStoreItem : class, INonCopyDataStoreItem
    {
        /// <summary>
        /// Returns a copy of the data store item with new Id and parent Id specified in parameters <paramref name="id"/> and <paramref name="parentId"/>.
        /// </summary>
        /// <param name="id">New Id in copied data store item.</param>
        /// <param name="parentId">New parent Id in copied data store item.</param>
        /// <returns></returns>
        TNonCopyDataStoreItem CloneWithNewParent(long id, long parentId);
    }
}