using System;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Threading;

namespace Tamagotchi.Juego;

/// Una única instancia para todo el juego que:
///   1) guarda el estado del pingüino,
///   2) tiene el "reloj" que hace que las necesidades bajen con el tiempo,
///   3) guarda si es de día o de noche,
///   4) avisa a las pantallas (vistas) cuando algo cambió, para que
///      se actualicen los dibujos y las barras de estado
///
/// Al ser un singleton, el estado del pingüino se mantiene igual aunque
/// naveguemos de una habitación a otra
public class GestorJuego
{
    public static GestorJuego Instancia { get; } = new GestorJuego();

    public EstadoMascota Estado { get; private set; } = new EstadoMascota();
    public bool EsDeDia { get; private set; } = true;

    ///Etapa de vida actual del pingüino (Bebe, Nino, Adolescente, Adulto)
    public Etapa Etapa { get; private set; } = Etapa.Bebe;

    ///Se dispara cada vez que cambian las estadísticas del pingüino
    public event Action? CambioEstado;

    ///Se dispara cuando se alterna entre día y noche
    public event Action? CambioDiaNoche;


    /// Se dispara justo antes de que el pingüino cambie de etapa: es la
    /// señal para que la vista lo tape por completo con Cambio.png
    public event Action? TransicionEtapaIniciada;

    /// Se dispara cuando terminó la transición hacia Nino o Adolescente
    /// (NO hacia Adulto, que en cambio dispara SeConvirtioEnAdulto): es la
    /// señal para destapar al pingüino, que ya está en su nueva etapa

    public event Action? TransicionEtapaFinalizada;

    /// Se dispara una sola vez, cuando el pingüino llega a Adulto. Todavía
    /// sigue tapado por Cambio.png en ese momento: quien escuche este
    /// evento debe navegar a la pantalla final.
    public event Action? SeConvirtioEnAdulto;

    // Cada cuántos segundos "pasa el tiempo" para el pingüino
    private static readonly TimeSpan IntervaloTick = TimeSpan.FromSeconds(4);

    // Cuánto dura cada etapa de vida (Bebe, Nino, Adolescente) antes de
    // pasar a la siguiente. 2 minutos de juego
    private static readonly TimeSpan DuracionEtapa = TimeSpan.FromMinutes(2);

    // Cuántos ticks equivalen a esos  minutos
    private static readonly int TicksParaCambioEtapa =
        (int)(DuracionEtapa.TotalSeconds / IntervaloTick.TotalSeconds);

    // Cuánto se queda tapado el pingüino con Cambio.png mientras cambia
    // de etapa "por dentro" (para que no se vea la transición en sí)
    private static readonly TimeSpan DuracionTransicion = TimeSpan.FromSeconds(1.6);

    private int _ticksEtapaActual;

    // Probabilidad de enfermarse en un tick de riesgo (0.08 = 8%).
    private const double ProbabilidadEnfermarse = 0.08;

    // Cuántos ticks seguidos con las 4 necesidades principales en 0 hacen
    // falta para que sea Game Over. 45 ticks * 4s = 180s (3 min) sostenidos
    // en ese extremo. Sumado a lo que tardan en llegar a 0 desde el
    // arranque (~100s), da un abandono total de varios minutos: no se
    // dispara por dejar la ventana abierta un rato sin mirarla.
    private const int TicksParaGameOver = 45;

    private readonly Random _rng = new();
    private int _ticksEnExtremo;

    // El "reloj" del juego. Se usa Task.Delay (depende del reloj del
    // sistema operativo) Decidi usar ese en vez de DispatcherTimer de Avalonia, porque
    // no me respetó bien el Interval.
    private CancellationTokenSource? _cts;

    ///Se dispara una sola vez, cuando el descuido llega a Game Over.
    public event Action? JuegoTerminado;

    public bool Terminado { get; private set; }

    private GestorJuego()
    {
    }

    /// Reinicia todo y arranca el reloj del juego. Se llama al apretar "Jugar"
    public void IniciarJuego()
    {
        Estado = new EstadoMascota();
        EsDeDia = true;
        Terminado = false;
        Etapa = Etapa.Bebe;
        _ticksEnExtremo = 0;
        _ticksEtapaActual = 0;

        // Cancela cualquier reloj anterior (por si venía de una partida
        // previa) y arranca uno nuevo
        _cts?.Cancel();
        _cts = new CancellationTokenSource();
        _ = EjecutarReloj(_cts.Token);

        CambioEstado?.Invoke();
        CambioDiaNoche?.Invoke();
    }

    /// Bucle async: espera IntervaloTick de verdad (reloj del sistema
    /// operativo vía Task.Delay) y recién ahí llama a Tick(), una y otra
    /// vez, hasta que se cancele el token (Game Over o nueva partida).
  
    private async Task EjecutarReloj(CancellationToken token)
    {
        try
        {
            while (!token.IsCancellationRequested)
            {
                await Task.Delay(IntervaloTick, token);

                if (token.IsCancellationRequested)
                    break;

                // Tick() toca propiedades leídas por la interfaz, así que
                // se ejecuta en el hilo de UI de Avalonia
                await Dispatcher.UIThread.InvokeAsync(Tick);
            }
        }
        catch (TaskCanceledException)
        {
            // Cancelación esperada al reiniciar o terminar la partida
        }
    }

    private void Tick()
    {
        Estado.Hambre = Math.Max(0, Estado.Hambre - 3);
        Estado.Energia = Math.Max(0, Estado.Energia - 2.5);
        Estado.Higiene = Math.Max(0, Estado.Higiene - 2);
        Estado.Diversion = Math.Max(0, Estado.Diversion - 3);
        Estado.Vejiga = Math.Min(100, Estado.Vejiga + 4);

        if (!Estado.Enfermo)
        {
         
            // Solo aparece si tanto la higiene como la energía están muy mal
            // Así, la mayor parte de la partida el pingüino se mantiene en Normal
            bool enRiesgo = Estado.Higiene < 15 && Estado.Energia < 15;
            if (enRiesgo && _rng.NextDouble() < ProbabilidadEnfermarse)
                Estado.Enfermo = true;
        }
        // La cura no depende del paso del tiempo: se cura directamente
        // cuando lo dormís o lo bañás (ver Dormir() y Banarse() más abajo).

        //Game Over: las 4 necesidades principales llegaron a 0 A LA
        // VEZ y se mantienen así varios ticks seguidos.

        bool enExtremo = Estado.Hambre <= 0 && Estado.Energia <= 0 &&
                          Estado.Higiene <= 0 && Estado.Diversion <= 0;

        if (enExtremo)
        {
            _ticksEnExtremo++;
            if (_ticksEnExtremo >= TicksParaGameOver && !Terminado)
            {
                Terminado = true;
                _cts?.Cancel();
                JuegoTerminado?.Invoke();
            }
        }
        else
        {
            _ticksEnExtremo = 0;
        }

        // Crecimiento: cada TicksParaCambioEtapa ticks, pasa a la
        // siguiente etapa de vida (Bebe -> Nino -> Adolescente -> Adulto).
        // Una vez Adulto, ya no hay más etapas: se deja de contar.
        if (Etapa != Etapa.Adulto && !Terminado)
        {
            _ticksEtapaActual++;
            if (_ticksEtapaActual >= TicksParaCambioEtapa)
            {
                _ticksEtapaActual = 0;
                _ = CambiarDeEtapa();
            }
        }

        CambioEstado?.Invoke();
    }

  
    /// Tapa al pingüino con Cambio.png, espera a que la transición "se
    /// coma" el cambio de sprite, y recién ahí lo pasa a la siguiente
    /// etapa. Si la etapa nueva es Adulto, en vez de destapar al pingüino
    /// se avisa con SeConvirtioEnAdulto para que se navegue a FinalView.
    
    private async Task CambiarDeEtapa()
    {
        TransicionEtapaIniciada?.Invoke();

        await Task.Delay(DuracionTransicion);

        Etapa = Etapa switch
        {
            Etapa.Bebe => Etapa.Nino,
            Etapa.Nino => Etapa.Adolescente,
            Etapa.Adolescente => Etapa.Adulto,
            _ => Etapa.Adulto
        };

        if (Etapa == Etapa.Adulto)
        {
            // Acá NO se dispara CambioEstado. El pingüino en Adulto no tiene sprites propios 

            SeConvirtioEnAdulto?.Invoke();
        }
        else
        {
            // Refresca el sprite mientras todavía está tapado por
            // Cambio.png, así cuando se destape ya se ve la nueva etapa
            // correcta, sin ningún parpadeo con el sprite de la vieja.
            CambioEstado?.Invoke();
            TransicionEtapaFinalizada?.Invoke();
        }
    }

    public void AlternarDiaNoche()
    {
        EsDeDia = !EsDeDia;
        CambioDiaNoche?.Invoke();
    }

    // Aca van las acciones que puede hacer el jugador

    public void Comer()
    {
        Estado.Hambre = Math.Min(100, Estado.Hambre + 40);
        CambioEstado?.Invoke();
    }

    public void Dormir()
    {
        Estado.Energia = Math.Min(100, Estado.Energia + 45);

        // Dormir cura la enfermedad.
        if (Estado.Enfermo)
            Estado.Enfermo = false;

        CambioEstado?.Invoke();
    }

    public void Banarse()
    {
        Estado.Higiene = Math.Min(100, Estado.Higiene + 40);

        // Bañarse también cura la enfermedad.
        if (Estado.Enfermo)
            Estado.Enfermo = false;

        CambioEstado?.Invoke();
    }

    public void UsarInodoro()
    {
        Estado.Vejiga = 0;
        Estado.Higiene = Math.Min(100, Estado.Higiene + 5);
        CambioEstado?.Invoke();
    }

    public void Jugar(double cantidad = 25)
    {
        Estado.Diversion = Math.Min(100, Estado.Diversion + cantidad);
        CambioEstado?.Invoke();
    }
}
