namespace WinFormsTest
{
  partial class PileForm
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
      this.btnStart = new System.Windows.Forms.Button();
      this.stbObjectCount = new System.Windows.Forms.TextBox();
      this.label1 = new System.Windows.Forms.Label();
      this.label2 = new System.Windows.Forms.Label();
      this.stbMemBytes = new System.Windows.Forms.TextBox();
      this.label3 = new System.Windows.Forms.Label();
      this.stbOverheadBytes = new System.Windows.Forms.TextBox();
      this.label4 = new System.Windows.Forms.Label();
      this.stbUtilizedBytes = new System.Windows.Forms.TextBox();
      this.label5 = new System.Windows.Forms.Label();
      this.stbSegTotalCount = new System.Windows.Forms.TextBox();
      this.label6 = new System.Windows.Forms.Label();
      this.stbSegments = new System.Windows.Forms.TextBox();
      this.btnStop = new System.Windows.Forms.Button();
      this.tmrStatus = new System.Windows.Forms.Timer(this.components);
      this.grpPerson = new System.Windows.Forms.GroupBox();
      this.btnPersonParaGet = new System.Windows.Forms.Button();
      this.btnStruct = new System.Windows.Forms.Button();
      this.btnPersonSizeOf = new System.Windows.Forms.Button();
      this.tbPersonVariance = new System.Windows.Forms.TextBox();
      this.label16 = new System.Windows.Forms.Label();
      this.label8 = new System.Windows.Forms.Label();
      this.tbPersonThreads = new System.Windows.Forms.TextBox();
      this.btnPersonParaPut = new System.Windows.Forms.Button();
      this.btnPersonGet = new System.Windows.Forms.Button();
      this.lbPerson = new System.Windows.Forms.ListBox();
      this.btnPersonDelete = new System.Windows.Forms.Button();
      this.btnPersonPut = new System.Windows.Forms.Button();
      this.label7 = new System.Windows.Forms.Label();
      this.tbPersonCount = new System.Windows.Forms.TextBox();
      this.btnPurge = new System.Windows.Forms.Button();
      this.btnGC = new System.Windows.Forms.Button();
      this.btnCrawl = new System.Windows.Forms.Button();
      this.chkSpeed = new System.Windows.Forms.CheckBox();
      this.label9 = new System.Windows.Forms.Label();
      this.tbSegmentSize = new System.Windows.Forms.TextBox();
      this.label10 = new System.Windows.Forms.Label();
      this.tbMaxMemoryMb = new System.Windows.Forms.TextBox();
      this.groupBox1 = new System.Windows.Forms.GroupBox();
      this.tbTraxVariance = new System.Windows.Forms.TextBox();
      this.lbVar = new System.Windows.Forms.Label();
      this.tbTraxDeletes = new System.Windows.Forms.TextBox();
      this.tbTraxWrites = new System.Windows.Forms.TextBox();
      this.tbTraxThreads = new System.Windows.Forms.TextBox();
      this.sbTraxDeletes = new System.Windows.Forms.VScrollBar();
      this.sbTraxWrites = new System.Windows.Forms.VScrollBar();
      this.chkTraxer = new System.Windows.Forms.CheckBox();
      this.label13 = new System.Windows.Forms.Label();
      this.label12 = new System.Windows.Forms.Label();
      this.label11 = new System.Windows.Forms.Label();
      this.lbErrors = new System.Windows.Forms.ListBox();
      this.label14 = new System.Windows.Forms.Label();
      this.stbUtilizedBytesObject = new System.Windows.Forms.TextBox();
      this.label15 = new System.Windows.Forms.Label();
      this.stbOverheadBytesObject = new System.Windows.Forms.TextBox();
      this.lbSpeed = new System.Windows.Forms.ListBox();
      this.label17 = new System.Windows.Forms.Label();
      this.stbMemCapacityBytes = new System.Windows.Forms.TextBox();
      this.btnCompact = new System.Windows.Forms.Button();
      this.chkRaw = new System.Windows.Forms.CheckBox();
      this.grpPerson.SuspendLayout();
      this.groupBox1.SuspendLayout();
      this.SuspendLayout();
      // 
      // btnStart
      // 
      this.btnStart.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.btnStart.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.btnStart.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(192)))), ((int)(((byte)(0)))));
      this.btnStart.Location = new System.Drawing.Point(802, 8);
      this.btnStart.Name = "btnStart";
      this.btnStart.Size = new System.Drawing.Size(75, 48);
      this.btnStart.TabIndex = 0;
      this.btnStart.Text = "START";
      this.btnStart.UseVisualStyleBackColor = true;
      this.btnStart.Click += new System.EventHandler(this.btnStart_Click);
      // 
      // stbObjectCount
      // 
      this.stbObjectCount.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(244)))), ((int)(((byte)(244)))), ((int)(((byte)(244)))));
      this.stbObjectCount.Font = new System.Drawing.Font("Verdana", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.stbObjectCount.ForeColor = System.Drawing.Color.Blue;
      this.stbObjectCount.Location = new System.Drawing.Point(151, 8);
      this.stbObjectCount.Name = "stbObjectCount";
      this.stbObjectCount.ReadOnly = true;
      this.stbObjectCount.Size = new System.Drawing.Size(150, 23);
      this.stbObjectCount.TabIndex = 2;
      this.stbObjectCount.Text = "000";
      this.stbObjectCount.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
      // 
      // label1
      // 
      this.label1.AutoSize = true;
      this.label1.Font = new System.Drawing.Font("Verdana", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.label1.Location = new System.Drawing.Point(12, 13);
      this.label1.Name = "label1";
      this.label1.Size = new System.Drawing.Size(82, 13);
      this.label1.TabIndex = 3;
      this.label1.Text = "Object Count";
      // 
      // label2
      // 
      this.label2.AutoSize = true;
      this.label2.Font = new System.Drawing.Font("Verdana", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.label2.Location = new System.Drawing.Point(12, 42);
      this.label2.Name = "label2";
      this.label2.Size = new System.Drawing.Size(70, 13);
      this.label2.TabIndex = 5;
      this.label2.Text = "Mem Bytes";
      // 
      // stbMemBytes
      // 
      this.stbMemBytes.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(244)))), ((int)(((byte)(244)))), ((int)(((byte)(244)))));
      this.stbMemBytes.Font = new System.Drawing.Font("Verdana", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.stbMemBytes.ForeColor = System.Drawing.Color.Blue;
      this.stbMemBytes.Location = new System.Drawing.Point(151, 37);
      this.stbMemBytes.Name = "stbMemBytes";
      this.stbMemBytes.ReadOnly = true;
      this.stbMemBytes.Size = new System.Drawing.Size(150, 23);
      this.stbMemBytes.TabIndex = 4;
      this.stbMemBytes.Text = "000";
      this.stbMemBytes.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
      // 
      // label3
      // 
      this.label3.AutoSize = true;
      this.label3.Font = new System.Drawing.Font("Verdana", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.label3.Location = new System.Drawing.Point(12, 129);
      this.label3.Name = "label3";
      this.label3.Size = new System.Drawing.Size(99, 13);
      this.label3.TabIndex = 9;
      this.label3.Text = "Overhead Bytes";
      // 
      // stbOverheadBytes
      // 
      this.stbOverheadBytes.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(244)))), ((int)(((byte)(244)))), ((int)(((byte)(244)))));
      this.stbOverheadBytes.Font = new System.Drawing.Font("Verdana", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.stbOverheadBytes.ForeColor = System.Drawing.Color.Blue;
      this.stbOverheadBytes.Location = new System.Drawing.Point(151, 124);
      this.stbOverheadBytes.Name = "stbOverheadBytes";
      this.stbOverheadBytes.ReadOnly = true;
      this.stbOverheadBytes.Size = new System.Drawing.Size(150, 23);
      this.stbOverheadBytes.TabIndex = 8;
      this.stbOverheadBytes.Text = "000";
      this.stbOverheadBytes.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
      // 
      // label4
      // 
      this.label4.AutoSize = true;
      this.label4.Font = new System.Drawing.Font("Verdana", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.label4.Location = new System.Drawing.Point(12, 71);
      this.label4.Name = "label4";
      this.label4.Size = new System.Drawing.Size(84, 13);
      this.label4.TabIndex = 7;
      this.label4.Text = "Utilized Bytes";
      // 
      // stbUtilizedBytes
      // 
      this.stbUtilizedBytes.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(244)))), ((int)(((byte)(244)))), ((int)(((byte)(244)))));
      this.stbUtilizedBytes.Font = new System.Drawing.Font("Verdana", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.stbUtilizedBytes.ForeColor = System.Drawing.Color.Blue;
      this.stbUtilizedBytes.Location = new System.Drawing.Point(151, 66);
      this.stbUtilizedBytes.Name = "stbUtilizedBytes";
      this.stbUtilizedBytes.ReadOnly = true;
      this.stbUtilizedBytes.Size = new System.Drawing.Size(150, 23);
      this.stbUtilizedBytes.TabIndex = 6;
      this.stbUtilizedBytes.Text = "000";
      this.stbUtilizedBytes.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
      // 
      // label5
      // 
      this.label5.AutoSize = true;
      this.label5.Font = new System.Drawing.Font("Verdana", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.label5.Location = new System.Drawing.Point(12, 216);
      this.label5.Name = "label5";
      this.label5.Size = new System.Drawing.Size(99, 13);
      this.label5.TabIndex = 13;
      this.label5.Text = "Seg Total Count";
      // 
      // stbSegTotalCount
      // 
      this.stbSegTotalCount.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(244)))), ((int)(((byte)(244)))), ((int)(((byte)(244)))));
      this.stbSegTotalCount.Font = new System.Drawing.Font("Verdana", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.stbSegTotalCount.ForeColor = System.Drawing.Color.Blue;
      this.stbSegTotalCount.Location = new System.Drawing.Point(151, 211);
      this.stbSegTotalCount.Name = "stbSegTotalCount";
      this.stbSegTotalCount.ReadOnly = true;
      this.stbSegTotalCount.Size = new System.Drawing.Size(150, 23);
      this.stbSegTotalCount.TabIndex = 12;
      this.stbSegTotalCount.Text = "000";
      this.stbSegTotalCount.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
      // 
      // label6
      // 
      this.label6.AutoSize = true;
      this.label6.Font = new System.Drawing.Font("Verdana", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.label6.Location = new System.Drawing.Point(12, 187);
      this.label6.Name = "label6";
      this.label6.Size = new System.Drawing.Size(64, 13);
      this.label6.TabIndex = 11;
      this.label6.Text = "Segments";
      // 
      // stbSegments
      // 
      this.stbSegments.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(244)))), ((int)(((byte)(244)))), ((int)(((byte)(244)))));
      this.stbSegments.Font = new System.Drawing.Font("Verdana", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.stbSegments.ForeColor = System.Drawing.Color.Blue;
      this.stbSegments.Location = new System.Drawing.Point(151, 182);
      this.stbSegments.Name = "stbSegments";
      this.stbSegments.ReadOnly = true;
      this.stbSegments.Size = new System.Drawing.Size(150, 23);
      this.stbSegments.TabIndex = 10;
      this.stbSegments.Text = "000";
      this.stbSegments.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
      // 
      // btnStop
      // 
      this.btnStop.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.btnStop.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.btnStop.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
      this.btnStop.Location = new System.Drawing.Point(883, 8);
      this.btnStop.Name = "btnStop";
      this.btnStop.Size = new System.Drawing.Size(75, 48);
      this.btnStop.TabIndex = 14;
      this.btnStop.Text = "STOP";
      this.btnStop.UseVisualStyleBackColor = true;
      this.btnStop.Click += new System.EventHandler(this.btnStop_Click);
      // 
      // tmrStatus
      // 
      this.tmrStatus.Enabled = true;
      this.tmrStatus.Interval = 500;
      this.tmrStatus.Tick += new System.EventHandler(this.tmrStatus_Tick);
      // 
      // grpPerson
      // 
      this.grpPerson.Controls.Add(this.chkRaw);
      this.grpPerson.Controls.Add(this.btnPersonParaGet);
      this.grpPerson.Controls.Add(this.btnStruct);
      this.grpPerson.Controls.Add(this.btnPersonSizeOf);
      this.grpPerson.Controls.Add(this.tbPersonVariance);
      this.grpPerson.Controls.Add(this.label16);
      this.grpPerson.Controls.Add(this.label8);
      this.grpPerson.Controls.Add(this.tbPersonThreads);
      this.grpPerson.Controls.Add(this.btnPersonParaPut);
      this.grpPerson.Controls.Add(this.btnPersonGet);
      this.grpPerson.Controls.Add(this.lbPerson);
      this.grpPerson.Controls.Add(this.btnPersonDelete);
      this.grpPerson.Controls.Add(this.btnPersonPut);
      this.grpPerson.Controls.Add(this.label7);
      this.grpPerson.Controls.Add(this.tbPersonCount);
      this.grpPerson.Location = new System.Drawing.Point(315, 13);
      this.grpPerson.Name = "grpPerson";
      this.grpPerson.Size = new System.Drawing.Size(236, 422);
      this.grpPerson.TabIndex = 15;
      this.grpPerson.TabStop = false;
      this.grpPerson.Text = "Person";
      // 
      // btnPersonParaGet
      // 
      this.btnPersonParaGet.Location = new System.Drawing.Point(155, 38);
      this.btnPersonParaGet.Name = "btnPersonParaGet";
      this.btnPersonParaGet.Size = new System.Drawing.Size(59, 23);
      this.btnPersonParaGet.TabIndex = 37;
      this.btnPersonParaGet.Text = "Para Get";
      this.btnPersonParaGet.UseVisualStyleBackColor = true;
      this.btnPersonParaGet.Click += new System.EventHandler(this.btnPersonParaGet_Click);
      // 
      // btnStruct
      // 
      this.btnStruct.Location = new System.Drawing.Point(157, 169);
      this.btnStruct.Name = "btnStruct";
      this.btnStruct.Size = new System.Drawing.Size(59, 23);
      this.btnStruct.TabIndex = 36;
      this.btnStruct.Text = "Struct";
      this.btnStruct.UseVisualStyleBackColor = true;
      this.btnStruct.Click += new System.EventHandler(this.btnStruct_Click);
      // 
      // btnPersonSizeOf
      // 
      this.btnPersonSizeOf.Location = new System.Drawing.Point(155, 138);
      this.btnPersonSizeOf.Name = "btnPersonSizeOf";
      this.btnPersonSizeOf.Size = new System.Drawing.Size(59, 24);
      this.btnPersonSizeOf.TabIndex = 35;
      this.btnPersonSizeOf.Text = "SizeOf";
      this.btnPersonSizeOf.UseVisualStyleBackColor = true;
      this.btnPersonSizeOf.Click += new System.EventHandler(this.btnPersonSizeOf_Click);
      // 
      // tbPersonVariance
      // 
      this.tbPersonVariance.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(244)))), ((int)(((byte)(244)))), ((int)(((byte)(244)))));
      this.tbPersonVariance.Font = new System.Drawing.Font("Verdana", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.tbPersonVariance.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(0)))), ((int)(((byte)(192)))));
      this.tbPersonVariance.Location = new System.Drawing.Point(143, 387);
      this.tbPersonVariance.Name = "tbPersonVariance";
      this.tbPersonVariance.Size = new System.Drawing.Size(64, 21);
      this.tbPersonVariance.TabIndex = 34;
      this.tbPersonVariance.Text = "0";
      this.tbPersonVariance.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
      // 
      // label16
      // 
      this.label16.AutoSize = true;
      this.label16.Location = new System.Drawing.Point(140, 371);
      this.label16.Name = "label16";
      this.label16.Size = new System.Drawing.Size(90, 13);
      this.label16.TabIndex = 33;
      this.label16.Text = "Payload Variance";
      // 
      // label8
      // 
      this.label8.AutoSize = true;
      this.label8.Location = new System.Drawing.Point(104, 19);
      this.label8.Name = "label8";
      this.label8.Size = new System.Drawing.Size(46, 13);
      this.label8.TabIndex = 8;
      this.label8.Text = "Threads";
      // 
      // tbPersonThreads
      // 
      this.tbPersonThreads.Location = new System.Drawing.Point(107, 35);
      this.tbPersonThreads.Name = "tbPersonThreads";
      this.tbPersonThreads.Size = new System.Drawing.Size(40, 20);
      this.tbPersonThreads.TabIndex = 7;
      this.tbPersonThreads.Text = "4";
      // 
      // btnPersonParaPut
      // 
      this.btnPersonParaPut.Location = new System.Drawing.Point(156, 14);
      this.btnPersonParaPut.Name = "btnPersonParaPut";
      this.btnPersonParaPut.Size = new System.Drawing.Size(59, 23);
      this.btnPersonParaPut.TabIndex = 6;
      this.btnPersonParaPut.Text = "Para Put";
      this.btnPersonParaPut.UseVisualStyleBackColor = true;
      this.btnPersonParaPut.Click += new System.EventHandler(this.btnPersonParaPut_Click);
      // 
      // btnPersonGet
      // 
      this.btnPersonGet.Location = new System.Drawing.Point(155, 64);
      this.btnPersonGet.Name = "btnPersonGet";
      this.btnPersonGet.Size = new System.Drawing.Size(61, 23);
      this.btnPersonGet.TabIndex = 5;
      this.btnPersonGet.Text = "Get";
      this.btnPersonGet.UseVisualStyleBackColor = true;
      this.btnPersonGet.Click += new System.EventHandler(this.btnPersonGet_Click);
      // 
      // lbPerson
      // 
      this.lbPerson.FormattingEnabled = true;
      this.lbPerson.Location = new System.Drawing.Point(10, 105);
      this.lbPerson.Name = "lbPerson";
      this.lbPerson.Size = new System.Drawing.Size(115, 303);
      this.lbPerson.TabIndex = 4;
      // 
      // btnPersonDelete
      // 
      this.btnPersonDelete.Location = new System.Drawing.Point(86, 64);
      this.btnPersonDelete.Name = "btnPersonDelete";
      this.btnPersonDelete.Size = new System.Drawing.Size(61, 23);
      this.btnPersonDelete.TabIndex = 3;
      this.btnPersonDelete.Text = "Del";
      this.btnPersonDelete.UseVisualStyleBackColor = true;
      this.btnPersonDelete.Click += new System.EventHandler(this.btnPersonDelete_Click);
      // 
      // btnPersonPut
      // 
      this.btnPersonPut.Location = new System.Drawing.Point(16, 64);
      this.btnPersonPut.Name = "btnPersonPut";
      this.btnPersonPut.Size = new System.Drawing.Size(59, 23);
      this.btnPersonPut.TabIndex = 2;
      this.btnPersonPut.Text = "Put";
      this.btnPersonPut.UseVisualStyleBackColor = true;
      this.btnPersonPut.Click += new System.EventHandler(this.btnPersonPut_Click);
      // 
      // label7
      // 
      this.label7.AutoSize = true;
      this.label7.Location = new System.Drawing.Point(9, 22);
      this.label7.Name = "label7";
      this.label7.Size = new System.Drawing.Size(58, 13);
      this.label7.TabIndex = 1;
      this.label7.Text = "How Many";
      // 
      // tbPersonCount
      // 
      this.tbPersonCount.Location = new System.Drawing.Point(6, 38);
      this.tbPersonCount.Name = "tbPersonCount";
      this.tbPersonCount.Size = new System.Drawing.Size(70, 20);
      this.tbPersonCount.TabIndex = 0;
      this.tbPersonCount.Text = "100";
      // 
      // btnPurge
      // 
      this.btnPurge.BackColor = System.Drawing.Color.Red;
      this.btnPurge.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.btnPurge.ForeColor = System.Drawing.Color.Yellow;
      this.btnPurge.Location = new System.Drawing.Point(15, 296);
      this.btnPurge.Name = "btnPurge";
      this.btnPurge.Size = new System.Drawing.Size(61, 42);
      this.btnPurge.TabIndex = 16;
      this.btnPurge.Text = "Purge";
      this.btnPurge.UseVisualStyleBackColor = false;
      this.btnPurge.Click += new System.EventHandler(this.btnPurge_Click);
      // 
      // btnGC
      // 
      this.btnGC.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.btnGC.ForeColor = System.Drawing.Color.Fuchsia;
      this.btnGC.Location = new System.Drawing.Point(226, 296);
      this.btnGC.Name = "btnGC";
      this.btnGC.Size = new System.Drawing.Size(75, 42);
      this.btnGC.TabIndex = 17;
      this.btnGC.Text = "GC";
      this.btnGC.UseVisualStyleBackColor = true;
      this.btnGC.Click += new System.EventHandler(this.btnGC_Click);
      // 
      // btnCrawl
      // 
      this.btnCrawl.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.btnCrawl.ForeColor = System.Drawing.Color.Green;
      this.btnCrawl.Location = new System.Drawing.Point(145, 296);
      this.btnCrawl.Name = "btnCrawl";
      this.btnCrawl.Size = new System.Drawing.Size(75, 42);
      this.btnCrawl.TabIndex = 18;
      this.btnCrawl.Text = "Crawl";
      this.btnCrawl.UseVisualStyleBackColor = true;
      this.btnCrawl.Click += new System.EventHandler(this.btnCrawl_Click);
      // 
      // chkSpeed
      // 
      this.chkSpeed.AutoSize = true;
      this.chkSpeed.Location = new System.Drawing.Point(226, 344);
      this.chkSpeed.Name = "chkSpeed";
      this.chkSpeed.Size = new System.Drawing.Size(85, 17);
      this.chkSpeed.TabIndex = 19;
      this.chkSpeed.Text = "Favor speed";
      this.chkSpeed.UseVisualStyleBackColor = true;
      this.chkSpeed.CheckedChanged += new System.EventHandler(this.chkSpeed_CheckedChanged);
      // 
      // label9
      // 
      this.label9.AutoSize = true;
      this.label9.Location = new System.Drawing.Point(799, 71);
      this.label9.Name = "label9";
      this.label9.Size = new System.Drawing.Size(67, 13);
      this.label9.TabIndex = 10;
      this.label9.Text = "Seg Size Mb";
      // 
      // tbSegmentSize
      // 
      this.tbSegmentSize.Location = new System.Drawing.Point(802, 87);
      this.tbSegmentSize.Name = "tbSegmentSize";
      this.tbSegmentSize.Size = new System.Drawing.Size(133, 20);
      this.tbSegmentSize.TabIndex = 9;
      this.tbSegmentSize.Text = "256";
      // 
      // label10
      // 
      this.label10.AutoSize = true;
      this.label10.Location = new System.Drawing.Point(799, 135);
      this.label10.Name = "label10";
      this.label10.Size = new System.Drawing.Size(85, 13);
      this.label10.TabIndex = 21;
      this.label10.Text = "Max Memory Mb";
      // 
      // tbMaxMemoryMb
      // 
      this.tbMaxMemoryMb.Location = new System.Drawing.Point(802, 151);
      this.tbMaxMemoryMb.Name = "tbMaxMemoryMb";
      this.tbMaxMemoryMb.Size = new System.Drawing.Size(133, 20);
      this.tbMaxMemoryMb.TabIndex = 20;
      this.tbMaxMemoryMb.Text = "0";
      // 
      // groupBox1
      // 
      this.groupBox1.Controls.Add(this.tbTraxVariance);
      this.groupBox1.Controls.Add(this.lbVar);
      this.groupBox1.Controls.Add(this.tbTraxDeletes);
      this.groupBox1.Controls.Add(this.tbTraxWrites);
      this.groupBox1.Controls.Add(this.tbTraxThreads);
      this.groupBox1.Controls.Add(this.sbTraxDeletes);
      this.groupBox1.Controls.Add(this.sbTraxWrites);
      this.groupBox1.Controls.Add(this.chkTraxer);
      this.groupBox1.Controls.Add(this.label13);
      this.groupBox1.Controls.Add(this.label12);
      this.groupBox1.Controls.Add(this.label11);
      this.groupBox1.Location = new System.Drawing.Point(557, 13);
      this.groupBox1.Name = "groupBox1";
      this.groupBox1.Size = new System.Drawing.Size(236, 421);
      this.groupBox1.TabIndex = 22;
      this.groupBox1.TabStop = false;
      this.groupBox1.Text = "Traxer";
      // 
      // tbTraxVariance
      // 
      this.tbTraxVariance.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(244)))), ((int)(((byte)(244)))), ((int)(((byte)(244)))));
      this.tbTraxVariance.Font = new System.Drawing.Font("Verdana", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.tbTraxVariance.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(0)))), ((int)(((byte)(192)))));
      this.tbTraxVariance.Location = new System.Drawing.Point(6, 166);
      this.tbTraxVariance.Name = "tbTraxVariance";
      this.tbTraxVariance.Size = new System.Drawing.Size(64, 21);
      this.tbTraxVariance.TabIndex = 32;
      this.tbTraxVariance.Text = "0";
      this.tbTraxVariance.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
      // 
      // lbVar
      // 
      this.lbVar.AutoSize = true;
      this.lbVar.Location = new System.Drawing.Point(3, 150);
      this.lbVar.Name = "lbVar";
      this.lbVar.Size = new System.Drawing.Size(90, 13);
      this.lbVar.TabIndex = 31;
      this.lbVar.Text = "Payload Variance";
      // 
      // tbTraxDeletes
      // 
      this.tbTraxDeletes.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(244)))), ((int)(((byte)(244)))), ((int)(((byte)(244)))));
      this.tbTraxDeletes.Font = new System.Drawing.Font("Verdana", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.tbTraxDeletes.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(0)))), ((int)(((byte)(192)))));
      this.tbTraxDeletes.Location = new System.Drawing.Point(150, 385);
      this.tbTraxDeletes.Name = "tbTraxDeletes";
      this.tbTraxDeletes.Size = new System.Drawing.Size(64, 21);
      this.tbTraxDeletes.TabIndex = 29;
      this.tbTraxDeletes.Text = "000";
      this.tbTraxDeletes.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
      // 
      // tbTraxWrites
      // 
      this.tbTraxWrites.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(244)))), ((int)(((byte)(244)))), ((int)(((byte)(244)))));
      this.tbTraxWrites.Font = new System.Drawing.Font("Verdana", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.tbTraxWrites.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(0)))), ((int)(((byte)(192)))));
      this.tbTraxWrites.Location = new System.Drawing.Point(81, 385);
      this.tbTraxWrites.Name = "tbTraxWrites";
      this.tbTraxWrites.Size = new System.Drawing.Size(64, 21);
      this.tbTraxWrites.TabIndex = 28;
      this.tbTraxWrites.Text = "000";
      this.tbTraxWrites.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
      // 
      // tbTraxThreads
      // 
      this.tbTraxThreads.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(244)))), ((int)(((byte)(244)))), ((int)(((byte)(244)))));
      this.tbTraxThreads.Font = new System.Drawing.Font("Verdana", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.tbTraxThreads.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(0)))), ((int)(((byte)(192)))));
      this.tbTraxThreads.Location = new System.Drawing.Point(6, 55);
      this.tbTraxThreads.Name = "tbTraxThreads";
      this.tbTraxThreads.Size = new System.Drawing.Size(64, 21);
      this.tbTraxThreads.TabIndex = 27;
      this.tbTraxThreads.Text = "1";
      this.tbTraxThreads.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
      // 
      // sbTraxDeletes
      // 
      this.sbTraxDeletes.Location = new System.Drawing.Point(162, 64);
      this.sbTraxDeletes.Maximum = 2000000;
      this.sbTraxDeletes.Name = "sbTraxDeletes";
      this.sbTraxDeletes.Size = new System.Drawing.Size(36, 318);
      this.sbTraxDeletes.TabIndex = 23;
      this.sbTraxDeletes.Scroll += new System.Windows.Forms.ScrollEventHandler(this.sbTraxDeletes_Scroll);
      // 
      // sbTraxWrites
      // 
      this.sbTraxWrites.Location = new System.Drawing.Point(92, 64);
      this.sbTraxWrites.Maximum = 2000000;
      this.sbTraxWrites.Name = "sbTraxWrites";
      this.sbTraxWrites.Size = new System.Drawing.Size(36, 318);
      this.sbTraxWrites.TabIndex = 22;
      this.sbTraxWrites.Scroll += new System.Windows.Forms.ScrollEventHandler(this.sbTraxWrites_Scroll);
      // 
      // chkTraxer
      // 
      this.chkTraxer.AutoSize = true;
      this.chkTraxer.Location = new System.Drawing.Point(54, 0);
      this.chkTraxer.Name = "chkTraxer";
      this.chkTraxer.Size = new System.Drawing.Size(56, 17);
      this.chkTraxer.TabIndex = 20;
      this.chkTraxer.Text = "Active";
      this.chkTraxer.UseVisualStyleBackColor = true;
      // 
      // label13
      // 
      this.label13.AutoSize = true;
      this.label13.Location = new System.Drawing.Point(158, 37);
      this.label13.Name = "label13";
      this.label13.Size = new System.Drawing.Size(45, 13);
      this.label13.TabIndex = 12;
      this.label13.Text = "Del/sec";
      // 
      // label12
      // 
      this.label12.AutoSize = true;
      this.label12.Location = new System.Drawing.Point(83, 37);
      this.label12.Name = "label12";
      this.label12.Size = new System.Drawing.Size(54, 13);
      this.label12.TabIndex = 10;
      this.label12.Text = "Write/sec";
      // 
      // label11
      // 
      this.label11.AutoSize = true;
      this.label11.Location = new System.Drawing.Point(15, 37);
      this.label11.Name = "label11";
      this.label11.Size = new System.Drawing.Size(46, 13);
      this.label11.TabIndex = 8;
      this.label11.Text = "Threads";
      // 
      // lbErrors
      // 
      this.lbErrors.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.lbErrors.Font = new System.Drawing.Font("Courier New", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.lbErrors.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
      this.lbErrors.FormattingEnabled = true;
      this.lbErrors.ItemHeight = 14;
      this.lbErrors.Location = new System.Drawing.Point(5, 496);
      this.lbErrors.Name = "lbErrors";
      this.lbErrors.Size = new System.Drawing.Size(960, 242);
      this.lbErrors.TabIndex = 23;
      // 
      // label14
      // 
      this.label14.AutoSize = true;
      this.label14.Font = new System.Drawing.Font("Verdana", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.label14.Location = new System.Drawing.Point(12, 100);
      this.label14.Name = "label14";
      this.label14.Size = new System.Drawing.Size(126, 13);
      this.label14.TabIndex = 25;
      this.label14.Text = "Utilized Bytes/Object";
      // 
      // stbUtilizedBytesObject
      // 
      this.stbUtilizedBytesObject.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(244)))), ((int)(((byte)(244)))), ((int)(((byte)(244)))));
      this.stbUtilizedBytesObject.Font = new System.Drawing.Font("Verdana", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.stbUtilizedBytesObject.ForeColor = System.Drawing.Color.Teal;
      this.stbUtilizedBytesObject.Location = new System.Drawing.Point(203, 95);
      this.stbUtilizedBytesObject.Name = "stbUtilizedBytesObject";
      this.stbUtilizedBytesObject.ReadOnly = true;
      this.stbUtilizedBytesObject.Size = new System.Drawing.Size(98, 23);
      this.stbUtilizedBytesObject.TabIndex = 24;
      this.stbUtilizedBytesObject.Text = "000";
      this.stbUtilizedBytesObject.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
      // 
      // label15
      // 
      this.label15.AutoSize = true;
      this.label15.Font = new System.Drawing.Font("Verdana", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.label15.Location = new System.Drawing.Point(12, 158);
      this.label15.Name = "label15";
      this.label15.Size = new System.Drawing.Size(120, 13);
      this.label15.TabIndex = 27;
      this.label15.Text = "Ovrhd Bytes/Object";
      // 
      // stbOverheadBytesObject
      // 
      this.stbOverheadBytesObject.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(244)))), ((int)(((byte)(244)))), ((int)(((byte)(244)))));
      this.stbOverheadBytesObject.Font = new System.Drawing.Font("Verdana", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.stbOverheadBytesObject.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(0)))), ((int)(((byte)(192)))));
      this.stbOverheadBytesObject.Location = new System.Drawing.Point(203, 153);
      this.stbOverheadBytesObject.Name = "stbOverheadBytesObject";
      this.stbOverheadBytesObject.ReadOnly = true;
      this.stbOverheadBytesObject.Size = new System.Drawing.Size(98, 23);
      this.stbOverheadBytesObject.TabIndex = 26;
      this.stbOverheadBytesObject.Text = "000";
      this.stbOverheadBytesObject.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
      // 
      // lbSpeed
      // 
      this.lbSpeed.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(238)))), ((int)(((byte)(241)))), ((int)(((byte)(235)))));
      this.lbSpeed.ColumnWidth = 65;
      this.lbSpeed.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.lbSpeed.FormattingEnabled = true;
      this.lbSpeed.Location = new System.Drawing.Point(5, 375);
      this.lbSpeed.MultiColumn = true;
      this.lbSpeed.Name = "lbSpeed";
      this.lbSpeed.Size = new System.Drawing.Size(304, 95);
      this.lbSpeed.TabIndex = 28;
      // 
      // label17
      // 
      this.label17.AutoSize = true;
      this.label17.Font = new System.Drawing.Font("Verdana", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.label17.Location = new System.Drawing.Point(12, 256);
      this.label17.Name = "label17";
      this.label17.Size = new System.Drawing.Size(124, 13);
      this.label17.TabIndex = 30;
      this.label17.Text = "Mem Capacity Bytes";
      // 
      // stbMemCapacityBytes
      // 
      this.stbMemCapacityBytes.BackColor = System.Drawing.Color.Black;
      this.stbMemCapacityBytes.Font = new System.Drawing.Font("Verdana", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.stbMemCapacityBytes.ForeColor = System.Drawing.Color.Lime;
      this.stbMemCapacityBytes.Location = new System.Drawing.Point(151, 251);
      this.stbMemCapacityBytes.Name = "stbMemCapacityBytes";
      this.stbMemCapacityBytes.ReadOnly = true;
      this.stbMemCapacityBytes.Size = new System.Drawing.Size(150, 23);
      this.stbMemCapacityBytes.TabIndex = 29;
      this.stbMemCapacityBytes.Text = "000";
      this.stbMemCapacityBytes.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
      // 
      // btnCompact
      // 
      this.btnCompact.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.btnCompact.ForeColor = System.Drawing.Color.Teal;
      this.btnCompact.Location = new System.Drawing.Point(90, 296);
      this.btnCompact.Name = "btnCompact";
      this.btnCompact.Size = new System.Drawing.Size(49, 42);
      this.btnCompact.TabIndex = 31;
      this.btnCompact.Text = "Compact";
      this.btnCompact.UseVisualStyleBackColor = true;
      this.btnCompact.Click += new System.EventHandler(this.btnCompact_Click);
      // 
      // chkRaw
      // 
      this.chkRaw.AutoSize = true;
      this.chkRaw.Location = new System.Drawing.Point(143, 111);
      this.chkRaw.Name = "chkRaw";
      this.chkRaw.Size = new System.Drawing.Size(79, 17);
      this.chkRaw.TabIndex = 38;
      this.chkRaw.Text = "Raw Buffer";
      this.chkRaw.UseVisualStyleBackColor = true;
      // 
      // PileForm
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.ClientSize = new System.Drawing.Size(970, 742);
      this.Controls.Add(this.btnCompact);
      this.Controls.Add(this.label17);
      this.Controls.Add(this.stbMemCapacityBytes);
      this.Controls.Add(this.lbSpeed);
      this.Controls.Add(this.label15);
      this.Controls.Add(this.stbOverheadBytesObject);
      this.Controls.Add(this.label14);
      this.Controls.Add(this.stbUtilizedBytesObject);
      this.Controls.Add(this.lbErrors);
      this.Controls.Add(this.groupBox1);
      this.Controls.Add(this.label10);
      this.Controls.Add(this.tbMaxMemoryMb);
      this.Controls.Add(this.label9);
      this.Controls.Add(this.tbSegmentSize);
      this.Controls.Add(this.chkSpeed);
      this.Controls.Add(this.btnCrawl);
      this.Controls.Add(this.btnGC);
      this.Controls.Add(this.btnPurge);
      this.Controls.Add(this.grpPerson);
      this.Controls.Add(this.btnStop);
      this.Controls.Add(this.label5);
      this.Controls.Add(this.stbSegTotalCount);
      this.Controls.Add(this.label6);
      this.Controls.Add(this.stbSegments);
      this.Controls.Add(this.label3);
      this.Controls.Add(this.stbOverheadBytes);
      this.Controls.Add(this.label4);
      this.Controls.Add(this.stbUtilizedBytes);
      this.Controls.Add(this.label2);
      this.Controls.Add(this.stbMemBytes);
      this.Controls.Add(this.label1);
      this.Controls.Add(this.stbObjectCount);
      this.Controls.Add(this.btnStart);
      this.Name = "PileForm";
      this.Text = "PileForm";
      this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.PileForm_FormClosed);
      this.Load += new System.EventHandler(this.PileForm_Load);
      this.grpPerson.ResumeLayout(false);
      this.grpPerson.PerformLayout();
      this.groupBox1.ResumeLayout(false);
      this.groupBox1.PerformLayout();
      this.ResumeLayout(false);
      this.PerformLayout();

    }

    #endregion

    private System.Windows.Forms.Button btnStart;
    private System.Windows.Forms.TextBox stbObjectCount;
    private System.Windows.Forms.Label label1;
    private System.Windows.Forms.Label label2;
    private System.Windows.Forms.TextBox stbMemBytes;
    private System.Windows.Forms.Label label3;
    private System.Windows.Forms.TextBox stbOverheadBytes;
    private System.Windows.Forms.Label label4;
    private System.Windows.Forms.TextBox stbUtilizedBytes;
    private System.Windows.Forms.Label label5;
    private System.Windows.Forms.TextBox stbSegTotalCount;
    private System.Windows.Forms.Label label6;
    private System.Windows.Forms.TextBox stbSegments;
    private System.Windows.Forms.Button btnStop;
    private System.Windows.Forms.Timer tmrStatus;
    private System.Windows.Forms.GroupBox grpPerson;
    private System.Windows.Forms.Button btnPersonGet;
    private System.Windows.Forms.ListBox lbPerson;
    private System.Windows.Forms.Button btnPersonDelete;
    private System.Windows.Forms.Button btnPersonPut;
    private System.Windows.Forms.Label label7;
    private System.Windows.Forms.TextBox tbPersonCount;
    private System.Windows.Forms.Button btnPurge;
    private System.Windows.Forms.Button btnGC;
    private System.Windows.Forms.Label label8;
    private System.Windows.Forms.TextBox tbPersonThreads;
    private System.Windows.Forms.Button btnPersonParaPut;
    private System.Windows.Forms.Button btnCrawl;
    private System.Windows.Forms.CheckBox chkSpeed;
    private System.Windows.Forms.Label label9;
    private System.Windows.Forms.TextBox tbSegmentSize;
    private System.Windows.Forms.Label label10;
    private System.Windows.Forms.TextBox tbMaxMemoryMb;
    private System.Windows.Forms.GroupBox groupBox1;
    private System.Windows.Forms.CheckBox chkTraxer;
    private System.Windows.Forms.Label label13;
    private System.Windows.Forms.Label label12;
    private System.Windows.Forms.Label label11;
    private System.Windows.Forms.VScrollBar sbTraxDeletes;
    private System.Windows.Forms.VScrollBar sbTraxWrites;
    private System.Windows.Forms.ListBox lbErrors;
    private System.Windows.Forms.Label label14;
    private System.Windows.Forms.TextBox stbUtilizedBytesObject;
    private System.Windows.Forms.Label label15;
    private System.Windows.Forms.TextBox stbOverheadBytesObject;
    private System.Windows.Forms.TextBox tbTraxDeletes;
    private System.Windows.Forms.TextBox tbTraxWrites;
    private System.Windows.Forms.TextBox tbTraxThreads;
    private System.Windows.Forms.ListBox lbSpeed;
    private System.Windows.Forms.TextBox tbTraxVariance;
    private System.Windows.Forms.Label lbVar;
    private System.Windows.Forms.TextBox tbPersonVariance;
    private System.Windows.Forms.Label label16;
    private System.Windows.Forms.Button btnPersonSizeOf;
    private System.Windows.Forms.Label label17;
    private System.Windows.Forms.TextBox stbMemCapacityBytes;
    private System.Windows.Forms.Button btnStruct;
    private System.Windows.Forms.Button btnPersonParaGet;
    private System.Windows.Forms.Button btnCompact;
    private System.Windows.Forms.CheckBox chkRaw;
  }
}