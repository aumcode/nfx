using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

using NFX;
using NFX.WinForms.Controls.ChartKit.Temporal;
using NFX.WinForms.Controls;
using NFX.Financial.Market;

namespace WinFormsTest
{
  public partial class GlueStressForm : Form
  {
    public GlueStressForm()
    {
      InitializeComponent();
      refresh();
    }

    private bool m_Running;
    private DateTime m_SD;
    private List<Thread> m_Threads = new List<Thread>();
    private int m_ThreadCount;
    private CandleTimeSeries m_Data;

    private int m_stat_TotalCallsOK;
    private int m_stat_TotalCallsError;

    private void tmr_Tick(object sender, EventArgs e)
    {
      refresh();
    }

    private void btnStart_Click(object sender, EventArgs e)
    {
      reset();
      m_Running = true;
    }

    private void btnStop_Click(object sender, EventArgs e)
    {
      m_Running = false;
      m_Threads.Clear();
    }

    private void btnReset_Click(object sender, EventArgs e)
    {
      reset();
    }

    private void refresh()
    {
     // btnStart.Enabled = !m_Running;
     // btnStop.Enabled = m_Running;

     // m_ThreadCount = cboThreads.Text.AsInt(0);
     //// var total = m_stat_TotalCalls;
     // lblTotalCalls.Text = "Total calls:   {0}".Args(total);
     // var duration = (DateTime.UtcNow - m_SD).TotalSeconds;
     // if (duration==0) duration = 1;
     // lblDuration.Text = "Duration calls:   {0}".Args(duration);
     // var throughput = (int)(total / duration);
     // lblThroughput.Text = "Throughput calls/sec:   {0}".Args(throughput);

     // if (!m_Running) return;

     // setThreads(cboThreads.Text.AsInt(0));


     // var sample = new CandleSample(DateTime.Now)
     // {
     //    OpenPrice = 100 * (float)NFX.ExternalRandomGenerator.Instance.NextRandomDouble,
     //    ClosePrice = 100 * (float)NFX.ExternalRandomGenerator.Instance.NextRandomDouble,
     //  //  ClosePrice = 100 * (float)NFX.ExternalRandomGenerator.Instance.NextRandomDouble,
     //     Count = 1,
     //  //    HighPrice = 200 * (float)NFX.ExternalRandomGenerator.Instance.NextRandomDouble,
     //   TimeSpanMs =10
     // };

     // m_Data.Add( sample );

     // chart.NotifySeriesChange();
    }

    private void reset()
    {
      m_SD = DateTime.UtcNow;
   //   m_stat_TotalCalls = 0;
      m_Data = new CandleTimeSeries("Throughput", 0);
      m_Data.Views.Register( new CandleView("Candles", 0));
      //m_Data.Views.Register(
      //      new CandleMidLineView("MidLineHiLo", 1)
      //      {
      //        MidLineType = MidLineType.OpenClose,
      //        LineStyle = new LineStyle{ Color = Color.FromArgb(200, 255, 0,0), Width = 1}
      //      });

      chart.Series = m_Data;
    }

    private void setThreads(int cnt)
    {
      while(m_Threads.Count < cnt)
      {
        var t = new Thread(threadSpin);
        t.Start();
      }

    }

    private void threadSpin(object n)
    {
      var tid = (int)n;
      try
      {
        threadSpinCore(tid);
      }
      catch(Exception error)
      {
        //log onscreen
      }
    }

    private void threadSpinCore(int tid)
    {
      while(m_Running)
      {
        try
        {
          //make call

          Interlocked.Increment(ref m_stat_TotalCallsOK);
        }
        catch(Exception error)
        {
          Interlocked.Increment(ref m_stat_TotalCallsError);
        }
      }

    }


  }
}
