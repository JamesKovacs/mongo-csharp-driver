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
    internal static class PickMethodToAggregationExpressionTranslator
    {
        private static readonly MethodInfo[] __pickMethods = new[]
        {
            EnumerableMethod.Bottom,
            EnumerableMethod.BottomN
        };

        private static readonly MethodInfo[] __withNMethods = new[]
        {
            EnumerableMethod.BottomN
        };

        public static AggregationExpression Translate(TranslationContext context, MethodCallExpression expression)
        {
            var method = expression.Method;
            var arguments = expression.Arguments;

            if (method.IsOneOf(__pickMethods))
            {
                var sourceExpression = arguments[0];
                var sourceTranslation = ExpressionToAggregationExpressionTranslator.TranslateEnumerable(context, sourceExpression);
                var itemSerializer = ArraySerializerHelper.GetItemSerializer(sourceTranslation.Serializer);

                var sortByExpression = arguments[1];
                var sortByDefinition = GetSortByDefinition(sortByExpression, expression);
                var sortBy = TranslateSortByDefinition(expression, sortByExpression, sortByDefinition, itemSerializer);

                var selectorLambda = (LambdaExpression)arguments[2];
                var selectorParameter = selectorLambda.Parameters.Single();
                var selectorParameterSymbol = context.CreateSymbol(selectorParameter, itemSerializer);
                var selectorContext = context.WithSymbol(selectorParameterSymbol);
                var selectorTranslation = ExpressionToAggregationExpressionTranslator.TranslateLambdaBody(selectorContext, selectorLambda, itemSerializer, asRoot: false);

                AggregationExpression nTranslation = null;
                IBsonSerializer resultSerializer;
                if (method.IsOneOf(__withNMethods))
                {
                    var nExpression = arguments.Last();
                    nTranslation = ExpressionToAggregationExpressionTranslator.Translate(context, nExpression);
                    resultSerializer = IEnumerableSerializer.Create(selectorTranslation.Serializer);
                }
                else
                {
                    resultSerializer = selectorTranslation.Serializer;
                }

                var @operator = GetOperator(method);
                var ast = AstExpression.PickExpression(
                    @operator,
                    sourceTranslation.Ast,
                    selectorParameterSymbol.Var,
                    sortBy,
                    selectorTranslation.Ast,
                    nTranslation?.Ast);

                return new AggregationExpression(expression, ast, resultSerializer);
            }

            throw new ExpressionNotSupportedException(expression);
        }

        private static AstPickOperator GetOperator(MethodInfo method)
        {
            return method.Name switch
            {
                "Bottom" => AstPickOperator.Bottom,
                "BottomN" => AstPickOperator.BottomN,
                "FirstN" => AstPickOperator.FirstN,
                "LastN" => AstPickOperator.LastN,
                "MaxN" => AstPickOperator.MaxN,
                "MinN" => AstPickOperator.MinN,
                "Top" => AstPickOperator.TopN,
                "TopN" => AstPickOperator.TopN,
                _ => throw new InvalidOperationException($"Invalid method name: {method.Name}.")
            };
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

        private static AstSortFields TranslateSortByDefinition(Expression expression, Expression sortByExpression, object sortByDefinition, IBsonSerializer documentSerializer)
        {
            var methodInfoDefinition = typeof(PickMethodToAggregationExpressionTranslator).GetMethod(nameof(TranslateSortByDefinitionGeneric), BindingFlags.Static | BindingFlags.NonPublic);
            var documentType = documentSerializer.ValueType;
            var methodInfo = methodInfoDefinition.MakeGenericMethod(documentType);
            return (AstSortFields)methodInfo.Invoke(null, new object[] { expression, sortByExpression, sortByDefinition, documentSerializer });
        }

        private static AstSortFields TranslateSortByDefinitionGeneric<TDocument>(Expression expression, Expression sortByExpression, SortDefinition<TDocument> sortByDefinition, IBsonSerializer<TDocument> documentSerializer)
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
