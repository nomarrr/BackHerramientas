using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;

[ApiController]
[Route("api/[controller]")]
public class ProyectoController : ControllerBase
{
    private readonly string _connectionString;
    private readonly ILogger<ProyectoController> _logger;

    public ProyectoController(ILogger<ProyectoController> logger)
    {
        _connectionString = @"Data Source=189.195.162.46;Initial Catalog=HerramientasV3;User ID=sa;Password=sqlSA%;Encrypt=True;TrustServerCertificate=True";
        _logger = logger;
    }

    // Obtener todos los proyectos
    [HttpGet]
    public IActionResult GetAllProyectos()
    {
        var response = new ProyectosResponse();

        using (SqlConnection conn = new SqlConnection(_connectionString))
        {
            try
            {
                conn.Open();
                string query = "SELECT Id, Nombre, Objetivo, FechaInicio, FechaFin, Estatus FROM Proyectos";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            response.proyectos.Add(new ProyectoItem
                            {
                                Id = (int)reader["Id"],
                                Nombre = reader["Nombre"].ToString(),
                                Objetivo = reader["Objetivo"].ToString(),
                                FechaInicio = reader["FechaInicio"] == DBNull.Value ? (DateTime?)null : (DateTime)reader["FechaInicio"],
                                FechaFin = reader["FechaFin"] == DBNull.Value ? (DateTime?)null : (DateTime)reader["FechaFin"],
                                Estatus = (int)reader["Estatus"]
                            });
                        }
                    }
                }

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener todos los proyectos");
                return StatusCode(500, new { error = ex.Message });
            }
        }
    }

    // Obtener proyecto por ID
    [HttpGet("{id}")]
    public IActionResult GetProyectoById(int id)
    {
        using (SqlConnection conn = new SqlConnection(_connectionString))
        {
            try
            {
                conn.Open();
                string query = "SELECT Id, Nombre, Objetivo, FechaInicio, FechaFin, Estatus FROM Proyectos WHERE Id = @Id";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@Id", id);

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            var proyecto = new ProyectoItem
                            {
                                Id = (int)reader["Id"],
                                Nombre = reader["Nombre"].ToString(),
                                Objetivo = reader["Objetivo"].ToString(),
                                FechaInicio = reader["FechaInicio"] == DBNull.Value ? (DateTime?)null : (DateTime)reader["FechaInicio"],
                                FechaFin = reader["FechaFin"] == DBNull.Value ? (DateTime?)null : (DateTime)reader["FechaFin"],
                                Estatus = (int)reader["Estatus"]
                            };
                            return Ok(proyecto);
                        }
                        else
                        {
                            return NotFound(new { error = $"No se encontró el proyecto con ID {id}" });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener proyecto por ID: {Id}", id);
                return StatusCode(500, new { error = ex.Message });
            }
        }
    }

    // Crear nuevo proyecto
    [HttpPost]
    public IActionResult CreateProyecto([FromBody] ProyectoRequest request)
    {
        try
        {
            if (request == null)
            {
                return BadRequest(new { error = "La solicitud no puede estar vacía" });
            }

            if (string.IsNullOrEmpty(request.Nombre))
            {
                return BadRequest(new { error = "El nombre del proyecto es requerido" });
            }

            if (string.IsNullOrEmpty(request.Objetivo))
            {
                return BadRequest(new { error = "El objetivo del proyecto es requerido" });
            }

            if (request.IdUsu <= 0)
            {
                return BadRequest(new { error = "El ID del usuario es requerido" });
            }

            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                
                // Iniciar transacción para asegurar que tanto el proyecto como la relación proyecto-usuario se creen
                using (var transaction = conn.BeginTransaction())
                {
                    try
                    {
                        // Crear el proyecto
                        string queryProyecto = @"INSERT INTO Proyectos (Nombre, Objetivo, FechaInicio, FechaFin, Estatus) 
                                               VALUES (@Nombre, @Objetivo, @FechaInicio, @FechaFin, @Estatus);
                                               SELECT SCOPE_IDENTITY();";

                        int proyectoId;
                        using (SqlCommand cmdProyecto = new SqlCommand(queryProyecto, conn, transaction))
                        {
                            cmdProyecto.Parameters.AddWithValue("@Nombre", request.Nombre);
                            cmdProyecto.Parameters.AddWithValue("@Objetivo", request.Objetivo);
                            cmdProyecto.Parameters.AddWithValue("@FechaInicio", request.FechaInicio ?? (object)DBNull.Value);
                            cmdProyecto.Parameters.AddWithValue("@FechaFin", request.FechaFin ?? (object)DBNull.Value);
                            cmdProyecto.Parameters.AddWithValue("@Estatus", request.Estatus);

                            proyectoId = Convert.ToInt32(cmdProyecto.ExecuteScalar());
                        }

                        // Insertar en Proyectos_Usuarios con rol FAC (Facilitador)
                        string queryProyectoUsuario = @"INSERT INTO Proyectos_Usuarios (IdProy, IdUsu, Rol) 
                                                      VALUES (@IdProy, @IdUsu, @Rol);";

                        using (SqlCommand cmdProyectoUsuario = new SqlCommand(queryProyectoUsuario, conn, transaction))
                        {
                            cmdProyectoUsuario.Parameters.AddWithValue("@IdProy", proyectoId);
                            cmdProyectoUsuario.Parameters.AddWithValue("@IdUsu", request.IdUsu);
                            cmdProyectoUsuario.Parameters.AddWithValue("@Rol", "FAC");

                            cmdProyectoUsuario.ExecuteNonQuery();
                        }

                        // En V3 ya no se crean automáticamente Categorizer y Voting
                        // Se crean desde la tabla Agenda según el tema/tópico

                        // Confirmar la transacción
                        transaction.Commit();

                        _logger.LogInformation("Proyecto creado exitosamente con ID: {ProyectoId}", proyectoId);

                        return Ok(new { 
                            message = "Proyecto creado correctamente", 
                            id = proyectoId
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
            _logger.LogError(ex, "Error al crear proyecto");
            return StatusCode(500, new { error = ex.Message });
        }
    }

    // Actualizar proyecto existente
    [HttpPut("{id}")]
    public IActionResult UpdateProyecto(int id, [FromBody] ProyectoRequest request)
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
                string query = @"UPDATE Proyectos
                               SET Nombre = @Nombre, Objetivo = @Objetivo, FechaInicio = @FechaInicio, FechaFin = @FechaFin, Estatus = @Estatus
                               WHERE Id = @Id";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@Id", id);
                    cmd.Parameters.AddWithValue("@Nombre", request.Nombre ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@Objetivo", request.Objetivo ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@FechaInicio", request.FechaInicio ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@FechaFin", request.FechaFin ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@Estatus", request.Estatus);

                    int rowsAffected = cmd.ExecuteNonQuery();

                    if (rowsAffected == 0)
                    {
                        return NotFound(new { error = $"No se encontró el proyecto con ID {id} para actualizar" });
                    }

                    return Ok(new { message = "Proyecto actualizado correctamente" });
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al actualizar proyecto con ID: {Id}", id);
            return StatusCode(500, new { error = ex.Message });
        }
    }

    // Eliminar proyecto
    [HttpDelete("{id}")]
    public IActionResult DeleteProyecto(int id)
    {
        using (SqlConnection conn = new SqlConnection(_connectionString))
        {
            try
            {
                conn.Open();
                string query = "DELETE FROM Proyectos WHERE Id = @Id";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@Id", id);
                    int rowsAffected = cmd.ExecuteNonQuery();

                    if (rowsAffected == 0)
                    {
                        return NotFound(new { error = $"No se encontró el proyecto con ID {id} para eliminar" });
                    }

                    return Ok(new { message = "Proyecto eliminado correctamente" });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al eliminar proyecto con ID: {Id}", id);
                return StatusCode(500, new { error = ex.Message });
            }
        }
    }

    // Obtener estadísticas del proyecto
    [HttpGet("{id}/estadisticas")]
    public IActionResult GetEstadisticasProyecto(int id)
    {
        var estadisticas = new ProyectoEstadisticas();

        using (SqlConnection conn = new SqlConnection(_connectionString))
        {
            try
            {
                conn.Open();

                // Contar sesiones de brainstorm
                string queryBrainstorm = "SELECT COUNT(*) FROM Brainstorming WHERE IdProy = @Id";
                using (SqlCommand cmd = new SqlCommand(queryBrainstorm, conn))
                {
                    cmd.Parameters.AddWithValue("@Id", id);
                    estadisticas.TotalBrainstorms = Convert.ToInt32(cmd.ExecuteScalar());
                }

                // Contar sesiones de categorizer
                string queryCategorizer = "SELECT COUNT(*) FROM Categorizer WHERE IdProy = @Id";
                using (SqlCommand cmd = new SqlCommand(queryCategorizer, conn))
                {
                    cmd.Parameters.AddWithValue("@Id", id);
                    estadisticas.TotalCategorizers = Convert.ToInt32(cmd.ExecuteScalar());
                }

                // Contar sesiones de chat
                string queryChat = "SELECT COUNT(*) FROM Chat WHERE IdProy = @Id";
                using (SqlCommand cmd = new SqlCommand(queryChat, conn))
                {
                    cmd.Parameters.AddWithValue("@Id", id);
                    estadisticas.TotalChats = Convert.ToInt32(cmd.ExecuteScalar());
                }

                // Contar comentarios
                string queryComentarios = "SELECT COUNT(*) FROM Comenter WHERE IdProy = @Id";
                using (SqlCommand cmd = new SqlCommand(queryComentarios, conn))
                {
                    cmd.Parameters.AddWithValue("@Id", id);
                    estadisticas.TotalComentarios = Convert.ToInt32(cmd.ExecuteScalar());
                }

                // Contar sesiones de voting
                string queryVoting = "SELECT COUNT(*) FROM Voting WHERE IdProy = @Id";
                using (SqlCommand cmd = new SqlCommand(queryVoting, conn))
                {
                    cmd.Parameters.AddWithValue("@Id", id);
                    estadisticas.TotalVotings = Convert.ToInt32(cmd.ExecuteScalar());
                }

                return Ok(estadisticas);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener estadísticas del proyecto con ID: {Id}", id);
                return StatusCode(500, new { error = ex.Message });
            }
        }
    }
}

public class ProyectoItem
{
    public int Id { get; set; }
    public string Nombre { get; set; }
    public string Objetivo { get; set; }
    public DateTime? FechaInicio { get; set; }
    public DateTime? FechaFin { get; set; }
    public int Estatus { get; set; } // 1=Activo, 2=Cerrado
}

public class ProyectoRequest
{
    public string Nombre { get; set; }
    public string Objetivo { get; set; }
    public DateTime? FechaInicio { get; set; }
    public DateTime? FechaFin { get; set; }
    public int Estatus { get; set; }
    public int IdUsu { get; set; } // ID del usuario que crea el proyecto
}

public class ProyectosResponse
{
    public List<ProyectoItem> proyectos { get; set; } = new List<ProyectoItem>();
}

public class ProyectoEstadisticas
{
    public int TotalBrainstorms { get; set; }
    public int TotalCategorizers { get; set; }
    public int TotalChats { get; set; }
    public int TotalComentarios { get; set; }
    public int TotalVotings { get; set; }
}
