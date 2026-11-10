using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Pedidos.Api.Data;
using Pedidos.Api.Models;
using Pedidos.Api.Services;

namespace Pedidos.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PedidosController : ControllerBase
{
    private readonly PedidosDbContext _context;
    private readonly MicroserviciosClient _microservicios;

    public PedidosController(PedidosDbContext context, MicroserviciosClient microservicios)
    {
        _context = context;
        _microservicios = microservicios;
    }

    // GET: api/Pedidos
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Pedido>>> GetPedidos()
    {
        var pedidos = await _context.Pedidos
            .Include(p => p.Cliente)
            .Include(p => p.Detalles)
            .OrderByDescending(p => p.Fecha)
            .ToListAsync();

        return Ok(pedidos);
    }

    // GET: api/Pedidos/5
    [HttpGet("{id:int}")]
    public async Task<ActionResult<Pedido>> GetPedido(int id)
    {
        var pedido = await _context.Pedidos
            .Include(p => p.Cliente)
            .Include(p => p.Detalles)
            .FirstOrDefaultAsync(p => p.IdPedido == id);

        if (pedido == null)
        {
            return NotFound(new { mensaje = $"El pedido con ID {id} no existe." });
        }

        return Ok(pedido);
    }

    // POST: api/Pedidos
    // Orquesta la validación con Catalogo.Api y reserva de stock en Inventario.Api
    [HttpPost]
    public async Task<ActionResult<Pedido>> PostPedido([FromBody] Pedido pedido)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        // 1. Validar FK Física: Que el Cliente exista en PedidosDB
        var clienteExiste = await _context.Clientes.AnyAsync(c => c.IdCliente == pedido.IdCliente);
        if (!clienteExiste)
        {
            return BadRequest(new { mensaje = $"El cliente con ID {pedido.IdCliente} no existe (FK Física)." });
        }

        if (pedido.Detalles == null || pedido.Detalles.Count == 0)
        {
            return BadRequest(new { mensaje = "El pedido debe incluir al menos un detalle de producto." });
        }

        decimal totalCalculado = 0;
        var reservasARealizar = new List<(int IdProducto, int IdBodega, int Cantidad)>();

        // 2. Validar cada detalle contra Catalogo.Api e Inventario.Api (FK Lógica)
        foreach (var detalle in pedido.Detalles)
        {
            // Consultar Producto en Catalogo.Api
            var producto = await _microservicios.ObtenerProductoAsync(detalle.IdProducto);
            if (producto == null || !producto.Estado)
            {
                return BadRequest(new
                {
                    mensaje = $"El producto con ID {detalle.IdProducto} no existe o está inactivo en Catalogo.Api (FK Lógica)."
                });
            }

            // Consultar Stock disponible en Inventario.Api
            var existencias = await _microservicios.ObtenerExistenciasPorProductoAsync(detalle.IdProducto);
            var bodegaConStock = existencias.FirstOrDefault(e => e.Stock >= detalle.Cantidad);

            if (bodegaConStock == null)
            {
                int stockTotal = existencias.Sum(e => e.Stock);
                return BadRequest(new
                {
                    mensaje = $"Stock insuficiente en las bodegas para el producto '{producto.Nombre}' (ID: {detalle.IdProducto}). Solicitado: {detalle.Cantidad}, Stock total disponible: {stockTotal}."
                });
            }

            // Tomar el precio oficial del catálogo si no se envió o actualizarlo
            detalle.PrecioUnitario = producto.Precio;
            totalCalculado += detalle.Cantidad * detalle.PrecioUnitario;

            reservasARealizar.Add((detalle.IdProducto, bodegaConStock.IdBodega, detalle.Cantidad));
        }

        // 3. Descontar / Reservar Stock en Inventario.Api
        foreach (var item in reservasARealizar)
        {
            var exitoReserva = await _microservicios.ReservarStockAsync(item.IdProducto, item.IdBodega, item.Cantidad);
            if (!exitoReserva)
            {
                return BadRequest(new
                {
                    mensaje = $"No se pudo procesar la reserva de inventario para el producto ID {item.IdProducto} en la bodega ID {item.IdBodega}."
                });
            }
        }

        // 4. Guardar el pedido y sus detalles en PedidosDB
        pedido.Fecha = DateTime.Now;
        pedido.Total = totalCalculado;

        _context.Pedidos.Add(pedido);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetPedido), new { id = pedido.IdPedido }, pedido);
    }

    // PUT: api/Pedidos/5
    [HttpPut("{id:int}")]
    public async Task<IActionResult> PutPedido(int id, [FromBody] Pedido pedidoActualizado)
    {
        if (id != pedidoActualizado.IdPedido)
        {
            return BadRequest(new { mensaje = "El ID de la URL no coincide con el ID del pedido." });
        }

        var pedido = await _context.Pedidos.FindAsync(id);
        if (pedido == null)
        {
            return NotFound(new { mensaje = $"El pedido con ID {id} no existe." });
        }

        pedido.Estado = pedidoActualizado.Estado;
        await _context.SaveChangesAsync();

        return Ok(pedido);
    }

    // DELETE: api/Pedidos/5
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeletePedido(int id)
    {
        var pedido = await _context.Pedidos
            .Include(p => p.Detalles)
            .FirstOrDefaultAsync(p => p.IdPedido == id);

        if (pedido == null)
        {
            return NotFound(new { mensaje = $"El pedido con ID {id} no existe." });
        }

        _context.Pedidos.Remove(pedido);
        await _context.SaveChangesAsync();

        return Ok(new { mensaje = "Pedido eliminado correctamente." });
    }
}

