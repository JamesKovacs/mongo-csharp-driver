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
        private static readonly MethodInfo __average;
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
            __average = ReflectionInfo.Method((ISetWindowFieldsPartition<object> partition, Func<object, object> selector, WindowBoundaries boundaries) => partition.Average(selector, boundaries));
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
        public static MethodInfo Average => __average;
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
