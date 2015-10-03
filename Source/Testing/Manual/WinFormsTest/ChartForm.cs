using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;


using NFX;
using NFX.WinForms;
using NFX.WinForms.Controls;
using NFX.WinForms.Controls.ChartKit;
using NFX.WinForms.Controls.ChartKit.Temporal;


namespace WinFormsTest
{
  public partial class ChartForm : Form
  {
    public ChartForm()
    {
      InitializeComponent();
    }

    private CandleTimeSeries m_Data;

    private void btnConnect_Click(object sender, EventArgs e)
    {
      if (m_Data!=null) return;
      m_Data = new CandleTimeSeries("My data", 0);
      
      var sd = DateTime.Now.AddSeconds(-500);

      var op = 120.11f;
      var cp = 123.23f;
      var hp = 200.92f;
      var lp = 110.11f;

      for (var i=0; i<150; i++)
      {
      
          m_Data.Add( 
            new CandleSample( sd )
            { 
              OpenPrice = op,
              ClosePrice = cp,
              HighPrice=hp,
              LowPrice=lp,
              BuyVolume=23,
              SellVolume=8
            });

        op += ExternalRandomGenerator.Instance.NextScaledRandomInteger(-2, +2);
        cp += ExternalRandomGenerator.Instance.NextScaledRandomInteger(-2, +2);
        hp += ExternalRandomGenerator.Instance.NextScaledRandomInteger(-2, +2);
        lp += ExternalRandomGenerator.Instance.NextScaledRandomInteger(-2, +2);

        sd = sd.AddSeconds(1);
      }


      m_Data.Views.Register( new CandleView("Candles", 0));
      m_Data.Views.Register( new CandleView("Avg1", 0, "Another Pane0"));
      m_Data.Views.Register( new CandleView("Avg2", 0, "Another Pane1"));
      //m_Data.Views.Register( new CandleView("Avg3", 0, "Another Pane2"));
      //m_Data.Views.Register( new CandleView("Avg4", 0, "Another Pane3"));
      //m_Data.Views.Register( new CandleView("Avg5", 0, "Another Pane4"));
      //m_Data.Views.Register( new CandleView("Avg6", 0, "Another Pane5"));


      chart.Series = m_Data;
    }

    private void timer_Tick(object sender, EventArgs e)
    {
      if (m_Data==null) return;


      if (chkSlide.Checked)
      {
        m_Data.Add( 
            new CandleSample( DateTime.Now )
            { 
              OpenPrice = 120.11f,
              ClosePrice = 123.23f,
              HighPrice=200.92f,
              LowPrice=110.11f,
              BuyVolume=23,
              SellVolume=8
            });
        return;
      }

      foreach(var sample in m_Data.Data)
      {
        sample.OpenPrice += ExternalRandomGenerator.Instance.NextScaledRandomInteger(-2, +2);
        sample.ClosePrice += ExternalRandomGenerator.Instance.NextScaledRandomInteger(-2, +2);
        sample.HighPrice += ExternalRandomGenerator.Instance.NextScaledRandomInteger(-2, +2);
        sample.LowPrice += ExternalRandomGenerator.Instance.NextScaledRandomInteger(-2, +2);
      }

      chart.NotifySeriesChange();
    }

    private void btnAnimate_Click(object sender, EventArgs e)
    {
      timer.Enabled = !timer.Enabled;
      Text = "Timer is: "+timer.Enabled;
    }

    private void btnGC_Click(object sender, EventArgs e)
    {
      GC.Collect();
    }


  }
}
