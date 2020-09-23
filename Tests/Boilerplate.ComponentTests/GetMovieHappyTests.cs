using Boilerplate.ComponentTests.Setup;
using Boilerplate.Infrastructure.Exceptions;
using FluentAssertions;
using FluentAssertions.Execution;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using SystemTestingTools;
using Xunit;

namespace Boilerplate.ComponentTests
{
    [Collection("SharedServer collection")]
    [Trait("Project", "MovieProject Component Tests (Happy)")]
    public class GetMovieHappyTests
    {
        private readonly TestServerFixture Fixture;

        private static string MovieUrl = "http://www.omdbapi.com/?apikey=863d6589&type=movie";

        public GetMovieHappyTests(TestServerFixture fixture)
        {
            Fixture = fixture;
        }

        [Fact]
        public async Task When_UserAsksForMovieWithMostFields_Then_ReturnMovieProperly()
        {
            // arrange
            var client = Fixture.Server.CreateClient();
            client.CreateSession();
            var response = ResponseFactory.FromFiddlerLikeResponseFile($"{Fixture.StubsFolder}/OmdbApi/Real_Responses/Happy/200_ContainsMostFields_TheMatrix.txt");
            var matrixMovieUrl = $"{MovieUrl}&t=matrix";
            client.AppendHttpCallStub(HttpMethod.Get, new System.Uri(matrixMovieUrl), response);

            // act
            var httpResponse = await client.GetAsync("/api/movie/matrix");

            using (new AssertionScope())
            {
                // assert logs
                var logs = client.GetSessionLogs();
                logs.Should().BeEmpty();

                // assert outgoing
                var outgoingRequests = client.GetSessionOutgoingRequests();
                outgoingRequests.Count.Should().Be(1);
                outgoingRequests[0].GetEndpoint().Should().Be($"GET {matrixMovieUrl}");
                outgoingRequests[0].GetHeaderValue("Referer").Should().Be(Web.Constants.Referer);

                // assert return
                httpResponse.StatusCode.Should().Be(HttpStatusCode.OK);

                var movie = await httpResponse.ReadJsonBody<Web.DTO.Media>();
                movie.Id.Should().Be("tt0133093");
                movie.Name.Should().Be("The Matrix");
                movie.Year.Should().Be("1999");
                movie.Runtime.Should().Be("2.3h");
                movie.Plot.Should().Be("A computer hacker learns from mysterious rebels about the true nature of his reality and his role in the war against its controllers.");
            }
        }

        [Fact]
        public async Task When_UserAsksForMovieThatDoesntExist_Then_Return400Status()
        {
            // arrange
            var client = Fixture.Server.CreateClient();
            client.CreateSession();
            var response = ResponseFactory.FromFiddlerLikeResponseFile($"{Fixture.StubsFolder}/OmdbApi/Real_Responses/Happy/200_MovieNotFound.txt");
            client.AppendHttpCallStub(HttpMethod.Get, new System.Uri($"{MovieUrl}&t=some_weird_title"), response);

            // act
            var httpResponse = await client.GetAsync("/api/movie/some_weird_title");

            using (new AssertionScope())
            {
                // assert logs
                var logs = client.GetSessionLogs();
                logs.Should().BeEmpty();

                // assert outgoing
                var outgoingRequests = client.GetSessionOutgoingRequests();
                outgoingRequests.Count.Should().Be(1);
                outgoingRequests[0].GetEndpoint().Should().Be($"GET {MovieUrl}&t=some_weird_title");
                outgoingRequests[0].GetHeaderValue("Referer").Should().Be(Web.Constants.Referer);

                // assert return
                httpResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
                var error = await httpResponse.ReadBody();
                error.Should().Be(@"""Search terms didn't match any movie""");
            }
        }

        [Fact]
        public async Task When_UserAsksForMoviFewFields_Then_ReturnMovieProperly()
        {
            // arrange
            var client = Fixture.Server.CreateClient();
            client.CreateSession();
            var response = ResponseFactory.FromFiddlerLikeResponseFile($"{Fixture.StubsFolder}/OmdbApi/Real_Responses/Happy/200_FewFields_OldMovie.txt");
            var url = $"{MovieUrl}&t=come along, do";
            client.AppendHttpCallStub(HttpMethod.Get, new System.Uri(url), response);

            // act
            var httpResponse = await client.GetAsync("/api/movie/come along, do");

            using (new AssertionScope())
            {
                // assert logs
                var logs = client.GetSessionLogs();
                logs.Should().BeEmpty();

                // assert outgoing
                var outgoingRequests = client.GetSessionOutgoingRequests();
                outgoingRequests.Count.Should().Be(1);
                outgoingRequests[0].GetEndpoint().Should().Be($"GET {url}");
                outgoingRequests[0].GetHeaderValue("Referer").Should().Be(Web.Constants.Referer);

                // assert return
                httpResponse.StatusCode.Should().Be(HttpStatusCode.OK);

                var movie = await httpResponse.ReadJsonBody<Web.DTO.Media>();
                movie.Id.Should().Be("tt0000182");
                movie.Name.Should().Be("Come Along, Do!");
                movie.Year.Should().Be("1898");
                movie.Runtime.Should().Be("1 min");
                movie.Plot.Should().Be("A couple look at a statue while eating in an art gallery.");
            }
        }

        [Fact]
        public async Task When_UserAsksForMovieWithSomeInvalidValues_Then_ReturnMovieProperly()
        {
            // arrange
            var client = Fixture.Server.CreateClient();
            client.CreateSession();
            var response = ResponseFactory.FromFiddlerLikeResponseFile($"{Fixture.StubsFolder}/OmdbApi/Fake_Responses/Happy/200_NoRunTime_NoPlot_YearTooOld.txt");
            var url = $"{MovieUrl}&t=fantastika";
            client.AppendHttpCallStub(HttpMethod.Get, new System.Uri(url), response);

            // act
            var httpResponse = await client.GetAsync("/api/movie/fantastika");

            using (new AssertionScope())
            {
                // assert logs
                var logs = client.GetSessionLogs();
                logs.Should().BeEmpty();

                // assert outgoing
                var outgoingRequests = client.GetSessionOutgoingRequests();
                outgoingRequests.Count.Should().Be(1);
                outgoingRequests[0].GetEndpoint().Should().Be($"GET {url}");
                outgoingRequests[0].GetHeaderValue("Referer").Should().Be(Web.Constants.Referer);

                // assert return
                httpResponse.StatusCode.Should().Be(HttpStatusCode.OK);

                var movie = await httpResponse.ReadJsonBody<Web.DTO.Media>();
                movie.Id.Should().Be("tt1185643");
                movie.Name.Should().Be("Fantastika vs. Wonderwoman");
                movie.Runtime.Should().Be("Unknown");
                movie.Year.Should().Be("Unknown");
                movie.Plot.Should().Be("");
            }
        }
    }
}
