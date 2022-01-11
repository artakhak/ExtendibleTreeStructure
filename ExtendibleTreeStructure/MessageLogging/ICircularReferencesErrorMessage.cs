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
using ExtendibleTreeStructure.CircularReferencePrevention;

namespace ExtendibleTreeStructure.MessageLogging
{
    /// <summary>
    /// Log message for scenarios when data store items end up referencing each other through child/parent or when copying data store items.
    /// </summary>
    public interface ICircularReferencesErrorMessage : ILoggedMessage
    {
        /// <summary>
        /// List of <see cref="PathComponentEdge"/> items that describes the path that resulted in circular relationship.
        /// The last item in this list will have <see cref="PathComponentEdge.EndingNode"/>
        /// that is similar to (same data store id and data store item id) <see cref="PathComponentEdge.StartingNode"/> in the first item in this list.
        /// </summary>
        IReadOnlyList<PathComponentEdge> CircularReferencesPath { get; }
    }

    /// <summary>
    /// In implementation of <see cref="ICircularReferencesErrorMessage"/>.
    /// </summary>
    public class CircularReferencesErrorMessage : LoggedMessage, ICircularReferencesErrorMessage
    {
        internal CircularReferencesErrorMessage(string message, IReadOnlyList<PathComponentEdge> cyclicReferencesPath,
            long dataStoreId, IDataStoreItem dataStoreItem) :
            base(MessageType.CircularReferences, message, dataStoreId, dataStoreItem, MessageCategory.Error)
        {
            CircularReferencesPath = cyclicReferencesPath;
        }

        /// <inheritdoc />
        public IReadOnlyList<PathComponentEdge> CircularReferencesPath { get; }
    }
}