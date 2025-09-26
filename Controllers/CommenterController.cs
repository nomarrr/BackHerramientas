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
        // Reemplaza con tu cadena de conexión a la base de datos si es diferente
        _connectionString = @"Data Source=LAPTOP-7MITNTQF\SQLEXPRESS;Initial Catalog=Herramientas;User ID=sa;Password=admin;Encrypt=True;TrustServerCertificate=True";
        _logger = logger;
    }

    // Endpoint para obtener todos los comentarios
    [HttpGet]
    public IActionResult GetAllComments()
    {
        var response = new CommenterResponse();

        using (SqlConnection conn = new SqlConnection(_connectionString))
        {
            try
            {
                conn.Open();
                string query = "SELECT Id, IdCategoria, Comentarios, IdProy, IdUsuario FROM Commenter";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            response.comments.Add(new CommenterItem
                            {
                                Id = (int)reader["Id"],
                                IdCategoria = (int)reader["IdCategoria"],
                                Comentarios = reader["Comentarios"].ToString(),
                                IdProy = (int)reader["IdProy"],
                                IdUsuario = (int)reader["IdUsuario"]
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

    // Endpoint para obtener un comentario por su ID
    [HttpGet("{id}")]
    public IActionResult GetCommentById(int id)
    {
        using (SqlConnection conn = new SqlConnection(_connectionString))
        {
            try
            {
                conn.Open();
                string query = "SELECT Id, IdCategoria, Comentarios, IdProy, IdUsuario FROM Commenter WHERE Id = @Id";

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
                                IdCategoria = (int)reader["IdCategoria"],
                                Comentarios = reader["Comentarios"].ToString(),
                                IdProy = (int)reader["IdProy"],
                                IdUsuario = (int)reader["IdUsuario"]
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

    // Endpoint para obtener comentarios por IdProy
    [HttpGet("categoriesbyproy/{idProy}")]
    public IActionResult GetItemsByProy(int idProy)
    {
        var response = new CommenterResponse();

        using (SqlConnection conn = new SqlConnection(_connectionString))
        {
            try
            {
                conn.Open();
                string query = "SELECT Id, IdCategoria, Comentarios, IdProy, IdUsuario FROM Commenter WHERE IdProy = @IdProy";

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
                                IdCategoria = (int)reader["IdCategoria"],
                                Comentarios = reader["Comentarios"].ToString(),
                                IdProy = (int)reader["IdProy"],
                                IdUsuario = (int)reader["IdUsuario"]
                            });
                        }
                    }
                }

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener comentarios por IdProy: {IdProy}", idProy);
                return StatusCode(500, new { error = ex.Message });
            }
        }
    }

    // Endpoint para crear un nuevo comentario
    [HttpPost]
    public IActionResult CreateComment([FromBody] CommenterRequest request)
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
                string query = @"INSERT INTO Commenter (IdCategoria, Comentarios, IdProy, IdUsuario) 
                               VALUES (@IdCategoria, @Comentarios, @IdProy, @IdUsuario); 
                               SELECT SCOPE_IDENTITY();";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@IdCategoria", request.IdCategoria);
                    cmd.Parameters.AddWithValue("@Comentarios", request.Comentarios ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@IdProy", request.IdProy);
                    cmd.Parameters.AddWithValue("@IdUsuario", request.IdUsuario);

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

    // Endpoint para actualizar un comentario existente
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
                               SET IdCategoria = @IdCategoria, Comentarios = @Comentarios, IdProy = @IdProy, IdUsuario = @IdUsuario
                               WHERE Id = @Id";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@Id", id);
                    cmd.Parameters.AddWithValue("@IdCategoria", request.IdCategoria);
                    cmd.Parameters.AddWithValue("@Comentarios", request.Comentarios ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@IdProy", request.IdProy);
                    cmd.Parameters.AddWithValue("@IdUsuario", request.IdUsuario);

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

    // Endpoint para eliminar un comentario
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
    public int IdCategoria { get; set; }
    public string? Comentarios { get; set; }
    public int IdProy { get; set; }
    public int IdUsuario { get; set; }
}

public class CommenterRequest
{
    public int IdCategoria { get; set; }
    public string? Comentarios { get; set; }
    public int IdProy { get; set; }
    public int IdUsuario { get; set; }
}

public class CommenterResponse
{
    public List<CommenterItem> comments { get; set; } = new List<CommenterItem>();
} 