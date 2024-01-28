using HNBS.Models.DTO;
using HNBS.Services.IServices;
using Newtonsoft.Json;
using StackExchange.Redis;

namespace HNBS.Services;

public class HackerNewsService : IHackerNewsService

{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IConnectionMultiplexer _redisConnection;

    public HackerNewsService(IHttpClientFactory httpClientFactory, IConnectionMultiplexer redisConnection)
    {
        _httpClientFactory = httpClientFactory;
        _redisConnection = redisConnection;
    }

    /// <summary>
    /// Method to get the number of best Hacker news stories
    /// </summary>
    /// <param name="numberOfStories"> Number of Best stories requested</param>
    /// <returns>List of best Hacker news story</returns>
    public async Task<List<HackerNewsStoryDTO>> GetHackerNewsBestStories(int numberOfStories)
    {
        try
        {
            var bestStories = new List<HackerNewsStoryDTO>();
            bestStories = await GetCachedStoriesAsync();
            if (bestStories.Count == 0)

            {
                var client = _httpClientFactory.CreateClient("HNBS");
                var response = await client.GetStringAsync("beststories.json");
                var bestStoryIds = JsonConvert.DeserializeObject<List<int>>(response);



                var tasks = bestStoryIds.Select(async bestStoryId =>
                {
                    var bestStoryResponse = await client.GetStringAsync($"item/{bestStoryId}.json");
                    var bestStory = JsonConvert.DeserializeObject<HackerNewsStoryDTO>(bestStoryResponse);
                    bestStories.Add(bestStory);
                });
                await Task.WhenAll(tasks);

                await SetCachedStoriesAsync(bestStories);


            }
            return bestStories.Take(numberOfStories).ToList();
        }
        catch (Exception e)
        {

            throw e;
        }

    }

    /// <summary>
    /// Method to list out cached best Hacker News stories (if any)
    /// </summary>
    /// <returns></returns>
    private async Task<List<HackerNewsStoryDTO>> GetCachedStoriesAsync()
    {

        var database = _redisConnection.GetDatabase();
        var value = await database.StringGetAsync("bestStories");
        var bestHNStories = new List<HackerNewsStoryDTO>();
        if (!string.IsNullOrEmpty(value))
        {
            bestHNStories = JsonConvert.DeserializeObject<List<HackerNewsStoryDTO>>(value);
        }

        return bestHNStories;
    }


    /// <summary>
    /// Method to cache the given hacker news stories
    /// </summary>
    /// <param name="hackerNewsStories"></param>
    /// <returns></returns>
    private async Task SetCachedStoriesAsync(List<HackerNewsStoryDTO> hackerNewsStories)
    {
        var database = _redisConnection.GetDatabase();
        var jsonBestHNStories = JsonConvert.SerializeObject(hackerNewsStories);
        //Considering the refresh frequency on an hourly basis.
        await database.StringSetAsync("bestStories", jsonBestHNStories, expiry: TimeSpan.FromHours(1));
    }
}