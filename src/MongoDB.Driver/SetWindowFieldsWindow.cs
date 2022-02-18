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
using MongoDB.Driver.Core.Misc;

namespace MongoDB.Driver
{
#pragma warning disable CS1591
    public abstract class SetWindowFieldsWindow
    {
    }

    public sealed class DocumentsWindow : SetWindowFieldsWindow
    {
        #region static
        private static readonly KeywordDocumentsWindowBoundary __current = new KeywordDocumentsWindowBoundary("current");
        private static readonly KeywordDocumentsWindowBoundary __unbounded = new KeywordDocumentsWindowBoundary("unbounded");

        public static DocumentsWindow Create(int lowerBoundary, int upperBoundary)
        {
            return new DocumentsWindow(new PositionDocumentsWindowBoundary(lowerBoundary), new PositionDocumentsWindowBoundary(upperBoundary));
        }

        public static DocumentsWindow Create(int lowerBoundary, KeywordDocumentsWindowBoundary upperBoundary)
        {
            return new DocumentsWindow(new PositionDocumentsWindowBoundary(lowerBoundary), upperBoundary);
        }

        public static DocumentsWindow Create(KeywordDocumentsWindowBoundary lowerBoundary, int upperBoundary)
        {
            return new DocumentsWindow(lowerBoundary, new PositionDocumentsWindowBoundary(upperBoundary));
        }

        public static DocumentsWindow Create(KeywordDocumentsWindowBoundary lowerBoundary, KeywordDocumentsWindowBoundary upperBoundary)
        {
            return new DocumentsWindow(lowerBoundary, upperBoundary);
        }

        public static KeywordDocumentsWindowBoundary Current => __current;
        public static KeywordDocumentsWindowBoundary Unbounded => __unbounded;
        #endregion

        private readonly DocumentsWindowBoundary _lowerBoundary;
        private readonly DocumentsWindowBoundary _upperBoundary;

        public DocumentsWindow(DocumentsWindowBoundary lowerBoundary, DocumentsWindowBoundary upperBoundary)
        {
            _lowerBoundary = Ensure.IsNotNull(lowerBoundary, nameof(lowerBoundary));
            _upperBoundary = Ensure.IsNotNull(upperBoundary, nameof(upperBoundary));
        }

        public DocumentsWindowBoundary LowerBoundary => _lowerBoundary;
        public DocumentsWindowBoundary UpperBoundary => _upperBoundary;

        public override string ToString() => $"documents : [{_lowerBoundary}, {_upperBoundary}]";
    }

    public sealed class RangeWindow : SetWindowFieldsWindow
    {
        #region static
        private static readonly KeywordRangeWindowBoundary __current = new KeywordRangeWindowBoundary("current");
        private static readonly KeywordRangeWindowBoundary __unbounded = new KeywordRangeWindowBoundary("unbounded");

        public static KeywordRangeWindowBoundary Current => __current;
        public static KeywordRangeWindowBoundary Unbounded => __unbounded;

        public static RangeWindow Create<TValue>(TValue lowerBoundary, TValue upperBoundary)
        {
            return new RangeWindow(new ValueRangeWindowBoundary<TValue>(lowerBoundary), new ValueRangeWindowBoundary<TValue>(upperBoundary));
        }

        public static RangeWindow Create<TValue>(TValue lowerBoundary, KeywordRangeWindowBoundary upperBoundary)
        {
            return new RangeWindow(new ValueRangeWindowBoundary<TValue>(lowerBoundary), upperBoundary);
        }

        public static RangeWindow Create<TValue>(KeywordRangeWindowBoundary lowerBoundary, TValue upperBoundary)
        {
            return new RangeWindow(lowerBoundary, new ValueRangeWindowBoundary<TValue>(upperBoundary));
        }

        public static RangeWindow Create(TimeRangeWindowBoundary lowerBoundary, TimeRangeWindowBoundary upperBoundary)
        {
            return new RangeWindow(lowerBoundary, upperBoundary);
        }

        public static RangeWindow Create(TimeRangeWindowBoundary lowerBoundary, KeywordRangeWindowBoundary upperBoundary)
        {
            return new RangeWindow(lowerBoundary, upperBoundary);
        }

        public static RangeWindow Create(KeywordRangeWindowBoundary lowerBoundary, TimeRangeWindowBoundary upperBoundary)
        {
            return new RangeWindow(lowerBoundary, upperBoundary);
        }

        public static RangeWindow Create(KeywordRangeWindowBoundary lowerBoundary, KeywordRangeWindowBoundary upperBoundary)
        {
            return new RangeWindow(lowerBoundary, upperBoundary);
        }

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

        private readonly RangeWindowBoundary _lowerBoundary;
        private readonly RangeWindowBoundary _upperBoundary;
        private readonly string _unit;

        public RangeWindow(RangeWindowBoundary lowerBoundary, RangeWindowBoundary upperBoundary)
        {
            _lowerBoundary = Ensure.IsNotNull(lowerBoundary, nameof(lowerBoundary));
            _upperBoundary = Ensure.IsNotNull(upperBoundary, nameof(upperBoundary));

            if (_lowerBoundary is TimeRangeWindowBoundary timeLowerBoundary &&
                _upperBoundary is TimeRangeWindowBoundary timeUpperBoundary)
            {
                if (timeLowerBoundary.Unit != timeUpperBoundary.Unit)
                {
                    throw new ArgumentException("Lower and upper time-based boundaries must use the same units.");
                }

                _unit = timeLowerBoundary.Unit;
            }
        }

        public RangeWindowBoundary LowerBoundary => _lowerBoundary;
        public RangeWindowBoundary UpperBoundary => _upperBoundary;

        public override string ToString()
        {
            var unit = (_lowerBoundary as TimeRangeWindowBoundary)?.Unit ?? (_upperBoundary as TimeRangeWindowBoundary)?.Unit;
            if (unit != null)
            {
                return $"range : [{_lowerBoundary}, {_upperBoundary}], unit : \"{unit}\"";
            }
            else
            {
                return $"range : [{_lowerBoundary}, {_upperBoundary}]";
            }
        }
    }
#pragma warning restore
}
