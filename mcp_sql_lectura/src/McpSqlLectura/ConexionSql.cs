using System.Data;
using System.Text.Json;
using Microsoft.Data.SqlClient;

namespace McpSqlLectura;

/// <summary>
/// Configuración y ejecución. La cadena de conexión se lee de un ARCHIVO fuera del repositorio
/// (MCP_SQL_CONEXION_ARCHIVO) para que la contraseña nunca pase por el modelo ni por git.
/// Toda consulta se ejecuta dentro de una transacción que SIEMPRE se deshace (segunda barrera),
/// en READ UNCOMMITTED para no bloquear a nadie en producción, con timeout y límite de filas.
/// </summary>
public sealed class ConexionSql
{
    public const int TopeFilas = 5000;

    public string CadenaConexion { get; }
    public int MaxFilasPorDefecto { get; }
    public int TimeoutSegundos { get; }
    public string Descripcion { get; }

    private ConexionSql(string cadena, int maxFilas, int timeout)
    {
        var b = new SqlConnectionStringBuilder(cadena)
        {
            ApplicationIntent = ApplicationIntent.ReadOnly, // tercera barrera (réplicas de solo lectura / intención declarada)
            ApplicationName = "mcp_sql_lectura",
        };
        CadenaConexion = b.ConnectionString;
        MaxFilasPorDefecto = maxFilas;
        TimeoutSegundos = timeout;
        Descripcion = $"{b.DataSource} / {b.InitialCatalog}"; // sin usuario ni contraseña
    }

    public static ConexionSql DesdeEntorno()
    {
        string? cadena = null;
        var archivo = Environment.GetEnvironmentVariable("MCP_SQL_CONEXION_ARCHIVO");
        if (!string.IsNullOrWhiteSpace(archivo))
        {
            if (!File.Exists(archivo))
                throw new InvalidOperationException($"No existe el archivo de conexión indicado en MCP_SQL_CONEXION_ARCHIVO: {archivo}");
            cadena = File.ReadAllLines(archivo)
                .Select(l => l.Trim())
                .FirstOrDefault(l => l.Length > 0 && !l.StartsWith('#'));
        }
        cadena ??= Environment.GetEnvironmentVariable("MCP_SQL_CONEXION");
        if (string.IsNullOrWhiteSpace(cadena))
            throw new InvalidOperationException("Falta la conexión: define MCP_SQL_CONEXION_ARCHIVO (recomendado) o MCP_SQL_CONEXION.");

        var maxFilas = Leer("MCP_SQL_MAX_FILAS", 200, 1, TopeFilas);
        var timeout = Leer("MCP_SQL_TIMEOUT", 30, 1, 300);
        return new ConexionSql(cadena, maxFilas, timeout);
    }

    private static int Leer(string variable, int porDefecto, int min, int max) =>
        int.TryParse(Environment.GetEnvironmentVariable(variable), out var v) ? Math.Clamp(v, min, max) : porDefecto;

    /// <summary>Ejecuta una consulta y devuelve JSON compacto: columnas, filas (arrays) y si se truncó.</summary>
    public async Task<string> EjecutarAsync(string sql, int maxFilas, IReadOnlyDictionary<string, object?>? parametros, CancellationToken ct)
    {
        maxFilas = Math.Clamp(maxFilas, 1, TopeFilas);
        await using var cn = new SqlConnection(CadenaConexion);
        await cn.OpenAsync(ct);
        await using var tx = (SqlTransaction)await cn.BeginTransactionAsync(IsolationLevel.ReadUncommitted, ct);
        try
        {
            await using var cmd = new SqlCommand(sql, cn, tx) { CommandTimeout = TimeoutSegundos };
            if (parametros is not null)
                foreach (var (nombre, valor) in parametros) cmd.Parameters.AddWithValue(nombre, valor ?? DBNull.Value);

            await using var rd = await cmd.ExecuteReaderAsync(ct);
            var columnas = Enumerable.Range(0, rd.FieldCount).Select(rd.GetName).ToArray();
            var filas = new List<object?[]>();
            var truncado = false;
            while (await rd.ReadAsync(ct))
            {
                if (filas.Count >= maxFilas) { truncado = true; cmd.Cancel(); break; }
                var fila = new object?[rd.FieldCount];
                for (var c = 0; c < rd.FieldCount; c++) fila[c] = Normalizar(rd.IsDBNull(c) ? null : rd.GetValue(c));
                filas.Add(fila);
            }
            return JsonSerializer.Serialize(new
            {
                columnas,
                filas,
                filasDevueltas = filas.Count,
                truncado,
                aviso = truncado ? $"Resultado cortado en {maxFilas} filas: usa TOP, filtros o max_filas." : null,
            });
        }
        finally
        {
            // Segunda barrera: pase lo que pase, nada de lo ejecutado se confirma.
            try { await tx.RollbackAsync(CancellationToken.None); } catch { /* conexión ya cerrada o transacción terminada */ }
        }
    }

    private static object? Normalizar(object? v) => v switch
    {
        null => null,
        DateTime d => d.ToString("yyyy-MM-ddTHH:mm:ss.fff"),
        DateTimeOffset d => d.ToString("yyyy-MM-ddTHH:mm:ss.fffzzz"),
        TimeSpan t => t.ToString(),
        byte[] b => $"<binario {b.Length} bytes>",
        Guid g => g.ToString(),
        string or bool or byte or short or int or long or decimal or double or float => v,
        _ => v.ToString(),
    };
}
