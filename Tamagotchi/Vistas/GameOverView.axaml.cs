using Avalonia.Controls;
using Avalonia.Interactivity;
using Tamagotchi.Navegacion;

namespace Tamagotchi.Vistas;


/// Pantalla simple de Game Over. Se muestra sola cuando
/// GestorJuego dispara el evento JuegoTerminado.
public partial class GameOverView : UserControl
{
    public GameOverView()
    {
        InitializeComponent();
    }

    // Vuelve a la selección: ahí se puede crear un pingüino nuevo o
    // seguir con otro que haya quedado guardado
    private void JugarDeNuevo_Click(object? sender, RoutedEventArgs e)
        => Navegador.IrASeleccion();
}
