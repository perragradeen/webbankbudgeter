using BudgeterCore.Entities;
using System.Text.Json;

namespace FileTests
{
    [TestClass]
    public class JsonSerializerTest
    {
        private static string PathToTheFile => Path.Combine(
            AppDomain.CurrentDomain.BaseDirectory,
            @"testFile.json"
        );

        [TestMethod]
        public void TestSerializeList()
        {
            WriteData();
        }

        private static void WriteData()
        {
            var testData = GetTestData();

            // Write to file
            var jsonString =
                JsonSerializer.Serialize(testData);
            File.WriteAllText(PathToTheFile, jsonString);
        }

        private static List<InBudget> GetTestData()
        {
            // Arrange
            return new()
            {
                new InBudget {BudgetValue = 1},
                new InBudget {BudgetValue = 2},
            };
        }

        [TestMethod]
        public void TestDeserializeList()
        {
            // Arrange
            WriteData();

            var jsonString = File.ReadAllText(PathToTheFile);

            var testData = JsonSerializer
                .Deserialize<List<InBudget>>(jsonString);

            Assert.IsNotNull(testData);
            Assert.AreEqual(1, testData.First().BudgetValue);
        }
    }
}