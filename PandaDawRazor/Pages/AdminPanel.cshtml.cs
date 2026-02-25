using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PandaBack.Models;
using PandaBack.Services;
using PandaDawRazor.Services;

namespace PandaDawRazor.Pages;

public class AdminPanelModel : PageModel
{
    private readonly IProductoService _productoService;
    private readonly NotificacionService _notificacionService;

    public AdminPanelModel(IProductoService productoService, NotificacionService notificacionService)
    {
        _productoService = productoService;
        _notificacionService = notificacionService;
    }

    public List<Producto> Productos { get; set; } = new();
    public string? ErrorMessage { get; set; }
    public string? SuccessMessage { get; set; }

    [BindProperty]
    public ProductoInputModel ProductoInput { get; set; } = new();

    [BindProperty(SupportsGet = true)]
    public string? Filtro { get; set; }

    [BindProperty(SupportsGet = true)]
    public bool MostrarEliminados { get; set; }

    public async Task<IActionResult> OnGetAsync()
    {
        // Verificar que el usuario sea admin
        var userRole = HttpContext.Session.GetString("UserRole");
        if (userRole != "Admin")
        {
            return RedirectToPage("/Index");
        }

        await CargarProductos();
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

        var producto = new Producto
        {
            Nombre = ProductoInput.Nombre,
            Descripcion = ProductoInput.Descripcion,
            Precio = ProductoInput.Precio,
            Stock = ProductoInput.Stock,
            Category = categoria,
            Imagen = ProductoInput.ImagenUrl
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

        var producto = new Producto
        {
            Nombre = ProductoInput.Nombre,
            Descripcion = ProductoInput.Descripcion,
            Precio = ProductoInput.Precio,
            Stock = ProductoInput.Stock,
            Category = categoria,
            Imagen = ProductoInput.ImagenUrl
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

    public class ProductoInputModel
    {
        public string Nombre { get; set; } = string.Empty;
        public string? Descripcion { get; set; }
        public decimal Precio { get; set; }
        public int Stock { get; set; }
        public string Categoria { get; set; } = string.Empty;
        public string? ImagenUrl { get; set; }
    }
}
