using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;

[ApiController]
[Route("api/[controller]")]
public class CategorizerController : ControllerBase
{
    private readonly string _connectionString;
    private readonly ILogger<CategorizerController> _logger;

    public CategorizerController(ILogger<CategorizerController> logger)
    {
        _connectionString = "Server=.;Database=HerramientasV3;Integrated Security=True;TrustServerCertificate=True";
        _logger = logger;
    }

    // Obtener todas las sesiones de categorizer
    [HttpGet]
    public IActionResult GetAllCategorizers()
    {
        var response = new CategorizerSessionsResponse();

        using (SqlConnection conn = new SqlConnection(_connectionString))
        {
            try
            {
                conn.Open();
                string query = "SELECT Id, IdProy, IdTopico, IdBrainstorm, Fases FROM Categorizer";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            response.sessions.Add(new CategorizerSession
                            {
                                Id = (int)reader["Id"],
                                IdProy = (int)reader["IdProy"],
                                IdTopico = reader["IdTopico"] == DBNull.Value ? (int?)null : (int)reader["IdTopico"],
                                IdBrainstorm = reader["IdBrainstorm"] == DBNull.Value ? (int?)null : (int)reader["IdBrainstorm"],
                                Fases = (int)reader["Fases"]
                            });
                        }
                    }
                }

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener sesiones de categorizer");
                return StatusCode(500, new { error = ex.Message });
            }
        }
    }

    // Obtener sesión de categorizer por ID
    [HttpGet("{id}")]
    public IActionResult GetCategorizerById(int id)
    {
        using (SqlConnection conn = new SqlConnection(_connectionString))
        {
            try
            {
                conn.Open();
                string query = "SELECT Id, IdProy, IdTopico, IdBrainstorm, Fases FROM Categorizer WHERE Id = @Id";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@Id", id);

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            var session = new CategorizerSession
                            {
                                Id = (int)reader["Id"],
                                IdProy = (int)reader["IdProy"],
                                IdTopico = reader["IdTopico"] == DBNull.Value ? (int?)null : (int)reader["IdTopico"],
                                IdBrainstorm = reader["IdBrainstorm"] == DBNull.Value ? (int?)null : (int)reader["IdBrainstorm"],
                                Fases = (int)reader["Fases"]
                            };
                            return Ok(session);
                        }
                        else
                        {
                            return NotFound(new { error = $"No se encontró la sesión de categorizer con ID {id}" });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener sesión de categorizer por ID: {Id}", id);
                return StatusCode(500, new { error = ex.Message });
            }
        }
    }

    // Obtener sesiones de categorizer por proyecto
    [HttpGet("byproy/{idProy}")]
    public IActionResult GetCategorizersByProy(int idProy)
    {
        var response = new CategorizerSessionsResponse();

        using (SqlConnection conn = new SqlConnection(_connectionString))
        {
            try
            {
                conn.Open();
                string query = "SELECT Id, IdProy, IdTopico, IdBrainstorm, Fases FROM Categorizer WHERE IdProy = @IdProy";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@IdProy", idProy);

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            response.sessions.Add(new CategorizerSession
                            {
                                Id = (int)reader["Id"],
                                IdProy = (int)reader["IdProy"],
                                IdTopico = reader["IdTopico"] == DBNull.Value ? (int?)null : (int)reader["IdTopico"],
                                IdBrainstorm = reader["IdBrainstorm"] == DBNull.Value ? (int?)null : (int)reader["IdBrainstorm"],
                                Fases = (int)reader["Fases"]
                            });
                        }
                    }
                }

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener sesiones de categorizer por proyecto: {IdProy}", idProy);
                return StatusCode(500, new { error = ex.Message });
            }
        }
    }

    // Crear nueva sesión de categorizer
    [HttpPost]
    public IActionResult CreateCategorizer([FromBody] CategorizerSessionRequest request)
    {
        try
        {
            _logger.LogInformation("=== Creando nueva sesión de categorizer ===");
            _logger.LogInformation("Request: {@Request}", request);

            if (request == null)
            {
                _logger.LogWarning("Request es null");
                return BadRequest(new { error = "La solicitud no puede estar vacía" });
            }

            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                string query = @"INSERT INTO Categorizer (IdProy, IdTopico, IdBrainstorm, Fases) 
                               VALUES (@IdProy, @IdTopico, @IdBrainstorm, @Fases);
                               SELECT SCOPE_IDENTITY();";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@IdProy", request.IdProy);
                    cmd.Parameters.AddWithValue("@IdTopico", request.IdTopico ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@IdBrainstorm", request.IdBrainstorm ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@Fases", request.Fases);

                    _logger.LogInformation("Ejecutando query de inserción de sesión");
                    int newId = Convert.ToInt32(cmd.ExecuteScalar());
                    
                    _logger.LogInformation("Sesión de categorizer creada exitosamente con ID: {NewId}", newId);
                    return Ok(new { message = "Sesión de categorizer creada correctamente", id = newId });
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al crear sesión de categorizer: {Message}", ex.Message);
            return StatusCode(500, new { error = ex.Message });
        }
    }

    // Actualizar sesión de categorizer
    [HttpPut("{id}")]
    public IActionResult UpdateCategorizer(int id, [FromBody] CategorizerSessionRequest request)
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
                string query = @"UPDATE Categorizer
                               SET IdProy = @IdProy, IdTopico = @IdTopico, IdBrainstorm = @IdBrainstorm, Fases = @Fases
                               WHERE Id = @Id";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@Id", id);
                    cmd.Parameters.AddWithValue("@IdProy", request.IdProy);
                    cmd.Parameters.AddWithValue("@IdTopico", request.IdTopico ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@IdBrainstorm", request.IdBrainstorm ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@Fases", request.Fases);

                    int rowsAffected = cmd.ExecuteNonQuery();

                    if (rowsAffected == 0)
                    {
                        return NotFound(new { error = $"No se encontró la sesión de categorizer con ID {id}" });
                    }

                    return Ok(new { message = "Sesión de categorizer actualizada correctamente" });
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al actualizar sesión de categorizer con ID: {Id}", id);
            return StatusCode(500, new { error = ex.Message });
        }
    }

    // Eliminar sesión de categorizer
    [HttpDelete("{id}")]
    public IActionResult DeleteCategorizer(int id)
    {
        using (SqlConnection conn = new SqlConnection(_connectionString))
        {
            try
            {
                conn.Open();
                string query = "DELETE FROM Categorizer WHERE Id = @Id";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@Id", id);
                    int rowsAffected = cmd.ExecuteNonQuery();

                    if (rowsAffected == 0)
                    {
                        return NotFound(new { error = $"No se encontró la sesión de categorizer con ID {id}" });
                    }

                    return Ok(new { message = "Sesión de categorizer eliminada correctamente" });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al eliminar sesión de categorizer con ID: {Id}", id);
                return StatusCode(500, new { error = ex.Message });
            }
        }
    }

    // Endpoint de prueba para verificar la estructura de la tabla
    [HttpGet("test-structure")]
    public IActionResult TestStructure()
    {
        using (SqlConnection conn = new SqlConnection(_connectionString))
        {
            try
            {
                conn.Open();
                string query = @"SELECT TOP 1 * FROM Categorias_Categorizer";
                
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        var columns = new List<string>();
                        for (int i = 0; i < reader.FieldCount; i++)
                        {
                            columns.Add(reader.GetName(i));
                        }
                        return Ok(new { columns = columns });
                    }
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }
    }

    // Endpoint de prueba para verificar la estructura detallada de la tabla
    [HttpGet("test-table-info")]
    public IActionResult TestTableInfo()
    {
        using (SqlConnection conn = new SqlConnection(_connectionString))
        {
            try
            {
                conn.Open();
                string query = @"SELECT 
                    COLUMN_NAME, 
                    DATA_TYPE, 
                    IS_NULLABLE, 
                    CHARACTER_MAXIMUM_LENGTH
                    FROM INFORMATION_SCHEMA.COLUMNS 
                    WHERE TABLE_NAME = 'Categorias_Categorizer'";
                
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        var columns = new List<object>();
                        while (reader.Read())
                        {
                            columns.Add(new {
                                ColumnName = reader["COLUMN_NAME"].ToString(),
                                DataType = reader["DATA_TYPE"].ToString(),
                                IsNullable = reader["IS_NULLABLE"].ToString(),
                                MaxLength = reader["CHARACTER_MAXIMUM_LENGTH"] == DBNull.Value ? null : reader["CHARACTER_MAXIMUM_LENGTH"]
                            });
                        }
                        return Ok(new { tableInfo = columns });
                    }
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }
    }

    // Endpoint de prueba para verificar sesiones de categorizer
    [HttpGet("test-sessions/{idProy}")]
    public IActionResult TestSessions(int idProy)
    {
        try
        {
            _logger.LogInformation("=== Probando endpoint de sesiones ===");
            _logger.LogInformation("IdProy: {IdProy}", idProy);

            var response = new CategorizerSessionsResponse();

            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                string query = "SELECT Id, IdProy, IdTopico, IdBrainstorm, Fases FROM Categorizer WHERE IdProy = @IdProy";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@IdProy", idProy);

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            response.sessions.Add(new CategorizerSession
                            {
                                Id = (int)reader["Id"],
                                IdProy = (int)reader["IdProy"],
                                IdTopico = reader["IdTopico"] == DBNull.Value ? (int?)null : (int)reader["IdTopico"],
                                IdBrainstorm = reader["IdBrainstorm"] == DBNull.Value ? (int?)null : (int)reader["IdBrainstorm"],
                                Fases = (int)reader["Fases"]
                            });
                        }
                    }
                }

                _logger.LogInformation("Sesiones encontradas: {Count}", response.sessions.Count);
                return Ok(response);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error en test-sessions: {Message}", ex.Message);
            return StatusCode(500, new { error = ex.Message });
        }
    }

    // Obtener categorías por proyecto (nuevo endpoint para el frontend)
    [HttpGet("categoriesbyproy/{idProy}")]
    public IActionResult GetCategoriasByProy(int idProy)
    {
        var response = new CategoriasResponse();

        using (SqlConnection conn = new SqlConnection(_connectionString))
        {
            try
            {
                conn.Open();
                string query = @"SELECT cc.Id, cc.IdCategorizer, cc.Fase, cc.Nombre as Categoria, c.IdProy
                               FROM Categorias_Categorizer cc
                               INNER JOIN Categorizer c ON cc.IdCategorizer = c.Id
                               WHERE c.IdProy = @IdProy";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@IdProy", idProy);

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            response.categorias.Add(new CategoriaItem
                            {
                                Id = (int)reader["Id"],
                                IdCategorizer = (int)reader["IdCategorizer"],
                                Fase = (int)reader["Fase"],
                                Nombre = reader["Categoria"].ToString(),
                                IdProy = (int)reader["IdProy"]
                            });
                        }
                    }
                }

                // Cambiar la estructura de respuesta para que coincida con lo que espera Angular
                return Ok(new { items = response.categorias });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener categorías por proyecto: {IdProy}", idProy);
                return StatusCode(500, new { error = ex.Message });
            }
        }
    }

    // Obtener categorías de una sesión de categorizer
    [HttpGet("{idCategorizer}/categorias")]
    public IActionResult GetCategorias(int idCategorizer)
    {
        var response = new CategoriasResponse();

        using (SqlConnection conn = new SqlConnection(_connectionString))
        {
            try
            {
                conn.Open();
                string query = @"SELECT Id, IdCategorizer, Fase, Nombre
                               FROM Categorias_Categorizer
                               WHERE IdCategorizer = @IdCategorizer";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@IdCategorizer", idCategorizer);

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            response.categorias.Add(new CategoriaItem
                            {
                                Id = (int)reader["Id"],
                                IdCategorizer = (int)reader["IdCategorizer"],
                                Fase = (int)reader["Fase"],
                                Nombre = reader["Nombre"].ToString()
                            });
                        }
                    }
                }

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener categorías");
                return StatusCode(500, new { error = ex.Message });
            }
        }
    }

    // Agregar categoría a una sesión de categorizer
    [HttpPost("{idCategorizer}/categorias")]
    public IActionResult AddCategoria(int idCategorizer, [FromBody] CategoriaRequest request)
    {
        try
        {
            _logger.LogInformation("=== Iniciando creación de categoría ===");
            _logger.LogInformation("IdCategorizer: {IdCategorizer}", idCategorizer);
            
            // Log del request body crudo
            _logger.LogInformation("Request recibido: {@Request}", request);
            
            // Verificar si el request es null
            if (request == null)
            {
                _logger.LogWarning("Request es null - problema de deserialización JSON");
                return BadRequest(new { error = "La solicitud no puede estar vacía. Verifique el formato JSON." });
            }

            // Log de cada propiedad del request
            _logger.LogInformation("Fase: {Fase}", request.Fase);
            _logger.LogInformation("Nombre: '{Nombre}'", request.Nombre ?? "NULL");

            if (string.IsNullOrEmpty(request.Nombre))
            {
                _logger.LogWarning("Nombre es null o vacío");
                return BadRequest(new { error = "El nombre de la categoría es requerido" });
            }

            // Verificar que el IdCategorizer existe
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                
                // Primero verificar que el IdCategorizer existe
                string checkQuery = "SELECT COUNT(*) FROM Categorizer WHERE Id = @IdCategorizer";
                using (SqlCommand checkCmd = new SqlCommand(checkQuery, conn))
                {
                    checkCmd.Parameters.AddWithValue("@IdCategorizer", idCategorizer);
                    int count = Convert.ToInt32(checkCmd.ExecuteScalar());
                    
                    if (count == 0)
                    {
                        _logger.LogError("IdCategorizer {IdCategorizer} no existe en la tabla Categorizer", idCategorizer);
                        return BadRequest(new { error = $"El IdCategorizer {idCategorizer} no existe" });
                    }
                }

                _logger.LogInformation("IdCategorizer {IdCategorizer} existe, procediendo con la inserción", idCategorizer);

                string query = @"INSERT INTO Categorias_Categorizer (IdCategorizer, Fase, Nombre) 
                               VALUES (@IdCategorizer, @Fase, @Nombre);
                               SELECT SCOPE_IDENTITY();";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@IdCategorizer", idCategorizer);
                    cmd.Parameters.AddWithValue("@Fase", request.Fase);
                    cmd.Parameters.AddWithValue("@Nombre", request.Nombre);

                    _logger.LogInformation("Ejecutando query de inserción");
                    _logger.LogInformation("Parámetros: IdCategorizer={IdCategorizer}, Fase={Fase}, Nombre='{Nombre}'", 
                        idCategorizer, request.Fase, request.Nombre ?? "NULL");
                    
                    int newId = Convert.ToInt32(cmd.ExecuteScalar());
                    
                    if (newId > 0)
                    {
                        _logger.LogInformation("Categoría creada exitosamente con ID: {NewId}", newId);
                        return Ok(new { message = "Categoría agregada correctamente", id = newId, IdCategorizer = idCategorizer });
                    }
                    else
                    {
                        _logger.LogError("No se pudo insertar la categoría - SCOPE_IDENTITY() retornó 0");
                        return StatusCode(500, new { error = "No se pudo insertar la categoría" });
                    }
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al agregar categoría: {Message}", ex.Message);
            return StatusCode(500, new { error = ex.Message });
        }
    }

    // Actualizar categoría
    [HttpPut("categorias/{id}")]
    public IActionResult UpdateCategoria(int id, [FromBody] CategoriaRequest request)
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
                string query = @"UPDATE Categorias_Categorizer
                               SET Fase = @Fase, Nombre = @Nombre
                               WHERE Id = @Id";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@Id", id);
                    cmd.Parameters.AddWithValue("@Fase", request.Fase);
                    cmd.Parameters.AddWithValue("@Nombre", request.Nombre ?? (object)DBNull.Value);

                    int rowsAffected = cmd.ExecuteNonQuery();

                    if (rowsAffected == 0)
                    {
                        return NotFound(new { error = $"No se encontró la categoría con ID {id}" });
                    }

                    return Ok(new { message = "Categoría actualizada correctamente", id = id });
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al actualizar categoría con ID: {Id}", id);
            return StatusCode(500, new { error = ex.Message });
        }
    }

    // Eliminar categoría
    [HttpDelete("categorias/{id}")]
    public IActionResult DeleteCategoria(int id)
    {
        using (SqlConnection conn = new SqlConnection(_connectionString))
        {
            try
            {
                conn.Open();
                string query = "DELETE FROM Categorias_Categorizer WHERE Id = @Id";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@Id", id);
                    int rowsAffected = cmd.ExecuteNonQuery();

                    if (rowsAffected == 0)
                    {
                        return NotFound(new { error = $"No se encontró la categoría con ID {id}" });
                    }

                    return Ok(new { message = "Categoría eliminada correctamente", id = id });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al eliminar categoría con ID: {Id}", id);
                return StatusCode(500, new { error = ex.Message });
            }
        }
    }

    // Obtener todas las ideas de un proyecto (para el frontend de categorizer)
    [HttpGet("byproy/{idProy}/ideas")]
    public IActionResult GetIdeasByProy(int idProy)
    {
        var response = new IdeasResponse();

        using (SqlConnection conn = new SqlConnection(_connectionString))
        {
            try
            {
                conn.Open();
                string query = @"SELECT ib.Id, ib.IdBrainstorm, ib.IdUsuario, ib.Idea, u.Nombre
                               FROM Ideas_Brainstorm ib
                               INNER JOIN Brainstorming b ON ib.IdBrainstorm = b.Id
                               INNER JOIN Usuarios u ON ib.IdUsuario = u.Id
                               WHERE b.IdProy = @IdProy";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@IdProy", idProy);

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            response.ideas.Add(new IdeaItem
                            {
                                Id = (int)reader["Id"],
                                IdBrainstorm = (int)reader["IdBrainstorm"],
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
                _logger.LogError(ex, "Error al obtener ideas por proyecto: {IdProy}", idProy);
                return StatusCode(500, new { error = ex.Message });
            }
        }
    }

    // Endpoint de prueba para verificar si un IdCategorizer existe
    [HttpGet("test-categorizer/{idCategorizer}")]
    public IActionResult TestCategorizerExists(int idCategorizer)
    {
        try
        {
            _logger.LogInformation("=== Verificando existencia de IdCategorizer ===");
            _logger.LogInformation("IdCategorizer: {IdCategorizer}", idCategorizer);

            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                string query = "SELECT Id, IdProy, Fases FROM Categorizer WHERE Id = @IdCategorizer";
                
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@IdCategorizer", idCategorizer);
                    
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            var categorizer = new CategorizerSession
                            {
                                Id = (int)reader["Id"],
                                IdProy = (int)reader["IdProy"],
                                Fases = (int)reader["Fases"]
                            };
                            
                            _logger.LogInformation("IdCategorizer {IdCategorizer} existe: {@Categorizer}", idCategorizer, categorizer);
                            return Ok(new { exists = true, categorizer = categorizer });
                        }
                        else
                        {
                            _logger.LogWarning("IdCategorizer {IdCategorizer} NO existe", idCategorizer);
                            return Ok(new { exists = false, message = $"IdCategorizer {idCategorizer} no existe" });
                        }
                    }
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al verificar IdCategorizer {IdCategorizer}: {Message}", idCategorizer, ex.Message);
            return StatusCode(500, new { error = ex.Message });
        }
    }

    // Endpoint de prueba para crear categoría sin validaciones
    [HttpPost("test-create-categoria/{idCategorizer}")]
    public IActionResult TestCreateCategoria(int idCategorizer, [FromBody] object request)
    {
        try
        {
            _logger.LogInformation("=== Test Create Categoria ===");
            _logger.LogInformation("IdCategorizer: {IdCategorizer}", idCategorizer);
            _logger.LogInformation("Request Type: {RequestType}", request?.GetType().Name ?? "NULL");
            _logger.LogInformation("Request: {@Request}", request);

            return Ok(new { 
                message = "Test endpoint funcionando", 
                idCategorizer = idCategorizer,
                request = request 
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error en test-create-categoria: {Message}", ex.Message);
            return StatusCode(500, new { error = ex.Message });
        }
    }

    // Iniciar nueva fase de categorización
    [HttpPost("{idCategorizer}/nueva-fase")]
    public IActionResult IniciarNuevaFase(int idCategorizer)
    {
        try
        {
            _logger.LogInformation("=== Iniciando nueva fase de categorización ===");
            _logger.LogInformation("IdCategorizer: {IdCategorizer}", idCategorizer);

            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                
                // Iniciar transacción para asegurar consistencia
                using (var transaction = conn.BeginTransaction())
                {
                    try
                    {
                        // Primero verificar que el IdCategorizer existe y obtener las fases actuales
                        string checkQuery = "SELECT Fases FROM Categorizer WHERE Id = @IdCategorizer";
                        int fasesActuales;
                        using (SqlCommand checkCmd = new SqlCommand(checkQuery, conn, transaction))
                        {
                            checkCmd.Parameters.AddWithValue("@IdCategorizer", idCategorizer);
                            var result = checkCmd.ExecuteScalar();
                            
                            if (result == null)
                            {
                                _logger.LogError("IdCategorizer {IdCategorizer} no existe", idCategorizer);
                                return NotFound(new { error = $"No se encontró la sesión de categorizer con ID {idCategorizer}" });
                            }
                            
                            fasesActuales = (int)result;
                        }

                        // Incrementar las fases en 1
                        int nuevaFase = fasesActuales + 1;
                        
                        // Actualizar el campo Fases en la tabla Categorizer
                        string updateQuery = "UPDATE Categorizer SET Fases = @NuevaFase WHERE Id = @IdCategorizer";
                        using (SqlCommand updateCmd = new SqlCommand(updateQuery, conn, transaction))
                        {
                            updateCmd.Parameters.AddWithValue("@IdCategorizer", idCategorizer);
                            updateCmd.Parameters.AddWithValue("@NuevaFase", nuevaFase);
                            
                            int rowsAffected = updateCmd.ExecuteNonQuery();
                            if (rowsAffected == 0)
                            {
                                throw new Exception("No se pudo actualizar el número de fases");
                            }
                        }

                        // Obtener todas las categorías de la fase anterior
                        string getCategoriasQuery = @"SELECT Nombre 
                                                    FROM Categorias_Categorizer 
                                                    WHERE IdCategorizer = @IdCategorizer AND Fase = @FaseAnterior";
                        
                        var categoriasAnteriores = new List<string>();
                        using (SqlCommand getCmd = new SqlCommand(getCategoriasQuery, conn, transaction))
                        {
                            getCmd.Parameters.AddWithValue("@IdCategorizer", idCategorizer);
                            getCmd.Parameters.AddWithValue("@FaseAnterior", fasesActuales);
                            
                            using (SqlDataReader reader = getCmd.ExecuteReader())
                            {
                                while (reader.Read())
                                {
                                    categoriasAnteriores.Add(reader["Nombre"].ToString());
                                }
                            }
                        }

                        // Duplicar las categorías con la nueva fase
                        string insertQuery = @"INSERT INTO Categorias_Categorizer (IdCategorizer, Fase, Nombre) 
                                             VALUES (@IdCategorizer, @NuevaFase, @Nombre)";
                        
                        int categoriasDuplicadas = 0;
                        foreach (var nombreCategoria in categoriasAnteriores)
                        {
                            using (SqlCommand insertCmd = new SqlCommand(insertQuery, conn, transaction))
                            {
                                insertCmd.Parameters.AddWithValue("@IdCategorizer", idCategorizer);
                                insertCmd.Parameters.AddWithValue("@NuevaFase", nuevaFase);
                                insertCmd.Parameters.AddWithValue("@Nombre", nombreCategoria);
                                
                                insertCmd.ExecuteNonQuery();
                                categoriasDuplicadas++;
                            }
                        }

                        // Confirmar la transacción
                        transaction.Commit();

                        _logger.LogInformation("Nueva fase iniciada exitosamente. Fase anterior: {FaseAnterior}, Nueva fase: {NuevaFase}, Categorías duplicadas: {CategoriasDuplicadas}", 
                            fasesActuales, nuevaFase, categoriasDuplicadas);

                        return Ok(new { 
                            message = "Nueva fase iniciada correctamente", 
                            idCategorizer = idCategorizer,
                            faseAnterior = fasesActuales,
                            nuevaFase = nuevaFase,
                            categoriasDuplicadas = categoriasDuplicadas
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
            _logger.LogError(ex, "Error al iniciar nueva fase para IdCategorizer {IdCategorizer}: {Message}", idCategorizer, ex.Message);
            return StatusCode(500, new { error = ex.Message });
        }
    }

    // Endpoint de prueba para insertar categoría directamente
    [HttpPost("test-insert-categoria/{idCategorizer}")]
    public IActionResult TestInsertCategoria(int idCategorizer, [FromBody] CategoriaRequest request)
    {
        try
        {
            _logger.LogInformation("=== Test Insert Categoria ===");
            _logger.LogInformation("IdCategorizer: {IdCategorizer}", idCategorizer);
            _logger.LogInformation("Request: {@Request}", request);

            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                
                string query = @"INSERT INTO Categorias_Categorizer (IdCategorizer, Fase, Nombre) 
                               VALUES (@IdCategorizer, @Fase, @Nombre);
                               SELECT SCOPE_IDENTITY();";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@IdCategorizer", idCategorizer);
                    cmd.Parameters.AddWithValue("@Fase", request.Fase);
                    cmd.Parameters.AddWithValue("@Nombre", request.Nombre ?? "");

                    _logger.LogInformation("Ejecutando inserción de prueba");
                    int newId = Convert.ToInt32(cmd.ExecuteScalar());
                    
                    _logger.LogInformation("Categoría insertada con ID: {NewId}", newId);
                    return Ok(new { 
                        message = "Categoría insertada correctamente", 
                        id = newId,
                        idCategorizer = idCategorizer 
                    });
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error en test-insert-categoria: {Message}", ex.Message);
            return StatusCode(500, new { error = ex.Message, details = ex.ToString() });
        }
    }

    // ========== ENDPOINTS PARA IDEAS-CATEGORÍAS ==========

    // Obtener todas las relaciones idea-categoría
    [HttpGet("ideas-categorias")]
    public IActionResult GetAllIdeasCategorias()
    {
        var response = new IdeasCategoriasResponse();

        using (SqlConnection conn = new SqlConnection(_connectionString))
        {
            try
            {
                conn.Open();
                string query = @"SELECT ic.IdCategoria, ic.IdIdea, c.Nombre as NombreCategoria, i.Idea, i.IdUsuario, u.Nombre as NombreUsuario
                               FROM Ideas_categorias_categorizer ic
                               INNER JOIN Categorias_Categorizer c ON ic.IdCategoria = c.Id
                               INNER JOIN Ideas_Brainstorm i ON ic.IdIdea = i.Id
                               INNER JOIN Usuarios u ON i.IdUsuario = u.Id";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            response.ideasCategorias.Add(new IdeaCategoriaItem
                            {
                                IdCategoria = (int)reader["IdCategoria"],
                                IdIdea = (int)reader["IdIdea"],
                                NombreCategoria = reader["NombreCategoria"].ToString(),
                                Idea = reader["Idea"].ToString(),
                                IdUsuario = (int)reader["IdUsuario"],
                                NombreUsuario = reader["NombreUsuario"].ToString()
                            });
                        }
                    }
                }

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener todas las relaciones idea-categoría");
                return StatusCode(500, new { error = ex.Message });
            }
        }
    }

    // Obtener ideas por categoría
    [HttpGet("categorias/{idCategoria}/ideas")]
    public IActionResult GetIdeasByCategoria(int idCategoria)
    {
        var response = new IdeasCategoriasResponse();

        using (SqlConnection conn = new SqlConnection(_connectionString))
        {
            try
            {
                conn.Open();
                string query = @"SELECT ic.IdCategoria, ic.IdIdea, c.Nombre as NombreCategoria, i.Idea, i.IdUsuario, u.Nombre as NombreUsuario
                               FROM Ideas_categorias_categorizer ic
                               INNER JOIN Categorias_Categorizer c ON ic.IdCategoria = c.Id
                               INNER JOIN Ideas_Brainstorm i ON ic.IdIdea = i.Id
                               INNER JOIN Usuarios u ON i.IdUsuario = u.Id
                               WHERE ic.IdCategoria = @IdCategoria";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@IdCategoria", idCategoria);

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            response.ideasCategorias.Add(new IdeaCategoriaItem
                            {
                                IdCategoria = (int)reader["IdCategoria"],
                                IdIdea = (int)reader["IdIdea"],
                                NombreCategoria = reader["NombreCategoria"].ToString(),
                                Idea = reader["Idea"].ToString(),
                                IdUsuario = (int)reader["IdUsuario"],
                                NombreUsuario = reader["NombreUsuario"].ToString()
                            });
                        }
                    }
                }

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener ideas por categoría: {IdCategoria}", idCategoria);
                return StatusCode(500, new { error = ex.Message });
            }
        }
    }

    // Obtener categorías por idea
    [HttpGet("ideas/{idIdea}/categorias")]
    public IActionResult GetCategoriasByIdea(int idIdea)
    {
        var response = new IdeasCategoriasResponse();

        using (SqlConnection conn = new SqlConnection(_connectionString))
        {
            try
            {
                conn.Open();
                string query = @"SELECT ic.IdCategoria, ic.IdIdea, c.Nombre as NombreCategoria, i.Idea, i.IdUsuario, u.Nombre as NombreUsuario
                               FROM Ideas_categorias_categorizer ic
                               INNER JOIN Categorias_Categorizer c ON ic.IdCategoria = c.Id
                               INNER JOIN Ideas_Brainstorm i ON ic.IdIdea = i.Id
                               INNER JOIN Usuarios u ON i.IdUsuario = u.Id
                               WHERE ic.IdIdea = @IdIdea";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@IdIdea", idIdea);

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            response.ideasCategorias.Add(new IdeaCategoriaItem
                            {
                                IdCategoria = (int)reader["IdCategoria"],
                                IdIdea = (int)reader["IdIdea"],
                                NombreCategoria = reader["NombreCategoria"].ToString(),
                                Idea = reader["Idea"].ToString(),
                                IdUsuario = (int)reader["IdUsuario"],
                                NombreUsuario = reader["NombreUsuario"].ToString()
                            });
                        }
                    }
                }

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener categorías por idea: {IdIdea}", idIdea);
                return StatusCode(500, new { error = ex.Message });
            }
        }
    }

    // Agregar idea a categoría
    [HttpPost("categorias/{idCategoria}/ideas")]
    public IActionResult AddIdeaToCategoria(int idCategoria, [FromBody] IdeaCategoriaRequest request)
    {
        try
        {
            if (request == null || request.IdIdea <= 0)
            {
                return BadRequest(new { error = "El ID de la idea es requerido y debe ser mayor a 0" });
            }

            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                
                // Verificar si ya existe la relación
                string checkQuery = "SELECT COUNT(*) FROM Ideas_categorias_categorizer WHERE IdCategoria = @IdCategoria AND IdIdea = @IdIdea";
                using (SqlCommand checkCmd = new SqlCommand(checkQuery, conn))
                {
                    checkCmd.Parameters.AddWithValue("@IdCategoria", idCategoria);
                    checkCmd.Parameters.AddWithValue("@IdIdea", request.IdIdea);
                    
                    if ((int)checkCmd.ExecuteScalar() > 0)
                    {
                        return BadRequest(new { error = "Esta idea ya está asignada a esta categoría" });
                    }
                }

                string query = @"INSERT INTO Ideas_categorias_categorizer (IdCategoria, IdIdea) 
                               VALUES (@IdCategoria, @IdIdea)";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@IdCategoria", idCategoria);
                    cmd.Parameters.AddWithValue("@IdIdea", request.IdIdea);

                    int rowsAffected = cmd.ExecuteNonQuery();
                    if (rowsAffected > 0)
                    {
                        return Ok(new { message = "Idea agregada a la categoría correctamente" });
                    }
                    else
                    {
                        return StatusCode(500, new { error = "No se pudo agregar la idea a la categoría" });
                    }
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al agregar idea a categoría");
            return StatusCode(500, new { error = ex.Message });
        }
    }

    // Remover idea de categoría
    [HttpDelete("categorias/{idCategoria}/ideas/{idIdea}")]
    public IActionResult RemoveIdeaFromCategoria(int idCategoria, int idIdea)
    {
        try
        {
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                string query = "DELETE FROM Ideas_categorias_categorizer WHERE IdCategoria = @IdCategoria AND IdIdea = @IdIdea";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@IdCategoria", idCategoria);
                    cmd.Parameters.AddWithValue("@IdIdea", idIdea);

                    int rowsAffected = cmd.ExecuteNonQuery();

                    if (rowsAffected == 0)
                    {
                        return NotFound(new { error = "No se encontró la relación idea-categoría para eliminar" });
                    }

                    return Ok(new { message = "Idea removida de la categoría correctamente" });
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al remover idea de categoría");
            return StatusCode(500, new { error = ex.Message });
        }
    }

    // Verificar si una idea está en una categoría
    [HttpGet("categorias/{idCategoria}/ideas/{idIdea}/check")]
    public IActionResult CheckIdeaInCategoria(int idCategoria, int idIdea)
    {
        using (SqlConnection conn = new SqlConnection(_connectionString))
        {
            try
            {
                conn.Open();
                string query = "SELECT COUNT(*) as Count FROM Ideas_categorias_categorizer WHERE IdCategoria = @IdCategoria AND IdIdea = @IdIdea";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@IdCategoria", idCategoria);
                    cmd.Parameters.AddWithValue("@IdIdea", idIdea);

                    int count = (int)cmd.ExecuteScalar();
                    bool exists = count > 0;

                    return Ok(new { 
                        exists = exists,
                        count = count,
                        message = exists ? "La idea está en esta categoría" : "La idea no está en esta categoría"
                    });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al verificar idea en categoría");
                return StatusCode(500, new { error = ex.Message });
            }
        }
    }
}

// Clases para sesiones de categorizer
public class CategorizerSession
{
    public int Id { get; set; }
    public int IdProy { get; set; }
    public int? IdTopico { get; set; }
    public int? IdBrainstorm { get; set; }
    public int Fases { get; set; }
}

public class CategorizerSessionRequest
{
    public int IdProy { get; set; }
    public int? IdTopico { get; set; }
    public int? IdBrainstorm { get; set; }
    public int Fases { get; set; }
}

public class CategorizerSessionsResponse
{
    public List<CategorizerSession> sessions { get; set; } = new List<CategorizerSession>();
}

// Clases para categorías
public class CategoriaItem
{
    public int Id { get; set; }
    public int IdCategorizer { get; set; }
    public int Fase { get; set; }
    public string? Nombre { get; set; }
    public int IdProy { get; set; }
}

public class CategoriaRequest
{
    public int Fase { get; set; }
    public string? Nombre { get; set; }
}

public class CategoriasResponse
{
    public List<CategoriaItem> categorias { get; set; } = new List<CategoriaItem>();
}

// Clases para ideas (para el endpoint de categorizer)
public class IdeaItem
{
    public int Id { get; set; }
    public int IdBrainstorm { get; set; }
    public int IdUsuario { get; set; }
    public string? Idea { get; set; }
    public string? Nombre { get; set; }
}

public class IdeasResponse
{
    public List<IdeaItem> ideas { get; set; } = new List<IdeaItem>();
}

// Clases para ideas-categorías
public class IdeaCategoriaItem
{
    public int IdCategoria { get; set; }
    public int IdIdea { get; set; }
    public string NombreCategoria { get; set; }
    public string Idea { get; set; }
    public int IdUsuario { get; set; }
    public string NombreUsuario { get; set; }
}

public class IdeaCategoriaRequest
{
    public int IdCategoria { get; set; }
    public int IdIdea { get; set; }
}

public class IdeasCategoriasResponse
{
    public List<IdeaCategoriaItem> ideasCategorias { get; set; } = new List<IdeaCategoriaItem>();
}