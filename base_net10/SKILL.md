---
name: base_net10
description: Genera un backend .NET 10 nuevo, listo para compilar, con una arquitectura de referencia probada (Clean Architecture de 6 capas, MediatR CQRS, Dapper + Dapper.FastCrud, envelope Response<T>/Transaction, JWT preconfigurado, Serilog, Swagger) y un módulo CRUD de ejemplo (Producto) mostrando el flujo completo Controller → Handler → Repository → Entity → SQL. Pide Empresa y Aplicación para nombrar la solución como {Empresa}.{Aplicacion}.{Capa}.
---

# base_net10 — Generador de backend base (.NET 10, arquitectura Clean Architecture)

Esta skill crea, en un directorio nuevo, un backend .NET 10 completo y compilable con exactamente la misma arquitectura, nomenclatura y forma de trabajar del proyecto de referencia: 6 capas (Host/API/Application/Domain/Infrastructure/Common), MediatR como CQRS, Dapper + Dapper.FastCrud como acceso a datos (sin EF Core), el envelope `Response<T>`/`Transaction` en todas las respuestas, JWT ya configurado y funcional, y un módulo `Producto` de ejemplo con el flujo end-to-end completo.

## Alcance ("core mínimo")

Incluye: 6 proyectos, MediatR, Dapper/FastCrud, JWT (login + refresh, hashing PBKDF2, bloqueo tras 3 intentos fallidos), `Response<T>`/`Transaction`, Serilog, Swagger con seguridad Bearer, health checks, middlewares de logging y cabeceras de seguridad, módulo `Producto` CRUD completo con su tabla SQL y un usuario `admin` sembrado.

NO incluye (documentado como "añadir a demanda" en el `PROJECT_GUIDELINES.md` que se genera dentro del proyecto): Hangfire, pipeline de Email (Azure Communication Email), exportación Excel (ClosedXML), el binder custom del paquete `Optional`.

`[Authorize]` queda **comentado** en `BaseApiController` (igual que en el proyecto de referencia): el JWT está 100% funcional pero ningún endpoint de negocio queda protegido por defecto — hay que descomentar el atributo cuando el producto lo requiera (instrucciones en `PROJECT_GUIDELINES.md` §7 del proyecto generado).

## Pasos a ejecutar

### 1. Recolectar datos

Usa `AskUserQuestion` (o toma lo que el usuario ya haya dado en su mensaje) para conseguir:

- **Empresa**: nombre corto, PascalCase, sin espacios ni acentos (ej. "Acme Corp" → `Acme`). Obligatorio.
- **Aplicación**: nombre corto del producto, PascalCase (ej. "Gestión de Facturas" → `Facturas`). Obligatorio.
- **Base de datos** (SQL Server `Initial Catalog`): puede ser distinta al namespace. Si el usuario no la indica, usa `{Aplicacion}_DB` como default razonable y dilo explícitamente.
- **Directorio destino**: si el usuario no indica una ruta, usa `./{Empresa}.{Aplicacion}` relativo al directorio de trabajo actual.

Normaliza espacios/acentos a PascalCase antes de pasarlos al script (ej. "Gestión de Facturas" → `GestionDeFacturas` o, mejor, algo corto como `Facturas` — confirma con el usuario si el nombre que armaste no es obvio).

### 2. Generar el proyecto

Ejecuta el script de scaffolding de esta skill (copia la plantilla, sustituye placeholders, corre `dotnet user-secrets init` y `dotnet build`):

```bash
bash ~/.claude/skills/base_net10/scaffold.sh "<Empresa>" "<Aplicacion>" "<Database>" "<TargetDir>"
```

El script falla si `<TargetDir>` ya existe (no sobrescribe nada) — si eso pasa, pregunta al usuario si quiere otro nombre/ubicación.

Si `dotnet build` (parte final del script) falla, lee el error, corrige el/los archivo(s) generados en `<TargetDir>` (no el archivo de plantilla en `reference/`, salvo que el bug esté en la plantilla misma — en ese caso corrige ambos para que la próxima vez que se use la skill no se repita) y vuelve a compilar hasta 0 errores. No des el trabajo por terminado si no compila.

### 3. Reportar al usuario

Al terminar, informa:

- Ruta del proyecto generado y namespace raíz (`{Empresa}.{Aplicacion}`).
- Cómo correrlo: `dotnet run --project <TargetDir>/src/{RootNamespace}.Host` (usa `~/.dotnet/dotnet` si `dotnet` no está en el PATH). Swagger queda en `/swagger` (`SwaggerAvailable: true`).
- Que hay un usuario de arranque sembrado por `create-Usuarios.sql`: `admin` / `Admin123!` — para probar `POST /api/identity/login` en cuanto la base de datos esté creada. Recomienda cambiar esa contraseña.
- Secrets que faltan configurar antes de arrancar, vía `dotnet user-secrets set --project <TargetDir>/src/{RootNamespace}.Host/{RootNamespace}.Host.csproj <Clave> <Valor>`:
  - `ConnectionStrings:DefaultConnection` (cadena de conexión SQL Server real)
  - `Jwt:SecretKey` (string largo y aleatorio)
  - `Jwt:KeyEncrypt` / `Jwt:IvEncrypt` (Base64 válidos — AES-256 key de 32 bytes e IV de 16 bytes; se pueden generar con `openssl rand -base64 32` y `openssl rand -base64 16`)
- Que hay que ejecutar los scripts de `<TargetDir>/scripts/query DB/` (`create-Usuarios.sql`, `create-Producto.sql`) contra la base de datos indicada antes de poder loguear o usar el módulo de ejemplo.
- Que el checklist para añadir un módulo nuevo está en `<TargetDir>/PROJECT_GUIDELINES.md` §5-§6, usando el módulo `Producto` (`src/**/*Producto*`) como plantilla de referencia end-to-end.

## Notas de implementación

- `dotnet` puede no estar en el `PATH` del shell aunque el SDK esté instalado — el script prueba primero `~/.dotnet/dotnet` y cae a `dotnet` del PATH si no existe. Si ninguno funciona, avisa al usuario que instale el SDK de .NET 10.
- La plantilla vive en `~/.claude/skills/base_net10/reference/` como árbol de archivos reales (no un `dotnet new template`), para tener control total y fidelidad exacta con la arquitectura de referencia. Si en el futuro hay que ajustar algo del scaffold (paquetes, patrones, capas), edita esos archivos de plantilla directamente — los placeholders son `__RootNamespace__`, `__Empresa__`, `__Aplicacion__`, `__Database__` y `__Year__`, tanto en contenido como en nombres de archivo/carpeta.
- El hash de la contraseña del usuario `admin` sembrado en `create-Usuarios.sql` es PBKDF2-HMACSHA256 (100000 iteraciones, salt 16 bytes, hash 32 bytes, formato `salt:hash` en Base64) — el mismo algoritmo que usa `PasswordHasher` en `Common/Utils/Encrypt.cs`. No lo regeneres salvo que cambies la contraseña de arranque.
