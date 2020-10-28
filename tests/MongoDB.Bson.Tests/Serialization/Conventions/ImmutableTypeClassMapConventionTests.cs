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
using System.Linq;
using FluentAssertions;
using MongoDB.Bson;
using MongoDB.Bson.IO;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson.Serialization.Conventions;
using Xunit;

namespace MongoDB.Bson.Tests.Serialization.Conventions
{
    public class ImmutableTypeClassMapConventionTests
    {
        private class TestClassA
        {
            public string A { get; set; }
            public string B { get; set; }
        }

        private class TestClassB
        {
            public string A { get; }
            public string B { get; }

            public TestClassB(string a, string b)
            {
                A = a;
                B = b;
            }
        }

        private class TestClassC
        {
            public string A { get; }
            public string B { get; }

            public TestClassC(string a)
                : this(a, null)
            {
            }

            public TestClassC(string a, string b)
            {
                A = a;
                B = b;
            }
        }

        private class TestClassD
        {
            public TestClassD(int a) { A = a; }

            public static TestClassD Default => new TestClassD(0);

            public int A { get; }
        }

        private class TestClassE
        {
            public TestClassE(int a) { A = a; }

            public static TestClassE Default => new TestClassE(0);

            public int A { get; set; }
        }

        private class TestClassF
        {
            public TestClassF(int a) { A = a; }

            public static TestClassF Default { get; set; } = new TestClassF(0);

            public int A { get; }
        }

        [Fact]
        public void Anonymous_class_should_map_correctly()
        {
            var c = new { A = 1, B = 2 };
            var classMap = BsonClassMap.LookupClassMap(c.GetType());

            classMap.DeclaredMemberMaps.Select(m => m.MemberName).Should().Equal("A", "B");
            classMap.CreatorMaps.Count().Should().Be(1);
        }

        [Fact]
        public void TestClassA_should_map_correctly()
        {
            var classMap = BsonClassMap.LookupClassMap(typeof(TestClassA));

            classMap.DeclaredMemberMaps.Select(m => m.MemberName).Should().Equal("A", "B");
            classMap.CreatorMaps.Count().Should().Be(0);
        }

        [Fact]
        public void TestClassB_should_map_correctly()
        {
            var classMap = BsonClassMap.LookupClassMap(typeof(TestClassB));

            classMap.DeclaredMemberMaps.Select(m => m.MemberName).Should().Equal("A", "B");
            classMap.CreatorMaps.Count().Should().Be(1);
        }

        [Fact]
        public void TestClassC_should_map_correctly()
        {
            var classMap = BsonClassMap.LookupClassMap(typeof(TestClassC));

            classMap.DeclaredMemberMaps.Select(m => m.MemberName).Should().Equal("A", "B");
            classMap.CreatorMaps.Count().Should().Be(1);
        }

        [Fact]
        public void TestNoDefaultConstructorClassMapConventionWithTestClassA()
        {
            var convention = new ImmutableTypeClassMapConvention();
            var classMap = new BsonClassMap<TestClassA>();
            convention.Apply(classMap);
            Assert.False(classMap.HasCreatorMaps);
        }

        [Fact]
        public void TestNoDefaultConstructorClassMapConventionWithTestClassB()
        {
            var convention = new ImmutableTypeClassMapConvention();
            var classMap = new BsonClassMap<TestClassB>();
            convention.Apply(classMap);
            Assert.True(classMap.HasCreatorMaps);
            Assert.Equal(1, classMap.CreatorMaps.Count());
        }

        [Fact]
        public void TestNoDefaultConstructorClassMapConventionWithTestClassC()
        {
            var convention = new ImmutableTypeClassMapConvention();
            var classMap = new BsonClassMap<TestClassC>();
            convention.Apply(classMap);
            Assert.True(classMap.HasCreatorMaps);
            Assert.Equal(1, classMap.CreatorMaps.Count());
        }

        [Fact]
        public void TestNoDefaultConstructorClassMapConventionWithTestClassD()
        {
            var convention = new ImmutableTypeClassMapConvention();
            var classMap = new BsonClassMap<TestClassD>();
            convention.Apply(classMap);
            Assert.True(classMap.HasCreatorMaps);
            Assert.Equal(1, classMap.CreatorMaps.Count());
        }

        [Fact]
        public void TestNoDefaultConstructorClassMapConventionWithTestClassE()
        {
            var convention = new ImmutableTypeClassMapConvention();
            var classMap = new BsonClassMap<TestClassE>();
            convention.Apply(classMap);
            Assert.False(classMap.HasCreatorMaps);
            Assert.Equal(0, classMap.CreatorMaps.Count());
        }

        [Fact]
        public void TestNoDefaultConstructorClassMapConventionWithTestClassF()
        {
            var convention = new ImmutableTypeClassMapConvention();
            var classMap = new BsonClassMap<TestClassF>();
            convention.Apply(classMap);
            Assert.True(classMap.HasCreatorMaps);
            Assert.Equal(1, classMap.CreatorMaps.Count());
        }

        [BsonDiscriminator(RootClass=true)]
        [BsonKnownTypes(typeof(Apple))]
        private abstract class Fruit
        {
            [BsonIgnore]
            public abstract string Colour { get; }
        }

        private class Apple : Fruit
        {
            [BsonConstructor]
            public Apple(int seeds)
            {
                Seeds = seeds;
            }

            public int Seeds { get; }

            [BsonIgnore]
            public override string Colour => "Red";
        }

        [Fact]
        public void CanMapImmutableFruit()
        {
            var convention1 = new ImmutableTypeClassMapConvention();
            var convention2 = new NamedParameterCreatorMapConvention();
            var classMap = new BsonClassMap<Apple>();
            convention1.Apply(classMap);
            foreach (var creatorMap in classMap.CreatorMaps)
            {
                convention2.Apply(creatorMap);
            }
            Assert.True(classMap.HasCreatorMaps);
            Assert.Single(classMap.CreatorMaps);

            classMap.Freeze();
            var serializer = new BsonClassMapSerializer<Apple>(classMap);

            const int numSeeds = 42;
            var reader = new BsonDocumentReader(new BsonDocument {{"Seeds", numSeeds}, {"Colour", "green"}});
            var context = BsonDeserializationContext.CreateRoot(reader);
            var apple = serializer.Deserialize(context);
            Assert.Equal(numSeeds, apple.Seeds);
        }

        private class ImmutableClassWithConstants
        {
            public ImmutableClassWithConstants(int someInteger)
            {
                SomeInteger = someInteger;
            }

            public int SomeInteger { get; private set; }

            [BsonIgnore]
            public string Greeting => "Hello, world!";
        }

        [Fact]
        public void CanMapImmutableClassWithConstants()
        {
            var convention1 = new ImmutableTypeClassMapConvention();
            var convention2 = new NamedParameterCreatorMapConvention();
            var classMap = new BsonClassMap<ImmutableClassWithConstants>();
            convention1.Apply(classMap);
            foreach (var creatorMap in classMap.CreatorMaps)
            {
                convention2.Apply(creatorMap);
            }
            Assert.True(classMap.HasCreatorMaps);
            Assert.Single(classMap.CreatorMaps);

            classMap.Freeze();
            var serializer = new BsonClassMapSerializer<ImmutableClassWithConstants>(classMap);

            const int dbConstant = 42;
            var reader = new BsonDocumentReader(new BsonDocument {{"SomeInteger", dbConstant}});
            var context = BsonDeserializationContext.CreateRoot(reader);
            var obj = serializer.Deserialize(context);
            Assert.Equal(dbConstant, obj.SomeInteger);
        }
    }
}
