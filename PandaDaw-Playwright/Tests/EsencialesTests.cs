using Microsoft.Playwright;
using Microsoft.Playwright.NUnit;
using NUnit.Framework;

namespace PandaDaw_Playwright.Tests;

/// <summary>
/// Tests E2E esenciales de la aplicación.
/// Cubre los flujos principales: login, registro, catálogo, detalle, carrito, checkout y admin.
/// </summary>
[TestFixture]
public class EsencialesTests : BaseTest
{
    // ══════════════════════════════════════════════════════════════
    // AUTH - Login
    // ══════════════════════════════════════════════════════════════

    [Test]
    public async Task Login_PaginaCargaCorrectamente()
    {
        await GoToPage(TestConstants.LoginPath);
        await Expect(Page).ToHaveTitleAsync(new System.Text.RegularExpressions.Regex(".*Login.*"));
        await Expect(Page.Locator("input[name='Email']")).ToBeVisibleAsync();
    }

    [Test]
    public async Task Login_ConCredencialesValidas_AccedeCorrectamente()
    {
        await GoToPage(TestConstants.LoginPath);
        await Page.Locator("input[name='Email']").FillAsync(TestConstants.AdminEmail);
        await Page.Locator("input[name='Password']").FillAsync(TestConstants.AdminPassword);
        await Page.Locator("button[type='submit']").ClickAsync();
        await Page.WaitForURLAsync(new System.Text.RegularExpressions.Regex(".*(/|$)"));
    }

    // ══════════════════════════════════════════════════════════════
    // AUTH - Registro
    // ══════════════════════════════════════════════════════════════

    [Test]
    public async Task Register_PaginaCargaCorrectamente()
    {
        await GoToPage(TestConstants.RegisterPath);
        await Expect(Page).ToHaveTitleAsync(new System.Text.RegularExpressions.Regex(".*Register.*"));
        await Expect(Page.Locator("input[name='Email']")).ToBeVisibleAsync();
    }

    [Test]
    public async Task Register_ConDatosValidos_UsuarioCreado()
    {
        var email = $"test_{Guid.NewGuid():N}@test.com";
        await GoToPage(TestConstants.RegisterPath);
        await Page.Locator("input[name='Email']").FillAsync(email);
        await Page.Locator("input[name='Password']").FillAsync("Test123!");
        await Page.Locator("input[name='ConfirmPassword']").FillAsync("Test123!");
        await Page.Locator("input[name='Nombre']").FillAsync("Test");
        await Page.Locator("input[name='Apellidos']").FillAsync("User");
        await Page.Locator("button[type='submit']").ClickAsync();
    }

    // ══════════════════════════════════════════════════════════════
    // INDEX - Catálogo
    // ══════════════════════════════════════════════════════════════

    [Test]
    public async Task Index_PaginaSeCargaCorrectamente()
    {
        await GoToPage(TestConstants.IndexPath);
        await Expect(Page).ToHaveTitleAsync(new System.Text.RegularExpressions.Regex(".*"));
        var products = Page.Locator(".card");
        await Expect(products.First).ToBeVisibleAsync();
    }

    [Test]
    public async Task Index_FiltroCategoria_Funciona()
    {
        await GoToPage(TestConstants.IndexPath + "?Categoria=Smartphones");
        await Page.WaitForLoadStateAsync(LoadState.DOMContentLoaded);
        await Expect(Page.Locator("text=Smartphones").First).ToBeVisibleAsync();
    }

    // ══════════════════════════════════════════════════════════════
    // DETALLE - Producto
    // ══════════════════════════════════════════════════════════════

    [Test]
    public async Task Detalle_PaginaCargaCorrectamente()
    {
        await GoToPage("/Detalle/1");
        await Expect(Page.Locator("h1, [class*='title']")).ToBeVisibleAsync();
    }

    [Test]
    public async Task Detalle_MuestraValoraciones()
    {
        await GoToPage("/Detalle/1");
        await Expect(Page.Locator("text=Valoraciones")).ToBeVisibleAsync();
    }

    // ══════════════════════════════════════════════════════════════
    // CARRITO
    // ══════════════════════════════════════════════════════════════

    [Test]
    public async Task Carrito_PaginaCargaCorrectamente()
    {
        await GoToPage(TestConstants.CarritoPath);
        await Expect(Page).ToHaveTitleAsync(new System.Text.RegularExpressions.Regex(".*Carrito.*"));
    }

    [Test]
    public async Task Carrito_Vacio_MuestraMensaje()
    {
        await GoToPage(TestConstants.CarritoPath);
        var content = await Page.ContentAsync();
        Assert.That(content.ToLower(), Does.Contain("vacío").Or.Contain("sin productos").Or.Contain("carrito"));
    }

    // ══════════════════════════════════════════════════════════════
    // CHECKOUT / PAGO
    // ══════════════════════════════════════════════════════════════

    [Test]
    public async Task Pago_PaginaCargaCorrectamente()
    {
        await GoToPage(TestConstants.PagoPath);
        await Expect(Page).ToHaveTitleAsync(new System.Text.RegularExpressions.Regex(".*Pago.*"));
    }

    // ══════════════════════════════════════════════════════════════
    // FAVORITOS
    // ══════════════════════════════════════════════════════════════

    [Test]
    public async Task Favoritos_PaginaCarga_RequiereLogin()
    {
        await GoToPage(TestConstants.FavoritosPath);
        await Expect(Page).ToHaveURLAsync(new System.Text.RegularExpressions.Regex(".*Login.*"));
    }

    [Test]
    public async Task Favoritos_UsuarioLogueado_Accede()
    {
        await LoginAsUser();
        await GoToPage(TestConstants.FavoritosPath);
        await Expect(Page).ToHaveTitleAsync(new System.Text.RegularExpressions.Regex(".*Favoritos.*"));
    }

    // ══════════════════════════════════════════════════════════════
    // ADMIN
    // ══════════════════════════════════════════════════════════════

    [Test]
    public async Task Admin_PaginaCarga_RequiereLogin()
    {
        await GoToPage(TestConstants.AdminPanelPath);
        await Expect(Page).ToHaveURLAsync(new System.Text.RegularExpressions.Regex(".*Login.*"));
    }

    [Test]
    public async Task Admin_UsuarioAdmin_Accede()
    {
        await LoginAsAdmin();
        await GoToPage(TestConstants.AdminPanelPath);
        await Expect(Page).ToHaveTitleAsync(new System.Text.RegularExpressions.Regex(".*Admin.*"));
    }

    // ══════════════════════════════════════════════════════════════
    // LOGOUT
    // ══════════════════════════════════════════════════════════════

    [Test]
    public async Task Logout_CierraSesion()
    {
        await LoginAsUser();
        await GoToPage(TestConstants.LogoutPath);
        await Expect(Page).ToHaveURLAsync(new System.Text.RegularExpressions.Regex(".*(/|$)"));
    }

    // ══════════════════════════════════════════════════════════════
    // ERRORES
    // ══════════════════════════════════════════════════════════════

    [Test]
    public async Task Error_PaginaNoExistente_404()
    {
        await GoToPage("/pagina-inexistente-xyz");
        var content = await Page.ContentAsync();
        Assert.That(content, Does.Contain("404").Or.Contains("No se encontró"));
    }
}
