using LTMPorjectTes.Interface;
using LTMPorjectTes.Models;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using System.Collections.Concurrent;

namespace LTMPorjectTes.Implimentaion
{
    public class Services : IService
    {
        private readonly HttpClient _httpClient;
        private readonly Settings _settings;
        private readonly IMemoryCache _cache;

        private const int MaxConcurrentRequests = 10;

        public Services(HttpClient httpClient,IOptions<Settings> settings,IMemoryCache cache)
        {
            _httpClient = httpClient;
            _settings = settings.Value;
            _cache = cache;
        }

        public async Task<List<ResponseModels>> GetBestStoriesAsync(int count)
        {
            try
            {

                var storyIds = await GetBestStoryIdsAsync();
                if (storyIds.Count == 0)
                {
                    return new List<ResponseModels>();
                }
                var stories = await GetStoriesConcurrentlyAsync(storyIds);
                var validStories = stories
                    .Where(x => x != null)
                    .Cast<HackerNewsStoryModel>().ToList();

                var topStories = validStories
                    .OrderByDescending(x => x.Score)
                    .Take(count).ToList();

                var response = new List<ResponseModels>();
                foreach (var story in topStories)
                {
                    var mappedStory =MapToResponseModel(story);
                    response.Add(mappedStory);
                }

                return response;
            }
            
            catch (HttpRequestException ex)
            {
                throw new Exception("error API.", ex);
            }
            
            catch (Exception ex)
            {
                throw new Exception("An error while retrieving stories.", ex);
            }
        }

        private async Task<List<int>> GetBestStoryIdsAsync()
        {
            try
            {
                var cacheKey = "BestStoryIds";
                if (_cache.TryGetValue(cacheKey,out List<int>? cachedIds))
                {
                    if (cachedIds != null)
                    {
                        return cachedIds;
                    }
                }
               
                var url =$"{_settings.BaseUrl}" +$"{_settings.BestStoriesEndpoint}";
                var storyIds =
                    await _httpClient.GetFromJsonAsync<List<int>>(url);

                var result = new List<int>();

                if (storyIds != null)
                {
                    foreach (var storyId in storyIds)
                    {
                        result.Add(storyId);
                    }
                }

                _cache.Set(cacheKey,result,TimeSpan.FromSeconds(30));

                return result;
            }
            
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private async Task<List<HackerNewsStoryModel?>>GetStoriesConcurrentlyAsync(List<int> storyIds)
        {
            try
            {
                var stories = new ConcurrentBag<HackerNewsStoryModel>();
                var parallelOptions = new ParallelOptions();
                parallelOptions.MaxDegreeOfParallelism = MaxConcurrentRequests;
                await Parallel.ForEachAsync(storyIds,parallelOptions,async (storyId, cancellationToken) =>
                    {
                        try
                        {
                            var story =await GetStoryByIdAsync(storyId, cancellationToken);
                            if (story != null)
                            {
                                stories.Add(story);
                            }
                        }
                        
                        catch (Exception ex)
                        {
                            throw new Exception("error" + ex);
                        }
                    });

                var result = new List<HackerNewsStoryModel?>();

                foreach (var story in stories)
                {
                    result.Add(story);
                }

                return result;
            }
            catch (Exception ex)
            {
                throw new Exception(" best stories.",ex);
            }
        }

        private async Task<HackerNewsStoryModel?> GetStoryByIdAsync(int storyId,CancellationToken cancellationToken)
        {
            try
            {
                var cacheKey =  $"Story_{storyId}";

                if (_cache.TryGetValue(cacheKey, out HackerNewsStoryModel? cachedStory))
                {
                    if (cachedStory != null)
                    {
                        return cachedStory;
                    }
                }

                
                var endpoint = string.Format(_settings.ItemEndpoint,storyId);

                var url = $"{_settings.BaseUrl}{endpoint}";

               
                var story =await _httpClient.GetFromJsonAsync<HackerNewsStoryModel>( url, cancellationToken);

                if (story != null)
                {
                   
                    _cache.Set( cacheKey, story, TimeSpan.FromMinutes(10));
                }

                return story;
            }
          
            catch (Exception ex)
            {
                throw new Exception($"Error on story with ID {storyId}.",ex);
            }
        }

        private ResponseModels MapToResponseModel( HackerNewsStoryModel story)
        {
            try
            {
                var response =  new ResponseModels();
                response.Title = story.Title;
                if (!string.IsNullOrEmpty(story.Url))
                {
                    response.Uri = story.Url;
                }
                else
                {
                    response.Uri =string.Format(_settings.Fallback, story.Id);
                }
                response.PostedBy =story.By;
                response.Time =  DateTimeOffset .FromUnixTimeSeconds(story.Time) .UtcDateTime;
                response.Score =story.Score;
                response.CommentCount =  story.Descendants;
                return response;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
    }
}