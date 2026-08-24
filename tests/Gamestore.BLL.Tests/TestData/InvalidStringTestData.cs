namespace Gamestore.BLL.Tests.TestData;

public class InvalidStringTestData : TheoryData<string>
{
    public InvalidStringTestData()
    {
        Add(null!);
        Add(string.Empty);
        Add("  ");
    }
}