using MongoDB.Driver;
using MoviesLibraryAPI.Controllers;
using MoviesLibraryAPI.Controllers.Contracts;
using MoviesLibraryAPI.Data.Models;
using MoviesLibraryAPI.Services;
using MoviesLibraryAPI.Services.Contracts;
using System.ComponentModel.DataAnnotations;
using Xunit;

namespace MoviesLibraryAPI.XUnitTests
{
    public class XUnitIntegrationTests : IClassFixture<DatabaseFixture>
    {
        private readonly MoviesLibraryXUnitTestDbContext _dbContext;
        private readonly IMoviesLibraryController _controller;
        private readonly IMoviesRepository _repository;

        public XUnitIntegrationTests(DatabaseFixture fixture)
        {
            _dbContext = fixture.DbContext;
            _repository = new MoviesRepository(_dbContext.Movies);
            _controller = new MoviesLibraryController(_repository);

            InitializeDatabaseAsync().GetAwaiter().GetResult();
        }

        private async Task InitializeDatabaseAsync()
        {
            await _dbContext.ClearDatabaseAsync();
        }

        [Fact]
        public async Task AddMovieAsync_WhenValidMovieProvided_ShouldAddToDatabase()
        {
            // Arrange
            var movie = new Movie
            {
                Title = "Test Movie",
                Director = "Test Director",
                YearReleased = 2022,
                Genre = "Action",
                Duration = 120,
                Rating = 7.5
            };

            // Act
            await _controller.AddAsync(movie);

            // Assert
            var resultMovie = await _dbContext.Movies.Find(m => m.Title == "Test Movie").FirstOrDefaultAsync();
            Assert.NotNull(resultMovie);
            Xunit.Assert.Equal("Test Movie", resultMovie.Title);
            Xunit.Assert.Equal("Test Director", resultMovie.Director);
            Xunit.Assert.Equal(2022, resultMovie.YearReleased);
            Xunit.Assert.Equal("Action", resultMovie.Genre);
            Xunit.Assert.Equal(120, resultMovie.Duration);
            Xunit.Assert.Equal(7.5, resultMovie.Rating);
        }

        [Fact]
        public async Task AddMovieAsync_WhenInvalidMovieProvided_ShouldThrowValidationException()
        {
            // Arrange
            var invalidMovie = new Movie
            {
               
                Director = "Test Director",
                YearReleased = 2022,
                Genre = "Action",
                Duration = 120,
                Rating = 7.5
            };

            // Act and Assert
            var result=Assert.ThrowsAsync<ValidationException>(()=>_controller.AddAsync(invalidMovie));
            Assert.Equal(result.Result.Message, "Movie is not valid.");
        }

        [Fact]
        public async Task DeleteAsync_WhenValidTitleProvided_ShouldDeleteMovie()
        {
            // Arrange

            var oldMovie = new Movie
            {
                Title = "Old Movie",
                Director = "Test Director",
                YearReleased = 2022,
                Genre = "Action",
                Duration = 120,
                Rating = 7.5
            };
            await _controller.AddAsync(oldMovie);

            // Act
            await _controller.DeleteAsync("Old Movie");

            // Assert
            var deletedMovie=_dbContext.Movies.Find(m=>m.Title == oldMovie.Title);
            //var deletedMovie1 = _controller.GetByTitle("Test Movie");
            Assert.NotNull(deletedMovie);

        }


        [Fact]
        public async Task DeleteAsync_WhenTitleIsNull_ShouldThrowArgumentException()
        {
            // Act and Assert
        }

        [Fact]
        public async Task DeleteAsync_WhenTitleIsEmpty_ShouldThrowArgumentException()
        {
            // Act and Assert            
        }

        [Fact]
        public async Task DeleteAsync_WhenTitleDoesNotExist_ShouldThrowInvalidOperationException()
        {
            // Act and Assert            
        }

        [Fact]
        public async Task GetAllAsync_WhenNoMoviesExist_ShouldReturnEmptyList()
        {
            // Act
            var result = await _controller.GetAllAsync();

            // Assert
        }

        [Fact]
        public async Task GetAllAsync_WhenMoviesExist_ShouldReturnAllMovies()
        {
            // Arrange

            // Act

            // Assert
            // Ensure that all movies are returned
        }

        [Fact]
        public async Task GetByTitle_WhenTitleExists_ShouldReturnMatchingMovie()
        {
            // Arrange

            // Act

            // Assert
        }

        [Fact]
        public async Task GetByTitle_WhenTitleDoesNotExist_ShouldReturnNull()
        {
            // Act

            // Assert
            //Assert.IsNull(resultMovie);
        }


        [Fact]
        public async Task SearchByTitleFragmentAsync_WhenTitleFragmentExists_ShouldReturnMatchingMovies()
        {
            // Arrange

            // Act

            // Assert // Should return one matching movie
        }

        [Fact]
        public async Task SearchByTitleFragmentAsync_WhenNoMatchingTitleFragment_ShouldThrowKeyNotFoundException()
        {
            // Act and Assert
        }

        [Fact]
        public async Task UpdateAsync_WhenValidMovieProvided_ShouldUpdateMovie()
        {
            // Arrange

            // Modify the movie

            // Act

            // Assert
        }

        [Fact]
        public async Task UpdateAsync_WhenInvalidMovieProvided_ShouldThrowValidationException()
        {
            // Arrange
            // Movie without required fields

            // Act and Assert
        }
    }
}
