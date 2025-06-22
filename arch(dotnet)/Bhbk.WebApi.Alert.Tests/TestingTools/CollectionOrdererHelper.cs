using System.Collections.Generic;
using System.Linq;
using Xunit;
using Xunit.Abstractions;

namespace Bhbk.WebApi.Alert.Tests.TestingTools
{
    public class CollectionOrdererHelper : ITestCollectionOrderer
    {
        public const string AssembyName = "Bhbk.WebApi.Alert.Tests";

        public const string TypeName = "Bhbk.WebApi.Alert.Tests.TestingTools.CollectionOrdererHelper";

        public IEnumerable<ITestCollection> OrderTestCollections(IEnumerable<ITestCollection> testCollections)
        {
            return testCollections.OrderBy(collection => collection.DisplayName);
        }
    }
}
