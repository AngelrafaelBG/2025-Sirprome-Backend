using MongoDB.Bson;

public class Tareas {
    public ObjectId Id;
    public string? IdTarea { get; set; } = string.Empty;
    public string? IdGrupo { get; set; } = string.Empty;
    public string? Titulo { get; set; } = string.Empty;
    public string? Descripcion { get; set; } = string.Empty;
    public int ValorMax { get; set; }
}

public class actualizarTarea {
    public string? Titulo { get; set; } = string.Empty;
    public string? Descripcion { get; set; } = string.Empty;
    public int ValorMax { get; set; }
}

public class TareasAlumnos {
    public ObjectId Id;
    public string? IdTarea { get; set; } = string.Empty;
    public string? IdUsuario { get; set; } = string.Empty;
    public string? IdGrupo { get; set; } = string.Empty;
    public string? Titulo { get; set; } = string.Empty;
    public string? Descripcion { get; set; } = string.Empty;
    public int Calificacion { get; set; } 
    public int ValorMax { get; set; }
    public string? Evidencia { get; set; } = string.Empty;
}

public class ActualizarTA {
    public int calificación { get; set; }
}

public class Subir {
    public string? Evidencia { get; set; } = string.Empty;
}

public class Calificar {
    public int Calificacion { get; set; }
}