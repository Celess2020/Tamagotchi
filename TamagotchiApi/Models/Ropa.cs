namespace TamagotchiApi.Models;

/// Catálogo de ropa que aparece en la rueda del ropero.
/// Cada ropa está dibujada para UNA etapa de vida (Bebe, Nino o Adolescente).
/// La Clave es el prefijo de los archivos de sprites en el juego:
/// Assets/Ropa/{Etapa}/{Clave}{Etapa}{Accion}.png  (ej: Assets/Ropa/Bebe/OsoBebeNormal.png)
public class Ropa
{
    public int Id { get; set; }

    public string Nombre { get; set; } = "";

    public string Clave { get; set; } = "";

    /// Etapa para la que está dibujada: "Bebe", "Nino" o "Adolescente"
    public string Etapa { get; set; } = "Bebe";

    /// Posición en la rueda (de menor a mayor)
    public int Orden { get; set; }

    public virtual ICollection<Pinguino> Pinguinos { get; set; } = new List<Pinguino>();
}
