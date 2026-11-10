using Inventario.Api.Data;
using Inventario.Api.Models;
using Inventario.Api.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Inventario.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MovimientosInventarioController : ControllerBase
{
    private readonly InventarioDbContext _context;
    private readonly CatalogoClient _catalogoClient;

    public MovimientosInventarioController(InventarioDbContext context, CatalogoClient catalogoClient)
    {
        _context = context;
        _catalogoClient = catalogoClient;
    }

    // GET: api/MovimientosInventario
    [HttpGet]
    public async Task<ActionResult<IEnumerable<MovimientoInventario>>> GetMovimientos()
    {
        return Ok(await _context.MovimientosInventario
            .Include(m => m.Bodega)
            .OrderByDescending(m => m.Fecha)
            .ToListAsync());
    }

    // GET: api/MovimientosInventario/5
    [HttpGet("{id:int}")]
    public async Task<ActionResult<MovimientoInventario>> GetMovimiento(int id)
    {
        var movimiento = await _context.MovimientosInventario
            .Include(m => m.Bodega)
            .FirstOrDefaultAsync(m => m.IdMovimiento == id);

        if (movimiento == null)
        {
            return NotFound(new { mensaje = $"El movimiento con ID {id} no existe." });
        }

        return Ok(movimiento);
    }

    // POST: api/MovimientosInventario
    [HttpPost]
    public async Task<ActionResult<MovimientoInventario>> PostMovimiento([FromBody] MovimientoInventario movimiento)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        // Validación FK Física: Bodega
        if (!await _context.Bodegas.AnyAsync(b => b.IdBodega == movimiento.IdBodega))
        {
            return BadRequest(new { mensaje = $"La bodega con ID {movimiento.IdBodega} no existe (FK Física)." });
        }

        // Validación FK Lógica: Producto en Catalogo.Api
        if (!await _catalogoClient.ProductoExisteYActivoAsync(movimiento.IdProducto))
        {
            return BadRequest(new { mensaje = $"El producto con ID {movimiento.IdProducto} no existe o está inactivo en Catalogo.Api (FK Lógica)." });
        }

        var existencia = await _context.Existencias
            .FirstOrDefaultAsync(e => e.IdProducto == movimiento.IdProducto && e.IdBodega == movimiento.IdBodega);

        if (movimiento.Tipo == "ENTRADA")
        {
            if (existencia == null)
            {
                existencia = new Existencia
                {
                    IdProducto = movimiento.IdProducto,
                    IdBodega = movimiento.IdBodega,
                    Stock = movimiento.Cantidad
                };
                _context.Existencias.Add(existencia);
            }
            else
            {
                existencia.Stock += movimiento.Cantidad;
            }
        }
        else if (movimiento.Tipo == "SALIDA")
        {
            if (existencia == null || existencia.Stock < movimiento.Cantidad)
            {
                return BadRequest(new
                {
                    mensaje = $"Stock insuficiente en la bodega {movimiento.IdBodega} para el producto {movimiento.IdProducto}. Stock actual: {existencia?.Stock ?? 0}."
                });
            }
            existencia.Stock -= movimiento.Cantidad;
        }
        else if (movimiento.Tipo == "AJUSTE")
        {
            if (existencia == null)
            {
                existencia = new Existencia
                {
                    IdProducto = movimiento.IdProducto,
                    IdBodega = movimiento.IdBodega,
                    Stock = movimiento.Cantidad
                };
                _context.Existencias.Add(existencia);
            }
            else
            {
                existencia.Stock = movimiento.Cantidad;
            }
        }

        movimiento.Fecha = DateTime.Now;
        _context.MovimientosInventario.Add(movimiento);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetMovimiento), new { id = movimiento.IdMovimiento }, movimiento);
    }

    // PUT: api/MovimientosInventario/5
    [HttpPut("{id:int}")]
    public async Task<IActionResult> PutMovimiento(int id, [FromBody] MovimientoInventario movimiento)
    {
        if (id != movimiento.IdMovimiento)
        {
            return BadRequest(new { mensaje = "El ID de la URL no coincide con el ID del movimiento." });
        }

        var movExistente = await _context.MovimientosInventario.AsNoTracking().FirstOrDefaultAsync(m => m.IdMovimiento == id);
        if (movExistente == null)
        {
            return NotFound(new { mensaje = $"El movimiento con ID {id} no existe." });
        }

        if (!await _context.Bodegas.AnyAsync(b => b.IdBodega == movimiento.IdBodega))
        {
            return BadRequest(new { mensaje = $"La bodega con ID {movimiento.IdBodega} no existe (FK Física)." });
        }

        if (!await _catalogoClient.ProductoExisteYActivoAsync(movimiento.IdProducto))
        {
            return BadRequest(new { mensaje = $"El producto con ID {movimiento.IdProducto} no existe en Catalogo.Api (FK Lógica)." });
        }

        _context.Entry(movimiento).State = EntityState.Modified;
        await _context.SaveChangesAsync();

        return Ok(movimiento);
    }

    // DELETE: api/MovimientosInventario/5
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteMovimiento(int id)
    {
        var movimiento = await _context.MovimientosInventario.FindAsync(id);
        if (movimiento == null)
        {
            return NotFound(new { mensaje = $"El movimiento con ID {id} no existe." });
        }

        _context.MovimientosInventario.Remove(movimiento);
        await _context.SaveChangesAsync();

        return Ok(new { mensaje = "Movimiento de inventario eliminado correctamente." });
    }
}
