using Boilerplate.Web.Controllers;
using Boilerplate.Web.DTO;
using Boilerplate.Web.Services;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace MovieProject.Web.Controllers
{
    [Route("api")]
    public class SearchController : ControllerRoot
    {
        private IMediaSearchService _searchService { get; }

        public SearchController(IMediaSearchService searchService)
        {
            _searchService = searchService;
        }

        [HttpGet("{type}/{name}")]
        public async Task<ActionResult<Media>> Get(MediaType type, string name)
        {
            var media = await _searchService.GetMovieOrTvSeries(type, name);

            if (media == null) return NotFound($"Search terms didn't match any {type}");

            return Ok(media);
        }      
    }
}
