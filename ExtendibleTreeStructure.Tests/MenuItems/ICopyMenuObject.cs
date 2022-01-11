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

namespace ExtendibleTreeStructure.Tests.MenuItems
{
    public interface ICopyMenuObject : IMenuObject, ICopyDataStoreItem
    {

    }

    public class CopyMenuObject : MenuObject, ICopyMenuObject
    {
        public CopyMenuObject(long referencedMenuDataStoreId, long referencedMenuObjectId, long? parentId) :
            this(referencedMenuObjectId, referencedMenuDataStoreId, referencedMenuObjectId, parentId)
        {

        }

        public CopyMenuObject(long id, long referencedMenuDataStoreId, long referencedMenuObjectId, long? parentId) : base(id, parentId)
        {
            ReferencedDataStoreId = referencedMenuDataStoreId;
            ReferencedDataStoreItemId = referencedMenuObjectId;
        }

        public long ReferencedDataStoreId { get; }
        public long ReferencedDataStoreItemId { get; }

        public ICopyDataStoreItem CloneWithNullParent()
        {
            return new CopyMenuObject(this.Id, ReferencedDataStoreItemId, ReferencedDataStoreItemId);
        }

        public override bool Equals(object? obj)
        {
            if (obj is not ICopyMenuObject copyMenuObject)
                return false;

            return this.Id == copyMenuObject.Id &&
                   (copyMenuObject.IsParentNulledOut || this.ParentId == copyMenuObject.ParentId) &&
                   this.Priority == copyMenuObject.Priority &&
                   ReferencedDataStoreId == copyMenuObject.ReferencedDataStoreId &&
                   ReferencedDataStoreItemId == copyMenuObject.ReferencedDataStoreItemId;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public override string GetDisplayValue(long dataStoreId)
        {
            var displayValue = new StringBuilder();

            displayValue.Append($"copy menu object: ({nameof(IDataStoreItem.Id)}:{TestHelpers.GetConstantNameForLogs(Id)})");

            if (ParentId != null)
                displayValue.Append($", ({nameof(ICanHaveParent.ParentId)}:{TestHelpers.GetConstantNameForLogs(ParentId.Value)})");

            displayValue.Append($", (DataStoreId:{TestHelpers.GetConstantNameForLogs(dataStoreId)})");
            displayValue.Append($", ({nameof(ReferencedDataStoreId)}:{TestHelpers.GetConstantNameForLogs(ReferencedDataStoreId)})");
            displayValue.Append($", ({nameof(ReferencedDataStoreItemId)}:{TestHelpers.GetConstantNameForLogs(ReferencedDataStoreItemId)})");

            return displayValue.ToString();
        }
    }
}