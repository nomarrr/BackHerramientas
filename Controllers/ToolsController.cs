using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;

[ApiController]
[Route("api/[controller]")]
public class ToolsController : ControllerBase
{
    private readonly string _connectionString;
    private readonly ILogger<ToolsController> _logger;

    public ToolsController(ILogger<ToolsController> logger)
    {
        _connectionString = @"Data Source=189.195.162.46;Initial Catalog=HerramientasV3;User ID=sa;Password=sqlSA%;Encrypt=True;TrustServerCertificate=True";
        _logger = logger;
    }

    // Obtener todas las herramientas
    [HttpGet]
    public IActionResult GetAllTools()
    {
        var response = new ToolsResponse();

        using (SqlConnection conn = new SqlConnection(_connectionString))
        {
            try
            {
                conn.Open();
                string query = "SELECT Id, Nombre, Descripcion FROM Tools";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            response.tools.Add(new ToolItem
                            {
                                Id = (int)reader["Id"],
                                Nombre = reader["Nombre"].ToString(),
                                Descripcion = reader["Descripcion"]?.ToString()
                            });
                        }
                    }
                }

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener todas las herramientas");
                return StatusCode(500, new { error = ex.Message });
            }
        }
    }

    // Obtener herramienta por ID
    [HttpGet("{id}")]
    public IActionResult GetToolById(int id)
    {
        using (SqlConnection conn = new SqlConnection(_connectionString))
        {
            try
            {
                conn.Open();
                string query = "SELECT Id, Nombre, Descripcion FROM Tools WHERE Id = @Id";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@Id", id);

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            var tool = new ToolItem
                            {
                                Id = (int)reader["Id"],
                                Nombre = reader["Nombre"].ToString(),
                                Descripcion = reader["Descripcion"]?.ToString()
                            };
                            return Ok(tool);
                        }
                        else
                        {
                            return NotFound(new { error = $"No se encontró la herramienta con ID {id}" });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener herramienta por ID: {Id}", id);
                return StatusCode(500, new { error = ex.Message });
            }
        }
    }

    // Crear nueva herramienta
    [HttpPost]
    public IActionResult CreateTool([FromBody] ToolRequest request)
    {
        try
        {
            if (request == null)
            {
                return BadRequest(new { error = "La solicitud no puede estar vacía" });
            }

            if (string.IsNullOrEmpty(request.Nombre))
            {
                return BadRequest(new { error = "El nombre de la herramienta es requerido" });
            }

            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                string query = @"INSERT INTO Tools (Nombre, Descripcion) 
                               VALUES (@Nombre, @Descripcion);
                               SELECT SCOPE_IDENTITY();";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@Nombre", request.Nombre);
                    cmd.Parameters.AddWithValue("@Descripcion", request.Descripcion ?? (object)DBNull.Value);

                    int newId = Convert.ToInt32(cmd.ExecuteScalar());
                    return Ok(new { message = "Herramienta creada correctamente", id = newId });
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al crear herramienta");
            return StatusCode(500, new { error = ex.Message });
        }
    }

    // Actualizar herramienta
    [HttpPut("{id}")]
    public IActionResult UpdateTool(int id, [FromBody] ToolRequest request)
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
                string query = @"UPDATE Tools
                               SET Nombre = @Nombre, Descripcion = @Descripcion
                               WHERE Id = @Id";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@Id", id);
                    cmd.Parameters.AddWithValue("@Nombre", request.Nombre ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@Descripcion", request.Descripcion ?? (object)DBNull.Value);

                    int rowsAffected = cmd.ExecuteNonQuery();

                    if (rowsAffected == 0)
                    {
                        return NotFound(new { error = $"No se encontró la herramienta con ID {id} para actualizar" });
                    }

                    return Ok(new { message = "Herramienta actualizada correctamente" });
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al actualizar herramienta con ID: {Id}", id);
            return StatusCode(500, new { error = ex.Message });
        }
    }

    // Eliminar herramienta
    [HttpDelete("{id}")]
    public IActionResult DeleteTool(int id)
    {
        using (SqlConnection conn = new SqlConnection(_connectionString))
        {
            try
            {
                conn.Open();
                string query = "DELETE FROM Tools WHERE Id = @Id";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@Id", id);
                    int rowsAffected = cmd.ExecuteNonQuery();

                    if (rowsAffected == 0)
                    {
                        return NotFound(new { error = $"No se encontró la herramienta con ID {id} para eliminar" });
                    }

                    return Ok(new { message = "Herramienta eliminada correctamente" });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al eliminar herramienta con ID: {Id}", id);
                return StatusCode(500, new { error = ex.Message });
            }
        }
    }
}

public class ToolItem
{
    public int Id { get; set; }
    public string Nombre { get; set; }
    public string? Descripcion { get; set; }
}

public class ToolRequest
{
    public string Nombre { get; set; }
    public string? Descripcion { get; set; }
}

public class ToolsResponse
{
    public List<ToolItem> tools { get; set; } = new List<ToolItem>();
}

