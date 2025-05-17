using MongoDB.Bson;
public class Criterios {
    public ObjectId Id;
    public string? IdCriterio { get; set; } = string.Empty;
    public string? Grupo { get; set; } = string.Empty;
    public List<string?> Criterio { get; set;} = new List<string?>();
    public List<int?> Valor { get; set; } = new List<int?>();
    public List<int?> Evaluacion { get; set; } = new List<int?>();   
}
public class ActualizarCriterio {
    public string? ACriterio { get; set; } = string.Empty;
    public string? Criterio { get; set; } = string.Empty;
    public int? Valor { get; set; }

}
public class Eliminar {
    public string? Criterio { get; set; } = string.Empty;
}
public class InsertarCriterio {
    public string? Criterio { get; set; } = string.Empty;
    public int? Valor { get; set; }
    public int? Evaluacion { get; set; }
}

public class CriterioAlumno {
    public ObjectId Id;
    public string? IdAlumno  { get; set; } = string.Empty;
    public string? IdCriterio { get; set; } = string.Empty;
    public string? IdGrupo { get; set; } = string.Empty;
    public List<string?> Criterio { get; set;} = new List<string?>();
    public List<int?> Valor { get; set; } = new List<int?>();
    public List<int?> Evaluacion { get; set; } = new List<int?>();  
}
public class Actu{
    public string? criterio { get; set; } = string.Empty; 
    public int? evaluacion { get; set; }
}