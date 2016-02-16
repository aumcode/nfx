using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NFX.ServiceModel
{
  /// <summary>
  /// Designates service-derivative classes that should NOT be auto-started by the app container
  /// </summary>
  [AttributeUsage(AttributeTargets.Class, AllowMultiple=false, Inherited=false)]
  public class ApplicationDontAutoStartServiceAttribute : Attribute
  {
    public ApplicationDontAutoStartServiceAttribute(){}
  }
}
