/* Copyright 2013-present MongoDB Inc.
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
using System.Net;
#if NET452
using System.Runtime.Serialization;
#endif
using MongoDB.Driver.Core.Misc;

namespace MongoDB.Driver
{
    /// <summary>
    /// Represents a MongoDB connection pool paused exception.
    /// </summary>
#if NET452
    [Serializable]
#endif
    public class MongoPoolPausedException : MongoClientException
    {
        #region static
        // static methods
        internal static MongoPoolPausedException ForConnectionPool(EndPoint endPoint)
        {
            var message = $"The connection pool is in paused state for server {EndPointHelper.ToString(endPoint)} is full.";
            return new MongoPoolPausedException(message);
        }

        #endregion

        // constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="MongoPoolPausedException"/> class.
        /// </summary>
        /// <param name="message">The error message.</param>
        public MongoPoolPausedException(string message)
            : base(message, null)
        {
        }

#if NET452
        /// <summary>
        /// Initializes a new instance of the <see cref="MongoWaitQueueFullException"/> class.
        /// </summary>
        /// <param name="info">The SerializationInfo.</param>
        /// <param name="context">The StreamingContext.</param>
        protected MongoPoolPausedException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
#endif
    }
}
