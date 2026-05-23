namespace AI_genda_API.Contracts.AppConnections;

public class IntegrationResponse<T>
{
    public string Status { get; set; } = "success";
    public T? Data { get; set; }
    public string? Message { get; set; }

    public static IntegrationResponse<T> Success(T data) => new() { Status = "success", Data = data };

    public static IntegrationResponse<T> Error(string message) => new() { Status = "error", Message = message };
}
