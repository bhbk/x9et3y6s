using System.Collections.Generic;
using System.Linq;
using Xunit;
using Xunit.Abstractions;

namespace Bhbk.Lib.Identity.Data.EF6.Tests.TestingTools
{
    public class CollectionOrdererHelper : ITestCollectionOrderer
    {
        public const string AssembyName = "Bhbk.Lib.Identity.Data.EF6.Tests";

        public const string TypeName = "Bhbk.Lib.Identity.Data.EF6.Tests.TestingTools.CollectionOrdererHelper";

        public IEnumerable<ITestCollection> OrderTestCollections(IEnumerable<ITestCollection> testCollections)
        {
            /*
             * certain collections of tests need to run before others. the specific need here is for the 
             * tests that use the in-memory sql provider run before the tests that use the normal sql provider.
             */

            return testCollections.OrderBy(collection => collection.DisplayName);
        }
    }
}
