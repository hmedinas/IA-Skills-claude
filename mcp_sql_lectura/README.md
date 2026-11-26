# mcp_sql_lectura

Servidor **MCP en C# / .NET 10** que da a Claude acceso de **solo lectura** a una base de datos SQL Server.
Claude lo ve como herramientas `mcp__sql-lectura__*`. La contraseña nunca pasa por el modelo: vive en un archivo fuera del repositorio.

Guía del tipo de componente: [docs/04-mcp.md](../docs/04-mcp.md).

## Herramientas
| Herramienta | Qué hace |
|---|---|
| `consultar(sql, max_filas?)` | Ejecuta **una** consulta `SELECT` / `WITH … SELECT` y devuelve `{columnas, filas, filasDevueltas, truncado}` |
| `listar_tablas(filtro?)` | Tablas y vistas con esquema, tipo y filas aproximadas |
| `describir_tabla(tabla)` | Columnas: tipo, longitud, nulos, identidad, clave primaria |
| `ver_definicion(objeto)` | Código de un procedimiento, vista, función o trigger |
| `buscar_en_codigo(texto)` | Qué procedimientos, vistas o funciones contienen un texto (p. ej. una tabla o columna) |

## Seguridad: tres barreras
1. **Validador** (`ValidadorSql.cs`):
   - quita comentarios, cadenas e identificadores entrecomillados;
   - exige **una sola sentencia** que empiece por `SELECT` o `WITH`;
   - rechaza `INSERT`, `UPDATE`, `DELETE`, `MERGE`, `CREATE`, `ALTER`, `DROP`, `EXEC`, `INTO`, `DECLARE`, `SET`, `OPENROWSET`, `sp_`, `xp_`, `@`, `GO`… (32 casos en `--autoprueba`).
2. **Transacción que siempre se deshace**:
   - toda consulta va dentro de una transacción con `ROLLBACK` en `finally`;
   - nivel `READ UNCOMMITTED`, para no bloquear a nadie en producción;
   - timeout y límite de filas.
3. **Conexión `ApplicationIntent=ReadOnly`**. Y, sobre todo, **usa un login con permisos solo de lectura** (`db_datareader` y, si quieres que vea el código de los procedimientos, `VIEW DEFINITION`). Es la barrera de verdad.

## Requisitos
- SDK de .NET 10 (`dotnet --list-sdks`).
- Un archivo de texto **fuera de cualquier repositorio** con la cadena de conexión en una línea. Las líneas que empiezan por `#` se ignoran:
  ```text
  # C:\Users\<tú>\.secretos\bd-lectura.txt
  Server=servidor,puerto;Database=MiBase;User Id=login_lectura;Password=***;TrustServerCertificate=true
  ```

## Instalar
### Opción A: plugin (una BD para todos tus proyectos)
```bash
claude plugin marketplace add hmedinas/IA-Skills-claude   # una vez
claude plugin install mcp_sql_lectura@ia-skills-claude
```
Al activarlo, Claude Code pide el **archivo de conexión**, el máximo de filas y el timeout (`userConfig`). Se pueden cambiar después en `/config`.
La primera conexión tarda más porque compila el proyecto.

### Opción B: a mano, por proyecto (recomendado si cada proyecto usa una BD distinta)
Desde la carpeta del proyecto, con el nombre que quieras para el servidor (p. ej. `mi-bd`):
```bash
claude mcp add mi-bd --scope local \
  -e MCP_SQL_CONEXION_ARCHIVO="C:/Users/<tú>/.secretos/bd-lectura.txt" \
  -e DOTNET_NOLOGO=1 \
  -- dotnet run --project "C:/ruta/IA-Skills-claude/mcp_sql_lectura/src/McpSqlLectura" -c Release -v q --nologo -p:NuGetAudit=false
```
`--scope local` lo deja solo para ti en ese proyecto: no se sube a git.

Comprobar: `/mcp` → `mi-bd · connected` y las 5 herramientas.

## Variables de entorno
| Variable | Por defecto | Qué es |
|---|---|---|
| `MCP_SQL_CONEXION_ARCHIVO` | — (obligatoria, o `MCP_SQL_CONEXION`) | Ruta del archivo con la cadena de conexión |
| `MCP_SQL_CONEXION` | — | Cadena directa (desaconsejada: queda en la configuración) |
| `MCP_SQL_MAX_FILAS` | 200 | Filas por defecto de `consultar` (tope 5000) |
| `MCP_SQL_TIMEOUT` | 30 | Segundos por consulta (1–300) |

## Probar sin Claude
```bash
dotnet run --project src/McpSqlLectura -- --autoprueba     # 32 casos del validador, no necesita BD
```

## Lecciones (por si haces otro MCP)
- **stdout es del protocolo**: los logs van a stderr. `dotnet run` necesita `-v q --nologo` y `DOTNET_NOLOGO=1` para no escribir en stdout al compilar.
- **En Windows no lances un MCP con `bash`**: desde un proceso de Windows, `bash` puede ser el de **WSL** (`System32\bash.exe`) y no Git Bash. Por eso aquí el comando es `dotnet` directamente.
- `System.Security.Cryptography.Pkcs` se fija a 10.0.14 porque `Microsoft.Data.SqlClient 7.1.1` arrastraba una versión con vulnerabilidades.
