using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Media.Imaging;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Tamagotchi.Juego;
using Tamagotchi.Servicios;

namespace Tamagotchi.ViewModels;

/// Una "porción" de la rueda del ropero
public class OpcionRopa
{
    /// null = la ropa original (la que viene dibujada en cada etapa)
    public RopaDto? Ropa { get; init; }

    public string Nombre { get; init; } = "";

    /// Cómo se ve el pingüino con esta ropa (sprite Normal)
    public Bitmap Vista { get; init; } = null!;

    /// false si la ropa es de otra etapa (sale con candado en la rueda)
    public bool Disponible { get; init; }

    /// Ej: "🔒 Para Niño". Vacío si está disponible.
    public string TextoBloqueo { get; init; } = "";
}

/// El ropero: una rueda que gira con todas las ropas. La que queda arriba
/// es la elegida. La parte de dibujar y animar la rueda está en
/// RoperoView.axaml.cs (es solo "cómo se ve"); acá está el "qué pasa".
public partial class RoperoViewModel(Action alVolver) : ViewModelBase
{
    public ObservableCollection<OpcionRopa> Opciones { get; } = new();

    /// La posición de la rueda que quedó arriba (la ropa elegida)
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(OpcionActual))]
    [NotifyPropertyChangedFor(nameof(TextoBoton))]
    [NotifyCanExecuteChangedFor(nameof(PonerseCommand))]
    private int _indiceSeleccionado;

    [ObservableProperty]
    private string _mensaje = "";

    /// Vueltas completas de más que tiene que dar la rueda en el próximo
    /// giro (para el giro al azar, que gira como una ruleta)
    public int VueltasExtra { get; set; }

    public OpcionRopa? OpcionActual =>
        IndiceSeleccionado >= 0 && IndiceSeleccionado < Opciones.Count ? Opciones[IndiceSeleccionado] : null;

    public string TextoBoton => EstaPuesta(OpcionActual) ? "✔ Ya la tiene puesta" : "👕 ¡Ponérmela!";

    private readonly Random _rng = new();

    private static string NombreEtapa(string etapa) => etapa switch
    {
        "Bebe" => "Bebé",
        "Nino" => "Niño",
        _ => etapa
    };

    private static bool EstaPuesta(OpcionRopa? opcion)
        => opcion != null && opcion.Ropa?.Id == GestorJuego.Instancia.RopaId;

    /// Arma la rueda con la ropa que viene de la API
    [RelayCommand]
    private async Task CargarAsync()
    {
        Mensaje = "Abriendo el ropero...";
        try
        {
            var ropas = await ApiClient.Instancia.GetRopasAsync();
            var etapa = GestorJuego.Instancia.Etapa;

            Opciones.Clear();

            // La primera porción siempre es la ropa de siempre
            Opciones.Add(new OpcionRopa
            {
                Ropa = null,
                Nombre = "Ropa original",
                Vista = Recursos.Sprite("Normal", etapa, null),
                Disponible = true
            });

            foreach (var ropa in ropas.OrderBy(r => r.Orden))
            {
                bool esDeEstaEtapa = ropa.Etapa == etapa.ToString();

                // Las que son de otra etapa se muestran con el dibujo de
                // SU etapa, para que se vea cómo va a quedar más adelante
                var etapaDibujo = Enum.TryParse(ropa.Etapa, out Etapa e) ? e : etapa;

                Opciones.Add(new OpcionRopa
                {
                    Ropa = ropa,
                    Nombre = ropa.Nombre,
                    Vista = Recursos.Sprite("Normal", etapaDibujo, ropa.Clave),
                    Disponible = esDeEstaEtapa,
                    TextoBloqueo = esDeEstaEtapa ? "" : $"🔒 Para {NombreEtapa(ropa.Etapa)}"
                });
            }

            // La rueda arranca mostrando la que tiene puesta ahora
            int puesta = Opciones.ToList().FindIndex(EstaPuesta);
            IndiceSeleccionado = puesta >= 0 ? puesta : 0;

            // Aunque el índice no haya cambiado, la vista tiene que dibujar la rueda
            OnPropertyChanged(nameof(Opciones));
            OnPropertyChanged(nameof(OpcionActual));
            OnPropertyChanged(nameof(TextoBoton));
            PonerseCommand.NotifyCanExecuteChanged();

            Mensaje = "Girá la rueda con las flechas o con la ruedita del mouse.";
        }
        catch (Exception ex)
        {
            Mensaje = "⚠ " + ErroresApi.Mensaje(ex);
        }
    }

    [RelayCommand]
    private void Anterior()
    {
        if (Opciones.Count == 0) return;
        // El + Count es para que de 0 pase a la última (y no a -1)
        IndiceSeleccionado = (IndiceSeleccionado - 1 + Opciones.Count) % Opciones.Count;
    }

    [RelayCommand]
    private void Siguiente()
    {
        if (Opciones.Count == 0) return;
        IndiceSeleccionado = (IndiceSeleccionado + 1) % Opciones.Count;
    }

    /// Gira como una ruleta y cae en cualquier ropa (puede caer en una con candado)
    [RelayCommand]
    private void GirarAlAzar()
    {
        if (Opciones.Count < 2) return;

        int nuevo;
        do
        {
            nuevo = _rng.Next(Opciones.Count);
        } while (nuevo == IndiceSeleccionado);

        VueltasExtra = 2;
        IndiceSeleccionado = nuevo;
    }

    /// La vista llama a esto cuando hacen clic directo en una porción
    public void Elegir(int indice)
    {
        if (indice >= 0 && indice < Opciones.Count)
            IndiceSeleccionado = indice;
    }

    private bool PuedePonerse() =>
        OpcionActual != null && OpcionActual.Disponible && !EstaPuesta(OpcionActual);

    [RelayCommand(CanExecute = nameof(PuedePonerse))]
    private async Task PonerseAsync()
    {
        var opcion = OpcionActual;
        if (opcion == null)
            return;

        Mensaje = "Cambiándose...";
        try
        {
            // Se guarda en la API y, si salió bien, cambia el dibujo
            await GestorJuego.Instancia.CambiarRopaAsync(opcion.Ropa);
            Mensaje = $"✔ ¡Listo! Ahora tiene: {opcion.Nombre}.";
            OnPropertyChanged(nameof(TextoBoton));
            PonerseCommand.NotifyCanExecuteChanged();
        }
        catch (Exception ex)
        {
            Mensaje = "⚠ " + ErroresApi.Mensaje(ex);
        }
    }

    [RelayCommand]
    private void Volver() => alVolver();
}
