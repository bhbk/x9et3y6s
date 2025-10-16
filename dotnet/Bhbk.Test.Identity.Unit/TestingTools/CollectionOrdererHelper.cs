using System.Collections.Generic;
using System.Linq;
using Xunit;
using Xunit.Abstractions;

namespace Bhbk.Test.Identity.Unit.TestingTools
{
    public class CollectionOrdererHelper : ITestCollectionOrderer
    {
        public const string AssembyName = "Bhbk.Test.Identity.Unit";

        public const string TypeName = "Bhbk.Test.Identity.Unit.TestingTools.CollectionOrdererHelper";

        public IEnumerable<ITestCollection> OrderTestCollections(IEnumerable<ITestCollection> testCollections)
        {
            return testCollections.OrderBy(collection => collection.DisplayName);
        }
    }
}
