using HNBS.Models.DTO;

namespace HNBS.Services.IServices;

public interface IHackerNewsService
{
    public Task<List<HackerNewsStoryDTO>> GetHackerNewsBestStories(int numberOfStories);
}
