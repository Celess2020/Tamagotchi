using Avalonia.Controls;

namespace Tamagotchi.Navegacion;

/// Clase estática muy simple para poder navegar entre pantallas
/// desde cualquier lugar del código, sin tener que pasar referencias
/// de la ventana principal por todos lados.

public static class Navegador
{
    public static MainWindow? VentanaPrincipal { get; set; }

    public static void Ir(UserControl vista)
    {
        VentanaPrincipal?.MostrarVista(vista);
    }
}
