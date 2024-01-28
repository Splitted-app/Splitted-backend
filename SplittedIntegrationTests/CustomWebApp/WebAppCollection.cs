using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SplittedIntegrationTests.CustomWebApp
{
    [CollectionDefinition("WebApp Collection")]
    public class WebAppCollection : ICollectionFixture<DatabaseFixture>, ICollectionFixture<CustomWebApplicationFactory>
    {
    }
}
