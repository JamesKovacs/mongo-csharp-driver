using System;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Linq;

var settings = new MongoClientSettings { LinqProvider = LinqProvider.V3 };
var client = new MongoClient(settings);
var db = client.GetDatabase("test");
var coll = db.GetCollection<Book>("books");

var query = coll.AsQueryable()
    .Select(x => new BookDto
    {
        Id = x.Id,
        PageCount = x.PageCount,
        Author = x.Author == null
            ? null
            : new AuthorDto
            {
                Id = x.Author.Id,
                Name = x.Author.Name
            }
    });
Console.WriteLine(query);

class BookDto
{
    public ObjectId Id { get; set; }
    public int PageCount { get; set; }
    public AuthorDto Author { get; set; }
}

class AuthorDto
{
    public ObjectId Id { get; set; }
    public string Name { get; set; }
}

class Author : IEquatable<Author>
{
    public ObjectId Id { get; set; }
    public string Name { get; set; }
    public bool Equals(Author? other) => Id == other.Id && Name == other.Name;
    public static bool operator ==(Author author1, Author author2) => author1.Equals(author2);
    public static bool operator !=(Author author1, Author author2) => !author1.Equals(author2);
}

class Book
{
    public ObjectId Id { get; set; }
    public int PageCount { get; set; }
    public Author Author { get; set; }
}
