//************************************************************************
// The VersaFix FIX Engine. 
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
    /// The FixDxImporter (Dx=Dictionary) class is used to 
    /// import FIX data dictionaries from an XML stream.
    /// </summary>
    public class FixDxImporter
    {
        /// <summary>
        /// The Import method deserializes an XML representation of
        /// a VersaFix dictionary from a file and converts it into a
        /// corresponding instance of FixDictionary.
        /// </summary>
        /// <param name="path">
        /// The path to the XML file that contains the dictionary.
        /// </param>
        /// <returns>
        /// The resulting VersaFix dictionary instance.
        /// </returns>
        public static FixDictionary Import(string path)
        {
            FixDictionary result = null;
            FileStream stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read);
            try
            {
                result = Import(stream);
                stream.Close();
            }
            catch (System.Exception)
            {
                stream.Close();
                throw;
            }

            return result;
        }

        /// <summary>
        /// The Import method deserializes an XML representation
        /// of a VersaFix dictionary from a stream and converts it
        /// into a corresponding instance of FixDictionary.
        /// </summary>
        /// <param name="stream">
        /// The stream that the XML data is to be read from.
        /// </param>
        /// <returns>
        /// The resulting instance of FixDictionary.
        /// </returns>
        public static FixDictionary Import(Stream stream)
        {
            XmlSerializer xs = new XmlSerializer(typeof(XmlDictionary));
            XmlDictionary xmlDictionary = xs.Deserialize(stream) as XmlDictionary;

            // REC: Construct a new instance of a VersaFix dictionary
            // that the XML data will be converted into:
            FixDictionary result = new FixDictionary();

            // REC: Populate the VersaFix dictionary instance with all
            // of the elements that were read from the XML stream:
            PopulateMetadata(xmlDictionary, result);
            PopulateHeader(xmlDictionary, result);
            PopulateTrailer(xmlDictionary, result);
            PopulateMessages(xmlDictionary, result);
            PopulateFields(xmlDictionary, result);
            PopulateDataTypes(xmlDictionary, result);
            PopulateBlocks(xmlDictionary, result);
            PopulateEnums(xmlDictionary, result);

            return result;
        }

        /// <summary>
        /// The PopulateMetadata method populates the metadata properties
        /// of a VersaFix FIX data dictionary with the properties that are
        /// found in an instance of an XML representation of a dictionary.
        /// </summary>
        /// <param name="source">
        /// The source dictionary to read the properties from.
        /// </param>
        /// <param name="target">
        /// The target dictionary to add the properties to. 
        /// </param>
        private static void PopulateMetadata(XmlDictionary source, FixDictionary target)
        {
            foreach (XmlFixDxProperty xmlProperty in source.Properties.Elements)
            {
                target.Properties.Add(xmlProperty.Name, xmlProperty.Value);
            }
        }

        /// <summary>
        /// The PopulateHeader method populates the header section
        /// of the supplied VFX dictionary with all of the header
        /// elements that are found in the supplied XML dictionary.
        /// </summary>
        /// <param name="source">
        /// The source dictionary for the trailer elements.
        /// </param>
        /// <param name="target">
        /// The target dictionary for the converted elements.
        /// </param>
        private static void PopulateHeader(XmlDictionary source, FixDictionary target)
        {
            foreach (IFixDxElement dxElement in TranslateElements(source.Header.Elements))
            {
                target.Header.Add(dxElement);
            }
        }

        /// <summary>
        /// The PopulateTrailer method populates the trailer section
        /// of the supplied VFX dictionary with all of the trailer 
        /// elements that are found in the supplied XML dictionary.
        /// </summary>
        /// <param name="source">
        /// The source dictionary for the trailer elements.
        /// </param>
        /// <param name="target">
        /// The target dictionary for the converted elements.
        /// </param>
        private static void PopulateTrailer(XmlDictionary source, FixDictionary target)
        {
            foreach (IFixDxElement dxElement in TranslateElements(source.Trailer.Elements))
            {
                target.Trailer.Add(dxElement);
            }
        }

        /// <summary>
        /// The PopulateMessages method populates the message section
        /// of the supplied VFX dictionary with all message definitions
        /// that are found in the supplied XML dictionary.
        /// </summary>
        /// <param name="source">
        /// The source dictionary for the message elements.
        /// </param>
        /// <param name="target">
        /// The target dictionary for the converted elements.
        /// </param>
        private static void PopulateMessages(XmlDictionary source, FixDictionary target)
        {
            foreach (XmlFixDxMessage src in source.Messages.Entries)
            {
                FixDxMessage dst = new FixDxMessage(src.MsgType, src.Name, src.MsgCategory);
                foreach (IFixDxElement dxElement in TranslateElements(src.Elements))
                {
                    dst.Elements.Add(dxElement);
                }

                target.AddMessage(dst);
            }
        }

        /// <summary>
        /// The PopulateFields method populates the fields section
        /// of the supplied VFX dictionary with all of the fields
        /// that are found in the supplied XML dictionary.
        /// </summary>
        /// <param name="source">
        /// The source dictionary for the field elements.
        /// </param>
        /// <param name="target">
        /// The target dictionary for the converted elements.
        /// </param>
        private static void PopulateFields(XmlDictionary source, FixDictionary target)
        {
            foreach (XmlFixDxField src in source.Fields.Entries)
            {
                if (src.LengthField == null)
                {
                    FixDxField dst = new FixDxField(src.Tag, src.Name, src.Type);
                    dst.Enumeration = src.Enumeration;
                    target.AddField(dst);
                }
                else
                {
                    FixDxField dst = new FixDxField(src.Tag, src.Name, src.Type, src.LengthField);
                    dst.Enumeration = src.Enumeration;
                    target.AddField(dst);
                }
            }
        }

        /// <summary>
        /// The PopulateDataTypes method populates the data types
        /// section of the supplied VFX dictionary with all of the
        /// data types that are found in the XML dictionary.
        /// </summary>
        /// <param name="source">
        /// The XML dictionary to read the data types from.
        /// </param>
        /// <param name="target">
        /// The VFX dictionary to write the data types to.
        /// </param>
        private static void PopulateDataTypes(XmlDictionary source, FixDictionary target)
        {
            foreach (XmlFixDxDataType xmlDataType in source.DataTypes.Entries)
            {
                FixDxDataType fixDataType = new FixDxDataType(xmlDataType.TypeName, xmlDataType.TypeBase);
                target.DataTypes.Add(fixDataType);
            }
        }

        /// <summary>
        /// The PopulateBlocks method populates the blocks section
        /// of the supplied VFX dictionary with all of the blocks
        /// that are defined in the supplied XML dictionary.
        /// </summary>
        /// <param name="source">
        /// The source dictionary for the block elements.
        /// </param>
        /// <param name="target">
        /// The target dictionary for the converted elements.
        /// </param>
        private static void PopulateBlocks(XmlDictionary source, FixDictionary target)
        {
            foreach (XmlFixDxBlock src in source.Blocks.Entries)
            {
                FixDxBlock dst = new FixDxBlock(src.Name);
                dst.Type = (FixDxBlockTypes)Enum.Parse(typeof(FixDxBlockTypes), src.Type);
                dst.Field = src.Field;
                dst.Category = src.Category;

                foreach (IFixDxElement element in TranslateElements(src.Elements))
                {
                    dst.Elements.Add(element);
                }

                target.AddBlock(dst);
            }
        }

        /// <summary>
        /// The PopulateEnums method populates the enumerations section
        /// of the supplied VFX dictionary with all of the enumerations
        /// that are defined in the supplied XML dictionary.
        /// </summary>
        /// <param name="source">
        /// The source dictionary for the enumeration elements.
        /// </param>
        /// <param name="target">
        /// The target dictionary for the converted elements.
        /// </param>
        private static void PopulateEnums(XmlDictionary source, FixDictionary target)
        {
            foreach (XmlFixDxEnumeration src in source.Enums.Entries)
            {
                FixDxEnumeration dst = new FixDxEnumeration(src.Name);
                foreach (object element in src.Enumerators)
                {
                    if (element is XmlFixDxEnumerator)
                    {
                        XmlFixDxEnumerator srcEnum = element as XmlFixDxEnumerator;
                        FixDxEnumerator dstEnum = new FixDxEnumerator(srcEnum.value, srcEnum.Description);
                        dst.Enumerators.Add(dstEnum);
                    }
                }
                target.AddEnumeration(dst);
            }
        }

        /// <summary>
        /// The TranslateElements method iterates over a list of objects
        /// from an entity in the XML representation of a dictionary and
        /// translates them into a collection of VFX dictionary types.
        /// </summary>
        /// <param name="elements">
        /// The list of objects from an entity in the XML representation
        /// of a VersaFix dictionary.
        /// </param>
        /// <returns>
        /// An instance of FixDxCollection that has been populated with
        /// the VersaFix dictionary types that correspond to the objects
        /// in the list of XML elements.
        /// </returns>
        private static FixDxCollection TranslateElements(List<object> elements)
        {
            FixDxCollection result = new FixDxCollection();

            foreach (object element in elements)
            {
                if (element is XmlFixDxFieldReference)
                {
                    XmlFixDxFieldReference src = element as XmlFixDxFieldReference;
                    result.Add(new FixDxFieldReference(src.Name, src.Required));
                }
                else if (element is XmlFixDxGroupReference)
                {
                    XmlFixDxGroupReference src = element as XmlFixDxGroupReference;
                    FixDxGroupReference dst = new FixDxGroupReference(src.Name, src.Required);

                    foreach (IFixDxElement member in TranslateElements(src.Elements))
                    {
                        dst.Elements.Add(member);
                    }

                    result.Add(dst);
                }
                else if(element is XmlFixDxBlockReference)
                {
                    XmlFixDxBlockReference src = element as XmlFixDxBlockReference;
                    result.Add(new FixDxBlockReference(src.Name, src.Required));
                }
            }

            return result;

        }
    }
}
