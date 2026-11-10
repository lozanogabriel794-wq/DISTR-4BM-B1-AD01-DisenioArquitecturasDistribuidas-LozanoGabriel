using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Pedidos.Api.Data;
using Pedidos.Api.Models;

namespace Pedidos.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ClientesController : ControllerBase
{
    private readonly PedidosDbContext _context;

    public ClientesController(PedidosDbContext context)
    {
        _context = context;
    }

    // GET: api/Clientes
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Cliente>>> GetClientes()
    {
        return Ok(await _context.Clientes.ToListAsync());
    }

    // GET: api/Clientes/5
    [HttpGet("{id:int}")]
    public async Task<ActionResult<Cliente>> GetCliente(int id)
    {
        var cliente = await _context.Clientes.FindAsync(id);
        if (cliente == null)
        {
            return NotFound(new { mensaje = $"El cliente con ID {id} no existe." });
        }
        return Ok(cliente);
    }

    // POST: api/Clientes
    [HttpPost]
    public async Task<ActionResult<Cliente>> PostCliente([FromBody] Cliente cliente)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        if (await _context.Clientes.AnyAsync(c => c.Cedula == cliente.Cedula))
        {
            return BadRequest(new { mensaje = $"Ya existe un cliente registrado con la cédula '{cliente.Cedula}'." });
        }

        _context.Clientes.Add(cliente);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetCliente), new { id = cliente.IdCliente }, cliente);
    }

    // PUT: api/Clientes/5
    [HttpPut("{id:int}")]
    public async Task<IActionResult> PutCliente(int id, [FromBody] Cliente cliente)
    {
        if (id != cliente.IdCliente)
        {
            return BadRequest(new { mensaje = "El ID de la URL no coincide con el ID del cliente." });
        }

        if (!await _context.Clientes.AnyAsync(c => c.IdCliente == id))
        {
            return NotFound(new { mensaje = $"El cliente con ID {id} no existe." });
        }

        if (await _context.Clientes.AnyAsync(c => c.Cedula == cliente.Cedula && c.IdCliente != id))
        {
            return BadRequest(new { mensaje = $"La cédula '{cliente.Cedula}' ya pertenece a otro cliente." });
        }

        _context.Entry(cliente).State = EntityState.Modified;
        await _context.SaveChangesAsync();

        return Ok(cliente);
    }

    // DELETE: api/Clientes/5
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteCliente(int id)
    {
        var cliente = await _context.Clientes.FindAsync(id);
        if (cliente == null)
        {
            return NotFound(new { mensaje = $"El cliente con ID {id} no existe." });
        }

        if (await _context.Pedidos.AnyAsync(p => p.IdCliente == id))
        {
            return BadRequest(new { mensaje = "No se puede eliminar el cliente porque tiene pedidos registrados (FK Física)." });
        }

        _context.Clientes.Remove(cliente);
        await _context.SaveChangesAsync();

        return Ok(new { mensaje = "Cliente eliminado correctamente." });
    }
}

