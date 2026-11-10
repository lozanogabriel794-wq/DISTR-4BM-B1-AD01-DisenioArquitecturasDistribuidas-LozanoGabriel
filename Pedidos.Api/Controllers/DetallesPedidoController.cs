using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Pedidos.Api.Data;
using Pedidos.Api.Models;
using Pedidos.Api.Services;

namespace Pedidos.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DetallesPedidoController : ControllerBase
{
    private readonly PedidosDbContext _context;
    private readonly MicroserviciosClient _microservicios;

    public DetallesPedidoController(PedidosDbContext context, MicroserviciosClient microservicios)
    {
        _context = context;
        _microservicios = microservicios;
    }

    // GET: api/DetallesPedido
    [HttpGet]
    public async Task<ActionResult<IEnumerable<DetallePedido>>> GetDetalles()
    {
        return Ok(await _context.DetallesPedido.ToListAsync());
    }

    // GET: api/DetallesPedido/5
    [HttpGet("{id:int}")]
    public async Task<ActionResult<DetallePedido>> GetDetalle(int id)
    {
        var detalle = await _context.DetallesPedido.FindAsync(id);
        if (detalle == null)
        {
            return NotFound(new { mensaje = $"El detalle de pedido con ID {id} no existe." });
        }
        return Ok(detalle);
    }

    // GET: api/DetallesPedido/pedido/1
    [HttpGet("pedido/{idPedido:int}")]
    public async Task<ActionResult<IEnumerable<DetallePedido>>> GetDetallesPorPedido(int idPedido)
    {
        if (!await _context.Pedidos.AnyAsync(p => p.IdPedido == idPedido))
        {
            return NotFound(new { mensaje = $"El pedido con ID {idPedido} no existe." });
        }

        var detalles = await _context.DetallesPedido
            .Where(d => d.IdPedido == idPedido)
            .ToListAsync();

        return Ok(detalles);
    }

    // POST: api/DetallesPedido
    [HttpPost]
    public async Task<ActionResult<DetallePedido>> PostDetalle([FromBody] DetallePedido detalle)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        // Validar FK Física: Pedido
        var pedido = await _context.Pedidos.Include(p => p.Detalles).FirstOrDefaultAsync(p => p.IdPedido == detalle.IdPedido);
        if (pedido == null)
        {
            return BadRequest(new { mensaje = $"El pedido con ID {detalle.IdPedido} no existe (FK Física)." });
        }

        // Validar FK Lógica: Producto en Catalogo.Api
        var producto = await _microservicios.ObtenerProductoAsync(detalle.IdProducto);
        if (producto == null || !producto.Estado)
        {
            return BadRequest(new { mensaje = $"El producto con ID {detalle.IdProducto} no existe o está inactivo en Catalogo.Api (FK Lógica)." });
        }

        detalle.PrecioUnitario = producto.Precio;
        _context.DetallesPedido.Add(detalle);

        // Actualizar el total del pedido
        pedido.Total += detalle.Cantidad * detalle.PrecioUnitario;
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetDetalle), new { id = detalle.IdDetalle }, detalle);
    }

    // PUT: api/DetallesPedido/5
    [HttpPut("{id:int}")]
    public async Task<IActionResult> PutDetalle(int id, [FromBody] DetallePedido detalleActualizado)
    {
        if (id != detalleActualizado.IdDetalle)
        {
            return BadRequest(new { mensaje = "El ID de la URL no coincide con el ID del detalle." });
        }

        var detalle = await _context.DetallesPedido.FindAsync(id);
        if (detalle == null)
        {
            return NotFound(new { mensaje = $"El detalle de pedido con ID {id} no existe." });
        }

        // Validar FK Física: Pedido
        var pedido = await _context.Pedidos.FindAsync(detalleActualizado.IdPedido);
        if (pedido == null)
        {
            return BadRequest(new { mensaje = $"El pedido con ID {detalleActualizado.IdPedido} no existe (FK Física)." });
        }

        // Validar FK Lógica: Producto en Catalogo.Api
        var producto = await _microservicios.ObtenerProductoAsync(detalleActualizado.IdProducto);
        if (producto == null || !producto.Estado)
        {
            return BadRequest(new { mensaje = $"El producto con ID {detalleActualizado.IdProducto} no existe en Catalogo.Api (FK Lógica)." });
        }

        detalle.IdPedido = detalleActualizado.IdPedido;
        detalle.IdProducto = detalleActualizado.IdProducto;
        detalle.Cantidad = detalleActualizado.Cantidad;
        detalle.PrecioUnitario = detalleActualizado.PrecioUnitario;

        await _context.SaveChangesAsync();

        // Recalcular el total del pedido
        pedido.Total = await _context.DetallesPedido
            .Where(d => d.IdPedido == pedido.IdPedido)
            .SumAsync(d => d.Cantidad * d.PrecioUnitario);
        await _context.SaveChangesAsync();

        return Ok(detalle);
    }

    // DELETE: api/DetallesPedido/5
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteDetalle(int id)
    {
        var detalle = await _context.DetallesPedido.FindAsync(id);
        if (detalle == null)
        {
            return NotFound(new { mensaje = $"El detalle de pedido con ID {id} no existe." });
        }

        int idPedido = detalle.IdPedido;
        _context.DetallesPedido.Remove(detalle);
        await _context.SaveChangesAsync();

        // Actualizar el total del pedido tras eliminar el detalle
        var pedido = await _context.Pedidos.FindAsync(idPedido);
        if (pedido != null)
        {
            pedido.Total = await _context.DetallesPedido
                .Where(d => d.IdPedido == idPedido)
                .SumAsync(d => d.Cantidad * d.PrecioUnitario);
            await _context.SaveChangesAsync();
        }

        return Ok(new { mensaje = "Detalle de pedido eliminado correctamente." });
    }
}
