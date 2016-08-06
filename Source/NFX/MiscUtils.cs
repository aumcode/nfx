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
 * Originated: 2006.01
 * Revision: NFX 1.0  2011.01.31
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.ComponentModel;
using System.Text;
using System.Net;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Data.Common;
using System.Text.RegularExpressions;
using System.Linq.Expressions;
using System.Net.Sockets;
using System.Threading.Tasks;

using NFX.Environment;
using NFX.DataAccess.CRUD;

namespace NFX
{
  public static class MiscUtils
  {
    public static readonly DateTime UNIX_EPOCH_START_DATE = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
    public static readonly string[] WIN_UNIX_LINE_BRAKES = new string[]{ "\r\n", "\n" };



    public static bool IsTrue(this Nullable<bool> value, bool dflt = false)
    {
      if (!value.HasValue) return dflt;
      return value.Value;
    }



    /// <summary>
    /// Checks the value for null and throws exception if it is.
    /// The method is useful for .ctor call chaining to preclude otherwise anonymous NullReferenceException
    /// </summary>
    public static T NonNull<T>(this T obj, Func<Exception> error = null, string text = null) where T : class
    {
      if (obj==null)
      {
        if (error!=null)
         throw error();
        else
         throw new NFXException(StringConsts.PARAMETER_MAY_NOT_BE_NULL_ERROR.Args(text ?? CoreConsts.UNKNOWN));
      }
      return obj;
    }

    /// <summary>
    /// Takes first X chars from a string. If string is null returns null. If string does not have enough
    /// the function returns what the string has
    /// </summary>
    public static string TakeFirstChars(this string str, int count, string ellipsis = null)
    {
      if (str==null) return null;
      if (str.Length<=count) return str;
      return str.Substring(0, count) + (ellipsis ?? string.Empty);
    }

    /// <summary>
    /// Gets number of seconds since Unix epoch start (1970/1/1 0:0:0)
    /// </summary>
    public static long ToSecondsSinceUnixEpochStart(this DateTime when)
    {
      return when.ToMillisecondsSinceUnixEpochStart() / 1000L;
    }

    /// <summary>
    /// Gets UTC DateTime from number of seconds since Unix epoch start (1970/1/1 0:0:0)
    /// </summary>
    public static DateTime FromSecondsSinceUnixEpochStart(this long when)
    {
      return UNIX_EPOCH_START_DATE.AddSeconds(when);
    }

    /// <summary>
    /// Gets UTC DateTime from number of seconds since Unix epoch start (1970/1/1 0:0:0)
    /// </summary>
    public static DateTime FromSecondsSinceUnixEpochStart(this ulong when)
    {
      return UNIX_EPOCH_START_DATE.AddSeconds(when);
    }

    /// <summary>
    /// Gets UTC DateTime from number of milliseconds since Unix epoch start (1970/1/1 0:0:0)
    /// </summary>
    public static DateTime FromMillisecondsSinceUnixEpochStart(this long when)
    {
      return UNIX_EPOCH_START_DATE.AddMilliseconds(when);
    }

    /// <summary>
    /// Gets UTC DateTime from number of microseconds since Unix epoch start (1970/1/1 0:0:0)
    /// </summary>
    public static DateTime FromMicrosecondsSinceUnixEpochStart(this long when)
    {
      return UNIX_EPOCH_START_DATE.AddTicks(when * 10);
    }

    /// <summary>
    /// Gets UTC DateTime from number of microseconds since Unix epoch start (1970/1/1 0:0:0)
    /// </summary>
    public static DateTime FromMicrosecondsSinceUnixEpochStart(this ulong when)
    {
      return UNIX_EPOCH_START_DATE.AddTicks((long)when * 10);
    }


    /// <summary>
    /// Gets UTC DateTime from number of milliseconds since Unix epoch start (1970/1/1 0:0:0)
    /// </summary>
    public static DateTime FromMillisecondsSinceUnixEpochStart(this ulong when)
    {
      return UNIX_EPOCH_START_DATE.AddMilliseconds(when);
    }

    /// <summary>
    /// Gets number of milliseconds since Unix epoch start (1970/1/1 0:0:0)
    /// </summary>
    public static long ToMillisecondsSinceUnixEpochStart(this DateTime when)
    {
        return (long)(when.ToUniversalTime() - UNIX_EPOCH_START_DATE).TotalMilliseconds;
    }

    /// <summary>
    /// Gets number of microseconds since Unix epoch start (1970/1/1 0:0:0)
    /// </summary>
    public static long ToMicrosecondsSinceUnixEpochStart(this DateTime when)
    {
        return (long)((when.ToUniversalTime() - UNIX_EPOCH_START_DATE).Ticks / 10);
    }

    /// <summary>
    /// Writes exception message with exception type
    /// </summary>
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    public static string ToMessageWithType(this Exception error)
    {
      return string.Format("[{0}] {1}", error.GetType().FullName, error.Message);
    }

    /// <summary>
    /// If there is error, converts its details to JSOnDataMap
    /// </summary>
    public static NFX.Serialization.JSON.JSONDataMap ToJSONDataMap(this Exception error, bool recurse = true, bool stackTrace = false)
    {
      if (error==null) return null;
      var result = new NFX.Serialization.JSON.JSONDataMap(false);

      result["Type"] = error.GetType().FullName;
      result["Msg"] = error.Message;
      result["Data"] = error.Data;
      result["Src"] = error.Source;
      if (stackTrace) result["STrace"] = error.StackTrace;

      if (recurse)
       result["Inner"] = error.InnerException.ToJSONDataMap(recurse);
      else
      {
        result["Inner.Type"] = error.InnerException!=null ? error.InnerException.GetType().FullName : null;
        result["Inner.Msg"] = error.InnerException!=null ? error.InnerException.Message : null;
        if (stackTrace) result["Inner.STrace"] = error.InnerException!=null ? error.InnerException.StackTrace : null;
      }

      return result;
    }

    /// <summary>
    /// Returns the full name of the type optionally prefixed with verbatim id specifier '@'.
    /// The generic arguments ar expanded into their full names i.e.
    ///   List'1[System.Object]  ->  System.Collections.Generic.List&lt;System.Object&gt;
    /// </summary>
    public static string FullNameWithExpandedGenericArgs(this Type type, bool verbatimPrefix = true)
    {
        var ns = type.Namespace;

        if (verbatimPrefix)
        {
            var nss = ns.Split('.');
            ns = string.Join(".@", nss);
        }


        var gargs = type.GetGenericArguments();

        if (gargs.Length==0)
        {
         return verbatimPrefix ? "@{0}.@{1}".Args( ns, type.Name) : type.FullName;
        }

        var sb = new StringBuilder();

        for(int i=0; i<gargs.Length; i++)
        {
            if (i>0) sb.Append(", ");
            sb.Append( gargs[i].FullNameWithExpandedGenericArgs(verbatimPrefix) );
        }

        var nm = type.Name;
        var idx = nm.IndexOf('`');
        if(idx>=0)
            nm = nm.Substring(0, idx);


        return verbatimPrefix ? "@{0}.@{1}<{2}>".Args(ns, nm, sb.ToString()) :
                                "{0}.{1}<{2}>".Args(ns, nm, sb.ToString());
    }

    /// <summary>
    /// Returns the the name of the type with expanded generic argument names.
    /// This helper is useful for printing class names to logs/messages.
    ///   List'1[System.Object]  ->  List&lt;Object&gt;
    /// </summary>
    public static string DisplayNameWithExpandedGenericArgs(this Type type)
    {

        var gargs = type.GetGenericArguments();

        if (gargs.Length==0)
        {
         return type.Name;
        }

        var sb = new StringBuilder();

        for(int i=0; i<gargs.Length; i++)
        {
            if (i>0) sb.Append(", ");
            sb.Append( gargs[i].DisplayNameWithExpandedGenericArgs() );
        }

        var nm = type.Name;
        var idx = nm.IndexOf('`');
        if(idx>=0)
            nm = nm.Substring(0, idx);


        return  "{0}<{1}>".Args(nm, sb.ToString());
    }





    /// <summary>
    /// Capitalizes first character of string
    /// </summary>
    public static string CapitalizeFirstChar(this string str)
    {
	    if (string.IsNullOrEmpty(str)) return string.Empty;

        char[] arr = str.ToCharArray();
	    arr[0] = char.ToUpper(arr[0]);

        return new String(arr);
    }

    /// <summary>
    /// Splits string into lines using Win or .nix line brakes
    /// </summary>
    public static string[] SplitLines(this string str)
    {
        return str.Split(WIN_UNIX_LINE_BRAKES, StringSplitOptions.None);
    }

    /// <summary>
    /// Returns MemberInfo described as short string
    /// </summary>
    public static string ToDescription(this MemberInfo mem)
    {
      var type = (mem is Type) ? ((Type)mem).FullName : (mem.DeclaringType != null ) ? mem.DeclaringType.FullName : CoreConsts.UNKNOWN;
      return string.Format("{0}{{{1} '{2}'}}", type, mem.MemberType, mem.Name );
    }


    /// <summary>
    /// Determines if component is being used within designer
    /// </summary>
    public static bool IsComponentDesignerHosted(Component cmp)
    {
      if (cmp != null)
      {

        if (cmp.Site != null)
          if (cmp.Site.DesignMode == true)
            return true;

      }

      return false;
    }

    /// <summary>
    /// Returns the name of entry point executable file optionaly with its path
    /// </summary>
    public static string EntryExeName(bool withPath = true)
    {
      var file = Assembly.GetEntryAssembly().Location;

      if (!withPath) file = Path.GetFileName(file);

      return file;
    }

    /// <summary>
    /// Shortcut helper for string.Format(tpl, params object[] args)
    /// </summary>
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    public static string Args(this string tpl, params object[] args)
    {
        return string.Format(tpl, args);
    }


    /// <summary>
    /// Interprets template of the form:  Some text {@value_name@:C} by replacing with property/field values.
    /// Note: this function does not recognize escapes for simplicity (as escapes can be replaced by regular strings instead)
    /// </summary>
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    public static string ArgsTpl(this string tpl, object args)
    {
        bool matched;
        return tpl.ArgsTpl(args, out matched);
    }


    /// <summary>
    /// Interprets template of the form:  Some text {@value_name@:C} by replacing with property/field values.
    /// Note: this function does not recognize escapes for simplicity (as escapes can be replaced by regular strings instead).
    /// Matched is set to true if at least one property match was made
    /// </summary>
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    public static string ArgsTpl(this string tpl, object args, out bool matched)
    {
        matched = false;
        if (tpl==null || args==null) return null;
        var result = tpl;
        var t = args.GetType();
        var lst = new List<object>();
        foreach(var pi in t.GetProperties())
        {
          var val = pi.GetValue(args);
          if (val==null) val = string.Empty;
          lst.Add(val);

          var replacedResult = result.Replace('@'+pi.Name+'@', (lst.Count-1).ToString());

          if (!string.Equals(result, replacedResult, StringComparison.Ordinal))
           matched = true;

          result = replacedResult;
        }
        return result.Args(lst.ToArray());
    }


    /// <summary>
    /// Converts string of format "xx.xx.xx.xx:nnnn" into IPEndPoint.
    /// DNS names are NOT supported
    /// </summary>
    // FIXME: should it be completely removed?
    [Obsolete]
    public static IPEndPoint IPStringToIPEndPoint(this string str)
    {
      try
      {
        str = str ?? string.Empty;
        string[] ip = str.Split(':');
        return new IPEndPoint(ip[0] == "*" ? IPAddress.Any : IPAddress.Parse(ip[0]), int.Parse(ip[1]));
      }
      catch
      {
        throw new NFXException(StringConsts.INVALID_IPSTRING_ERROR + str);
      }
    }


    /// <summary>
    /// Resolve IP address by Name
    /// </summary>
    /// <param name="epoint">An ip address or DNS host name with optional port separated by ':'</param>
    /// <param name="dfltPort">Port number to use if not supplied in endpoint string</param>
    /// <returns>IPEndPoint instance or null supplied string could not be parsed</returns>
    public static IPEndPoint ToIPEndPoint(this string epoint, int dfltPort = 0)
    {
        if (string.IsNullOrWhiteSpace(epoint)) throw new NFXException(string.Format(StringConsts.INVALID_EPOINT_ERROR, StringConsts.NULL_STRING, "null arg"));

        try
        {
            string[] parts = epoint.Split(':');
            var port = parts.Length > 1 && !parts[1].IsNullOrEmpty() ? int.Parse(parts[1]) : dfltPort;

            // Note that the GetHostEntry("127.0.0.1") call looks up the host entry that
            // may not contain localhost, even though it does do IPAddress.TryParse(epoint) call,
            // So we have no other way but to TryParse it outselves.
            IPAddress address;
            if (parts[0] == "*")
                return new IPEndPoint(IPAddress.Any, port);
            else if (IPAddress.TryParse(parts[0], out address))
                return new IPEndPoint(address, port);

            var hostEntry = Dns.GetHostEntry(parts[0]);

            return new IPEndPoint(hostEntry.AddressList.First(a => a.AddressFamily==AddressFamily.InterNetwork), port);
        }
        catch(Exception error)
        {
            throw new NFXException(string.Format(StringConsts.INVALID_EPOINT_ERROR, epoint, error.ToMessageWithType()), error);
        }
    }

    /// <summary>
    /// Runs specified process and waits for termination returning standard process output.
    /// This is a blocking call
    /// </summary>
    public static string RunAndCompleteProcess(string name, string args)
    {
      string std_out = string.Empty;

      Process p = new Process();
      p.StartInfo.FileName = name;
      p.StartInfo.Arguments = args;
      p.StartInfo.UseShellExecute = false;
      p.StartInfo.CreateNoWindow = true;
      p.StartInfo.RedirectStandardOutput = true;
      p.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
      p.Start();

      try
      {
        std_out = p.StandardOutput.ReadToEnd();
        p.WaitForExit();
      }
      finally
      {
        p.Close();
      }

      return std_out;
    }


    /// <summary>
    /// Walks all file names that match the pattern in a directory
    /// </summary>
    public static IEnumerable<string> AllFileNamesThatMatch(this string fromFolder, string pattern, bool recurse)
    {
      return Directory.GetFiles(fromFolder,
                         pattern,
                         recurse ? SearchOption.AllDirectories :
                                   SearchOption.TopDirectoryOnly);
    }

    /// <summary>
    /// Walks all file names in a directory
    /// </summary>
    public static IEnumerable<string> AllFileNames(this string fromFolder, bool recurse)
    {
      return fromFolder.AllFileNamesThatMatch("*.*", recurse);
    }

    /// <summary>
    /// Generates GUID based on a string MD5 hash
    /// </summary>
    public static Guid ToGUID(this string input)
    {
      using (var md5 = new System.Security.Cryptography.MD5CryptoServiceProvider())
        return new Guid(md5.ComputeHash(Encoding.Default.GetBytes(input)));
    }


    /// <summary>
    /// Returns a MD5 hash of a UTF8 string represented as hex string
    /// </summary>
    public static string ToMD5String(this string input)
    {
      if (string.IsNullOrEmpty(input))
       return "00000000000000000000000000000000";

      using (var md5 = new System.Security.Cryptography.MD5CryptoServiceProvider())
      {
         var hash = md5.ComputeHash(Encoding.UTF8.GetBytes(input));

         var result = new StringBuilder();

         for(var i=0;i<hash.Length;i++)
          result.Append(hash[i].ToString("x2"));

         return result.ToString();
      }
    }

    /// <summary>
    /// Returns a MD5 hash of a UTF8 string represented as byte[]
    /// </summary>
    public static byte[] ToMD5(this string input)
    {
      if (string.IsNullOrEmpty(input)) return new byte[16];

      using (var md5 = new System.Security.Cryptography.MD5CryptoServiceProvider())
      {
         return md5.ComputeHash(Encoding.UTF8.GetBytes(input));
      }
    }

    /// <summary>
    /// Returns a MD5 hash of a byte array
    /// </summary>
    public static byte[] ToMD5(this byte[] input)
    {
      if (input==null) return new byte[16];

      using (var md5 = new System.Security.Cryptography.MD5CryptoServiceProvider())
      {
         return md5.ComputeHash(input);
      }
    }





    /// <summary>
    /// Returns true when supplied name can be used for XML node naming (node names, attribute names)
    /// </summary>
    public static bool IsValidXMLName(this string name)
    {
      for(int i=0; i<name.Length; i++)
      {
        char c = name[i];
        if (c=='-' || c=='_') continue;
        if (!Char.IsLetterOrDigit(c) || (i==0 && !Char.IsLetter(c))) return false;
      }

      return true;
    }

    /// <summary>
    /// Helper function that performs GetHashcode for string using invariant comparison.
    /// Use this in conjunction with EqualsSenseCase
    /// </summary>
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    public static int GetHashCodeSenseCase(this string str)
    {
        return StringComparer.InvariantCulture.GetHashCode(str);
    }

    /// <summary>
    /// Helper function that performs comparison between strings using invariant comparison.
    /// Either lhs and rhs can be null.
    /// Use this in conjunction with GetHashCodeSenseCase
    /// </summary>
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    public static bool EqualsSenseCase(this string lhs, string rhs)
    {
        return StringComparer.InvariantCulture.Equals(lhs, rhs);
    }


    /// <summary>
    /// Helper function that performs case-insensitive GetHashcode for string using invariant comparison.
    /// Use this in conjunction with EqualsIgnoreCase
    /// </summary>
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    public static int GetHashCodeIgnoreCase(this string str)
    {
        return StringComparer.InvariantCultureIgnoreCase.GetHashCode(str);
    }

    /// <summary>
    /// Helper function that performs case-insensitive comparison between strings using invariant comparison.
    /// Either lhs and rhs can be null.
    /// Use this in conjunction with GetHashCodeIgnoreCase
    /// </summary>
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    public static bool EqualsIgnoreCase(this string lhs, string rhs)
    {
        return StringComparer.InvariantCultureIgnoreCase.Equals(lhs, rhs);
    }


    /// <summary>
    /// Helper function that performs case-insensitive GetHashcode for string using ordinal comparison.
    /// Use this in conjunction with EqualsOrdSenseCase
    /// </summary>
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    public static int GetHashCodeOrdSenseCase(this string str)
    {
        return StringComparer.Ordinal.GetHashCode(str);
    }

    /// <summary>
    /// Helper function that performs case-insensitive comparison between strings using ordinal comparison.
    /// Either lhs and rhs can be null.
    /// Use this in conjunction with GetHashCodeOrdSenseCase
    /// </summary>
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    public static bool EqualsOrdSenseCase(this string lhs, string rhs)
    {
        return StringComparer.Ordinal.Equals(lhs, rhs);
    }


    /// <summary>
    /// Helper function that performs case-insensitive GetHashcode for string using ordinal comparison.
    /// Use this in conjunction with EqualsOrdIgnoreCase
    /// </summary>
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    public static int GetHashCodeOrdIgnoreCase(this string str)
    {
        return StringComparer.OrdinalIgnoreCase.GetHashCode(str);
    }

    /// <summary>
    /// Helper function that performs case-insensitive comparison between strings using ordinal comparison.
    /// Either lhs and rhs can be null.
    /// Use this in conjunction with GetHashCodeOrdIgnoreCase
    /// </summary>
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    public static bool EqualsOrdIgnoreCase(this string lhs, string rhs)
    {
        return StringComparer.OrdinalIgnoreCase.Equals(lhs, rhs);
    }

    /// <summary>
    /// Helper function that calls string.IsNullOrEmpty()
    /// </summary>
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    public static bool IsNullOrEmpty(this string s)
    {
        return string.IsNullOrEmpty(s);
    }

    /// <summary>
    /// Helper function that calls !string.IsNullOrEmpty()
    /// </summary>
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    public static bool IsNotNullOrEmpty(this string s)
    {
        return !string.IsNullOrEmpty(s);
    }

    /// <summary>
    /// Helper function that calls string.IsNullOrWhiteSpace()
    /// </summary>
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    public static bool IsNullOrWhiteSpace(this string s)
    {
        return string.IsNullOrWhiteSpace(s);
    }

    /// <summary>
    /// Helper function that calls !string.IsNullOrWhiteSpace()
    /// </summary>
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    public static bool IsNotNullOrWhiteSpace(this string s)
    {
        return !string.IsNullOrWhiteSpace(s);
    }

    /// <summary>
    /// Defaults string if it is null or whitespace
    /// </summary>
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    public static string Default(this string str, string val = "")
    {
       if (str.IsNullOrWhiteSpace()) return val;
       return str;
    }


    /// <summary>
    /// Convert a buffer to a printable string
    /// </summary>
    /// <param name="buf">Buffer to convert</param>
    /// <param name="fmt">Dumping format</param>
    /// <param name="offset">Starting index</param>
    /// <param name="count">Number of bytes to process (-1 means all bytes in the buffer)</param>
    /// <param name="eol">If true, terminate with end-of-line</param>
    /// <param name="encoding">Encoding to use for writing data in Binary format</param>
    /// <param name="maxLen">Max length of the returned string. Pass 0 for unlimited length</param>
    /// <returns></returns>
    public static string ToDumpString(this byte[] buf, DumpFormat fmt, int offset = 0,
        int count = -1, bool eol = false, Encoding encoding = null, int maxLen = 0)
    {
        if (count == -1) count = buf.Length - offset;

        int n;

        switch (fmt)
        {
            case DumpFormat.Decimal:    n = count * 4 + 4; break;
            case DumpFormat.Hex:        n = count * 3; break;
            case DumpFormat.Printable:  n = count * 4; break;
            default:                    throw new NFXException(StringConsts.OPERATION_NOT_SUPPORTED_ERROR + fmt.ToString());
        }

        var sb = new StringBuilder(n);

        int k = offset+count;
        int m = maxLen > 0 ? Math.Max(2, maxLen) : Int16.MaxValue;
        bool shrink = maxLen > 0;

        switch (fmt)
        {
            case DumpFormat.Decimal:
                m -= 2;
                sb.Append("<<");
                for (int i = offset; i < k && sb.Length+1/*comma*/ < m; i++)
                {
                    sb.Append(buf[i]);
                    sb.Append(',');
                }
                if (sb.Length > 2)
                    sb.Remove(sb.Length-1, 1);
                sb.Append(!shrink || sb.Length+1 < m ? ">>" : "...>>");
                break;
            case DumpFormat.Hex:
                m -= 3;
                for (int i = offset, j = 0; i < k && sb.Length < m; i++, j++)
                    sb.AppendFormat("{0:X2}{1}", buf[i], (j & 3) == 3 ? " " : "");
                if (sb.Length > 0 && sb[sb.Length-1] == ' ')
                    sb.Remove(sb.Length-1, 1);
                if (shrink) sb.Append("...");
                break;
            case DumpFormat.Printable:
                m -= 3;
                for (int i = offset; i < k && sb.Length < m; i++)
                {
                    byte c = buf[i];
                    if (c >= 32 && c < 127)
                        sb.Append((char)c);
                    else
                        switch (c)
                        {
                            case 10: sb.Append("\n"); break;
                            case 13: sb.Append("\r"); break;
                            case  8: sb.Append("\t"); break;
                            default: sb.AppendFormat("\\{0,3:D3}", c); break;
                        }
                }
                if (shrink) sb.Append("...");
                break;
        }

        if (eol)
            sb.Append('\n');

        return sb.ToString();
    }

    /// <summary>
    /// Converts dictionary into configuration where every original node gets represented as a sub-section of config's root
    /// </summary>
    public static Configuration ToConfigSections(this IDictionary<string, object> dict)
    {
        var result = new MemoryConfiguration();
        result.Create();
        foreach(var pair in dict)
         result.Root.AddChildNode(pair.Key, pair.Value);

        return result;
    }

    /// <summary>
    /// Converts dictionary into configuration where every original node gets represented as an attribute of config's root
    /// </summary>
    public static Configuration ToConfigAttributes(this IDictionary<string, object> dict)
    {
        var result = new MemoryConfiguration();
        result.Create();
        foreach(var pair in dict)
         result.Root.AddAttributeNode(pair.Key, pair.Value);

        return result;
    }

    /// <summary>
    ///  Evaluates variables in a context of optional configuration supplied in XML format
    /// </summary>
    public static string EvaluateVarsInXMLConfigScope(this string line, string xmlScope = null, IEnvironmentVariableResolver envResolver = null, IMacroRunner macroRunner = null)
    {
        Configuration config = null;
        if (!string.IsNullOrWhiteSpace(xmlScope))
        {
          config =  XMLConfiguration.CreateFromXML(xmlScope);
          config.EnvironmentVarResolver = envResolver;
          config.MacroRunner = macroRunner;
        }
        return line.EvaluateVarsInConfigScope( config );
    }

    /// <summary>
    ///  Evaluates variables in a context of optional configuration supplied as dictionary which is converted to attributes
    /// </summary>
    public static string EvaluateVarsInDictionaryScope(this string line, IDictionary<string, object> dict = null, IEnvironmentVariableResolver envResolver = null, IMacroRunner macroRunner = null)
    {
        if (dict==null)
          return line.EvaluateVarsInConfigScope(null);
        else
        {
          var config = dict.ToConfigAttributes();
          config.EnvironmentVarResolver = envResolver;
          config.MacroRunner = macroRunner;
          return line.EvaluateVarsInConfigScope(config);
        }
    }


    /// <summary>
    ///  Evaluates variables in a context of optional variable resolver and macro runner
    /// </summary>
    public static string EvaluateVars(this string line, IEnvironmentVariableResolver envResolver = null, IMacroRunner macroRunner = null)
    {
       var config =  new MemoryConfiguration();
       config.Create();
       config.EnvironmentVarResolver = envResolver;
       config.MacroRunner = macroRunner;
       return EvaluateVarsInConfigScope(line, config);
    }

    /// <summary>
    ///  Evaluates variables in a context of optional configuration supplied as config object
    /// </summary>
    public static string EvaluateVarsInConfigScope(this string line, Configuration scopeConfig = null)
    {
        if (scopeConfig==null)
        {
          scopeConfig = new MemoryConfiguration();
          scopeConfig.Create();
        }

        return scopeConfig.Root.EvaluateValueVariables(line);
    }

    /// <summary>
    /// Swaps string letters that "obfuscates" string- usefull for generation of keys from strings that have to become non-obvious to user.
    /// This function does not offer any real protection (as it is easy to decipher the original value), just visual.
    /// The name comes from non-existing science "Burmatography" used in "Neznaika" kids books
    /// </summary>
    public static string Burmatographize(this string src, bool rtl = false)
    {
        if (src.IsNullOrWhiteSpace()) return src;
        var sb = new StringBuilder(src.Length);

        var odd = rtl;
        for(int f=0,b=src.Length-1; f<=b;)
        {
          if (odd) { sb.Append(src[f]); f++;}
             else  { sb.Append(src[b]); b--;}
          odd = !odd;
        }
        return sb.ToString();
    }

    /// <summary>
    /// Converts expression tree to simple textual form for debugging
    /// </summary>
    public static string ToDebugView(this Expression expr, int indent = 0)
    {
      const int TS = 2;

      var pad = "".PadLeft(TS*indent,' ');

      if (expr==null) return "";
      if (expr is BlockExpression)
      {
        var block = (BlockExpression)expr;
        var sb = new StringBuilder();
        sb.Append(pad);  sb.AppendLine("{0}{{".Args(expr.GetType().Name));

        sb.Append(pad);  sb.AppendLine("//variables");
        foreach( var v in block.Variables)
        {
           sb.Append(pad);
           sb.AppendLine("var {0}  {1};" .Args(v.Type.FullName, v.Name));
        }


        sb.Append(pad); sb.AppendLine("//sub expressions");
        foreach( var se in block.Expressions)
        {
           sb.Append(pad);
           sb.AppendLine( se.ToDebugView(indent+1) );
        }

        sb.Append(pad); sb.AppendLine("}");
        return sb.ToString();
      }
      return pad + expr.ToString();
    }

    /// <summary>
    /// Appends the string followed by new line and returned by processing a composite format string, which contains zero or more format items, to this instance.
    /// Each format item is replaced by the string representation of a single argument.
    /// </summary>
    public static StringBuilder AppendFormatLine(this StringBuilder builder, string str, object arg0)
    {
      builder.AppendFormat(str, arg0);
      return builder.AppendLine();
    }

    /// <summary>
    /// Appends the string followed by new line and returned by processing a composite format string, which contains zero or more format items, to this instance.
    /// Each format item is replaced by the string representation of a single argument.
    /// </summary>
    public static StringBuilder AppendFormatLine(this StringBuilder builder, string str, object arg0, object arg1)
    {
      builder.AppendFormat(str, arg0, arg1);
      return builder.AppendLine();
    }

    /// <summary>
    /// Appends the string followed by new line and returned by processing a composite format string, which contains zero or more format items, to this instance.
    /// Each format item is replaced by the string representation of a single argument.
    /// </summary>
    public static StringBuilder AppendFormatLine(this StringBuilder builder, string str, object arg0, object arg1, object arg2)
    {
      builder.AppendFormat(str, arg0, arg1, arg2);
      return builder.AppendLine();
    }

    /// <summary>
    /// Appends the string followed by new line and returned by processing a composite format string, which contains zero or more format items, to this instance.
    /// Each format item is replaced by the string representation of a single argument.
    /// </summary>
    public static StringBuilder AppendFormatLine(this StringBuilder builder, string str, params object[] args)
    {
      builder.AppendFormat(str, args);
      return builder.AppendLine();
    }
  }


  public static class URIUtils
  {
    public static readonly char[] PATH_JOIN_TRIM_CHARS = new char[]{'/', ' ', '\\'};

    /// <summary>
    /// Joins URI path segments with "/". This function just concats strings, it does not evaluate relative paths etc.
    /// The first segment may or may not start with '/'
    /// </summary>
    public static string JoinPathSegs(params string[] segments)
    {
      if (segments==null || segments.Length==0) return string.Empty;

      var sb = new StringBuilder();

      var first = true;
      for(var i=0; i<segments.Length; i++)
      {
         var seg = segments[i];
         if (seg==null) continue;

         seg = (first ? seg.TrimStart(' ') : seg.TrimStart(PATH_JOIN_TRIM_CHARS)).TrimEnd(PATH_JOIN_TRIM_CHARS);

         if (seg.IsNullOrWhiteSpace()) continue;

         if (!first) sb.Append('/');
         sb.Append(seg);
         first = false;
      }

      return sb.ToString();
    }


    /// <summary>
    /// Performs URI.EscapeDataString with additional replacement of " and ' chars with their hex equivalents.
    /// This method is suitable for escaping client-side intelligent keys that may have single/double quotes
    /// </summary>
    public static string EscapeURIDataStringWithQuotes(this string uri)
    {
      const string SQ = "%27";
      const string DQ = "%22";

      if (uri.IsNullOrWhiteSpace()) return string.Empty;

      var ds = Uri.EscapeDataString(uri);
      var result = new StringBuilder();
      for(var i=0; i<ds.Length; i++)
      {
        var c = ds[i];
        if (c=='\'') result.Append(SQ);
        else
        if (c=='"') result.Append(DQ);
        else
         result.Append(c);
      }

      return result.ToString();
    }

    public static string ComposeURLQueryString(IDictionary<string, object> qParams)
    {
      if (qParams == null || qParams.Count <= 0)
        return string.Empty;

      var builder = new StringBuilder();
      foreach (var kvp in qParams)
      {
        if (kvp.Key.IsNullOrWhiteSpace()) continue;

        builder.Append(EscapeAllDataStringChars(kvp.Key));
        if (kvp.Value != null)
          builder.Append('=').Append(EscapeAllDataStringChars(kvp.Value.AsString()));
        builder.Append('&');
      }

      if (builder.Length > 0) builder.Length--;

      return builder.ToString();
    }

    /// <summary>
    /// Escapes all chars except latin A..Z, 0..9, and . - _
    /// This function is based on EscapeDataString but does not depend on .NET 4/4.5 differences encoding ! * ( ) and others...
    /// </summary>
    public static string EscapeAllDataStringChars(this string str)
    {
      if (str.IsNullOrWhiteSpace()) return string.Empty;

      str = Uri.EscapeDataString(str);//.NET <4.5.2 does not encode ! * ( ) properly

      var sb = new StringBuilder();
      for(var i=0; i<str.Length; i++)
      {
        var ch = str[i];
        if ((ch>='A' && ch<='Z') ||
            (ch>='a' && ch<='z') ||
            (ch>='0' && ch<='9') ||
            (ch=='%' || ch=='-' || ch=='_' || ch=='.')
           )
        {
          sb.Append(ch);
          continue;
        }
        sb.Append(Uri.HexEscape(ch));
      }

      return sb.ToString();
    }

  }
}
