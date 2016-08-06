/*<FILE_LICENSE>
* NFX (.NET Framework Extension) Unistack Library
* Copyright 2003-2014 IT Adapter Inc / 2015 Aum Code LLC
*
* Licensed under the Apache License, Version 2.0 (the "License");
* you may not use this file except in compliance with the License.
* You may obtain a copy of the License at
*
* http://www.apache.org/licenses/LICENSE-2.0
*
* Unless required by applicable law or agreed to in writing, software
* distributed under the License is distributed on an "AS IS" BASIS,
* WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
* See the License for the specific language governing permissions and
* limitations under the License.
</FILE_LICENSE>*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using NFX.Environment;

namespace NFX.Glue.Protocol
{
    /// <summary>
    /// Represents a header base - custom headers must inherit from this class
    /// </summary>
    [Serializable]
    public abstract class Header
    {

    }



    /// <summary>
    /// List of headers
    /// </summary>
    [Serializable]
    public sealed class Headers : List<Header>
    {
    }


    /// <summary>
    /// Provides general configuration reading logic for headers.
    /// Note: This class is not invoked by default glue runtime, so default application
    ///  configurations that include header injections will be ignored unless this class is specifically called
    ///  from code. This is because conf-based header injection is a rare case that may need to be controlled by a particular application
    /// </summary>
    public static class HeaderConfigurator
    {
       #region CONSTS

            public const string CONFIG_HEADERS_SECTION = "headers";

            public const string CONFIG_HEADER_SECTION = "header";

       #endregion

       public static void ConfigureHeaders(IList<Header> headers, IConfigSectionNode node)
       {
         node = node[CONFIG_HEADERS_SECTION];
         if (!node.Exists) return;

         foreach(var hnode in node.Children.Where(c => c.IsSameName(CONFIG_HEADER_SECTION)))
         {
           var header = FactoryUtils.Make<Header>(hnode);

           var chdr = header as IConfigurable;
           if (chdr!=null) chdr.Configure(hnode);

           headers.Add(header);
         }

       }

    }


}
