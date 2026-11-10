using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace Pedidos.Api.Models;

[Table("DetallePedido")]
public class DetallePedido
{
    [Key]
    public int IdDetalle { get; set; }

    // FK FÍSICA: Referencia a PedidosDB.dbo.Pedido
    public int IdPedido { get; set; }

    // FK LÓGICA: Referencia a CatalogoDB.dbo.Producto
    [Required(ErrorMessage = "El IdProducto es obligatorio.")]
    [Range(1, int.MaxValue, ErrorMessage = "El IdProducto debe ser mayor a 0.")]
    public int IdProducto { get; set; }

    [Required(ErrorMessage = "La cantidad es obligatoria.")]
    [Range(1, int.MaxValue, ErrorMessage = "La cantidad debe ser al menos 1.")]
    public int Cantidad { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    [Range(0.01, 999999.99, ErrorMessage = "El precio unitario debe ser mayor a 0.")]
    public decimal PrecioUnitario { get; set; }

    [JsonIgnore]
    [ForeignKey(nameof(IdPedido))]
    public Pedido? Pedido { get; set; }
}

