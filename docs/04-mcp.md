# 04 · MCP (Model Context Protocol)

## Qué es
Un **servidor MCP** es un programa que expone **herramientas** (con nombre, parámetros y descripción) siguiendo un protocolo estándar.
Claude Code arranca el programa (o se conecta a él por HTTP) y Claude ve esas herramientas como propias:
`mcp__<servidor>__<herramienta>`.

Es lo que usas cuando Claude **no puede** acceder a algo: tu base de datos, Azure DevOps, una API interna, el navegador.

## Cuándo usarlo
- Dar acceso **controlado** a un sistema. El servidor decide qué se puede hacer: solo `SELECT`, solo leer tickets…
- **Ocultar credenciales:** la contraseña la tiene el servidor, nunca el modelo.
- Algo que harías con muchos comandos de shell frágiles, convertido en una herramienta limpia.
- **No** hace falta si basta con un comando de shell sencillo y seguro.

## Tipos de conexión
| Transporte | Cómo funciona | Típico |
|---|---|---|
| `stdio` | Claude Code arranca el programa y le habla por la entrada y salida estándar | Servidores locales tuyos (C#, Node, Python) |
| `http` | Claude Code se conecta a una URL | Servicios remotos (Postman, Jira, etc.) |

## Dónde vive
| Ámbito | Cómo |
|---|---|
| Local (solo tú, este proyecto) | `claude mcp add <nombre> -- <comando>` (ámbito por defecto) |
| Proyecto (se sube a git) | `<proyecto>/.mcp.json`, o `claude mcp add --scope project …` |
| Personal (todos tus proyectos) | `claude mcp add --scope user …` |
| Plugin | `<plugin>/.mcp.json` |

Formato de `.mcp.json`:
```json
{
  "mcpServers": {
    "nexus-db": {
      "command": "dotnet",
      "args": ["run", "--project", "C:/ruta/NexusDbMcp"],
      "env": { "NEXUSDB_CONEXION_ARCHIVO": "C:/Users/yo/.secretos/nexusdb.txt" }
    }
  }
}
```
**Nunca** pongas contraseñas en `.mcp.json` si se sube a git. Usa variables de entorno o un archivo fuera del repo.

## Comandos útiles
```bash
claude mcp add nexus-db -- dotnet run --project C:/ruta/NexusDbMcp        # stdio
claude mcp add --transport http mi-api https://ejemplo.com/mcp             # http
claude mcp list                                                            # listar y ver el estado
claude mcp remove nexus-db
```
Dentro de la sesión, `/mcp` muestra los servidores conectados, sus herramientas y permite autenticar.

## Cómo se crea uno (C# / .NET)
Hay SDK oficial en NuGet: `ModelContextProtocol` (también hay SDK para TypeScript, Python y otros lenguajes). Esquema mínimo:

```csharp
// Program.cs — dotnet new console; dotnet add package ModelContextProtocol --prerelease; dotnet add package Microsoft.Extensions.Hosting
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ModelContextProtocol.Server;
using System.ComponentModel;

var builder = Host.CreateApplicationBuilder(args);
builder.Logging.AddConsole(o => o.LogToStandardErrorThreshold = LogLevel.Trace); // stdout es del protocolo: logs a stderr
builder.Services.AddMcpServer().WithStdioServerTransport().WithToolsFromAssembly();
await builder.Build().RunAsync();

[McpServerToolType]
public static class HerramientasBd
{
    [McpServerTool, Description("Ejecuta una consulta SELECT de solo lectura y devuelve las filas en JSON.")]
    public static string Consultar([Description("Consulta SQL; solo SELECT")] string sql)
    {
        // 1. Rechazar todo lo que no sea SELECT (como hook_solo_select, pero en el servidor).
        // 2. Abrir conexión con el login de LECTURA (la cadena se lee de un archivo fuera del repo).
        // 3. Ejecutar con timeout y límite de filas; devolver JSON.
        throw new NotImplementedException();
    }
}
```
Reglas importantes:
- **stdout es del protocolo**: nada de `Console.WriteLine` para logs; los logs van a stderr.
- La `Description` de cada herramienta y de cada parámetro es lo que Claude lee para decidir cómo usarla. Escríbela bien.
- **El servidor valida**: no confíes en que el modelo respete reglas; el MCP es la última barrera.

## Ejemplo real: `mcp_sql_lectura`
El esquema de arriba, completo y probado, está en [`mcp_sql_lectura/`](../mcp_sql_lectura/README.md):
- **5 herramientas:** `consultar`, `listar_tablas`, `describir_tabla`, `ver_definicion` y `buscar_en_codigo`.
- **Tres barreras:** validador, transacción que siempre se deshace y conexión `ReadOnly`.
- **Configuración:** el plugin pide al usuario el archivo de conexión (`userConfig`) y lo pasa como `${user_config.archivo_conexion}`.

Estructura:
```text
mcp_sql_lectura/
├── .claude-plugin/plugin.json   ← nombre, versión y userConfig (archivo de conexión, máx. filas, timeout)
├── .mcp.json                    ← cómo arrancarlo: dotnet run --project ${CLAUDE_PLUGIN_ROOT}/src/McpSqlLectura …
└── src/McpSqlLectura/           ← Program.cs, HerramientasSql.cs, ValidadorSql.cs, ConexionSql.cs, Autoprueba.cs
```

## Cómo se instala
- **A mano:** `claude mcp add …` o añadir la entrada a `.mcp.json`.
- **Como plugin:** el plugin lleva su `.mcp.json`; en las rutas se usa `${CLAUDE_PLUGIN_ROOT}`. Los valores que pide al usuario
  se declaran en `userConfig` de `plugin.json` y se usan como `${user_config.clave}`.
- Comprobar con `/mcp` (estado `connected`) y pidiendo a Claude que liste las herramientas del servidor.

## Probar un MCP sin Claude
El protocolo es JSON-RPC, un mensaje por línea, por stdin y stdout. Se puede probar con un script que envíe
`initialize` → `notifications/initialized` → `tools/list` → `tools/call` y lea las respuestas.
Así se probó `mcp_sql_lectura` contra LocalDB antes de publicarlo.

## Errores típicos
| Síntoma | Causa |
|---|---|
| `failed` en `/mcp` | El programa no arranca: probar el mismo comando en una terminal |
| Se conecta pero se cae al llamar | Algo escribe en stdout (logs, `Console.WriteLine`) y rompe el protocolo |
| Claude no usa la herramienta | `Description` vaga, o hay otra herramienta que parece servir |
| Se filtró una contraseña | Estaba en `.mcp.json` o en `args`. Pasarla a una variable de entorno o a un archivo fuera del repo |
| La primera conexión falla y las siguientes no | `dotnet run` escribió mensajes de compilación en stdout. Usar `-v q --nologo` y `DOTNET_NOLOGO=1` |
| En Windows: `execvpe(/bin/bash) failed` | El comando es `bash` y Windows lanzó el de **WSL**, no Git Bash. Lanza el ejecutable directamente (`dotnet`, `node`, `python`) |
| Avisos `NU1902`/`NU1903` al compilar | Una dependencia indirecta tiene vulnerabilidades. Fíjala a la versión corregida con un `PackageReference` explícito |
