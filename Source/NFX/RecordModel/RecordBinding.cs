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


/* NFX by ITAdapter
 * Originated: 2008.03
 * Revision: NFX 1.0  2011.02.06
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NFX.RecordModel
{

  /// <summary>
  /// Provides an abstract base for record view bindings. Concrete binding implementations are defined by 
  /// actual presentation classes that may be (but certainly not limited to) derivatives of standard 
  /// controls such as WinForms.PropertyGrid, ASPNet.Grid etc. 
  /// A binding serves two purposes:
  ///  links a control with a record instance (hence the name)
  ///  and allows control to react to changes in a record (model)
  /// </summary>
  public abstract class RecordBinding : BindingBase, IRecordContext
  {
    #region .ctor
   
      /// <summary>
      /// Creates an instance of binding, setting an owner reference to an instance of a concrete GUI class
      /// that is allocating this binding and what this binding provides services for
      /// </summary>
      public RecordBinding(object owner) : base(owner)
      {
      }

    #endregion


    #region Public


      /// <summary>
      /// Sets value from view binding. This method is used to commit data from view controls into fields
      /// </summary>
      public override void SetValueFromGUI(object value)
      {
        //todo: maybe set data for whole record 
      }
      
      /// <summary>
      /// Attaches binding to a particular record
      /// </summary>
      public void Attach(Record record)
      {
          if (record == null)
           throw new RecordModelException(string.Format(StringConsts.INVALID_ARGUMENT_ERROR, "Attach(record=null)"));
          
          if (Attached) Detach();
          m_Record = record;
          m_Record.RegisterBinding(this);
          m_Attached = true;
          
          BindingStatusChanged();
      }

    #endregion


    #region Protected
    #endregion
  }
  
  
  
  public abstract class RecordBinding<TOwnerType> : RecordBinding
  where TOwnerType : class
  {
   public RecordBinding(TOwnerType owner) : base(owner) 
   {
   }
  
   public new TOwnerType Owner
   {
    get{ return base.Owner as TOwnerType; }
   }
  
  }


  /// <summary>
  /// Simple record binding used by custom designers
  /// </summary>
  public class DesignerRecordBinding : RecordBinding
  {
    #region .ctor

    public DesignerRecordBinding(object owner)
      : base(owner)
    {
    }

    #endregion


    #region Public

    
    #endregion


    #region Protected
    protected override bool DesignTime
    {
      get { return true; }
    }

    #endregion
  }
  
  
}
