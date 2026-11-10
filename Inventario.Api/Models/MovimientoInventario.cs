using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Inventario.Api.Models;

[Table("MovimientoInventario")]
public class MovimientoInventario
{
    [Key]
    public int IdMovimiento { get; set; }

    // FK LÓGICA: Referencia a CatalogoDB.dbo.Producto
    [Required(ErrorMessage = "El IdProducto es obligatorio.")]
    [Range(1, int.MaxValue, ErrorMessage = "El IdProducto debe ser válido.")]
    public int IdProducto { get; set; }

    // FK FÍSICA: Referencia a InventarioDB.dbo.Bodega
    [Required(ErrorMessage = "El IdBodega es obligatorio.")]
    [Range(1, int.MaxValue, ErrorMessage = "El IdBodega debe ser válido.")]
    public int IdBodega { get; set; }

    [Required(ErrorMessage = "El tipo de movimiento es obligatorio (ENTRADA, SALIDA, AJUSTE).")]
    [RegularExpression("^(ENTRADA|SALIDA|AJUSTE)$", ErrorMessage = "El tipo debe ser ENTRADA, SALIDA o AJUSTE.")]
    [StringLength(50)]
    public string Tipo { get; set; } = "ENTRADA";

    [Required(ErrorMessage = "La cantidad es obligatoria.")]
    [Range(1, int.MaxValue, ErrorMessage = "La cantidad del movimiento debe ser mayor a 0.")]
    public int Cantidad { get; set; }

    public DateTime Fecha { get; set; } = DateTime.Now;

    [ForeignKey(nameof(IdBodega))]
    public Bodega? Bodega { get; set; }
}

