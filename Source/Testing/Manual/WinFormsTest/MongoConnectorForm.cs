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
using NFX.Serialization.BSON;
using NFX.Serialization.JSON;
using NFX.DataAccess.MongoDB.Connector;


namespace WinFormsTest
{
  public partial class MongoConnectorForm : Form
  {
    public MongoConnectorForm()
    {
      InitializeComponent();
    }

    private void btnInsert_Click(object sender, EventArgs e)
    {
      const int CNT = 50000;

      var c = MongoClient.Instance.DefaultLocalServer["db1"]["t1"];

      var sw = new System.Diagnostics.Stopwatch();

      for(var i=0; i<CNT; i++)
      {
        if (i==1) sw.Start();
        c.Insert( new BSONDocument()
                       .Set( new BSONInt32Element("_id", i) )
                       .Set( new BSONStringElement("name", "Kozlov-"+i.ToString()) )
                       .Set( new BSONInt32Element("age", i) )
                );
      }

      var el = sw.ElapsedMilliseconds;

      Text = "Inserted {0} in {1} ms at {2}ops/sec".Args(CNT, el, CNT / (el / 1000d));
    }

    private void btnFetch_Click(object sender, EventArgs e)
    {
      var c = MongoClient.Instance.DefaultLocalServer["db1"]["t1"];

      var sw = new System.Diagnostics.Stopwatch();

      var cnt = 0;
      var query = new Query();
      BSONDocument lastDoc = null;
      using(var cursor = c.Find(query))
        foreach(var doc in cursor)
        {
          cnt++;
          if (cnt==1) sw.Start();
          lastDoc = doc;
        //  if (cnt>3) break;
        }

      var el = sw.ElapsedMilliseconds;

      Text = "Fetched {0} in {1} ms at {2}ops/sec".Args(cnt, el, cnt / (el / 1000d));
      MessageBox.Show(lastDoc.ToString());
    }


     private void btnQuery_Click(object sender, EventArgs e)
     {
       var qry = new Query("{'$and': [{lastName: {'$ge': '$$ln'}}, {firstName: {'$ge': '$$fn'}}], kaka: true}", true,
                               new TemplateArg("fn", BSONElementType.String, "Dima"),
                               new TemplateArg("ln", BSONElementType.String, "Kozlevich")
                          );

       MessageBox.Show( qry.ToString() );
     }

     private void btnUpdate_Click(object sender, EventArgs e)
     {
         const int CNT = 5000;

      var c = MongoClient.Instance.DefaultLocalServer["db1"]["t1"];

      var sw = new System.Diagnostics.Stopwatch();

      var total = 0;
      for(var i=0; i<CNT; i++)
      {
        if (i==1) sw.Start();
        var result = c.Update( new UpdateEntry(
                    new Query("{'_id': '$$ID'}", true, new TemplateArg("ID", BSONElementType.Int32, i)),
                    new Update("{'$inc': {'age': '$$INCREMENT'}}", true, new TemplateArg("INCREMENT", BSONElementType.Int32, 1)),
                    multi: false,
                    upsert: false
                  ));
        total += result.TotalDocumentsAffected;
      }

      var el = sw.ElapsedMilliseconds;

      Text = "Did {0}, Updated {1} in {2} ms at {3}ops/sec".Args(CNT, total, el, CNT / (el / 1000d));
     }

     private void btnSave_Click(object sender, EventArgs e)
     {
          var c = MongoClient.Instance.DefaultLocalServer["db1"]["t1"];

          var query =  new Query("{'_id': '$$ID'}", true, new TemplateArg("ID", BSONElementType.Int32, 7));

          var doc = c.FindOne(query);

          doc["age"].ObjectValue = 777;

          var result = c.Save(doc);

          Text = "Affected: {0} Updated: {1}".Args(result.TotalDocumentsAffected, result.TotalDocumentsUpdatedAffected);
     }

     private void btnOpenCursors_Click(object sender, EventArgs e)
     {
        var c = MongoClient.Instance.DefaultLocalServer["db1"]["t1"];

        var query = new Query();
        for(var i=0; i<10; i++)
          c.Find(query);
       
     }

     private void btnListCollections_Click(object sender, EventArgs e)
     {
       var colls = MongoClient.Instance.DefaultLocalServer["db1"].GetCollectionNames();

       MessageBox.Show( string.Join("\n\r", colls) );
     }

     private void btnFetchOrderBy_Click(object sender, EventArgs e)
     {
        var c = MongoClient.Instance.DefaultLocalServer["db1"]["t1"];

        var query =  new Query(
        @"{
           '$query': {'_id': {'$lte': '$$ID'}},
           '$orderby': {'age': '$$ORD'}
          }",
           true, 
         
         new TemplateArg("ID", BSONElementType.Int32, 197),
         new TemplateArg("ORD", BSONElementType.Int32, -1)
        );
        
        var data = c.FindAndFetchAll(query, cursorFetchLimit: 4);
        MessageBox.Show( "Got {0} \n=======================\n {1}".Args(data.Count, data.ToJSON(JSONWritingOptions.PrettyPrint)) );
     }

     private void btnCreateIndex_Click(object sender, EventArgs e)
     {
        var db = MongoClient.Instance.DefaultLocalServer["db1"];


        var idx = new BSONDocument(@"
          {
           createIndexes: 't1',
            indexes: [
              {
                key: {name: 1},
                name: 'idxT1_Name',
                unique: false
              },
              {
                key: {age: -11},
                name: 'idxT1_Age',
                unique: false
              }
            ]
          }


        ",false);

        MessageBox.Show( db.RunCommand( idx ).ToString() );

     }

     private void btnListIndexes_Click(object sender, EventArgs e)
     {
       var db = MongoClient.Instance.DefaultLocalServer["db1"];


        var idx = new BSONDocument("{listIndexes: 't1'}",false);

        MessageBox.Show( db.RunCommand( idx ).ToString() );
     }

  }
}
