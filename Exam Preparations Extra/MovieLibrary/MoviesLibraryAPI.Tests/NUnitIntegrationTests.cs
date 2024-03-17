using Microsoft.Extensions.Configuration;
using MongoDB.Driver;
using MoviesLibraryAPI.Controllers;
using MoviesLibraryAPI.Controllers.Contracts;
using MoviesLibraryAPI.Data.Models;
using MoviesLibraryAPI.Services;
using MoviesLibraryAPI.Services.Contracts;
using System.ComponentModel.DataAnnotations;

namespace MoviesLibraryAPI.Tests
{
    [TestFixture]
    public class NUnitIntegrationTests
    {
        private MoviesLibraryNUnitTestDbContext _dbContext;
        private IMoviesLibraryController _controller;
        private IMoviesRepository _repository;
        IConfiguration _configuration;

        [OneTimeSetUp]
        public void OneTimeSetup()
        {
            _configuration = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json")
                .Build();
        }

        [SetUp]
        public async Task Setup()
        {
            string dbName = $"MoviesLibraryTestDb_{Guid.NewGuid()}";
            _dbContext = new MoviesLibraryNUnitTestDbContext(_configuration, dbName);

            _repository = new MoviesRepository(_dbContext.Movies);
            _controller = new MoviesLibraryController(_repository);
        }

        [TearDown]
        public async Task TearDown()
        {
            await _dbContext.ClearDatabaseAsync();
        }

        [Test]
        public async Task AddMovieAsync_WhenValidMovieProvided_ShouldAddToDatabase()
        {
            // Arrange
            var movie = new Movie
            {
                Title = "Test Movie",
                Director = "Test Director",
                YearReleased = 2022,
                Genre = "Action",
                Duration = 86,
                Rating = 7.5
            };

            // Act
            await _controller.AddAsync(movie);

            // Assert
            var resultMovie = await _dbContext.Movies.Find(m => m.Title == "Test Movie").FirstOrDefaultAsync();
            var result = await _controller.GetByTitle("Test Movie");
            Assert.IsNotNull(resultMovie);
            Assert.That(result.Title, Is.EqualTo(movie.Title));
        }

        [Test]
        public async Task AddMovieAsync_WhenInvalidMovieProvided_ShouldThrowValidationException()
        {
            // Arrange
            var invalidMovie = new Movie
            {
                Title = "",
                Director = "Test Director",
                YearReleased = 2022,
                Genre = "Action",
                Duration = 86,
                Rating = 7.5

            };

            // Act and Assert
            // Expect a ValidationException because the movie is missing a required field
            var exception = Assert.ThrowsAsync<ValidationException>(() => _controller.AddAsync(invalidMovie));
        }

        [Test]
        public async Task DeleteAsync_WhenValidTitleProvided_ShouldDeleteMovie()
        {
            // Arrange
            var newMovie = new Movie
            {
                Title = "New Year",
                Director = "Test Director",
                YearReleased = 2022,
                Genre = "Action",
                Duration = 86,
                Rating = 7.5
            };

            await _controller.AddAsync(newMovie);
            // Act
            await _controller.DeleteAsync("New Year");

            // Assert
            var result = await _controller.GetByTitle("New Year");
            var result1=await _dbContext.Movies.Find(m=>m.Title == newMovie.Title).FirstOrDefaultAsync();
            Assert.That(result, Is.Null);
            Assert.IsNull(result1);
            
        }

        [Test]
        public async Task DeleteAsync_WhenTitleIsNull_ShouldThrowArgumentException()
        {
            // Act and Assert
            var message=Assert.ThrowsAsync<ArgumentException>(() => _controller.DeleteAsync(null));
            Assert.That(message.Message, Is.EqualTo("Title cannot be empty."));
        }

        [Test]
        public async Task DeleteAsync_WhenTitleIsEmpty_ShouldThrowArgumentException()
        {
            // Act and Assert
            var message = Assert.ThrowsAsync<ArgumentException>(() => _controller.DeleteAsync(" "));
            Assert.That(message.Message, Is.EqualTo("Title cannot be empty."));
        }   

        [Test]
        public async Task DeleteAsync_WhenTitleDoesNotExist_ShouldThrowInvalidOperationException()
        {
            // Act and Assert
            Assert.ThrowsAsync<InvalidOperationException>(() => _controller.DeleteAsync("New Title")); 


        }

        [Test]
        public async Task GetAllAsync_WhenNoMoviesExist_ShouldReturnEmptyList()
        {
            // Act
            var result = await _controller.GetAllAsync();

            // Assert
            Assert.IsEmpty(result);
        }

        [Test]
        public async Task GetAllAsync_WhenMoviesExist_ShouldReturnAllMovies()
        {
            // Arrange
            var newMovie = new Movie
            {
                Title = "New Year",
                Director = "Test Director",
                YearReleased = 2022,
                Genre = "Action",
                Duration = 86,
                Rating = 7.5
            };
            var oldMovie = new Movie
            {
                Title = "SciFI",
                Director = "New Director",
                YearReleased = 2022,
                Genre = "Action",
                Duration = 86,
                Rating = 7.5
            };

            await _controller.AddAsync(newMovie);
            await _controller.AddAsync(oldMovie);

            // Act 

            var resultAllMovies=await _controller.GetAllAsync();
            var firstMovie = resultAllMovies.FirstOrDefault(m => m.Title == newMovie.Title);

            // Assert
            Assert.NotNull(resultAllMovies);
            Assert.That(resultAllMovies.Count(), Is.EqualTo(2));
            Assert.That(firstMovie.Title, Is.EqualTo(firstMovie.Title));
                
        }

        [Test]
        public async Task GetByTitle_WhenTitleExists_ShouldReturnMatchingMovie()
        {
            // Arrange
            var newMovie = new Movie
            {
                Title = "New Year",
                Director = "Test Director",
                YearReleased = 2022,
                Genre = "Action",
                Duration = 86,
                Rating = 7.5
            };
            await _controller.AddAsync(newMovie);

            // Act
            var result=await _controller.GetByTitle("New Year");

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Title, Is.EqualTo(newMovie.Title));
        }

        [Test]
        public async Task GetByTitle_WhenTitleDoesNotExist_ShouldReturnNull()
        {
            // Act
            var resultMovie=await _controller.GetByTitle("");

            // Assert
            Assert.IsNull(resultMovie);
            Assert.That(resultMovie, Is.Null);
        }


        [Test]
        public async Task SearchByTitleFragmentAsync_WhenTitleFragmentExists_ShouldReturnMatchingMovies()
        {
            // Arrange
            var newMovie = new Movie
            {
                Title = "New Year",
                Director = "Test Director",
                YearReleased = 2022,
                Genre = "Action",
                Duration = 86,
                Rating = 7.5
            };
            await _controller.AddAsync(newMovie);

            // Act
           var resultMovie=await _controller.SearchByTitleFragmentAsync("Year");
           var title=resultMovie.FirstOrDefault(m=>m.Title==newMovie.Title);

            // Assert // Should return one matching movie
            Assert.IsNotNull(resultMovie);
            Assert.That(title.Id, Is.EqualTo(newMovie.Id));
        }

        [Test]
        public async Task SearchByTitleFragmentAsync_WhenNoMatchingTitleFragment_ShouldThrowKeyNotFoundException()
        {
            // Act and Assert
            var message=Assert.ThrowsAsync<KeyNotFoundException>(() => _controller.SearchByTitleFragmentAsync("New"));
            Assert.That(message.Message, Is.EqualTo("No movies found."));
        }

        [Test]
        public async Task UpdateAsync_WhenValidMovieProvided_ShouldUpdateMovie()
        {
            // Arrange
            var newMovie = new Movie
            {
                Title = "New Year",
                Director = "Test Director",
                YearReleased = 2022,
                Genre = "Action",
                Duration = 86,
                Rating = 7.5
            };
            await _controller.AddAsync(newMovie);

            // Modify the movie

            newMovie.Duration = 120;
            newMovie.Director = "Tesla";
            newMovie.Rating = 9.0;

            // Act
            await _controller.UpdateAsync(newMovie);

            // Assert
            var updatedMovie = await _controller.GetByTitle("New Year");
            Assert.IsNotNull(updatedMovie);
            Assert.That(updatedMovie.Duration, Is.EqualTo(newMovie.Duration));
            Assert.That(updatedMovie.Rating, Is.EqualTo(9.0));
            Assert.That(updatedMovie.Director, Is.EqualTo("Tesla"));
        }

        [Test]
        public async Task UpdateAsync_WhenInvalidMovieProvided_ShouldThrowValidationException()
        {
            // Arrange
            // Movie without required fields

            // Act and Assert
        }


        [OneTimeTearDown]
        public async Task OneTimeTearDown()
        {
            await _dbContext.ClearDatabaseAsync();
        }
    }
}
