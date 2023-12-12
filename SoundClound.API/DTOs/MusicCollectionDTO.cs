using SoundClound.API.Models;

namespace SoundClound.API.DTOs;

public class MusicCollectionDTO
{
    public string ArtworkUrl { get; set; }
    public string Title { get; set; }
    public string Description { get; set; }
    public string TrackAuthorization { get; set; }
    public string TrackFormat { get; set; }
    public string Kind { get; set; }
    public Media Media { get; set; }
}
