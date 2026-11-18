using System.Text.Json.Serialization;

namespace Api.Staff;

public sealed record StaffMeResponse([property: JsonPropertyName("user_id")] string UserId);
