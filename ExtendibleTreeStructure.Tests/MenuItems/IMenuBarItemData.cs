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
    /// <summary>
    /// Top level menu bar item.
    /// </summary>
    public interface IMenuBarItemData : INonCopyMenuObject, ICanHaveParent
    {
        long CommandId { get; }
    }

    public class MenuBarItemData : MenuItemDataBase, IMenuBarItemData

    {
        public MenuBarItemData(long commandId, long? parentMenuBarId = null) : base(commandId, parentMenuBarId)
        {
        }

        public override INonCopyMenuObject CloneWithNewParent(long id, long parentId)
        {
            return new MenuBarItemData(id, parentId)
            {
                Priority = Priority
            };
        }

        public override INonCopyMenuObject CloneWithNullParent(long id)
        {
            return new MenuBarItemData(id)
            {
                Priority = Priority
            };
        }

        public override bool Equals(object? obj)
        {
            if (obj is not IMenuBarItemData menuBarItemData)
                return false;

            return this.Id == menuBarItemData.Id && this.CommandId == menuBarItemData.CommandId &&
                   (menuBarItemData.IsParentNulledOut || this.ParentId == menuBarItemData.ParentId) &&
                   this.Priority == menuBarItemData.Priority;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }
}