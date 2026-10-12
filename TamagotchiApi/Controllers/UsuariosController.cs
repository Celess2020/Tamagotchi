using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TamagotchiApi.Dtos;
using TamagotchiApi.Models;

namespace TamagotchiApi.Controllers;

/// CRUD completo de usuarios (las personas que juegan).
[ApiController]
[Route("api/usuarios")]
public class UsuariosController(AppDbContext db) : ControllerBase
{
    // GET api/usuarios -> todos los usuarios, con cuántos pingüinos tiene cada uno
    [HttpGet]
    public async Task<ActionResult<List<UsuarioResponse>>> GetTodos()
    {
        var usuarios = await db.Usuarios
            .OrderBy(u => u.Nombre)
            .Select(u => new UsuarioResponse
            {
                Id = u.Id,
                Nombre = u.Nombre,
                FechaCreacion = u.FechaCreacion,
                CantidadPinguinos = u.Pinguinos.Count()
            })
            .ToListAsync();

        return Ok(usuarios);
    }

    // GET api/usuarios/5
    [HttpGet("{id:int}")]
    public async Task<ActionResult<UsuarioResponse>> GetPorId(int id)
    {
        var usuario = await db.Usuarios
            .Where(u => u.Id == id)
            .Select(u => new UsuarioResponse
            {
                Id = u.Id,
                Nombre = u.Nombre,
                FechaCreacion = u.FechaCreacion,
                CantidadPinguinos = u.Pinguinos.Count()
            })
            .FirstOrDefaultAsync();

        if (usuario == null)
            return NotFound(new { mensaje = "El usuario no existe." });

        return Ok(usuario);
    }

    // POST api/usuarios -> crea un usuario nuevo (409 si el nombre ya está usado)
    [HttpPost]
    public async Task<ActionResult<UsuarioResponse>> Crear(UsuarioRequest request)
    {
        string nombre = request.Nombre.Trim();
        if (nombre.Length < 2)
            return BadRequest(new { mensaje = "El nombre de usuario debe tener entre 2 y 30 caracteres." });

        // La columna es NOCASE, así que "Celes" y "celes" se consideran iguales
        if (await db.Usuarios.AnyAsync(u => u.Nombre == nombre))
            return Conflict(new { mensaje = $"Ya existe un usuario llamado \"{nombre}\"." });

        var usuario = new Usuario
        {
            Nombre = nombre,
            FechaCreacion = DateTime.UtcNow
        };

        db.Usuarios.Add(usuario);
        await db.SaveChangesAsync();

        var respuesta = new UsuarioResponse
        {
            Id = usuario.Id,
            Nombre = usuario.Nombre,
            FechaCreacion = usuario.FechaCreacion,
            CantidadPinguinos = 0
        };

        return CreatedAtAction(nameof(GetPorId), new { id = usuario.Id }, respuesta);
    }

    // PUT api/usuarios/5 -> cambia el nombre del usuario
    [HttpPut("{id:int}")]
    public async Task<ActionResult<UsuarioResponse>> Renombrar(int id, UsuarioRequest request)
    {
        var usuario = await db.Usuarios.FindAsync(id);
        if (usuario == null)
            return NotFound(new { mensaje = "El usuario no existe." });

        string nombre = request.Nombre.Trim();
        if (nombre.Length < 2)
            return BadRequest(new { mensaje = "El nombre de usuario debe tener entre 2 y 30 caracteres." });

        if (await db.Usuarios.AnyAsync(u => u.Nombre == nombre && u.Id != id))
            return Conflict(new { mensaje = $"Ya existe un usuario llamado \"{nombre}\"." });

        usuario.Nombre = nombre;
        await db.SaveChangesAsync();

        return await GetPorId(id);
    }

    // DELETE api/usuarios/5 -> borra el usuario y, en cascada, todos sus pingüinos
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Borrar(int id)
    {
        var usuario = await db.Usuarios.FindAsync(id);
        if (usuario == null)
            return NotFound(new { mensaje = "El usuario no existe." });

        db.Usuarios.Remove(usuario);
        await db.SaveChangesAsync();

        return NoContent();
    }
}
