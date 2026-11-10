using Catalogo.Api.Data;
using Catalogo.Api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Catalogo.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProductosController : ControllerBase
{
    private readonly CatalogoDbContext _context;

    public ProductosController(CatalogoDbContext context)
    {
        _context = context;
    }

    // GET: api/Productos
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Producto>>> GetProductos()
    {
        var productos = await _context.Productos
            .Include(p => p.Categoria)
            .Include(p => p.Marca)
            .ToListAsync();
        return Ok(productos);
    }

    // GET: api/Productos/5
    [HttpGet("{id:int}")]
    public async Task<ActionResult<Producto>> GetProducto(int id)
    {
        var producto = await _context.Productos
            .Include(p => p.Categoria)
            .Include(p => p.Marca)
            .FirstOrDefaultAsync(p => p.IdProducto == id);

        if (producto == null)
        {
            return NotFound(new { mensaje = $"El producto con ID {id} no existe en el catálogo." });
        }

        return Ok(producto);
    }

    // POST: api/Productos
    [HttpPost]
    public async Task<ActionResult<Producto>> PostProducto([FromBody] Producto producto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        // Validación de FK Física: Categoria
        var categoriaExiste = await _context.Categorias.AnyAsync(c => c.IdCategoria == producto.IdCategoria);
        if (!categoriaExiste)
        {
            return BadRequest(new { mensaje = $"La categoría con ID {producto.IdCategoria} no existe." });
        }

        // Validación de FK Física: Marca
        var marcaExiste = await _context.Marcas.AnyAsync(m => m.IdMarca == producto.IdMarca);
        if (!marcaExiste)
        {
            return BadRequest(new { mensaje = $"La marca con ID {producto.IdMarca} no existe." });
        }

        // Validación de SKU único
        var skuDuplicado = await _context.Productos.AnyAsync(p => p.SKU == producto.SKU);
        if (skuDuplicado)
        {
            return BadRequest(new { mensaje = $"Ya existe un producto con el SKU '{producto.SKU}'." });
        }

        _context.Productos.Add(producto);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetProducto), new { id = producto.IdProducto }, producto);
    }

    // PUT: api/Productos/5
    [HttpPut("{id:int}")]
    public async Task<IActionResult> PutProducto(int id, [FromBody] Producto producto)
    {
        if (id != producto.IdProducto)
        {
            return BadRequest(new { mensaje = "El ID de la URL no coincide con el ID del producto." });
        }

        var prodExistente = await _context.Productos.AsNoTracking().FirstOrDefaultAsync(p => p.IdProducto == id);
        if (prodExistente == null)
        {
            return NotFound(new { mensaje = $"El producto con ID {id} no existe." });
        }

        if (!await _context.Categorias.AnyAsync(c => c.IdCategoria == producto.IdCategoria))
        {
            return BadRequest(new { mensaje = $"La categoría con ID {producto.IdCategoria} no existe." });
        }

        if (!await _context.Marcas.AnyAsync(m => m.IdMarca == producto.IdMarca))
        {
            return BadRequest(new { mensaje = $"La marca con ID {producto.IdMarca} no existe." });
        }

        if (await _context.Productos.AnyAsync(p => p.SKU == producto.SKU && p.IdProducto != id))
        {
            return BadRequest(new { mensaje = $"El SKU '{producto.SKU}' ya está registrado en otro producto." });
        }

        _context.Entry(producto).State = EntityState.Modified;
        await _context.SaveChangesAsync();

        return Ok(producto);
    }

    // DELETE: api/Productos/5
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteProducto(int id)
    {
        var producto = await _context.Productos.FindAsync(id);
        if (producto == null)
        {
            return NotFound(new { mensaje = $"El producto con ID {id} no existe." });
        }

        _context.Productos.Remove(producto);
        await _context.SaveChangesAsync();

        return Ok(new { mensaje = "Producto eliminado correctamente." });
    }
}

