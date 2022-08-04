using System;
using System.Linq;
using FluentAssertions;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson.Serialization.Serializers;
using MongoDB.Driver.Linq;
using MongoDB.Driver.Tests.Linq.Linq3ImplementationTests;
using Xunit;

namespace MongoDB.Driver.Tests.Jira
{
    public class CSharp4261 : Linq3IntegrationTest
    {
        [Fact]
        public void Linq3_should_respect_custom_BsonSerializerAttributes()
        {
            var person = new Person { Id = ObjectId.GenerateNewId().ToString() };
            var collection = GetCollection<Person>();
            CreateCollection(collection, person);

            var query = collection.AsQueryable().Where(x => x.Id == person.Id);
            var stages = Translate(collection, query);
            AssertStages(stages, $"{{ $match: {{ _id: ObjectId(\"{person.Id}\") }} }}");
        }

        private class Person
        {
            // [BsonRepresentation(BsonType.ObjectId)]
            [AsObjectId]
            public string Id { get; set; }
        }

        [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
        private class AsObjectIdAttribute : BsonSerializerAttribute
        {
            public AsObjectIdAttribute() : base(typeof(ObjectIdSerializer)) { }

            private class ObjectIdSerializer : SerializerBase<string>//, IRepresentationConfigurable<ObjectIdSerializer>
            {
                public override void Serialize(BsonSerializationContext ctx, BsonSerializationArgs args, string value)
                {
                    if (value == null)
                    {
                        ctx.Writer.WriteNull(); return;
                    }

                    if (value.Length == 24 && ObjectId.TryParse(value, out var oID))
                    {
                        ctx.Writer.WriteObjectId(oID); return;
                    }

                    ctx.Writer.WriteString(value);
                }

                public override string Deserialize(BsonDeserializationContext ctx, BsonDeserializationArgs args)
                {
                    switch (ctx.Reader.CurrentBsonType)
                    {
                        case BsonType.String:
                            return ctx.Reader.ReadString();

                        case BsonType.ObjectId:
                            return ctx.Reader.ReadObjectId().ToString();

                        case BsonType.Null:
                            ctx.Reader.ReadNull();
                            return null;

                        default:
                            throw new BsonSerializationException($"'{ctx.Reader.CurrentBsonType}' values are not valid on properties decorated with an [AsObjectId] attribute!");
                    }
                }

                // public BsonType Representation => BsonType.ObjectId;
                // public ObjectIdSerializer WithRepresentation(BsonType representation) => throw new NotImplementedException();

                // IBsonSerializer IRepresentationConfigurable.WithRepresentation(BsonType representation) => WithRepresentation(representation);
            }
        }

    }
}
