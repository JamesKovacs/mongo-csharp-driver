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
using System.Reflection;

namespace MongoDB.Driver.Linq.Linq3Implementation.Reflection
{
    internal static class SetWindowFieldsMethod
    {
        // private static fields
        private static readonly MethodInfo __addToSet;
        private static readonly MethodInfo __averageWithDecimal;
        private static readonly MethodInfo __averageWithDouble;
        private static readonly MethodInfo __averageWithInt32;
        private static readonly MethodInfo __averageWithInt64;
        private static readonly MethodInfo __averageWithNullableDecimal;
        private static readonly MethodInfo __averageWithNullableDouble;
        private static readonly MethodInfo __averageWithNullableInt32;
        private static readonly MethodInfo __averageWithNullableInt64;
        private static readonly MethodInfo __averageWithNullableSingle;
        private static readonly MethodInfo __averageWithSingle;
        private static readonly MethodInfo __count;
        private static readonly MethodInfo __covariancePopWithDecimals;
        private static readonly MethodInfo __covariancePopWithDoubles;
        private static readonly MethodInfo __covariancePopWithInt32s;
        private static readonly MethodInfo __covariancePopWithInt64s;
        private static readonly MethodInfo __covariancePopWithNullableDecimals;
        private static readonly MethodInfo __covariancePopWithNullableDoubles;
        private static readonly MethodInfo __covariancePopWithNullableInt32s;
        private static readonly MethodInfo __covariancePopWithNullableInt64s;
        private static readonly MethodInfo __covariancePopWithNullableSingles;
        private static readonly MethodInfo __covariancePopWithSingles;
        private static readonly MethodInfo __covarianceSampWithDecimals;
        private static readonly MethodInfo __covarianceSampWithDoubles;
        private static readonly MethodInfo __covarianceSampWithInt32s;
        private static readonly MethodInfo __covarianceSampWithInt64s;
        private static readonly MethodInfo __covarianceSampWithNullableDecimals;
        private static readonly MethodInfo __covarianceSampWithNullableDoubles;
        private static readonly MethodInfo __covarianceSampWithNullableInt32s;
        private static readonly MethodInfo __covarianceSampWithNullableInt64s;
        private static readonly MethodInfo __covarianceSampWithNullableSingles;
        private static readonly MethodInfo __covarianceSampWithSingles;
        private static readonly MethodInfo __derivativeWithDecimal;
        private static readonly MethodInfo __derivativeWithDecimalAndUnit;
        private static readonly MethodInfo __derivativeWithDouble;
        private static readonly MethodInfo __derivativeWithDoubleAndUnit;
        private static readonly MethodInfo __derivativeWithInt32;
        private static readonly MethodInfo __derivativeWithInt32AndUnit;
        private static readonly MethodInfo __derivativeWithInt64;
        private static readonly MethodInfo __derivativeWithInt64AndUnit;
        private static readonly MethodInfo __derivativeWithSingle;
        private static readonly MethodInfo __derivativeWithSingleAndUnit;
        private static readonly MethodInfo __exponentialMovingAverageWithDecimal;
        private static readonly MethodInfo __exponentialMovingAverageWithDouble;
        private static readonly MethodInfo __exponentialMovingAverageWithInt32;
        private static readonly MethodInfo __exponentialMovingAverageWithInt64;
        private static readonly MethodInfo __exponentialMovingAverageWithSingle;
        private static readonly MethodInfo __integralWithDecimal;
        private static readonly MethodInfo __integralWithDecimalAndUnit;
        private static readonly MethodInfo __integralWithDouble;
        private static readonly MethodInfo __integralWithDoubleAndUnit;
        private static readonly MethodInfo __integralWithInt32;
        private static readonly MethodInfo __integralWithInt32AndUnit;
        private static readonly MethodInfo __integralWithInt64;
        private static readonly MethodInfo __integralWithInt64AndUnit;
        private static readonly MethodInfo __integralWithSingle;
        private static readonly MethodInfo __integralWithSingleAndUnit;
        private static readonly MethodInfo __max;
        private static readonly MethodInfo __min;
        private static readonly MethodInfo __push;
        private static readonly MethodInfo __standardDeviationPopulationWithDecimal;
        private static readonly MethodInfo __standardDeviationPopulationWithDouble;
        private static readonly MethodInfo __standardDeviationPopulationWithInt32;
        private static readonly MethodInfo __standardDeviationPopulationWithInt64;
        private static readonly MethodInfo __standardDeviationPopulationWithNullableDecimal;
        private static readonly MethodInfo __standardDeviationPopulationWithNullableDouble;
        private static readonly MethodInfo __standardDeviationPopulationWithNullableInt32;
        private static readonly MethodInfo __standardDeviationPopulationWithNullableInt64;
        private static readonly MethodInfo __standardDeviationPopulationWithNullableSingle;
        private static readonly MethodInfo __standardDeviationPopulationWithSingle;
        private static readonly MethodInfo __sumWithDecimal;
        private static readonly MethodInfo __sumWithDouble;
        private static readonly MethodInfo __sumWithInt32;
        private static readonly MethodInfo __sumWithInt64;
        private static readonly MethodInfo __sumWithNullableDecimal;
        private static readonly MethodInfo __sumWithNullableDouble;
        private static readonly MethodInfo __sumWithNullableInt32;
        private static readonly MethodInfo __sumWithNullableInt64;
        private static readonly MethodInfo __sumWithNullableSingle;
        private static readonly MethodInfo __sumWithSingle;

        // static constructor
        static SetWindowFieldsMethod()
        {
            __addToSet = ReflectionInfo.Method((ISetWindowFieldsPartition<object> partition, Func<object, object> selector, WindowBoundaries boundaries) => partition.AddToSet(selector, boundaries));
            __averageWithDecimal = ReflectionInfo.Method((ISetWindowFieldsPartition<object> partition, Func<object, decimal> selector, WindowBoundaries boundaries) => partition.Average(selector, boundaries));
            __averageWithDouble = ReflectionInfo.Method((ISetWindowFieldsPartition<object> partition, Func<object, double> selector, WindowBoundaries boundaries) => partition.Average(selector, boundaries));
            __averageWithInt32 = ReflectionInfo.Method((ISetWindowFieldsPartition<object> partition, Func<object, int> selector, WindowBoundaries boundaries) => partition.Average(selector, boundaries));
            __averageWithInt64 = ReflectionInfo.Method((ISetWindowFieldsPartition<object> partition, Func<object, long> selector, WindowBoundaries boundaries) => partition.Average(selector, boundaries));
            __averageWithNullableDecimal = ReflectionInfo.Method((ISetWindowFieldsPartition<object> partition, Func<object, decimal?> selector, WindowBoundaries boundaries) => partition.Average(selector, boundaries));
            __averageWithNullableDouble = ReflectionInfo.Method((ISetWindowFieldsPartition<object> partition, Func<object, double?> selector, WindowBoundaries boundaries) => partition.Average(selector, boundaries));
            __averageWithNullableInt32 = ReflectionInfo.Method((ISetWindowFieldsPartition<object> partition, Func<object, int?> selector, WindowBoundaries boundaries) => partition.Average(selector, boundaries));
            __averageWithNullableInt64 = ReflectionInfo.Method((ISetWindowFieldsPartition<object> partition, Func<object, long?> selector, WindowBoundaries boundaries) => partition.Average(selector, boundaries));
            __averageWithNullableSingle = ReflectionInfo.Method((ISetWindowFieldsPartition<object> partition, Func<object, float?> selector, WindowBoundaries boundaries) => partition.Average(selector, boundaries));
            __averageWithSingle = ReflectionInfo.Method((ISetWindowFieldsPartition<object> partition, Func<object, float> selector, WindowBoundaries boundaries) => partition.Average(selector, boundaries));
            __count = ReflectionInfo.Method((ISetWindowFieldsPartition<object> partition, WindowBoundaries boundaries) => partition.Count(boundaries));
            __covariancePopWithDecimals = ReflectionInfo.Method((ISetWindowFieldsPartition<object> partition, Func<object, decimal> selector1, Func<object, decimal> selector2, WindowBoundaries boundaries) => partition.CovariancePop(selector1, selector2, boundaries));
            __covariancePopWithDoubles = ReflectionInfo.Method((ISetWindowFieldsPartition<object> partition, Func<object, double> selector1, Func<object, double> selector2, WindowBoundaries boundaries) => partition.CovariancePop(selector1, selector2, boundaries));
            __covariancePopWithInt32s = ReflectionInfo.Method((ISetWindowFieldsPartition<object> partition, Func<object, int> selector1, Func<object, int> selector2, WindowBoundaries boundaries) => partition.CovariancePop(selector1, selector2, boundaries));
            __covariancePopWithInt64s = ReflectionInfo.Method((ISetWindowFieldsPartition<object> partition, Func<object, long> selector1, Func<object, long> selector2, WindowBoundaries boundaries) => partition.CovariancePop(selector1, selector2, boundaries));
            __covariancePopWithNullableDecimals = ReflectionInfo.Method((ISetWindowFieldsPartition<object> partition, Func<object, decimal?> selector1, Func<object, decimal?> selector2, WindowBoundaries boundaries) => partition.CovariancePop(selector1, selector2, boundaries));
            __covariancePopWithNullableDoubles = ReflectionInfo.Method((ISetWindowFieldsPartition<object> partition, Func<object, double?> selector1, Func<object, double?> selector2, WindowBoundaries boundaries) => partition.CovariancePop(selector1, selector2, boundaries));
            __covariancePopWithNullableInt32s = ReflectionInfo.Method((ISetWindowFieldsPartition<object> partition, Func<object, int?> selector1, Func<object, int?> selector2, WindowBoundaries boundaries) => partition.CovariancePop(selector1, selector2, boundaries));
            __covariancePopWithNullableInt64s = ReflectionInfo.Method((ISetWindowFieldsPartition<object> partition, Func<object, long?> selector1, Func<object, long?> selector2, WindowBoundaries boundaries) => partition.CovariancePop(selector1, selector2, boundaries));
            __covariancePopWithNullableSingles = ReflectionInfo.Method((ISetWindowFieldsPartition<object> partition, Func<object, float?> selector1, Func<object, float?> selector2, WindowBoundaries boundaries) => partition.CovariancePop(selector1, selector2, boundaries));
            __covariancePopWithSingles = ReflectionInfo.Method((ISetWindowFieldsPartition<object> partition, Func<object, float> selector1, Func<object, float> selector2, WindowBoundaries boundaries) => partition.CovariancePop(selector1, selector2, boundaries));
            __covarianceSampWithDecimals = ReflectionInfo.Method((ISetWindowFieldsPartition<object> partition, Func<object, decimal> selector1, Func<object, decimal> selector2, WindowBoundaries boundaries) => partition.CovarianceSamp(selector1, selector2, boundaries));
            __covarianceSampWithDoubles = ReflectionInfo.Method((ISetWindowFieldsPartition<object> partition, Func<object, double> selector1, Func<object, double> selector2, WindowBoundaries boundaries) => partition.CovarianceSamp(selector1, selector2, boundaries));
            __covarianceSampWithInt32s = ReflectionInfo.Method((ISetWindowFieldsPartition<object> partition, Func<object, int> selector1, Func<object, int> selector2, WindowBoundaries boundaries) => partition.CovarianceSamp(selector1, selector2, boundaries));
            __covarianceSampWithInt64s = ReflectionInfo.Method((ISetWindowFieldsPartition<object> partition, Func<object, long> selector1, Func<object, long> selector2, WindowBoundaries boundaries) => partition.CovarianceSamp(selector1, selector2, boundaries));
            __covarianceSampWithNullableDecimals = ReflectionInfo.Method((ISetWindowFieldsPartition<object> partition, Func<object, decimal?> selector1, Func<object, decimal?> selector2, WindowBoundaries boundaries) => partition.CovarianceSamp(selector1, selector2, boundaries));
            __covarianceSampWithNullableDoubles = ReflectionInfo.Method((ISetWindowFieldsPartition<object> partition, Func<object, double?> selector1, Func<object, double?> selector2, WindowBoundaries boundaries) => partition.CovarianceSamp(selector1, selector2, boundaries));
            __covarianceSampWithNullableInt32s = ReflectionInfo.Method((ISetWindowFieldsPartition<object> partition, Func<object, int?> selector1, Func<object, int?> selector2, WindowBoundaries boundaries) => partition.CovarianceSamp(selector1, selector2, boundaries));
            __covarianceSampWithNullableInt64s = ReflectionInfo.Method((ISetWindowFieldsPartition<object> partition, Func<object, long?> selector1, Func<object, long?> selector2, WindowBoundaries boundaries) => partition.CovarianceSamp(selector1, selector2, boundaries));
            __covarianceSampWithNullableSingles = ReflectionInfo.Method((ISetWindowFieldsPartition<object> partition, Func<object, float?> selector1, Func<object, float?> selector2, WindowBoundaries boundaries) => partition.CovarianceSamp(selector1, selector2, boundaries));
            __covarianceSampWithSingles = ReflectionInfo.Method((ISetWindowFieldsPartition<object> partition, Func<object, float> selector1, Func<object, float> selector2, WindowBoundaries boundaries) => partition.CovarianceSamp(selector1, selector2, boundaries));
            __derivativeWithDecimal = ReflectionInfo.Method((ISetWindowFieldsPartition<object> partition, Func<object, decimal> selector, WindowBoundaries boundaries) => partition.Derivative(selector, boundaries));
            __derivativeWithDecimalAndUnit = ReflectionInfo.Method((ISetWindowFieldsPartition<object> partition, Func<object, decimal> selector, WindowTimeUnit unit, WindowBoundaries boundaries) => partition.Derivative(selector, unit, boundaries));
            __derivativeWithDouble = ReflectionInfo.Method((ISetWindowFieldsPartition<object> partition, Func<object, double> selector, WindowBoundaries boundaries) => partition.Derivative(selector, boundaries));
            __derivativeWithDoubleAndUnit = ReflectionInfo.Method((ISetWindowFieldsPartition<object> partition, Func<object, double> selector, WindowTimeUnit unit, WindowBoundaries boundaries) => partition.Derivative(selector, unit, boundaries));
            __derivativeWithInt32 = ReflectionInfo.Method((ISetWindowFieldsPartition<object> partition, Func<object, int> selector, WindowBoundaries boundaries) => partition.Derivative(selector, boundaries));
            __derivativeWithInt32AndUnit = ReflectionInfo.Method((ISetWindowFieldsPartition<object> partition, Func<object, int> selector, WindowTimeUnit unit, WindowBoundaries boundaries) => partition.Derivative(selector, unit, boundaries));
            __derivativeWithInt64 = ReflectionInfo.Method((ISetWindowFieldsPartition<object> partition, Func<object, long> selector, WindowBoundaries boundaries) => partition.Derivative(selector, boundaries));
            __derivativeWithInt64AndUnit = ReflectionInfo.Method((ISetWindowFieldsPartition<object> partition, Func<object, long> selector, WindowTimeUnit unit, WindowBoundaries boundaries) => partition.Derivative(selector, unit, boundaries));
            __derivativeWithSingle = ReflectionInfo.Method((ISetWindowFieldsPartition<object> partition, Func<object, float> selector, WindowBoundaries boundaries) => partition.Derivative(selector, boundaries));
            __derivativeWithSingleAndUnit = ReflectionInfo.Method((ISetWindowFieldsPartition<object> partition, Func<object, float> selector, WindowTimeUnit unit, WindowBoundaries boundaries) => partition.Derivative(selector, unit, boundaries));;
            __exponentialMovingAverageWithDecimal = ReflectionInfo.Method((ISetWindowFieldsPartition<object> partition, Func<object, decimal> selector, ExponentialMovingAverageWeighting weighting, WindowBoundaries boundaries) => partition.ExponentialMovingAverage(selector, weighting, boundaries));
            __exponentialMovingAverageWithDouble = ReflectionInfo.Method((ISetWindowFieldsPartition<object> partition, Func<object, double> selector, ExponentialMovingAverageWeighting weighting, WindowBoundaries boundaries) => partition.ExponentialMovingAverage(selector, weighting, boundaries));
            __exponentialMovingAverageWithInt32 = ReflectionInfo.Method((ISetWindowFieldsPartition<object> partition, Func<object, int> selector, ExponentialMovingAverageWeighting weighting, WindowBoundaries boundaries) => partition.ExponentialMovingAverage(selector, weighting, boundaries));
            __exponentialMovingAverageWithInt64 = ReflectionInfo.Method((ISetWindowFieldsPartition<object> partition, Func<object, long> selector, ExponentialMovingAverageWeighting weighting, WindowBoundaries boundaries) => partition.ExponentialMovingAverage(selector, weighting, boundaries));
            __exponentialMovingAverageWithSingle = ReflectionInfo.Method((ISetWindowFieldsPartition<object> partition, Func<object, float> selector, ExponentialMovingAverageWeighting weighting, WindowBoundaries boundaries) => partition.ExponentialMovingAverage(selector, weighting, boundaries));
            __integralWithDecimal = ReflectionInfo.Method((ISetWindowFieldsPartition<object> partition, Func<object, decimal> selector, WindowBoundaries boundaries) => partition.Integral(selector, boundaries));
            __integralWithDecimalAndUnit = ReflectionInfo.Method((ISetWindowFieldsPartition<object> partition, Func<object, decimal> selector, WindowTimeUnit unit, WindowBoundaries boundaries) => partition.Integral(selector, unit, boundaries));
            __integralWithDouble = ReflectionInfo.Method((ISetWindowFieldsPartition<object> partition, Func<object, double> selector, WindowBoundaries boundaries) => partition.Integral(selector, boundaries));
            __integralWithDoubleAndUnit = ReflectionInfo.Method((ISetWindowFieldsPartition<object> partition, Func<object, double> selector, WindowTimeUnit unit, WindowBoundaries boundaries) => partition.Integral(selector, unit, boundaries));
            __integralWithInt32 = ReflectionInfo.Method((ISetWindowFieldsPartition<object> partition, Func<object, int> selector, WindowBoundaries boundaries) => partition.Integral(selector, boundaries));
            __integralWithInt32AndUnit = ReflectionInfo.Method((ISetWindowFieldsPartition<object> partition, Func<object, int> selector, WindowTimeUnit unit, WindowBoundaries boundaries) => partition.Integral(selector, unit, boundaries));
            __integralWithInt64 = ReflectionInfo.Method((ISetWindowFieldsPartition<object> partition, Func<object, long> selector, WindowBoundaries boundaries) => partition.Integral(selector, boundaries));
            __integralWithInt64AndUnit = ReflectionInfo.Method((ISetWindowFieldsPartition<object> partition, Func<object, long> selector, WindowTimeUnit unit, WindowBoundaries boundaries) => partition.Integral(selector, unit, boundaries));
            __integralWithSingle = ReflectionInfo.Method((ISetWindowFieldsPartition<object> partition, Func<object, float> selector, WindowBoundaries boundaries) => partition.Integral(selector, boundaries));
            __integralWithSingleAndUnit = ReflectionInfo.Method((ISetWindowFieldsPartition<object> partition, Func<object, float> selector, WindowTimeUnit unit, WindowBoundaries boundaries) => partition.Integral(selector, unit, boundaries)); ;
            __max = ReflectionInfo.Method((ISetWindowFieldsPartition<object> partition, Func<object, object> selector, WindowBoundaries boundaries) => partition.Max(selector, boundaries));
            __min = ReflectionInfo.Method((ISetWindowFieldsPartition<object> partition, Func<object, object> selector, WindowBoundaries boundaries) => partition.Min(selector, boundaries));
            __push = ReflectionInfo.Method((ISetWindowFieldsPartition<object> partition, Func<object, object> selector, WindowBoundaries boundaries) => partition.Push(selector, boundaries));
            __standardDeviationPopulationWithDecimal = ReflectionInfo.Method((ISetWindowFieldsPartition<object> partition, Func<object, decimal> selector, WindowBoundaries boundaries) => partition.StandardDeviationPopulation(selector, boundaries));
            __standardDeviationPopulationWithDouble = ReflectionInfo.Method((ISetWindowFieldsPartition<object> partition, Func<object, double> selector, WindowBoundaries boundaries) => partition.StandardDeviationPopulation(selector, boundaries));
            __standardDeviationPopulationWithInt32 = ReflectionInfo.Method((ISetWindowFieldsPartition<object> partition, Func<object, int> selector, WindowBoundaries boundaries) => partition.StandardDeviationPopulation(selector, boundaries));
            __standardDeviationPopulationWithInt64 = ReflectionInfo.Method((ISetWindowFieldsPartition<object> partition, Func<object, long> selector, WindowBoundaries boundaries) => partition.StandardDeviationPopulation(selector, boundaries));
            __standardDeviationPopulationWithNullableDecimal = ReflectionInfo.Method((ISetWindowFieldsPartition<object> partition, Func<object, decimal?> selector, WindowBoundaries boundaries) => partition.StandardDeviationPopulation(selector, boundaries));
            __standardDeviationPopulationWithNullableDouble = ReflectionInfo.Method((ISetWindowFieldsPartition<object> partition, Func<object, double?> selector, WindowBoundaries boundaries) => partition.StandardDeviationPopulation(selector, boundaries));
            __standardDeviationPopulationWithNullableInt32 = ReflectionInfo.Method((ISetWindowFieldsPartition<object> partition, Func<object, int?> selector, WindowBoundaries boundaries) => partition.StandardDeviationPopulation(selector, boundaries));
            __standardDeviationPopulationWithNullableInt64 = ReflectionInfo.Method((ISetWindowFieldsPartition<object> partition, Func<object, long?> selector, WindowBoundaries boundaries) => partition.StandardDeviationPopulation(selector, boundaries));
            __standardDeviationPopulationWithNullableSingle = ReflectionInfo.Method((ISetWindowFieldsPartition<object> partition, Func<object, float?> selector, WindowBoundaries boundaries) => partition.StandardDeviationPopulation(selector, boundaries));
            __standardDeviationPopulationWithSingle = ReflectionInfo.Method((ISetWindowFieldsPartition<object> partition, Func<object, float> selector, WindowBoundaries boundaries) => partition.StandardDeviationPopulation(selector, boundaries));
            __sumWithDecimal = ReflectionInfo.Method((ISetWindowFieldsPartition<object> partition, Func<object, decimal> selector, WindowBoundaries boundaries) => partition.Sum(selector, boundaries));
            __sumWithDouble = ReflectionInfo.Method((ISetWindowFieldsPartition<object> partition, Func<object, double> selector, WindowBoundaries boundaries) => partition.Sum(selector, boundaries));
            __sumWithInt32 = ReflectionInfo.Method((ISetWindowFieldsPartition<object> partition, Func<object, int> selector, WindowBoundaries boundaries) => partition.Sum(selector, boundaries));
            __sumWithInt64 = ReflectionInfo.Method((ISetWindowFieldsPartition<object> partition, Func<object, long> selector, WindowBoundaries boundaries) => partition.Sum(selector, boundaries));
            __sumWithNullableDecimal = ReflectionInfo.Method((ISetWindowFieldsPartition<object> partition, Func<object, decimal?> selector, WindowBoundaries boundaries) => partition.Sum(selector, boundaries));
            __sumWithNullableDouble = ReflectionInfo.Method((ISetWindowFieldsPartition<object> partition, Func<object, double?> selector, WindowBoundaries boundaries) => partition.Sum(selector, boundaries));
            __sumWithNullableInt32 = ReflectionInfo.Method((ISetWindowFieldsPartition<object> partition, Func<object, int?> selector, WindowBoundaries boundaries) => partition.Sum(selector, boundaries));
            __sumWithNullableInt64 = ReflectionInfo.Method((ISetWindowFieldsPartition<object> partition, Func<object, long?> selector, WindowBoundaries boundaries) => partition.Sum(selector, boundaries));
            __sumWithNullableSingle = ReflectionInfo.Method((ISetWindowFieldsPartition<object> partition, Func<object, float?> selector, WindowBoundaries boundaries) => partition.Sum(selector, boundaries));
            __sumWithSingle = ReflectionInfo.Method((ISetWindowFieldsPartition<object> partition, Func<object, float> selector, WindowBoundaries boundaries) => partition.Sum(selector, boundaries));
        }

        // public properties
        public static MethodInfo AddToSet => __addToSet;
        public static MethodInfo AverageWithDecimal => __averageWithDecimal;
        public static MethodInfo AverageWithDouble => __averageWithDouble;
        public static MethodInfo AverageWithInt32 => __averageWithInt32;
        public static MethodInfo AverageWithInt64 => __averageWithInt64;
        public static MethodInfo AverageWithNullableDecimal => __averageWithNullableDecimal;
        public static MethodInfo AverageWithNullableDouble => __averageWithNullableDouble;
        public static MethodInfo AverageWithNullableInt32 => __averageWithNullableInt32;
        public static MethodInfo AverageWithNullableInt64 => __averageWithNullableInt64;
        public static MethodInfo AverageWithNullableSingle => __averageWithNullableSingle;
        public static MethodInfo AverageWithSingle => __averageWithSingle;
        public static MethodInfo Count => __count;
        public static MethodInfo CovariancePopWithDecimals => __covariancePopWithDecimals;
        public static MethodInfo CovariancePopWithDoubles => __covariancePopWithDoubles;
        public static MethodInfo CovariancePopWithInt32s => __covariancePopWithInt32s;
        public static MethodInfo CovariancePopWithInt64s => __covariancePopWithInt64s;
        public static MethodInfo CovariancePopWithNullableDecimals => __covariancePopWithNullableDecimals;
        public static MethodInfo CovariancePopWithNullableDoubles => __covariancePopWithNullableDoubles;
        public static MethodInfo CovariancePopWithNullableInt32s => __covariancePopWithNullableInt32s;
        public static MethodInfo CovariancePopWithNullableInt64s => __covariancePopWithNullableInt64s;
        public static MethodInfo CovariancePopWithNullableSingles => __covariancePopWithNullableSingles;
        public static MethodInfo CovariancePopWithSingles => __covariancePopWithSingles;
        public static MethodInfo CovarianceSampWithDecimals => __covarianceSampWithDecimals;
        public static MethodInfo CovarianceSampWithDoubles => __covarianceSampWithDoubles;
        public static MethodInfo CovarianceSampWithInt32s => __covarianceSampWithInt32s;
        public static MethodInfo CovarianceSampWithInt64s => __covarianceSampWithInt64s;
        public static MethodInfo CovarianceSampWithNullableDecimals => __covarianceSampWithNullableDecimals;
        public static MethodInfo CovarianceSampWithNullableDoubles => __covarianceSampWithNullableDoubles;
        public static MethodInfo CovarianceSampWithNullableInt32s => __covarianceSampWithNullableInt32s;
        public static MethodInfo CovarianceSampWithNullableInt64s => __covarianceSampWithNullableInt64s;
        public static MethodInfo CovarianceSampWithNullableSingles => __covarianceSampWithNullableSingles;
        public static MethodInfo CovarianceSampWithSingles => __covarianceSampWithSingles;
        public static MethodInfo DerivativeWithDecimal => __derivativeWithDecimal;
        public static MethodInfo DerivativeWithDecimalAndUnit => __derivativeWithDecimalAndUnit;
        public static MethodInfo DerivativeWithDouble => __derivativeWithDouble;
        public static MethodInfo DerivativeWithDoubleAndUnit => __derivativeWithDoubleAndUnit;
        public static MethodInfo DerivativeWithInt32 => __derivativeWithInt32;
        public static MethodInfo DerivativeWithInt32AndUnit => __derivativeWithInt32AndUnit;
        public static MethodInfo DerivativeWithInt64 => __derivativeWithInt64;
        public static MethodInfo DerivativeWithInt64AndUnit => __derivativeWithInt64AndUnit;
        public static MethodInfo DerivativeWithSingle => __derivativeWithSingle;
        public static MethodInfo DerivativeWithSingleAndUnit => __derivativeWithSingleAndUnit;
        public static MethodInfo ExponentialMovingAverageWithDecimal => __exponentialMovingAverageWithDecimal;
        public static MethodInfo ExponentialMovingAverageWithDouble => __exponentialMovingAverageWithDouble;
        public static MethodInfo ExponentialMovingAverageWithInt32 => __exponentialMovingAverageWithInt32;
        public static MethodInfo ExponentialMovingAverageWithInt64 => __exponentialMovingAverageWithInt64;
        public static MethodInfo ExponentialMovingAverageWithSingle => __exponentialMovingAverageWithSingle;
        public static MethodInfo IntegralWithDecimal => __integralWithDecimal;
        public static MethodInfo IntegralWithDecimalAndUnit => __integralWithDecimalAndUnit;
        public static MethodInfo IntegralWithDouble => __integralWithDouble;
        public static MethodInfo IntegralWithDoubleAndUnit => __integralWithDoubleAndUnit;
        public static MethodInfo IntegralWithInt32 => __integralWithInt32;
        public static MethodInfo IntegralWithInt32AndUnit => __integralWithInt32AndUnit;
        public static MethodInfo IntegralWithInt64 => __integralWithInt64;
        public static MethodInfo IntegralWithInt64AndUnit => __integralWithInt64AndUnit;
        public static MethodInfo IntegralWithSingle => __integralWithSingle;
        public static MethodInfo IntegralWithSingleAndUnit => __integralWithSingleAndUnit;
        public static MethodInfo Max => __max;
        public static MethodInfo Min => __min;
        public static MethodInfo Push => __push;
        public static MethodInfo StandardDeviationPopulationWithDecimal => __standardDeviationPopulationWithDecimal;
        public static MethodInfo StandardDeviationPopulationWithDouble => __standardDeviationPopulationWithDouble;
        public static MethodInfo StandardDeviationPopulationWithInt32 => __standardDeviationPopulationWithInt32;
        public static MethodInfo StandardDeviationPopulationWithInt64 => __standardDeviationPopulationWithInt64;
        public static MethodInfo StandardDeviationPopulationWithNullableDecimal => __standardDeviationPopulationWithNullableDecimal;
        public static MethodInfo StandardDeviationPopulationWithNullableDouble => __standardDeviationPopulationWithNullableDouble;
        public static MethodInfo StandardDeviationPopulationWithNullableInt32 => __standardDeviationPopulationWithNullableInt32;
        public static MethodInfo StandardDeviationPopulationWithNullableInt64 => __standardDeviationPopulationWithNullableInt64;
        public static MethodInfo StandardDeviationPopulationWithNullableSingle => __standardDeviationPopulationWithNullableSingle;
        public static MethodInfo StandardDeviationPopulationWithSingle => __standardDeviationPopulationWithSingle;
        public static MethodInfo SumWithDecimal => __sumWithDecimal;
        public static MethodInfo SumWithDouble => __sumWithDouble;
        public static MethodInfo SumWithInt32 => __sumWithInt32;
        public static MethodInfo SumWithInt64 => __sumWithInt64;
        public static MethodInfo SumWithNullableDecimal => __sumWithNullableDecimal;
        public static MethodInfo SumWithNullableDouble => __sumWithNullableDouble;
        public static MethodInfo SumWithNullableInt32 => __sumWithNullableInt32;
        public static MethodInfo SumWithNullableInt64 => __sumWithNullableInt64;
        public static MethodInfo SumWithNullableSingle => __sumWithNullableSingle;
        public static MethodInfo SumWithSingle => __sumWithSingle;
    }
}
