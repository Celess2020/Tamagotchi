using Avalonia.Controls;
using Tamagotchi.ViewModels;
using Tamagotchi.Vistas;

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

    /// Las pantallas con MVVM necesitan su ViewModel como DataContext:
    /// por eso tienen su propio método para ir a ellas.

    /// Pantalla de elegir usuario y pingüino. Apenas se muestra, pide los
    /// usuarios a la API (sin esperar: la pantalla aparece al toque y la
    /// lista se llena cuando llega la respuesta).
    public static void IrASeleccion()
    {
        var vm = new SeleccionViewModel(alEmpezarAJugar: () => Ir(new HabitacionView()));
        Ir(new SeleccionView { DataContext = vm });
        _ = vm.CargarUsuariosCommand.ExecuteAsync(null);
    }

    /// El ropero con la rueda de ropa. Al volver, regresa a la habitación.
    public static void IrARopero()
    {
        var vm = new RoperoViewModel(alVolver: () => Ir(new HabitacionView()));
        Ir(new RoperoView { DataContext = vm });
        _ = vm.CargarCommand.ExecuteAsync(null);
    }
}
