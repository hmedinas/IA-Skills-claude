using McpSqlLectura;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

// Servidor MCP (stdio) de SOLO LECTURA para SQL Server.
// stdout es del protocolo MCP: todos los logs van a stderr.
//
//   dotnet run --project src/McpSqlLectura                 → servidor MCP
//   dotnet run --project src/McpSqlLectura -- --autoprueba → prueba el validador (sin BD)

// El protocolo es JSON en UTF-8; en Windows la consola usa otra página de códigos y rompe los acentos.
Console.InputEncoding = new System.Text.UTF8Encoding(false);
Console.OutputEncoding = new System.Text.UTF8Encoding(false);

if (args.Contains("--autoprueba")) return Autoprueba.Ejecutar();

ConexionSql conexion;
try
{
    conexion = ConexionSql.DesdeEntorno();
}
catch (Exception ex)
{
    Console.Error.WriteLine($"[mcp_sql_lectura] {ex.Message}");
    return 1;
}

var builder = Host.CreateApplicationBuilder(args);
builder.Logging.ClearProviders();
builder.Logging.AddConsole(o => o.LogToStandardErrorThreshold = LogLevel.Trace);
builder.Logging.SetMinimumLevel(LogLevel.Warning);

builder.Services.AddSingleton(conexion);
builder.Services
    .AddMcpServer(o =>
    {
        o.ServerInfo = new() { Name = "sql-lectura", Version = "1.0.0" };
        o.ServerInstructions = $"Base de datos SQL Server de SOLO LECTURA ({conexion.Descripcion}). " +
                               "Usa listar_tablas, describir_tabla, ver_definicion y buscar_en_codigo para explorar, y consultar para SELECT. " +
                               "Nunca intentes modificar datos: los cambios se escriben como script para que los ejecute el usuario.";
    })
    .WithStdioServerTransport()
    .WithToolsFromAssembly();

Console.Error.WriteLine($"[mcp_sql_lectura] listo: {conexion.Descripcion} · máx. {conexion.MaxFilasPorDefecto} filas · timeout {conexion.TimeoutSegundos}s");
await builder.Build().RunAsync();
return 0;
