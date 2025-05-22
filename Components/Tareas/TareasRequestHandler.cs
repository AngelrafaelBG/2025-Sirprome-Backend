using MongoDB.Driver;
public static class TareasRequestHandler {
    public static IResult IngresarTarea(Tareas tareas, string IdProfesor, string IdGrupo) {
        var Profesor = IdProfesor.ToString().ToLower();
        tareas.IdGrupo = IdGrupo.ToLower();
        tareas.IdTarea = Guid.NewGuid().ToString().ToLower();
        if (string.IsNullOrWhiteSpace(Profesor)) {
            return Results.BadRequest("Introduce el id del profesor.");
        }
        if (string.IsNullOrWhiteSpace(tareas.IdGrupo)) {
            return Results.BadRequest("Introduce el id del grupo.");
        }
        if (string.IsNullOrWhiteSpace(tareas.Titulo)) {
            return Results.BadRequest("Introduce el titulo de la tarea.");
        }
        if (string.IsNullOrWhiteSpace(tareas.Descripcion)) {
            return Results.BadRequest("Introduce una descripción a la tarea.");
        }
        if (tareas.ValorMax == null || tareas.ValorMax <= 0) {
            return Results.BadRequest("El valor de la tarea debe ser mayor a 0.");
        }

        BaseDatos bd = new BaseDatos();
        var colectionUsers = bd.ObtenerColeccion<Registro>("Usuarios");
        if (colectionUsers == null) {
            throw new Exception("No existe la colección Usuarios.");
        }

        var colectionGroup = bd.ObtenerColeccion<Crear>("Grupos");
        if (colectionGroup == null) {
            throw new Exception("No existe la colección Grupos.");
        }

        var colectionWorks = bd.ObtenerColeccion<Tareas>("Tareas");
        if (colectionWorks == null) {
            throw new Exception("No existe la colección Tareas.");
        }

        var colectionStudents = bd.ObtenerColeccion<TareasAlumnos>("TareasAlumnos");
        if (colectionStudents == null) {
            throw new Exception("No existe la coleccción TareasAlumnos.");
        }

        FilterDefinitionBuilder<Registro> filterBuilder1 = new FilterDefinitionBuilder<Registro>();
        var filter1 = filterBuilder1.Eq(x => x.IdUsuario, Profesor);
        Registro? usuarioExistente = colectionUsers.Find(filter1).FirstOrDefault();
        if (usuarioExistente == null) {
            return Results.BadRequest("No existe el usuario.");
        }
        if (usuarioExistente.Rol != "profesor") {
            return Results.BadRequest("El usuario debe de ser un profesor.");
        }
        
        FilterDefinitionBuilder<Crear> filterBuilder2 = new FilterDefinitionBuilder<Crear>();
        var filter2 = filterBuilder2.Eq(x => x.IdGrupo, tareas.IdGrupo);
        Crear? grupoExistente = colectionGroup.Find(filter2).FirstOrDefault();
        if (grupoExistente == null) {
            return Results.BadRequest("No existe el grupo.");
        }
        if (grupoExistente.IdProfesor != Profesor) {
            return Results.BadRequest("El usuario no es el creador del grupo.");
        }

        FilterDefinitionBuilder<Tareas> filterBuilder4 = new FilterDefinitionBuilder<Tareas>();
        var filter4 = filterBuilder4.Eq(x => x.Titulo, tareas.Titulo);
        Tareas? tareaExistente = colectionWorks.Find(filter4).FirstOrDefault();
        if (tareaExistente != null && tareaExistente.IdGrupo == tareas.IdGrupo) {
            return Results.BadRequest("Ya existe una Tarea llamada así en el grupo.");
        }
        foreach (var miembro in grupoExistente.Miembros) {
            if (!string.IsNullOrWhiteSpace(miembro)) {
                var TareaAlumno = new TareasAlumnos {
                    IdTarea = tareas.IdTarea.ToLower(),
                    IdUsuario = miembro,
                    IdGrupo = tareas.IdGrupo,
                    Titulo = tareas.Titulo,
                    Descripcion = tareas.Descripcion,
                    ValorMax = tareas.ValorMax,
                    Evidencia = null,
                    Calificacion = 0
                };
                colectionStudents.InsertOne(TareaAlumno);
            }
        }

        colectionWorks.InsertOne(tareas);
        return Results.Ok("Tarea insertada.");
    }

    public static IResult ActualizarTarea(actualizarTarea actu, string IdProfesor, string IdGrupo, string IdTarea) {
        var Profesor = IdProfesor.ToLower();
        var Grupo = IdGrupo.ToLower();
        var Tarea = IdTarea.ToLower();
        if (string.IsNullOrWhiteSpace(Profesor)) {
            return Results.BadRequest("Introduce el Id del profesor.");
        }
        if (string.IsNullOrWhiteSpace(Grupo)) {
            return Results.BadRequest("Introduce el Id del grupo.");
        }
        if (string.IsNullOrWhiteSpace(Tarea)) {
            return Results.BadRequest("Introduce el Id de la tarea.");
        }
        if (string.IsNullOrWhiteSpace(actu.Titulo)) {
            return Results.BadRequest("Introduce el titulo al que lo quieres cambiar.");
        }
        if (string.IsNullOrWhiteSpace(actu.Descripcion)) {
            return Results.BadRequest("Introduce la descripción a la que quieres cambiar.");
        }
        if (actu.ValorMax == null || actu.ValorMax <= 0) {
            return Results.BadRequest("Introduce el nuevo valor que le vas a dar a la tarea.");
        }

        BaseDatos bd = new BaseDatos();
        var colectionUsers = bd.ObtenerColeccion<Registro>("Usuarios");
        if (colectionUsers == null) {
            throw new Exception("No existe la colección Usuarios.");
        }

        var colectionGroup = bd.ObtenerColeccion<Crear>("Grupos");
        if (colectionGroup == null) {
            throw new Exception("No existe la colección Grupos.");
        }

        var colectionWorks = bd.ObtenerColeccion<Tareas>("Tareas");
        if (colectionGroup == null) {
            throw new Exception("No existe la colección Tareas.");
        }

        var colectionTA = bd.ObtenerColeccion<TareasAlumnos>("TareasAlumnos");
        if (colectionTA == null) {
            throw new Exception("No existe la colección TareasAlumnos");
        }

        FilterDefinitionBuilder<Registro> filterBuilder1 = new FilterDefinitionBuilder<Registro>();
        var filter1 = filterBuilder1.Eq(x => x.IdUsuario, Profesor);
        Registro? usuarioExistente = colectionUsers.Find(filter1).FirstOrDefault();
        if (usuarioExistente == null) {
            return Results.BadRequest("No existe el usuario.");
        }
        if (usuarioExistente.Rol != "profesor") {
            return Results.BadRequest("El usuario debe de ser un profesor.");
        }
        
        FilterDefinitionBuilder<Crear> filterBuilder2 = new FilterDefinitionBuilder<Crear>();
        var filter2 = filterBuilder2.Eq(x => x.IdGrupo, Grupo);
        Crear? grupoExistente = colectionGroup.Find(filter2).FirstOrDefault();
        if (grupoExistente == null) {
            return Results.BadRequest("No existe el grupo.");
        }
        if (grupoExistente.IdProfesor != Profesor) {
            return Results.BadRequest("El usuario no es el creador del grupo.");
        }
        

        FilterDefinitionBuilder<Tareas> filterBuilder4 = new FilterDefinitionBuilder<Tareas>();
        var filter4 = filterBuilder4.Eq(x => x.IdTarea, Tarea);
        Tareas? tareaExistente = colectionWorks.Find(filter4).FirstOrDefault();
        if (tareaExistente == null) {
            return Results.BadRequest("No existe la tarea.");
        }
        if (tareaExistente.IdGrupo != Grupo) {
            return Results.BadRequest("No existe la tarea en este grupo.");
        }

        var update = Builders<Tareas>.Update.Set(x => x.Titulo, actu.Titulo).Set(x => x.Descripcion, actu.Descripcion).Set(x => x.ValorMax, actu.ValorMax);
        colectionWorks.UpdateOne(Builders<Tareas>.Filter.Eq(x => x.IdTarea, Tarea), update);

        var filterTA = Builders<TareasAlumnos>.Filter.Eq(x => x.IdGrupo, Grupo);
        var updateTA = Builders<TareasAlumnos>.Update.Set(x => x.Titulo, actu.Titulo).Set(x => x.Descripcion, actu.Descripcion).Set(x => x.ValorMax, actu.ValorMax);
        colectionTA.UpdateMany(filterTA, updateTA);
        return Results.Ok("Tarea actualizada.");
    }
    public static IResult EliminarTarea(string IdProfesor, string IdGrupo, string IdTarea) {
        var Profesor = IdProfesor.ToLower();
        var Grupo = IdGrupo.ToLower();
        var Tarea = IdTarea.ToLower();
        if (string.IsNullOrWhiteSpace(Profesor)) {
            return Results.BadRequest("Introduce el Id del profesor.");
        }
        if (string.IsNullOrWhiteSpace(Grupo)) {
            return Results.BadRequest("Introduce el Id del grupo.");
        }
        if (string.IsNullOrWhiteSpace(Tarea)) {
            return Results.BadRequest("Introduce el Id de la tarea.");
        }

        BaseDatos bd = new BaseDatos();
        var colectionUsers = bd.ObtenerColeccion<Registro>("Usuarios");
        if (colectionUsers == null) {
            throw new Exception("No existe la colección Usuarios.");
        }

        var colectionGroup = bd.ObtenerColeccion<Crear>("Grupos");
        if (colectionGroup == null) {
            throw new Exception("No existe la colección Grupos.");
        }

        var colectionWorks = bd.ObtenerColeccion<Tareas>("Tareas");
        if (colectionGroup == null) {
            throw new Exception("No existe la colección Tareas.");
        }

        var colectionTA = bd.ObtenerColeccion<TareasAlumnos>("TareasAlumnos");
        if (colectionTA == null) {
            throw new Exception("No existe la colección TareasAlumnos.");
        }

        FilterDefinitionBuilder<Registro> filterBuilder1 = new FilterDefinitionBuilder<Registro>();
        var filter1 = filterBuilder1.Eq(x => x.IdUsuario, Profesor);
        Registro? usuarioExistente = colectionUsers.Find(filter1).FirstOrDefault();
        if (usuarioExistente == null) {
            return Results.BadRequest("No existe el usuario.");
        }
        if (usuarioExistente.Rol != "profesor") {
            return Results.BadRequest("El usuario debe de ser un profesor.");
        }
        
        FilterDefinitionBuilder<Crear> filterBuilder2 = new FilterDefinitionBuilder<Crear>();
        var filter2 = filterBuilder2.Eq(x => x.IdGrupo, Grupo);
        Crear? grupoExistente = colectionGroup.Find(filter2).FirstOrDefault();
        if (grupoExistente == null) {
            return Results.BadRequest("No existe el grupo.");
        }
        if (grupoExistente.IdProfesor != Profesor) {
            return Results.BadRequest("El usuario no es el creador del grupo.");
        }

        FilterDefinitionBuilder<Tareas> filterBuilder3 = new FilterDefinitionBuilder<Tareas>();
        var filter3 = filterBuilder3.Eq(x => x.IdTarea, Tarea);
        Tareas? tareaExistente = colectionWorks.Find(filter3).FirstOrDefault();
        if (tareaExistente == null) {
            return Results.BadRequest("No existe la tarea.");
        }
        if (tareaExistente.IdGrupo != Grupo) {
            return Results.BadRequest("No existe la tarea en este grupo.");
        }
        colectionTA.DeleteMany(Builders<TareasAlumnos>.Filter.Eq(x => x.IdTarea, Tarea));      
        
        colectionWorks.DeleteOne(filter3);
        return Results.Ok("Tarea eliminada.");
    }

    public static async Task<IResult> Subir(HttpRequest request, string IdUsuario, string IdGrupo, string IdTarea, BaseDatos bd) {
        try {
            // Leer el cuerpo de la solicitud
            var body = await request.ReadFromJsonAsync<SubirEvidenciaRequest>();
            if (body == null || string.IsNullOrWhiteSpace(body.Evidencia)) {
                return Results.BadRequest("La evidencia no puede estar vacía.");
            }

            // Lógica para procesar la subida
            var tareasAlumnosCollection = bd.ObtenerColeccion<TareasAlumnos>("TareasAlumnos");

            var filtro = Builders<TareasAlumnos>.Filter.And(
                Builders<TareasAlumnos>.Filter.Eq(t => t.IdTarea, IdTarea),
                Builders<TareasAlumnos>.Filter.Eq(t => t.IdGrupo, IdGrupo),
                Builders<TareasAlumnos>.Filter.Eq(t => t.IdUsuario, IdUsuario)
            );
            var tareaAlumno = tareasAlumnosCollection.Find(filtro).FirstOrDefault();
            /*
            if (tareaAlumno != null) {
                return Results.BadRequest("La tarea ya fue subida.");
            }
            */
            var nuevaTareaAlumno = new TareasAlumnos {
                IdTarea = IdTarea,
                IdUsuario = IdUsuario,
                IdGrupo = IdGrupo,
                Titulo = "Título de ejemplo",
                Descripcion = "Descripción de ejemplo",
                Calificacion = 0,
                ValorMax = 100,
                Evidencia = body.Evidencia
            };

            tareasAlumnosCollection.InsertOne(nuevaTareaAlumno);

            return Results.Ok(new {
                mensaje = "Tarea subida correctamente."
            });
        } catch (Exception ex) {
            return Results.BadRequest(new { mensaje = ex.Message });
        }
    }
   
   public static IResult Calificar(Calificar cal, string IdProfesor, string IdGrupo, string IdTarea, string IdUsuario){
        var Usuario = IdUsuario.ToLower();
        var Profesor = IdProfesor.ToLower();
        var Grupo = IdGrupo.ToLower();
        var Tarea = IdTarea.ToLower();
        if (string.IsNullOrWhiteSpace(Profesor)) {
            return Results.BadRequest("Introduce el Id del profesor.");
        }
        if (string.IsNullOrWhiteSpace(Grupo)) {
            return Results.BadRequest("Introduce el Id del grupo.");
        }
        if (string.IsNullOrWhiteSpace(Tarea)) {
            return Results.BadRequest("Introduce el Id de la tarea.");
        }
        if (cal.Calificacion == null || cal.Calificacion < 0 ) {
            return Results.BadRequest("Introduce la evidencia.");
        }

        BaseDatos bd = new BaseDatos();
        var colectionUsers = bd.ObtenerColeccion<Registro>("Usuarios");
        if (colectionUsers == null) {
            throw new Exception("No existe la colección Usuarios.");
        }

        var colectionGroup = bd.ObtenerColeccion<Crear>("Grupos");
        if (colectionGroup == null) {
            throw new Exception("No existe la colección Grupos.");
        }

        var colectionWorks = bd.ObtenerColeccion<Tareas>("Tareas");
        if (colectionWorks == null) {
            throw new Exception("No existe la colección Tareas.");
        }

        var colectionTA = bd.ObtenerColeccion<TareasAlumnos>("TareasAlumnos");
        if (colectionTA == null){
            throw new Exception("No existe la colección TareasAlumnos.");
        }

        FilterDefinitionBuilder<Registro> filterBuilder1 = new FilterDefinitionBuilder<Registro>();
        var filter1 = filterBuilder1.Eq(x => x.IdUsuario, Profesor);
        Registro? usuarioExistente = colectionUsers.Find(filter1).FirstOrDefault();
        if (usuarioExistente == null) {
            return Results.BadRequest("No existe el usuario.");
        }
        if (usuarioExistente.Rol != "profesor") {
            return Results.BadRequest("El usuario debe de ser un profesor.");
        }
        
        filter1 = filterBuilder1.Eq(x => x.IdUsuario, Usuario);
        usuarioExistente = colectionUsers.Find(filter1).FirstOrDefault();
        if (usuarioExistente == null) {
            return Results.BadRequest("No existe el usuario.");
        }
        if (usuarioExistente.Rol != "estudiante") {
            return Results.BadRequest("El usuario debe de ser un estudiante.");
        }
        
        FilterDefinitionBuilder<Crear> filterBuilder2 = new FilterDefinitionBuilder<Crear>();
        var filter2 = filterBuilder2.Eq(x => x.IdGrupo, Grupo);
        Crear? grupoExistente = colectionGroup.Find(filter2).FirstOrDefault();
        if (grupoExistente == null) {
            return Results.BadRequest("No existe el grupo.");
        }

        FilterDefinitionBuilder<Tareas> filterBuilder3 = new FilterDefinitionBuilder<Tareas>();
        var filter3 = filterBuilder3.Eq(x => x.IdTarea, Tarea);
        Tareas? tareaExistente = colectionWorks.Find(filter3).FirstOrDefault();
        if (tareaExistente == null) {
            return Results.BadRequest("No existe la tarea.");
        }
        if (tareaExistente.IdGrupo != Grupo) {
            return Results.BadRequest("No existe la tarea en este grupo.");
        }
        FilterDefinitionBuilder<TareasAlumnos> filterBuilder4 = new FilterDefinitionBuilder<TareasAlumnos>();
        var filter4 = filterBuilder4.And(Builders<TareasAlumnos>.Filter.Eq(x => x.IdTarea, Tarea) & Builders<TareasAlumnos>.Filter.Eq(x => x.IdGrupo, Grupo) & Builders<TareasAlumnos>.Filter.Eq(x => x.IdUsuario, Usuario));
        TareasAlumnos? tareasA = colectionTA.Find(filter4).FirstOrDefault();
        if (tareasA == null) {
            return Results.BadRequest("No existe la tarea.");
        }
        var update = Builders<TareasAlumnos>.Update.Set(x => x.Calificacion, cal.Calificacion);
        colectionTA.UpdateOne(filter4, update);
        
        return Results.Ok("Tarea calificacda.");
    }

    public static void Subir(TareasAlumnos nuevaTareaAlumno, IMongoCollection<TareasAlumnos> tareasAlumnosCollection) {
        // Insertar la nueva tarea en la colección
        tareasAlumnosCollection.InsertOne(nuevaTareaAlumno);
    }
}