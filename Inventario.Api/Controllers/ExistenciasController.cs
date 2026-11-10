using Inventario.Api.Data;
using Inventario.Api.Models;
using Inventario.Api.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Inventario.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ExistenciasController : ControllerBase
{
    private readonly InventarioDbContext _context;
    private readonly CatalogoClient _catalogoClient;

    public ExistenciasController(InventarioDbContext context, CatalogoClient catalogoClient)
    {
        _context = context;
        _catalogoClient = catalogoClient;
    }

    // GET: api/Existencias
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Existencia>>> GetExistencias()
    {
        return Ok(await _context.Existencias.Include(e => e.Bodega).ToListAsync());
    }

    // GET: api/Existencias/5
    [HttpGet("{id:int}")]
    public async Task<ActionResult<Existencia>> GetExistencia(int id)
    {
        var existencia = await _context.Existencias
            .Include(e => e.Bodega)
            .FirstOrDefaultAsync(e => e.IdExistencia == id);

        if (existencia == null)
        {
            return NotFound(new { mensaje = $"El registro de existencia con ID {id} no existe." });
        }

        return Ok(existencia);
    }

    // GET: api/Existencias/producto/1
    [HttpGet("producto/{idProducto:int}")]
    public async Task<ActionResult<IEnumerable<Existencia>>> GetExistenciasPorProducto(int idProducto)
    {
        var existencias = await _context.Existencias
            .Include(e => e.Bodega)
            .Where(e => e.IdProducto == idProducto)
            .ToListAsync();

        return Ok(existencias);
    }

    // POST: api/Existencias
    [HttpPost]
    public async Task<ActionResult<Existencia>> PostExistencia([FromBody] Existencia existencia)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        // Validación FK Física: Bodega
        if (!await _context.Bodegas.AnyAsync(b => b.IdBodega == existencia.IdBodega))
        {
            return BadRequest(new { mensaje = $"La bodega con ID {existencia.IdBodega} no existe (FK Física)." });
        }

        // Validación FK Lógica: Producto en Catalogo.Api
        if (!await _catalogoClient.ProductoExisteYActivoAsync(existencia.IdProducto))
        {
            return BadRequest(new { mensaje = $"El producto con ID {existencia.IdProducto} no existe o está inactivo en Catalogo.Api (FK Lógica)." });
        }

        // Validar que no esté duplicada la relación Producto-Bodega
        if (await _context.Existencias.AnyAsync(e => e.IdProducto == existencia.IdProducto && e.IdBodega == existencia.IdBodega))
        {
            return BadRequest(new { mensaje = "Ya existe un registro de existencia para este producto en la bodega seleccionada." });
        }

        _context.Existencias.Add(existencia);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetExistencia), new { id = existencia.IdExistencia }, existencia);
    }

    // PUT: api/Existencias/5
    [HttpPut("{id:int}")]
    public async Task<IActionResult> PutExistencia(int id, [FromBody] Existencia existencia)
    {
        if (id != existencia.IdExistencia)
        {
            return BadRequest(new { mensaje = "El ID de la URL no coincide con el ID de la existencia." });
        }

        if (!await _context.Existencias.AnyAsync(e => e.IdExistencia == id))
        {
            return NotFound(new { mensaje = $"El registro de existencia con ID {id} no existe." });
        }

        if (!await _context.Bodegas.AnyAsync(b => b.IdBodega == existencia.IdBodega))
        {
            return BadRequest(new { mensaje = $"La bodega con ID {existencia.IdBodega} no existe (FK Física)." });
        }

        if (!await _catalogoClient.ProductoExisteYActivoAsync(existencia.IdProducto))
        {
            return BadRequest(new { mensaje = $"El producto con ID {existencia.IdProducto} no existe en Catalogo.Api (FK Lógica)." });
        }

        // Validar que no duplique otra combinación IdProducto + IdBodega
        if (await _context.Existencias.AnyAsync(e => e.IdProducto == existencia.IdProducto && e.IdBodega == existencia.IdBodega && e.IdExistencia != id))
        {
            return BadRequest(new { mensaje = "Ya existe otro registro de existencia para este producto en esa bodega." });
        }

        _context.Entry(existencia).State = EntityState.Modified;
        await _context.SaveChangesAsync();

        return Ok(existencia);
    }

    // DELETE: api/Existencias/5
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteExistencia(int id)
    {
        var existencia = await _context.Existencias.FindAsync(id);
        if (existencia == null)
        {
            return NotFound(new { mensaje = $"El registro de existencia con ID {id} no existe." });
        }

        _context.Existencias.Remove(existencia);
        await _context.SaveChangesAsync();

        return Ok(new { mensaje = "Registro de existencia eliminado correctamente." });
    }
}
