using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace Catalogo.Api.Models;

[Table("Categoria")]
public class Categoria
{
    [Key]
    public int IdCategoria { get; set; }

    [Required(ErrorMessage = "El nombre de la categoría es obligatorio.")]
    [StringLength(100, ErrorMessage = "El nombre no puede exceder los 100 caracteres.")]
    public string Nombre { get; set; } = string.Empty;

    [StringLength(250, ErrorMessage = "La descripción no puede exceder los 250 caracteres.")]
    public string? Descripcion { get; set; }

    [JsonIgnore]
    public ICollection<Producto>? Productos { get; set; }
}

