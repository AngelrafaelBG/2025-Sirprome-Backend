using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

public class Document
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; }

    public string DocId { get; set; }  // ID único del documento (ej: "tarea-matematicas-1")
    public string Content { get; set; }
    public DateTime LastUpdated { get; set; }
}