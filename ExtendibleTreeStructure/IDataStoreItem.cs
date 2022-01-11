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

using System.Text;

namespace ExtendibleTreeStructure
{
    /// <summary>
    /// Data store item.
    /// </summary>
    public interface IDataStoreItem
    {
        /// <summary>
        /// A unique Id.
        /// </summary>
        long Id { get; }

        /// <summary>
        /// Priority used to sort data store items in parent data store items child collection.
        /// Data store items with lower values will appear first.
        /// If the value is null, the item has no priority and will appear after data store items that have non-null values.
        /// </summary>
        int? Priority { get; set; }

        /// <summary>
        /// Implement this method to do any additional initialization.
        /// </summary>
        void Initialize();

        /// <summary>
        /// Text displayed for this data store item in error messages.
        /// If the value is null, default display value will be used (e.g., as 'data store item (Id: 637754493334317206, ParentId: 637754489977943646, DataStoreId: 637754483806963195)'.
        /// Examples of how to override this are
        /// "menu bar (Id:637754493334317206, ParentId:637754489977943646, DataStoreId: 637754483806963195)" or
        /// "menu bar item (Id:637754493334317206, ParentId:637754489977943646, DataStoreId: 637754483806963195)".
        /// </summary>
        string? GetDisplayValue(long dataStoreId);
    }

    /// <summary>
    /// Extension methods for <see cref="IDataStoreItem"/>
    /// </summary>
    public static class DataStoreItemExtensions
    {
        /// <summary>
        /// Returns a display value for data store item <see cref="IDataStoreItem"/> that are used in logs.
        /// </summary>
        public static string GetDisplayValue(this IDataStoreItem dataStoreItem, long dataStoreId, bool capitalize)
        {
            var displayValueStrBldr = new StringBuilder();

            var displayValue = dataStoreItem.GetDisplayValue(dataStoreId);

            if (displayValue != null && displayValue.Length > 0)
            {
                displayValueStrBldr.Append(displayValue);

                displayValueStrBldr[0] = capitalize ? char.ToUpper(displayValueStrBldr[0]) : char.ToLower(displayValueStrBldr[0]);
                return displayValueStrBldr.ToString();
            }

            ICopyDataStoreItem? copyDataStoreItem = dataStoreItem as ICopyDataStoreItem;

            if (copyDataStoreItem != null)
                displayValueStrBldr.Append($"{(capitalize ? 'C' : 'c')}opy data");
            else
                displayValueStrBldr.Append($"{(capitalize ? 'D' : 'd')}ata");

            displayValueStrBldr.Append($" store item: ({nameof(IDataStoreItem.Id)}:{dataStoreItem.Id})");

            if (dataStoreItem is ICanHaveParent {ParentId: { }} canHaveParent)
                displayValueStrBldr.Append($", ({nameof(ICanHaveParent.ParentId)}:{canHaveParent.ParentId})");

            displayValueStrBldr.Append($", DataStoreId:{dataStoreId}");

            if (copyDataStoreItem != null)
            {
                displayValueStrBldr.Append($", ({nameof(ICopyDataStoreItem.ReferencedDataStoreId)}:{copyDataStoreItem.ReferencedDataStoreId})");
                displayValueStrBldr.Append($", ({nameof(ICopyDataStoreItem.ReferencedDataStoreItemId)}:{copyDataStoreItem.ReferencedDataStoreItemId})");
            }

            return displayValueStrBldr.ToString();
        }
    }
}