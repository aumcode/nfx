/*<FILE_LICENSE>
* NFX (.NET Framework Extension) Unistack Library
* Copyright 2003-2018 Agnicore Inc. portions ITAdapter Corp. Inc.
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
using System.Diagnostics;

using NFX;
using NFX.IO;
using NFX.Environment;
using NFX.Security;
using NFX.ApplicationModel;
using NFX.Serialization.JSON;


namespace phash
{
    class Program
    {
        static void Main(string[] args)
        {
          try
          {

           using(var app = new ServiceBaseApplication(args, null))
            run(app.CommandArgs);

           Environment.ExitCode = 0;
          }
          catch(Exception error)
          {
           ConsoleUtils.Error(error.ToMessageWithType());
           Environment.ExitCode = -1;
          }
        }

        private static void run(IConfigSectionNode args)
        {
          var pretty = args["pp", "pretty"].Exists;
          var noEntropy = args["ne", "noentropy"].Exists;
          var scoreThreshold = args["st", "score"].AttrByIndex(0).ValueAsInt(80);
          if (scoreThreshold<20) scoreThreshold = 20;
          if (scoreThreshold>100) scoreThreshold = 100;
          var strength = args["lvl","level"].AttrByIndex(0).ValueAsEnum<PasswordStrengthLevel>(PasswordStrengthLevel.Default);

          ConsoleUtils.WriteMarkupContent( typeof(Program).GetText("Welcome.txt") );

          if (args["?", "h", "help"].Exists)
          {
             ConsoleUtils.WriteMarkupContent( typeof(Program).GetText("Help.txt") );
             return;
          }

          ConsoleUtils.Info("Score Threshold: {0}%".Args(scoreThreshold));
          ConsoleUtils.Info("Stength level: {0}".Args(strength));

          if (!noEntropy)
          {
             var count = ExternalRandomGenerator.Instance.NextScaledRandomInteger(47, 94);
             ConsoleUtils.Info("Acquiring entropy from user...");
             Console.WriteLine();
             ConsoleUtils.WriteMarkupContent(
@"<push>
<f color=magenta>Please make <f color=white>{0}<f color=magenta> random keystrokes
Do not hit the same key and try to space key presses in time:<pop>
".Args(count));

             var pnow = Stopwatch.GetTimestamp();

             Console.WriteLine();
             for(var i=0; i<count;i++)
             {
               var k = Console.ReadKey(true).KeyChar;
               if (k<0x20) continue;
               var now = Stopwatch.GetTimestamp();
               var elapsed = (int)(39621 * (k - 0x19) * (now - pnow));
               pnow = now;
               ExternalRandomGenerator.Instance.FeedExternalEntropySample(elapsed);
               Console.Write("\r{0}  {1} characters to go ...", elapsed, count-i-1);
             }
             ConsoleUtils.Info("OK. Entropy key entered");
             Console.WriteLine("-----------------------");
             System.Threading.Thread.Sleep(3000);
             while( Console.KeyAvailable) Console.ReadKey(true);
          }

          SecureBuffer password = null;

          while(true)
          {
             Console.WriteLine("Please type-in your password and press <enter>:");
             password = ConsoleUtils.ReadPasswordToSecureBuffer('*');
             var score = App.SecurityManager.PasswordManager.CalculateStrenghtPercent(PasswordFamily.Text, password);
             var pass = score >= scoreThreshold;
             Console.WriteLine();
             var t = "Password score: {0}% is {1} strong".Args(score, pass ? "sufficiently" : "insufficiently");
             if (pass)
             {
               ConsoleUtils.Info(t);
               break;
             }

             ConsoleUtils.Error(t);
             Console.WriteLine();
          }

          Console.WriteLine();

          while(true)
          {
            Console.WriteLine("Please re-type your password and press <enter>:");
            using(var p2 = ConsoleUtils.ReadPasswordToSecureBuffer('*'))
              if (password.Content.MemBufferEquals(p2.Content)) break;
            ConsoleUtils.Error("Passwords do not match");
          }

          Console.WriteLine();
          Console.WriteLine();

          var hashed = App.SecurityManager.PasswordManager.ComputeHash(
                                    NFX.Security.PasswordFamily.Text,
                                    password,
                                    strength);

          password.Dispose();

          var toPrint = JSONWriter.Write(hashed, pretty ? JSONWritingOptions.PrettyPrintASCII : JSONWritingOptions.CompactASCII);

          Console.WriteLine("Hashed Password:");
          Console.WriteLine();

          Console.WriteLine( toPrint );
        }
    }
}
