using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;

/// <summary>
/// Controlador para gestionar la relación N:M entre Proyectos y Usuarios.
/// Se mantiene separado porque maneja específicamente la tabla de relación Proyectos_Usuarios
/// con funcionalidades como obtener usuarios por proyecto, proyectos por usuario, y gestionar roles.
/// También permite queries especializadas sobre estas relaciones que serían más complejas
/// si estuvieran integradas en ProyectoController o UsuarioController.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class ProyectosUsuariosController : ControllerBase
{
    private readonly string _connectionString;
    private readonly ILogger<ProyectosUsuariosController> _logger;

    public ProyectosUsuariosController(ILogger<ProyectosUsuariosController> logger)
    {
        _connectionString = @"Data Source=189.195.162.46;Initial Catalog=HerramientasV3;User ID=sa;Password=sqlSA%;Encrypt=True;TrustServerCertificate=True";
        _logger = logger;
    }

    // Obtener todas las relaciones proyecto-usuario
    [HttpGet]
    public IActionResult GetAllProyectosUsuarios()
    {
        var response = new ProyectosUsuariosResponse();

        using (SqlConnection conn = new SqlConnection(_connectionString))
        {
            try
            {
                conn.Open();
                string query = @"SELECT pu.Id, pu.IdProy, pu.IdUsu, pu.Rol, p.Nombre as NombreProyecto, u.Nombre as NombreUsuario
                               FROM Proyectos_Usuarios pu
                               INNER JOIN Proyectos p ON pu.IdProy = p.Id
                               INNER JOIN Usuarios u ON pu.IdUsu = u.Id";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            response.proyectosUsuarios.Add(new ProyectoUsuarioItem
                            {
                                Id = (int)reader["Id"],
                                IdProy = (int)reader["IdProy"],
                                IdUsu = (int)reader["IdUsu"],
                                Rol = reader["Rol"]?.ToString(),
                                NombreProyecto = reader["NombreProyecto"].ToString(),
                                NombreUsuario = reader["NombreUsuario"].ToString()
                            });
                        }
                    }
                }

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener todas las relaciones proyecto-usuario");
                return StatusCode(500, new { error = ex.Message });
            }
        }
    }

    // Obtener relación proyecto-usuario por ID
    [HttpGet("{id}")]
    public IActionResult GetProyectoUsuarioById(int id)
    {
        using (SqlConnection conn = new SqlConnection(_connectionString))
        {
            try
            {
                conn.Open();
                string query = @"SELECT pu.Id, pu.IdProy, pu.IdUsu, pu.Rol, p.Nombre as NombreProyecto, u.Nombre as NombreUsuario
                               FROM Proyectos_Usuarios pu
                               INNER JOIN Proyectos p ON pu.IdProy = p.Id
                               INNER JOIN Usuarios u ON pu.IdUsu = u.Id
                               WHERE pu.Id = @Id";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@Id", id);

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            var proyectoUsuario = new ProyectoUsuarioItem
                            {
                                Id = (int)reader["Id"],
                                IdProy = (int)reader["IdProy"],
                                IdUsu = (int)reader["IdUsu"],
                                Rol = reader["Rol"]?.ToString(),
                                NombreProyecto = reader["NombreProyecto"].ToString(),
                                NombreUsuario = reader["NombreUsuario"].ToString()
                            };
                            return Ok(proyectoUsuario);
                        }
                        else
                        {
                            return NotFound(new { error = $"No se encontró la relación proyecto-usuario con ID {id}" });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener relación proyecto-usuario por ID: {Id}", id);
                return StatusCode(500, new { error = ex.Message });
            }
        }
    }

    // Obtener usuarios por proyecto
    [HttpGet("byproy/{idProy}")]
    public IActionResult GetUsuariosByProy(int idProy)
    {
        var response = new ProyectosUsuariosResponse();

        using (SqlConnection conn = new SqlConnection(_connectionString))
        {
            try
            {
                conn.Open();
                string query = @"SELECT pu.Id, pu.IdProy, pu.IdUsu, pu.Rol, u.Nombre as NombreUsuario
                               FROM Proyectos_Usuarios pu
                               INNER JOIN Usuarios u ON pu.IdUsu = u.Id
                               WHERE pu.IdProy = @IdProy";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@IdProy", idProy);

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            response.proyectosUsuarios.Add(new ProyectoUsuarioItem
                            {
                                Id = (int)reader["Id"],
                                IdProy = idProy,
                                IdUsu = (int)reader["IdUsu"],
                                Rol = reader["Rol"]?.ToString(),
                                NombreUsuario = reader["NombreUsuario"].ToString()
                            });
                        }
                    }
                }

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener usuarios por proyecto: {IdProy}", idProy);
                return StatusCode(500, new { error = ex.Message });
            }
        }
    }

    // Obtener proyectos por usuario
    [HttpGet("byusu/{idUsu}")]
    public IActionResult GetProyectosByUsu(int idUsu)
    {
        var response = new ProyectosUsuariosResponse();

        using (SqlConnection conn = new SqlConnection(_connectionString))
        {
            try
            {
                conn.Open();
                string query = @"SELECT pu.Id, pu.IdProy, pu.IdUsu, pu.Rol, p.Nombre as NombreProyecto
                               FROM Proyectos_Usuarios pu
                               INNER JOIN Proyectos p ON pu.IdProy = p.Id
                               WHERE pu.IdUsu = @IdUsu";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@IdUsu", idUsu);

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            response.proyectosUsuarios.Add(new ProyectoUsuarioItem
                            {
                                Id = (int)reader["Id"],
                                IdProy = (int)reader["IdProy"],
                                IdUsu = idUsu,
                                Rol = reader["Rol"]?.ToString(),
                                NombreProyecto = reader["NombreProyecto"].ToString()
                            });
                        }
                    }
                }

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener proyectos por usuario: {IdUsu}", idUsu);
                return StatusCode(500, new { error = ex.Message });
            }
        }
    }

    // Crear nueva relación proyecto-usuario
    [HttpPost]
    public IActionResult CreateProyectoUsuario([FromBody] ProyectoUsuarioRequest request)
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
                
                // Verificar si ya existe la relación
                string checkQuery = "SELECT COUNT(*) FROM Proyectos_Usuarios WHERE IdProy = @IdProy AND IdUsu = @IdUsu";
                using (SqlCommand checkCmd = new SqlCommand(checkQuery, conn))
                {
                    checkCmd.Parameters.AddWithValue("@IdProy", request.IdProy);
                    checkCmd.Parameters.AddWithValue("@IdUsu", request.IdUsu);
                    
                    if ((int)checkCmd.ExecuteScalar() > 0)
                    {
                        return BadRequest(new { error = "Ya existe una relación entre este proyecto y usuario" });
                    }
                }

                string query = @"INSERT INTO Proyectos_Usuarios (IdProy, IdUsu, Rol) 
                               VALUES (@IdProy, @IdUsu, @Rol);
                               SELECT SCOPE_IDENTITY();";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@IdProy", request.IdProy);
                    cmd.Parameters.AddWithValue("@IdUsu", request.IdUsu);
                    cmd.Parameters.AddWithValue("@Rol", request.Rol ?? (object)DBNull.Value);

                    int newId = Convert.ToInt32(cmd.ExecuteScalar());
                    return Ok(new { message = "Relación proyecto-usuario creada correctamente", id = newId });
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al crear relación proyecto-usuario");
            return StatusCode(500, new { error = ex.Message });
        }
    }

    // Actualizar relación proyecto-usuario
    [HttpPut("{id}")]
    public IActionResult UpdateProyectoUsuario(int id, [FromBody] ProyectoUsuarioRequest request)
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
                string query = @"UPDATE Proyectos_Usuarios
                               SET IdProy = @IdProy, IdUsu = @IdUsu, Rol = @Rol
                               WHERE Id = @Id";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@Id", id);
                    cmd.Parameters.AddWithValue("@IdProy", request.IdProy);
                    cmd.Parameters.AddWithValue("@IdUsu", request.IdUsu);
                    cmd.Parameters.AddWithValue("@Rol", request.Rol ?? (object)DBNull.Value);

                    int rowsAffected = cmd.ExecuteNonQuery();

                    if (rowsAffected == 0)
                    {
                        return NotFound(new { error = $"No se encontró la relación proyecto-usuario con ID {id} para actualizar" });
                    }

                    return Ok(new { message = "Relación proyecto-usuario actualizada correctamente" });
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al actualizar relación proyecto-usuario con ID: {Id}", id);
            return StatusCode(500, new { error = ex.Message });
        }
    }

    // Eliminar relación proyecto-usuario
    [HttpDelete("{id}")]
    public IActionResult DeleteProyectoUsuario(int id)
    {
        using (SqlConnection conn = new SqlConnection(_connectionString))
        {
            try
            {
                conn.Open();
                string query = "DELETE FROM Proyectos_Usuarios WHERE Id = @Id";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@Id", id);
                    int rowsAffected = cmd.ExecuteNonQuery();

                    if (rowsAffected == 0)
                    {
                        return NotFound(new { error = $"No se encontró la relación proyecto-usuario con ID {id} para eliminar" });
                    }

                    return Ok(new { message = "Relación proyecto-usuario eliminada correctamente" });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al eliminar relación proyecto-usuario con ID: {Id}", id);
                return StatusCode(500, new { error = ex.Message });
            }
        }
    }
}

public class ProyectoUsuarioItem
{
    public int Id { get; set; }
    public int IdProy { get; set; }
    public int IdUsu { get; set; }
    public string? Rol { get; set; }
    public string? NombreProyecto { get; set; }
    public string? NombreUsuario { get; set; }
}

public class ProyectoUsuarioRequest
{
    public int IdProy { get; set; }
    public int IdUsu { get; set; }
    public string? Rol { get; set; }
}

public class ProyectosUsuariosResponse
{
    public List<ProyectoUsuarioItem> proyectosUsuarios { get; set; } = new List<ProyectoUsuarioItem>();
}

