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
using FluentAssertions;
using Xunit;

namespace MongoDB.Driver.Core.Misc
{
    public class InterlockedEnumInt32Tests
    {
        public enum SampleEnum
        {
            Value0,
            Value1,
            Value2
        }

        public enum SampleEnumSByte : long { }
        public enum SampleEnumByte: byte { }
        public enum SampleEnumShort : short { }
        public enum SampleEnumUShort : ushort { }
        public enum SampleEnumUInt : uint { }
        public enum SampleEnumLong : long { }
        public enum SampleEnumULong : ulong { }

        [Fact]
        public void Value_should_return_initial_value_after_construction()
        {
            var subject = new InterlockedEnumInt32<SampleEnum>(SampleEnum.Value1);

            subject.Value.Should().Be(SampleEnum.Value1);
        }

        [Theory]
        [InlineData((SampleEnumSByte)0)]
        [InlineData((SampleEnumByte)0)]
        [InlineData((SampleEnumShort)0)]
        [InlineData((SampleEnumUShort)0)]
        [InlineData((SampleEnumLong)0)]
        [InlineData((SampleEnumULong)0)]
        public void Should_support_only_int32_enum<T>(T parameter) where T : Enum
        {
            var exception = Record.Exception(() => new InterlockedEnumInt32<T>(parameter))
                .Should().BeOfType<ArgumentOutOfRangeException>().Subject;
            exception.ParamName.Should().Be(nameof(T));
        }

        [Theory]
        [InlineData(SampleEnum.Value0, SampleEnum.Value0, SampleEnum.Value0, false)]
        [InlineData(SampleEnum.Value0, SampleEnum.Value1, SampleEnum.Value1, true)]
        [InlineData(SampleEnum.Value1, SampleEnum.Value0, SampleEnum.Value0, true)]
        [InlineData(SampleEnum.Value1, SampleEnum.Value1, SampleEnum.Value1, false)]
        public void TryChange_with_one_parameter(SampleEnum initialValue, SampleEnum toValue, SampleEnum expectedValue, bool expectedResult)
        {
            var subject = new InterlockedEnumInt32<SampleEnum>(initialValue);
            var result = subject.TryChange(toValue);
            subject.Value.Should().Be(expectedValue);
            result.Should().Be(expectedResult);
        }

        [Theory]
        [InlineData(SampleEnum.Value0, SampleEnum.Value0, SampleEnum.Value1, SampleEnum.Value1, true)]
        [InlineData(SampleEnum.Value0, SampleEnum.Value1, SampleEnum.Value2, SampleEnum.Value0, false)]
        [InlineData(SampleEnum.Value1, SampleEnum.Value0, SampleEnum.Value1, SampleEnum.Value1, false)]
        [InlineData(SampleEnum.Value1, SampleEnum.Value1, SampleEnum.Value2, SampleEnum.Value2, true)]
        public void TryChange_with_two_parameters(SampleEnum startingValue, SampleEnum fromValue, SampleEnum toValue, SampleEnum expectedValue, bool expectedResult)
        {
            var subject = new InterlockedEnumInt32<SampleEnum>(startingValue);
            var result = subject.TryChange(fromValue, toValue);
            subject.Value.Should().Be(expectedValue);
            result.Should().Be(expectedResult);
        }

        [Fact]
        public void TryChange_with_two_parameters_should_throw_if_values_are_equal()
        {
            var subject = new InterlockedEnumInt32<SampleEnum>(SampleEnum.Value0);
            Action action = () => subject.TryChange(SampleEnum.Value1, SampleEnum.Value1);
            action.ShouldThrow<ArgumentException>();
        }
    }
}
