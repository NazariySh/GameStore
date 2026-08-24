namespace Gamestore.BLL.Tests.TestData;

public class InvalidEntityIdOrEmptyIdTestData : TheoryData<string>
{
    public InvalidEntityIdOrEmptyIdTestData()
    {
        Add(string.Empty);
        Add("  ");
        Add(Guid.Empty.ToString());
        Add((-10).ToString());
    }
}