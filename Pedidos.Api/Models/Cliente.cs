using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace Pedidos.Api.Models;

[Table("Cliente")]
public class Cliente
{
    [Key]
    public int IdCliente { get; set; }

    [Required(ErrorMessage = "Los nombres del cliente son obligatorios.")]
    [StringLength(150, ErrorMessage = "Los nombres no pueden exceder los 150 caracteres.")]
    public string Nombres { get; set; } = string.Empty;

    [Required(ErrorMessage = "La cédula es obligatoria.")]
    [StringLength(20, MinimumLength = 10, ErrorMessage = "La cédula debe tener entre 10 y 20 caracteres.")]
    public string Cedula { get; set; } = string.Empty;

    [Required(ErrorMessage = "El correo electrónico es obligatorio.")]
    [EmailAddress(ErrorMessage = "El formato del correo electrónico no es válido.")]
    [StringLength(150)]
    public string Correo { get; set; } = string.Empty;

    [JsonIgnore]
    public ICollection<Pedido>? Pedidos { get; set; }
}

