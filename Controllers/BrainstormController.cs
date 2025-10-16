using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;

[ApiController]
[Route("api/[controller]")]
public class BrainstormController : ControllerBase
{
    private readonly string _connectionString;
    private readonly ILogger<BrainstormController> _logger;

    public BrainstormController(ILogger<BrainstormController> logger)
    {
        _connectionString = @"Data Source=LAPTOP-7MITNTQF\SQLEXPRESS;Initial Catalog=HerramientasV2;User ID=sa;Password=admin;Encrypt=True;TrustServerCertificate=True";
        _logger = logger;
    }

    // Obtener todas las sesiones de brainstorm
    [HttpGet]
    public IActionResult GetAllBrainstorms()
    {
        var response = new BrainstormSessionsResponse();

        using (SqlConnection conn = new SqlConnection(_connectionString))
        {
            try
            {
                conn.Open();
                string query = "SELECT Id, IdProy, Titulo, Descripcion, MinIdeas, MaxIdeas FROM Brainstorm";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            response.sessions.Add(new BrainstormSession
                            {
                                Id = (int)reader["Id"],
                                IdProy = (int)reader["IdProy"],
                                Titulo = reader["Titulo"].ToString(),
                                Descripcion = reader["Descripcion"].ToString(),
                                MinIdeas = (int)reader["MinIdeas"],
                                MaxIdeas = (int)reader["MaxIdeas"]
                            });
                        }
                    }
                }

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener sesiones de brainstorm");
                return StatusCode(500, new { error = ex.Message });
            }
        }
    }

    // Obtener sesión de brainstorm por ID
    [HttpGet("{id}")]
    public IActionResult GetBrainstormById(int id)
    {
        using (SqlConnection conn = new SqlConnection(_connectionString))
        {
            try
            {
                conn.Open();
                string query = "SELECT Id, IdProy, Titulo, Descripcion, MinIdeas, MaxIdeas FROM Brainstorm WHERE Id = @Id";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@Id", id);

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            var session = new BrainstormSession
                            {
                                Id = (int)reader["Id"],
                                IdProy = (int)reader["IdProy"],
                                Titulo = reader["Titulo"].ToString(),
                                Descripcion = reader["Descripcion"].ToString(),
                                MinIdeas = (int)reader["MinIdeas"],
                                MaxIdeas = (int)reader["MaxIdeas"]
                            };
                            return Ok(session);
                        }
                        else
                        {
                            return NotFound(new { error = $"No se encontró la sesión de brainstorm con ID {id}" });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener sesión de brainstorm por ID: {Id}", id);
                return StatusCode(500, new { error = ex.Message });
            }
        }
    }

    // Obtener sesiones de brainstorm por proyecto
    [HttpGet("byproy/{idProy}")]
    public IActionResult GetBrainstormsByProy(int idProy)
    {
        var response = new BrainstormSessionsResponse();

        using (SqlConnection conn = new SqlConnection(_connectionString))
        {
            try
            {
                conn.Open();
                string query = "SELECT Id, IdProy, Titulo, Descripcion, MinIdeas, MaxIdeas FROM Brainstorm WHERE IdProy = @IdProy";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@IdProy", idProy);

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            response.sessions.Add(new BrainstormSession
                            {
                                Id = (int)reader["Id"],
                                IdProy = (int)reader["IdProy"],
                                Titulo = reader["Titulo"].ToString(),
                                Descripcion = reader["Descripcion"].ToString(),
                                MinIdeas = (int)reader["MinIdeas"],
                                MaxIdeas = (int)reader["MaxIdeas"]
                            });
                        }
                    }
                }

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener sesiones de brainstorm por proyecto: {IdProy}", idProy);
                return StatusCode(500, new { error = ex.Message });
            }
        }
    }

    // Crear nueva sesión de brainstorm
    [HttpPost]
    public IActionResult CreateBrainstorm([FromBody] BrainstormSessionRequest request)
    {
        try
        {
            if (request == null)
            {
                return BadRequest(new { error = "La solicitud no puede estar vacía" });
            }

            if (string.IsNullOrEmpty(request.Titulo))
            {
                return BadRequest(new { error = "El título es requerido" });
            }

            if (string.IsNullOrEmpty(request.Descripcion))
            {
                return BadRequest(new { error = "La descripción es requerida" });
            }

            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                string query = @"INSERT INTO Brainstorm (IdProy, Titulo, Descripcion, MinIdeas, MaxIdeas) 
                               VALUES (@IdProy, @Titulo, @Descripcion, @MinIdeas, @MaxIdeas);
                               SELECT SCOPE_IDENTITY();";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@IdProy", request.IdProy);
                    cmd.Parameters.AddWithValue("@Titulo", request.Titulo);
                    cmd.Parameters.AddWithValue("@Descripcion", request.Descripcion);
                    cmd.Parameters.AddWithValue("@MinIdeas", request.MinIdeas);
                    cmd.Parameters.AddWithValue("@MaxIdeas", request.MaxIdeas);

                    int newId = Convert.ToInt32(cmd.ExecuteScalar());
                    return Ok(new { message = "Sesión de brainstorm creada correctamente", id = newId });
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al crear sesión de brainstorm");
            return StatusCode(500, new { error = ex.Message });
        }
    }

    // Actualizar sesión de brainstorm
    [HttpPut("{id}")]
    public IActionResult UpdateBrainstorm(int id, [FromBody] BrainstormSessionRequest request)
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
                string query = @"UPDATE Brainstorm
                               SET IdProy = @IdProy, Titulo = @Titulo, Descripcion = @Descripcion, 
                                   MinIdeas = @MinIdeas, MaxIdeas = @MaxIdeas
                               WHERE Id = @Id";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@Id", id);
                    cmd.Parameters.AddWithValue("@IdProy", request.IdProy);
                    cmd.Parameters.AddWithValue("@Titulo", request.Titulo ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@Descripcion", request.Descripcion ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@MinIdeas", request.MinIdeas);
                    cmd.Parameters.AddWithValue("@MaxIdeas", request.MaxIdeas);

                    int rowsAffected = cmd.ExecuteNonQuery();

                    if (rowsAffected == 0)
                    {
                        return NotFound(new { error = $"No se encontró la sesión de brainstorm con ID {id}" });
                    }

                    return Ok(new { message = "Sesión de brainstorm actualizada correctamente" });
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al actualizar sesión de brainstorm con ID: {Id}", id);
            return StatusCode(500, new { error = ex.Message });
        }
    }

    // Eliminar sesión de brainstorm
    [HttpDelete("{id}")]
    public IActionResult DeleteBrainstorm(int id)
    {
        using (SqlConnection conn = new SqlConnection(_connectionString))
        {
            try
            {
                conn.Open();
                string query = "DELETE FROM Brainstorm WHERE Id = @Id";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@Id", id);
                    int rowsAffected = cmd.ExecuteNonQuery();

                    if (rowsAffected == 0)
                    {
                        return NotFound(new { error = $"No se encontró la sesión de brainstorm con ID {id}" });
                    }

                    return Ok(new { message = "Sesión de brainstorm eliminada correctamente" });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al eliminar sesión de brainstorm con ID: {Id}", id);
                return StatusCode(500, new { error = ex.Message });
            }
        }
    }

    // Obtener ideas de una sesión de brainstorm
    [HttpGet("{idBrainstorm}/ideas")]
    public IActionResult GetIdeas(int idBrainstorm)
    {
        var response = new BrainstormIdeasResponse();

        using (SqlConnection conn = new SqlConnection(_connectionString))
        {
            try
            {
                conn.Open();
                string query = @"SELECT ib.IdBrainstorm, ib.IdUsuario, ib.Idea, u.Nombre
                               FROM Ideas_brainstorm ib
                               INNER JOIN Usuarios u ON ib.IdUsuario = u.Id
                               WHERE ib.IdBrainstorm = @IdBrainstorm";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@IdBrainstorm", idBrainstorm);

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            response.ideas.Add(new BrainstormIdea
                            {
                                IdBrainstorm = (int)reader["IdBrainstorm"],
                                IdUsuario = (int)reader["IdUsuario"],
                                Idea = reader["Idea"].ToString(),
                                NombreUsuario = reader["Nombre"].ToString()
                            });
                        }
                    }
                }

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener ideas de brainstorm");
                return StatusCode(500, new { error = ex.Message });
            }
        }
    }

    // Agregar idea a una sesión de brainstorm
    [HttpPost("{idBrainstorm}/ideas")]
    public IActionResult AddIdea(int idBrainstorm, [FromBody] BrainstormIdeaRequest request)
    {
        try
        {
            if (request == null)
            {
                return BadRequest(new { error = "La solicitud no puede estar vacía" });
            }

            if (string.IsNullOrEmpty(request.Idea))
            {
                return BadRequest(new { error = "La idea es requerida" });
            }

            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                string query = @"INSERT INTO Ideas_brainstorm (IdBrainstorm, IdUsuario, Idea) 
                               VALUES (@IdBrainstorm, @IdUsuario, @Idea);
                               SELECT SCOPE_IDENTITY();";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@IdBrainstorm", idBrainstorm);
                    cmd.Parameters.AddWithValue("@IdUsuario", request.IdUsuario);
                    cmd.Parameters.AddWithValue("@Idea", request.Idea);

                    int newId = Convert.ToInt32(cmd.ExecuteScalar());
                    return Ok(new { message = "Idea agregada correctamente", id = newId });
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al agregar idea");
            return StatusCode(500, new { error = ex.Message });
        }
    }
}

// Clases para sesiones de brainstorm
public class BrainstormSession
{
    public int Id { get; set; }
    public int IdProy { get; set; }
    public string Titulo { get; set; }
    public string Descripcion { get; set; }
    public int MinIdeas { get; set; }
    public int MaxIdeas { get; set; }
}

public class BrainstormSessionRequest
{
    public int IdProy { get; set; }
    public string Titulo { get; set; }
    public string Descripcion { get; set; }
    public int MinIdeas { get; set; }
    public int MaxIdeas { get; set; }
}

public class BrainstormSessionsResponse
{
    public List<BrainstormSession> sessions { get; set; } = new List<BrainstormSession>();
}

// Clases para ideas de brainstorm
public class BrainstormIdea
{
    public int IdBrainstorm { get; set; }
    public int IdUsuario { get; set; }
    public string Idea { get; set; }
    public string NombreUsuario { get; set; }
}

public class BrainstormIdeaRequest
{
    public int IdUsuario { get; set; }
    public string Idea { get; set; }
}

public class BrainstormIdeasResponse
{
    public List<BrainstormIdea> ideas { get; set; } = new List<BrainstormIdea>();
} 
