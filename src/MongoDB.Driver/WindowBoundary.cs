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
using System.Collections.Generic;
using System.Linq;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;
using MongoDB.Driver.Core.Misc;
using MongoDB.Driver.Linq.Linq3Implementation.Misc;

namespace MongoDB.Driver
{
#pragma warning disable CS1591
    public abstract class WindowBoundary
    {
        #region static
        private static readonly KeywordWindowBoundary __current = new KeywordWindowBoundary("current");
        private static readonly KeywordWindowBoundary __unbounded = new KeywordWindowBoundary("unbounded");

        public static KeywordWindowBoundary Current => __current;
        public static KeywordWindowBoundary Unbounded => __unbounded;

        public static TimeRangeWindowBoundary Days(int value) => new TimeRangeWindowBoundary(value, unit: "day");
        public static TimeRangeWindowBoundary Hours(int value) => new TimeRangeWindowBoundary(value, unit: "hour");
        public static TimeRangeWindowBoundary Milliseconds(int value) => new TimeRangeWindowBoundary(value, unit: "millisecond");
        public static TimeRangeWindowBoundary Minutes(int value) => new TimeRangeWindowBoundary(value, unit: "minute");
        public static TimeRangeWindowBoundary Months(int value) => new TimeRangeWindowBoundary(value, unit: "month");
        public static TimeRangeWindowBoundary Quarters(int value) => new TimeRangeWindowBoundary(value, unit: "quarter");
        public static TimeRangeWindowBoundary Seconds(int value) => new TimeRangeWindowBoundary(value, unit: "second");
        public static TimeRangeWindowBoundary Weeks(int value) => new TimeRangeWindowBoundary(value, unit: "week");
        public static TimeRangeWindowBoundary Years(int value) => new TimeRangeWindowBoundary(value, unit: "year");
        #endregion
    }

    public sealed class KeywordWindowBoundary
    {
        private readonly string _keyword;

        internal KeywordWindowBoundary(string keyword)
        {
            _keyword = Ensure.IsNotNullOrEmpty(keyword, nameof(keyword));
        }

        public string Keyword => _keyword;

        public override string ToString() => $"\"{_keyword}\"";
    }

    public abstract class DocumentsWindowBoundary : WindowBoundary
    {
        public abstract BsonValue Render();
    }

    public sealed class KeywordDocumentsWindowBoundary : DocumentsWindowBoundary
    {
        private readonly string _keyword;

        public KeywordDocumentsWindowBoundary(string keyword)
        {
            _keyword = Ensure.IsNotNullOrEmpty(keyword, nameof(keyword));
        }

        public string Keyword => _keyword;

        public override BsonValue Render() => _keyword;
        public override string ToString() => $"\"{_keyword}\"";
    }

    public sealed class PositionDocumentsWindowBoundary : DocumentsWindowBoundary
    {
        private readonly int _position;

        public PositionDocumentsWindowBoundary(int position)
        {
            _position = position;
        }

        public int Position => _position;

        public override BsonValue Render() => _position;
        public override string ToString() => _position.ToString();
    }

    public abstract class RangeWindowBoundary : WindowBoundary
    {
        public abstract BsonValue Render(IBsonSerializer valueSerializer);
    }

    public sealed class KeywordRangeWindowBoundary : RangeWindowBoundary
    {
        private readonly string _keyword;

        public KeywordRangeWindowBoundary(string keyword)
        {
            _keyword = Ensure.IsNotNullOrEmpty(keyword, nameof(keyword));
        }

        public string Keyword => _keyword;

        public override BsonValue Render(IBsonSerializer valueSerializer) => _keyword;
        public override string ToString() => $"\"{_keyword}\"";
    }

    public abstract class ValueRangeWindowBoundary : RangeWindowBoundary
    {
        public abstract Type ValueType { get; }
    }

    public sealed class ValueRangeWindowBoundary<TValue> : ValueRangeWindowBoundary
    {
        private readonly TValue _value;

        public ValueRangeWindowBoundary(TValue value)
        {
            _value = value;
        }

        public TValue Value => _value;
        public override Type ValueType => typeof(TValue);

        public override BsonValue Render(IBsonSerializer valueSerializer)
        {
            if (valueSerializer == null)
            {
                throw new ArgumentNullException("A value serializer is required to serialize range values.", nameof(valueSerializer));
            }
            return SerializationHelper.SerializeValue(valueSerializer, _value);
        }

        public override string ToString() => _value.ToString();
    }

    public sealed class TimeRangeWindowBoundary : RangeWindowBoundary
    {
        private readonly string _unit;
        private readonly int _value;

        internal TimeRangeWindowBoundary(int value, string unit)
        {
            _value = value;
            _unit = Ensure.IsNotNullOrEmpty(unit, nameof(unit));
        }

        public string Unit => _unit;
        public int Value => _value;

        public override BsonValue Render(IBsonSerializer valueSerializer) => _value;
        public override string ToString() => $"{_value} ({_unit})";
    }

    internal static class ValueRangeWindowBoundaryConvertingValueSerializerFactory
    {
        private static readonly IReadOnlyDictionary<Type, Type[]> __allowedConversions = new Dictionary<Type, Type[]>
        {
            // sortByType => list of allowed valueTypes
            { typeof(byte), new[] { typeof(sbyte)} },
            { typeof(sbyte), new[] { typeof(byte)} },
            { typeof(short), new[] { typeof(byte), typeof(sbyte), typeof(ushort) } },
            { typeof(ushort), new[] { typeof(byte), typeof(sbyte), typeof(short) } },
            { typeof(int), new[] { typeof(byte), typeof(sbyte), typeof(short), typeof(ushort), typeof(uint) } },
            { typeof(uint), new[] { typeof(byte), typeof(sbyte), typeof(short), typeof(ushort), typeof(int) } },
            { typeof(long), new[] { typeof(byte), typeof(sbyte), typeof(short), typeof(ushort), typeof(int), typeof(uint), typeof(ulong) } },
            { typeof(ulong), new[] { typeof(byte), typeof(sbyte), typeof(short), typeof(ushort), typeof(int), typeof(uint), typeof(long) } },
            { typeof(float), new[] { typeof(byte), typeof(sbyte), typeof(short), typeof(ushort), typeof(uint) } },
            { typeof(double), new[] { typeof(byte), typeof(sbyte), typeof(short), typeof(ushort), typeof(int), typeof(uint), typeof(long), typeof(ulong), typeof(float) } },
            { typeof(decimal), new[] { typeof(byte), typeof(sbyte), typeof(short), typeof(ushort), typeof(int), typeof(uint), typeof(long), typeof(ulong), typeof(float), typeof(double) } }
        };

        public static IBsonSerializer Create(ValueRangeWindowBoundary boundary, IBsonSerializer sortBySerializer)
        {
            var valueType = boundary.ValueType;
            var sortByType = sortBySerializer.ValueType;

            if (valueType == sortByType)
            {
                return sortBySerializer;
            }

            if (IsAllowedConversion(valueType, sortByType))
            {
                var serializerTypeDescription = typeof(ValueRangeWindowBoundaryConvertingValueSerializer<,>);
                var serializerType = serializerTypeDescription.MakeGenericType(valueType, sortByType);
                var constructorInfo = serializerType.GetConstructors().Single();
                return (IBsonSerializer)constructorInfo.Invoke(new object[] { sortBySerializer });
            }

            throw new InvalidOperationException("SetWindowFields range window value must be of the same type as the sortBy field (or convertible to that type).");
        }

        private static bool IsAllowedConversion(Type valueType, Type sortByType)
        {
            if (__allowedConversions.TryGetValue(sortByType, out var allowedValueTypes))
            {
                return allowedValueTypes.Contains(valueType);
            }

            return false;
        }
    }

    internal class ValueRangeWindowBoundaryConvertingValueSerializer<TValue, TSortBy> : SerializerBase<TValue>
    {
        private readonly IBsonSerializer<TSortBy> _sortBySerializer;

        public ValueRangeWindowBoundaryConvertingValueSerializer(IBsonSerializer<TSortBy> sortBySerializer)
        {
            _sortBySerializer = sortBySerializer;
        }

        public override void Serialize(BsonSerializationContext context, BsonSerializationArgs args, TValue value)
        {
            _sortBySerializer.Serialize(context, args, Coerce(value));
        }

        private static TSortBy Coerce(TValue value)
        {
            return (TSortBy)Convert.ChangeType(value, typeof(TSortBy));
        }
    }
#pragma warning restore
}
