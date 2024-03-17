namespace TownApplication.IntegrationTests
{
    public class TownControllerIntegrationTests
    {
        private readonly TownController _controller;

        public TownControllerIntegrationTests()
        {
            _controller = new TownController();
            _controller.ResetDatabase();
        }

        [Fact]
        public void AddTown_ValidInput_ShouldAddTown()
        {
            // TODO: This test checks if the AddTown method correctly adds a town with valid inputs.

            // Arrange: Prepare the data for the test.
            // 1. Define a town name that is valid (e.g., not too long, not empty).
            // 2. Define a valid population number (positive integer).
            // Replace the placeholder values with actual valid data.
            // Ensure the name is within the valid length
            var validTown = "Sofia";
            var population = 100000; 

            // Act: Perform the action to be tested.
            // Call the AddTown method on the _controller with the arranged data.
            _controller.AddTown(validTown, population);

            // Assert: Verify the outcome of the action.
            // 1. Check if the town was actually added to the database.
            // 2. Verify that the town's data in the database matches the data provided.
            // Use Assert.NotNull to ensure the town is found in the database.
            // Use Assert.Equal to compare the expected and actual population values.
            var result = _controller.ListTowns().Count;
            Assert.Equal(1, result);
            var townresult=_controller.GetTownByName(validTown);
            Assert.Equal(townresult.Name, validTown);
            Assert.Equal(townresult.Population, population);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("AB")]
        public void AddTown_InvalidName_ShouldThrowArgumentException(string invalidName)
        {
            // TODO: This test verifies that the AddTown method throws an ArgumentException for invalid names.

            // Arrange: Set up the test data.
            // 1. The 'invalidName' parameter is provided by the InlineData attributes.
            // 2. Define a population number that is valid (e.g., positive integer).
            // Replace the placeholder value with actual valid data.
            var population=10000;

            // Act & Assert: Execute the test and check for expected outcomes.
            // 1. Call the AddTown method with the invalid name and check that it throws an ArgumentException.
            // 2. Verify that the exception message is as expected.
            Assert.Throws<ArgumentException>(()=>_controller.AddTown(invalidName, population));
            var message=Assert.Throws<ArgumentException>(()=>_controller.AddTown(invalidName,population));
            Assert.Equal(message.Message, "Invalid town name.");
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        public void AddTown_InvalidPopulation_ShouldThrowArgumentException(int invalidPopulation)
        {
            // TODO: This test ensures that the AddTown method correctly handles invalid population values.

            // Arrange: Prepare the test data.
            // 1. Define a valid town name.
            // 2. The 'invalidPopulation' parameter is provided by the InlineData attributes.
            // Replace the placeholder value with actual valid data.
            var townName = "Plovdiv";


            // Act: Execute the action to be tested.
            // Wrap the call to AddTown in an Action delegate to use with Assert.Throws.
               var result =()=>_controller.AddTown(townName, invalidPopulation);
              Action result1 = () => _controller.AddTown(townName, invalidPopulation);

            // Assert: Verify the outcome of the action.
            // Check if an ArgumentException is thrown and validate the exception message.
            Assert.Throws<ArgumentException>(result);
            var message=Assert.Throws<ArgumentException>(result1);
            Assert.Equal(message.Message, "Population must be a positive number.");

        }

        [Fact]
        public void AddTown_DuplicateTownName_DoesNotAddDuplicateTown()
        {
            // TODO: This test verifies that the AddTown method does not add a duplicate town.

            // Arrange: Set up the test data.
            // 1. Define a town name to use for testing.
            // 2. Define a population number for the town.
            // Use the same name for both additions to simulate a duplicate entry.
            var townName = "Pleven";
            var population = 1000000;
            var population1 = 10; 

            // Add a town initially to set up the duplicate scenario.
            _controller.AddTown(townName, population);
            _controller.AddTown(townName, population1);

            // Act: Try to add the same town again.
            _controller.AddTown(townName, population1);

            // Assert: Verify that a duplicate town was not added.
            // 1. Check the total number of towns in the database.
            // 2. Ensure that the count is still 1, indicating no duplicate was added.
            var countTowns = _controller.ListTowns().Count;
            Assert.Equal(countTowns, 1);
            var addedTown = _controller.ListTowns();
            Assert.Equal(addedTown[0].Population, population);        }

        [Fact]
        public void UpdateTown_ShouldUpdatePopulation()
        {
            // TODO: This test checks if the UpdateTown method correctly updates the population of a town.

            // Arrange: Set up the initial state and the data for the test.
            // 1. Define a town name and an initial population number.
            // 2. Add the town to the database.
            // 3. Define an updated population number.
            var townName = "Vratca";
            var population = 100000;
            var updatedPopulation = 100;
            _controller.AddTown(townName,population);

            // Act: Perform the update action.
            // 1. Retrieve the town to be updated.
            // 2. Call the UpdateTown method with the new population value.
            var townId=_controller.GetTownByName(townName).Id;
            //var townId = towns[0].Id;
                       
            _controller.UpdateTown(townId, updatedPopulation);

            // Assert: Verify that the town's population was updated.
            // 1. Retrieve the town again and check if the population matches the updated value.
            // 2. Use Assert.NotNull to ensure the town is found in the database.
            // 3. Use Assert.Equal to check if the population is updated as expected.
            var updatedTown = _controller.GetTownByName(townName);
            Assert.Equal(100, updatedTown.Population);
            Assert.NotNull(updatedTown);
        }

        [Fact]
        public void DeleteTown_ShouldDeleteTown()
        {
            // TODO: This test ensures that the DeleteTown method correctly removes a town from the database.

            // Arrange: Prepare the data and initial state for the test.
            // 1. Define a town name and population number.
            // 2. Add the town to the database to set up the deletion scenario.
            var townName = "Vratca";
            var population = 100000;
            _controller.AddTown(townName,population);

            // Act: Perform the deletion action.
            // 1. Retrieve the town to be deleted.
            // 2. Call the DeleteTown method with the town's ID
            var town = _controller.GetTownByName(townName);
            var townId = town.Id;
            _controller.DeleteTown(townId);

            // Assert: Verify that the town was successfully deleted.
            // 1. Attempt to retrieve the deleted town and check that it no longer exists in the database.
            // 2. Use Assert.Null to ensure that the town is not found.
            var deletedTowns = _controller.ListTowns().Count;
            var townDel = _controller.GetTownByName(townName);
            Assert.Equal(0,deletedTowns);
            Assert.Null(townDel);
        }

        [Fact]
        public void ListTowns_ShouldReturnTowns()
        {
            // TODO: This test checks if the ListTowns method correctly returns a list of all towns in the database.

            // Arrange: Prepare the data for the test.
            // 1. Add several towns to the database to create a list of towns.
            // Each town should have a unique name and a valid population number.
            var townName = "Vratca";
            var population = 100000;
            _controller.AddTown(townName, population);
            var validTown = "Sofia";
            var population1 = 100000;
            _controller.AddTown(validTown, population1);
            var towntoAdd = new List<string> { "Plovdiv", "Sofia1", "Varna" };
            
            foreach(var town in towntoAdd)
            {
                _controller.AddTown(town, town.Length * 100);
            }
                
                // Act: Retrieve the list of towns from the database.
            var  townList=_controller.ListTowns();

            // Assert: Verify the correctness of the retrieved list.
            // 1. Check if the number of towns in the list matches the number of towns added.
            // 2. Verify that each added town is present in the list.
            var addedTownsNumber = _controller.ListTowns().Count;
            //var addedTowns = _controller.ListTowns();
            Assert.Equal(5,addedTownsNumber);
            
            foreach(var town in towntoAdd)
            {
                var existingTown = townList.FirstOrDefault(t => t.Name == town);
                Assert.NotNull(existingTown);
                Assert.Equal(town.Length * 100, existingTown.Population);
            }
        }
    }
}
