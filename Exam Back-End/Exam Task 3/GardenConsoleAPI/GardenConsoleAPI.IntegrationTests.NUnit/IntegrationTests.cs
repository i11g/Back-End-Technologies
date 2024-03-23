using GardenConsoleAPI.Business;
using GardenConsoleAPI.Business.Contracts;
using GardenConsoleAPI.Data.Models;
using GardenConsoleAPI.DataAccess;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using System.ComponentModel.DataAnnotations;

namespace GardenConsoleAPI.IntegrationTests.NUnit
{
    public class IntegrationTests
    {
        private TestPlantsDbContext dbContext;
        private IPlantsManager plantsManager;

        [SetUp]
        public void SetUp()
        {
            this.dbContext = new TestPlantsDbContext();
            this.plantsManager = new PlantsManager(new PlantsRepository(this.dbContext));
        }


        [TearDown]
        public void TearDown()
        {
            this.dbContext.Database.EnsureDeleted();
            this.dbContext.Dispose();
        }

        //positive test
        [Test]
        public async Task AddPlantAsync_ShouldAddNewPlant()
        {
            // Arrange 
            var newPlant = new Plant()
            {
                Name = "Rose",
                PlantType = "Flower",
                FoodType = "Water",
                Quantity = 20,
                CatalogNumber= "123IOP789UYT",
                IsEdible= false,
                
            };

            // Act
            await plantsManager.AddAsync(newPlant);
            var addedPlant = await dbContext.Plants.FirstOrDefaultAsync(p => p.Name == newPlant.Name);

            // Assert
            Assert.NotNull(addedPlant);
            Assert.That(addedPlant.Name, Is.EqualTo(newPlant.Name));
            Assert.That(addedPlant.PlantType, Is.EqualTo(newPlant.PlantType));
            Assert.That(addedPlant.FoodType, Is.EqualTo(newPlant.FoodType));
            Assert.False(addedPlant.IsEdible);
            Assert.That(addedPlant.CatalogNumber, Is.EqualTo(newPlant.CatalogNumber));
            Assert.That(addedPlant.Quantity, Is.EqualTo(newPlant.Quantity));
            
        }

        //Negative test
        [Test]
        public async Task AddPlantAsync_TryToAddPlantWithInvalidCredentials_ShouldThrowException()
        {
            // Arrange           
            var invalidPlant = new Plant()
            {
                Name = "Rose",
                PlantType = "Flower",
                FoodType = "Water",
                Quantity = 20,
                IsEdible = false,

            };

            // Act&Assert
            var exception=Assert.ThrowsAsync<ValidationException>(async ()=> await plantsManager.AddAsync(invalidPlant));
            Assert.NotNull(exception);
            Assert.That(exception.Message, Is.EqualTo("Invalid plant!"));            

        }

        [Test]
        public async Task DeletePlantAsync_WithValidCatalogNumber_ShouldRemovePlantFromDb()
        {
          
            // Arrange 
            var newPlant = new Plant()
            {
                Name = "Rose",
                PlantType = "Flower",
                FoodType = "Water",
                Quantity = 20,
                CatalogNumber = "123IOP789UYT",
                IsEdible = false,

            };

            await plantsManager.AddAsync(newPlant);

            // Act
            await plantsManager.DeleteAsync(newPlant.CatalogNumber);

            var deletedPlant=await dbContext.Plants.FirstOrDefaultAsync(p=>p.CatalogNumber== newPlant.CatalogNumber);

            // Assert
            Assert.IsNull(deletedPlant);           
            
        }

        [Test]
        [TestCase(null)]
        [TestCase(" ")]
        [TestCase("")]
        public async Task DeletePlantAsync_TryToDeleteWithNullOrWhiteSpaceCatalogNumber_ShouldThrowException(string invalidCatalogNumber)
        {
            // Arrange

            // Act&Assert
            var message=Assert.ThrowsAsync<ArgumentException>(async ()=> await plantsManager.DeleteAsync(invalidCatalogNumber));
            Assert.That(message.Message, Is.EqualTo("Catalog number cannot be empty."));
            
        }

        [Test]
        public async Task GetAllAsync_WhenPlantsExist_ShouldReturnAllPlants()
        {
            // Arrange 
            var newPlant = new Plant()
            {
                Name = "Rose",
                PlantType = "Flower",
                FoodType = "Water",
                Quantity = 20,
                CatalogNumber = "123IOP789UYT",
                IsEdible = false,
            };

            var existingPlant = new Plant()
            {
                Name = "Tullip",
                PlantType = "Flower",
                FoodType = "Water",
                Quantity = 50,
                CatalogNumber = "009IOP789JJO",
                IsEdible = false,
            };

            await plantsManager.AddAsync(newPlant);
            await plantsManager.AddAsync(existingPlant);

            // Act

            var allPlants = await plantsManager.GetAllAsync();

            // Assert
            Assert.NotNull(allPlants);
            Assert.That(allPlants.Count, Is.EqualTo(2));
            var firstPlant=allPlants.FirstOrDefault(p=>p.CatalogNumber==newPlant.CatalogNumber);
            Assert.That(firstPlant.Name, Is.EqualTo(newPlant.Name));
            Assert.That(firstPlant.PlantType, Is.EqualTo(newPlant.PlantType));
            Assert.That(firstPlant.FoodType, Is.EqualTo(newPlant.FoodType));
            Assert.That(firstPlant.Quantity, Is.EqualTo(newPlant.Quantity));
            var secondPlant = allPlants.FirstOrDefault(p => p.CatalogNumber == existingPlant.CatalogNumber);
            Assert.That(secondPlant.Name, Is.EqualTo(existingPlant.Name));
            Assert.That(secondPlant.PlantType, Is.EqualTo(existingPlant.PlantType));
            Assert.That(secondPlant.FoodType, Is.EqualTo(existingPlant.FoodType));
            Assert.That(secondPlant.Quantity, Is.EqualTo(existingPlant.Quantity));
            
        }

        [Test]
        public async Task GetAllAsync_WhenNoPlantsExist_ShouldThrowKeyNotFoundException()
        {


            // Act&Assert
            var exception=Assert.ThrowsAsync<KeyNotFoundException>(async ()=> await plantsManager.GetAllAsync());
            Assert.That(exception.Message, Is.EqualTo("No plant found."));
           

        }

        [Test]
        public async Task SearchByFoodTypeAsync_WithExistingFoodType_ShouldReturnMatchingPlants()
        {
            // Arrange 
            var newPlant = new Plant()
            {
                Name = "Rose",
                PlantType = "Flower",
                FoodType = "Water",
                Quantity = 20,
                CatalogNumber = "123IOP789UYT",
                IsEdible = false,
            };

            await plantsManager.AddAsync(newPlant);

            // Act

            var result =await plantsManager.SearchByFoodTypeAsync("Water");

            // Assert
            Assert.NotNull(result);
            Assert.That(result.Count, Is.EqualTo(1));
            var searchedPlant = result.FirstOrDefault(p => p.CatalogNumber == newPlant.CatalogNumber);
            Assert.That(searchedPlant.Name, Is.EqualTo(newPlant.Name));
            Assert.That(searchedPlant.PlantType, Is.EqualTo(newPlant.PlantType));
            Assert.That(searchedPlant.FoodType, Is.EqualTo(newPlant.FoodType));
            Assert.That(searchedPlant.Quantity, Is.EqualTo(newPlant.Quantity));
            Assert.False(searchedPlant.IsEdible);
        }

        [Test]
        public async Task SearchByFoodTypeAsync_WithNonExistingFoodType_ShouldThrowKeyNotFoundException()
        {
            // Arrange 
            var newPlant = new Plant()
            {
                Name = "Rose",
                PlantType = "Flower",
                FoodType = "Water",
                Quantity = 20,
                CatalogNumber = "123IOP789UYT",
                IsEdible = false,
            };

            await plantsManager.AddAsync(newPlant);

            // Act&Assert 

            var exception = Assert.ThrowsAsync<KeyNotFoundException>(async()=> await plantsManager.SearchByFoodTypeAsync("Oil"));
            Assert.That(exception.Message, Is.EqualTo("No plant found with the given food type."));          

            
        }

        [Test]
        public async Task GetSpecificAsync_WithValidCatalogNumber_ShouldReturnPlant()
        {
            // Arrange 
            var newPlant = new Plant()
            {
                Name = "Rose",
                PlantType = "Flower",
                FoodType = "Water",
                Quantity = 20,
                CatalogNumber = "123IOP789UYT",
                IsEdible = false,
            };

            await plantsManager.AddAsync(newPlant);

            // Act

            var result=await plantsManager.GetSpecificAsync(newPlant.CatalogNumber);

            // Assert
            Assert.NotNull(result);
            Assert.That(result.Name, Is.EqualTo(newPlant.Name));
            Assert.That(result.FoodType, Is.EqualTo(newPlant.FoodType));
            Assert.That(result.PlantType, Is.EqualTo(newPlant.PlantType));
            Assert.That(result.Quantity, Is.EqualTo(newPlant.Quantity));
            Assert.False(result.IsEdible);

        }

        [Test]
        public async Task GetSpecificAsync_WithInvalidCatalogNumber_ShouldThrowKeyNotFoundException()
        {
            // Arrange 
            var newPlant = new Plant()
            {
                Name = "Rose",
                PlantType = "Flower",
                FoodType = "Water",
                Quantity = 20,
                CatalogNumber = "123IOP789UYT",
                IsEdible = false,
            };

            await plantsManager.AddAsync(newPlant);

            //Act& Assert
            string invalidCatalogNum = "123R";
            var message = Assert.ThrowsAsync<KeyNotFoundException>(async () => await plantsManager.GetSpecificAsync(invalidCatalogNum));
            Assert.NotNull(message);
            Assert.That(message.Message, Is.EqualTo($"No plant found with catalog number: {invalidCatalogNum}"));
                
          }

        [Test]
        public async Task UpdateAsync_WithValidPlant_ShouldUpdatePlant()
        {
            // Arrange 
            var newPlant = new Plant()
            {
                Name = "Rose",
                PlantType = "Flower",
                FoodType = "Water",
                Quantity = 20,
                CatalogNumber = "123IOP789UYT",
                IsEdible = false,
            };

            await plantsManager.AddAsync(newPlant);

            newPlant.Name = "Cactus";
            newPlant.FoodType = "Sun";
            newPlant.Quantity = 100;

            // Act
            await plantsManager.UpdateAsync(newPlant);
            var updatedPlant= await dbContext.Plants.FirstOrDefaultAsync(p=>p.CatalogNumber== newPlant.CatalogNumber);

            // Assert
            Assert.IsNotNull(updatedPlant);
            Assert.That(updatedPlant.Name, Is.EqualTo(newPlant.Name));
            Assert.That(updatedPlant.FoodType, Is.EqualTo(newPlant.FoodType));
            Assert.That(updatedPlant.Quantity, Is.EqualTo(newPlant.Quantity));            
            
        }

        [Test]
        public async Task UpdateAsync_WithInvalidPlant_ShouldThrowValidationException()
        {
            // Arrange
            var invalidPlant = new Plant()
            {
               
                PlantType = "Flower",
                FoodType = "Water",
                Quantity = 20,
                CatalogNumber = "123IOP789UYT",
                IsEdible = false,
            };
            

            // Act&Assert
            var exception=Assert.ThrowsAsync<ValidationException>(async ()=>await plantsManager.UpdateAsync(invalidPlant));
            Assert.That(exception.Message, Is.EqualTo("Invalid plant!"));

            
        }
    }
}
