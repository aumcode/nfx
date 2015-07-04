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

namespace NFX.IO.FileSystem
{
    /// <summary>
    /// Supplies capabilities for the file system. The implementation must be thread safe
    /// </summary>
    public interface IFileSystemCapabilities
    {
        /// <summary>
        /// Indicates whether a file system supports versioning
        /// </summary>
        bool SupportsVersioning { get;}

        /// <summary>
        /// Indicates whether a file system supports transactions
        /// </summary>
        bool SupportsTransactions { get;}

        /// <summary>
        /// Returns maximum allowed length of the whole path that includes directory name/s and/or separator chars and/or file name
        /// </summary>
        int MaxFilePathLength { get;}

        /// <summary>
        /// Returns maximum allowed length of a file name
        /// </summary>
        int MaxFileNameLength { get;}

        /// <summary>
        /// Returns maximum allowed length of a directory name
        /// </summary>
        int MaxDirectoryNameLength { get;}


        /// <summary>
        /// Returns the maximum size of a file
        /// </summary>
        ulong MaxFileSize { get;}

        /// <summary>
        /// Returns understood path separator characters
        /// </summary>
        char[] PathSeparatorCharacters { get;}

        /// <summary>
        /// Indicates whether file system supports modification of its files and structure
        /// </summary>
        bool IsReadonly { get;}

        /// <summary>
        /// Indicates whether the file system supports security permissions
        /// </summary>
        bool SupportsSecurity { get;}

        /// <summary>
        /// Indicates whether the file system supports custom metadata for files and folders
        /// </summary>
        bool SupportsCustomMetadata { get;}

        bool SupportsDirectoryRenaming { get;}
        bool SupportsFileRenaming { get; }

        bool SupportsStreamSeek { get;}

        bool SupportsFileModification { get;}

        bool SupportsCreationTimestamps     { get;}
        bool SupportsModificationTimestamps { get;}
        bool SupportsLastAccessTimestamps   { get;}

        bool SupportsReadonlyDirectories   { get;}
        bool SupportsReadonlyFiles         { get;}


        bool SupportsCreationUserNames     { get;}
        bool SupportsModificationUserNames { get;}
        bool SupportsLastAccessUserNames   { get;}

        bool SupportsFileSizes      { get;}
        bool SupportsDirectorySizes { get;}

        /// <summary>
        /// Defines if this FileSystem implements Async methods in real asynchronous manner.
        /// By default asynchronous methods are actually executed syncronously and return Task with execution result or exception
        /// </summary>
        bool SupportsAsyncronousAPI { get;}
    }

}
