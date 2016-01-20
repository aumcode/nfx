using System;
using NUnit.Framework;
using System.Threading;
using System.Globalization;

namespace NFX.NUnit
{
    [SetUpFixture]
    class NUnitRootSetup
    {
        public object CulrureInfo { get; private set; }

        [SetUp]
        public void BeforeAnyTests()
        {
            CultureInfo.DefaultThreadCurrentCulture = CultureInfo.InvariantCulture;
        }

        [TearDown]
        public void AfterAllTests()
        {
        }
    }
}
