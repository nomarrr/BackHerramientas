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
        _connectionString = "Server=.;Database=HerramientasV3;Integrated Security=True;TrustServerCertificate=True";
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
                string query = "SELECT Id, IdProy, IdTopico, IdCategorizer, Fases, MaxVotos FROM Voting";

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
                                IdTopico = reader["IdTopico"] == DBNull.Value ? (int?)null : (int)reader["IdTopico"],
                                IdCategorizer = reader["IdCategorizer"] == DBNull.Value ? (int?)null : (int)reader["IdCategorizer"],
                                Fases = (int)reader["Fases"],
                                MaxVotos = reader["MaxVotos"] == DBNull.Value ? (int?)null : (int)reader["MaxVotos"]
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
                string query = "SELECT Id, IdProy, IdTopico, IdCategorizer, Fases, MaxVotos FROM Voting WHERE Id = @Id";

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
                string query = "SELECT Id, IdProy, IdTopico, IdCategorizer, Fases, MaxVotos FROM Voting WHERE IdProy = @IdProy";

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
                                IdTopico = reader["IdTopico"] == DBNull.Value ? (int?)null : (int)reader["IdTopico"],
                                IdCategorizer = reader["IdCategorizer"] == DBNull.Value ? (int?)null : (int)reader["IdCategorizer"],
                                Fases = (int)reader["Fases"],
                                MaxVotos = reader["MaxVotos"] == DBNull.Value ? (int?)null : (int)reader["MaxVotos"]
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
            _logger.LogInformation("=== Creando nueva sesión de voting ===");
            _logger.LogInformation("Request: {@Request}", request);

            if (request == null)
            {
                _logger.LogWarning("Request es null");
                return BadRequest(new { error = "La solicitud no puede estar vacía" });
            }

            // Validar que IdProy sea válido
            if (request.IdProy <= 0)
            {
                _logger.LogWarning("IdProy inválido: {IdProy}", request.IdProy);
                return BadRequest(new { error = "El ID del proyecto debe ser mayor a 0" });
            }

            // Validar que Fases sea válido
            if (request.Fases <= 0)
            {
                _logger.LogWarning("Fases inválido: {Fases}", request.Fases);
                return BadRequest(new { error = "El número de fases debe ser mayor a 0" });
            }

            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                _logger.LogInformation("Conexión a base de datos establecida");

                // Verificar que el proyecto existe
                string checkProyectoQuery = "SELECT COUNT(*) FROM Proyectos WHERE Id = @IdProy";
                using (SqlCommand checkCmd = new SqlCommand(checkProyectoQuery, conn))
                {
                    checkCmd.Parameters.AddWithValue("@IdProy", request.IdProy);
                    int proyectoExists = (int)checkCmd.ExecuteScalar();
                    
                    if (proyectoExists == 0)
                    {
                        _logger.LogError("Proyecto con ID {IdProy} no existe", request.IdProy);
                        return BadRequest(new { error = $"El proyecto con ID {request.IdProy} no existe" });
                    }
                }

                string query = @"INSERT INTO Voting (IdProy, IdTopico, IdCategorizer, Fases, MaxVotos) 
                               VALUES (@IdProy, @IdTopico, @IdCategorizer, @Fases, @MaxVotos);
                               SELECT SCOPE_IDENTITY();";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@IdProy", request.IdProy);
                    cmd.Parameters.AddWithValue("@IdTopico", request.IdTopico ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@IdCategorizer", request.IdCategorizer ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@Fases", request.Fases);
                    cmd.Parameters.AddWithValue("@MaxVotos", request.MaxVotos ?? (object)DBNull.Value);

                    _logger.LogInformation("Ejecutando inserción de sesión de voting");
                    int newId = Convert.ToInt32(cmd.ExecuteScalar());
                    
                    _logger.LogInformation("Sesión de voting creada exitosamente con ID: {NewId}", newId);
                    return Ok(new { message = "Sesión de voting creada correctamente", id = newId });
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al crear sesión de voting: {Message}", ex.Message);
            _logger.LogError(ex, "Stack trace: {StackTrace}", ex.StackTrace);
            return StatusCode(500, new { error = ex.Message, details = ex.ToString() });
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
                               SET IdProy = @IdProy, IdTopico = @IdTopico, IdCategorizer = @IdCategorizer, Fases = @Fases, MaxVotos = @MaxVotos
                               WHERE Id = @Id";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@Id", id);
                    cmd.Parameters.AddWithValue("@IdProy", request.IdProy);
                    cmd.Parameters.AddWithValue("@IdTopico", request.IdTopico ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@IdCategorizer", request.IdCategorizer ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@Fases", request.Fases);
                    cmd.Parameters.AddWithValue("@MaxVotos", request.MaxVotos ?? (object)DBNull.Value);

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
    public IActionResult GetVotos(int idVoting, [FromQuery] int? fase = null)
    {
        var response = new VotosResponse();

        using (SqlConnection conn = new SqlConnection(_connectionString))
        {
            try
            {
                conn.Open();
                
                string query = @"SELECT vv.Id, vv.IdVoting, vv.Fase, vv.IdCategoria, vv.IdUsuario, u.Nombre
                               FROM Votos_voting vv
                               INNER JOIN Usuarios u ON vv.IdUsuario = u.Id
                               WHERE vv.IdVoting = @IdVoting";
                
                // Agregar filtro por fase si se especifica
                if (fase.HasValue)
                {
                    query += " AND vv.Fase = @Fase";
                }

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@IdVoting", idVoting);
                    
                    if (fase.HasValue)
                    {
                        cmd.Parameters.AddWithValue("@Fase", fase.Value);
                        _logger.LogInformation("Obteniendo votos para IdVoting {IdVoting} y Fase {Fase}", idVoting, fase.Value);
                    }
                    else
                    {
                        _logger.LogInformation("Obteniendo todos los votos para IdVoting {IdVoting}", idVoting);
                    }

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            response.votos.Add(new VotoItem
                            {
                                Id = (int)reader["Id"],
                                IdVoting = (int)reader["IdVoting"],
                                Fase = (int)reader["Fase"],
                                IdCategoria = (int)reader["IdCategoria"],
                                IdUsuario = (int)reader["IdUsuario"],
                                NombreUsuario = reader["Nombre"].ToString()
                            });
                        }
                    }
                }

                _logger.LogInformation("Se obtuvieron {Count} votos", response.votos.Count);
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener votos para IdVoting {IdVoting} y Fase {Fase}", idVoting, fase);
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
                
                // Verificar si el usuario ya votó por esta categoría en esta fase
                string checkQuery = @"SELECT COUNT(*) FROM Votos_voting 
                                    WHERE IdVoting = @IdVoting AND Fase = @Fase AND IdCategoria = @IdCategoria AND IdUsuario = @IdUsuario";
                
                using (SqlCommand checkCmd = new SqlCommand(checkQuery, conn))
                {
                    checkCmd.Parameters.AddWithValue("@IdVoting", idVoting);
                    checkCmd.Parameters.AddWithValue("@Fase", request.Fase);
                    checkCmd.Parameters.AddWithValue("@IdCategoria", request.IdCategoria);
                    checkCmd.Parameters.AddWithValue("@IdUsuario", request.IdUsuario);

                    int existingVotes = (int)checkCmd.ExecuteScalar();
                    
                    if (existingVotes > 0)
                    {
                        return BadRequest(new { error = "El usuario ya votó por esta categoría en esta fase" });
                    }
                }

                // Insertar el voto
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
                string query = "DELETE FROM Votos_voting WHERE Id = @Id";

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

    // Iniciar nueva fase de voting
    [HttpPost("{idVoting}/nueva-fase")]
    public IActionResult IniciarNuevaFase(int idVoting)
    {
        try
        {
            _logger.LogInformation("=== Iniciando nueva fase de voting ===");
            _logger.LogInformation("IdVoting: {IdVoting}", idVoting);

            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                
                // Iniciar transacción para asegurar consistencia
                using (var transaction = conn.BeginTransaction())
                {
                    try
                    {
                        // Primero verificar que el IdVoting existe y obtener las fases actuales
                        string checkQuery = "SELECT Fases FROM Voting WHERE Id = @IdVoting";
                        int fasesActuales;
                        using (SqlCommand checkCmd = new SqlCommand(checkQuery, conn, transaction))
                        {
                            checkCmd.Parameters.AddWithValue("@IdVoting", idVoting);
                            var result = checkCmd.ExecuteScalar();
                            
                            if (result == null)
                            {
                                _logger.LogError("IdVoting {IdVoting} no existe", idVoting);
                                return NotFound(new { error = $"No se encontró la sesión de voting con ID {idVoting}" });
                            }
                            
                            fasesActuales = (int)result;
                        }

                        // Incrementar las fases en 1
                        int nuevaFase = fasesActuales + 1;
                        
                        // Actualizar el campo Fases en la tabla Voting
                        string updateQuery = "UPDATE Voting SET Fases = @NuevaFase WHERE Id = @IdVoting";
                        using (SqlCommand updateCmd = new SqlCommand(updateQuery, conn, transaction))
                        {
                            updateCmd.Parameters.AddWithValue("@IdVoting", idVoting);
                            updateCmd.Parameters.AddWithValue("@NuevaFase", nuevaFase);
                            
                            int rowsAffected = updateCmd.ExecuteNonQuery();
                            if (rowsAffected == 0)
                            {
                                throw new Exception("No se pudo actualizar el número de fases");
                            }
                        }

                        // Confirmar la transacción
                        transaction.Commit();

                        _logger.LogInformation("Nueva fase de voting iniciada exitosamente. Fase anterior: {FaseAnterior}, Nueva fase: {NuevaFase}. Los votos no se duplican entre fases.", 
                            fasesActuales, nuevaFase);

                        return Ok(new { 
                            message = "Nueva fase de voting iniciada correctamente. Los votos de la fase anterior no se duplican.", 
                            idVoting = idVoting,
                            faseAnterior = fasesActuales,
                            nuevaFase = nuevaFase,
                            nota = "Cada fase de votación es independiente - los votos no se transfieren entre fases"
                        });
                    }
                    catch (Exception ex)
                    {
                        // Si hay error, hacer rollback de la transacción
                        transaction.Rollback();
                        throw ex;
                    }
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al iniciar nueva fase para IdVoting {IdVoting}: {Message}", idVoting, ex.Message);
            return StatusCode(500, new { error = ex.Message });
        }
    }

    // Verificar si un usuario ya votó en una fase específica
    [HttpGet("{idVoting}/check-voto/{idUsuario}/{fase}")]
    public IActionResult CheckVotoUsuario(int idVoting, int idUsuario, int fase)
    {
        using (SqlConnection conn = new SqlConnection(_connectionString))
        {
            try
            {
                conn.Open();
                string query = @"SELECT COUNT(*) as VotoCount
                               FROM Votos_voting 
                               WHERE IdVoting = @IdVoting AND IdUsuario = @IdUsuario AND Fase = @Fase";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@IdVoting", idVoting);
                    cmd.Parameters.AddWithValue("@IdUsuario", idUsuario);
                    cmd.Parameters.AddWithValue("@Fase", fase);

                    int votoCount = (int)cmd.ExecuteScalar();
                    bool hasVoted = votoCount > 0;

                    return Ok(new { 
                        hasVoted = hasVoted,
                        votoCount = votoCount,
                        message = hasVoted ? 
                            $"El usuario {idUsuario} ya votó {votoCount} vez(es) en la fase {fase}" : 
                            $"El usuario {idUsuario} no ha votado en la fase {fase}"
                    });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al verificar voto de usuario {IdUsuario} en fase {Fase}", idUsuario, fase);
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
    public int? IdTopico { get; set; }
    public int? IdCategorizer { get; set; }
    public int Fases { get; set; }
    public int? MaxVotos { get; set; }
}

public class VotingSessionRequest
{
    public int IdProy { get; set; }
    public int? IdTopico { get; set; }
    public int? IdCategorizer { get; set; }
    public int Fases { get; set; }
    public int? MaxVotos { get; set; }
}

public class VotingSessionsResponse
{
    public List<VotingSession> sessions { get; set; } = new List<VotingSession>();
}

// Clases para votos
public class VotoItem
{
    public int Id { get; set; }
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