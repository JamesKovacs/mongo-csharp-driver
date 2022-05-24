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

using System.Linq;
using MongoDB.Bson;
using MongoDB.Driver.Core.Misc;
using MongoDB.Driver.Linq.Linq3Implementation.Ast.Visitors;

namespace MongoDB.Driver.Linq.Linq3Implementation.Ast.Expressions
{
    internal sealed class AstBottomAccumulatorExpression : AstAccumulatorExpression
    {
        private readonly AstExpression _n;
        private readonly AstExpression _output;
        private readonly AstSortFields _sortBy;

        public AstBottomAccumulatorExpression(
            AstSortFields sortBy,
            AstExpression output,
            AstExpression n)
        {
            _sortBy = Ensure.IsNotNull(sortBy, nameof(sortBy));
            _output = Ensure.IsNotNull(output, nameof(output));
            _n = n;
        }

        public AstExpression N => _n;
        public override AstNodeType NodeType => AstNodeType.BottomAccumulatorExpression;
        public AstExpression Output => _output;
        public AstSortFields SortBy => _sortBy;

        public override AstNode Accept(AstNodeVisitor visitor)
        {
            return visitor.VisitBottomAccumulatorExpression(this);
        }

        public override BsonValue Render()
        {
            var operatorName = _n == null ? "$bottom" : "$bottomN";
            return new BsonDocument
            {
                { operatorName, new BsonDocument
                    {
                        { "sortBy", new BsonDocument(_sortBy.Select(f => f.RenderAsElement())) },
                        { "output", _output.Render() },
                        { "n", _n?.Render(), _n != null }
                    }
                }
            };
        }

        public AstBottomAccumulatorExpression Update(
            AstSortFields sortBy,
            AstExpression output,
            AstExpression n)
        {
            if (sortBy == _sortBy && output == _output && n == _n)
            {
                return this;
            }

            return new AstBottomAccumulatorExpression(sortBy, output, n);
        }
    }
}
