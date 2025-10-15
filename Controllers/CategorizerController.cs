using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;

[ApiController]
[Route("api/[controller]")]
public class CategorizerController : ControllerBase
{
    private readonly string _connectionString;
    private readonly ILogger<CategorizerController> _logger;

    public CategorizerController(ILogger<CategorizerController> logger)
    {
        _connectionString = @"Data Source=LAPTOP-7MITNTQF\SQLEXPRESS;Initial Catalog=Herramientas;User ID=sa;Password=admin;Encrypt=True;TrustServerCertificate=True";
        _logger = logger;
    }

    // Obtener todas las sesiones de categorizer
    [HttpGet]
    public IActionResult GetAllCategorizers()
    {
        var response = new CategorizerSessionsResponse();

        using (SqlConnection conn = new SqlConnection(_connectionString))
        {
            try
            {
                conn.Open();
                string query = "SELECT Id, IdProy, Fases FROM Categorizer";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            response.sessions.Add(new CategorizerSession
                            {
                                Id = (int)reader["Id"],
                                IdProy = (int)reader["IdProy"],
                                Fases = (int)reader["Fases"]
                            });
                        }
                    }
                }

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener sesiones de categorizer");
                return StatusCode(500, new { error = ex.Message });
            }
        }
    }

    // Obtener sesión de categorizer por ID
    [HttpGet("{id}")]
    public IActionResult GetCategorizerById(int id)
    {
        using (SqlConnection conn = new SqlConnection(_connectionString))
        {
            try
            {
                conn.Open();
                string query = "SELECT Id, IdProy, Fases FROM Categorizer WHERE Id = @Id";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@Id", id);

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            var session = new CategorizerSession
                            {
                                Id = (int)reader["Id"],
                                IdProy = (int)reader["IdProy"],
                                Fases = (int)reader["Fases"]
                            };
                            return Ok(session);
                        }
                        else
                        {
                            return NotFound(new { error = $"No se encontró la sesión de categorizer con ID {id}" });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener sesión de categorizer por ID: {Id}", id);
                return StatusCode(500, new { error = ex.Message });
            }
        }
    }

    // Obtener sesiones de categorizer por proyecto
    [HttpGet("byproy/{idProy}")]
    public IActionResult GetCategorizersByProy(int idProy)
    {
        var response = new CategorizerSessionsResponse();

        using (SqlConnection conn = new SqlConnection(_connectionString))
        {
            try
            {
                conn.Open();
                string query = "SELECT Id, IdProy, Fases FROM Categorizer WHERE IdProy = @IdProy";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@IdProy", idProy);

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            response.sessions.Add(new CategorizerSession
                            {
                                Id = (int)reader["Id"],
                                IdProy = (int)reader["IdProy"],
                                Fases = (int)reader["Fases"]
                            });
                        }
                    }
                }

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener sesiones de categorizer por proyecto: {IdProy}", idProy);
                return StatusCode(500, new { error = ex.Message });
            }
        }
    }

    // Crear nueva sesión de categorizer
    [HttpPost]
    public IActionResult CreateCategorizer([FromBody] CategorizerSessionRequest request)
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
                string query = @"INSERT INTO Categorizer (IdProy, Fases) 
                               VALUES (@IdProy, @Fases);
                               SELECT SCOPE_IDENTITY();";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@IdProy", request.IdProy);
                    cmd.Parameters.AddWithValue("@Fases", request.Fases);

                    int newId = Convert.ToInt32(cmd.ExecuteScalar());
                    return Ok(new { message = "Sesión de categorizer creada correctamente", id = newId });
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al crear sesión de categorizer");
            return StatusCode(500, new { error = ex.Message });
        }
    }

    // Actualizar sesión de categorizer
    [HttpPut("{id}")]
    public IActionResult UpdateCategorizer(int id, [FromBody] CategorizerSessionRequest request)
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
                               SET IdProy = @IdProy, Fases = @Fases
                               WHERE Id = @Id";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@Id", id);
                    cmd.Parameters.AddWithValue("@IdProy", request.IdProy);
                    cmd.Parameters.AddWithValue("@Fases", request.Fases);

                    int rowsAffected = cmd.ExecuteNonQuery();

                    if (rowsAffected == 0)
                    {
                        return NotFound(new { error = $"No se encontró la sesión de categorizer con ID {id}" });
                    }

                    return Ok(new { message = "Sesión de categorizer actualizada correctamente" });
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al actualizar sesión de categorizer con ID: {Id}", id);
            return StatusCode(500, new { error = ex.Message });
        }
    }

    // Eliminar sesión de categorizer
    [HttpDelete("{id}")]
    public IActionResult DeleteCategorizer(int id)
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
                        return NotFound(new { error = $"No se encontró la sesión de categorizer con ID {id}" });
                    }

                    return Ok(new { message = "Sesión de categorizer eliminada correctamente" });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al eliminar sesión de categorizer con ID: {Id}", id);
                return StatusCode(500, new { error = ex.Message });
            }
        }
    }

    // Obtener categorías de una sesión de categorizer
    [HttpGet("{idCategorizer}/categorias")]
    public IActionResult GetCategorias(int idCategorizer)
    {
        var response = new CategoriasResponse();

        using (SqlConnection conn = new SqlConnection(_connectionString))
        {
            try
            {
                conn.Open();
                string query = @"SELECT IdCategoria, Fase, Categoria, ListaIdeas
                               FROM Categorias_categorizer
                               WHERE IdCategoria = @IdCategorizer";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@IdCategorizer", idCategorizer);

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            response.categorias.Add(new CategoriaItem
                            {
                                IdCategoria = (int)reader["IdCategoria"],
                                Fase = (int)reader["Fase"],
                                Categoria = reader["Categoria"].ToString(),
                                ListaIdeas = reader["ListaIdeas"].ToString()
                            });
                        }
                    }
                }

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener categorías");
                return StatusCode(500, new { error = ex.Message });
            }
        }
    }

    // Agregar categoría a una sesión de categorizer
    [HttpPost("{idCategorizer}/categorias")]
    public IActionResult AddCategoria(int idCategorizer, [FromBody] CategoriaRequest request)
    {
        try
        {
            if (request == null)
            {
                return BadRequest(new { error = "La solicitud no puede estar vacía" });
            }

            if (string.IsNullOrEmpty(request.Categoria))
            {
                return BadRequest(new { error = "La categoría es requerida" });
            }

            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                string query = @"INSERT INTO Categorias_categorizer (IdCategoria, Fase, Categoria, ListaIdeas) 
                               VALUES (@IdCategoria, @Fase, @Categoria, @ListaIdeas);
                               SELECT SCOPE_IDENTITY();";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@IdCategoria", idCategorizer);
                    cmd.Parameters.AddWithValue("@Fase", request.Fase);
                    cmd.Parameters.AddWithValue("@Categoria", request.Categoria);
                    cmd.Parameters.AddWithValue("@ListaIdeas", request.ListaIdeas ?? (object)DBNull.Value);

                    int newId = Convert.ToInt32(cmd.ExecuteScalar());
                    return Ok(new { message = "Categoría agregada correctamente", id = newId });
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al agregar categoría");
            return StatusCode(500, new { error = ex.Message });
        }
    }

    // Actualizar categoría
    [HttpPut("categorias/{id}")]
    public IActionResult UpdateCategoria(int id, [FromBody] CategoriaRequest request)
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
                string query = @"UPDATE Categorias_categorizer
                               SET Fase = @Fase, Categoria = @Categoria, ListaIdeas = @ListaIdeas
                               WHERE IdCategoria = @Id";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@Id", id);
                    cmd.Parameters.AddWithValue("@Fase", request.Fase);
                    cmd.Parameters.AddWithValue("@Categoria", request.Categoria ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@ListaIdeas", request.ListaIdeas ?? (object)DBNull.Value);

                    int rowsAffected = cmd.ExecuteNonQuery();

                    if (rowsAffected == 0)
                    {
                        return NotFound(new { error = $"No se encontró la categoría con ID {id}" });
                    }

                    return Ok(new { message = "Categoría actualizada correctamente" });
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al actualizar categoría con ID: {Id}", id);
            return StatusCode(500, new { error = ex.Message });
        }
    }

    // Eliminar categoría
    [HttpDelete("categorias/{id}")]
    public IActionResult DeleteCategoria(int id)
    {
        using (SqlConnection conn = new SqlConnection(_connectionString))
        {
            try
            {
                conn.Open();
                string query = "DELETE FROM Categorias_categorizer WHERE IdCategoria = @Id";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@Id", id);
                    int rowsAffected = cmd.ExecuteNonQuery();

                    if (rowsAffected == 0)
                    {
                        return NotFound(new { error = $"No se encontró la categoría con ID {id}" });
                    }

                    return Ok(new { message = "Categoría eliminada correctamente" });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al eliminar categoría con ID: {Id}", id);
                return StatusCode(500, new { error = ex.Message });
            }
        }
    }
}

// Clases para sesiones de categorizer
public class CategorizerSession
{
    public int Id { get; set; }
    public int IdProy { get; set; }
    public int Fases { get; set; }
}

public class CategorizerSessionRequest
{
    public int IdProy { get; set; }
    public int Fases { get; set; }
}

public class CategorizerSessionsResponse
{
    public List<CategorizerSession> sessions { get; set; } = new List<CategorizerSession>();
}

// Clases para categorías
public class CategoriaItem
{
    public int IdCategoria { get; set; }
    public int Fase { get; set; }
    public string Categoria { get; set; }
    public string ListaIdeas { get; set; }
}

public class CategoriaRequest
{
    public int Fase { get; set; }
    public string Categoria { get; set; }
    public string ListaIdeas { get; set; }
}

public class CategoriasResponse
{
    public List<CategoriaItem> categorias { get; set; } = new List<CategoriaItem>();
}