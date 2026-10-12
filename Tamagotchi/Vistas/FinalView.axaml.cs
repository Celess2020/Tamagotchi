using Avalonia.Controls;
using Avalonia.Interactivity;
using Tamagotchi.Navegacion;

namespace Tamagotchi.Vistas;


/// Pantalla de "final feliz": se muestra sola cuando el pingüino
/// llegó a Adulto después de haber pasado por Bebe, Nino y Adolescente.
/// Es la contraparte de GameOverView, pero de final bueno.

public partial class FinalView : UserControl
{
    public FinalView()
    {
        InitializeComponent();
    }

    // Vuelve a la selección: ahí se puede crear un pingüino nuevo o
    // seguir con otro que haya quedado guardado
    private void JugarDeNuevo_Click(object? sender, RoutedEventArgs e)
        => Navegador.IrASeleccion();
}
