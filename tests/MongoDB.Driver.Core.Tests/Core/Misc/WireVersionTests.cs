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

namespace MongoDB.Driver.Core.Tests.Core.Misc
{
    public class WireVersionTests
    {
        [Fact]
        public void FirstSupportedServerVersion_should_be_correct()
        {
            WireVersion.FirstSupportedServerVersion.Should().Be(new SemanticVersion(3, 6, 0));
        }

        [Fact]
        public void SupportedWireRange_should_be_correct()
        {
            WireVersion.SupportedWireRange.Should().Be(new Range<int>(6, 14));
        }

        [Theory]
        // 6
        [InlineData(3, 6, 0, 6)]
        [InlineData(3, 6, 99, 6)]
        [InlineData(3, 7, 0, 6)]
        [InlineData(3, 8, 0, 6)]
        [InlineData(3, 9, 9, 6)]
        [InlineData(3, 99, 99, 6)]
        // 7
        [InlineData(4, 0, 0, 7)]
        [InlineData(4, 0, 99, 7)]
        [InlineData(4, 1, 0, 7)]
        [InlineData(4, 1, 99, 7)]
        // 8
        [InlineData(4, 2, 0, 8)]
        [InlineData(4, 2, 99, 8)]
        [InlineData(4, 3, 0, 8)]
        [InlineData(4, 3, 99, 8)]
        // 9
        [InlineData(4, 4, 0, 9)]
        [InlineData(4, 4, 99, 9)]
        [InlineData(4, 5, 0, 9)]
        [InlineData(4, 6, 0, 9)]
        [InlineData(4, 6, 99, 9)]
        // 10
        [InlineData(4, 7, 0, 10)]
        [InlineData(4, 7, 99, 10)]
        // 11
        [InlineData(4, 8, 0, 11)]
        [InlineData(4, 8, 99, 11)]
        // 12
        [InlineData(4, 9, 0, 12)]
        [InlineData(4, 9, 99, 12)]
        // 13
        [InlineData(5, 0, 0, 13)]
        [InlineData(5, 0, 99, 13)]
        // 14
        [InlineData(5, 1, 0, 14)]
        [InlineData(5, 1, 99, 14)]
        // not specified servers in the mapping are mapped to the latest specified
        [InlineData(10, 0, 0, 14)]
        [InlineData(10, 0, 99, 14)]
        public void GetWireVersion_with_semanticVersion_should_get_correct_maxWireVersion(int major, int minor, int patch, int expectedMaxWireVersion)
        {
            var wireVersion = WireVersion.ToWireVersion(new SemanticVersion(major, minor, patch));

            wireVersion.Should().Be(expectedMaxWireVersion);
        }
    }
}
