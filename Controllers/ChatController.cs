using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;

[ApiController]
[Route("api/[controller]")]
public class ChatController : ControllerBase
{
    private readonly IHubContext<ChatHub> _hubContext;
    private readonly string _connectionString;
    private readonly ILogger<ChatController> _logger;

    public ChatController(IHubContext<ChatHub> hubContext, IConfiguration configuration, ILogger<ChatController> logger)
    {
        _connectionString = "Server=.;Database=HerramientasV3;Integrated Security=True;TrustServerCertificate=True";
        _hubContext = hubContext;
        _logger = logger;
    }

    // Obtener todas las sesiones de chat
    [HttpGet]
    public IActionResult GetAllChats()
    {
        var response = new ChatSessionsResponse();

        using (SqlConnection conn = new SqlConnection(_connectionString))
        {
            try
            {
                conn.Open();
                string query = "SELECT Id, IdProy, IdTopico, Titulo, Descripcion FROM Chat";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            response.sessions.Add(new ChatSession
                            {
                                Id = (int)reader["Id"],
                                IdProy = (int)reader["IdProy"],
                                IdTopico = reader["IdTopico"] == DBNull.Value ? (int?)null : (int)reader["IdTopico"],
                                Titulo = reader["Titulo"].ToString(),
                                Descripcion = reader["Descripcion"].ToString()
                            });
                        }
                    }
                }

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener sesiones de chat");
                return StatusCode(500, new { error = ex.Message });
            }
        }
    }

    // Obtener sesión de chat por ID
    [HttpGet("{id}")]
    public IActionResult GetChatById(int id)
    {
        using (SqlConnection conn = new SqlConnection(_connectionString))
        {
            try
            {
                conn.Open();
                string query = "SELECT Id, IdProy, IdTopico, Titulo, Descripcion FROM Chat WHERE Id = @Id";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@Id", id);

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            var session = new ChatSession
                            {
                                Id = (int)reader["Id"],
                                IdProy = (int)reader["IdProy"],
                                IdTopico = reader["IdTopico"] == DBNull.Value ? (int?)null : (int)reader["IdTopico"],
                                Titulo = reader["Titulo"].ToString(),
                                Descripcion = reader["Descripcion"].ToString()
                            };
                            return Ok(session);
                        }
                        else
                        {
                            return NotFound(new { error = $"No se encontró la sesión de chat con ID {id}" });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener sesión de chat por ID: {Id}", id);
                return StatusCode(500, new { error = ex.Message });
            }
        }
    }

    // Obtener sesiones de chat por proyecto
    [HttpGet("byproy/{idProy}")]
    public IActionResult GetChatsByProy(int idProy)
    {
        var response = new ChatSessionsResponse();

        using (SqlConnection conn = new SqlConnection(_connectionString))
        {
            try
            {
                conn.Open();
                string query = "SELECT Id, IdProy, IdTopico, Titulo, Descripcion FROM Chat WHERE IdProy = @IdProy";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@IdProy", idProy);

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            response.sessions.Add(new ChatSession
                            {
                                Id = (int)reader["Id"],
                                IdProy = (int)reader["IdProy"],
                                IdTopico = reader["IdTopico"] == DBNull.Value ? (int?)null : (int)reader["IdTopico"],
                                Titulo = reader["Titulo"].ToString(),
                                Descripcion = reader["Descripcion"].ToString()
                            });
                        }
                    }
                }

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener sesiones de chat por proyecto: {IdProy}", idProy);
                return StatusCode(500, new { error = ex.Message });
            }
        }
    }

    // Crear nueva sesión de chat
    [HttpPost]
    public IActionResult CreateChat([FromBody] ChatSessionRequest request)
    {
        try
        {
            if (request == null)
            {
                return BadRequest(new { error = "La solicitud no puede estar vacía" });
            }

            if (string.IsNullOrEmpty(request.Titulo))
            {
                return BadRequest(new { error = "El título es requerido" });
            }

            if (string.IsNullOrEmpty(request.Descripcion))
            {
                return BadRequest(new { error = "La descripción es requerida" });
            }

            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                string query = @"INSERT INTO Chat (IdProy, IdTopico, Titulo, Descripcion) 
                               VALUES (@IdProy, @IdTopico, @Titulo, @Descripcion);
                               SELECT SCOPE_IDENTITY();";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@IdProy", request.IdProy);
                    cmd.Parameters.AddWithValue("@IdTopico", request.IdTopico ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@Titulo", request.Titulo);
                    cmd.Parameters.AddWithValue("@Descripcion", request.Descripcion);

                    int newId = Convert.ToInt32(cmd.ExecuteScalar());
                    return Ok(new { message = "Sesión de chat creada correctamente", id = newId });
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al crear sesión de chat");
            return StatusCode(500, new { error = ex.Message });
        }
    }

    // Actualizar sesión de chat
    [HttpPut("{id}")]
    public IActionResult UpdateChat(int id, [FromBody] ChatSessionRequest request)
    {
        try
        {
            if (request == null)
            {
                return BadRequest(new { error = "La solicitud no puede estar vacía" });
            }

            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                string query = @"UPDATE Chat
                               SET IdProy = @IdProy, IdTopico = @IdTopico, Titulo = @Titulo, Descripcion = @Descripcion
                               WHERE Id = @Id";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@Id", id);
                    cmd.Parameters.AddWithValue("@IdProy", request.IdProy);
                    cmd.Parameters.AddWithValue("@IdTopico", request.IdTopico ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@Titulo", request.Titulo ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@Descripcion", request.Descripcion ?? (object)DBNull.Value);

                    int rowsAffected = cmd.ExecuteNonQuery();

                    if (rowsAffected == 0)
                    {
                        return NotFound(new { error = $"No se encontró la sesión de chat con ID {id}" });
                    }

                    return Ok(new { message = "Sesión de chat actualizada correctamente" });
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al actualizar sesión de chat con ID: {Id}", id);
            return StatusCode(500, new { error = ex.Message });
        }
    }

    // Eliminar sesión de chat
    [HttpDelete("{id}")]
    public IActionResult DeleteChat(int id)
    {
        using (SqlConnection conn = new SqlConnection(_connectionString))
        {
            try
            {
                conn.Open();
                string query = "DELETE FROM Chat WHERE Id = @Id";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@Id", id);
                    int rowsAffected = cmd.ExecuteNonQuery();

                    if (rowsAffected == 0)
                    {
                        return NotFound(new { error = $"No se encontró la sesión de chat con ID {id}" });
                    }

                    return Ok(new { message = "Sesión de chat eliminada correctamente" });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al eliminar sesión de chat con ID: {Id}", id);
                return StatusCode(500, new { error = ex.Message });
            }
        }
    }

    // Obtener mensajes de una sesión de chat
    [HttpGet("{idChat}/mensajes")]
    public IActionResult GetMensajes(int idChat)
    {
        var response = new ChatMensajesResponse();

        using (SqlConnection conn = new SqlConnection(_connectionString))
        {
            try
            {
                conn.Open();
                string query = @"SELECT mc.IdChat, mc.IdUsuario, mc.Fecha, mc.Hora, mc.Mensaje, u.Nombre
                               FROM Mensajes_chat mc
                               INNER JOIN Usuarios u ON mc.IdUsuario = u.Id
                               WHERE mc.IdChat = @IdChat
                               ORDER BY mc.Fecha, mc.Hora";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@IdChat", idChat);

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            response.mensajes.Add(new ChatMensaje
                            {
                                IdChat = (int)reader["IdChat"],
                                IdUsuario = (int)reader["IdUsuario"],
                                Fecha = (DateTime)reader["Fecha"],
                                Hora = (TimeSpan)reader["Hora"],
                                Mensaje = reader["Mensaje"].ToString(),
                                NombreUsuario = reader["Nombre"].ToString()
                            });
                        }
                    }
                }

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener mensajes de chat");
                return StatusCode(500, new { error = ex.Message });
            }
        }
    }

    // Enviar mensaje a una sesión de chat
    [HttpPost("{idChat}/mensajes")]
    public async Task<IActionResult> SendMensaje(int idChat, [FromBody] ChatMensajeRequest request)
    {
        try
        {
            if (request == null)
            {
                return BadRequest(new { error = "La solicitud no puede estar vacía" });
            }

            if (string.IsNullOrEmpty(request.Mensaje))
            {
                return BadRequest(new { error = "El mensaje es requerido" });
            }

            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                string query = @"INSERT INTO Mensajes_chat (IdChat, IdUsuario, Fecha, Hora, Mensaje) 
                               VALUES (@IdChat, @IdUsuario, GETDATE(), CAST(GETDATE() AS TIME), @Mensaje);
                               SELECT SCOPE_IDENTITY();";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@IdChat", idChat);
                    cmd.Parameters.AddWithValue("@IdUsuario", request.IdUsuario);
                    cmd.Parameters.AddWithValue("@Mensaje", request.Mensaje);

                    int newId = Convert.ToInt32(cmd.ExecuteScalar());

                    // Obtener nombre del usuario para enviar por SignalR
                    string nombreUsuario = "";
                    string queryUsuario = "SELECT Nombre FROM Usuarios WHERE Id = @IdUsuario";
                    using (SqlCommand cmdUsuario = new SqlCommand(queryUsuario, conn))
                    {
                        cmdUsuario.Parameters.AddWithValue("@IdUsuario", request.IdUsuario);
                        var result = cmdUsuario.ExecuteScalar();
                        if (result != null)
                        {
                            nombreUsuario = result.ToString();
                        }
                    }

                    // Enviar mensaje por SignalR
                    await _hubContext.Clients.All.SendAsync("ReceiveMessage", nombreUsuario, request.Mensaje);

                    return Ok(new { message = "Mensaje enviado correctamente", id = newId });
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al enviar mensaje");
            return StatusCode(500, new { error = ex.Message });
        }
    }

    // Broadcast mensaje (para compatibilidad con el código existente)
    [HttpPost("broadcast")]
    public async Task<IActionResult> BroadcastMessage([FromBody] MessageDto message)
    {
        await _hubContext.Clients.All.SendAsync("ReceiveMessage", message.User, message.Text);
        return Ok();
    }
}

// Clases para sesiones de chat
public class ChatSession
{
    public int Id { get; set; }
    public int IdProy { get; set; }
    public int? IdTopico { get; set; }
    public string Titulo { get; set; }
    public string Descripcion { get; set; }
}

public class ChatSessionRequest
{
    public int IdProy { get; set; }
    public int? IdTopico { get; set; }
    public string Titulo { get; set; }
    public string Descripcion { get; set; }
}

public class ChatSessionsResponse
{
    public List<ChatSession> sessions { get; set; } = new List<ChatSession>();
}

// Clases para mensajes de chat
public class ChatMensaje
{
    public int IdChat { get; set; }
    public int IdUsuario { get; set; }
    public DateTime Fecha { get; set; }
    public TimeSpan Hora { get; set; }
    public string Mensaje { get; set; }
    public string NombreUsuario { get; set; }
}

public class ChatMensajeRequest
{
    public int IdUsuario { get; set; }
    public string Mensaje { get; set; }
}

public class ChatMensajesResponse
{
    public List<ChatMensaje> mensajes { get; set; } = new List<ChatMensaje>();
}

// Clases para compatibilidad
public class MessageDto
{
    public string User { get; set; }
    public string Text { get; set; }
} 