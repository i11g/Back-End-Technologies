using Microsoft.EntityFrameworkCore;
using ProductConsoleAPI.Business;
using ProductConsoleAPI.Business.Contracts;
using ProductConsoleAPI.Data.Models;
using ProductConsoleAPI.DataAccess;
using System.ComponentModel.DataAnnotations;

namespace ProductConsoleAPI.IntegrationTests.NUnit
{
    public class IntegrationTests
    {
        private TestProductsDbContext dbContext;
        private IProductsManager productsManager;

        [SetUp]
        public void SetUp()
        {
            this.dbContext = new TestProductsDbContext();
            this.productsManager = new ProductsManager(new ProductsRepository(this.dbContext));
        }


        [TearDown]
        public void TearDown()
        {
            this.dbContext.Database.EnsureDeleted();
            this.dbContext.Dispose();
        }


        //positive test
        [Test]
        public async Task AddProductAsync_ShouldAddNewProduct()
        {
            var newProduct = new Product()
            {
                OriginCountry = "Bulgaria",
                ProductName = "TestProduct",
                ProductCode = "AB12C",
                Price = 1.25m,
                Quantity = 100,
                Description = "Anything for description"
            };

            await productsManager.AddAsync(newProduct);

            var dbProduct = await this.dbContext.Products.FirstOrDefaultAsync(p => p.ProductCode == newProduct.ProductCode);

            Assert.NotNull(dbProduct);
            Assert.AreEqual(newProduct.ProductName, dbProduct.ProductName);
            Assert.AreEqual(newProduct.Description, dbProduct.Description);
            Assert.AreEqual(newProduct.Price, dbProduct.Price);
            Assert.AreEqual(newProduct.Quantity, dbProduct.Quantity);
            Assert.AreEqual(newProduct.OriginCountry, dbProduct.OriginCountry);
            Assert.AreEqual(newProduct.ProductCode, dbProduct.ProductCode);
        }

        //Negative test
        [Test]
        public async Task AddProductAsync_TryToAddProductWithInvalidCredentials_ShouldThrowException()
        {
            var newProduct = new Product()
            {
                OriginCountry = "Bulgaria",
                ProductName = "TestProduct",
                ProductCode = "AB12C",
                Price = -1m,
                Quantity = 100,
                Description = "Anything for description"
            };

            var ex = Assert.ThrowsAsync<ValidationException>(async () => await productsManager.AddAsync(newProduct));
            var actual = await dbContext.Products.FirstOrDefaultAsync(c => c.ProductCode == newProduct.ProductCode);

            Assert.IsNull(actual);
            Assert.That(ex?.Message, Is.EqualTo("Invalid product!"));

        }

        [Test]
        public async Task DeleteProductAsync_WithValidProductCode_ShouldRemoveProductFromDb()
        {
            // Arrange
            var newProduct = new Product()
            {
                OriginCountry = "Bulgaria",
                ProductName = "TestProduct",
                ProductCode = "AB12C",
                Price = 1.25m,
                Quantity = 100,
                Description = "Anything for description"

            };

            await productsManager.AddAsync(newProduct);

            // Act

            await productsManager.DeleteAsync(newProduct.ProductCode);

            // Assert

            var deletedProduct = await dbContext.Products.FirstOrDefaultAsync(p => p.ProductCode == newProduct.ProductCode);
            Assert.IsNull(deletedProduct);
            string productdcode = "AB12C";
            var message = Assert.ThrowsAsync<KeyNotFoundException>(async () => await productsManager.GetSpecificAsync(productdcode));
            Assert.That(message.Message, Is.EqualTo($"No product found with product code: {productdcode}"));
        }

        [Test]
        [TestCase("")]
        [TestCase(null)]
        [TestCase(" ")]
        public async Task DeleteProductAsync_TryToDeleteWithNullOrWhiteSpaceProductCode_ShouldThrowException(string invalidPostCode)
        {
            // Arrange

            // Act&Assert
            var message = Assert.ThrowsAsync<ArgumentException>(async () => await productsManager.DeleteAsync(invalidPostCode));
            Assert.That(message.Message, Is.EqualTo("Product code cannot be empty."));


        }

        [Test]
        public async Task GetAllAsync_WhenProductsExist_ShouldReturnAllProducts()
        {
            // Arrange
            List<Product> newProducts = new List<Product>()
            {
                 new Product()
                 {
                     OriginCountry = "Bulgaria",
                     ProductName = "TestProduct",
                     ProductCode = "AB12C",
                     Price = 1.25m,
                     Quantity = 100,
                     Description = "Anything for description"

                 },
                 new Product()
                 {
                     OriginCountry = "Greece",
                     ProductName = "Banana",
                     ProductCode = "AB12D",
                     Price = 1.25m,
                     Quantity = 100,
                     Description = "Anything for description"

                 }

            };

            foreach (var product in newProducts)
            {
                await productsManager.AddAsync(product);
            }

            // Act
            var result = await productsManager.GetAllAsync();

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Count(), Is.EqualTo(2));
            var iteminDB = result.FirstOrDefault(p => p.ProductCode == newProducts[0].ProductCode);
            Assert.That(iteminDB.ProductName, Is.EqualTo(newProducts[0].ProductName));
            Assert.That(iteminDB.OriginCountry, Is.EqualTo(newProducts[0].OriginCountry));
            var secondItemInDB = result.FirstOrDefault(p => p.ProductCode == newProducts[1].ProductCode);
            Assert.That(secondItemInDB.Quantity, Is.EqualTo(newProducts[1].Quantity));
        }

        [Test]
        public async Task GetAllAsync_WhenNoProductsExist_ShouldThrowKeyNotFoundException()
        {
            // Arrange

            // Act&Assert

            var message = Assert.ThrowsAsync<KeyNotFoundException>(async () => await productsManager.GetAllAsync());
            Assert.That(message.Message, Is.EqualTo("No product found."));

        }

        [Test]
        public async Task SearchByOriginCountry_WithExistingOriginCountry_ShouldReturnMatchingProducts()
        {
            // Arrange
            var banana = new Product()
            {
                OriginCountry = "Bulgaria",
                ProductName = "banana",
                ProductCode = "AB12C",
                Price = 1.25m,
                Quantity = 100,
                Description = "Anything for description"
            };
            await productsManager.AddAsync(banana);

            // Act
            var result = await productsManager.SearchByOriginCountry("Bulgaria");

            // Assert
            Assert.NotNull(result);
            Assert.That(result.Count, Is.EqualTo(1));
            var searchedProduct = result.First();
            Assert.That(searchedProduct.Quantity, Is.EqualTo(banana.Quantity));
            Assert.That(searchedProduct.ProductName, Is.EqualTo(banana.ProductName));
        }

        [Test]
        public async Task SearchByOriginCountryAsync_WithNonExistingOriginCountry_ShouldThrowKeyNotFoundException()
        {
            // Arrange
            var banana = new Product()
            {
                OriginCountry = "Bulgaria",
                ProductName = "banana",
                ProductCode = "AB12C",
                Price = 1.25m,
                Quantity = 100,
                Description = "Anything for description"
            };
            await productsManager.AddAsync(banana);

            // Act& Assert
            var message = Assert.ThrowsAsync<KeyNotFoundException>(async () => await productsManager.SearchByOriginCountry("Greece"));
            Assert.That(message.Message, Is.EqualTo("No product found with the given first name."));

        }

        [Test]
        public async Task GetSpecificAsync_WithValidProductCode_ShouldReturnProduct()
        {
            var banana = new Product()
            {
                OriginCountry = "Bulgaria",
                ProductName = "banana",
                ProductCode = "AB12C",
                Price = 1.25m,
                Quantity = 100,
                Description = "Anything for description"
            };
            await productsManager.AddAsync(banana);

            // Act
            var result = await productsManager.GetSpecificAsync(banana.ProductCode);

            // Assert
            Assert.NotNull(result);
            Assert.That(result.Quantity, Is.EqualTo(banana.Quantity));
            Assert.That(result.OriginCountry, Is.EqualTo(banana.OriginCountry));
            Assert.That(result.ProductName, Is.EqualTo(banana.ProductName));


        }

        [Test]
        public async Task GetSpecificAsync_WithInvalidProductCode_ShouldThrowKeyNotFoundException()
        {
            // Arrange
            string invalidProductCode = "123";

            // Act&Assert
            var message = Assert.ThrowsAsync<KeyNotFoundException>(async () => await productsManager.GetSpecificAsync(invalidProductCode));
            Assert.That(message.Message, Is.EqualTo($"No product found with product code: {invalidProductCode}"));

        }

        [Test]
        public async Task UpdateAsync_WithValidProduct_ShouldUpdateProduct()
        {
            var existingProduct = new Product()
            {
                OriginCountry = "Bulgaria",
                ProductName = "banana",
                ProductCode = "AB12C",
                Price = 1.25m,
                Quantity = 100,
                Description = "Anything for description"
            };

            await productsManager.AddAsync(existingProduct);

            existingProduct.ProductName = "orange";
            existingProduct.Price = 100;
            existingProduct.Quantity = 200;

            // Act

            await productsManager.UpdateAsync(existingProduct);

            // Assert
            var updatedProduct = await dbContext.Products.FirstOrDefaultAsync(p => p.ProductCode == existingProduct.ProductCode);
            Assert.NotNull(updatedProduct);
            Assert.That(updatedProduct.Price, Is.EqualTo(existingProduct.Price));
            Assert.That(updatedProduct.ProductName, Is.EqualTo(existingProduct.ProductName));
            Assert.That(updatedProduct.Quantity, Is.EqualTo(existingProduct.Quantity));

        }

        [Test]
        public async Task UpdateAsync_WithInvalidProduct_ShouldThrowValidationException()
        {
            // Arrange
            var invalidProduct = new Product()
            {
                OriginCountry = "Bulgaria",
                ProductName = "banana",
                ProductCode = "",
                Price = 1.25m,
                Quantity = 100,
                Description = "Anything for description"
            };

            // Act&Assert

            var message = Assert.ThrowsAsync<ValidationException>(async () => await productsManager.UpdateAsync(invalidProduct));
            Assert.That(message.Message, Is.EqualTo("Invalid prduct!"));

           

        }
    }
}
