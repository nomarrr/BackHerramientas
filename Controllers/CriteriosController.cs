using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;

[ApiController]
[Route("api/[controller]")]
public class CriteriosController : ControllerBase
{
    private readonly string _connectionString;
    private readonly ILogger<CriteriosController> _logger;

    public CriteriosController(ILogger<CriteriosController> logger)
    {
        _connectionString = @"Data Source=LAPTOP-7MITNTQF\SQLEXPRESS;Initial Catalog=HerramientasV3;User ID=sa;Password=admin;Encrypt=True;TrustServerCertificate=True";
        _logger = logger;
    }

    // Obtener todos los criterios
    [HttpGet]
    public IActionResult GetAllCriterios()
    {
        var response = new CriteriosResponse();

        using (SqlConnection conn = new SqlConnection(_connectionString))
        {
            try
            {
                conn.Open();
                string query = "SELECT Id, IdProy, Nombre, Descripcion, Peso FROM Criterios";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            response.criterios.Add(new CriterioItem
                            {
                                Id = (int)reader["Id"],
                                IdProy = (int)reader["IdProy"],
                                Nombre = reader["Nombre"].ToString(),
                                Descripcion = reader["Descripcion"]?.ToString(),
                                Peso = reader["Peso"] == DBNull.Value ? (decimal?)null : (decimal)reader["Peso"]
                            });
                        }
                    }
                }

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener todos los criterios");
                return StatusCode(500, new { error = ex.Message });
            }
        }
    }

    // Obtener criterio por ID
    [HttpGet("{id}")]
    public IActionResult GetCriterioById(int id)
    {
        using (SqlConnection conn = new SqlConnection(_connectionString))
        {
            try
            {
                conn.Open();
                string query = "SELECT Id, IdProy, Nombre, Descripcion, Peso FROM Criterios WHERE Id = @Id";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@Id", id);

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            var criterio = new CriterioItem
                            {
                                Id = (int)reader["Id"],
                                IdProy = (int)reader["IdProy"],
                                Nombre = reader["Nombre"].ToString(),
                                Descripcion = reader["Descripcion"]?.ToString(),
                                Peso = reader["Peso"] == DBNull.Value ? (decimal?)null : (decimal)reader["Peso"]
                            };
                            return Ok(criterio);
                        }
                        else
                        {
                            return NotFound(new { error = $"No se encontró el criterio con ID {id}" });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener criterio por ID: {Id}", id);
                return StatusCode(500, new { error = ex.Message });
            }
        }
    }

    // Obtener criterios por proyecto
    [HttpGet("byproy/{idProy}")]
    public IActionResult GetCriteriosByProy(int idProy)
    {
        var response = new CriteriosResponse();

        using (SqlConnection conn = new SqlConnection(_connectionString))
        {
            try
            {
                conn.Open();
                string query = "SELECT Id, IdProy, Nombre, Descripcion, Peso FROM Criterios WHERE IdProy = @IdProy";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@IdProy", idProy);

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            response.criterios.Add(new CriterioItem
                            {
                                Id = (int)reader["Id"],
                                IdProy = (int)reader["IdProy"],
                                Nombre = reader["Nombre"].ToString(),
                                Descripcion = reader["Descripcion"]?.ToString(),
                                Peso = reader["Peso"] == DBNull.Value ? (decimal?)null : (decimal)reader["Peso"]
                            });
                        }
                    }
                }

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener criterios por proyecto: {IdProy}", idProy);
                return StatusCode(500, new { error = ex.Message });
            }
        }
    }

    // Crear nuevo criterio
    [HttpPost]
    public IActionResult CreateCriterio([FromBody] CriterioRequest request)
    {
        try
        {
            if (request == null)
            {
                return BadRequest(new { error = "La solicitud no puede estar vacía" });
            }

            if (string.IsNullOrEmpty(request.Nombre))
            {
                return BadRequest(new { error = "El nombre del criterio es requerido" });
            }

            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                string query = @"INSERT INTO Criterios (IdProy, Nombre, Descripcion, Peso) 
                               VALUES (@IdProy, @Nombre, @Descripcion, @Peso);
                               SELECT SCOPE_IDENTITY();";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@IdProy", request.IdProy);
                    cmd.Parameters.AddWithValue("@Nombre", request.Nombre);
                    cmd.Parameters.AddWithValue("@Descripcion", request.Descripcion ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@Peso", request.Peso ?? (object)DBNull.Value);

                    int newId = Convert.ToInt32(cmd.ExecuteScalar());
                    return Ok(new { message = "Criterio creado correctamente", id = newId });
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al crear criterio");
            return StatusCode(500, new { error = ex.Message });
        }
    }

    // Actualizar criterio
    [HttpPut("{id}")]
    public IActionResult UpdateCriterio(int id, [FromBody] CriterioRequest request)
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
                string query = @"UPDATE Criterios
                               SET IdProy = @IdProy, Nombre = @Nombre, Descripcion = @Descripcion, Peso = @Peso
                               WHERE Id = @Id";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@Id", id);
                    cmd.Parameters.AddWithValue("@IdProy", request.IdProy);
                    cmd.Parameters.AddWithValue("@Nombre", request.Nombre ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@Descripcion", request.Descripcion ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@Peso", request.Peso ?? (object)DBNull.Value);

                    int rowsAffected = cmd.ExecuteNonQuery();

                    if (rowsAffected == 0)
                    {
                        return NotFound(new { error = $"No se encontró el criterio con ID {id} para actualizar" });
                    }

                    return Ok(new { message = "Criterio actualizado correctamente" });
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al actualizar criterio con ID: {Id}", id);
            return StatusCode(500, new { error = ex.Message });
        }
    }

    // Eliminar criterio
    [HttpDelete("{id}")]
    public IActionResult DeleteCriterio(int id)
    {
        using (SqlConnection conn = new SqlConnection(_connectionString))
        {
            try
            {
                conn.Open();
                string query = "DELETE FROM Criterios WHERE Id = @Id";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@Id", id);
                    int rowsAffected = cmd.ExecuteNonQuery();

                    if (rowsAffected == 0)
                    {
                        return NotFound(new { error = $"No se encontró el criterio con ID {id} para eliminar" });
                    }

                    return Ok(new { message = "Criterio eliminado correctamente" });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al eliminar criterio con ID: {Id}", id);
                return StatusCode(500, new { error = ex.Message });
            }
        }
    }
}

public class CriterioItem
{
    public int Id { get; set; }
    public int IdProy { get; set; }
    public string Nombre { get; set; }
    public string? Descripcion { get; set; }
    public decimal? Peso { get; set; }
}

public class CriterioRequest
{
    public int IdProy { get; set; }
    public string Nombre { get; set; }
    public string? Descripcion { get; set; }
    public decimal? Peso { get; set; }
}

public class CriteriosResponse
{
    public List<CriterioItem> criterios { get; set; } = new List<CriterioItem>();
}

