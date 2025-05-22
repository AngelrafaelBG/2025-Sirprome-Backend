using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Net;
using System.Net.Mail;

public static class UsuarioRequestHandlers {
    public static IResult Registrar(Registro datos, LogrosService logrosService) {
        if (string.IsNullOrWhiteSpace(datos.Nombre)) {
            return Results.BadRequest("El nombre es necesario.");
        }
        if (string.IsNullOrWhiteSpace(datos.Correo)) {
            return Results.BadRequest("El correo es necesario.");
        }
        if (string.IsNullOrWhiteSpace(datos.Contraseña)) {
            return Results.BadRequest("Es necesaria una contraseña.");
        }

        datos.IdUsuario = Guid.NewGuid().ToString().ToLower();

        BaseDatos bd = new BaseDatos();
        var coleccion = bd.ObtenerColeccion<Registro>("Usuarios");
        if (coleccion == null) {
            throw new Exception("No existe la colección Usuarios.");
        }

        var filter = Builders<Registro>.Filter.Eq(x => x.Correo, datos.Correo);
        Registro? usuarioExistente = coleccion.Find(filter).FirstOrDefault();
        /*
        if (usuarioExistente != null) {
            return Results.BadRequest($"Ya existe un usuario con el correo electrónico {datos.Correo}");
        }
*/
        // Crear el logro "SIRPROME" si no existe
        logrosService.CrearLogroSiNoExiste(new Logro {
            Id = "SIRPROME",
            Nombre = "Un cambio",
            Descripcion = "Registrarse por primera vez a SIRPROME"
        });

        // Asignar el logro al usuario
        datos.LogrosDesbloqueados = new List<string> { "SIRPROME" };

        // Insertar el usuario en la base de datos
        coleccion.InsertOne(datos);

        return Results.Ok(new {
            id = datos.IdUsuario,
            mensaje = "Usuario registrado correctamente.",
            logroDesbloqueado = new {
                Id = "SIRPROME",
                Nombre = "Un cambio",
                Descripcion = "Registrarse por primera vez a SIRPROME"
            }
        });
    }
    public static IResult InicioSesion(Inicio inicio) {
        if (string.IsNullOrWhiteSpace(inicio.Correo)) {
            return Results.BadRequest("Falta el Correo electronico.");
        }
        if (string.IsNullOrWhiteSpace(inicio.Contraseña)) {
            return Results.BadRequest("Falta la contraseña.");
        }
        
        BaseDatos bd = new BaseDatos();
        var coleccion = bd.ObtenerColeccion<Registro>("Usuarios");
        if (coleccion == null) {
            throw new Exception("No existe la colección Usuarios");
        }

        FilterDefinitionBuilder<Registro> filterBuilder = new FilterDefinitionBuilder<Registro>();
        var filter = filterBuilder.Eq(x => x.Correo, inicio.Correo);
        Registro? Inicio = coleccion.Find(filter).FirstOrDefault();
        if (Inicio == null) {
            return Results.BadRequest($"No existe el correo {inicio.Correo}");
        }
        filter = filterBuilder.Eq(x => x.Contraseña, inicio.Contraseña);
        Inicio = coleccion.Find(filter).FirstOrDefault();
        if (Inicio == null) {
            return Results.BadRequest("Contraseña incorrecta");
        }
        return Results.Ok(new {id = Inicio.IdUsuario, rol = Inicio.Rol, mensaje = "inicio exitoso"});
    }
    
    public static IResult Recuperar(Recuperar recuperar) {
        if (string.IsNullOrWhiteSpace(recuperar.Correo)) {
            return Results.BadRequest("Se requiere el correo para continuar");
        }
        BaseDatos bd = new BaseDatos();
        var coleccion = bd.ObtenerColeccion<Registro>("Usuarios");
        if (coleccion == null) {
            throw new Exception("No existe la colección Usuarios");
        }
        FilterDefinitionBuilder<Registro> filterBuilder = new FilterDefinitionBuilder<Registro>();
        var filter = filterBuilder.Eq(x => x.Correo, recuperar.Correo);
        Registro? usuario = coleccion.Find(filter).FirstOrDefault();
        if (usuario == null) {
            return Results.BadRequest($"No existe el correo {recuperar.Correo}");
        } 
        Correo correo = new Correo{
            Destinatario = usuario.Correo,
            Asunto = "Recuperar contraseña.",
            Mensaje = $"Tu contraseña es: {usuario.Contraseña}" 
        };
        EnviarCorreoAsync(correo);
        return Results.Ok("Correo enviado.");
    }
    
    public static IResult ActualizarRol(Actualizar actualizar, string IdUsuario) {
        if (string.IsNullOrWhiteSpace(IdUsuario.ToLower())) {
            return Results.BadRequest("Introduce el usuario.");
        }
        if (string.IsNullOrWhiteSpace(actualizar.Rol)) {
            return Results.BadRequest("Rol no puede estar vacio.");
        }
        if (!actualizar.Rol.ToLower().Equals("estudiante") && !actualizar.Rol.ToLower().Equals("profesor")) {
            return Results.BadRequest("El rol debe ser profesor o estudiante.");
        }

        BaseDatos bd = new BaseDatos();
        var coleccion = bd.ObtenerColeccion<Registro>("Usuarios");
        if (coleccion == null) {
            throw new Exception("No existe la coleccón Usuarios.");
        }

        FilterDefinitionBuilder<Registro> filterBuilder = new FilterDefinitionBuilder<Registro>();
        var filter1 = filterBuilder.Eq(x => x.IdUsuario, IdUsuario.ToLower());
        Registro? filtro = coleccion.Find(filter1).FirstOrDefault();
        if (filtro == null) {
            return Results.BadRequest("No se encontro la cuenta que busca.");
        }
        
        var update = Builders<Registro>.Update.Set(x => x.Rol, actualizar.Rol);

        var result = coleccion.UpdateOne(filter1, update);

        if (result.ModifiedCount == 0) {
            return Results.BadRequest("No se pudo actualizar el rol.");
        }

        return Results.Ok("El rol fue actualizado.");
    }
    public static IResult Eliminar(string IdUsuario){
        if (string.IsNullOrWhiteSpace(IdUsuario.ToLower())) {
            return Results.BadRequest("Se necesita el correo para continuar.");
        }

        BaseDatos bd = new BaseDatos();
        var colection = bd.ObtenerColeccion<Registro>("Usuarios");
        if (colection == null) {
            throw new Exception("No existe la colección Usuarios.");
        }

        FilterDefinitionBuilder<Registro> filterBuilder = new FilterDefinitionBuilder<Registro>();
        var filter = filterBuilder.Eq(x => x.IdUsuario, IdUsuario.ToLower());
        Registro? cuentaExistente = colection.Find(filter).FirstOrDefault();
        if (cuentaExistente == null) {
            return Results.BadRequest("No existe el usuario.");
        }

        colection.DeleteOne(filter);
        return Results.Ok("Usuario eliminado");
    }
    private static async void EnviarCorreoAsync(Correo correo) {
        using (SmtpClient smtp = new SmtpClient("smtp.gmail.com", 587)) {
            smtp.EnableSsl = true;
            smtp.Credentials = new System.Net.NetworkCredential("sirprome@gmail.com", "xbzk zmwm qqyt rcqd");

            MailMessage mensaje = new MailMessage {
                From = new MailAddress("sirprome@gmail.com", "SIRPROME"),
                Subject = correo.Asunto,
                Body = correo.Mensaje,
                IsBodyHtml = true
            };
            mensaje.To.Add(correo.Destinatario);
            await smtp.SendMailAsync(mensaje);
        }
    }
}

