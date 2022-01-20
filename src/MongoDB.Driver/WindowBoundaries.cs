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
    public abstract class WindowBoundaries
    {
        #region static
        public static DocumentsWindowBoundaries Documents(int lowerBoundary, int upperBoundary)
        {
            return new DocumentsWindowBoundaries(new PositionDocumentsWindowBoundary(lowerBoundary), new PositionDocumentsWindowBoundary(upperBoundary));
        }

        public static DocumentsWindowBoundaries Documents(KeywordWindowBoundary lowerBoundary, int upperBoundary)
        {
            return new DocumentsWindowBoundaries(new KeywordDocumentsWindowBoundary(lowerBoundary.Keyword), new PositionDocumentsWindowBoundary(upperBoundary));
        }

        public static DocumentsWindowBoundaries Documents(int lowerBoundary, KeywordWindowBoundary upperBoundary)
        {
            return new DocumentsWindowBoundaries(new PositionDocumentsWindowBoundary(lowerBoundary), new KeywordDocumentsWindowBoundary(upperBoundary.Keyword));
        }

        public static DocumentsWindowBoundaries Documents(KeywordWindowBoundary lowerBoundary, KeywordWindowBoundary upperBoundary)
        {
            return new DocumentsWindowBoundaries(new KeywordDocumentsWindowBoundary(lowerBoundary.Keyword), new KeywordDocumentsWindowBoundary(upperBoundary.Keyword));
        }

        public static RangeWindowBoundaries Range<TValue>(TValue lowerBoundary, TValue upperBoundary)
        {
            return new RangeWindowBoundaries(new ValueRangeWindowBoundary<TValue>(lowerBoundary), new ValueRangeWindowBoundary<TValue>(upperBoundary));
        }

        public static RangeWindowBoundaries Range<TValue>(KeywordWindowBoundary lowerBoundary, TValue upperBoundary)
        {
            return new RangeWindowBoundaries(new KeywordRangeWindowBoundary(lowerBoundary.Keyword), new ValueRangeWindowBoundary<TValue>(upperBoundary));
        }

        public static RangeWindowBoundaries Range<TValue>(TValue lowerBoundary, KeywordWindowBoundary upperBoundary)
        {
            return new RangeWindowBoundaries(new ValueRangeWindowBoundary<TValue>(lowerBoundary), new KeywordRangeWindowBoundary(upperBoundary.Keyword));
        }

        public static RangeWindowBoundaries Range(KeywordWindowBoundary lowerBoundary, KeywordWindowBoundary upperBoundary)
        {
            return new RangeWindowBoundaries(new KeywordRangeWindowBoundary(lowerBoundary.Keyword), new KeywordRangeWindowBoundary(upperBoundary.Keyword));
        }
        #endregion
    }

    public sealed class DocumentsWindowBoundaries : WindowBoundaries
    {
        private readonly DocumentsWindowBoundary _lowerBoundary;
        private readonly DocumentsWindowBoundary _upperBoundary;

        public DocumentsWindowBoundaries(DocumentsWindowBoundary lowerBoundary, DocumentsWindowBoundary upperBoundary)
        {
            _lowerBoundary = Ensure.IsNotNull(lowerBoundary, nameof(lowerBoundary));
            _upperBoundary = Ensure.IsNotNull(upperBoundary, nameof(upperBoundary));
        }

        public DocumentsWindowBoundary LowerBoundary => _lowerBoundary;
        public DocumentsWindowBoundary UpperBoundary => _upperBoundary;
    }

    public sealed class RangeWindowBoundaries : WindowBoundaries
    {
        private readonly RangeWindowBoundary _lowerBoundary;
        private readonly RangeWindowBoundary _upperBoundary;

        public RangeWindowBoundaries(RangeWindowBoundary lowerBoundary, RangeWindowBoundary upperBoundary)
        {
            _lowerBoundary = Ensure.IsNotNull(lowerBoundary, nameof(lowerBoundary));
            _upperBoundary = Ensure.IsNotNull(upperBoundary, nameof(upperBoundary));
        }

        public RangeWindowBoundary LowerBoundary => _lowerBoundary;
        public RangeWindowBoundary UpperBoundary => _upperBoundary;
    }
#pragma warning restore
}
