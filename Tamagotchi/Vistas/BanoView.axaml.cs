using Avalonia.Controls;
using Avalonia.Input;
using Tamagotchi.Juego;
using Tamagotchi.Navegacion;

namespace Tamagotchi.Vistas;

public partial class BanoView : UserControl
{
    public BanoView()
    {
        InitializeComponent();
    }

    private void Inodoro_PointerPressed(object? sender, PointerPressedEventArgs e)
    {
        GestorJuego.Instancia.UsarInodoro();
        // Se posiciona sentado arriba del inodoro para ir al baño.
        Mascota.MostrarAccion("Bano", 3, left: -75, top: 121);
    }

    private void Ducha_PointerPressed(object? sender, PointerPressedEventArgs e)
    {
        GestorJuego.Instancia.Banarse();
        // Se posiciona en la ducha para bañarse.
        Mascota.MostrarAccion("Banandose", 3, left: 908, top: 60);
    }

    private void Puerta_PointerPressed(object? sender, PointerPressedEventArgs e)
        => Navegador.Ir(new MapaView());
}
