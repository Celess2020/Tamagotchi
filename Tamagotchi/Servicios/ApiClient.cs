using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;

namespace Tamagotchi.Servicios;

/// Todo lo que el juego le pide a TamagotchiApi pasa por acá.
/// Igual que GestorJuego, hay una sola instancia para todo el juego.
///
/// Todos los métodos son async: mientras se espera la respuesta de la API
/// la ventana sigue respondiendo (no se "congela" el pingüino).
public class ApiClient
{
    public static ApiClient Instancia { get; } = new ApiClient();

    // Tiene que coincidir con applicationUrl de TamagotchiApi/Properties/launchSettings.json
    private const string DireccionApi = "http://localhost:5080/";

    private readonly HttpClient _httpClient;

    private ApiClient()
    {
        _httpClient = new HttpClient
        {
            BaseAddress = new Uri(DireccionApi),
            Timeout = TimeSpan.FromSeconds(10)
        };
    }

    // ---------------- Usuarios ----------------

    public async Task<List<UsuarioRespuesta>> GetUsuariosAsync()
    {
        using var response = await _httpClient.GetAsync("api/usuarios");
        await AsegurarExito(response);
        return await LeerJson<List<UsuarioRespuesta>>(response);
    }

    public async Task<UsuarioRespuesta> CrearUsuarioAsync(string nombre)
    {
        using var response = await _httpClient.PostAsJsonAsync("api/usuarios", new { Nombre = nombre });
        await AsegurarExito(response);
        return await LeerJson<UsuarioRespuesta>(response);
    }

    public async Task BorrarUsuarioAsync(int id)
    {
        using var response = await _httpClient.DeleteAsync($"api/usuarios/{id}");
        await AsegurarExito(response);
    }

    // ---------------- Pingüinos ----------------

    public async Task<List<PinguinoResumen>> GetPinguinosDeUsuarioAsync(int usuarioId)
    {
        using var response = await _httpClient.GetAsync($"api/usuarios/{usuarioId}/pinguinos");
        await AsegurarExito(response);
        return await LeerJson<List<PinguinoResumen>>(response);
    }

    public async Task<PinguinoDetalle> GetPinguinoAsync(int id)
    {
        using var response = await _httpClient.GetAsync($"api/pinguinos/{id}");
        await AsegurarExito(response);
        return await LeerJson<PinguinoDetalle>(response);
    }

    public async Task<PinguinoDetalle> CrearPinguinoAsync(int usuarioId, string nombre)
    {
        using var response = await _httpClient.PostAsJsonAsync("api/pinguinos",
            new { UsuarioId = usuarioId, Nombre = nombre });
        await AsegurarExito(response);
        return await LeerJson<PinguinoDetalle>(response);
    }

    public async Task GuardarPartidaAsync(int pinguinoId, GuardarPartidaPedido pedido)
    {
        using var response = await _httpClient.PutAsJsonAsync($"api/pinguinos/{pinguinoId}/partida", pedido);
        await AsegurarExito(response);
    }

    public async Task<PinguinoDetalle> CambiarRopaAsync(int pinguinoId, int? ropaId)
    {
        using var response = await _httpClient.PutAsJsonAsync($"api/pinguinos/{pinguinoId}/ropa",
            new { RopaId = ropaId });
        await AsegurarExito(response);
        return await LeerJson<PinguinoDetalle>(response);
    }

    public async Task BorrarPinguinoAsync(int id)
    {
        using var response = await _httpClient.DeleteAsync($"api/pinguinos/{id}");
        await AsegurarExito(response);
    }

    public async Task RegistrarAccionAsync(int pinguinoId, string accion)
    {
        using var response = await _httpClient.PostAsJsonAsync($"api/pinguinos/{pinguinoId}/acciones",
            new { Accion = accion });
        await AsegurarExito(response);
    }

    // ---------------- Ropa ----------------

    public async Task<List<RopaDto>> GetRopasAsync()
    {
        using var response = await _httpClient.GetAsync("api/ropas");
        await AsegurarExito(response);
        return await LeerJson<List<RopaDto>>(response);
    }

    // ---------------- Ayudantes ----------------

    /// Si la API devolvió un error (400, 404, 409...), lanza una
    /// ApiException con el mensaje que mandó la API, para mostrarlo tal cual.
    private static async Task AsegurarExito(HttpResponseMessage response)
    {
        if (response.IsSuccessStatusCode)
            return;

        string mensaje = $"La API respondió con un error ({(int)response.StatusCode}).";
        try
        {
            var error = await response.Content.ReadFromJsonAsync<ErrorApi>();
            if (!string.IsNullOrWhiteSpace(error?.Mensaje))
                mensaje = error.Mensaje;
            else if (error?.Errors != null && error.Errors.Count > 0)
                // De las validaciones automáticas, mostramos la primera
                mensaje = error.Errors.SelectMany(e => e.Value).FirstOrDefault() ?? mensaje;
        }
        catch (Exception)
        {
            // La respuesta de error no era JSON: nos quedamos con el mensaje genérico
        }

        throw new ApiException(mensaje, (int)response.StatusCode);
    }

    // Convertimos el JSON recibido en un objeto de C#.
    private static async Task<T> LeerJson<T>(HttpResponseMessage response)
    {
        var resultado = await response.Content.ReadFromJsonAsync<T>();
        if (resultado == null)
            throw new ApiException("La API devolvió una respuesta vacía.", (int)response.StatusCode);

        return resultado;
    }
}

/// Error "esperado" que devuelve la API (validación, no existe, repetido...)
public class ApiException(string mensaje, int codigo) : Exception(mensaje)
{
    public int Codigo { get; } = codigo;
}

/// Traduce cualquier error de la API o de la conexión a un texto para el
/// jugador, así ninguna pantalla se cierra por un error.
public static class ErroresApi
{
    public static string Mensaje(Exception exception) => exception switch
    {
        ApiException api => api.Message,
        TaskCanceledException => "La API tardó demasiado en responder. Probá de nuevo.",
        JsonException => "La API devolvió datos que no se entienden.",
        HttpRequestException => "No se pudo conectar con la API. ¿Está iniciado el proyecto TamagotchiApi?",
        _ => "Algo salió mal: " + exception.Message
    };
}
