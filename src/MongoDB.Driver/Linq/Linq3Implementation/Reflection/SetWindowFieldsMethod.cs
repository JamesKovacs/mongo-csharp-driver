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
        private static readonly MethodInfo __max;
        private static readonly MethodInfo __min;
        private static readonly MethodInfo __push;
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
            __max = ReflectionInfo.Method((ISetWindowFieldsPartition<object> partition, Func<object, object> selector, WindowBoundaries boundaries) => partition.Max(selector, boundaries));
            __min = ReflectionInfo.Method((ISetWindowFieldsPartition<object> partition, Func<object, object> selector, WindowBoundaries boundaries) => partition.Min(selector, boundaries));
            __push = ReflectionInfo.Method((ISetWindowFieldsPartition<object> partition, Func<object, object> selector, WindowBoundaries boundaries) => partition.Push(selector, boundaries));
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
        public static MethodInfo Max => __max;
        public static MethodInfo Min => __min;
        public static MethodInfo Push => __push;
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
