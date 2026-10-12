# Tamagotchi Pingüino

TP 4 - Programación II
Alumna: Celeste Gómez (trabajo individual)

Hice un tamagotchi de un pingüino que hay que cuidar para que crezca. Pasa por bebé, niño y adolescente, y si lo cuidás bien llega a adulto. Si lo descuidás mucho tiempo, pierde (Game Over). Todos los dibujos del juego los hice yo: los escenarios, el pingüino en cada etapa y la ropa.

Para jugar se hace clic en los objetos de cada lugar: la cama para dormir, la mesa para comer, la ducha para bañarse, los juguetes y la compu para jugar, etc. En el ropero de la habitación se le puede cambiar la ropa con una rueda que gira.

Al empezar se elige un usuario y un pingüino. Cada usuario puede tener varios pingüinos guardados, así puede jugar más de una persona. La partida se va guardando sola en la base de datos a través de la API.

## Proyectos

- Tamagotchi: la aplicación de escritorio, hecha con Avalonia
- TamagotchiApi: la Web API con Controllers, Entity Framework Core y SQLite

## Cómo ejecutarlo

Hace falta tener .NET 8. No hay que crear la base a mano: la primera vez que se ejecuta la API se crea sola (queda el archivo tamagotchi.db). Igual dejé el script database.sql por si se quiere crear con sqlite3.

Primero hay que ejecutar la API y después el juego:

1. dotnet run --project TamagotchiApi
   (para probar los endpoints: http://localhost:5080/swagger)
2. dotnet run --project Tamagotchi

En Visual Studio se pueden iniciar los dos juntos desde "Configurar proyectos de inicio", poniendo la API primero.

## Base de datos

Tiene 5 tablas:

- usuario: las personas que juegan. Un usuario puede tener varios pingüinos.
- pinguino: cada partida guardada (nombre, etapa, si es de día o de noche, ropa que tiene puesta y si ya terminó). Pertenece a un usuario.
- estado_pinguino: las barras del pingüino (hambre, energía, higiene, diversión y ganas de ir al baño). Hay uno por pingüino.
- ropa: la ropa que aparece en el ropero. Cada una es para una etapa (bebé, niño o adolescente).
- accion_registrada: lo que va haciendo cada pingüino (comer, dormir, bañarse...). Con esto se muestra cuál es su acción favorita.

Si se borra un usuario se borran sus pingüinos, y si se borra un pingüino se borra su estado y su historial.

## Endpoints

Usuarios
- GET /api/usuarios
- GET /api/usuarios/{id}
- POST /api/usuarios
- PUT /api/usuarios/{id}
- DELETE /api/usuarios/{id}

Pingüinos
- GET /api/usuarios/{usuarioId}/pinguinos
- GET /api/pinguinos/{id}
- POST /api/pinguinos
- PUT /api/pinguinos/{id}/partida
- PUT /api/pinguinos/{id}/ropa
- DELETE /api/pinguinos/{id}
- POST /api/pinguinos/{id}/acciones

Ropa
- GET /api/ropas
- GET /api/ropas/{id}

## Pantallas

Inicio, selección de usuario y pingüino, habitación, cocina, baño, parque, mapa, ropero, game over y final.
