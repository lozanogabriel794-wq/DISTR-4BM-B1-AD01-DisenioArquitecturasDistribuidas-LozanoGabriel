using Inventario.Api.Data;
using Inventario.Api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Inventario.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class BodegasController : ControllerBase
{
    private readonly InventarioDbContext _context;

    public BodegasController(InventarioDbContext context)
    {
        _context = context;
    }

    // GET: api/Bodegas
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Bodega>>> GetBodegas()
    {
        return Ok(await _context.Bodegas.ToListAsync());
    }

    // GET: api/Bodegas/5
    [HttpGet("{id:int}")]
    public async Task<ActionResult<Bodega>> GetBodega(int id)
    {
        var bodega = await _context.Bodegas.FindAsync(id);
        if (bodega == null)
        {
            return NotFound(new { mensaje = $"La bodega con ID {id} no existe." });
        }
        return Ok(bodega);
    }

    // POST: api/Bodegas
    [HttpPost]
    public async Task<ActionResult<Bodega>> PostBodega([FromBody] Bodega bodega)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        _context.Bodegas.Add(bodega);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetBodega), new { id = bodega.IdBodega }, bodega);
    }

    // PUT: api/Bodegas/5
    [HttpPut("{id:int}")]
    public async Task<IActionResult> PutBodega(int id, [FromBody] Bodega bodega)
    {
        if (id != bodega.IdBodega)
        {
            return BadRequest(new { mensaje = "El ID de la URL no coincide con el ID de la bodega." });
        }

        if (!await _context.Bodegas.AnyAsync(b => b.IdBodega == id))
        {
            return NotFound(new { mensaje = $"La bodega con ID {id} no existe." });
        }

        _context.Entry(bodega).State = EntityState.Modified;
        await _context.SaveChangesAsync();

        return Ok(bodega);
    }

    // DELETE: api/Bodegas/5
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteBodega(int id)
    {
        var bodega = await _context.Bodegas.FindAsync(id);
        if (bodega == null)
        {
            return NotFound(new { mensaje = $"La bodega con ID {id} no existe." });
        }

        var tieneRegistros = await _context.Existencias.AnyAsync(e => e.IdBodega == id)
            || await _context.MovimientosInventario.AnyAsync(m => m.IdBodega == id)
            || await _context.ReservasStock.AnyAsync(r => r.IdBodega == id);

        if (tieneRegistros)
        {
            return BadRequest(new { mensaje = "No se puede eliminar la bodega porque tiene existencias, movimientos o reservas asociadas (FK Física)." });
        }

        _context.Bodegas.Remove(bodega);
        await _context.SaveChangesAsync();

        return Ok(new { mensaje = "Bodega eliminada correctamente." });
    }
}

