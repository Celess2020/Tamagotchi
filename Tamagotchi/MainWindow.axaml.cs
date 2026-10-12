using System;
using System.Threading.Tasks;
using Avalonia.Controls;
using Tamagotchi.Juego;
using Tamagotchi.Navegacion;
using Tamagotchi.Vistas;

namespace Tamagotchi;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();

        // Le avisamos al Navegador cuál es la ventana principal para que
        // pueda cambiar de pantalla desde cualquier vista.
        Navegador.VentanaPrincipal = this;

        // Sin importar en qué cuarto esté el jugador, si el descuido llega
        // a Game Over, mostramos esa pantalla.
        GestorJuego.Instancia.JuegoTerminado += () => Navegador.Ir(new GameOverView());

        // Y si en cambio el pingüino crece hasta Adulto, mostramos la
        // pantalla final
        GestorJuego.Instancia.SeConvirtioEnAdulto += () => Navegador.Ir(new FinalView());

        // Al cerrar la ventana, si hay una partida en curso, la guardamos
        Closing += MainWindow_Closing;

        // Arrancamos siempre en la pantalla de inicio.
        Contenido.Content = new InicioView();
    }

    private bool _yaGuardoAlCerrar;

    /// Closing no puede esperar (await) antes de cerrarse, así que el truco
    /// es: cancelamos el cierre, guardamos, y después cerramos de verdad.
    /// Si la API no contesta en 3 segundos, se cierra igual.
    private async void MainWindow_Closing(object? sender, WindowClosingEventArgs e)
    {
        if (_yaGuardoAlCerrar || !GestorJuego.Instancia.HayPartidaEnCurso)
            return;

        e.Cancel = true;
        _yaGuardoAlCerrar = true;
        Title = "Guardando...";

        GestorJuego.Instancia.DetenerJuego();
        await Task.WhenAny(
            GestorJuego.Instancia.GuardarAsync(esperarTurno: true),
            Task.Delay(TimeSpan.FromSeconds(3)));

        Close();
    }

    public void MostrarVista(UserControl vista)
    {
        Contenido.Content = vista;
    }
}
