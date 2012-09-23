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
    /// The QfxFixDxImporter class provides an implementation of
    /// the IFixDxImporter interface for importing dictionaries that
    /// have been persisted to disk in QuickFix format.
    /// </summary>
    public class QfxFixDxImporter : IFixDxImporter
    {
        #region IFixDxImporter Members

        /// <summary>
        /// The Import method is invoked to import a dictionary
        /// from a representation that is contained in a file.
        /// </summary>
        /// <param name="path">
        /// The complete path to the file that contains the
        /// contents of the dictionary.
        /// </param>
        /// <returns>
        /// The FIX dictionary that is read from the stream or
        /// null if the import operation could not complete.
        /// </returns>
        public FixDictionary Import(string path)
        {
            try
            {
                FileStream fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read);
                FixDictionary result = Import(fs);
                fs.Close();
                return result;
            }
            catch (System.Exception)
            {
                return null;
            }
        }

        /// <summary>
        /// The Import method is invoked to import a dictionary
        /// from a representation contained in a stream.
        /// </summary>
        /// <param name="stream">
        /// The .NET stream that the dictionary representation
        /// is to be read from.
        /// </param>
        /// <returns>
        /// The FIX dictionary that is read from the stream or
        /// null if the import operation could not complete.
        /// </returns>
        public FixDictionary Import(System.IO.Stream stream)
        {
            try
            {
                // REC: Create a new serializer that can deserialize
                // the XmlQfxDictionary type from the stream:
                XmlSerializer xs = new XmlSerializer(typeof(XmlQfxDictionary));


                // REC: Attempt to deserialize the QuickFix dictionary
                // from the stream into its corresponding representation
                // as an instance of XmlQfxDictionary:
                XmlQfxDictionary xmlDictionary = xs.Deserialize(stream) as XmlQfxDictionary;

                // REC: After the XML details have been deserialized from
                // the stream, convert them into an instance of the VersaFix
                // data dictionary class and return that to the caller:
                FixDictionary result = new FixDictionary();

                if (!string.IsNullOrEmpty(xmlDictionary.Type))
                {
                    result.Properties.Add("Type", xmlDictionary.Type);
                }

                if (!string.IsNullOrEmpty(xmlDictionary.Major))
                {
                    result.Properties.Add("Fix.Major", xmlDictionary.Major);
                }

                if (!string.IsNullOrEmpty(xmlDictionary.Minor))
                {
                    result.Properties.Add("Fix.Minor", xmlDictionary.Minor);
                }

                // REC: Populate the dictionary's header elements using all
                // of the header elements from the QuickFix dictionary:
                PopulateHeader(xmlDictionary, result);

                // REC: Populate the dictionary's enumerations using all of
                // the enumerations from the QuickFix dictionary:
                PopulateEnums(xmlDictionary, result);

                // REC: Populate the dictionary's field definitions using all
                // of the field definitions from the QuickFix dictionary:
                PopulateFields(xmlDictionary, result);

                // REC: Populate the dictionary's block definitions using all
                // of the block definitions from the QuickFix dictionary:
                PopulateBlocks(xmlDictionary, result);

                // REC: Populate the dictionary's message definitions
                // with all of the message definitions that are found
                // in the QuickFix dictionary:
                PopulateMessages(xmlDictionary, result);

                // REC: Populate the dictionary's trailer elements using all
                // of the trailer elements from the QuickFix dictionary:
                PopulateTrailer(xmlDictionary, result);

                return result;
            }
            catch (System.Exception)
            {
                return null;
            }
        }

        #endregion


        /// <summary>
        /// The PopulateHeader method iterates over all of the elements
        /// that are defined in the header section of the QuickFix data
        /// dictionary and converts them into the header section of the
        /// supplied VersaFix data dictionary.
        /// </summary>
        /// <param name="src">
        /// The XML representation of the QuickFix dictionary that the
        /// header elements are to be read from.
        /// </param>
        /// <param name="dst">
        /// The VersaFix dictionary that the header elements are to be
        /// converted out to.
        /// </param>
        private void PopulateHeader(XmlQfxDictionary src, FixDictionary dst)
        {
            foreach (object element in src.Header)
            {
                IFixDxElement dxElement = ConvertElement(element);
                if (dxElement != null)
                {
                    dst.Header.Add(dxElement);
                }
            }
        }

        /// <summary>
        /// The PopulateEnums method iterates over all of the fields that
        /// are defined in a QuickFix dictionary and converts their inline
        /// enumeration definitions into discreet enumeration instances in
        /// the target VersaFix dictionary instance.
        /// </summary>
        /// <param name="src">
        /// The XML representation of a QuickFix dictionary that the 
        /// enumerations are to be read from.
        /// </param>
        /// <param name="dst">
        /// The VersaFix dictionary that the enumerations are to be
        /// written out to.
        /// </param>
        private void PopulateEnums(XmlQfxDictionary src, FixDictionary dst)
        {
            foreach (object field in src.Fields)
            {
                XmlQfxField xmlField = field as XmlQfxField;
                if (xmlField != null)
                {
                    if (xmlField.Enumeration.Count > 0)
                    {
                        string xmlName = xmlField.Name;
                        if (!string.IsNullOrEmpty(xmlName))
                        {
                            FixDxEnumeration dxEnum = new FixDxEnumeration(xmlName);
                            foreach (object enumerator in xmlField.Enumeration)
                            {
                                XmlQfxFieldEnumerator xmlEnumerator = enumerator as XmlQfxFieldEnumerator;
                                if (xmlEnumerator != null)
                                {
                                    dxEnum.Enumerators.Add(new FixDxEnumerator(xmlEnumerator.Enum, xmlEnumerator.Description));
                                }
                            }

                            dst.Enums.Add(dxEnum);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// The PopulateFields method is invoked to convert all of 
        /// the FIX field definitions in an instance of a QuickFix
        /// dictionary into their corresponding representations as
        /// elements of a VersaFix dictionary.
        /// </summary>
        /// <param name="src">
        /// The XML representation of a QuickFix dictionary that the
        /// fields are to be copied from.
        /// </param>
        /// <param name="dst">
        /// The VersaFix data dictionary that the field definitions
        /// are to be copied into.
        /// </param>
        private void PopulateFields(XmlQfxDictionary src, FixDictionary dst)
        {
            foreach (object field in src.Fields)
            {
                XmlQfxField xmlField = field as XmlQfxField;
                if (xmlField != null)
                {
                    if (!string.IsNullOrEmpty(xmlField.Name))
                    {
                        if (!string.IsNullOrEmpty(xmlField.Number))
                        {
                            int nTag = int.Parse(xmlField.Number);
                            FixDxField dxField = new FixDxField(nTag, xmlField.Name);
                            
                            // REC: Determine if there's an enumeration that corresponds
                            // to the name of this field:
                            if (dst.Enums.GetElement(xmlField.Name) != null)
                            {
                                // REC: If an enumeration exists for this field
                                // then assign it to the VersaFix field:
                                dxField.Enumeration = xmlField.Name;
                            }

                            // REC: Assign the field's data type directly
                            // from the data type in the QuickFix field.
                            dxField.Type = xmlField.Type;

                            dst.Fields.Add(dxField);

                            // REC: The QuickFix dictionaries do not provide
                            // a separate section for data types, so we need
                            // to just copy the data type directly from each
                            // of the QuickFix fields into the data types of
                            // the VersaFix dictionary:
                            if (!string.IsNullOrEmpty(xmlField.Type))
                            {
                                IFixDxElement exists = dst.DataTypes.GetElement(xmlField.Type);
                                if (exists == null)
                                {
                                    dst.DataTypes.Add(new FixDxDataType(xmlField.Type));
                                }
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// The PopulateBlocks method is invoked to convert all of the
        /// component block definitions in a QuickFix dictionary into 
        /// corresponding component block definitions in an instance of
        /// a VersaFix dictionary.
        /// </summary>
        /// <param name="src">
        /// The QuickFix dictionary to copy the blocks from.
        /// </param>
        /// <param name="dst">
        /// The VersaFix dictionary to copy the blocks into.
        /// </param>
        private void PopulateBlocks(XmlQfxDictionary src, FixDictionary dst)
        {
            foreach (object block in src.Blocks)
            {
                XmlQfxBlock xmlBlock = block as XmlQfxBlock;
                if (xmlBlock != null)
                {
                    if (!string.IsNullOrEmpty(xmlBlock.Name))
                    {
                        FixDxBlock dxBlock = new FixDxBlock(xmlBlock.Name);
                        foreach (object element in xmlBlock.Elements)
                        {
                            IFixDxElement dxElement = ConvertElement(element);
                            if (dxElement != null)
                            {
                                dxBlock.Elements.Add(dxElement);
                            }
                        }

                        dst.Blocks.Add(dxBlock);
                    }
                }
            }
        }

        /// <summary>
        /// The PopulateMessages method is invoked to convert all of
        /// the message definitions in a QuickFix dictionary into their
        /// corresponding representations in a VersaFix dictionary.
        /// </summary>
        /// <param name="src">
        /// The source dictionary for the converted elements.
        /// </param>
        /// <param name="dst">
        /// The target dictionary for the converted elements.
        /// </param>
        private void PopulateMessages(XmlQfxDictionary src, FixDictionary dst)
        {
            foreach (object message in src.Messages)
            {
                XmlQfxMessage xmlMessage = message as XmlQfxMessage;
                if (xmlMessage != null)
                {
                    FixDxMessage dxMessage = new FixDxMessage(xmlMessage.MsgType, xmlMessage.Name, xmlMessage.MsgCategory);
                    foreach (object element in xmlMessage.Elements)
                    {
                        dxMessage.Elements.Add(ConvertElement(element));
                    }

                    dst.Messages.Add(dxMessage);
                }
            }
        }

        /// <summary>
        /// The PopulateTrailer method iterates over all of the elements
        /// that are defined in the trailer section of the QuickFix data
        /// dictionary and converts them into the trailer section of the
        /// supplied VersaFix data dictionary.
        /// </summary>
        /// <param name="src">
        /// The XML representation of the QuickFix dictionary that the
        /// trailer elements are to be read from.
        /// </param>
        /// <param name="dst">
        /// The VersaFix dictionary that the trailer elements are to be
        /// converted out to.
        /// </param>
        private void PopulateTrailer(XmlQfxDictionary src, FixDictionary dst)
        {
            foreach (object element in src.Trailer)
            {
                IFixDxElement dxElement = ConvertElement(element);
                if (dxElement != null)
                {
                    dst.Trailer.Add(dxElement);
                }
            }
        }

        /// <summary>
        /// The ConvertElement method converts an instance of an XML element
        /// from a QuickFix dictionary into its corresponding representation
        /// as an element in a VersaFix dictionary.
        /// </summary>
        /// <param name="xmlElement">
        /// The XML representation of a QuickFix dictionary element that is
        /// to be converted into an instance of a VersaFix element.
        /// </param>
        /// <returns>
        /// The VersaFix dictionary element that results from the conversion
        /// attempt, or null if the conversion could not be carried out.
        /// </returns>
        private IFixDxElement ConvertElement(object xmlElement)
        {
            IFixDxElement result = null;

            if (xmlElement is XmlQfxFieldReference)
            {
                XmlQfxFieldReference xmlField = xmlElement as XmlQfxFieldReference;
                if (xmlField != null)
                {
                    if (!string.IsNullOrEmpty(xmlField.Name))
                    {
                        bool required = false;
                        if (xmlField.Required.CompareTo("Y") == 0)
                        {
                            required = true;
                        }

                        FixDxFieldReference dxField = new FixDxFieldReference(xmlField.Name, required);
                        result = dxField;
                    }
                }
            }
            else if (xmlElement is XmlQfxBlockReference)
            {
                XmlQfxBlockReference xmlBlock = xmlElement as XmlQfxBlockReference;
                if (xmlBlock != null)
                {
                    if (!string.IsNullOrEmpty(xmlBlock.Name))
                    {
                        bool required = false;
                        if (xmlBlock.Required.CompareTo("Y") == 0)
                        {
                            required = true;
                        }

                        FixDxBlockReference dxBlock = new FixDxBlockReference(xmlBlock.Name, required);
                        result = dxBlock;
                    }
                }
            }
            else if (xmlElement is XmlQfxGroupReference)
            {
                XmlQfxGroupReference xmlGroup = xmlElement as XmlQfxGroupReference;
                if (xmlGroup != null)
                {
                    if (!string.IsNullOrEmpty(xmlGroup.Name))
                    {
                        bool required = false;
                        if (xmlGroup.Required.CompareTo("Y") == 0)
                        {
                            required = true;
                        }

                        FixDxGroupReference dxGroup = new FixDxGroupReference(xmlGroup.Name, required);
                        foreach (object element in xmlGroup.Elements)
                        {
                            IFixDxElement dxElement = ConvertElement(element);
                            if (dxElement != null)
                            {
                                dxGroup.Elements.Add(dxElement);
                            }
                        }

                        result = dxGroup;
                    }
                }
            }

            return result;
        }
    }
}
