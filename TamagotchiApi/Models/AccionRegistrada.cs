namespace TamagotchiApi.Models;

/// Historial de lo que hizo cada pingüino (Comer, Dormir, Banarse...).
/// Con esto se calcula, por ejemplo, la acción favorita de cada uno.
public class AccionRegistrada
{
    public int Id { get; set; }

    public int PinguinoId { get; set; }

    public string Accion { get; set; } = "";

    public DateTime Fecha { get; set; }

    public virtual Pinguino Pinguino { get; set; } = null!;
}
