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

        // Arrancamos siempre en la pantalla de inicio.
        Contenido.Content = new InicioView();
    }

    public void MostrarVista(UserControl vista)
    {
        Contenido.Content = vista;
    }
}
