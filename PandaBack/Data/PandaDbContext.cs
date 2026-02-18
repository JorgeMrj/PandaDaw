using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using PandaBack.Models;

namespace PandaBack.Data;

public class PandaDbContext : IdentityDbContext<User>
{
    public DbSet<Venta> Ventas => Set<Venta>();
    public DbSet<Carrito> Carritos => Set<Carrito>();
    // No hace falta poner DbSet<User>, Identity lo trae por defecto como 'Users'

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // Configuración de tu Enum Role para que se guarde como String
        builder.Entity<User>()
            .Property(u => u.Role)
            .HasConversion<string>();

        // Tus otras configuraciones (Relaciones Carrito, Venta, etc.)
    }
}