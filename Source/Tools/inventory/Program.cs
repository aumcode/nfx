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

using NFX;
using NFX.Environment;
using NFX.Inventorization;
using NFX.IO;

namespace inventory
{
    class Program
    {
        static void Main(string[] args)
        {
          try
          {
           run(args);
           Environment.ExitCode = 0;
          }
          catch(Exception error)
          {
           ConsoleUtils.Error(error.ToMessageWithType());
           Environment.ExitCode = -1;
          }
        }

        private static void run(string[] args)
        {
          var config = new CommandArgsConfiguration(args);


          if (config.Root["?"].Exists ||
              config.Root["h"].Exists ||
              config.Root["help"].Exists)
          {
             ConsoleUtils.WriteMarkupContent( typeof(Program).GetText("Help.txt") );
             return;
          }


          if (!config.Root.AttrByIndex(0).Exists)
          {
            Console.WriteLine("Specify ';'-delimited assembly list");
            return;
          }



          var manager = new InventorizationManager(config.Root.AttrByIndex(0).Value);

          var fnode = config.Root["f"];
          if (!fnode.Exists) fnode = config.Root["filter"];
          if (fnode.Exists)
           ConfigAttribute.Apply(manager, fnode);

          foreach(var n in config.Root.Children.Where(chi=>chi.IsSameName("s") || chi.IsSameName("strat") || chi.IsSameName("strategy")))
          {
            var tname = n.AttrByIndex(0).Value ?? "<unspecified>";
            Type t = Type.GetType(tname);
            if (t == null)
               throw new NFXException("Can not create strategy type: " + tname);

            var strategy = Activator.CreateInstance(t) as IInventorization;

            if (strategy == null)
               throw new NFXException("The supplied type is not strategy: " + tname);

            manager.Strategies.Add(strategy);
          }

          if (manager.Strategies.Count==0)
          {
            manager.Strategies.Add( new BasicInventorization());
          }




           // if (config.Root["any"].Exists)
           //  manager.OnlyAttributed = false;

            var result = new XMLConfiguration();
            result.Create("inventory");
            manager.Run(result.Root);
            Console.WriteLine(result.SaveToString());

        }
    }
}
