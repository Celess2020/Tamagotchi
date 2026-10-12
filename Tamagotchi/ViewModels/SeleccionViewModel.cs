using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Tamagotchi.Juego;
using Tamagotchi.Servicios;

namespace Tamagotchi.ViewModels;

/// Pantalla que aparece después de "JUGAR": se elige (o crea) el usuario
/// y después se elige uno de sus pingüinos guardados (o se crea uno nuevo).
///
/// [ObservableProperty] crea sola la propiedad pública (por ejemplo
/// _nombreUsuarioNuevo -> NombreUsuarioNuevo) y avisa a la vista cada vez
/// que cambia. [RelayCommand] crea el comando para los botones (por
/// ejemplo CrearUsuarioAsync -> CrearUsuarioCommand).
///
/// alEmpezarAJugar: lo que hay que hacer cuando ya se cargó el pingüino
/// (navegar a la habitación). Se recibe desde afuera para que el ViewModel
/// no tenga que conocer a las vistas.
public partial class SeleccionViewModel(Action alEmpezarAJugar) : ViewModelBase
{
    // ---------------- Usuarios ----------------

    public ObservableCollection<UsuarioRespuesta> Usuarios { get; } = new();

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HayUsuarioSeleccionado))]
    [NotifyPropertyChangedFor(nameof(TituloPinguinos))]
    [NotifyCanExecuteChangedFor(nameof(BorrarUsuarioCommand))]
    [NotifyCanExecuteChangedFor(nameof(CrearPinguinoCommand))]
    private UsuarioRespuesta? _usuarioSeleccionado;

    [ObservableProperty]
    private string _nombreUsuarioNuevo = "";

    [ObservableProperty]
    private string _mensajeUsuarios = "";

    public bool HayUsuarioSeleccionado => UsuarioSeleccionado != null;

    // ---------------- Pingüinos ----------------

    public ObservableCollection<PinguinoResumen> Pinguinos { get; } = new();

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(ContinuarCommand))]
    [NotifyCanExecuteChangedFor(nameof(BorrarPinguinoCommand))]
    private PinguinoResumen? _pinguinoSeleccionado;

    [ObservableProperty]
    private string _nombrePinguinoNuevo = "";

    [ObservableProperty]
    private string _mensajePinguinos = "";

    public string TituloPinguinos => UsuarioSeleccionado == null
        ? "🐧 Elegí un usuario para ver sus pingüinos"
        : $"🐧 Pingüinos de {UsuarioSeleccionado.Nombre}";

    // Para los "¿seguro?" de los botones de borrar: el primer clic avisa,
    // el segundo borra de verdad
    private int? _usuarioABorrar;
    private int? _pinguinoABorrar;

    /// Se llama al cambiar de usuario en la lista (lo genera [ObservableProperty])
    partial void OnUsuarioSeleccionadoChanged(UsuarioRespuesta? value)
    {
        _usuarioABorrar = null;
        _ = CargarPinguinosAsync();
    }

    partial void OnPinguinoSeleccionadoChanged(PinguinoResumen? value)
    {
        _pinguinoABorrar = null;
    }

    // ---------------- Comandos de usuarios ----------------

    [RelayCommand]
    private async Task CargarUsuariosAsync()
    {
        MensajeUsuarios = "Cargando usuarios...";
        try
        {
            var usuarios = await ApiClient.Instancia.GetUsuariosAsync();

            Usuarios.Clear();
            foreach (var usuario in usuarios)
                Usuarios.Add(usuario);

            MensajeUsuarios = Usuarios.Count == 0
                ? "Todavía no hay usuarios. ¡Creá el primero!"
                : "Elegí quién va a jugar.";
        }
        catch (Exception ex)
        {
            MensajeUsuarios = "⚠ " + ErroresApi.Mensaje(ex);
        }
    }

    [RelayCommand]
    private async Task CrearUsuarioAsync()
    {
        string nombre = NombreUsuarioNuevo.Trim();

        // Validamos antes de molestar a la API
        if (nombre.Length < 2 || nombre.Length > 30)
        {
            MensajeUsuarios = "⚠ El nombre de usuario tiene que tener entre 2 y 30 letras.";
            return;
        }

        // Consulta LINQ sobre la lista que ya tenemos: si el nombre ya está,
        // ni siquiera hace falta preguntarle a la API
        if (Usuarios.Any(u => string.Equals(u.Nombre, nombre, StringComparison.OrdinalIgnoreCase)))
        {
            MensajeUsuarios = $"⚠ Ya existe un usuario llamado \"{nombre}\".";
            return;
        }

        try
        {
            var usuario = await ApiClient.Instancia.CrearUsuarioAsync(nombre);
            Usuarios.Add(usuario);
            UsuarioSeleccionado = usuario;
            NombreUsuarioNuevo = "";
            MensajeUsuarios = $"✔ ¡Hola, {usuario.Nombre}!";
        }
        catch (Exception ex)
        {
            MensajeUsuarios = "⚠ " + ErroresApi.Mensaje(ex);
        }
    }

    private bool PuedeBorrarUsuario() => UsuarioSeleccionado != null;

    [RelayCommand(CanExecute = nameof(PuedeBorrarUsuario))]
    private async Task BorrarUsuarioAsync()
    {
        var usuario = UsuarioSeleccionado;
        if (usuario == null)
            return;

        // Primer clic: solo avisa
        if (_usuarioABorrar != usuario.Id)
        {
            _usuarioABorrar = usuario.Id;
            MensajeUsuarios = $"¿Seguro? Se borra {usuario.Nombre} con TODOS sus pingüinos. Tocá Borrar otra vez para confirmar.";
            return;
        }

        try
        {
            await ApiClient.Instancia.BorrarUsuarioAsync(usuario.Id);
            Usuarios.Remove(usuario);
            UsuarioSeleccionado = null;
            MensajeUsuarios = $"🗑 Se borró {usuario.Nombre}.";
        }
        catch (Exception ex)
        {
            MensajeUsuarios = "⚠ " + ErroresApi.Mensaje(ex);
        }
        finally
        {
            _usuarioABorrar = null;
        }
    }

    // ---------------- Comandos de pingüinos ----------------

    private async Task CargarPinguinosAsync()
    {
        Pinguinos.Clear();
        PinguinoSeleccionado = null;

        var usuario = UsuarioSeleccionado;
        if (usuario == null)
        {
            MensajePinguinos = "";
            return;
        }

        MensajePinguinos = "Cargando pingüinos...";
        try
        {
            var pinguinos = await ApiClient.Instancia.GetPinguinosDeUsuarioAsync(usuario.Id);

            // Si mientras esperábamos se eligió otro usuario, esta respuesta ya no sirve
            if (UsuarioSeleccionado?.Id != usuario.Id)
                return;

            foreach (var pinguino in pinguinos)
                Pinguinos.Add(pinguino);

            MensajePinguinos = Pinguinos.Count == 0
                ? "No tiene pingüinos todavía. ¡Ponele nombre a uno nuevo!"
                : "Elegí un pingüino y tocá Continuar, o creá uno nuevo.";
        }
        catch (Exception ex)
        {
            MensajePinguinos = "⚠ " + ErroresApi.Mensaje(ex);
        }
    }

    private bool PuedeCrearPinguino() => UsuarioSeleccionado != null;

    [RelayCommand(CanExecute = nameof(PuedeCrearPinguino))]
    private async Task CrearPinguinoAsync()
    {
        var usuario = UsuarioSeleccionado;
        if (usuario == null)
            return;

        string nombre = NombrePinguinoNuevo.Trim();
        if (nombre.Length < 2 || nombre.Length > 20)
        {
            MensajePinguinos = "⚠ El nombre del pingüino tiene que tener entre 2 y 20 letras.";
            return;
        }

        MensajePinguinos = "Preparando el huevo... 🥚";
        try
        {
            var pinguino = await ApiClient.Instancia.CrearPinguinoAsync(usuario.Id, nombre);
            NombrePinguinoNuevo = "";
            Empezar(pinguino);
        }
        catch (Exception ex)
        {
            MensajePinguinos = "⚠ " + ErroresApi.Mensaje(ex);
        }
    }

    // Solo se puede continuar una partida que no terminó
    private bool PuedeContinuar() => PinguinoSeleccionado != null && !PinguinoSeleccionado.Terminado;

    [RelayCommand(CanExecute = nameof(PuedeContinuar))]
    private async Task ContinuarAsync()
    {
        var resumen = PinguinoSeleccionado;
        if (resumen == null)
            return;

        MensajePinguinos = $"Despertando a {resumen.Nombre}...";
        try
        {
            // La lista trae solo un resumen: pedimos la partida completa
            var pinguino = await ApiClient.Instancia.GetPinguinoAsync(resumen.Id);
            Empezar(pinguino);
        }
        catch (Exception ex)
        {
            MensajePinguinos = "⚠ " + ErroresApi.Mensaje(ex);
        }
    }

    private bool PuedeBorrarPinguino() => PinguinoSeleccionado != null;

    [RelayCommand(CanExecute = nameof(PuedeBorrarPinguino))]
    private async Task BorrarPinguinoAsync()
    {
        var pinguino = PinguinoSeleccionado;
        if (pinguino == null)
            return;

        if (_pinguinoABorrar != pinguino.Id)
        {
            _pinguinoABorrar = pinguino.Id;
            MensajePinguinos = $"¿Seguro que querés borrar a {pinguino.Nombre}? Tocá Borrar otra vez para confirmar.";
            return;
        }

        try
        {
            await ApiClient.Instancia.BorrarPinguinoAsync(pinguino.Id);
            Pinguinos.Remove(pinguino);
            PinguinoSeleccionado = null;
            MensajePinguinos = $"🗑 Se borró a {pinguino.Nombre}.";
        }
        catch (Exception ex)
        {
            MensajePinguinos = "⚠ " + ErroresApi.Mensaje(ex);
        }
        finally
        {
            _pinguinoABorrar = null;
        }
    }

    /// Le pasa la partida al GestorJuego (arranca el reloj) y navega
    private void Empezar(PinguinoDetalle pinguino)
    {
        GestorJuego.Instancia.IniciarJuego(pinguino);
        alEmpezarAJugar();
    }
}
