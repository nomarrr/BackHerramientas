using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;

public class ChatHub : Hub
{
    private readonly string _connectionString;
    private readonly ILogger<ChatHub> _logger;

    public ChatHub(IConfiguration configuration, ILogger<ChatHub> logger)
    {
        _connectionString = "Server=DESKTOP-N7HILThpc;Database=HerramientasV3;Integrated Security=True;TrustServerCertificate=True";
        _logger = logger;
    }

    public override Task OnConnectedAsync()
    {
        _logger.LogInformation("Cliente conectado: {ConnectionId}", Context.ConnectionId);
        return base.OnConnectedAsync();
    }

    public override Task OnDisconnectedAsync(Exception? exception)
    {
        _logger.LogInformation("Cliente desconectado: {ConnectionId}", Context.ConnectionId);
        return base.OnDisconnectedAsync(exception);
    }

    // Método para enviar mensaje con información completa
    public async Task SendMessage(int idChat, int idUsuario, string message)
    {
        try
        {
            // Obtener nombre del usuario
            string nombreUsuario = await GetUsuarioNombre(idUsuario);
            
            // Guardar mensaje en la tabla Mensajes_chat
            await SaveMessageToDatabase(idChat, idUsuario, message);

            // Enviar a todos los clientes
            await Clients.All.SendAsync("ReceiveMessage", nombreUsuario, message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al enviar mensaje");
            await Clients.Caller.SendAsync("Error", "Error al enviar el mensaje");
        }
    }

    // Método para compatibilidad con el código existente (solo nombre y mensaje)
    public async Task SendMessageLegacy(string user, string message)
    {
        try
        {
            // Buscar usuario por nombre para obtener su ID
            int idUsuario = await GetUsuarioIdByNombre(user);
            
            // Usar chat por defecto (ID 1) o crear uno si no existe
            int idChat = 1;

            // Guardar mensaje en la tabla Mensajes_chat
            await SaveMessageToDatabase(idChat, idUsuario, message);

            // Enviar a todos los clientes
            await Clients.All.SendAsync("ReceiveMessage", user, message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al enviar mensaje con usuario: {User}", user);
            await Clients.Caller.SendAsync("Error", "Error al enviar el mensaje");
        }
    }

    // Método para unirse a una sala de chat específica
    public async Task JoinChatRoom(int idChat)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, $"ChatRoom_{idChat}");
        _logger.LogInformation("Usuario {ConnectionId} se unió a la sala de chat {IdChat}", Context.ConnectionId, idChat);
    }

    // Método para salir de una sala de chat específica
    public async Task LeaveChatRoom(int idChat)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"ChatRoom_{idChat}");
        _logger.LogInformation("Usuario {ConnectionId} salió de la sala de chat {IdChat}", Context.ConnectionId, idChat);
    }

    // Método para enviar mensaje a una sala específica
    public async Task SendMessageToRoom(int idChat, int idUsuario, string message)
    {
        try
        {
            // Obtener nombre del usuario
            string nombreUsuario = await GetUsuarioNombre(idUsuario);
            
            // Guardar mensaje en la tabla Mensajes_chat
            await SaveMessageToDatabase(idChat, idUsuario, message);

            // Enviar solo a los usuarios de esa sala
            await Clients.Group($"ChatRoom_{idChat}").SendAsync("ReceiveMessage", nombreUsuario, message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al enviar mensaje a la sala {IdChat}", idChat);
            await Clients.Caller.SendAsync("Error", "Error al enviar el mensaje");
        }
    }

    // Método privado para guardar mensaje en la base de datos
    private async Task SaveMessageToDatabase(int idChat, int idUsuario, string message)
    {
        using (SqlConnection conn = new SqlConnection(_connectionString))
        {
            try
            {
                await conn.OpenAsync();
                string query = @"INSERT INTO Mensajes_chat (IdChat, IdUsuario, Fecha, Hora, Mensaje) 
                               VALUES (@IdChat, @IdUsuario, GETDATE(), CAST(GETDATE() AS TIME), @Mensaje);";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@IdChat", idChat);
                    cmd.Parameters.AddWithValue("@IdUsuario", idUsuario);
                    cmd.Parameters.AddWithValue("@Mensaje", message);

                    await cmd.ExecuteNonQueryAsync();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al guardar mensaje en la base de datos");
                throw;
            }
        }
    }

    // Método privado para obtener el nombre del usuario por ID
    private async Task<string> GetUsuarioNombre(int idUsuario)
    {
        using (SqlConnection conn = new SqlConnection(_connectionString))
        {
            try
            {
                await conn.OpenAsync();
                string query = "SELECT Nombre FROM Usuarios WHERE Id = @IdUsuario";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@IdUsuario", idUsuario);
                    var result = await cmd.ExecuteScalarAsync();
                    return result?.ToString() ?? "Usuario Desconocido";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener nombre del usuario {IdUsuario}", idUsuario);
                return "Usuario Desconocido";
            }
        }
    }

    // Método privado para obtener el ID del usuario por nombre
    private async Task<int> GetUsuarioIdByNombre(string nombre)
    {
        using (SqlConnection conn = new SqlConnection(_connectionString))
        {
            try
            {
                await conn.OpenAsync();
                string query = "SELECT Id FROM Usuarios WHERE Nombre = @Nombre";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@Nombre", nombre);
                    var result = await cmd.ExecuteScalarAsync();
                    return result != null ? Convert.ToInt32(result) : 1; // Retorna 1 como fallback
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener ID del usuario {Nombre}", nombre);
                return 1; // Retorna 1 como fallback
            }
        }
    }
} 