using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using NFX;
using System.Threading;
using System.Collections.Specialized;
using NFX.Serialization.JSON;
using System.Text.RegularExpressions;
using NFX.DataAccess.CRUD;

namespace WinFormsTest
{
  public partial class WaveForm : Form
  {

    #region Consts

      private const string EXPECTED_400 = "400 expected but wasn't thrown";
      private const string EXPECTED_404 = "404 expected but wasn't thrown";

      private const string USER_ID = "dxw";
      private const string USER_PWD = "thejake";
      private const string USER_NAME = "Denis";
      private const string USER_STATUS = "User";

      private const string WAVE_COOKIE_NAME = "ZEKRET";
      private const string WAVE_COOKIE_VALUE = "Hello";
      public static Cookie S_WAVE_COOKIE = new Cookie(WAVE_COOKIE_NAME, WAVE_COOKIE_VALUE);

    #endregion

          #region Nested

            public enum TestStatus { Ok, Err};

            public class SpanRow: TypedRow
            {
              [Field] public TimeSpan Span { get; set; }
            }

            public class TestRow: TypedRow
            {
              [Field] public int ID { get; set; }
              [Field] public string Name { get; set; }

              [Field] public DateTime Date { get; set; }
              [Field] public DateTime? DateNullable { get; set; }

              [Field] public TimeSpan Span { get; set; }
              [Field] public TimeSpan? SpanNullable { get; set; }

              [Field] public TestStatus Status { get; set; }
              [Field] public TestStatus? StatusNullable { get; set; }

              [Field] public bool Is { get; set; }
              [Field] public bool? IsNullable { get; set; }

              [Field] public decimal Money { get; set; }
              [Field] public decimal? MoneyNullable { get; set; }

              [Field] public float Float { get; set; }
              [Field] public float? FloatNullable { get; set; }

              [Field] public double Double { get; set; }
              [Field] public double? DoubleNullable { get; set; }
            }

            public class TestComplexRow: TypedRow
            {
              [Field] public int ID { get; set; }
              [Field] public TestRow Row1 { get; set; }
              [Field] public TestRow Row2 { get; set; }
              [Field] public TestRow[] ErrorRows { get; set; }
            }

            public class WebClientCookied: WebClient
            {
              public WebClientCookied(bool keepAlive = true)
              {
                m_KeepAlive = keepAlive;
              }

              private bool m_KeepAlive = true;
              private CookieContainer m_CookieContainer = new CookieContainer();

              public CookieContainer CookieContainer { get { return m_CookieContainer; }}

              protected override WebRequest GetWebRequest(Uri address)
              {
                var request = base.GetWebRequest(address);

                var httpRequest = request as HttpWebRequest;
                if (httpRequest != null)
                {
                  httpRequest.KeepAlive = m_KeepAlive;
                  httpRequest.CookieContainer = CookieContainer;
                  httpRequest.AllowAutoRedirect = true;
                }

                return request;
              }
            }

            private struct TestItem
            {
              public CheckBox Chk;
              public NumericUpDown Nud;
              public Action<WebClient> Action;  

              public bool ShouldRun { get { return Chk.Checked && Nud.Value > 0; } }

              public static implicit operator TestBatchItem(TestItem testItem)
              {
                if (!testItem.ShouldRun) throw new InvalidOperationException();
                return new TestBatchItem() { Action = testItem.Action, Cnt = (int)testItem.Nud.Value};
              }
            }

            private struct TestBatchItem
            {
              public Action<WebClient> Action;
              public int Cnt;

              public static implicit operator TestBatchItem(TestItem testItem)
              {
                if (!testItem.ShouldRun) throw new InvalidOperationException();
                return new TestBatchItem() { Action = testItem.Action, Cnt = (int)testItem.Nud.Value};
              }
            }

            private class ThreadArg
            {
              public int CallCnt;
              public TestBatchItem[] BatchItems;
              public int RawBatches;
              public bool KeepAlive;
            }

          #endregion

    private List<TestItem> m_testItems;

    private long m_CallCnt, m_PriorProcessedCnt, m_ErrCnt;
    private DateTime m_StartMeasure, m_PriorMeasure;
    private bool m_IsRunning;
    private Dictionary<int, bool> m_ThreadsState = new Dictionary<int,bool>();

    public WaveForm()
    {
      InitializeComponent();

      m_testItems = new List<TestItem>() 
      {
        new TestItem { Chk = chkEmpty, Nud = nudEmpty, Action = Action_Empty},
        new TestItem { Chk = chkNoAction, Nud = nudNoAction, Action = Action_Action0Name},
        new TestItem { Chk = chkActionPost_Found, Nud = nudActionPost_Found, Action = Action_ActionPost_Found},
        new TestItem { Chk = chkGetSetTimeSpan, Nud = nudGetSetTimeSpan, Action = Action_GetSetTimeSpan},
        new TestItem { Chk = chkAdd_BothArgs, Nud = nudAdd_BothArgs, Action = Action_Add_BothArgs},
        new TestItem { Chk = chkGetList, Nud = nudGetList, Action = Action_GetList},
        new TestItem { Chk = chkGetWithNoPermission, Nud = nudGetWithNoPermission, Action = Action_GetWithNoPermission},
        new TestItem { Chk = chkRowGet_JSONDataMap, Nud = nudRowGet_JSONDataMap, Action = Action_RowGet_JSONDataMap},
        new TestItem { Chk = chkRowGet_TypeRow, Nud = nudRowGet_TypeRow, Action = Action_RowGet_TypeRow},
        new TestItem { Chk = chkComplexRow, Nud = nudComplexRow, Action = Action_ComplexRow},
        new TestItem { Chk = chkLogin, Nud = nudLogin, Action = Action_Login}
      };
    }

    protected override void OnLoad(EventArgs e) { syncServerURI(); }

    private void btnRun_Click(object sender, EventArgs e)
    {
      syncServerURI();
      m_ThreadsState.Clear();
      m_IsRunning = true;

      setControlEnabled(false);

      tbLog.Clear();

      m_ErrCnt = m_CallCnt = m_PriorProcessedCnt = 0;
      m_PriorMeasure = m_StartMeasure = DateTime.Now;

      var batchItems = m_testItems.Where(i => i.ShouldRun).Select(i => new TestBatchItem { Action = i.Action, Cnt = (int)i.Nud.Value}).ToArray();

      var taskCnt = (int)nudTaskCnt.Value;
      var callsCount = (int)nudCallsCnt.Value;
      var rawBatchCnt = (int)nudBatchSize.Value;

      var callsPerTask = callsCount / taskCnt;

      var tasks = new List<Task>(taskCnt);

      for (int iTask = 0; iTask < taskCnt; iTask++)
      {
        var thread = new Thread(threadFunc);
        m_ThreadsState[thread.ManagedThreadId] = true;
        thread.Start( new ThreadArg { CallCnt = callsPerTask, BatchItems = batchItems, RawBatches = rawBatchCnt, KeepAlive = chkKeepAlive.Checked});
      }
    }

    private void threadFunc(object arg)
    {
      var threadArg = arg as ThreadArg;
      var batchItems = threadArg.BatchItems;
      var rawBatches = threadArg.RawBatches;
      var keepAlive = threadArg.KeepAlive;
      int callCnt, errCnt;

      WebClient wc = null;

      for (int iBatch = 0; iBatch < threadArg.CallCnt && m_IsRunning; iBatch++)
      {
        if (iBatch % rawBatches == 0)
        {
          if (wc != null) wc.Dispose();
          wc = this.CreateWebClient(keepAlive);
        }

        errCnt = callCnt = 0;
        foreach (var item in batchItems)
        {
          if (!m_IsRunning) break;
          for (int iItem = 0; iItem < item.Cnt && m_IsRunning; iItem++)
          {
            try
            {
              callCnt++;
              item.Action(wc);
            }
            catch (Exception ex)
            {
              errCnt++;
              LogMsg("Method " + item.Action.Method.Name + " " + ex.Message);
            }
          }
          
        } 
        Interlocked.Add(ref m_CallCnt, callCnt);
        Interlocked.Add(ref m_ErrCnt, errCnt);
      }

      if (wc != null) 
      {
        wc.Dispose();
        wc = null;
      }

      lock (m_ThreadsState)
      {
        m_ThreadsState[System.Threading.Thread.CurrentThread.ManagedThreadId] = false;
        if (m_ThreadsState.Any(ts => ts.Value)) return;

        setControlEnabled(true);
        
        LogMsg("OK={0:n0} ERR={1:n0}".Args(m_CallCnt - m_ErrCnt, m_ErrCnt), true);
        updateCaption();
        m_IsRunning = false;
      }
    }

    private void setControlEnabled(bool enabled)
    {
      if (this.InvokeRequired)
      {
        var cb = new Action<bool>(setControlEnabled);
        this.Invoke(cb, enabled);
      }
      else
      {
        pnlManage.Controls.OfType<Control>().ForEach(c => c.Enabled = enabled);
        btnCancel.Enabled = !enabled;
        tmrUpdate.Enabled = !enabled;
      }
    }

    private void Cancel_Click(object sender, EventArgs e) { m_IsRunning = false; }

    private void LogMsg(string msg, bool start = false)
    {
      if (tbLog.InvokeRequired)
      {
        var cb = new Action<string, bool>(LogMsg);
        this.Invoke(cb, msg, start);
      }
      else
      {
        if (!start)
        {
          tbLog.Text += msg + Environment.NewLine;
          tbLog.SelectionStart = tbLog.Text.Length;
        }
        else
        {
          tbLog.Text = msg + Environment.NewLine + Environment.NewLine + tbLog.Text;
          tbLog.SelectionStart = 0;
        }
        tbLog.ScrollToCaret();
      }
    }

    private void tmrUpdate_Tick(object sender, EventArgs e)
    {
      updateCaption();
    }

    private void updateCaption()
    {
      if (this.InvokeRequired)
      {
        var cb = new Action(updateCaption);
        this.Invoke(cb);
      }
      else
      {
        if (m_CallCnt == m_PriorProcessedCnt) return;

        var intervalProcessedCnt = m_CallCnt - m_PriorProcessedCnt;
        var now = DateTime.Now;

        var intervalSeconds = (now - m_PriorMeasure).TotalSeconds;
        var intervalRate = (double)intervalProcessedCnt / intervalSeconds;

        Console.WriteLine("intervalSeconds={0} intervalProcessedCnt={1} intervalRate={2}", intervalSeconds, intervalProcessedCnt, intervalRate);

        var totalSeconds = (now - m_StartMeasure).TotalSeconds;
        var totalRate = (double)m_CallCnt / totalSeconds;

        Console.WriteLine("totalSeconds={0} m_ProcessedCnt={1} totalRate={2}", totalSeconds, m_CallCnt, totalRate);

        m_PriorProcessedCnt = m_CallCnt;
        m_PriorMeasure = now;

        this.Text = "Processed {0:n0} in {1:hh\\:mm\\:ss} (avg: {2:n0}/sec, total: {3:n0}/sec)"
                      .Args(m_CallCnt, (now - m_StartMeasure), intervalRate, totalRate);
      }
    }

    private void btnSelectAll_Click(object sender, EventArgs e) { m_testItems.ForEach(i => i.Chk.Checked = true); }

    private void btnUnselectAll_Click(object sender, EventArgs e) { m_testItems.ForEach(i => i.Chk.Checked = false); }

    private void syncServerURI() { m_ServerURI = new Uri(cmbURL.Text); }

    private Uri m_ServerURI;

    #region Tests

      private void Action_Empty(WebClient wc)
      {
        //using (var wc = CreateWebClient())
        {
          var res = wc.DownloadString(m_ServerURI + "Empty");

          if (res.IsNotNullOrEmpty()) throw new Exception();
        }
      }

      private void Action_Action0Name(WebClient wc)
      {
        //using (var wc = CreateWebClient())
        {
          try
          {
            wc.DownloadString(m_ServerURI + "ActionName0");
            throw new Exception(EXPECTED_404);
          }
          catch (WebException ex)
          {
            if (!Is404(ex)) throw new Exception();
          }
        }
      }

      public void Action_ActionPost_Found(WebClient wc)
      {
        //using (var wc = CreateWebClient())
        {
          var res = wc.UploadString(m_ServerURI + "ActionPost", string.Empty);
          if (res != "ActionPost") throw new Exception();
        }
      }

      public void Action_GetSetTimeSpan(WebClient wc)
      {
        var initTs = TimeSpan.FromDays(10);

        //using (var wc = CreateWebClient())
        {
          var values = new NameValueCollection();
          values.Add("ts", initTs.AsString());

          byte[] bytes = wc.UploadValues(m_ServerURI + "GetSetTimeSpan", values);
          string str = GetUTF8StringWOBOM(bytes);

          var gotTs = TimeSpan.Parse(str);

          if (initTs.Add(TimeSpan.FromDays(1)) != gotTs) throw new Exception();
        }
      }

      public void Action_Add_BothArgs(WebClient wc)
      {
        int a = 3, b = 14;

        //using (var wc = CreateWebClient())
        {
          var values = new NameValueCollection();
          values.Add("a", a.AsString());
          values.Add("b", b.AsString());

          byte[] bytes = wc.UploadValues(m_ServerURI + "Add", values);
          string str = Encoding.ASCII.GetString(bytes);
          int sum = str.AsInt();

          if (a+b != sum) throw new Exception();
        }
      }

      public void Action_GetList(WebClient wc)
      {
        //using (var wc = CreateWebClient())
        {
          string str = wc.DownloadString(m_ServerURI + "GetList");

          if ("application/json" != wc.ResponseHeaders[HttpResponseHeader.ContentType]) throw new Exception();

          var obj = JSONReader.DeserializeDynamic(str);

          if (3 != obj.Data.Count) throw new Exception();
          if (1UL != obj.Data[0]) throw new Exception();
          if (2UL != obj.Data[1]) throw new Exception();
          if (3UL != obj.Data[2]) throw new Exception();
        }
      }

      public void Action_GetWithNoPermission(WebClient wc)
      {
        //using (var wc = CreateWebClient())
        {
          string str = wc.DownloadString(m_ServerURI + "GetWithPermission");

          if ("text/html" != wc.ResponseHeaders[HttpResponseHeader.ContentType]) throw new Exception();
          if (!Regex.IsMatch(str, "Authorization to .+/TestPath/TestPermission.+ failed")) throw new Exception();
        }
      }

      public void Action_RowGet_JSONDataMap(WebClient wc)
      {
        var start = DateTime.Now;

        System.Threading.Thread.Sleep(3000);

        //using (var wc = CreateWebClient())
        {
          string str = wc.DownloadString(m_ServerURI + "RowGet");

          if ("application/json" != wc.ResponseHeaders[HttpResponseHeader.ContentType]) throw new Exception();

          var obj = JSONReader.DeserializeDynamic(str);

          if (16 != obj.Data.Count) throw new Exception();
          if (777UL != obj.Data["ID"]) throw new Exception();
          if ("Test Name" != obj.Data["Name"]) throw new Exception();

          var date = DateTime.Parse(obj.Data["Date"]);
          if ((DateTime.Now - start).TotalSeconds < 2.0d) throw new Exception();
          if ("Ok" != obj.Data["Status"]) throw new Exception();

          var gotTs = TimeSpan.FromTicks((long)(ulong)(obj.Data["Span"]));
          if (TimeSpan.FromSeconds(1) != gotTs) throw new Exception();
        }
      }

      public void Action_RowGet_TypeRow(WebClient wc)
      {
        var start = DateTime.Now;

        //using (var wc = CreateWebClient())
        {
          string str = wc.DownloadString(m_ServerURI + "RowGet");

          if ("application/json" != wc.ResponseHeaders[HttpResponseHeader.ContentType]) throw new Exception();
            
          var map = JSONReader.DeserializeDataObject(str) as JSONDataMap;
          var gotRow = JSONReader.ToRow<TestRow>(map);
        }
      }

      public void Action_ComplexRow(WebClient wc)
      {
        var initalRow = new TestComplexRow();

        initalRow.ID = 777;

        initalRow.Row1 = new TestRow(){ID = 101, Name = "Test Row 1", Date = DateTime.Now};
        initalRow.Row2 = new TestRow(){ID = 102, Name = "Test Row 2", Date = DateTime.Now};

        initalRow.ErrorRows = new TestRow[] { 
          new TestRow() {ID = 201, Name = "Err Row 1", Date = DateTime.Now},
          new TestRow() {ID = 202, Name = "Err Row 2", Date = DateTime.Now},
          new TestRow() {ID = 203, Name = "Err Row 3", Date = DateTime.Now}
        };

        var str = initalRow.ToJSON(JSONWritingOptions.CompactRowsAsMap);

        //using (var wc = CreateWebClient())
        {
          wc.Headers[HttpRequestHeader.ContentType] = NFX.Web.ContentType.JSON;
          var res = wc.UploadString(m_ServerURI + "ComplexRowSet", str);

          var map = JSONReader.DeserializeDataObject(res) as JSONDataMap;
          var gotRow = JSONReader.ToRow<TestComplexRow>(map);

          if (initalRow.ID + 1 != gotRow.ID) throw new Exception();
          if (initalRow.Row1.ID + 2 != gotRow.Row1.ID) throw new Exception();
          if (gotRow.ErrorRows[2].Date - initalRow.ErrorRows[2].Date.AddDays(-2) >= TimeSpan.FromMilliseconds(1)) throw new Exception();
        }
      }

      public void Action_Login(WebClient wc)
      {
        var values = new NameValueCollection();
        values.Add("id", USER_ID);
        values.Add("pwd", USER_PWD);

        //using (var wc = CreateWebClient())
        {
          var bytes = wc.UploadValues(m_ServerURI + "LoginUser", values);
          var str = GetUTF8StringWOBOM(bytes);

          Console.WriteLine(str);

          if ("application/json" != wc.ResponseHeaders[HttpResponseHeader.ContentType]) throw new Exception();

          var obj = JSONReader.DeserializeDynamic(str);
          if (USER_STATUS != obj["Status"]) throw new Exception();
          if (USER_NAME != obj["Name"]) throw new Exception();

          str = wc.DownloadString(m_ServerURI + "GetWithPermission");

          if (Regex.IsMatch(str, "Authorization to .+/TestPath/TestPermission.+ failed")) throw new Exception();
        }
      }

      private WebClientCookied CreateWebClient(bool keepAlive)
      {
        var wc = new WebClientCookied(keepAlive);
        wc.CookieContainer.Add(m_ServerURI, S_WAVE_COOKIE);
        return wc;
      }

      private bool Is400(WebException ex)
      {
        return ex.Message.IndexOf("(400)") >= 0;
      }

      private bool Is404(WebException ex)
      {
        return ex.Message.IndexOf("(404)") >= 0;
      }

      private static string GetUTF8StringWOBOM(byte[] buf)
      {
        if (buf.Length > 3 && (buf[0] == 0xEF && buf[1] == 0xBB && buf[2] == 0xBF))
            return Encoding.UTF8.GetString(buf, 3, buf.Length-3);

        return Encoding.UTF8.GetString(buf);
      }

    #endregion

  }
}
