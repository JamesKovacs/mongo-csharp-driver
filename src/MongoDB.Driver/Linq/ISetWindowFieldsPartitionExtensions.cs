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
* 
*/

using System;
using System.Collections.Generic;

namespace MongoDB.Driver.Linq
{
    /// <summary>
    /// Extension methods that represent operations for SetWindowFields.
    /// </summary>
    public static class ISetWindowFieldsPartitionExtensions
    {
        /// <summary>
        /// Returns the average value of the numeric values. Average ignores non-numeric values.
        /// </summary>
        /// <typeparam name="TInput">The type of the input documents in the partition.</typeparam>
        /// <typeparam name="TValue">The type of the selected values.</typeparam>
        /// <param name="partition">The partition.</param>
        /// <param name="selector">The selector that selects a value from the input document.</param>
        /// <param name="boundaries">The window boundaries.</param>
        /// <returns>The average of the selected values.</returns>
        public static double Average<TInput, TValue>(this ISetWindowFieldsPartition<TInput> partition, Func<TInput, TValue> selector, WindowBoundaries boundaries = null)
        {
            throw new InvalidOperationException("This method is only intended to be used with SetWindowFields.");
        }

        /// <summary>
        /// Returns the maximum value.
        /// </summary>
        /// <typeparam name="TInput">The type of the input documents in the partition.</typeparam>
        /// <typeparam name="TValue">The type of the selected values.</typeparam>
        /// <param name="partition">The partition.</param>
        /// <param name="selector">The selector that selects a value from the input document.</param>
        /// <param name="boundaries">The window boundaries.</param>
        /// <returns>The maximum of the selected values.</returns>
        public static TValue Max<TInput, TValue>(this ISetWindowFieldsPartition<TInput> partition, Func<TInput, TValue> selector, WindowBoundaries boundaries = null)
        {
            throw new InvalidOperationException("This method is only intended to be used with SetWindowFields.");
        }

        /// <summary>
        /// Returns the minimum value.
        /// </summary>
        /// <typeparam name="TInput">The type of the input documents in the partition.</typeparam>
        /// <typeparam name="TValue">The type of the selected values.</typeparam>
        /// <param name="partition">The partition.</param>
        /// <param name="selector">The selector that selects a value from the input document.</param>
        /// <param name="boundaries">The window boundaries.</param>
        /// <returns>The minimum of the selected values.</returns>
        public static TValue Min<TInput, TValue>(this ISetWindowFieldsPartition<TInput> partition, Func<TInput, TValue> selector, WindowBoundaries boundaries = null)
        {
            throw new InvalidOperationException("This method is only intended to be used with SetWindowFields.");
        }

        /// <summary>
        /// Returns a sequence of values.
        /// </summary>
        /// <typeparam name="TInput">The type of the input documents in the partition.</typeparam>
        /// <typeparam name="TValue">The type of the selected values.</typeparam>
        /// <param name="partition">The partition.</param>
        /// <param name="selector">The selector that selects a value from the input document.</param>
        /// <param name="boundaries">The window boundaries.</param>
        /// <returns>A sequence of the selected values.</returns>
        public static IEnumerable<TValue> Push<TInput, TValue>(this ISetWindowFieldsPartition<TInput> partition, Func<TInput, TValue> selector, WindowBoundaries boundaries = null)
        {
            throw new InvalidOperationException("This method is only intended to be used with SetWindowFields.");
        }

        /// <summary>
        /// Returns the sum of numeric values. $sum ignores non-numeric values.
        /// </summary>
        /// <typeparam name="TInput">The type of the input documents in the partition.</typeparam>
        /// <typeparam name="TValue">The type of the selected values.</typeparam>
        /// <param name="partition">The partition.</param>
        /// <param name="selector">The selector that selects a value from the input document.</param>
        /// <param name="boundaries">The window boundaries.</param>
        /// <returns>The sum of the values.</returns>
        public static TValue Sum<TInput, TValue>(this ISetWindowFieldsPartition<TInput> partition, Func<TInput, TValue> selector, WindowBoundaries boundaries = null)
        {
            throw new InvalidOperationException("This method is only intended to be used with SetWindowFields.");
        }
    }
}
