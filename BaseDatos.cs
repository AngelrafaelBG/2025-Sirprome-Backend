using MongoDB.Bson;
using MongoDB.Driver;

public class BaseDatos() {
    public string connection = "mongodb+srv://Carlos_404:Carlos_404@cluster0.gj9zq8u.mongodb.net/?retryWrites=true&w=majority&appName=Cluster0";
    public string baseDatos = "SIRPROME";

    public IMongoCollection<T> ObtenerColeccion<T>(string coleccion) {
        MongoClient client = new MongoClient(this.connection);
        IMongoCollection<T>? collection = client.GetDatabase(this.baseDatos).GetCollection<T>(coleccion);
        return collection;
    }
}