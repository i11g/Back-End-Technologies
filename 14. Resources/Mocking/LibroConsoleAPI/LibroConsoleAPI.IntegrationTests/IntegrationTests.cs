using LibroConsoleAPI.Business.Contracts;
using LibroConsoleAPI.Data.Models;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace LibroConsoleAPI.IntegrationTests
{
    public class IntegrationTests : IClassFixture<BookManagerFixture>
    {
        private readonly IBookManager _bookManager;
        private readonly TestLibroDbContext _dbContext;

        public IntegrationTests(BookManagerFixture fixture)
        {
            _bookManager = fixture.BookManager;
            _dbContext = fixture.DbContext;
        }

        [Fact]
        public async Task AddBookAsync_ShouldAddBook()
        {
            // Arrange
            var newBook = new Book
            {
                Title = "Test Book",
                Author = "John Doe",
                ISBN = "1234567890123",
                YearPublished = 2021,
                Genre = "Fiction",
                Pages = 100,
                Price = 19.99
            };

            // Act
            await _bookManager.AddAsync(newBook);

            // Assert
            var bookInDb = await _dbContext.Books.FirstOrDefaultAsync(b => b.ISBN == newBook.ISBN);
            Assert.NotNull(bookInDb);
            Assert.Equal("Test Book", bookInDb.Title);
            Assert.Equal("John Doe", bookInDb.Author);
        }

        [Fact]
        public async Task AddBookAsync_TryToAddBookWithInvalidCredentials_ShouldThrowException()
        {
            // Arrange
            var newBook = new Book

            {
                Title =new string('A', 600) ,
                Author = "John Doe",
                ISBN = "1234567890123",
                YearPublished = 2021,
                Genre = "Fiction",
                Pages = 100,
                Price = 19.99

            };

            // Act
            var action = Assert.ThrowsAsync<ValidationException>(() => _bookManager.AddAsync(newBook));

            // Assert
            
            Assert.Equal(action.Result.Message, "Book is invalid.");        }

        [Fact]
        public async Task DeleteBookAsync_WithValidISBN_ShouldRemoveBookFromDb()
        {
            // Arrange
            var newBook = new Book
            {
                Title = "Sofia",
                Author = "John Doe",
                ISBN = "1234567890123",
                YearPublished = 2021,
                Genre = "Fiction",
                Pages = 100,
                Price = 19.99

            };

            // Act
            await _bookManager.AddAsync(newBook);
            //await _bookManager.DeleteAsync("1234567890123");
            await _bookManager.DeleteAsync("1234567890123");


            // Assert
            var bookInDb = await _dbContext.Books.FirstOrDefaultAsync(b=>b.ISBN==newBook.ISBN);
            Assert.Null(bookInDb);
            
            
        }
        [Fact]
        public async Task DeleteBookAsync_TryToDeleteWithNullOrWhiteSpaceISBN_ShouldThrowException()
        {
            // Arrange
            var newBook = new Book
            {
                Title = "Sofia",
                Author = "John Doe",
                ISBN = "1234567890123",
                YearPublished = 2021,
                Genre = "Fiction",
                Pages = 100,
                Price = 19.99

            };
            await _bookManager.AddAsync(newBook);

            // Act
            //Assert.ThrowsAsync<ValidationException>(() => _bookManager.DeleteAsync(null));    
            Assert.ThrowsAsync<ValidationException>(() => _bookManager.DeleteAsync(""));

            // Assert

            
        }

        [Fact]
        public async Task GetAllAsync_WhenBooksExist_ShouldReturnAllBooks()
        {
            // Arrange
            var newBook = new Book
            {
                Title = "Sofia",
                Author = "John Doe",
                ISBN = "1234567890123",
                YearPublished = 2021,
                Genre = "Fiction",
                Pages = 100,
                Price = 19.99

            };
            await _bookManager.AddAsync(newBook);

            var secondBook = new Book
            {
                Title = "Pleven",
                Author = "John Doe1",
                ISBN = "4564567890123",
                YearPublished = 2021,
                Genre = "Fiction",
                Pages = 100,
                Price = 19.99

            };
            await _bookManager.AddAsync(secondBook);

            // Act
            var result=_bookManager.GetAllAsync().Result.Count();
            var result1 = _bookManager.GetAllAsync().Result.ToList();

            // Assert
            Assert.Equal(2,result);
            foreach(var res in result1)
            {
                var existingTitle=res.Title;
                Assert.NotNull(existingTitle);
            }

            Assert.Equal(result1[0].Title, newBook.Title);
            Assert.Equal(result1[1].Author,secondBook.Author);
        }

        [Fact]
        public async Task GetAllAsync_WhenNoBooksExist_ShouldThrowKeyNotFoundException()
        {
            // Arrange

            // Act
            Assert.ThrowsAsync<KeyNotFoundException>(() => _bookManager.GetAllAsync());

            // Assert
        }

        [Fact]
        public async Task SearchByTitleAsync_WithValidTitleFragment_ShouldReturnMatchingBooks()
        {
            // Arrange

            // Act

            // Assert
        }

        [Fact]
        public async Task SearchByTitleAsync_WithInvalidTitleFragment_ShouldThrowKeyNotFoundException()
        {
            // Arrange

            // Act

            // Assert
        }

        [Fact]
        public async Task GetSpecificAsync_WithValidIsbn_ShouldReturnBook()
        {
            // Arrange

            // Act

            // Assert
        }

        [Fact]
        public async Task GetSpecificAsync_WithInvalidIsbn_ShouldThrowKeyNotFoundException()
        {
            // Arrange

            // Act

            // Assert
        }

        [Fact]
        public async Task UpdateAsync_WithValidBook_ShouldUpdateBook()
        {
            // Arrange

            // Act

            // Assert
        }

        [Fact]
        public async Task UpdateAsync_WithInvalidBook_ShouldThrowValidationException()
        {
            // Arrange

            // Act

            // Assert
        }

    }
}
