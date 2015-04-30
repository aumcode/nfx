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


namespace WinFormsTest
{
    partial class MongoDBForm
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
            this.edtID = new System.Windows.Forms.TextBox();
            this.btnLoad = new System.Windows.Forms.Button();
            this.btnCreate = new System.Windows.Forms.Button();
            this.btnEdit = new System.Windows.Forms.Button();
            this.btnPost = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            this.btnRevert = new System.Windows.Forms.Button();
            this.btnSave = new System.Windows.Forms.Button();
            this.btnBatch = new System.Windows.Forms.Button();
            this.btnDelete = new System.Windows.Forms.Button();
            this.pnl = new NFX.WinForms.Views.RecordContextPanel();
            this.fieldView8 = new NFX.WinForms.Views.FieldView();
            this.fieldView7 = new NFX.WinForms.Views.FieldView();
            this.fieldView6 = new NFX.WinForms.Views.FieldView();
            this.fieldView5 = new NFX.WinForms.Views.FieldView();
            this.fieldView4 = new NFX.WinForms.Views.FieldView();
            this.fieldView3 = new NFX.WinForms.Views.FieldView();
            this.fieldView2 = new NFX.WinForms.Views.FieldView();
            this.fieldView1 = new NFX.WinForms.Views.FieldView();
            this.fieldView9 = new NFX.WinForms.Views.FieldView();
            ((System.ComponentModel.ISupportInitialize)(this.pnl)).BeginInit();
            this.pnl.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.fieldView8)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.fieldView7)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.fieldView6)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.fieldView5)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.fieldView4)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.fieldView3)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.fieldView2)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.fieldView1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.fieldView9)).BeginInit();
            this.SuspendLayout();
            // 
            // edtID
            // 
            this.edtID.Location = new System.Drawing.Point(171, 15);
            this.edtID.Name = "edtID";
            this.edtID.Size = new System.Drawing.Size(293, 20);
            this.edtID.TabIndex = 13;
            // 
            // btnLoad
            // 
            this.btnLoad.Location = new System.Drawing.Point(503, 15);
            this.btnLoad.Name = "btnLoad";
            this.btnLoad.Size = new System.Drawing.Size(75, 23);
            this.btnLoad.TabIndex = 14;
            this.btnLoad.Text = "Load";
            this.btnLoad.UseVisualStyleBackColor = true;
            this.btnLoad.Click += new System.EventHandler(this.btnLoad_Click);
            // 
            // btnCreate
            // 
            this.btnCreate.Location = new System.Drawing.Point(503, 233);
            this.btnCreate.Name = "btnCreate";
            this.btnCreate.Size = new System.Drawing.Size(75, 23);
            this.btnCreate.TabIndex = 15;
            this.btnCreate.Text = "Create";
            this.btnCreate.UseVisualStyleBackColor = true;
            this.btnCreate.Click += new System.EventHandler(this.btnCreate_Click);
            // 
            // btnEdit
            // 
            this.btnEdit.Location = new System.Drawing.Point(503, 264);
            this.btnEdit.Name = "btnEdit";
            this.btnEdit.Size = new System.Drawing.Size(75, 23);
            this.btnEdit.TabIndex = 16;
            this.btnEdit.Text = "Edit";
            this.btnEdit.UseVisualStyleBackColor = true;
            this.btnEdit.Click += new System.EventHandler(this.btnEdit_Click);
            // 
            // btnPost
            // 
            this.btnPost.Location = new System.Drawing.Point(503, 434);
            this.btnPost.Name = "btnPost";
            this.btnPost.Size = new System.Drawing.Size(75, 23);
            this.btnPost.TabIndex = 17;
            this.btnPost.Text = "Post";
            this.btnPost.UseVisualStyleBackColor = true;
            this.btnPost.Click += new System.EventHandler(this.btnPost_Click);
            // 
            // btnCancel
            // 
            this.btnCancel.Location = new System.Drawing.Point(503, 463);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(75, 23);
            this.btnCancel.TabIndex = 18;
            this.btnCancel.Text = "Cancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
            // 
            // btnRevert
            // 
            this.btnRevert.Location = new System.Drawing.Point(503, 342);
            this.btnRevert.Name = "btnRevert";
            this.btnRevert.Size = new System.Drawing.Size(75, 23);
            this.btnRevert.TabIndex = 19;
            this.btnRevert.Text = "Revert";
            this.btnRevert.UseVisualStyleBackColor = true;
            this.btnRevert.Click += new System.EventHandler(this.btnRevert_Click);
            // 
            // btnSave
            // 
            this.btnSave.Location = new System.Drawing.Point(503, 44);
            this.btnSave.Name = "btnSave";
            this.btnSave.Size = new System.Drawing.Size(75, 23);
            this.btnSave.TabIndex = 20;
            this.btnSave.Text = "Save";
            this.btnSave.UseVisualStyleBackColor = true;
            this.btnSave.Click += new System.EventHandler(this.btnSave_Click);
            // 
            // btnBatch
            // 
            this.btnBatch.Location = new System.Drawing.Point(503, 162);
            this.btnBatch.Name = "btnBatch";
            this.btnBatch.Size = new System.Drawing.Size(75, 44);
            this.btnBatch.TabIndex = 21;
            this.btnBatch.Text = "Batch of 10,000";
            this.btnBatch.UseVisualStyleBackColor = true;
            this.btnBatch.Click += new System.EventHandler(this.btnBatch_Click);
            // 
            // btnDelete
            // 
            this.btnDelete.Location = new System.Drawing.Point(503, 79);
            this.btnDelete.Name = "btnDelete";
            this.btnDelete.Size = new System.Drawing.Size(75, 23);
            this.btnDelete.TabIndex = 22;
            this.btnDelete.Text = "Delete";
            this.btnDelete.UseVisualStyleBackColor = true;
            this.btnDelete.Click += new System.EventHandler(this.btnDelete_Click);
            // 
            // pnl
            // 
            this.pnl.Controls.Add(this.fieldView9);
            this.pnl.Controls.Add(this.fieldView8);
            this.pnl.Controls.Add(this.fieldView7);
            this.pnl.Controls.Add(this.fieldView6);
            this.pnl.Controls.Add(this.fieldView5);
            this.pnl.Controls.Add(this.fieldView4);
            this.pnl.Controls.Add(this.fieldView3);
            this.pnl.Controls.Add(this.fieldView2);
            this.pnl.Controls.Add(this.fieldView1);
            this.pnl.Location = new System.Drawing.Point(12, 48);
            this.pnl.Name = "pnl";
            this.pnl.Size = new System.Drawing.Size(452, 579);
            this.pnl.TabIndex = 12;
            this.pnl.TabStop = true;
            // 
            // fieldView8
            // 
            this.fieldView8.AttachToFieldName = "ATTITUDE_CLASSIFICATION";
            this.fieldView8.ControlType = NFX.WinForms.Views.ControlType.Radio;
            this.fieldView8.ElementVSpacing = 8;
            this.fieldView8.LineCount = 2;
            this.fieldView8.Location = new System.Drawing.Point(22, 488);
            this.fieldView8.MarkerColor = System.Drawing.Color.Yellow;
            this.fieldView8.Name = "fieldView8";
            this.fieldView8.Size = new System.Drawing.Size(394, 73);
            this.fieldView8.TabIndex = 17;
            this.fieldView8.Zoom = 1F;
            // 
            // fieldView7
            // 
            this.fieldView7.AttachToFieldName = "MONTHLY_COST";
            this.fieldView7.Location = new System.Drawing.Point(22, 398);
            this.fieldView7.MarkerColor = System.Drawing.Color.Yellow;
            this.fieldView7.Name = "fieldView7";
            this.fieldView7.Size = new System.Drawing.Size(394, 40);
            this.fieldView7.TabIndex = 16;
            this.fieldView7.Zoom = 1F;
            // 
            // fieldView6
            // 
            this.fieldView6.AttachToFieldName = "ADM_DATE";
            this.fieldView6.Location = new System.Drawing.Point(22, 355);
            this.fieldView6.MarkerColor = System.Drawing.Color.Fuchsia;
            this.fieldView6.Name = "fieldView6";
            this.fieldView6.Size = new System.Drawing.Size(394, 40);
            this.fieldView6.TabIndex = 15;
            this.fieldView6.Zoom = 1F;
            // 
            // fieldView5
            // 
            this.fieldView5.AttachToFieldName = "DOB";
            this.fieldView5.Location = new System.Drawing.Point(22, 291);
            this.fieldView5.MarkerColor = System.Drawing.Color.Yellow;
            this.fieldView5.Name = "fieldView5";
            this.fieldView5.Size = new System.Drawing.Size(394, 40);
            this.fieldView5.TabIndex = 14;
            this.fieldView5.Zoom = 1F;
            // 
            // fieldView4
            // 
            this.fieldView4.AttachToFieldName = "PHONE_CONTACT";
            this.fieldView4.Location = new System.Drawing.Point(22, 228);
            this.fieldView4.MarkerColor = System.Drawing.Color.Yellow;
            this.fieldView4.Name = "fieldView4";
            this.fieldView4.Size = new System.Drawing.Size(394, 40);
            this.fieldView4.TabIndex = 13;
            this.fieldView4.Zoom = 1F;
            // 
            // fieldView3
            // 
            this.fieldView3.AttachToFieldName = "PATIENT_ID";
            this.fieldView3.Location = new System.Drawing.Point(22, 31);
            this.fieldView3.MarkerColor = System.Drawing.Color.Yellow;
            this.fieldView3.Name = "fieldView3";
            this.fieldView3.Size = new System.Drawing.Size(394, 40);
            this.fieldView3.TabIndex = 12;
            this.fieldView3.Zoom = 1F;
            // 
            // fieldView2
            // 
            this.fieldView2.AttachToFieldName = "LAST_NAME";
            this.fieldView2.Location = new System.Drawing.Point(22, 156);
            this.fieldView2.MarkerColor = System.Drawing.Color.Yellow;
            this.fieldView2.Name = "fieldView2";
            this.fieldView2.Size = new System.Drawing.Size(394, 40);
            this.fieldView2.TabIndex = 11;
            this.fieldView2.Zoom = 1F;
            // 
            // fieldView1
            // 
            this.fieldView1.AttachToFieldName = "FIRST_NAME";
            this.fieldView1.Location = new System.Drawing.Point(22, 92);
            this.fieldView1.MarkerColor = System.Drawing.Color.Yellow;
            this.fieldView1.Name = "fieldView1";
            this.fieldView1.Size = new System.Drawing.Size(394, 40);
            this.fieldView1.TabIndex = 10;
            this.fieldView1.Zoom = 1F;
            // 
            // fieldView9
            // 
            this.fieldView9.AttachToFieldName = "IS_RELIGIOUS";
            this.fieldView9.Location = new System.Drawing.Point(159, 444);
            this.fieldView9.MarkerColor = System.Drawing.Color.Yellow;
            this.fieldView9.Name = "fieldView9";
            this.fieldView9.Size = new System.Drawing.Size(123, 38);
            this.fieldView9.TabIndex = 18;
            this.fieldView9.Zoom = 1F;
            // 
            // MongoDBForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(617, 654);
            this.Controls.Add(this.btnDelete);
            this.Controls.Add(this.btnBatch);
            this.Controls.Add(this.btnSave);
            this.Controls.Add(this.btnRevert);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.btnPost);
            this.Controls.Add(this.btnEdit);
            this.Controls.Add(this.btnCreate);
            this.Controls.Add(this.btnLoad);
            this.Controls.Add(this.edtID);
            this.Controls.Add(this.pnl);
            this.Name = "MongoDBForm";
            this.Text = "MongoDBForm";
            ((System.ComponentModel.ISupportInitialize)(this.pnl)).EndInit();
            this.pnl.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.fieldView8)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.fieldView7)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.fieldView6)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.fieldView5)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.fieldView4)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.fieldView3)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.fieldView2)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.fieldView1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.fieldView9)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private NFX.WinForms.Views.RecordContextPanel pnl;
        private NFX.WinForms.Views.FieldView fieldView7;
        private NFX.WinForms.Views.FieldView fieldView6;
        private NFX.WinForms.Views.FieldView fieldView5;
        private NFX.WinForms.Views.FieldView fieldView4;
        private NFX.WinForms.Views.FieldView fieldView3;
        private NFX.WinForms.Views.FieldView fieldView2;
        private NFX.WinForms.Views.FieldView fieldView1;
        private System.Windows.Forms.TextBox edtID;
        private System.Windows.Forms.Button btnLoad;
        private System.Windows.Forms.Button btnCreate;
        private System.Windows.Forms.Button btnEdit;
        private System.Windows.Forms.Button btnPost;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.Button btnRevert;
        private System.Windows.Forms.Button btnSave;
        private System.Windows.Forms.Button btnBatch;
        private System.Windows.Forms.Button btnDelete;
        private NFX.WinForms.Views.FieldView fieldView8;
        private NFX.WinForms.Views.FieldView fieldView9;

    }
}