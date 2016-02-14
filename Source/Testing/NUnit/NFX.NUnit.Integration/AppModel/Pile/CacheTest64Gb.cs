using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using NUnit.Framework;

using NFX.NUnit.AppModel.Pile;
using NFX.ApplicationModel.Pile;

namespace NFX.NUnit.Integration.AppModel.Pile
{
    public class CacheTest64gb : HighMemoryLoadTest64RAM
    {
        [TestCase(10000000, 1)]
        [TestCase(10000000, 16)]
        [TestCase(10000000, 512)]
        public void T190_FID_PutGetCorrectness(int cnt, int tbls)
        {
            PileCacheTestCore.FID_PutGetCorrectness(cnt, tbls);
        }

        [TestCase(16, 7, 25000, 40)]
        [TestCase(5, 20, 50000, 20)]
        [TestCase(16, 20, 150000, 40)]
        public void T9000000_ParalellGetPutRemove(int workers, int tables, int putCount, int durationSec)
        {
            PileCacheTestCore.ParalellGetPutRemove(workers, tables, putCount, durationSec);
        }
    }
}
