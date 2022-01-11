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
    /// Message
    /// </summary>
    public interface ILoggedMessage
    {
        /// <summary>
        /// Message text.
        /// </summary>
        string Message { get; }

        /// <summary>
        /// Data store Id for which the message is logged.
        /// </summary>
        long DataStoreId { get; }

        /// <summary>
        /// Data store item that is the source of the message.
        /// The value can be null.
        /// </summary>
        IDataStoreItem? DataStoreItem { get; }

        /// <summary>
        /// Logged message type, such us <see>  <cref>MessageType.Error</cref>
        /// </see>
        /// or <see>
        ///     <cref>MessageType.Info</cref>
        /// </see>.
        /// </summary>
        MessageCategory MessageCategory { get; }

        /// <summary>
        /// Message type that provides more context about the logged message.
        /// </summary>
        MessageType MessageType { get; }
    }

    /// <inheritdoc />
    public class LoggedMessage : ILoggedMessage
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        public LoggedMessage(MessageType messageType, string message, long dataStoreId, IDataStoreItem? dataStoreItem,
            MessageCategory messageCategory)
        {
            MessageType = messageType;
            Message = message;
            DataStoreId = dataStoreId;
            DataStoreItem = dataStoreItem;
            MessageCategory = messageCategory;
        }


        /// <inheritdoc />
        public string Message { get; }

        /// <inheritdoc />
        public long DataStoreId { get; }

        /// <inheritdoc />
        public MessageCategory MessageCategory { get; }

        /// <inheritdoc />
        public MessageType MessageType { get; }

        /// <inheritdoc />
        public IDataStoreItem? DataStoreItem { get; }
    }
}