using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using PandaBack.Data;
using PandaBack.Models;
using PandaBack.Services.Auth;

var builder = WebApplication.CreateBuilder(args);

// Configuracion de identity
builder.Services.AddIdentity<User, IdentityRole>(options => 
    {
        options.Password.RequireDigit = true;
        options.Password.RequireLowercase = true;
        options.Password.RequiredLength = 6;
        options.User.RequireUniqueEmail = true;
    })
    .AddEntityFrameworkStores<PandaDbContext>()
    .AddDefaultTokenProviders();

// Autenticación JWT
var jwtKey = builder.Configuration["Jwt:Key"] ?? throw new InvalidOperationException("Jwt:Key no configurada");

builder.Services.AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
        };
    });

// Registro de servicios
builder.Services.AddScoped<TokenService>();


var app = builder.Build();


app.UseHttpsRedirection();

// Middlewares de autenticación y autorización
app.UseAuthentication(); 
app.UseAuthorization();  

app.MapControllers();

app.Run();