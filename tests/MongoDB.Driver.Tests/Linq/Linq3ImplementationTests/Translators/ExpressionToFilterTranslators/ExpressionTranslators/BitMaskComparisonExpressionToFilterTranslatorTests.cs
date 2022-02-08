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
using System.Linq;
using System.Linq.Expressions;
using FluentAssertions;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Driver.Linq.Linq3Implementation.Ast.Filters;
using MongoDB.Driver.Linq.Linq3Implementation.Translators;
using MongoDB.Driver.Linq.Linq3Implementation.Translators.ExpressionToFilterTranslators.ExpressionTranslators;
using Xunit;

namespace MongoDB.Driver.Tests.Linq.Linq3ImplementationTests.Translators.ExpressionToFilterTranslators.ExpressionTranslators
{
    public class BitMaskComparisonExpressionToFilterTranslatorTests
    {
        [Fact]
        public void Translate_should_return_expected_result_with_integer_arguments()
        {
            var (parameter, expression) = CreateExpression((C c) => (c.Integer & 3) == 0);
            var context = CreateContext(parameter);
            var canTranslate = BitMaskComparisonExpressionToFilterTranslator.CanTranslate(expression.Left);
            canTranslate.Should().BeTrue();

            var result = BitMaskComparisonExpressionToFilterTranslator.Translate(context, expression, expression.Left, AstComparisonFilterOperator.Eq, expression.Right);

            AssertAllBitsClearFilterOperation(result, "Integer", 3);
        }

        [Fact]
        public void Translate_should_return_expected_result_with_enum_arguments()
        {
            var (parameter, expression) = CreateExpression((C c) => (c.Bits & Bit.B3) == 0);
            var context = CreateContext(parameter);
            var canTranslate = BitMaskComparisonExpressionToFilterTranslator.CanTranslate(expression.Left);
            canTranslate.Should().BeTrue();

            var result = BitMaskComparisonExpressionToFilterTranslator.Translate(context, expression, expression.Left, AstComparisonFilterOperator.Eq, expression.Right);

            AssertAllBitsClearFilterOperation(result, "Bits", 8);
        }

        private void AssertAllBitsClearFilterOperation(AstFilter result, string path, BsonValue bits)
        {
            var fieldOperationFilter = result.Should().BeOfType<AstFieldOperationFilter>().Subject;
            fieldOperationFilter.Field.Path.Should().Be(path);
            var operation = fieldOperationFilter.Operation.Should().BeOfType<AstBitsAllClearFilterOperation>().Subject;
            operation.Bitmask.Should().Be(bits);
        }

        private TranslationContext CreateContext(ParameterExpression parameter)
        {
            var serializer = BsonSerializer.LookupSerializer(parameter.Type);
            var context = TranslationContext.Create(parameter, serializer);
            var symbol = context.CreateSymbol(parameter, serializer, isCurrent: true);
            return context.WithSymbol(symbol);
        }

        private (ParameterExpression, BinaryExpression) CreateExpression<TField>(Expression<Func<TField, bool>> lambda)
        {
            var parameter = lambda.Parameters.Single();
            var expression = (BinaryExpression)lambda.Body;
            return (parameter, expression);
        }

        private enum Bit
        {
            B0 = 1<<0,
            B1 = 1<<1,
            B2 = 1<<2,
            B3 = 1<<3,
            B4 = 1<<4,
        }
        private class C
        {
            public Bit Bits { get; set; }
            public int Integer { get; set; }
        }
    }
}
