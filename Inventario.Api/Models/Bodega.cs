using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace Inventario.Api.Models;

[Table("Bodega")]
public class Bodega
{
    [Key]
    public int IdBodega { get; set; }

    [Required(ErrorMessage = "El nombre de la bodega es obligatorio.")]
    [StringLength(100, ErrorMessage = "El nombre no puede exceder los 100 caracteres.")]
    public string Nombre { get; set; } = string.Empty;

    [Required(ErrorMessage = "La ciudad es obligatoria.")]
    [StringLength(100, ErrorMessage = "La ciudad no puede exceder los 100 caracteres.")]
    public string Ciudad { get; set; } = string.Empty;

    [Required(ErrorMessage = "La dirección es obligatoria.")]
    [StringLength(200, ErrorMessage = "La dirección no puede exceder los 200 caracteres.")]
    public string Direccion { get; set; } = string.Empty;

    [JsonIgnore]
    public ICollection<Existencia>? Existencias { get; set; }

    [JsonIgnore]
    public ICollection<MovimientoInventario>? Movimientos { get; set; }

    [JsonIgnore]
    public ICollection<ReservaStock>? Reservas { get; set; }
}

