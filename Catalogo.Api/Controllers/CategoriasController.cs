using Catalogo.Api.Data;
using Catalogo.Api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Catalogo.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CategoriasController : ControllerBase
{
    private readonly CatalogoDbContext _context;

    public CategoriasController(CatalogoDbContext context)
    {
        _context = context;
    }

    // GET: api/Categorias
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Categoria>>> GetCategorias()
    {
        return Ok(await _context.Categorias.ToListAsync());
    }

    // GET: api/Categorias/5
    [HttpGet("{id:int}")]
    public async Task<ActionResult<Categoria>> GetCategoria(int id)
    {
        var categoria = await _context.Categorias.FindAsync(id);
        if (categoria == null)
        {
            return NotFound(new { mensaje = $"La categoría con ID {id} no existe." });
        }
        return Ok(categoria);
    }

    // POST: api/Categorias
    [HttpPost]
    public async Task<ActionResult<Categoria>> PostCategoria([FromBody] Categoria categoria)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        _context.Categorias.Add(categoria);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetCategoria), new { id = categoria.IdCategoria }, categoria);
    }

    // PUT: api/Categorias/5
    [HttpPut("{id:int}")]
    public async Task<IActionResult> PutCategoria(int id, [FromBody] Categoria categoria)
    {
        if (id != categoria.IdCategoria)
        {
            return BadRequest(new { mensaje = "El ID de la URL no coincide con el ID del cuerpo de la solicitud." });
        }

        var existe = await _context.Categorias.AnyAsync(c => c.IdCategoria == id);
        if (!existe)
        {
            return NotFound(new { mensaje = $"La categoría con ID {id} no existe." });
        }

        _context.Entry(categoria).State = EntityState.Modified;
        await _context.SaveChangesAsync();

        return Ok(categoria);
    }

    // DELETE: api/Categorias/5
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteCategoria(int id)
    {
        var categoria = await _context.Categorias.FindAsync(id);
        if (categoria == null)
        {
            return NotFound(new { mensaje = $"La categoría con ID {id} no existe." });
        }

        var tieneProductos = await _context.Productos.AnyAsync(p => p.IdCategoria == id);
        if (tieneProductos)
        {
            return BadRequest(new { mensaje = "No se puede eliminar la categoría porque tiene productos asociados (FK Física)." });
        }

        _context.Categorias.Remove(categoria);
        await _context.SaveChangesAsync();

        return Ok(new { mensaje = "Categoría eliminada correctamente." });
    }
}

