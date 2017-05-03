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

namespace NFX.CodeAnalysis.Source
{


  /// <summary>
  /// Represents position in source input
  /// </summary>
  public struct SourcePosition
  {
    public static readonly SourcePosition UNASSIGNED = new SourcePosition(-1, -1, -1);


    public readonly int LineNumber;
    public readonly int ColNumber;
    public readonly int CharNumber;

    public bool IsAssigned
    {
        get { return LineNumber >=0 && ColNumber>=0 && CharNumber >=0; }
    }

    public SourcePosition(int lineNum, int colNum, int charNum)
    {
      LineNumber = lineNum;
      ColNumber = colNum;
      CharNumber = charNum;
    }

    public override string ToString()
    {
      return IsAssigned ?  "Line: {0} Col: {1} Char: {2}".Args(LineNumber, ColNumber, CharNumber) : string.Empty;
    }

  }

}
