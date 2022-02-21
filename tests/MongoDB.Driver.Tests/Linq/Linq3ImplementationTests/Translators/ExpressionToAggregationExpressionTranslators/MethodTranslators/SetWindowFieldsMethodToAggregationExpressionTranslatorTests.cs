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
        public void Translate_should_return_expected_result_for_CovariancePopulation_with_Decimal()
        {
            var collection = GetCollection<C>();

            var aggregate = collection.Aggregate()
                .SetWindowFields(output: p => new { Result = p.CovariancePopulation(x => x.DecimalField1, x => x.DecimalField2, null) });

            var stages = Translate(collection, aggregate);
            var expectedStages = new[] { "{ $setWindowFields : { output : { Result : { $covariancePop : ['$DecimalField1', '$DecimalField2'] } } } }" };
            AssertStages(stages, expectedStages);
        }

        [Fact]
        public void Translate_should_return_expected_result_for_CovariancePopulation_with_Double()
        {
            var collection = GetCollection<C>();

            var aggregate = collection.Aggregate()
                .SetWindowFields(output: p => new { Result = p.CovariancePopulation(x => x.DoubleField1, x => x.DoubleField2, null) });

            var stages = Translate(collection, aggregate);
            var expectedStages = new[] { "{ $setWindowFields : { output : { Result : { $covariancePop : ['$DoubleField1', '$DoubleField2'] } } } }" };
            AssertStages(stages, expectedStages);
        }

        [Fact]
        public void Translate_should_return_expected_result_for_CovariancePopulation_with_Int32()
        {
            var collection = GetCollection<C>();

            var aggregate = collection.Aggregate()
                .SetWindowFields(output: p => new { Result = p.CovariancePopulation(x => x.Int32Field1, x => x.Int32Field2, null) });

            var stages = Translate(collection, aggregate);
            var expectedStages = new[] { "{ $setWindowFields : { output : { Result : { $covariancePop : ['$Int32Field1', '$Int32Field2'] } } } }" };
            AssertStages(stages, expectedStages);
        }

        [Fact]
        public void Translate_should_return_expected_result_for_CovariancePopulation_with_Int64()
        {
            var collection = GetCollection<C>();

            var aggregate = collection.Aggregate()
                .SetWindowFields(output: p => new { Result = p.CovariancePopulation(x => x.Int64Field1, x => x.Int64Field2, null) });

            var stages = Translate(collection, aggregate);
            var expectedStages = new[] { "{ $setWindowFields : { output : { Result : { $covariancePop : ['$Int64Field1', '$Int64Field2'] } } } }" };
            AssertStages(stages, expectedStages);
        }

        [Fact]
        public void Translate_should_return_expected_result_for_CovariancePopulation_with_nullable_Decimal()
        {
            var collection = GetCollection<C>();

            var aggregate = collection.Aggregate()
                .SetWindowFields(output: p => new { Result = p.CovariancePopulation(x => x.NullableDecimalField1, x => x.NullableDecimalField2, null) });

            var stages = Translate(collection, aggregate);
            var expectedStages = new[] { "{ $setWindowFields : { output : { Result : { $covariancePop : ['$NullableDecimalField1', '$NullableDecimalField2'] } } } }" };
            AssertStages(stages, expectedStages);
        }

        [Fact]
        public void Translate_should_return_expected_result_for_CovariancePopulation_with_nullable_Double()
        {
            var collection = GetCollection<C>();

            var aggregate = collection.Aggregate()
                .SetWindowFields(output: p => new { Result = p.CovariancePopulation(x => x.NullableDoubleField1, x => x.NullableDoubleField2, null) });

            var stages = Translate(collection, aggregate);
            var expectedStages = new[] { "{ $setWindowFields : { output : { Result : { $covariancePop : ['$NullableDoubleField1', '$NullableDoubleField2'] } } } }" };
            AssertStages(stages, expectedStages);
        }

        [Fact]
        public void Translate_should_return_expected_result_for_CovariancePopulation_with_nullable_Int32()
        {
            var collection = GetCollection<C>();

            var aggregate = collection.Aggregate()
                .SetWindowFields(output: p => new { Result = p.CovariancePopulation(x => x.NullableInt32Field1, x => x.NullableInt32Field2, null) });

            var stages = Translate(collection, aggregate);
            var expectedStages = new[] { "{ $setWindowFields : { output : { Result : { $covariancePop : ['$NullableInt32Field1', '$NullableInt32Field2'] } } } }" };
            AssertStages(stages, expectedStages);
        }

        [Fact]
        public void Translate_should_return_expected_result_for_CovariancePopulation_with_nullable_Int64()
        {
            var collection = GetCollection<C>();

            var aggregate = collection.Aggregate()
                .SetWindowFields(output: p => new { Result = p.CovariancePopulation(x => x.NullableInt64Field1, x => x.NullableInt64Field2, null) });

            var stages = Translate(collection, aggregate);
            var expectedStages = new[] { "{ $setWindowFields : { output : { Result : { $covariancePop : ['$NullableInt64Field1', '$NullableInt64Field2'] } } } }" };
            AssertStages(stages, expectedStages);
        }

        [Fact]
        public void Translate_should_return_expected_result_for_CovariancePopulation_with_nullable_Single()
        {
            var collection = GetCollection<C>();

            var aggregate = collection.Aggregate()
                .SetWindowFields(output: p => new { Result = p.CovariancePopulation(x => x.NullableSingleField1, x => x.NullableSingleField2, null) });

            var stages = Translate(collection, aggregate);
            var expectedStages = new[] { "{ $setWindowFields : { output : { Result : { $covariancePop : ['$NullableSingleField1', '$NullableSingleField2'] } } } }" };
            AssertStages(stages, expectedStages);
        }

        [Fact]
        public void Translate_should_return_expected_result_for_CovariancePopulation_with_Single()
        {
            var collection = GetCollection<C>();

            var aggregate = collection.Aggregate()
                .SetWindowFields(output: p => new { Result = p.CovariancePopulation(x => x.SingleField1, x => x.SingleField2, null) });

            var stages = Translate(collection, aggregate);
            var expectedStages = new[] { "{ $setWindowFields : { output : { Result : { $covariancePop : ['$SingleField1', '$SingleField2'] } } } }" };
            AssertStages(stages, expectedStages);
        }

        [Fact]
        public void Translate_should_return_expected_result_for_CovarianceSample_with_Decimal()
        {
            var collection = GetCollection<C>();

            var aggregate = collection.Aggregate()
                .SetWindowFields(output: p => new { Result = p.CovarianceSample(x => x.DecimalField1, x => x.DecimalField2, null) });

            var stages = Translate(collection, aggregate);
            var expectedStages = new[] { "{ $setWindowFields : { output : { Result : { $covarianceSamp : ['$DecimalField1', '$DecimalField2'] } } } }" };
            AssertStages(stages, expectedStages);
        }

        [Fact]
        public void Translate_should_return_expected_result_for_CovarianceSample_with_Double()
        {
            var collection = GetCollection<C>();

            var aggregate = collection.Aggregate()
                .SetWindowFields(output: p => new { Result = p.CovarianceSample(x => x.DoubleField1, x => x.DoubleField2, null) });

            var stages = Translate(collection, aggregate);
            var expectedStages = new[] { "{ $setWindowFields : { output : { Result : { $covarianceSamp : ['$DoubleField1', '$DoubleField2'] } } } }" };
            AssertStages(stages, expectedStages);
        }

        [Fact]
        public void Translate_should_return_expected_result_for_CovarianceSample_with_Int32()
        {
            var collection = GetCollection<C>();

            var aggregate = collection.Aggregate()
                .SetWindowFields(output: p => new { Result = p.CovarianceSample(x => x.Int32Field1, x => x.Int32Field2, null) });

            var stages = Translate(collection, aggregate);
            var expectedStages = new[] { "{ $setWindowFields : { output : { Result : { $covarianceSamp : ['$Int32Field1', '$Int32Field2'] } } } }" };
            AssertStages(stages, expectedStages);
        }

        [Fact]
        public void Translate_should_return_expected_result_for_CovarianceSample_with_Int64()
        {
            var collection = GetCollection<C>();

            var aggregate = collection.Aggregate()
                .SetWindowFields(output: p => new { Result = p.CovarianceSample(x => x.Int64Field1, x => x.Int64Field2, null) });

            var stages = Translate(collection, aggregate);
            var expectedStages = new[] { "{ $setWindowFields : { output : { Result : { $covarianceSamp : ['$Int64Field1', '$Int64Field2'] } } } }" };
            AssertStages(stages, expectedStages);
        }

        [Fact]
        public void Translate_should_return_expected_result_for_CovarianceSample_with_nullable_Decimal()
        {
            var collection = GetCollection<C>();

            var aggregate = collection.Aggregate()
                .SetWindowFields(output: p => new { Result = p.CovarianceSample(x => x.NullableDecimalField1, x => x.NullableDecimalField2, null) });

            var stages = Translate(collection, aggregate);
            var expectedStages = new[] { "{ $setWindowFields : { output : { Result : { $covarianceSamp : ['$NullableDecimalField1', '$NullableDecimalField2'] } } } }" };
            AssertStages(stages, expectedStages);
        }

        [Fact]
        public void Translate_should_return_expected_result_for_CovarianceSample_with_nullable_Double()
        {
            var collection = GetCollection<C>();

            var aggregate = collection.Aggregate()
                .SetWindowFields(output: p => new { Result = p.CovarianceSample(x => x.NullableDoubleField1, x => x.NullableDoubleField2, null) });

            var stages = Translate(collection, aggregate);
            var expectedStages = new[] { "{ $setWindowFields : { output : { Result : { $covarianceSamp : ['$NullableDoubleField1', '$NullableDoubleField2'] } } } }" };
            AssertStages(stages, expectedStages);
        }

        [Fact]
        public void Translate_should_return_expected_result_for_CovarianceSample_with_nullable_Int32()
        {
            var collection = GetCollection<C>();

            var aggregate = collection.Aggregate()
                .SetWindowFields(output: p => new { Result = p.CovarianceSample(x => x.NullableInt32Field1, x => x.NullableInt32Field2, null) });

            var stages = Translate(collection, aggregate);
            var expectedStages = new[] { "{ $setWindowFields : { output : { Result : { $covarianceSamp : ['$NullableInt32Field1', '$NullableInt32Field2'] } } } }" };
            AssertStages(stages, expectedStages);
        }

        [Fact]
        public void Translate_should_return_expected_result_for_CovarianceSample_with_nullable_Int64()
        {
            var collection = GetCollection<C>();

            var aggregate = collection.Aggregate()
                .SetWindowFields(output: p => new { Result = p.CovarianceSample(x => x.NullableInt64Field1, x => x.NullableInt64Field2, null) });

            var stages = Translate(collection, aggregate);
            var expectedStages = new[] { "{ $setWindowFields : { output : { Result : { $covarianceSamp : ['$NullableInt64Field1', '$NullableInt64Field2'] } } } }" };
            AssertStages(stages, expectedStages);
        }

        [Fact]
        public void Translate_should_return_expected_result_for_CovarianceSample_with_nullable_Single()
        {
            var collection = GetCollection<C>();

            var aggregate = collection.Aggregate()
                .SetWindowFields(output: p => new { Result = p.CovarianceSample(x => x.NullableSingleField1, x => x.NullableSingleField2, null) });

            var stages = Translate(collection, aggregate);
            var expectedStages = new[] { "{ $setWindowFields : { output : { Result : { $covarianceSamp : ['$NullableSingleField1', '$NullableSingleField2'] } } } }" };
            AssertStages(stages, expectedStages);
        }

        [Fact]
        public void Translate_should_return_expected_result_for_CovarianceSample_with_Single()
        {
            var collection = GetCollection<C>();

            var aggregate = collection.Aggregate()
                .SetWindowFields(output: p => new { Result = p.CovarianceSample(x => x.SingleField1, x => x.SingleField2, null) });

            var stages = Translate(collection, aggregate);
            var expectedStages = new[] { "{ $setWindowFields : { output : { Result : { $covarianceSamp : ['$SingleField1', '$SingleField2'] } } } }" };
            AssertStages(stages, expectedStages);
        }

        [Fact]
        public void Translate_should_return_expected_result_for_DenseRank()
        {
            var collection = GetCollection<C>();

            var aggregate = collection.Aggregate()
                .SetWindowFields(output: p => new { Result = p.DenseRank() });

            var stages = Translate(collection, aggregate);
            var expectedStages = new[] { "{ $setWindowFields : { output : { Result : { $denseRank : { } } } } }" };
            AssertStages(stages, expectedStages);
        }

        [Fact]
        public void Translate_should_return_expected_result_for_Derivative_with_Decimal()
        {
            var collection = GetCollection<C>();

            var aggregate = collection.Aggregate()
                .SetWindowFields(output: p => new { Result = p.Derivative(x => x.DecimalField, null) });

            var stages = Translate(collection, aggregate);
            var expectedStages = new[] { "{ $setWindowFields : { output : { Result : { $derivative : { input : '$DecimalField' } } } } }" };
            AssertStages(stages, expectedStages);
        }

        [Fact]
        public void Translate_should_return_expected_result_for_Derivative_with_Decimal_and_unit()
        {
            var collection = GetCollection<C>();

            var aggregate = collection.Aggregate()
                .SetWindowFields(output: p => new { Result = p.Derivative(x => x.DecimalField, WindowTimeUnit.Day, null) });

            var stages = Translate(collection, aggregate);
            var expectedStages = new[] { "{ $setWindowFields : { output : { Result : { $derivative : { input : '$DecimalField', unit : 'day' } } } } }" };
            AssertStages(stages, expectedStages);
        }

        [Fact]
        public void Translate_should_return_expected_result_for_Derivative_with_Double()
        {
            var collection = GetCollection<C>();

            var aggregate = collection.Aggregate()
                .SetWindowFields(output: p => new { Result = p.Derivative(x => x.DoubleField, null) });

            var stages = Translate(collection, aggregate);
            var expectedStages = new[] { "{ $setWindowFields : { output : { Result : { $derivative : { input : '$DoubleField' } } } } }" };
            AssertStages(stages, expectedStages);
        }

        [Fact]
        public void Translate_should_return_expected_result_for_Derivative_with_Double_and_unit()
        {
            var collection = GetCollection<C>();

            var aggregate = collection.Aggregate()
                .SetWindowFields(output: p => new { Result = p.Derivative(x => x.DoubleField, WindowTimeUnit.Day, null) });

            var stages = Translate(collection, aggregate);
            var expectedStages = new[] { "{ $setWindowFields : { output : { Result : { $derivative : { input : '$DoubleField', unit : 'day' } } } } }" };
            AssertStages(stages, expectedStages);
        }

        [Fact]
        public void Translate_should_return_expected_result_for_Derivative_with_Int32()
        {
            var collection = GetCollection<C>();

            var aggregate = collection.Aggregate()
                .SetWindowFields(output: p => new { Result = p.Derivative(x => x.Int32Field, null) });

            var stages = Translate(collection, aggregate);
            var expectedStages = new[] { "{ $setWindowFields : { output : { Result : { $derivative : { input : '$Int32Field' } } } } }" };
            AssertStages(stages, expectedStages);
        }

        [Fact]
        public void Translate_should_return_expected_result_for_Derivative_with_Int32_and_unit()
        {
            var collection = GetCollection<C>();

            var aggregate = collection.Aggregate()
                .SetWindowFields(output: p => new { Result = p.Derivative(x => x.Int32Field, WindowTimeUnit.Day, null) });

            var stages = Translate(collection, aggregate);
            var expectedStages = new[] { "{ $setWindowFields : { output : { Result : { $derivative : { input : '$Int32Field', unit : 'day' } } } } }" };
            AssertStages(stages, expectedStages);
        }

        [Fact]
        public void Translate_should_return_expected_result_for_Derivative_with_Int64()
        {
            var collection = GetCollection<C>();

            var aggregate = collection.Aggregate()
                .SetWindowFields(output: p => new { Result = p.Derivative(x => x.Int64Field, null) });

            var stages = Translate(collection, aggregate);
            var expectedStages = new[] { "{ $setWindowFields : { output : { Result : { $derivative : { input : '$Int64Field' } } } } }" };
            AssertStages(stages, expectedStages);
        }

        [Fact]
        public void Translate_should_return_expected_result_for_Derivative_with_Int64_and_unit()
        {
            var collection = GetCollection<C>();

            var aggregate = collection.Aggregate()
                .SetWindowFields(output: p => new { Result = p.Derivative(x => x.Int64Field, WindowTimeUnit.Day, null) });

            var stages = Translate(collection, aggregate);
            var expectedStages = new[] { "{ $setWindowFields : { output : { Result : { $derivative : { input : '$Int64Field', unit : 'day' } } } } }" };
            AssertStages(stages, expectedStages);
        }

        [Fact]
        public void Translate_should_return_expected_result_for_Derivative_with_Single()
        {
            var collection = GetCollection<C>();

            var aggregate = collection.Aggregate()
                .SetWindowFields(output: p => new { Result = p.Derivative(x => x.SingleField, null) });

            var stages = Translate(collection, aggregate);
            var expectedStages = new[] { "{ $setWindowFields : { output : { Result : { $derivative : { input : '$SingleField' } } } } }" };
            AssertStages(stages, expectedStages);
        }

        [Fact]
        public void Translate_should_return_expected_result_for_Derivative_with_Single_and_unit()
        {
            var collection = GetCollection<C>();

            var aggregate = collection.Aggregate()
                .SetWindowFields(output: p => new { Result = p.Derivative(x => x.SingleField, WindowTimeUnit.Day, null) });

            var stages = Translate(collection, aggregate);
            var expectedStages = new[] { "{ $setWindowFields : { output : { Result : { $derivative : { input : '$SingleField', unit : 'day' } } } } }" };
            AssertStages(stages, expectedStages);
        }

        [Fact]
        public void Translate_should_return_expected_result_for_ExponentialMovingAverage_with_Decimal_and_alpha()
        {
            var collection = GetCollection<C>();

            var aggregate = collection.Aggregate()
                .SetWindowFields(output: p => new { Result = p.ExponentialMovingAverage(x => x.DecimalField, ExponentialMovingAverageWeighting.Alpha(0.5), null) });

            var stages = Translate(collection, aggregate);
            var expectedStages = new[] { "{ $setWindowFields : { output : { Result : { $expMovingAvg : { input : '$DecimalField', alpha : 0.5 } } } } }" };
            AssertStages(stages, expectedStages);
        }

        [Fact]
        public void Translate_should_return_expected_result_for_ExponentialMovingAverage_with_Decimal_and_n()
        {
            var collection = GetCollection<C>();

            var aggregate = collection.Aggregate()
                .SetWindowFields(output: p => new { Result = p.ExponentialMovingAverage(x => x.DecimalField, ExponentialMovingAverageWeighting.N(2), null) });

            var stages = Translate(collection, aggregate);
            var expectedStages = new[] { "{ $setWindowFields : { output : { Result : { $expMovingAvg : { input : '$DecimalField', n : 2 } } } } }" };
            AssertStages(stages, expectedStages);
        }

        [Fact]
        public void Translate_should_return_expected_result_for_ExponentialMovingAverage_with_Double_and_alpha()
        {
            var collection = GetCollection<C>();

            var aggregate = collection.Aggregate()
                .SetWindowFields(output: p => new { Result = p.ExponentialMovingAverage(x => x.DoubleField, ExponentialMovingAverageWeighting.Alpha(0.5), null) });

            var stages = Translate(collection, aggregate);
            var expectedStages = new[] { "{ $setWindowFields : { output : { Result : { $expMovingAvg : { input : '$DoubleField', alpha : 0.5 } } } } }" };
            AssertStages(stages, expectedStages);
        }

        [Fact]
        public void Translate_should_return_expected_result_for_ExponentialMovingAverage_with_Double_and_n()
        {
            var collection = GetCollection<C>();

            var aggregate = collection.Aggregate()
                .SetWindowFields(output: p => new { Result = p.ExponentialMovingAverage(x => x.DoubleField, ExponentialMovingAverageWeighting.N(2), null) });

            var stages = Translate(collection, aggregate);
            var expectedStages = new[] { "{ $setWindowFields : { output : { Result : { $expMovingAvg : { input : '$DoubleField', n : 2 } } } } }" };
            AssertStages(stages, expectedStages);
        }

        [Fact]
        public void Translate_should_return_expected_result_for_ExponentialMovingAverage_with_Int32_and_alpha()
        {
            var collection = GetCollection<C>();

            var aggregate = collection.Aggregate()
                .SetWindowFields(output: p => new { Result = p.ExponentialMovingAverage(x => x.Int32Field, ExponentialMovingAverageWeighting.Alpha(0.5), null) });

            var stages = Translate(collection, aggregate);
            var expectedStages = new[] { "{ $setWindowFields : { output : { Result : { $expMovingAvg : { input : '$Int32Field', alpha : 0.5 } } } } }" };
            AssertStages(stages, expectedStages);
        }

        [Fact]
        public void Translate_should_return_expected_result_for_ExponentialMovingAverage_with_Int32_and_n()
        {
            var collection = GetCollection<C>();

            var aggregate = collection.Aggregate()
                .SetWindowFields(output: p => new { Result = p.ExponentialMovingAverage(x => x.Int32Field, ExponentialMovingAverageWeighting.N(2), null) });

            var stages = Translate(collection, aggregate);
            var expectedStages = new[] { "{ $setWindowFields : { output : { Result : { $expMovingAvg : { input : '$Int32Field', n : 2 } } } } }" };
            AssertStages(stages, expectedStages);
        }

        [Fact]
        public void Translate_should_return_expected_result_for_ExponentialMovingAverage_with_Int64_and_alpha()
        {
            var collection = GetCollection<C>();

            var aggregate = collection.Aggregate()
                .SetWindowFields(output: p => new { Result = p.ExponentialMovingAverage(x => x.Int64Field, ExponentialMovingAverageWeighting.Alpha(0.5), null) });

            var stages = Translate(collection, aggregate);
            var expectedStages = new[] { "{ $setWindowFields : { output : { Result : { $expMovingAvg : { input : '$Int64Field', alpha : 0.5 } } } } }" };
            AssertStages(stages, expectedStages);
        }

        [Fact]
        public void Translate_should_return_expected_result_for_ExponentialMovingAverage_with_Int64_and_n()
        {
            var collection = GetCollection<C>();

            var aggregate = collection.Aggregate()
                .SetWindowFields(output: p => new { Result = p.ExponentialMovingAverage(x => x.Int64Field, ExponentialMovingAverageWeighting.N(2), null) });

            var stages = Translate(collection, aggregate);
            var expectedStages = new[] { "{ $setWindowFields : { output : { Result : { $expMovingAvg : { input : '$Int64Field', n : 2 } } } } }" };
            AssertStages(stages, expectedStages);
        }

        [Fact]
        public void Translate_should_return_expected_result_for_ExponentialMovingAverage_with_Single_and_alpha()
        {
            var collection = GetCollection<C>();

            var aggregate = collection.Aggregate()
                .SetWindowFields(output: p => new { Result = p.ExponentialMovingAverage(x => x.SingleField, ExponentialMovingAverageWeighting.Alpha(0.5), null) });

            var stages = Translate(collection, aggregate);
            var expectedStages = new[] { "{ $setWindowFields : { output : { Result : { $expMovingAvg : { input : '$SingleField', alpha : 0.5 } } } } }" };
            AssertStages(stages, expectedStages);
        }

        [Fact]
        public void Translate_should_return_expected_result_for_ExponentialMovingAverage_with_Single_and_n()
        {
            var collection = GetCollection<C>();

            var aggregate = collection.Aggregate()
                .SetWindowFields(output: p => new { Result = p.ExponentialMovingAverage(x => x.SingleField, ExponentialMovingAverageWeighting.N(2), null) });

            var stages = Translate(collection, aggregate);
            var expectedStages = new[] { "{ $setWindowFields : { output : { Result : { $expMovingAvg : { input : '$SingleField', n : 2 } } } } }" };
            AssertStages(stages, expectedStages);
        }

        [Fact]
        public void Translate_should_return_expected_result_for_First()
        {
            var collection = GetCollection<C>();

            var aggregate = collection.Aggregate()
                .SetWindowFields(output: p => new { Result = p.First(x => x.Int32Field, null) });

            var stages = Translate(collection, aggregate);
            var expectedStages = new[] { "{ $setWindowFields : { output : { Result : { $first : '$Int32Field' } } } }" };
            AssertStages(stages, expectedStages);
        }

        [Fact]
        public void Translate_should_return_expected_result_for_Integral_with_Decimal()
        {
            var collection = GetCollection<C>();

            var aggregate = collection.Aggregate()
                .SetWindowFields(output: p => new { Result = p.Integral(x => x.DecimalField, null) });

            var stages = Translate(collection, aggregate);
            var expectedStages = new[] { "{ $setWindowFields : { output : { Result : { $integral : { input : '$DecimalField' } } } } }" };
            AssertStages(stages, expectedStages);
        }

        [Fact]
        public void Translate_should_return_expected_result_for_Integral_with_Decimal_and_unit()
        {
            var collection = GetCollection<C>();

            var aggregate = collection.Aggregate()
                .SetWindowFields(output: p => new { Result = p.Integral(x => x.DecimalField, WindowTimeUnit.Day, null) });

            var stages = Translate(collection, aggregate);
            var expectedStages = new[] { "{ $setWindowFields : { output : { Result : { $integral : { input : '$DecimalField', unit : 'day' } } } } }" };
            AssertStages(stages, expectedStages);
        }

        [Fact]
        public void Translate_should_return_expected_result_for_Integral_with_Double()
        {
            var collection = GetCollection<C>();

            var aggregate = collection.Aggregate()
                .SetWindowFields(output: p => new { Result = p.Integral(x => x.DoubleField, null) });

            var stages = Translate(collection, aggregate);
            var expectedStages = new[] { "{ $setWindowFields : { output : { Result : { $integral : { input : '$DoubleField' } } } } }" };
            AssertStages(stages, expectedStages);
        }

        [Fact]
        public void Translate_should_return_expected_result_for_Integral_with_Double_and_unit()
        {
            var collection = GetCollection<C>();

            var aggregate = collection.Aggregate()
                .SetWindowFields(output: p => new { Result = p.Integral(x => x.DoubleField, WindowTimeUnit.Day, null) });

            var stages = Translate(collection, aggregate);
            var expectedStages = new[] { "{ $setWindowFields : { output : { Result : { $integral : { input : '$DoubleField', unit : 'day' } } } } }" };
            AssertStages(stages, expectedStages);
        }

        [Fact]
        public void Translate_should_return_expected_result_for_Integral_with_Int32()
        {
            var collection = GetCollection<C>();

            var aggregate = collection.Aggregate()
                .SetWindowFields(output: p => new { Result = p.Integral(x => x.Int32Field, null) });

            var stages = Translate(collection, aggregate);
            var expectedStages = new[] { "{ $setWindowFields : { output : { Result : { $integral : { input : '$Int32Field' } } } } }" };
            AssertStages(stages, expectedStages);
        }

        [Fact]
        public void Translate_should_return_expected_result_for_Integral_with_Int32_and_unit()
        {
            var collection = GetCollection<C>();

            var aggregate = collection.Aggregate()
                .SetWindowFields(output: p => new { Result = p.Integral(x => x.Int32Field, WindowTimeUnit.Day, null) });

            var stages = Translate(collection, aggregate);
            var expectedStages = new[] { "{ $setWindowFields : { output : { Result : { $integral : { input : '$Int32Field', unit : 'day' } } } } }" };
            AssertStages(stages, expectedStages);
        }

        [Fact]
        public void Translate_should_return_expected_result_for_Integral_with_Int64()
        {
            var collection = GetCollection<C>();

            var aggregate = collection.Aggregate()
                .SetWindowFields(output: p => new { Result = p.Integral(x => x.Int64Field, null) });

            var stages = Translate(collection, aggregate);
            var expectedStages = new[] { "{ $setWindowFields : { output : { Result : { $integral : { input : '$Int64Field' } } } } }" };
            AssertStages(stages, expectedStages);
        }

        [Fact]
        public void Translate_should_return_expected_result_for_Integral_with_Int64_and_unit()
        {
            var collection = GetCollection<C>();

            var aggregate = collection.Aggregate()
                .SetWindowFields(output: p => new { Result = p.Integral(x => x.Int64Field, WindowTimeUnit.Day, null) });

            var stages = Translate(collection, aggregate);
            var expectedStages = new[] { "{ $setWindowFields : { output : { Result : { $integral : { input : '$Int64Field', unit : 'day' } } } } }" };
            AssertStages(stages, expectedStages);
        }

        [Fact]
        public void Translate_should_return_expected_result_for_Integral_with_Single()
        {
            var collection = GetCollection<C>();

            var aggregate = collection.Aggregate()
                .SetWindowFields(output: p => new { Result = p.Integral(x => x.SingleField, null) });

            var stages = Translate(collection, aggregate);
            var expectedStages = new[] { "{ $setWindowFields : { output : { Result : { $integral : { input : '$SingleField' } } } } }" };
            AssertStages(stages, expectedStages);
        }

        [Fact]
        public void Translate_should_return_expected_result_for_Integral_with_Single_and_unit()
        {
            var collection = GetCollection<C>();

            var aggregate = collection.Aggregate()
                .SetWindowFields(output: p => new { Result = p.Integral(x => x.SingleField, WindowTimeUnit.Day, null) });

            var stages = Translate(collection, aggregate);
            var expectedStages = new[] { "{ $setWindowFields : { output : { Result : { $integral : { input : '$SingleField', unit : 'day' } } } } }" };
            AssertStages(stages, expectedStages);
        }

        [Fact]
        public void Translate_should_return_expected_result_for_Last()
        {
            var collection = GetCollection<C>();

            var aggregate = collection.Aggregate()
                .SetWindowFields(output: p => new { Result = p.Last(x => x.Int32Field, null) });

            var stages = Translate(collection, aggregate);
            var expectedStages = new[] { "{ $setWindowFields : { output : { Result : { $last : '$Int32Field' } } } }" };
            AssertStages(stages, expectedStages);
        }

        [Fact]
        public void Translate_should_return_expected_result_for_Max()
        {
            var collection = GetCollection<C>();

            var aggregate = collection.Aggregate()
                .SetWindowFields(output: p => new { Result = p.Max(x => x.Int32Field, null) });

            var stages = Translate(collection, aggregate);
            var expectedStages = new[] { "{ $setWindowFields : { output : { Result : { $max : '$Int32Field' } } } }" };
            AssertStages(stages, expectedStages);
        }

        [Fact]
        public void Translate_should_return_expected_result_for_Min()
        {
            var collection = GetCollection<C>();

            var aggregate = collection.Aggregate()
                .SetWindowFields(output: p => new { Result = p.Min(x => x.Int32Field, null) });

            var stages = Translate(collection, aggregate);
            var expectedStages = new[] { "{ $setWindowFields : { output : { Result : { $min : '$Int32Field' } } } }" };
            AssertStages(stages, expectedStages);
        }

        [Fact]
        public void Translate_should_return_expected_result_for_Push()
        {
            var collection = GetCollection<C>();

            var aggregate = collection.Aggregate()
                .SetWindowFields(output: p => new { Result = p.Push(x => x.Int32Field, null) });

            var stages = Translate(collection, aggregate);
            var expectedStages = new[] { "{ $setWindowFields : { output : { Result : { $push : '$Int32Field' } } } }" };
            AssertStages(stages, expectedStages);
        }

        [Fact]
        public void Translate_should_return_expected_result_for_Rank()
        {
            var collection = GetCollection<C>();

            var aggregate = collection.Aggregate()
                .SetWindowFields(output: p => new { Result = p.Rank() });

            var stages = Translate(collection, aggregate);
            var expectedStages = new[] { "{ $setWindowFields : { output : { Result : { $rank : { } } } } }" };
            AssertStages(stages, expectedStages);
        }

        [Fact]
        public void Translate_should_return_expected_result_for_Shift()
        {
            var collection = GetCollection<C>();

            var aggregate = collection.Aggregate()
                .SetWindowFields(output: p => new { Result = p.Shift(x => x.Int32Field, 1) });

            var stages = Translate(collection, aggregate);
            var expectedStages = new[] { "{ $setWindowFields : { output : { Result : { $shift : { output : '$Int32Field', by : 1 } } } } }" };
            AssertStages(stages, expectedStages);
        }

        [Fact]
        public void Translate_should_return_expected_result_for_Shift_with_defaultValue()
        {
            var collection = GetCollection<C>();

            var aggregate = collection.Aggregate()
                .SetWindowFields(output: p => new { Result = p.Shift(x => x.DoubleField, 1, 2.5) });

            var stages = Translate(collection, aggregate);
            var expectedStages = new[] { "{ $setWindowFields : { output : { Result : { $shift : { output : '$DoubleField', by : 1, default : 2.5 } } } } }" };
            AssertStages(stages, expectedStages);
        }

        [Fact]
        public void Translate_should_return_expected_result_for_StandardDeviationPopulation_with_Decimal()
        {
            var collection = GetCollection<C>();

            var aggregate = collection.Aggregate()
                .SetWindowFields(output: p => new { Result = p.StandardDeviationPopulation(x => x.DecimalField, null) });

            var stages = Translate(collection, aggregate);
            var expectedStages = new[] { "{ $setWindowFields : { output : { Result : { $stdDevPop : '$DecimalField' } } } }" };
            AssertStages(stages, expectedStages);
        }

        [Fact]
        public void Translate_should_return_expected_result_for_StandardDeviationPopulation_with_Double()
        {
            var collection = GetCollection<C>();

            var aggregate = collection.Aggregate()
                .SetWindowFields(output: p => new { Result = p.StandardDeviationPopulation(x => x.DoubleField, null) });

            var stages = Translate(collection, aggregate);
            var expectedStages = new[] { "{ $setWindowFields : { output : { Result : { $stdDevPop : '$DoubleField' } } } }" };
            AssertStages(stages, expectedStages);
        }

        [Fact]
        public void Translate_should_return_expected_result_for_StandardDeviationPopulation_with_Int32()
        {
            var collection = GetCollection<C>();

            var aggregate = collection.Aggregate()
                .SetWindowFields(output: p => new { Result = p.StandardDeviationPopulation(x => x.Int32Field, null) });

            var stages = Translate(collection, aggregate);
            var expectedStages = new[] { "{ $setWindowFields : { output : { Result : { $stdDevPop : '$Int32Field' } } } }" };
            AssertStages(stages, expectedStages);
        }

        [Fact]
        public void Translate_should_return_expected_result_for_StandardDeviationPopulation_with_Int64()
        {
            var collection = GetCollection<C>();

            var aggregate = collection.Aggregate()
                .SetWindowFields(output: p => new { Result = p.StandardDeviationPopulation(x => x.Int64Field, null) });

            var stages = Translate(collection, aggregate);
            var expectedStages = new[] { "{ $setWindowFields : { output : { Result : { $stdDevPop : '$Int64Field' } } } }" };
            AssertStages(stages, expectedStages);
        }

        [Fact]
        public void Translate_should_return_expected_result_for_StandardDeviationPopulation_with_nullable_Decimal()
        {
            var collection = GetCollection<C>();

            var aggregate = collection.Aggregate()
                .SetWindowFields(output: p => new { Result = p.StandardDeviationPopulation(x => x.NullableDecimalField, null) });

            var stages = Translate(collection, aggregate);
            var expectedStages = new[] { "{ $setWindowFields : { output : { Result : { $stdDevPop : '$NullableDecimalField' } } } }" };
            AssertStages(stages, expectedStages);
        }

        [Fact]
        public void Translate_should_return_expected_result_for_StandardDeviationPopulation_with_nullable_Double()
        {
            var collection = GetCollection<C>();

            var aggregate = collection.Aggregate()
                .SetWindowFields(output: p => new { Result = p.StandardDeviationPopulation(x => x.NullableDoubleField, null) });

            var stages = Translate(collection, aggregate);
            var expectedStages = new[] { "{ $setWindowFields : { output : { Result : { $stdDevPop : '$NullableDoubleField' } } } }" };
            AssertStages(stages, expectedStages);
        }

        [Fact]
        public void Translate_should_return_expected_result_for_StandardDeviationPopulation_with_nullable_Int32()
        {
            var collection = GetCollection<C>();

            var aggregate = collection.Aggregate()
                .SetWindowFields(output: p => new { Result = p.StandardDeviationPopulation(x => x.NullableInt32Field, null) });

            var stages = Translate(collection, aggregate);
            var expectedStages = new[] { "{ $setWindowFields : { output : { Result : { $stdDevPop : '$NullableInt32Field' } } } }" };
            AssertStages(stages, expectedStages);
        }

        [Fact]
        public void Translate_should_return_expected_result_for_StandardDeviationPopulation_with_nullable_Int64()
        {
            var collection = GetCollection<C>();

            var aggregate = collection.Aggregate()
                .SetWindowFields(output: p => new { Result = p.StandardDeviationPopulation(x => x.NullableInt64Field, null) });

            var stages = Translate(collection, aggregate);
            var expectedStages = new[] { "{ $setWindowFields : { output : { Result : { $stdDevPop : '$NullableInt64Field' } } } }" };
            AssertStages(stages, expectedStages);
        }

        [Fact]
        public void Translate_should_return_expected_result_for_StandardDeviationPopulation_with_nullable_Single()
        {
            var collection = GetCollection<C>();

            var aggregate = collection.Aggregate()
                .SetWindowFields(output: p => new { Result = p.StandardDeviationPopulation(x => x.NullableSingleField, null) });

            var stages = Translate(collection, aggregate);
            var expectedStages = new[] { "{ $setWindowFields : { output : { Result : { $stdDevPop : '$NullableSingleField' } } } }" };
            AssertStages(stages, expectedStages);
        }

        [Fact]
        public void Translate_should_return_expected_result_for_StandardDeviationPopulation_with_Single()
        {
            var collection = GetCollection<C>();

            var aggregate = collection.Aggregate()
                .SetWindowFields(output: p => new { Result = p.StandardDeviationPopulation(x => x.SingleField, null) });

            var stages = Translate(collection, aggregate);
            var expectedStages = new[] { "{ $setWindowFields : { output : { Result : { $stdDevPop : '$SingleField' } } } }" };
            AssertStages(stages, expectedStages);
        }

        [Fact]
        public void Translate_should_return_expected_result_for_StandardDeviationSample_with_Decimal()
        {
            var collection = GetCollection<C>();

            var aggregate = collection.Aggregate()
                .SetWindowFields(output: p => new { Result = p.StandardDeviationSample(x => x.DecimalField, null) });

            var stages = Translate(collection, aggregate);
            var expectedStages = new[] { "{ $setWindowFields : { output : { Result : { $stdDevSamp : '$DecimalField' } } } }" };
            AssertStages(stages, expectedStages);
        }

        [Fact]
        public void Translate_should_return_expected_result_for_StandardDeviationSample_with_Double()
        {
            var collection = GetCollection<C>();

            var aggregate = collection.Aggregate()
                .SetWindowFields(output: p => new { Result = p.StandardDeviationSample(x => x.DoubleField, null) });

            var stages = Translate(collection, aggregate);
            var expectedStages = new[] { "{ $setWindowFields : { output : { Result : { $stdDevSamp : '$DoubleField' } } } }" };
            AssertStages(stages, expectedStages);
        }

        [Fact]
        public void Translate_should_return_expected_result_for_StandardDeviationSample_with_Int32()
        {
            var collection = GetCollection<C>();

            var aggregate = collection.Aggregate()
                .SetWindowFields(output: p => new { Result = p.StandardDeviationSample(x => x.Int32Field, null) });

            var stages = Translate(collection, aggregate);
            var expectedStages = new[] { "{ $setWindowFields : { output : { Result : { $stdDevSamp : '$Int32Field' } } } }" };
            AssertStages(stages, expectedStages);
        }

        [Fact]
        public void Translate_should_return_expected_result_for_StandardDeviationSample_with_Int64()
        {
            var collection = GetCollection<C>();

            var aggregate = collection.Aggregate()
                .SetWindowFields(output: p => new { Result = p.StandardDeviationSample(x => x.Int64Field, null) });

            var stages = Translate(collection, aggregate);
            var expectedStages = new[] { "{ $setWindowFields : { output : { Result : { $stdDevSamp : '$Int64Field' } } } }" };
            AssertStages(stages, expectedStages);
        }

        [Fact]
        public void Translate_should_return_expected_result_for_StandardDeviationSample_with_nullable_Decimal()
        {
            var collection = GetCollection<C>();

            var aggregate = collection.Aggregate()
                .SetWindowFields(output: p => new { Result = p.StandardDeviationSample(x => x.NullableDecimalField, null) });

            var stages = Translate(collection, aggregate);
            var expectedStages = new[] { "{ $setWindowFields : { output : { Result : { $stdDevSamp : '$NullableDecimalField' } } } }" };
            AssertStages(stages, expectedStages);
        }

        [Fact]
        public void Translate_should_return_expected_result_for_StandardDeviationSample_with_nullable_Double()
        {
            var collection = GetCollection<C>();

            var aggregate = collection.Aggregate()
                .SetWindowFields(output: p => new { Result = p.StandardDeviationSample(x => x.NullableDoubleField, null) });

            var stages = Translate(collection, aggregate);
            var expectedStages = new[] { "{ $setWindowFields : { output : { Result : { $stdDevSamp : '$NullableDoubleField' } } } }" };
            AssertStages(stages, expectedStages);
        }

        [Fact]
        public void Translate_should_return_expected_result_for_StandardDeviationSample_with_nullable_Int32()
        {
            var collection = GetCollection<C>();

            var aggregate = collection.Aggregate()
                .SetWindowFields(output: p => new { Result = p.StandardDeviationSample(x => x.NullableInt32Field, null) });

            var stages = Translate(collection, aggregate);
            var expectedStages = new[] { "{ $setWindowFields : { output : { Result : { $stdDevSamp : '$NullableInt32Field' } } } }" };
            AssertStages(stages, expectedStages);
        }

        [Fact]
        public void Translate_should_return_expected_result_for_StandardDeviationSample_with_nullable_Int64()
        {
            var collection = GetCollection<C>();

            var aggregate = collection.Aggregate()
                .SetWindowFields(output: p => new { Result = p.StandardDeviationSample(x => x.NullableInt64Field, null) });

            var stages = Translate(collection, aggregate);
            var expectedStages = new[] { "{ $setWindowFields : { output : { Result : { $stdDevSamp : '$NullableInt64Field' } } } }" };
            AssertStages(stages, expectedStages);
        }

        [Fact]
        public void Translate_should_return_expected_result_for_StandardDeviationSample_with_nullable_Single()
        {
            var collection = GetCollection<C>();

            var aggregate = collection.Aggregate()
                .SetWindowFields(output: p => new { Result = p.StandardDeviationSample(x => x.NullableSingleField, null) });

            var stages = Translate(collection, aggregate);
            var expectedStages = new[] { "{ $setWindowFields : { output : { Result : { $stdDevSamp : '$NullableSingleField' } } } }" };
            AssertStages(stages, expectedStages);
        }

        [Fact]
        public void Translate_should_return_expected_result_for_StandardDeviationSample_with_Single()
        {
            var collection = GetCollection<C>();

            var aggregate = collection.Aggregate()
                .SetWindowFields(output: p => new { Result = p.StandardDeviationSample(x => x.SingleField, null) });

            var stages = Translate(collection, aggregate);
            var expectedStages = new[] { "{ $setWindowFields : { output : { Result : { $stdDevSamp : '$SingleField' } } } }" };
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
            public decimal DecimalField1 { get; set; }
            public decimal DecimalField2 { get; set; }
            public double DoubleField { get; set; }
            public double DoubleField1 { get; set; }
            public double DoubleField2 { get; set; }
            public int Int32Field { get; set; }
            public int Int32Field1 { get; set; }
            public int Int32Field2 { get; set; }
            public long Int64Field { get; set; }
            public long Int64Field1 { get; set; }
            public long Int64Field2 { get; set; }
            public decimal? NullableDecimalField { get; set; }
            public decimal? NullableDecimalField1 { get; set; }
            public decimal? NullableDecimalField2 { get; set; }
            public double? NullableDoubleField { get; set; }
            public double? NullableDoubleField1 { get; set; }
            public double? NullableDoubleField2 { get; set; }
            public int? NullableInt32Field { get; set; }
            public int? NullableInt32Field1 { get; set; }
            public int? NullableInt32Field2 { get; set; }
            public long? NullableInt64Field { get; set; }
            public long? NullableInt64Field1 { get; set; }
            public long? NullableInt64Field2 { get; set; }
            public float? NullableSingleField { get; set; }
            public float? NullableSingleField1 { get; set; }
            public float? NullableSingleField2 { get; set; }
            public float SingleField { get; set; }
            public float SingleField1 { get; set; }
            public float SingleField2 { get; set; }
        }
    }
}
