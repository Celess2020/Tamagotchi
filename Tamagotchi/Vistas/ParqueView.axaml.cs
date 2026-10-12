using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Tamagotchi.Juego;
using Tamagotchi.Navegacion;

namespace Tamagotchi.Vistas;

public partial class ParqueView : UserControl
{
    public ParqueView()
    {
        InitializeComponent();
    }

    private void Arena_PointerPressed(object? sender, PointerPressedEventArgs e)
    {
        GestorJuego.Instancia.Jugar(25);
        // Se posiciona dentro del arenero para jugar con la arena.
        Mascota.MostrarAccion("Arena", 3, left: 704, top: -8);
    }

    // Matas de flores de arriba a la derecha (el arbusto naranja).

    private void Flores1_PointerPressed(object? sender, PointerPressedEventArgs e)
    {
        GestorJuego.Instancia.Jugar(15);
        Mascota.MostrarAccion("Flores", 3, left: 729, top: -10);
    }

    // Matas de flores de abajo a la derecha (las violetas).
    private void Flores2_PointerPressed(object? sender, PointerPressedEventArgs e)
    {
        GestorJuego.Instancia.Jugar(15);
        Mascota.MostrarAccion("Flores", 3, left: 688, top: 301);
    }

    private void Mapa_Click(object? sender, RoutedEventArgs e)
        => Navegador.Ir(new MapaView());
}
