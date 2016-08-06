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
using System.Reflection;
using System.IO;
using System.Collections.Specialized;

using NFX.Environment;

namespace NFX.Health
{
    /// <summary>
    /// Stipulates status for Health.CheckList
    /// </summary>
    public enum CheckListStatus { Created, Running,  Run  }

    /// <summary>
    /// Represents a list of health checks to be performed.
    /// Health checks are classes derived from BaseCheck class and must implement DoRun() method
    ///  that performs application-specific checking. CheckList obtains a list of Check instances
    ///   from assemblies supplied to its constructor.
    /// Health checks are executed in no predictable order.
    /// If particular execution order is necessary then a check has to be written to explicitly
    ///  coordinate sub-checks.
    /// The class is thread safe for all operations.
    /// </summary>
    public sealed class CheckList
    {
       #region .ctor

        /// <summary>
        /// Initializaes check list from assembly names separated by semicolons.
        /// Optional path will be prepended to every assembly name
        /// </summary>
        public CheckList(string path, string checkAssemblies, ConfigSectionNode config)
        {
          path = path ?? string.Empty;

          var anames = checkAssemblies.Split(';');

          var al = new List<Assembly>();
          foreach(var aname in anames)
            al.Add(Assembly.LoadFrom(Path.Combine(path, aname)));

          load(al, config);
        }

        /// <summary>
        /// Initializaes check list from assemblies
        /// </summary>
        public CheckList(IEnumerable<Assembly> checkAssemblies, ConfigSectionNode config)
        {
          load(checkAssemblies, config);
        }

       #endregion

       #region Private Fields

         private CheckListStatus m_Status = CheckListStatus.Created;

         private List<BaseCheck> m_Checks = new List<BaseCheck>();

         private DateTime? m_RunStart;
         private DateTime? m_RunFinish;
       #endregion


       #region Properties
         /// <summary>
         /// Returns current status - whether checks are running  or have already been run
         /// </summary>
         public CheckListStatus Status
         {
           get { return m_Status;}
         }

         /// <summary>
         /// Returns health checks
         /// </summary>
         public IEnumerable<BaseCheck> Checks
         {
           get { return m_Checks; }//this is thread safe as list is never written to but .ctor
         }


         /// <summary>
         /// Returns true when all checks that could be run have successfuly run
         /// </summary>
         public bool Successful
         {
           get
           {
             return m_Status == CheckListStatus.Run &&
                    m_Checks.Where(c=> !c.Result.Skipped && !c.Result.Successful).Count()==0;
           }
         }

         /// <summary>
         /// Returns true when all checks have successfuly run
         /// </summary>
         public bool AllSuccessful
         {
           get
           {
             return m_Status == CheckListStatus.Run &&
                    m_Checks.Where(c=>!c.Result.Successful).Count()==0;
           }
         }

         /// <summary>
         /// Returns when run started
         /// </summary>
         public DateTime? RunStart
         {
           get { return m_RunStart; }
         }

         /// <summary>
         /// Returns when run finished
         /// </summary>
         public DateTime? RunFinish
         {
           get { return m_RunFinish; }
         }


       #endregion

        /// <summary>
        /// Runs health checks. Checks are ran in NO PARTICULAR order.
        /// This method can not be called twice
        /// </summary>
        public void Run(NameValueCollection parameters)
        {
           if (m_Status != CheckListStatus.Created)
            throw new HealthCheckListException(StringConsts.CHECK_LIST_ALREADY_RUN_ERROR);

           try
           {
             m_Status = CheckListStatus.Running;
             m_RunStart = App.LocalizedTime;
             foreach(var check in m_Checks)
              if (check.CanRun)
               check.Run(parameters);
              else
               check.Result.Skipped = true;
           }
           finally
           {
             m_Status = CheckListStatus.Run;
             m_RunFinish = App.LocalizedTime;
           }
        }

        /// <summary>
        /// Dumps check results into writer in on of the supported formats
        /// </summary>
        public string Report(TextWriter writer, string format)
        {
           var contentType = "text/xml";

           format = (format ?? string.Empty).Trim().ToLower();

           Reporter reporter = null;
           switch (format)
           {
             case "html":
             case "htm":
                          {
                             contentType = "text/html";
                             reporter = new HTMLReporter(this);
                             break;
                          }

             case "text":
             case "txt":
                          {
                             contentType = "text/plain";
                             reporter = new TextReporter(this);
                             break;
                          }

             default: //xml
               reporter = new XMLReporter(this); break;
           }

           reporter.Report(writer);

           return contentType;
        }



        #region .pvt .impl



         private void load(IEnumerable<Assembly> assemblies, ConfigSectionNode config)
         {
           foreach(var assembly in assemblies)
             foreach(var type in assembly.GetExportedTypes().Where(t=>t.IsSubclassOf(typeof(BaseCheck))))
             {
               var check = Activator.CreateInstance(type) as BaseCheck;

               if (config!=null)
                check.Configure(config);

               m_Checks.Add(check);
             }
         }

       #endregion


    }
}
