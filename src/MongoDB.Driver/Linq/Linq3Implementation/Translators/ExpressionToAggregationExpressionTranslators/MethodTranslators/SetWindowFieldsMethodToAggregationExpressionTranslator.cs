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
using MongoDB.Driver.Linq.Linq3Implementation.Ast.Expressions;
using MongoDB.Driver.Linq.Linq3Implementation.ExtensionMethods;
using MongoDB.Driver.Linq.Linq3Implementation.Misc;
using MongoDB.Driver.Linq.Linq3Implementation.Reflection;
using MongoDB.Driver.Linq.Linq3Implementation.Serializers;

namespace MongoDB.Driver.Linq.Linq3Implementation.Translators.ExpressionToAggregationExpressionTranslators.MethodTranslators
{
    internal static class SetWindowFieldsMethodMethodToAggregationExpressionTranslator
    {
        private static readonly MethodInfo[] __setWindowFieldsMethods =
        {
            SetWindowFieldsMethod.AddToSet,
            SetWindowFieldsMethod.AverageWithDecimal,
            SetWindowFieldsMethod.AverageWithDouble,
            SetWindowFieldsMethod.AverageWithInt32,
            SetWindowFieldsMethod.AverageWithInt64,
            SetWindowFieldsMethod.AverageWithNullableDecimal,
            SetWindowFieldsMethod.AverageWithNullableDouble,
            SetWindowFieldsMethod.AverageWithNullableInt32,
            SetWindowFieldsMethod.AverageWithNullableInt64,
            SetWindowFieldsMethod.AverageWithNullableSingle,
            SetWindowFieldsMethod.AverageWithSingle,
            SetWindowFieldsMethod.Count,
            SetWindowFieldsMethod.CovariancePopulationWithDecimals,
            SetWindowFieldsMethod.CovariancePopulationWithDoubles,
            SetWindowFieldsMethod.CovariancePopulationWithInt32s,
            SetWindowFieldsMethod.CovariancePopulationWithInt64s,
            SetWindowFieldsMethod.CovariancePopulationWithNullableDecimals,
            SetWindowFieldsMethod.CovariancePopulationWithNullableDoubles,
            SetWindowFieldsMethod.CovariancePopulationWithNullableInt32s,
            SetWindowFieldsMethod.CovariancePopulationWithNullableInt64s,
            SetWindowFieldsMethod.CovariancePopulationWithNullableSingles,
            SetWindowFieldsMethod.CovariancePopulationWithSingles,
            SetWindowFieldsMethod.CovarianceSampleWithDecimals,
            SetWindowFieldsMethod.CovarianceSampleWithDoubles,
            SetWindowFieldsMethod.CovarianceSampleWithInt32s,
            SetWindowFieldsMethod.CovarianceSampleWithInt64s,
            SetWindowFieldsMethod.CovarianceSampleWithNullableDecimals,
            SetWindowFieldsMethod.CovarianceSampleWithNullableDoubles,
            SetWindowFieldsMethod.CovarianceSampleWithNullableInt32s,
            SetWindowFieldsMethod.CovarianceSampleWithNullableInt64s,
            SetWindowFieldsMethod.CovarianceSampleWithNullableSingles,
            SetWindowFieldsMethod.CovarianceSampleWithSingles,
            SetWindowFieldsMethod.DerivativeWithDecimal,
            SetWindowFieldsMethod.DerivativeWithDecimalAndUnit,
            SetWindowFieldsMethod.DerivativeWithDouble,
            SetWindowFieldsMethod.DerivativeWithDoubleAndUnit,
            SetWindowFieldsMethod.DerivativeWithInt32,
            SetWindowFieldsMethod.DerivativeWithInt32AndUnit,
            SetWindowFieldsMethod.DerivativeWithInt64,
            SetWindowFieldsMethod.DerivativeWithInt64AndUnit,
            SetWindowFieldsMethod.DerivativeWithSingle,
            SetWindowFieldsMethod.DerivativeWithSingleAndUnit,
            SetWindowFieldsMethod.ExponentialMovingAverageWithDecimal,
            SetWindowFieldsMethod.ExponentialMovingAverageWithDouble,
            SetWindowFieldsMethod.ExponentialMovingAverageWithInt32,
            SetWindowFieldsMethod.ExponentialMovingAverageWithInt64,
            SetWindowFieldsMethod.ExponentialMovingAverageWithSingle,
            SetWindowFieldsMethod.IntegralWithDecimal,
            SetWindowFieldsMethod.IntegralWithDecimalAndUnit,
            SetWindowFieldsMethod.IntegralWithDouble,
            SetWindowFieldsMethod.IntegralWithDoubleAndUnit,
            SetWindowFieldsMethod.IntegralWithInt32,
            SetWindowFieldsMethod.IntegralWithInt32AndUnit,
            SetWindowFieldsMethod.IntegralWithInt64,
            SetWindowFieldsMethod.IntegralWithInt64AndUnit,
            SetWindowFieldsMethod.IntegralWithSingle,
            SetWindowFieldsMethod.IntegralWithSingleAndUnit,
            SetWindowFieldsMethod.Max,
            SetWindowFieldsMethod.Min,
            SetWindowFieldsMethod.Push,
            SetWindowFieldsMethod.StandardDeviationPopulationWithDecimal,
            SetWindowFieldsMethod.StandardDeviationPopulationWithDouble,
            SetWindowFieldsMethod.StandardDeviationPopulationWithInt32,
            SetWindowFieldsMethod.StandardDeviationPopulationWithInt64,
            SetWindowFieldsMethod.StandardDeviationPopulationWithNullableDecimal,
            SetWindowFieldsMethod.StandardDeviationPopulationWithNullableDouble,
            SetWindowFieldsMethod.StandardDeviationPopulationWithNullableInt32,
            SetWindowFieldsMethod.StandardDeviationPopulationWithNullableInt64,
            SetWindowFieldsMethod.StandardDeviationPopulationWithNullableSingle,
            SetWindowFieldsMethod.StandardDeviationPopulationWithSingle,
            SetWindowFieldsMethod.StandardDeviationSampleWithDecimal,
            SetWindowFieldsMethod.StandardDeviationSampleWithDouble,
            SetWindowFieldsMethod.StandardDeviationSampleWithInt32,
            SetWindowFieldsMethod.StandardDeviationSampleWithInt64,
            SetWindowFieldsMethod.StandardDeviationSampleWithNullableDecimal,
            SetWindowFieldsMethod.StandardDeviationSampleWithNullableDouble,
            SetWindowFieldsMethod.StandardDeviationSampleWithNullableInt32,
            SetWindowFieldsMethod.StandardDeviationSampleWithNullableInt64,
            SetWindowFieldsMethod.StandardDeviationSampleWithNullableSingle,
            SetWindowFieldsMethod.StandardDeviationSampleWithSingle,
            SetWindowFieldsMethod.SumWithDecimal,
            SetWindowFieldsMethod.SumWithDouble,
            SetWindowFieldsMethod.SumWithInt32,
            SetWindowFieldsMethod.SumWithInt64,
            SetWindowFieldsMethod.SumWithNullableDecimal,
            SetWindowFieldsMethod.SumWithNullableDouble,
            SetWindowFieldsMethod.SumWithNullableInt32,
            SetWindowFieldsMethod.SumWithNullableInt64,
            SetWindowFieldsMethod.SumWithNullableSingle,
            SetWindowFieldsMethod.SumWithSingle
        };

        public static bool CanTranslate(MethodCallExpression expression)
        {
            return CanTranslate(expression.Method);
        }

        public static bool CanTranslate(MethodInfo method)
        {
            return method.IsOneOf(__setWindowFieldsMethods);
        }

        public static AggregationExpression Translate(TranslationContext context, MethodCallExpression expression)
        {
            var method = expression.Method;
            var arguments = expression.Arguments;

            if (method.IsOneOf(__setWindowFieldsMethods))
            {
                var partitionExpression = arguments[0];
                var partitionTranslation = ExpressionToAggregationExpressionTranslator.TranslateEnumerable(context, partitionExpression);
                var partitionSerializer = (ISetWindowFieldsPartitionSerializer)partitionTranslation.Serializer;
                var inputSerializer = partitionSerializer.InputSerializer;

                var @operator = ToOperator(method);
                var operatorArgs = new List<AstExpression>();
                if (arguments.Count >= 3)
                {
                    for (var n = 1; n <= arguments.Count - 2; n++)
                    {
                        var argument = arguments[n];

                        if (argument is LambdaExpression selectorLambda)
                        {
                            var selectorParameter = selectorLambda.Parameters[0];
                            var selectorSymbol = context.CreateSymbol(selectorParameter, inputSerializer, isCurrent: true);
                            var selectorContext = context.WithSymbol(selectorSymbol);
                            var selectorTranslation = ExpressionToAggregationExpressionTranslator.Translate(selectorContext, selectorLambda.Body);
                            operatorArgs.Add(selectorTranslation.Ast);
                            continue;
                        }

                        if (argument is ConstantExpression constantExpression)
                        {
                            var value = constantExpression.GetConstantValue<object>(expression);

                            if (value is WindowTimeUnit unit)
                            {
                                var renderedUnit = unit.Render();
                                operatorArgs.Add(renderedUnit);
                                continue;
                            }

                            if (value is ExponentialMovingAverageAlphaWeighting alphaWeighting)
                            {
                                @operator = AstSetWindowFieldsOperator.ExpMovingAvgWithAlphaWeighting;
                                operatorArgs.Add(alphaWeighting.Alpha);
                                continue;
                            }

                            if (value is ExponentialMovingAveragePositionalWeighting positionalWeighting)
                            {
                                @operator = AstSetWindowFieldsOperator.ExpMovingAvgWithPositionalWeighting;
                                operatorArgs.Add(positionalWeighting.N);
                                continue;
                            }
                        }

                        throw new ExpressionNotSupportedException(argument, expression);
                    }
                }
                else
                {
                    operatorArgs.Add(AstExpression.Constant(new BsonDocument()));
                }

                var windowExpression = arguments.Last();
                var window = windowExpression.GetConstantValue<WindowBoundaries>(expression);
                var sortBy = context.Data.GetValueOrDefault("SortBy", null);
                var serializerRegistry = (BsonSerializerRegistry)context.Data.GetValueOrDefault("SerializerRegistry", null);

                var ast = AstExpression.SetWindowFieldsWindowExpression(
                    @operator,
                    operatorArgs,
                    ToAstWindow(window, sortBy, inputSerializer, serializerRegistry));
                var serializer = BsonSerializer.LookupSerializer(method.ReturnType);

                return new AggregationExpression(expression, ast, serializer);
            }

            throw new ExpressionNotSupportedException(expression);
        }

        private static AstSetWindowFieldsOperator ToOperator(MethodInfo method)
        {
            return method.Name switch
            {
                "AddToSet" => AstSetWindowFieldsOperator.AddToSet,
                "Average" => AstSetWindowFieldsOperator.Average,
                "Count" => AstSetWindowFieldsOperator.Count,
                "CovariancePopulation" => AstSetWindowFieldsOperator.CovariancePop,
                "CovarianceSample" => AstSetWindowFieldsOperator.CovarianceSamp,
                "Derivative" => AstSetWindowFieldsOperator.Derivative,
                "ExponentialMovingAverage" => AstSetWindowFieldsOperator.ExpMovingAvgPlaceholder, // will be replaced when weighting argument is processed
                "Integral" => AstSetWindowFieldsOperator.Integral,
                "Max" => AstSetWindowFieldsOperator.Max,
                "Min" => AstSetWindowFieldsOperator.Min,
                "Push" => AstSetWindowFieldsOperator.Push,
                "StandardDeviationPopulation" => AstSetWindowFieldsOperator.StdDevPop,
                "StandardDeviationSample" => AstSetWindowFieldsOperator.StdDevSamp,
                "Sum" => AstSetWindowFieldsOperator.Sum,
                _ => throw new ArgumentException($"Unsupported method: {method.Name}.")
            };
        }

        private static AstSetWindowFieldsWindow ToAstWindow(WindowBoundaries window, object sortBy, IBsonSerializer inputSerializer, BsonSerializerRegistry serializerRegistry)
        {
            if (window == null)
            {
                return null;
            }

            if (window is DocumentsWindowBoundaries documentsWindow)
            {
                var lowerBoundary = documentsWindow.LowerBoundary;
                var upperBoundary = documentsWindow.UpperBoundary;
                return new AstSetWindowFieldsWindow("documents", lowerBoundary.Render(), upperBoundary.Render(), unit: null);
            }

            if (window is RangeWindowBoundaries rangeWindow)
            {
                var lowerBoundary = rangeWindow.LowerBoundary;
                var upperBoundary = rangeWindow.UpperBoundary;
                var unit = (lowerBoundary as TimeRangeWindowBoundary)?.Unit ?? (upperBoundary as TimeRangeWindowBoundary)?.Unit;

                var lowerValueBoundary = lowerBoundary as ValueRangeWindowBoundary;
                var upperValueBoundary = upperBoundary as ValueRangeWindowBoundary;
                IBsonSerializer lowerBoundaryValueSerializer = null;
                IBsonSerializer upperBoundaryValueSerializer = null;
                if (lowerValueBoundary != null || upperBoundary != null)
                {
                    var sortBySerializer = GetSortBySerializer(sortBy, inputSerializer, serializerRegistry);
                    if (lowerValueBoundary != null)
                    {
                        lowerBoundaryValueSerializer = ValueRangeWindowBoundaryConvertingValueSerializerFactory.Create(lowerValueBoundary, sortBySerializer);
                    }
                    if (upperValueBoundary != null)
                    {
                        if (lowerBoundaryValueSerializer != null && lowerBoundaryValueSerializer.ValueType == upperValueBoundary.ValueType)
                        {
                            upperBoundaryValueSerializer = lowerBoundaryValueSerializer;
                        }
                        else
                        {
                            upperBoundaryValueSerializer = ValueRangeWindowBoundaryConvertingValueSerializerFactory.Create(upperValueBoundary, sortBySerializer);
                        }
                    }
                }

                return new AstSetWindowFieldsWindow("range", lowerBoundary.Render(lowerBoundaryValueSerializer), upperBoundary.Render(upperBoundaryValueSerializer), unit);
            }

            throw new ArgumentException($"Invalid window type: {window.GetType().FullName}.");
        }

        private static IBsonSerializer GetSortBySerializer(object sortBy, IBsonSerializer inputSerializer, BsonSerializerRegistry serializerRegistry)
        {
            var sortByType = sortBy.GetType();
            var documentType = sortByType.GetGenericArguments().Single();
            var methodInfoDefinition = typeof(SetWindowFieldsMethodMethodToAggregationExpressionTranslator).GetMethod(
                nameof(GetSortBySerializerGeneric),
                BindingFlags.NonPublic | BindingFlags.Static);
            var methodInfo = methodInfoDefinition.MakeGenericMethod(documentType);
            return (IBsonSerializer)methodInfo.Invoke(null, new object[] { sortBy, inputSerializer, serializerRegistry });
        }

        private static IBsonSerializer GetSortBySerializerGeneric<TDocument>(SortDefinition<TDocument> sortBy, IBsonSerializer<TDocument> documentSerializer, BsonSerializerRegistry serializerRegistry)
        {
            var directionalSortBy = sortBy as DirectionalSortDefinition<TDocument>;
            if (directionalSortBy == null)
            {
                throw new InvalidOperationException("SetWindowFields SortBy with range window must be on a single field.");
            }
            if (directionalSortBy.Direction != SortDirection.Ascending)
            {
                throw new InvalidOperationException("SetWindowFields SortBy with range window must be ascending.");
            }

            var field = directionalSortBy.Field;
            var renderedField = field.Render(documentSerializer, serializerRegistry);

            return renderedField.FieldSerializer;
        }
    }
}
