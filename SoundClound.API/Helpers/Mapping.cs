using AutoMapper;
using SoundClound.API.DTOs;
using SoundClound.API.Models;

namespace SoundClound.API.Helpers;

public class Mapping : Profile
{
    public Mapping()
    {
        CreateMap<MusicCollectionResponse, MusicCollectionDTO>();
    }
}
