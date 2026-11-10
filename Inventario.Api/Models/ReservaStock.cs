using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Inventario.Api.Models;

[Table("ReservaStock")]
public class ReservaStock
{
    [Key]
    public int IdReserva { get; set; }

    // FK LÓGICA: Referencia a CatalogoDB.dbo.Producto
    [Required(ErrorMessage = "El IdProducto es obligatorio.")]
    [Range(1, int.MaxValue, ErrorMessage = "El IdProducto debe ser válido.")]
    public int IdProducto { get; set; }

    // FK FÍSICA: Referencia a InventarioDB.dbo.Bodega
    [Required(ErrorMessage = "El IdBodega es obligatorio.")]
    [Range(1, int.MaxValue, ErrorMessage = "El IdBodega debe ser válido.")]
    public int IdBodega { get; set; }

    [Required(ErrorMessage = "La cantidad a reservar es obligatoria.")]
    [Range(1, int.MaxValue, ErrorMessage = "La cantidad debe ser mayor a 0.")]
    public int Cantidad { get; set; }

    [Required(ErrorMessage = "El estado es obligatorio.")]
    [RegularExpression("^(PENDIENTE|CONFIRMADA|CANCELADA)$", ErrorMessage = "El estado debe ser PENDIENTE, CONFIRMADA o CANCELADA.")]
    [StringLength(50)]
    public string Estado { get; set; } = "PENDIENTE";

    [ForeignKey(nameof(IdBodega))]
    public Bodega? Bodega { get; set; }
}

