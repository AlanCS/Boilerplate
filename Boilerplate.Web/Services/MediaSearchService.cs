using Boilerplate.Infrastructure;
using Boilerplate.Web.DTO;
using Boilerplate.Web.Proxy;
using System.Threading.Tasks;

namespace Boilerplate.Web.Services
{
    public interface IMediaSearchService
    {
        Task<Media> GetMovieOrTvSeries(MediaType type, string name);
    }

    public class MediaSearchService : IMediaSearchService
    {
        private readonly IMovieDatabaseProxy _movieDatabaseProxy;
        private readonly IMemoryCacheAdapter _memoryCache;

        public MediaSearchService(IMovieDatabaseProxy movieDatabaseProxy, IMemoryCacheAdapter memoryCache)
        {
            this._movieDatabaseProxy = movieDatabaseProxy;
            this._memoryCache = memoryCache;
        }

        public async Task<DTO.Media> GetMovieOrTvSeries(DTO.MediaType type, string name)
        {
            // could have used usual validators, but because this is simple, and in the spirit of no "magic code", I preferred to do manually
            if (name == null) throw new BadRequestException("name is empty", name);
            name = name.Trim();
            if (name.Length < 2) throw new BadRequestException("name has too few characters", name);

            string cacheKey = $"{type}_{name.ToLower().Replace(" ", "")}";

            var result = await _memoryCache.GetOrSetFromCache(cacheKey, async () =>
            {
                return await _movieDatabaseProxy.GetMovieOrTvSeries(type.ToString(), name);
            });

            return result;
        }
    }
}
