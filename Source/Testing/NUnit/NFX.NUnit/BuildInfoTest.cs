using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using NUnit.Framework;

using NFX.Environment;

namespace NFX.NUnit
{
  [TestFixture]
  public class BuildInfoTest
  {
    [Test]
    public void ForFramework()
    {
     Console.WriteLine(BuildInformation.ForFramework);
    }

  }
}
