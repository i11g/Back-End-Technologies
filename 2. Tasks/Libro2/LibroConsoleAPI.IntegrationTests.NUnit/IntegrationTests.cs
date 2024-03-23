using LibroConsoleAPI.Business;
using LibroConsoleAPI.Business.Contracts;
using LibroConsoleAPI.Data.Models;
using LibroConsoleAPI.Repositories;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace LibroConsoleAPI.IntegrationTests.NUnit
{
    public class IntegrationTests
    {
        private TestLibroDbContext dbContext;
        private IBookManager bookManager;

        [SetUp]
        public void SetUp()
        {
            string dbName = $"TestDb_{Guid.NewGuid()}";
            this.dbContext = new TestLibroDbContext(dbName);
            this.bookManager = new BookManager(new BookRepository(this.dbContext));
        }

        [TearDown]
        public void TearDown()
        {
            this.dbContext.Dispose();
        }

        [Test]
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
            await bookManager.AddAsync(newBook);

            // Assert
            var bookInDb = await dbContext.Books.FirstOrDefaultAsync(b => b.ISBN == newBook.ISBN);
            Assert.That(bookInDb, Is.Not.Null);
            Assert.That(bookInDb.Title, Is.EqualTo("Test Book"));
            Assert.That(bookInDb.Author, Is.EqualTo("John Doe"));
        }

        [Test]
        public async Task AddBookAsync_TryToAddBookWithInvalidTitle_ShouldThrowException()
        {
            //Arrange
            var newBook = new Book()
            {
                Title = new string('B', 500),
                Author = "John Doe",
                ISBN = "1234567890123",
                YearPublished = 2021,
                Genre = "Fiction",
                Pages = 100,
                Price = 19.99
            };
            //Act&Assert
            var result= Assert.ThrowsAsync<ValidationException>(() => bookManager.AddAsync(newBook));
            Assert.IsNotNull(result);
            Assert.AreEqual(result.Message,"Book is invalid.");
            Assert.That("Book is invalid.",Is.EqualTo(result.Message));
            
        }

        [Test]
        public async Task DeleteBookAsync_WithValidISBN_ShouldRemoveBookFromDb()
        {
            //Arrange
            var newBook = new Book()
            {
                Title = "Test Book",
                Author = "John Doe",
                ISBN = "2345678910113",
                YearPublished = 2021,
                Genre = "Fiction",
                Pages = 100,
                Price = 19.99
            };

            //Act 
            await bookManager.AddAsync(newBook);

            await bookManager.DeleteAsync(newBook.ISBN);

            //Assert
            var deletedBook=await dbContext.Books.FirstOrDefaultAsync(b=>b.Title == newBook.Title);
            //var delBook1=await bookManager.GetSpecificAsync("1234567890123");
            Assert.IsNull(deletedBook);
            Assert.That(deletedBook, Is.Null);
            Assert.ThrowsAsync<KeyNotFoundException>(() => bookManager.GetSpecificAsync("2345678910113"));
            //Assert.That("No book found with ISBN: 1234567890123", Is.EqualTo(exception.Message));
            
        }


        [Test]
        public async Task DeleteBookAsync_TryToDeleteWithNullOrWhiteSpaceISBN_ShouldThrowException()
        {
            //Arrange
            var invalidISBN = string.IsNullOrWhiteSpace;
            //Act&Assert
            Assert.ThrowsAsync<ArgumentException>(() => bookManager.DeleteAsync(null));
            Assert.ThrowsAsync<ArgumentException>(() => bookManager.DeleteAsync(" "));
        }


        [Test]
        public async Task GetAllAsync_WhenBooksExist_ShouldReturnAllBooks()
        {
            //Arrange
            var newBook = new Book
            {
                Title = "Test Book",
                Author = "John Doe",
                ISBN = "2345678910113",
                YearPublished = 2021,
                Genre = "Fiction",
                Pages = 100,
                Price = 19.99
            };
            var oldBook = new Book
            {
                Title = "Old Book",
                Author = "John Doe",
                ISBN = "2345678910567",
                YearPublished = 2020,
                Genre = "Fiction",
                Pages = 100,
                Price = 19.99

            };
            await bookManager.AddAsync(newBook);
            await bookManager.AddAsync(oldBook);
            //Act
            var result=await bookManager.GetAllAsync();
            var numberOfBooks=result.Count();
            Assert.NotNull(result);
            Assert.AreEqual(numberOfBooks, 2);
            var firstBookTitle = result.FirstOrDefault(b => b.Title == newBook.Title);
            var firstBookPages= result.FirstOrDefault(b=>b.Pages==newBook.Pages);
            var secondBook = result.FirstOrDefault(b => b.Title == oldBook.Title);
        }


        [Test]
        public async Task GetAllAsync_WhenNoBooksExist_ShouldThrowKeyNotFoundException()
        {
            //Act&Assert
            Assert.ThrowsAsync<KeyNotFoundException>(()=> bookManager.GetAllAsync());
        }


        [Test]
        public async Task SearchByTitleAsync_WithValidTitleFragment_ShouldReturnMatchingBooks()
        {
            //Arrange
            var newBook = new Book
            {
                Title = "Old Book",
                Author = "John Doe",
                ISBN = "2345678910567",
                YearPublished = 2020,
                Genre = "Fiction",
                Pages = 100,
                Price = 19.99
            };

            await bookManager.AddAsync(newBook);
            //Act
            var result=  await bookManager.SearchByTitleAsync("Old");
            var existingBook=result.FirstOrDefault(b => b.Title == newBook.Title);
            
            //Arrange
            Assert.NotNull(result);
            Assert.NotNull(existingBook);
            Assert.That(existingBook.Pages,Is.EqualTo(newBook.Pages));
            
        }


        [Test]
        public async Task SearchByTitleAsync_WithInvalidTitleFragment_ShouldThrowKeyNotFoundException()
        {
            //Act&Assert
            Assert.ThrowsAsync<KeyNotFoundException>(() => bookManager.SearchByTitleAsync("Title"));
        }


        [Test]
        public async Task GetSpecificAsync_WithValidIsbn_ShouldReturnBook()
        {
            //Arrange
            var newBook = new Book
            {
                Title = "Old Book",
                Author = "John Doe",
                ISBN = "2345678910567",
                YearPublished = 2020,
                Genre = "Fiction",
                Pages = 100,
                Price = 19.99
            };
            await bookManager.AddAsync(newBook);

            //Act
            var result=await bookManager.GetSpecificAsync(newBook.ISBN);

            //Assert
            Assert.NotNull(result);
            Assert.That(result.Author, Is.EqualTo(newBook.Author));
        }


        [Test]
        public async Task GetSpecificAsync_WithInvalidIsbn_ShouldThrowKeyNotFoundException()
        {
            //Act&Assert
            var result=Assert.ThrowsAsync<KeyNotFoundException>(() => bookManager.GetSpecificAsync("123456789")).Message;
            Assert.That(result, Is.EqualTo("No book found with ISBN: 123456789"));
        }


        [Test]
        public async Task UpdateAsync_WithValidBook_ShouldUpdateBook()
        {
            //Arrange
            var newBook = new Book
            {
                Title = "Old Book",
                Author = "John Doe",
                ISBN = "2345678910567",
                YearPublished = 2020,
                Genre = "Fiction",
                Pages = 100,
                Price = 19.99
            };
            await bookManager.AddAsync(newBook);
            //Act
            newBook.Title = "New Title";
            newBook.Pages = 1001;
            
            await bookManager.UpdateAsync(newBook);

            //Assert
            var updatedBook = bookManager.GetSpecificAsync(newBook.ISBN);
            Assert.NotNull(updatedBook);
            Assert.That(updatedBook.Result.Title, Is.EqualTo("New Title"));
            Assert.That(updatedBook.Result.Pages, Is.EqualTo((int)newBook.Pages));
            var updateBook=dbContext.Books.FirstOrDefault(b=>b.ISBN==newBook.ISBN);
            Assert.NotNull(updateBook);
            Assert.That(updateBook.Title, Is.EqualTo("New Title"));
        }


        [Test]
        public async Task UpdateAsync_WithInvalidBook_ShouldThrowValidationException()
        {
            //Act&Assert
            Assert.ThrowsAsync<ValidationException>(() => bookManager.UpdateAsync(null));
        }

    }
}
