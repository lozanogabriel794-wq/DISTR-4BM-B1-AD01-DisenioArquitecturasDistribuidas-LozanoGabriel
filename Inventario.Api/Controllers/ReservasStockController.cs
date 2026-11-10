using Inventario.Api.Data;
using Inventario.Api.Models;
using Inventario.Api.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Inventario.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ReservasStockController : ControllerBase
{
    private readonly InventarioDbContext _context;
    private readonly CatalogoClient _catalogoClient;

    public ReservasStockController(InventarioDbContext context, CatalogoClient catalogoClient)
    {
        _context = context;
        _catalogoClient = catalogoClient;
    }

    // GET: api/ReservasStock
    [HttpGet]
    public async Task<ActionResult<IEnumerable<ReservaStock>>> GetReservas()
    {
        return Ok(await _context.ReservasStock.Include(r => r.Bodega).ToListAsync());
    }

    // GET: api/ReservasStock/5
    [HttpGet("{id:int}")]
    public async Task<ActionResult<ReservaStock>> GetReserva(int id)
    {
        var reserva = await _context.ReservasStock
            .Include(r => r.Bodega)
            .FirstOrDefaultAsync(r => r.IdReserva == id);

        if (reserva == null)
        {
            return NotFound(new { mensaje = $"La reserva con ID {id} no existe." });
        }

        return Ok(reserva);
    }

    // POST: api/ReservasStock
    [HttpPost]
    public async Task<ActionResult<ReservaStock>> PostReserva([FromBody] ReservaStock reserva)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        // Validación FK Física: Bodega
        if (!await _context.Bodegas.AnyAsync(b => b.IdBodega == reserva.IdBodega))
        {
            return BadRequest(new { mensaje = $"La bodega con ID {reserva.IdBodega} no existe (FK Física)." });
        }

        // Validación FK Lógica: Producto en Catalogo.Api
        if (!await _catalogoClient.ProductoExisteYActivoAsync(reserva.IdProducto))
        {
            return BadRequest(new { mensaje = $"El producto con ID {reserva.IdProducto} no existe en Catalogo.Api (FK Lógica)." });
        }

        var existencia = await _context.Existencias
            .FirstOrDefaultAsync(e => e.IdProducto == reserva.IdProducto && e.IdBodega == reserva.IdBodega);

        if (existencia == null || existencia.Stock < reserva.Cantidad)
        {
            return BadRequest(new
            {
                mensaje = $"Stock insuficiente para reservar. Disponible en bodega {reserva.IdBodega}: {existencia?.Stock ?? 0}, Solicitado: {reserva.Cantidad}."
            });
        }

        // Descontar del stock disponible y registrar movimiento de salida
        existencia.Stock -= reserva.Cantidad;

        var movimiento = new MovimientoInventario
        {
            IdProducto = reserva.IdProducto,
            IdBodega = reserva.IdBodega,
            Tipo = "SALIDA",
            Cantidad = reserva.Cantidad,
            Fecha = DateTime.Now
        };

        _context.MovimientosInventario.Add(movimiento);
        _context.ReservasStock.Add(reserva);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetReserva), new { id = reserva.IdReserva }, reserva);
    }

    // PUT: api/ReservasStock/5
    [HttpPut("{id:int}")]
    public async Task<IActionResult> PutReserva(int id, [FromBody] ReservaStock reservaActualizada)
    {
        if (id != reservaActualizada.IdReserva)
        {
            return BadRequest(new { mensaje = "El ID de la URL no coincide con el ID de la reserva." });
        }

        var reserva = await _context.ReservasStock.FindAsync(id);
        if (reserva == null)
        {
            return NotFound(new { mensaje = $"La reserva con ID {id} no existe." });
        }

        if (!await _context.Bodegas.AnyAsync(b => b.IdBodega == reservaActualizada.IdBodega))
        {
            return BadRequest(new { mensaje = $"La bodega con ID {reservaActualizada.IdBodega} no existe (FK Física)." });
        }

        // Si pasa a CANCELADA y antes no lo estaba, devolver el stock
        if (reserva.Estado != "CANCELADA" && reservaActualizada.Estado == "CANCELADA")
        {
            var existencia = await _context.Existencias
                .FirstOrDefaultAsync(e => e.IdProducto == reserva.IdProducto && e.IdBodega == reserva.IdBodega);

            if (existencia != null)
            {
                existencia.Stock += reserva.Cantidad;
            }
        }

        reserva.IdProducto = reservaActualizada.IdProducto;
        reserva.IdBodega = reservaActualizada.IdBodega;
        reserva.Cantidad = reservaActualizada.Cantidad;
        reserva.Estado = reservaActualizada.Estado;
        await _context.SaveChangesAsync();

        return Ok(reserva);
    }

    // DELETE: api/ReservasStock/5
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteReserva(int id)
    {
        var reserva = await _context.ReservasStock.FindAsync(id);
        if (reserva == null)
        {
            return NotFound(new { mensaje = $"La reserva con ID {id} no existe." });
        }

        _context.ReservasStock.Remove(reserva);
        await _context.SaveChangesAsync();

        return Ok(new { mensaje = "Reserva de stock eliminada correctamente." });
    }
}
