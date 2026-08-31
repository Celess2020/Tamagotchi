using Avalonia.Controls;
using Avalonia.Input;
using Tamagotchi.Navegacion;

namespace Tamagotchi.Vistas;

public partial class MapaView : UserControl
{
    public MapaView()
    {
        InitializeComponent();
    }

    private void Habitacion_PointerPressed(object? sender, PointerPressedEventArgs e)
        => Navegador.Ir(new HabitacionView());

    private void Cocina_PointerPressed(object? sender, PointerPressedEventArgs e)
        => Navegador.Ir(new CocinaView());

    private void Parque_PointerPressed(object? sender, PointerPressedEventArgs e)
        => Navegador.Ir(new ParqueView());

    private void Bano_PointerPressed(object? sender, PointerPressedEventArgs e)
        => Navegador.Ir(new BanoView());
}
