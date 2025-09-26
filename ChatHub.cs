using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;

public class ChatHub : Hub
{
    private readonly string _connectionString;

    public ChatHub(IConfiguration configuration)
    {
        _connectionString = "Server=LAPTOP-7MITNTQF\\SQLEXPRESS;Database=chat;User Id=sa;Password=admin;TrustServerCertificate=True";
    }

    public override Task OnConnectedAsync()
    {
        return base.OnConnectedAsync();
    }

    public override Task OnDisconnectedAsync(Exception? exception)
    {
        return base.OnDisconnectedAsync(exception);
    }

    public async Task SendMessage(string user, string message)
    {
        // Guardar en BD
        using (SqlConnection conn = new SqlConnection(_connectionString))
        {
            try
            {
                conn.Open();
                string query = "INSERT INTO Chat (IdProy, IdUsuario, Fecha, Hora, Mensaje, Nombre) " +
                             "VALUES (@IdProy, @IdUsuario, GETDATE(), CAST(GETDATE() AS TIME), @Mensaje, @Nombre);";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@IdProy", 1); // Valor por defecto
                    cmd.Parameters.AddWithValue("@IdUsuario", 1); // Valor por defecto
                    cmd.Parameters.AddWithValue("@Mensaje", message);
                    cmd.Parameters.AddWithValue("@Nombre", user);

                    cmd.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al guardar en BD: {ex.Message}");
            }
        }

        // Enviar a todos los clientes
        await Clients.All.SendAsync("ReceiveMessage", user, message);
    }
} 