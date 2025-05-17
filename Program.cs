using Microsoft.AspNetCore.Http.Json;

var builder = WebApplication.CreateBuilder(args);
builder.Services.Configure<JsonOptions>(options => options.SerializerOptions.PropertyNamingPolicy = null);
builder.Services.AddCors();
var app = builder.Build();

app.UseCors(policy => policy.AllowAnyHeader().AllowAnyMethod().AllowAnyOrigin());

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
app.MapPost("/SubirTarea/{Idusuario}/{IdGrupo}/{IdTarea}", TareasRequestHandler.Subir);
app.MapPost("/CalificarTarea/{IdProfesor}/{IdGrupo}/{IdTarea}/{IdUsuario}", TareasRequestHandler.Calificar);

app.MapPost("/InsertarComentario/{IdUsuario1}/{IdGrupo}/{IdUsuario2}", ComentariosRequestHandler.IngresarComentario);
app.MapPost("/EliminarComentario/{Usuario1}/{IdGrupo}/{IdComentario}", ComentariosRequestHandler.EliminarComentario);

app.Run();
