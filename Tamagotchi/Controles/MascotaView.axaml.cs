using System;
using Avalonia.Controls;
using Avalonia.Threading;
using Tamagotchi.Juego;

namespace Tamagotchi.Controles;

/// <summary>
/// Muestra al pingüino y decide automáticamente qué sprite usar:
///   - Si le pediste una acción puntual (comer, dormir, jugar...) la muestra
///     unos segundos y después vuelve a su estado normal.
///   - Si no, se fija el bienestar general: Enfermo > Triste > Feliz > Normal.
///   - En "Normal" parpadea cada tanto (Normal / NormalParpadeo).
/// </summary>
public partial class MascotaView : UserControl
{
    private readonly DispatcherTimer _timerParpadeo;
    private readonly DispatcherTimer _timerFinParpadeo;
    private readonly DispatcherTimer _timerFinAccion;

    private bool _parpadeando;
    private bool _mostrandoAccion;

    // Posición "de reposo" del pingüino en la habitación actual (la que
    // tiene puesta en el .axaml). La guardamos para poder volver acá
    // cuando termina una acción que lo movió (a la cama, a la ducha, etc.).
    private double _posReposoLeft;
    private double _posReposoTop;
    private bool _posReposoCapturada;

    public MascotaView()
    {
        InitializeComponent();

        _timerParpadeo = new DispatcherTimer { Interval = TimeSpan.FromSeconds(3.5) };
        _timerParpadeo.Tick += (_, _) => Parpadear();

        _timerFinParpadeo = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(200) };
        _timerFinParpadeo.Tick += (_, _) =>
        {
            _timerFinParpadeo.Stop();
            _parpadeando = false;
            Refrescar();
        };

        _timerFinAccion = new DispatcherTimer();
        _timerFinAccion.Tick += (_, _) =>
        {
            _timerFinAccion.Stop();
            _mostrandoAccion = false;

            // Al terminar la animación, vuelve a su posición de reposo.
            if (_posReposoCapturada)
            {
                Canvas.SetLeft(this, _posReposoLeft);
                Canvas.SetTop(this, _posReposoTop);
            }

            Refrescar();
        };

        AttachedToVisualTree += (_, _) =>
        {
            if (!_posReposoCapturada)
            {
                _posReposoLeft = Canvas.GetLeft(this);
                _posReposoTop = Canvas.GetTop(this);
                _posReposoCapturada = true;
            }

            GestorJuego.Instancia.CambioEstado += Refrescar;
            GestorJuego.Instancia.TransicionEtapaIniciada += TaparPorTransicion;
            GestorJuego.Instancia.TransicionEtapaFinalizada += DestaparPorTransicion;
            _timerParpadeo.Start();
            Refrescar();
        };

        DetachedFromVisualTree += (_, _) =>
        {
            GestorJuego.Instancia.CambioEstado -= Refrescar;
            GestorJuego.Instancia.TransicionEtapaIniciada -= TaparPorTransicion;
            GestorJuego.Instancia.TransicionEtapaFinalizada -= DestaparPorTransicion;
            _timerParpadeo.Stop();
            _timerFinParpadeo.Stop();
            _timerFinAccion.Stop();
        };
    }

    private void Parpadear()
    {
        if (_mostrandoAccion) return;

        _parpadeando = true;
        Refrescar();

        _timerFinParpadeo.Stop();
        _timerFinParpadeo.Start();
    }

    /// <summary>
    /// Lo llaman las vistas (Habitacion, Cocina, etc.) cuando el usuario
    /// interactúa con algo. Ej: Mascota.MostrarAccion("Comiendo", 3).
    /// El nombre de la acción va SIN el prefijo de la etapa (nada de
    /// "Bebe..."): Recursos.Sprite ya sabe en qué etapa está el pingüino
    /// y arma la ruta sola, así el mismo llamado sirve en las tres etapas.
    ///
    /// Si se pasan <paramref name="left"/> y <paramref name="top"/>, además
    /// mueve al pingüino a esa posición del Canvas (por ejemplo, arriba de
    /// la cama o frente a la mesa) mientras dura la acción. Al terminar,
    /// vuelve solo a su posición de reposo original.
    /// </summary>
    public void MostrarAccion(string nombreAccion, double segundos = 2.5, double? left = null, double? top = null)
    {
        _mostrandoAccion = true;
        ImagenMascota.Source = Recursos.Sprite(nombreAccion);

        if (left.HasValue && top.HasValue)
        {
            Canvas.SetLeft(this, left.Value);
            Canvas.SetTop(this, top.Value);
        }

        _timerFinAccion.Stop();
        _timerFinAccion.Interval = TimeSpan.FromSeconds(segundos);
        _timerFinAccion.Start();
    }

    private void Refrescar()
    {
        if (_mostrandoAccion) return;

        var estado = GestorJuego.Instancia.Estado;
        string accion;

        if (estado.Enfermo)
            accion = "Enfermo";
        else if (estado.Bienestar < 35)
            accion = "Triste";
        else
            // "Feliz" ya no es un estado de reposo automático: con las
            // necesidades al 100% se ve Normal (parpadeando), tal como
            // corresponde al arrancar el juego.
            accion = _parpadeando ? "NormalParpadeo" : "Normal";

        // Recursos.Sprite ya sabe en qué etapa está el pingüino (Bebe,
        // Nino o Adolescente) y arma la ruta sola.
        ImagenMascota.Source = Recursos.Sprite(accion);
    }

    /// <summary>Tapa al pingüino con Cambio.png al arrancar un cambio de etapa.</summary>
    private void TaparPorTransicion()
    {
        ImagenTransicion.Source = Recursos.Transicion();
        ImagenTransicion.IsVisible = true;
    }

    /// <summary>
    /// Destapa al pingüino cuando terminó de cambiar de etapa (a Nino o
    /// Adolescente). Si en cambio se convirtió en Adulto, esta vista se va
    /// a desmontar sola porque MainWindow navega a FinalView, así que no
    /// hace falta manejar ese caso acá.
    /// </summary>
    private void DestaparPorTransicion()
    {
        ImagenTransicion.IsVisible = false;
        Refrescar();
    }
}
