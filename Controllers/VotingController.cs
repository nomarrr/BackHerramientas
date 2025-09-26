using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;

[ApiController]
[Route("api/[controller]")]
public class VotingController : ControllerBase
{
    private readonly string _connectionString;
    private readonly ILogger<VotingController> _logger;

    public VotingController(ILogger<VotingController> logger)
    {
        // Reemplaza con tu cadena de conexión a la base de datos si es diferente
        _connectionString = @"Data Source=LAPTOP-7MITNTQF\SQLEXPRESS;Initial Catalog=Herramientas;User ID=sa;Password=admin;Encrypt=True;TrustServerCertificate=True";
        _logger = logger;
    }

    [HttpGet]
    public IActionResult GetAllItems()
    {
        var response = new VotingResponse();

        using (SqlConnection conn = new SqlConnection(_connectionString))
        {
            try
            {
                conn.Open();
                string query = "SELECT Id, IdCategoria, IdProy, IdUsuario FROM Voting";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            response.items.Add(new VotingItem
                            {
                                Id = (int)reader["Id"],
                                IdCategoria = (int)reader["IdCategoria"],
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
                _logger.LogError(ex, "Error al obtener todos los items de voting");
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
                string query = "SELECT Id, IdCategoria, IdProy, IdUsuario FROM Voting WHERE Id = @Id";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@Id", id);

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            var item = new VotingItem
                            {
                                Id = (int)reader["Id"],
                                IdCategoria = (int)reader["IdCategoria"],
                                IdProy = (int)reader["IdProy"],
                                IdUsuario = (int)reader["IdUsuario"]
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
                _logger.LogError(ex, "Error al obtener item de voting por ID: {Id}", id);
                return StatusCode(500, new { error = ex.Message });
            }
        }
    }

    [HttpPost]
    public IActionResult CreateItem([FromBody] VotingRequest request)
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
                string query = @"INSERT INTO Voting (IdCategoria, IdProy, IdUsuario)
                               VALUES (@IdCategoria, @IdProy, @IdUsuario);
                               SELECT SCOPE_IDENTITY();";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@IdCategoria", request.IdCategoria);
                    cmd.Parameters.AddWithValue("@IdProy", request.IdProy);
                    cmd.Parameters.AddWithValue("@IdUsuario", request.IdUsuario);

                    int newId = Convert.ToInt32(cmd.ExecuteScalar());
                    return Ok(new { message = "Voto agregado correctamente", id = newId });
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al crear voto");
            return StatusCode(500, new { error = ex.Message });
        }
    }

    [HttpDelete("{id}")]
    public IActionResult DeleteItem(int id)
    {
        using (SqlConnection conn = new SqlConnection(_connectionString))
        {
            try
            {
                conn.Open();
                string query = "DELETE FROM Voting WHERE Id = @Id";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@Id", id);
                    int rowsAffected = cmd.ExecuteNonQuery();

                    if (rowsAffected == 0)
                    {
                         return NotFound(new { error = $"No se encontró el item con ID {id} para eliminar" });
                    }

                    return Ok(new { message = "Voto eliminado correctamente" });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al eliminar voto con ID: {Id}", id);
                return StatusCode(500, new { error = ex.Message });
            }
        }
    }

    [HttpGet("byproy/{idProy}")]
    public IActionResult GetItemsByProy(int idProy)
    {
        var response = new VotingResponse();

        using (SqlConnection conn = new SqlConnection(_connectionString))
        {
            try
            {
                conn.Open();
                string query = "SELECT Id, IdCategoria, IdProy, IdUsuario FROM Voting WHERE IdProy = @IdProy";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@IdProy", idProy);

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            response.items.Add(new VotingItem
                            {
                                Id = (int)reader["Id"],
                                IdCategoria = (int)reader["IdCategoria"],
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
                _logger.LogError(ex, "Error al obtener items de voting por IdProy: {IdProy}", idProy);
                return StatusCode(500, new { error = ex.Message });
            }
        }
    }

    // Aquí irán los endpoints para CRUD

}

public class VotingItem
{
    public int Id { get; set; }
    public int IdCategoria { get; set; }
    public int IdProy { get; set; }
    public int IdUsuario { get; set; }
}

public class VotingRequest
{
    public int IdCategoria { get; set; }
    public int IdProy { get; set; }
    public int IdUsuario { get; set; }
}

public class VotingResponse
{
    public List<VotingItem> items { get; set; } = new List<VotingItem>();
} 