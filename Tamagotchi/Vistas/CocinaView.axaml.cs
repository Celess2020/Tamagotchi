using Avalonia.Controls;
using Avalonia.Input;
using Tamagotchi.Juego;
using Tamagotchi.Navegacion;

namespace Tamagotchi.Vistas;

public partial class CocinaView : UserControl
{
    public CocinaView()
    {
        InitializeComponent();

        ActualizarFondo();
        GestorJuego.Instancia.CambioDiaNoche += ActualizarFondo;
        DetachedFromVisualTree += (_, _) => GestorJuego.Instancia.CambioDiaNoche -= ActualizarFondo;
    }

    private void ActualizarFondo()
    {
        FondoDia.IsVisible = GestorJuego.Instancia.EsDeDia;
        FondoNoche.IsVisible = !GestorJuego.Instancia.EsDeDia;
    }

    private void Fondo_PointerPressed(object? sender, PointerPressedEventArgs e)
        => GestorJuego.Instancia.AlternarDiaNoche();

    private void Mesa_PointerPressed(object? sender, PointerPressedEventArgs e)
    {
        GestorJuego.Instancia.Comer();
        // Se posiciona frente a la mesa con el plato para comer.
        Mascota.MostrarAccion("Comiendo", 3, left: 393, top: 238);
    }

    private void Puerta_PointerPressed(object? sender, PointerPressedEventArgs e)
        => Navegador.Ir(new MapaView());
}
