using MongoDB.Driver;

public class LogrosService {
    private readonly IMongoCollection<Logro> _logros;
    private readonly IMongoCollection<Registro> _usuarios;

    public LogrosService(BaseDatos baseDatos) {
        _logros = baseDatos.ObtenerColeccion<Logro>("Logros");
        _usuarios = baseDatos.ObtenerColeccion<Registro>("Usuarios");
    }

    public List<Logro> ObtenerLogrosDeUsuario(string idUsuario) {
        // Buscar el usuario por su IdUsuario
        var usuario = _usuarios.Find(u => u.IdUsuario == idUsuario).FirstOrDefault();
        if (usuario == null) {
            throw new Exception("Usuario no encontrado.");
        }

        // Obtener los logros correspondientes a los IDs almacenados en el usuario
        return _logros.Find(logro => usuario.Rol.Contains(logro.Id)).ToList();
    }

    public List<Logro> ObtenerTodosLosLogros() {
        // Devuelve todos los logros de la colección
        return _logros.Find(_ => true).ToList();
    }

    public List<string> ObtenerLogrosDesbloqueadosPorUsuario(string idUsuario) {
        // Buscar el usuario por su IdUsuario
        var usuario = _usuarios.Find(u => u.IdUsuario == idUsuario).FirstOrDefault();
        if (usuario == null) {
            throw new Exception("Usuario no encontrado.");
        }

        // Devolver la lista de IDs de logros desbloqueados
        return usuario.LogrosDesbloqueados ?? new List<string>();
    }

    public void CrearLogro(Logro nuevoLogro) {
        // Verificar si el logro ya existe
        var logroExistente = _logros.Find(l => l.Id == nuevoLogro.Id).FirstOrDefault();
        if (logroExistente != null) {
            throw new Exception("El logro con el ID especificado ya existe.");
        }

        // Insertar el nuevo logro en la colección
        _logros.InsertOne(nuevoLogro);
    }

    public void CrearLogroSiNoExiste(Logro nuevoLogro) {
        // Verificar si el logro ya existe en la base de datos
        var logroExistente = _logros.Find(l => l.Id == nuevoLogro.Id).FirstOrDefault();
        if (logroExistente != null) {
            // Si el logro ya existe, no lanzar excepción, simplemente retornar
            return;
        }

        // Insertar el nuevo logro si no existe
        _logros.InsertOne(nuevoLogro);
    }

    public void DesbloquearLogro(string idUsuario, string logroId) {
        // Lógica para desbloquear logros
    }

    // Otros métodos existentes...
}