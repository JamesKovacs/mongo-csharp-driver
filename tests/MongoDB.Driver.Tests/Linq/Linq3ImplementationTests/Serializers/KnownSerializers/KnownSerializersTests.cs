using System.Linq;
using FluentAssertions;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson.Serialization.Serializers;
using MongoDB.Driver.Linq;
using MongoDB.Driver.Linq.Linq3Implementation;
using MongoDB.Driver.Linq.Linq3Implementation.Translators.ExpressionToExecutableQueryTranslators;
using Xunit;

namespace MongoDB.Driver.Tests.Linq.Linq3ImplementationTests.Serializers.KnownSerializers
{
    public class KnownSerializersTests
    {
        private static readonly IMongoClient __client;
        private static readonly IMongoCollection<C> __collection;
        private static readonly IMongoCollection<C2> __collection2;
        private static readonly IMongoCollection<C3> __collection3;
        private static readonly IMongoDatabase __database;

        static KnownSerializersTests()
        {
            __client = DriverTestConfiguration.Client;
            __database = __client.GetDatabase(DriverTestConfiguration.DatabaseNamespace.DatabaseName);
            __collection = __database.GetCollection<C>(DriverTestConfiguration.CollectionNamespace.CollectionName);
            __collection2 = __database.GetCollection<C2>(DriverTestConfiguration.CollectionNamespace.CollectionName);

            BsonClassMap.RegisterClassMap<C3>(cm =>
            {
                cm.AutoMap();
                cm.MapMember(p => p.E).SetSerializer(new EnumSerializer<E>(BsonType.String));
            });
            __collection3 = __database.GetCollection<C3>(DriverTestConfiguration.CollectionNamespace.CollectionName);
        }

        public enum E { A, B };

        public class C
        {
            public E E { get; set; }
        }

        public class C2
        {
            [BsonRepresentation(BsonType.String)]
            public E E { get; set; }
        }

        public class C3
        {
            public E E { get; set; }
        }

        [Theory]
        [InlineData(E.A, "{ \"E\" : 0 }")]
        [InlineData(E.B, "{ \"E\" : 1 }")]
        public void Where_operator_equal_should_render_correctly(E value, string expectedFilter)
        {
            var subject = __collection.AsQueryable3();

            var queryable = subject.Where(x => x.E == value);

            AssertFilter(queryable, expectedFilter);
        }

        [Theory]
        [InlineData(E.A, "{ \"E\" : \"A\" }")]
        [InlineData(E.B, "{ \"E\" : \"B\" }")]
        public void Where_operator_equal_should_render_enum_as_string(E value, string expectedFilter)
        {
            var subject = __collection2.AsQueryable3();

            var queryable = subject.Where(x => x.E == value);

            AssertFilter(queryable, expectedFilter);
        }

        [Theory]
        [InlineData(E.A, "{ \"E\" : \"A\" }")]
        [InlineData(E.B, "{ \"E\" : \"B\" }")]
        public void Where_operator_equal_should_render_enum_as_string_when_configured_with_class_map(E value, string expectedFilter)
        {
            var subject = __collection3.AsQueryable3();

            var queryable = subject.Where(x => x.E == value);

            AssertFilter(queryable, expectedFilter);
        }

        // private methods
        private static void AssertFilter<T>(IQueryable<T> queryable, string expectedFilter)
        {
            var stages = Translate(queryable);
            stages.Should().HaveCount(1);
            stages[0].Should().Be($"{{ \"$match\" : {expectedFilter} }}");
        }

        private static BsonDocument[] Translate<T>(IQueryable<T> queryable)
        {
            var provider = (MongoQueryProvider<T>)queryable.Provider;
            var executableQuery = ExpressionToExecutableQueryTranslator.Translate<T, T>(provider, queryable.Expression);
            return executableQuery.Pipeline.Stages.Select(s => (BsonDocument)s.Render()).ToArray();
        }
    }
}
