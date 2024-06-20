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
    private readonly HttpClient _httpClient;

    private readonly string _urlPrincipal;
    private readonly string _urlAPI;

    public MusicService(IAPIService apiService, IConfiguration configuration, IMapper mapper)
    {
        _apiService = apiService;
        _configuration = configuration;
        _mapper = mapper;

        _urlPrincipal = _configuration.GetValue<string>("UrlPrincipal");
        _urlAPI = _configuration.GetValue<string>("UrlApi2");

        _httpClient = new HttpClient();
    }

    public async Task<ResponseDTO<List<MusicCollectionDTO>>> GetListMusic(string term)
    {
        try
        {
            var clientId = await GetClientId();

            var urlMusica = $"{_urlAPI}/search?q={term}&client_id={clientId}&limit=20&offset=0&linked_partitioning=1&app_locale=pt_BR";

            var musicList = await _httpClient.GetFromJsonAsync<ListMusicResponse>(urlMusica);
            if (musicList is null) throw new Exception("Erro ao obter lista de musicas");

            var response = _mapper.Map<List<MusicCollectionResponse>, List<MusicCollectionDTO>>(musicList.Collection);
            response.RemoveAll(r => r.Kind != "track");

            return new ResponseDTO<List<MusicCollectionDTO>>(response, null);

        }
        catch (Exception ex)
        {
            throw;
        }
    }

    public async Task<ResponseDTO<MusicUrl>> GetUrlMusic(RequestMusicDTO request)
    {
        try
        {
            if (request.Type != "progressive") return new ResponseDTO<MusicUrl>(null, "Protocolo indisponivel.");

            var clientId = await GetClientId();

            var musicUrl = await _httpClient.GetFromJsonAsync<MusicUrl>($"{request.Url}?client_id={clientId}&track_authorization={request.TrackAuthorization}");

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
            var clientId = await GetClientId();

            var musicUrl = await _httpClient.GetFromJsonAsync<MusicUrl>($"{request.Url}?client_id={clientId}&track_authorization={request.TrackAuthorization}");

            var bytes = await _httpClient.GetByteArrayAsync(musicUrl.Url);

            var file = new FileDownloaderDTO { Bytes = bytes, Name = request.Name, Type = request.Type };

            return new ResponseDTO<FileDownloaderDTO>(file, null);
        }
        catch (Exception ex)
        {
            return new ResponseDTO<FileDownloaderDTO>(null, ex.Message);
        }
    }

    public async Task<ResponseDTO<FileDownloaderDTO>> GetFileMusicHLS(RequestMusicDTO request)
    {
        try
        {
            var clientId = await GetClientId();

            var url = $"{request.Url}?client_id={clientId}&track_authorization={request.TrackAuthorization}";

            var musicUrl = await _httpClient.GetFromJsonAsync<MusicUrl>($"{request.Url}?client_id={clientId}&track_authorization={request.TrackAuthorization}");

            var playList = await _httpClient.GetStringAsync(musicUrl.Url);
            var linksTrechos = ExtrairLinksDosTrechos(playList);
            
            var bytes = await DownloadTrechos(linksTrechos);
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
            RequestMusicDTO request = null;

            var listMusic = await GetListMusic(term);
            var music = listMusic.Data.FirstOrDefault();

            var type = music.Media.Transcodings.FirstOrDefault(t => t.Format.Protocol == "progressive")?.Format.MimeType;
            var url = music.Media.Transcodings.FirstOrDefault(t => t.Format.Protocol == "progressive")?.Url;

            if (url is null || type is null)
            {
                type = music.Media.Transcodings.FirstOrDefault(t => t.Format.Protocol == "hls")?.Format.MimeType;
                url = music.Media.Transcodings.FirstOrDefault(t => t.Format.Protocol == "hls")?.Url;

                request = new RequestMusicDTO(music.Title, type, url, music.TrackAuthorization );

                return await GetFileMusicHLS(request);
            }

            request = new RequestMusicDTO(music.Title, type, url, music.TrackAuthorization);

            return await GetFileMusic(request);

        }
        catch (Exception ex)
        {
            return new ResponseDTO<FileDownloaderDTO>(null, ex.Message);
        }
    }

    private async Task<string>? GetPagePrincipal(string url)
    {
        var response = await _httpClient.GetAsync(url);

        if (!response.IsSuccessStatusCode)
            return null;

        var pagePrincipal = response.Content.ReadAsStringAsync().Result;

        return pagePrincipal;
    }

    private async Task<string> GetClientId()
    {
        var pagePrincipal = await GetPagePrincipal(_urlPrincipal);

        var comecoLinks = pagePrincipal.IndexOf("<script src=\"https://a-v2.sndcdn.com");
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

            using HttpResponseMessage js = await _httpClient.GetAsync(link);
            var texto = js.Content.ReadAsStringAsync().Result;
            var comecoLink = texto.IndexOf("{client_id:\"");
            var fimLink = texto.IndexOf("\",nonce:e.nonce}))");

            if (comecoLink == -1) continue;

            clientId = texto.Substring(comecoLink + 12, fimLink - (comecoLink + 12));
            if (clientId.Length == 32) break;
        }

        return clientId;
    }

    private static IEnumerable<string>? ExtrairLinksDosTrechos(string playlist)
    {
        if (playlist is null) return null;

        var trechos = playlist.Split("#EXTINF").ToList().Select(trecho =>
        {
            var comecoLink = trecho.IndexOf("h");

            if (comecoLink == -1) return string.Empty;

            return trecho.Substring(comecoLink, trecho.Length - comecoLink).Replace("\n#EXT-X-ENDLIST", "");
        });

        return trechos.Where(trecho => trecho != string.Empty).ToList();
    }

    private async Task<byte[]> DownloadTrechos(IEnumerable<string> linkTrechos)
    {
        using var stream = new MemoryStream();

        foreach (var trecho in linkTrechos)
        {
            var dados = await _httpClient.GetByteArrayAsync(trecho);
            if(dados is null) continue;
            stream.Write(dados);
        }        

        return stream.ToArray();
    }
}
