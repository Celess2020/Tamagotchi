using System;
using System.Collections.Generic;

namespace Tamagotchi.Servicios;

// Clases que representan los datos enviados y recibidos de la API.
// Son "espejo" de las clases de TamagotchiApi/Dtos: mismos nombres de
// propiedades, así el JSON se convierte solo.

public class UsuarioRespuesta
{
    public int Id { get; set; }
    public string Nombre { get; set; } = "";
    public DateTime FechaCreacion { get; set; }
    public int CantidadPinguinos { get; set; }

    // Lo que se ve en la lista de usuarios
    public override string ToString() => $"{Nombre}  ({CantidadPinguinos} 🐧)";
}

public class PinguinoResumen
{
    public int Id { get; set; }
    public string Nombre { get; set; } = "";
    public string Etapa { get; set; } = "";
    public string? Resultado { get; set; }
    public string? RopaNombre { get; set; }
    public DateTime UltimoGuardado { get; set; }
    public string? AccionFavorita { get; set; }

    /// Si ya llegó a Adulto o fue Game Over, no se puede seguir jugando
    public bool Terminado => Resultado != null;

    // Textos ya armados para mostrar en la lista (así el .axaml queda simple)
    public string TextoEtapa => Resultado switch
    {
        "Adulto" => "🏆 Llegó a adulto",
        "GameOver" => "💔 Game Over",
        _ => Etapa switch
        {
            "Bebe" => "🍼 Bebé",
            "Nino" => "🧒 Niño",
            "Adolescente" => "🎧 Adolescente",
            _ => Etapa
        }
    };

    public string TextoDetalle
    {
        get
        {
            // La API guarda la fecha en UTC: se pasa a la hora de la compu
            string fecha = DateTime.SpecifyKind(UltimoGuardado, DateTimeKind.Utc)
                .ToLocalTime().ToString("dd/MM HH:mm");
            string ropa = RopaNombre ?? "Ropa original";
            string favorita = AccionFavorita == null ? "" : $" · le encanta: {AccionFavorita}";
            return $"👕 {ropa} · 💾 {fecha}{favorita}";
        }
    }
}

public class EstadoDto
{
    public double Hambre { get; set; }
    public double Energia { get; set; }
    public double Higiene { get; set; }
    public double Diversion { get; set; }
    public double Vejiga { get; set; }
    public bool Enfermo { get; set; }
}

public class PinguinoDetalle
{
    public int Id { get; set; }
    public string Nombre { get; set; } = "";
    public int UsuarioId { get; set; }
    public string UsuarioNombre { get; set; } = "";
    public string Etapa { get; set; } = "Bebe";
    public int TicksEtapa { get; set; }
    public bool EsDeDia { get; set; } = true;
    public int? RopaId { get; set; }
    public string? RopaClave { get; set; }
    public string? RopaNombre { get; set; }
    public string? Resultado { get; set; }
    public EstadoDto Estado { get; set; } = new EstadoDto();
}

public class GuardarPartidaPedido
{
    public string Etapa { get; set; } = "Bebe";
    public int TicksEtapa { get; set; }
    public bool EsDeDia { get; set; }
    public EstadoDto Estado { get; set; } = new EstadoDto();
    public string? Resultado { get; set; }
}

public class RopaDto
{
    public int Id { get; set; }
    public string Nombre { get; set; } = "";
    public string Clave { get; set; } = "";
    public string Etapa { get; set; } = "";
    public int Orden { get; set; }
}

/// Respuesta de error de la API: o viene { "mensaje": "..." } (errores que
/// armamos nosotros en los Controllers) o viene { "errors": {...} } (las
/// validaciones automáticas de [ApiController])
public class ErrorApi
{
    public string? Mensaje { get; set; }
    public Dictionary<string, string[]>? Errors { get; set; }
}
