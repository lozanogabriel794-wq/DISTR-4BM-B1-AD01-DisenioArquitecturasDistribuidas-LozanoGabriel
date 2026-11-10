using System.Net.Http.Json;

namespace Inventario.Api.Services;

public class CatalogoClient
{
    private readonly HttpClient _httpClient;

    public CatalogoClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<bool> ProductoExisteYActivoAsync(int idProducto)
    {
        try
        {
            var response = await _httpClient.GetAsync($"api/Productos/{idProducto}");
            if (!response.IsSuccessStatusCode)
            {
                return false;
            }

            var producto = await response.Content.ReadFromJsonAsync<ProductoDto>();
            return producto != null && producto.Estado;
        }
        catch
        {
            return false;
        }
    }

    private class ProductoDto
    {
        public int IdProducto { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public decimal Precio { get; set; }
        public bool Estado { get; set; }
    }
}

