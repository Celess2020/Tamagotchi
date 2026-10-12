using Microsoft.EntityFrameworkCore;
using TamagotchiApi.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();

// La base es SQLite: un archivo tamagotchi.db en la carpeta del proyecto
// de la API (la ruta está en appsettings.json)
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

// Swagger, como indicó el profe
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Si la base todavía no existe, la crea con todas las tablas y la ropa
// inicial (HasData en AppDbContext). Así no hace falta ningún paso manual:
// se ejecuta la API y la base ya queda lista. (Equivalente: database.sql)
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.EnsureCreated();
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthorization();

app.MapControllers();

app.Run();
