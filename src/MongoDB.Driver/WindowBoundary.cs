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

using MongoDB.Driver.Core.Misc;

namespace MongoDB.Driver
{
#pragma warning disable CS1591
    public static class WindowBoundary
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
    }

    public abstract class DocumentsWindowBoundary
    {
    }

    public sealed class KeywordDocumentsWindowBoundary : DocumentsWindowBoundary
    {
        private readonly string _keyword;

        public KeywordDocumentsWindowBoundary(string keyword)
        {
            _keyword = Ensure.IsNotNullOrEmpty(keyword, nameof(keyword));
        }

        public string Keyword => _keyword;
    }

    public sealed class PositionDocumentsWindowBoundary : DocumentsWindowBoundary
    {
        private readonly int _position;

        public PositionDocumentsWindowBoundary(int position)
        {
            _position = position;
        }

        public int Position => _position;
    }


    public abstract class RangeWindowBoundary
    {
    }

    public sealed class KeywordRangeWindowBoundary : RangeWindowBoundary
    {
        private readonly string _keyword;

        public KeywordRangeWindowBoundary(string keyword)
        {
            _keyword = Ensure.IsNotNullOrEmpty(keyword, nameof(keyword));
        }

        public string Keyword => _keyword;
    }

    public sealed class ValueRangeWindowBoundary<TValue> : RangeWindowBoundary
    {
        private readonly TValue _value;

        public ValueRangeWindowBoundary(TValue value)
        {
            _value = value;
        }

        public TValue Value => _value;
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
    }
#pragma warning restore
}
