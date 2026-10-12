using Avalonia.Controls;
using Avalonia.Input;
using Tamagotchi.Juego;
using Tamagotchi.Navegacion;

namespace Tamagotchi.Vistas;

public partial class HabitacionView : UserControl
{
    public HabitacionView()
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

    private void Cama_PointerPressed(object? sender, PointerPressedEventArgs e)
    {
        GestorJuego.Instancia.Dormir();
        // Se posiciona sobre la cama para dormir.
        Mascota.MostrarAccion("Durmiendo", 3, left: -49, top: 76);
    }

    private void Juguetes_PointerPressed(object? sender, PointerPressedEventArgs e)
    {
        GestorJuego.Instancia.Jugar(25);
        // Se posiciona en el cajón de los juguetes.
        Mascota.MostrarAccion("Jugando", 3, left: 463, top: 100);
    }

    private void PC_PointerPressed(object? sender, PointerPressedEventArgs e)
    {
        GestorJuego.Instancia.Jugar(20);
        // Se posiciona delante de la PC para jugar en ella.
        Mascota.MostrarAccion("PC", 3, left: 935, top: 71);
    }

    private void Ropero_PointerPressed(object? sender, PointerPressedEventArgs e)
        => Navegador.IrARopero();

    private void Puerta_PointerPressed(object? sender, PointerPressedEventArgs e)
        => Navegador.Ir(new MapaView());
}
