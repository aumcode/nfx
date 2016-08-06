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


namespace NFX.CodeAnalysis.Source
{
      /// <summary>
      /// Represents a reference to the source code which may be named buffer or project source item (i.e. solution project item)
      /// </summary>
      public struct SourceCodeRef
      {
            /// <summary>
            /// Provides name for the source, this property is set to ProjectItem.Name when IProjectItem is supplied in .ctor
            /// </summary>
            public readonly string SourceName;

            /// <summary>
            /// References project source item, this property may be null
            /// </summary>
            public readonly IProjectItem ProjectItem;



            public SourceCodeRef(string srcName)
            {
              SourceName = srcName ?? CoreConsts.UNNAMED_MEMORY_BUFFER;
              ProjectItem = null;
            }

            public SourceCodeRef(IProjectItem srcItem)
            {
              ProjectItem = srcItem;
              SourceName = srcItem.Name ?? CoreConsts.UNNAMED_PROJECT_ITEM;
            }


            public override string ToString()
            {
              return SourceName;
            }


      }


}