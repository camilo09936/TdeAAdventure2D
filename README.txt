# TdeAAdventure

Videojuego 2D de plataformas hecho en Unity, conectado a una API REST (ASP.NET Core) y a una base de datos MongoDB.
El jugador se registra, inicia sesión, sube plataformas sin fin, junta monedas y gemas, guarda sus partidas, ve el ranking y compra skins en la tienda.

| Parte            | Tecnología                   | Repositorio |
| Frontend (juego) | Unity 6 (6000.5.9f1), 2D URP | https://github.com/camilo09936/TdeAAdventure2D |
| Backend (API)    | ASP.NET Core (.NET 10)       | https://github.com/camilo09936/ApiTdeAAdventure |
| Base de datos    | MongoDB (Atlas o local)      | Carpeta `Entrega` (dump) |

## 1. Requisitos

- Unity Hub y Unity 6000.5.9f1 (con soporte para Windows Build).
- .NET SDK 10 → https://dotnet.microsoft.com/download
- MongoDB (una de estas dos opciones):
  - MongoDB Community Server local, o
  - una cuenta gratuita de MongoDB Atlas.
- MongoDB Database Tools (para `mongorestore`) → https://www.mongodb.com/try/download/database-tools
- Git.

## 2. Base de datos

El dump está en la carpeta `Entrega` (`dump_TdeAAdventure.zip`).

1. Descomprime el zip. Queda una carpeta `dump/TdeAAdventureDB` con las colecciones `jugadores`, `partidas` y `cosmeticos`.
2. Restaura la base:

   MongoDB local:
   ```powershell
   mongorestore --uri="mongodb://localhost:27017" RUTA\A\dump
   ```

   MongoDB Atlas (tu propio clúster):
   ```powershell
   mongorestore --uri="TU_CADENA_DE_CONEXION" RUTA\A\dump
   ```
   En Atlas, agrega tu IP en Security → Network Access.

3. Comprueba que existan la base `TdeAAdventureDB` y sus 3 colecciones (por ejemplo, con MongoDB Compass).
   Los índices únicos de `jugadores` (usuario y correo) vienen incluidos en el dump.

## 3. Backend (API)

```powershell
git clone https://github.com/camilo09936/ApiTdeAAdventure.git
cd ApiTdeAAdventure\ApiTdeAAdventure
```

### Configurar la conexión a MongoDB
La cadena de conexión no está en el repositorio. Configúrala con una de estas opciones:

Opción A: user-secrets (recomendada)
```powershell
dotnet user-secrets set "MongoDB:ConnectionString" "TU_CADENA_DE_CONEXION"
```

Opción B: variable de entorno
```powershell
$env:MongoDB__ConnectionString = "TU_CADENA_DE_CONEXION"
```

Ejemplos de cadena:
- Local: `mongodb://localhost:27017`
- Atlas: `mongodb+srv://USUARIO:CONTRASEÑA@cluster0.xxxxx.mongodb.net/`

El nombre de la base (`TdeAAdventureDB`) está en `appsettings.json` → `MongoDB:DatabaseName`.

### Ejecutar
```powershell
dotnet run --launch-profile http
```
La API queda en http://localhost:5023.
Documentación interactiva (Swagger): http://localhost:5023/swagger

Endpoints principales: `api/Jugador`, `api/Partida`, ranking (leaderboard) y `api/Cosmetico`.
Deja esta consola abierta mientras juegas.

## 4. Frontend (Unity)

```powershell
git clone https://github.com/camilo09936/TdeAAdventure2D.git
```

1. Abre Unity Hub → Add → Add project from disk y elige la carpeta que contiene `Assets` y `ProjectSettings` (la subcarpeta `TdeAAdventure2D` del repo).
2. Abre el proyecto con Unity 6000.5.9f1. La primera vez tarda unos minutos en importar.
3. Verifica la dirección de la API en `Assets/Scripts/ApiConfig.cs`:
   ```csharp
   BaseUrl = "http://localhost:5023"
   ```
   Si tu API corre en otro puerto, cámbialo aquí.
4. Si Unity bloquea las peticiones HTTP, ve a Edit → Project Settings → Player → Other Settings y pon Allow downloads over HTTP en Always allowed.
5. Abre la escena `Assets/Scenes/Login.unity` y pulsa Play.

## 5. Cómo jugar

1. Con la API corriendo, regístrate (usuario, correo y contraseña) y luego inicia sesión.
2. En el menú: Jugar, Ranking y Tienda.
3. Controles:
   - `A` / `D`: moverse
   - `Espacio`: saltar
   - `R`: reintentar al perder · `M`: volver al menú
4. Monedas (moneda = 1, gema = 3) y obstáculos: parciales, lápices, calculadoras que caen y bloques de código que empujan.
5. En la esquina superior hay botones para activar o desactivar la música y los efectos.
6. Al perder, la partida se guarda en la API (puntaje, monedas, tiempo y velocidad final).

## 6. Solución de problemas

| Problema                                         | Solución |
| "No se pudo conectar con el servidor" en Unity   | La API no está corriendo, o `BaseUrl` es incorrecta (debe ser `http://`, no `https://`). |
| La API no arranca o da error de conexión a Mongo | Revisa la cadena de conexión. En Atlas, tu IP debe estar permitida. |
| Login dice credenciales incorrectas              | El inicio de sesión usa nombre de usuario, no correo. |
| `mongorestore` o `mongodump` no se reconocen     | Instala las Database Tools y reabre la consola. |
| Unity no abre el proyecto                        | Usa exactamente la versión 6000.5.9f1. |