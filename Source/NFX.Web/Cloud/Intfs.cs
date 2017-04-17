using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using NFX;
using NFX.Environment;
using NFX.Instrumentation;
using NFX.Log;

namespace NFX.Web.Cloud
{
  public interface ICloudSystem : INamed
  {
    IConfigSectionNode DefaultSessionConnectParamsCfg { get; set; }
    CloudSession StartSession(CloudConnectionParameters cParams = null);

    void Deploy(CloudSession session, string id, CloudTemplate template, IConfigSectionNode customData);
  }

  public interface ICloudSystemImplementation : ICloudSystem, IConfigurable, IInstrumentable
  {
    MessageType LogLevel { get; set; }
  }
}
