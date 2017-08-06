using System;

using NFX.DataAccess.CRUD;
using NFX.DataAccess.Distributed;

namespace NFX.Serialization.BSON
{
  public static class BSONExtensions
  {
    public static BSONDocument Add(this BSONDocument document, string name, object value, bool skipNull = false, bool required = false)
    {
      if (value == null) return onNullOrEmpty(document, name, skipNull, required);

      switch (Type.GetTypeCode(value.GetType()))
      {
        case TypeCode.Empty:
        case TypeCode.DBNull:   return document.Set(new BSONNullElement(name));
        case TypeCode.Boolean:  return document.Set(new BSONBooleanElement(name, (bool)value));
        case TypeCode.Char:     return document.Set(new BSONStringElement(name, value.ToString()));
        case TypeCode.SByte:    return document.Set(new BSONInt32Element(name, (sbyte)value));
        case TypeCode.Byte:     return document.Set(new BSONInt32Element(name, (byte)value));
        case TypeCode.Int16:    return document.Set(new BSONInt32Element(name, (short)value));
        case TypeCode.UInt16:   return document.Set(new BSONInt32Element(name, (ushort)value));
        case TypeCode.Int32:    return document.Set(new BSONInt32Element(name, (int)value));
        case TypeCode.UInt32:   return document.Set(new BSONInt32Element(name, (int)(uint)value));
        case TypeCode.Int64:    return document.Set(new BSONInt64Element(name, (long)value));
        case TypeCode.UInt64:   return document.Set(new BSONInt64Element(name, (long)(ulong)value));
        case TypeCode.Single:   return document.Set(new BSONDoubleElement(name, (float)value));
        case TypeCode.Double:   return document.Set(new BSONDoubleElement(name, (double)value));
        case TypeCode.Decimal:  return document.Set(RowConverter.Decimal_CLRtoBSON(name, (decimal)value));
        case TypeCode.DateTime: return document.Set(new BSONDateTimeElement(name, (DateTime)value));
        case TypeCode.String:   return document.Set(new BSONStringElement(name, (string)value));
        case TypeCode.Object:
          {
            if (value is Guid)
            {
              var guid = (Guid)value;
              if (guid == Guid.Empty) return onNullOrEmpty(document, name, skipNull, required);
              return document.Set(new BSONBinaryElement(name, new BSONBinary(BSONBinaryType.UUID, ((Guid)value).ToByteArray())));
            }
            else if (value is GDID)
            {
              var gdid = (GDID)value;
              if (gdid.IsZero) return onNullOrEmpty(document, name, skipNull, required);
              return document.Set(RowConverter.GDID_CLRtoBSON(name, gdid));
            }
            else if (value is TimeSpan)     return document.Set(new BSONInt64Element(name, ((TimeSpan)value).Ticks));
            else if (value is BSONDocument) return document.Set(new BSONDocumentElement(name, (BSONDocument)value));
            else if (value is byte[])       return document.Set(new BSONBinaryElement(name, new BSONBinary(BSONBinaryType.GenericBinary, (byte[])value)));
            throw new BSONException("BSONDocument.Add(not supported object type '{0}')".Args(value.GetType().Name));
          }
        default: throw new BSONException("BSONDocument.Add(not supported object type '{0}')".Args(value.GetType().Name));
      }
    }

    private static BSONDocument onNullOrEmpty(BSONDocument document, string name, bool skipNull, bool required)
    {
      if (required) throw new BSONException("BSONDocument.Add(required=true&&value=null)");
      if (!skipNull) return document.Set(new BSONNullElement(name));
      return document;
    }
  }
}
