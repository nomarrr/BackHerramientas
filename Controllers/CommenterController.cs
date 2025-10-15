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
        _connectionString = @"Data Source=LAPTOP-7MITNTQF\SQLEXPRESS;Initial Catalog=Herramientas;User ID=sa;Password=admin;Encrypt=True;TrustServerCertificate=True";
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
                string query = @"SELECT c.Id, c.IdProy, c.IdCategoria, c.Fase, c.IdUsuario, c.Comentarios, u.Nombre
                               FROM Commenter c
                               INNER JOIN Usuarios u ON c.IdUsuario = u.Id";

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
                                IdCategoria = (int)reader["IdCategoria"],
                                Fase = (int)reader["Fase"],
                                IdUsuario = (int)reader["IdUsuario"],
                                Comentarios = reader["Comentarios"].ToString(),
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
                string query = @"SELECT c.Id, c.IdProy, c.IdCategoria, c.Fase, c.IdUsuario, c.Comentarios, u.Nombre
                               FROM Commenter c
                               INNER JOIN Usuarios u ON c.IdUsuario = u.Id
                               WHERE c.Id = @Id";

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
                                IdCategoria = (int)reader["IdCategoria"],
                                Fase = (int)reader["Fase"],
                                IdUsuario = (int)reader["IdUsuario"],
                                Comentarios = reader["Comentarios"].ToString(),
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
                string query = @"SELECT c.Id, c.IdProy, c.IdCategoria, c.Fase, c.IdUsuario, c.Comentarios, u.Nombre
                               FROM Commenter c
                               INNER JOIN Usuarios u ON c.IdUsuario = u.Id
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
                                IdCategoria = (int)reader["IdCategoria"],
                                Fase = (int)reader["Fase"],
                                IdUsuario = (int)reader["IdUsuario"],
                                Comentarios = reader["Comentarios"].ToString(),
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
                string query = @"SELECT c.Id, c.IdProy, c.IdCategoria, c.Fase, c.IdUsuario, c.Comentarios, u.Nombre
                               FROM Commenter c
                               INNER JOIN Usuarios u ON c.IdUsuario = u.Id
                               WHERE c.IdCategoria = @IdCategoria";

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
                                IdCategoria = (int)reader["IdCategoria"],
                                Fase = (int)reader["Fase"],
                                IdUsuario = (int)reader["IdUsuario"],
                                Comentarios = reader["Comentarios"].ToString(),
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

            if (string.IsNullOrEmpty(request.Comentarios))
            {
                return BadRequest(new { error = "El comentario es requerido" });
            }

            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                string query = @"INSERT INTO Commenter (IdProy, IdCategoria, Fase, IdUsuario, Comentarios) 
                               VALUES (@IdProy, @IdCategoria, @Fase, @IdUsuario, @Comentarios);
                               SELECT SCOPE_IDENTITY();";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@IdProy", request.IdProy);
                    cmd.Parameters.AddWithValue("@IdCategoria", request.IdCategoria);
                    cmd.Parameters.AddWithValue("@Fase", request.Fase);
                    cmd.Parameters.AddWithValue("@IdUsuario", request.IdUsuario);
                    cmd.Parameters.AddWithValue("@Comentarios", request.Comentarios);

                    int newId = Convert.ToInt32(cmd.ExecuteScalar());
                    return Ok(new { message = "Comentario agregado correctamente", id = newId });
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
                string query = @"UPDATE Commenter
                               SET IdProy = @IdProy, IdCategoria = @IdCategoria, Fase = @Fase, 
                                   IdUsuario = @IdUsuario, Comentarios = @Comentarios
                               WHERE Id = @Id";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@Id", id);
                    cmd.Parameters.AddWithValue("@IdProy", request.IdProy);
                    cmd.Parameters.AddWithValue("@IdCategoria", request.IdCategoria);
                    cmd.Parameters.AddWithValue("@Fase", request.Fase);
                    cmd.Parameters.AddWithValue("@IdUsuario", request.IdUsuario);
                    cmd.Parameters.AddWithValue("@Comentarios", request.Comentarios ?? (object)DBNull.Value);

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

    // Eliminar comentario
    [HttpDelete("{id}")]
    public IActionResult DeleteComment(int id)
    {
        using (SqlConnection conn = new SqlConnection(_connectionString))
        {
            try
            {
                conn.Open();
                string query = "DELETE FROM Commenter WHERE Id = @Id";

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
}

public class CommenterItem
{
    public int Id { get; set; }
    public int IdProy { get; set; }
    public int IdCategoria { get; set; }
    public int Fase { get; set; }
    public int IdUsuario { get; set; }
    public string Comentarios { get; set; }
    public string NombreUsuario { get; set; }
}

public class CommenterRequest
{
    public int IdProy { get; set; }
    public int IdCategoria { get; set; }
    public int Fase { get; set; }
    public int IdUsuario { get; set; }
    public string Comentarios { get; set; }
}

public class CommenterResponse
{
    public List<CommenterItem> comments { get; set; } = new List<CommenterItem>();
} 