using SoundClound.API.Helpers;

namespace SoundClound.API.DTOs;

public class RequestMusicDTO
{
    public RequestMusicDTO(string? name, string? type, string url, string trackAutorization)
    {
        Name = name.ToClean();
        Type = type;
        Url = url;
        TrackAuthorization = trackAutorization;
    }

    public RequestMusicDTO() { }

    public string? Name { get; set; }
    public string? Type { get; set; }
    public string Url { get; set; }
    public string TrackAuthorization { get; set; }

}
