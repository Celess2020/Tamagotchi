using System;

namespace Tamagotchi.Juego;

/// Las "necesidades" del pingüino. Todas van de 0 a 100.
/// Hambre, Energia, Higiene y Diversion: 100 = perfecto, 0 = pésimo.
/// Vejiga: al revés, 0 = no tiene ganas, 100 = urgente ir al baño.

public class EstadoMascota
{
    // Arrancan a "mitad de camino": así el jugador tiene algo para hacer
    // desde el principio (no todo maxeado) pero también margen de sobra
    // antes de que algo se ponga crítico.
    public double Hambre { get; set; } = 50;
    public double Energia { get; set; } = 50;
    public double Higiene { get; set; } = 50;
    public double Diversion { get; set; } = 50;
    public double Vejiga { get; set; } = 20;

    public bool Enfermo { get; set; } = false;


    /// Promedio general de bienestar (0 a 100). Se usa para decidir si el
    /// pingüino se ve Triste o Normal (además de Enfermo). Si tiene muchas
    /// ganas de ir al baño, el bienestar baja aunque el resto esté bien.
    
       public double Bienestar
    {
        get
        {
            double promedio = (Hambre + Energia + Higiene + Diversion) / 4.0;
            if (Vejiga > 80)
                promedio -= (Vejiga - 80);

            return Math.Clamp(promedio, 0, 100);
        }
    }
}
