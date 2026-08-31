namespace Tamagotchi.Juego;

/// Las etapas de vida del pingüino, en orden. El nombre de cada valor
/// coincide con el nombre de la carpeta de sprites en Assets/Mascota/
/// (Bebe, Nino, Adolescente), así Recursos.Sprite() arma la ruta sola.

/// Adulto es la etapa "final": al llegar ahí no se usan más sprites de
/// Mascota, sino la escena completa Final.png

public enum Etapa
{
    Bebe,
    Nino,
    Adolescente,
    Adulto
}
