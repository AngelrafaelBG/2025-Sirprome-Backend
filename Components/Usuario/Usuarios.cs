using MongoDB.Bson;
public class Registro {
    public ObjectId Id;
    public string? IdUsuario { get; set; } = string.Empty;
    public string? Nombre { get; set; } = string.Empty;
    public string? Correo { get; set; } = string.Empty;
    public string? Contraseña { get; set; } = string.Empty;
    public string? Rol { get; set; } = string.Empty;
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