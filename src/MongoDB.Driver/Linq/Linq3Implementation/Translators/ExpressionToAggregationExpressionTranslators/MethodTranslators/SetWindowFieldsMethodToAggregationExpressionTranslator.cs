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
using System.Linq.Expressions;
using System.Reflection;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;
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
            SetWindowFieldsMethod.Average,
            SetWindowFieldsMethod.Max,
            SetWindowFieldsMethod.Min,
            SetWindowFieldsMethod.Sum
        };

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

                var selectorLambda = (LambdaExpression)arguments[1];
                var selectorParameter = selectorLambda.Parameters[0];
                var selectorSymbol = context.CreateSymbol(selectorParameter, inputSerializer, isCurrent: true);
                var selectorContext = context.WithSymbol(selectorSymbol);
                var selectorTranslation = ExpressionToAggregationExpressionTranslator.Translate(selectorContext, selectorLambda.Body);

                var windowExpression = arguments[2];
                var window = windowExpression.GetConstantValue<WindowBoundaries>(expression);

                var ast = AstExpression.SetWindowFieldsWindowExpression(
                    ToOperator(method),
                    new AstExpression[] { selectorTranslation.Ast },
                    ToAstWindow(window));
                var serializer = BsonSerializer.LookupSerializer(method.ReturnType);

                return new AggregationExpression(expression, ast, serializer);
            }

            throw new ExpressionNotSupportedException(expression);
        }

        private static AstSetWindowFieldsOperator ToOperator(MethodInfo method)
        {
            return method.Name switch
            {
                "Average" => AstSetWindowFieldsOperator.Average,
                "Max" => AstSetWindowFieldsOperator.Max,
                "Min" => AstSetWindowFieldsOperator.Min,
                "Sum" => AstSetWindowFieldsOperator.Sum,
                _ => throw new ArgumentException($"Unsupported method: {method.Name}.")
            };
        }

        private static AstSetWindowFieldsWindow ToAstWindow(WindowBoundaries window)
        {
            if (window is DocumentsWindowBoundaries documentsWindow)
            {
                var lowerBoundary = documentsWindow.LowerBoundary;
                var upperBoundary = documentsWindow.UpperBoundary;
                return new AstSetWindowFieldsWindow("documents", lowerBoundary.Render(), upperBoundary.Render(), unit: null);
            }

            if (window is RangeWindowBoundaries rangeWindow)
            {
                //var lowerBoundary = rangeWindow.LowerBoundary;
                //var upperBoundary = rangeWindow.UpperBoundary;
                //var unit = (lowerBoundary as TimeRangeWindowBoundary)?.Unit ?? (upperBoundary as TimeRangeWindowBoundary)?.Unit;
                //return new AstSetWindowFieldsWindow("range", lowerBoundary.Render(), upperBoundary.Render(), unit);
                throw new NotImplementedException();
            }

            throw new ArgumentException($"Invalid window type: {window.GetType().FullName}.");
        }
    }
}
