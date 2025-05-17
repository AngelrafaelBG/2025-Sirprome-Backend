using MongoDB.Driver;

public static class CriteriosRequestHandler {
    public static IResult InsertarCriterios(InsertarCriterio datos, string IdProfesor, string IdGrupo, string IdCriterio) {
        var Profesor = IdProfesor.ToLower();
        var Grupo = IdGrupo.ToLower();
        var Criterio = IdCriterio.ToLower();
        if (string.IsNullOrWhiteSpace(Profesor)) {
            return Results.BadRequest("Introduce el id del profesor.");
        }
        if (string.IsNullOrWhiteSpace(Grupo)) {
            return Results.BadRequest("Introduce el id del grupo.");
        }
        if (string.IsNullOrWhiteSpace(Criterio)) {
            return Results.BadRequest("Introduce el id de los criterios.");
        }
        if (string.IsNullOrWhiteSpace(datos.Criterio)) {
            return Results.BadRequest("Introduce el nombre del criterio.");
        }
        if (datos.Valor <= 0 || datos.Valor == null) {
            return Results.BadRequest("El número debe ser mayor a 0.");
        }
        datos.Evaluacion = 0;

        BaseDatos bd = new BaseDatos();
        var colectionUsers = bd.ObtenerColeccion<Registro>("Usuarios");
        var colectionGroup = bd.ObtenerColeccion<Crear>("Grupos");
        var colection = bd.ObtenerColeccion<Criterios>("Criterios");
        
        FilterDefinitionBuilder<Registro> filterBuilder1 = new FilterDefinitionBuilder<Registro>();
        var filter1 = filterBuilder1.Eq(x => x.IdUsuario, Profesor);
        Registro? usuario = colectionUsers.Find(filter1).FirstOrDefault();
        if (usuario == null) {
            return Results.BadRequest("No existe el usuario.");
        }
        if (usuario.Rol != "profesor") {
            return Results.BadRequest("El usuario debe ser un profesor.");
        }

        FilterDefinitionBuilder<Crear> filterBuilder2 = new FilterDefinitionBuilder<Crear>();
        var filter2 = filterBuilder2.Eq(x => x.IdGrupo, Grupo);
        Crear? grupo = colectionGroup.Find(filter2).FirstOrDefault();
        if (grupo == null) {
            return Results.BadRequest("No existe el grupo.");
        }
        if (grupo.IdProfesor != Profesor) {
            return Results.BadRequest("El usuario debe ser el creador del grupo.");
        }

        FilterDefinitionBuilder<Criterios> filterBuilder3 = new FilterDefinitionBuilder<Criterios>();
        var filter3 = filterBuilder3.Eq(x => x.IdCriterio, Criterio);
        Criterios? criterio = colection.Find(filter3).FirstOrDefault();
        if (criterio == null) {
            return Results.BadRequest("No existen los criterios.");
        }
        if (criterio.Grupo != Grupo) {
            return Results.BadRequest("El criterio debe ser del grupo.");
        }
        if (criterio.Criterio.Contains(datos.Criterio)) {
            return Results.BadRequest("El criterio ya existe.");
        }

        var Update = Builders<Criterios>.Update.Push(x => x.Criterio, datos.Criterio).Push(x => x.Valor, datos.Valor).Push(x => x.Evaluacion, datos.Evaluacion);
        colection.UpdateOne(x => x.IdCriterio == Criterio, Update);
        return Results.Ok("Criterios insertados.");
    }

   public static IResult EliminarCriterio(Eliminar eliminar, string IdProfesor, string IdGrupo, string IdCriterio) {
        var Profesor = IdProfesor.ToLower();
        var Grupo = IdGrupo.ToLower();
        var Criterio = IdCriterio.ToLower();
        if (string.IsNullOrWhiteSpace(Profesor)) {
            return Results.BadRequest("Introduce el id del profesor.");
        }
        if (string.IsNullOrWhiteSpace(Grupo)) {
            return Results.BadRequest("Introduce el id del grupo.");
        }
        if (string.IsNullOrWhiteSpace(Criterio)) {
            return Results.BadRequest("Introduce el id del criterio.");
        }
        if (string.IsNullOrWhiteSpace(eliminar.Criterio)) {
            return Results.BadRequest("Introduce el criterio a eliminar");
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

        var colectionCriterios = bd.ObtenerColeccion<Criterios>("Criterios");
        if (colectionCriterios == null) {
            throw new Exception("No existe la colección Criterios.");
        }

        var colectionCA = bd.ObtenerColeccion<CriterioAlumno>("CriteriosAlumnos");
        if (colectionCA == null) {
            throw new Exception("No existe la colección CriteriosAlumnos.");
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

        FilterDefinitionBuilder<Criterios> filterBuilder3 = new FilterDefinitionBuilder<Criterios>();
        var filter3 = filterBuilder3.Eq(x => x.IdCriterio, Criterio);
        Criterios? criterioExistente = colectionCriterios.Find(filter3).FirstOrDefault();
        if (criterioExistente == null) {
            return Results.BadRequest("No existe el criterio.");
        }
        if (criterioExistente.Grupo != Grupo) {
            return Results.BadRequest("El criterio no es del grupo.");
        }

        int indice = criterioExistente.Criterio.FindIndex(c => c != null && c.ToLower() == eliminar.Criterio.ToLower());
        if (indice == -1) {
            return Results.BadRequest("El criterio especificado no se encuentra en la lista.");
        }

        criterioExistente.Criterio.RemoveAt(indice);
        criterioExistente.Valor.RemoveAt(indice);
        criterioExistente.Evaluacion.RemoveAt(indice);

        var update = Builders<Criterios>.Update.Set(x => x.Criterio, criterioExistente.Criterio).Set(x => x.Valor, criterioExistente.Valor).Set(x => x.Evaluacion, criterioExistente.Evaluacion);
        colectionCriterios.UpdateOne(Builders<Criterios>.Filter.Eq(x => x.IdCriterio, Criterio) & Builders<Criterios>.Filter.Eq(x => x.Grupo, Grupo), update);

        FilterDefinitionBuilder<CriterioAlumno> filterBuilder4 = new FilterDefinitionBuilder<CriterioAlumno>();
        var filter4 = filterBuilder4.Eq(x => x.IdGrupo, Grupo);
        var criterioAlumno = colectionCA.Find(filter4).ToList();

        foreach (var alumno in criterioAlumno) {
            int alumnoIndice = alumno.Criterio.FindIndex(c => c != null && c.ToLower() == eliminar.Criterio.ToLower());
            if (alumnoIndice != -1) {
                var updateCriterio = Builders<CriterioAlumno>.Update
                    .PullFilter(x => x.Criterio, Builders<string?>.Filter.Eq(c => c, eliminar.Criterio))
                    .PullFilter(x => x.Valor, Builders<int?>.Filter.Eq(v => v, alumno.Valor[alumnoIndice]))
                    .PullFilter(x => x.Evaluacion, Builders<int?>.Filter.Eq(e => e, alumno.Evaluacion[alumnoIndice]));

                colectionCA.UpdateOne(
                    Builders<CriterioAlumno>.Filter.Eq(x => x.IdGrupo, Grupo) &
                    Builders<CriterioAlumno>.Filter.Eq(x => x.IdAlumno, alumno.IdAlumno),
                    updateCriterio
                );
            }
        }
        return Results.Ok("Criterio eliminado.");
    }

    public static IResult ActualizarCriterio(ActualizarCriterio actu, string IdProfesor, string IdGrupo, string IdCriterio) {
        var Profesor = IdProfesor.ToLower();
        var Grupo = IdGrupo.ToLower();
        var Criterio = IdCriterio.ToLower();
        if (string.IsNullOrWhiteSpace(actu.ACriterio)) {
            return Results.BadRequest("Introduce el nombre del criterio a corregir.");
        }
        if (string.IsNullOrWhiteSpace(actu.Criterio)) {
            return Results.BadRequest("Introduce el nuevo nombre del criterio.");
        }
        if (actu.Valor <= 0) {
            return Results.BadRequest("Introduc el nuevo valor del criterio.");
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

        var colectionCriterios = bd.ObtenerColeccion<Criterios>("Criterios");
        if (colectionCriterios == null) {
            throw new Exception("No existe la colección Criterios.");
        }

        var colectionCA = bd.ObtenerColeccion<CriterioAlumno>("CriteriosAlumnos");
        if (colectionCA == null) {
            throw new Exception("No existe la colección CriteriosAlumnos.");
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

        FilterDefinitionBuilder<Criterios> filterBuilder3 = new FilterDefinitionBuilder<Criterios>();
        var filter3 = filterBuilder3.Eq(x => x.IdCriterio, Criterio);
        Criterios? criterioExistente = colectionCriterios.Find(filter3).FirstOrDefault();
        if (criterioExistente == null) {
            return Results.BadRequest("No existe el criterio.");
        }
        if (criterioExistente.Grupo != Grupo) {
            return Results.BadRequest("El criterio no es el del grupo.");
        }

        int indice = criterioExistente.Criterio.IndexOf(actu.ACriterio);
        if (indice == -1) {
            return Results.BadRequest("El criterio a atualizar no existe.");
        }
        var UpdateC = Builders<Criterios>.Update.Set(x => x.Criterio[indice], actu.Criterio).Set(x => x.Valor[indice], actu.Valor);
        colectionCriterios.UpdateOne(filter3, UpdateC);

        var filterCriterioAlumnos = Builders<CriterioAlumno>.Filter.Eq(x => x.IdGrupo, Grupo) & Builders<CriterioAlumno>.Filter.Eq(x => x.Criterio[indice], actu.ACriterio);
        var UpdateCA = Builders<CriterioAlumno>.Update.Set(x => x.Criterio[indice], actu.Criterio).Set(x => x.Valor[indice], actu.Valor);
        colectionCA.UpdateMany(filterCriterioAlumnos, UpdateCA);
        return Results.Ok("Criterio Actualizado.");
    }

    public static IResult Calificar(Actu actu, string IdProfesor, string IdGrupo, string IdCriterio, string IdAlumno) {
        var Profesor = IdProfesor.ToLower();
        var Grupo = IdGrupo.ToLower();
        var Criterio = IdCriterio.ToLower();
        var Alumnos = IdAlumno.ToLower();
        if (string.IsNullOrEmpty(actu.criterio)) {
            return Results.BadRequest("Introduce el criterio que vas a calificar.");
        }
        if (actu.evaluacion <= 0 ) {
            return Results.BadRequest("Introduce la nnueva evaluación");
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

        var colectionCriterios = bd.ObtenerColeccion<Criterios>("Criterios");
        if (colectionCriterios == null) {
            throw new Exception("No existe la colección Criterios.");
        }

        var colectionCA = bd.ObtenerColeccion<CriterioAlumno>("CriteriosAlumnos");
        if (colectionCA == null) {
            throw new Exception("No existe la colección CriteriosAlumnos.");
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

        filter1 = filterBuilder1.Eq(x => x.IdUsuario, Alumnos);
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
        if (grupoExistente.IdProfesor != Profesor) {
            return Results.BadRequest("El usuario no es el creador del grupo.");
        }

        FilterDefinitionBuilder<Criterios> filterBuilder3 = new FilterDefinitionBuilder<Criterios>();
        var filter3 = filterBuilder3.Eq(x => x.IdCriterio, Criterio);
        Criterios? criterioExistente = colectionCriterios.Find(filter3).FirstOrDefault();
        if (criterioExistente == null) {
            return Results.BadRequest("No existe el criterio.");
        }
        if (criterioExistente.Grupo != Grupo) {
            return Results.BadRequest("El criterio no es el del grupo.");
        }
        if (!criterioExistente.Criterio.Contains(actu.criterio)) {
            return Results.BadRequest("No exxiste el criterio.");
        }

        int indice = criterioExistente.Criterio.IndexOf(actu.criterio);
        if (indice == -1) {
            return Results.BadRequest("No existe el criterio.");
        }
        if (criterioExistente.Valor[indice] < actu.evaluacion) {
            return Results.BadRequest("La evaluación no puede superar el valor maximo establecido.");
        }

        var filterCriterioAlumnos = Builders<CriterioAlumno>.Filter.Eq(x => x.IdGrupo, Grupo) & Builders<CriterioAlumno>.Filter.Eq(x => x.IdAlumno, Alumnos) & Builders<CriterioAlumno>.Filter.Eq(x => x.Criterio[indice], actu.criterio);
        var UpdateCA = Builders<CriterioAlumno>.Update.Set(x => x.Evaluacion[indice], actu.evaluacion);

        colectionCA.UpdateMany(filterCriterioAlumnos, UpdateCA);
        return Results.Ok("Criterio del alumno actualizado.");
    }
}