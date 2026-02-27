using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PandaBack.Dtos.Ventas;
using PandaBack.Models;
using PandaBack.Services;
using PandaDawRazor.Hubs;
using PandaDawRazor.Services;


namespace PandaDawRazor.Pages;

[RequestFormLimits(MultipartBodyLengthLimit = 52428800)]
[RequestSizeLimit(52428800)]
public class AdminPanelModel : PageModel
{
    private readonly IProductoService _productoService;
    private readonly IVentaService _ventaService;
    private readonly NotificacionService _notificacionService;
    private readonly SignalRNotificacionService _signalRNotificacionService;
    private readonly IWebHostEnvironment _env;
    private readonly UserManager<User> _userManager;

    public AdminPanelModel(IProductoService productoService, IVentaService ventaService, NotificacionService notificacionService, SignalRNotificacionService signalRNotificacionService, IWebHostEnvironment env, UserManager<User> userManager)
    {
        _productoService = productoService;
        _ventaService = ventaService;
        _notificacionService = notificacionService;
        _signalRNotificacionService = signalRNotificacionService;
        _env = env;
        _userManager = userManager;
    }

    public List<Producto> Productos { get; set; } = new();
    public List<VentaResponseDto> Ventas { get; set; } = new();
    public List<User> Usuarios { get; set; } = new();
    public string? ErrorMessage { get; set; }
    public string? SuccessMessage { get; set; }

    [BindProperty]
    public ProductoInputModel ProductoInput { get; set; } = new();

    [BindProperty]
    public PedidoInputModel PedidoInput { get; set; } = new();

    [BindProperty]
    public UsuarioInputModel UsuarioInput { get; set; } = new();

    [BindProperty(SupportsGet = true)]
    public string? Filtro { get; set; }

    [BindProperty(SupportsGet = true)]
    public bool MostrarEliminados { get; set; }

    [BindProperty(SupportsGet = true)]
    public string? Tab { get; set; }

    [BindProperty(SupportsGet = true)]
    public string? FiltroUsuario { get; set; }

    public List<User> UsuariosFiltrados { get; set; } = new();
    public List<UsuarioAdminDto> UsuariosConRoles { get; set; } = new();

    public class UsuarioAdminDto
    {
        public User Usuario { get; set; } = null!;
        public string Role { get; set; } = "Usuario";
        public bool EstaBloqueado { get; set; }
    }

    public async Task<IActionResult> OnGetAsync()
    {
        // Verificar que el usuario sea admin
        var userRole = HttpContext.Session.GetString("UserRole");
        if (userRole != "Admin")
        {
            return RedirectToPage("/Index");
        }

        await CargarProductos();
        await CargarVentas();
        await CargarUsuarios();
        return Page();
    }

    public async Task<IActionResult> OnPostCrearAsync()
    {
        var userRole = HttpContext.Session.GetString("UserRole");
        if (userRole != "Admin")
        {
            return RedirectToPage("/Index");
        }

        if (!Enum.TryParse<Categoria>(ProductoInput.Categoria, out var categoria))
        {
            ErrorMessage = "Categoría no válida";
            await CargarProductos();
            return Page();
        }

        // Procesar imagen subida
        string? imagenPath = null;
        if (ProductoInput.ImagenArchivo != null && ProductoInput.ImagenArchivo.Length > 0)
        {
            imagenPath = await GuardarImagenAsync(ProductoInput.ImagenArchivo);
        }

        var producto = new Producto
        {
            Nombre = ProductoInput.Nombre,
            Descripcion = ProductoInput.Descripcion,
            Precio = ProductoInput.Precio,
            Stock = ProductoInput.Stock,
            Category = categoria,
            Imagen = imagenPath
        };

        var result = await _productoService.CreateProductoAsync(producto);
        
        if (result.IsSuccess)
        {
            SuccessMessage = "Producto creado correctamente";
            _notificacionService.EnviarATodos(new Notificacion
            {
                Tipo = "success",
                Titulo = "Producto creado",
                Mensaje = $"Se ha creado el producto: {producto.Nombre}",
                Icono = "fa-solid fa-plus-circle"
            });
            
            try { await _signalRNotificacionService.EnviarNotificacionAsync("", new Notificacion { Tipo = "success", Titulo = "Producto creado", Mensaje = $"Nuevo producto: {producto.Nombre}", Icono = "fa-solid fa-plus-circle" }); } catch { }
        }
        else
        {
            ErrorMessage = result.Error.Message;
        }

        await CargarProductos();
        return Page();
    }

    public async Task<IActionResult> OnPostEditarAsync(long id)
    {
        var userRole = HttpContext.Session.GetString("UserRole");
        if (userRole != "Admin")
        {
            return RedirectToPage("/Index");
        }

        if (!Enum.TryParse<Categoria>(ProductoInput.Categoria, out var categoria))
        {
            ErrorMessage = "Categoría no válida";
            await CargarProductos();
            return Page();
        }

        // Procesar imagen subida (si hay nueva)
        string? imagenPath = ProductoInput.ImagenActual;
        if (ProductoInput.ImagenArchivo != null && ProductoInput.ImagenArchivo.Length > 0)
        {
            try
            {
                imagenPath = await GuardarImagenAsync(ProductoInput.ImagenArchivo);
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error al subir la imagen: {ex.Message}";
                await CargarProductos();
                return Page();
            }
        }

        var producto = new Producto
        {
            Nombre = ProductoInput.Nombre,
            Descripcion = ProductoInput.Descripcion,
            Precio = ProductoInput.Precio,
            Stock = ProductoInput.Stock,
            Category = categoria,
            Imagen = imagenPath
        };

        var result = await _productoService.UpdateProductoAsync(id, producto);
        
        if (result.IsSuccess)
        {
            SuccessMessage = "Producto actualizado correctamente";
            _notificacionService.EnviarATodos(new Notificacion
            {
                Tipo = "info",
                Titulo = "Producto actualizado",
                Mensaje = $"Se ha actualizado: {producto.Nombre}",
                Icono = "fa-solid fa-pen-to-square"
            });
        }
        else
        {
            ErrorMessage = result.Error.Message;
        }

        await CargarProductos();
        return Page();
    }

    public async Task<IActionResult> OnPostEliminarAsync(long id)
    {
        var userRole = HttpContext.Session.GetString("UserRole");
        if (userRole != "Admin")
        {
            return RedirectToPage("/Index");
        }

var result = await _productoService.DeleteProductoAsync(id);
        
        if (result.IsSuccess)
        {
            SuccessMessage = "Producto eliminado correctamente";
            _notificacionService.EnviarATodos(new Notificacion
            {
                Tipo = "error",
                Titulo = "Producto eliminado",
                Mensaje = "Un producto ha sido eliminado del catálogo",
                Icono = "fa-solid fa-trash"
            });
        }
        else
        {
            ErrorMessage = result.Error.Message;
        }

        await CargarProductos();
        return Page();
    }

    public async Task<IActionResult> OnPostRestaurarAsync(long id)
    {
        var userRole = HttpContext.Session.GetString("UserRole");
        if (userRole != "Admin")
        {
            return RedirectToPage("/Index");
        }

        // Obtener el producto y restaurarlo
        var productoResult = await _productoService.GetProductoByIdAsync(id);
        if (productoResult.IsSuccess)
        {
            var producto = productoResult.Value;
            producto.IsDeleted = false;
var result = await _productoService.UpdateProductoAsync(id, producto);
            
            if (result.IsSuccess)
            {
                SuccessMessage = "Producto restaurado correctamente";
                _notificacionService.EnviarATodos(new Notificacion
                {
                    Tipo = "success",
                    Titulo = "Producto restaurado",
                    Mensaje = $"El producto {producto.Nombre} ha sido restaurado",
                    Icono = "fa-solid fa-trash-arrow-up"
                });
            }
            else
            {
                ErrorMessage = result.Error.Message;
            }
        }
        else
        {
            ErrorMessage = productoResult.Error.Message;
        }

        await CargarProductos();
        return Page();
    }

private async Task CargarProductos()
    {
        var result = await _productoService.GetAllProductosAsync();
        if (result.IsSuccess)
        {
            Productos = result.Value.ToList();

            // Filtrar por texto si hay filtro
            if (!string.IsNullOrEmpty(Filtro))
            {
                Productos = Productos
                    .Where(p => p.Nombre.Contains(Filtro, StringComparison.OrdinalIgnoreCase))
                    .ToList();
            }

            // Filtrar eliminados según opción
            if (!MostrarEliminados)
            {
                Productos = Productos.Where(p => !p.IsDeleted).ToList();
            }

            // Ordenar por fecha de alta descendente
            Productos = Productos.OrderByDescending(p => p.FechaAlta).ToList();
        }
    }

    private async Task CargarVentas()
    {
        var result = await _ventaService.GetAllVentasAsync();
        if (result.IsSuccess)
        {
            Ventas = result.Value.ToList();
            // Ordenar por fecha descendente
            Ventas = Ventas.OrderByDescending(v => v.FechaCompra).ToList();
        }
    }

    private async Task CargarUsuarios()
    {
        var users = _userManager.Users.ToList();
        Usuarios = users.OrderBy(u => u.UserName).ToList();

        // Cargar roles y estado de bloqueo
        UsuariosConRoles = new List<UsuarioAdminDto>();
        foreach (var user in users)
        {
            var roles = await _userManager.GetRolesAsync(user);
            var dto = new UsuarioAdminDto
            {
                Usuario = user,
                Role = roles.FirstOrDefault() ?? "Usuario",
                EstaBloqueado = user.LockoutEnd.HasValue && user.LockoutEnd > DateTimeOffset.Now
            };
            UsuariosConRoles.Add(dto);
        }

        // Filtrar si hay filtro
        if (!string.IsNullOrEmpty(FiltroUsuario))
        {
            UsuariosFiltrados = Usuarios
                .Where(u => (u.UserName?.Contains(FiltroUsuario, StringComparison.OrdinalIgnoreCase) ?? false) ||
                            (u.Email?.Contains(FiltroUsuario, StringComparison.OrdinalIgnoreCase) ?? false))
                .ToList();
            UsuariosConRoles = UsuariosConRoles
                .Where(u => (u.Usuario.UserName?.Contains(FiltroUsuario, StringComparison.OrdinalIgnoreCase) ?? false) ||
                            (u.Usuario.Email?.Contains(FiltroUsuario, StringComparison.OrdinalIgnoreCase) ?? false))
                .ToList();
        }
    }

    public async Task<IActionResult> OnPostActualizarEstadoAsync(long ventaId)
    {
        var userRole = HttpContext.Session.GetString("UserRole");
        if (userRole != "Admin")
        {
            return RedirectToPage("/Index");
        }

        if (!Enum.TryParse<EstadoPedido>(PedidoInput.NuevoEstado, out var nuevoEstado))
        {
            ErrorMessage = "Estado no válido";
            await CargarProductos();
            await CargarVentas();
            await CargarUsuarios();
            return Page();
        }

        var result = await _ventaService.UpdateEstadoVentaAsync(ventaId, nuevoEstado);
        
        if (result.IsSuccess)
        {
            SuccessMessage = $"Pedido #{ventaId} actualizado a {nuevoEstado}";
        }
        else
        {
            ErrorMessage = result.Error.Message;
        }

        await CargarProductos();
        await CargarVentas();
        await CargarUsuarios();
        return Page();
    }

    /// <summary>
    /// Guarda el archivo de imagen en wwwroot/img/productos/ y devuelve la ruta relativa.
    /// </summary>
    private async Task<string> GuardarImagenAsync(IFormFile archivo)
    {
        var uploadsDir = Path.Combine(_env.WebRootPath, "img", "productos");
        Directory.CreateDirectory(uploadsDir);

        var extension = Path.GetExtension(archivo.FileName).ToLowerInvariant();
        var nombreArchivo = $"{Guid.NewGuid()}{extension}";
        var rutaCompleta = Path.Combine(uploadsDir, nombreArchivo);

        await using var stream = new FileStream(rutaCompleta, FileMode.Create);
        await archivo.CopyToAsync(stream);

        return $"/img/productos/{nombreArchivo}";
    }

    public class ProductoInputModel
    {
        public string Nombre { get; set; } = string.Empty;
        public string? Descripcion { get; set; }
        public decimal Precio { get; set; }
        public int Stock { get; set; }
        public string Categoria { get; set; } = string.Empty;
        public IFormFile? ImagenArchivo { get; set; }
        public string? ImagenActual { get; set; }
    }

    public class PedidoInputModel
    {
        public string NuevoEstado { get; set; } = string.Empty;
    }

    public class UsuarioInputModel
    {
        public string UserId { get; set; } = string.Empty;
    }

    public async Task<IActionResult> OnPostEliminarUsuarioAsync(string userId)
    {
        var userRole = HttpContext.Session.GetString("UserRole");
        if (userRole != "Admin")
        {
            return RedirectToPage("/Index");
        }

        var user = await _userManager.FindByIdAsync(userId);
        if (user != null)
        {
            // No permitir eliminar al propio admin
            var currentUserId = HttpContext.Session.GetString("UserId");
            if (user.Id == currentUserId)
            {
                ErrorMessage = "No puedes eliminarte a ti mismo";
                await CargarUsuarios();
                return Page();
            }

            var result = await _userManager.DeleteAsync(user);
            if (result.Succeeded)
            {
                SuccessMessage = $"Usuario {user.UserName} eliminado";
                _notificacionService.EnviarATodos(new Notificacion
                {
                    Tipo = "error",
                    Titulo = "Usuario eliminado",
                    Mensaje = $"El usuario {user.UserName} ha sido eliminado del sistema",
                    Icono = "fa-solid fa-user-slash"
                });
            }
            else
            {
                ErrorMessage = "Error al eliminar usuario";
            }
        }
        else
        {
            ErrorMessage = "Usuario no encontrado";
        }

        await CargarUsuarios();
        return Page();
    }

    public async Task<IActionResult> OnPostBloquearUsuarioAsync(string userId)
    {
        var userRole = HttpContext.Session.GetString("UserRole");
        if (userRole != "Admin")
        {
            return RedirectToPage("/Index");
        }

        var user = await _userManager.FindByIdAsync(userId);
        if (user != null)
        {
            user.LockoutEnd = DateTimeOffset.Now.AddYears(100);
            await _userManager.UpdateAsync(user);
            SuccessMessage = $"Usuario {user.UserName} bloqueado";
            
            _notificacionService.Enviar(user.Id, new Notificacion
            {
                Tipo = "error",
                Titulo = "Cuenta bloqueada",
                Mensaje = "Tu cuenta ha sido bloqueada por un administrador",
                Icono = "fa-solid fa-lock"
            });
            
            try { await _signalRNotificacionService.EnviarNotificacionAsync(user.Id, new Notificacion { Tipo = "error", Titulo = "Cuenta bloqueada", Mensaje = "Tu cuenta ha sido bloqueada", Icono = "fa-solid fa-lock" }); } catch { }
        }

        await CargarUsuarios();
        return Page();
    }

    public async Task<IActionResult> OnPostDesbloquearUsuarioAsync(string userId)
    {
        var userRole = HttpContext.Session.GetString("UserRole");
        if (userRole != "Admin")
        {
            return RedirectToPage("/Index");
        }

        var user = await _userManager.FindByIdAsync(userId);
        if (user != null)
        {
            user.LockoutEnd = null;
            await _userManager.UpdateAsync(user);
            SuccessMessage = $"Usuario {user.UserName} desbloqueado";
            
            _notificacionService.Enviar(user.Id, new Notificacion
            {
                Tipo = "success",
                Titulo = "Cuenta desbloqueada",
                Mensaje = "Tu cuenta ha sido desbloqueada",
                Icono = "fa-solid fa-unlock"
            });
            
            try { await _signalRNotificacionService.EnviarNotificacionAsync(user.Id, new Notificacion { Tipo = "success", Titulo = "Cuenta desbloqueada", Mensaje = "Tu cuenta ha sido desbloqueada", Icono = "fa-solid fa-unlock" }); } catch { }
        }

        await CargarUsuarios();
        return Page();
    }
}
