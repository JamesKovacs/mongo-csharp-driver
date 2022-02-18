/* Copyright 2010-present MongoDB Inc.
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

namespace MongoDB.Driver
{
#pragma warning disable CS1591
    public abstract class DocumentsWindowBoundary
    {
        public abstract BsonValue Render();
    }

    public sealed class KeywordDocumentsWindowBoundary : DocumentsWindowBoundary
    {
        private readonly string _keyword;

        public KeywordDocumentsWindowBoundary(string keyword)
        {
            _keyword = Ensure.IsNotNullOrEmpty(keyword, nameof(keyword));
        }

        public string Keyword => _keyword;

        public override BsonValue Render() => _keyword;
        public override string ToString() => $"\"{_keyword}\"";
    }

    public sealed class PositionDocumentsWindowBoundary : DocumentsWindowBoundary
    {
        private readonly int _position;

        public PositionDocumentsWindowBoundary(int position)
        {
            _position = position;
        }

        public int Position => _position;

        public override BsonValue Render() => _position;
        public override string ToString() => _position.ToString();
    }
#pragma warning restore
}
