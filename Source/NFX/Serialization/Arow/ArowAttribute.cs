using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NFX.Serialization.Arow
{
  /// <summary>
  /// Denotes types that generate Arow ser/deser core
  /// </summary>
  [AttributeUsage(AttributeTargets.Class, AllowMultiple=false, Inherited=false)]
  public sealed class ArowAttribute : Attribute
  {
  }
}
