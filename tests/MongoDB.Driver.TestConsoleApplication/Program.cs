using System;
using System.Collections.Generic;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Encryption;

var uri = "mongodb+srv://james:aVEQ3ry7Do3O5xS6@cluster0.1foxh.mongodb.net/test?retryWrites=true&w=majority";
var settings = MongoClientSettings.FromConnectionString(uri);

var localMasterKey = Convert.FromBase64String("Mng0NCt4ZHVUYUJCa1kxNkVyNUR1QURhZ2h2UzR2d2RrZzh0cFBwM3R6NmdWMDFBMUN3YkQ5aXRRMkhGRGdQV09wOGVNYUMxT2k3NjZKelhaQmRCZGJkTXVyZG9uSjFk");
var kmsProviders = new Dictionary<string, IReadOnlyDictionary<string, object>>();
var localKey = new Dictionary<string, object>
{
    { "key", localMasterKey }
};
kmsProviders.Add("local", localKey);
var keyVaultNamespace = CollectionNamespace.FromFullName("encryption.__keyVault");
var autoEncryptionOptions = new AutoEncryptionOptions(keyVaultNamespace, kmsProviders);

settings.AutoEncryptionOptions = autoEncryptionOptions;
var client = new MongoClient(settings);
var db = client.GetDatabase("sample_mflix");
var collection = db.GetCollection<BsonDocument>("movies");

// Simple search example
var searchStage = @"
{
    $search : {
        text : {
            query : 'baseball',
            path : 'plot'
        }
    }
}";
var projection = @"
{
    _id : 0,
    title : 1,
    plot : 1
}";
var pipeline = new EmptyPipelineDefinition<BsonDocument>()
    .AppendStage<BsonDocument, BsonDocument, BsonDocument>(searchStage)
    .Project(projection);
var moviesAboutBaseball = collection.Aggregate(pipeline).ToList();

Console.WriteLine($"{moviesAboutBaseball.Count} movies about baseball:");
foreach (var movie in moviesAboutBaseball)
{
    Console.WriteLine($"  {movie}");
}
Console.WriteLine();
