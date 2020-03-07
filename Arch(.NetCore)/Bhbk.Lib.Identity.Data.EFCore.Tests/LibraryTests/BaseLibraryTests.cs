using Xunit;

namespace Bhbk.Lib.Identity.Data.EFCore.Tests.LibraryTests
{
    [CollectionDefinition("LibraryTests")]
    public class BaseLibraryTestsCollection : ICollectionFixture<BaseLibraryTests> { }

    public class BaseLibraryTests { }
}
