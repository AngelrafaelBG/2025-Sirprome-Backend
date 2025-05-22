public class Logro {
    public string Id { get; set; } 
    public string Nombre { get; set; } 
    public string Descripcion { get; set; }
    public int XP { get; set; } 
}

public class LogrosContainer {
    public List<Logro> Logros { get; set; } = new List<Logro>();

    public string ObtenerCadenaDeLogros() {
        return string.Join(", ", Logros.Select(logro => logro.Nombre));
    }
}