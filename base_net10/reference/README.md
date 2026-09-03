# __RootNamespace__

Backend API .NET 10 — Clean Architecture (6 capas), CQRS con MediatR, Dapper + Dapper.FastCrud, JWT.

Generado con la skill `base_net10` a partir de la arquitectura y convenciones de referencia (ver `PROJECT_GUIDELINES.md`).

## Estructura

```
src/
  __RootNamespace__.Host/            # punto de entrada (Program.cs, appsettings, middlewares)
  __RootNamespace__.API/             # controladores
  __RootNamespace__.Application/     # casos de uso (MediatR: Commands/Queries + Handlers)
  __RootNamespace__.Infrastructure/  # persistencia (Dapper), repositorios
  __RootNamespace__.Domain/          # entidades e interfaces de repositorio
  __RootNamespace__.Common/          # DTOs, Response/Transaction, utilidades, JWT
```

## Arrancar en local

```bash
dotnet user-secrets set --project src/__RootNamespace__.Host/__RootNamespace__.Host.csproj ConnectionStrings:DefaultConnection "Data Source=...;Initial Catalog=__Database__;User Id=...;Password=...;TrustServerCertificate=True;"
dotnet user-secrets set --project src/__RootNamespace__.Host/__RootNamespace__.Host.csproj Jwt:SecretKey "<clave-larga-aleatoria>"
dotnet user-secrets set --project src/__RootNamespace__.Host/__RootNamespace__.Host.csproj Jwt:KeyEncrypt "<base64-32-bytes>"
dotnet user-secrets set --project src/__RootNamespace__.Host/__RootNamespace__.Host.csproj Jwt:IvEncrypt "<base64-16-bytes>"

# Ejecutar los scripts de scripts/query DB/ (create-Usuarios.sql, create-Producto.sql) contra la base de datos antes de arrancar

dotnet run --project src/__RootNamespace__.Host
```

Swagger: `https://localhost:5081/swagger` (perfil `https` de `launchSettings.json`).

## Módulo de ejemplo

`Producto` (`/api/productos`) muestra el flujo completo Controller → MediatR Command/Query → Handler → Repository → Entity → SQL. Úsalo como plantilla para módulos nuevos (ver `PROJECT_GUIDELINES.md` §7-§8).

## Seguridad

JWT (`/api/identity/login`, `/api/identity/refresh`) está listo y funcional (hash PBKDF2, bloqueo tras 3 intentos fallidos), pero `[Authorize]` está **comentado** en `BaseApiController` — ningún endpoint de negocio está protegido por defecto. Descomenta el atributo en `src/__RootNamespace__.API/BaseApiController.cs` cuando el producto lo requiera.
