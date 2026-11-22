namespace McpSqlLectura;

/// <summary>Casos del validador. Se ejecutan con: dotnet run -- --autoprueba (no necesita BD).</summary>
public static class Autoprueba
{
    private static readonly (string Sql, bool Permitida)[] Casos =
    [
        // Permitidas
        ("SELECT 1", true),
        ("select top 10 T1.IdTicket, T1.Titulo from dbo.TicketSoporte T1 with (nolock) where T1.IdEstado = 'ACT';", true),
        ("WITH X AS (SELECT T1.Id FROM T T1) SELECT * FROM X", true),
        ("SELECT T1.FechaUpdate, T1.UsuarioCreacion FROM T T1", true),               // palabras dentro de identificadores
        ("SELECT [Update], \"Delete\" FROM T", true),                                 // identificadores entrecomillados
        ("SELECT * FROM Log T1 WHERE T1.Accion = 'DELETE; DROP TABLE x'", true),      // dentro de cadena
        ("SELECT 1 -- DELETE FROM T\n", true),                                        // dentro de comentario
        ("SELECT /* DROP /* anidado */ TABLE */ 1", true),
        ("SELECT N'O''Brien'", true),
        ("SELECT COUNT(*) FROM T GROUP BY a HAVING COUNT(*) > 1 ORDER BY 1 OFFSET 0 ROWS FETCH NEXT 5 ROWS ONLY", true), // paginación
        ("SELECT REPLACE(T1.Titulo, 'a', 'b') FROM T T1", true),                    // REPLACE es función de texto
        // Rechazadas
        ("", false),
        ("DELETE FROM T", false),
        ("UPDATE T SET a = 1", false),
        ("SELECT * INTO #tmp FROM T", false),
        ("SELECT 1; DELETE FROM T", false),
        ("SELECT 1; SELECT 2", false),
        ("EXEC USP_Algo", false),
        ("WITH X AS (SELECT 1 a) DELETE FROM X", false),
        ("SELECT * FROM OPENROWSET('SQLNCLI', 'x', 'SELECT 1')", false),
        ("SELECT dbo.fn(1); EXEC sp_configure", false),
        ("SELECT * FROM sys.objects; EXEC xp_cmdshell 'dir'", false),
        ("SELECT xp_cmdshell", false),
        ("SELECT @@VERSION", false),
        ("DECLARE @a INT SELECT @a", false),
        ("SELECT 1 /* sin cerrar", false),
        ("SELECT 'sin cerrar", false),
        ("WAITFOR DELAY '00:00:10'", false),
        ("SELECT 1\nGO\nDROP TABLE T", false),
        ("BEGIN TRAN SELECT 1", false),
        ("TRUNCATE TABLE T", false),
        ("MERGE T USING S ON 1=1 WHEN MATCHED THEN DELETE;", false),
    ];

    public static int Ejecutar()
    {
        var fallos = 0;
        foreach (var (sql, permitida) in Casos)
        {
            var (ok, motivo) = ValidadorSql.Validar(sql);
            var bien = ok == permitida;
            if (!bien) fallos++;
            Console.WriteLine($"{(bien ? "OK " : "XX ")} {(ok ? "permitida" : "rechazada"),-9}  {Recortar(sql)}{(ok ? "" : $"  → {motivo}")}");
        }
        Console.WriteLine(fallos == 0 ? $"== {Casos.Length} casos correctos" : $"== {fallos} caso(s) FALLAN");
        return fallos == 0 ? 0 : 1;
    }

    private static string Recortar(string s) => (s.Replace('\n', ' ') is var t && t.Length > 70) ? t[..70] + "…" : s.Replace('\n', ' ');
}
