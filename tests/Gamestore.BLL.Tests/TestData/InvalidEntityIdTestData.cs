namespace Gamestore.BLL.Tests.TestData;

public class InvalidEntityIdTestData : TheoryData<string>
{
    public InvalidEntityIdTestData()
    {
        Add(Guid.Empty.ToString());
        Add((-10).ToString());
    }
}