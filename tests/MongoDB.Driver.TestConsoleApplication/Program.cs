using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;

namespace mongodb
{
	internal class Program
	{
		private static readonly FacilityAttributeStringValue Country = new()
			{Id = Guid.NewGuid(), AttributeId = Guid.NewGuid(), Value = "Spain"};

		private static async Task Main(string[] args)
		{
			BsonClassMap.RegisterClassMap<FacilityAttributeDateTimeValue>();
			BsonClassMap.RegisterClassMap<FacilityAttributeStringValue>();
			BsonClassMap.RegisterClassMap<FacilityAttributeDecimalValue>();
			BsonClassMap.RegisterClassMap<MonitoredDataSourceId>();
			BsonClassMap.RegisterClassMap<CustomDataSourceId>();

			var client =
				new MongoClient(
					"mongodb://localhost:27017/?readPreference=primary&appname=mongodb-vscode%200.4.1&ssl=false");

			await client.DropDatabaseAsync("mongo-benchmark");
			var db = client.GetDatabase("mongo-benchmark");

			var typedCollection = db.GetCollection<Facility>("facilities");
			//Insert 5000 facilities with 150 parameter and 500 attributes each

			var sw = Stopwatch.StartNew();
			var facilities = GetFacilities().ToList();
			sw.Stop();
			Console.WriteLine($"Created 5000 facilities in {sw.ElapsedMilliseconds} ms.");
			await typedCollection.InsertManyAsync(facilities);

            //Bson
			var bsonCollection = db.GetCollection<BsonDocument>("facilities");
			sw.Restart();
            var bsonValues = await (await bsonCollection
                .FindAsync(new BsonDocumentFilterDefinition<BsonDocument>(new BsonDocument()))).ToListAsync();
			sw.Stop();
			Console.WriteLine(bsonValues.Count + " BSON values took " + sw.ElapsedMilliseconds);

            //RawBson
			var rawCollection = db.GetCollection<RawBsonDocument>("facilities");
			sw.Restart();
            var rawValues = await (await rawCollection
                .FindAsync(new BsonDocumentFilterDefinition<RawBsonDocument>(new BsonDocument()))).ToListAsync();
			sw.Stop();
			Console.WriteLine(rawValues.Count + " raw values took " + sw.ElapsedMilliseconds);

            sw.Restart();
            foreach (var doc in rawValues)
            {
                foreach (var field in doc.Elements)
                {
                    switch (field.Name)
                    {
                        case "_id":
                            var id = field.Value;
                            break;
                        default:
                            foreach (var elem in (RawBsonArray)field.Value)
                            {
                                var value = elem.AsBsonValue;
                            }
                            break;
                    }
                }
            }
            sw.Stop();
			Console.WriteLine($"Iterating the raw BSON took {sw.ElapsedMilliseconds} ms.");

			//Typed
			sw.Restart();
			var typedValues = await (await typedCollection.FindAsync(new BsonDocument())).ToListAsync();
			sw.Stop();
			Console.WriteLine(typedValues.Count + " typed values took " + sw.ElapsedMilliseconds);
		}

		private static IEnumerable<Facility> GetFacilities()
		{
			var parameter = Enumerable.Range(0, 150).Select(_ => Guid.NewGuid()).ToList();
			var attributes = Enumerable.Range(0, 500).Select(_ => Guid.NewGuid()).ToList();

			for (var i = 0; i < 5000; i++)
				yield return new Facility
				{
					Id = Guid.NewGuid(),
					ParametersValues = parameter.Select(p => new ParameterValue
					{
						Id = Guid.NewGuid(),
						ParameterId = p,
						DataSourceId = new CustomDataSourceId {Id = Guid.NewGuid()}
					}).ToList(),
					AttributesValues =
						i % 3 == 0
							? new List<AttributeValue>(attributes.Take(499).Select(a =>
								new FacilityAttributeStringValue
									{Id = Guid.NewGuid(), AttributeId = a, Value = a.ToString()}).Append(Country))
							: new List<AttributeValue>(attributes.Select(a =>
								new FacilityAttributeStringValue
									{Id = Guid.NewGuid(), AttributeId = a, Value = a.ToString()}))
				};
		}
	}
}
