using Boilerplate.Infrastructure;
using Boilerplate.Web.Proxy;
using Boilerplate.Web.Services;
using Xunit;
using NSubstitute;
using FluentAssertions;
using System.Threading.Tasks;
using System;

namespace Boilerplate.UnitTests
{
    public class MediaSearchServiceTests
    {
        MediaSearchService sut;
        IMovieDatabaseProxy movieDatabaseProxy;
        IMemoryCacheAdapter memoryCacheAdapter;

        public MediaSearchServiceTests()
        {
            movieDatabaseProxy = Substitute.For<IMovieDatabaseProxy>();
            memoryCacheAdapter = Substitute.For<IMemoryCacheAdapter>();

            sut = new MediaSearchService(movieDatabaseProxy, memoryCacheAdapter);
        }

        [InlineData(null, "name is empty")]
        [InlineData("a", "name has too few characters")]
        [Theory]
        public async Task When_UserAsksForInvalidName_Then_ThrowBadRequest(string name, string expectedException)
        {
            var ex = await Assert.ThrowsAsync<BadRequestException>(() => sut.GetMovieOrTvSeries(Web.DTO.MediaType.movie, name));
            ex.Message.Should().Be(expectedException);
        }

        [Fact]
        public async Task When_UserAsksForMovieTheMatrix_Then_AttemptsToReadCache_And_ReturnCorrectMovie()
        {
            movieDatabaseProxy
                .GetMovieOrTvSeries("movie", "the matri")
                .Returns(new Web.DTO.Media() { Id = "123", Name = "The Matrix" });

            // just executes the method
            memoryCacheAdapter
                .GetOrSetFromCache("movie_thematri",  Arg.Any<Func<Task<Web.DTO.Media>>>())
                .Returns(callinfo => callinfo.ArgAt<Func<Task<Web.DTO.Media>>>(1).Invoke());

            var result = await sut.GetMovieOrTvSeries(Web.DTO.MediaType.movie, "the matri");

            result.Should().NotBeNull();
            result.Id.Should().Be("123");
            result.Name.Should().Be("The Matrix");
        }
    }
}
