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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;
using NFX.IO;
using NFX.Serialization.POD;

namespace NFX.NUnit.Integration.Serialization
{
   #pragma warning disable 0649,0169,0659
    
    
    public struct DataStruct
    {
      public int fInt;
      public int? fNullableInt;
      public string fString;
      public object fObject;
      public object[] fObjectArray1D;
      public object[,] fObjectArray2D;
    }

    public class DataNode
    {
      public string fValue;
      public DataNode fParent;
      public DataNode[] fChildren;

      public DataNode fLeft;
      public DataNode fRight;


      public override bool Equals(object obj)
      {
          var other = obj as DataNode;
          if (other==null) return false;
          
          if (fValue!=other.fValue) return false;

          if (fChildren!=null && other.fChildren!=null)
           return fChildren.SequenceEqual(other.fChildren);
           
          return true;
      }
    }


    [DataContract]
    public class DataObject : IEquatable<DataObject>
    {
       [DataMember]public string    fString;

       [DataMember]public int       fInt;
       [DataMember]public uint      fUInt;
       [DataMember]public int?      fIntNullable;
       [DataMember]public uint?     fUIntNullable;

       [DataMember]public long       fLong;
       [DataMember]public ulong      fULong;
       [DataMember]public long?      fLongNullable;
       [DataMember]public ulong?     fULongNUllable;

       [DataMember]public short       fShort;
       [DataMember]public ushort      fUShort;
       [DataMember]public short?      fShortNullable;
       [DataMember]public ushort?     fUShortNullable;

       [DataMember]public byte      fByte;
       [DataMember]public sbyte     fSByte;
       [DataMember]public byte?     fByteNullable;
       [DataMember]public sbyte?    fSByteNullable;

       [DataMember]public float     fFloat;
       [DataMember]public double    fDouble;
       [DataMember]public decimal   fDecimal;
       [DataMember]public float?     fFloatNullable;
       [DataMember]public double?    fDoubleNullable;
       [DataMember]public decimal?   fDecimalNullable;

       [DataMember]public TimeSpan  fTimeSpan;
       [DataMember]public DateTime  fDateTime;
       [DataMember]public TimeSpan?  fTimeSpanNullable;
       [DataMember]public DateTime?  fDateTimeNullable;

       [DataMember]public bool      fBool;
       [DataMember]public char      fChar;
       [DataMember]public bool?     fBoolNullable;
       [DataMember]public char?     fCharNullable;

       [DataMember]public byte[]    fByteArray;
       [DataMember]public char[]    fCharArray;
       [DataMember]public string[]  fStringArray;

       [DataMember]public MetaHandle  fMetaHandle;


       public override bool Equals(object obj)
       {
           return this.Equals(obj as DataObject);
       }

       public virtual bool Equals(DataObject other)
       {
        if (other==null) return false;
         return 
           ( this.   fString              == other.   fString                  ) &&
           ( this.   fInt                 == other.   fInt                     ) &&
           ( this.   fUInt                == other.   fUInt                    ) &&
           ( this.   fIntNullable         == other.   fIntNullable             ) &&
           ( this.   fUIntNullable        == other.   fUIntNullable            ) &&
                                          
           ( this.   fLong                == other.   fLong                    ) &&
           ( this.   fULong               == other.   fULong                   ) &&
           ( this.   fLongNullable        == other.   fLongNullable            ) &&
           ( this.   fULongNUllable       == other.   fULongNUllable           ) &&
                                          
           ( this.   fShort               == other.   fShort                   ) &&
           ( this.   fUShort              == other.   fUShort                  ) &&
           ( this.   fShortNullable       == other.   fShortNullable           ) &&
           ( this.   fUShortNullable      == other.   fUShortNullable          ) &&

           ( this.   fByte                == other.   fByte                    ) &&
           ( this.   fSByte               == other.   fSByte                   ) &&
           ( this.   fByteNullable        == other.   fByteNullable            ) &&
           ( this.   fSByteNullable       == other.   fSByteNullable           ) &&
                                          
           ( this.   fFloat               == other.   fFloat                   ) &&
           ( this.   fDouble              == other.   fDouble                  ) &&
           ( this.   fDecimal             == other.   fDecimal                 ) &&
           ( this.   fFloatNullable       == other.   fFloatNullable           ) &&
           ( this.   fDoubleNullable      == other.   fDoubleNullable          ) &&
           ( this.   fDecimalNullable     == other.   fDecimalNullable         ) &&
                                          
           ( this.   fTimeSpan            == other.   fTimeSpan                ) &&
           ( this.   fDateTime            == other.   fDateTime                ) &&
           ( this.   fTimeSpanNullable    == other.   fTimeSpanNullable        ) &&
           ( this.   fDateTimeNullable    == other.   fDateTimeNullable        ) &&
                                          
           ( this.   fBool                == other.   fBool                    ) &&
           ( this.   fChar                == other.   fChar                    ) &&
           ( this.   fBoolNullable        == other.   fBoolNullable            ) &&
           ( this.   fCharNullable        == other.   fCharNullable            ) &&
                                          
           ( this.   fByteArray.SequenceEqual(other.   fByteArray)             ) &&
           ( this.   fCharArray.SequenceEqual(other.   fCharArray)             ) &&
           ( this.   fStringArray.SequenceEqual(other. fStringArray)           ) &&
                                          
           ( this.   fMetaHandle.Equals(other.   fMetaHandle) );



       }
    }


    public class DataObjectWithList : DataObject
    {
      public List<DataObject> OtherObjects = new List<DataObject>();

       public override bool Equals(DataObject other)
       {
         var ao = other as DataObjectWithList;
         if (ao==null) return false;
         if (!base.Equals(ao)) return false;

         return this.OtherObjects.SequenceEqual(ao.OtherObjects);
       }
    }



                internal class ClassWithAutoPropFields
                {
                    public string Name{ get; set;}
                    public int Age{ get; set;}
                }

                [DataContract]
                public class APerson
                {
                  [DataMember]
                  public string FirstName { get; set;}
                  [DataMember]
                  public string LastName { get; set;}
                  [DataMember]
                  public int Age { get; set;}
                  [DataMember]
                  public DateTime DOB { get; set;}
                } 

                internal enum PersonRespect {Normal, Low, High, Guru}

                internal class TestPerson
                {
                    public string Name;
                    public DateTime DOB;
                    public decimal Assets;
                    public double Luck;
                    public bool IsRegistered;
                    public PersonRespect Respect;

                    public override bool Equals(object obj)
                    {
                        var other = obj as TestPerson;
                        if (other==null) return false;
                        return this.Name == other.Name &&
                        this.DOB == other.DOB &&
                        this.Assets == other.Assets &&
                        this.Luck == other.Luck &&
                        this.IsRegistered == other.IsRegistered &&
                        this.Respect == other.Respect;
                    }
                }

                internal class TestFamily
                {
                    public TestPerson Husband;
                    public TestPerson Wife;
                    public TestPerson Kid;
                    
                   
                    public override bool Equals(object obj)
                    {
                        var other = obj as TestFamily;
                        if (other==null) return false;
                        return this.Husband.Equals(other.Husband ) &&
                               this.Wife.Equals(other.Wife) &&
                               this.Kid.Equals(other.Kid);

                    }
                }

                internal class TestBusinessFamily : TestFamily
                {
                    public decimal Assets;
                    public bool IsCertified;

                    public override bool Equals(object obj)
                    {
                        var other = obj as TestBusinessFamily;
                        if (other==null) return false;
                        return base.Equals(obj) && this.Assets==other.Assets && this.IsCertified==other.IsCertified ;
                    }
                }




}
