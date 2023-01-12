﻿/* Copyright 2016-present MongoDB Inc.
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

using MongoDB.Bson;
using MongoDB.Driver.Core.Misc;

namespace MongoDB.Driver.Search
{
    /// <summary>
    /// Options for counting the search results.
    /// </summary>
    public sealed class SearchCountOptions
    {
        private int? _threshold;
        private SearchCountType _type = SearchCountType.LowerBound;

        /// <summary>
        /// Gets or sets the number of documents to include in the exact count if
        /// <see cref="Type"/> is <see cref="SearchCountType.LowerBound"/>.
        /// </summary>
        public int? Threshold
        {
            get => _threshold;
            set => _threshold = Ensure.IsNullOrGreaterThanZero(value, nameof(value));
        }

        /// <summary>
        /// Gets or sets the type of count of the documents in the result set.
        /// </summary>
        public SearchCountType Type
        {
            get => _type;
            set => _type = value;
        }

        internal BsonDocument Render() =>
            new()
            {
                { "type", _type.ToCamelCase(), _type != SearchCountType.LowerBound },
                { "threshold", _threshold, _threshold != null }
            };
    }
}
