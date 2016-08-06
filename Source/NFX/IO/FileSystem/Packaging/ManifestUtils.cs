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

using NFX.Environment;
using NFX.IO.ErrorHandling;

namespace NFX.IO.FileSystem.Packaging
{

  /// <summary>
  /// Provides utilities for manifest generation
  /// </summary>
  public static class ManifestUtils
  {
      #region CONSTS
        public const string MANIFEST_FILE_NAME = "packaging-manifest.pm";

        public const string CONFIG_PACKAGES_SECTION = "packages";
        public const string CONFIG_PACKAGE_SECTION = "package";

        public const string CONFIG_DIR_SECTION = "dir";
        public const string CONFIG_FILE_SECTION = "file";

        public const string CONFIG_NAME_ATTR = Configuration.CONFIG_NAME_ATTR;
        public const string CONFIG_SIZE_ATTR = "size";
        public const string CONFIG_CSUM_ATTR = "csum";
        public const string CONFIG_LOCAL_PATH_ATTR = "local-path";

      #endregion


      /// <summary>
      /// Generates packaging manifest for the specified directory. Optionally may specify root node name
      /// </summary>
      /// <param name="directory">Source directory to generate manifest for</param>
      /// <param name="rootNodeName">Name of root manifest node, if omitted then 'package' is defaulted</param>
      /// <param name="packageName">Optional 'name' attribute value under root node</param>
      /// <param name="packageLocalPath">Optional 'local-path' attribute value under root node</param>
      public static ConfigSectionNode GeneratePackagingManifest(this FileSystemDirectory directory,
                                                                string rootNodeName = null,
                                                                string packageName = null,
                                                                string packageLocalPath = null)
      {
        if (directory==null)
         throw new NFXIOException(StringConsts.ARGUMENT_ERROR + "GeneratePackagingManifest(directory==null)");

        var conf = new MemoryConfiguration();
        conf.Create(rootNodeName.IsNullOrWhiteSpace()?CONFIG_PACKAGE_SECTION : rootNodeName);
        var root = conf.Root;
        if (packageName.IsNotNullOrWhiteSpace())
          root.AddAttributeNode(CONFIG_NAME_ATTR, packageName);

        if (packageLocalPath.IsNotNullOrWhiteSpace())
          root.AddAttributeNode(CONFIG_LOCAL_PATH_ATTR, packageLocalPath);

        buildDirLevel(root, directory);

        root.ResetModified();

        return root;
      }

      /// <summary>
      /// Returns true when both config nodes represents the same manifest - that is the same file structure
      /// </summary>
      /// <param name="master">Master sample copy</param>
      /// <param name="comparand">The second manifest being compared to the master</param>
      /// <param name="oneWay">If true iterates on master, so extra files in comparand will not be detected. False by default in whoch case iterates on master first then on comparand</param>
      /// <returns>True when comparand has all files/directories that the master lists</returns>
      public static bool HasTheSameContent(this IConfigSectionNode master, IConfigSectionNode comparand, bool oneWay = false)
      {
        if (!hasTheSameContent(master, comparand)) return false;
        if (oneWay) return true;

        return hasTheSameContent(comparand, master);
      }

      private static bool hasTheSameContent(IConfigSectionNode master, IConfigSectionNode comparand)
      {
        if (master==null | comparand==null)
         throw new NFXIOException(StringConsts.ARGUMENT_ERROR + "HasTheSameContent(master|comparand==null)");

        foreach(var mnode in master.Children)
        {
          var cnode = comparand.Children.FirstOrDefault(cn=>cn.IsSameName(mnode) && cn.IsSameNameAttr(mnode));
          if (cnode==null) return false;
          if (!mnode.HasTheSameContent(cnode)) return false;

          if (mnode.IsSameName(CONFIG_FILE_SECTION))
          {
            if (mnode.AttrByName(CONFIG_SIZE_ATTR).ValueAsLong() != cnode.AttrByName(CONFIG_SIZE_ATTR).ValueAsLong() ||
                mnode.AttrByName(CONFIG_CSUM_ATTR).ValueAsLong() != cnode.AttrByName(CONFIG_CSUM_ATTR).ValueAsLong()) return false;
          }
        }

        return true;
      }


      private static void buildDirLevel(ConfigSectionNode pNode, FileSystemDirectory directory)
      {
        const int BUFF_SIZE = 64 * 1024;

        foreach(var sdn in directory.SubDirectoryNames)
          using(var sdir = directory.GetSubDirectory(sdn))
          {
            var dnode = pNode.AddChildNode(CONFIG_DIR_SECTION);
            dnode.AddAttributeNode(CONFIG_NAME_ATTR, sdir.Name);
            buildDirLevel(dnode, sdir);
          }

        foreach(var fn in directory.FileNames.Where(fn => !MANIFEST_FILE_NAME.EqualsIgnoreCase(fn)))
          using(var file = directory.GetFile(fn))
          {
            var fnode = pNode.AddChildNode(CONFIG_FILE_SECTION);
            fnode.AddAttributeNode(CONFIG_NAME_ATTR, file.Name);

            long size = 0;
            var csum = new Adler32();
            var buff = new byte[BUFF_SIZE];
            using(var fs = file.FileStream)
              while(true)
              {
                var read = fs.Read(buff, 0, BUFF_SIZE);
                if (read<=0) break;
                size += read;
                csum.Add(buff, 0, read);
              }

            fnode.AddAttributeNode(CONFIG_SIZE_ATTR, size);
            fnode.AddAttributeNode(CONFIG_CSUM_ATTR, csum.Value);
          }

      }

  }
}
