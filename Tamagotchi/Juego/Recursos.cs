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
        => Sprite(accion, GestorJuego.Instancia.Etapa, GestorJuego.Instancia.RopaClave);

    /// Igual que el de arriba, pero pidiendo una etapa y una ropa puntual
    /// (lo usa la rueda del ropero para mostrar cómo queda cada ropa).
    /// ropa = null es la ropa de siempre.
    ///
    /// Con ropa, los sprites están en Assets/Ropa/{Etapa}/{Ropa}{Etapa}{Accion}.png
    /// (ej: Assets/Ropa/Bebe/OsoBebeComiendo.png). La única acción que NO
    /// usa ropa es "Banandose": se baña sin ropa, siempre con el mismo dibujo.
    public static Bitmap Sprite(string accion, Etapa etapaPedida, string? ropa)
    {
        // Si algo llegara a pedir un sprite en ese momento, mostramos el último
        // sprite "real" del pingüino (Adolescente) en vez de romper el codigo por falta de sprites.

        string etapa = etapaPedida == Etapa.Adulto ? "Adolescente" : etapaPedida.ToString();

        if (ropa != null && accion != "Banandose")
        {
            string rutaRopa = $"avares://Tamagotchi/Assets/Ropa/{etapa}/{ropa}{etapa}{accion}.png";

            // Si esa acción no está dibujada con esa ropa (por ejemplo
            // "Triste" en Nino), usamos el Normal CON la ropa, así no se le
            // "cae" la ropa de golpe.
            if (!ExisteAsset(rutaRopa))
                rutaRopa = $"avares://Tamagotchi/Assets/Ropa/{etapa}/{ropa}{etapa}Normal.png";

            if (ExisteAsset(rutaRopa))
                return CargarBitmap(rutaRopa);

            // Si ni siquiera existe el Normal con esa ropa, sigue de largo
            // y se muestra el sprite de siempre.
        }

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
