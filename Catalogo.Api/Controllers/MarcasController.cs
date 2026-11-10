using Catalogo.Api.Data;
using Catalogo.Api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Catalogo.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MarcasController : ControllerBase
{
    private readonly CatalogoDbContext _context;

    public MarcasController(CatalogoDbContext context)
    {
        _context = context;
    }

    // GET: api/Marcas
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Marca>>> GetMarcas()
    {
        return Ok(await _context.Marcas.ToListAsync());
    }

    // GET: api/Marcas/5
    [HttpGet("{id:int}")]
    public async Task<ActionResult<Marca>> GetMarca(int id)
    {
        var marca = await _context.Marcas.FindAsync(id);
        if (marca == null)
        {
            return NotFound(new { mensaje = $"La marca con ID {id} no existe." });
        }
        return Ok(marca);
    }

    // POST: api/Marcas
    [HttpPost]
    public async Task<ActionResult<Marca>> PostMarca([FromBody] Marca marca)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        _context.Marcas.Add(marca);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetMarca), new { id = marca.IdMarca }, marca);
    }

    // PUT: api/Marcas/5
    [HttpPut("{id:int}")]
    public async Task<IActionResult> PutMarca(int id, [FromBody] Marca marca)
    {
        if (id != marca.IdMarca)
        {
            return BadRequest(new { mensaje = "El ID de la URL no coincide con el ID del cuerpo de la solicitud." });
        }

        var existe = await _context.Marcas.AnyAsync(m => m.IdMarca == id);
        if (!existe)
        {
            return NotFound(new { mensaje = $"La marca con ID {id} no existe." });
        }

        _context.Entry(marca).State = EntityState.Modified;
        await _context.SaveChangesAsync();

        return Ok(marca);
    }

    // DELETE: api/Marcas/5
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteMarca(int id)
    {
        var marca = await _context.Marcas.FindAsync(id);
        if (marca == null)
        {
            return NotFound(new { mensaje = $"La marca con ID {id} no existe." });
        }

        var tieneProductos = await _context.Productos.AnyAsync(p => p.IdMarca == id);
        if (tieneProductos)
        {
            return BadRequest(new { mensaje = "No se puede eliminar la marca porque tiene productos asociados (FK Física)." });
        }

        _context.Marcas.Remove(marca);
        await _context.SaveChangesAsync();

        return Ok(new { mensaje = "Marca eliminada correctamente." });
    }
}

