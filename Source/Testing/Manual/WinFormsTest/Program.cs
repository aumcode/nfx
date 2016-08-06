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
using System.Windows.Forms;

using NFX.ApplicationModel;
using NFX.WinForms;

namespace WinFormsTest
{
  static class Program
  {
    /// <summary>
    /// The main entry point for the application.
    /// </summary>
    [STAThread]
    static void Main(string[] args)
    {
      //This line initializes  NFX Application Model dependency injection services container
      //Separate class was needed because Application  class is sealed
      using(new ServiceBaseApplication(args, null))
      {
        Application.EnableVisualStyles();
       // Application.SetCompatibleTextRenderingDefault(false);

        Application.Run(new MenuForm());
     //   Application.Run( new ChartFormDemo());

      // Application.Run(new Form2());
      //    Application.Run(new MongoDBForm());
      //    Application.Run(new LogForm());

       //  Application.Run(new SerializerForm());
//          Application.Run(new SerializerForm2());

//        Application.Run(new WinFormsTest.Glue.JokeCalculatorClientForm());
         //Application.Run(new SerializerForm());
//         Application.Run(new SerializerForm2());
        
  //      Application.Run(new GlueForm());
       Application.Run(new ELinkForm());
   //     Application.Run(new CacheTest());

       // Application.Run(new QRTestForm());

     //  Application.Run(new BlankForm());
      // Application.Run(new WaveServerForm());
   //     Application.Run(new WaveForm());

  // Application.Run(new FIDForm());

     //   Application.Run(new WinFormsTest.ConsoleUtils.ConsoleUtilsFrm());
      }  

    //   Application.Run(new BlankForm());
    }
    
  }
}
