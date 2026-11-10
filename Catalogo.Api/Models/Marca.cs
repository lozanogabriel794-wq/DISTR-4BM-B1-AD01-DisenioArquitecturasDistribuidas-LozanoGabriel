using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace Catalogo.Api.Models;

[Table("Marca")]
public class Marca
{
    [Key]
    public int IdMarca { get; set; }

    [Required(ErrorMessage = "El nombre de la marca es obligatorio.")]
    [StringLength(100, ErrorMessage = "El nombre no puede exceder los 100 caracteres.")]
    public string Nombre { get; set; } = string.Empty;

    [StringLength(100, ErrorMessage = "El país de origen no puede exceder los 100 caracteres.")]
    public string? PaisOrigen { get; set; }

    [JsonIgnore]
    public ICollection<Producto>? Productos { get; set; }
}

