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
using MongoDB.Bson.Serialization;
using MongoDB.Driver.Linq3.Ast.Expressions;
using MongoDB.Driver.Linq3.Ast.Stages;
using MongoDB.Driver.Linq3.Misc;
using MongoDB.Driver.Linq3.Reflection;
using MongoDB.Driver.Linq3.Serializers;
using MongoDB.Driver.Linq3.Translators.ExpressionToAggregationExpressionTranslators;
using MongoDB.Driver.Linq3.Translators.ExpressionToExecutableQueryTranslators.Finalizers;
using MongoDB.Driver.Linq3.Translators.ExpressionToPipelineTranslators;

namespace MongoDB.Driver.Linq3.Translators.ExpressionToExecutableQueryTranslators
{
    internal static class MinMethodToExecutableQueryTranslator<TOutput>
    {
        // private static fields
        private static readonly IExecutableQueryFinalizer<TOutput, TOutput> __finalizer = new SingleFinalizer<TOutput>();
        private static readonly MethodInfo[] __minMethods;
        private static readonly MethodInfo[] __minWithSelectorMethods;

        // static constructor
        static MinMethodToExecutableQueryTranslator()
        {
            __minMethods = new[]
            {
                QueryableMethod.Min,
                QueryableMethod.MinWithSelector,
                MongoQueryableMethod.MinAsync,
                MongoQueryableMethod.MinWithSelectorAsync,
            };

            __minWithSelectorMethods = new[]
            {
                QueryableMethod.MinWithSelector,
                MongoQueryableMethod.MinWithSelectorAsync,
            };
        }

        // public static methods
        public static ExecutableQuery<TDocument, TOutput> Translate<TDocument>(MongoQueryProvider<TDocument> provider, TranslationContext context, MethodCallExpression expression)
        {
            var method = expression.Method;
            var arguments = expression.Arguments;

            if (method.IsOneOf(__minMethods))
            {
                var sourceExpression = arguments[0];
                var pipeline = ExpressionToPipelineTranslator.Translate(context, sourceExpression);
                var sourceSerializer = pipeline.OutputSerializer;

                AstExpression valueAst;
                IBsonSerializer valueSerializer;
                if (method.IsOneOf(__minWithSelectorMethods))
                {
                    var selectorLambda = ExpressionHelper.UnquoteLambda(arguments[1]);
                    var selectorTranslation = ExpressionToAggregationExpressionTranslator.TranslateLambdaBody(context, selectorLambda, sourceSerializer, asCurrentSymbol: true);
                    if (selectorTranslation.Serializer is IBsonDocumentSerializer)
                    {
                        valueAst = selectorTranslation.Ast;
                        valueSerializer = selectorTranslation.Serializer;
                    }
                    else
                    {
                        valueAst = AstExpression.ComputedDocument(new[] { AstExpression.ComputedField("_v", selectorTranslation.Ast) });
                        valueSerializer = WrappedValueSerializer.Create(selectorTranslation.Serializer);
                    }
                }
                else
                {
                    valueAst = AstExpression.Field("$ROOT");
                    valueSerializer = pipeline.OutputSerializer;
                }

                pipeline = pipeline.AddStages(
                    valueSerializer,
                    AstStage.Group(
                        id: BsonNull.Value,
                        fields: AstExpression.ComputedField("_min", AstExpression.Min(valueAst))),
                    AstStage.ReplaceRoot(AstExpression.Field("_min")));

                return new ExecutableQuery<TDocument, TOutput, TOutput>(
                    provider.Collection,
                    provider.Options,
                    pipeline,
                    __finalizer);
            }

            throw new ExpressionNotSupportedException(expression);
        }
    }
}
