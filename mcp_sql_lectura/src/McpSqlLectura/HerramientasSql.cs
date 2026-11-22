using System.ComponentModel;
using Microsoft.Data.SqlClient;
using ModelContextProtocol;
using ModelContextProtocol.Server;

namespace McpSqlLectura;

/// <summary>
/// Herramientas que ve Claude (mcp__sql-lectura__*). Las de catálogo usan SQL fijo con parámetros;
/// "consultar" pasa por el validador. Todas se ejecutan en una transacción que se deshace.
/// </summary>
[McpServerToolType]
public sealed class HerramientasSql(ConexionSql conexion)
{
    [McpServerTool(Name = "consultar", ReadOnly = true, Destructive = false)]
    [Description("Ejecuta UNA consulta SQL de solo lectura (SELECT o WITH ... SELECT) contra la base de datos configurada y devuelve " +
                 "las filas en JSON {columnas, filas, filasDevueltas, truncado}. Rechaza cualquier escritura (INSERT, UPDATE, DELETE, " +
                 "CREATE, EXEC, SELECT INTO, variables, varios lotes...). Usa TOP y filtros: el resultado se corta en max_filas.")]
    public async Task<string> Consultar(
        [Description("Consulta SQL: una sola sentencia que empiece por SELECT o WITH.")] string sql,
        [Description("Máximo de filas a devolver (por defecto el configurado, tope 5000).")] int? max_filas = null,
        CancellationToken ct = default)
    {
        var (ok, motivo) = ValidadorSql.Validar(sql);
        if (!ok) throw new McpException($"Consulta rechazada: {motivo} Este servidor es de SOLO LECTURA; los cambios se entregan como script para que los ejecute el usuario.");
        return await Ejecutar(sql, max_filas ?? conexion.MaxFilasPorDefecto, null, ct);
    }

    [McpServerTool(Name = "listar_tablas", ReadOnly = true, Destructive = false)]
    [Description("Lista tablas y vistas de la base de datos (esquema, nombre, tipo y filas aproximadas). Filtro opcional por parte del nombre.")]
    public Task<string> ListarTablas(
        [Description("Texto que debe contener el nombre (opcional).")] string? filtro = null,
        CancellationToken ct = default)
    {
        const string sql = """
            SELECT T2.name AS Esquema, T1.name AS Nombre,
                   CASE T1.type WHEN 'U' THEN 'TABLA' ELSE 'VISTA' END AS Tipo,
                   (SELECT SUM(TX1.rows) FROM sys.partitions TX1 WHERE TX1.object_id = T1.object_id AND TX1.index_id IN (0, 1)) AS FilasAprox
            FROM sys.objects T1
            INNER JOIN sys.schemas T2 ON T2.schema_id = T1.schema_id
            WHERE T1.type IN ('U', 'V') AND T1.is_ms_shipped = 0
              AND (@Filtro IS NULL OR T1.name LIKE '%' + @Filtro + '%')
            ORDER BY T2.name, T1.name
            """;
        return Ejecutar(sql, ConexionSql.TopeFilas, new Dictionary<string, object?> { ["@Filtro"] = Vacio(filtro) }, ct);
    }

    [McpServerTool(Name = "describir_tabla", ReadOnly = true, Destructive = false)]
    [Description("Columnas de una tabla o vista: nombre, tipo, longitud, si admite nulos, si es identidad y si es clave primaria. Acepta 'Tabla' o 'esquema.Tabla'.")]
    public Task<string> DescribirTabla(
        [Description("Nombre de la tabla o vista, p. ej. 'TicketSoporte' o 'dbo.TicketSoporte'.")] string tabla,
        CancellationToken ct = default)
    {
        const string sql = """
            SELECT T1.column_id AS Orden, T1.name AS Columna, T2.name AS Tipo,
                   CASE WHEN T2.name IN ('nvarchar', 'nchar') AND T1.max_length > 0 THEN T1.max_length / 2 ELSE T1.max_length END AS Longitud,
                   T1.precision AS Precision, T1.scale AS Escala, T1.is_nullable AS AdmiteNulos, T1.is_identity AS EsIdentidad,
                   CASE WHEN EXISTS (
                        SELECT 1 FROM sys.index_columns TX1
                        INNER JOIN sys.indexes TX2 ON TX2.object_id = TX1.object_id AND TX2.index_id = TX1.index_id
                        WHERE TX2.is_primary_key = 1 AND TX1.object_id = T1.object_id AND TX1.column_id = T1.column_id)
                   THEN 1 ELSE 0 END AS EsClavePrimaria
            FROM sys.columns T1
            INNER JOIN sys.types T2 ON T2.user_type_id = T1.user_type_id
            WHERE T1.object_id = OBJECT_ID(@Tabla)
            ORDER BY T1.column_id
            """;
        return Ejecutar(sql, ConexionSql.TopeFilas, new Dictionary<string, object?> { ["@Tabla"] = tabla }, ct);
    }

    [McpServerTool(Name = "ver_definicion", ReadOnly = true, Destructive = false)]
    [Description("Devuelve el código fuente de un procedimiento almacenado, vista, función o trigger (OBJECT_DEFINITION). Acepta 'Nombre' o 'esquema.Nombre'.")]
    public Task<string> VerDefinicion(
        [Description("Nombre del objeto, p. ej. 'USP_GetAll_TicketByFilter'.")] string objeto,
        CancellationToken ct = default)
    {
        const string sql = """
            SELECT T1.type_desc AS Tipo, T1.modify_date AS Modificado, OBJECT_DEFINITION(T1.object_id) AS Definicion
            FROM sys.all_objects T1
            WHERE T1.object_id = OBJECT_ID(@Objeto)
            """;
        return Ejecutar(sql, 1, new Dictionary<string, object?> { ["@Objeto"] = objeto }, ct);
    }

    [McpServerTool(Name = "buscar_en_codigo", ReadOnly = true, Destructive = false)]
    [Description("Busca un texto dentro del código de procedimientos, vistas, funciones y triggers (p. ej. qué SP usan una tabla o columna). Devuelve nombre y tipo.")]
    public Task<string> BuscarEnCodigo(
        [Description("Texto a buscar, p. ej. un nombre de tabla o columna.")] string texto,
        CancellationToken ct = default)
    {
        const string sql = """
            SELECT T3.name AS Esquema, T2.name AS Nombre, T2.type_desc AS Tipo, T2.modify_date AS Modificado
            FROM sys.sql_modules T1
            INNER JOIN sys.objects T2 ON T2.object_id = T1.object_id
            INNER JOIN sys.schemas T3 ON T3.schema_id = T2.schema_id
            WHERE T1.definition LIKE '%' + @Texto + '%'
            ORDER BY T2.type_desc, T2.name
            """;
        return Ejecutar(sql, 500, new Dictionary<string, object?> { ["@Texto"] = texto }, ct);
    }

    private async Task<string> Ejecutar(string sql, int maxFilas, IReadOnlyDictionary<string, object?>? parametros, CancellationToken ct)
    {
        try
        {
            return await conexion.EjecutarAsync(sql, maxFilas, parametros, ct);
        }
        catch (SqlException ex)
        {
            // El mensaje de SQL Server ayuda a corregir la consulta y no contiene credenciales.
            throw new McpException($"Error SQL {ex.Number}: {ex.Message}");
        }
    }

    private static string? Vacio(string? s) => string.IsNullOrWhiteSpace(s) ? null : s.Trim();
}
