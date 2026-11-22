using System.Text;
using System.Text.RegularExpressions;

namespace McpSqlLectura;

/// <summary>
/// Primera barrera: decide si un texto SQL es UNA consulta de solo lectura.
/// Quita comentarios, cadenas e identificadores entrecomillados (pueden contener cualquier palabra)
/// y analiza lo que queda: debe empezar por SELECT o WITH, ser una sola sentencia y no contener
/// ninguna palabra de escritura, ejecución o administración.
/// No es la única barrera: además se ejecuta dentro de una transacción que siempre se deshace,
/// con ApplicationIntent=ReadOnly, y lo correcto es usar un login con permisos solo de lectura.
/// </summary>
public static partial class ValidadorSql
{
    private static readonly HashSet<string> Prohibidas = new(StringComparer.OrdinalIgnoreCase)
    {
        // Escritura de datos y esquema
        // (REPLACE no: en T-SQL es una función de texto)
        "INSERT", "UPDATE", "DELETE", "MERGE", "TRUNCATE", "CREATE", "ALTER", "DROP", "INTO",
        // Permisos y administración
        "GRANT", "REVOKE", "DENY", "BACKUP", "RESTORE", "DBCC", "SHUTDOWN", "KILL", "RECONFIGURE",
        "CHECKPOINT", "USE", "SETUSER", "ATTACH", "DETACH",
        // Ejecución de código y acceso externo
        "EXEC", "EXECUTE", "SP_EXECUTESQL", "BULK", "OPENROWSET", "OPENDATASOURCE", "OPENQUERY", "OPENXML",
        // Control de flujo / lotes (una consulta no los necesita).
        // (FETCH no: OFFSET ... FETCH NEXT es paginación; un FETCH de cursor ya exige DECLARE, que está bloqueado)
        "DECLARE", "SET", "BEGIN", "COMMIT", "ROLLBACK", "SAVE", "TRANSACTION", "TRAN", "WAITFOR", "GO",
        "WHILE", "GOTO", "RETURN", "RAISERROR", "THROW", "PRINT", "CURSOR", "OPEN", "CLOSE", "DEALLOCATE",
        "LOAD", "WRITETEXT", "UPDATETEXT", "READTEXT", "ENABLE", "DISABLE",
    };

    public static (bool Ok, string Motivo) Validar(string? sql)
    {
        if (string.IsNullOrWhiteSpace(sql)) return (false, "la consulta está vacía.");
        if (sql.Length > 20_000) return (false, "la consulta es demasiado larga (máximo 20.000 caracteres).");

        var (esqueleto, error) = Esqueleto(sql);
        if (error is not null) return (false, error);

        var cuerpo = esqueleto.Trim().TrimEnd(';').Trim();
        if (cuerpo.Contains(';')) return (false, "solo se permite UNA sentencia (hay más de un ';').");

        var palabras = Palabra().Matches(cuerpo).Select(m => m.Value).ToList();
        if (palabras.Count == 0) return (false, "no se encontró ninguna instrucción.");

        var primera = palabras[0].ToUpperInvariant();
        if (primera is not ("SELECT" or "WITH"))
            return (false, $"solo se permiten consultas que empiezan por SELECT o WITH (empieza por '{palabras[0]}').");

        foreach (var p in palabras)
        {
            if (Prohibidas.Contains(p)) return (false, $"contiene '{p.ToUpperInvariant()}', que no está permitido en modo solo lectura.");
            if (p.StartsWith("sp_", StringComparison.OrdinalIgnoreCase) || p.StartsWith("xp_", StringComparison.OrdinalIgnoreCase))
                return (false, $"llama a '{p}': no se permiten procedimientos del sistema.");
        }
        if (cuerpo.Contains('@')) return (false, "no se permiten variables (@) ni funciones del sistema con @@.");

        return (true, "");
    }

    /// <summary>
    /// Devuelve el SQL con comentarios, cadenas ('...', N'...') e identificadores ([...] y "...")
    /// sustituidos por espacios o por un marcador neutro, para que su contenido no cuente.
    /// </summary>
    internal static (string Esqueleto, string? Error) Esqueleto(string sql)
    {
        var sb = new StringBuilder(sql.Length);
        var i = 0;
        while (i < sql.Length)
        {
            var c = sql[i];
            var sig = i + 1 < sql.Length ? sql[i + 1] : '\0';

            if (c == '-' && sig == '-')
            {   // comentario de línea
                while (i < sql.Length && sql[i] != '\n') i++;
                sb.Append(' ');
            }
            else if (c == '/' && sig == '*')
            {   // comentario de bloque (SQL Server admite anidados)
                var nivel = 0;
                do
                {
                    if (i + 1 < sql.Length && sql[i] == '/' && sql[i + 1] == '*') { nivel++; i += 2; }
                    else if (i + 1 < sql.Length && sql[i] == '*' && sql[i + 1] == '/') { nivel--; i += 2; }
                    else i++;
                } while (nivel > 0 && i < sql.Length);
                if (nivel > 0) return ("", "hay un comentario /* sin cerrar.");
                sb.Append(' ');
            }
            else if (c == '\'')
            {   // cadena; '' es una comilla escapada
                i++;
                var cerrada = false;
                while (i < sql.Length)
                {
                    if (sql[i] == '\'' && i + 1 < sql.Length && sql[i + 1] == '\'') { i += 2; continue; }
                    if (sql[i] == '\'') { i++; cerrada = true; break; }
                    i++;
                }
                if (!cerrada) return ("", "hay una cadena ' sin cerrar.");
                sb.Append(" 'x' ");
            }
            else if (c == '[')
            {   // identificador entre corchetes; ]] es un ] escapado
                i++;
                var cerrado = false;
                while (i < sql.Length)
                {
                    if (sql[i] == ']' && i + 1 < sql.Length && sql[i + 1] == ']') { i += 2; continue; }
                    if (sql[i] == ']') { i++; cerrado = true; break; }
                    i++;
                }
                if (!cerrado) return ("", "hay un identificador [ sin cerrar.");
                sb.Append(" ident ");
            }
            else if (c == '"')
            {   // identificador entre comillas dobles
                i++;
                var cerrado = false;
                while (i < sql.Length)
                {
                    if (sql[i] == '"' && i + 1 < sql.Length && sql[i + 1] == '"') { i += 2; continue; }
                    if (sql[i] == '"') { i++; cerrado = true; break; }
                    i++;
                }
                if (!cerrado) return ("", "hay un identificador \" sin cerrar.");
                sb.Append(" ident ");
            }
            else
            {
                sb.Append(c);
                i++;
            }
        }
        return (sb.ToString(), null);
    }

    [GeneratedRegex(@"[A-Za-z_#][A-Za-z0-9_#$]*")]
    private static partial Regex Palabra();
}
