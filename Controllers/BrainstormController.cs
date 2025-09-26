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
        _connectionString = @"Data Source=LAPTOP-7MITNTQF\SQLEXPRESS;Initial Catalog=Herramientas;User ID=sa;Password=admin;Encrypt=True;TrustServerCertificate=True";
        _logger = logger;
    }

    [HttpGet("ideas/{idProy}")]
    public IActionResult GetIdeas(int idProy)
    {
        var response = new BrainstormResponse
        {
            ideas = new List<BrainstormIdea>()
        };

        using (SqlConnection conn = new SqlConnection(_connectionString))
        {
            try
            {
                conn.Open();
                string query = @"SELECT Id, IdProy, IdUsuario, Idea, Nombre
                               FROM Brainstorm 
                               WHERE IdProy = @IdProy";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@IdProy", idProy);

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            response.ideas.Add(new BrainstormIdea
                            {
                                Id = (int)reader["Id"],
                                IdProy = (int)reader["IdProy"],
                                IdUsuario = (int)reader["IdUsuario"],
                                Idea = reader["Idea"].ToString(),
                                Nombre = reader["Nombre"].ToString()
                            });
                        }
                    }
                }

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener ideas");
                return StatusCode(500, new { error = ex.Message });
            }
        }
    }

    [HttpGet("ideas/{idProy}/{idUsuario}")]
    public IActionResult GetIdeasPorUsuario(int idProy, int idUsuario)
    {
        var response = new BrainstormResponse
        {
            ideas = new List<BrainstormIdea>()
        };

        using (SqlConnection conn = new SqlConnection(_connectionString))
        {
            try
            {
                conn.Open();
                string query = @"SELECT Id, IdProy, IdUsuario, Idea, Nombre
                             FROM Brainstorm
                             WHERE IdProy = @IdProy AND IdUsuario = @IdUsuario";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@IdProy", idProy);
                    cmd.Parameters.AddWithValue("@IdUsuario", idUsuario);

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            response.ideas.Add(new BrainstormIdea
                            {
                                Id = (int)reader["Id"],
                                IdProy = (int)reader["IdProy"],
                                IdUsuario = (int)reader["IdUsuario"],
                                Idea = reader["Idea"].ToString(),
                                Nombre = reader["Nombre"].ToString()
                            });
                        }
                    }
                }

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener ideas por usuario");
                return StatusCode(500, new { error = ex.Message });
            }
        }
    }

    [HttpPost("agregar")]
    public IActionResult AgregarIdea([FromBody] BrainstormRequest request)
    {
        try
        {
            _logger.LogInformation("Iniciando AgregarIdea con request: {@Request}", request);

            if (request == null)
            {
                _logger.LogWarning("Request es null");
                return BadRequest(new { error = "La solicitud no puede estar vacía" });
            }

            if (string.IsNullOrEmpty(request.Idea))
            {
                _logger.LogWarning("Campo Idea está vacío");
                return BadRequest(new { error = "El campo Idea es requerido" });
            }

            if (string.IsNullOrEmpty(request.Nombre))
            {
                _logger.LogWarning("Campo Nombre está vacío");
                return BadRequest(new { error = "El campo Nombre es requerido" });
            }

            if (request.Nombre.Length > 20)
            {
                _logger.LogWarning("Nombre excede 20 caracteres: {Nombre}", request.Nombre);
                return BadRequest(new { error = "El nombre no puede exceder los 20 caracteres" });
            }

            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                _logger.LogInformation("Intentando abrir conexión a la base de datos");
                conn.Open();
                
                string query = @"INSERT INTO Brainstorm (IdProy, IdUsuario, Idea, Nombre) 
                               VALUES (@IdProy, @IdUsuario, @Idea, @Nombre);
                               SELECT SCOPE_IDENTITY();";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    _logger.LogInformation("Configurando parámetros: IdProy={IdProy}, IdUsuario={IdUsuario}, Idea={Idea}, Nombre={Nombre}", 
                        request.IdProy, request.IdUsuario, request.Idea, request.Nombre);

                    cmd.Parameters.AddWithValue("@IdProy", request.IdProy);
                    cmd.Parameters.AddWithValue("@IdUsuario", request.IdUsuario);
                    cmd.Parameters.AddWithValue("@Idea", request.Idea);
                    cmd.Parameters.AddWithValue("@Nombre", request.Nombre);

                    _logger.LogInformation("Ejecutando comando SQL");
                    int newId = Convert.ToInt32(cmd.ExecuteScalar());
                    _logger.LogInformation("Idea agregada exitosamente con ID: {Id}", newId);
                    return Ok(new { message = "Idea agregada correctamente", id = newId });
                }
            }
        }
        catch (SqlException ex)
        {
            _logger.LogError(ex, "Error de SQL al agregar idea");
            return StatusCode(500, new { error = "Error de base de datos: " + ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error inesperado al agregar idea");
            return StatusCode(500, new { error = "Error al agregar la idea: " + ex.Message });
        }
    }

    [HttpDelete("eliminar/{id}")]
    public IActionResult EliminarIdea(int id)
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
                    cmd.ExecuteNonQuery();
                }

                return Ok(new { message = "Idea eliminada correctamente" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al eliminar idea");
                return StatusCode(500, new { error = ex.Message });
            }
        }
    }

    [HttpPut("editar/{id}")]
    public IActionResult EditarIdea(int id, [FromBody] BrainstormRequest request)
    {
        try
        {
            _logger.LogInformation("Iniciando EditarIdea para ID: {Id} con request: {@Request}", id, request);

            if (request == null)
            {
                _logger.LogWarning("Request para editar es null");
                return BadRequest(new { error = "La solicitud no puede estar vacía" });
            }

            // Opcional: Validar si los campos a editar son nulos o vacíos si no se permite editarlos a vacío
            // if (string.IsNullOrEmpty(request.Idea) && string.IsNullOrEmpty(request.Nombre))
            // {
            //     return BadRequest(new { error = "Se requiere al menos el campo Idea o Nombre para editar" });
            // }

            if (request.Nombre != null && request.Nombre.Length > 20)
            {
                _logger.LogWarning("Nombre excede 20 caracteres en edición: {Nombre}", request.Nombre);
                return BadRequest(new { error = "El nombre no puede exceder los 20 caracteres" });
            }


            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                _logger.LogInformation("Intentando abrir conexión a la base de datos para editar");
                conn.Open();

                // Construir la consulta de UPDATE dinámicamente si solo queremos actualizar los campos que vienen en el request
                // O actualizar todos los campos Idea y Nombre que vienen en el request
                string query = @"UPDATE Brainstorm
                               SET Idea = @Idea, Nombre = @Nombre
                               WHERE Id = @Id";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@Id", id);
                    cmd.Parameters.AddWithValue("@Idea", request.Idea ?? (object)DBNull.Value); // Manejar posibles nulos si la columna lo permite
                    cmd.Parameters.AddWithValue("@Nombre", request.Nombre ?? (object)DBNull.Value); // Manejar posibles nulos si la columna lo permite

                    _logger.LogInformation("Ejecutando comando SQL para editar ID: {Id}", id);
                    int rowsAffected = cmd.ExecuteNonQuery();

                    if (rowsAffected == 0)
                    {
                        _logger.LogWarning("No se encontró idea con ID: {Id} para editar", id);
                        return NotFound(new { error = $"No se encontró la idea con ID {id}" });
                    }

                    _logger.LogInformation("Idea con ID: {Id} editada correctamente. Filas afectadas: {RowsAffected}", id, rowsAffected);
                    return Ok(new { message = "Idea editada correctamente" });
                }
            }
        }
        catch (SqlException ex)
        {
            _logger.LogError(ex, "Error de SQL al editar idea con ID: {Id}", id);
            return StatusCode(500, new { error = "Error de base de datos: " + ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error inesperado al editar idea con ID: {Id}", id);
            return StatusCode(500, new { error = "Error al editar la idea: " + ex.Message });
        }
    }
}

public class BrainstormRequest
{
    public int IdProy { get; set; }
    public int IdUsuario { get; set; }
    public string Idea { get; set; }
    public string Nombre { get; set; }
}

public class BrainstormResponse
{
    public List<BrainstormIdea> ideas { get; set; }
}

public class BrainstormIdea
{
    public int Id { get; set; }
    public int IdProy { get; set; }
    public int IdUsuario { get; set; }
    public string Idea { get; set; }
    public string Nombre { get; set; }
} 