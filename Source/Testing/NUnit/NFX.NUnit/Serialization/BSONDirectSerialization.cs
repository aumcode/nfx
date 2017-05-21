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
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using NUnit.Framework;

using NFX.Log;
using NFX.DataAccess.Distributed;
using NFX.Serialization.BSON;
using NFX.Serialization.JSON;
using NFX.Financial;


namespace NFX.NUnit.Serialization
{
  [TestFixture]
  public class BSONDirectSerialization
  {
    [Test]
    public void SerializeDeserializeLogMessage()
    {
      var ser = new BSONSerializer( new BSONTypeResolver(typeof(Message)) );

      var msg = new NFX.Log.Message
      {
         Type = MessageType.DebugGlue,
         TimeStamp = App.TimeSource.UTCNow,
         Channel = "MTV",
          From = "Zhaba",
           Topic = "App",
            Text = "Hello text",
             Source = 12345,
              Parameters = "aaaaa",
               ArchiveDimensions = "a=1 b=2"
      };

      var doc = ser.Serialize( msg );

      Console.WriteLine( doc.ToJSON(JSONWritingOptions.PrettyPrint) );


      Aver.IsTrue( doc.IndexOfName(ser.TypeIDFieldName)>=0);//field was added

      var got = ser.Deserialize(doc) as NFX.Log.Message;

      testMsgEquWoError( msg, got);
    }


    [Test]
    public void SerializeDeserializeLogMessage_KnownTypes()
    {
      var ser = new BSONSerializer();//notice no resolver

      var msg = new NFX.Log.Message
      {
         Type = MessageType.DebugGlue,
         TimeStamp = App.TimeSource.UTCNow,
         Channel = "MTV",
          From = "Zhaba",
           Topic = "App",
            Text = "Hello text",
             Source = 12345,
              Parameters = "aaaaa",
               ArchiveDimensions = "a=1 b=2"
      };

      var doc = ser.Serialize( msg , new BSONParentKnownTypes(typeof(Message)));

      Console.WriteLine( doc.ToJSON(JSONWritingOptions.PrettyPrint) );

      Aver.IsFalse( doc.IndexOfName(ser.TypeIDFieldName)>=0);//field was NOT added as the root type is known

      var got = new NFX.Log.Message();//pre-allocate before deserialize, as __t was not emitted
      Aver.IsNotNull( ser.Deserialize(doc, result: got) as NFX.Log.Message );
      testMsgEquWoError( msg, got);
    }


    [Test]
    public void SerializeDeserializeLogMessage_withException()
    {
      var ser = new BSONSerializer( new BSONTypeResolver(typeof(Message)) );

      var msg = new NFX.Log.Message
      {
         Type = MessageType.DebugGlue,
         TimeStamp = App.TimeSource.UTCNow,
         Channel = "MTV",
          From = "Zhaba",
           Topic = "App",
            Text = "Hello text",
             Source = 12345,
              Parameters = "aaaaa",
               ArchiveDimensions = "a=1 b=2",
                Exception =  WrappedException.ForException( new Exception("It is an error!!!") )
      };

      var doc = ser.Serialize( msg );

      Console.WriteLine( doc.ToJSON(JSONWritingOptions.PrettyPrint) );

      var got = ser.Deserialize(doc) as NFX.Log.Message;

      testMsgEquWoError( msg, got);

      Aver.IsTrue( got.Exception is WrappedException );
      Aver.AreEqual( msg.Exception.Message, got.Exception.Message );
      Aver.AreEqual( ((WrappedException)msg.Exception).Wrapped.Message, ((WrappedException)got.Exception).Wrapped.Message );
    }




    [Test]
    public void SerializeDeserializeLogMessage_withNestedException()
    {
      var ser = new BSONSerializer( new BSONTypeResolver(typeof(Message)) );

      var msg = new NFX.Log.Message
      {
         Type = MessageType.DebugGlue,
         TimeStamp = App.TimeSource.UTCNow,
         Channel = "MTV",
          From = "Zhaba",
           Topic = "App",
            Text = "Hello text in Chinese: 中原千军逐蒋",
             Source = 0,
              Parameters = new string('a', 128000),
               ArchiveDimensions = "a=1 b=2",
                Exception =  WrappedException.ForException( new Exception("It is an error!!!", new Exception("Inside")) )
      };

      var doc = ser.Serialize( msg );

      Console.WriteLine( doc.ToJSON(JSONWritingOptions.PrettyPrint) );

      var got = ser.Deserialize(doc) as NFX.Log.Message;

      testMsgEquWoError( msg, got);

      Aver.IsTrue( got.Exception is WrappedException );
      Aver.AreEqual( msg.Exception.Message, got.Exception.Message );
      Aver.AreEqual( ((WrappedException)msg.Exception).Wrapped.Message, ((WrappedException)got.Exception).Wrapped.Message );
      Aver.IsNotNull( ((WrappedException)msg.Exception).Wrapped.InnerException );

      Aver.AreEqual( ((WrappedException)msg.Exception).Wrapped.InnerException.Message, ((WrappedException)got.Exception).Wrapped.InnerException.Message );
    }







    private void testMsgEquWoError(NFX.Log.Message msg, NFX.Log.Message got)
    {
      Aver.IsNotNull( got );
      Aver.AreNotSameRef( msg, got);

      Aver.AreEqual( msg.Guid, got.Guid );
      Aver.AreEqual( msg.RelatedTo, got.RelatedTo );
      Aver.AreEqual( msg.Host, got.Host );
      Aver.AreEqual( msg.TimeStamp, got.TimeStamp );

      Aver.IsTrue( msg.Type == got.Type );
      Aver.AreEqual( msg.Channel, got.Channel );
      Aver.AreEqual( msg.From, got.From );
      Aver.AreEqual( msg.Topic, got.Topic );
      Aver.AreEqual( msg.Text, got.Text );
      Aver.AreEqual( msg.Source, got.Source );
      Aver.AreEqual( msg.Parameters, got.Parameters );
      Aver.AreEqual( msg.ArchiveDimensions, got.ArchiveDimensions );
    }

  }
}
