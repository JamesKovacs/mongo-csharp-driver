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
        private static readonly MethodInfo __sum;

        // static constructor
        static SetWindowFieldsMethod()
        {
            __average = ReflectionInfo.Method((ISetWindowFieldsPartition<object> partition, Func<object, object> selector, WindowBoundaries boundaries) => partition.Average(selector, boundaries));
            __max = ReflectionInfo.Method((ISetWindowFieldsPartition<object> partition, Func<object, object> selector, WindowBoundaries boundaries) => partition.Max(selector, boundaries));
            __min = ReflectionInfo.Method((ISetWindowFieldsPartition<object> partition, Func<object, object> selector, WindowBoundaries boundaries) => partition.Min(selector, boundaries));
            __sum = ReflectionInfo.Method((ISetWindowFieldsPartition<object> partition, Func<object, object> selector, WindowBoundaries boundaries) => partition.Sum(selector, boundaries));
        }

        // public properties
        public static MethodInfo Average => __average;
        public static MethodInfo Max => __max;
        public static MethodInfo Min => __min;
        public static MethodInfo Sum => __sum;
    }
}
