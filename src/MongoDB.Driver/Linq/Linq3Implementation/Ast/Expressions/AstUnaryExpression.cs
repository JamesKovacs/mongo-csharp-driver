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

namespace MongoDB.Driver.Linq.Linq3Implementation.Ast.Expressions
{
    internal sealed class AstUnaryExpression : AstExpression
    {
        private readonly AstExpression _arg;
        private readonly AstUnaryOperator _operator;

        public AstUnaryExpression(AstUnaryOperator @operator, AstExpression arg)
        {
            _operator = @operator;
            _arg = Ensure.IsNotNull(arg, nameof(arg));
        }

        public AstExpression Arg => _arg;
        public override AstNodeType NodeType => AstNodeType.UnaryExpression;
        public AstUnaryOperator Operator => _operator;

        public override BsonValue Render()
        {
            return new BsonDocument(_operator.Render(), RenderArg());
        }

        private BsonValue RenderArg()
        {
            var rendered = _arg.Render();
            if (rendered.IsBsonArray)
            {
                rendered = new BsonArray { rendered };
            }
            return rendered;
        }
    }
}
