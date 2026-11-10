using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Catalogo.Api.Models;

[Table("Producto")]
public class Producto
{
    [Key]
    public int IdProducto { get; set; }

    [Required(ErrorMessage = "El IdCategoria es obligatorio.")]
    [Range(1, int.MaxValue, ErrorMessage = "Debe seleccionar una categoría válida.")]
    public int IdCategoria { get; set; }

    [Required(ErrorMessage = "El IdMarca es obligatorio.")]
    [Range(1, int.MaxValue, ErrorMessage = "Debe seleccionar una marca válida.")]
    public int IdMarca { get; set; }

    [Required(ErrorMessage = "El nombre del producto es obligatorio.")]
    [StringLength(150, ErrorMessage = "El nombre no puede exceder los 150 caracteres.")]
    public string Nombre { get; set; } = string.Empty;

    [Required(ErrorMessage = "El código SKU es obligatorio.")]
    [StringLength(50, ErrorMessage = "El SKU no puede exceder los 50 caracteres.")]
    public string SKU { get; set; } = string.Empty;

    [Required(ErrorMessage = "El precio es obligatorio.")]
    [Range(0.01, 999999.99, ErrorMessage = "El precio debe ser mayor a 0.")]
    [Column(TypeName = "decimal(18,2)")]
    public decimal Precio { get; set; }

    public bool Estado { get; set; } = true;

    [ForeignKey(nameof(IdCategoria))]
    public Categoria? Categoria { get; set; }

    [ForeignKey(nameof(IdMarca))]
    public Marca? Marca { get; set; }
}

