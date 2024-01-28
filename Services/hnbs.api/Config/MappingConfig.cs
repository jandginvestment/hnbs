using AutoMapper;
using HNBS.Models;
using HNBS.Models.DTO;

namespace HNBS.Config;

public class MappingConfig
{
    public static MapperConfiguration RegisterMaps()
    {
        var mappingConfig = new MapperConfiguration(config =>
        {
            config.CreateMap<HackerNewsStory, HackerNewsStoryDTO>().ReverseMap();
        }); return mappingConfig;
    }
}
