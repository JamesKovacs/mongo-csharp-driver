using System;
using System.Linq;
using MongoDB.Bson;
using MongoDB.Driver;

var client = new MongoClient();
var db = client.GetDatabase("test");
var coll = db.GetCollection<Series>("library");

// var book1 = new Book(ObjectId.GenerateNewId(), [new Chapter("Chapter 1"), new Chapter("Chapter 2")]);
// var book2 = new Book(ObjectId.GenerateNewId(), [new Chapter("John Coltrane"), new Chapter("Miles Davis"), new Chapter("Weather Report")]);
// var book3 = new Book(ObjectId.GenerateNewId(), [new Chapter("Life"), new Chapter("The Universe"), new Chapter("Everything"), new Chapter("42")]);
// var series1 = new Series([book1, book2, book3]);
// Series[] library = [series1];

// var l2oChained = library.AsQueryable().SelectMany(series => series.Books).SelectMany(book => book.Chapters).Select(chapter => chapter.Title);
// Dump(l2oChained);
// var l2oNested = library.AsQueryable().SelectMany(series => series.Books.SelectMany(book => book.Chapters.Select(chapter => chapter.Title)));
// Dump(l2oNested);
var linq3Chained = coll.AsQueryable().SelectMany(series => series.Books).SelectMany(book => book.Chapters).Select(chapter => chapter.Title);
Dump(linq3Chained);
// var linq3Nested = coll.AsQueryable().SelectMany(series => series.Books.SelectMany(book => book.Chapters.Select(chapter => chapter.Title)));
// Dump(linq3Nested);
var aggUnwind = coll.Aggregate().Unwind<Series, Book>(series => series.Books).Unwind<Book, Chapter>(book => book.Chapters).Project(chapter => chapter.Title);
Console.WriteLine(aggUnwind);
var aggNested = coll.Aggregate().Project(series => series.Books.SelectMany(book => book.Chapters.Select(chapter => chapter.Title)));
Console.WriteLine(aggNested);

void Dump<T>(IQueryable<T> query)
{
    Console.WriteLine(query);
    foreach (var item in query.ToList())
    {
        Console.WriteLine(item);
    }
}

record Series(Book[] Books);
record Book(ObjectId Id, Chapter[] Chapters);
record Chapter(string Title);

namespace System.Runtime.CompilerServices
{
    [ComponentModel.EditorBrowsable(ComponentModel.EditorBrowsableState.Never)]
    internal class IsExternalInit{}
}
