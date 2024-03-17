using ContactsConsoleAPI.Business;
using ContactsConsoleAPI.Business.Contracts;
using ContactsConsoleAPI.Data.Models;
using ContactsConsoleAPI.DataAccess;
using ContactsConsoleAPI.DataAccess.Contrackts;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ContactsConsoleAPI.IntegrationTests.NUnit
{
    public class IntegrationTests
    {
        private TestContactDbContext dbContext;
        private IContactManager contactManager;

        [SetUp]
        public void SetUp()
        {
            this.dbContext = new TestContactDbContext();
            this.contactManager = new ContactManager(new ContactRepository(this.dbContext));
        }


        [TearDown]
        public void TearDown()
        {
            this.dbContext.Database.EnsureDeleted();
            this.dbContext.Dispose();
        }


        //positive test
        [Test]
        public async Task AddContactAsync_ShouldAddNewContact()
        {
            var newContact = new Contact()
            {
                FirstName = "TestFirstName",
                LastName = "TestLastName",
                Address = "Anything for testing address",
                Contact_ULID = "1ABC23456HH", //must be minimum 10 symbols - numbers or Upper case letters
                Email = "test@gmail.com",
                Gender = "Male",
                Phone = "0889933779"
            };

            await contactManager.AddAsync(newContact);

            var dbContact = await dbContext.Contacts.FirstOrDefaultAsync(c => c.Contact_ULID == newContact.Contact_ULID);

            Assert.NotNull(dbContact);
            Assert.AreEqual(newContact.FirstName, dbContact.FirstName);
            Assert.AreEqual(newContact.LastName, dbContact.LastName);
            Assert.AreEqual(newContact.Phone, dbContact.Phone);
            Assert.AreEqual(newContact.Email, dbContact.Email);
            Assert.AreEqual(newContact.Address, dbContact.Address);
            Assert.AreEqual(newContact.Contact_ULID, dbContact.Contact_ULID);
        }

        //Negative test
        [Test]
        public async Task AddContactAsync_TryToAddContactWithInvalidCredentials_ShouldThrowException()
        {
            var newContact = new Contact()
            {
                FirstName = "TestFirstName",
                LastName = "TestLastName",
                Address = "Anything for testing address",
                Contact_ULID = "1ABC23456HH", //must be minimum 10 symbols - numbers or Upper case letters
                Email = "invalid_Mail", //invalid email
                Gender = "Male",
                Phone = "0889933779"
            };

            var ex = Assert.ThrowsAsync<ValidationException>(async () => await contactManager.AddAsync(newContact));
            var actual = await dbContext.Contacts.FirstOrDefaultAsync(c => c.Contact_ULID == newContact.Contact_ULID);

            Assert.IsNull(actual);
            Assert.That(ex?.Message, Is.EqualTo("Invalid contact!"));

        }

        [Test]
        public async Task DeleteContactAsync_WithValidULID_ShouldRemoveContactFromDb()
        {
            // Arrange
            var newContact = new Contact()
            {
                FirstName = "Pesho",
                LastName = "Draganov",
                Address = "Sofia 1000",
                Contact_ULID = "1ABC23456HH", //must be minimum 10 symbols - numbers or Upper case letters
                Email = "test@hotmail.com",
                Gender = "Male",
                Phone = "0889933779"

            };
            await contactManager.AddAsync(newContact);

            // Act
            await contactManager.DeleteAsync(newContact.Contact_ULID);


            // Assert
            string contact ="1ABC23456HH";
            var deletedContact= await dbContext.Contacts.FirstOrDefaultAsync(c=>c.Contact_ULID==newContact.Contact_ULID);
            Assert.Null(deletedContact);
            Assert.That(deletedContact, Is.Null);
            var message = Assert.ThrowsAsync<KeyNotFoundException>(() => contactManager.GetSpecificAsync(newContact.Contact_ULID));
            Assert.That(message.Message, Is.EqualTo($"No contact found with ULID: {contact}"));            

        }

        [Test]
        [TestCase(null)]
        [TestCase(" ")]

        public async Task DeleteContactAsync_TryToDeleteWithNullOrWhiteSpaceULID_ShouldThrowException(string invalidULID )
        {
            // Arrange

            // Act& Assert
            Assert.ThrowsAsync<ArgumentException>(()=>contactManager.DeleteAsync(invalidULID));
            
        }

        [Test]
        public async Task GetAllAsync_WhenContactsExist_ShouldReturnAllContacts()
        {
            // Arrange
            var newContact = new Contact()
            {
                FirstName = "Pesho",
                LastName = "Draganov",
                Address = "Sofia 1000",
                Contact_ULID = "1ABC23456HH", 
                Email = "test@hotmail.com",
                Gender = "Male",
                Phone = "0889933779"

            };

            var existingContact = new Contact()
            {
                FirstName = "Goran",
                LastName = "Drago",
                Address = "Plovdiv 1500",
                Contact_ULID = "1ABC23498OO",
                Email = "test1@hotmail.com",
                Gender = "Male",
                Phone = "0889933779"

            };

            await contactManager.AddAsync(newContact);
            await contactManager.AddAsync(existingContact);

            //await dbContext.Contacts.AddRangeAsync(new[] {newContact,existingContact});
            //await dbContext.Contacts.AddAsync(existingContact);


            // Act
            var result=await contactManager.GetAllAsync();

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(2, result.Count());
            var firstContact = result.FirstOrDefault(c => c.Contact_ULID == newContact.Contact_ULID);
            Assert.That(firstContact.Email, Is.EqualTo(newContact.Email));
            var secondContact = result.FirstOrDefault(c => c.Contact_ULID == existingContact.Contact_ULID);
            Assert.That(secondContact.FirstName, Is.EqualTo(existingContact.FirstName));
        }

        [Test]
        public async Task GetAllAsync_WhenNoContactsExist_ShouldThrowKeyNotFoundException()
        {
            

            // Act& Assert
           var message=Assert.ThrowsAsync<KeyNotFoundException>(()=>contactManager.GetAllAsync());
            Assert.That(message.Message, Is.EqualTo("No contact found."));
            
        }

        [Test]
        public async Task SearchByFirstNameAsync_WithExistingFirstName_ShouldReturnMatchingContacts()
        {
            // Arrange
            var newContact = new Contact()
            {
                FirstName = "Pesho",
                LastName = "Draganov",
                Address = "Sofia 1000",
                Contact_ULID = "1ABC23456HH",
                Email = "test@hotmail.com",
                Gender = "Male",
                Phone = "0889933779"

            };
            await contactManager.AddAsync(newContact);

            // Act
            var result=await contactManager.SearchByFirstNameAsync("Pesho");

            // Assert
            Assert.IsNotNull(result);
            Assert.That(result.Count, Is.EqualTo(1));
            var contact=result.FirstOrDefault(c=>c.Contact_ULID==newContact.Contact_ULID);
            Assert.That(contact.Email, Is.EqualTo(newContact.Email));            
        }

        [Test]
        public async Task SearchByFirstNameAsync_WithNonExistingFirstName_ShouldThrowKeyNotFoundException()
        {
            // Arrange
            var newContact = new Contact()
            {
                FirstName = "Pesho",
                LastName = "Draganov",
                Address = "Sofia 1000",
                Contact_ULID = "1ABC23456HH",
                Email = "test@hotmail.com",
                Gender = "Male",
                Phone = "0889933779"

            };
            await contactManager.AddAsync(newContact);

            // Act&Act&Assert
            var message=Assert.ThrowsAsync<KeyNotFoundException>(() => contactManager.SearchByFirstNameAsync("Gosho"));
            Assert.That(message.Message, Is.EqualTo("No contact found with the given first name."));

        }

        [Test]
        public async Task SearchByLastNameAsync_WithExistingLastName_ShouldReturnMatchingContacts()
        {
            // Arrange
            // Arrange
            var newContact = new Contact()
            {
                FirstName = "Pesho",
                LastName = "Draganov",
                Address = "Sofia 1000",
                Contact_ULID = "1ABC23456HH",
                Email = "test@hotmail.com",
                Gender = "Male",
                Phone = "0889933779"

            };
            await contactManager.AddAsync(newContact);

            // Act
            var result=await contactManager.SearchByLastNameAsync("Draganov");

            // Assert
            Assert.NotNull(result);
            Assert.That(result.Count, Is.EqualTo(1));
            var contact = result.FirstOrDefault(c => c.Contact_ULID == newContact.Contact_ULID);
            Assert.That(contact.Address, Is.EqualTo(newContact.Address));
         }

        [Test]
        public async Task SearchByLastNameAsync_WithNonExistingLastName_ShouldThrowKeyNotFoundException()
        {
            // Arrange
            var newContact = new Contact()
            {
                FirstName = "Pesho",
                LastName = "Draganov",
                Address = "Sofia 1000",
                Contact_ULID = "1ABC23456HH",
                Email = "test@hotmail.com",
                Gender = "Male",
                Phone = "0889933779"

            };
            await contactManager.AddAsync(newContact);

            // Act& Assert
            var message=Assert.ThrowsAsync<KeyNotFoundException>(() => contactManager.SearchByLastNameAsync("Gosho"));
            Assert.That(message.Message, Is.EqualTo("No contact found with the given last name."));
        }

        [Test]
        public async Task GetSpecificAsync_WithValidULID_ShouldReturnContact()
        {

            // Arrange
            var newContact = new Contact()
            {
                FirstName = "Pesho",
                LastName = "Draganov",
                Address = "Sofia 1000",
                Contact_ULID = "1ABC23456HH",
                Email = "test@hotmail.com",
                Gender = "Male",
                Phone = "0889933779"

            };
            await contactManager.AddAsync(newContact);

            // Act
            var result = await contactManager.GetSpecificAsync(newContact.Contact_ULID);

            // Assert
            Assert.NotNull(result);
            Assert.That(result.FirstName, Is.EqualTo(newContact.FirstName));
            Assert.That(result.LastName, Is.EqualTo(newContact.LastName));
            Assert.That(result.Email, Is.EqualTo(newContact.Email));
            Assert.That(result.Address, Is.EqualTo(newContact.Address));
            Assert.That(result.Gender, Is.EqualTo(newContact.Gender));
            Assert.That(result.Phone, Is.EqualTo(newContact.Phone));
        }

        [Test]
        public async Task GetSpecificAsync_WithInvalidULID_ShouldThrowKeyNotFoundException()
        {
            // Arrange
            var newContact = new Contact()
            {
                FirstName = "Pesho",
                LastName = "Draganov",
                Address = "Sofia 1000",
                Contact_ULID = "1ABC23456HH",
                Email = "test@hotmail.com",
                Gender = "Male",
                Phone = "0889933779"

            };
            await contactManager.AddAsync(newContact);

            // Act&Assert
            string invalidULID = "123";
            var message=Assert.ThrowsAsync<KeyNotFoundException>(() => contactManager.GetSpecificAsync(invalidULID));
            Assert.That(message.Message, Is.EqualTo($"No contact found with ULID: {invalidULID}"));
        }

        [Test]
        public async Task UpdateAsync_WithValidContact_ShouldUpdateContact()
        {
            
            // Arrange
            var newContact = new Contact()
            {
                FirstName = "Pesho",
                LastName = "Draganov",
                Address = "Sofia 1000",
                Contact_ULID = "1ABC23456HH",
                Email = "test@hotmail.com",
                Gender = "Male",
                Phone = "0889933779"

            };
            await contactManager.AddAsync(newContact);
            newContact.FirstName = "Gosho";
            newContact.Address = "Plovdiv";

            // Act
            await contactManager.UpdateAsync(newContact);

            // Assert
            var updatedContact = await contactManager.GetSpecificAsync(newContact.Contact_ULID);
            Assert.NotNull(updatedContact);
            Assert.That(updatedContact.FirstName, Is.EqualTo(newContact.FirstName));
            Assert.That(updatedContact.Address, Is.EqualTo("Plovdiv"));
        }

        [Test]
        public async Task UpdateAsync_WithInvalidContact_ShouldThrowValidationException()
        {
           
            // Arrange
            var newContact = new Contact()
            {
                
                LastName = "Draganov",
                Address = "Sofia 1000",
                Contact_ULID = "1ABC23456HH",
                Email = "test@hotmail.com",
                Gender = "Male",
                Phone = "0889933779"

            };

            // Act&Assert
            Assert.ThrowsAsync<ValidationException>(() => contactManager.UpdateAsync(newContact));

            
            
        }
    }
}
