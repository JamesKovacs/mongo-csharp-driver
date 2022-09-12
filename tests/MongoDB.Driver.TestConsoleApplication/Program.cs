using System;
using System.Net.Sockets;
using MongoDB.Bson;
using MongoDB.Driver;

void Configure(Socket socket) => socket.SetRawSocketOption((int)SocketOptionLevel.Tcp, 0x105, (byte[])BitConverter.GetBytes(1));
var settings = new MongoClientSettings
{
    ClusterConfigurator = cb => cb.ConfigureTcp(tcp => tcp.With(socketConfigurator: (Action<Socket>)Configure))
};
var client = new MongoClient(settings);
var db = client.GetDatabase("test");
var result = db.RunCommand<BsonDocument>("{ping:1}");
Console.WriteLine(result);
