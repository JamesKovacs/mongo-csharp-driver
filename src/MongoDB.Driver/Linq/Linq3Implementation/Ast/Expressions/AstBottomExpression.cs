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
    // note: the server doesn't actually (yet) have this aggregation operator
    // we expect this intermediate expression to be converted to an AstBottomAccumulatorExpression by the AstGroupPipelineOptimizer
    internal sealed class AstBottomExpression : AstExpression
    {
        private readonly AstVarExpression _as;
        private readonly AstExpression _input;
        private readonly AstExpression _n;
        private readonly AstExpression _output;
        private readonly AstSortFields _sortBy;

        public AstBottomExpression(
            AstExpression input,
            AstVarExpression @as,
            AstSortFields sortBy,
            AstExpression output,
            AstExpression n)
        {
            _input = Ensure.IsNotNull(input, nameof(input));
            _as = Ensure.IsNotNull(@as, nameof(@as));
            _sortBy = Ensure.IsNotNull(sortBy, nameof(sortBy));
            _output = Ensure.IsNotNull(output, nameof(output));
            _n = n;
        }

        public AstVarExpression As => _as;
        public AstExpression Input => _input;
        public AstExpression N => _n;
        public override AstNodeType NodeType => AstNodeType.BottomExpression;
        public AstExpression Output => _output;
        public AstSortFields SortBy => _sortBy;

        public override AstNode Accept(AstNodeVisitor visitor)
        {
            return visitor.VisitBottomExpression(this);
        }

        public override BsonValue Render()
        {
            var operatorName = _n == null ? "$bottom" : "$bottomN";
            return new BsonDocument
            {
                { operatorName, new BsonDocument
                    {
                        { "input", _input.Render() },
                        { "as", _as.Name },
                        { "sortBy", new BsonDocument(_sortBy.Select(f => f.RenderAsElement())) },
                        { "output", _output.Render() },
                        { "n", _n?.Render(), _n != null }
                    }
                }
            };
        }

        public AstBottomExpression Update(
            AstExpression input,
            AstVarExpression @as,
            AstSortFields sortBy,
            AstExpression output,
            AstExpression n)
        {
            if (input == _input && @as == _as && sortBy == _sortBy && output == _output && n == _n)
            {
                return this;
            }

            return new AstBottomExpression(input, @as, sortBy, output, n);
        }
    }
}
