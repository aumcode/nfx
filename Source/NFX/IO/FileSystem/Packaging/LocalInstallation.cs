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
using System.IO;


using NFX.Environment;

namespace NFX.IO.FileSystem.Packaging
{

  /// <summary>
  /// Represents the local installation - facilitates working with locally installed packages
  /// </summary>
  public class LocalInstallation : DisposableObject
  {
      /// <summary>
      /// Provides package descriptor
      /// </summary>
      public class PackageInfo
      {
        /// <summary>Provides package descriptor</summary>
        /// <param name="name">Mnemonic name of the package (i.e. application name)</param>
        /// <param name="source">Source directory where to take files from</param>
        /// <param name="relPath">Relative path which is appended to the root path where files will be placed</param>
        public PackageInfo(string name, FileSystemDirectory source, string relPath)
        {
          Name = name ?? CoreConsts.UNKNOWN;
          Source = source;
          RelativePath = relPath;

          ConfigSectionNode manifest = null;
          var mFile = source.GetFile(ManifestUtils.MANIFEST_FILE_NAME);
          if (mFile!=null)
               try
               {
                 manifest = LaconicConfiguration.CreateFromString(mFile.ReadAllText()).Root;
               }
               catch(Exception error)
               {
                 throw new NFXIOException(StringConsts.LOCAL_INSTALL_INSTALL_SET_PACKAGE_MANIFEST_READ_ERROR.Args(Name, error.ToMessageWithType()), error);
               }

          if (manifest==null)
           throw new NFXIOException(StringConsts.LOCAL_INSTALL_INSTALL_SET_PACKAGE_WITHOUT_MANIFEST_ERROR.Args(Name, ManifestUtils.MANIFEST_FILE_NAME));

          manifest.AttrByName(ManifestUtils.CONFIG_NAME_ATTR, true).Value = name;
          manifest.AttrByName(ManifestUtils.CONFIG_LOCAL_PATH_ATTR, true).Value = relPath;
          manifest.ResetModified();

          Manifest = manifest;
        }

        public readonly string Name;
        public readonly FileSystemDirectory Source;
        public readonly string RelativePath;
        public readonly IConfigSectionNode Manifest;

        public override int GetHashCode()
        {
          return Name.GetHashCodeIgnoreCase();
        }

        public override bool Equals(object obj)
        {
          var other = obj as PackageInfo;
          if (other==null) return false;
          return this.Name.EqualsIgnoreCase(other.Name);
        }

        public override string ToString()
        {
          return "['{0}']{1}".Args(Name, RelativePath);
        }
      }


      /// <summary>
      /// Initializes local installation, tries to read local manifest from rootPath or localManifestDir if it is !=null
      /// </summary>
      public LocalInstallation(string rootPath, string localManifestDir = null)
      {
        if (rootPath.IsNullOrWhiteSpace())
          throw new NFXIOException(StringConsts.ARGUMENT_ERROR + GetType().Name + ".ctor(rootPath==null|empty)");

        var parent = Directory.GetParent(rootPath);
        if (!parent.Exists)
          throw new NFXIOException(StringConsts.LOCAL_INSTALL_ROOT_PATH_NOT_FOUND_ERROR.Args(parent.FullName));

        m_RootPath = rootPath;

        var manifestDir = localManifestDir.IsNotNullOrWhiteSpace() ? localManifestDir : m_RootPath;

        if (Directory.Exists(manifestDir))
        {
          var fn = Path.Combine(manifestDir, ManifestUtils.MANIFEST_FILE_NAME);
          if (File.Exists(fn))
          try
          {
             m_Packages = new LaconicConfiguration(fn).Root;
          }
          catch(Exception error)
          {
            throw new NFXIOException(StringConsts.LOCAL_INSTALL_LOCAL_MANIFEST_READ_ERROR.Args(fn, error.ToMessageWithType()), error);
          }
        }

        if (m_Packages==null)
        {
          var cfg = new LaconicConfiguration();
          cfg.Create(ManifestUtils.CONFIG_PACKAGES_SECTION);
          m_Packages = cfg.Root;
        }
      }

      protected override void Destructor()
      {
          EndInstallation();
      }

      private bool m_InstallationStarted;
      private string m_RootPath;
      private ConfigSectionNode m_Packages;
      private bool m_Modified;

      /// <summary>
      /// Root path of the installation
      /// </summary>
      public string RootPath { get{return m_RootPath;}}

      /// <summary>
      /// Returns true to indicate that BeginInstallation() has been called
      /// </summary>
      public bool InstallationStarted { get{return m_InstallationStarted;}}

      /// <summary>
      /// Returns true to indicate that local installation has changed as the result of package installation
      /// </summary>
      public bool Modified { get{return m_Modified;}}


      /// <summary>
      /// Gets package names
      /// </summary>
      public IEnumerable<string> PackageNames
      {
        get
        {
          return m_Packages.Children.Where(cn=>cn.IsSameName(ManifestUtils.CONFIG_PACKAGE_SECTION))
                                                 .Select(n=>n.AttrByName(Configuration.CONFIG_NAME_ATTR)
                                                             .ValueAsString(string.Empty));
        }
      }

      /// <summary>
      /// Gets package manifests
      /// </summary>
      public IEnumerable<IConfigSectionNode> PackageManifests
      {
        get { return m_Packages.Children.Where(cn=>cn.IsSameName(ManifestUtils.CONFIG_PACKAGE_SECTION));}
      }

      /// <summary>
      /// Returns installed package manifest by name or null
      /// </summary>
      public IConfigSectionNode this[string name]
      {
        get { return m_Packages.Children.FirstOrDefault(cn=>cn.IsSameName(ManifestUtils.CONFIG_PACKAGE_SECTION) && cn.IsSameNameAttr(name));}
      }

      /// <summary>
      /// Starts the installation so InstallPackage() can be called
      /// </summary>
      public void BeginInstallation()
      {
        if (m_InstallationStarted) return;

        IOMiscUtils.EnsureDirectoryDeleted(m_RootPath);
        IOMiscUtils.EnsureAccessibleDirectory(m_RootPath);

        m_InstallationStarted = true;

      }

      /// <summary>
      /// Unconditionally installs a package - copies a set of files contained in the FileSystemDirectory assigning it some mnemonic name
      /// </summary>
      public void InstallPackage(PackageInfo package)
      {
        if (!m_InstallationStarted)
         throw new NFXIOException(StringConsts.LOCAL_INSTALL_NOT_STARTED_INSTALL_PACKAGE_ERROR);

        var path = m_RootPath;
        if (package.RelativePath.IsNotNullOrWhiteSpace())
        {
         path = Path.Combine(path, package.RelativePath);
         IOMiscUtils.EnsureAccessibleDirectory(path);
        }

        var packageName = package.Name;
        var source = package.Source;
        var manifest = package.Manifest;

        using(var lfs = new Local.LocalFileSystem(null))
         using(var fss = lfs.StartSession(null))
         {
           var targetDir = fss[path] as FileSystemDirectory;
           if (targetDir==null)
             throw new NFXIOException(StringConsts.LOCAL_INSTALL_ROOT_PATH_NOT_FOUND_ERROR.Args(path));

           source.DeepCopyTo(targetDir, FileSystemDirectory.DirCopyFlags.FilesAndDirsOnly,
              filter: (item) =>
              {
                var file = item as FileSystemFile;
                if (file==null) return true;

                if (file.ParentPath==source.Path && file.Name == ManifestUtils.MANIFEST_FILE_NAME) return false;

                return true;
              }

            );
         }

        var existing = this[packageName];
        if (existing!=null) ((ConfigSectionNode)existing).Delete();
        m_Packages.AddChildNode(manifest);

        m_Modified = true;
      }

      /// <summary>
      /// Updates local installation manifest if changes have been made (Modified=true)
      /// </summary>
      public void EndInstallation()
      {
        if (!m_InstallationStarted) return;
        if (!m_Modified) return;

        var fn = Path.Combine(m_RootPath, ManifestUtils.MANIFEST_FILE_NAME);
        if (File.Exists(fn))
           throw new NFXIOException(StringConsts.LOCAL_INSTALL_PACKAGES_MANIFEST_FILE_NAME_COLLISION_ERROR.Args(fn));

        ((LaconicConfiguration)m_Packages.Configuration).SaveAs(fn);

        m_Modified = false;
        m_InstallationStarted = false;
      }


      /// <summary>
      /// Finds a package form the install set which is either missing on local machine or is not the same as the one in installSet.
      /// Returns the package info from install set
      /// </summary>
      public PackageInfo FindMissingOrDifferentPackage(IEnumerable<PackageInfo> installSet)
      {
        foreach(var pi in installSet)
        {
          var local = this[pi.Name];
          if (local==null) return pi;//not found locally
          if (!local.HasTheSameContent( pi.Manifest)) return pi;//different content
        }
        return null;
      }


      /// <summary>
      /// Checks local installation first for missing of different packages and if there are no differences then returns false,
      /// otherwise re-installs all packages defined by in install-set locally and returns true.
      /// Pass force=true to re-install regardless of manifest comparison (false by default)
      /// </summary>
      public bool CheckLocalAndInstallIfNeeded(IEnumerable<PackageInfo> installSet, bool force = false)
      {
        if (!force)
        {
          var difference = FindMissingOrDifferentPackage(installSet);
          if (difference==null) return false;//nothing to install
        }

        BeginInstallation();
        try
        {
          installSet.ForEach(pi => InstallPackage(pi));
        }
        finally
        {
          EndInstallation();
        }

        return true;
      }

  }
}
