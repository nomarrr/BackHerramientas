using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;

// Asumiendo que BrainstormIdea y BrainstormResponse están en el mismo namespace que BrainstormController
// Si has añadido namespaces, ajusta este using según corresponda

[ApiController]
[Route("api/[controller]")]
public class CategorizerController : ControllerBase
{
    private readonly string _connectionString;
    private readonly ILogger<CategorizerController> _logger;

    public CategorizerController(ILogger<CategorizerController> logger)
    {
        // Reemplaza con tu cadena de conexión a la base de datos si es diferente
        _connectionString = @"Data Source=LAPTOP-7MITNTQF\SQLEXPRESS;Initial Catalog=Herramientas;User ID=sa;Password=admin;Encrypt=True;TrustServerCertificate=True";
        _logger = logger;
    }

    // Aquí irán los endpoints para CRUD

    [HttpGet]
    public IActionResult GetAllItems()
    {
        var response = new CategorizerResponse();

        using (SqlConnection conn = new SqlConnection(_connectionString))
        {
            try
            {
                conn.Open();
                string query = "SELECT Id, Categoria, ListaIdeas, comentarios, IdProy FROM Categorizer";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            response.items.Add(new CategorizerItem
                            {
                                Id = (int)reader["Id"],
                                Categoria = reader["Categoria"].ToString(),
                                ListaIdeas = reader["ListaIdeas"].ToString(),
                                comentarios = reader["comentarios"].ToString(),
                                IdProy = (int)reader["IdProy"]
                            });
                        }
                    }
                }

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener todos los items de categorizer");
                return StatusCode(500, new { error = ex.Message });
            }
        }
    }

    [HttpGet("{id}")]
    public IActionResult GetItemById(int id)
    {
        using (SqlConnection conn = new SqlConnection(_connectionString))
        {
            try
            {
                conn.Open();
                string query = "SELECT Id, Categoria, ListaIdeas, comentarios, IdProy FROM Categorizer WHERE Id = @Id";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@Id", id);

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            var item = new CategorizerItem
                            {
                                Id = (int)reader["Id"],
                                Categoria = reader["Categoria"].ToString(),
                                ListaIdeas = reader["ListaIdeas"].ToString(),
                                comentarios = reader["comentarios"].ToString(),
                                IdProy = (int)reader["IdProy"]
                            };
                            return Ok(item);
                        }
                        else
                        {
                            return NotFound(new { error = $"No se encontró el item con ID {id}" });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener item de categorizer por ID: {Id}", id);
                return StatusCode(500, new { error = ex.Message });
            }
        }
    }

    // Endpoint para obtener items de categorizer por IdProy
    [HttpGet("categoriesbyproy/{idProy}")]
    public IActionResult GetItemsByProy(int idProy)
    {
        var response = new CategorizerResponse();

        using (SqlConnection conn = new SqlConnection(_connectionString))
        {
            try
            {
                conn.Open();
                string query = "SELECT Id, Categoria, ListaIdeas, comentarios, IdProy FROM Categorizer WHERE IdProy = @IdProy";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@IdProy", idProy);

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            response.items.Add(new CategorizerItem
                            {
                                Id = (int)reader["Id"],
                                Categoria = reader["Categoria"].ToString(),
                                ListaIdeas = reader["ListaIdeas"].ToString(),
                                comentarios = reader["comentarios"].ToString(),
                                IdProy = (int)reader["IdProy"]
                            });
                        }
                    }
                }

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener items de categorizer por IdProy: {IdProy}", idProy);
                return StatusCode(500, new { error = ex.Message });
            }
        }
    }

    // Endpoint para crear un nuevo item de categorizer
    [HttpPost]
    public IActionResult CreateItem([FromBody] CategorizerRequest request)
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
                string query = @"INSERT INTO Categorizer (Categoria, ListaIdeas, comentarios, IdProy) 
                               VALUES (@Categoria, @ListaIdeas, @comentarios, @IdProy); 
                               SELECT SCOPE_IDENTITY();";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@Categoria", request.Categoria ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@ListaIdeas", request.ListaIdeas ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@comentarios", request.comentarios ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@IdProy", request.IdProy);

                    int newId = Convert.ToInt32(cmd.ExecuteScalar());
                    return Ok(new { message = "Item de categorizer agregado correctamente", id = newId });
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al crear item de categorizer");
            return StatusCode(500, new { error = ex.Message });
        }
    }

    // Endpoint para actualizar un item de categorizer existente
    [HttpPut("{id}")]
    public IActionResult UpdateItem(int id, [FromBody] CategorizerRequest request)
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
                string query = @"UPDATE Categorizer
                               SET Categoria = @Categoria, ListaIdeas = @ListaIdeas, comentarios = @comentarios, IdProy = @IdProy
                               WHERE Id = @Id";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@Id", id);
                    cmd.Parameters.AddWithValue("@Categoria", request.Categoria ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@ListaIdeas", request.ListaIdeas ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@comentarios", request.comentarios ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@IdProy", request.IdProy);

                    int rowsAffected = cmd.ExecuteNonQuery();

                    if (rowsAffected == 0)
                    {
                        return NotFound(new { error = $"No se encontró el item con ID {id} para actualizar" });
                    }

                    return Ok(new { message = "Item de categorizer actualizado correctamente" });
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al actualizar item de categorizer con ID: {Id}", id);
            return StatusCode(500, new { error = ex.Message });
        }
    }

    // Endpoint para eliminar un item de categorizer
    [HttpDelete("{id}")]
    public IActionResult DeleteItem(int id)
    {
        using (SqlConnection conn = new SqlConnection(_connectionString))
        {
            try
            {
                conn.Open();
                string query = "DELETE FROM Categorizer WHERE Id = @Id";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@Id", id);
                    int rowsAffected = cmd.ExecuteNonQuery();

                    if (rowsAffected == 0)
                    {
                         return NotFound(new { error = $"No se encontró el item con ID {id} para eliminar" });
                    }

                    return Ok(new { message = "Item de categorizer eliminado correctamente" });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al eliminar item de categorizer con ID: {Id}", id);
                return StatusCode(500, new { error = ex.Message });
            }
        }
    }

    // Endpoint para obtener ideas por IdProy
    [HttpGet("byproy/{idProy}")]
    public IActionResult GetByProy(int idProy)
    {
        var response = new BrainstormResponse
        {
            ideas = new List<BrainstormIdea>()
        };

        using (SqlConnection conn = new SqlConnection(_connectionString))
        {
            try
            {
                conn.Open();
                string query = @"SELECT Id, IdProy, IdUsuario, Idea, Nombre
                               FROM Brainstorm 
                               WHERE IdProy = @IdProy";

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
                                IdProy = (int)reader["IdProy"],
                                IdUsuario = (int)reader["IdUsuario"],
                                Idea = reader["Idea"].ToString(),
                                Nombre = reader["Nombre"].ToString()
                            });
                        }
                    }
                }

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener ideas por IdProy: {IdProy}", idProy);
                return StatusCode(500, new { error = ex.Message });
            }
        }
    }
}

public class CategorizerItem
{
    public int Id { get; set; }
    public string? Categoria { get; set; }
    public string? ListaIdeas { get; set; }
    public string? comentarios { get; set; }
    public int IdProy { get; set; }
}

public class CategorizerRequest
{
    public string? Categoria { get; set; }
    public string? ListaIdeas { get; set; }
    public string? comentarios { get; set; }
    public int IdProy { get; set; }
}

public class CategorizerResponse
{
    public List<CategorizerItem> items { get; set; } = new List<CategorizerItem>();
}