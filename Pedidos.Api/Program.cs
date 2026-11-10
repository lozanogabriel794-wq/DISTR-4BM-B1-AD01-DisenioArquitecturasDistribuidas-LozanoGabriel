using Microsoft.EntityFrameworkCore;
using Pedidos.Api.Data;
using Pedidos.Api.Services;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// Configuración de DbContext con SQL Server (PedidosDB)
builder.Services.AddDbContext<PedidosDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Clientes HTTP para comunicación con Catalogo.Api e Inventario.Api
builder.Services.AddHttpClient("CatalogoApi", client =>
{
    var url = builder.Configuration["Microservicios:CatalogoUrl"] ?? "http://localhost:5001/";
    client.BaseAddress = new Uri(url);
});

builder.Services.AddHttpClient("InventarioApi", client =>
{
    var url = builder.Configuration["Microservicios:InventarioUrl"] ?? "http://localhost:5002/";
    client.BaseAddress = new Uri(url);
});

builder.Services.AddScoped<MicroserviciosClient>();

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
