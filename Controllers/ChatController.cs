using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;

[ApiController]
[Route("api/[controller]")]
public class ChatController : ControllerBase
{
    private readonly IHubContext<ChatHub> _hubContext;
    private readonly string _connectionString;

    public ChatController(IHubContext<ChatHub> hubContext, IConfiguration configuration)
    {
        _connectionString = @"Data Source=LAPTOP-7MITNTQF\SQLEXPRESS;Initial Catalog=Herramientas;User ID=sa;Password=admin;Encrypt=True;TrustServerCertificate=True";


        _hubContext = hubContext;
    }

    [HttpGet("messages")]
    public IActionResult GetMessages()
    {
        // Implementa la lógica para obtener mensajes
        return Ok(new { message = "Lista de mensajes" });
    }

    [HttpPost("broadcast")]
    public async Task<IActionResult> BroadcastMessage([FromBody] MessageDto message)
    {
        // Puedes enviar mensajes a través de SignalR desde la API
        await _hubContext.Clients.All.SendAsync("ReceiveMessage", message.User, message.Text);
        return Ok();
    }

    [HttpGet("messages/{idProy}")]
    public IActionResult GetMessages(int idProy)
    {
        var response = new ChatMessagesResponse
        {
            mensajes = new List<ChatMessageResponse>()
        };

        using (SqlConnection conn = new SqlConnection(_connectionString))
        {
            try
            {
                conn.Open();
                string query = @"SELECT Mensaje as texto, 
                                      CONVERT(datetime, CONVERT(varchar, Fecha) + ' ' + CONVERT(varchar, Hora)) as fecha,
                                      Nombre as usuario
                               FROM Chat 
                               WHERE IdProy = @IdProy 
                               ORDER BY Fecha, Hora";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@IdProy", idProy);

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            response.mensajes.Add(new ChatMessageResponse
                            {
                                texto = reader["texto"].ToString(),
                                fecha = (DateTime)reader["fecha"],
                                usuario = reader["usuario"].ToString()
                            });
                        }
                    }
                }

                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }
    }

    [HttpPost("send")]
    public async Task<IActionResult> SendMessage([FromBody] SendMessageRequest request)
    {
        using (SqlConnection conn = new SqlConnection(_connectionString))
        {
            try
            {
                conn.Open();
                string query = "INSERT INTO Chat (IdProy, IdUsuario, Fecha, Hora, Mensaje, Nombre) " +
                             "VALUES (@IdProy, @IdUsuario, GETDATE(), CAST(GETDATE() AS TIME), @Mensaje, @Nombre);";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@IdProy", request.IdProy);
                    cmd.Parameters.AddWithValue("@IdUsuario", request.IdUsuario);
                    cmd.Parameters.AddWithValue("@Mensaje", request.Mensaje);
                    cmd.Parameters.AddWithValue("@Nombre", request.Nombre);

                    cmd.ExecuteNonQuery();
                }

                // Enviamos el mensaje usando el mismo formato que el cliente
                await _hubContext.Clients.All.SendAsync("ReceiveMessage", request.Nombre, request.Mensaje);

                return Ok(new { message = "Mensaje enviado correctamente" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }
    }
}

public class MessageDto
{
    public string User { get; set; }
    public string Text { get; set; }
}

public class SendMessageRequest
{
    public int IdProy { get; set; }
    public int IdUsuario { get; set; }
    public string Mensaje { get; set; }
    public string Nombre { get; set; }
}

public class ChatMessageResponse
{
    public string texto { get; set; }
    public DateTime fecha { get; set; }
    public string usuario { get; set; }
}

public class ChatMessagesResponse
{
    public List<ChatMessageResponse> mensajes { get; set; }
} 