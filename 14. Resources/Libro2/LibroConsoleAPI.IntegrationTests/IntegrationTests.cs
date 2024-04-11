using LibroConsoleAPI.Business;
using LibroConsoleAPI.Business.Contracts;
using LibroConsoleAPI.Data.Models;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.Runtime.CompilerServices;

namespace LibroConsoleAPI.IntegrationTests
{
    public class IntegrationTests : IClassFixture<BookManagerFixture>
    {
        private readonly BookManagerFixture _fixture;
        private readonly IBookManager _bookManager;
        private readonly TestLibroDbContext _dbContext;

        public IntegrationTests()
        {
            _fixture = new BookManagerFixture();
            _bookManager = _fixture.BookManager;
            _dbContext = _fixture.DbContext;
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
            var bookByBookManager = _bookManager.GetSpecificAsync(newBook.ISBN);            
            var bookInDb = await _dbContext.Books.FirstOrDefaultAsync(b => b.ISBN == newBook.ISBN);
            Assert.NotNull(bookInDb);
            Assert.Equal("Test Book", bookInDb.Title);
            Assert.Equal("John Doe", bookInDb.Author);

            //Arrange
            var newBook1 = new Book()
            {
                Title = "Test Book",
                Author = "John Doe",
                ISBN = "1234567890123",
                YearPublished = 2021,
                Genre = "Fiction",
                Pages = 100,
                Price = 19.99
            };

            //Act
            await _bookManager.AddAsync(newBook1);

            var addedBook = await _dbContext.Books.FirstOrDefaultAsync(b => b.Title == newBook1.Title);

            //Assert
            Assert.NotNull(addedBook);
            Assert.Equal(addedBook.ISBN, newBook1.ISBN);
            Assert.Equal(addedBook.Author, newBook1.Author);
        }

        [Fact]
        public async Task AddBookAsync_TryToAddBookWithInvalidCredentials_ShouldThrowException()
        {
            //Arrange
            var newBook1 = new Book()
            {
                Title = "Test Book",
                Author = "John Doe",
                ISBN = "1234567890123",
                YearPublished = 2025,
                Genre = "Fiction",
                Pages = 100,
                
            };            

            //Act&Assert
            var result= ()=> _bookManager.AddAsync(newBook1);
            var message=Assert.ThrowsAsync<ValidationException>(result);
            Assert.Equal(message.Result.Message, "Book is invalid.");
            
            Assert.ThrowsAsync<ValidationException>(()=>_bookManager.AddAsync(newBook1));
            
        }

        [Fact] 
        public async Task AddBookAsync_WithInvalidISBN_ShouldThorwValidationException()
        {
            //Arrange
            var newBook = new Book()
            {
                Title = "Test Book",
                Author = "John Doe",
                ISBN = "",
                YearPublished = 2025,
                Genre = "Fiction",
                Pages = 100,

            };
            //Act&Assert
            await Assert.ThrowsAsync<ValidationException>(() => _bookManager.AddAsync(newBook));
            var result = Assert.ThrowsAsync<ValidationException>(() => _bookManager.AddAsync(newBook));
            Assert.Equal(result.Result.Message, "Book is invalid.");
        }


        [Fact]
        public async Task DeleteBookAsync_WithValidISBN_ShouldRemoveBookFromDb()
        {

            //Arrange
            var newBook2 = new Book()
            {
                Title = "Test Book",
                Author = "John Doe",
                ISBN = "1234567890123",
                YearPublished = 2021,
                Genre = "Fiction",
                Pages = 100,
                Price = 19.99
            };

            _bookManager.AddAsync(newBook2);

            var ISBN = _dbContext.Books.FirstOrDefaultAsync(b=>b.Title==newBook2.Title).Result.ISBN;
            

            //Act
           await _bookManager.DeleteAsync(ISBN);
           //await _bookManager.DeleteAsync("1234567890123");


            //Assert
            var deletedBook = await _dbContext.Books.FirstOrDefaultAsync(b=>b.Title==newBook2.Title);
            var deletedBook1 = await _dbContext.Books.FirstOrDefaultAsync();
            Assert.Null(deletedBook);
            Assert.Null(deletedBook1);
            Assert.ThrowsAsync<KeyNotFoundException>(() => _bookManager.GetSpecificAsync("1234567890123"));
                        
        }


        [Fact]
        public async Task DeleteBookAsync_TryToDeleteWithNullOrWhiteSpaceISBN_ShouldThrowException()
        {
            //Arrange
            var newBook = new Book()
            {
                Title = "Test Book",
                Author = "John Doe",
                ISBN = " ",
                YearPublished = 2021,
                Genre = "Fiction",
                Pages = 100,
                Price = 19.99

            };
            _bookManager.AddAsync(newBook);

            //Act&Assert
            await Assert.ThrowsAsync<ArgumentException>(() => _bookManager.DeleteAsync(" "));            
        }

        [Fact] 

        public async Task DeleteBookAsync_TryToDleteBookWithNullISBN_ShouldThrowException ()
        {   
            //Arrange
            var newBook = new Book()
            {
                Title = "Test Book",
                Author = "John Doe",
                ISBN = null,
                YearPublished = 2021,
                Genre = "Fiction",
                Pages = 100,
                Price = 19.99
            };

            _bookManager.AddAsync(newBook);

            //Act&Assert
            var result=Assert.ThrowsAsync<ArgumentException>(() => _bookManager.DeleteAsync(null));
            Assert.Equal(result.Result.Message, "ISBN cannot be empty.");
        }


        [Fact]
        public async Task GetAllAsync_WhenBooksExist_ShouldReturnAllBooks()
        {   
            //Arrange
            var newBook2 = new Book()
            {
                Title = "Test Book",
                Author = "John Doe",
                ISBN = "1234567890123",
                YearPublished = 2021,
                Genre = "Fiction",
                Pages = 100,
                Price = 19.99
            };

            

            var newBook = new Book()
            {
                Title = "Test Books",
                Author = "John",
                ISBN = "1234567890321",
                YearPublished = 2021,
                Genre = "Fiction",
                Pages = 100,
                Price = 19.99
            };

           
            //Act
            await _bookManager.AddAsync(newBook2);
            await _bookManager.AddAsync(newBook);
            
            //Assert
            var result=await _bookManager.GetAllAsync();
            Assert.NotNull(result);
            var result1 = _dbContext.Books.ToList();
            Assert.Equal(2, result1.Count);
            Assert.Contains(result, b => b.Title == "Test Books");
            Assert.Contains(result, b => b.ISBN == "1234567890123");

        }


        [Fact]
        public async Task GetAllAsync_WhenNoBooksExist_ShouldThrowKeyNotFoundException()
        {
            //Arrange
            //Act&Assert
            await Assert.ThrowsAsync<KeyNotFoundException>(() => _bookManager.GetAllAsync());
        }


        [Fact]
        public async Task SearchByTitleAsync_WithValidTitleFragment_ShouldReturnMatchingBooks()
        { 
            //Arrange
            var newBook = new Book()
            {                
            
                Title = "Test Book",
                Author = "John Doe",
                ISBN = "1234567891011",
                YearPublished = 2021,
                Genre = "Fiction",
                Pages = 100,
                Price = 19.99
            };

            _bookManager.AddAsync(newBook);

            //Act
            var result= _bookManager.SearchByTitleAsync("Test").Result.FirstOrDefault().ISBN;
            result.Count();

            //Assert
            Assert.Equal(result, newBook.ISBN);
            
        }


        [Fact]
        public async Task SearchByTitleAsync_WithInvalidTitleFragment_ShouldThrowKeyNotFoundException()
        {
            //Arrange
            var newBook = new Book()
            {
                Title = "The Super Book",
                Author = "John Doe",
                ISBN = "1234567891011",
                YearPublished = 2021,
                Genre = "Fiction",
                Pages = 100,
                Price = 19.99
            };
            _bookManager.AddAsync(newBook);
            //Act&Assert
            var result=await Assert.ThrowsAsync<KeyNotFoundException>(() => _bookManager.SearchByTitleAsync("Gool"));
            Assert.Equal(result.Message, "No books found with the given title fragment.");
        }


        [Fact]
        public async Task GetSpecificAsync_WithValidIsbn_ShouldReturnBook()
        {
            //Arrange
            var nBook = new Book()
            {
                Title = "The Super Book",
                Author = "John Doe",
                ISBN = "1234567891011",
                YearPublished = 2021,
                Genre = "Fiction",
                Pages = 100,
                Price = 19.99

            };
            _bookManager.AddAsync(nBook);
            //Act

            var result=await _bookManager.GetSpecificAsync(nBook.ISBN);

            //Assert
            Assert.NotNull(result);
            Assert.Equal(result.Title, nBook.Title);
        }


        [Fact]
        public async Task GetSpecificAsync_WithInvalidIsbn_ShouldThrowKeyNotFoundException()
        {
            //Arrange
            var invalidISBN = "1234567891012";
            //Act&Assert
           var result=await Assert.ThrowsAsync<KeyNotFoundException>(() => _bookManager.GetSpecificAsync(invalidISBN));
            Assert.Equal(result.Message, "No book found with ISBN: 1234567891012");
        }


        [Fact]
        public async Task UpdateAsync_WithValidBook_ShouldUpdateBook()
        {
            //Arrange
            var newBook = new Book()
            {
                Title = "The Super Book",
                Author = "John Doe",
                ISBN = "1234567891011",
                YearPublished = 2021,
                Genre = "Fiction",
                Pages = 100,
                Price = 19.99
            };

            await _bookManager.AddAsync(newBook);

            //Act
            newBook.Title = "Gool";
            newBook.Author = "Ivan";

            await _bookManager.UpdateAsync(newBook);
            
            //Assert
            var bookInDB=_dbContext.Books.FirstOrDefaultAsync(b=>b.ISBN==newBook.ISBN);
            Assert.Equal(bookInDB.Result.Title, "Gool");

        }


        [Fact]
        public async Task UpdateAsync_WithInvalidBook_ShouldThrowValidationException()
        {
            //Arrange
            var invalidBook = new Book()
            {

            };

            //Act&Assert
            await Assert.ThrowsAsync<ValidationException>(() => _bookManager.UpdateAsync(invalidBook));
        }


    }
}
