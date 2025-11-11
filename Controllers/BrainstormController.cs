using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;

[ApiController]
[Route("api/[controller]")]
public class BrainstormController : ControllerBase
{
    private readonly string _connectionString;
    private readonly ILogger<BrainstormController> _logger;

    public BrainstormController(ILogger<BrainstormController> logger)
    {
        _connectionString = "Server=.;Database=HerramientasV3;Integrated Security=True;TrustServerCertificate=True";
        _logger = logger;
    }

    // Obtener todas las sesiones de brainstorm
    [HttpGet]
    public IActionResult GetAllBrainstorms()
    {
        var response = new BrainstormSessionsResponse();

        using (SqlConnection conn = new SqlConnection(_connectionString))
        {
            try
            {
                conn.Open();
                string query = "SELECT Id, IdProy, IdTopico, Titulo, Descripcion, MiniIdeas, MaxIdeas FROM Brainstorming";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            response.sessions.Add(new BrainstormSession
                            {
                                Id = (int)reader["Id"],
                                IdProy = (int)reader["IdProy"],
                                IdTopico = reader["IdTopico"] == DBNull.Value ? (int?)null : (int)reader["IdTopico"],
                                Titulo = reader["Titulo"].ToString(),
                                Descripcion = reader["Descripcion"].ToString(),
                                MiniIdeas = (int)reader["MiniIdeas"],
                                MaxIdeas = (int)reader["MaxIdeas"]
                            });
                        }
                    }
                }

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener sesiones de brainstorm");
                return StatusCode(500, new { error = ex.Message });
            }
        }
    }

    // Obtener sesión de brainstorm por ID
    [HttpGet("{id}")]
    public IActionResult GetBrainstormById(int id)
    {
        using (SqlConnection conn = new SqlConnection(_connectionString))
        {
            try
            {
                conn.Open();
                string query = "SELECT Id, IdProy, IdTopico, Titulo, Descripcion, MiniIdeas, MaxIdeas FROM Brainstorming WHERE Id = @Id";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@Id", id);

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            var session = new BrainstormSession
                            {
                                Id = (int)reader["Id"],
                                IdProy = (int)reader["IdProy"],
                                IdTopico = reader["IdTopico"] == DBNull.Value ? (int?)null : (int)reader["IdTopico"],
                                Titulo = reader["Titulo"].ToString(),
                                Descripcion = reader["Descripcion"].ToString(),
                                MiniIdeas = (int)reader["MiniIdeas"],
                                MaxIdeas = (int)reader["MaxIdeas"]
                            };
                            return Ok(session);
                        }
                        else
                        {
                            return NotFound(new { error = $"No se encontró la sesión de brainstorm con ID {id}" });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener sesión de brainstorm por ID: {Id}", id);
                return StatusCode(500, new { error = ex.Message });
            }
        }
    }

    // Obtener sesiones de brainstorm por proyecto
    [HttpGet("byproy/{idProy}")]
    public IActionResult GetBrainstormsByProy(int idProy)
    {
        var response = new BrainstormSessionsResponse();

        using (SqlConnection conn = new SqlConnection(_connectionString))
        {
            try
            {
                conn.Open();
                string query = "SELECT Id, IdProy, IdTopico, Titulo, Descripcion, MiniIdeas, MaxIdeas FROM Brainstorming WHERE IdProy = @IdProy";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@IdProy", idProy);

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            response.sessions.Add(new BrainstormSession
                            {
                                Id = (int)reader["Id"],
                                IdProy = (int)reader["IdProy"],
                                IdTopico = reader["IdTopico"] == DBNull.Value ? (int?)null : (int)reader["IdTopico"],
                                Titulo = reader["Titulo"].ToString(),
                                Descripcion = reader["Descripcion"].ToString(),
                                MiniIdeas = (int)reader["MiniIdeas"],
                                MaxIdeas = (int)reader["MaxIdeas"]
                            });
                        }
                    }
                }

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener sesiones de brainstorm por proyecto: {IdProy}", idProy);
                return StatusCode(500, new { error = ex.Message });
            }
        }
    }

    // Crear nueva sesión de brainstorm
    [HttpPost]
    public IActionResult CreateBrainstorm([FromBody] BrainstormSessionRequest request)
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
                string query = @"INSERT INTO Brainstorming (IdProy, IdTopico, Titulo, Descripcion, MiniIdeas, MaxIdeas) 
                               VALUES (@IdProy, @IdTopico, @Titulo, @Descripcion, @MiniIdeas, @MaxIdeas);
                               SELECT SCOPE_IDENTITY();";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@IdProy", request.IdProy);
                    cmd.Parameters.AddWithValue("@IdTopico", request.IdTopico ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@Titulo", request.Titulo);
                    cmd.Parameters.AddWithValue("@Descripcion", request.Descripcion);
                    cmd.Parameters.AddWithValue("@MiniIdeas", request.MiniIdeas);
                    cmd.Parameters.AddWithValue("@MaxIdeas", request.MaxIdeas);

                    int newId = Convert.ToInt32(cmd.ExecuteScalar());
                    return Ok(new { message = "Sesión de brainstorm creada correctamente", id = newId });
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al crear sesión de brainstorm");
            return StatusCode(500, new { error = ex.Message });
        }
    }

    // Actualizar sesión de brainstorm
    [HttpPut("{id}")]
    public IActionResult UpdateBrainstorm(int id, [FromBody] BrainstormSessionRequest request)
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
                string query = @"UPDATE Brainstorming
                               SET IdProy = @IdProy, IdTopico = @IdTopico, Titulo = @Titulo, Descripcion = @Descripcion, 
                                   MiniIdeas = @MiniIdeas, MaxIdeas = @MaxIdeas
                               WHERE Id = @Id";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@Id", id);
                    cmd.Parameters.AddWithValue("@IdProy", request.IdProy);
                    cmd.Parameters.AddWithValue("@IdTopico", request.IdTopico ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@Titulo", request.Titulo ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@Descripcion", request.Descripcion ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@MiniIdeas", request.MiniIdeas);
                    cmd.Parameters.AddWithValue("@MaxIdeas", request.MaxIdeas);

                    int rowsAffected = cmd.ExecuteNonQuery();

                    if (rowsAffected == 0)
                    {
                        return NotFound(new { error = $"No se encontró la sesión de brainstorm con ID {id}" });
                    }

                    return Ok(new { message = "Sesión de brainstorm actualizada correctamente" });
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al actualizar sesión de brainstorm con ID: {Id}", id);
            return StatusCode(500, new { error = ex.Message });
        }
    }

    // Eliminar sesión de brainstorm
    [HttpDelete("{id}")]
    public IActionResult DeleteBrainstorm(int id)
    {
        using (SqlConnection conn = new SqlConnection(_connectionString))
        {
            try
            {
                conn.Open();
                string query = "DELETE FROM Brainstorming WHERE Id = @Id";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@Id", id);
                    int rowsAffected = cmd.ExecuteNonQuery();

                    if (rowsAffected == 0)
                    {
                        return NotFound(new { error = $"No se encontró la sesión de brainstorm con ID {id}" });
                    }

                    return Ok(new { message = "Sesión de brainstorm eliminada correctamente" });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al eliminar sesión de brainstorm con ID: {Id}", id);
                return StatusCode(500, new { error = ex.Message });
            }
        }
    }

    // Obtener todas las ideas de un proyecto (nuevo endpoint para el frontend)
    [HttpGet("byproy/{idProy}/ideas")]
    public IActionResult GetIdeasByProy(int idProy)
    {
        var response = new BrainstormIdeasResponse();

        using (SqlConnection conn = new SqlConnection(_connectionString))
        {
            try
            {
                conn.Open();
                string query = @"SELECT ib.Id, ib.IdBrainstorm, ib.IdUsuario, ib.Idea, u.Nombre
                               FROM Ideas_Brainstorm ib
                               INNER JOIN Brainstorming b ON ib.IdBrainstorm = b.Id
                               INNER JOIN Usuarios u ON ib.IdUsuario = u.Id
                               WHERE b.IdProy = @IdProy";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@IdProy", idProy);

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            response.ideas.Add(new BrainstormIdea
                            {
                                Id = (int)reader["Id"],
                                IdBrainstorm = (int)reader["IdBrainstorm"],
                                IdUsuario = (int)reader["IdUsuario"],
                                Idea = reader["Idea"].ToString(),
                                NombreUsuario = reader["Nombre"].ToString()
                            });
                        }
                    }
                }

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener ideas por proyecto: {IdProy}", idProy);
                return StatusCode(500, new { error = ex.Message });
            }
        }
    }

    // Obtener ideas de un usuario específico en un proyecto
    [HttpGet("byproy/{idProy}/usuario/{idUsuario}/ideas")]
    public IActionResult GetIdeasByUsuario(int idProy, int idUsuario)
    {
        var response = new BrainstormIdeasResponse();

        using (SqlConnection conn = new SqlConnection(_connectionString))
        {
            try
            {
                conn.Open();
                string query = @"SELECT ib.Id, ib.IdBrainstorm, ib.IdUsuario, ib.Idea, u.Nombre
                               FROM Ideas_Brainstorm ib
                               INNER JOIN Usuarios u ON ib.IdUsuario = u.Id
                               INNER JOIN Brainstorming b ON ib.IdBrainstorm = b.Id
                               WHERE b.IdProy = @IdProy AND ib.IdUsuario = @IdUsuario";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@IdProy", idProy);
                    cmd.Parameters.AddWithValue("@IdUsuario", idUsuario);

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            response.ideas.Add(new BrainstormIdea
                            {
                                Id = (int)reader["Id"],
                                IdBrainstorm = (int)reader["IdBrainstorm"],
                                IdUsuario = (int)reader["IdUsuario"],
                                Idea = reader["Idea"].ToString(),
                                NombreUsuario = reader["Nombre"].ToString()
                            });
                        }
                    }
                }

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener ideas del usuario {IdUsuario} en proyecto {IdProy}", idUsuario, idProy);
                return StatusCode(500, new { error = ex.Message });
            }
        }
    }

    // Obtener ideas de una sesión de brainstorm
    [HttpGet("{idBrainstorm}/ideas")]
    public IActionResult GetIdeas(int idBrainstorm)
    {
        var response = new BrainstormIdeasResponse();

        using (SqlConnection conn = new SqlConnection(_connectionString))
        {
            try
            {
                conn.Open();
                string query = @"SELECT ib.Id, ib.IdBrainstorm, ib.IdUsuario, ib.Idea, u.Nombre
                               FROM Ideas_Brainstorm ib
                               INNER JOIN Usuarios u ON ib.IdUsuario = u.Id
                               WHERE ib.IdBrainstorm = @IdBrainstorm";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@IdBrainstorm", idBrainstorm);

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            response.ideas.Add(new BrainstormIdea
                            {
                                Id = (int)reader["Id"],
                                IdBrainstorm = (int)reader["IdBrainstorm"],
                                IdUsuario = (int)reader["IdUsuario"],
                                Idea = reader["Idea"].ToString(),
                                NombreUsuario = reader["Nombre"].ToString()
                            });
                        }
                    }
                }

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener ideas de brainstorm");
                return StatusCode(500, new { error = ex.Message });
            }
        }
    }

    // Agregar idea a una sesión de brainstorm
    [HttpPost("{idBrainstorm}/ideas")]
    public IActionResult AddIdea(int idBrainstorm, [FromBody] BrainstormIdeaRequest request)
    {
        try
        {
            if (request == null)
            {
                return BadRequest(new { error = "La solicitud no puede estar vacía" });
            }

            if (string.IsNullOrEmpty(request.Idea))
            {
                return BadRequest(new { error = "La idea es requerida" });
            }

            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                string query = @"INSERT INTO Ideas_Brainstorm (IdBrainstorm, IdUsuario, Idea) 
                               VALUES (@IdBrainstorm, @IdUsuario, @Idea);
                               SELECT SCOPE_IDENTITY();";

                int newId;
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@IdBrainstorm", idBrainstorm);
                    cmd.Parameters.AddWithValue("@IdUsuario", request.IdUsuario);
                    cmd.Parameters.AddWithValue("@Idea", request.Idea);

                    var result = cmd.ExecuteScalar();
                    if (result == null || result == DBNull.Value)
                    {
                        return StatusCode(500, new { error = "No se pudo insertar la idea" });
                    }
                    newId = Convert.ToInt32(result);
                }

                // Obtener la idea completa con los datos del usuario
                string selectQuery = @"SELECT ib.Id, ib.IdBrainstorm, ib.IdUsuario, ib.Idea, ISNULL(u.Nombre, 'Usuario') as Nombre
                                     FROM Ideas_Brainstorm ib
                                     LEFT JOIN Usuarios u ON ib.IdUsuario = u.Id
                                     WHERE ib.Id = @Id";

                using (SqlCommand selectCmd = new SqlCommand(selectQuery, conn))
                {
                    selectCmd.Parameters.AddWithValue("@Id", newId);
                    
                    using (SqlDataReader reader = selectCmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            var idea = new BrainstormIdea
                            {
                                Id = (int)reader["Id"],
                                IdBrainstorm = (int)reader["IdBrainstorm"],
                                IdUsuario = (int)reader["IdUsuario"],
                                Idea = reader["Idea"].ToString(),
                                NombreUsuario = reader["Nombre"].ToString()
                            };
                            
                            return Ok(new { message = "Idea agregada correctamente", idea = idea });
                        }
                        else
                        {
                            return StatusCode(500, new { error = "No se pudo recuperar la idea después de crearla" });
                        }
                    }
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al agregar idea");
            return StatusCode(500, new { error = ex.Message });
        }
    }

    // Editar idea de una sesión de brainstorm
    [HttpPut("{idBrainstorm}/ideas")]
    public IActionResult UpdateIdea(int idBrainstorm, [FromBody] BrainstormIdeaUpdateRequest request)
    {
        try
        {
            if (request == null)
            {
                return BadRequest(new { error = "La solicitud no puede estar vacía" });
            }

            if (string.IsNullOrEmpty(request.Idea))
            {
                return BadRequest(new { error = "La idea es requerida" });
            }

            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                string query = @"UPDATE Ideas_Brainstorm 
                               SET Idea = @Idea 
                               WHERE IdBrainstorm = @IdBrainstorm AND IdUsuario = @IdUsuario AND CAST(Idea AS NVARCHAR(MAX)) = @IdeaOriginal";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@IdBrainstorm", idBrainstorm);
                    cmd.Parameters.AddWithValue("@IdUsuario", request.IdUsuario);
                    cmd.Parameters.AddWithValue("@Idea", request.Idea);
                    cmd.Parameters.AddWithValue("@IdeaOriginal", request.IdeaOriginal);

                    int rowsAffected = cmd.ExecuteNonQuery();
                    
                    if (rowsAffected == 0)
                    {
                        return NotFound(new { error = "No se encontró la idea para editar" });
                    }
                }

                // Obtener la idea actualizada
                string selectQuery = @"SELECT ib.IdBrainstorm, ib.IdUsuario, ib.Idea, ISNULL(u.Nombre, 'Usuario') as Nombre
                                     FROM Ideas_Brainstorm ib
                                     LEFT JOIN Usuarios u ON ib.IdUsuario = u.Id
                                     WHERE ib.IdBrainstorm = @IdBrainstorm AND ib.IdUsuario = @IdUsuario AND CAST(ib.Idea AS NVARCHAR(MAX)) = @Idea";

                using (SqlCommand selectCmd = new SqlCommand(selectQuery, conn))
                {
                    selectCmd.Parameters.AddWithValue("@IdBrainstorm", idBrainstorm);
                    selectCmd.Parameters.AddWithValue("@IdUsuario", request.IdUsuario);
                    selectCmd.Parameters.AddWithValue("@Idea", request.Idea);
                    
                    using (SqlDataReader reader = selectCmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            var idea = new BrainstormIdea
                            {
                                IdBrainstorm = reader["IdBrainstorm"] != DBNull.Value ? (int)reader["IdBrainstorm"] : idBrainstorm,
                                IdUsuario = reader["IdUsuario"] != DBNull.Value ? (int)reader["IdUsuario"] : request.IdUsuario,
                                Idea = reader["Idea"] != DBNull.Value ? reader["Idea"].ToString() : request.Idea,
                                NombreUsuario = reader["Nombre"] != DBNull.Value ? reader["Nombre"].ToString() : "Usuario"
                            };
                            
                            return Ok(new { message = "Idea actualizada correctamente", idea = idea });
                        }
                        else
                        {
                            return StatusCode(500, new { error = "No se pudo recuperar la idea después de actualizarla" });
                        }
                    }
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al actualizar idea");
            return StatusCode(500, new { error = ex.Message });
        }
    }

    // Eliminar idea de una sesión de brainstorm
    [HttpDelete("{idBrainstorm}/ideas")]
    public IActionResult DeleteIdea(int idBrainstorm, [FromBody] BrainstormIdeaDeleteRequest request)
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
                string query = @"DELETE FROM Ideas_Brainstorm 
                               WHERE IdBrainstorm = @IdBrainstorm AND IdUsuario = @IdUsuario AND CAST(Idea AS NVARCHAR(MAX)) = @Idea";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@IdBrainstorm", idBrainstorm);
                    cmd.Parameters.AddWithValue("@IdUsuario", request.IdUsuario);
                    cmd.Parameters.AddWithValue("@Idea", request.Idea);

                    int rowsAffected = cmd.ExecuteNonQuery();
                    
                    if (rowsAffected == 0)
                    {
                        return NotFound(new { error = "No se encontró la idea para eliminar" });
                    }

                    return Ok(new { message = "Idea eliminada correctamente" });
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al eliminar idea");
            return StatusCode(500, new { error = ex.Message });
        }
    }

    // Editar idea individual por ID
    [HttpPut("ideas/{id}")]
    public IActionResult EditIdeaById(int id, [FromBody] BrainstormIdeaEditRequest request)
    {
        try
        {
            if (request == null)
            {
                return BadRequest(new { error = "La solicitud no puede estar vacía" });
            }

            if (string.IsNullOrEmpty(request.Idea))
            {
                return BadRequest(new { error = "La idea es requerida" });
            }

            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                
                // Verificar que la idea existe y pertenece al usuario
                string checkQuery = @"SELECT ib.Id, ib.IdBrainstorm, ib.IdUsuario, ib.Idea, u.Nombre
                                    FROM Ideas_Brainstorm ib
                                    LEFT JOIN Usuarios u ON ib.IdUsuario = u.Id
                                    WHERE ib.Id = @Id";

                using (SqlCommand checkCmd = new SqlCommand(checkQuery, conn))
                {
                    checkCmd.Parameters.AddWithValue("@Id", id);
                    
                    using (SqlDataReader reader = checkCmd.ExecuteReader())
                    {
                        if (!reader.Read())
                        {
                            return NotFound(new { error = "No se encontró la idea con el ID especificado" });
                        }
                        
                        // Verificar que el usuario es el propietario de la idea
                        int ideaUserId = (int)reader["IdUsuario"];
                        if (ideaUserId != request.IdUsuario)
                        {
                            return Forbid("No tienes permisos para editar esta idea");
                        }
                    }
                }

                // Actualizar la idea
                string updateQuery = @"UPDATE Ideas_Brainstorm 
                                     SET Idea = @Idea 
                                     WHERE Id = @Id";

                using (SqlCommand cmd = new SqlCommand(updateQuery, conn))
                {
                    cmd.Parameters.AddWithValue("@Id", id);
                    cmd.Parameters.AddWithValue("@Idea", request.Idea);

                    int rowsAffected = cmd.ExecuteNonQuery();
                    
                    if (rowsAffected == 0)
                    {
                        return StatusCode(500, new { error = "No se pudo actualizar la idea" });
                    }
                }

                // Obtener la idea actualizada
                string selectQuery = @"SELECT ib.Id, ib.IdBrainstorm, ib.IdUsuario, ib.Idea, ISNULL(u.Nombre, 'Usuario') as Nombre
                                     FROM Ideas_Brainstorm ib
                                     LEFT JOIN Usuarios u ON ib.IdUsuario = u.Id
                                     WHERE ib.Id = @Id";

                using (SqlCommand selectCmd = new SqlCommand(selectQuery, conn))
                {
                    selectCmd.Parameters.AddWithValue("@Id", id);
                    
                    using (SqlDataReader reader = selectCmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            var idea = new BrainstormIdea
                            {
                                Id = (int)reader["Id"],
                                IdBrainstorm = (int)reader["IdBrainstorm"],
                                IdUsuario = (int)reader["IdUsuario"],
                                Idea = reader["Idea"].ToString(),
                                NombreUsuario = reader["Nombre"].ToString()
                            };
                            
                            return Ok(new { message = "Idea actualizada correctamente", idea = idea });
                        }
                        else
                        {
                            return StatusCode(500, new { error = "No se pudo recuperar la idea después de actualizarla" });
                        }
                    }
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al actualizar idea por ID: {Id}", id);
            return StatusCode(500, new { error = ex.Message });
        }
    }

    // Eliminar idea individual por ID
    [HttpDelete("ideas/{id}")]
    public IActionResult DeleteIdeaById(int id, [FromBody] BrainstormIdeaDeleteByIdRequest request)
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
                
                // Verificar que la idea existe y pertenece al usuario
                string checkQuery = @"SELECT Id, IdUsuario FROM Ideas_Brainstorm WHERE Id = @Id";

                using (SqlCommand checkCmd = new SqlCommand(checkQuery, conn))
                {
                    checkCmd.Parameters.AddWithValue("@Id", id);
                    
                    using (SqlDataReader reader = checkCmd.ExecuteReader())
                    {
                        if (!reader.Read())
                        {
                            return NotFound(new { error = "No se encontró la idea con el ID especificado" });
                        }
                        
                        // Verificar que el usuario es el propietario de la idea
                        int ideaUserId = (int)reader["IdUsuario"];
                        if (ideaUserId != request.IdUsuario)
                        {
                            return Forbid("No tienes permisos para eliminar esta idea");
                        }
                    }
                }

                // Eliminar la idea
                string deleteQuery = @"DELETE FROM Ideas_Brainstorm WHERE Id = @Id";

                using (SqlCommand cmd = new SqlCommand(deleteQuery, conn))
                {
                    cmd.Parameters.AddWithValue("@Id", id);

                    int rowsAffected = cmd.ExecuteNonQuery();
                    
                    if (rowsAffected == 0)
                    {
                        return StatusCode(500, new { error = "No se pudo eliminar la idea" });
                    }

                    return Ok(new { message = "Idea eliminada correctamente" });
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al eliminar idea por ID: {Id}", id);
            return StatusCode(500, new { error = ex.Message });
        }
    }
}

// Clases para sesiones de brainstorm
public class BrainstormSession
{
    public int Id { get; set; }
    public int IdProy { get; set; }
    public int? IdTopico { get; set; }
    public string Titulo { get; set; }
    public string Descripcion { get; set; }
    public int MiniIdeas { get; set; }
    public int MaxIdeas { get; set; }
}

public class BrainstormSessionRequest
{
    public int IdProy { get; set; }
    public int? IdTopico { get; set; }
    public string Titulo { get; set; }
    public string Descripcion { get; set; }
    public int MiniIdeas { get; set; }
    public int MaxIdeas { get; set; }
}

public class BrainstormSessionsResponse
{
    public List<BrainstormSession> sessions { get; set; } = new List<BrainstormSession>();
}

// Clases para ideas de brainstorm
public class BrainstormIdea
{
    public int Id { get; set; }
    public int IdBrainstorm { get; set; }
    public int IdUsuario { get; set; }
    public string Idea { get; set; }
    public string NombreUsuario { get; set; }
}

public class BrainstormIdeaRequest
{
    public int IdUsuario { get; set; }
    public string Idea { get; set; }
}

public class BrainstormIdeaUpdateRequest
{
    public int IdUsuario { get; set; }
    public string Idea { get; set; }
    public string IdeaOriginal { get; set; }
}

public class BrainstormIdeaDeleteRequest
{
    public int IdUsuario { get; set; }
    public string Idea { get; set; }
}

public class BrainstormIdeasResponse
{
    public List<BrainstormIdea> ideas { get; set; } = new List<BrainstormIdea>();
}

public class BrainstormIdeaEditRequest
{
    public int IdUsuario { get; set; }
    public string Idea { get; set; }
}

public class BrainstormIdeaDeleteByIdRequest
{
    public int IdUsuario { get; set; }
} 
