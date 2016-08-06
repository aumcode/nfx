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


/* NFX by ITAdapter
 * Originated: 2006.02
 * Revision: NFX 1.0  2011.02.12
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.IO;

namespace NFX.Environment
{
  /// <summary>
  /// Provides implementation of configuration based on a classic XML content
  /// </summary>
  [Serializable]
  public class XMLConfiguration : FileConfiguration
  {
    #region .ctor / static

        /// <summary>
        /// Creates an instance of a new configuration not bound to any XML file
        /// </summary>
        public XMLConfiguration() : base()
        {

        }

        /// <summary>
        /// Creates an isntance of the new configuration and reads contents from an XML file
        /// </summary>
        public XMLConfiguration(string filename) : base(filename)
        {
          readFromFile();
        }

        /// <summary>
        /// Creates an instance of configuration initialized from XML content passed as string
        /// </summary>
        public static XMLConfiguration CreateFromXML(string content)
        {
          var result = new XMLConfiguration();
          result.readFromString(content);

          return result;
        }


    #endregion


    #region Public Properties

    #endregion


    #region Public

              /// <summary>
              /// Saves configuration into a file
              /// </summary>
              public override void SaveAs(string filename)
              {
                SaveAs(filename, null);

                base.SaveAs(filename);
              }

              /// <summary>
              /// Saves configuration to a file with optional link to XSL file
              /// </summary>
              public void SaveAs(string filename, string xsl)
              {
                if (string.IsNullOrEmpty(filename))
                  throw new ConfigException(StringConsts.CONFIGURATION_FILE_UNKNOWN_ERROR);

                var doc = buildXmlDoc(xsl);
                doc.Save(filename);
              }


              /// <summary>
              /// Saves XML configuration with optional link to XSL file, into string and returns it
              /// </summary>
              public string SaveToString(string xsl =null)
              {
                var doc = buildXmlDoc(xsl);
                using(var writer = new StringWriter())
                {
                  doc.Save(writer);
                  return writer.ToString();
                }
              }


            public override void Refresh()
            {
              readFromFile();
            }


            public override void Save()
            {
              SaveAs(m_FileName);
            }

            public override string ToString()
            {
                XmlDocument doc = new XmlDocument();

                buildDocNode(doc, null, m_Root);

                return doc.OuterXml;
            }



    #endregion

    #region Protected

         //for XML we only allow printable chars and 0..9 and - or _ .
          protected override string AdjustNodeName(string name)
          {
              var result = new StringBuilder(32);//average id size is 16-20 chars

              foreach(var c in name)
               if (char.IsLetterOrDigit(c) || c=='_' || c=='.')
                result.Append(c);
               else
                result.Append('-');

              return result.ToString();
          }

    #endregion

    #region Private Utils

        private void readFromFile()
        {
          XmlDocument doc = new XmlDocument();

          doc.Load(m_FileName);

          read(doc);
        }

        private void readFromString(string content)
        {
          XmlDocument doc = new XmlDocument();

          doc.LoadXml(content);

          read(doc);
        }

        private void read(XmlDocument doc)
        {
          m_Root = buildNode(doc.DocumentElement, null);
          if (m_Root!=null)
            m_Root.ResetModified();
          else
            m_Root = m_EmptySectionNode;
        }


        private ConfigSectionNode buildNode(XmlNode xnode, ConfigSectionNode parent)
        {
          ConfigSectionNode result;

          if (xnode.NodeType == XmlNodeType.Text && parent != null)
          {
            parent.Value = xnode.Value;
            return null;
          }

          if (parent != null)
            result = parent.AddChildNode(xnode.Name, string.Empty);
          else
            result = new ConfigSectionNode(this, null, xnode.Name, string.Empty);

          if (xnode.Attributes != null)
            foreach (XmlAttribute xattr in xnode.Attributes)
              result.AddAttributeNode(xattr.Name, xattr.Value);


          foreach (XmlNode xn in xnode)
           if (xn.NodeType != XmlNodeType.Comment)
             buildNode(xn, result);

          return result;
        }



        private XmlDocument buildXmlDoc(string xsl)
        {
          var doc = new XmlDocument();

          //insert XSL link
          if (!string.IsNullOrEmpty(xsl))
          {
            var decl = doc.CreateXmlDeclaration("1.0", null, null);
            doc.AppendChild(decl);
            var link = doc.CreateProcessingInstruction(
                               "xml-stylesheet",
                               "type=\"text/xsl\" href=\"" + xsl + "\"");
            doc.AppendChild(link);
          }

          buildDocNode(doc, null, m_Root);

          return doc;
        }



        private void buildDocNode(XmlDocument doc, XmlNode xnode, ConfigSectionNode node)
        {
          XmlNode xnew = doc.CreateElement(node.Name);

          if (xnode != null)
            xnode.AppendChild(xnew);
          else
            doc.AppendChild(xnew);

          foreach (ConfigAttrNode anode in node.Attributes)
          {
            XmlNode xattr = doc.CreateNode(XmlNodeType.Attribute, anode.Name, string.Empty);
            xattr.Value = anode.Value;
            xnew.Attributes.SetNamedItem(xattr);
          }


          if (node.HasChildren)
          {
            foreach (ConfigSectionNode cnode in node.Children)
              buildDocNode(doc, xnew, cnode);
          }
          else
          {
            xnew.AppendChild(doc.CreateTextNode(node.Value));
          }
        }


    #endregion
  }
}
