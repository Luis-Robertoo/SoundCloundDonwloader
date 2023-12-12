using Refit;
using SoundClound.API.Models;

namespace SoundClound.API.Interfaces;

public interface IAPIService
{
    [Get("/search?q={query}&client_id={clientId}&limit=20&offset=0&linked_partitioning=1&app_locale=pt_BR")]
    Task<ApiResponse<ListMusicResponse>> GetListMusic(string query, string clientId);
}


