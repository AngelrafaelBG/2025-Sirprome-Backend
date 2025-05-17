using MongoDB.Driver;

public static class ComentariosRequestHandler {
    public static IResult IngresarComentario(Comentarios comments, string IdUsuario1, string IdGrupo, string IdUsuario2) {
        var User1 = IdUsuario1.ToLower();
        var User2 = IdUsuario2.ToLower();
        var Grupo = IdGrupo.ToLower();
        
        if (string.IsNullOrWhiteSpace(comments.Comentario)) {
            return Results.BadRequest("Introduce el comentario.");
        }

        comments.IdGrupo = Grupo;
        comments.IdEscrito = User1;
        comments.IdRecibir = User2;
        comments.IdComentario = Guid.NewGuid().ToString();
        var comentarioId = comments.IdComentario; 

        BaseDatos bd = new BaseDatos();
        var colectionUsers = bd.ObtenerColeccion<Registro>("Usuarios");
        if (colectionUsers == null) {
            throw new Exception("No existe la colección Usuarios.");
        }

        var colectionGroup = bd.ObtenerColeccion<Crear>("Grupos");
        if (colectionGroup == null) {
            throw new Exception("No existe la colección Grupos.");
        }

        var colectionComments = bd.ObtenerColeccion<Comentarios>("Comentarios");
        if (colectionComments == null) {
            throw new Exception("No existe la colección Comentarios.");
        }

        // Verificar el usuario que escribe el comentario
        FilterDefinitionBuilder<Registro> filterBuilder1 = new FilterDefinitionBuilder<Registro>();
        var filter1 = filterBuilder1.Eq(x => x.IdUsuario, comments.IdEscrito);
        Registro? Datos = colectionUsers.Find(filter1).FirstOrDefault();
        if (Datos == null) {
            return Results.BadRequest("No existe el usuario.");
        }
        if (Datos.Rol != "profesor") {
            return Results.BadRequest("El que envia el comentario debe ser profesor.");
        }

        // Verificar el usuario que recibe el comentario
        FilterDefinitionBuilder<Registro> filterBuilder2 = new FilterDefinitionBuilder<Registro>();
        var filter2 = filterBuilder2.Eq(x => x.IdUsuario, comments.IdRecibir);
        Registro? Datos2 = colectionUsers.Find(filter2).FirstOrDefault();
        if (Datos2 == null) {
            return Results.BadRequest("No existe el usuario.");
        }
        if (Datos2.Rol != "estudiante") {
            return Results.BadRequest("El que recibe el comentario debe ser estudiante.");
        }
        if (Datos.Rol.ToLower() == "estudiante" && Datos2.Rol.ToLower() == "estudiante") {
            return Results.BadRequest("No se puede comentar con otros estudiantes.");
        }

        // Verificar si el comentario ya existe
        FilterDefinitionBuilder<Comentarios> filterBuilder3 = new FilterDefinitionBuilder<Comentarios>();
        var filter3 = filterBuilder3.And(
            filterBuilder3.Eq(x => x.IdEscrito, comments.IdEscrito),
            filterBuilder3.Eq(x => x.IdRecibir, comments.IdRecibir),
            filterBuilder3.Eq(x => x.IdGrupo, comments.IdGrupo)
        );
        Comentarios? comentarioExistente = colectionComments.Find(filter3).FirstOrDefault();
        if (comentarioExistente != null) {
            return Results.BadRequest("Este comentario ya existe.");
        }

        // Verificar si los profesores pertenecen al mismo grupo
        FilterDefinitionBuilder<Crear> filterBuilder4 = new FilterDefinitionBuilder<Crear>();
        var filter4 = filterBuilder4.Eq(x => x.IdGrupo, Grupo);
        Crear? grupo = colectionGroup.Find(filter4).FirstOrDefault();
        if (grupo == null) {
            return Results.BadRequest("No existe el grupo.");
        }
        
        if (Datos2.Rol.ToLower() == "profesor" && Datos2.IdUsuario != grupo.IdProfesor) {
            return Results.BadRequest("El profesor debe ser del mismo grupo.");
        }
        if (Datos.Rol.ToLower() == "profesor" && Datos.IdUsuario != grupo.IdProfesor) {
            return Results.BadRequest("El profesor debe ser del mismo grupo.");
        }

        // Insertar el comentario
        colectionComments.InsertOne(comments);
        return Results.Ok(new { mensaje = "Comentario enviado", comentarioId = comments.IdComentario });
    }

    
    public static IResult EliminarComentario(string Usuario1, string IdGrupo, string IdComentario) {
        var User1 = Usuario1.ToLower();
        var Comentario = IdComentario.ToLower();
        var Grupo = IdGrupo.ToLower();

        BaseDatos bd = new BaseDatos();
        var colectionUsers = bd.ObtenerColeccion<Registro>("Usuarios");
        if (colectionUsers == null) {
            throw new Exception("No existe la colección Usuario.");
        }

        var colectionGroup = bd.ObtenerColeccion<Crear>("Grupos");
        if (colectionGroup == null) {
            throw new Exception("No existe la colección Grupos.");
        }

        var colectionComments = bd.ObtenerColeccion<Comentarios>("Comentarios");
        if (colectionComments == null) {
            throw new Exception("No existe la colección Comentarios.");
        }

        FilterDefinitionBuilder<Registro> filterBuilder1 = new FilterDefinitionBuilder<Registro>();
        var filter1 = filterBuilder1.Eq(x => x.IdUsuario, User1);
        Registro? usuarioExistente = colectionUsers.Find(filter1).FirstOrDefault();
        if (usuarioExistente == null) {
            return Results.BadRequest("No existe el usuario.");
        }
        if (usuarioExistente.Rol != "profesor") {
            return Results.BadRequest("El que envia el comentario debe tenr rol de profesor.");
        }

        FilterDefinitionBuilder<Crear> filterBuilder2 = new FilterDefinitionBuilder<Crear>();
        var filter2 = filterBuilder2.Eq(x => x.IdGrupo, Grupo);
        Crear? grupo = colectionGroup.Find(filter2).FirstOrDefault();
        if (grupo == null) {
            return Results.BadRequest("No existe el grupo");
        }

        FilterDefinitionBuilder<Comentarios> filterBuilder3 = new FilterDefinitionBuilder<Comentarios>();
        var filter3 = filterBuilder3.Eq(x => x.IdComentario, Comentario);
        Comentarios? comentarioExistente = colectionComments.Find(filter3).FirstOrDefault();
        if (comentarioExistente == null) {
            return Results.BadRequest("No existe el comentario.");
        }
        if (comentarioExistente.IdGrupo != Grupo) {
            return Results.BadRequest("El comentario no es de este grupo.");
        }
        if (comentarioExistente.IdEscrito != User1) {
            return Results.BadRequest("Los usuarios solo pueden eliminar sus propios mensajes.");
        }

        colectionComments.DeleteOne(filter3);
        return Results.Ok("Comentario eliminado.");
    }
}