using Bunit;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using PandaBack.Dtos.Valoraciones;
using PandaBack.Services;
using PandaDawRazor.Services;
using PandaDawRazor.Components;
using CSharpFunctionalExtensions;
using PandaBack.Errors;
using Xunit;
using Assert = Xunit.Assert;

namespace Tests.Blazor;

public class ValoracionesRealtimeTests
{
    private readonly Mock<IValoracionService> _valoracionServiceMock;
    private readonly NotificacionService _notificacionService;
    private readonly Bunit.TestContext _ctx;

    public ValoracionesRealtimeTests()
    {
        _valoracionServiceMock = new Mock<IValoracionService>();
        _notificacionService = new NotificacionService();
        _ctx = new Bunit.TestContext();
        _ctx.Services.AddSingleton(_valoracionServiceMock.Object);
        _ctx.Services.AddSingleton(_notificacionService);
    }

    [Fact]
    public void RendersComponent()
    {
        _valoracionServiceMock
            .Setup(x => x.GetValoracionesByProductoAsync(It.IsAny<long>()))
            .ReturnsAsync(Result.Success<IEnumerable<ValoracionResponseDto>, PandaError>(
                new List<ValoracionResponseDto>()));

        var cut = _ctx.Render<ValoracionesRealtime>(parameters => parameters
            .Add(p => p.ProductoId, 1)
            .Add(p => p.UserId, "user1")
            .Add(p => p.EstaAutenticado, true)
            .Add(p => p.UsuarioNombre, "Test User"));

        var html = cut.Markup;
        Assert.Contains("Valoraciones", html);
    }

    [Fact]
    public void ShowsLoginPromptWhenNotAuthenticated()
    {
        _valoracionServiceMock
            .Setup(x => x.GetValoracionesByProductoAsync(It.IsAny<long>()))
            .ReturnsAsync(Result.Success<IEnumerable<ValoracionResponseDto>, PandaError>(
                new List<ValoracionResponseDto>()));

        var cut = _ctx.Render<ValoracionesRealtime>(parameters => parameters
            .Add(p => p.ProductoId, 1)
            .Add(p => p.UserId, (string?)null)
            .Add(p => p.EstaAutenticado, false));

        var html = cut.Markup;
        Assert.Contains("Inicia sesión", html);
    }

    [Fact]
    public void DisplaysValoraciones()
    {
        var valoraciones = new List<ValoracionResponseDto>
        {
            new ValoracionResponseDto
            {
                Id = 1,
                ProductoId = 1,
                UsuarioNombre = "John Doe",
                Estrellas = 5,
                Resena = "Great product!",
                Fecha = DateTime.Now
            }
        };

        _valoracionServiceMock
            .Setup(x => x.GetValoracionesByProductoAsync(1))
            .ReturnsAsync(Result.Success<IEnumerable<ValoracionResponseDto>, PandaError>(valoraciones));

        var cut = _ctx.Render<ValoracionesRealtime>(parameters => parameters
            .Add(p => p.ProductoId, 1)
            .Add(p => p.UserId, "user1")
            .Add(p => p.EstaAutenticado, true));

        var html = cut.Markup;
        Assert.Contains("John Doe", html);
    }
}

public class ContadorCarritoTests
{
    private readonly NotificacionService _notificacionService;
    private readonly Bunit.TestContext _ctx;

    public ContadorCarritoTests()
    {
        _notificacionService = new NotificacionService();
        _ctx = new Bunit.TestContext();
        _ctx.Services.AddSingleton(_notificacionService);
    }

    [Fact]
    public void RendersWithInitialCount()
    {
        var cut = _ctx.Render<ContadorCarrito>(parameters => parameters
            .Add(p => p.InitialCount, 5));

        var html = cut.Markup;
        Assert.Contains("5", html);
    }

    [Fact]
    public void ShowsZeroWhenEmpty()
    {
        var cut = _ctx.Render<ContadorCarrito>(parameters => parameters
            .Add(p => p.InitialCount, 0));

        var html = cut.Markup;
        Assert.Contains("0", html);
    }

    [Fact]
    public void Shows99PlusWhenOver99()
    {
        var cut = _ctx.Render<ContadorCarrito>(parameters => parameters
            .Add(p => p.InitialCount, 150));

        var html = cut.Markup;
        Assert.Contains("99+", html);
    }

    [Fact]
    public void UpdatesOnNotification()
    {
        var cut = _ctx.Render<ContadorCarrito>(parameters => parameters
            .Add(p => p.InitialCount, 1)
            .Add(p => p.UserId, "user1"));

        _notificacionService.NotificarCarritoActualizado("user1", 5);
        
        cut.WaitForAssertion(() => Assert.Contains("5", cut.Markup));
    }

    [Fact]
    public void IgnoresOtherUserUpdates()
    {
        var cut = _ctx.Render<ContadorCarrito>(parameters => parameters
            .Add(p => p.InitialCount, 1)
            .Add(p => p.UserId, "user1"));

        _notificacionService.NotificarCarritoActualizado("otherUser", 100);
        
        Assert.Contains("1", cut.Markup);
    }
}

public class NotificacionesToastTests
{
    private readonly NotificacionService _notificacionService;
    private readonly Bunit.TestContext _ctx;

    public NotificacionesToastTests()
    {
        _notificacionService = new NotificacionService();
        _ctx = new Bunit.TestContext();
        _ctx.Services.AddSingleton(_notificacionService);
    }

    [Fact]
    public void StartsEmpty()
    {
        var cut = _ctx.Render<NotificacionesToast>(parameters => parameters
            .Add(p => p.UserId, "user1"));

        Assert.DoesNotContain("notification", cut.Markup);
    }

    [Fact]
    public void DisplaysNotification()
    {
        var cut = _ctx.Render<NotificacionesToast>(parameters => parameters
            .Add(p => p.UserId, "user1"));

        _notificacionService.Enviar("user1", new Notificacion
        {
            Tipo = "success",
            Titulo = "Test Title",
            Mensaje = "Test Message",
            Icono = "fa-solid fa-check"
        });

        cut.WaitForAssertion(() => Assert.Contains("Test Title", cut.Markup));
    }

    [Fact]
    public void FiltersByUserId()
    {
        var cut = _ctx.Render<NotificacionesToast>(parameters => parameters
            .Add(p => p.UserId, "user1"));

        _notificacionService.Enviar("user2", new Notificacion
        {
            Titulo = "Should Not Appear"
        });

        Assert.DoesNotContain("Should Not Appear", cut.Markup);
    }

    [Fact]
    public void ShowsToAllWhenEmptyUserId()
    {
        var cut = _ctx.Render<NotificacionesToast>(parameters => parameters
            .Add(p => p.UserId, "user1"));

        _notificacionService.EnviarATodos(new Notificacion
        {
            Titulo = "Broadcast Message"
        });

        cut.WaitForAssertion(() => Assert.Contains("Broadcast Message", cut.Markup));
    }

    [Fact]
    public void CloseButtonRemovesNotification()
    {
        var cut = _ctx.Render<NotificacionesToast>(parameters => parameters
            .Add(p => p.UserId, "user1"));

        var notif = new Notificacion
        {
            Titulo = "To Be Closed",
            Mensaje = "Message"
        };

        _notificacionService.Enviar("user1", notif);

        cut.WaitForAssertion(() => Assert.Contains("To Be Closed", cut.Markup));

        var closeButton = cut.Find("button");
        closeButton.Click();

        cut.WaitForAssertion(() => Assert.DoesNotContain("To Be Closed", cut.Markup));
    }

    [Fact]
    public void MultipleNotifications()
    {
        var cut = _ctx.Render<NotificacionesToast>(parameters => parameters
            .Add(p => p.UserId, "user1"));

        _notificacionService.Enviar("user1", new Notificacion { Titulo = "Notif 1" });
        _notificacionService.Enviar("user1", new Notificacion { Titulo = "Notif 2" });
        _notificacionService.Enviar("user1", new Notificacion { Titulo = "Notif 3" });

        cut.WaitForAssertion(() => 
        {
            var html = cut.Markup;
            Assert.Contains("Notif 1", html);
            Assert.Contains("Notif 2", html);
            Assert.Contains("Notif 3", html);
        });
    }

    [Fact]
    public void DifferentTypesHaveCorrectStyles()
    {
        var cut = _ctx.Render<NotificacionesToast>(parameters => parameters
            .Add(p => p.UserId, "user1"));

        _notificacionService.Enviar("user1", new Notificacion { Tipo = "success", Titulo = "Success" });
        _notificacionService.Enviar("user1", new Notificacion { Tipo = "error", Titulo = "Error" });
        _notificacionService.Enviar("user1", new Notificacion { Tipo = "warning", Titulo = "Warning" });
        _notificacionService.Enviar("user1", new Notificacion { Tipo = "info", Titulo = "Info" });

        cut.WaitForAssertion(() =>
        {
            var html = cut.Markup;
            Assert.Contains("border-success", html);
            Assert.Contains("border-error", html);
            Assert.Contains("border-warning", html);
            Assert.Contains("border-info", html);
        });
    }
}
