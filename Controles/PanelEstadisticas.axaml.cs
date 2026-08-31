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
            Refrescar();
        };

        DetachedFromVisualTree += (_, _) =>
        {
            GestorJuego.Instancia.CambioEstado -= Refrescar;
        };
    }

    private void Refrescar()
    {
        var e = GestorJuego.Instancia.Estado;

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
