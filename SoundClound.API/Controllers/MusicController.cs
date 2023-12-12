using Microsoft.AspNetCore.Mvc;
using SoundClound.API.DTOs;
using SoundClound.API.Interfaces;

namespace SoundClound.API.Controllers;

[ApiController]
[Route("[controller]")]
public class MusicController : ControllerBase
{
    private readonly IMusicService _musicService;
    public MusicController(IMusicService musicService)
    {
        _musicService = musicService;
    }

    [HttpGet]
    public async Task<IActionResult> GetListMusic([FromQuery] string nameMusic)
    {
        var response = await _musicService.GetListMusic(nameMusic);
        return Ok(response);
    }

    [HttpGet("download")]
    public async Task<IActionResult> DonwloadMusic([FromQuery] string nameMusic)
    {
        var response = await _musicService.GetFileMusic(nameMusic);
        if (response.Error != null) return BadRequest(response.Error);

        return File(response.Data.Bytes, response.Data.Type, response.Data.Name);
    }

    [HttpPost]
    public async Task<IActionResult> GetUrlMusic([FromBody] RequestMusicDTO dto)
    {
        var response = await _musicService.GetUrlMusic(dto);
        return Ok(response);
    }

    [HttpPost("download")]
    public async Task<IActionResult> DonwloadMusic([FromBody] RequestMusicDTO dto)
    {
        var response = await _musicService.GetFileMusic(dto);
        if (response.Error != null) return BadRequest(response.Error);

        return File(response.Data.Bytes, response.Data.Type, response.Data.Name);
    }
}
