using Microsoft.AspNetCore.SignalR;
using PandaDawRazor.Services;

namespace PandaDawRazor.Hubs;

public class NotificacionHub : Hub
{
    public async Task JoinGroup(string userId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, userId);
    }

    public async Task LeaveGroup(string userId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, userId);
    }

    public async Task JoinProductoGroup(long productoId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, $"producto_{productoId}");
    }
}

public class SignalRNotificacionService
{
    private readonly IHubContext<NotificacionHub> _hubContext;

    public SignalRNotificacionService(IHubContext<NotificacionHub> hubContext)
    {
        _hubContext = hubContext;
    }

    public async Task NotificarNuevaValoracionAsync(long productoId)
    {
        Console.WriteLine($"Broadcasting NuevaValoracion for producto {productoId}");
        await _hubContext.Clients.Group($"producto_{productoId}").SendAsync("NuevaValoracion", productoId);
    }

    public async Task NotificarCarritoActualizadoAsync(string userId, int totalItems)
    {
        await _hubContext.Clients.Group(userId).SendAsync("CarritoActualizado", userId, totalItems);
    }

    public async Task EnviarNotificacionAsync(string userId, Notificacion notif)
    {
        if (string.IsNullOrEmpty(userId))
        {
            await _hubContext.Clients.All.SendAsync("NotificacionRecibida", notif);
        }
        else
        {
            await _hubContext.Clients.Group(userId).SendAsync("NotificacionRecibida", notif);
        }
    }

    public async Task NotificarUsuarioActualizadoAsync(string userId)
    {
        await _hubContext.Clients.Group(userId).SendAsync("UsuarioActualizado", userId);
    }
}
