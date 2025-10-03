using Xunit;

namespace Bhbk.Lib.Identity.Data.EF.Tests.LLMTests
{
    [CollectionDefinition("LLMTests")]
    public class BaseLLMTestsCollection : ICollectionFixture<BaseLLMTests> { }

    public class BaseLLMTests
    {
        // Base fixture for LLM tests
        // Add shared setup logic here if needed
    }
}
