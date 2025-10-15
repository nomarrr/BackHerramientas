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
        _connectionString = @"Data Source=LAPTOP-7MITNTQF\SQLEXPRESS;Initial Catalog=Herramientas;User ID=sa;Password=admin;Encrypt=True;TrustServerCertificate=True";
        _logger = logger;
    }

    // Obtener todas las sesiones de voting
    [HttpGet]
    public IActionResult GetAllVotings()
    {
        var response = new VotingSessionsResponse();

        using (SqlConnection conn = new SqlConnection(_connectionString))
        {
            try
            {
                conn.Open();
                string query = "SELECT Id, IdProy, Fases FROM Voting";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            response.sessions.Add(new VotingSession
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
                _logger.LogError(ex, "Error al obtener sesiones de voting");
                return StatusCode(500, new { error = ex.Message });
            }
        }
    }

    // Obtener sesión de voting por ID
    [HttpGet("{id}")]
    public IActionResult GetVotingById(int id)
    {
        using (SqlConnection conn = new SqlConnection(_connectionString))
        {
            try
            {
                conn.Open();
                string query = "SELECT Id, IdProy, Fases FROM Voting WHERE Id = @Id";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@Id", id);

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            var session = new VotingSession
                            {
                                Id = (int)reader["Id"],
                                IdProy = (int)reader["IdProy"],
                                Fases = (int)reader["Fases"]
                            };
                            return Ok(session);
                        }
                        else
                        {
                            return NotFound(new { error = $"No se encontró la sesión de voting con ID {id}" });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener sesión de voting por ID: {Id}", id);
                return StatusCode(500, new { error = ex.Message });
            }
        }
    }

    // Obtener sesiones de voting por proyecto
    [HttpGet("byproy/{idProy}")]
    public IActionResult GetVotingsByProy(int idProy)
    {
        var response = new VotingSessionsResponse();

        using (SqlConnection conn = new SqlConnection(_connectionString))
        {
            try
            {
                conn.Open();
                string query = "SELECT Id, IdProy, Fases FROM Voting WHERE IdProy = @IdProy";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@IdProy", idProy);

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            response.sessions.Add(new VotingSession
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
                _logger.LogError(ex, "Error al obtener sesiones de voting por proyecto: {IdProy}", idProy);
                return StatusCode(500, new { error = ex.Message });
            }
        }
    }

    // Crear nueva sesión de voting
    [HttpPost]
    public IActionResult CreateVoting([FromBody] VotingSessionRequest request)
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
                string query = @"INSERT INTO Voting (IdProy, Fases) 
                               VALUES (@IdProy, @Fases);
                               SELECT SCOPE_IDENTITY();";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@IdProy", request.IdProy);
                    cmd.Parameters.AddWithValue("@Fases", request.Fases);

                    int newId = Convert.ToInt32(cmd.ExecuteScalar());
                    return Ok(new { message = "Sesión de voting creada correctamente", id = newId });
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al crear sesión de voting");
            return StatusCode(500, new { error = ex.Message });
        }
    }

    // Actualizar sesión de voting
    [HttpPut("{id}")]
    public IActionResult UpdateVoting(int id, [FromBody] VotingSessionRequest request)
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
                string query = @"UPDATE Voting
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
                        return NotFound(new { error = $"No se encontró la sesión de voting con ID {id}" });
                    }

                    return Ok(new { message = "Sesión de voting actualizada correctamente" });
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al actualizar sesión de voting con ID: {Id}", id);
            return StatusCode(500, new { error = ex.Message });
        }
    }

    // Eliminar sesión de voting
    [HttpDelete("{id}")]
    public IActionResult DeleteVoting(int id)
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
                        return NotFound(new { error = $"No se encontró la sesión de voting con ID {id}" });
                    }

                    return Ok(new { message = "Sesión de voting eliminada correctamente" });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al eliminar sesión de voting con ID: {Id}", id);
                return StatusCode(500, new { error = ex.Message });
            }
        }
    }

    // Obtener votos de una sesión de voting
    [HttpGet("{idVoting}/votos")]
    public IActionResult GetVotos(int idVoting)
    {
        var response = new VotosResponse();

        using (SqlConnection conn = new SqlConnection(_connectionString))
        {
            try
            {
                conn.Open();
                string query = @"SELECT vv.IdVoting, vv.Fase, vv.IdCategoria, vv.IdUsuario, u.Nombre
                               FROM Votos_voting vv
                               INNER JOIN Usuarios u ON vv.IdUsuario = u.Id
                               WHERE vv.IdVoting = @IdVoting";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@IdVoting", idVoting);

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            response.votos.Add(new VotoItem
                            {
                                IdVoting = (int)reader["IdVoting"],
                                Fase = (int)reader["Fase"],
                                IdCategoria = (int)reader["IdCategoria"],
                                IdUsuario = (int)reader["IdUsuario"],
                                NombreUsuario = reader["Nombre"].ToString()
                            });
                        }
                    }
                }

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener votos");
                return StatusCode(500, new { error = ex.Message });
            }
        }
    }

    // Agregar voto a una sesión de voting
    [HttpPost("{idVoting}/votos")]
    public IActionResult AddVoto(int idVoting, [FromBody] VotoRequest request)
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
                string query = @"INSERT INTO Votos_voting (IdVoting, Fase, IdCategoria, IdUsuario) 
                               VALUES (@IdVoting, @Fase, @IdCategoria, @IdUsuario);
                               SELECT SCOPE_IDENTITY();";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@IdVoting", idVoting);
                    cmd.Parameters.AddWithValue("@Fase", request.Fase);
                    cmd.Parameters.AddWithValue("@IdCategoria", request.IdCategoria);
                    cmd.Parameters.AddWithValue("@IdUsuario", request.IdUsuario);

                    int newId = Convert.ToInt32(cmd.ExecuteScalar());
                    return Ok(new { message = "Voto agregado correctamente", id = newId });
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al agregar voto");
            return StatusCode(500, new { error = ex.Message });
        }
    }

    // Eliminar voto
    [HttpDelete("votos/{id}")]
    public IActionResult DeleteVoto(int id)
    {
        using (SqlConnection conn = new SqlConnection(_connectionString))
        {
            try
            {
                conn.Open();
                string query = "DELETE FROM Votos_voting WHERE IdVoting = @Id";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@Id", id);
                    int rowsAffected = cmd.ExecuteNonQuery();

                    if (rowsAffected == 0)
                    {
                        return NotFound(new { error = $"No se encontró el voto con ID {id}" });
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

    // Obtener estadísticas de votos por categoría
    [HttpGet("{idVoting}/estadisticas")]
    public IActionResult GetEstadisticas(int idVoting)
    {
        var response = new EstadisticasResponse();

        using (SqlConnection conn = new SqlConnection(_connectionString))
        {
            try
            {
                conn.Open();
                string query = @"SELECT vv.IdCategoria, COUNT(*) as TotalVotos
                               FROM Votos_voting vv
                               WHERE vv.IdVoting = @IdVoting
                               GROUP BY vv.IdCategoria";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@IdVoting", idVoting);

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            response.estadisticas.Add(new EstadisticaItem
                            {
                                IdCategoria = (int)reader["IdCategoria"],
                                TotalVotos = (int)reader["TotalVotos"]
                            });
                        }
                    }
                }

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener estadísticas de votos");
                return StatusCode(500, new { error = ex.Message });
            }
        }
    }
}

// Clases para sesiones de voting
public class VotingSession
{
    public int Id { get; set; }
    public int IdProy { get; set; }
    public int Fases { get; set; }
}

public class VotingSessionRequest
{
    public int IdProy { get; set; }
    public int Fases { get; set; }
}

public class VotingSessionsResponse
{
    public List<VotingSession> sessions { get; set; } = new List<VotingSession>();
}

// Clases para votos
public class VotoItem
{
    public int IdVoting { get; set; }
    public int Fase { get; set; }
    public int IdCategoria { get; set; }
    public int IdUsuario { get; set; }
    public string NombreUsuario { get; set; }
}

public class VotoRequest
{
    public int Fase { get; set; }
    public int IdCategoria { get; set; }
    public int IdUsuario { get; set; }
}

public class VotosResponse
{
    public List<VotoItem> votos { get; set; } = new List<VotoItem>();
}

// Clases para estadísticas
public class EstadisticaItem
{
    public int IdCategoria { get; set; }
    public int TotalVotos { get; set; }
}

public class EstadisticasResponse
{
    public List<EstadisticaItem> estadisticas { get; set; } = new List<EstadisticaItem>();
} 