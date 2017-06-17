using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NFX.ApplicationModel.Pile
{
  /// <summary>
  /// Provides a central multiton interface for mapping PileID -> pile instance.
  /// This class is thread-safe.
  /// This class is used by system code and should not be used by business developers
  /// </summary>
  public class PileInstances
  {

    private static Dictionary<int, IPile> s_Instances = new Dictionary<int,IPile>();

    /// <summary>
    /// Internal method used by IPile implementation, business logic developers do not call
    /// </summary>
    public static bool _____Register(IPile instance)
    {
      if (instance==null) return false;
      var id = instance.Identity;
      if (id<=0) return false;

      lock(s_Instances)
      {
        if (s_Instances.ContainsKey(id)) return false;
        s_Instances.Add(id, instance);
        return true;
      }
    }

    /// <summary>
    /// Internal method used by IPile implementation, business logic developers do not call
    /// </summary>
    public static bool _____Unregister(IPile instance)
    {
      if (instance==null) return false;
      var id = instance.Identity;
      if (id<=0) return false;

      lock(s_Instances)
      {
        return s_Instances.Remove(id);
      }
    }

    /// <summary>
    /// Maps pileID to IPile intsance or null if not found
    /// </summary>
    public static IPile Map(int pileID)
    {
      if (pileID<=0) return null;

      IPile result;
      lock(s_Instances)
       if (s_Instances.TryGetValue(pileID, out result)) return result;

      return null;
    }
  }
}
