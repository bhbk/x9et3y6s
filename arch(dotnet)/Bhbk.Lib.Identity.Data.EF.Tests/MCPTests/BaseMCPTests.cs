using Xunit;

namespace Bhbk.Lib.Identity.Data.EF.Tests.MCPTests
{
    [CollectionDefinition("MCPTests")]
    public class BaseMCPTestsCollection : ICollectionFixture<BaseMCPTests> { }

    public class BaseMCPTests
    {
        // Base fixture for MCP tests
        // Add shared setup logic here if needed
    }
}
