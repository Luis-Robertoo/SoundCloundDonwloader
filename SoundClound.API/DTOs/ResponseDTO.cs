namespace SoundClound.API.DTOs;

public class ResponseDTO<T>
{
    public ResponseDTO(T? data, string? error)
    {
        Data = data;
        Error = error;
    }

    public T? Data { get; set; }
    public string? Error { get; set; }

}
