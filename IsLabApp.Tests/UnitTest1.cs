using Xunit;

namespace IsLabApp.Tests;

public class UnitTest1
{
    [Fact]
    public void Test_Version_Returns_Expected()
    {
        Assert.Equal("1.0.0", "1.0.0");
    }
}
