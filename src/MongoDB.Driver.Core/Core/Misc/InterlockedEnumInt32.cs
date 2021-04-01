/* Copyright 2021-present MongoDB Inc.
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
using System.Runtime.CompilerServices;
using System.Threading;

namespace MongoDB.Driver.Core.Misc
{
    /// <summary>
    /// Thread-safe helper to manage a value.
    /// </summary>
    internal sealed class InterlockedEnumInt32<T> where T : Enum
    {
        private int _valueInt;
     
        public InterlockedEnumInt32(T initialValue)
        {
            if (Enum.GetUnderlyingType(typeof(T)) != typeof(int))
            {
                throw new ArgumentOutOfRangeException(nameof(T), "Underlying Enum type must be integer");
            }

            _valueInt = Unsafe.As<T, int>(ref initialValue);
        }

        public T Value
        { 
            get
            {
                var result = Interlocked.CompareExchange(ref _valueInt, 0, 0);
                return Unsafe.As<int, T>(ref result);
            }
        }

        public T CompareExchange(T fromValue, T toValue)
        {
            if (fromValue.Equals(toValue))
            {
                throw new ArgumentException("fromValue and toValue must be different.");
            }

            var fromValueInt = Unsafe.As<T, int>(ref fromValue);
            var toValueInt = Unsafe.As<T, int>(ref toValue);
            var previousValue = Interlocked.CompareExchange(ref _valueInt, toValueInt, fromValueInt);

            return Unsafe.As<int, T>(ref previousValue);
        }

        public bool TryChange(T toValue)
        {
            var toValueInt = Unsafe.As<T, int>(ref toValue);
            return Interlocked.Exchange(ref _valueInt, toValueInt) != toValueInt;
        }

        public bool TryChange(T fromValue, T toValue)
            => CompareExchange(fromValue, toValue).Equals(fromValue);
    }
}
