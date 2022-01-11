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
    public interface INonCopyMenuObject : IMenuObject, INonCopyDataStoreItem, IDataStoreItemCanBeCopiedWithNullParent<INonCopyMenuObject>
    {

    }

    public abstract class NonCopyMenuObject : MenuObject, INonCopyMenuObject
    {
        protected NonCopyMenuObject(long id, long? parentId = null) : base(id, parentId)
        {
        }

        public abstract INonCopyMenuObject CloneWithNullParent(long id);

        public override string GetDisplayValue(long dataStoreId)
        {
            var strBldr = new StringBuilder();
            strBldr.Append($"menu object ({nameof(IDataStoreItem.Id)}:{TestHelpers.GetConstantNameForLogs(Id)})");

            if (ParentId != null)
                strBldr.Append($", ({nameof(ICanHaveParent.ParentId)}:{TestHelpers.GetConstantNameForLogs(ParentId.Value)})");

            strBldr.Append($", (DataStoreId:{TestHelpers.GetConstantNameForLogs(dataStoreId)})");

            return strBldr.ToString();
        }
    }
}