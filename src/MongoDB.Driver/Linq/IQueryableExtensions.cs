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
using System.Linq;
using MongoDB.Bson;

namespace MongoDB.Driver.Linq
{
    /// <summary>
    /// Extension methods for IQueryble.
    /// </summary>
    public static class IQueryableExtensions
    {

        /// <summary>
        /// Gets the most recently logged stages.
        /// </summary>
        /// <typeparam name="TSource">The type of the source documents.</typeparam>
        /// <param name="source">The source.</param>
        /// <returns>The logged stages.</returns>
        public static BsonDocument[] GetLoggedStages<TSource>(this IQueryable<TSource> source)
        {
            return source.GetMongoQueryProvider().LoggedStages;
        }

        /// <summary>
        /// Gets the source's provider cast to an IMongoQueryProvider.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <returns>The MongoDB query provider.</returns>
        public static IMongoQueryProvider GetMongoQueryProvider(this IQueryable source)
        {
            var provider = source.Provider as IMongoQueryProvider;
            if (provider == null)
            {
                throw new NotSupportedException($"The source argument must be a MongoDB IQueryable.");
            }

            return provider;
        }
    }
}
