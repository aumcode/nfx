/*<FILE_LICENSE>
* NFX (.NET Framework Extension) Unistack Library
* Copyright 2003-2017 ITAdapter Corp. Inc.
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
using System.Threading;

using NFX;
using NFX.Wave;
using NFX.Wave.Handlers;
using NFX.Environment;
using NFX.Serialization.JSON;

namespace WaveTestSite.Handlers
{
  public class PushHandler : WorkHandler
  {
    protected PushHandler(WorkDispatcher dispatcher, string name, int order, WorkMatch match) : base(dispatcher, name, order, match){ ctor(); }
    protected PushHandler(WorkDispatcher dispatcher, IConfigSectionNode confNode) : base(dispatcher, confNode){ ctor();}

    private void ctor()
    {
      m_Works = new List<WorkContext>();
      m_Thread =new Thread(spin);
      m_Thread.Start();
    }

    protected override void Destructor()
    {
      base.Destructor();
      m_Thread.Join();

      foreach(var w in m_Works)
       w.Dispose();
    }

    private Thread m_Thread;
    private List<WorkContext> m_Works;


    protected override void DoHandleWork(WorkContext work)
    {
      //Prepare connection for SSE
      work.Response.ContentType = NFX.Web.ContentType.SSE;
      work.Response.Buffered = false;//
   //   work.Response.Write("".PadLeft(1000, ' '));//Microsoft bug. need 1000 chars at first to start buffer flushing
   //   work.Response.Write("Connection srarted at {0}\n".Args(App.TimeSource.UTCNow));
   //   work.Response.Flush();
      work.Response.Write("event:a\ndata: aaaa\n\n");

      work.NoDefaultAutoClose = true;



      lock(m_Works) m_Works.Add(work);
    }


    private void spin()
    {
      var last = DateTime.Now;
      while(App.Active)
      {
        var now = App.TimeSource.UTCNow;
        if ((now-last).TotalSeconds>2)
        {
         send();
         last=now;
        }
        Thread.Sleep(500);
      }
    }

    private void send()
    {
      WorkContext[] works;
      lock(m_Works) works = m_Works.ToArray();

      foreach(var w in works)
       try
       {
         var evt = "event: {0}\ndata:{1}\n\n".Args("teztEvent", new {a=1, dt=DateTime.Now, count=works.Length}.ToJSON());

         w.Response.Write(evt);
       }
       catch(Exception error)
       {
         App.Log.Write(new NFX.Log.Message{Type = NFX.Log.MessageType.Error, Text = error.ToMessageWithType(), Exception = error});
         lock(m_Works) m_Works.Remove(w);
         w.Dispose();
       }

    }


  }
}
