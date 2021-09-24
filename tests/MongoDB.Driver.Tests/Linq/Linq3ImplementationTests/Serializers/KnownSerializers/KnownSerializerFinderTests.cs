using System;
using System.Linq;
using System.Linq.Expressions;
using FluentAssertions;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson.Serialization.Serializers;
using MongoDB.Driver.Linq.Linq3Implementation.Serializers.KnownSerializers;
using Xunit;

namespace MongoDB.Driver.Tests.Linq.Linq3ImplementationTests.Serializers.KnownSerializers
{
    public class KnownSerializerFinderTests
    {
        enum E { A, B }

        class C
        {
            public int P { get; set; }
            [BsonRepresentation(BsonType.Int32)]
            public E Ei { get; set; }
            [BsonRepresentation(BsonType.String)]
            public E Es { get; set; }
            public A A { get; set; }
        }

        class A
        {
            public B B { get; set; }
        }

        class B { }

        [Fact]
        public void Identity_expression_should_return_collection_serializer()
        {
            Expression<Func<C, C>> expression = x => x;
            var collectionBsonSerializer = (IBsonDocumentSerializer)BsonSerializer.LookupSerializer<C>();
            var result = KnownSerializerFinder.FindKnownSerializers(expression, collectionBsonSerializer);
            var serializer = result.GetSerializer(expression.Body);
            serializer.Should().Be(collectionBsonSerializer);
            serializer.Should().BeOfType<BsonClassMapSerializer<C>>();
        }

        [Fact]
        public void Int_property_expression_should_return_int_serializer()
        {
            Expression<Func<C, int>> expression = x => x.P;
            var collectionBsonSerializer = (IBsonDocumentSerializer)BsonSerializer.LookupSerializer<C>();
            var result = KnownSerializerFinder.FindKnownSerializers(expression, collectionBsonSerializer);
            collectionBsonSerializer.TryGetMemberSerializationInfo(nameof(C.P), out var expectedPropertySerializer);
            var serializer = result.GetSerializer(expression.Body);
            serializer.Should().Be(expectedPropertySerializer.Serializer);
            serializer.Should().BeOfType<Int32Serializer>();
        }

        [Fact]
        public void Enum_property_expression_should_return_enum_serializer_with_int_representation()
        {
            Expression<Func<C, E>> expression = x => x.Ei;
            var collectionBsonSerializer = (IBsonDocumentSerializer)BsonSerializer.LookupSerializer<C>();
            var result = KnownSerializerFinder.FindKnownSerializers(expression, collectionBsonSerializer);
            collectionBsonSerializer.TryGetMemberSerializationInfo(nameof(C.Ei), out var expectedPropertySerializer);
            var serializer = result.GetSerializer(expression.Body);
            serializer.Should().Be(expectedPropertySerializer.Serializer);
            serializer.Should().BeOfType<EnumSerializer<E>>();
            var enumRepresentation = ((EnumSerializer<E>)serializer).Representation;
            enumRepresentation.Should().Be(BsonType.Int32);
        }

        [Fact(Skip="Deferred")]
        public void Enum_comparison_expression_should_return_enum_serializer_with_int_representation()
        {
            Expression<Func<C, bool>> expression = x => x.Ei == E.A;
            var collectionBsonSerializer = (IBsonDocumentSerializer)BsonSerializer.LookupSerializer<C>();
            var result = KnownSerializerFinder.FindKnownSerializers(expression, collectionBsonSerializer);
            collectionBsonSerializer.TryGetMemberSerializationInfo(nameof(C.Ei), out var expectedPropertySerializer);
            BinaryExpression equals = (BinaryExpression)expression.Body;
            var serializer = result.GetSerializer(equals.Right);
            serializer.Should().Be(expectedPropertySerializer.Serializer);
            serializer.Should().BeOfType<EnumSerializer<E>>();
            var enumRepresentation = ((EnumSerializer<E>)serializer).Representation;
            enumRepresentation.Should().Be(BsonType.Int32);
        }

        [Fact]
        public void Enum_property_expression_should_return_enum_serializer_with_string_representation()
        {
            Expression<Func<C, E>> expression = x => x.Es;
            var collectionBsonSerializer = (IBsonDocumentSerializer)BsonSerializer.LookupSerializer<C>();
            var result = KnownSerializerFinder.FindKnownSerializers(expression, collectionBsonSerializer);
            collectionBsonSerializer.TryGetMemberSerializationInfo(nameof(C.Es), out var expectedPropertySerializer);
            var serializer = result.GetSerializer(expression.Body);
            serializer.Should().Be(expectedPropertySerializer.Serializer);
            serializer.Should().BeOfType<EnumSerializer<E>>();
            var enumRepresentation = ((EnumSerializer<E>)serializer).Representation;
            enumRepresentation.Should().Be(BsonType.String);
        }

        [Fact(Skip="Deferred")]
        public void Enum_comparison_expression_should_return_enum_serializer_with_string_representation()
        {
            Expression<Func<C, bool>> expression = x => x.Es == E.A;
            var collectionBsonSerializer = (IBsonDocumentSerializer)BsonSerializer.LookupSerializer<C>();
            var result = KnownSerializerFinder.FindKnownSerializers(expression, collectionBsonSerializer);
            collectionBsonSerializer.TryGetMemberSerializationInfo(nameof(C.Es), out var expectedPropertySerializer);
            BinaryExpression equals = (BinaryExpression)expression.Body;
            var serializer = result.GetSerializer(equals.Right);
            serializer.Should().Be(expectedPropertySerializer.Serializer);
            serializer.Should().BeOfType<EnumSerializer<E>>();
            var enumRepresentation = ((EnumSerializer<E>)serializer).Representation;
            enumRepresentation.Should().Be(BsonType.String);
        }

        [Fact]
        public void Conditional_expression_should_return_enum_serializer_with_int_representation()
        {
            Expression<Func<C, E>> expression = x => x.Ei == E.A ? E.B : x.Ei;
            var collectionBsonSerializer = (IBsonDocumentSerializer)BsonSerializer.LookupSerializer<C>();
            var result = KnownSerializerFinder.FindKnownSerializers(expression, collectionBsonSerializer);
            collectionBsonSerializer.TryGetMemberSerializationInfo(nameof(C.Ei), out var expectedPropertySerializer);
            ConditionalExpression cond = (ConditionalExpression)expression.Body;
            var serializer = result.GetSerializer(cond.IfTrue);
            serializer.Should().Be(expectedPropertySerializer.Serializer);
            serializer.Should().BeOfType<EnumSerializer<E>>();
            var enumRepresentation = ((EnumSerializer<E>)serializer).Representation;
            enumRepresentation.Should().Be(BsonType.Int32);
        }

        [Fact]
        public void Conditional_expression_should_return_enum_serializer_with_string_representation()
        {
            Expression<Func<C, E>> expression = x => x.Es == E.A ? E.B : x.Es;
            var collectionBsonSerializer = (IBsonDocumentSerializer)BsonSerializer.LookupSerializer<C>();
            var result = KnownSerializerFinder.FindKnownSerializers(expression, collectionBsonSerializer);
            collectionBsonSerializer.TryGetMemberSerializationInfo(nameof(C.Es), out var expectedPropertySerializer);
            ConditionalExpression cond = (ConditionalExpression)expression.Body;
            var serializer = result.GetSerializer(cond.IfTrue);
            serializer.Should().Be(expectedPropertySerializer.Serializer);
            serializer.Should().BeOfType<EnumSerializer<E>>();
            var enumRepresentation = ((EnumSerializer<E>)serializer).Representation;
            enumRepresentation.Should().Be(BsonType.String);
        }

        [Fact]
        public void Conditional_expression_with_different_enum_representations_should_return_both_representations()
        {
            Expression<Func<C, E>> expression = x => x.Ei == E.A ? E.B : x.Es;
            var collectionBsonSerializer = (IBsonDocumentSerializer)BsonSerializer.LookupSerializer<C>();
            var result = KnownSerializerFinder.FindKnownSerializers(expression, collectionBsonSerializer);
            collectionBsonSerializer.TryGetMemberSerializationInfo(nameof(C.Ei), out var expectedEnumIntSerializer);
            collectionBsonSerializer.TryGetMemberSerializationInfo(nameof(C.Es), out var expectedEnumStringSerializer);
            ConditionalExpression cond = (ConditionalExpression)expression.Body;
            var possibleSerializers = result.GetPossibleSerializers(cond.IfTrue);
            possibleSerializers.Should().HaveCount(2);
            possibleSerializers.Should().Contain(expectedEnumIntSerializer.Serializer);
            possibleSerializers.Should().Contain(expectedEnumStringSerializer.Serializer);
        }

        [Fact]
        public void Property_chain_should_return_correct_nested_serializer()
        {
            Expression<Func<C, B>> expression = x => x.A.B;
            var collectionBsonSerializer = (IBsonDocumentSerializer)BsonSerializer.LookupSerializer<C>();
            var result = KnownSerializerFinder.FindKnownSerializers(expression, collectionBsonSerializer);
            collectionBsonSerializer.TryGetMemberSerializationInfo(nameof(C.A), out var aSerializer);
            ((BsonClassMapSerializer<A>)aSerializer.Serializer).TryGetMemberSerializationInfo(nameof(A.B), out var expectedSerializer);
            var possibleSerializers = result.GetPossibleSerializers(expression.Body);
            possibleSerializers.Should().HaveCount(1);
            possibleSerializers.Should().Contain(expectedSerializer.Serializer);
        }
    }
}
