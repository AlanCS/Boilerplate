using Boilerplate.Infrastructure;
using Boilerplate.Web.Extensions;
using Microsoft.Extensions.Logging;
using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace Boilerplate.Web.Proxy
{
    public interface IMovieDatabaseProxy
    {
        Task<Boilerplate.Web.DTO.Media> GetMovieOrTvSeries(string type, string movieName);
    }

    public class MovieDatabaseProxy : BaseHttpClientProxy, IMovieDatabaseProxy
    {
        public MovieDatabaseProxy(HttpClient client, ILogger<MovieDatabaseProxy> logger) : base(client)
        {


        }

        public async Task<Boilerplate.Web.DTO.Media> GetMovieOrTvSeries(string type, string name)
        {
            string route = $"?apikey={Constants.OmdbApiKey}&type={type}&t={name}";

            var result = await SendJson(HttpMethod.Get, route, (DTO.OmdbMedia externalDTO, HttpStatusCode status) =>
            {
                // any exceptions throwm in here will be wrapped inside a DownstreamException, will all the details of the request / response for investigation
                // also applies the concept of anti-corruption layer, meaning we parse the external DTO inside the proxy, and only return valid / clean internal DTOs

                if (string.IsNullOrWhiteSpace(externalDTO?.Response)) throw new Exception("DTO is invalid");

                if (!string.IsNullOrWhiteSpace(externalDTO.Error))
                {
                    // movies and tv series not found get incorrectly returned as "errors", so we filter it out here
                    if (externalDTO.Error.EndsWith("not found!", System.StringComparison.CurrentCultureIgnoreCase)) return null;

                    throw new Exception("Responsed contained error");
                }

                if (string.IsNullOrWhiteSpace(externalDTO.Title)) throw new Exception("Response didn't have title");

                if (externalDTO.Response?.ToLower() != "true") return null;

                return new Boilerplate.Web.DTO.Media()
                {
                    Id = externalDTO.ImdbId,
                    Name = externalDTO.Title.CleanNA(),
                    Year = externalDTO.Year.CleanYear(),
                    Plot = externalDTO.Plot.CleanNA(),
                    Runtime = externalDTO.Runtime.FormatDuration()
                };
            });

            return result;
        }
    }
}
