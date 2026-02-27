using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PandaBack.Dtos.Ventas;
using PandaBack.Models;
using PandaBack.Services;
using PandaBack.Services.Factura;

namespace PandaDawRazor.Pages;

public class PedidosModel : PageModel
{
    private readonly IVentaService _ventaService;
    private readonly IFacturaService _facturaService;

    public PedidosModel(IVentaService ventaService, IFacturaService facturaService)
    {
        _ventaService = ventaService;
        _facturaService = facturaService;
    }

    public List<VentaResponseDto> Pedidos { get; set; } = new();
    public string? UserId => HttpContext.Session.GetString("UserId");

    public async Task<IActionResult> OnGetAsync()
    {
        if (string.IsNullOrEmpty(UserId))
        {
            return RedirectToPage("/Login");
        }

        var result = await _ventaService.GetVentasByUserAsync(UserId);
        if (result.IsSuccess)
        {
            Pedidos = result.Value.ToList();
        }
        return Page();
    }

    public async Task<IActionResult> OnGetDescargarFacturaAsync(long id)
    {
        if (string.IsNullOrEmpty(UserId))
        {
            return RedirectToPage("/Login");
        }

        var result = await _ventaService.GetVentaByIdAsync(id);
        if (result.IsFailure)
        {
            return NotFound();
        }

        var venta = result.Value;

        // Solo el dueño puede descargar su factura
        if (venta.UsuarioId != UserId)
        {
            return Forbid();
        }

        var pdfBytes = _facturaService.GenerarFacturaPdf(venta);
        return File(pdfBytes, "application/pdf", $"Factura_PandaDaw_{id:D6}.pdf");
    }

    public async Task<IActionResult> OnGetConfirmarEntregaAsync(long id)
    {
        if (string.IsNullOrEmpty(UserId))
        {
            return RedirectToPage("/Login");
        }

        var result = await _ventaService.GetVentaByIdAsync(id);
        if (result.IsFailure)
        {
            return NotFound();
        }

        var venta = result.Value;

        // Solo el dueño puede confirmar la entrega
        if (venta.UsuarioId != UserId)
        {
            return Forbid();
        }

        // Solo puede confirmar si está en estado "Enviado"
        if (venta.Estado != "Enviado")
        {
            TempData["Error"] = "Solo puedes confirmar la entrega de pedidos que han sido enviados.";
            return RedirectToPage();
        }

        await _ventaService.UpdateEstadoVentaAsync(id, EstadoPedido.Entregado);
        return RedirectToPage();
    }
}
