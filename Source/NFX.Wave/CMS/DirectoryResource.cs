using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using NFX.Environment;
using NFX.DataAccess.CRUD;

namespace NFX.Wave.CMS
{
  [Serializable]
  public class DirectoryResource : Resource
  {

     IEnumerable<string> GetConfigNames(ICacheParams caching = null)
     {
       return null;
     }

     IEnumerable<string> GetFileNames(ICacheParams caching = null)
     {
       return null;
     }

     /// <summary>
     /// Returns Config by name or null
     /// </summary>
     public ConfigResource GetConfig(string name, ICacheParams caching = null)
     {
       return null;
     }


     /// <summary>
     /// Returns Config by name or null
     /// </summary>
     public FileResource GetFile(string name, ICacheParams caching = null)
     {
       return null;
     }


  }
}
