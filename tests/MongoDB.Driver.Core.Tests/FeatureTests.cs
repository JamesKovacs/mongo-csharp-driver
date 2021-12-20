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

using FluentAssertions;
using MongoDB.Driver.Core.Misc;
using Xunit;

namespace MongoDB.Driver.Core.Tests
{
    public class FeatureTests
    {
        [Theory]
        [InlineData(/*supported wireRange*/6, 7, /*feature*/8, 12, /*isSupported*/false)]
        [InlineData(/*supported wireRange*/13, 14, /*feature*/8, 12, /*isSupported*/false)]
        [InlineData(/*supported wireRange*/0, 14, /*feature*/8, 12, /*isSupported*/true)]
        [InlineData(/*supported wireRange*/12, 14, /*feature*/8, 12, /*isSupported*/true)]
        [InlineData(/*supported wireRange*/6, 7, /*feature*/0, 12, /*isSupported*/true)]
        [InlineData(/*supported wireRange*/6, 6, /*feature*/5, 6, /*isSupported*/true)]
        [InlineData(/*supported wireRange*/6, 6, /*feature*/6, 7, /*isSupported*/true)]
        public void IsSupported_should_return_correct_result(
            int minSupportedWireVersion, int maxSupportedWireVersion,
            int featureIsAddedWireVersion, int featureIsRemovedWireVersion,
            bool isSupported)
        {
            var wireRange = new Range<int>(minSupportedWireVersion, maxSupportedWireVersion);

            var feature = new Feature("test", WireVersion.GetWireVersion(featureIsAddedWireVersion).FirstSupportedVersion, WireVersion.GetWireVersion(featureIsRemovedWireVersion).FirstSupportedVersion);
            var result = feature.IsSupported(wireRange);

            result.Should().Be(isSupported);
        }
    }
}
