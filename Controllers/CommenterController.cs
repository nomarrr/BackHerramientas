using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;

[ApiController]
[Route("api/[controller]")]
public class CommenterController : ControllerBase
{
    private readonly string _connectionString;
    private readonly ILogger<CommenterController> _logger;

    public CommenterController(ILogger<CommenterController> logger)
    {
        _connectionString = "Server=.;Database=HerramientasV3;Integrated Security=True;TrustServerCertificate=True";
        _logger = logger;
    }

    // Obtener todos los comentarios
    [HttpGet]
    public IActionResult GetAllComments()
    {
        var response = new CommenterResponse();

        using (SqlConnection conn = new SqlConnection(_connectionString))
        {
            try
            {
                conn.Open();
                string query = @"SELECT cc.Id, c.IdProy, c.IdTopico, c.IdCategorizer, cc.IdCategoria, cc.Fase, cc.IdUsuario, cc.Comentario, u.Nombre
                               FROM Comenter c
                               INNER JOIN Comentarios_comenter cc ON c.Id = cc.IdComenter
                               INNER JOIN Usuarios u ON cc.IdUsuario = u.Id";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            response.comments.Add(new CommenterItem
                            {
                                Id = (int)reader["Id"],
                                IdProy = (int)reader["IdProy"],
                                IdTopico = reader["IdTopico"] == DBNull.Value ? (int?)null : (int)reader["IdTopico"],
                                IdCategorizer = reader["IdCategorizer"] == DBNull.Value ? (int?)null : (int)reader["IdCategorizer"],
                                IdCategoria = (int)reader["IdCategoria"],
                                Fase = (int)reader["Fase"],
                                IdUsuario = (int)reader["IdUsuario"],
                                Comentario = reader["Comentario"].ToString(),
                                NombreUsuario = reader["Nombre"].ToString()
                            });
                        }
                    }
                }

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener todos los comentarios");
                return StatusCode(500, new { error = ex.Message });
            }
        }
    }

    // Obtener comentario por ID
    [HttpGet("{id}")]
    public IActionResult GetCommentById(int id)
    {
        using (SqlConnection conn = new SqlConnection(_connectionString))
        {
            try
            {
                conn.Open();
                string query = @"SELECT cc.Id, c.IdProy, c.IdTopico, c.IdCategorizer, cc.IdCategoria, cc.Fase, cc.IdUsuario, cc.Comentario, u.Nombre
                               FROM Comenter c
                               INNER JOIN Comentarios_comenter cc ON c.Id = cc.IdComenter
                               INNER JOIN Usuarios u ON cc.IdUsuario = u.Id
                               WHERE cc.Id = @Id";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@Id", id);

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            var comment = new CommenterItem
                            {
                                Id = (int)reader["Id"],
                                IdProy = (int)reader["IdProy"],
                                IdTopico = reader["IdTopico"] == DBNull.Value ? (int?)null : (int)reader["IdTopico"],
                                IdCategorizer = reader["IdCategorizer"] == DBNull.Value ? (int?)null : (int)reader["IdCategorizer"],
                                IdCategoria = (int)reader["IdCategoria"],
                                Fase = (int)reader["Fase"],
                                IdUsuario = (int)reader["IdUsuario"],
                                Comentario = reader["Comentario"].ToString(),
                                NombreUsuario = reader["Nombre"].ToString()
                            };
                            return Ok(comment);
                        }
                        else
                        {
                            return NotFound(new { error = $"No se encontró el comentario con ID {id}" });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener comentario por ID: {Id}", id);
                return StatusCode(500, new { error = ex.Message });
            }
        }
    }

    // Obtener comentarios por proyecto
    [HttpGet("byproy/{idProy}")]
    public IActionResult GetCommentsByProy(int idProy)
    {
        var response = new CommenterResponse();

        using (SqlConnection conn = new SqlConnection(_connectionString))
        {
            try
            {
                conn.Open();
                string query = @"SELECT cc.Id, c.IdProy, c.IdTopico, c.IdCategorizer, cc.IdCategoria, cc.Fase, cc.IdUsuario, cc.Comentario, u.Nombre
                               FROM Comenter c
                               INNER JOIN Comentarios_comenter cc ON c.Id = cc.IdComenter
                               INNER JOIN Usuarios u ON cc.IdUsuario = u.Id
                               WHERE c.IdProy = @IdProy";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@IdProy", idProy);

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            response.comments.Add(new CommenterItem
                            {
                                Id = (int)reader["Id"],
                                IdProy = (int)reader["IdProy"],
                                IdTopico = reader["IdTopico"] == DBNull.Value ? (int?)null : (int)reader["IdTopico"],
                                IdCategorizer = reader["IdCategorizer"] == DBNull.Value ? (int?)null : (int)reader["IdCategorizer"],
                                IdCategoria = (int)reader["IdCategoria"],
                                Fase = (int)reader["Fase"],
                                IdUsuario = (int)reader["IdUsuario"],
                                Comentario = reader["Comentario"].ToString(),
                                NombreUsuario = reader["Nombre"].ToString()
                            });
                        }
                    }
                }

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener comentarios por proyecto: {IdProy}", idProy);
                return StatusCode(500, new { error = ex.Message });
            }
        }
    }

    // Obtener comentarios por categoría
    [HttpGet("bycategoria/{idCategoria}")]
    public IActionResult GetCommentsByCategoria(int idCategoria)
    {
        var response = new CommenterResponse();

        using (SqlConnection conn = new SqlConnection(_connectionString))
        {
            try
            {
                conn.Open();
                string query = @"SELECT cc.Id, c.IdProy, c.IdTopico, c.IdCategorizer, cc.IdCategoria, cc.Fase, cc.IdUsuario, cc.Comentario, u.Nombre
                               FROM Comenter c
                               INNER JOIN Comentarios_comenter cc ON c.Id = cc.IdComenter
                               INNER JOIN Usuarios u ON cc.IdUsuario = u.Id
                               WHERE cc.IdCategoria = @IdCategoria";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@IdCategoria", idCategoria);

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            response.comments.Add(new CommenterItem
                            {
                                Id = (int)reader["Id"],
                                IdProy = (int)reader["IdProy"],
                                IdTopico = reader["IdTopico"] == DBNull.Value ? (int?)null : (int)reader["IdTopico"],
                                IdCategorizer = reader["IdCategorizer"] == DBNull.Value ? (int?)null : (int)reader["IdCategorizer"],
                                IdCategoria = (int)reader["IdCategoria"],
                                Fase = (int)reader["Fase"],
                                IdUsuario = (int)reader["IdUsuario"],
                                Comentario = reader["Comentario"].ToString(),
                                NombreUsuario = reader["Nombre"].ToString()
                            });
                        }
                    }
                }

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener comentarios por categoría: {IdCategoria}", idCategoria);
                return StatusCode(500, new { error = ex.Message });
            }
        }
    }

    // Crear nuevo comentario
    [HttpPost]
    public IActionResult CreateComment([FromBody] CommenterRequest request)
    {
        try
        {
            if (request == null)
            {
                return BadRequest(new { error = "La solicitud no puede estar vacía" });
            }

            if (string.IsNullOrEmpty(request.Comentario))
            {
                return BadRequest(new { error = "El comentario es requerido" });
            }

            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                using (var transaction = conn.BeginTransaction())
                {
                    try
                    {
                        // Primero crear o verificar si existe Comenter
                        string queryComenter = @"SELECT Id FROM Comenter 
                                               WHERE IdProy = @IdProy AND IdTopico = @IdTopico AND IdCategorizer = @IdCategorizer";
                        int idComenter = 0;
                        
                        using (SqlCommand cmd = new SqlCommand(queryComenter, conn, transaction))
                        {
                            cmd.Parameters.AddWithValue("@IdProy", request.IdProy);
                            cmd.Parameters.AddWithValue("@IdTopico", request.IdTopico ?? (object)DBNull.Value);
                            cmd.Parameters.AddWithValue("@IdCategorizer", request.IdCategorizer ?? (object)DBNull.Value);
                            
                            var result = cmd.ExecuteScalar();
                            if (result != null)
                            {
                                idComenter = (int)result;
                            }
                            else
                            {
                                // Crear nuevo Comenter
                                string insertComenter = @"INSERT INTO Comenter (IdProy, IdTopico, IdCategorizer) 
                                                       VALUES (@IdProy, @IdTopico, @IdCategorizer);
                                                       SELECT SCOPE_IDENTITY();";
                                using (SqlCommand cmdInsert = new SqlCommand(insertComenter, conn, transaction))
                                {
                                    cmdInsert.Parameters.AddWithValue("@IdProy", request.IdProy);
                                    cmdInsert.Parameters.AddWithValue("@IdTopico", request.IdTopico ?? (object)DBNull.Value);
                                    cmdInsert.Parameters.AddWithValue("@IdCategorizer", request.IdCategorizer ?? (object)DBNull.Value);
                                    idComenter = Convert.ToInt32(cmdInsert.ExecuteScalar());
                                }
                            }
                        }
                        
                        // Ahora crear el comentario en Comentarios_comenter
                        string query = @"INSERT INTO Comentarios_comenter (IdComenter, IdCategoria, Fase, IdUsuario, Comentario) 
                                       VALUES (@IdComenter, @IdCategoria, @Fase, @IdUsuario, @Comentario);
                                       SELECT SCOPE_IDENTITY();";

                        using (SqlCommand cmd = new SqlCommand(query, conn, transaction))
                        {
                            cmd.Parameters.AddWithValue("@IdComenter", idComenter);
                            cmd.Parameters.AddWithValue("@IdCategoria", request.IdCategoria);
                            cmd.Parameters.AddWithValue("@Fase", request.Fase);
                            cmd.Parameters.AddWithValue("@IdUsuario", request.IdUsuario);
                            cmd.Parameters.AddWithValue("@Comentario", request.Comentario);

                            int newId = Convert.ToInt32(cmd.ExecuteScalar());
                            transaction.Commit();
                            return Ok(new { message = "Comentario agregado correctamente", id = newId });
                        }
                    }
                    catch
                    {
                        transaction.Rollback();
                        throw;
                    }
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al crear comentario");
            return StatusCode(500, new { error = ex.Message });
        }
    }

    // Actualizar comentario existente
    [HttpPut("{id}")]
    public IActionResult UpdateComment(int id, [FromBody] CommenterRequest request)
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
                string query = @"UPDATE Comentarios_comenter
                               SET IdCategoria = @IdCategoria, Fase = @Fase, 
                                   IdUsuario = @IdUsuario, Comentario = @Comentario
                               WHERE Id = @Id";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@Id", id);
                    cmd.Parameters.AddWithValue("@IdCategoria", request.IdCategoria);
                    cmd.Parameters.AddWithValue("@Fase", request.Fase);
                    cmd.Parameters.AddWithValue("@IdUsuario", request.IdUsuario);
                    cmd.Parameters.AddWithValue("@Comentario", request.Comentario ?? (object)DBNull.Value);

                    int rowsAffected = cmd.ExecuteNonQuery();

                    if (rowsAffected == 0)
                    {
                        return NotFound(new { error = $"No se encontró el comentario con ID {id} para actualizar" });
                    }

                    return Ok(new { message = "Comentario actualizado correctamente" });
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al actualizar comentario con ID: {Id}", id);
            return StatusCode(500, new { error = ex.Message });
        }
    }

    // Verificar si existe comentario de usuario en categoría
    [HttpGet("check/{idUsuario}/{idCategoria}")]
    public IActionResult CheckUserCommentInCategory(int idUsuario, int idCategoria)
    {
        using (SqlConnection conn = new SqlConnection(_connectionString))
        {
            try
            {
                conn.Open();
                string query = @"SELECT COUNT(*) as CommentCount
                               FROM Comentarios_comenter 
                               WHERE IdUsuario = @IdUsuario AND IdCategoria = @IdCategoria";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@IdUsuario", idUsuario);
                    cmd.Parameters.AddWithValue("@IdCategoria", idCategoria);

                    int commentCount = (int)cmd.ExecuteScalar();
                    bool hasComment = commentCount > 0;

                    return Ok(new { 
                        hasComment = hasComment,
                        commentCount = commentCount,
                        message = hasComment ? 
                            $"El usuario {idUsuario} ya tiene {commentCount} comentario(s) en la categoría {idCategoria}" : 
                            $"El usuario {idUsuario} no tiene comentarios en la categoría {idCategoria}"
                    });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al verificar comentario de usuario {IdUsuario} en categoría {IdCategoria}", idUsuario, idCategoria);
                return StatusCode(500, new { error = ex.Message });
            }
        }
    }

    // Eliminar comentario
    [HttpDelete("{id}")]
    public IActionResult DeleteComment(int id)
    {
        using (SqlConnection conn = new SqlConnection(_connectionString))
        {
            try
            {
                conn.Open();
                string query = "DELETE FROM Comentarios_comenter WHERE Id = @Id";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@Id", id);
                    int rowsAffected = cmd.ExecuteNonQuery();

                    if (rowsAffected == 0)
                    {
                        return NotFound(new { error = $"No se encontró el comentario con ID {id} para eliminar" });
                    }

                    return Ok(new { message = "Comentario eliminado correctamente" });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al eliminar comentario con ID: {Id}", id);
                return StatusCode(500, new { error = ex.Message });
            }
        }
    }

    // Crear sesión de Commenter (similar a otras herramientas)
    [HttpPost("session")]
    public IActionResult CreateCommenterSession([FromBody] CommenterSessionRequest request)
    {
        try
        {
            _logger.LogInformation("=== Creando sesión de Commenter ===");
            _logger.LogInformation("Request recibido: {@Request}", request);
            
            if (request == null)
            {
                _logger.LogWarning("Request es null");
                return BadRequest(new { error = "La solicitud no puede estar vacía" });
            }

            // Validar que IdProy sea válido
            if (request.IdProy <= 0)
            {
                _logger.LogWarning("IdProy inválido: {IdProy}", request.IdProy);
                return BadRequest(new { error = "El ID del proyecto debe ser mayor a 0" });
            }

            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                conn.Open();

                // Verificar que el proyecto existe
                string checkProyectoQuery = "SELECT COUNT(*) FROM Proyectos WHERE Id = @IdProy";
                using (SqlCommand checkCmd = new SqlCommand(checkProyectoQuery, conn))
                {
                    checkCmd.Parameters.AddWithValue("@IdProy", request.IdProy);
                    int proyectoExists = (int)checkCmd.ExecuteScalar();
                    
                    if (proyectoExists == 0)
                    {
                        _logger.LogError("Proyecto con ID {IdProy} no existe", request.IdProy);
                        return BadRequest(new { error = $"El proyecto con ID {request.IdProy} no existe" });
                    }
                }

                // Verificar si ya existe una sesión de Commenter con estos parámetros
                string queryCheck = @"SELECT Id FROM Comenter 
                                    WHERE IdProy = @IdProy 
                                      AND (IdTopico = @IdTopico OR (IdTopico IS NULL AND @IdTopico IS NULL))
                                      AND (IdCategorizer = @IdCategorizer OR (IdCategorizer IS NULL AND @IdCategorizer IS NULL))";
                
                using (SqlCommand checkCmd = new SqlCommand(queryCheck, conn))
                {
                    checkCmd.Parameters.AddWithValue("@IdProy", request.IdProy);
                    checkCmd.Parameters.AddWithValue("@IdTopico", request.IdTopico ?? (object)DBNull.Value);
                    checkCmd.Parameters.AddWithValue("@IdCategorizer", request.IdCategorizer ?? (object)DBNull.Value);
                    
                    var existingId = checkCmd.ExecuteScalar();
                    if (existingId != null && existingId != DBNull.Value)
                    {
                        int idComenter = (int)existingId;
                        _logger.LogInformation("Sesión de Commenter ya existe con ID: {IdComenter}", idComenter);
                        return Ok(new { message = "Sesión de Commenter ya existe", id = idComenter });
                    }
                }

                // Crear nueva sesión de Commenter
                _logger.LogInformation("Creando nueva sesión de Commenter con IdProy: {IdProy}, IdTopico: {IdTopico}, IdCategorizer: {IdCategorizer}", 
                    request.IdProy, request.IdTopico, request.IdCategorizer);
                
                string query = @"INSERT INTO Comenter (IdProy, IdTopico, IdCategorizer) 
                               VALUES (@IdProy, @IdTopico, @IdCategorizer);
                               SELECT SCOPE_IDENTITY();";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@IdProy", request.IdProy);
                    cmd.Parameters.AddWithValue("@IdTopico", request.IdTopico ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@IdCategorizer", request.IdCategorizer ?? (object)DBNull.Value);

                    _logger.LogInformation("Ejecutando INSERT en Comenter");
                    var result = cmd.ExecuteScalar();
                    _logger.LogInformation("Resultado de ExecuteScalar: {Result}", result);
                    
                    if (result != null && result != DBNull.Value)
                    {
                        int newId = Convert.ToInt32(result);
                        _logger.LogInformation("Sesión de Commenter creada exitosamente con ID: {NewId}", newId);
                        return Ok(new { message = "Sesión de Commenter creada correctamente", id = newId });
                    }
                    else
                    {
                        _logger.LogError("No se obtuvo ID de la inserción");
                        return StatusCode(500, new { error = "No se pudo obtener el ID de la sesión creada" });
                    }
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al crear sesión de Commenter");
            return StatusCode(500, new { error = ex.Message });
        }
    }
}

public class CommenterItem
{
    public int Id { get; set; }
    public int IdProy { get; set; }
    public int? IdTopico { get; set; }
    public int? IdCategorizer { get; set; }
    public int IdCategoria { get; set; }
    public int Fase { get; set; }
    public int IdUsuario { get; set; }
    public string Comentario { get; set; }
    public string NombreUsuario { get; set; }
}

public class CommenterRequest
{
    public int IdProy { get; set; }
    public int? IdTopico { get; set; }
    public int? IdCategorizer { get; set; }
    public int IdCategoria { get; set; }
    public int Fase { get; set; }
    public int IdUsuario { get; set; }
    public string Comentario { get; set; }
}

public class CommenterResponse
{
    public List<CommenterItem> comments { get; set; } = new List<CommenterItem>();
}

public class CommenterSessionRequest
{
    public int IdProy { get; set; }
    public int? IdTopico { get; set; }
    public int? IdCategorizer { get; set; }
} 