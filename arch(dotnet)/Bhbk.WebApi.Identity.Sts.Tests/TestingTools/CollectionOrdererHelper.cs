using System.Collections.Generic;
using System.Linq;
using Xunit;
using Xunit.Abstractions;

namespace Bhbk.WebApi.Identity.Sts.Tests.TestingTools
{
    public class CollectionOrdererHelper : ITestCollectionOrderer
    {
        public const string AssembyName = "Bhbk.WebApi.Identity.Sts.Tests";

        public const string TypeName = "Bhbk.WebApi.Identity.Sts.Tests.TestingTools.CollectionOrdererHelper";

        public IEnumerable<ITestCollection> OrderTestCollections(IEnumerable<ITestCollection> testCollections)
        {
            return testCollections.OrderBy(collection => collection.DisplayName);
        }
    }
}
