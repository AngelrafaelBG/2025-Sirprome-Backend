using Microsoft.AspNetCore.SignalR;
using MongoDB.Driver;
using System.Threading.Tasks;

public class CollaborationHub : Hub
{
    private readonly IMongoCollection<Document> _documents;

    public CollaborationHub(IMongoClient mongoClient)
    {
        var database = mongoClient.GetDatabase("classroom_db");
        _documents = database.GetCollection<Document>("documents");
    }

    // Unirse a un documento
    public async Task JoinDoc(string docId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, docId);

        var doc = await _documents.Find(d => d.DocId == docId).FirstOrDefaultAsync();
        if (doc == null)
        {
            doc = new Document { DocId = docId, Content = "", LastUpdated = DateTime.UtcNow };
            await _documents.InsertOneAsync(doc);
        }

        await Clients.Caller.SendAsync("ReceiveDoc", doc.Content);
    }

    // Actualizar documento
    public async Task UpdateDoc(string docId, string content)
    {
        var filter = Builders<Document>.Filter.Eq(d => d.DocId, docId);
        var update = Builders<Document>.Update
            .Set(d => d.Content, content)
            .Set(d => d.LastUpdated, DateTime.UtcNow);

        await _documents.UpdateOneAsync(filter, update);
        await Clients.OthersInGroup(docId).SendAsync("ReceiveUpdate", content);
    }
}