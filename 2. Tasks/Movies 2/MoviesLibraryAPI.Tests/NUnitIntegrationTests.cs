using DnsClient;
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
            Assert.IsNotNull(resultMovie);
            Assert.That(resultMovie.Title,Is.EqualTo(movie.Title));
        }

        [Test]
        public async Task AddMovieAsync_WhenInvalidMovieProvided_ShouldThrowValidationException()
        {
            // Arrange
            var invalidMovie = new Movie
            {
                
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
             var movie = new Movie 
            {
                Title = "Test Movie",
                Director = "Test Director",
                YearReleased = 2022,
                Genre = "Action",
                Duration = 86,
                Rating = 7.5
            };
            await _controller.AddAsync(movie);

            // Act
            await _controller.DeleteAsync(movie.Title);

            // Assert
            var result = await _dbContext.Movies.Find(m => m.Title == movie.Title).FirstOrDefaultAsync();
            Assert.Null(result);
            var exception  =Assert.ThrowsAsync<KeyNotFoundException>(() => _controller.SearchByTitleFragmentAsync("Test"));
            Assert.That(exception.Message, Is.EqualTo("No movies found."));
        }


        [Test]
        public async Task DeleteAsync_WhenTitleIsNull_ShouldThrowArgumentException()
        {
            //Arrange

            // Act and Assert
           Assert.ThrowsAsync<ArgumentException>(() => _controller.DeleteAsync(null));
        }

        [Test]
        public async Task DeleteAsync_WhenTitleIsEmpty_ShouldThrowArgumentException()
        {
            //Act and Assert
            Assert.ThrowsAsync<ArgumentException>(() => _controller.DeleteAsync(" "));
        }

        [Test]
        public async Task DeleteAsync_WhenTitleDoesNotExist_ShouldThrowInvalidOperationException()
        {
            //Act and Assert
            Assert.ThrowsAsync<InvalidOperationException>(() => _controller.DeleteAsync("Title"));
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
            var movie = new Movie
            {
                Title = "Test Movie",
                Director = "Test Director",
                YearReleased = 2022,
                Genre = "Action",
                Duration = 86,
                Rating = 7.5
            };
            await _controller.AddAsync(movie);
            var movieTwo = new Movie
            {
                Title = "Movie",
                Director = "New Director",
                YearReleased = 2020,
                Genre = "Action",
                Duration = 86,
                Rating = 9.5
            };
            await _controller.AddAsync(movieTwo);

            // Act
            var result= _controller.GetAllAsync().Result.Count();
            var allmovies=await _controller.GetAllAsync();
            var result1= _controller.GetAllAsync().Result.Any();
            var firstMovie = allmovies.Any(m => m.Title == movie.Title);
            var secMovie = allmovies.FirstOrDefault(m => m.Title == movieTwo.Title);

            // Assert
            Assert.That(2, Is.EqualTo(result));
            Assert.NotNull(result);
            Assert.NotNull(result1);
            Assert.IsTrue(firstMovie);
            Assert.That(secMovie.Director, Is.EqualTo("New Director"));
            
            
        }

        [Test]
        public async Task GetByTitle_WhenTitleExists_ShouldReturnMatchingMovie()
        {
            // Arrange
            var newMovie = new Movie
            {
                Title = "Test Movie",
                Director = "Test Director",
                YearReleased = 2022,
                Genre = "Action",
                Duration = 86,
                Rating = 7.5
            };

           await _controller.AddAsync(newMovie);

            // Act
            var result= _controller.GetByTitle(newMovie.Title);

            // Assert 
            Assert.NotNull(result);
            Assert.That(result.Result.Title,Is.EqualTo(newMovie.Title));
            Assert.That(result.Result.Genre, Is.EqualTo(newMovie.Genre));

        }

        [Test]
        public async Task GetByTitle_WhenTitleDoesNotExist_ShouldReturnNull()
        {
            // Act
            var result=await _controller.GetByTitle("Title");
            // Assert
            Assert.IsNull(result);
        }


        [Test]
        public async Task SearchByTitleFragmentAsync_WhenTitleFragmentExists_ShouldReturnMatchingMovies()
        {
            // Arrange
            var newMovie = new Movie()
            {
                Title = "Test Movie",
                Director = "Test Director",
                YearReleased = 2022,
                Genre = "Action",
                Duration = 86,
                Rating = 7.5
            };
           await _controller.AddAsync(newMovie);

            // Act
            var result = await _controller.SearchByTitleFragmentAsync("Test");
            var oneresult=result.FirstOrDefault(m=>m.Title=="Test Movie");
            var number=result.Count();

            // Assert 
            // Should return one matching movie
            Assert.That(oneresult.Title, Is.EqualTo("Test Movie"));
            Assert.NotNull(result);
            Assert.That(number, Is.EqualTo(1));

        }

        [Test]
        public async Task SearchByTitleFragmentAsync_WhenNoMatchingTitleFragment_ShouldThrowKeyNotFoundException()
        {
            // Act and Assert
            Assert.ThrowsAsync<KeyNotFoundException>(() => _controller.SearchByTitleFragmentAsync("Title"));
        }

        [Test]
        public async Task UpdateAsync_WhenValidMovieProvided_ShouldUpdateMovie()
        {
            // Arrange
            var newMovie = new Movie
            {
                Title = "Test Movie",
                Director = "Test Director",
                YearReleased = 2022,
                Genre = "Action",
                Duration = 86,
                Rating = 7.5
            };
            await _controller.AddAsync(newMovie);

            // Modify the movie
            newMovie.Director = "New";
            newMovie.YearReleased = 2020;
            newMovie.Genre = "Comics";

            // Act
            var updatedMovie= _controller.UpdateAsync(newMovie);
            var result = _dbContext.Movies.Find(m => m.Director == "New").FirstOrDefaultAsync();

            Assert.NotNull(updatedMovie);
            Assert.That(result.Result.Genre, Is.EqualTo("Comics"));
            
        }

        [Test]
        public async Task UpdateAsync_WhenInvalidMovieProvided_ShouldThrowValidationException()
        {
            // Arrange
            var newMovie = new Movie
            {
                
                Director = "Test Director",
                YearReleased = 2022,
                Genre = "Action",
                Duration = 86,
                Rating = 7.5
            };
            

            // Act and Assert
            Assert.ThrowsAsync<ValidationException>(()=> _controller.UpdateAsync(newMovie));
        }


        [OneTimeTearDown]
        public async Task OneTimeTearDown()
        {
            await _dbContext.ClearDatabaseAsync();
        }
    }
}
