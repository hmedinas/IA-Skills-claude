using __RootNamespace__.Common.Enumeraciones;

namespace __RootNamespace__.Common.Result;

public class Response<T> where T : class
{
    public int IdResponse { get; set; }
    public string Message { get; set; } = string.Empty;
    public int TotalRows { get; set; }
    public T? Data { get; set; }
    public TypeTransaction Type { get; set; }

    public Response() { }

    public Response(TypeTransaction type) => Configure(type, Default(type));
    public Response(TypeTransaction type, string message) => Configure(type, message);
    public Response(TypeTransaction type, T data)
    {
        Configure(type, Default(type));
        Data = data;
        TotalRows = data != null ? 1 : 0;
    }
    public Response(TypeTransaction type, int totalRows, T data)
    {
        Configure(type, Default(type));
        TotalRows = totalRows;
        Data = data;
    }
    public Response(TypeTransaction type, string message, int totalRows, T data)
    {
        Configure(type, message);
        TotalRows = totalRows;
        Data = data;
    }

    private void Configure(TypeTransaction type, string message)
    {
        Type = type;
        Message = message;
        IdResponse = type switch
        {
            TypeTransaction.OK => 200,
            TypeTransaction.ERR => 400,
            TypeTransaction.NOT_FOUND => 404,
            _ => 500
        };
    }

    private static string Default(TypeTransaction type) => type switch
    {
        TypeTransaction.OK => "OK",
        TypeTransaction.ERR => "Bad Request",
        TypeTransaction.NOT_FOUND => "Not Found",
        _ => "Error"
    };
}
