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
