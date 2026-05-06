using Xunit;
using GenMail.Core;

namespace GenMail.Core.Tests;

public sealed class GenMailStudioInfoTests
{
    [Fact]
    public void Constructor_SetsProperties()
    {
        GenMailStudioInfo info = new("GenMail Studio", "0.1.0");

        Assert.Equal("GenMail Studio", info.ProductName);
        Assert.Equal("0.1.0", info.Version);
    }
}
