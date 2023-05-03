using FluentAssertions;
using MongoDB.Bson;
using MongoDB.Driver.Linq;
using MongoDB.TestHelpers.XunitExtensions;
using Xunit;

namespace MongoDB.Driver.Tests.Linq.Linq3ImplementationTests.Jira
{
    public class CSharp4632Tests : Linq3IntegrationTest
    {
        [Theory]
        [ParameterAttributeData]
        public void Projection_into_chained_child_ctor_should_work(
            [Values(LinqProvider.V2, LinqProvider.V3)] LinqProvider linqProvider)
        {
            var collection = GetCollection<TestDocument>("test", linqProvider);
            var doc = new TestDocument { Name = "test" };
            collection.InsertOne(doc);
            var dto = collection
                .Find(x => x.Id == doc.Id)
                .Project(x => new ChildTestDto(x.Id, x.Name))
                .First();
            dto.Id.Should().Be(doc.Id);
            dto.Name.Should().Be(doc.Name);
        }

        class TestDocument
        {
            public ObjectId Id { get; set; }
            public string Name { get; set; }
        }

        class ParentTestDto
        {
            public ObjectId Id { get; set; }
            public string Name { get; set; }

            public ParentTestDto()
            {

            }
            public ParentTestDto(ObjectId id, string name)
            {
                Id = id;
                Name = name;
            }
        }

        class ChildTestDto : ParentTestDto
        {
            public ChildTestDto()
            {

            }
            public ChildTestDto(ObjectId id, string name) : base(id, name)
            {
            }
        }
    }
}
