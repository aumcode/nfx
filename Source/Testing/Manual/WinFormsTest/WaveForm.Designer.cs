namespace WinFormsTest
{
  partial class WaveForm
  {
    /// <summary>
    /// Required designer variable.
    /// </summary>
    private System.ComponentModel.IContainer components = null;

    /// <summary>
    /// Clean up any resources being used.
    /// </summary>
    /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
    protected override void Dispose(bool disposing)
    {
      if (disposing && (components != null))
      {
        components.Dispose();
      }
      base.Dispose(disposing);
    }

    #region Windows Form Designer generated code

    /// <summary>
    /// Required method for Designer support - do not modify
    /// the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent()
    {
      this.components = new System.ComponentModel.Container();
      this.tbLog = new System.Windows.Forms.TextBox();
      this.tmrUpdate = new System.Windows.Forms.Timer(this.components);
      this.pnlManage = new System.Windows.Forms.Panel();
      this.btnCancel = new System.Windows.Forms.Button();
      this.btnUnselectAll = new System.Windows.Forms.Button();
      this.btnSelectAll = new System.Windows.Forms.Button();
      this.nudCallsCnt = new System.Windows.Forms.NumericUpDown();
      this.lblCallsCnt = new System.Windows.Forms.Label();
      this.nudTaskCnt = new System.Windows.Forms.NumericUpDown();
      this.lblTaskCnt = new System.Windows.Forms.Label();
      this.cmbURL = new System.Windows.Forms.ComboBox();
      this.btnRun = new System.Windows.Forms.Button();
      this.gbTests = new System.Windows.Forms.GroupBox();
      this.tblTests = new System.Windows.Forms.TableLayoutPanel();
      this.nudNoAction = new System.Windows.Forms.NumericUpDown();
      this.chkNoAction = new System.Windows.Forms.CheckBox();
      this.chkEmpty = new System.Windows.Forms.CheckBox();
      this.nudEmpty = new System.Windows.Forms.NumericUpDown();
      this.chkActionPost_Found = new System.Windows.Forms.CheckBox();
      this.nudActionPost_Found = new System.Windows.Forms.NumericUpDown();
      this.chkGetSetTimeSpan = new System.Windows.Forms.CheckBox();
      this.nudGetSetTimeSpan = new System.Windows.Forms.NumericUpDown();
      this.chkAdd_BothArgs = new System.Windows.Forms.CheckBox();
      this.nudAdd_BothArgs = new System.Windows.Forms.NumericUpDown();
      this.chkGetList = new System.Windows.Forms.CheckBox();
      this.nudGetList = new System.Windows.Forms.NumericUpDown();
      this.chkGetWithNoPermission = new System.Windows.Forms.CheckBox();
      this.nudGetWithNoPermission = new System.Windows.Forms.NumericUpDown();
      this.chkRowGet_JSONDataMap = new System.Windows.Forms.CheckBox();
      this.nudRowGet_JSONDataMap = new System.Windows.Forms.NumericUpDown();
      this.chkRowGet_TypeRow = new System.Windows.Forms.CheckBox();
      this.nudRowGet_TypeRow = new System.Windows.Forms.NumericUpDown();
      this.chkComplexRow = new System.Windows.Forms.CheckBox();
      this.nudComplexRow = new System.Windows.Forms.NumericUpDown();
      this.chkLogin = new System.Windows.Forms.CheckBox();
      this.nudLogin = new System.Windows.Forms.NumericUpDown();
      this.nudBatchSize = new System.Windows.Forms.NumericUpDown();
      this.label1 = new System.Windows.Forms.Label();
      this.pnlManage.SuspendLayout();
      ((System.ComponentModel.ISupportInitialize)(this.nudCallsCnt)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this.nudTaskCnt)).BeginInit();
      this.gbTests.SuspendLayout();
      this.tblTests.SuspendLayout();
      ((System.ComponentModel.ISupportInitialize)(this.nudNoAction)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this.nudEmpty)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this.nudActionPost_Found)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this.nudGetSetTimeSpan)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this.nudAdd_BothArgs)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this.nudGetList)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this.nudGetWithNoPermission)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this.nudRowGet_JSONDataMap)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this.nudRowGet_TypeRow)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this.nudComplexRow)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this.nudLogin)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this.nudBatchSize)).BeginInit();
      this.SuspendLayout();
      // 
      // tbLog
      // 
      this.tbLog.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.tbLog.Location = new System.Drawing.Point(320, 5);
      this.tbLog.Multiline = true;
      this.tbLog.Name = "tbLog";
      this.tbLog.ReadOnly = true;
      this.tbLog.ScrollBars = System.Windows.Forms.ScrollBars.Both;
      this.tbLog.Size = new System.Drawing.Size(480, 435);
      this.tbLog.TabIndex = 5;
      // 
      // tmrUpdate
      // 
      this.tmrUpdate.Interval = 1000;
      this.tmrUpdate.Tick += new System.EventHandler(this.tmrUpdate_Tick);
      // 
      // pnlManage
      // 
      this.pnlManage.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
      this.pnlManage.Controls.Add(this.nudBatchSize);
      this.pnlManage.Controls.Add(this.label1);
      this.pnlManage.Controls.Add(this.btnCancel);
      this.pnlManage.Controls.Add(this.btnUnselectAll);
      this.pnlManage.Controls.Add(this.btnSelectAll);
      this.pnlManage.Controls.Add(this.nudCallsCnt);
      this.pnlManage.Controls.Add(this.lblCallsCnt);
      this.pnlManage.Controls.Add(this.nudTaskCnt);
      this.pnlManage.Controls.Add(this.lblTaskCnt);
      this.pnlManage.Controls.Add(this.cmbURL);
      this.pnlManage.Controls.Add(this.btnRun);
      this.pnlManage.Controls.Add(this.gbTests);
      this.pnlManage.Location = new System.Drawing.Point(3, 4);
      this.pnlManage.Name = "pnlManage";
      this.pnlManage.Size = new System.Drawing.Size(311, 436);
      this.pnlManage.TabIndex = 12;
      // 
      // btnCancel
      // 
      this.btnCancel.Enabled = false;
      this.btnCancel.Location = new System.Drawing.Point(144, 62);
      this.btnCancel.Name = "btnCancel";
      this.btnCancel.Size = new System.Drawing.Size(129, 23);
      this.btnCancel.TabIndex = 21;
      this.btnCancel.Text = "Cancel";
      this.btnCancel.UseVisualStyleBackColor = true;
      this.btnCancel.Click += new System.EventHandler(this.Cancel_Click);
      // 
      // btnUnselectAll
      // 
      this.btnUnselectAll.Location = new System.Drawing.Point(84, 396);
      this.btnUnselectAll.Name = "btnUnselectAll";
      this.btnUnselectAll.Size = new System.Drawing.Size(75, 23);
      this.btnUnselectAll.TabIndex = 20;
      this.btnUnselectAll.Text = "Unselect All";
      this.btnUnselectAll.UseVisualStyleBackColor = true;
      this.btnUnselectAll.Click += new System.EventHandler(this.btnUnselectAll_Click);
      // 
      // btnSelectAll
      // 
      this.btnSelectAll.Location = new System.Drawing.Point(3, 396);
      this.btnSelectAll.Name = "btnSelectAll";
      this.btnSelectAll.Size = new System.Drawing.Size(75, 23);
      this.btnSelectAll.TabIndex = 19;
      this.btnSelectAll.Text = "Select All";
      this.btnSelectAll.UseVisualStyleBackColor = true;
      this.btnSelectAll.Click += new System.EventHandler(this.btnSelectAll_Click);
      // 
      // nudCallsCnt
      // 
      this.nudCallsCnt.Location = new System.Drawing.Point(113, 35);
      this.nudCallsCnt.Maximum = new decimal(new int[] {
            1000000000,
            0,
            0,
            0});
      this.nudCallsCnt.Name = "nudCallsCnt";
      this.nudCallsCnt.Size = new System.Drawing.Size(82, 20);
      this.nudCallsCnt.TabIndex = 18;
      this.nudCallsCnt.Value = new decimal(new int[] {
            1000000000,
            0,
            0,
            0});
      // 
      // lblCallsCnt
      // 
      this.lblCallsCnt.AutoSize = true;
      this.lblCallsCnt.Location = new System.Drawing.Point(81, 37);
      this.lblCallsCnt.Name = "lblCallsCnt";
      this.lblCallsCnt.Size = new System.Drawing.Size(29, 13);
      this.lblCallsCnt.TabIndex = 17;
      this.lblCallsCnt.Text = "Calls";
      // 
      // nudTaskCnt
      // 
      this.nudTaskCnt.Location = new System.Drawing.Point(40, 35);
      this.nudTaskCnt.Maximum = new decimal(new int[] {
            1000,
            0,
            0,
            0});
      this.nudTaskCnt.Name = "nudTaskCnt";
      this.nudTaskCnt.Size = new System.Drawing.Size(35, 20);
      this.nudTaskCnt.TabIndex = 16;
      this.nudTaskCnt.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
      // 
      // lblTaskCnt
      // 
      this.lblTaskCnt.AutoSize = true;
      this.lblTaskCnt.Location = new System.Drawing.Point(3, 37);
      this.lblTaskCnt.Name = "lblTaskCnt";
      this.lblTaskCnt.Size = new System.Drawing.Size(36, 13);
      this.lblTaskCnt.TabIndex = 15;
      this.lblTaskCnt.Text = "Tasks";
      // 
      // cmbURL
      // 
      this.cmbURL.FormattingEnabled = true;
      this.cmbURL.Items.AddRange(new object[] {
            "http://127.0.0.1:8080/mvc/IntegrationTester/",
            "http://10.0.0.1:8080/mvc/IntegrationTester/"});
      this.cmbURL.Location = new System.Drawing.Point(3, 3);
      this.cmbURL.Name = "cmbURL";
      this.cmbURL.Size = new System.Drawing.Size(301, 21);
      this.cmbURL.TabIndex = 14;
      this.cmbURL.Text = "http://127.0.0.1:8080/mvc/IntegrationTester/";
      this.cmbURL.Click += new System.EventHandler(this.cmbURL_TextChanged);
      // 
      // btnRun
      // 
      this.btnRun.Location = new System.Drawing.Point(9, 62);
      this.btnRun.Name = "btnRun";
      this.btnRun.Size = new System.Drawing.Size(129, 23);
      this.btnRun.TabIndex = 13;
      this.btnRun.Text = "Run";
      this.btnRun.UseVisualStyleBackColor = true;
      this.btnRun.Click += new System.EventHandler(this.btnRun_Click);
      // 
      // gbTests
      // 
      this.gbTests.Controls.Add(this.tblTests);
      this.gbTests.Location = new System.Drawing.Point(3, 91);
      this.gbTests.Name = "gbTests";
      this.gbTests.Size = new System.Drawing.Size(263, 302);
      this.gbTests.TabIndex = 12;
      this.gbTests.TabStop = false;
      this.gbTests.Text = "Tests";
      // 
      // tblTests
      // 
      this.tblTests.ColumnCount = 2;
      this.tblTests.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 72.6087F));
      this.tblTests.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 27.3913F));
      this.tblTests.Controls.Add(this.nudNoAction, 1, 1);
      this.tblTests.Controls.Add(this.chkNoAction, 0, 1);
      this.tblTests.Controls.Add(this.chkEmpty, 0, 0);
      this.tblTests.Controls.Add(this.nudEmpty, 1, 0);
      this.tblTests.Controls.Add(this.chkActionPost_Found, 0, 2);
      this.tblTests.Controls.Add(this.nudActionPost_Found, 1, 2);
      this.tblTests.Controls.Add(this.chkGetSetTimeSpan, 0, 3);
      this.tblTests.Controls.Add(this.nudGetSetTimeSpan, 1, 3);
      this.tblTests.Controls.Add(this.chkAdd_BothArgs, 0, 4);
      this.tblTests.Controls.Add(this.nudAdd_BothArgs, 1, 4);
      this.tblTests.Controls.Add(this.chkGetList, 0, 5);
      this.tblTests.Controls.Add(this.nudGetList, 1, 5);
      this.tblTests.Controls.Add(this.chkGetWithNoPermission, 0, 6);
      this.tblTests.Controls.Add(this.nudGetWithNoPermission, 1, 6);
      this.tblTests.Controls.Add(this.chkRowGet_JSONDataMap, 0, 7);
      this.tblTests.Controls.Add(this.nudRowGet_JSONDataMap, 1, 7);
      this.tblTests.Controls.Add(this.chkRowGet_TypeRow, 0, 8);
      this.tblTests.Controls.Add(this.nudRowGet_TypeRow, 1, 8);
      this.tblTests.Controls.Add(this.chkComplexRow, 0, 9);
      this.tblTests.Controls.Add(this.nudComplexRow, 1, 9);
      this.tblTests.Controls.Add(this.chkLogin, 0, 10);
      this.tblTests.Controls.Add(this.nudLogin, 1, 10);
      this.tblTests.Dock = System.Windows.Forms.DockStyle.Fill;
      this.tblTests.Location = new System.Drawing.Point(3, 16);
      this.tblTests.Name = "tblTests";
      this.tblTests.RowCount = 12;
      this.tblTests.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 25F));
      this.tblTests.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 25F));
      this.tblTests.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 25F));
      this.tblTests.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 25F));
      this.tblTests.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 25F));
      this.tblTests.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 25F));
      this.tblTests.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 25F));
      this.tblTests.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 25F));
      this.tblTests.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 25F));
      this.tblTests.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 25F));
      this.tblTests.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 25F));
      this.tblTests.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 25F));
      this.tblTests.Size = new System.Drawing.Size(257, 283);
      this.tblTests.TabIndex = 2;
      // 
      // nudNoAction
      // 
      this.nudNoAction.Location = new System.Drawing.Point(189, 28);
      this.nudNoAction.Maximum = new decimal(new int[] {
            1000,
            0,
            0,
            0});
      this.nudNoAction.Name = "nudNoAction";
      this.nudNoAction.Size = new System.Drawing.Size(57, 20);
      this.nudNoAction.TabIndex = 4;
      this.nudNoAction.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
      // 
      // chkNoAction
      // 
      this.chkNoAction.AutoSize = true;
      this.chkNoAction.Location = new System.Drawing.Point(3, 28);
      this.chkNoAction.Name = "chkNoAction";
      this.chkNoAction.Size = new System.Drawing.Size(73, 17);
      this.chkNoAction.TabIndex = 3;
      this.chkNoAction.Text = "No Action";
      this.chkNoAction.UseVisualStyleBackColor = true;
      // 
      // chkEmpty
      // 
      this.chkEmpty.AutoSize = true;
      this.chkEmpty.Checked = true;
      this.chkEmpty.CheckState = System.Windows.Forms.CheckState.Checked;
      this.chkEmpty.Location = new System.Drawing.Point(3, 3);
      this.chkEmpty.Name = "chkEmpty";
      this.chkEmpty.Size = new System.Drawing.Size(55, 17);
      this.chkEmpty.TabIndex = 1;
      this.chkEmpty.Text = "Empty";
      this.chkEmpty.UseVisualStyleBackColor = true;
      // 
      // nudEmpty
      // 
      this.nudEmpty.Location = new System.Drawing.Point(189, 3);
      this.nudEmpty.Maximum = new decimal(new int[] {
            1000,
            0,
            0,
            0});
      this.nudEmpty.Name = "nudEmpty";
      this.nudEmpty.Size = new System.Drawing.Size(57, 20);
      this.nudEmpty.TabIndex = 2;
      this.nudEmpty.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
      // 
      // chkActionPost_Found
      // 
      this.chkActionPost_Found.AutoSize = true;
      this.chkActionPost_Found.Location = new System.Drawing.Point(3, 53);
      this.chkActionPost_Found.Name = "chkActionPost_Found";
      this.chkActionPost_Found.Size = new System.Drawing.Size(113, 17);
      this.chkActionPost_Found.TabIndex = 5;
      this.chkActionPost_Found.Text = "ActionPost_Found";
      this.chkActionPost_Found.UseVisualStyleBackColor = true;
      // 
      // nudActionPost_Found
      // 
      this.nudActionPost_Found.Location = new System.Drawing.Point(189, 53);
      this.nudActionPost_Found.Maximum = new decimal(new int[] {
            1000,
            0,
            0,
            0});
      this.nudActionPost_Found.Name = "nudActionPost_Found";
      this.nudActionPost_Found.Size = new System.Drawing.Size(57, 20);
      this.nudActionPost_Found.TabIndex = 6;
      this.nudActionPost_Found.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
      // 
      // chkGetSetTimeSpan
      // 
      this.chkGetSetTimeSpan.AutoSize = true;
      this.chkGetSetTimeSpan.Location = new System.Drawing.Point(3, 78);
      this.chkGetSetTimeSpan.Name = "chkGetSetTimeSpan";
      this.chkGetSetTimeSpan.Size = new System.Drawing.Size(107, 17);
      this.chkGetSetTimeSpan.TabIndex = 7;
      this.chkGetSetTimeSpan.Text = "GetSetTimeSpan";
      this.chkGetSetTimeSpan.UseVisualStyleBackColor = true;
      // 
      // nudGetSetTimeSpan
      // 
      this.nudGetSetTimeSpan.Location = new System.Drawing.Point(189, 78);
      this.nudGetSetTimeSpan.Maximum = new decimal(new int[] {
            1000,
            0,
            0,
            0});
      this.nudGetSetTimeSpan.Name = "nudGetSetTimeSpan";
      this.nudGetSetTimeSpan.Size = new System.Drawing.Size(57, 20);
      this.nudGetSetTimeSpan.TabIndex = 8;
      this.nudGetSetTimeSpan.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
      // 
      // chkAdd_BothArgs
      // 
      this.chkAdd_BothArgs.AutoSize = true;
      this.chkAdd_BothArgs.Location = new System.Drawing.Point(3, 103);
      this.chkAdd_BothArgs.Name = "chkAdd_BothArgs";
      this.chkAdd_BothArgs.Size = new System.Drawing.Size(94, 17);
      this.chkAdd_BothArgs.TabIndex = 9;
      this.chkAdd_BothArgs.Text = "Add_BothArgs";
      this.chkAdd_BothArgs.UseVisualStyleBackColor = true;
      // 
      // nudAdd_BothArgs
      // 
      this.nudAdd_BothArgs.Location = new System.Drawing.Point(189, 103);
      this.nudAdd_BothArgs.Maximum = new decimal(new int[] {
            1000,
            0,
            0,
            0});
      this.nudAdd_BothArgs.Name = "nudAdd_BothArgs";
      this.nudAdd_BothArgs.Size = new System.Drawing.Size(57, 20);
      this.nudAdd_BothArgs.TabIndex = 10;
      this.nudAdd_BothArgs.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
      // 
      // chkGetList
      // 
      this.chkGetList.AutoSize = true;
      this.chkGetList.Location = new System.Drawing.Point(3, 128);
      this.chkGetList.Name = "chkGetList";
      this.chkGetList.Size = new System.Drawing.Size(59, 17);
      this.chkGetList.TabIndex = 11;
      this.chkGetList.Text = "GetList";
      this.chkGetList.UseVisualStyleBackColor = true;
      // 
      // nudGetList
      // 
      this.nudGetList.Location = new System.Drawing.Point(189, 128);
      this.nudGetList.Maximum = new decimal(new int[] {
            1000,
            0,
            0,
            0});
      this.nudGetList.Name = "nudGetList";
      this.nudGetList.Size = new System.Drawing.Size(57, 20);
      this.nudGetList.TabIndex = 12;
      this.nudGetList.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
      // 
      // chkGetWithNoPermission
      // 
      this.chkGetWithNoPermission.AutoSize = true;
      this.chkGetWithNoPermission.Location = new System.Drawing.Point(3, 153);
      this.chkGetWithNoPermission.Name = "chkGetWithNoPermission";
      this.chkGetWithNoPermission.Size = new System.Drawing.Size(129, 17);
      this.chkGetWithNoPermission.TabIndex = 13;
      this.chkGetWithNoPermission.Text = "GetWithNoPermission";
      this.chkGetWithNoPermission.UseVisualStyleBackColor = true;
      // 
      // nudGetWithNoPermission
      // 
      this.nudGetWithNoPermission.Location = new System.Drawing.Point(189, 153);
      this.nudGetWithNoPermission.Maximum = new decimal(new int[] {
            1000,
            0,
            0,
            0});
      this.nudGetWithNoPermission.Name = "nudGetWithNoPermission";
      this.nudGetWithNoPermission.Size = new System.Drawing.Size(57, 20);
      this.nudGetWithNoPermission.TabIndex = 14;
      this.nudGetWithNoPermission.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
      // 
      // chkRowGet_JSONDataMap
      // 
      this.chkRowGet_JSONDataMap.AutoSize = true;
      this.chkRowGet_JSONDataMap.Location = new System.Drawing.Point(3, 178);
      this.chkRowGet_JSONDataMap.Name = "chkRowGet_JSONDataMap";
      this.chkRowGet_JSONDataMap.Size = new System.Drawing.Size(143, 17);
      this.chkRowGet_JSONDataMap.TabIndex = 15;
      this.chkRowGet_JSONDataMap.Text = "RowGet_JSONDataMap";
      this.chkRowGet_JSONDataMap.UseVisualStyleBackColor = true;
      // 
      // nudRowGet_JSONDataMap
      // 
      this.nudRowGet_JSONDataMap.Location = new System.Drawing.Point(189, 178);
      this.nudRowGet_JSONDataMap.Maximum = new decimal(new int[] {
            1000,
            0,
            0,
            0});
      this.nudRowGet_JSONDataMap.Name = "nudRowGet_JSONDataMap";
      this.nudRowGet_JSONDataMap.Size = new System.Drawing.Size(57, 20);
      this.nudRowGet_JSONDataMap.TabIndex = 16;
      this.nudRowGet_JSONDataMap.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
      // 
      // chkRowGet_TypeRow
      // 
      this.chkRowGet_TypeRow.AutoSize = true;
      this.chkRowGet_TypeRow.Location = new System.Drawing.Point(3, 203);
      this.chkRowGet_TypeRow.Name = "chkRowGet_TypeRow";
      this.chkRowGet_TypeRow.Size = new System.Drawing.Size(117, 17);
      this.chkRowGet_TypeRow.TabIndex = 17;
      this.chkRowGet_TypeRow.Text = "RowGet_TypeRow";
      this.chkRowGet_TypeRow.UseVisualStyleBackColor = true;
      // 
      // nudRowGet_TypeRow
      // 
      this.nudRowGet_TypeRow.Location = new System.Drawing.Point(189, 203);
      this.nudRowGet_TypeRow.Maximum = new decimal(new int[] {
            1000,
            0,
            0,
            0});
      this.nudRowGet_TypeRow.Name = "nudRowGet_TypeRow";
      this.nudRowGet_TypeRow.Size = new System.Drawing.Size(57, 20);
      this.nudRowGet_TypeRow.TabIndex = 18;
      this.nudRowGet_TypeRow.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
      // 
      // chkComplexRow
      // 
      this.chkComplexRow.AutoSize = true;
      this.chkComplexRow.Location = new System.Drawing.Point(3, 228);
      this.chkComplexRow.Name = "chkComplexRow";
      this.chkComplexRow.Size = new System.Drawing.Size(88, 17);
      this.chkComplexRow.TabIndex = 19;
      this.chkComplexRow.Text = "ComplexRow";
      this.chkComplexRow.UseVisualStyleBackColor = true;
      // 
      // nudComplexRow
      // 
      this.nudComplexRow.Location = new System.Drawing.Point(189, 228);
      this.nudComplexRow.Maximum = new decimal(new int[] {
            1000,
            0,
            0,
            0});
      this.nudComplexRow.Name = "nudComplexRow";
      this.nudComplexRow.Size = new System.Drawing.Size(57, 20);
      this.nudComplexRow.TabIndex = 20;
      this.nudComplexRow.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
      // 
      // chkLogin
      // 
      this.chkLogin.AutoSize = true;
      this.chkLogin.Location = new System.Drawing.Point(3, 253);
      this.chkLogin.Name = "chkLogin";
      this.chkLogin.Size = new System.Drawing.Size(52, 17);
      this.chkLogin.TabIndex = 21;
      this.chkLogin.Text = "Login";
      this.chkLogin.UseVisualStyleBackColor = true;
      // 
      // nudLogin
      // 
      this.nudLogin.Location = new System.Drawing.Point(189, 253);
      this.nudLogin.Maximum = new decimal(new int[] {
            1000,
            0,
            0,
            0});
      this.nudLogin.Name = "nudLogin";
      this.nudLogin.Size = new System.Drawing.Size(57, 20);
      this.nudLogin.TabIndex = 22;
      this.nudLogin.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
      // 
      // nudBatchSize
      // 
      this.nudBatchSize.Location = new System.Drawing.Point(238, 35);
      this.nudBatchSize.Maximum = new decimal(new int[] {
            1000000000,
            0,
            0,
            0});
      this.nudBatchSize.Name = "nudBatchSize";
      this.nudBatchSize.Size = new System.Drawing.Size(66, 20);
      this.nudBatchSize.TabIndex = 23;
      this.nudBatchSize.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
      // 
      // label1
      // 
      this.label1.AutoSize = true;
      this.label1.Location = new System.Drawing.Point(201, 37);
      this.label1.Name = "label1";
      this.label1.Size = new System.Drawing.Size(35, 13);
      this.label1.TabIndex = 22;
      this.label1.Text = "Batch";
      // 
      // WaveForm
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.ClientSize = new System.Drawing.Size(803, 443);
      this.Controls.Add(this.pnlManage);
      this.Controls.Add(this.tbLog);
      this.Name = "WaveForm";
      this.Text = "WaveForm";
      this.pnlManage.ResumeLayout(false);
      this.pnlManage.PerformLayout();
      ((System.ComponentModel.ISupportInitialize)(this.nudCallsCnt)).EndInit();
      ((System.ComponentModel.ISupportInitialize)(this.nudTaskCnt)).EndInit();
      this.gbTests.ResumeLayout(false);
      this.tblTests.ResumeLayout(false);
      this.tblTests.PerformLayout();
      ((System.ComponentModel.ISupportInitialize)(this.nudNoAction)).EndInit();
      ((System.ComponentModel.ISupportInitialize)(this.nudEmpty)).EndInit();
      ((System.ComponentModel.ISupportInitialize)(this.nudActionPost_Found)).EndInit();
      ((System.ComponentModel.ISupportInitialize)(this.nudGetSetTimeSpan)).EndInit();
      ((System.ComponentModel.ISupportInitialize)(this.nudAdd_BothArgs)).EndInit();
      ((System.ComponentModel.ISupportInitialize)(this.nudGetList)).EndInit();
      ((System.ComponentModel.ISupportInitialize)(this.nudGetWithNoPermission)).EndInit();
      ((System.ComponentModel.ISupportInitialize)(this.nudRowGet_JSONDataMap)).EndInit();
      ((System.ComponentModel.ISupportInitialize)(this.nudRowGet_TypeRow)).EndInit();
      ((System.ComponentModel.ISupportInitialize)(this.nudComplexRow)).EndInit();
      ((System.ComponentModel.ISupportInitialize)(this.nudLogin)).EndInit();
      ((System.ComponentModel.ISupportInitialize)(this.nudBatchSize)).EndInit();
      this.ResumeLayout(false);
      this.PerformLayout();

    }

    #endregion

    private System.Windows.Forms.TextBox tbLog;
    private System.Windows.Forms.Timer tmrUpdate;
    private System.Windows.Forms.Panel pnlManage;
    private System.Windows.Forms.Button btnUnselectAll;
    private System.Windows.Forms.Button btnSelectAll;
    private System.Windows.Forms.NumericUpDown nudCallsCnt;
    private System.Windows.Forms.Label lblCallsCnt;
    private System.Windows.Forms.NumericUpDown nudTaskCnt;
    private System.Windows.Forms.Label lblTaskCnt;
    private System.Windows.Forms.ComboBox cmbURL;
    private System.Windows.Forms.Button btnRun;
    private System.Windows.Forms.GroupBox gbTests;
    private System.Windows.Forms.TableLayoutPanel tblTests;
    private System.Windows.Forms.NumericUpDown nudNoAction;
    private System.Windows.Forms.CheckBox chkNoAction;
    private System.Windows.Forms.CheckBox chkEmpty;
    private System.Windows.Forms.NumericUpDown nudEmpty;
    private System.Windows.Forms.CheckBox chkActionPost_Found;
    private System.Windows.Forms.NumericUpDown nudActionPost_Found;
    private System.Windows.Forms.CheckBox chkGetSetTimeSpan;
    private System.Windows.Forms.NumericUpDown nudGetSetTimeSpan;
    private System.Windows.Forms.CheckBox chkAdd_BothArgs;
    private System.Windows.Forms.NumericUpDown nudAdd_BothArgs;
    private System.Windows.Forms.CheckBox chkGetList;
    private System.Windows.Forms.NumericUpDown nudGetList;
    private System.Windows.Forms.CheckBox chkGetWithNoPermission;
    private System.Windows.Forms.NumericUpDown nudGetWithNoPermission;
    private System.Windows.Forms.CheckBox chkRowGet_JSONDataMap;
    private System.Windows.Forms.NumericUpDown nudRowGet_JSONDataMap;
    private System.Windows.Forms.CheckBox chkRowGet_TypeRow;
    private System.Windows.Forms.NumericUpDown nudRowGet_TypeRow;
    private System.Windows.Forms.CheckBox chkComplexRow;
    private System.Windows.Forms.NumericUpDown nudComplexRow;
    private System.Windows.Forms.CheckBox chkLogin;
    private System.Windows.Forms.NumericUpDown nudLogin;
    private System.Windows.Forms.Button btnCancel;
    private System.Windows.Forms.NumericUpDown nudBatchSize;
    private System.Windows.Forms.Label label1;
  }
}