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
using MongoDB.Bson.Serialization.Serializers;
using MongoDB.Driver.Linq3.Ast.Expressions;
using MongoDB.Driver.Linq3.Methods;
using MongoDB.Driver.Linq3.Misc;

namespace MongoDB.Driver.Linq3.Translators.ExpressionTranslators.MethodTranslators
{
    public static class AnyMethodTranslator
    {
        public static TranslatedExpression Translate(TranslationContext context, MethodCallExpression expression)
        {
            var sourceExpression = expression.Arguments[0];
            var sourceTranslation = ExpressionTranslator.Translate(context, sourceExpression);

            if (expression.Method.Is(EnumerableMethod.Any))
            {
                var translation = new AstBinaryExpression(
                    AstBinaryOperator.Gt,
                    new AstUnaryExpression(AstUnaryOperator.Size, sourceTranslation.Translation),
                    0);
                return new TranslatedExpression(expression, translation, new BooleanSerializer());
            }

            if (expression.Method.Is(EnumerableMethod.AnyWithPredicate))
            {
                var predicateExpression = (LambdaExpression)expression.Arguments[1];
                var predicateParameter = predicateExpression.Parameters[0];
                var predicateParameterSerializer = ArraySerializerHelper.GetItemSerializer(sourceTranslation.Serializer);
                var predicateContext = context.WithSymbol(predicateParameter, new Symbol("$this", predicateParameterSerializer));
                var predicateTranslation = ExpressionTranslator.Translate(predicateContext, predicateExpression.Body);

                var translation = new AstReduceExpression(
                    input: sourceTranslation.Translation,
                    initialValue: false,
                    @in: new AstCondExpression(
                        @if: new AstFieldExpression("$$value"),
                        then: true,
                        @else: predicateTranslation.Translation));

                return new TranslatedExpression(expression, translation, new BooleanSerializer());
            }

            throw new ExpressionNotSupportedException(expression);
        }
    }
}
