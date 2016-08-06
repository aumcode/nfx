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

using NFX.CodeAnalysis.Source;

namespace NFX.CodeAnalysis
{
  /// <summary>
  /// Represents a message emitted from code-analyzing entities such as lexers, parsers, semantic analyzers and compilers.
  /// Messages have severity type like warning, info or error etc.
  /// </summary>
  public sealed class Message
  {
    #region .ctor
      private Message(){}

      public Message(SourceCodeRef srcRef, MessageType type, int code, ICodeProcessor from, SourcePosition position, Token token, string text, Exception exception)
      {
         SourceCodeReference = srcRef;
         Type = type;
         Code = code;
         From = from;
         Position = position;
         this.Token = token;
         Text = text ?? string.Empty;
         AssociatedException = exception;
      }

      public Message(SourceCodeRef srcRef, MessageType type, int code, ICodeProcessor from, SourcePosition position, Token token, string text) :
           this(srcRef, type, code,  from, position, token, text, null)
      {
      }

      public Message(SourceCodeRef srcRef, MessageType type, int code, ICodeProcessor from, SourcePosition position, Token token) :
        this(srcRef, type, code, from, position, token, null, null)
      {
      }

      public Message(SourceCodeRef srcRef, MessageType type, int code, ICodeProcessor from, SourcePosition position) :
        this(srcRef, type, code, from, position, null, null, null)
      {
      }

      public Message(SourceCodeRef srcRef, MessageType type, int code, ICodeProcessor from) :
        this(srcRef, type, code, from, SourcePosition.UNASSIGNED, null, null, null)
      {
      }

    #endregion


    #region Properties
      /// <summary>
      /// Indicates whether this message is an internal message
      /// </summary>
      public bool IsInternal
      {
        get { return Type < MessageType.Info || Type>=MessageType.InternalError; }
      }


      /// <summary>
      /// Indicates whether this message is an info message
      /// </summary>
      public bool IsInfo
      {
        get { return (Type >= MessageType.Info) && (Type < MessageType.Warning); }
      }


      /// <summary>
      /// Indicates whether this message is a warning message
      /// </summary>
      public bool IsWarning
      {
        get { return (Type >= MessageType.Warning) && (Type < MessageType.Error); }
      }


      /// <summary>
      /// Indicates whether this message is an error message
      /// </summary>
      public bool IsError
      {
        get { return Type >= MessageType.Error; }
      }
    #endregion

    #region Public
      public readonly SourceCodeRef SourceCodeReference;
      public readonly MessageType Type;
      public readonly int Code;
      public readonly ICodeProcessor From;
      public readonly SourcePosition Position;
      public readonly Token Token;
      public readonly string Text;
      public readonly Exception AssociatedException;

      public override string ToString()
      {


        return
            @"#{0}.{1} {2} {3} at ""{4}"" {5} {6} {7} {8}".Args(
             Code,
             From.MessageCodeToString(Code),
             Type,
             From.GetType().Name,
             SourceCodeReference,
             (Token == null) ? Position.ToString() : string.Empty,
             (Token != null) ? "\"" + Token.ToString() + "\"" : string.Empty,
             (!string.IsNullOrWhiteSpace(Text)) ? "\"" + Text + "\"" : string.Empty,
             (AssociatedException!=null) ? AssociatedException.ToMessageWithType() : string.Empty
            );
      }
    #endregion

  }



  /// <summary>
  /// Describes an entity capable of receiving messages as they are emitted by entities such as parsers and compilers
  /// </summary>
  public interface ILanguageProcessorNotifications
  {
    void MessageAdded(Message message);
  }


  /// <summary>
  /// Provides a list of messages, this class is not thread-safe
  /// </summary>
  public class MessageList : List<Message>
  {

    public ILanguageProcessorNotifications NotificationsSink { get; set;}


    /// <summary>
    /// Emits new message
    /// </summary>
    public new void Add(Message message)
    {
      base.Add(message);
      if (NotificationsSink!=null) NotificationsSink.MessageAdded(message);
    }



    /// <summary>
    /// Enumerates all internal messages
    /// </summary>
    public IEnumerable<Message> Internals
    {
      get { return this.Where(msg => msg.IsInternal); }
    }

    /// <summary>
    /// Enumerates all info messages
    /// </summary>
    public IEnumerable<Message> Infos
    {
      get { return this.Where(msg => msg.IsInfo); }
    }

    /// <summary>
    /// Enumerates all warning messages
    /// </summary>
    public IEnumerable<Message> Warnings
    {
      get { return this.Where(msg => msg.IsWarning); }
    }

    /// <summary>
    /// Enumerates all error messages
    /// </summary>
    public IEnumerable<Message> Errors
    {
      get { return this.Where(msg => msg.IsError); }
    }


    public override string ToString()
    {
      StringBuilder sb = new StringBuilder();

      foreach (Message msg in this)
        sb.AppendLine(msg.ToString());

      return sb.ToString();
    }
  }


   /// <summary>
   /// Message type like: Info, Warning, Error
   /// </summary>
   public enum MessageType
   {
      //======================================
      Internal = 0,
      //======================================
      Info = 1000,
      //======================================
      Warning = 5000,
      //======================================
      Error = 1000000,
      InternalError = 10000000
   }










}
