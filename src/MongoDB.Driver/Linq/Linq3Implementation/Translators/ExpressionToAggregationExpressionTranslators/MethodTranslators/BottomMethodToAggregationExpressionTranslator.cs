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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Driver.Linq.Linq3Implementation.Ast;
using MongoDB.Driver.Linq.Linq3Implementation.Ast.Expressions;
using MongoDB.Driver.Linq.Linq3Implementation.ExtensionMethods;
using MongoDB.Driver.Linq.Linq3Implementation.Misc;
using MongoDB.Driver.Linq.Linq3Implementation.Reflection;
using MongoDB.Driver.Linq.Linq3Implementation.Serializers;

namespace MongoDB.Driver.Linq.Linq3Implementation.Translators.ExpressionToAggregationExpressionTranslators.MethodTranslators
{
    internal static class BottomMethodToAggregationExpressionTranslator
    {
        public static AggregationExpression Translate(TranslationContext context, MethodCallExpression expression)
        {
            var method = expression.Method;
            var arguments = expression.Arguments;

            if (method.IsOneOf(EnumerableMethod.Bottom, EnumerableMethod.BottomN))
            {
                var sourceExpression = arguments[0];
                var sourceTranslation = ExpressionToAggregationExpressionTranslator.TranslateEnumerable(context, sourceExpression);
                var itemSerializer = ArraySerializerHelper.GetItemSerializer(sourceTranslation.Serializer);

                var sortByExpression = arguments[1];
                var sortByDefinition = GetSortByDefinition(sortByExpression, expression);
                var sortBy = RenderSortByDefinition(expression, sortByExpression, sortByDefinition, itemSerializer);

                var outputLambda = (LambdaExpression)arguments[2];
                var outputParameter = outputLambda.Parameters.Single();
                var outputParameterSymbol = context.CreateSymbol(outputParameter, itemSerializer);
                var outputContext = context.WithSymbol(outputParameterSymbol);
                var outputTranslation = ExpressionToAggregationExpressionTranslator.TranslateLambdaBody(outputContext, outputLambda, itemSerializer, asRoot: false);

                AstExpression ast;
                IBsonSerializer resultSerializer;
                if (method.Is(EnumerableMethod.Bottom))
                {
                    ast = AstExpression.Bottom(
                        input: sourceTranslation.Ast,
                        @as: outputParameterSymbol.Var,
                        sortBy: sortBy,
                        output: outputTranslation.Ast);
                    resultSerializer = outputTranslation.Serializer;
                }
                else
                {
                    var nExpression = arguments[3];
                    var nTranslation = ExpressionToAggregationExpressionTranslator.Translate(context, nExpression);

                    ast = AstExpression.BottomN(
                        input: sourceTranslation.Ast,
                        @as: outputParameterSymbol.Var,
                        sortBy: sortBy,
                        output: outputTranslation.Ast,
                        n: nTranslation.Ast);
                    resultSerializer = IEnumerableSerializer.Create(outputTranslation.Serializer);
                }

                return new AggregationExpression(expression, ast, resultSerializer);
            }

            throw new ExpressionNotSupportedException(expression);
        }

        private static object GetSortByDefinition(Expression sortByExpression, Expression expression)
        {
            if (sortByExpression.NodeType == ExpressionType.Constant)
            {
                return sortByExpression.GetConstantValue<object>(sortByExpression);
            }

            // we get here when the PartialEvaluator couldn't fully evalute the SortDefinition
            try
            {
                LambdaExpression lambda = Expression.Lambda(sortByExpression);
                Delegate @delegate = lambda.Compile();
                return @delegate.DynamicInvoke(null);
            }
            catch (Exception ex)
            {
                throw new ExpressionNotSupportedException(sortByExpression, expression, because: $"attempting to evaluate the sortBy expression failed: {ex.Message}");
            }
        }

        private static AstSortFields RenderSortByDefinition(Expression expression, Expression sortByExpression, object sortByDefinition, IBsonSerializer documentSerializer)
        {
            var methodInfoDefinition = typeof(BottomMethodToAggregationExpressionTranslator).GetMethod("RenderSortByDefinitionGeneric", BindingFlags.Static | BindingFlags.NonPublic);
            var documentType = documentSerializer.ValueType;
            var methodInfo = methodInfoDefinition.MakeGenericMethod(documentType);
            return (AstSortFields)methodInfo.Invoke(null, new object[] { expression, sortByExpression, sortByDefinition, documentSerializer });
        }

#pragma warning disable IDE0051 // Remove unused private members
        // Visual Studio thinks this method is unused but that's only because we call it using reflection
        private static AstSortFields RenderSortByDefinitionGeneric<TDocument>(Expression expression, Expression sortByExpression, SortDefinition<TDocument> sortByDefinition, IBsonSerializer<TDocument> documentSerializer)
#pragma warning restore IDE0051 // Remove unused private members
        {
            var serializerRegistry = BsonSerializer.SerializerRegistry;
            var sortDocument = sortByDefinition.Render(documentSerializer, serializerRegistry);
            var fields = new List<AstSortField>();
            foreach (var element in sortDocument)
            {
                var order = element.Value switch
                {
                    BsonInt32 v when v.Value == 1 => AstSortOrder.Ascending,
                    BsonInt32 v when v.Value == -1 => AstSortOrder.Descending,
                    BsonString s when element.Name == "$meta" && s.Value == "textScore" => AstSortOrder.MetaTextScore,
                    _ => throw new ExpressionNotSupportedException(sortByExpression, expression, because: $"sort order is not supported: {{ {element.Name} : {element.Value} }}")
                };
                var field = AstSort.Field(element.Name, order);
                fields.Add(field);
            }

            return new AstSortFields(fields);
        }
    }
}
