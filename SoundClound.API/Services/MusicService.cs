using AutoMapper;
using SoundClound.API.DTOs;
using SoundClound.API.Interfaces;
using SoundClound.API.Models;

namespace SoundClound.API.Services;

public class MusicService : IMusicService
{
    private readonly IAPIService _apiService;
    private readonly IConfiguration _configuration;
    private readonly IMapper _mapper;

    private readonly string _urlPrincipal;

    public MusicService(IAPIService apiService, IConfiguration configuration, IMapper mapper)
    {
        _apiService = apiService;
        _configuration = configuration;
        _mapper = mapper;

        _urlPrincipal = _configuration.GetValue<string>("UrlPrincipal");
    }

    public async Task<ResponseDTO<List<MusicCollectionDTO>>> GetListMusic(string term)
    {
        try
        {
            var httpClient = new HttpClient();
            var clientId = await GetClientId(httpClient);

            var musicList = await _apiService.GetListMusic(term, clientId);
            if (!musicList.IsSuccessStatusCode) throw new Exception(musicList.Error.Message);

            var response = _mapper.Map<List<MusicCollectionResponse>, List<MusicCollectionDTO>>(musicList.Content.Collection);
            response.RemoveAll(r => r.Kind != "track");

            return new ResponseDTO<List<MusicCollectionDTO>>(response, null);

        }
        catch (Exception ex)
        {
            return new ResponseDTO<List<MusicCollectionDTO>>(null, ex.Message);
        }
    }

    public async Task<ResponseDTO<MusicUrl>> GetUrlMusic(RequestMusicDTO request)
    {
        try
        {
            var httpClient = new HttpClient();
            var clientId = await GetClientId(httpClient);

            var musicUrl = await httpClient.GetFromJsonAsync<MusicUrl>($"{request.Url}?client_id={clientId}&track_authorization={request.TrackAuthorization}");

            return new ResponseDTO<MusicUrl>(musicUrl, null);
        }
        catch (Exception ex)
        {
            return new ResponseDTO<MusicUrl>(null, ex.Message);
        }
    }

    public async Task<ResponseDTO<FileDownloaderDTO>> GetFileMusic(RequestMusicDTO request)
    {
        try
        {
            var httpClient = new HttpClient();
            var clientId = await GetClientId(httpClient);

            var musicUrl = await httpClient.GetFromJsonAsync<MusicUrl>($"{request.Url}?client_id={clientId}&track_authorization={request.TrackAuthorization}");
            var bytes = await httpClient.GetByteArrayAsync(musicUrl.Url);

            var file = new FileDownloaderDTO { Bytes = bytes, Name = request.Name, Type = request.Type };

            return new ResponseDTO<FileDownloaderDTO>(file, null);
        }
        catch (Exception ex)
        {
            return new ResponseDTO<FileDownloaderDTO>(null, ex.Message);
        }
    }

    public async Task<ResponseDTO<FileDownloaderDTO>> GetFileMusic(string term)
    {
        try
        {
            var listMusic = await GetListMusic(term);
            var music = listMusic.Data.FirstOrDefault();
            var type = music.Media.Transcodings.FirstOrDefault(t => t.Format.Protocol == "progressive").Format.MimeType;
            var url = music.Media.Transcodings.FirstOrDefault(t => t.Format.Protocol == "progressive").Url;
            var request = new RequestMusicDTO { Name = music.Title, Type = type, TrackAuthorization = music.TrackAuthorization, Url = url };

            return await GetFileMusic(request);

        }
        catch (Exception ex)
        {
            return new ResponseDTO<FileDownloaderDTO>(null, ex.Message);
        }
    }

    private async Task<string>? GetPagePrincipal(HttpClient httpClient, string url)
    {
        var response = await httpClient.GetAsync(url);

        if (!response.IsSuccessStatusCode)
            return null;

        var pagePrincipal = response.Content.ReadAsStringAsync().Result;

        return pagePrincipal;
    }

    private async Task<string> GetClientId(HttpClient httpClient)
    {
        var pagePrincipal = await GetPagePrincipal(httpClient, _urlPrincipal);

        var comecoLinks = pagePrincipal.IndexOf("<script src=\"https://a-v2.sndcdn.com");
        var links = pagePrincipal.Substring(comecoLinks, pagePrincipal.Length - comecoLinks);
        var listaDeLinks = pagePrincipal.Substring(comecoLinks, pagePrincipal.Length - comecoLinks).Split("</script>\n<script").ToList().Select(src =>
        {
            if (src.Contains("<script src=\"https")) return string.Empty;
            string nova = src.Replace(" crossorigin src=\"", "").Replace("\">", "").Replace("</script>\n</body>\n</html>", "").Replace("\n", "");
            return nova;
        });

        var clientId = string.Empty;

        foreach (var link in listaDeLinks)
        {
            if (link.Equals("")) continue;

            using HttpResponseMessage js = await httpClient.GetAsync(link);
            var texto = js.Content.ReadAsStringAsync().Result;
            var comecoLink = texto.IndexOf("{client_id:\"");
            var fimLink = texto.IndexOf("\",nonce:e.nonce}))");

            if (comecoLink == -1) continue;

            clientId = texto.Substring(comecoLink + 12, fimLink - (comecoLink + 12));
            if (clientId.Length == 32) break;
        }

        return clientId;
    }
}
