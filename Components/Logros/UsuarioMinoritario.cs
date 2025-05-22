public class UsuarioMinoritario {
    public string Id { get; set; }
    public string Nombre { get; set; }
    public int XP { get; set; }
    public List<string> Logros { get; set; } // IDs de logros obtenidos

    public UsuarioMinoritario() {
        XP = 0;
        Logros = new List<string>();
    }
}