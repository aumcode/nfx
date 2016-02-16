using NFX.ApplicationModel.Pile;
using NFX.Environment;
using NUnit.Framework;

namespace NFX.NUnit.Integration.AppModel.Pile
{
    /// <summary>
    /// Base for all high-load tests
    /// </summary>
    public abstract class HighMemoryLoadTest
    {
        public abstract ulong MinRAM { get; }

        [TestFixtureSetUp]
        public void RigSetup()
        {
            var ms = NFX.OS.Computer.GetMemoryStatus();

            var has = ms.TotalPhysicalBytes;
            if (has < MinRAM)
                Assert.Ignore("The machine has to have at least {0:n0} bytes of ram for this test, but it only has {1:n0} bytes".Args(MinRAM, has));
        }
    }

    /// <summary>
    /// Base for all high-load tests for PCs with 32 GB RAM at least
    /// </summary>
    public class HighMemoryLoadTest32RAM : HighMemoryLoadTest
    {
        public override ulong MinRAM { get { return 32ul * 1000ul * 1000ul * 1000ul; } }
    }

    /// <summary>
    /// Base for all high-load tests for PCs with 64 GB RAM at least
    /// </summary>
    public class HighMemoryLoadTest64RAM : HighMemoryLoadTest
    {
        public override ulong MinRAM { get { return 64ul * 1000ul * 1000ul * 1000ul; } }
    }
}
