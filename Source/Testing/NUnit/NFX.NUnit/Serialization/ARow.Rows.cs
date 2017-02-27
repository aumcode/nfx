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
using System.IO;

using NUnit.Framework;

using NFX.IO;
using NFX.Serialization.Arow;
using NFX.DataAccess.CRUD;
using NFX.Financial;
using NFX.DataAccess.Distributed;

namespace NFX.NUnit.Serialization
{
  [Arow]
  public class SimplePersonRow : TypedRow
  {
    [Field(backendName: "id",   isArow: true)]public GDID ID{get; set;}
    [Field(backendName: "name", isArow: true)]public string Name{get; set;}
    [Field(backendName: "age",  isArow: true)]public int Age{ get;set;}
    [Field(backendName: "b1",   isArow: true)]public bool Bool1{ get;set;}
    [Field(backendName: "s1",   isArow: true)]public string Str1{ get;set;}
    [Field(backendName: "s2",   isArow: true)]public string Str2{ get;set;}
    [Field(backendName: "d1",   isArow: true)]public DateTime Date{ get;set;}
    [Field(backendName: "slr",  isArow: true)]public double Salary{ get;set;}
  }


  [Arow]
  public class FamilyRow : TypedRow
  {
    [Field(backendName: "id",   isArow: true)] public GDID ID{get; set;}
    [Field(backendName: "fam",  isArow: true)] public string Name{get; set;}
    [Field(backendName: "reg",  isArow: true)] public bool RegisteredToVote{ get;set;}

    [Field(backendName: "fat", isArow: true)]public SimplePersonRow Father{get; set;}
    [Field(backendName: "mot", isArow: true)]public SimplePersonRow Mother{get; set;}

    [Field(backendName: "bro", isArow: true)]public SimplePersonRow[] Brothers{get; set;}
    [Field(backendName: "sis", isArow: true)]public SimplePersonRow[] Sisters{get; set;}

    [Field(backendName: "adv", isArow: true)]public List<SimplePersonRow> Advisers{get; set;}
  }


  [Arow]
  public class AllArowTypesRow : TypedRow
  {
    [Field(backendName: "bool1",   isArow: true)]public bool Bool1{get; set;}
    [Field(backendName: "bool2",   isArow: true)]public bool? Bool2{get; set;}
    [Field(backendName: "bool3",   isArow: true)]public bool[] Bool3{get; set;}
    [Field(backendName: "bool4",   isArow: true)]public List<bool> Bool4{get; set;}


    [Field(backendName: "gdid1",   isArow: true)]public GDID GDID1{get; set;}
    [Field(backendName: "gdid2",   isArow: true)]public GDID? GDID2{get; set;}
    [Field(backendName: "gdid3",   isArow: true)]public GDID[] GDID3{get; set;}
    [Field(backendName: "gdid4",   isArow: true)]public List<GDID> GDID4{get; set;}

    [Field(backendName: "guid1",   isArow: true)]public Guid Guid1{get; set;}
    [Field(backendName: "guid2",   isArow: true)]public Guid? Guid2{get; set;}
    [Field(backendName: "guid3",   isArow: true)]public Guid[] Guid3{get; set;}
    [Field(backendName: "guid4",   isArow: true)]public List<Guid> Guid4{get; set;}

    [Field(backendName: "fid1",   isArow: true)]public FID Fid1{get; set;}
    [Field(backendName: "fid2",   isArow: true)]public FID? Fid2{get; set;}
    [Field(backendName: "fid3",   isArow: true)]public FID[] Fid3{get; set;}
    [Field(backendName: "fid4",   isArow: true)]public List<FID> Fid4{get; set;}


    [Field(backendName: "dt1",   isArow: true)]public DateTime Dt1{get; set;}
    [Field(backendName: "dt2",   isArow: true)]public DateTime? Dt2{get; set;}
    [Field(backendName: "dt3",   isArow: true)]public DateTime[] Dt3{get; set;}
    [Field(backendName: "dt4",   isArow: true)]public List<DateTime> Dt4{get; set;}

    [Field(backendName: "ts1",   isArow: true)]public TimeSpan Ts1{get; set;}
    [Field(backendName: "ts2",   isArow: true)]public TimeSpan? Ts2{get; set;}
    [Field(backendName: "ts3",   isArow: true)]public TimeSpan[] Ts3{get; set;}
    [Field(backendName: "ts4",   isArow: true)]public List<TimeSpan> Ts4{get; set;}

    [Field(backendName: "int1",   isArow: true)]public int int1{get; set;}
    [Field(backendName: "int2",   isArow: true)]public int? int2{get; set;}
    [Field(backendName: "int3",   isArow: true)]public int[] int3{get; set;}
    [Field(backendName: "int4",   isArow: true)]public List<int> int4{get; set;}

    [Field(backendName: "uint1",   isArow: true)]public uint uint1{get; set;}
    [Field(backendName: "uint2",   isArow: true)]public uint? uint2{get; set;}
    [Field(backendName: "uint3",   isArow: true)]public uint[] uint3{get; set;}
    [Field(backendName: "uint4",   isArow: true)]public List<uint> uint4{get; set;}
  }
}
