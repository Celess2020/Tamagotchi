namespace TamagotchiApi.Models;

/// Las necesidades del pingüino al momento de guardar. Es la "foto" de
/// la clase EstadoMascota del juego. Relación 1 a 1 con Pinguino.
public class EstadoPinguino
{
    public int Id { get; set; }

    public int PinguinoId { get; set; }

    // Mismos valores iniciales que EstadoMascota en el juego
    public double Hambre { get; set; } = 50;
    public double Energia { get; set; } = 50;
    public double Higiene { get; set; } = 50;
    public double Diversion { get; set; } = 50;
    public double Vejiga { get; set; } = 20;

    public bool Enfermo { get; set; }

    public virtual Pinguino Pinguino { get; set; } = null!;
}
