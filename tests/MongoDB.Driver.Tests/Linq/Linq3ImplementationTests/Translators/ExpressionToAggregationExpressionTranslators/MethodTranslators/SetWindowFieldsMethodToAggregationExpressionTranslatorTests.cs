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

using MongoDB.Driver.Linq;
using Xunit;

namespace MongoDB.Driver.Tests.Linq.Linq3ImplementationTests.Translators.ExpressionToAggregationExpressionTranslators.MethodTranslators
{
    public class SetWindowFieldsMethodToAggregationExpressionTranslatorTests : Linq3IntegrationTest
    {
        [Fact]
        public void Translate_should_return_expected_result_for_AddToSet()
        {
            var collection = GetCollection<C>();

            var aggregate = collection.Aggregate()
                .SetWindowFields(output: p => new { Result = p.AddToSet(x => x.Int32Field, null) });

            var stages = Translate(collection, aggregate);
            var expectedStages = new[] { "{ $setWindowFields : { output : { Result : { $addToSet : '$Int32Field' } } } }" };
            AssertStages(stages, expectedStages);
        }

        [Fact]
        public void Translate_should_return_expected_result_for_Average_with_Decimal()
        {
            var collection = GetCollection<C>();

            var aggregate = collection.Aggregate()
                .SetWindowFields(output: p => new { Result = p.Average(x => x.DecimalField, null) });

            var stages = Translate(collection, aggregate);
            var expectedStages = new[] { "{ $setWindowFields : { output : { Result : { $avg : '$DecimalField' } } } }" };
            AssertStages(stages, expectedStages);
        }

        [Fact]
        public void Translate_should_return_expected_result_for_Average_with_Double()
        {
            var collection = GetCollection<C>();

            var aggregate = collection.Aggregate()
                .SetWindowFields(output: p => new { Result = p.Average(x => x.DoubleField, null) });

            var stages = Translate(collection, aggregate);
            var expectedStages = new[] { "{ $setWindowFields : { output : { Result : { $avg : '$DoubleField' } } } }" };
            AssertStages(stages, expectedStages);
        }

        [Fact]
        public void Translate_should_return_expected_result_for_Average_with_Int32()
        {
            var collection = GetCollection<C>();

            var aggregate = collection.Aggregate()
                .SetWindowFields(output: p => new { Result = p.Average(x => x.Int32Field, null) });

            var stages = Translate(collection, aggregate);
            var expectedStages = new[] { "{ $setWindowFields : { output : { Result : { $avg : '$Int32Field' } } } }" };
            AssertStages(stages, expectedStages);
        }

        [Fact]
        public void Translate_should_return_expected_result_for_Average_with_Int64()
        {
            var collection = GetCollection<C>();

            var aggregate = collection.Aggregate()
                .SetWindowFields(output: p => new { Result = p.Average(x => x.Int64Field, null) });

            var stages = Translate(collection, aggregate);
            var expectedStages = new[] { "{ $setWindowFields : { output : { Result : { $avg : '$Int64Field' } } } }" };
            AssertStages(stages, expectedStages);
        }

        [Fact]
        public void Translate_should_return_expected_result_for_Average_with_nullable_Decimal()
        {
            var collection = GetCollection<C>();

            var aggregate = collection.Aggregate()
                .SetWindowFields(output: p => new { Result = p.Average(x => x.NullableDecimalField, null) });

            var stages = Translate(collection, aggregate);
            var expectedStages = new[] { "{ $setWindowFields : { output : { Result : { $avg : '$NullableDecimalField' } } } }" };
            AssertStages(stages, expectedStages);
        }

        [Fact]
        public void Translate_should_return_expected_result_for_Average_with_nullable_Double()
        {
            var collection = GetCollection<C>();

            var aggregate = collection.Aggregate()
                .SetWindowFields(output: p => new { Result = p.Average(x => x.NullableDoubleField, null) });

            var stages = Translate(collection, aggregate);
            var expectedStages = new[] { "{ $setWindowFields : { output : { Result : { $avg : '$NullableDoubleField' } } } }" };
            AssertStages(stages, expectedStages);
        }

        [Fact]
        public void Translate_should_return_expected_result_for_Average_with_nullable_Int32()
        {
            var collection = GetCollection<C>();

            var aggregate = collection.Aggregate()
                .SetWindowFields(output: p => new { Result = p.Average(x => x.NullableInt32Field, null) });

            var stages = Translate(collection, aggregate);
            var expectedStages = new[] { "{ $setWindowFields : { output : { Result : { $avg : '$NullableInt32Field' } } } }" };
            AssertStages(stages, expectedStages);
        }

        [Fact]
        public void Translate_should_return_expected_result_for_Average_with_nullable_Int64()
        {
            var collection = GetCollection<C>();

            var aggregate = collection.Aggregate()
                .SetWindowFields(output: p => new { Result = p.Average(x => x.NullableInt64Field, null) });

            var stages = Translate(collection, aggregate);
            var expectedStages = new[] { "{ $setWindowFields : { output : { Result : { $avg : '$NullableInt64Field' } } } }" };
            AssertStages(stages, expectedStages);
        }

        [Fact]
        public void Translate_should_return_expected_result_for_Average_with_nullable_Single()
        {
            var collection = GetCollection<C>();

            var aggregate = collection.Aggregate()
                .SetWindowFields(output: p => new { Result = p.Average(x => x.NullableSingleField, null) });

            var stages = Translate(collection, aggregate);
            var expectedStages = new[] { "{ $setWindowFields : { output : { Result : { $avg : '$NullableSingleField' } } } }" };
            AssertStages(stages, expectedStages);
        }

        [Fact]
        public void Translate_should_return_expected_result_for_Average_with_Single()
        {
            var collection = GetCollection<C>();

            var aggregate = collection.Aggregate()
                .SetWindowFields(output: p => new { Result = p.Average(x => x.SingleField, null) });

            var stages = Translate(collection, aggregate);
            var expectedStages = new[] { "{ $setWindowFields : { output : { Result : { $avg : '$SingleField' } } } }" };
            AssertStages(stages, expectedStages);
        }

        [Fact]
        public void Translate_should_return_expected_result_for_Count()
        {
            var collection = GetCollection<C>();

            var aggregate = collection.Aggregate()
                .SetWindowFields(output: p => new { Result = p.Count(null) });

            var stages = Translate(collection, aggregate);
            var expectedStages = new[] { "{ $setWindowFields : { output : { Result : { $count : { } } } } }" };
            AssertStages(stages, expectedStages);
        }

        [Fact]
        public void Translate_should_return_expected_result_for_Sum_with_Decimal()
        {
            var collection = GetCollection<C>();

            var aggregate = collection.Aggregate()
                .SetWindowFields(output: p => new { Result = p.Sum(x => x.DecimalField, null) });

            var stages = Translate(collection, aggregate);
            var expectedStages = new[] { "{ $setWindowFields : { output : { Result : { $sum : '$DecimalField' } } } }" };
            AssertStages(stages, expectedStages);
        }

        [Fact]
        public void Translate_should_return_expected_result_for_Sum_with_Double()
        {
            var collection = GetCollection<C>();

            var aggregate = collection.Aggregate()
                .SetWindowFields(output: p => new { Result = p.Sum(x => x.DoubleField, null) });

            var stages = Translate(collection, aggregate);
            var expectedStages = new[] { "{ $setWindowFields : { output : { Result : { $sum : '$DoubleField' } } } }" };
            AssertStages(stages, expectedStages);
        }

        [Fact]
        public void Translate_should_return_expected_result_for_Sum_with_Int32()
        {
            var collection = GetCollection<C>();

            var aggregate = collection.Aggregate()
                .SetWindowFields(output: p => new { Result = p.Sum(x => x.Int32Field, null) });

            var stages = Translate(collection, aggregate);
            var expectedStages = new[] { "{ $setWindowFields : { output : { Result : { $sum : '$Int32Field' } } } }" };
            AssertStages(stages, expectedStages);
        }

        [Fact]
        public void Translate_should_return_expected_result_for_Sum_with_Int64()
        {
            var collection = GetCollection<C>();

            var aggregate = collection.Aggregate()
                .SetWindowFields(output: p => new { Result = p.Sum(x => x.Int64Field, null) });

            var stages = Translate(collection, aggregate);
            var expectedStages = new[] { "{ $setWindowFields : { output : { Result : { $sum : '$Int64Field' } } } }" };
            AssertStages(stages, expectedStages);
        }

        [Fact]
        public void Translate_should_return_expected_result_for_Sum_with_nullable_Decimal()
        {
            var collection = GetCollection<C>();

            var aggregate = collection.Aggregate()
                .SetWindowFields(output: p => new { Result = p.Sum(x => x.NullableDecimalField, null) });

            var stages = Translate(collection, aggregate);
            var expectedStages = new[] { "{ $setWindowFields : { output : { Result : { $sum : '$NullableDecimalField' } } } }" };
            AssertStages(stages, expectedStages);
        }

        [Fact]
        public void Translate_should_return_expected_result_for_Sum_with_nullable_Double()
        {
            var collection = GetCollection<C>();

            var aggregate = collection.Aggregate()
                .SetWindowFields(output: p => new { Result = p.Sum(x => x.NullableDoubleField, null) });

            var stages = Translate(collection, aggregate);
            var expectedStages = new[] { "{ $setWindowFields : { output : { Result : { $sum : '$NullableDoubleField' } } } }" };
            AssertStages(stages, expectedStages);
        }

        [Fact]
        public void Translate_should_return_expected_result_for_Sum_with_nullable_Int32()
        {
            var collection = GetCollection<C>();

            var aggregate = collection.Aggregate()
                .SetWindowFields(output: p => new { Result = p.Sum(x => x.NullableInt32Field, null) });

            var stages = Translate(collection, aggregate);
            var expectedStages = new[] { "{ $setWindowFields : { output : { Result : { $sum : '$NullableInt32Field' } } } }" };
            AssertStages(stages, expectedStages);
        }

        [Fact]
        public void Translate_should_return_expected_result_for_Sum_with_nullable_Int64()
        {
            var collection = GetCollection<C>();

            var aggregate = collection.Aggregate()
                .SetWindowFields(output: p => new { Result = p.Sum(x => x.NullableInt64Field, null) });

            var stages = Translate(collection, aggregate);
            var expectedStages = new[] { "{ $setWindowFields : { output : { Result : { $sum : '$NullableInt64Field' } } } }" };
            AssertStages(stages, expectedStages);
        }

        [Fact]
        public void Translate_should_return_expected_result_for_Sum_with_nullable_Single()
        {
            var collection = GetCollection<C>();

            var aggregate = collection.Aggregate()
                .SetWindowFields(output: p => new { Result = p.Sum(x => x.NullableSingleField, null) });

            var stages = Translate(collection, aggregate);
            var expectedStages = new[] { "{ $setWindowFields : { output : { Result : { $sum : '$NullableSingleField' } } } }" };
            AssertStages(stages, expectedStages);
        }

        [Fact]
        public void Translate_should_return_expected_result_for_Sum_with_Single()
        {
            var collection = GetCollection<C>();

            var aggregate = collection.Aggregate()
                .SetWindowFields(output: p => new { Result = p.Sum(x => x.SingleField, null) });

            var stages = Translate(collection, aggregate);
            var expectedStages = new[] { "{ $setWindowFields : { output : { Result : { $sum : '$SingleField' } } } }" };
            AssertStages(stages, expectedStages);
        }

        public class C
        {
            public int X { get; set; }
            public int Y { get; set; }
            public decimal DecimalField { get; set; }
            public double DoubleField { get; set; }
            public int Int32Field { get; set; }
            public long Int64Field { get; set; }
            public decimal? NullableDecimalField { get; set; }
            public double? NullableDoubleField { get; set; }
            public int? NullableInt32Field { get; set; }
            public long? NullableInt64Field { get; set; }
            public float? NullableSingleField { get; set; }
            public float SingleField { get; set; }
        }
    }
}
