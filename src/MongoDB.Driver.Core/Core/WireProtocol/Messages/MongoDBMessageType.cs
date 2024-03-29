/* Copyright 2015-present MongoDB Inc.
*
* Licensed under the Apache License, Version 2.0 (the "License");
* you may not use this file except in compliance with the License.
* You may obtain a copy of the License at
*
* http://www.apache.org/licenses/LICENSE-2.0
*
* Unless required by applicable law or agreed to in writing, software
* distributed under the License is distributed on an "AS IS" BASIS,
* WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
* See the License for the specific language governing permissions and
* limitations under the License.
*/


using System;

namespace MongoDB.Driver.Core.WireProtocol.Messages
{
    /// <summary>
    /// Represents the type of message.
    /// </summary>
    [Obsolete("This enum will be removed in later release.")]
    public enum MongoDBMessageType
    {
        /// <summary>
        /// OP MSG
        /// </summary>
        Command,
        /// <summary>
        /// OP_COMPRESSED
        /// </summary>
        Compressed,
        /// <summary>
        /// OP_QUERY
        /// </summary>
        Query,
        /// <summary>
        /// OP_REPLY
        /// </summary>
        Reply
    }
}
