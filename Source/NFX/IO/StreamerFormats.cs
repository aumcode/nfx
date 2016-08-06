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
using System.Linq.Expressions;
using System.Text;
using System.IO;
using System.Reflection;



namespace NFX.IO
{

    /// <summary>
    /// Describes a format - apair of readers/writers along with their capabilities - what types format supports natively
    /// </summary>
    public abstract class StreamerFormat
    {
      public abstract Type ReaderType { get; }
      public abstract Type WriterType { get; }



      /// <summary>
      /// Makes new reader instance
      /// </summary>
      public abstract ReadingStreamer GetReadingStreamer(Encoding encoding = null);

      /// <summary>
      /// Makes new writer instance
      /// </summary>
      public abstract WritingStreamer GetWritingStreamer(Encoding encoding = null);

      /// <summary>
      /// Returns true when the supplied type is natively supported by format
      /// </summary>
      public abstract bool IsTypeSupported(Type t);

      /// <summary>
      /// Returns true when the supplied ref type is natively supported by format
      /// </summary>
      public abstract bool IsRefTypeSupported(Type t);


      /// <summary>
      /// Returns a method info for reading a certain value type for this format or null if type is not supported
      /// </summary>
      public abstract MethodInfo GetReadMethodForType(Type t);

      /// <summary>
      /// Returns a method info for reading a certain ref type for this format or null if type is not supported
      /// </summary>
      public abstract MethodInfo GetReadMethodForRefType(Type t);

      /// <summary>
      /// Returns a method info for writing a certain value type for this format or null if type is not supported
      /// </summary>
      public abstract MethodInfo GetWriteMethodForType(Type t);

      /// <summary>
      /// Returns a method info for writing a certain ref type for this format or null if type is not supported
      /// </summary>
      public abstract MethodInfo GetWriteMethodForRefType(Type t);

    }


     /// <summary>
    /// Describes a format - apair of readers/writers along with their capabilities.
    /// Developers may derive new formats that support custom serialization schemes
    /// </summary>
    public abstract class StreamerFormat<TReader, TWriter> : StreamerFormat where TReader : ReadingStreamer
                                                                            where TWriter : WritingStreamer
    {

      //.ctor
       protected StreamerFormat()
       {
          m_ReadMethods = new Dictionary<Type,MethodInfo>();
          m_WriteMethods = new Dictionary<Type,MethodInfo>();

          m_ReadMethodsRefT = new Dictionary<Type,MethodInfo>();
          m_WriteMethodsRefT = new Dictionary<Type,MethodInfo>();

          var t = this.WriterType;//typeof(TWriter);

              var wMethods = t.GetMethods(BindingFlags.Instance | BindingFlags.Public)
                              .Where(mi => mi.Name.Equals("Write") &&   //do not localize
                                          mi.ReturnType==typeof(void) &&
                                          mi.GetParameters().Length==1);

              foreach(var mi in wMethods)
              {
                var pt = mi.GetParameters()[0].ParameterType;

                if (pt.IsValueType || pt==typeof(string))//string is treated like a value type
                  m_WriteMethods.Add(pt, mi);
                else
                  m_WriteMethodsRefT.Add(pt, mi);
              }

          t = this.ReaderType;//typeof(TReader);

              var rMethods = t.GetMethods(BindingFlags.Instance | BindingFlags.Public)
                              .Where(mi => mi.Name.StartsWith("Read") && mi.ReturnType!=typeof(void) && !mi.IsConstructor);//do not localize

              foreach(var mi in rMethods)
              {
                var rt = mi.ReturnType;

                if (rt.IsValueType || rt==typeof(string))//string is treated like a value type
                  m_ReadMethods.Add(rt, mi);
                else
                  m_ReadMethodsRefT.Add(rt, mi);
              }

          compileActions();
       }


        private Dictionary<Type, MethodInfo> m_ReadMethods;
        private Dictionary<Type, MethodInfo> m_WriteMethods;

        private Dictionary<Type, Func<TReader, object>> m_ReadActions;
        private Dictionary<Type, Action<TWriter, object>> m_WriteActions;


        private Dictionary<Type, MethodInfo> m_ReadMethodsRefT;
        private Dictionary<Type, MethodInfo> m_WriteMethodsRefT;

        private Dictionary<Type, Func<TReader, object>> m_ReadActionsRefT;
        private Dictionary<Type, Action<TWriter, object>> m_WriteActionsRefT;




      /// <summary>
      /// Makes new reader instance
      /// </summary>
      public sealed override ReadingStreamer GetReadingStreamer(Encoding encoding = null)
      {
          return MakeReadingStreamer(encoding);
      }

      /// <summary>
      /// Makes new writer instance
      /// </summary>
      public sealed override WritingStreamer GetWritingStreamer(Encoding encoding = null)
      {
         return MakeWritingStreamer(encoding);
      }

       /// <summary>
      /// Makes new reader instance
      /// </summary>
      public abstract TReader MakeReadingStreamer(Encoding encoding = null);

      /// <summary>
      /// Makes new writer instance
      /// </summary>
      public abstract TWriter MakeWritingStreamer(Encoding encoding = null);


          /// <summary>
          /// Returns true when the supplied value type is natively supported by format, that is - when this format
          ///  can directly write instances of this type without reflection/complex graph walk.
          /// </summary>
          public override bool IsTypeSupported(Type t)
          {
             return m_WriteMethods.ContainsKey(t);
          }

          /// <summary>
          /// Returns true when the supplied reference type is natively supported by format, that is - when this format
          ///  can directly write instances of this type without reflection/complex graph walk.
          /// </summary>
          public override bool IsRefTypeSupported(Type t)
          {
             return m_WriteMethodsRefT.ContainsKey(t);
          }


          /// <summary>
          /// Returns a method info for reading a certain value type for this format or null if this type is not directly supported.
          /// Use IsTypeSupported(type) to see if the type is native to this format.
          /// </summary>
          public sealed override MethodInfo GetReadMethodForType(Type t)
          {
            MethodInfo result = null;
            if (m_ReadMethods.TryGetValue(t, out result)) return result;
            return null;
          }


          /// <summary>
          /// Returns a method info for reading a certain reference type for this format or null if this type is not directly supported.
          /// Use IsRefTypeSupported(type) to see if the ref type is native to this format.
          /// </summary>
          public sealed override MethodInfo GetReadMethodForRefType(Type t)
          {
            MethodInfo result = null;
            if (m_ReadMethodsRefT.TryGetValue(t, out result)) return result;
            return null;
          }


          /// <summary>
          /// Returns a method info for writing a certain value type for this format.
          /// Use IsTypeSupported(type) to see if the type is native to this format.
          /// </summary>
          public sealed override MethodInfo GetWriteMethodForType(Type t)
          {
            MethodInfo result = null;
            if (m_WriteMethods.TryGetValue(t, out result)) return result;
            return null;
          }

          /// <summary>
          /// Returns a method info for writing a certain ref type for this format.
          /// Use IsRefTypeSupported(type) to see if the type is native to this format.
          /// </summary>
          public sealed override MethodInfo GetWriteMethodForRefType(Type t)
          {
            MethodInfo result = null;
            if (m_WriteMethodsRefT.TryGetValue(t, out result)) return result;
            return null;
          }


          /// <summary>
          /// Returns a function that reads the specified value type and returns it as object
          /// </summary>
          public Func<TReader, object> GetReadActionForType(Type t)
          {
            Func<TReader, object> result = null;
            if (m_ReadActions.TryGetValue(t, out result)) return result;
            return null;
          }

          /// <summary>
          /// Returns a function that reads the specified ref type and returns it as object
          /// </summary>
          public Func<TReader, object> GetReadActionForRefType(Type t)
          {
            Func<TReader, object> result = null;
            if (m_ReadActionsRefT.TryGetValue(t, out result)) return result;
            return null;
          }

          /// <summary>
          /// Returns an action that writes the value of the specified value type
          /// </summary>
          public Action<TWriter, object> GetWriteActionForType(Type t)
          {
            Action<TWriter, object> result = null;
            if (m_WriteActions.TryGetValue(t, out result)) return result;
            return null;
          }

          /// <summary>
          /// Returns an action that writes the value of the specified ref type
          /// </summary>
          public Action<TWriter, object> GetWriteActionForRefType(Type t)
          {
            Action<TWriter, object> result = null;
            if (m_WriteActionsRefT.TryGetValue(t, out result)) return result;
            return null;
          }


          private void compileActions()
          {
            m_ReadActions = new Dictionary<Type,Func<TReader,object>>();
            m_WriteActions = new Dictionary<Type,Action<TWriter,object>>();

            m_ReadActionsRefT = new Dictionary<Type,Func<TReader,object>>();
            m_WriteActionsRefT = new Dictionary<Type,Action<TWriter,object>>();

            foreach(var rkvp in m_ReadMethods)
            {
              var returnType = rkvp.Key;
              var mi = rkvp.Value;
              m_ReadActions.Add(returnType, compileReader(returnType, mi));
            }

              foreach(var rkvp in m_ReadMethodsRefT)
              {
                var returnType = rkvp.Key;
                var mi = rkvp.Value;
                m_ReadActionsRefT.Add(returnType, compileReader(returnType, mi));
              }

            foreach(var wkvp in m_WriteMethods)
            {
              var argType = wkvp.Key;
              var mi = wkvp.Value;
              m_WriteActions.Add(argType, compileWriter(argType, mi));
            }

              foreach(var wkvp in m_WriteMethodsRefT)
              {
                var argType = wkvp.Key;
                var mi = wkvp.Value;
                m_WriteActionsRefT.Add(argType, compileWriter(argType, mi));
              }
          }


          private Func<TReader, object> compileReader(Type tp, MethodInfo miRead)
          {
              var pReadingStreamer = Expression.Parameter(typeof(TReader));
              return Expression.Lambda<Func<TReader, object>>(
                              Expression.Convert(  Expression.Call(pReadingStreamer, miRead), typeof(object)  ),
                              pReadingStreamer).Compile();
          }

          private Action<TWriter, object> compileWriter(Type tp, MethodInfo miWrite)
          {
              var pWritingStreamer = Expression.Parameter(typeof(TWriter));
              var pValue = Expression.Parameter(typeof(object));
              return Expression.Lambda<Action<TWriter, object>>(
                              Expression.Call(pWritingStreamer, miWrite,
                                                Expression.Convert( pValue, tp)
                                              ),
                              pWritingStreamer, pValue).Compile();
          }


    }

}
