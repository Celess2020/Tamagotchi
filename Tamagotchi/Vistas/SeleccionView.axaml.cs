using Avalonia.Controls;
using Avalonia.Interactivity;
using Tamagotchi.Navegacion;

namespace Tamagotchi.Vistas;

/// Vista de la pantalla de selección. Toda la lógica está en
/// SeleccionViewModel (su DataContext); acá solo queda la navegación de
/// "Volver", que es puramente de pantallas.
public partial class SeleccionView : UserControl
{
    public SeleccionView()
    {
        InitializeComponent();
    }

    private void Volver_Click(object? sender, RoutedEventArgs e)
        => Navegador.Ir(new InicioView());
}
