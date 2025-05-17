using MongoDB.Bson;

public class Crear {
    public ObjectId Id;
    public string? IdGrupo { get; set; } = string.Empty;
    public string? IdProfesor { get; set; } = string.Empty;
    public string? Codigo { get; set; } = string.Empty;
    public string? Nombre { get; set; } = string.Empty;
    public string? Imagen { get; set; } = string.Empty;
    public List<string?> Miembros { get; set; } = new List<string?>();
    public List<string?> Correos { get; set; } = new List<string?>();
}

public class IngresarMiembro {
    public string? Codigo { get; set; } = string.Empty;
}

public class EliminarGrupo {
    public string? IdProfesor { get; set; } = string.Empty;
    public string? Id { get; set; } = string.Empty;
}

public class Materias {
    public string? Id { get; set; } = string.Empty;
    public string? IdProfesor { get; set; } = string.Empty;
    public string? Materia { get; set; } = string.Empty;
}
