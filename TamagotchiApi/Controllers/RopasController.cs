using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TamagotchiApi.Dtos;
using TamagotchiApi.Models;

namespace TamagotchiApi.Controllers;

/// Catálogo de ropa para la rueda del ropero (solo lectura: la ropa se
/// carga desde AppDbContext, porque cada una necesita sus dibujos).
[ApiController]
[Route("api/ropas")]
public class RopasController(AppDbContext db) : ControllerBase
{
    // GET api/ropas -> toda la ropa, en el orden en que aparece en la rueda
    [HttpGet]
    public async Task<ActionResult<List<RopaDto>>> GetTodas()
    {
        var ropas = await db.Ropas
            .OrderBy(r => r.Orden)
            .Select(r => new RopaDto { Id = r.Id, Nombre = r.Nombre, Clave = r.Clave, Etapa = r.Etapa, Orden = r.Orden })
            .ToListAsync();

        return Ok(ropas);
    }

    // GET api/ropas/2
    [HttpGet("{id:int}")]
    public async Task<ActionResult<RopaDto>> GetPorId(int id)
    {
        var ropa = await db.Ropas.FindAsync(id);
        if (ropa == null)
            return NotFound(new { mensaje = "Esa ropa no existe." });

        return Ok(new RopaDto { Id = ropa.Id, Nombre = ropa.Nombre, Clave = ropa.Clave, Etapa = ropa.Etapa, Orden = ropa.Orden });
    }
}
