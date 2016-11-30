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
namespace WinFormsTest
{
  partial class MongoConnectorForm
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
      System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MongoConnectorForm));
      this.btnInsert = new System.Windows.Forms.Button();
      this.btnFetch = new System.Windows.Forms.Button();
      this.btnQuery = new System.Windows.Forms.Button();
      this.btnUpdate = new System.Windows.Forms.Button();
      this.btnSave = new System.Windows.Forms.Button();
      this.btnOpenCursors = new System.Windows.Forms.Button();
      this.btnListCollections = new System.Windows.Forms.Button();
      this.btnFetchOrderBy = new System.Windows.Forms.Button();
      this.btnCreateIndex = new System.Windows.Forms.Button();
      this.btnListIndexes = new System.Windows.Forms.Button();
      this.btnAgregate = new System.Windows.Forms.Button();
      this.btnInserComplexObjects = new System.Windows.Forms.Button();
      this.btnAggComplex = new System.Windows.Forms.Button();
      this.tbQuery = new System.Windows.Forms.TextBox();
      this.SuspendLayout();
      // 
      // btnInsert
      // 
      this.btnInsert.Location = new System.Drawing.Point(25, 30);
      this.btnInsert.Name = "btnInsert";
      this.btnInsert.Size = new System.Drawing.Size(75, 23);
      this.btnInsert.TabIndex = 0;
      this.btnInsert.Text = "Insert+Fetch";
      this.btnInsert.UseVisualStyleBackColor = true;
      this.btnInsert.Click += new System.EventHandler(this.btnInsert_Click);
      // 
      // btnFetch
      // 
      this.btnFetch.Location = new System.Drawing.Point(120, 30);
      this.btnFetch.Name = "btnFetch";
      this.btnFetch.Size = new System.Drawing.Size(75, 23);
      this.btnFetch.TabIndex = 1;
      this.btnFetch.Text = "Fetch";
      this.btnFetch.UseVisualStyleBackColor = true;
      this.btnFetch.Click += new System.EventHandler(this.btnFetch_Click);
      // 
      // btnQuery
      // 
      this.btnQuery.Location = new System.Drawing.Point(365, 181);
      this.btnQuery.Name = "btnQuery";
      this.btnQuery.Size = new System.Drawing.Size(75, 23);
      this.btnQuery.TabIndex = 2;
      this.btnQuery.Text = "Query";
      this.btnQuery.UseVisualStyleBackColor = true;
      this.btnQuery.Click += new System.EventHandler(this.btnQuery_Click);
      // 
      // btnUpdate
      // 
      this.btnUpdate.Location = new System.Drawing.Point(25, 70);
      this.btnUpdate.Name = "btnUpdate";
      this.btnUpdate.Size = new System.Drawing.Size(75, 23);
      this.btnUpdate.TabIndex = 3;
      this.btnUpdate.Text = "Update";
      this.btnUpdate.UseVisualStyleBackColor = true;
      this.btnUpdate.Click += new System.EventHandler(this.btnUpdate_Click);
      // 
      // btnSave
      // 
      this.btnSave.Location = new System.Drawing.Point(40, 135);
      this.btnSave.Name = "btnSave";
      this.btnSave.Size = new System.Drawing.Size(75, 23);
      this.btnSave.TabIndex = 4;
      this.btnSave.Text = "Save";
      this.btnSave.UseVisualStyleBackColor = true;
      this.btnSave.Click += new System.EventHandler(this.btnSave_Click);
      // 
      // btnOpenCursors
      // 
      this.btnOpenCursors.Location = new System.Drawing.Point(289, 79);
      this.btnOpenCursors.Name = "btnOpenCursors";
      this.btnOpenCursors.Size = new System.Drawing.Size(75, 66);
      this.btnOpenCursors.TabIndex = 5;
      this.btnOpenCursors.Text = "Open Cursros";
      this.btnOpenCursors.UseVisualStyleBackColor = true;
      this.btnOpenCursors.Click += new System.EventHandler(this.btnOpenCursors_Click);
      // 
      // btnListCollections
      // 
      this.btnListCollections.Location = new System.Drawing.Point(412, 79);
      this.btnListCollections.Name = "btnListCollections";
      this.btnListCollections.Size = new System.Drawing.Size(75, 66);
      this.btnListCollections.TabIndex = 6;
      this.btnListCollections.Text = "List Collections";
      this.btnListCollections.UseVisualStyleBackColor = true;
      this.btnListCollections.Click += new System.EventHandler(this.btnListCollections_Click);
      // 
      // btnFetchOrderBy
      // 
      this.btnFetchOrderBy.Location = new System.Drawing.Point(239, 30);
      this.btnFetchOrderBy.Name = "btnFetchOrderBy";
      this.btnFetchOrderBy.Size = new System.Drawing.Size(75, 23);
      this.btnFetchOrderBy.TabIndex = 7;
      this.btnFetchOrderBy.Text = "Fetch Order By";
      this.btnFetchOrderBy.UseVisualStyleBackColor = true;
      this.btnFetchOrderBy.Click += new System.EventHandler(this.btnFetchOrderBy_Click);
      // 
      // btnCreateIndex
      // 
      this.btnCreateIndex.Location = new System.Drawing.Point(365, 30);
      this.btnCreateIndex.Name = "btnCreateIndex";
      this.btnCreateIndex.Size = new System.Drawing.Size(75, 23);
      this.btnCreateIndex.TabIndex = 8;
      this.btnCreateIndex.Text = "Create Index";
      this.btnCreateIndex.UseVisualStyleBackColor = true;
      this.btnCreateIndex.Click += new System.EventHandler(this.btnCreateIndex_Click);
      // 
      // btnListIndexes
      // 
      this.btnListIndexes.Location = new System.Drawing.Point(458, 30);
      this.btnListIndexes.Name = "btnListIndexes";
      this.btnListIndexes.Size = new System.Drawing.Size(75, 23);
      this.btnListIndexes.TabIndex = 9;
      this.btnListIndexes.Text = "List Indexes";
      this.btnListIndexes.UseVisualStyleBackColor = true;
      this.btnListIndexes.Click += new System.EventHandler(this.btnListIndexes_Click);
      // 
      // btnAgregate
      // 
      this.btnAgregate.Location = new System.Drawing.Point(129, 70);
      this.btnAgregate.Name = "btnAgregate";
      this.btnAgregate.Size = new System.Drawing.Size(75, 23);
      this.btnAgregate.TabIndex = 10;
      this.btnAgregate.Text = "Agregate";
      this.btnAgregate.UseVisualStyleBackColor = true;
      this.btnAgregate.Click += new System.EventHandler(this.btnAgregate_Click);
      // 
      // btnInserComplexObjects
      // 
      this.btnInserComplexObjects.Location = new System.Drawing.Point(25, 294);
      this.btnInserComplexObjects.Name = "btnInserComplexObjects";
      this.btnInserComplexObjects.Size = new System.Drawing.Size(139, 23);
      this.btnInserComplexObjects.TabIndex = 11;
      this.btnInserComplexObjects.Text = "Insert Complex Object";
      this.btnInserComplexObjects.UseVisualStyleBackColor = true;
      this.btnInserComplexObjects.Click += new System.EventHandler(this.btnInserComplexObjects_Click);
      // 
      // btnAggComplex
      // 
      this.btnAggComplex.Location = new System.Drawing.Point(25, 346);
      this.btnAggComplex.Name = "btnAggComplex";
      this.btnAggComplex.Size = new System.Drawing.Size(139, 23);
      this.btnAggComplex.TabIndex = 12;
      this.btnAggComplex.Text = "Complex Aggregation";
      this.btnAggComplex.UseVisualStyleBackColor = true;
      this.btnAggComplex.Click += new System.EventHandler(this.btnAggComplex_Click);
      // 
      // tbQuery
      // 
      this.tbQuery.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
      this.tbQuery.Location = new System.Drawing.Point(170, 221);
      this.tbQuery.Multiline = true;
      this.tbQuery.Name = "tbQuery";
      this.tbQuery.Size = new System.Drawing.Size(420, 306);
      this.tbQuery.TabIndex = 13;
      this.tbQuery.Text = resources.GetString("tbQuery.Text");
      // 
      // MongoConnectorForm
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.ClientSize = new System.Drawing.Size(796, 548);
      this.Controls.Add(this.tbQuery);
      this.Controls.Add(this.btnAggComplex);
      this.Controls.Add(this.btnInserComplexObjects);
      this.Controls.Add(this.btnAgregate);
      this.Controls.Add(this.btnListIndexes);
      this.Controls.Add(this.btnCreateIndex);
      this.Controls.Add(this.btnFetchOrderBy);
      this.Controls.Add(this.btnListCollections);
      this.Controls.Add(this.btnOpenCursors);
      this.Controls.Add(this.btnSave);
      this.Controls.Add(this.btnUpdate);
      this.Controls.Add(this.btnQuery);
      this.Controls.Add(this.btnFetch);
      this.Controls.Add(this.btnInsert);
      this.Name = "MongoConnectorForm";
      this.Text = "Mongo Connector";
      this.ResumeLayout(false);
      this.PerformLayout();

    }

    #endregion

    private System.Windows.Forms.Button btnInsert;
    private System.Windows.Forms.Button btnFetch;
    private System.Windows.Forms.Button btnQuery;
    private System.Windows.Forms.Button btnUpdate;
    private System.Windows.Forms.Button btnSave;
    private System.Windows.Forms.Button btnOpenCursors;
    private System.Windows.Forms.Button btnListCollections;
    private System.Windows.Forms.Button btnFetchOrderBy;
    private System.Windows.Forms.Button btnCreateIndex;
    private System.Windows.Forms.Button btnListIndexes;
    private System.Windows.Forms.Button btnAgregate;
    private System.Windows.Forms.Button btnInserComplexObjects;
    private System.Windows.Forms.Button btnAggComplex;
    private System.Windows.Forms.TextBox tbQuery;
  }
}