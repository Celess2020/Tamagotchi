using Avalonia.Controls;
using Avalonia.Interactivity;
using Tamagotchi.Juego;
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

    private void JugarDeNuevo_Click(object? sender, RoutedEventArgs e)
    {
        GestorJuego.Instancia.IniciarJuego();
        Navegador.Ir(new HabitacionView());
    }
}
