# PROJECT_GUIDELINES — __RootNamespace__

> **Uso previsto:** este archivo es la referencia para añadir módulos nuevos a este backend manteniendo la misma arquitectura, nomenclatura y patrones con los que fue generado. Está pensado para que tanto humanos como una IA lo usen como "norte" al trabajar en este repo.
>
> Namespace raíz: **`__RootNamespace__`** (`Empresa = __Empresa__`, `Aplicación = __Aplicacion__`). No se cambia por producto — cada capa es `__RootNamespace__.{Capa}`.

---

# 1. STACK Y FILOSOFÍA

## 1.1 Stack

| Pieza | Elección |
|-------|----------|
| TFM | `net10.0` |
| Host | ASP.NET Core Web (`Microsoft.NET.Sdk.Web`) |
| HTTP | Controllers `[ApiController]`, prefijo `/api`, URLs **lowercase** |
| Casos de uso | **MediatR** 12.x (`IRequest` / `IRequestHandler`) |
| Datos | **Dapper** + **Dapper.FastCrud** + `Microsoft.Data.SqlClient` — **sin EF Core** |
| BD | SQL Server — scripts manuales en `scripts/query DB/` (sin EF Migrations) |
| JSON | Newtonsoft.Json en Controllers |
| Auth | JWT Bearer (HS256), hashing PBKDF2-HMACSHA256 |
| Logs | Serilog (Console + Seq opcional) |
| Docs | Swashbuckle + este `api_resumen.md` |
| Health | `/health` JSON + check SQL (si hay connection string) |

### Opcional — NO incluido en el scaffold base

Estas piezas existen en el proyecto de referencia pero se dejaron fuera del scaffold base para mantenerlo mínimo. Añádelas solo si el producto las necesita:

- **Hangfire** (jobs recurrentes): `Hangfire.AspNetCore` + `Hangfire.SqlServer`, jobs en `Infrastructure/Jobs/`, dashboard `/hangfire`.
- **Pipeline de Email**: `IEmailService` → `ChannelEmailQueue` (Singleton) → `EmailBackgroundWorker` (HostedService) → `EmailSender` (Scoped) → Azure Communication Email. Plantillas HTML en `Host/templates/` con `{{Clave}}`, `CopyToOutputDirectory=Always`.
- **Excel**: `ClosedXML` para exportación.
- **Multipart/Uploads**: `[FromForm] CrudDto` con `IEnumerable<IFormFile>? Files`, guardar en `Host/Uploads/{tipo}/{id}/…`.

## 1.2 Filosofía

- Clean Architecture, Controller **delgado** → MediatR → Handler → `I*Repository` → Dapper.
- Soft-delete: `IdEstado = 'ACT' | 'ANU'` (u otros códigos de dominio, ej. `'BLO'` para usuarios bloqueados).
- Multiempresa: filtrar por `IdEmpresa` cuando aplique.
- Lecturas → `Response<T>` · Escrituras → `Transaction`.
- **Errores de negocio: `type=ERR` con HTTP 200** — el cliente mira el campo `type`, no el status code. Solo `NOT_FOUND` produce 404 real.
- CRUD: `IdXxx = 0` crea · `IdXxx > 0` actualiza (mismo endpoint POST).
- `[Authorize]` está **comentado** en `BaseApiController` — el JWT está funcional pero no hay enforcement por defecto. Descoméntalo antes de exponer el backend fuera de un entorno controlado.

---

# 2. ESTRUCTURA DEL REPOSITORIO

```
__RootNamespace__.slnx
Directory.Build.props
build/dependencies.props
api_resumen.md                          # contrato API (mantener siempre actualizado)
PROJECT_GUIDELINES.md                   # este archivo
README.md
scripts/
  └── query DB/                         # create-*.sql, alter-*.sql
src/
  __RootNamespace__.Host/
    Program.cs
    GlobalUsing.cs                      # global using Serilog;
    appsettings.json
    appsettings.Development.json
    Properties/launchSettings.json
    Extensions/
      ├── ApiPrefixConvention.cs
      ├── ServiceCollectionExtensions.cs   # JWT, FormOptions
      └── DbContextCollectionExtensions.cs # DapperContext
    Middleware/
      ├── RequestLoggingMiddleware.cs
      └── SecurityHeadersMiddleware.cs
  __RootNamespace__.API/
    GlobalUsing.cs
    BaseApiController.cs
    DefaultApiConventions.cs
    Controllers/
      ├── DefaultController.cs
      ├── IdentityController.cs
      └── {Modulo}Controller.cs         # plural REST: Productos, Clientes…
  __RootNamespace__.Application/
    GlobalUsing.cs
    ApplicationServiceExtensions.cs
    Auth/AuthService.cs
    {Modulo}/                           # Producto, Cliente…
      └── {Modulo}Service.cs            # records (Commands/Queries) + Handler, un archivo por módulo
  __RootNamespace__.Domain/
    GlobalUsing.cs
    Common/BaseEntity.cs
    Auth/UserLoginCommand.cs
    Entities/{Entidad}.cs
    Interfaces/I{Entidad}Repository.cs
  __RootNamespace__.Infrastructure/
    GlobalUsing.cs
    InfrastructureServiceExtensions.cs
    Context/DapperContext.cs
    Persistence/{Entidad}Repository.cs
  __RootNamespace__.Common/
    DTOs/{Modulo}/
    Result/Response.cs, Transaction.cs
    Enumeraciones/TypeTransaction.cs
    Auth/JwtHandler.cs
    Utils/Encrypt.cs                    # clase Encrypt (AES) + PasswordHasher (PBKDF2)
```

## 2.1 Referencias entre proyectos

```
Host           → API, Application, Infrastructure
API            → Application, Common
Application    → Infrastructure, Common
Infrastructure → Domain, Common
Domain         → Common
Common         → (ningún proyecto interno)
```

## 2.2 Nomenclatura

| Concepto | Convención | Ejemplo |
|----------|------------|---------|
| Root namespace | `__RootNamespace__` | fijo, no cambia por módulo |
| Namespace C# | `__RootNamespace__.{Capa}...` | `__RootNamespace__.API.Controllers` |
| Controller class | Plural + `Controller` | `ProductosController` |
| Ruta HTTP | `/api/{controller}` lowercase | `/api/productos` |
| Carpeta Application | Singular (nombre del módulo) | `Producto/` |
| Archivo Application | `{Modulo}Service.cs` | `ProductoService.cs` |
| Handler | `{Modulo}Handler` | `ProductoHandler` |
| Query / Command | `Get…Query` / `Crud…Command` / `Anular…Command` | |
| Entity / Tabla / PK | Singular / `Id{Entidad}` | `Producto`, `IdProducto` |
| Interface / Repo | `I{Entidad}Repository` / `{Entidad}Repository` | |
| DTO | `{Entidad}Dto` (lectura) / `Crud{Entidad}Dto` (escritura) | |
| Script SQL | `create-{Entidad}.sql` | |

---

# 3. NÚCLEO — YA CREADO, NO LO DUPLIQUES

Estas piezas ya existen en el proyecto y se usan tal cual en todos los módulos nuevos — no las reimplementes por módulo:

- `TypeTransaction` (enum ERR/OK/NOT_FOUND) — `src/__RootNamespace__.Common/Enumeraciones/TypeTransaction.cs`
- `Response<T>` / `Transaction` (envelopes) — `src/__RootNamespace__.Common/Result/`
- `BaseEntity` (IdEstado, UsuarioCreacion, FechaCreacion, UsuarioModificacion, FechaModificacion) — `src/__RootNamespace__.Domain/Common/BaseEntity.cs`
- `DapperContext` — `src/__RootNamespace__.Infrastructure/Context/DapperContext.cs`
- `BaseApiController` (Mediator, HandleResponse, HandleTransaction) — `src/__RootNamespace__.API/BaseApiController.cs`
- `ApiPrefixConvention` — `src/__RootNamespace__.Host/Extensions/ApiPrefixConvention.cs`
- `JwtHandler` / `Encrypt` / `PasswordHasher` — `src/__RootNamespace__.Common/Auth/` y `Common/Utils/Encrypt.cs`

## Envelope JSON (contrato)

**Response**
```json
{ "idResponse": 200, "type": "OK", "message": "OK", "totalRows": 1, "data": {} }
```

**Transaction**
```json
{ "type": "OK", "message": "...", "key": "15", "output": null }
```

`type` se serializa como **string** del enum (`"OK"|"ERR"|"NOT_FOUND"`).

---

# 4. SQL — CONVENCIONES Y PLANTILLA

## 4.1 Columnas estándar (heredadas de `BaseEntity`)

| Columna | Tipo | Notas |
|---------|------|-------|
| `Id{Entidad}` | INT IDENTITY PK | |
| `IdEmpresa` | INT NOT NULL | si multiempresa |
| `IdEstado` | VARCHAR(5) DEFAULT 'ACT' | ACT/ANU (u otros: BLO en Usuarios) |
| `UsuarioCreacion` | INT NOT NULL | |
| `FechaCreacion` | DATETIME DEFAULT GETDATE() | |
| `UsuarioModificacion` | INT NULL | |
| `FechaModificacion` | DATETIME NULL | |

## 4.2 Plantilla `scripts/query DB/create-{Entidad}.sql`

Ver `scripts/query DB/create-Producto.sql` como ejemplo real y funcionando.

```sql
USE [__Database__];
GO

IF OBJECT_ID(N'dbo.{Entidad}', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.{Entidad}
    (
        Id{Entidad}         INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_{Entidad} PRIMARY KEY,
        IdEmpresa           INT NOT NULL,
        -- … campos de negocio …
        IdEstado            VARCHAR(5) NOT NULL CONSTRAINT DF_{Entidad}_IdEstado DEFAULT ('ACT'),
        UsuarioCreacion     INT NOT NULL,
        FechaCreacion       DATETIME NOT NULL CONSTRAINT DF_{Entidad}_FechaCreacion DEFAULT (GETDATE()),
        UsuarioModificacion INT NULL,
        FechaModificacion   DATETIME NULL
    );

    CREATE INDEX IX_{Entidad}_IdEmpresa_Estado ON dbo.{Entidad} (IdEmpresa, IdEstado);
    PRINT 'OK: tabla {Entidad} creada';
END
ELSE
    PRINT 'INFO: {Entidad} ya existe';
GO
```

---

# 5. MÓDULO CRUD COMPLETO — PLANTILLA CANÓNICA

Usa el módulo **`Producto`** (ya implementado en este repo) como plantilla textual para cualquier módulo nuevo. Sustituye:
- `{Entidad}` = nombre singular (ej. `Cliente`)
- `{Entidades}` = nombre plural del controller (ej. `Clientes`)
- Campos de negocio a medida

## 5.1 Entity — `src/__RootNamespace__.Domain/Entities/{Entidad}.cs`

```csharp
[Table("{Entidad}")]
public class {Entidad} : BaseEntity
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [Column("Id{Entidad}")]
    public int Id{Entidad} { get; set; }

    [Required]
    [Column("IdEmpresa")]
    public int IdEmpresa { get; set; }

    // … propiedades con [Column] …
}
```

## 5.2 Interface — `src/__RootNamespace__.Domain/Interfaces/I{Entidad}Repository.cs`

```csharp
using __RootNamespace__.Common.DTOs.{Entidad};

namespace __RootNamespace__.Domain.Interfaces;

public interface I{Entidad}Repository
{
    Task<{Entidad}Dto?> GetByIdAsync(int id);
    Task<IEnumerable<{Entidad}Dto>> GetFilterAsync(int idEmpresa, string? estado = "ACT");
    Task<(int NuevoId, string Mensaje, bool IsOk)> CreateAsync(Crud{Entidad}Dto dto);
    Task<(bool IsOk, string Mensaje)> UpdateAsync(Crud{Entidad}Dto dto);
    Task<(bool IsOk, string Mensaje)> AnularAsync(int id, int idUsuario);
}
```

## 5.3 DTOs — `src/__RootNamespace__.Common/DTOs/{Entidad}/`

```csharp
public class {Entidad}Dto
{
    public int Id{Entidad} { get; set; }
    public int IdEmpresa { get; set; }
    // … campos de lectura (fechas como string formateado) …
    public string IdEstado { get; set; } = "ACT";
    public int UsuarioCreacion { get; set; }
    public string FechaCreacion { get; set; } = string.Empty;
}

public class Crud{Entidad}Dto
{
    public int Id{Entidad} { get; set; }
    public int IdEmpresa { get; set; }
    // … campos de escritura …
    public int IdUsuario { get; set; } // usuario que ejecuta la acción
}
```

## 5.4 Repository — `src/__RootNamespace__.Infrastructure/Persistence/{Entidad}Repository.cs`

Ver `ProductoRepository.cs` como implementación real completa (Get/GetFilter con SQL+FORMAT, Create/Update/Anular con Dapper.FastCrud).

## 5.5 Application Handler — `src/__RootNamespace__.Application/{Entidad}/{Entidad}Service.cs`

Ver `ProductoService.cs` como implementación real completa (records `Get{Entidad}ByIdQuery`, `Get{Entidades}FilterQuery`, `Crud{Entidad}Command`, `Anular{Entidad}Command` + `{Entidad}Handler`).

## 5.6 Controller — `src/__RootNamespace__.API/Controllers/{Entidades}Controller.cs`

Ver `ProductosController.cs`:

```csharp
public class {Entidades}Controller(ILogger<{Entidades}Controller> logger) : BaseApiController(logger)
{
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
        => HandleResponse(await Mediator.Send(new Get{Entidad}ByIdQuery(id)));

    [HttpGet]
    public async Task<IActionResult> GetFilter([FromQuery] int idEmpresa, [FromQuery] string? estado = "ACT")
        => HandleResponse(await Mediator.Send(new Get{Entidades}FilterQuery(idEmpresa, estado)));

    [HttpPost]
    public async Task<ActionResult<Transaction>> Crud([FromBody] Crud{Entidad}Dto dto)
        => HandleTransaction(await Mediator.Send(new Crud{Entidad}Command(dto)));

    [HttpPut("{id}/anular")]
    public async Task<ActionResult<Transaction>> Anular(int id, [FromQuery] int idUsuario)
        => HandleTransaction(await Mediator.Send(new Anular{Entidad}Command(id, idUsuario)));
}
```

## 5.7 Endpoints resultantes

| Método | Ruta |
|--------|------|
| GET | `/api/{entidades}?idEmpresa=&estado=ACT` |
| GET | `/api/{entidades}/{id}` |
| POST | `/api/{entidades}` |
| PUT | `/api/{entidades}/{id}/anular?idUsuario=` |

## 5.8 Documentar en `api_resumen.md`

Añadir sección con objetivo, tabla de endpoints y JSON de ejemplo del Crud. Actualizar el índice.

---

# 6. CHECKLIST POR MÓDULO (copiar y tachar)

```
[ ] scripts/query DB/create-{Entidad}.sql  (ejecutar en SQL Server)
[ ] Domain/Entities/{Entidad}.cs
[ ] Domain/Interfaces/I{Entidad}Repository.cs
[ ] Common/DTOs/{Entidad}/{Entidad}Dto.cs + Crud{Entidad}Dto.cs
[ ] Infrastructure/Persistence/{Entidad}Repository.cs
[ ] InfrastructureServiceExtensions: AddScoped<I…, …>()
[ ] Application/{Entidad}/{Entidad}Service.cs (queries/commands/handler)
[ ] API/Controllers/{Entidades}Controller.cs
[ ] api_resumen.md actualizado
[ ] dotnet build OK
```

---

# 7. ROLES / AUTORIZACIÓN

`[Authorize]` está comentado en `BaseApiController`. Cuando el producto necesite proteger endpoints:

1. Descomentar `[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]` en `src/__RootNamespace__.API/BaseApiController.cs`.
2. Marcar `IdentityController` con `[AllowAnonymous]` (login/refresh deben seguir siendo públicos).
3. Si necesitas roles/permisos, decide entre: (a) claims individuales en el JWT (`JwtHandler.GenerateToken` empaqueta hoy todo en un único claim `ClaimTypes.UserData` como JSON — puedes añadir claims propios ahí), o (b) flags de negocio en la tabla `Usuarios` + políticas de autorización registradas en `ServiceCollectionExtensions.AddCustomAuthorization`.

---

# 8. ANTI-PATRONES (EVITAR)

1. Lógica de negocio en Controllers — va en el Handler.
2. Introducir EF Core / EF Migrations — este proyecto usa Dapper + scripts SQL manuales.
3. Devolver errores de validación de negocio como HTTP 500 — usa `TypeTransaction.ERR` (HTTP 200) o `NOT_FOUND` (404) según corresponda.
4. Olvidar registrar el repositorio nuevo en `InfrastructureServiceExtensions.AddInfrastructure`.
5. Crear un módulo sin actualizar `api_resumen.md`.
6. Inventar una nomenclatura distinta a la de este documento (`Services/` anémicos, Minimal APIs, etc.) — rompe la coherencia con el resto del proyecto.
7. Meter secretos (`Jwt:SecretKey`, `Jwt:KeyEncrypt`, `Jwt:IvEncrypt`, connection strings) en el repositorio — usan `dotnet user-secrets` en desarrollo y variables de entorno en producción.

---

# 9. MAPA MENTAL DE UN REQUEST

```
HTTP /api/productos
  → ProductosController.GetFilter
    → Mediator.Send(GetProductosFilterQuery)
      → ProductoHandler.Handle
        → IProductoRepository.GetFilterAsync
          → DapperContext.CreateConnection
            → SQL / Dapper.FastCrud
        → Response<IEnumerable<ProductoDto>>
  → HandleResponse → JSON
```
