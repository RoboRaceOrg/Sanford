using BlueCorpDispatchAuto;
    public class JsonToCsvConverterTests
    {
        private readonly string _testFilesPath = Path.Combine(Directory.GetCurrentDirectory(), "TestFiles");

        [Fact]
        public void ConvertJsonToCsv_ShouldProduceExpectedCsvOutput()
        {
            // Arrange
            string jsonFilePath = Path.Combine(_testFilesPath, "bluecorp-ready-for-dispatch-event.json");
            string expectedCsvFilePath = Path.Combine(_testFilesPath, "bluecorp-3pl.csv");

            Assert.True(File.Exists(jsonFilePath), $"Test JSON file not found: {jsonFilePath}");
            Assert.True(File.Exists(expectedCsvFilePath), $"Expected CSV file not found: {expectedCsvFilePath}");

            string jsonContent = File.ReadAllText(jsonFilePath);
            string expectedCsvContent = File.ReadAllText(expectedCsvFilePath);

            // Act
            string actualCsvContent = JsonToCsvConverter.ConvertJsonToCsv(jsonContent);

            // Assert
            Assert.Equal(NormalizeLineEndings(expectedCsvContent), NormalizeLineEndings(actualCsvContent));
        }

        private string NormalizeLineEndings(string input)
        {
            return input.Replace("\r\n", "\n").Replace("\r", "\n");
        }
    }
