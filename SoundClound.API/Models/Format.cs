using System.Text.Json.Serialization;

namespace SoundClound.API.Models;

public class Format
{
    public string Protocol { get; set; }
    [JsonPropertyName("mime_type")] public string MimeType { get; set; }
}
