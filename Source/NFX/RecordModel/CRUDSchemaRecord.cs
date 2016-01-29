using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


using NFX.DataAccess;
using NFX.DataAccess.CRUD;

namespace NFX.RecordModel
{
  /// <summary>
  /// Implements a record model that auto-generates field models from CRUD.Schema
  /// </summary>
  public sealed class CRUDSchemaRecord : Record
  {
    /// <summary>
    /// Create a record of specified schema. If row is null then puts record in CREATE mode
    /// </summary>
    public CRUDSchemaRecord(Schema schema, Row row, string targetName = null)
    {
      if (schema==null)
        throw new RecordModelException(StringConsts.ARGUMENT_ERROR+GetType().Name+".ctor(schema==null)");

      if (row!=null && !row.Schema.IsEquivalentTo(schema))
        throw new RecordModelException(StringConsts.ARGUMENT_ERROR+GetType().Name+".ctor(row.schema!=schema)");
      
      m_Schema = schema;
      m_Row = row;
      m_TargetName = targetName ?? TargetedAttribute.ANY_TARGET;

      base.ctor_MakeRecordInstanceFromScratch();
      
      if (row==null)
      {
       this.Create();
      }
      else
      {
        loadRowData();
        this.Edit(); 
      }  
    }
    
    private Schema m_Schema;
    private Row   m_Row;
    private string m_TargetName;
    
   
    public Schema Schema { get { return m_Schema;} }
    
    public Row Row{ get{ return m_Row;}}


    public Row ToRow()
    {
      var result = new DynamicRow(m_Schema);
      foreach(var field in this.Fields)
       result[field.FieldName] = field.ValueAsObject;

      return result;
    }

    /// <summary>
    /// Returns target name used for schema attr binding
    /// </summary>
    public string TargetName{ get{ return m_TargetName; }}
    
        
    protected override void ConstructFields()
    {
       foreach(var fdef in m_Schema)
       {
         var type = fdef.Type;
         Field fld = Field.MakeFieldOfType(type);

         var atr = fdef[m_TargetName];

         fld.FieldName = fdef.Name;
         fld.Description = fdef.Description;
         fld.Visible  = atr.Visible;
         fld.Required = atr.Required;
         fld.Readonly = false;
         fld.KeyField = atr.Key;
         fld.Note = fdef.Description;
         var fw = atr.MaxLength > 0 ? atr.MaxLength : 12;
         
         var watr = atr.Metadata.Navigate("$width|$display-width|$displayWidth|$display_width");
         
         fld.DisplayWidth = watr.ValueAsInt( fw );

         fld.DisplayFormat = atr.DisplayFormat;
         
         {
           var sfld = fld as StringField;
           if (sfld!=null)
           {
             var min = (int)atr.MinLength;
             
             if (min>0)
              sfld.Validation += 
                       (sender,args) =>
                       {
                         if (sfld.Value!=null&&sfld.Value.Length < min)
                          throw new ModelValidationException("Value must be at least that long: " + min);
                       }; 
             
             sfld.Size = (int)atr.MaxLength;
              
             sfld.CharCase = atr.CharCase;

             sfld.FormatRegExp = atr.FormatRegExp;
             sfld.FormatRegExpDescription = atr.FormatDescription;
           }
         }
         
         {
           var lfld = fld as LongField;
           if (lfld!=null)
           {
             lfld.MinMaxChecking = atr.Min!=null || atr.Max!=null;
             if (atr.Min!=null) lfld.MinValue = (long)atr.Min.ToString().AsType(typeof(long));
             if (atr.Max!=null) lfld.MaxValue = (long)atr.Max.ToString().AsType(typeof(long));
           }   
         }
         
         {
           var ifld = fld as IntField;
           if (ifld!=null)
           {
             ifld.MinMaxChecking = atr.Min!=null || atr.Max!=null;
             if (atr.Min!=null) ifld.MinValue = (int)atr.Min.ToString().AsType(typeof(int));
             if (atr.Max!=null) ifld.MaxValue = (int)atr.Max.ToString().AsType(typeof(int));
           }   
         }
         
         {
           var dfld = fld as DoubleField;
           if (dfld!=null)
           {
             dfld.MinMaxChecking = atr.Min!=null || atr.Max!=null;
             if (atr.Min!=null) dfld.MinValue = (double)atr.Min.ToString().AsType(typeof(double));
             if (atr.Max!=null) dfld.MaxValue = (double)atr.Max.ToString().AsType(typeof(double));
           }   
         }
         
         
         
         
         if (!string.IsNullOrEmpty(atr.ValueList))
         {
           foreach(var item in atr.ParseValueList())
           fld.LookupDictionary.Add("{0},{1}".Args(item.Key, item.Value));
           fld.DataEntryType = DataEntryType.DirectEntryOrLookupWithValidation;
         }
         

         var sdv = atr.Default!=null ? atr.Default.ToString() : (string)null;

         if (sdv!=null)
         {
           if (fld is StringField)
           {
             ((StringField)fld).DefaultValue = sdv;
             fld.HasDefaultValue = true;
           }

           if (fld is IntField)
           { 
             int val = 0;
             if (fld.HasDefaultValue = Int32.TryParse(sdv, out val))
              ((IntField)fld).DefaultValue = val;
           }
         
           if (fld is DoubleField)
           { 
             double val = 0d;
             if (fld.HasDefaultValue = Double.TryParse(sdv, out val))
              ((DoubleField)fld).DefaultValue = val;
           }
         
           if (fld is LongField)
           {
             long val = 0;
             if (fld.HasDefaultValue = Int64.TryParse(sdv, out val))
              ((LongField)fld).DefaultValue = val;
           }
         
           if (fld is BoolField) 
           {
             bool val = false;
             if (fld.HasDefaultValue = Boolean.TryParse(sdv, out val))
              ((BoolField)fld).DefaultValue = val;
           }

           
         }
         fld.Owner = this;//registers field with record
       }
      
    }

   
    
    
    private void loadRowData()
    {
      BeginInit();
      try
      {
        for(var i=0; i<m_Row.Schema.FieldCount; i++)
        {
          var fdata = m_Row[i];
          var fld = this.Fields.ElementAt(i);
          
          fld.ValueAsObject = fdata;

          if (fdata!=null && fdata is ValueType)
             fld._setValueTypeHasValue();
        } 
      }
      finally
      {
        try
        {
          EndInit();
        }
        catch(Exception error)
        {
          throw new ModelValidationException("Error loadRowData(): {0}\n{1}".Args(error.ToMessageWithType(), this.AllValidationExceptionMessages), error);
        }
      }
    }
    
    
  }
}
