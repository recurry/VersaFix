using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace VfxEngine.Fix
{
    /// <summary>
    /// The QfxFixDxExporter class provides an implementation
    /// of the interface IFixDxExporter for exporting the contents
    /// of a VersaFix data dictionary to format that is compatible
    /// with the QuickFix system.
    /// </summary>
    public class QfxFixDxExporter : IFixDxExporter
    {
        #region IFixDxExporter Members

        /// <summary>
        /// The Export method is invoked to export the contents
        /// of a VersaFix data dictionary out to a file in a format
        /// that is compatible with the QuickFix system.
        /// </summary>
        /// <param name="dictionary">
        /// The data dictionary that is to be exported.
        /// </param>
        /// <param name="path">
        /// The file the dictionary's data is to be saved to.
        /// </param>
        public void Export(FixDictionary dictionary, string path)
        {
            FileStream fs = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.None);
            Export(dictionary, fs);
            fs.Close();
        }

        /// <summary>
        /// The Export method is invoked to export the contents
        /// of a VersaFix data dictionary out to a stream in a format
        /// that is compatible with the QuickFix system.
        /// </summary>
        /// <param name="dictionary">
        /// The data dictionary that is to be exported.
        /// </param>
        /// <param name="stream">
        /// The stream the dictionary is to be exported to.
        /// </param>
        public void Export(FixDictionary dictionary, Stream stream)
        {
            // REC: Convert the supplied VersaFix data dictionary
            // into its corresponding representation as an instance
            // of an QuickFix data dictionary:
            XmlQfxDictionary xmlDictionary = new XmlQfxDictionary();

            // REC: Pull the metadata elements that correspond
            // to the QuickFIX attributes for the root element
            // and assign them to the appropriate fields:
            xmlDictionary.Type = "FIX";
            if (dictionary.Properties.ContainsKey("Type"))
            {
                xmlDictionary.Type = dictionary.Properties["Type"];
            }

            xmlDictionary.Major = "0";
            if (dictionary.Properties.ContainsKey("Major"))
            {
                xmlDictionary.Major = dictionary.Properties["Major"];
            }
            else if (dictionary.Properties.ContainsKey("Fix.Major"))
            {
                xmlDictionary.Major = dictionary.Properties["Fix.Major"];
            }

            xmlDictionary.Minor = "0";
            if (dictionary.Properties.ContainsKey("Minor"))
            {
                xmlDictionary.Minor = dictionary.Properties["Minor"];
            }
            else if (dictionary.Properties.ContainsKey("Fix.Minor"))
            {
                xmlDictionary.Minor = dictionary.Properties["Fix.Minor"];
            }

            // REC: Convert the header elements:
            ExportHeader(dictionary, xmlDictionary);

            // REC: Convert all of the fields:
            ExportFields(dictionary, xmlDictionary);

            // REC: Convert all of the blocks:
            ExportBlocks(dictionary, xmlDictionary);

            // REC: Convert all of the messages:
            ExportMessages(dictionary, xmlDictionary);

            // REC: Convert the trailer elements:
            ExportTrailer(dictionary, xmlDictionary);

            XmlSerializer xs = new XmlSerializer(typeof(XmlQfxDictionary));
            XmlWriterSettings xwSettings = new XmlWriterSettings();
            xwSettings.Indent = true;
            xwSettings.OmitXmlDeclaration = true;
            XmlSerializerNamespaces xsn = new XmlSerializerNamespaces();
            xsn.Add("", "");

            XmlWriter writer = XmlWriter.Create(stream, xwSettings);
            xs.Serialize(writer, xmlDictionary, xsn);
            return;
        }

        #endregion

        /// <summary>
        /// The ExportHeader method is invoked to export all of the
        /// elements in the header section of a VersaFix dictionary
        /// out to an XML QuickFix dictionary.
        /// </summary>
        /// <param name="src">
        /// The source dictionary for the header elements.
        /// </param>
        /// <param name="dst">
        /// The target dictionary for the header elements.
        /// </param>
        private void ExportHeader(FixDictionary src, XmlQfxDictionary dst)
        {
            foreach (IFixDxElement dxElement in src.Header)
            {
                ExportElement(dxElement, dst.Header);
            }
        }

        /// <summary>
        /// The ExportFields method is invoked to convert all of
        /// the field elements in an instance of a VersaFix data
        /// dictionary into corresponding entries in an instance
        /// of an XML representation of a QuickFix dictionary.
        /// </summary>
        /// <param name="src">
        /// The source dictionary for the field definitions.
        /// </param>
        /// <param name="dst">
        /// The target dictionary for the field definitions.
        /// </param>
        private void ExportFields(FixDictionary src, XmlQfxDictionary dst)
        {
            foreach (IFixDxElement dxElement in src.Fields)
            {
                FixDxField dxField = dxElement as FixDxField;
                if (dxField != null)
                {
                    XmlQfxField xmlField = new XmlQfxField();
                    xmlField.Name = dxField.Name;
                    xmlField.Number = dxField.Tag.ToString();
                    xmlField.Type = dxField.Type;

                    // REC: QuickFix stores the enumerators for each
                    // field inside the field definition, so we have 
                    // to check if there is an enumeration associated
                    // with this field and add the enumeration to the
                    // field definition if one is found:
                    FixDxEnumeration dxEnum = src.Enums.GetElement(dxField.Name) as FixDxEnumeration;
                    if (dxEnum != null)
                    {
                        foreach (FixDxEnumerator dxEnumerator in dxEnum.Enumerators)
                        {
                            XmlQfxFieldEnumerator xmlEnumerator = new XmlQfxFieldEnumerator();
                            xmlEnumerator.Enum = dxEnumerator.Value;
                            xmlEnumerator.Description = dxEnumerator.Description;
                            xmlField.Enumeration.Add(xmlEnumerator);
                        }
                    }

                    dst.Fields.Add(xmlField);
                }
            }
        }

        /// <summary>
        /// The ExportBlocks method is invoked to convert all of
        /// the block elements in an instance of a VersaFix data
        /// dictionary into corresponding entries in an instance
        /// of an XML representation of a QuickFix dictionary.
        /// </summary>
        /// <param name="src">
        /// The source dictionary for the block definitions.
        /// </param>
        /// <param name="dst">
        /// The target dictionary for the block definitions.
        /// </param>
        private void ExportBlocks(FixDictionary src, XmlQfxDictionary dst)
        {
            foreach (IFixDxElement dxElement in src.Blocks)
            {
                FixDxBlock dxBlock = dxElement as FixDxBlock;
                if (dxBlock != null)
                {
                    XmlQfxBlock xmlBlock = new XmlQfxBlock();
                    xmlBlock.Name = dxBlock.Name;
                    foreach (IFixDxElement dxBlockElement in dxBlock.Elements)
                    {
                        ExportElement(dxBlockElement, xmlBlock.Elements);
                    }

                    dst.Blocks.Add(xmlBlock);
                }
            }
        }

        /// <summary>
        /// The ExportMessages method is invoked to convert all of
        /// the message elements in an instance of a VersaFix data
        /// dictionary into corresponding entries in an instance
        /// of an XML representation of a QuickFix dictionary.
        /// </summary>
        /// <param name="src">
        /// The source dictionary for the message definitions.
        /// </param>
        /// <param name="dst">
        /// The target dictionary for the message definitions.
        /// </param>
        private void ExportMessages(FixDictionary src, XmlQfxDictionary dst)
        {
            foreach (IFixDxElement dxElement in src.Messages)
            {
                FixDxMessage dxMessage = dxElement as FixDxMessage;
                if (dxMessage != null)
                {
                    XmlQfxMessage xmlMessage = new XmlQfxMessage();
                    xmlMessage.Name = dxMessage.Name;
                    xmlMessage.MsgType = dxMessage.MsgType;
                    xmlMessage.MsgCategory = dxMessage.MsgCategory;

                    foreach (IFixDxElement dxMessageElement in dxMessage.Elements)
                    {
                        ExportElement(dxMessageElement, xmlMessage.Elements);
                    }

                    dst.Messages.Add(xmlMessage);
                }
            }

        }

        /// <summary>
        /// The ExportTrailer method is invoked to convert all of the
        /// elements in the trailer section of a VersaFix dictionary
        /// out to an XML QuickFix dictionary.
        /// </summary>
        /// <param name="src">
        /// The source dictionary for the trailer elements.
        /// </param>
        /// <param name="dst">
        /// The target dictionary for the trailer elements.
        /// </param>
        private void ExportTrailer(FixDictionary src, XmlQfxDictionary dst)
        {
            foreach (IFixDxElement dxElement in src.Trailer)
            {
                ExportElement(dxElement, dst.Trailer);
            }
        }

        private void ExportElement(IFixDxElement dxElement, List<object> dst)
        {
            if (dxElement is FixDxFieldReference)
            {
                FixDxFieldReference dxField = dxElement as FixDxFieldReference;
                if (dxField != null)
                {
                    XmlQfxFieldReference xmlField = new XmlQfxFieldReference();
                    xmlField.Name = dxField.Name;
                    xmlField.Required = (dxField.Required == true) ? "Y" : "N";
                    dst.Add(xmlField);
                }
            }
            else if (dxElement is FixDxBlockReference)
            {
                FixDxBlockReference dxBlock = dxElement as FixDxBlockReference;
                if (dxBlock != null)
                {
                    XmlQfxBlockReference xmlBlock = new XmlQfxBlockReference();
                    xmlBlock.Name = dxBlock.Name;
                    xmlBlock.Required = (dxBlock.Required == true) ? "Y" : "N";
                    dst.Add(xmlBlock);
                }
            }
            else if (dxElement is FixDxGroupReference)
            {
                FixDxGroupReference dxGroup = dxElement as FixDxGroupReference;
                if (dxGroup != null)
                {
                    XmlQfxGroupReference xmlGroup = new XmlQfxGroupReference();
                    xmlGroup.Name = dxGroup.Name;
                    xmlGroup.Required = (dxGroup.Required == true) ? "Y" : "N";

                    foreach (IFixDxElement dxGroupElement in dxGroup.Elements)
                    {
                        ExportElement(dxGroupElement, xmlGroup.Elements);
                    }

                    dst.Add(xmlGroup);
                }
            }
        }
    }

}
