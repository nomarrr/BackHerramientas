using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;

[ApiController]
[Route("api/[controller]")]
public class MatrizValuadaController : ControllerBase
{
    private readonly string _connectionString;
    private readonly ILogger<MatrizValuadaController> _logger;

    public MatrizValuadaController(ILogger<MatrizValuadaController> logger)
    {
        _connectionString = @"Data Source=189.195.162.46;Initial Catalog=HerramientasV3;User ID=sa;Password=sqlSA%;Encrypt=True;TrustServerCertificate=True";
        _logger = logger;
    }

    // Obtener todas las entradas de matriz valuada
    [HttpGet]
    public IActionResult GetAllMatrizValuada()
    {
        var response = new MatrizValuadaResponse();

        using (SqlConnection conn = new SqlConnection(_connectionString))
        {
            try
            {
                conn.Open();
                string query = "SELECT Id, IdProy, IdAlt, IdCrit, Valor, Peso FROM MatrizValuada";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            response.matrizItems.Add(new MatrizValuadaItem
                            {
                                Id = (int)reader["Id"],
                                IdProy = (int)reader["IdProy"],
                                IdAlt = (int)reader["IdAlt"],
                                IdCrit = (int)reader["IdCrit"],
                                Valor = reader["Valor"] == DBNull.Value ? (int?)null : (int)reader["Valor"],
                                Peso = reader["Peso"] == DBNull.Value ? (decimal?)null : (decimal)reader["Peso"]
                            });
                        }
                    }
                }

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener todas las entradas de matriz valuada");
                return StatusCode(500, new { error = ex.Message });
            }
        }
    }

    // Obtener entrada de matriz valuada por ID
    [HttpGet("{id}")]
    public IActionResult GetMatrizValuadaById(int id)
    {
        using (SqlConnection conn = new SqlConnection(_connectionString))
        {
            try
            {
                conn.Open();
                string query = "SELECT Id, IdProy, IdAlt, IdCrit, Valor, Peso FROM MatrizValuada WHERE Id = @Id";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@Id", id);

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            var matrizItem = new MatrizValuadaItem
                            {
                                Id = (int)reader["Id"],
                                IdProy = (int)reader["IdProy"],
                                IdAlt = (int)reader["IdAlt"],
                                IdCrit = (int)reader["IdCrit"],
                                Valor = reader["Valor"] == DBNull.Value ? (int?)null : (int)reader["Valor"],
                                Peso = reader["Peso"] == DBNull.Value ? (decimal?)null : (decimal)reader["Peso"]
                            };
                            return Ok(matrizItem);
                        }
                        else
                        {
                            return NotFound(new { error = $"No se encontró la entrada de matriz valuada con ID {id}" });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener entrada de matriz valuada por ID: {Id}", id);
                return StatusCode(500, new { error = ex.Message });
            }
        }
    }

    // Obtener matriz valuada por proyecto
    [HttpGet("byproy/{idProy}")]
    public IActionResult GetMatrizValuadaByProy(int idProy)
    {
        var response = new MatrizValuadaResponse();

        using (SqlConnection conn = new SqlConnection(_connectionString))
        {
            try
            {
                conn.Open();
                string query = "SELECT Id, IdProy, IdAlt, IdCrit, Valor, Peso FROM MatrizValuada WHERE IdProy = @IdProy";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@IdProy", idProy);

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            response.matrizItems.Add(new MatrizValuadaItem
                            {
                                Id = (int)reader["Id"],
                                IdProy = (int)reader["IdProy"],
                                IdAlt = (int)reader["IdAlt"],
                                IdCrit = (int)reader["IdCrit"],
                                Valor = reader["Valor"] == DBNull.Value ? (int?)null : (int)reader["Valor"],
                                Peso = reader["Peso"] == DBNull.Value ? (decimal?)null : (decimal)reader["Peso"]
                            });
                        }
                    }
                }

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener matriz valuada por proyecto: {IdProy}", idProy);
                return StatusCode(500, new { error = ex.Message });
            }
        }
    }

    // Obtener matriz valuada por alternativa
    [HttpGet("byalt/{idAlt}")]
    public IActionResult GetMatrizValuadaByAlt(int idAlt)
    {
        var response = new MatrizValuadaResponse();

        using (SqlConnection conn = new SqlConnection(_connectionString))
        {
            try
            {
                conn.Open();
                string query = "SELECT Id, IdProy, IdAlt, IdCrit, Valor, Peso FROM MatrizValuada WHERE IdAlt = @IdAlt";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@IdAlt", idAlt);

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            response.matrizItems.Add(new MatrizValuadaItem
                            {
                                Id = (int)reader["Id"],
                                IdProy = (int)reader["IdProy"],
                                IdAlt = (int)reader["IdAlt"],
                                IdCrit = (int)reader["IdCrit"],
                                Valor = reader["Valor"] == DBNull.Value ? (int?)null : (int)reader["Valor"],
                                Peso = reader["Peso"] == DBNull.Value ? (decimal?)null : (decimal)reader["Peso"]
                            });
                        }
                    }
                }

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener matriz valuada por alternativa: {IdAlt}", idAlt);
                return StatusCode(500, new { error = ex.Message });
            }
        }
    }

    // Obtener matriz completa (con nombres de alternativas y criterios)
    [HttpGet("byproy/{idProy}/complete")]
    public IActionResult GetMatrizValuadaComplete(int idProy)
    {
        var response = new MatrizValuadaCompleteResponse();

        using (SqlConnection conn = new SqlConnection(_connectionString))
        {
            try
            {
                conn.Open();
                string query = @"SELECT mv.Id, mv.IdProy, mv.IdAlt, mv.IdCrit, mv.Valor, mv.Peso,
                               a.Nombre as NombreAlternativa, c.Nombre as NombreCriterio, c.Peso as PesoCriterio
                               FROM MatrizValuada mv
                               INNER JOIN Alternativas a ON mv.IdAlt = a.Id
                               INNER JOIN Criterios c ON mv.IdCrit = c.Id
                               WHERE mv.IdProy = @IdProy";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@IdProy", idProy);

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            response.matrizItems.Add(new MatrizValuadaCompleteItem
                            {
                                Id = (int)reader["Id"],
                                IdProy = (int)reader["IdProy"],
                                IdAlt = (int)reader["IdAlt"],
                                IdCrit = (int)reader["IdCrit"],
                                Valor = reader["Valor"] == DBNull.Value ? (int?)null : (int)reader["Valor"],
                                Peso = reader["Peso"] == DBNull.Value ? (decimal?)null : (decimal)reader["Peso"],
                                NombreAlternativa = reader["NombreAlternativa"].ToString(),
                                NombreCriterio = reader["NombreCriterio"].ToString(),
                                PesoCriterio = reader["PesoCriterio"] == DBNull.Value ? (decimal?)null : (decimal)reader["PesoCriterio"]
                            });
                        }
                    }
                }

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener matriz valuada completa por proyecto: {IdProy}", idProy);
                return StatusCode(500, new { error = ex.Message });
            }
        }
    }

    // Crear nueva entrada de matriz valuada
    [HttpPost]
    public IActionResult CreateMatrizValuada([FromBody] MatrizValuadaRequest request)
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
                
                // Verificar si ya existe la entrada
                string checkQuery = "SELECT COUNT(*) FROM MatrizValuada WHERE IdProy = @IdProy AND IdAlt = @IdAlt AND IdCrit = @IdCrit";
                using (SqlCommand checkCmd = new SqlCommand(checkQuery, conn))
                {
                    checkCmd.Parameters.AddWithValue("@IdProy", request.IdProy);
                    checkCmd.Parameters.AddWithValue("@IdAlt", request.IdAlt);
                    checkCmd.Parameters.AddWithValue("@IdCrit", request.IdCrit);
                    
                    if ((int)checkCmd.ExecuteScalar() > 0)
                    {
                        return BadRequest(new { error = "Ya existe una entrada para esta combinación de alternativa y criterio" });
                    }
                }

                string query = @"INSERT INTO MatrizValuada (IdProy, IdAlt, IdCrit, Valor, Peso) 
                               VALUES (@IdProy, @IdAlt, @IdCrit, @Valor, @Peso);
                               SELECT SCOPE_IDENTITY();";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@IdProy", request.IdProy);
                    cmd.Parameters.AddWithValue("@IdAlt", request.IdAlt);
                    cmd.Parameters.AddWithValue("@IdCrit", request.IdCrit);
                    cmd.Parameters.AddWithValue("@Valor", request.Valor ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@Peso", request.Peso ?? (object)DBNull.Value);

                    int newId = Convert.ToInt32(cmd.ExecuteScalar());
                    return Ok(new { message = "Entrada de matriz valuada creada correctamente", id = newId });
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al crear entrada de matriz valuada");
            return StatusCode(500, new { error = ex.Message });
        }
    }

    // Actualizar entrada de matriz valuada
    [HttpPut("{id}")]
    public IActionResult UpdateMatrizValuada(int id, [FromBody] MatrizValuadaRequest request)
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
                string query = @"UPDATE MatrizValuada
                               SET IdProy = @IdProy, IdAlt = @IdAlt, IdCrit = @IdCrit, Valor = @Valor, Peso = @Peso
                               WHERE Id = @Id";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@Id", id);
                    cmd.Parameters.AddWithValue("@IdProy", request.IdProy);
                    cmd.Parameters.AddWithValue("@IdAlt", request.IdAlt);
                    cmd.Parameters.AddWithValue("@IdCrit", request.IdCrit);
                    cmd.Parameters.AddWithValue("@Valor", request.Valor ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@Peso", request.Peso ?? (object)DBNull.Value);

                    int rowsAffected = cmd.ExecuteNonQuery();

                    if (rowsAffected == 0)
                    {
                        return NotFound(new { error = $"No se encontró la entrada de matriz valuada con ID {id} para actualizar" });
                    }

                    return Ok(new { message = "Entrada de matriz valuada actualizada correctamente" });
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al actualizar entrada de matriz valuada con ID: {Id}", id);
            return StatusCode(500, new { error = ex.Message });
        }
    }

    // Eliminar entrada de matriz valuada
    [HttpDelete("{id}")]
    public IActionResult DeleteMatrizValuada(int id)
    {
        using (SqlConnection conn = new SqlConnection(_connectionString))
        {
            try
            {
                conn.Open();
                string query = "DELETE FROM MatrizValuada WHERE Id = @Id";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@Id", id);
                    int rowsAffected = cmd.ExecuteNonQuery();

                    if (rowsAffected == 0)
                    {
                        return NotFound(new { error = $"No se encontró la entrada de matriz valuada con ID {id} para eliminar" });
                    }

                    return Ok(new { message = "Entrada de matriz valuada eliminada correctamente" });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al eliminar entrada de matriz valuada con ID: {Id}", id);
                return StatusCode(500, new { error = ex.Message });
            }
        }
    }
}

public class MatrizValuadaItem
{
    public int Id { get; set; }
    public int IdProy { get; set; }
    public int IdAlt { get; set; }
    public int IdCrit { get; set; }
    public int? Valor { get; set; }
    public decimal? Peso { get; set; }
}

public class MatrizValuadaCompleteItem
{
    public int Id { get; set; }
    public int IdProy { get; set; }
    public int IdAlt { get; set; }
    public int IdCrit { get; set; }
    public int? Valor { get; set; }
    public decimal? Peso { get; set; }
    public string NombreAlternativa { get; set; }
    public string NombreCriterio { get; set; }
    public decimal? PesoCriterio { get; set; }
}

public class MatrizValuadaRequest
{
    public int IdProy { get; set; }
    public int IdAlt { get; set; }
    public int IdCrit { get; set; }
    public int? Valor { get; set; }
    public decimal? Peso { get; set; }
}

public class MatrizValuadaResponse
{
    public List<MatrizValuadaItem> matrizItems { get; set; } = new List<MatrizValuadaItem>();
}

public class MatrizValuadaCompleteResponse
{
    public List<MatrizValuadaCompleteItem> matrizItems { get; set; } = new List<MatrizValuadaCompleteItem>();
}

