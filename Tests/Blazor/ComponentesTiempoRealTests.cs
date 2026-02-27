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

#pragma warning disable CS0618 // TestContext is obsolete but works

namespace Tests.Blazor;

public class ContadorCarritoTests : Bunit.TestContext
{
    private readonly NotificacionService _notificacionService;

    public ContadorCarritoTests()
    {
        _notificacionService = new NotificacionService();
        Services.AddSingleton(_notificacionService);
    }

    [Fact]
    public void RendersWithInitialCount()
    {
        var cut = Render<ContadorCarrito>(parameters => parameters
            .Add(p => p.InitialCount, 5));

        var html = cut.Markup;
        Assert.Contains("5", html);
    }

    [Fact]
    public void ShowsZeroWhenEmpty()
    {
        var cut = Render<ContadorCarrito>(parameters => parameters
            .Add(p => p.InitialCount, 0));

        var html = cut.Markup;
        Assert.Contains("0", html);
    }

    [Fact]
    public void Shows99PlusWhenOver99()
    {
        var cut = Render<ContadorCarrito>(parameters => parameters
            .Add(p => p.InitialCount, 150));

        var html = cut.Markup;
        Assert.Contains("99+", html);
    }
}

public class NotificacionesToastTests : Bunit.TestContext
{
    private readonly NotificacionService _notificacionService;

    public NotificacionesToastTests()
    {
        _notificacionService = new NotificacionService();
        Services.AddSingleton(_notificacionService);
    }

    [Fact]
    public void StartsEmpty()
    {
        var cut = Render<NotificacionesToast>(parameters => parameters
            .Add(p => p.UserId, "user1"));

        Assert.DoesNotContain("notification", cut.Markup);
    }

    [Fact]
    public void DisplaysNotification()
    {
        var cut = Render<NotificacionesToast>(parameters => parameters
            .Add(p => p.UserId, "user1"));

        _notificacionService.Enviar("user1", new Notificacion
        {
            Tipo = "success",
            Titulo = "Test Title",
            Mensaje = "Test Message",
            Icono = "fa-solid fa-check"
        });

        cut.Render();

        var html = cut.Markup;
        Assert.Contains("Test Title", html);
    }

    [Fact]
    public void FiltersByUserId()
    {
        var cut = Render<NotificacionesToast>(parameters => parameters
            .Add(p => p.UserId, "user1"));

        _notificacionService.Enviar("user2", new Notificacion
        {
            Titulo = "Should Not Appear"
        });

        cut.Render();

        Assert.DoesNotContain("Should Not Appear", cut.Markup);
    }

    [Fact]
    public void ShowsToAllWhenEmptyUserId()
    {
        var cut = Render<NotificacionesToast>(parameters => parameters
            .Add(p => p.UserId, "user1"));

        _notificacionService.EnviarATodos(new Notificacion
        {
            Titulo = "Broadcast Message"
        });

        cut.Render();

        var html = cut.Markup;
        Assert.Contains("Broadcast Message", html);
    }

    [Fact]
    public void MultipleNotifications()
    {
        var cut = Render<NotificacionesToast>(parameters => parameters
            .Add(p => p.UserId, "user1"));

        _notificacionService.Enviar("user1", new Notificacion { Titulo = "Notif 1" });
        _notificacionService.Enviar("user1", new Notificacion { Titulo = "Notif 2" });
        _notificacionService.Enviar("user1", new Notificacion { Titulo = "Notif 3" });

        cut.Render();

        var html = cut.Markup;
        Assert.Contains("Notif 1", html);
        Assert.Contains("Notif 2", html);
        Assert.Contains("Notif 3", html);
    }

    [Fact]
    public void DifferentTypesHaveCorrectStyles()
    {
        var cut = Render<NotificacionesToast>(parameters => parameters
            .Add(p => p.UserId, "user1"));

        _notificacionService.Enviar("user1", new Notificacion { Tipo = "success", Titulo = "Success" });
        _notificacionService.Enviar("user1", new Notificacion { Tipo = "error", Titulo = "Error" });
        _notificacionService.Enviar("user1", new Notificacion { Tipo = "warning", Titulo = "Warning" });
        _notificacionService.Enviar("user1", new Notificacion { Tipo = "info", Titulo = "Info" });

        cut.Render();

        var html = cut.Markup;
        Assert.Contains("border-success", html);
        Assert.Contains("border-error", html);
        Assert.Contains("border-warning", html);
        Assert.Contains("border-info", html);
    }
}

#pragma warning restore CS0618
