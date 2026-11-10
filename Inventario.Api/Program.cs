using Inventario.Api.Data;
using Inventario.Api.Services;
using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// Configuración de DbContext con SQL Server (InventarioDB)
builder.Services.AddDbContext<InventarioDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Cliente HTTP para validar FK lógica contra Catalogo.Api
builder.Services.AddHttpClient<CatalogoClient>(client =>
{
    var url = builder.Configuration["Microservicios:CatalogoUrl"] ?? "http://localhost:5001/";
    client.BaseAddress = new Uri(url);
});

builder.Services.AddControllers();
builder.Services.AddOpenApi();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference(); // Interfaz visual interactiva
}

app.UseAuthorization();
app.MapControllers();

app.Run();
