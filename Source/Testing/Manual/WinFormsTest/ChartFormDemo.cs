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
using NFX.IO.FileSystem;
using NFX.IO.FileSystem.Local;
using NFX.Financial.Market;
using NFX.Financial.Market.SecDb;
using NFX.WinForms.Controls;
using NFX.WinForms.Controls.ChartKit;
using NFX.WinForms.Controls.ChartKit.Temporal;

namespace WinFormsTest
{
  public partial class ChartFormDemo : Form
  {
    public ChartFormDemo()
    {
      InitializeComponent();
    }

    private void ChartFormDemo_Load(object sender, EventArgs e)
    {
      cboFile.Items.AddRange( @"c:\nfx\".AllFileNamesThatMatch("*.sdb", false).ToArray() );
      cboResolution.SelectedIndex = 3;
      cboVolumeKind.SelectedIndex = 1;
      chart.SetPaneVProportion("Price", 0.75f);
      chart.SetPaneVProportion("Volume", 0.20f);
    }

    private LocalFileSystem m_FS = new LocalFileSystem();
    private SecDBFileReader m_SecDBFile;

    private void cboFile_SelectedValueChanged(object sender, EventArgs e)
    {
      m_SecDBFile = new SecDBFileReader(m_FS, new FileSystemSessionConnectParams(), cboFile.Text);
      Text = "Exchange: {0}, Origin time: {1}, {2}".Args(m_SecDBFile.SystemHeader.Exchange,
                                                        m_SecDBFile.SystemHeader.Date+m_SecDBFile.SystemHeader.OriginLocalTimeOffset, 
                                                        m_SecDBFile.SystemHeader.OriginLocalTimeName); 
      loadData();
    }


    private CandleSample[] m_OriginalDataFromFile; 

    private void loadData()
    {
      if (m_SecDBFile==null)
      {
       chart.Series = null;
       return;
      }

      var series = new CandleTimeSeries("Candles", 0);
      m_OriginalDataFromFile = m_SecDBFile.GetAllStreamData()
                                              .SynthesizeCandles(m_SecResolution)
                                              .ToArray();

      m_OriginalDataFromFile.ForEach( s =>  series.Add( s ));

      series.Views.Register( 
            new CandleView("Candles", 1)
            {
              ShowYLevels = true,
              ShowBalloons = true
            });

      series.Views.Register( new CandleBuySellView("Volume", 2)
                            { 
                             Visible = chkVolumes.Checked,
                             Kind = getVolumeKind()
                            } );
      series.Views.Register( 
            new CandleMidLineView("MidLine", 3)
            {
              MidLineType = MidLineType.HighLow,
              LineStyle = new LineStyle{ Color = Color.FromArgb(200, 255, 0,0), Width = 2},
              Visible = chkMidLines.Checked
            });

      series.YLevels.Register(
            new TimeSeries.YLevel("FIRST_PRICE", 1)
            {
              Visible =  chkMidLevels.Checked,
              HLineStyle = new LineStyle{ Color = Color.FromArgb(255, 20, 10), DashStyle = System.Drawing.Drawing2D.DashStyle.Dash}
            });

      series.YLevels.Register(
            new TimeSeries.YLevel("MID_PRICE", 2)
            {
              Visible =  chkMidLevels.Checked,
              HLineStyle = new LineStyle{ Color = Color.FromArgb(0, 180, 80), DashStyle = System.Drawing.Drawing2D.DashStyle.Dot}
            });

      series.YLevels.Register(
            new TimeSeries.YLevel("LAST_PRICE", 3)
            {
              Value =  series.DataReveresed.First().ClosePrice,
              HLineStyle = new LineStyle{ Color = Color.FromArgb(120, 40, 255), DashStyle = System.Drawing.Drawing2D.DashStyle.Dash}
            });

      updateLevels(series);

      chart.Series = series;
    }


    private void chkMidLines_CheckedChanged(object sender, EventArgs e)
    {
      var series = chart.Series;
      if (series==null) return;
      series.Views["MidLine"].Visible = chkMidLines.Checked;

      chart.NotifySeriesChange(true);
    }

    private void chkVolumes_CheckedChanged(object sender, EventArgs e)
    {
      var series = chart.Series;
      if (series==null) return;
      series.Views["Volume"].Visible = chkVolumes.Checked;

      chart.NotifySeriesChange(true);
    }


    private uint m_SecResolution = 5;

    private void cboResolution_TextChanged(object sender, EventArgs e)
    {
      var str = cboResolution.Text.Split(' ');
      var i = str[0].AsInt();

      if (str[1]=="min") i *= 60;
      if (str[1]=="hr") i *= 3600;

      m_SecResolution = (uint)i;

      loadData();
    }

    private int timerX;

    private void tmrUpdate_Tick(object sender, EventArgs e)
    {
      if (!chkRealtime.Checked) return;
      
      var series = chart.Series as CandleTimeSeries;
      if (series==null) return;
      if (m_OriginalDataFromFile==null) return;


      var sample = m_OriginalDataFromFile.Skip(timerX).FirstOrDefault();
      if (sample==null)
      {
       timerX = 0;
       sample = m_OriginalDataFromFile.Skip(timerX).FirstOrDefault();
      }
      else
       timerX++;

      if (sample==null) return;

      var last = series.DataReveresed.First();

      var newSample = new CandleSample(last.TimeStamp.AddSeconds(m_SecResolution))
      {
        OpenPrice = sample.OpenPrice,
        ClosePrice = sample.ClosePrice,
        HighPrice = sample.HighPrice,
        LowPrice = sample.LowPrice,
        BuyVolume = sample.BuyVolume,
        SellVolume = sample.SellVolume,
        TimeSpanMs = sample.TimeSpanMs
      };

      series.Add( newSample );

      updateLevels(series);

      chart.NotifySeriesChange();
    }

    private void updateLevels(CandleTimeSeries series)
    {
      var last = series.DataReveresed.First();
      var fp = series.Data.First().ClosePrice;
      var lp = last.ClosePrice;
      var mp = (fp + lp) / 2f;

      series.YLevels["FIRST_PRICE"].Value =  fp;
      series.YLevels["MID_PRICE"].Value =  mp;
      series.YLevels["LAST_PRICE"].Value =  lp;
    }

    private void chkAutoScroll_CheckedChanged(object sender, EventArgs e)
    {
      chart.AutoScroll = chkAutoScroll.Checked;
      chart.NotifySeriesChange();
    }

    private void cboVolumeKind_TextChanged(object sender, EventArgs e)
    {
      if (chart.Series==null) return;

      var kind = getVolumeKind();
      var view = chart.Series.Views["Volume"] as CandleBuySellView;
      if (view==null) return;
      view.Kind = kind;
      
      chart.NotifySeriesChange();
    }

    private CandleBuySellView.ViewKind getVolumeKind()
    {
      CandleBuySellView.ViewKind kind;
      if (Enum.TryParse(cboVolumeKind.Text, true, out kind)) return kind;

      return CandleBuySellView.ViewKind.SideBySide;
    }

    private void btnResetZoom_Click(object sender, EventArgs e)
    {
      chart.Zoom = 1f;
    }

    private void chart_ChartPaneMouseEvent(object sender, ChartPaneMouseEventArgs args)
    {
      var isPrice = args.Pane.Name.EqualsIgnoreCase("price");
      var unit = isPrice ? "$" : getVolumeKind()== CandleBuySellView.ViewKind.Centered?(  args.ValueAtY>0?"Bought ":"Sold ") : "Units ";

      var sample = args.SampleAtX;

      lblStatus.Text = "{0}:  {1}{2:n2}  at  {3}".Args(args.Pane.Name,
                                                      unit,
                                                      isPrice ? args.ValueAtY : Math.Abs(args.ValueAtY), 
                                                      sample!=null ? sample.TimeStamp.ToString() : "<none>");
    }

    private void chkMidLevels_CheckedChanged(object sender, EventArgs e)
    {
      if (chart.Series==null) return;
      chart.Series.YLevels["FIRST_PRICE"].Visible =  chkMidLevels.Checked;
      chart.Series.YLevels["MID_PRICE"].Visible =  chkMidLevels.Checked;
      chart.NotifySeriesChange();
    }

  }
}
