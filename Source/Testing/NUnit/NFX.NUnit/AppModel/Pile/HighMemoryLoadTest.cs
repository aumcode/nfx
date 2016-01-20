using NUnit.Framework;

namespace NFX.NUnit.AppModel.Pile
{
    /// <summary>
    /// Base for all high-load tests
    /// </summary>
    public class HighMemoryLoadTest
    {
        [TestFixtureSetUp]
        public void RigSetup()
        {
            var ms = NFX.OS.Computer.GetMemoryStatus();

            const ulong MIN = 64ul * 1000ul * 1000ul * 1000ul;

            var has = ms.TotalPhysicalBytes;
            if (has < MIN)
                Assert.Ignore("The machine has to have at least {0:n0} bytes of ram for this test, but it only has {1:n0} bytes".Args(MIN, has));
        }
    }
}
