//************************************************************************
// The VersaFix Fix Engine.
// Copyright (c) 2009 Assimilate Technology, Inc.
//------------------------------------------------------------------------
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with this program. If not, see <http://www.gnu.org/licenses/>.
//************************************************************************
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
    /// The FixDxExporter (Dx=Dictionary) class encapsulates the
    /// functionality required to export the contents of a dictionary
    /// out to an XML file. The exporter converts the contents of the
    /// dictionary into an XML serializable representation and then
    /// uses the .NET XML serializer to write the file.
    /// </summary>
    public sealed class FixDxExporter : IFixDxExporter
    {

        #region IFixDxExporter Members

        /// <summary>
        /// This implementation of the Export method serializes the 
        /// specified dictionary out to the specified file.
        /// </summary>
        /// <param name="dx">
        /// The dictionary that is to be exported.
        /// </param>
        /// <param name="path">
        /// The fully qualified path to the XML file the dictionary
        /// is to be exported to.
        /// </param>
        public void Export(FixDictionary dx, string path)
        {
            FileStream stream = null;

            try
            {
                stream = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.None);
                Export(dx, stream);
                stream.Close();
            }
            catch (System.Exception)
            {
                if (stream != null)
                {
                    stream.Close();
                }

                throw;
            }
        }

        /// <summary>
        /// This implementation of the Export method serializes the
        /// specified dictionary out to a stream that has already been
        /// opened by the caller.
        /// </summary>
        /// <param name="dictionary">
        /// The dictionary that is to be exported.
        /// </param>
        /// <param name="stream">
        /// The stream that the XML data is to be written to.
        /// </param>
        public void Export(FixDictionary dictionary, Stream stream)
        {
            XmlDictionary xmlDictionary = new XmlDictionary();

            // REC: Populate the XML dictionary with all of the
            // elements from the supplied FixDictionary instance:
            PopulateMetadata(dictionary, xmlDictionary);
            PopulateHeader(xmlDictionary, dictionary);
            PopulateTrailer(xmlDictionary, dictionary);
            PopulateMessages(xmlDictionary, dictionary);
            PopulateFields(xmlDictionary, dictionary);
            PopulateDataTypes(xmlDictionary, dictionary);
            PopulateBlocks(xmlDictionary, dictionary);
            PopulateEnums(xmlDictionary, dictionary);

            // REC: After the XML serializable representation of
            // the dictionary has been populated, use the serializer
            // to export its contents out to the specified stream:
            XmlSerializer xs = new XmlSerializer(typeof(XmlDictionary));
            XmlWriterSettings xwSettings = new XmlWriterSettings();
            xwSettings.Indent = true;
            xwSettings.OmitXmlDeclaration = true;
            XmlSerializerNamespaces xsn = new XmlSerializerNamespaces();
            xsn.Add("", "");

            XmlWriter writer = XmlWriter.Create(stream, xwSettings);
            xs.Serialize(writer, xmlDictionary, xsn);
        }

        #endregion

        /// <summary>
        /// The PopulateMetadata method populates the metadata properties
        /// in an XmlDictionary instance with the corresponding entries from
        /// an instance of a VersaFix dictionary.
        /// </summary>
        /// <param name="source">
        /// The source dictionary for the elements being copied.
        /// </param>
        /// <param name="target">
        /// The target dictionary for the elements to be copied into.
        /// </param>
        private static void PopulateMetadata(FixDictionary source, XmlDictionary target)
        {
            foreach (string key in source.Properties.Keys)
            {
                // REC: Construct an XML representation of the property
                // and add it to the target dictionary:
                XmlFixDxProperty xmlProperty = new XmlFixDxProperty();
                xmlProperty.Name = key;
                xmlProperty.Value = source.Properties[key];

                target.Properties.Elements.Add(xmlProperty);
            }
        }

        /// <summary>
        /// The PopulateHeader method populates the header elements
        /// in an XmlDictionary instance with the elements from the
        /// source dictionary.
        /// </summary>
        /// <param name="xmlDictionary">
        /// The XmlDictionary instance that is being populated.
        /// </param>
        /// <param name="fixDictionary">
        /// The FixDictionary instance that is the source of the
        /// elements the XmlDictionary is to be populated with.
        /// </param>
        private static void PopulateHeader(XmlDictionary xmlDictionary, FixDictionary fixDictionary)
        {
            // REC: Iterate over all of the header elements in the
            // source dictionary and convert them into instances of
            // their corresponding XML serializable types:
            foreach (IFixDxElement dxElement in fixDictionary.Header)
            {
                if (dxElement is FixDxFieldReference)
                {
                    FixDxFieldReference fieldReference = dxElement as FixDxFieldReference;
                    XmlFixDxFieldReference xmlReference = new XmlFixDxFieldReference();
                    xmlReference.Name = fieldReference.Name;
                    xmlReference.Required = fieldReference.Required;

                    xmlDictionary.Header.Elements.Add(xmlReference);
                }
                else if (dxElement is FixDxBlockReference)
                {
                    FixDxBlockReference blockReference = dxElement as FixDxBlockReference;
                    XmlFixDxBlockReference xmlReference = new XmlFixDxBlockReference();
                    xmlReference.Name = blockReference.Name;
                    xmlReference.Required = blockReference.Required;

                    xmlDictionary.Header.Elements.Add(xmlReference);
                }
                else if (dxElement is FixDxGroupReference)
                {
                    FixDxGroupReference groupReference = dxElement as FixDxGroupReference;
                    XmlFixDxGroupReference xmlReference = new XmlFixDxGroupReference();
                    xmlReference.Name = groupReference.Name;
                    xmlReference.Required = groupReference.Required;

                    XmlFixDxElements xmlElements = TranslateElements(groupReference.Elements);
                    foreach (object xmlElement in xmlElements.Elements)
                    {
                        xmlReference.Elements.Add(xmlElement);
                    }

                    xmlDictionary.Header.Elements.Add(xmlReference);
                }

            }
        }

        /// <summary>
        /// The PopulateTrailer method populates the trailer elements
        /// in an XmlDictionary instance with the elements from the
        /// source dictionary.
        /// </summary>
        /// <param name="xmlDictionary">
        /// The XmlDictionary instance that is being populated.
        /// </param>
        /// <param name="fixDictionary">
        /// The FixDictionary instance that is the source of the
        /// elements the XmlDictionary is to be populated with.
        /// </param>
        private static void PopulateTrailer(XmlDictionary xmlDictionary, FixDictionary fixDictionary)
        {
            // REC: Iterate over all of the trailer elements in the
            // source dictionary and convert them into instances of
            // their corresponding XML serializable types:
            foreach (IFixDxElement dxElement in fixDictionary.Trailer)
            {
                if (dxElement is FixDxFieldReference)
                {
                    FixDxFieldReference fieldReference = dxElement as FixDxFieldReference;
                    XmlFixDxFieldReference xmlReference = new XmlFixDxFieldReference();
                    xmlReference.Name = fieldReference.Name;
                    xmlReference.Required = fieldReference.Required;

                    xmlDictionary.Trailer.Elements.Add(xmlReference);
                }
                else if (dxElement is FixDxBlockReference)
                {
                    FixDxBlockReference blockReference = dxElement as FixDxBlockReference;
                    XmlFixDxBlockReference xmlReference = new XmlFixDxBlockReference();
                    xmlReference.Name = blockReference.Name;
                    xmlReference.Required = blockReference.Required;

                    xmlDictionary.Trailer.Elements.Add(xmlReference);
                }
                else if (dxElement is FixDxGroupReference)
                {
                    FixDxGroupReference groupReference = dxElement as FixDxGroupReference;
                    XmlFixDxGroupReference xmlReference = new XmlFixDxGroupReference();
                    xmlReference.Name = groupReference.Name;
                    xmlReference.Required = groupReference.Required;

                    XmlFixDxElements xmlElements = TranslateElements(groupReference.Elements);
                    foreach (object xmlElement in xmlElements.Elements)
                    {
                        xmlReference.Elements.Add(xmlElement);
                    }

                    xmlDictionary.Trailer.Elements.Add(xmlReference);
                }
            }
        }

        /// <summary>
        /// The PopulateMessages method populates the messages in
        /// an XmlDictionary instance with the message definitions
        /// from the source dictionary.
        /// </summary>
        /// <param name="xmlDictionary">
        /// The XmlDictionary instance that the message definitions
        /// are to be added to.
        /// </param>
        /// <param name="fixDictionary">
        /// The FixDictionary instance that is the source of the
        /// elements the XmlDictionary is to be populated with.
        /// </param>
        private static void PopulateMessages(XmlDictionary xmlDictionary, FixDictionary fixDictionary)
        {
            // REC: Iterate over all of the message definitions in the
            // source dictionary and convert them to XML serializable
            // representations for the serializable dictionary:
            foreach (FixDxMessage dxMessage in fixDictionary.Messages)
            {
                XmlFixDxMessage xmlMessage = new XmlFixDxMessage();
                xmlMessage.Name = dxMessage.Name;
                xmlMessage.MsgType = dxMessage.MsgType;
                xmlMessage.MsgCategory = dxMessage.MsgCategory;

                XmlFixDxElements xmlElements = TranslateElements(dxMessage.Elements);
                foreach (object xmlElement in xmlElements.Elements)
                {
                    xmlMessage.Elements.Add(xmlElement);
                }

                xmlDictionary.Messages.Entries.Add(xmlMessage);
            }
        }


        /// <summary>
        /// The PopulateFields method populates the fields section
        /// of an XmlDictionary instance with the field definitions
        /// from the supplied FixDictionary instance.
        /// </summary>
        /// <param name="xmlDictionary">
        /// The XmlDictinary instance that the field definitions
        /// are to be added to.
        /// </param>
        /// <param name="fixDictionary">
        /// The FixDictionary instance that is the source of the
        /// elements the XmlDictionary is to be populated with.
        /// </param>
        private static void PopulateFields(XmlDictionary xmlDictionary, FixDictionary fixDictionary)
        {
            foreach (FixDxField dxField in fixDictionary.Fields)
            {
                XmlFixDxField xmlField = new XmlFixDxField();
                xmlField.Tag = dxField.Tag;
                xmlField.Name = dxField.Name;
                xmlField.Type = dxField.Type;
                xmlField.Enumeration = dxField.Enumeration;

                if (dxField.LengthCoded == true)
                {
                    xmlField.LengthField = dxField.LengthField.ToString();
                }
                else
                {
                    xmlField.LengthField = null;
                }


                xmlDictionary.Fields.Entries.Add(xmlField);

            }
        }

        /// <summary>
        /// The PopulateDataTypes method populates the data types 
        /// section of the supplied XML dictionary with all of the
        /// data type definitions that are in the FIX dictionary.
        /// </summary>
        /// <param name="xmlDictionary">
        /// The XML dictionary to be populated.
        /// </param>
        /// <param name="fixDictionary">
        /// The FIX dictionary to retrieve the data from.
        /// </param>
        private static void PopulateDataTypes(XmlDictionary xmlDictionary, FixDictionary fixDictionary)
        {
            foreach (FixDxDataType fixDataType in fixDictionary.DataTypes)
            {
                XmlFixDxDataType xmlDataType = new XmlFixDxDataType();
                xmlDataType.TypeName = fixDataType.Name;
                xmlDataType.TypeBase = fixDataType.BaseType;

                xmlDictionary.DataTypes.Entries.Add(xmlDataType);
            }
        }

        /// <summary>
        /// The PopulateBlocks method populates the blocks section
        /// of the supplied XML dictionary with the corresponding
        /// elements from the supplied FixDictionary instance.
        /// </summary>
        /// <param name="xmlDictionary">
        /// The XmlDictionary instance that is to be populated.
        /// </param>
        /// <param name="fixDictionary">
        /// The FixDictionary instance that is the source of the
        /// elements the XmlDictionary is to be populated with.
        /// </param>
        private static void PopulateBlocks(XmlDictionary xmlDictionary, FixDictionary fixDictionary)
        {
            foreach (FixDxBlock dxBlock in fixDictionary.Blocks)
            {
                XmlFixDxBlock xmlBlock = new XmlFixDxBlock();
                xmlBlock.Name = dxBlock.Name;
                xmlBlock.Type = dxBlock.Type.ToString();
                xmlBlock.Field = dxBlock.Field;

                xmlBlock.Elements = TranslateElements(dxBlock.Elements).Elements;
                xmlDictionary.Blocks.Entries.Add(xmlBlock);
            }
        }

        /// <summary>
        /// The PopulateEnums method populates the enumerations
        /// section of the supplied XmlDictionary with all the
        /// enumerations from the supplied FixDictionary.
        /// </summary>
        /// <param name="xmlDictionary">
        /// The XmlDictionary instance being populated.
        /// </param>
        /// <param name="fixDictionary">
        /// The FixDictionary instance that is the source of the
        /// elements the XmlDictionary is to be populated with.
        /// </param>
        private static void PopulateEnums(XmlDictionary xmlDictionary, FixDictionary fixDictionary)
        {
            foreach (FixDxEnumeration dxEnumeration in fixDictionary.Enums)
            {
                XmlFixDxEnumeration xmlEnumeration = new XmlFixDxEnumeration();
                xmlEnumeration.Name = dxEnumeration.Name;
                foreach (FixDxEnumerator dxEnumerator in dxEnumeration.Enumerators)
                {
                    XmlFixDxEnumerator xmlEnumerator = new XmlFixDxEnumerator();
                    xmlEnumerator.value = dxEnumerator.Value;
                    xmlEnumerator.Description = dxEnumerator.Description;
                    xmlEnumeration.Enumerators.Add(xmlEnumerator);
                }

                xmlDictionary.Enums.Entries.Add(xmlEnumeration);
            }
        }

        /// <summary>
        /// The TranslateElements method converts a collection of
        /// dictionary elements into their corresponding representation
        /// as instances of XML serializable classes. The method only
        /// translates field, group, and block references, since those
        /// are the only elements that should be found in collections
        /// of dictionary elements under normal usage.
        /// </summary>
        /// <param name="dxElements">
        /// The collection of field, group, and block references that
        /// is to be converted to XML serializable classes.
        /// </param>
        /// <returns>
        /// The resulting collection of XML serializable classes.
        /// </returns>
        private static XmlFixDxElements TranslateElements(FixDxCollection dxElements)
        {
            XmlFixDxElements result = new XmlFixDxElements();
            foreach (IFixDxElement dxElement in dxElements)
            {
                if (dxElement is FixDxFieldReference)
                {
                    FixDxFieldReference dxReference = dxElement as FixDxFieldReference;
                    XmlFixDxFieldReference xmlReference = new XmlFixDxFieldReference();
                    xmlReference.Name = dxReference.Name;
                    xmlReference.Required = dxReference.Required;
                    result.Elements.Add(xmlReference);
                }
                else if (dxElement is FixDxGroupReference)
                {
                    FixDxGroupReference dxReference = dxElement as FixDxGroupReference;
                    XmlFixDxGroupReference xmlReference = new XmlFixDxGroupReference();
                    xmlReference.Name = dxReference.Name;
                    xmlReference.Required = dxReference.Required;
                    XmlFixDxElements xmlElements = TranslateElements(dxReference.Elements);
                    foreach (object xmlElement in xmlElements.Elements)
                    {
                        xmlReference.Elements.Add(xmlElement);
                    }
                    result.Elements.Add(xmlReference);

                }
                else if (dxElement is FixDxBlockReference)
                {
                    FixDxBlockReference dxReference = dxElement as FixDxBlockReference;
                    XmlFixDxBlockReference xmlReference = new XmlFixDxBlockReference();
                    xmlReference.Name = dxReference.Name;
                    xmlReference.Required = dxReference.Required;
                    result.Elements.Add(xmlReference);
                }
            }

            return result;
        }
    }
}
