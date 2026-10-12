namespace TamagotchiApi.Models;

/// La persona que juega. Cada usuario puede tener varios pingüinos
/// guardados (por ejemplo, yo juego con uno y otra persona con otro).
public class Usuario
{
    public int Id { get; set; }

    public string Nombre { get; set; } = "";

    public DateTime FechaCreacion { get; set; }

    public virtual ICollection<Pinguino> Pinguinos { get; set; } = new List<Pinguino>();
}
