using System.Collections.ObjectModel;
using MongoDB.Driver;

public static class GruposRequestHandler {
    public static IResult CrearGrupo(Crear crear, string IdProfesor) {
        crear.IdProfesor = IdProfesor.ToLower();
        if (string.IsNullOrWhiteSpace(crear.IdProfesor)) {
            return Results.BadRequest("Introduzca el id del creador del grupo.");
        }
        if (string.IsNullOrWhiteSpace(crear.Nombre)) {
            return Results.BadRequest("Se nesecita el nombre del grupo.");
        }
        if (string.IsNullOrWhiteSpace(crear.Imagen)) {
            return Results.BadRequest("Se necesita una imagen para el grupo.");
        }
        crear.Codigo = Guid.NewGuid().ToString("N").Substring(0, 8).ToLower();
        crear.IdGrupo = Guid.NewGuid().ToString().ToLower();

        BaseDatos bd = new BaseDatos();
        var colectionUsers = bd.ObtenerColeccion<Registro>("Usuarios");
        if (colectionUsers == null) {
            throw new Exception("No existe la colección usurios.");
        }

        var colectionGrupos = bd.ObtenerColeccion<Crear>("Grupos");
        if (colectionGrupos == null) {
            throw new Exception("No existe la colección Grupos.");
        }

        var colection = bd.ObtenerColeccion<Criterios>("Criterios");
        if (colection == null) {
            throw new Exception("No existe la colección.");
        }

        FilterDefinitionBuilder<Registro> filterBuilder1 = new FilterDefinitionBuilder<Registro>();
        var filter1 = filterBuilder1.Eq(x => x.IdUsuario, crear.IdProfesor);
        Registro? UsuarioExistente = colectionUsers.Find(filter1).FirstOrDefault();
        if (UsuarioExistente == null) {
            return Results.BadRequest("No existe el usuario.");
        }
        if (UsuarioExistente.Rol != "profesor") {
            return Results.BadRequest("El correo debe tener el rol de profesor.");
        }
        FilterDefinitionBuilder<Crear> filterBuilder2 = new FilterDefinitionBuilder<Crear>();
        var filter2 = filterBuilder2.Eq(x => x.Nombre, crear.Nombre);
        Crear? creacion = colectionGrupos.Find(filter2).FirstOrDefault();
        if (creacion != null) {
            return Results.BadRequest($"Ya existe una clase nombrada {crear.Nombre}");
        }

        var Criterios = new Criterios {
            IdCriterio = Guid.NewGuid().ToString().ToLower(),
            Grupo = crear.IdGrupo
        };
        colection.InsertOne(Criterios);
        colectionGrupos.InsertOne(crear);
        return Results.Ok("Grupo Creado.");
    }

    public static IResult IngresarMiembro(IngresarMiembro ingresar, string IdUsuario) {
        var Usuario = IdUsuario.ToLower();
        if (string.IsNullOrWhiteSpace(Usuario)) {
            return Results.BadRequest("Introduce el id del profesor.");
        }
        if (string.IsNullOrWhiteSpace(ingresar.Codigo)) {
            return Results.BadRequest("Introduce el código del grupo.");
        }
        ingresar.Codigo = ingresar.Codigo.ToLower();

        BaseDatos bd = new BaseDatos();
        var colectionGrupos = bd.ObtenerColeccion<Crear>("Grupos");
        if (colectionGrupos == null) {
            throw new Exception("No existe la colección Grupos.");
        }
        
        var colectionUsers = bd.ObtenerColeccion<Registro>("Usuarios");
        if (colectionUsers == null) {
            throw new Exception("No existe la colección Usuarios.");
        }

        var colectionCriterios = bd.ObtenerColeccion<Criterios>("Criterios");
        if (colectionCriterios == null) {
            throw new Exception("No existe la colección Criterios.");
        }

        var colectionCA = bd.ObtenerColeccion<CriterioAlumno>("CriteriosAlumnos"); 
        if (colectionCA == null) {
            throw new Exception("No existe la colección CriteriosAlumnos.");
        }
        var colectionWorks = bd.ObtenerColeccion<Tareas>("Tareas");
        if (colectionWorks == null) {
            throw new Exception("No existe la colección Tareas.");
        }

        var colectionTA = bd.ObtenerColeccion<TareasAlumnos>("TareasAlumnos");
        if (colectionTA == null) {
            throw new Exception("No existe la colección TareasAlumnos");
        }

        FilterDefinitionBuilder<Registro> filterBuilder1 = new FilterDefinitionBuilder<Registro>();
        var filter1 = filterBuilder1.Eq(x => x.IdUsuario, Usuario);
        Registro? DatosUsuario = colectionUsers.Find(filter1).FirstOrDefault();
        if (DatosUsuario == null) {
            return Results.BadRequest("No existe el usuario.");
        }
        if (DatosUsuario.Rol != "estudiante") {
            return Results.BadRequest("El rol del usuario que sera introducido debe ser estudiante.");
        }

        FilterDefinitionBuilder<Crear> filterBuilder2 = new FilterDefinitionBuilder<Crear>();
        var filter2 = filterBuilder2.Eq(x => x.Codigo, ingresar.Codigo);
        Crear? DatosGrupos = colectionGrupos.Find(filter2).FirstOrDefault();
        if (DatosGrupos == null) {
            return Results.BadRequest("No existe el grupo.");
        }

        FilterDefinitionBuilder<Criterios> filterBuilder3 = new FilterDefinitionBuilder<Criterios>();
        var filter3 = filterBuilder3.Eq(x => x.Grupo, DatosGrupos.IdGrupo.ToLower());
        Criterios? Criterio = colectionCriterios.Find(filter3).FirstOrDefault();
        if (Criterio == null) {
            return Results.BadRequest("No existen los criterios de este grupo.");
        }
        
        FilterDefinitionBuilder<Tareas> filterBuilder4 = new FilterDefinitionBuilder<Tareas>();
        var filter4 = filterBuilder4.Eq(x => x.IdGrupo, DatosGrupos.IdGrupo.ToLower());
        var tareas = colectionWorks.Find(filter4).ToList();
        
        foreach (var tarea in tareas) {
            var filterTareaAlumno = Builders<TareasAlumnos>.Filter.And(Builders<TareasAlumnos>.Filter.Eq(x => x.IdTarea, tarea.IdTarea),Builders<TareasAlumnos>.Filter.Eq(x => x.IdUsuario, Usuario));
            TareasAlumnos? tareaExistente = colectionTA.Find(filterTareaAlumno).FirstOrDefault();
            if (tareaExistente == null) {
                var tareaAlumno = new TareasAlumnos {
                    IdTarea = tarea.IdTarea,
                    IdGrupo = tarea.IdGrupo,
                    IdUsuario = Usuario,
                    Titulo = tarea.Titulo,
                    Descripcion = tarea.Descripcion,
                    ValorMax = tarea.ValorMax,
                    Calificacion = 0,
                    Evidencia = string.Empty
                };
                colectionTA.InsertOne(tareaAlumno);
            }
        }
        if (Criterio.Criterio == null || Criterio.Valor == null || Criterio.Evaluacion == null) {
            return Results.BadRequest("No estan definidos los criterios y valores");
        } 

        if (Criterio.Criterio.Count != Criterio.Valor.Count || Criterio.Criterio.Count != Criterio.Evaluacion.Count) {
            return Results.BadRequest("No tienen el mismo indice.");
        }
        var filterCA = Builders<CriterioAlumno>.Filter.And(
            Builders<CriterioAlumno>.Filter.Eq(x => x.IdCriterio, Criterio.IdCriterio),
            Builders<CriterioAlumno>.Filter.Eq(x => x.IdAlumno, Usuario),
            Builders<CriterioAlumno>.Filter.Eq(x => x.IdGrupo, DatosGrupos.IdGrupo)
        );

        CriterioAlumno? criterioExistente = colectionCA.Find(filterCA).FirstOrDefault();
        if (criterioExistente == null) {
            var nuevoCriterioAlumno = new CriterioAlumno {
                IdCriterio = Criterio.IdCriterio,
                IdGrupo = DatosGrupos.IdGrupo.ToLower(),
                IdAlumno = Usuario,
                Criterio = new List<string?>(Criterio.Criterio),
                Valor = new List<int?> (Criterio.Valor),
                Evaluacion = new List<int?>(Criterio.Evaluacion)
            };
            colectionCA.InsertOne(nuevoCriterioAlumno);
        } else {
            var updateCA = Builders<CriterioAlumno>.Update
                .AddToSetEach(x => x.Criterio, Criterio.Criterio)
                .AddToSetEach(x => x.Valor, Criterio.Valor)
                .AddToSetEach(x => x.Evaluacion, Criterio.Evaluacion);
            colectionCA.UpdateOne(filterCA, updateCA);
        }
        

        var filtroGrupo = Builders<Crear>.Filter.Eq(x => x.Codigo, ingresar.Codigo);
        var update = Builders<Crear>.Update.AddToSet(x => x.Miembros, Usuario).AddToSet(x => x.Correos, DatosUsuario.Correo);
        colectionGrupos.UpdateOne(filtroGrupo, update);

        return Results.Ok("Miembro insertado.");
    }
    public static IResult EliminarGrupo(string IdProfesor, string IdGrupo){
        var Profesor = IdProfesor.ToLower();
        var Grupo = IdGrupo.ToLower();
        if (string.IsNullOrWhiteSpace(Grupo)) {
            return Results.BadRequest("Introduce el id del grupo.");
        }
        if (string.IsNullOrWhiteSpace(Profesor)) {
            return Results.BadRequest("Introduce el id del creador.");
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

        var colectionWorks = bd.ObtenerColeccion<Tareas>("Tareas");
        if (colectionWorks == null) {
            throw new Exception("No existe la colección Tareas.");
        }

        var colectionTA = bd.ObtenerColeccion<TareasAlumnos>("TareasAlumnos");
        if (colectionTA == null) {
            throw new Exception("No existe la colección TareasAlumnos");
        }

        FilterDefinitionBuilder<Registro> filterBuilder = new FilterDefinitionBuilder<Registro>();
        var filter = filterBuilder.Eq(x => x.IdUsuario, Profesor);
        Registro? cuentaExistente = colectionUsers.Find(filter).FirstOrDefault();
        if (cuentaExistente == null) {
            return Results.BadRequest("No existe el usuario.");
        }
        if (cuentaExistente.Rol != "profesor") {
            return Results.BadRequest("El que borra el comentario debe ser un profesor.");
        }

        FilterDefinitionBuilder<Crear> filterBuilder2 = new FilterDefinitionBuilder<Crear>();
        var filter2 = filterBuilder2.Eq(x => x.IdGrupo, Grupo);
        Crear? grupoExistente = colectionGroup.Find(filter2).FirstOrDefault();
        if (grupoExistente == null) {
            return Results.BadRequest("No existe el grupo.");
        }
        if (grupoExistente.IdProfesor != Profesor) {
            return Results.BadRequest("No es el creador del grupo.");
        }

        colectionCriterios.DeleteMany(Builders<Criterios>.Filter.Eq(x => x.Grupo, Grupo));
        colectionCA.DeleteMany(Builders<CriterioAlumno>.Filter.Eq(x => x.IdGrupo, Grupo));
        colectionWorks.DeleteMany(Builders<Tareas>.Filter.Eq(x => x.IdGrupo, Grupo));
        colectionTA.DeleteMany(Builders<TareasAlumnos>.Filter.Eq(x => x.IdGrupo, Grupo));      
        colectionGroup.DeleteOne(filter2);
        return Results.Ok("Grupo eliminado.");
    }

    public static IResult BorrarMiembros(string IdProfesor, string IdGrupo, string IdMiembro) {
        var Profesor = IdProfesor.ToLower();
        var Grupo = IdGrupo.ToLower();
        var Miembro = IdMiembro.ToLower();
        if (string.IsNullOrWhiteSpace(Profesor)) {
            return Results.BadRequest("Introduce el id del profesor.");
        }
        if (string.IsNullOrWhiteSpace(Grupo)) {
            return Results.BadRequest("Introduce el id del grupo.");
        }
        if (string.IsNullOrWhiteSpace(Miembro)) {
            return Results.BadRequest("Introduce el id del miembro.");
        }
        BaseDatos bd = new BaseDatos();
        var colectionUsers = bd.ObtenerColeccion<Registro>("Usuarios");
        if (colectionUsers == null) {
            throw new Exception("No existe la colección Usuarios");
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

        var colectionWorks = bd.ObtenerColeccion<Tareas>("Tareas");
        if (colectionWorks == null) {
            throw new Exception("No existe la colección Tareas.");
        }

        var colectionTA = bd.ObtenerColeccion<TareasAlumnos>("TareasAlumnos");
        if (colectionTA == null) {
            throw new Exception("No existe la colección TareasAlumnos");
        }
        var colectionComments = bd.ObtenerColeccion<Comentarios>("Comentarios");
        if (colectionComments == null) {
            throw new Exception("No existe la colección Comentarios.");
        }
        
        FilterDefinitionBuilder<Registro> filterBuilder1 = new FilterDefinitionBuilder<Registro>();
        var filter1 = filterBuilder1.Eq(x => x.IdUsuario, Profesor);
        Registro? cuentaExistente = colectionUsers.Find(filter1).FirstOrDefault();
        if (cuentaExistente == null) {
            return Results.BadRequest("No existe el usuario");
        }
        if (cuentaExistente.Rol != "profesor") {
            return Results.BadRequest("El usuario debe ser un profesor.");
        }

        filter1 = filterBuilder1.Eq(x => x.IdUsuario, Miembro);
        cuentaExistente = colectionUsers.Find(filter1).FirstOrDefault();
        if (cuentaExistente == null) {
            return Results.BadRequest("No existe el usuario");
        }
        if (cuentaExistente.Rol != "estudiante") {
            return Results.BadRequest("El usuario debe ser un estudiante.");
        }

        FilterDefinitionBuilder<Crear> filterBuilder2 = new FilterDefinitionBuilder<Crear>();
        var filter2 = filterBuilder2.Eq(x => x.IdGrupo, Grupo);
        Crear? grupoExistente = colectionGroup.Find(filter2).FirstOrDefault();
        if (grupoExistente == null) {
            return Results.BadRequest("No existe el grupo");
        }
        if (!grupoExistente.Miembros.Contains(Miembro)) {
            return Results.BadRequest("No existe el usuario en este grupo.");
        }
        
        FilterDefinitionBuilder<CriterioAlumno> filterBuilder3 = new FilterDefinitionBuilder<CriterioAlumno>();
        var filter3 = filterBuilder3.Eq(x => x.IdAlumno, Miembro); 
        colectionCA.DeleteMany(filter3);

        FilterDefinitionBuilder<TareasAlumnos> filterBuilder4 = new FilterDefinitionBuilder<TareasAlumnos>();
        var filter4 = filterBuilder4.Eq(x => x.IdGrupo, Grupo);
        colectionTA.DeleteMany(filter4);

        var ComentariosEnviados = Builders<Comentarios>.Filter.Eq(x => x.IdEscrito, Miembro);
        var ComentariosRecibidos = Builders<Comentarios>.Filter.Eq(x => x.IdRecibir, Miembro);
        var ComentariosMixtos = Builders<Comentarios>.Filter.Or(ComentariosEnviados, ComentariosRecibidos);
        colectionComments.DeleteMany(ComentariosMixtos);

        var filter = Builders<Crear>.Filter.Eq(x => x.IdGrupo, Grupo);
        var Update = Builders<Crear>.Update.Pull(x => x.Miembros, Miembro).Pull(x => x.Correos, cuentaExistente.Correo);
        var result = colectionGroup.UpdateOne(filter, Update);

        return Results.Ok("Miembro eliminado.");
    }

    public static IResult VerGrupos(string IdUsuario) {
        var Usuario = IdUsuario.ToLower();
        if (string.IsNullOrWhiteSpace(IdUsuario)) {
            return Results.BadRequest("Introduce el id de un usuario.");
        }
        BaseDatos bd = new BaseDatos();

        var colectionUsers = bd.ObtenerColeccion<Registro>("Usuarios");
        var colectionGroup = bd.ObtenerColeccion<Crear>("Grupos");

        var filterBuilder1 = new FilterDefinitionBuilder<Crear>();
        var filter1 = filterBuilder1.AnyEq(x => x.Miembros, Usuario);
        var lista = colectionGroup.Find(filter1).ToList();

        var filterBuilder2 = new FilterDefinitionBuilder<Registro>();
        var filter = filterBuilder2.Eq(x => x.IdUsuario, Usuario);
        var UsuarioExistente = colectionUsers.Find(filter).FirstOrDefault();
        if (UsuarioExistente == null) {
            return Results.BadRequest("No existe el usuario.");
        }
        if (UsuarioExistente.Rol != "estudiante") {
            return Results.BadRequest("El usuario debe ser estudiante.");
        } 

        var Grupos = lista.Select(x => {
            var filter2 = filterBuilder2.Eq(y => y.IdUsuario, x.IdProfesor);
            var Profesor = colectionUsers.Find(filter2).FirstOrDefault();

            return new {
                IdGrupo = x.IdGrupo,
                Grupo = x.Nombre,
                Imagen = x.Imagen,
                Profesor = Profesor != null ? Profesor.Nombre : "Desconocido",
                Miembros = x.Miembros.Count()
            };
        });
        
        return Results.Ok(Grupos);
    }

    
public static IResult VerMisGrupos (string IdProfesor) {
        var Profesor = IdProfesor.ToLower();
        if (string.IsNullOrWhiteSpace(IdProfesor)) {
            return Results.BadRequest("Introduce el id de un usuario.");
        }
        BaseDatos bd = new BaseDatos();

        var colectionUsers = bd.ObtenerColeccion<Registro>("Usuarios");
        var colectionGroup = bd.ObtenerColeccion<Crear>("Grupos");

        var filterBuilder1 = new FilterDefinitionBuilder<Registro>();
        var filter1 = filterBuilder1.Eq(x => x.IdUsuario, Profesor);
        var profesor = colectionUsers.Find(filter1).FirstOrDefault();
        if (profesor == null) {
            return Results.BadRequest("No existe el usuario.");
        }
        if (profesor.Rol != "profesor") {
            return Results.BadRequest("El usuario debe ser profesor.");
        } 

        FilterDefinitionBuilder<Crear> filterBuilder2 = new FilterDefinitionBuilder<Crear>();
        var filter2 = filterBuilder2.Eq(x => x.IdProfesor, Profesor);
        var lista = colectionGroup.Find(filter2).ToList();
        var MisGrupos = lista.Select(x => {
            return new {
                IdGrupo = x.IdGrupo,
                Grupo = x.Nombre,
                Codigo = x.Codigo,
                Imagen = x.Imagen,
                Miembros = x.Miembros.Count()
            };
        });
        
        return Results.Ok(MisGrupos);
    }
    public static IResult Grupo (string Id, string IdGrupo) {
        var Usuario = Id.ToLower();
        var Grupo = IdGrupo.ToLower();
        if (string.IsNullOrWhiteSpace(Usuario)) { 
            return Results.BadRequest("Introduce el id del usuario.");
        }
        BaseDatos bd = new BaseDatos();
        var colectionUsers = bd.ObtenerColeccion<Registro>("Usuarios");
        var colectionGroup = bd.ObtenerColeccion<Crear>("Grupos");
        var colectionWorks = bd.ObtenerColeccion<Tareas>("Tareas");
        var colectionTA = bd.ObtenerColeccion<TareasAlumnos>("TareasAlumnos");

        FilterDefinitionBuilder<Registro> filterBuilder1 = new FilterDefinitionBuilder<Registro>();
        var filter1 = filterBuilder1.Eq(x => x.IdUsuario, Usuario);
        Registro? usuario = colectionUsers.Find(filter1).FirstOrDefault();
        if (usuario == null) {
            return Results.BadRequest("No existe el usuario.");
        }
        FilterDefinitionBuilder<Crear> filterBuilder2 = new FilterDefinitionBuilder<Crear>();
        var filter2 = filterBuilder2.Eq(x => x.IdGrupo, Grupo);
        Crear? grupo = colectionGroup.Find(filter2).FirstOrDefault();
        if (grupo == null) {
            return Results.BadRequest("No existe el grupo.");
        }
        if (grupo.IdProfesor != Usuario && !grupo.Miembros.Contains(Usuario)) {
            return Results.BadRequest("El usuario no pertenece al grupo.");
        }
        FilterDefinitionBuilder<Tareas> filterBuilder3 = new FilterDefinitionBuilder<Tareas>();
        var filter3 = filterBuilder3.Eq(x => x.IdGrupo, Grupo);
        var tareas = colectionWorks.Find(filter3).ToList();

        if (usuario.Rol.ToLower() == "profesor") {
            var filtro = Builders<Tareas>.Filter.Eq(x => x.IdGrupo, Grupo);
            var mostrar = colectionWorks.Find(filtro).ToList();
            return Results.Ok(new {
                IdProfesor = grupo.IdProfesor,
                Nombre = grupo.Nombre,
                Rol = usuario.Rol,
                Imagen = grupo.Imagen,
                Tareas = mostrar.Select(x => new {
                    x.IdTarea,
                    x.Titulo,
                    x.Descripcion,
                    x.ValorMax
                })
            });
        } else if (usuario.Rol.ToLower() == "estudiante" && (grupo.Miembros != null && grupo.Miembros.Contains(Usuario))) {
            var filtro = Builders<TareasAlumnos>.Filter.And(Builders<TareasAlumnos>.Filter.Eq(x => x.IdGrupo, Grupo), Builders<TareasAlumnos>.Filter.Eq(x => x.IdUsuario, Usuario));
            var mostrar = colectionTA.Find(filtro).ToList();
            return Results.Ok(new {
                IdProfesor= grupo.IdProfesor,
                Nombre = grupo.Nombre,
                Rol = usuario.Rol,
                Imagen = grupo.Imagen,
                Tareas = mostrar.Select(x => new {
                    x.IdUsuario,
                    x.IdTarea,
                    x.Titulo,
                    x.Descripcion,
                    x.ValorMax,
                    x.Calificacion,
                    x.Evidencia
                })
            });
        } else {
            return Results.BadRequest("No tiene rol.");
        }
    }
public static IResult InfoGrupo(string Id, string IdGrupo) {
    var Usuario = Id.ToLower();
    var Grupo = IdGrupo.ToLower();
    BaseDatos bd = new BaseDatos();
    var colectionUsers = bd.ObtenerColeccion<Registro>("Usuarios");
    var colectionGroup = bd.ObtenerColeccion<Crear>("Grupos");
    var colectionCriterio = bd.ObtenerColeccion<Criterios>("Criterios");

    // Buscar usuario
    var filter1 = Builders<Registro>.Filter.Eq(x => x.IdUsuario, Usuario);
    var usuario = colectionUsers.Find(filter1).FirstOrDefault();
    if (usuario == null) return Results.BadRequest("No existe el usuario.");

    // Buscar grupo
    var filter2 = Builders<Crear>.Filter.Eq(x => x.IdGrupo, Grupo);
    var grupo = colectionGroup.Find(filter2).FirstOrDefault();
    if (grupo == null) return Results.BadRequest("No existe el grupo.");

    // Verificar si el usuario pertenece al grupo
    if (grupo.IdProfesor != Usuario && !grupo.Miembros.Contains(Usuario))
        return Results.BadRequest("El usuario no pertenece al grupo.");

    // Obtener miembros del grupo
    var filterMembers = Builders<Registro>.Filter.In(x => x.IdUsuario, grupo.Miembros);
    var miembros = colectionUsers.Find(filterMembers).ToList();
    var DatosMiembros = miembros.Select(x => new {
        x.IdUsuario,
        x.Nombre,
        x.Correo
    });

    var rol = usuario.Rol.ToLower(); // Obtener el rol del usuario

    if (rol == "profesor") {
        // Buscar criterios asociados al grupo
        var filterCriterios = Builders<Criterios>.Filter.Eq(x => x.Grupo, Grupo);
        var criterio = colectionCriterio.Find(filterCriterios).FirstOrDefault();

        return Results.Ok(new {
            IdUsuario = Usuario,
            IdCriterio = criterio?.IdCriterio,
            Rol = rol,
            Nombre = grupo.Nombre,
            Codigo = grupo.Codigo,
            Imagen = grupo.Imagen,
            Miembros = grupo.Miembros.Count(),
            MiembrosDatos = DatosMiembros
        });
    } else if (rol == "estudiante") {
        return Results.Ok(new {
            IdUsuario = Usuario,
            Rol=rol,
            Nombre = grupo.Nombre,
            Imagen = grupo.Imagen,
            Miembros = grupo.Miembros.Count(),
            MiembrosDatos = DatosMiembros,
        });
    } else {
        return Results.BadRequest("No tiene rol.");
    }
}

    public static IResult CriterioAlumno(string Id, string IdGrupo)
{
    var Profesor = Id.ToLower();
    var Grupo = IdGrupo.ToLower();
    BaseDatos bd = new BaseDatos();
    var colectionUsers = bd.ObtenerColeccion<Registro>("Usuarios");
    var colectionGroup = bd.ObtenerColeccion<Crear>("Grupos");
    var colectionCA = bd.ObtenerColeccion<CriterioAlumno>("CriteriosAlumnos");
    var colectionComments = bd.ObtenerColeccion<Comentarios>("Comentarios");

    // Validar usuario
    var filter1 = Builders<Registro>.Filter.Eq(x => x.IdUsuario, Profesor);
    Registro? usuario = colectionUsers.Find(filter1).FirstOrDefault();
    if (usuario == null)
    {
        return Results.BadRequest("No existe el usuario.");
    }
    if (usuario.Rol.ToLower() != "estudiante")
    {
        return Results.BadRequest("El usuario debe ser un profesor.");
    }

    // Validar grupo
    var filter2 = Builders<Crear>.Filter.Eq(x => x.IdGrupo, Grupo);
    Crear? grupo = colectionGroup.Find(filter2).FirstOrDefault();
    if (grupo == null)
    {
        return Results.BadRequest("No existe el grupo.");
    }
    if (usuario.Rol.ToLower() == "profesor" && grupo.IdProfesor != Profesor)
    {
        return Results.BadRequest("El profesor no es el creador del grupo");
    }

    // Validar criterios
    var filter3 = Builders<CriterioAlumno>.Filter.Eq(x => x.IdGrupo, Grupo);
    var criterioA = colectionCA.Find(filter3).ToList();
    if (!criterioA.Any())
    {
        return Results.BadRequest("No existen criterios.");
    }

    // Obtener comentarios
    var filter4 = Builders<Comentarios>.Filter.Eq(x => x.IdGrupo, Grupo);
    var comentario = colectionComments.Find(filter4).ToList();

    var CriterioAlumno = criterioA.Select(y =>
    {
        var alumno = colectionUsers.Find(Builders<Registro>.Filter.Eq(x => x.IdUsuario, y.IdAlumno)).FirstOrDefault();
        var comentariosRecibidos = comentario
            .Where(w => w.IdRecibir == y.IdAlumno && w.IdGrupo == y.IdGrupo)
            .Select(w => new
            {
                Id = w.IdComentario,
                EscritoPor = w.IdEscrito,
                Comentario = w.Comentario
            });

        var comentariosEnviados = comentario
            .Where(w => w.IdEscrito == y.IdAlumno && w.IdGrupo == y.IdGrupo)
            .Select(w => new
            {
                Id = w.IdComentario,
                EscritoPor = w.IdEscrito,
                Comentario = w.Comentario
            });

        return new
        {
            Alumno = alumno?.Nombre ?? "Alumno no encontrado",
            Criterios = y.Criterio,
            Valor = y.Valor,
            Evaluacion = y.Evaluacion,
            ComentariosRecibidos = comentariosRecibidos,
            ComentariosEnviados = comentariosEnviados
        };
    });

    return Results.Ok(new
    {
        Grupo = grupo.Nombre,
        Imagen = grupo.Imagen,
        IdProfesor = grupo.IdProfesor,
        CriteriosAlumnos = CriterioAlumno
    });
}

    public static IResult TareaAlumno(string Id, string IdGrupo, string IdTarea, string IdUsuario) {
        var Usuario = IdUsuario.ToLower();
        var Grupo = IdGrupo.ToLower();
        var Tarea = IdTarea.ToLower();
        var Revisador = Id.ToLower();

        BaseDatos bd = new BaseDatos();
        var colectionUsers = bd.ObtenerColeccion<Registro>("Usuarios");
        var colectionGroup = bd.ObtenerColeccion<Crear>("Grupos");
        var colectionTA = bd.ObtenerColeccion<TareasAlumnos>("TareasAlumnos");
        
        FilterDefinitionBuilder<Registro> filterBuilder1 = new FilterDefinitionBuilder<Registro>();
        var filter1 = filterBuilder1.Eq(x => x.IdUsuario, Revisador);
        Registro? usuario = colectionUsers.Find(filter1).FirstOrDefault();
        if (usuario == null) {
            return Results.BadRequest("No existe el usuario.");
        }
        if (usuario.Rol.ToLower() != "profesor" && usuario.Rol.ToLower() != "estudiante") {
            return Results.BadRequest("El usuario debe ser tener un rol.");
        }

        if (usuario.Rol.ToLower() == "estudiante" && usuario.IdUsuario != Usuario) {
            return Results.BadRequest("No puedes revisar tareas que noson tuyas.");
        }
        var filter2 = filterBuilder1.Eq(x => x.IdUsuario, Usuario);
        Registro? usuario2 = colectionUsers.Find(filter2).FirstOrDefault();
        if (usuario2 == null) {
            return Results.BadRequest("No existe el usuario.");
        }
        if (usuario2.Rol.ToLower() != "estudiante") {
            return Results.BadRequest("El usuario debe ser un estudiante.");
        }

        FilterDefinitionBuilder<Crear> filterBuilder2 = new FilterDefinitionBuilder<Crear>();
        var filter3 = filterBuilder2.Eq(x => x.IdGrupo, Grupo);
        Crear? grupo = colectionGroup.Find(filter3).FirstOrDefault();
        if (grupo == null) {
            return Results.BadRequest("No existe el gurpo.");
        }
        if (usuario.Rol.ToLower() == "profesor" && grupo.IdProfesor != Revisador) {
            return Results.BadRequest("El profesor no es el creador del grupo");
        }

        FilterDefinitionBuilder<TareasAlumnos> filterBuilder3 = new FilterDefinitionBuilder<TareasAlumnos>();
        var filter4 = filterBuilder3.Eq(x => x.IdGrupo, Grupo);
        TareasAlumnos? tarea = colectionTA.Find(filter4).FirstOrDefault();
        if (tarea == null) { 
            return Results.BadRequest("No existe la tarea.");
        }
        if (tarea.IdUsuario != Usuario) {
            return Results.BadRequest("El la tarea no es de este usuario");
        }

        return Results.Ok(new {
            Grupo = grupo.Nombre,
            Alumno = usuario2.Nombre,
            Tarea = new {
                tarea.Titulo,
                tarea.Descripcion,
                tarea.ValorMax,
                tarea.Calificacion,
                tarea.Evidencia
            }
        });
    }

    public static IResult CriterioGrupo(string IdProfesor, string IdGrupo)
{
    var Profesor = IdProfesor.ToLower();
    var Grupo = IdGrupo.ToLower();

    BaseDatos bd = new BaseDatos();
    var colectionUsers = bd.ObtenerColeccion<Registro>("Usuarios");
    var colectionGroup = bd.ObtenerColeccion<Crear>("Grupos");
    var colectionCriterios = bd.ObtenerColeccion<Criterios>("Criterios");

    // Verificar que el usuario sea un profesor
    FilterDefinitionBuilder<Registro> filterBuilder1 = new FilterDefinitionBuilder<Registro>();
    var filter1 = filterBuilder1.Eq(x => x.IdUsuario, Profesor);
    Registro? usuario = colectionUsers.Find(filter1).FirstOrDefault();
    if (usuario == null)
    {
        return Results.BadRequest("No existe el usuario.");
    }
    if (usuario.Rol != "profesor")
    {
        return Results.BadRequest("El usuario debe ser el profesor.");
    }

    // Verificar que el grupo existe y el profesor es el creador
    FilterDefinitionBuilder<Crear> filterBuilder2 = new FilterDefinitionBuilder<Crear>();
    var filter2 = filterBuilder2.Eq(x => x.IdGrupo, Grupo);
    Crear? grupo = colectionGroup.Find(filter2).FirstOrDefault();
    if (grupo == null)
    {
        return Results.BadRequest("No existe el grupo.");
    }
    if (grupo.IdProfesor != Profesor)
    {
        return Results.BadRequest("El usuario no es el creador del grupo.");
    }

    // Obtener los criterios para el grupo
    FilterDefinitionBuilder<Criterios> filterBuilder3 = new FilterDefinitionBuilder<Criterios>();
    var filter3 = filterBuilder3.Eq(x => x.Grupo, Grupo);
    var criterios = colectionCriterios.Find(filter3).ToList();
    if (!criterios.Any())
    {
        return Results.BadRequest("No existen criterios en el grupo.");
    }

    // Retornar los resultados
    return Results.Ok(new
    {
        Grupo = grupo.Nombre,
        Imagen = grupo.Imagen,
        Criterio = criterios.Select(x => new
        {
            x.IdCriterio,
            x.Criterio,
            x.Valor,
            x.Evaluacion
        })
    });
}

    public static IResult TareaAlumnos(string IdProfesor, string IdGrupo, string IdTarea) {
        var Profesor = IdProfesor.ToLower();
        var Grupo = IdGrupo.ToLower();
        var Tarea = IdTarea.ToLower();
        BaseDatos bd = new BaseDatos();
        var colectionUsers = bd.ObtenerColeccion<Registro>("Usuarios");
        var colectionGroup = bd.ObtenerColeccion<Crear>("Grupos");
        var colectionTA = bd.ObtenerColeccion<TareasAlumnos>("TareasAlumnos");

        FilterDefinitionBuilder<Registro> filterBuilder1 = new FilterDefinitionBuilder<Registro>();
        var filter1 = filterBuilder1.Eq(x => x.IdUsuario, Profesor);
        Registro? usuario = colectionUsers.Find(filter1).FirstOrDefault();
        if (usuario == null) {
            return Results.BadRequest("No existe el usuario.");
        }
        if (usuario.Rol.ToLower() != "profesor") {
            return Results.BadRequest("El usuario debe ser un profesor.");
        }

        FilterDefinitionBuilder<Crear> filterBuilder2 = new FilterDefinitionBuilder<Crear>();
        var filter2 = filterBuilder2.Eq(x => x.IdGrupo, Grupo);
        Crear? grupo = colectionGroup.Find(filter2).FirstOrDefault();
        if (grupo == null) {
            return Results.BadRequest("No existe el gurpo.");
        }
        if (usuario.Rol.ToLower() == "profesor" && grupo.IdProfesor != Profesor) {
            return Results.BadRequest("El profesor no es el creador del grupo");
        }

        FilterDefinitionBuilder<TareasAlumnos> filterBuilder3 = new FilterDefinitionBuilder<TareasAlumnos>();
        var filter4 = filterBuilder3.And(Builders<TareasAlumnos>.Filter.Eq(x => x.IdGrupo, Grupo) & Builders<TareasAlumnos>.Filter.Eq(x => x.IdTarea, Tarea));
        var tarea = colectionTA.Find(filter4).ToList();
        if (tarea == null || tarea.Count == 0) {
            return Results.BadRequest("No existe las tareas o el grupo.");
        }
        
        var tareaDeAlumnos = tarea.Select(t => new {
            Id = t.IdTarea,
            IdMiembro = t.IdUsuario,
            Miembro = usuario.Nombre,
            Titulo = t.Titulo,
            Descripcion = t.Descripcion,
            ValorMaximo = t.ValorMax,
            Calificacion = t.Calificacion
        }).ToList();
        return Results.Ok(tareaDeAlumnos);
    }
}