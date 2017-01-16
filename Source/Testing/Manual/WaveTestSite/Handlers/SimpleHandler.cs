using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using NFX;
using NFX.Wave;
using NFX.Wave.Handlers;
using NFX.Environment;

namespace WaveTestSite.Handlers
{
  public class SimpleHandler : WorkHandler
  {
    protected SimpleHandler(WorkDispatcher dispatcher, string name, int order, WorkMatch match) : base(dispatcher, name, order, match){ }
    protected SimpleHandler(WorkDispatcher dispatcher, IConfigSectionNode confNode) : base(dispatcher, confNode){ }

    protected override void DoHandleWork(WorkContext work)
    {
      work.Response.Write("{0} came".Args(5));
    }
  }
}
