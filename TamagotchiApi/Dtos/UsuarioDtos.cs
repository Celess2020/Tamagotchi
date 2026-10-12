using System.ComponentModel.DataAnnotations;

namespace TamagotchiApi.Dtos;

// Clases que viajan como JSON entre la API y el juego.
// Las validaciones ([Required], [StringLength]...) las chequea solo
// [ApiController]: si algo no cumple, devuelve 400 sin entrar al método.

public class UsuarioRequest
{
    [Required(ErrorMessage = "El nombre de usuario es obligatorio.")]
    [StringLength(30, MinimumLength = 2, ErrorMessage = "El nombre de usuario debe tener entre 2 y 30 caracteres.")]
    public string Nombre { get; set; } = "";
}

public class UsuarioResponse
{
    public int Id { get; set; }
    public string Nombre { get; set; } = "";
    public DateTime FechaCreacion { get; set; }
    public int CantidadPinguinos { get; set; }
}
