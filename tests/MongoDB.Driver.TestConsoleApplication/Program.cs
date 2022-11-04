using System;
using System.IO;
using MongoDB.Bson;
using Newtonsoft.Json;

BsonDefaults.GuidRepresentationMode = GuidRepresentationMode.V3;

var jsonSerializer = new JsonSerializer();
var bson = new BsonDocument
{
    // { "array", new BsonArray { 1, 2, 3 } },
    // { "string", "Hello, world!" },
    // { "null", BsonNull.Value },
    // { "timestamp", BsonTimestamp.Create(42L)},
    // { "binary", new BsonBinaryData(Guid.NewGuid(), GuidRepresentation.Standard) },
    { "minKey", BsonMinKey.Value },
    { "maxKey", BsonMaxKey.Value }
};
Console.WriteLine(bson.ToJson());

using var stringWriter = new StringWriter();
jsonSerializer.Serialize(stringWriter, bson);
Console.WriteLine(stringWriter.ToString());

/*
using var memory = new MemoryStream();
using var writer = new BsonBinaryWriter(memory);
var context = BsonSerializationContext.CreateRoot(writer);
BsonDocumentSerializer.Instance.Serialize(context, new BsonSerializationArgs(), bson);
memory.Seek(0, SeekOrigin.Begin);
var raw = new RawBsonDocument(memory.GetBuffer());
Console.WriteLine(raw.ToJson());

using (var stringWriter = new StringWriter())
{
    jsonSerializer.Serialize(stringWriter, raw);
    Console.WriteLine(stringWriter.ToString());
}
*/
