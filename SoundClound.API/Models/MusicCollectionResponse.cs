using System.Text.Json.Serialization;

namespace SoundClound.API.Models;

public class MusicCollectionResponse
{
    [JsonPropertyName("artwork_url")] public string ArtworkUrl { get; set; }
    public string Title { get; set; }
    public string Description { get; set; }
    [JsonPropertyName("track_authorization")] public string TrackAuthorization { get; set; }
    [JsonPropertyName("track_format")] public string TrackFormat { get; set; }
    public string Kind { get; set; }
    public Media Media { get; set; }
}
