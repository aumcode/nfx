/*<FILE_LICENSE>
* NFX (.NET Framework Extension) Unistack Library
* Copyright 2003-2016 IT Adapter Inc.
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
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;


using NFX;
using NFX.Financial.Market;
using NFX.Financial.Market.SecDb;

using NFX.IO.FileSystem;
using NFX.IO.FileSystem.Local;
using NFX.Serialization.JSON;

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
      m_Data = new CandleTimeSeries("My data", 0);
    //  m_Data.MaxSamples = 10;

      var sd = DateTime.Now.AddSeconds(-500);

      var samples = CandleSample.GenerateRandom(300,
                                                sd, 
                                                1000,
                                                10,
                                                
                                                20, 8,
                                                
                                                120.0f);
      
      samples.ForEach( s => m_Data.Add(s) );
       
      m_Data.Views.Register( new CandleView("Candles", 0));
      m_Data.Views.Register( 
            new CandleMidLineView("MidLineHiLo", 1)
            {
              MidLineType = MidLineType.HighLow,
              LineStyle = new LineStyle{ Color = Color.FromArgb(200, 255, 0,0), Width = 2}
            });

      m_Data.Views.Register( 
            new CandleMidLineView("MidLineOpCl1", 2)
            {
              MidLineType = MidLineType.OpenClose, 
              LineStyle = new LineStyle{ Color = Color.FromArgb(200, 50, 0, 200), Width = 1.5f, DashStyle = System.Drawing.Drawing2D.DashStyle.Dot}
            });




      m_Data.Views.Register( new CandleView("Candles2", 0, "MLPane"){ BlackWhite = true });
      m_Data.Views.Register(
            new CandleMidLineView("MidLineOpCl2", 1, "MLPane")
            {
              MidLineType = MidLineType.OpenClose,
              LineStyle = new LineStyle{ Color = Color.FromArgb(200, 255, 180, 0), Width = 3f}
            });

 //     m_Data.Views.Register( new CandleView("Avg1", 0, "Another Pane0"));
 //     m_Data.Views.Register( new CandleView("Avg2", 0, "Another Pane1"));
   
   
   
      //m_Data.Views.Register( new CandleView("Avg3", 0, "Another Pane2"));
      //m_Data.Views.Register( new CandleView("Avg4", 0, "Another Pane3"));
      //m_Data.Views.Register( new CandleView("Avg5", 0, "Another Pane4"));
      //m_Data.Views.Register( new CandleView("Avg6", 0, "Another Pane5"));


      chart.Series = m_Data;
    }

    private DateTime DT = DateTime.Now; 

    private void timer_Tick(object sender, EventArgs e)
    {
      if (m_Data==null) return;


      if (chkSlide.Checked)
      {
        var sample = CandleSample.GenerateRandom(1, DT, 1000, 10, 20, 15, 120.0f);
        
        m_Data.Add( sample[0] );
        
        DT = DT.AddSeconds( tbInterval.Text.AsInt(1) );

        chart.NotifySeriesChange();
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

    private void chkLeftRuller_CheckStateChanged(object sender, EventArgs e)
    {
      chart.VRulerPosition = chkLeftRuller.Checked ? VRulerPosition.Left : VRulerPosition.Right;
    }

    private void chart_MouseDown(object sender, MouseEventArgs e)
    {
      var sample = chart.MapXToSample( e.X );
      if (sample!=null)
      {
       Text = sample.TimeStamp.ToString();
       var cs = sample as CandleSample;
       if (cs!=null)
       {
         //cs.OpenPrice += 8f;
         cs.SellVolume +=10;
         chart.NotifySeriesChange();
       }
      }
      else
       Text = "<null>";
    }

    private void chkAutoScroll_CheckStateChanged(object sender, EventArgs e)
    {
      chart.AutoScroll = chkAutoScroll.Checked;
    }

    private void btnResetZoom_Click(object sender, EventArgs e)
    {
      chart.Zoom = 1.0f;
    }

    private void btnLoadSecDB_Click(object sender, EventArgs e)
    {
      using(var fs = new LocalFileSystem())
      {
        var sdb = new SecDBFileReader(fs, new FileSystemSessionConnectParams(), tbSecDBFile.Text);

        Text = "Exchange: {0}, Origin time: {1}, {2}".Args(sdb.SystemHeader.Exchange,
                                                        sdb.SystemHeader.Date+sdb.SystemHeader.OriginLocalTimeOffset, 
                                                        sdb.SystemHeader.OriginLocalTimeName); 

        MessageBox.Show( sdb.Headers.ToJSON() );

        m_Data = new CandleTimeSeries("From file", 0);
        m_Data.YLevels.Register( new TimeSeries.YLevel("LAST_PRICE", 0){Value = 0f});//,  HLineStyle = new LineStyle{Color=Color.Red}});

        m_Data.Views.Register( new CandleView("Candles", 0){ ShowYLevels = true, ShowBalloons = true } );
        m_Data.Views.Register( new CandleBuySellView("BuySell", 0){} );

        m_Data.Views.Register( 
            new CandleMidLineView("MidLineHiLo", 1)
            {
              MidLineType = MidLineType.HighLow,
              LineStyle = new LineStyle{ Color = Color.FromArgb(200, 255, 0,0), Width = 2}
            });

        m_Data.Views.Register( 
            new CandleMidLineView("MidLineOpCl1", 2)
            {
              MidLineType = MidLineType.OpenClose, 
              LineStyle = new LineStyle{ Color = Color.FromArgb(200, 50, 0, 200), Width = 1.5f, DashStyle = System.Drawing.Drawing2D.DashStyle.Dot}
            });

        m_Data.MaxSamples = 100000;

        var data = sdb.GetCandleDataAsCandleSamples( sdb.CandlesMetadata.Resolutions.Min() );

        data.ForEach( s =>  m_Data.Add( s ));

        
        m_Data.YLevels["LAST_PRICE"].Value = m_Data.DataReveresed.First().ClosePrice;
        m_Data.YLevels.Register( new TimeSeries.YLevel("Lo", 0){Value = m_Data.Data.Min(cs=>cs.LowPrice), AffectsScale = false,  HLineStyle = new LineStyle{Color=Color.Red, Width =2}});
        m_Data.YLevels.Register( new TimeSeries.YLevel("Hi", 0){Value = m_Data.Data.Max(cs=>cs.HighPrice), AffectsScale = false, HLineStyle = new LineStyle{Color=Color.Blue, Width =2}});
        
        m_Data.Views.Register( new CandleView("Candles2", 0, "MLPane"){ BlackWhite = true, ShowBalloons = true});
        m_Data.Views.Register(
            new CandleMidLineView("MidLineOpCl2", 1, "MLPane")
            {
              MidLineType = MidLineType.OpenClose,
              ShowYLevels = true,
              LineStyle   = new LineStyle{ Color = Color.FromArgb(200, 255, 180, 0), Width = 3f}
            });
        
        chart.Series = m_Data;
        //chart.NotifySeriesChange();
      }
    }

    private void btnResample_Click(object sender, EventArgs e)
    {
      var resampled = m_Data.Data.AggregateHomogeneousSamples(2).ToList();
      m_Data.ReplaceSamples(resampled);

      chart.NotifySeriesChange();
    }

    private void btnQuotes_Click(object sender, EventArgs e)
    {
      using(var fs = new LocalFileSystem())
      {
        var sdb = new SecDBFileReader(fs, new FileSystemSessionConnectParams(), tbSecDBFile.Text);

        Text = "Exchange: {0}, Origin time: {1}, {2}".Args(sdb.SystemHeader.Exchange,
                                                        sdb.SystemHeader.Date+sdb.SystemHeader.OriginLocalTimeOffset, 
                                                        sdb.SystemHeader.OriginLocalTimeName); 

        
        var sw = System.Diagnostics.Stopwatch.StartNew();

        var cnt = sdb.GetAllStreamData().Count( s => s is SecDBFileReader.SecondSample);
        var el = sw.ElapsedMilliseconds;

        MessageBox.Show("{0} samples in {1} msec at {2}/sec".Args(cnt, el, cnt / (el /1000d)));

        try
        {
         System.IO.File.WriteAllText(@"c:\Users\Anton\Desktop\SEC_DBDUMP.json", sdb.GetAllStreamData().Take(tbHowMany.Text.AsInt()).ToJSON( JSONWritingOptions.PrettyPrint) );
        }
        catch(Exception err)
        {
          MessageBox.Show("{0}\n------------\n{1}".Args(err.ToMessageWithType(), err.StackTrace));
        }
      }
    }

    private void btnSynthCandles_Click(object sender, EventArgs e)
    {
       using(var fs = new LocalFileSystem())
      {
        var sdb = new SecDBFileReader(fs, new FileSystemSessionConnectParams(), tbSecDBFile.Text);

        //var tradedata = sdb.GetAllStreamData()
        //                   .Where( s => s is SecDBFileReader.TradeSample)
        //                   .Cast<SecDBFileReader.TradeSample>()
        //                   .Select( t => new CandleSample(t.TimeStamp)
        //                                 {
        //                                   OpenPrice = t.Price,
        //                                   ClosePrice = t.Price,
        //                                   HighPrice = t.Price,
        //                                   LowPrice = t.Price
        //                                 } 
        //                    );

        var data = sdb.GetAllStreamData().Where(s => s is SecDBFileReader.TradeSample).SynthesizeCandles(tbSecPeriod.Text.AsUInt(), 
             (cs, qs, i) => {},
             (cs, ts, i) => // "Beautifier function"
              {                      
                if (i==0) cs.OpenPrice = ts.Price;
                cs.HighPrice = Math.Max(cs.HighPrice, ts.Price);
                cs.LowPrice  = cs.LowPrice!=0f ? Math.Min( cs.LowPrice , ts.Price) : ts.Price;
                cs.ClosePrice = ts.Price;

                if (ts.IsQty) 
                {
                  if (ts.Side==SecDBFileReader.TradeSample.SideType.Buy)
                    cs.BuyVolume += ts.Qty;
                  else
                    cs.SellVolume += ts.Qty;
                }
              }
        );


        m_Data = new CandleTimeSeries("Candles", 0);
        
        m_Data.Views.Register( 
            new CandleView("Candle View", 1)
            {
              ShowYLevels = true,
              ShowBalloons = true
            });

        //m_Data.Views.Register( 
        //    new CandleMidLineView("MidLineHiLo", 1)
        //    {
        //      MidLineType = MidLineType.HighLow,
        //      LineStyle = new LineStyle{ Color = Color.FromArgb(200, 255, 0,0), Width = 2}
        //    });

        m_Data.Views.Register( new CandleBuySellView("BuySell", 2){} );

        data.ForEach( s =>  m_Data.Add( s ));

        m_Data.YLevels.Register( new TimeSeries.YLevel("Lo", 0){Value = m_Data.Data.Min(cs=>cs.ClosePrice), AffectsScale = true,  HLineStyle = new LineStyle{Color=Color.Red, Width =2}});
        m_Data.YLevels.Register( new TimeSeries.YLevel("Hi", 0){Value = m_Data.Data.Max(cs=>cs.ClosePrice), AffectsScale = true, HLineStyle = new LineStyle{Color=Color.Blue, Width =2}});
        
        chart.Series = m_Data;
      }
    }

   


  }
}
