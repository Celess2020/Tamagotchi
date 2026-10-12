using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TamagotchiApi.Dtos;
using TamagotchiApi.Models;

namespace TamagotchiApi.Controllers;

/// Las partidas guardadas: crear un pingüino, cargarlo, guardar cómo va,
/// cambiarle la ropa, borrarlo y registrar lo que hace.
[ApiController]
[Route("api/pinguinos")]
public class PinguinosController(AppDbContext db) : ControllerBase
{
    // GET api/usuarios/3/pinguinos -> los pingüinos guardados de un usuario
    // (el "~/" hace que la ruta no arranque con api/pinguinos)
    [HttpGet("~/api/usuarios/{usuarioId:int}/pinguinos")]
    public async Task<ActionResult<List<PinguinoResumen>>> GetDeUsuario(int usuarioId)
    {
        if (!await db.Usuarios.AnyAsync(u => u.Id == usuarioId))
            return NotFound(new { mensaje = "El usuario no existe." });

        // Primero los pingüinos, el último que se jugó arriba de todo
        var pinguinos = await db.Pinguinos
            .Where(p => p.UsuarioId == usuarioId)
            .OrderByDescending(p => p.UltimoGuardado)
            .Select(p => new PinguinoResumen
            {
                Id = p.Id,
                Nombre = p.Nombre,
                Etapa = p.Etapa,
                Resultado = p.Resultado,
                RopaNombre = p.RopaPuesta != null ? p.RopaPuesta.Nombre : null,
                UltimoGuardado = p.UltimoGuardado
            })
            .ToListAsync();

        // Consulta LINQ: cuántas veces hizo cada acción cada pingüino
        // (agrupando por pingüino y acción). La de más veces es su favorita.
        var ids = pinguinos.Select(p => p.Id).ToList();

        var conteos = await db.AccionesRegistradas
            .Where(a => ids.Contains(a.PinguinoId))
            .GroupBy(a => new { a.PinguinoId, a.Accion })
            .Select(g => new { g.Key.PinguinoId, g.Key.Accion, Cantidad = g.Count() })
            .ToListAsync();

        foreach (var pinguino in pinguinos)
        {
            pinguino.AccionFavorita = conteos
                .Where(c => c.PinguinoId == pinguino.Id)
                .OrderByDescending(c => c.Cantidad)
                .Select(c => c.Accion)
                .FirstOrDefault();
        }

        return Ok(pinguinos);
    }

    // GET api/pinguinos/7 -> todo lo necesario para seguir jugando
    [HttpGet("{id:int}")]
    public async Task<ActionResult<PinguinoDetalle>> GetPorId(int id)
    {
        var pinguino = await BuscarCompleto(id);
        if (pinguino == null)
            return NotFound(new { mensaje = "Ese pingüino no existe (¿lo borraron?)." });

        return Ok(MapearDetalle(pinguino));
    }

    // POST api/pinguinos -> pingüino nuevo, arranca como Bebe
    [HttpPost]
    public async Task<ActionResult<PinguinoDetalle>> Crear(CrearPinguinoRequest request)
    {
        var usuario = await db.Usuarios.FindAsync(request.UsuarioId);
        if (usuario == null)
            return NotFound(new { mensaje = "El usuario no existe." });

        string nombre = request.Nombre.Trim();
        if (nombre.Length < 2)
            return BadRequest(new { mensaje = "El nombre del pingüino debe tener entre 2 y 20 caracteres." });

        var ahora = DateTime.UtcNow;
        var pinguino = new Pinguino
        {
            Nombre = nombre,
            UsuarioId = usuario.Id,
            Etapa = "Bebe",
            TicksEtapa = 0,
            EsDeDia = true,
            FechaCreacion = ahora,
            UltimoGuardado = ahora,
            // Las necesidades arrancan con los mismos valores que EstadoMascota
            Estado = new EstadoPinguino()
        };

        db.Pinguinos.Add(pinguino);
        await db.SaveChangesAsync();

        pinguino.Usuario = usuario;
        return CreatedAtAction(nameof(GetPorId), new { id = pinguino.Id }, MapearDetalle(pinguino));
    }

    // PUT api/pinguinos/7/partida -> guarda cómo va la partida
    [HttpPut("{id:int}/partida")]
    public async Task<IActionResult> GuardarPartida(int id, GuardarPartidaRequest request)
    {
        var pinguino = await BuscarCompleto(id);
        if (pinguino == null)
            return NotFound(new { mensaje = "Ese pingüino no existe (¿lo borraron?)." });

        if (pinguino.Resultado != null)
            return Conflict(new { mensaje = "Esa partida ya terminó, no se puede seguir guardando." });

        pinguino.Etapa = request.Etapa;
        pinguino.TicksEtapa = request.TicksEtapa;
        pinguino.EsDeDia = request.EsDeDia;
        pinguino.Resultado = request.Resultado;
        pinguino.UltimoGuardado = DateTime.UtcNow;

        // Si creció, la ropa de la etapa anterior ya no le entra:
        // vuelve a su ropa de siempre
        if (pinguino.RopaPuesta != null && pinguino.RopaPuesta.Etapa != pinguino.Etapa)
            pinguino.RopaPuestaId = null;

        pinguino.Estado ??= new EstadoPinguino();
        pinguino.Estado.Hambre = request.Estado.Hambre;
        pinguino.Estado.Energia = request.Estado.Energia;
        pinguino.Estado.Higiene = request.Estado.Higiene;
        pinguino.Estado.Diversion = request.Estado.Diversion;
        pinguino.Estado.Vejiga = request.Estado.Vejiga;
        pinguino.Estado.Enfermo = request.Estado.Enfermo;

        await db.SaveChangesAsync();
        return NoContent();
    }

    // PUT api/pinguinos/7/ropa -> le pone otra ropa (RopaId null = la de siempre)
    [HttpPut("{id:int}/ropa")]
    public async Task<ActionResult<PinguinoDetalle>> CambiarRopa(int id, CambiarRopaRequest request)
    {
        var pinguino = await BuscarCompleto(id);
        if (pinguino == null)
            return NotFound(new { mensaje = "Ese pingüino no existe (¿lo borraron?)." });

        if (request.RopaId == null)
        {
            pinguino.RopaPuestaId = null;
            pinguino.RopaPuesta = null;
        }
        else
        {
            var ropa = await db.Ropas.FindAsync(request.RopaId.Value);
            if (ropa == null)
                return NotFound(new { mensaje = "Esa ropa no existe." });

            // Cada ropa está dibujada para una sola etapa
            if (ropa.Etapa != pinguino.Etapa)
                return BadRequest(new { mensaje = $"\"{ropa.Nombre}\" no es para la etapa {pinguino.Etapa}." });

            pinguino.RopaPuestaId = ropa.Id;
            pinguino.RopaPuesta = ropa;
        }

        pinguino.UltimoGuardado = DateTime.UtcNow;
        await db.SaveChangesAsync();

        return Ok(MapearDetalle(pinguino));
    }

    // DELETE api/pinguinos/7 -> borra la partida (y su estado e historial, en cascada)
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Borrar(int id)
    {
        var pinguino = await db.Pinguinos.FindAsync(id);
        if (pinguino == null)
            return NotFound(new { mensaje = "Ese pingüino no existe." });

        db.Pinguinos.Remove(pinguino);
        await db.SaveChangesAsync();

        return NoContent();
    }

    // POST api/pinguinos/7/acciones -> anota que hizo algo (Comer, Dormir...)
    [HttpPost("{id:int}/acciones")]
    public async Task<IActionResult> RegistrarAccion(int id, RegistrarAccionRequest request)
    {
        if (!await db.Pinguinos.AnyAsync(p => p.Id == id))
            return NotFound(new { mensaje = "Ese pingüino no existe." });

        var accion = new AccionRegistrada
        {
            PinguinoId = id,
            Accion = request.Accion,
            Fecha = DateTime.UtcNow
        };

        db.AccionesRegistradas.Add(accion);
        await db.SaveChangesAsync();

        return StatusCode(StatusCodes.Status201Created, new { accion.Id, accion.Accion, accion.Fecha });
    }

    private Task<Pinguino?> BuscarCompleto(int id)
        => db.Pinguinos
            .Include(p => p.Usuario)
            .Include(p => p.Estado)
            .Include(p => p.RopaPuesta)
            .FirstOrDefaultAsync(p => p.Id == id);

    private static PinguinoDetalle MapearDetalle(Pinguino p) => new PinguinoDetalle
    {
        Id = p.Id,
        Nombre = p.Nombre,
        UsuarioId = p.UsuarioId,
        UsuarioNombre = p.Usuario?.Nombre ?? "",
        Etapa = p.Etapa,
        TicksEtapa = p.TicksEtapa,
        EsDeDia = p.EsDeDia,
        RopaId = p.RopaPuestaId,
        RopaClave = p.RopaPuesta?.Clave,
        RopaNombre = p.RopaPuesta?.Nombre,
        Resultado = p.Resultado,
        Estado = p.Estado == null
            ? new EstadoDto()
            : new EstadoDto
            {
                Hambre = p.Estado.Hambre,
                Energia = p.Estado.Energia,
                Higiene = p.Estado.Higiene,
                Diversion = p.Estado.Diversion,
                Vejiga = p.Estado.Vejiga,
                Enfermo = p.Estado.Enfermo
            }
    };
}
