using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Pedidos.Api.Models;

[Table("Pedido")]
public class Pedido
{
    [Key]
    public int IdPedido { get; set; }

    // FK FÍSICA: Referencia a PedidosDB.dbo.Cliente
    [Required(ErrorMessage = "El IdCliente es obligatorio.")]
    [Range(1, int.MaxValue, ErrorMessage = "El IdCliente debe ser válido.")]
    public int IdCliente { get; set; }

    public DateTime Fecha { get; set; } = DateTime.Now;

    [Column(TypeName = "decimal(18,2)")]
    [Range(0, 9999999.99, ErrorMessage = "El total no puede ser negativo.")]
    public decimal Total { get; set; }

    [Required(ErrorMessage = "El estado es obligatorio.")]
    [RegularExpression("^(PENDIENTE|PAGADO|CANCELADO|PROCESADO)$", ErrorMessage = "El estado debe ser PENDIENTE, PAGADO, CANCELADO o PROCESADO.")]
    [StringLength(50)]
    public string Estado { get; set; } = "PENDIENTE";

    [ForeignKey(nameof(IdCliente))]
    public Cliente? Cliente { get; set; }

    public List<DetallePedido> Detalles { get; set; } = new();
}

