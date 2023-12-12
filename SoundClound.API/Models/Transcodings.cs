namespace SoundClound.API.Models;

public class Transcodings
{
    public string Url { get; set; }
    public string Preset { get; set; }
    public int Duration { get; set; }
    public bool Snipped { get; set; }
    public string Quality { get; set; }
    public Format Format { get; set; }

}
