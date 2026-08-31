using System;
using System.Collections.Generic;
using Avalonia.Media.Imaging;
using Avalonia.Platform;

namespace Tamagotchi.Juego;


/// Carga las imágenes del pingüino (y la de transición) una sola vez y
/// las guarda en memoria (así no se lee el archivo de nuevo cada vez que
/// cambia la animación).

public static class Recursos
{
    private static readonly Dictionary<string, Bitmap> _cache = new();


    /// Devuelve el sprite de una acción (ej: "Comiendo", "Normal",
    /// "Triste") para la etapa ACTUAL del pingüino (GestorJuego.Etapa)
    /// La carpeta y el prefijo del archivo son el nombre de la etapa,
    /// por eso "Comiendo" en Nino busca Assets/Mascota/Nino/NinoComiendo.png

    /// En Adulto el pingüino ya no tiene sprites por acción (esa
    /// etapa se muestra con la escena completa Final.png en FinalView.

  
    public static Bitmap Sprite(string accion)
    {
        var etapaActual = GestorJuego.Instancia.Etapa;

        // Si algo llegara a pedir un sprite en ese momento, mostramos el último
        // sprite "real" del pingüino (Adolescente) en vez de romper el codigo por falta de sprites.

        string etapa = etapaActual == Etapa.Adulto ? "Adolescente" : etapaActual.ToString();

        string ruta = $"avares://Tamagotchi/Assets/Mascota/{etapa}/{etapa}{accion}.png";

        if (!ExisteAsset(ruta))
        {
            // Nino y Adolescente no tienen dibujado el sprite "Triste", y
            // Adolescente tampoco tiene "Flores", para eso  uso
            // el sprite Normal.
            ruta = $"avares://Tamagotchi/Assets/Mascota/{etapa}/{etapa}Normal.png";
        }

        return CargarBitmap(ruta);
    }

    /// La imagen que tapa por completo al pingüino durante un cambio de etapa
    public static Bitmap Transicion()
        => CargarBitmap("avares://Tamagotchi/Assets/Efectos/Cambio.png");

    private static bool ExisteAsset(string ruta) => AssetLoader.Exists(new Uri(ruta));

    private static Bitmap CargarBitmap(string ruta)
    {
        if (!_cache.TryGetValue(ruta, out var bitmap))
        {
            bitmap = new Bitmap(AssetLoader.Open(new Uri(ruta)));
            _cache[ruta] = bitmap;
        }

        return bitmap;
    }
}
