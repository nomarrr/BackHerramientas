using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;

[ApiController]
[Route("api/[controller]")]
public class AgendaController : ControllerBase
{
    private readonly string _connectionString;
    private readonly ILogger<AgendaController> _logger;

    public AgendaController(ILogger<AgendaController> logger)
    {
        _connectionString = "Server=.;Database=HerramientasV3;Integrated Security=True;TrustServerCertificate=True";
        _logger = logger;
    }

    // Obtener todos los temas de agenda
    [HttpGet]
    public IActionResult GetAllAgenda()
    {
        var response = new AgendaResponse();

        using (SqlConnection conn = new SqlConnection(_connectionString))
        {
            try
            {
                conn.Open();
                string query = "SELECT Id, IdProy, Topico, Rol, IdTool FROM Agenda";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            response.agendaItems.Add(new AgendaItem
                            {
                                Id = (int)reader["Id"],
                                IdProy = (int)reader["IdProy"],
                                Topico = reader["Topico"].ToString(),
                                Rol = reader["Rol"]?.ToString(),
                                IdTool = reader["IdTool"] == DBNull.Value ? (int?)null : (int)reader["IdTool"]
                            });
                        }
                    }
                }

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener todos los temas de agenda");
                return StatusCode(500, new { error = ex.Message });
            }
        }
    }

    // Obtener tema de agenda por ID
    [HttpGet("{id}")]
    public IActionResult GetAgendaById(int id)
    {
        using (SqlConnection conn = new SqlConnection(_connectionString))
        {
            try
            {
                conn.Open();
                string query = "SELECT Id, IdProy, Topico, Rol, IdTool FROM Agenda WHERE Id = @Id";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@Id", id);

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            var agenda = new AgendaItem
                            {
                                Id = (int)reader["Id"],
                                IdProy = (int)reader["IdProy"],
                                Topico = reader["Topico"].ToString(),
                                Rol = reader["Rol"]?.ToString(),
                                IdTool = reader["IdTool"] == DBNull.Value ? (int?)null : (int)reader["IdTool"]
                            };
                            return Ok(agenda);
                        }
                        else
                        {
                            return NotFound(new { error = $"No se encontró el tema de agenda con ID {id}" });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener tema de agenda por ID: {Id}", id);
                return StatusCode(500, new { error = ex.Message });
            }
        }
    }

    // Obtener temas de agenda por proyecto
    [HttpGet("byproy/{idProy}")]
    public IActionResult GetAgendaByProy(int idProy)
    {
        var response = new AgendaResponse();

        using (SqlConnection conn = new SqlConnection(_connectionString))
        {
            try
            {
                conn.Open();
                string query = "SELECT Id, IdProy, Topico, Rol, IdTool FROM Agenda WHERE IdProy = @IdProy";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@IdProy", idProy);

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            response.agendaItems.Add(new AgendaItem
                            {
                                Id = (int)reader["Id"],
                                IdProy = (int)reader["IdProy"],
                                Topico = reader["Topico"].ToString(),
                                Rol = reader["Rol"]?.ToString(),
                                IdTool = reader["IdTool"] == DBNull.Value ? (int?)null : (int)reader["IdTool"]
                            });
                        }
                    }
                }

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener temas de agenda por proyecto: {IdProy}", idProy);
                return StatusCode(500, new { error = ex.Message });
            }
        }
    }

    // Crear nuevo tema de agenda
    [HttpPost]
    public IActionResult CreateAgenda([FromBody] AgendaRequest request)
    {
        try
        {
            if (request == null)
            {
                return BadRequest(new { error = "La solicitud no puede estar vacía" });
            }

            if (string.IsNullOrEmpty(request.Topico))
            {
                return BadRequest(new { error = "El tópico es requerido" });
            }

            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                string query = @"INSERT INTO Agenda (IdProy, Topico, Rol, IdTool) 
                               VALUES (@IdProy, @Topico, @Rol, @IdTool);
                               SELECT SCOPE_IDENTITY();";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@IdProy", request.IdProy);
                    cmd.Parameters.AddWithValue("@Topico", request.Topico);
                    cmd.Parameters.AddWithValue("@Rol", request.Rol ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@IdTool", request.IdTool ?? (object)DBNull.Value);

                    int newId = Convert.ToInt32(cmd.ExecuteScalar());
                    return Ok(new { message = "Tema de agenda creado correctamente", id = newId });
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al crear tema de agenda");
            return StatusCode(500, new { error = ex.Message });
        }
    }

    // Actualizar tema de agenda
    [HttpPut("{id}")]
    public IActionResult UpdateAgenda(int id, [FromBody] AgendaRequest request)
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
                string query = @"UPDATE Agenda
                               SET IdProy = @IdProy, Topico = @Topico, Rol = @Rol, IdTool = @IdTool
                               WHERE Id = @Id";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@Id", id);
                    cmd.Parameters.AddWithValue("@IdProy", request.IdProy);
                    cmd.Parameters.AddWithValue("@Topico", request.Topico ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@Rol", request.Rol ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@IdTool", request.IdTool ?? (object)DBNull.Value);

                    int rowsAffected = cmd.ExecuteNonQuery();

                    if (rowsAffected == 0)
                    {
                        return NotFound(new { error = $"No se encontró el tema de agenda con ID {id} para actualizar" });
                    }

                    return Ok(new { message = "Tema de agenda actualizado correctamente" });
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al actualizar tema de agenda con ID: {Id}", id);
            return StatusCode(500, new { error = ex.Message });
        }
    }

    // Eliminar tema de agenda
    [HttpDelete("{id}")]
    public IActionResult DeleteAgenda(int id)
    {
        using (SqlConnection conn = new SqlConnection(_connectionString))
        {
            try
            {
                conn.Open();
                string query = "DELETE FROM Agenda WHERE Id = @Id";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@Id", id);
                    int rowsAffected = cmd.ExecuteNonQuery();

                    if (rowsAffected == 0)
                    {
                        return NotFound(new { error = $"No se encontró el tema de agenda con ID {id} para eliminar" });
                    }

                    return Ok(new { message = "Tema de agenda eliminado correctamente" });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al eliminar tema de agenda con ID: {Id}", id);
                return StatusCode(500, new { error = ex.Message });
            }
        }
    }
}

public class AgendaItem
{
    public int Id { get; set; }
    public int IdProy { get; set; }
    public string Topico { get; set; }
    public string? Rol { get; set; }
    public int? IdTool { get; set; }
}

public class AgendaRequest
{
    public int IdProy { get; set; }
    public string Topico { get; set; }
    public string? Rol { get; set; }
    public int? IdTool { get; set; }
}

public class AgendaResponse
{
    public List<AgendaItem> agendaItems { get; set; } = new List<AgendaItem>();
}

