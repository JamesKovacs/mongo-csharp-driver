﻿/* Copyright 2010-present MongoDB Inc.
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
using MongoDB.Driver.Linq.Linq3Implementation.Ast.Visitors;

namespace MongoDB.Driver.Linq.Linq3Implementation.Ast.Filters
{
    internal sealed class AstBitsAllSetFilterOperation : AstFilterOperation
    {
        private readonly BsonValue _bitmask;

        public AstBitsAllSetFilterOperation(BsonValue bitmask)
        {
            _bitmask = Ensure.IsNotNull(bitmask, nameof(bitmask));
        }

        public BsonValue Bitmask => _bitmask;
        public override AstNodeType NodeType => AstNodeType.BitsAllSetFilterOperation;

        public override AstNode Accept(AstNodeVisitor visitor)
        {
            return visitor.VisitBitsAllSetFilterOperation(this);
        }

        public override BsonValue Render()
        {
            return new BsonDocument("$bitsAllSet", _bitmask);
        }
    }
}
