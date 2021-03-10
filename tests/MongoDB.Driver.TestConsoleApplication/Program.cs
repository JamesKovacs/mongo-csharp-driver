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
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;

var type = typeof(Widget1);
Console.WriteLine(type.FullName);

var client = new MongoClient();
var db = client.GetDatabase("polymorphismTest");
var col = db.GetCollection<IFoo>("iFoo");

// Clean up any existing documents
await col.DeleteManyAsync(Builders<IFoo>.Filter.Empty);

// Insert 2 new test documents of different types, each implementing IFoo
await col.InsertOneAsync(new Widget1() { Id = ObjectId.GenerateNewId(), SomeField = "ABC", Bar = 10 });
await col.InsertOneAsync(new Widget2() { Id = ObjectId.GenerateNewId(), SomeField = "ABC", Bar = 10M });

// Build a query using .OfType<> on the collection.
var widget1QueryColOfType = col.OfType<Widget1>().Find(Builders<Widget1>.Filter.Eq(x => x.SomeField, "ABC"));
var widget2QueryColOfType = col.OfType<Widget2>().Find(Builders<Widget2>.Filter.Eq(x => x.SomeField, "ABC"));
Console.WriteLine("OfType<> on the Collection");
Console.WriteLine(widget1QueryColOfType.ToString());
widget1QueryColOfType.ToList().ForEach(Console.WriteLine);
Console.WriteLine(widget2QueryColOfType.ToString());
widget2QueryColOfType.ToList();
Console.WriteLine();

// Build a query using .OfType<> on the Builder to change from IFoo to the concrete type.
var widget1QueryBuilderOfType = col.Find(Builders<IFoo>.Filter.OfType<Widget1>(Builders<Widget1>.Filter.Eq(x => x.SomeField, "ABC")));
var widget2QueryBuilderOfType = col.Find(Builders<IFoo>.Filter.OfType<Widget2>(Builders<Widget2>.Filter.Eq(x => x.SomeField, "ABC")));
Console.WriteLine("OfType<> on the Builder");
Console.WriteLine(widget1QueryBuilderOfType.ToString());
Console.WriteLine(widget2QueryBuilderOfType.ToString());
Console.WriteLine();

Console.WriteLine("Documents in the database");
var rawDocsInDatabase = await db.GetCollection<BsonDocument>("iFoo").Find(Builders<BsonDocument>.Filter.Empty).ToListAsync();
foreach(var doc in rawDocsInDatabase)
    Console.WriteLine(doc.ToJson());

public interface IFoo
{
	ObjectId Id { get; set;}
	string SomeField { get; set;}
}

public class Widget1 : IFoo
{
	public ObjectId Id {get;set;}
	public string SomeField { get; set; }
	public int Bar { get; set; }

    public override string ToString() => $"Widget1: Id={Id}, SomeField={SomeField}, Bar={Bar}";
}

public class Widget2 : IFoo
{
	public ObjectId Id {get;set;}
	public string SomeField { get; set; }
	public Decimal Bar { get; set; }

    public override string ToString() => $"Widget2: Id={Id}, SomeField={SomeField}, Bar={Bar}";
}
