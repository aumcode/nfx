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
namespace WinFormsTest
{
    partial class GlueForm
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
      this.button1 = new System.Windows.Forms.Button();
      this.button2 = new System.Windows.Forms.Button();
      this.button3 = new System.Windows.Forms.Button();
      this.button4 = new System.Windows.Forms.Button();
      this.cbo = new System.Windows.Forms.ComboBox();
      this.btnInit = new System.Windows.Forms.Button();
      this.btnAdd = new System.Windows.Forms.Button();
      this.btnSub = new System.Windows.Forms.Button();
      this.btnDone = new System.Windows.Forms.Button();
      this.tbCalc = new System.Windows.Forms.TextBox();
      this.label1 = new System.Windows.Forms.Label();
      this.edRepeat = new System.Windows.Forms.TextBox();
      this.button5 = new System.Windows.Forms.Button();
      this.btnReactor2 = new System.Windows.Forms.Button();
      this.tbID = new System.Windows.Forms.TextBox();
      this.tbPwd = new System.Windows.Forms.TextBox();
      this.label2 = new System.Windows.Forms.Label();
      this.label3 = new System.Windows.Forms.Label();
      this.btnBadMsg = new System.Windows.Forms.Button();
      this.btnBadPayload = new System.Windows.Forms.Button();
      this.btnManyParallel = new System.Windows.Forms.Button();
      this.tbReactors = new System.Windows.Forms.TextBox();
      this.label4 = new System.Windows.Forms.Label();
      this.label5 = new System.Windows.Forms.Label();
      this.tbCallsPerReactor = new System.Windows.Forms.TextBox();
      this.chkImpersonate = new System.Windows.Forms.CheckBox();
      this.tbNote = new System.Windows.Forms.TextBox();
      this.btnParallelDispatch = new System.Windows.Forms.Button();
      this.chkUnsecureEcho = new System.Windows.Forms.CheckBox();
      this.btnManyDBWorks = new System.Windows.Forms.Button();
      this.button6 = new System.Windows.Forms.Button();
      this.button7 = new System.Windows.Forms.Button();
      this.btnSimple = new System.Windows.Forms.Button();
      this.btGCCollect = new System.Windows.Forms.Button();
      this.chkAutoDispatch = new System.Windows.Forms.CheckBox();
      this.tmrAuto = new System.Windows.Forms.Timer(this.components);
      this.chkArgsMarshalling = new System.Windows.Forms.CheckBox();
      this.btnSimpleWork = new System.Windows.Forms.Button();
      this.chkDumpMessages = new System.Windows.Forms.CheckBox();
      this.tbRecCount = new System.Windows.Forms.TextBox();
      this.tbWaitFrom = new System.Windows.Forms.TextBox();
      this.tbWaitTo = new System.Windows.Forms.TextBox();
      this.SuspendLayout();
      // 
      // button1
      // 
      this.button1.Location = new System.Drawing.Point(12, 92);
      this.button1.Name = "button1";
      this.button1.Size = new System.Drawing.Size(75, 23);
      this.button1.TabIndex = 0;
      this.button1.Text = "Echo ";
      this.button1.UseVisualStyleBackColor = true;
      this.button1.Click += new System.EventHandler(this.button1_Click);
      // 
      // button2
      // 
      this.button2.Location = new System.Drawing.Point(104, 92);
      this.button2.Name = "button2";
      this.button2.Size = new System.Drawing.Size(75, 23);
      this.button2.TabIndex = 1;
      this.button2.Text = "Many Echos";
      this.button2.UseVisualStyleBackColor = true;
      this.button2.Click += new System.EventHandler(this.button2_Click);
      // 
      // button3
      // 
      this.button3.Location = new System.Drawing.Point(12, 159);
      this.button3.Name = "button3";
      this.button3.Size = new System.Drawing.Size(75, 23);
      this.button3.TabIndex = 2;
      this.button3.Text = "Notify One Way";
      this.button3.UseVisualStyleBackColor = true;
      this.button3.Click += new System.EventHandler(this.button3_Click);
      // 
      // button4
      // 
      this.button4.Location = new System.Drawing.Point(104, 159);
      this.button4.Name = "button4";
      this.button4.Size = new System.Drawing.Size(75, 23);
      this.button4.TabIndex = 3;
      this.button4.Text = "Notify Many";
      this.button4.UseVisualStyleBackColor = true;
      this.button4.Click += new System.EventHandler(this.button4_Click);
      // 
      // cbo
      // 
      this.cbo.FormattingEnabled = true;
      this.cbo.Items.AddRange(new object[] {
            "mpx://sextod:5701",
            "mpx://octod:5701",
            "mpx://cube:5701",
            "mpx://127.0.0.1:5701",
            "-------------------------",
            "sync://sextod:8000",
            "sync://octod:8000",
            "sync://cube:8000",
            "sync://127.0.0.1:8000",
            ""});
      this.cbo.Location = new System.Drawing.Point(12, 12);
      this.cbo.Name = "cbo";
      this.cbo.Size = new System.Drawing.Size(368, 21);
      this.cbo.TabIndex = 4;
      this.cbo.SelectedIndexChanged += new System.EventHandler(this.comboBox1_SelectedIndexChanged);
      // 
      // btnInit
      // 
      this.btnInit.Location = new System.Drawing.Point(549, 92);
      this.btnInit.Name = "btnInit";
      this.btnInit.Size = new System.Drawing.Size(75, 23);
      this.btnInit.TabIndex = 5;
      this.btnInit.Text = "Init";
      this.btnInit.UseVisualStyleBackColor = true;
      this.btnInit.Click += new System.EventHandler(this.btnInit_Click);
      // 
      // btnAdd
      // 
      this.btnAdd.Location = new System.Drawing.Point(549, 121);
      this.btnAdd.Name = "btnAdd";
      this.btnAdd.Size = new System.Drawing.Size(75, 23);
      this.btnAdd.TabIndex = 6;
      this.btnAdd.Text = "Add";
      this.btnAdd.UseVisualStyleBackColor = true;
      this.btnAdd.Click += new System.EventHandler(this.btnAdd_Click);
      // 
      // btnSub
      // 
      this.btnSub.Location = new System.Drawing.Point(549, 150);
      this.btnSub.Name = "btnSub";
      this.btnSub.Size = new System.Drawing.Size(75, 23);
      this.btnSub.TabIndex = 7;
      this.btnSub.Text = "Subtract";
      this.btnSub.UseVisualStyleBackColor = true;
      this.btnSub.Click += new System.EventHandler(this.btnSub_Click);
      // 
      // btnDone
      // 
      this.btnDone.Location = new System.Drawing.Point(549, 179);
      this.btnDone.Name = "btnDone";
      this.btnDone.Size = new System.Drawing.Size(75, 23);
      this.btnDone.TabIndex = 8;
      this.btnDone.Text = "Done";
      this.btnDone.UseVisualStyleBackColor = true;
      this.btnDone.Click += new System.EventHandler(this.btnDone_Click);
      // 
      // tbCalc
      // 
      this.tbCalc.Location = new System.Drawing.Point(549, 48);
      this.tbCalc.Name = "tbCalc";
      this.tbCalc.Size = new System.Drawing.Size(167, 20);
      this.tbCalc.TabIndex = 9;
      // 
      // label1
      // 
      this.label1.AutoSize = true;
      this.label1.Location = new System.Drawing.Point(13, 54);
      this.label1.Name = "label1";
      this.label1.Size = new System.Drawing.Size(45, 13);
      this.label1.TabIndex = 10;
      this.label1.Text = "Repeat:";
      // 
      // edRepeat
      // 
      this.edRepeat.Location = new System.Drawing.Point(65, 51);
      this.edRepeat.Name = "edRepeat";
      this.edRepeat.Size = new System.Drawing.Size(50, 20);
      this.edRepeat.TabIndex = 11;
      this.edRepeat.Text = "1000";
      this.edRepeat.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.edRepeat_KeyPress);
      // 
      // button5
      // 
      this.button5.Location = new System.Drawing.Point(345, 92);
      this.button5.Name = "button5";
      this.button5.Size = new System.Drawing.Size(75, 23);
      this.button5.TabIndex = 12;
      this.button5.Text = "Reactor";
      this.button5.UseVisualStyleBackColor = true;
      this.button5.Click += new System.EventHandler(this.btnReactor_Click);
      // 
      // btnReactor2
      // 
      this.btnReactor2.ForeColor = System.Drawing.Color.Black;
      this.btnReactor2.Location = new System.Drawing.Point(345, 121);
      this.btnReactor2.Name = "btnReactor2";
      this.btnReactor2.Size = new System.Drawing.Size(75, 23);
      this.btnReactor2.TabIndex = 13;
      this.btnReactor2.Text = "Reactor2";
      this.btnReactor2.UseVisualStyleBackColor = true;
      this.btnReactor2.Click += new System.EventHandler(this.btnReactor2_Click);
      // 
      // tbID
      // 
      this.tbID.Location = new System.Drawing.Point(9, 242);
      this.tbID.Name = "tbID";
      this.tbID.Size = new System.Drawing.Size(106, 20);
      this.tbID.TabIndex = 14;
      this.tbID.Text = "dima";
      // 
      // tbPwd
      // 
      this.tbPwd.Location = new System.Drawing.Point(9, 282);
      this.tbPwd.Name = "tbPwd";
      this.tbPwd.Size = new System.Drawing.Size(106, 20);
      this.tbPwd.TabIndex = 15;
      this.tbPwd.Text = "dima";
      // 
      // label2
      // 
      this.label2.AutoSize = true;
      this.label2.Location = new System.Drawing.Point(9, 226);
      this.label2.Name = "label2";
      this.label2.Size = new System.Drawing.Size(18, 13);
      this.label2.TabIndex = 16;
      this.label2.Text = "ID";
      // 
      // label3
      // 
      this.label3.AutoSize = true;
      this.label3.Location = new System.Drawing.Point(9, 266);
      this.label3.Name = "label3";
      this.label3.Size = new System.Drawing.Size(53, 13);
      this.label3.TabIndex = 17;
      this.label3.Text = "Password";
      // 
      // btnBadMsg
      // 
      this.btnBadMsg.Location = new System.Drawing.Point(196, 92);
      this.btnBadMsg.Name = "btnBadMsg";
      this.btnBadMsg.Size = new System.Drawing.Size(75, 23);
      this.btnBadMsg.TabIndex = 18;
      this.btnBadMsg.Text = "Bad Msg";
      this.btnBadMsg.UseVisualStyleBackColor = true;
      this.btnBadMsg.Click += new System.EventHandler(this.btnBadMsg_Click);
      // 
      // btnBadPayload
      // 
      this.btnBadPayload.Location = new System.Drawing.Point(196, 121);
      this.btnBadPayload.Name = "btnBadPayload";
      this.btnBadPayload.Size = new System.Drawing.Size(75, 23);
      this.btnBadPayload.TabIndex = 19;
      this.btnBadPayload.Text = "Bad Payload";
      this.btnBadPayload.UseVisualStyleBackColor = true;
      this.btnBadPayload.Click += new System.EventHandler(this.btnBadPayload_Click);
      // 
      // btnManyParallel
      // 
      this.btnManyParallel.Location = new System.Drawing.Point(226, 394);
      this.btnManyParallel.Name = "btnManyParallel";
      this.btnManyParallel.Size = new System.Drawing.Size(171, 24);
      this.btnManyParallel.TabIndex = 20;
      this.btnManyParallel.Text = "Many Parallel Echos";
      this.btnManyParallel.UseVisualStyleBackColor = true;
      this.btnManyParallel.Click += new System.EventHandler(this.btnManyParallel_Click);
      // 
      // tbReactors
      // 
      this.tbReactors.Location = new System.Drawing.Point(236, 315);
      this.tbReactors.Name = "tbReactors";
      this.tbReactors.Size = new System.Drawing.Size(106, 20);
      this.tbReactors.TabIndex = 21;
      this.tbReactors.Text = "10";
      // 
      // label4
      // 
      this.label4.AutoSize = true;
      this.label4.Location = new System.Drawing.Point(233, 297);
      this.label4.Name = "label4";
      this.label4.Size = new System.Drawing.Size(94, 13);
      this.label4.TabIndex = 22;
      this.label4.Text = "Reactors/Threads";
      // 
      // label5
      // 
      this.label5.AutoSize = true;
      this.label5.Location = new System.Drawing.Point(233, 342);
      this.label5.Name = "label5";
      this.label5.Size = new System.Drawing.Size(89, 13);
      this.label5.TabIndex = 24;
      this.label5.Text = "Calls Per Reactor";
      // 
      // tbCallsPerReactor
      // 
      this.tbCallsPerReactor.Location = new System.Drawing.Point(236, 360);
      this.tbCallsPerReactor.Name = "tbCallsPerReactor";
      this.tbCallsPerReactor.Size = new System.Drawing.Size(106, 20);
      this.tbCallsPerReactor.TabIndex = 23;
      this.tbCallsPerReactor.Text = "1000";
      // 
      // chkImpersonate
      // 
      this.chkImpersonate.AutoSize = true;
      this.chkImpersonate.Location = new System.Drawing.Point(348, 338);
      this.chkImpersonate.Name = "chkImpersonate";
      this.chkImpersonate.Size = new System.Drawing.Size(84, 17);
      this.chkImpersonate.TabIndex = 25;
      this.chkImpersonate.Text = "Impersonate";
      this.chkImpersonate.UseVisualStyleBackColor = true;
      // 
      // tbNote
      // 
      this.tbNote.Location = new System.Drawing.Point(240, 162);
      this.tbNote.Multiline = true;
      this.tbNote.Name = "tbNote";
      this.tbNote.Size = new System.Drawing.Size(259, 117);
      this.tbNote.TabIndex = 26;
      // 
      // btnParallelDispatch
      // 
      this.btnParallelDispatch.Location = new System.Drawing.Point(421, 394);
      this.btnParallelDispatch.Name = "btnParallelDispatch";
      this.btnParallelDispatch.Size = new System.Drawing.Size(106, 37);
      this.btnParallelDispatch.TabIndex = 27;
      this.btnParallelDispatch.Text = "Many Parallel Dispatches";
      this.btnParallelDispatch.UseVisualStyleBackColor = true;
      this.btnParallelDispatch.Click += new System.EventHandler(this.btnParallelDispatch_Click);
      // 
      // chkUnsecureEcho
      // 
      this.chkUnsecureEcho.AutoSize = true;
      this.chkUnsecureEcho.Location = new System.Drawing.Point(144, 54);
      this.chkUnsecureEcho.Name = "chkUnsecureEcho";
      this.chkUnsecureEcho.Size = new System.Drawing.Size(120, 17);
      this.chkUnsecureEcho.TabIndex = 28;
      this.chkUnsecureEcho.Text = "Call Unsecure Echo";
      this.chkUnsecureEcho.UseVisualStyleBackColor = true;
      // 
      // btnManyDBWorks
      // 
      this.btnManyDBWorks.Location = new System.Drawing.Point(526, 266);
      this.btnManyDBWorks.Name = "btnManyDBWorks";
      this.btnManyDBWorks.Size = new System.Drawing.Size(171, 49);
      this.btnManyDBWorks.TabIndex = 29;
      this.btnManyDBWorks.Text = "Many Parallel DBWorks Dispatches";
      this.btnManyDBWorks.UseVisualStyleBackColor = true;
      this.btnManyDBWorks.Click += new System.EventHandler(this.btnManyDBWorks_Click);
      // 
      // button6
      // 
      this.button6.Location = new System.Drawing.Point(680, 394);
      this.button6.Name = "button6";
      this.button6.Size = new System.Drawing.Size(106, 37);
      this.button6.TabIndex = 30;
      this.button6.Text = "Many Parallel Dispatches";
      this.button6.UseVisualStyleBackColor = true;
      this.button6.Click += new System.EventHandler(this.button6_Click);
      // 
      // button7
      // 
      this.button7.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))));
      this.button7.Location = new System.Drawing.Point(549, 394);
      this.button7.Name = "button7";
      this.button7.Size = new System.Drawing.Size(106, 37);
      this.button7.TabIndex = 31;
      this.button7.Text = "Many Parallel Dispatches";
      this.button7.UseVisualStyleBackColor = false;
      this.button7.Click += new System.EventHandler(this.button7_Click);
      // 
      // btnSimple
      // 
      this.btnSimple.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(192)))));
      this.btnSimple.Location = new System.Drawing.Point(421, 448);
      this.btnSimple.Name = "btnSimple";
      this.btnSimple.Size = new System.Drawing.Size(106, 37);
      this.btnSimple.TabIndex = 27;
      this.btnSimple.Text = "Many Parallel Dispatches";
      this.btnSimple.UseVisualStyleBackColor = false;
      this.btnSimple.Click += new System.EventHandler(this.btnSimple_Click);
      // 
      // btGCCollect
      // 
      this.btGCCollect.Location = new System.Drawing.Point(705, 106);
      this.btGCCollect.Name = "btGCCollect";
      this.btGCCollect.Size = new System.Drawing.Size(88, 38);
      this.btGCCollect.TabIndex = 32;
      this.btGCCollect.Text = "GC Collect";
      this.btGCCollect.UseVisualStyleBackColor = true;
      this.btGCCollect.Click += new System.EventHandler(this.btGCCollect_Click);
      // 
      // chkAutoDispatch
      // 
      this.chkAutoDispatch.AutoSize = true;
      this.chkAutoDispatch.Location = new System.Drawing.Point(705, 266);
      this.chkAutoDispatch.Name = "chkAutoDispatch";
      this.chkAutoDispatch.Size = new System.Drawing.Size(93, 17);
      this.chkAutoDispatch.TabIndex = 33;
      this.chkAutoDispatch.Text = "Auto Dispatch";
      this.chkAutoDispatch.UseVisualStyleBackColor = true;
      // 
      // tmrAuto
      // 
      this.tmrAuto.Interval = 1000;
      this.tmrAuto.Tick += new System.EventHandler(this.tmrAuto_Tick);
      // 
      // chkArgsMarshalling
      // 
      this.chkArgsMarshalling.AutoSize = true;
      this.chkArgsMarshalling.Location = new System.Drawing.Point(580, 459);
      this.chkArgsMarshalling.Name = "chkArgsMarshalling";
      this.chkArgsMarshalling.Size = new System.Drawing.Size(112, 17);
      this.chkArgsMarshalling.TabIndex = 34;
      this.chkArgsMarshalling.Text = "ARGS Marshalling";
      this.chkArgsMarshalling.UseVisualStyleBackColor = true;
      // 
      // btnSimpleWork
      // 
      this.btnSimpleWork.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(192)))), ((int)(((byte)(255)))));
      this.btnSimpleWork.Location = new System.Drawing.Point(580, 482);
      this.btnSimpleWork.Name = "btnSimpleWork";
      this.btnSimpleWork.Size = new System.Drawing.Size(106, 37);
      this.btnSimpleWork.TabIndex = 35;
      this.btnSimpleWork.Text = "Simple Work";
      this.btnSimpleWork.UseVisualStyleBackColor = false;
      this.btnSimpleWork.Click += new System.EventHandler(this.btnSimpleWork_Click);
      // 
      // chkDumpMessages
      // 
      this.chkDumpMessages.AutoSize = true;
      this.chkDumpMessages.Location = new System.Drawing.Point(3, 640);
      this.chkDumpMessages.Name = "chkDumpMessages";
      this.chkDumpMessages.Size = new System.Drawing.Size(134, 17);
      this.chkDumpMessages.TabIndex = 36;
      this.chkDumpMessages.Text = "Dump Client Messages";
      this.chkDumpMessages.UseVisualStyleBackColor = true;
      this.chkDumpMessages.CheckedChanged += new System.EventHandler(this.chkDumpMessages_CheckedChanged);
      // 
      // tbRecCount
      // 
      this.tbRecCount.Location = new System.Drawing.Point(705, 294);
      this.tbRecCount.Name = "tbRecCount";
      this.tbRecCount.Size = new System.Drawing.Size(83, 20);
      this.tbRecCount.TabIndex = 21;
      this.tbRecCount.Text = "10";
      // 
      // tbWaitFrom
      // 
      this.tbWaitFrom.Location = new System.Drawing.Point(705, 320);
      this.tbWaitFrom.Name = "tbWaitFrom";
      this.tbWaitFrom.Size = new System.Drawing.Size(83, 20);
      this.tbWaitFrom.TabIndex = 21;
      this.tbWaitFrom.Text = "0";
      // 
      // tbWaitTo
      // 
      this.tbWaitTo.Location = new System.Drawing.Point(705, 346);
      this.tbWaitTo.Name = "tbWaitTo";
      this.tbWaitTo.Size = new System.Drawing.Size(83, 20);
      this.tbWaitTo.TabIndex = 21;
      this.tbWaitTo.Text = "0";
      // 
      // GlueForm
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.ClientSize = new System.Drawing.Size(805, 669);
      this.Controls.Add(this.chkDumpMessages);
      this.Controls.Add(this.btnSimpleWork);
      this.Controls.Add(this.chkArgsMarshalling);
      this.Controls.Add(this.chkAutoDispatch);
      this.Controls.Add(this.btGCCollect);
      this.Controls.Add(this.button7);
      this.Controls.Add(this.button6);
      this.Controls.Add(this.btnManyDBWorks);
      this.Controls.Add(this.chkUnsecureEcho);
      this.Controls.Add(this.btnSimple);
      this.Controls.Add(this.btnParallelDispatch);
      this.Controls.Add(this.tbNote);
      this.Controls.Add(this.chkImpersonate);
      this.Controls.Add(this.label5);
      this.Controls.Add(this.tbCallsPerReactor);
      this.Controls.Add(this.label4);
      this.Controls.Add(this.tbWaitTo);
      this.Controls.Add(this.tbWaitFrom);
      this.Controls.Add(this.tbRecCount);
      this.Controls.Add(this.tbReactors);
      this.Controls.Add(this.btnManyParallel);
      this.Controls.Add(this.btnBadPayload);
      this.Controls.Add(this.btnBadMsg);
      this.Controls.Add(this.label3);
      this.Controls.Add(this.label2);
      this.Controls.Add(this.tbPwd);
      this.Controls.Add(this.tbID);
      this.Controls.Add(this.btnReactor2);
      this.Controls.Add(this.button5);
      this.Controls.Add(this.edRepeat);
      this.Controls.Add(this.label1);
      this.Controls.Add(this.tbCalc);
      this.Controls.Add(this.btnDone);
      this.Controls.Add(this.btnSub);
      this.Controls.Add(this.btnAdd);
      this.Controls.Add(this.btnInit);
      this.Controls.Add(this.cbo);
      this.Controls.Add(this.button4);
      this.Controls.Add(this.button3);
      this.Controls.Add(this.button2);
      this.Controls.Add(this.button1);
      this.Name = "GlueForm";
      this.Text = "GlueForm";
      this.ResumeLayout(false);
      this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Button button2;
        private System.Windows.Forms.Button button3;
        private System.Windows.Forms.Button button4;
        private System.Windows.Forms.ComboBox cbo;
        private System.Windows.Forms.Button btnInit;
        private System.Windows.Forms.Button btnAdd;
        private System.Windows.Forms.Button btnSub;
        private System.Windows.Forms.Button btnDone;
        private System.Windows.Forms.TextBox tbCalc;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox edRepeat;
        private System.Windows.Forms.Button button5;
        private System.Windows.Forms.Button btnReactor2;
        private System.Windows.Forms.TextBox tbID;
        private System.Windows.Forms.TextBox tbPwd;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Button btnBadMsg;
        private System.Windows.Forms.Button btnBadPayload;
        private System.Windows.Forms.Button btnManyParallel;
        private System.Windows.Forms.TextBox tbReactors;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.TextBox tbCallsPerReactor;
        private System.Windows.Forms.CheckBox chkImpersonate;
        private System.Windows.Forms.TextBox tbNote;
        private System.Windows.Forms.Button btnParallelDispatch;
        private System.Windows.Forms.CheckBox chkUnsecureEcho;
        private System.Windows.Forms.Button btnManyDBWorks;
        private System.Windows.Forms.Button button6;
        private System.Windows.Forms.Button button7;
        private System.Windows.Forms.Button btnSimple;
        private System.Windows.Forms.Button btGCCollect;
        private System.Windows.Forms.CheckBox chkAutoDispatch;
        private System.Windows.Forms.Timer tmrAuto;
        private System.Windows.Forms.CheckBox chkArgsMarshalling;
        private System.Windows.Forms.Button btnSimpleWork;
        private System.Windows.Forms.CheckBox chkDumpMessages;
        private System.Windows.Forms.TextBox tbRecCount;
        private System.Windows.Forms.TextBox tbWaitFrom;
        private System.Windows.Forms.TextBox tbWaitTo;
    }
}