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

using System;
using System.Collections.Generic;

namespace MongoDB.Driver.Core
{
    internal static class IDictionaryExtensions
    {
        public static void AddRange<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, IEnumerable<KeyValuePair<TKey, TValue>> values)
        {
            foreach (var p in values)
            {
                dictionary.Add(p);
            }
        }

        public static TValue GetOrAdd<TKey, TValue>(
            this IDictionary<TKey, TValue> dictionary,
            TKey key,
            Func<TValue> addValueFactory,
            Func<TValue, TValue> updateValueFactory = null)
        {
            if (dictionary.TryGetValue(key, out var value))
            {
                return updateValueFactory != null ? updateValueFactory(value) : value;
            }
            else
            {
                value = addValueFactory();
                dictionary.Add(key, value);
                return value;
            }
        }
    }
}
