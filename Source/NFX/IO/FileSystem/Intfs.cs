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

using NFX.Security;
using NFX.ApplicationModel;

namespace NFX.IO.FileSystem
{
    /// <summary>
    /// Stipulates contract for various file system abstractions.
    /// FileSystem abstractions are mostly useful for working with components/classes that may need to inter-operate not only with local file system
    ///  but also with distributed systems like ApacheHDFS, SVN, GIT, or Aum Cluster File System (ACFS).
    ///  NFX library provides compatibility wrapper 'NFX.IO.FileSystem.Local.LocalFileSystem' for access to local machine file system
    ///  (which is based on this class and is implemented using a traditional System.IO.* set of classes).
    /// The FileSystem abstraction supports the following concepts: versioning, transactions, metadata, security; however it does not guarantee that
    ///  every implementation is capable of providing all of these functions. Query "GeneralCapabilities" and "InstanceCapabilities" to see what functions
    ///   are supported by a particular instance.
    /// </summary>
    public interface IFileSystem : IApplicationComponent, INamed, IDisposable
    {
        /// <summary>
        /// Returns a list of sessions
        /// </summary>
        IEnumerable<FileSystemSession> Sessions { get; }

        /// <summary>
        /// Returns capabilities for this file system in general
        /// </summary>
        IFileSystemCapabilities GeneralCapabilities { get;}

        /// <summary>
        /// Returns capabilities for this file system instance, that may or may not be the same as GeneralCapabilities
        /// </summary>
        IFileSystemCapabilities InstanceCapabilities { get;}


        /// <summary>
        /// Creates a new session for the specified user and version
        /// </summary>
        FileSystemSession StartSession(FileSystemSessionConnectParams cParams);

       /// <summary>
        /// Combines two or more path segments joining them using primary file system path separator
        /// </summary>
        string CombinePaths(string first, params string[] others);
    }

    /// <summary>
    /// Decorates entities that represent version for file systems that support versioning
    /// </summary>
    public interface IFileSystemVersion : INamed
    {
    }

    /// <summary>
    /// Denotes a handle for item in a file system.
    /// For example: in a distributed system this may be some form of unique file/directory id (i.e. a GUID) or
    ///  object instance internal to implementation
    /// </summary>
    public interface IFileSystemHandle
    {

    }


    /// <summary>
    /// Denotes a handle for transaction in a file system.
    /// For example: in a distributed system this may be an object instance holding information about transaction which is internal to implementation
    /// </summary>
    public interface IFileSystemTransactionHandle
    {

    }


}
