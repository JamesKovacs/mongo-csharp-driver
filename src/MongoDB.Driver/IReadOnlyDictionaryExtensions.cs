/* Copyright 2019-present MongoDB Inc.
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

namespace MongoDB.Driver
{
    internal static class IReadOnlyDictionaryExtensions
    {
        public static IReadOnlyDictionary<TKey, TValue> Add<TKey, TValue>(this IReadOnlyDictionary<TKey, TValue> dictionary, TKey key, TValue value)
        {
            var clone = new Dictionary<TKey, TValue>(dictionary.Count + 1);
            foreach (var kv in dictionary)
            {
                clone.Add(kv.Key, kv.Value);
            }
            clone.Add(key, value);
            return clone;
        }

        public static TValue GetValueOrDefault<TKey, TValue>(this IReadOnlyDictionary<TKey, TValue> dictionary, TKey key, TValue @default)
        {
            return (dictionary != null && dictionary.TryGetValue(key, out var value)) ? value : @default;
        }

        public static bool IsEquivalentTo<TKey, TValue>(this IReadOnlyDictionary<TKey, TValue> x, IReadOnlyDictionary<TKey, TValue> y, Func<TValue, TValue, bool> equals)
        {
            if (object.ReferenceEquals(x, y))
            {
                return true;
            }

            if (object.ReferenceEquals(x, null) || object.ReferenceEquals(y, null))
            {
                return false;
            }

            if (x.Count != y.Count)
            {
                return false;
            }

            foreach (var keyValuePair in x)
            {
                var key = keyValuePair.Key;
                var xValue = keyValuePair.Value;
                if (!y.TryGetValue(key, out var yValue) || !equals(xValue, yValue))
                {
                    return false;
                }
            }

            return true;
        }
    }
}
