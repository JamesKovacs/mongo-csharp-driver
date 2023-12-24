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

using System.Linq.Expressions;
using System.Reflection;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Serializers;
using MongoDB.Driver.Linq.Linq3Implementation.Ast;
using MongoDB.Driver.Linq.Linq3Implementation.Ast.Expressions;
using MongoDB.Driver.Linq.Linq3Implementation.Misc;
using MongoDB.Driver.Linq.Linq3Implementation.Reflection;

namespace MongoDB.Driver.Linq.Linq3Implementation.Translators.ExpressionToAggregationExpressionTranslators.MethodTranslators
{
    internal static class IsMissingMethodToAggregationExpressionTranslator
    {
        private static readonly MethodInfo[] __isMissingMethods =
        {
            MongoDBFunctionsExtensionsMethod.IsMissing,
            MongoDBFunctionsExtensionsMethod.IsNullOrMissing,
       };

        public static AggregationExpression Translate(TranslationContext context, MethodCallExpression expression)
        {
            var method = expression.Method;
            var arguments = expression.Arguments;

            if (method.IsOneOf(__isMissingMethods))
            {
                var fieldExpression = arguments[1];
                var fieldTranslation = ExpressionToAggregationExpressionTranslator.Translate(context, fieldExpression);
                var fieldAst = fieldTranslation.Ast;
                if (fieldAst.NodeType != AstNodeType.GetFieldExpression)
                {
                    throw new ExpressionNotSupportedException(expression, because: $"argument to {method.Name} must be a reference to a field");
                }

                var ast = method.Is(MongoDBFunctionsExtensionsMethod.IsNullOrMissing) ?
                    AstExpression.In(AstExpression.Type(fieldAst), new BsonArray { "null", "missing" }) :
                    AstExpression.Eq(AstExpression.Type(fieldAst), "missing");

                return new AggregationExpression(expression, ast, new BooleanSerializer());
            }

            throw new ExpressionNotSupportedException(expression);
        }
    }
}
