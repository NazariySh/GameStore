using System.Text.Json;
using Gamestore.BLL.Tests.TestData;
using Gamestore.BLL.Utilities;

namespace Gamestore.BLL.Tests.Utilities;

public class FileContentExporterTests
{
    private const string TextContentType = "text/plain";
    private const string TextFileExtension = ".txt";

    [Fact]
    public void ExportToTextFile_ShouldThrowArgumentNullException_WhenInstanceIsNull()
    {
        const string fileName = "fileName";
        TestClass? instance = null;

        var act = () => FileContentExporter.ExportToTextFile(instance!, fileName);

        Assert.Throws<ArgumentNullException>(act);
    }

    [Theory]
    [ClassData(typeof(InvalidStringTestData))]
    public void ExportToTextFile_ShouldThrowArgumentException_WhenFileNameIsInvalid(string invalidFileName)
    {
        var instance = GetInstance();

        var act = () => FileContentExporter.ExportToTextFile(instance, invalidFileName);

        Assert.ThrowsAny<ArgumentException>(act);
    }

    [Fact]
    public void ExportToTextFile_ShouldReturnCorrectFileContent_WhenInstanceIsValid()
    {
        const string fileName = "fileName";
        var instance = GetInstance();

        var result = FileContentExporter.ExportToTextFile(instance, fileName);

        Assert.NotNull(result);
        Assert.Contains(fileName, result.FileName);
        Assert.Equal(TextContentType, result.ContentType);

        var expectedContent = JsonSerializer.SerializeToUtf8Bytes(instance, FileContentExporter.JsonOptions);
        Assert.Equal(expectedContent, result.Content);
    }

    [Fact]
    public void ExportToTextFile_ShouldReturnFileNameWithTxtExtension()
    {
        const string fileName = "fileName";
        var instance = GetInstance();

        var result = FileContentExporter.ExportToTextFile(instance, fileName);

        Assert.StartsWith(fileName, result.FileName);
        Assert.EndsWith(TextFileExtension, result.FileName);
    }

    private static TestClass GetInstance()
    {
        return new TestClass
        {
            Name = "Sample",
            Value = 42,
        };
    }

    public class TestClass
    {
        public string Name { get; set; }

        public int Value { get; set; }
    }
}