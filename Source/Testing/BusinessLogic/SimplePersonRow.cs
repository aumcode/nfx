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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using NFX.DataAccess.CRUD;
using NFX.DataAccess.Distributed;


namespace BusinessLogic
{
  public class SimplePersonRow : TypedRow
  {
    [Field]public GDID ID{get; set;}   
    [Field]public string Name{get; set;}   
    [Field]public int Age{ get;set;}
    [Field]public bool Bool1{ get;set;}
    [Field]public string Str1{ get;set;}
    [Field]public string Str2{ get;set;}
    [Field]public DateTime Date{ get;set;}
    [Field]public double Salary{ get;set;}
  }
}
