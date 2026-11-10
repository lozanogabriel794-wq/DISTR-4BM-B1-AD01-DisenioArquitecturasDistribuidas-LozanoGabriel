using System.Net.Http.Json;

namespace Pedidos.Api.Services;

public class MicroserviciosClient
{
    private readonly HttpClient _catalogoHttp;
    private readonly HttpClient _inventarioHttp;

    public MicroserviciosClient(IHttpClientFactory httpClientFactory)
    {
        _catalogoHttp = httpClientFactory.CreateClient("CatalogoApi");
        _inventarioHttp = httpClientFactory.CreateClient("InventarioApi");
    }

    public async Task<ProductoInfoDto?> ObtenerProductoAsync(int idProducto)
    {
        try
        {
            var response = await _catalogoHttp.GetAsync($"api/Productos/{idProducto}");
            if (!response.IsSuccessStatusCode)
            {
                return null;
            }
            return await response.Content.ReadFromJsonAsync<ProductoInfoDto>();
        }
        catch
        {
            return null;
        }
    }

    public async Task<List<ExistenciaInfoDto>> ObtenerExistenciasPorProductoAsync(int idProducto)
    {
        try
        {
            var response = await _inventarioHttp.GetAsync($"api/Existencias/producto/{idProducto}");
            if (!response.IsSuccessStatusCode)
            {
                return new List<ExistenciaInfoDto>();
            }
            return await response.Content.ReadFromJsonAsync<List<ExistenciaInfoDto>>() ?? new List<ExistenciaInfoDto>();
        }
        catch
        {
            return new List<ExistenciaInfoDto>();
        }
    }

    public async Task<bool> ReservarStockAsync(int idProducto, int idBodega, int cantidad)
    {
        try
        {
            var payload = new
            {
                IdProducto = idProducto,
                IdBodega = idBodega,
                Cantidad = cantidad,
                Estado = "CONFIRMADA"
            };
            var response = await _inventarioHttp.PostAsJsonAsync("api/ReservasStock", payload);
            return response.IsSuccessStatusCode;
        }
        catch
        {
            return false;
        }
    }
}

public class ProductoInfoDto
{
    public int IdProducto { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string SKU { get; set; } = string.Empty;
    public decimal Precio { get; set; }
    public bool Estado { get; set; }
}

public class ExistenciaInfoDto
{
    public int IdExistencia { get; set; }
    public int IdProducto { get; set; }
    public int IdBodega { get; set; }
    public int Stock { get; set; }
}

