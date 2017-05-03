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


/* NFX by ITAdapter
 * Originated: 2006.01
 * Revision: NFX 0.2  2009.07.03
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace delbydate
{
  class Program
  {
    static void Main(string[] args)
    {
      if ((args.Length == 0) || ((args.Length > 0) && (args[0] == "/?")))
      {
        ConsoleColor cForeground = Console.ForegroundColor;
        ConsoleColor cBackground = Console.BackgroundColor;

        Console.ForegroundColor = ConsoleColor.White;
        Console.BackgroundColor = ConsoleColor.Black;
        Console.WriteLine();
        Console.WriteLine("Delete by Date Utility");
        Console.WriteLine("Deletes file(s) in date range");
        Console.ForegroundColor = ConsoleColor.Gray;
        Console.WriteLine("Copyright (c) 2007 - 2009 D.Khmaladze / IT Adapter Inc.");
        Console.WriteLine("Version 2.0 / NFX as of July 3rd 2009");
        Console.WriteLine();
        Console.ForegroundColor = ConsoleColor.Magenta;
        Console.WriteLine(" Usage:");
        Console.ForegroundColor = ConsoleColor.Gray;
        Console.WriteLine("    delbydate \"path\" \"expr\" ");
        Console.WriteLine("  path - wildcards permitted ");
        Console.WriteLine("  expr - standard C# date time expression. file - is the reference to current file date time");
        Console.WriteLine();
        Console.ForegroundColor = ConsoleColor.Magenta;
        Console.WriteLine(" Example:");
        Console.WriteLine();
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("  delbydate \"c:\\temp\\*.bak\"  \"((file.Hour==5)||(file<DateTime.Now.AddMonths(-6)))\"  ");
        Console.ForegroundColor = ConsoleColor.Gray;
        Console.WriteLine("    This will delete any file that has .BAK extension and was written to last time either at 5 am or over 6 month ago");


        Console.ForegroundColor = cForeground;
        Console.BackgroundColor = cBackground;

        return;
      }//help

      try
      {
        if (args.Length < 2) throw new Exception("Invalid command syntax");

        string path = args[0].Trim();
        string expr = args[1].Trim();

        if (path.Length == 0) path = System.IO.Directory.GetCurrentDirectory();


        string dir = Path.GetDirectoryName(path);
        string msk = Path.GetFileName(path);

        if (msk.Length == 0) msk = "*";

        string[] files = System.IO.Directory.GetFiles(dir, msk);

        DateParser dp = new DateParser(expr);

        for (int i = 0; i < files.Length; i++)
          if (dp.Eval(File.GetLastWriteTime(files[i])))
          {
            Console.WriteLine("Deleting: " + files[i]);
            try
            {
              File.Delete(files[i]);
            }
            catch (Exception err)
            {
              Console.WriteLine("Error deleting " + files[i] + " " + err.Message);
            }

          }//match


        Environment.ExitCode = 0;
      }
      catch (Exception err)
      {
        Console.WriteLine("Error: " + err.Message);
        Environment.ExitCode = -1;
        return;
      }//catch


    }//Main
  }
}
