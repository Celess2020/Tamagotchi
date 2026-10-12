using Avalonia.Controls;
using Tamagotchi.Juego;

namespace Tamagotchi.Controles;

public partial class PanelEstadisticas : UserControl
{
    public PanelEstadisticas()
    {
        InitializeComponent();

        AttachedToVisualTree += (_, _) =>
        {
            GestorJuego.Instancia.CambioEstado += Refrescar;
            GestorJuego.Instancia.MensajeGuardado += MostrarGuardado;
            Refrescar();
        };

        DetachedFromVisualTree += (_, _) =>
        {
            GestorJuego.Instancia.CambioEstado -= Refrescar;
            GestorJuego.Instancia.MensajeGuardado -= MostrarGuardado;
        };
    }

    private void MostrarGuardado(string mensaje)
    {
        TextoGuardado.Text = mensaje;
        TextoGuardado.IsVisible = true;
    }

    private void Refrescar()
    {
        var gestor = GestorJuego.Instancia;
        var e = gestor.Estado;

        TextoNombre.Text = $"🐧 {gestor.NombrePinguino} · 👤 {gestor.NombreUsuario}";

        BarraHambre.Value = e.Hambre;
        BarraEnergia.Value = e.Energia;
        BarraHigiene.Value = e.Higiene;
        BarraDiversion.Value = e.Diversion;
        BarraVejiga.Value = e.Vejiga;

        if (e.Enfermo)
            TextoEstado.Text = "😷 ¡Está enfermo!";
        else if (e.Bienestar < 35)
            TextoEstado.Text = "😢 Triste";
        else
            TextoEstado.Text = "🙂 Normal";
    }
}
