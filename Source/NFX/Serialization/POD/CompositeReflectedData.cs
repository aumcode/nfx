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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NFX.Serialization.POD
{
    /// <summary>
    /// Represents a composite (non primitive) data stored in Portable Object Document.
    /// This data is obtained from native types automatically using reflection.
    /// This class is NOT used for native types that perform custom serialization using ISerializable interface,
    ///  for that CompositeCustomData is used
    /// </summary>
    [Serializable]
    public sealed class CompositeReflectedData : CompositeData
    {
        #region .ctor

            internal CompositeReflectedData(PortableObjectDocument document, object data, int metaTypeIndex = -1)
                      : base(document, data, metaTypeIndex)
            {
                if (!ExistingReference)
                    serializeFields(data);
            }

        #endregion

        #region Fields
            public object[] m_FieldData;
        #endregion


        #region Properties
            /// <summary>
            /// Returns field data that this instance contains, or null if this instance is a reference to another object
            /// </summary>
            public object[] FieldData { get { return m_FieldData;} }


        #endregion


        #region .pvt

                private void serializeFields(object compositeData)
                {
                    var mtp = Type;

                    var result = new object[mtp.FieldCount];
                    var i=0;
                    foreach(var fld in mtp)
                    {
                        var fdata = fld.m_FieldInfo.GetValue(compositeData);

                        result[i] = m_Document.NativeDataToPortableData( fdata );
                        i++;
                    }

                    m_FieldData = result;
                }

        #endregion
    }
}
