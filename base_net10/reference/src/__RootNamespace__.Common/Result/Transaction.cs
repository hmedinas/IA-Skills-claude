using __RootNamespace__.Common.Enumeraciones;

namespace __RootNamespace__.Common.Result;

public class Transaction
{
    public TypeTransaction Type { get; set; }
    public string? Message { get; set; }
    public string? Key { get; set; }
    public dynamic? Output { get; set; }

    public Transaction() { }
    public Transaction(TypeTransaction type, string? message)
    { Type = type; Message = message; }
    public Transaction(TypeTransaction type, string? message, string? key)
    { Type = type; Message = message; Key = key; }
    public Transaction(TypeTransaction type, string? message, string? key, dynamic? output)
    { Type = type; Message = message; Key = key; Output = output; }
}
