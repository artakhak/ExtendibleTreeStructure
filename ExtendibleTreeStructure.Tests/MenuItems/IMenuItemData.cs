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

namespace ExtendibleTreeStructure.Tests.MenuItems
{
    public interface IMenuItemData : INonCopyMenuObject, ICanHaveParent
    {
        long CommandId { get; }
    }

    public class MenuItemData : MenuItemDataBase, IMenuItemData
    {
        public MenuItemData(long commandId, long? parentMenuBarId = null) : base(commandId, parentMenuBarId)
        {
        }

        public override INonCopyMenuObject CloneWithNewParent(long id, long parentId)
        {
            return new MenuItemData(id, parentId)
            {
                Priority = Priority
            };
        }

        public override INonCopyMenuObject CloneWithNullParent(long id)
        {
            return new MenuItemData(id)
            {
                Priority = Priority
            };
        }

        public override bool Equals(object? obj)
        {
            if (obj is not IMenuItemData menuItemData)
                return false;

            return this.Id == menuItemData.Id && this.CommandId == menuItemData.CommandId &&
                   (menuItemData.IsParentNulledOut || this.ParentId == menuItemData.ParentId) &&
                   this.Priority == menuItemData.Priority;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }
}