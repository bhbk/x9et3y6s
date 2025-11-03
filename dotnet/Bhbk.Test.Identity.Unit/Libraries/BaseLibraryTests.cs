using Bhbk.Test.Identity.Unit.TestingTools;
using Xunit;

[assembly: CollectionBehavior(DisableTestParallelization = true)]
[assembly: TestCollectionOrderer(CollectionOrdererHelper.TypeName, CollectionOrdererHelper.AssembyName)]
namespace Bhbk.Test.Identity.Unit.Libraries
{
    [CollectionDefinition("LibraryTests")]
    public class BaseLibraryTestsCollection : ICollectionFixture<BaseLibraryTests> { }

    public class BaseLibraryTests { }
}
