using Avalonia.Controls;
using Avalonia.Input;
using Tamagotchi.Navegacion;

namespace Tamagotchi.Vistas;

public partial class InicioView : UserControl
{
    public InicioView()
    {
        InitializeComponent();
    }

    // Antes de jugar hay que elegir usuario y pingüino
    private void Jugar_PointerPressed(object? sender, PointerPressedEventArgs e)
        => Navegador.IrASeleccion();
}
