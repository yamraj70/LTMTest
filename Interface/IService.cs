using LTMPorjectTes.Models;

namespace LTMPorjectTes.Interface
{
    public interface IService
    {
        Task<List<ResponseModels>> GetBestStoriesAsync(int count);
    }
}
