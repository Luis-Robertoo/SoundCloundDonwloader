using SoundClound.API.DTOs;
using SoundClound.API.Models;

namespace SoundClound.API.Interfaces;

public interface IMusicService
{
    Task<ResponseDTO<List<MusicCollectionDTO>>> GetListMusic(string term);
    Task<ResponseDTO<MusicUrl>> GetUrlMusic(RequestMusicDTO request);
    Task<ResponseDTO<FileDownloaderDTO>> GetFileMusic(RequestMusicDTO request);
    Task<ResponseDTO<FileDownloaderDTO>> GetFileMusic(string term);
}
