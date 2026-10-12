namespace TamagotchiApi.Models;

/// Una partida guardada: el pingüino de un usuario, en qué etapa de vida
/// va, si es de día o de noche y qué ropa tiene puesta.
/// Las necesidades (hambre, energía...) están aparte, en EstadoPinguino.
public class Pinguino
{
    public int Id { get; set; }

    public string Nombre { get; set; } = "";

    public int UsuarioId { get; set; }

    /// Mismo texto que el enum Etapa del juego: "Bebe", "Nino", "Adolescente" o "Adulto"
    public string Etapa { get; set; } = "Bebe";

    /// Cuántos ticks lleva en la etapa actual, para que al volver a jugar
    /// no arranque la etapa de cero
    public int TicksEtapa { get; set; }

    public bool EsDeDia { get; set; } = true;

    /// null = está con su ropa de siempre (la que viene dibujada en cada etapa)
    public int? RopaPuestaId { get; set; }

    /// null = la partida sigue en juego. "Adulto" o "GameOver" = ya terminó
    public string? Resultado { get; set; }

    public DateTime FechaCreacion { get; set; }

    public DateTime UltimoGuardado { get; set; }

    public virtual Usuario Usuario { get; set; } = null!;

    public virtual EstadoPinguino? Estado { get; set; }

    public virtual Ropa? RopaPuesta { get; set; }

    public virtual ICollection<AccionRegistrada> Acciones { get; set; } = new List<AccionRegistrada>();
}
