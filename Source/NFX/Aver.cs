using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using NFX.DataAccess.Distributed;

namespace NFX
{
  /// <summary>
  /// Provides basic averments for test construction. May call Aver.Fail(msg) manually
  /// </summary>
  public static class Aver
  {
    /// <summary>
    /// Fails averment by throwing AvermentException
    /// </summary>
    public static void Fail(string message, string from = null)
    {
      throw new AvermentException(message, from);
    }

    #region AreEqual/AreNotEqual

      /// <summary>
      /// Test for equality via object.Equals()only disregarding all other possible equality comparers like IEquatable etc...
      /// </summary>
      public static bool AreObjectEqualTest(object expect, object got)
      {
        if (expect==null && got==null) return true;
        if (expect==null) return false;

        return expect.Equals(got);
      }

      /// <summary>
      /// Test for equality via object.Equals()only disregarding all other possible equality comparers like IEquatable etc...
      /// </summary>
      public static void AreObjectsEqual(object expect, object got, string from = null)
      {
        if (!AreObjectEqualTest(expect, got)) Fail("AreObjectsEqual({0}, {1})".args(expect, got), from);
      }

      /// <summary>
      /// Test for inequality via object.Equals()only disregarding all other possible equality comparers like IEquatable etc...
      /// </summary>
      public static void AreObjectsNotEqual(object expect, object got, string from = null)
      {
        if (AreObjectEqualTest(expect, got)) Fail("AreObjectsNotEqual({0}, {1})".args(expect, got), from);
      }


      public static void AreEqual(string expect, string got, StringComparison comparison = StringComparison.InvariantCulture, string from = null)
      {
        if (!string.Equals(expect, got, comparison)) Fail("AreEqual({0}, {1}, {2})".args(expect, got, comparison), from);
      }

      public static void AreNotEqual(string expect, string got, StringComparison comparison = StringComparison.InvariantCulture, string from = null)
      {
        if (string.Equals(expect, got, comparison)) Fail("AreNotEqual({0}, {1}, {2})".args(expect, got, comparison), from);
      }


      public static void AreEqual   <T>(T expect, T got, string from = null) where T : IEquatable<T>
      { if (!expect.Equals(got)) Fail("AreEqual({0}, {1})"   .args(expect, got), from); }

      public static void AreNotEqual<T>(T expect, T got, string from = null) where T : IEquatable<T>
      { if (expect.Equals(got))  Fail("AreNotEqual({0}, {1})".args(expect, got), from); }


      public static bool AreEqualTest<T>(Nullable<T> expect, Nullable<T> got) where T : struct, IEquatable<T>
      {
        if (!expect.HasValue && !got.HasValue) return true;
        if (!expect.HasValue || !got.HasValue) return false;

        return expect.Value.Equals(got.Value);
      }

      public static void AreEqual<T>(Nullable<T> expect, Nullable<T> got, string from = null) where T : struct, IEquatable<T>
      { if (!AreEqualTest(expect, got)) Fail("AreEqual({0}, {1})"   .args(expect, got), from); }

      public static void AreNotEqual<T>(Nullable<T> expect, Nullable<T> got, string from = null) where T : struct, IEquatable<T>
      { if (AreEqualTest(expect, got)) Fail("AreNotEqual({0}, {1})"   .args(expect, got), from); }




      public static bool AreWithinTest(decimal expect, decimal got, decimal delta) { return Math.Abs(expect - got) <= delta;  }

      public static void AreWithin    (decimal expect, decimal got, decimal delta, string from = null)
      { if (!AreWithinTest(expect, got, delta)) Fail("AreWithin({0}, {1}, {2})".args(expect, got, delta), from); }

      public static void AreNotWithin (decimal expect, decimal got, decimal delta, string from = null)
      { if (AreWithinTest(expect, got, delta)) Fail("AreNotWithin({0}, {1}, {2})".args(expect, got, delta), from); }



      public static bool AreWithinTest(float expect, float got, float delta) { return Math.Abs(expect - got) <= delta;  }

      public static void AreWithin    (float expect, float got, float delta, string from = null)
      { if (!AreWithinTest(expect, got, delta)) Fail("AreWithin({0}, {1}, {2})".args(expect, got, delta), from); }

      public static void AreNotWithin (float expect, float got, float delta, string from = null)
      { if (AreWithinTest(expect, got, delta)) Fail("AreNotWithin({0}, {1}, {2})".args(expect, got, delta), from); }



      public static bool AreWithinTest(double expect, double got, double delta) { return Math.Abs(expect - got) <= delta;  }

      public static void AreWithin    (double expect, double got, double delta, string from = null)
      { if (!AreWithinTest(expect, got, delta)) Fail("AreWithin({0}, {1}, {2})".args(expect, got, delta), from); }

      public static void AreNotWithin (double expect, double got, double delta, string from = null)
      { if (AreWithinTest(expect, got, delta)) Fail("AreNotWithin({0}, {1}, {2})".args(expect, got, delta), from); }


    #endregion

    #region IsTrue/False

      public static void IsTrue(bool condition, string from = null)
      {
        if (!condition) Fail("IsTrue({0})".args(condition), from);
      }

      public static void IsFalse(bool condition, string from = null)
      {
        if (condition) Fail("IsFalse({0})".args(condition), from);
      }
    #endregion

    #region AreSameRef/IsNull
      public static bool AreSameRefTest(object expect, object got)
      {
        if (expect==null && got==null) return true;
        if (expect==null) return false;

        return object.ReferenceEquals(expect, got);
      }

      public static void AreSameRef(object expect, object got, string from = null)
      {
        if (!AreSameRefTest(expect, got)) Fail("AreSameRef({0}, {1})".args(expect, got), from);
      }

      public static void AreNotSameRef(object expect, object got, string from = null)
      {
        if (AreSameRefTest(expect, got)) Fail("AreNotSameRef({0}, {1})".args(expect, got), from);
      }

      public static void IsNull(object reference, string from = null)
      {
        if (reference!=null) Fail("IsNull({0})".args(reference), from);
      }

      public static void IsNotNull(object reference, string from = null)
      {
        if (reference==null) Fail("IsNotNull({0})".args(reference), from);
      }

      public static void IsNull<T>(Nullable<T> got, string from = null) where T : struct
      {
        if (got.HasValue) Fail("IsNull({0})".args(got), from);
      }

      public static void IsNotNull<T>(Nullable<T> got, string from = null) where T : struct
      {
        if (!got.HasValue) Fail("IsNotNull({0})".args(got), from);
      }

    #endregion

    #region .pvt
      private static string args(this string pat, params object[] args)
      {
        for(var i=0; i<args.Length; i++)
          args[i] = argToStr(args[i]);

        return pat.Args(args);
      }

      private static string argToStr(object arg)
      {
         const int MAX_LEN = 32;

         if (arg==null) return StringConsts.NULL_STRING;

         var sarg = arg as string;
         if (sarg!=null)
           return "(string)\"{0}\" of {1} chars".Args(sarg.TakeFirstChars(MAX_LEN, "..."), sarg.Length);

         var barg = arg as byte[];
         if (barg!=null)
           return "(byte[])\"{0}\" of {1} bytes".Args(barg.ToDumpString(DumpFormat.Hex, maxLen: MAX_LEN), barg.Length);


         var tp = arg.GetType();

         if (tp.IsEnum) return "{0}.{1}".Args(tp.Name, arg);
         if (tp.IsPrimitive) return "({0}){1}".Args(tp.Name, arg);
         if (arg is Type) return "({0}){1}".Args(tp.Name, ((Type)arg).FullNameWithExpandedGenericArgs());

         return "({0})obj".Args(arg.GetType().FullNameWithExpandedGenericArgs(false));
      }
    #endregion

  }
}
