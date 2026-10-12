using System.ComponentModel.DataAnnotations;

namespace TamagotchiApi.Dtos;

public class CrearPinguinoRequest
{
    [Range(1, int.MaxValue, ErrorMessage = "Falta indicar el usuario.")]
    public int UsuarioId { get; set; }

    [Required(ErrorMessage = "El nombre del pingüino es obligatorio.")]
    [StringLength(20, MinimumLength = 2, ErrorMessage = "El nombre del pingüino debe tener entre 2 y 20 caracteres.")]
    public string Nombre { get; set; } = "";
}

/// Las necesidades del pingüino (igual que EstadoMascota en el juego)
public class EstadoDto
{
    [Range(0, 100)] public double Hambre { get; set; }
    [Range(0, 100)] public double Energia { get; set; }
    [Range(0, 100)] public double Higiene { get; set; }
    [Range(0, 100)] public double Diversion { get; set; }
    [Range(0, 100)] public double Vejiga { get; set; }
    public bool Enfermo { get; set; }
}

/// Lo que manda el juego cada vez que guarda
public class GuardarPartidaRequest
{
    [Required]
    [RegularExpression("^(Bebe|Nino|Adolescente|Adulto)$", ErrorMessage = "Etapa inválida.")]
    public string Etapa { get; set; } = "Bebe";

    [Range(0, int.MaxValue)]
    public int TicksEtapa { get; set; }

    public bool EsDeDia { get; set; } = true;

    [Required]
    public EstadoDto Estado { get; set; } = new EstadoDto();

    /// null mientras se sigue jugando; "Adulto" o "GameOver" cuando termina
    [RegularExpression("^(Adulto|GameOver)$", ErrorMessage = "Resultado inválido.")]
    public string? Resultado { get; set; }
}

public class CambiarRopaRequest
{
    /// null = volver a la ropa de siempre
    public int? RopaId { get; set; }
}

public class RegistrarAccionRequest
{
    [Required]
    [RegularExpression("^(Comer|Dormir|Banarse|Inodoro|Jugar)$", ErrorMessage = "Acción inválida.")]
    public string Accion { get; set; } = "";
}

/// Una fila de la lista de pingüinos guardados de un usuario
public class PinguinoResumen
{
    public int Id { get; set; }
    public string Nombre { get; set; } = "";
    public string Etapa { get; set; } = "";
    public string? Resultado { get; set; }
    public string? RopaNombre { get; set; }
    public DateTime UltimoGuardado { get; set; }
    public string? AccionFavorita { get; set; }
}

/// Todo lo necesario para retomar la partida de un pingüino
public class PinguinoDetalle
{
    public int Id { get; set; }
    public string Nombre { get; set; } = "";
    public int UsuarioId { get; set; }
    public string UsuarioNombre { get; set; } = "";
    public string Etapa { get; set; } = "";
    public int TicksEtapa { get; set; }
    public bool EsDeDia { get; set; }
    public int? RopaId { get; set; }
    public string? RopaClave { get; set; }
    public string? RopaNombre { get; set; }
    public string? Resultado { get; set; }
    public EstadoDto Estado { get; set; } = new EstadoDto();
}

public class RopaDto
{
    public int Id { get; set; }
    public string Nombre { get; set; } = "";
    public string Clave { get; set; } = "";
    public string Etapa { get; set; } = "";
    public int Orden { get; set; }
}
