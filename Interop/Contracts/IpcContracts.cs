using System.Text.Json;
using System.Text.Json.Serialization;

namespace DemoPulse.Interop.Contracts
{
    public class IpcRequest
    {
        [JsonPropertyName("id")]
        public string? Id { get; set; }

        [JsonPropertyName("command")]
        public string Command { get; set; } = string.Empty;

        [JsonPropertyName("payload")]
        public JsonElement Payload { get; set; }
    }

    public class IpcResponse
    {
        [JsonPropertyName("id")]
        public string? Id { get; set; }

        [JsonPropertyName("type")]
        public string Type { get; set; } = string.Empty;

        [JsonPropertyName("success")]
        public bool Success { get; set; } = true;

        [JsonPropertyName("payload")]
        public object? Payload { get; set; }

        [JsonPropertyName("error")]
        public string? Error { get; set; }

        public static IpcResponse Ok(string? id, string type, object? payload = null)
        {
            return new IpcResponse { Id = id, Type = type, Success = true, Payload = payload };
        }

        public static IpcResponse Fail(string? id, string type, string error)
        {
            return new IpcResponse { Id = id, Type = type, Success = false, Error = error };
        }
    }
}
