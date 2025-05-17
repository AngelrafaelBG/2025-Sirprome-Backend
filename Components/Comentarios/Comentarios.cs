using MongoDB.Bson;

public class Comentarios {
    public ObjectId Id { get; set; }
    public string? IdGrupo { get; set; } = string.Empty; 
    public string? IdComentario { get; set; } = string.Empty;
    public string? IdRecibir { get; set; } = string.Empty;
    public string? Comentario { get; set; } = string.Empty;
    public string? IdEscrito { get; set; } = string.Empty;
}