using Microsoft.Playwright;
using Microsoft.Playwright.NUnit;
using NUnit.Framework;
using System.Text.RegularExpressions;

namespace PandaDaw_Playwright.Tests;

/// <summary>
/// Tests E2E de la página de Pago / Checkout (/Pago).
/// Cubre: acceso protegido, steps visuales, formulario de envío, métodos de pago,
/// datos de tarjeta, resumen lateral, confirmar pago.
/// </summary>
[TestFixture]
public class PagoTests : BaseTest
{
    /// <summary>
    /// Helper: añade un producto al carrito y navega a pago.
    /// </summary>
    private async Task PrepararCarritoYNavegar()
    {
        // Añadir un producto al carrito
        await GoToPage("/Detalle/1");
        var addBtn = Page.Locator("form[action*='AddToCart'] button, button:has-text('carrito'), button:has-text('Añadir')").First;
        await addBtn.ClickAsync();
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        // Ir a la página de pago
        await GoToPage(TestConstants.PagoPath);
    }

    // ══════════════════════════════════════════════════════════════
    // ACCESO SIN LOGIN → REDIRIGE A LOGIN
    // ══════════════════════════════════════════════════════════════

    [Test]
    public async Task Pago_SinLogin_RedirigeALogin()
    {
        await GoToPage(TestConstants.PagoPath);
        await Expect(Page).ToHaveURLAsync(new System.Text.RegularExpressions.Regex("Login"));
    }

    // ══════════════════════════════════════════════════════════════
    // CARGA Y ESTRUCTURA
    // ══════════════════════════════════════════════════════════════

    [Test]
    public async Task Pago_ConLogin_PaginaSeCarga()
    {
        await LoginAsUser();
        await PrepararCarritoYNavegar();
        await Expect(Page).ToHaveURLAsync(new System.Text.RegularExpressions.Regex("Pago"));
    }

    [Test]
    public async Task Pago_MuestraStepsVisuales()
    {
        await LoginAsUser();
        await PrepararCarritoYNavegar();

        var pageText = await Page.Locator("body").TextContentAsync();
        var tieneSteps = pageText!.Contains("Carrito", StringComparison.OrdinalIgnoreCase)
                         && pageText.Contains("Pago", StringComparison.OrdinalIgnoreCase);
        Assert.That(tieneSteps, Is.True, "Debe mostrar los pasos del checkout");
    }

    // ══════════════════════════════════════════════════════════════
    // FORMULARIO DE DATOS DE ENVÍO
    // ══════════════════════════════════════════════════════════════

    [Test]
    public async Task Pago_FormularioEnvio_CamposExisten()
    {
        await LoginAsUser();
        await PrepararCarritoYNavegar();

        // El formulario puede necesitar esperar a que esté visible
        await Page.WaitForSelectorAsync("#pagoForm, form");
        var pagoForm = Page.Locator("#pagoForm, form").First;
        await Expect(pagoForm).ToBeVisibleAsync();

        // Verificar campos de envío
        var pageContent = await Page.ContentAsync();
        Assert.That(pageContent, Does.Contain("nombre").IgnoreCase, "Debe tener campo nombre");
        Assert.That(pageContent, Does.Contain("email").IgnoreCase.Or.Contain("correo").IgnoreCase,
            "Debe tener campo email");
    }

    // ══════════════════════════════════════════════════════════════
    // MÉTODOS DE PAGO (radio buttons)
    // ══════════════════════════════════════════════════════════════

    [Test]
    public async Task Pago_MetodosDePago_RadioButtonsExisten()
    {
        await LoginAsUser();
        await PrepararCarritoYNavegar();

        // Los métodos de pago pueden mostrarse después de añadir productos
        var metodoRadios = Page.Locator("input[name='metodo'], input[type='radio']");
        var count = await metodoRadios.CountAsync();
        Assert.That(count, Is.GreaterThan(0), "Debe haber al menos 1 método de pago");
    }

    [Test]
    public async Task Pago_SeleccionarTarjeta_MuestraDatosTarjeta()
    {
        await LoginAsUser();
        await PrepararCarritoYNavegar();

        // Seleccionar método tarjeta
        var tarjetaRadio = Page.Locator("input[value='tarjeta']");
        if (await tarjetaRadio.IsVisibleAsync())
        {
            await tarjetaRadio.CheckAsync(new() { Force = true });

            // Verificar que aparecen los campos de tarjeta
            var datosTarjeta = Page.Locator("#datosTarjeta");
            await Expect(datosTarjeta).ToBeVisibleAsync();
        }
    }

    [Test]
    public async Task Pago_SeleccionarPaypal_OcultaDatosTarjeta()
    {
        await LoginAsUser();
        await PrepararCarritoYNavegar();

        var paypalRadio = Page.Locator("input[value='paypal']");
        if (await paypalRadio.IsVisibleAsync())
        {
            await paypalRadio.CheckAsync(new() { Force = true });
            // Los datos de tarjeta deberían ocultarse o no ser obligatorios
            var datosTarjeta = Page.Locator("#datosTarjeta");
            await Expect(datosTarjeta).ToBeHiddenAsync();
        }
    }

    [Test]
    public async Task Pago_SeleccionarBizum_OcultaDatosTarjeta()
    {
        await LoginAsUser();
        await PrepararCarritoYNavegar();

        var bizumRadio = Page.Locator("input[value='bizum']");
        if (await bizumRadio.IsVisibleAsync())
        {
            await bizumRadio.CheckAsync(new() { Force = true });
            var datosTarjeta = Page.Locator("#datosTarjeta");
            await Expect(datosTarjeta).ToBeHiddenAsync();
        }
    }

    // ══════════════════════════════════════════════════════════════
    // RESUMEN LATERAL
    // ══════════════════════════════════════════════════════════════

    [Test]
    public async Task Pago_ResumenLateral_MuestraTotalEIva()
    {
        await LoginAsUser();
        await PrepararCarritoYNavegar();

        var pageText = await Page.Locator("body").TextContentAsync();
        Assert.That(pageText, Does.Contain("€"), "Debe mostrar precios");
        var tieneResumen = pageText!.Contains("Total", StringComparison.OrdinalIgnoreCase)
                           || pageText.Contains("IVA", StringComparison.OrdinalIgnoreCase);
        Assert.That(tieneResumen, Is.True, "Debe mostrar total o IVA");
    }

    // ══════════════════════════════════════════════════════════════
    // CONFIRMAR PAGO EXITOSO
    // ══════════════════════════════════════════════════════════════

    [Test]
    public async Task Pago_CompletarFormularioYPagar_MuestraConfirmacion()
    {
        await LoginAsUser();
        await PrepararCarritoYNavegar();

        // Rellenar datos de envío usando GetByLabel para evitar el token CSRF
        var nombreField = Page.GetByLabel(new Regex("ombre", RegexOptions.IgnoreCase));
        if (await nombreField.CountAsync() == 0)
            nombreField = Page.Locator("#pagoForm input[type='text']").First;
        await nombreField.FillAsync("Test");

        var apellidoField = Page.GetByLabel(new Regex("apellido", RegexOptions.IgnoreCase));
        if (await apellidoField.CountAsync() == 0)
            apellidoField = Page.Locator("#pagoForm input[type='text']").Nth(1);
        await apellidoField.FillAsync("Playwright");

        var emailField = Page.GetByLabel(new Regex("mail|correo", RegexOptions.IgnoreCase));
        if (await emailField.CountAsync() == 0)
            emailField = Page.Locator("#pagoForm input[type='email']").First;
        if (await emailField.IsVisibleAsync())
            await emailField.FillAsync("test@pw.com");

        var dirField = Page.GetByLabel(new Regex("direcci", RegexOptions.IgnoreCase));
        if (await dirField.CountAsync() == 0)
            dirField = Page.Locator("#pagoForm input[type='text']").Nth(2);
        if (await dirField.IsVisibleAsync())
            await dirField.FillAsync("Calle Playwright 123");

        var cpField = Page.GetByLabel(new Regex("postal|código", RegexOptions.IgnoreCase));
        if (await cpField.CountAsync() == 0)
            cpField = Page.Locator("#pagoForm input[maxlength='5']").First;
        if (await cpField.IsVisibleAsync())
            await cpField.FillAsync("28001");

        var ciudadField = Page.GetByLabel(new Regex("ciudad", RegexOptions.IgnoreCase));
        if (await ciudadField.CountAsync() == 0)
            ciudadField = Page.Locator("#pagoForm input[type='text']").Nth(3);
        if (await ciudadField.IsVisibleAsync())
            await ciudadField.FillAsync("Madrid");

        // Seleccionar PayPal (no requiere datos de tarjeta)
        var paypalRadio = Page.Locator("input[value='paypal'], input[value*='PayPal']");
        if (await paypalRadio.IsVisibleAsync())
        {
            await paypalRadio.CheckAsync(new() { Force = true });
        }

        // Click en pagar
        var pagarBtn = Page.Locator("#pagoForm button[type='submit'], button:has-text('Pagar')").First;
        await pagarBtn.ClickAsync();
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        // Tras pago exitoso debe mostrar confirmación con nº pedido
        var pageText = await Page.Locator("body").TextContentAsync();
        var esConfirmacion = pageText!.Contains("pedido", StringComparison.OrdinalIgnoreCase)
                             || pageText.Contains("confirmación", StringComparison.OrdinalIgnoreCase)
                             || pageText.Contains("confirmacion", StringComparison.OrdinalIgnoreCase)
                             || pageText.Contains("gracias", StringComparison.OrdinalIgnoreCase)
                             || pageText.Contains("éxito", StringComparison.OrdinalIgnoreCase);
        Assert.That(esConfirmacion, Is.True, "Tras pagar debe mostrar confirmación");
    }

    // ══════════════════════════════════════════════════════════════
    // PAGO CON TARJETA - CAMPOS DE TARJETA
    // ══════════════════════════════════════════════════════════════

    [Test]
    public async Task Pago_DatosTarjeta_CamposTienenMaxLength()
    {
        await LoginAsUser();
        await PrepararCarritoYNavegar();

        // Seleccionar tarjeta
        var tarjetaRadio = Page.Locator("input[value='tarjeta']");
        if (await tarjetaRadio.IsVisibleAsync())
        {
            await tarjetaRadio.CheckAsync(new() { Force = true });

            // Verificar maxlength de campos
            var numTarjeta = Page.Locator("#datosTarjeta input[maxlength='19']");
            await Expect(numTarjeta).ToBeVisibleAsync();

            var cvv = Page.Locator("#datosTarjeta input[type='password'], #datosTarjeta input[maxlength='4']").First;
            await Expect(cvv).ToBeVisibleAsync();
        }
    }
}
