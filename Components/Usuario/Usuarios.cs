using MongoDB.Bson;
using System.Collections.Generic;

public class Registro {
    public ObjectId Id;
    public string? IdUsuario { get; set; } = string.Empty;
    public string? Nombre { get; set; } = string.Empty;
    public string? Correo { get; set; } = string.Empty;
    public string? Contraseña { get; set; } = string.Empty;
    public string? Rol { get; set; } = string.Empty;

    public List<string> LogrosDesbloqueados { get; set; } = new List<string>();
}

public class Inicio {
    public string? Correo { get; set; } = string.Empty;
    public string? Contraseña { get; set; } = string.Empty;
}
public class Recuperar {
    public string? Correo { get; set; } = string.Empty;
}
public class Actualizar {
    public string? Rol { get; set; } = string.Empty;
}

public class Correo{
    public string? Destinatario { get; set; }
    public string? Asunto { get; set; }
    public string? Mensaje { get; set;}
}