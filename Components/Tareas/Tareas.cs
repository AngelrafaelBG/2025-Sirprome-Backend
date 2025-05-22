using MongoDB.Bson;
using MongoDB.Driver;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using System.Text.Json;

public class Tareas {
    public ObjectId Id;
    public string? IdTarea { get; set; } = string.Empty;
    public string? IdGrupo { get; set; } = string.Empty;
    public string? Titulo { get; set; } = string.Empty;
    public string? Descripcion { get; set; } = string.Empty;
    public int ValorMax { get; set; }
}

public class actualizarTarea {
    public string? Titulo { get; set; } = string.Empty;
    public string? Descripcion { get; set; } = string.Empty;
    public int ValorMax { get; set; }
}

public class TareasAlumnos {
    public ObjectId Id;
    public string? IdTarea { get; set; } = string.Empty;
    public string? IdUsuario { get; set; } = string.Empty;
    public string? IdGrupo { get; set; } = string.Empty;
    public string? Titulo { get; set; } = string.Empty;
    public string? Descripcion { get; set; } = string.Empty;
    public int Calificacion { get; set; } 
    public int ValorMax { get; set; }
    public string? Evidencia { get; set; } = string.Empty;
    
}

public class ActualizarTA {
    public int calificación { get; set; }
}

public class Subir {
    public string? Evidencia { get; set; } = string.Empty;
}

public class Calificar {
    public int Calificacion { get; set; }
}

public static class TareasEndpoints {
    public static void MapTareasEndpoints (this IEndpointRouteBuilder routes) {
        routes.MapPost("/subir/{IdUsuario}/{IdGrupo}/{IdTarea}", Subir);
    }

    public static async Task<IResult> Subir(HttpRequest request, string IdUsuario, string IdGrupo, string IdTarea, BaseDatos bd) {
        try {
            var body = await request.ReadFromJsonAsync<Subir>();
            if (body == null || string.IsNullOrWhiteSpace(body.Evidencia)) {
                return Results.BadRequest("La evidencia no puede estar vacía.");
            }

            return ProcesarSubida(body, IdUsuario, IdGrupo, IdTarea, bd);
        } catch (Exception ex) {
            return Results.BadRequest(new { mensaje = ex.Message });
        }
    }

    private static IResult ProcesarSubida(Subir body, string IdUsuario, string IdGrupo, string IdTarea, BaseDatos bd) {
        var tareasAlumnosCollection = bd.ObtenerColeccion<TareasAlumnos>("TareasAlumnos");

        var filtro = Builders<TareasAlumnos>.Filter.And(
            Builders<TareasAlumnos>.Filter.Eq(t => t.IdTarea, IdTarea),
            Builders<TareasAlumnos>.Filter.Eq(t => t.IdGrupo, IdGrupo),
            Builders<TareasAlumnos>.Filter.Eq(t => t.IdUsuario, IdUsuario)
        );
        var tareaAlumno = tareasAlumnosCollection.Find(filtro).FirstOrDefault();
        if (tareaAlumno != null) {
            return Results.BadRequest("La tarea ya fue subida.");
        }

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
    }
}