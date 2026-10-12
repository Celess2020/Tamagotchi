using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Tamagotchi.Juego;
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

    // Se pone en true si el guardado falló: el segundo clic sale igual
    private bool _salirSinGuardar;

    private async void Menu_Click(object? sender, RoutedEventArgs e)
    {
        BotonMenu.IsEnabled = false;
        TextoMenu.IsVisible = true;
        TextoMenu.Text = "Guardando...";

        bool guardado = _salirSinGuardar || await GestorJuego.Instancia.GuardarAsync(esperarTurno: true);

        if (guardado)
        {
            GestorJuego.Instancia.DetenerJuego();
            Navegador.IrASeleccion();
            return;
        }

        // No se pudo guardar (la API no responde): avisamos y dejamos
        // que decida si sale igual, perdiendo lo último que no se guardó
        _salirSinGuardar = true;
        TextoMenu.Text = "⚠ No se pudo guardar. Tocá el botón de nuevo para salir igual (se pierde lo que no se guardó).";
        BotonMenu.IsEnabled = true;
    }
}
