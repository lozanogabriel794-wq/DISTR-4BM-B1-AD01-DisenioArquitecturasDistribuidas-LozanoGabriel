using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Inventario.Api.Models;

[Table("Existencia")]
public class Existencia
{
    [Key]
    public int IdExistencia { get; set; }

    // FK LÓGICA: Referencia a CatalogoDB.dbo.Producto
    [Required(ErrorMessage = "El IdProducto es obligatorio.")]
    [Range(1, int.MaxValue, ErrorMessage = "El IdProducto debe ser válido.")]
    public int IdProducto { get; set; }

    // FK FÍSICA: Referencia a InventarioDB.dbo.Bodega
    [Required(ErrorMessage = "El IdBodega es obligatorio.")]
    [Range(1, int.MaxValue, ErrorMessage = "El IdBodega debe ser válido.")]
    public int IdBodega { get; set; }

    [Required(ErrorMessage = "El stock es obligatorio.")]
    [Range(0, int.MaxValue, ErrorMessage = "El stock no puede ser negativo.")]
    public int Stock { get; set; }

    [ForeignKey(nameof(IdBodega))]
    public Bodega? Bodega { get; set; }
}

