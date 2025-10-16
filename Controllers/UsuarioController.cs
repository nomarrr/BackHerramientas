using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;

[ApiController]
[Route("api/[controller]")]
public class UsuarioController : ControllerBase
{
    private readonly string _connectionString;
    private readonly ILogger<UsuarioController> _logger;

    public UsuarioController(ILogger<UsuarioController> logger)
    {
        _connectionString = @"Data Source=LAPTOP-7MITNTQF\SQLEXPRESS;Initial Catalog=HerramientasV2;User ID=sa;Password=admin;Encrypt=True;TrustServerCertificate=True";
        _logger = logger;
    }

    // Obtener todos los usuarios
    [HttpGet]
    public IActionResult GetAllUsuarios()
    {
        var response = new UsuariosResponse();

        using (SqlConnection conn = new SqlConnection(_connectionString))
        {
            try
            {
                conn.Open();
                string query = "SELECT Id, Nombre FROM Usuarios";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            response.usuarios.Add(new UsuarioItem
                            {
                                Id = (int)reader["Id"],
                                Nombre = reader["Nombre"].ToString()
                            });
                        }
                    }
                }

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener todos los usuarios");
                return StatusCode(500, new { error = ex.Message });
            }
        }
    }

    // Obtener usuario por ID
    [HttpGet("{id}")]
    public IActionResult GetUsuarioById(int id)
    {
        using (SqlConnection conn = new SqlConnection(_connectionString))
        {
            try
            {
                conn.Open();
                string query = "SELECT Id, Nombre FROM Usuarios WHERE Id = @Id";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@Id", id);

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            var usuario = new UsuarioItem
                            {
                                Id = (int)reader["Id"],
                                Nombre = reader["Nombre"].ToString()
                            };
                            return Ok(usuario);
                        }
                        else
                        {
                            return NotFound(new { error = $"No se encontró el usuario con ID {id}" });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener usuario por ID: {Id}", id);
                return StatusCode(500, new { error = ex.Message });
            }
        }
    }

    // Crear nuevo usuario
    [HttpPost]
    public IActionResult CreateUsuario([FromBody] UsuarioRequest request)
    {
        try
        {
            if (request == null)
            {
                return BadRequest(new { error = "La solicitud no puede estar vacía" });
            }

            if (string.IsNullOrEmpty(request.Nombre))
            {
                return BadRequest(new { error = "El nombre del usuario es requerido" });
            }

            if (request.Nombre.Length > 50)
            {
                return BadRequest(new { error = "El nombre del usuario no puede exceder los 50 caracteres" });
            }

            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                string query = @"INSERT INTO Usuarios (Nombre) 
                               VALUES (@Nombre);
                               SELECT SCOPE_IDENTITY();";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@Nombre", request.Nombre);

                    int newId = Convert.ToInt32(cmd.ExecuteScalar());
                    return Ok(new { message = "Usuario creado correctamente", id = newId });
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al crear usuario");
            return StatusCode(500, new { error = ex.Message });
        }
    }

    // Actualizar usuario existente
    [HttpPut("{id}")]
    public IActionResult UpdateUsuario(int id, [FromBody] UsuarioRequest request)
    {
        try
        {
            if (request == null)
            {
                return BadRequest(new { error = "La solicitud no puede estar vacía" });
            }

            if (request.Nombre != null && request.Nombre.Length > 50)
            {
                return BadRequest(new { error = "El nombre del usuario no puede exceder los 50 caracteres" });
            }

            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                string query = @"UPDATE Usuarios
                               SET Nombre = @Nombre
                               WHERE Id = @Id";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@Id", id);
                    cmd.Parameters.AddWithValue("@Nombre", request.Nombre ?? (object)DBNull.Value);

                    int rowsAffected = cmd.ExecuteNonQuery();

                    if (rowsAffected == 0)
                    {
                        return NotFound(new { error = $"No se encontró el usuario con ID {id} para actualizar" });
                    }

                    return Ok(new { message = "Usuario actualizado correctamente" });
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al actualizar usuario con ID: {Id}", id);
            return StatusCode(500, new { error = ex.Message });
        }
    }

    // Eliminar usuario
    [HttpDelete("{id}")]
    public IActionResult DeleteUsuario(int id)
    {
        using (SqlConnection conn = new SqlConnection(_connectionString))
        {
            try
            {
                conn.Open();
                string query = "DELETE FROM Usuarios WHERE Id = @Id";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@Id", id);
                    int rowsAffected = cmd.ExecuteNonQuery();

                    if (rowsAffected == 0)
                    {
                        return NotFound(new { error = $"No se encontró el usuario con ID {id} para eliminar" });
                    }

                    return Ok(new { message = "Usuario eliminado correctamente" });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al eliminar usuario con ID: {Id}", id);
                return StatusCode(500, new { error = ex.Message });
            }
        }
    }

    // Obtener estadísticas del usuario
    [HttpGet("{id}/estadisticas")]
    public IActionResult GetEstadisticasUsuario(int id)
    {
        var estadisticas = new UsuarioEstadisticas();

        using (SqlConnection conn = new SqlConnection(_connectionString))
        {
            try
            {
                conn.Open();

                // Contar ideas de brainstorm
                string queryIdeas = "SELECT COUNT(*) FROM Ideas_brainstorm WHERE IdUsuario = @Id";
                using (SqlCommand cmd = new SqlCommand(queryIdeas, conn))
                {
                    cmd.Parameters.AddWithValue("@Id", id);
                    estadisticas.TotalIdeas = Convert.ToInt32(cmd.ExecuteScalar());
                }

                // Contar mensajes de chat
                string queryMensajes = "SELECT COUNT(*) FROM Mensajes_chat WHERE IdUsuario = @Id";
                using (SqlCommand cmd = new SqlCommand(queryMensajes, conn))
                {
                    cmd.Parameters.AddWithValue("@Id", id);
                    estadisticas.TotalMensajes = Convert.ToInt32(cmd.ExecuteScalar());
                }

                // Contar comentarios
                string queryComentarios = "SELECT COUNT(*) FROM Commenter WHERE IdUsuario = @Id";
                using (SqlCommand cmd = new SqlCommand(queryComentarios, conn))
                {
                    cmd.Parameters.AddWithValue("@Id", id);
                    estadisticas.TotalComentarios = Convert.ToInt32(cmd.ExecuteScalar());
                }

                // Contar votos
                string queryVotos = "SELECT COUNT(*) FROM Votos_voting WHERE IdUsuario = @Id";
                using (SqlCommand cmd = new SqlCommand(queryVotos, conn))
                {
                    cmd.Parameters.AddWithValue("@Id", id);
                    estadisticas.TotalVotos = Convert.ToInt32(cmd.ExecuteScalar());
                }

                return Ok(estadisticas);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener estadísticas del usuario con ID: {Id}", id);
                return StatusCode(500, new { error = ex.Message });
            }
        }
    }

    // Obtener actividad del usuario por proyecto
    [HttpGet("{id}/actividad/{idProy}")]
    public IActionResult GetActividadUsuario(int id, int idProy)
    {
        var actividad = new UsuarioActividad();

        using (SqlConnection conn = new SqlConnection(_connectionString))
        {
            try
            {
                conn.Open();

                // Ideas en brainstorm del proyecto
                string queryIdeas = @"SELECT ib.Idea, b.Titulo as BrainstormTitulo
                                    FROM Ideas_brainstorm ib
                                    INNER JOIN Brainstorm b ON ib.IdBrainstorm = b.Id
                                    WHERE ib.IdUsuario = @IdUsuario AND b.IdProy = @IdProy";

                using (SqlCommand cmd = new SqlCommand(queryIdeas, conn))
                {
                    cmd.Parameters.AddWithValue("@IdUsuario", id);
                    cmd.Parameters.AddWithValue("@IdProy", idProy);

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            actividad.Ideas.Add(new IdeaActividad
                            {
                                Idea = reader["Idea"].ToString(),
                                BrainstormTitulo = reader["BrainstormTitulo"].ToString()
                            });
                        }
                    }
                }

                // Comentarios en el proyecto
                string queryComentarios = @"SELECT c.Comentarios, c.Fase
                                          FROM Commenter c
                                          WHERE c.IdUsuario = @IdUsuario AND c.IdProy = @IdProy";

                using (SqlCommand cmd = new SqlCommand(queryComentarios, conn))
                {
                    cmd.Parameters.AddWithValue("@IdUsuario", id);
                    cmd.Parameters.AddWithValue("@IdProy", idProy);

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            actividad.Comentarios.Add(new ComentarioActividad
                            {
                                Comentario = reader["Comentarios"].ToString(),
                                Fase = (int)reader["Fase"]
                            });
                        }
                    }
                }

                return Ok(actividad);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener actividad del usuario {Id} en proyecto {IdProy}", id, idProy);
                return StatusCode(500, new { error = ex.Message });
            }
        }
    }
}

public class UsuarioItem
{
    public int Id { get; set; }
    public string Nombre { get; set; }
}

public class UsuarioRequest
{
    public string Nombre { get; set; }
}

public class UsuariosResponse
{
    public List<UsuarioItem> usuarios { get; set; } = new List<UsuarioItem>();
}

public class UsuarioEstadisticas
{
    public int TotalIdeas { get; set; }
    public int TotalMensajes { get; set; }
    public int TotalComentarios { get; set; }
    public int TotalVotos { get; set; }
}

public class UsuarioActividad
{
    public List<IdeaActividad> Ideas { get; set; } = new List<IdeaActividad>();
    public List<ComentarioActividad> Comentarios { get; set; } = new List<ComentarioActividad>();
}

public class IdeaActividad
{
    public string Idea { get; set; }
    public string BrainstormTitulo { get; set; }
}

public class ComentarioActividad
{
    public string Comentario { get; set; }
    public int Fase { get; set; }
}
