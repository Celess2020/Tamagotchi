using Microsoft.EntityFrameworkCore;

namespace TamagotchiApi.Models;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public DbSet<Usuario> Usuarios { get; set; }

    public DbSet<Pinguino> Pinguinos { get; set; }

    public DbSet<EstadoPinguino> EstadosPinguino { get; set; }

    public DbSet<Ropa> Ropas { get; set; }

    public DbSet<AccionRegistrada> AccionesRegistradas { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Usuario>(entity =>
        {
            entity.ToTable("usuario");

            entity.Property(e => e.Id).HasColumnName("id");
            // NOCASE: "Celes" y "celes" cuentan como el mismo usuario
            entity.Property(e => e.Nombre)
                .HasColumnName("nombre")
                .HasMaxLength(30)
                .UseCollation("NOCASE")
                .IsRequired();
            entity.Property(e => e.FechaCreacion).HasColumnName("fecha_creacion");

            // No puede haber dos usuarios con el mismo nombre
            entity.HasIndex(e => e.Nombre).IsUnique();
        });

        modelBuilder.Entity<Pinguino>(entity =>
        {
            entity.ToTable("pinguino");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Nombre).HasColumnName("nombre").HasMaxLength(20).IsRequired();
            entity.Property(e => e.UsuarioId).HasColumnName("usuario_id");
            entity.Property(e => e.Etapa).HasColumnName("etapa").HasMaxLength(20).IsRequired();
            entity.Property(e => e.TicksEtapa).HasColumnName("ticks_etapa");
            entity.Property(e => e.EsDeDia).HasColumnName("es_de_dia");
            entity.Property(e => e.RopaPuestaId).HasColumnName("ropa_puesta_id");
            entity.Property(e => e.Resultado).HasColumnName("resultado").HasMaxLength(20);
            entity.Property(e => e.FechaCreacion).HasColumnName("fecha_creacion");
            entity.Property(e => e.UltimoGuardado).HasColumnName("ultimo_guardado");

            // Si se borra el usuario, se borran también sus pingüinos
            entity.HasOne(p => p.Usuario)
                .WithMany(u => u.Pinguinos)
                .HasForeignKey(p => p.UsuarioId)
                .OnDelete(DeleteBehavior.Cascade);

            // Si se borra una ropa del catálogo, el pingüino vuelve a su ropa de siempre
            entity.HasOne(p => p.RopaPuesta)
                .WithMany(r => r.Pinguinos)
                .HasForeignKey(p => p.RopaPuestaId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<EstadoPinguino>(entity =>
        {
            entity.ToTable("estado_pinguino");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.PinguinoId).HasColumnName("pinguino_id");
            entity.Property(e => e.Hambre).HasColumnName("hambre");
            entity.Property(e => e.Energia).HasColumnName("energia");
            entity.Property(e => e.Higiene).HasColumnName("higiene");
            entity.Property(e => e.Diversion).HasColumnName("diversion");
            entity.Property(e => e.Vejiga).HasColumnName("vejiga");
            entity.Property(e => e.Enfermo).HasColumnName("enfermo");

            // 1 a 1: cada pingüino tiene un único estado
            entity.HasIndex(e => e.PinguinoId).IsUnique();
            entity.HasOne(e => e.Pinguino)
                .WithOne(p => p.Estado)
                .HasForeignKey<EstadoPinguino>(e => e.PinguinoId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Ropa>(entity =>
        {
            entity.ToTable("ropa");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Nombre).HasColumnName("nombre").HasMaxLength(30).IsRequired();
            entity.Property(e => e.Clave).HasColumnName("clave").HasMaxLength(30).IsRequired();
            entity.Property(e => e.Etapa).HasColumnName("etapa").HasMaxLength(20).IsRequired();
            entity.Property(e => e.Orden).HasColumnName("orden");

            entity.HasIndex(e => e.Clave).IsUnique();

            // Ropa que viene cargada desde el principio en la base.
            // La Clave tiene que coincidir con el prefijo de los sprites en
            // el juego (Assets/Ropa/{Etapa}/{Clave}{Etapa}{Accion}.png).
            // Si se cambia o agrega algo acá, hay que borrar tamagotchi.db
            // para que se vuelva a crear con los datos nuevos.
            // (Se usan objetos anónimos "new { ... }" porque HasData no acepta
            // entidades que tengan cargada una navegación, como Pinguinos)
            entity.HasData(
                new { Id = 1, Nombre = "Traje de oso", Clave = "Oso", Etapa = "Bebe", Orden = 1 },
                new { Id = 2, Nombre = "Traje de araña", Clave = "Arana", Etapa = "Nino", Orden = 2 },
                new { Id = 3, Nombre = "Remera de Mario", Clave = "Remera1", Etapa = "Adolescente", Orden = 3 }
            );
        });

        modelBuilder.Entity<AccionRegistrada>(entity =>
        {
            entity.ToTable("accion_registrada");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.PinguinoId).HasColumnName("pinguino_id");
            entity.Property(e => e.Accion).HasColumnName("accion").HasMaxLength(30).IsRequired();
            entity.Property(e => e.Fecha).HasColumnName("fecha");

            entity.HasOne(a => a.Pinguino)
                .WithMany(p => p.Acciones)
                .HasForeignKey(a => a.PinguinoId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
