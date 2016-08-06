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

using System.Collections.Generic;

namespace NFX.CodeAnalysis.Source
{

      /// <summary>
      /// Represents source code input text (usually text from file)
      /// </summary>
      public interface ISourceText
      {
            /// <summary>
            /// Resets source to beginning
            /// </summary>
            void Reset();


            /// <summary>
            /// Indicates whether last character has been read
            /// </summary>
            bool EOF { get; }

            /// <summary>
            /// Returns next char and advances position
            /// </summary>
            char ReadChar();


            /// <summary>
            /// Returns next char without advancing position
            /// </summary>
            char PeekChar();


            /// <summary>
            /// Indicates what language this source is supplied in
            /// </summary>
            Language Language { get; }

            /// <summary>
            /// Provides a meaningfull name to a source code
            /// </summary>
            string Name { get; }
      }

      /// <summary>
      /// Represents a list of strings used as source text
      /// </summary>
      public class ListOfISourceText : List<ISourceText>
      {

      }

}
