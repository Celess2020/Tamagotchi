using Avalonia.Controls;
using Avalonia.Input;
using Tamagotchi.Juego;
using Tamagotchi.Navegacion;

namespace Tamagotchi.Vistas;

public partial class InicioView : UserControl
{
    public InicioView()
    {
        InitializeComponent();
    }

    private void Jugar_PointerPressed(object? sender, PointerPressedEventArgs e)
    {
        GestorJuego.Instancia.IniciarJuego();
        Navegador.Ir(new HabitacionView());
    }
}
