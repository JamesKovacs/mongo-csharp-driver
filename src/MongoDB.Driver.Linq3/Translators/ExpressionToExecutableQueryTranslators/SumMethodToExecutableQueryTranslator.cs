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
using MongoDB.Driver.Core.Misc;
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
    public static class SumMethodToExecutableQueryTranslator<TOutput>
    {
        // private static fields
        private static readonly IExecutableQueryFinalizer<TOutput, TOutput> __finalizer = new SingleOrDefaultFinalizer<TOutput>();
        private static readonly MethodInfo[] __sumAsyncMethods;
        private static readonly MethodInfo[] __sumMethods;
        private static readonly MethodInfo[] __sumWithSelectorMethods;

        // static constructor
        static SumMethodToExecutableQueryTranslator()
        {
            __sumMethods = new[]
            {
                QueryableMethod.SumDecimal,
                QueryableMethod.SumDecimalWithSelector,
                QueryableMethod.SumDouble,
                QueryableMethod.SumDoubleWithSelector,
                QueryableMethod.SumInt32,
                QueryableMethod.SumInt32WithSelector,
                QueryableMethod.SumInt64,
                QueryableMethod.SumInt64WithSelector,
                QueryableMethod.SumNullableDecimal,
                QueryableMethod.SumNullableDecimalWithSelector,
                QueryableMethod.SumNullableDouble,
                QueryableMethod.SumNullableDoubleWithSelector,
                QueryableMethod.SumNullableInt32,
                QueryableMethod.SumNullableInt32WithSelector,
                QueryableMethod.SumNullableInt64,
                QueryableMethod.SumNullableInt64WithSelector,
                QueryableMethod.SumNullableSingle,
                QueryableMethod.SumNullableSingleWithSelector,
                QueryableMethod.SumSingle,
                QueryableMethod.SumSingleWithSelector,
                MongoQueryableMethod.SumDecimalAsync,
                MongoQueryableMethod.SumDecimalWithSelectorAsync,
                MongoQueryableMethod.SumDoubleAsync,
                MongoQueryableMethod.SumDoubleWithSelectorAsync,
                MongoQueryableMethod.SumInt32Async,
                MongoQueryableMethod.SumInt32WithSelectorAsync,
                MongoQueryableMethod.SumInt64Async,
                MongoQueryableMethod.SumInt64WithSelectorAsync,
                MongoQueryableMethod.SumNullableDecimalAsync,
                MongoQueryableMethod.SumNullableDecimalWithSelectorAsync,
                MongoQueryableMethod.SumNullableDoubleAsync,
                MongoQueryableMethod.SumNullableDoubleWithSelectorAsync,
                MongoQueryableMethod.SumNullableInt32Async,
                MongoQueryableMethod.SumNullableInt32WithSelectorAsync,
                MongoQueryableMethod.SumNullableInt64Async,
                MongoQueryableMethod.SumNullableInt64WithSelectorAsync,
                MongoQueryableMethod.SumNullableSingleAsync,
                MongoQueryableMethod.SumNullableSingleWithSelectorAsync,
                MongoQueryableMethod.SumSingleAsync,
                MongoQueryableMethod.SumSingleWithSelectorAsync
            };

            __sumWithSelectorMethods = new[]
            {
                QueryableMethod.SumDecimalWithSelector,
                QueryableMethod.SumDoubleWithSelector,
                QueryableMethod.SumInt32WithSelector,
                QueryableMethod.SumInt64WithSelector,
                QueryableMethod.SumNullableDecimalWithSelector,
                QueryableMethod.SumNullableDoubleWithSelector,
                QueryableMethod.SumNullableInt32WithSelector,
                QueryableMethod.SumNullableInt64WithSelector,
                QueryableMethod.SumNullableSingleWithSelector,
                QueryableMethod.SumSingleWithSelector,
                MongoQueryableMethod.SumDecimalWithSelectorAsync,
                MongoQueryableMethod.SumDoubleWithSelectorAsync,
                MongoQueryableMethod.SumInt32WithSelectorAsync,
                MongoQueryableMethod.SumInt64WithSelectorAsync,
                MongoQueryableMethod.SumNullableDecimalWithSelectorAsync,
                MongoQueryableMethod.SumNullableDoubleWithSelectorAsync,
                MongoQueryableMethod.SumNullableInt32WithSelectorAsync,
                MongoQueryableMethod.SumNullableInt64WithSelectorAsync,
                MongoQueryableMethod.SumNullableSingleWithSelectorAsync,
                MongoQueryableMethod.SumSingleWithSelectorAsync
            };

            __sumAsyncMethods = new[]
            {
                MongoQueryableMethod.SumDecimalAsync,
                MongoQueryableMethod.SumDecimalWithSelectorAsync,
                MongoQueryableMethod.SumDoubleAsync,
                MongoQueryableMethod.SumDoubleWithSelectorAsync,
                MongoQueryableMethod.SumInt32Async,
                MongoQueryableMethod.SumInt32WithSelectorAsync,
                MongoQueryableMethod.SumInt64Async,
                MongoQueryableMethod.SumInt64WithSelectorAsync,
                MongoQueryableMethod.SumNullableDecimalAsync,
                MongoQueryableMethod.SumNullableDecimalWithSelectorAsync,
                MongoQueryableMethod.SumNullableDoubleAsync,
                MongoQueryableMethod.SumNullableDoubleWithSelectorAsync,
                MongoQueryableMethod.SumNullableInt32Async,
                MongoQueryableMethod.SumNullableInt32WithSelectorAsync,
                MongoQueryableMethod.SumNullableInt64Async,
                MongoQueryableMethod.SumNullableInt64WithSelectorAsync,
                MongoQueryableMethod.SumNullableSingleAsync,
                MongoQueryableMethod.SumNullableSingleWithSelectorAsync,
                MongoQueryableMethod.SumSingleAsync,
                MongoQueryableMethod.SumSingleWithSelectorAsync
            };
        }

        // public static methods
        public static ExecutableQuery<TDocument, TOutput> Translate<TDocument>(MongoQueryProvider<TDocument> provider, TranslationContext context, MethodCallExpression expression)
        {
            var method = expression.Method;
            var arguments = expression.Arguments;

            if (method.IsOneOf(__sumMethods))
            { 
                var sourceExression = arguments[0];
                var pipeline = ExpressionToPipelineTranslator.Translate(context, sourceExression);
                var sourceSerializer = pipeline.OutputSerializer;

                AstExpression valueAst;
                if (method.IsOneOf(__sumWithSelectorMethods))
                {
                    var selectorLambda = ExpressionHelper.UnquoteLambda(arguments[1]);
                    var selectorTranslation = ExpressionToAggregationExpressionTranslator.TranslateLambdaBody(context, selectorLambda, sourceSerializer, asCurrentSymbol: true);
                    valueAst = selectorTranslation.Ast;
                }
                else
                {
                    Ensure.That(sourceSerializer is IWrappedValueSerializer, "Expected sourceSerializer to be an IWrappedValueSerializer.", nameof(sourceSerializer));
                    valueAst = AstExpression.Field("_v");
                }

                var outputValueType = method.IsOneOf(__sumAsyncMethods) ? expression.Type.GetGenericArguments()[0] : expression.Type;
                var outputValueSerializer = BsonSerializer.LookupSerializer(outputValueType);
                var outputWrappedValueSerializer = WrappedValueSerializer.Create(outputValueSerializer);

                pipeline = pipeline.AddStages(
                    outputWrappedValueSerializer,
                    AstStage.Group(
                        id: BsonNull.Value,
                        fields: AstExpression.ComputedField("_v", AstExpression.Sum(valueAst))),
                    AstStage.Project(AstProject.ExcludeId()));

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
