using MongoDB.Driver;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http.Json;
using Microsoft.Extensions.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

// Configurar servicios
builder.Services.AddSingleton<BaseDatos>();
builder.Services.AddSingleton<LogrosService>();
builder.Services.AddControllers();
builder.Services.Configure<JsonOptions>(options => options.SerializerOptions.PropertyNamingPolicy = null);
builder.Services.AddCors(options => {
    options.AddPolicy("PermitirFrontend", policy => {
        policy.WithOrigins("http://localhost:5173") // Permitir solicitudes desde el frontend
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

var app = builder.Build();

// Configurar middleware
app.UseCors("PermitirFrontend"); // Aplica la política de CORS
app.UseRouting();
app.UseEndpoints(endpoints => {
    endpoints.MapControllers();
});

// Endpoints existentes
app.MapGet("/VerGrupos/{IdUsuario}", GruposRequestHandler.VerGrupos);
app.MapGet("/MisGrupos/{IdProfesor}", GruposRequestHandler.VerMisGrupos);
app.MapGet("/Grupo/{Id}/{IdGrupo}", GruposRequestHandler.Grupo);
app.MapGet("/InfoGrupo/{Id}/{IdGrupo}", GruposRequestHandler.InfoGrupo);
app.MapGet("/CriterioGrupo/{IdProfesor}/{IdGrupo}", GruposRequestHandler.CriterioGrupo);
app.MapGet("/CriterioAlumno/{Id}/{IdGrupo}", GruposRequestHandler.CriterioAlumno);
app.MapGet("/TareaAlumnos/{IdProfesor}/{IdGrupo}/{IdTarea}", GruposRequestHandler.TareaAlumnos);
app.MapGet("/TareaAlumno/{Id}/{IdGrupo}/{IdTarea}/{IdUsuario}", GruposRequestHandler.TareaAlumno);

app.MapPost("/Registrar", UsuarioRequestHandlers.Registrar);
app.MapPost("/Inicio", UsuarioRequestHandlers.InicioSesion);
app.MapPost("/Recuperar", UsuarioRequestHandlers.Recuperar);
app.MapPost("/ActualizarRol/{IdUsuario}", UsuarioRequestHandlers.ActualizarRol);
app.MapPost("/EliminarUsuario/{IdUsuario}", UsuarioRequestHandlers.Eliminar);

app.MapPost("/CrearGrupo/{IdProfesor}", GruposRequestHandler.CrearGrupo);
app.MapPost("/InsertarMiembro/{Idusuario}", GruposRequestHandler.IngresarMiembro);
app.MapPost("/EliminarGrupo/{IdProfesor}/{IdGrupo}", GruposRequestHandler.EliminarGrupo);
app.MapPost("/EliminarMiembro/{IdProfesor}/{IdGrupo}/{IdMiembro}", GruposRequestHandler.BorrarMiembros);

app.MapPost("/InsertarCriterio/{IdProfesor}/{IdGrupo}/{IdCriterio}", CriteriosRequestHandler.InsertarCriterios);
app.MapPost("/EliminarCriterio/{IdProfesor}/{IdGrupo}/{IdCriterio}", CriteriosRequestHandler.EliminarCriterio);
app.MapPost("/ActualizarCriterio/{IdProfesor}/{IdGrupo}/{IdCriterio}", CriteriosRequestHandler.ActualizarCriterio);
app.MapPost("/CalificarCriterio/{IdProfesor}/{IdGrupo}/{IdCriterio}/{IdAlumno}", CriteriosRequestHandler.Calificar);

app.MapPost("/InsertarTarea/{idProfesor}/{Idgrupo}", TareasRequestHandler.IngresarTarea);
app.MapPost("/ActualizarTarea/{idProfesor}/{Idgrupo}/{IdTarea}", TareasRequestHandler.ActualizarTarea);
app.MapPost("/EliminarTarea/{idProfesor}/{Idgrupo}/{IdTarea}", TareasRequestHandler.EliminarTarea);
app.MapPost("/SubirTarea/{IdUsuario}/{IdGrupo}/{IdTarea}", async (HttpRequest request, string IdUsuario, string IdGrupo, string IdTarea, BaseDatos bd) =>
{
    return await TareasRequestHandler.Subir(request, IdUsuario, IdGrupo, IdTarea, bd);
});

app.MapPost("/CalificarTarea/{IdProfesor}/{IdGrupo}/{IdTarea}/{IdUsuario}", (string IdProfesor, string IdGrupo, string IdTarea, string IdUsuario, Calificar calificacion, LogrosService logrosService, BaseDatos bd) => {
    try {
        // Obtener la colección de tareas de alumnos
        var tareasAlumnosCollection = bd.ObtenerColeccion<TareasAlumnos>("TareasAlumnos");

        // Buscar la tarea del alumno
        var filtro = Builders<TareasAlumnos>.Filter.And(
            Builders<TareasAlumnos>.Filter.Eq(t => t.IdTarea, IdTarea),
            Builders<TareasAlumnos>.Filter.Eq(t => t.IdUsuario, IdUsuario)
        );
        var tareaAlumno = tareasAlumnosCollection.Find(filtro).FirstOrDefault();
        if (tareaAlumno == null) {
            return Results.BadRequest("La tarea no existe para este usuario.");
        }

        // Actualizar la calificación de la tarea
        tareaAlumno.Calificacion = calificacion.Calificacion;
        tareasAlumnosCollection.ReplaceOne(t => t.Id == tareaAlumno.Id, tareaAlumno);

        // Verificar si el logro ya estaba desbloqueado
        var usuario = bd.ObtenerColeccion<Registro>("Usuarios").Find(u => u.IdUsuario == IdUsuario).FirstOrDefault();
        if (usuario.LogrosDesbloqueados != null && usuario.LogrosDesbloqueados.Contains("logro1")) {
            return Results.Ok(new {
                mensaje = "Tarea calificada. El logro ya estaba desbloqueado.",
                logroDesbloqueado = false
            });
        }

        // Desbloquear el logro
        logrosService.DesbloquearLogro(IdUsuario, "logro1");

        return Results.Ok(new {
            mensaje = "Tarea calificada y logro desbloqueado.",
            logroDesbloqueado = true
        });
    } catch (Exception ex) {
        return Results.BadRequest(new { mensaje = ex.Message });
    }
});

app.MapPost("/InsertarComentario/{IdUsuario1}/{IdGrupo}/{IdUsuario2}", ComentariosRequestHandler.IngresarComentario);
app.MapPost("/EliminarComentario/{Usuario1}/{IdGrupo}/{IdComentario}", ComentariosRequestHandler.EliminarComentario);

// Endpoints para el sistema de logros
app.MapPost("/api/Logros/CrearLogro", (Logro logro, LogrosService logrosService) => {
    try {
        logrosService.CrearLogro(logro);
        return Results.Ok("Logro creado correctamente.");
    } catch (Exception ex) {
        return Results.BadRequest(ex.Message);
    }
});

app.MapGet("/api/Logros/ObtenerLogros", (LogrosService logrosService) => {
    try {
        var logros = logrosService.ObtenerTodosLosLogros();
        return Results.Ok(logros);
    } catch (Exception ex) {
        return Results.BadRequest(ex.Message);
    }
});

app.MapPost("/subir/{IdUsuario}/{IdGrupo}/{IdTarea}", async (HttpRequest request, string IdUsuario, string IdGrupo, string IdTarea, BaseDatos bd) =>
{
    return await TareasRequestHandler.Subir(request, IdUsuario, IdGrupo, IdTarea, bd);
});

app.MapGet("/api/Logros/Usuario/{idUsuario}", (string idUsuario, LogrosService logrosService) => {
    try {
        // Obtener todos los logros
        var todosLosLogros = logrosService.ObtenerTodosLosLogros();

        // Obtener los logros desbloqueados por el usuario
        var logrosDesbloqueados = logrosService.ObtenerLogrosDesbloqueadosPorUsuario(idUsuario);

        // Marcar cuáles logros están desbloqueados
        var logrosConEstado = todosLosLogros.Select(logro => new {
            Id = logro.Id,
            Nombre = logro.Nombre,
            Descripcion = logro.Descripcion,
            Desbloqueado = logrosDesbloqueados.Contains(logro.Id)
        });

        return Results.Ok(logrosConEstado);
    } catch (Exception ex) {
        return Results.BadRequest(new { mensaje = ex.Message });
    }
});

app.Run();
