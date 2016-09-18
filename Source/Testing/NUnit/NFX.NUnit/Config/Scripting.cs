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
using System.Text;
using NUnit.Framework;

using NFX.Environment;
using NFX.CodeAnalysis.Laconfig;

namespace NFX.NUnit.Config
{
    [TestFixture]   
    public class Scripting
    {

    const string src1 = 
@"
root
{
   a=12
   b=true
   _loop='$(/$a) < 15'
   {
        _set{ path=/$a to=$(/$a)+1}
        sectionLoop { name=section_$(/$a) value='something'}
   }

   _call=/sub_Loop {}

   kerosine {}

   _call=/sub_Loop {}

   sub_Loop
   {
        script-only=true
        cnt=0{script-only=true}
         _set{ path=../cnt to=0}
         _loop='$(../cnt) < 5'
         {
                _set{ path=/sub_Loop/cnt to=$(/sub_Loop/cnt)+1}
                
                _if='$(/sub_Loop/cnt)==3'
                {
                    fromSubLoopFOR_THREE { name=section_$(/sub_Loop/cnt) value='3 gets special handling'}
                }
                _else
                {
                    fromSubLoop { name=section_$(/sub_Loop/cnt) value='something'}
                }
         }
        
   }

   benzin{}

   c = 45
}//root
";

 const string expected1=
@"root
{
  a=12
  b=true
  c=45
  sectionLoop
  {
    name=section_13
    value=something
  }
  sectionLoop
  {
    name=section_14
    value=something
  }
  sectionLoop
  {
    name=section_15
    value=something
  }
  fromSubLoop
  {
    name=section_1
    value=something
  }
  fromSubLoop
  {
    name=section_2
    value=something
  }
  fromSubLoopFOR_THREE
  {
    name=section_3
    value=""3 gets special handling""
  }
  fromSubLoop
  {
    name=section_4
    value=something
  }
  fromSubLoop
  {
    name=section_5
    value=something
  }
  kerosine
  {
  }
  fromSubLoop
  {
    name=section_1
    value=something
  }
  fromSubLoop
  {
    name=section_2
    value=something
  }
  fromSubLoopFOR_THREE
  {
    name=section_3
    value=""3 gets special handling""
  }
  fromSubLoop
  {
    name=section_4
    value=something
  }
  fromSubLoop
  {
    name=section_5
    value=something
  }
  benzin
  {
  }
}";



        [TestCase]
        public void VarsLoopIfElseCall()
        {
          var src = NFX.Environment.LaconicConfiguration.CreateFromString(src1);
          var result = new NFX.Environment.LaconicConfiguration();

          new ScriptRunner().Execute(src, result);
           
          var got =  result.SaveToString();
          Console.WriteLine( got );
          Assert.AreEqual(expected1, got);
        }

        [TestCase]
        public void ExprEval1_TernaryIf()
        {
          var src = NFX.Environment.LaconicConfiguration.CreateFromString(
@"
root
{
   a=12
   b=true
   
   var1=0{script-only=true}
   var2=175.4{script-only=true}
   var3=true{script-only=true}
   
   _block
   {
       _set{ path=/var1 to=(?$(/var2)>10;15;-10)+100 }
       RESULT=$(/var1){}
   }   
}
");
          var result = new NFX.Environment.LaconicConfiguration();

          new ScriptRunner().Execute(src, result);
           
          var got =  result.SaveToString();
          Console.WriteLine( got );

          Assert.AreEqual(115, result.Root["RESULT"].ValueAsInt());
        }


        [TestCase]
        public void ExprEval1_TernaryIfWithMixingTypes()
        {
          var src = NFX.Environment.LaconicConfiguration.CreateFromString(
@"
root
{
   a=12
   b=true
   
   var1=0{script-only=true}
   var2=175.4{script-only=true}
   var3=true{script-only=true}
   
   _block
   {
       _set{ path=/var1 to='((?$(/var3);$(/var2);-10)+100)+kozel' }
       RESULT=$(/var1){}
   }   
}
");
          var result = new NFX.Environment.LaconicConfiguration();

          new ScriptRunner().Execute(src, result);
           
          var got =  result.SaveToString();
          Console.WriteLine( got );

          Assert.AreEqual("275.4kozel", result.Root["RESULT"].Value);
        }



        [TestCase]
        [ExpectedException(typeof(ConfigException), ExpectedMessage="which does not exist", MatchType=MessageMatch.Contains)]
        public void Error_SetVarDoesNotExist()
        {
          var src = NFX.Environment.LaconicConfiguration.CreateFromString(
@"
root
{
       _set{ path=/NONE to=5+5 }
}
");
          var result = new NFX.Environment.LaconicConfiguration();

          new ScriptRunner().Execute(src, result);
        }

        [TestCase]
        [ExpectedException(typeof(ConfigException), ExpectedMessage="is not after IF", MatchType=MessageMatch.Contains)]
        public void Error_ELSEWithoutIF()
        {
          var src = NFX.Environment.LaconicConfiguration.CreateFromString(
@"
root
{
       _else{  }
}
");
          var result = new NFX.Environment.LaconicConfiguration();

          new ScriptRunner().Execute(src, result);
        }

        [TestCase]
        [ExpectedException(typeof(ConfigException), ExpectedMessage="is not after IF", MatchType=MessageMatch.Contains)]
        public void Error_ELSEWithoutIF2()
        {
          var src = NFX.Environment.LaconicConfiguration.CreateFromString(
@"
root
{
       _if=true{}
       in-the-middle{}
       _else{  }
}
");
          var result = new NFX.Environment.LaconicConfiguration();

          new ScriptRunner().Execute(src, result);
        }


        [TestCase]
        [ExpectedException(typeof(ConfigException), ExpectedMessage="exceeded allowed timeout", MatchType=MessageMatch.Contains)]
        public void Error_Timeout()
        {
          var src = NFX.Environment.LaconicConfiguration.CreateFromString(
@"
root
{
       _loop=true {}
}
");
          var result = new NFX.Environment.LaconicConfiguration();

          new ScriptRunner().Execute(src, result);
        }




        [TestCase]
        public void SectionNameWithVar()
        {
          var src = NFX.Environment.LaconicConfiguration.CreateFromString(
@"
root
{
       i=0
       _loop=$(/$i)<10
       {
           section_$(/$i) {}
           _set{path=/$i to=$(/$i)+1}
       }
}
");
          var result = new NFX.Environment.LaconicConfiguration();

          new ScriptRunner().Execute(src, result);

          var got =  result.SaveToString();
          Console.WriteLine( got );

          Assert.AreEqual("section_0", result.Root[0].Name);
          Assert.AreEqual("section_9", result.Root[9].Name);
          Assert.IsFalse( result.Root[10].Exists);
          
        }


const string rschema=@"
schema{
    PK_COLUMN=counter
    table=patient
    {
      column=$(/$PK_COLUMN) {type=TCounter required=true}
      column=resident_id {type=TMeaningfulID required=true}
      column=name {type=THumanName}
      _call=/AUDIT_COLUMNS {}
    }

    table=charge
    {
      column=$(/$PK_COLUMN) {type=TCounter required=true}
      column=transaction_date {type=TTimeStamp required=true}
      column=description {type=TDescription}
      column=amount {type=TMonetaryAmount}
      _call=/AUDIT_COLUMNS {}
    }


    AUDIT_COLUMNS
    {
        script-only=true
        column=change_user_id {type=TMeaningfulID required=true}
        column=change_date {type=TTimeStamp required=true}
    }
 
}";

const string rschemaExpected=
@"schema
{
  PK_COLUMN=counter
  table=patient
  {
    column=$(/$PK_COLUMN)
    {
      type=TCounter
      required=true
    }
    column=resident_id
    {
      type=TMeaningfulID
      required=true
    }
    column=name
    {
      type=THumanName
    }
    column=change_user_id
    {
      type=TMeaningfulID
      required=true
    }
    column=change_date
    {
      type=TTimeStamp
      required=true
    }
  }
  table=charge
  {
    column=$(/$PK_COLUMN)
    {
      type=TCounter
      required=true
    }
    column=transaction_date
    {
      type=TTimeStamp
      required=true
    }
    column=description
    {
      type=TDescription
    }
    column=amount
    {
      type=TMonetaryAmount
    }
    column=change_user_id
    {
      type=TMeaningfulID
      required=true
    }
    column=change_date
    {
      type=TTimeStamp
      required=true
    }
  }
}";


        [TestCase]
        public void RSchema()
        {
          var src = NFX.Environment.LaconicConfiguration.CreateFromString( rschema );
          var result = new NFX.Environment.LaconicConfiguration();

          new ScriptRunner().Execute(src, result);

          var got =  result.SaveToString();
          Console.WriteLine( got );
          Assert.AreEqual(rschemaExpected, got);
        }


        [TestCase]
        public void LoopWithRealArithmetic()
        {
          var src = NFX.Environment.LaconicConfiguration.CreateFromString(
@"
root
{
       i=0
       _loop='$(/$i)<=1.2'
       {
           section_$(/$i) {}
           _set{path=/$i to=$(/$i)+0.2}
       }
}
");
          var result = new NFX.Environment.LaconicConfiguration();

          new ScriptRunner().Execute(src, result);

          var got =  result.SaveToString();
          Console.WriteLine( got );

          Assert.AreEqual("section_0", result.Root[0].Name);
          Assert.AreEqual("section_0.2", result.Root[1].Name);
          Assert.AreEqual("section_0.4", result.Root[2].Name);
          Assert.AreEqual("section_0.6", result.Root[3].Name);
          Assert.AreEqual("section_0.8", result.Root[4].Name);
          Assert.AreEqual("section_1", result.Root[5].Name);
          Assert.AreEqual("section_1.2", result.Root[6].Name);
          Assert.IsFalse( result.Root[7].Exists);
          
        }


    }

}
