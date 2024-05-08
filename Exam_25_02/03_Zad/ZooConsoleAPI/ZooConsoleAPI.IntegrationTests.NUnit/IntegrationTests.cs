using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using System.ComponentModel.DataAnnotations;
using System.Xml.Linq;
using ZooConsoleAPI.Business;
using ZooConsoleAPI.Business.Contracts;
using ZooConsoleAPI.Data.Model;
using ZooConsoleAPI.DataAccess;

namespace ZooConsoleAPI.IntegrationTests.NUnit
{
    public class IntegrationTests
    {
        private TestAnimalDbContext dbContext;
        private IAnimalsManager animalsManager;

        [SetUp]
        public void SetUp()
        {
            this.dbContext = new TestAnimalDbContext();
            this.animalsManager = new AnimalsManager(new AnimalRepository(this.dbContext));
        }


        [TearDown]
        public void TearDown()
        {
            this.dbContext.Database.EnsureDeleted();
            this.dbContext.Dispose();
        }


        //positive test
        [Test]
        public async Task AddAnimalAsync_ShouldAddNewAnimal()
        {
            // Arrange
            var newAnimal = new Animal()
            {
                CatalogNumber = "00HNTWXTQSH4",
                Name = "Puppi",
                Breed = "Cape fox",
                Type = "Mammal",
                Age = 3,
                Gender = "Male",
                IsHealthy = true

            };



            // Act
            await animalsManager.AddAsync(newAnimal);

            // Assert
            var result = await dbContext.Animals.FirstOrDefaultAsync(c => c.CatalogNumber == newAnimal.CatalogNumber);

            Assert.IsNotNull(dbContext);
            Assert.AreEqual(newAnimal.CatalogNumber, result.CatalogNumber);
            Assert.AreEqual(newAnimal.Name, result.Name);
            Assert.AreEqual(newAnimal.Breed, result.Breed);
            Assert.AreEqual(newAnimal.Type, result.Type);
            Assert.AreEqual(newAnimal.Age, result.Age);
            Assert.AreEqual(newAnimal.Gender, result.Gender);
            Assert.AreEqual(newAnimal.IsHealthy, result.IsHealthy);


            


            //Negative test
        }
                [Test]
        public async Task AddAnimalAsync_TryToAddAnimalWithInvalidCredentials_ShouldThrowException()
        {
            // Arrange
            var newAnimal = new Animal()
            {
                CatalogNumber = "00HNTWXTQSH4wwwww", //invaid input
                Name = "Puppi",
                Breed = "Cape fox",
                Type = "Mammal",
                Age = 3,
                Gender = "Malesssssssssssssssssssssss", //invalid input
                IsHealthy = true

            };

            // Act && Assert
            var ex = Assert.ThrowsAsync<ValidationException>(async () => await animalsManager.AddAsync(newAnimal));
            var actual = await dbContext.Animals.FirstOrDefaultAsync(c => c.CatalogNumber == newAnimal.CatalogNumber);

            Assert.IsNull(actual);
            Assert.That(ex.Message, Is.EqualTo("Invalid animal!"));



        }

        [Test]
        public async Task DeleteAnimalAsync_WithValidCatalogNumber_ShouldRemoveAnimalFromDb()
        {
            // Arrange
            var newAnimal = new Animal()
            {
                CatalogNumber = "00HNTWXTQSH4",
                Name = "Puppi",
                Breed = "Cape fox",
                Type = "Mammal",
                Age = 3,
                Gender = "Male",
                IsHealthy = true

            };
            await animalsManager.AddAsync(newAnimal);

            // Act
            await animalsManager.DeleteAsync(newAnimal.CatalogNumber);

            // Assert
            var animalInDB = await dbContext.Animals.FirstOrDefaultAsync(x => x.CatalogNumber == newAnimal.CatalogNumber); //Проверяваме, дали контактът е в БД

            Assert.IsNull(animalInDB);
        }

        [TestCase("")]
        [TestCase("  ")]
        [TestCase(null)]
        public async Task DeleteAnimalAsync_TryToDeleteWithNullOrWhiteSpaceCatalogNumber_ShouldThrowException(string invalidCatN)
        {


            // Act && Assert
            var exception = Assert.ThrowsAsync<ArgumentException>(() => animalsManager.DeleteAsync(invalidCatN));

            Assert.That(exception.Message, Is.EqualTo("Catalog number cannot be empty."));


        }

        [Test]
        public async Task GetAllAsync_WhenAnimalsExist_ShouldReturnAllAnimals()
        {
            // Arrange
            var newAnimal = new Animal()
            {
                CatalogNumber = "00HNTWXTQSH4",
                Name = "Puppi",
                Breed = "Cape fox",
                Type = "Mammal",
                Age = 3,
                Gender = "Male",
                IsHealthy = true

            };
            await animalsManager.AddAsync(newAnimal);

            var newAnimal2 = new Animal()
            {
                CatalogNumber = "01HNTWXTQSH4",
                Name = "Max",
                Breed = "Cape fox",
                Type = "Mammal",
                Age = 3,
                Gender = "Male",
                IsHealthy = false

            };
            await animalsManager.AddAsync(newAnimal2);

            // Act
            var result = await animalsManager.GetAllAsync();

            // Assert
            Assert.That(result.Count(), Is.EqualTo(2));

            var firstAnimal = result.First();
            Assert.That(firstAnimal.CatalogNumber, Is.EqualTo(newAnimal.CatalogNumber));
            Assert.That(firstAnimal.Name, Is.EqualTo(newAnimal.Name));
            Assert.That(firstAnimal.Breed, Is.EqualTo(newAnimal.Breed));
            Assert.That(firstAnimal.Type, Is.EqualTo(newAnimal.Type));
            Assert.That(firstAnimal.Age, Is.EqualTo(newAnimal.Age));
            Assert.That(firstAnimal.Gender, Is.EqualTo(newAnimal.Gender));
            Assert.That(firstAnimal.IsHealthy, Is.EqualTo(newAnimal.IsHealthy));


        }

        [Test]
        public async Task GetAllAsync_WhenNoAnimalsExist_ShouldThrowKeyNotFoundException()
        {
            // Arrange

            // Act
            var exception = Assert.ThrowsAsync<KeyNotFoundException>(() => animalsManager.GetAllAsync());

            // Assert
            Assert.That(exception.Message, Is.EqualTo("No animal found."));


            
        }

        [Test]
        public async Task SearchByTypeAsync_WithExistingType_ShouldReturnMatchingAnimals()
        {
            // Arrange
            var newAnimal = new Animal()
            {
                CatalogNumber = "00HNTWXTQSH4",
                Name = "Puppi",
                Breed = "Cape fox",
                Type = "kkk",
                Age = 3,
                Gender = "Male",
                IsHealthy = true

            };
            await animalsManager.AddAsync(newAnimal);

            var newAnimal2 = new Animal()
            {
                CatalogNumber = "01HNTWXTQSH4",
                Name = "Max",
                Breed = "Cape fox",
                Type = "Mammal",
                Age = 3,
                Gender = "Male",
                IsHealthy = false

            };
            await animalsManager.AddAsync(newAnimal2);

            // Act
            var result = await animalsManager.SearchByTypeAsync(newAnimal2.Type);

            // Assert
            Assert.That(result.Count(), Is.EqualTo(1));
            var animalsInDB = result.First();
            Assert.That(animalsInDB.CatalogNumber, Is.EqualTo(newAnimal2.CatalogNumber));
            Assert.That(animalsInDB.Name, Is.EqualTo(newAnimal2.Name));
            Assert.That(animalsInDB.Breed, Is.EqualTo(newAnimal2.Breed));
            Assert.That(animalsInDB.Type, Is.EqualTo(newAnimal2.Type));
            Assert.That(animalsInDB.Age, Is.EqualTo(newAnimal2.Age));
            Assert.That(animalsInDB.Gender, Is.EqualTo(newAnimal2.Gender));
            Assert.That(animalsInDB.IsHealthy, Is.EqualTo(newAnimal2.IsHealthy));
        }

        [Test]
        public async Task SearchByTypeAsync_WithNonExistingType_ShouldThrowKeyNotFoundException()
        {


            // Act
            var exception = Assert.ThrowsAsync<KeyNotFoundException>(() => animalsManager.SearchByTypeAsync("NO_SUCH_..."));


            // Assert
            Assert.That(exception.Message, Is.EqualTo("No animal found with the given type."));
        }

        [Test]
        public async Task GetSpecificAsync_WithValidCatalogNumber_ShouldReturnAnimal()
        {
            // Arrange
            var newAnimals = new List<Animal>()
            {
                new Animal()
                {
                   CatalogNumber = "00HNTWXTQSH4",
                Name = "Puppi",
                Breed = "Cape fox",
                Type = "kkk",
                Age = 3,
                Gender = "Male",
                IsHealthy = true
                },
                new Animal()
                {
                    CatalogNumber = "01HNTWXTQSH4",
                Name = "Max",
                Breed = "Cape fox",
                Type = "Mammal",
                Age = 3,
                Gender = "Male",
                IsHealthy = false
                }
            };

            foreach (var animal in newAnimals)
            {
                await animalsManager.AddAsync(animal);
            }

            // Act
            var result = await animalsManager.GetSpecificAsync(newAnimals[1].CatalogNumber);

            // Assert
            Assert.NotNull(result);
            Assert.That(result.Name, Is.EqualTo(newAnimals[1].Name));
            Assert.That(result.Breed, Is.EqualTo(newAnimals[1].Breed));
            Assert.That(result.Type, Is.EqualTo(newAnimals[1].Type));
            Assert.That(result.Age, Is.EqualTo(newAnimals[1].Age));
            Assert.That(result.Gender, Is.EqualTo(newAnimals[1].Gender));
            Assert.That(result.IsHealthy, Is.EqualTo(newAnimals[1].IsHealthy));
        }

        [Test]
        public async Task GetSpecificAsync_WithInvalidCatalogNumber_ShouldThrowKeyNotFoundException()
        {

            // Act && Assert
            const string invalidCatalogNumber = "NON_VALID_ID";

            var exception = Assert.ThrowsAsync<KeyNotFoundException>(() => animalsManager.GetSpecificAsync(invalidCatalogNumber));

            Assert.That(exception.Message, Is.EqualTo($"No animal found with catalog number: {invalidCatalogNumber}"));


        }

        [Test]
        public async Task UpdateAsync_WithValidAnimal_ShouldUpdateAnimal()
        {
            // Arrange
            var newAnimals = new List<Animal>()
            {
                new Animal()
                {
                   CatalogNumber = "00HNTWXTQSH4",
                Name = "Puppi",
                Breed = "Cape fox",
                Type = "kkk",
                Age = 3,
                Gender = "Male",
                IsHealthy = true
                },
                new Animal()
                {
                    CatalogNumber = "01HNTWXTQSH4",
                Name = "Max",
                Breed = "Cape fox",
                Type = "Mammal",
                Age = 3,
                Gender = "Male",
                IsHealthy = false
                }
            };

            foreach (var animal in newAnimals)
            {
                await animalsManager.AddAsync(animal);
            }
            var modifiedAnimal = newAnimals[0];
            modifiedAnimal.Name = "UPDATED!";
            // Act
            await animalsManager.UpdateAsync(modifiedAnimal);
            // Assert
            var animalInDb = await dbContext.Animals.FirstOrDefaultAsync(x => x.CatalogNumber == modifiedAnimal.CatalogNumber);

            Assert.NotNull(animalInDb);
            Assert.That(animalInDb.Name, Is.EqualTo(animalInDb.Name));
            Assert.That(animalInDb.Breed, Is.EqualTo(animalInDb.Breed));
            Assert.That(animalInDb.Type, Is.EqualTo(animalInDb.Type));
            Assert.That(animalInDb.Age, Is.EqualTo(animalInDb.Age));
            Assert.That(animalInDb.Gender, Is.EqualTo(animalInDb.Gender));
            Assert.That(animalInDb.IsHealthy, Is.EqualTo(animalInDb.IsHealthy));

        }

            [Test]
        public async Task UpdateAsync_WithInvalidAnimal_ShouldThrowValidationException()
        {


            // Act && Assert
            var exception = Assert.ThrowsAsync<ValidationException>(() => animalsManager.UpdateAsync(new Animal()));

            Assert.That(exception.Message, Is.EqualTo("Invalid animal!"));
        }
    }
}

