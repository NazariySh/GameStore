namespace Gamestore.BLL.Tests.TestData;

public class InvalidGuidTestData : TheoryData<Guid>
{
    public InvalidGuidTestData()
    {
        Add(Guid.Empty);
    }
}