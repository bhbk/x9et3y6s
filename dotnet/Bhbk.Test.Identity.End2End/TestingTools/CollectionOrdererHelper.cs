using System.Collections.Generic;
using System.Linq;
using Xunit;
using Xunit.Abstractions;

namespace Bhbk.Test.Identity.End2End.TestingTools
{
    public class CollectionOrdererHelper : ITestCollectionOrderer
    {
        public const string AssembyName = "Bhbk.Test.Identity.End2End";

        public const string TypeName = "Bhbk.Test.Identity.End2End.TestingTools.CollectionOrdererHelper";

        public IEnumerable<ITestCollection> OrderTestCollections(IEnumerable<ITestCollection> testCollections)
        {
            return testCollections.OrderBy(collection => collection.DisplayName);
        }
    }
}
