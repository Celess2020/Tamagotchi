using System;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Threading;
using Tamagotchi.Servicios;

namespace Tamagotchi.Juego;

/// Una única instancia para todo el juego que:
///   1) guarda el estado del pingüino,
///   2) tiene el "reloj" que hace que las necesidades bajen con el tiempo,
///   3) guarda si es de día o de noche,
///   4) avisa a las pantallas (vistas) cuando algo cambió, para que
///      se actualicen los dibujos y las barras de estado
///   5) sabe qué pingüino (y de qué usuario) se está jugando, y lo guarda
///      en la API cada tanto, para poder seguir otro día
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

    // Datos de la partida guardada que se está jugando (vienen de la API)
    public int PinguinoId { get; private set; }
    public string NombrePinguino { get; private set; } = "";
    public string NombreUsuario { get; private set; } = "";

    /// Ropa puesta. null = la ropa de siempre (la que viene dibujada en cada etapa).
    /// RopaClave es el prefijo de los sprites: Assets/Ropa/{Etapa}/{RopaClave}{Etapa}{Accion}.png
    public int? RopaId { get; private set; }
    public string? RopaClave { get; private set; }

    /// true mientras hay una partida corriendo (el reloj anda y hay algo que guardar)
    public bool HayPartidaEnCurso => _enJuego && !Terminado;

    /// Se dispara después de cada guardado (o si falló), con un texto para
    /// mostrar en el panel de estadísticas. Ej: "💾 Guardado 21:34"
    public event Action<string>? MensajeGuardado;

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

    // Guardado automático cada 8 ticks (8 * 4s = 32 segundos de juego)
    private const int TicksEntreGuardados = 8;
    private int _ticksDesdeGuardado;
    private bool _enJuego;

    // Para que no se pisen dos guardados a la vez (por ejemplo el automático
    // y el de Game Over). Es como un "turno": entra uno solo por vez.
    private readonly SemaphoreSlim _candadoGuardado = new(1, 1);

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

    /// Carga una partida guardada (o recién creada) y arranca el reloj del
    /// juego. Se llama desde la pantalla de selección al elegir un pingüino.
    public void IniciarJuego(PinguinoDetalle partida)
    {
        PinguinoId = partida.Id;
        NombrePinguino = partida.Nombre;
        NombreUsuario = partida.UsuarioNombre;
        RopaId = partida.RopaId;
        RopaClave = partida.RopaClave;

        // Las necesidades arrancan como quedaron la última vez
        Estado = new EstadoMascota
        {
            Hambre = partida.Estado.Hambre,
            Energia = partida.Estado.Energia,
            Higiene = partida.Estado.Higiene,
            Diversion = partida.Estado.Diversion,
            Vejiga = partida.Estado.Vejiga,
            Enfermo = partida.Estado.Enfermo
        };

        EsDeDia = partida.EsDeDia;
        Terminado = false;
        // La API guarda la etapa como texto ("Bebe", "Nino"...), igual que el enum
        Etapa = Enum.TryParse(partida.Etapa, out Etapa etapa) ? etapa : Etapa.Bebe;
        _ticksEnExtremo = 0;
        _ticksEtapaActual = partida.TicksEtapa;
        _ticksDesdeGuardado = 0;
        _enJuego = true;

        // Cancela cualquier reloj anterior (por si venía de una partida
        // previa) y arranca uno nuevo
        _cts?.Cancel();
        _cts = new CancellationTokenSource();
        _ = EjecutarReloj(_cts.Token);

        CambioEstado?.Invoke();
        CambioDiaNoche?.Invoke();
    }

    /// Frena el reloj sin terminar la partida (al volver al menú para
    /// elegir otro pingüino). Antes de llamarlo conviene guardar.
    public void DetenerJuego()
    {
        _cts?.Cancel();
        _enJuego = false;
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
                // Se guarda como terminada, así ya no aparece para continuar
                _ = GuardarAsync("GameOver");
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

        // Guardado automático cada tanto
        _ticksDesdeGuardado++;
        if (_ticksDesdeGuardado >= TicksEntreGuardados && !Terminado)
        {
            _ticksDesdeGuardado = 0;
            _ = GuardarAsync();
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

        // Cada ropa está dibujada para una sola etapa: al crecer ya no le
        // entra, así que vuelve a su ropa de siempre (en el ropero puede
        // elegir la de su etapa nueva)
        RopaId = null;
        RopaClave = null;

        if (Etapa == Etapa.Adulto)
        {
            // Acá NO se dispara CambioEstado. El pingüino en Adulto no tiene sprites propios 

            // La partida terminó bien: se frena el reloj (para que no pueda
            // caer en Game Over mientras mira la pantalla final) y se guarda
            Terminado = true;
            _cts?.Cancel();
            _ = GuardarAsync("Adulto");

            SeConvirtioEnAdulto?.Invoke();
        }
        else
        {
            // Se guarda enseguida, así la etapa nueva no se pierde
            _ = GuardarAsync();

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
    // (cada una además queda anotada en la API, para el historial)

    public void Comer()
    {
        Estado.Hambre = Math.Min(100, Estado.Hambre + 40);
        RegistrarAccion("Comer");
        CambioEstado?.Invoke();
    }

    public void Dormir()
    {
        Estado.Energia = Math.Min(100, Estado.Energia + 45);
        RegistrarAccion("Dormir");

        // Dormir cura la enfermedad.
        if (Estado.Enfermo)
            Estado.Enfermo = false;

        CambioEstado?.Invoke();
    }

    public void Banarse()
    {
        Estado.Higiene = Math.Min(100, Estado.Higiene + 40);
        RegistrarAccion("Banarse");

        // Bañarse también cura la enfermedad.
        if (Estado.Enfermo)
            Estado.Enfermo = false;

        CambioEstado?.Invoke();
    }

    public void UsarInodoro()
    {
        Estado.Vejiga = 0;
        Estado.Higiene = Math.Min(100, Estado.Higiene + 5);
        RegistrarAccion("Inodoro");
        CambioEstado?.Invoke();
    }

    public void Jugar(double cantidad = 25)
    {
        Estado.Diversion = Math.Min(100, Estado.Diversion + cantidad);
        RegistrarAccion("Jugar");
        CambioEstado?.Invoke();
    }

    // ---------------- Ropa ----------------

    /// Le cambia la ropa al pingüino (ropa null = la de siempre). Primero se
    /// guarda en la API y, si salió bien, recién ahí cambia el dibujo.
    /// Si la API da error, se lanza la excepción para que el ropero la muestre.
    public async Task CambiarRopaAsync(RopaDto? ropa)
    {
        var actualizado = await ApiClient.Instancia.CambiarRopaAsync(PinguinoId, ropa?.Id);

        RopaId = actualizado.RopaId;
        RopaClave = actualizado.RopaClave;
        CambioEstado?.Invoke();
    }

    // ---------------- Guardado ----------------

    /// Manda a la API cómo va la partida. resultado = null mientras se
    /// sigue jugando, "GameOver" o "Adulto" cuando termina.
    /// Devuelve true si se pudo guardar. Nunca lanza excepciones: si falla,
    /// avisa con MensajeGuardado y el juego sigue como si nada.
    /// esperarTurno = true cuando el guardado es importante (salir al menú,
    /// cerrar la ventana): si hay otro guardado en curso, lo espera.
    public async Task<bool> GuardarAsync(string? resultado = null, bool esperarTurno = false)
    {
        if (PinguinoId == 0)
            return false;

        // La "foto" se saca ANTES de esperar: así se guarda cómo estaba el
        // pingüino justo en este momento
        var pedido = new GuardarPartidaPedido
        {
            Etapa = Etapa.ToString(),
            TicksEtapa = _ticksEtapaActual,
            EsDeDia = EsDeDia,
            Resultado = resultado,
            Estado = new EstadoDto
            {
                Hambre = Estado.Hambre,
                Energia = Estado.Energia,
                Higiene = Estado.Higiene,
                Diversion = Estado.Diversion,
                Vejiga = Estado.Vejiga,
                Enfermo = Estado.Enfermo
            }
        };

        // Un guardado automático, si ya hay otro en curso, se saltea (en 32s
        // viene el próximo). Los importantes y los finales (Game Over /
        // Adulto) esperan su turno sí o sí.
        if (resultado == null && !esperarTurno)
        {
            if (!await _candadoGuardado.WaitAsync(0))
                return false;
        }
        else
        {
            await _candadoGuardado.WaitAsync();
        }

        try
        {
            await ApiClient.Instancia.GuardarPartidaAsync(PinguinoId, pedido);
            MensajeGuardado?.Invoke($"💾 Guardado {DateTime.Now:HH:mm}");
            return true;
        }
        catch (Exception ex)
        {
            MensajeGuardado?.Invoke("⚠ No se pudo guardar: " + ErroresApi.Mensaje(ex));
            return false;
        }
        finally
        {
            _candadoGuardado.Release();
        }
    }

    /// Anota la acción en el historial de la API sin esperar la respuesta
    /// (si la API no está, el juego sigue igual y solo se avisa en el panel)
    private void RegistrarAccion(string accion)
    {
        if (PinguinoId == 0)
            return;

        _ = RegistrarAccionAsync(accion);
    }

    private async Task RegistrarAccionAsync(string accion)
    {
        try
        {
            await ApiClient.Instancia.RegistrarAccionAsync(PinguinoId, accion);
        }
        catch (Exception ex)
        {
            MensajeGuardado?.Invoke("⚠ " + ErroresApi.Mensaje(ex));
        }
    }
}
