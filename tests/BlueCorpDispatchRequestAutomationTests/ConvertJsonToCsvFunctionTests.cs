using System.Text;
using Microsoft.Extensions.Logging;
using Moq;
using BlueCorpDispatchAuto;


public class ConvertJsonToCsvFunctionTests
{
    private readonly Mock<ILogger<ConvertJsonToCsvFunction>> _loggerMock;
    private readonly Mock<IQueueService> _queueServiceMock;
    private readonly ConvertJsonToCsvFunction _function;

    private readonly string _testFilesPath = Path.Combine(Directory.GetCurrentDirectory(), "TestFiles");

    public ConvertJsonToCsvFunctionTests()
    {
        _loggerMock = new Mock<ILogger<ConvertJsonToCsvFunction>>();
        _queueServiceMock = new Mock<IQueueService>();
        _function = new ConvertJsonToCsvFunction(_loggerMock.Object, _queueServiceMock.Object);
    }

    [Fact]
    public async Task Run_ValidJson_EnqueuesCsv()
    {
        // Arrange
        string jsonFilePath = Path.Combine(_testFilesPath, "bluecorp-ready-for-dispatch-event.json");
        string expectedCsvFilePath = Path.Combine(_testFilesPath, "bluecorp-3pl.csv");

        Assert.True(File.Exists(jsonFilePath), $"Test JSON file not found: {jsonFilePath}");
        Assert.True(File.Exists(expectedCsvFilePath), $"Expected CSV file not found: {expectedCsvFilePath}");

        string jsonContent = File.ReadAllText(jsonFilePath);
        string expectedCsvContent = File.ReadAllText(expectedCsvFilePath);

        string fileName = "test.csv";
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(jsonContent));

        _queueServiceMock.Setup(s => s.EnqueueMessageAsync("processed-csv", fileName, expectedCsvContent))
                         .Returns(Task.CompletedTask)
                         .Verifiable();

        // Act
        await _function.Run(stream, "test.json");

        // Assert
        _queueServiceMock.Verify(s => s.EnqueueMessageAsync("processed-csv", fileName, expectedCsvContent), Times.Once);
    }

    [Fact]
    public async Task Run_EmptyJson_DoesNotEnqueue()
    {
        // Arrange
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(""));

        // Act
        await _function.Run(stream, "empty.json");

        // Assert
        _queueServiceMock.Verify(s => s.EnqueueMessageAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task Run_ExceptionThrown_LogsError()
    {
        // Arrange
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes("invalid json"));
        _queueServiceMock.Setup(s => s.EnqueueMessageAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                         .ThrowsAsync(new Exception("Queue error"));

        // Act
        await _function.Run(stream, "error.json");

        // Assert
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Error processing JSON")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()
            ), Times.Once);
    }
}