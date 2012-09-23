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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace VfxEngine.Fix
{
    /// <summary>
    /// The XmlDictionary class provides a representation of
    /// an instance of a FIX dictionary that is compatible with
    /// the .NET XML serializer. This class is used to serialize
    /// and deserialize instances of VfxEngine.Fix.FixDictionary.
    /// </summary>
    [XmlRoot(ElementName = "fix", IsNullable = true)]
    public class XmlDictionary
    {

        /// <summary>
        /// The Properties element in the XmlDictionary class
        /// provides a representation of the metadata properties
        /// that are associated with a dictionary, in a format that
        /// is compatible with the .NET XML serializer.
        /// </summary>
        [XmlElement("metadata")]
        public XmlFixDxProperties Properties = new XmlFixDxProperties();
        
        /// <summary>
        /// The Header element in the XmlDictionary class
        /// provides a representation of the dictionary's
        /// header elements in a format that is compatible
        /// with the .NET XML serializer.
        /// </summary>
        [XmlElement("header")]
        public XmlFixDxElements Header = new XmlFixDxElements();

        /// <summary>
        /// The Trailer element in the XmlDictionary class
        /// provides a representation of the dictionary's
        /// trailer elements in a format that is compatible
        /// with the .NET XML serializer.
        /// </summary>
        [XmlElement("trailer")]
        public XmlFixDxElements Trailer = new XmlFixDxElements();

        /// <summary>
        /// The Messages element in the XmlDictionary class
        /// provides a representation of all of the message
        /// instances in a dictionary in a format that is
        /// compatible with the .NET XML serializer.
        /// </summary>
        [XmlElement("messages")]
        public XmlFixDxMessages Messages = new XmlFixDxMessages();

        /// <summary>
        /// The Fields member of the XmlDictionary class
        /// provides a representation of all of the field
        /// definitions in a dictionary in a format that is
        /// compatible with the .NET XML serializer.
        /// </summary>
        [XmlElement("fields")]
        public XmlFixDxFields Fields = new XmlFixDxFields();

        /// <summary>
        /// The DataTypes member of the XmlDictionary class
        /// provides a representation of all of the data type
        /// definitions in a dictionary, in a format that is
        /// compatible with the .NET XML serializer.
        /// </summary>
        [XmlElement("DataTypes")]
        public XmlFixDxDataTypes DataTypes = new XmlFixDxDataTypes();

        /// <summary>
        /// The Blocks member of the XmlDictionary class
        /// provides a representation of all of the block
        /// definitions in a dictionary in a format that is
        /// compatible with the .NET XML serializer.
        /// </summary>
        [XmlElement("blocks")]
        public XmlFixDxBlocks Blocks = new XmlFixDxBlocks();


        /// <summary>
        /// The Enums member of the XmlDictionary class
        /// provides a representation of the enumerations
        /// in the dictionary in a format that is compatible
        /// with the .NET XML serializer.
        /// </summary>
        [XmlElement("enums")]
        public XmlFixDxEnums Enums = new XmlFixDxEnums();
    }

    /// <summary>
    /// The XmlFixDxMessages class provides a representation
    /// of a list of message definitions that is compatible for
    /// use with the .NET XML serializer.
    /// </summary>
    public class XmlFixDxMessages
    {
        /// <summary>
        /// The Entries member of the XmlFixDxMessages class provides
        /// access to the list of FIX message definitions that are found
        /// in an instance of a dictionary.
        /// </summary>
        [XmlElement("message")]
        public List<XmlFixDxMessage> Entries = new List<XmlFixDxMessage>();
    }

    /// <summary>
    /// The XmlFixDxMessage class provides a representation
    /// of a message definition that is fully compatible with
    /// the .NET XML serializer.
    /// </summary>
    public class XmlFixDxMessage
    {
        /// <summary>
        /// The message's FIX message name.
        /// </summary>
        [XmlAttribute("name")]
        public string Name;

        /// <summary>
        /// The message's FIX message type.
        /// </summary>
        [XmlAttribute("msgType")]
        public string MsgType;

        /// <summary>
        /// The message's FIX message category.
        /// </summary>
        [XmlAttribute("msgCat")]
        public string MsgCategory;

        /// <summary>
        /// The FIX dictionary elements that an instance of
        /// the message is composed of.
        /// </summary>
        [XmlArray("elements")]
        [XmlArrayItem("field", typeof(XmlFixDxFieldReference))]
        [XmlArrayItem("group", typeof(XmlFixDxGroupReference))]
        [XmlArrayItem("block", typeof(XmlFixDxBlockReference))]
        public List<object> Elements = new List<object>();
    }

    /// <summary>
    /// The XmlFixDxFields class provides a representation of
    /// all of the FIX field definitions in a dictionary, that
    /// is compatible with the .NET XML serializer.
    /// </summary>
    public class XmlFixDxFields
    {
        /// <summary>
        /// The list of all FIX field definitions that are defined
        /// in an instance of a FIX dictionary.
        /// </summary>
        [XmlElement("field")]
        public List<XmlFixDxField> Entries = new List<XmlFixDxField>();
    }

    /// <summary>
    /// The XmlFixDxField class provides a representation of
    /// a single FIX field definition in a dictionary, that is
    /// compatible with the .NET XML serializer.
    /// </summary>
    public class XmlFixDxField
    {
        /// <summary>
        /// The Tag property provides access to the FIX tag that
        /// is assigned to a field definition.
        /// </summary>
        [XmlAttribute("tag")]
        public int Tag;

        /// <summary>
        /// The Name property provides access to the FIX name that
        /// is assigned to a field definition.
        /// </summary>
        [XmlAttribute("name")]
        public string Name;

        /// <summary>
        /// The Type property provides access to the FIX data type
        /// that is assigned to a field definition.
        /// </summary>
        [XmlAttribute("type")]
        public string Type;

        /// <summary>
        /// The Enumeration property provides access to the enum
        /// identifier (if any) that is associated with the field.
        /// </summary>
        [XmlAttribute("Enumeration")]
        public string Enumeration;

        /// <summary>
        /// The LengthField property provides access to the name
        /// of the FIX field that is expected to contain the data
        /// length for the content of this field, if the field is
        /// a length-encoded field (FIX specification).
        /// </summary>
        [XmlAttribute("LengthField")]
        public string LengthField;
    }

    /// <summary>
    /// The XmlFixDataTypes class provides a representation
    /// of the list of data type definitions that are contained
    /// in an instance of a FIX dictionary, in a format that is
    /// compatible with the .NET XML serializer.
    /// </summary>
    public class XmlFixDxDataTypes
    {
        /// <summary>
        /// The Entries property provides access to the list of
        /// data type definitions that the class contains.
        /// </summary>
        [XmlElement("DataType")]
        public List<XmlFixDxDataType> Entries = new List<XmlFixDxDataType>();
    }

    /// <summary>
    /// The XmlFixDataType class provides a representation
    /// of an instance of a data type definition in a format
    /// that is compatible with the .NET XML serializer.
    /// </summary>
    public class XmlFixDxDataType
    {
        /// <summary>
        /// The TypeName property provides access to the name of
        /// the data type, as it is defined in the dictionary.
        /// </summary>
        [XmlElement("TypeName")]
        public string TypeName;

        /// <summary>
        /// The TypeBase property provides access to the name of
        /// the base type for the data type.
        /// </summary>
        [XmlElement("TypeBase")]
        public string TypeBase;
    }

    /// <summary>
    /// The XmlFixDxEnums (Dx=Dictionary) class encapsulates the
    /// list of all enumerations that are defined in an instance
    /// of a dictionary, in a format that is compatible with the
    /// .NET XML serializer.
    /// </summary>
    public class XmlFixDxEnums
    {
        /// <summary>
        /// The Entries property provides access to the list of
        /// all enumerations defined in an instance of a dictionary.
        /// </summary>
        [XmlElement("enumeration")]
        public List<XmlFixDxEnumeration> Entries = new List<XmlFixDxEnumeration>();
    }

    /// <summary>
    /// The XmlFixDxEnumeration (Dx=Dictionary) class encapsulates
    /// the details of a single instance of an enumeration, in a format
    /// that is compatible with the .NET XML serializer.
    /// </summary>
    public class XmlFixDxEnumeration
    {
        /// <summary>
        /// The Name property provides access to the FIX name
        /// of an instance of an enum in a dictionary.
        /// </summary>
        [XmlAttribute("name")]
        public string Name;

        /// <summary>
        /// The Elements property provides access to the list of
        /// enumerators that belong to an enumeration.
        /// </summary>
        [XmlArray("elements")]
        [XmlArrayItem("enumerator", typeof(XmlFixDxEnumerator))]
        public List<object> Enumerators = new List<object>();

    }
    
    /// <summary>
    /// The XmlFixDxEnumerator (Dx=Dictionary) class encapsulates
    /// the details of a single instance of an enumerator in a format
    /// that is compatible with the .NET XML serializer.
    /// </summary>
    public class XmlFixDxEnumerator
    {
        /// <summary>
        /// The Value property provides access to the value that has
        /// been assigned to an instance of an enumerator.
        /// </summary>
        [XmlAttribute("value")]
        public string value;

        /// <summary>
        /// The Description property provides access to the description
        /// of the instance of an enumerator, as it should be displayed
        /// to a user or written out to a document.
        /// </summary>
        [XmlAttribute("description")]
        public string Description;
    }

    /// <summary>
    /// The XmlFixDxBlocks class encapsulates a list of all
    /// of the component blocks in a dictionary.
    /// </summary>
    public class XmlFixDxBlocks
    {
        /// <summary>
        /// The Entries property provides access to the list of
        /// component blocks that are defined in the dictionary.
        /// </summary>
        [XmlElement("block")]
        public List<XmlFixDxBlock> Entries = new List<XmlFixDxBlock>();
    }

    /// <summary>
    /// The XmlFixDxBlock class encapsulates the details of a
    /// single component block definition in an XML dictionary.
    /// </summary>
    public class XmlFixDxBlock
    {
        /// <summary>
        /// The Name property provides access to the FIX name
        /// that is assigned to a component block definition.
        /// </summary>
        [XmlAttribute("name")]
        public string Name;

        /// <summary>
        /// The Type property provides access to the FIX type
        /// that is assigned to a component block definition.
        /// </summary>
        [XmlAttribute("type")]
        public string Type;

        /// <summary>
        /// The Field property provides access to the FIX name
        /// of the field that marks the start of a block, if the
        /// block represents a repeating block.
        /// </summary>
        [XmlAttribute("field")]
        public string Field;

        /// <summary>
        /// The Category property provides access to the category
        /// designator that is assigned to a component block definition.
        /// </summary>
        [XmlAttribute("Category")]
        public string Category;

        /// <summary>
        /// The Elements property provides access to the list of
        /// elements that a component block is composed of.
        /// </summary>
        [XmlArray("elements")]
        [XmlArrayItem("field", typeof(XmlFixDxFieldReference))]
        [XmlArrayItem("group", typeof(XmlFixDxGroupReference))]
        [XmlArrayItem("block", typeof(XmlFixDxBlockReference))]
        public List<object> Elements = new List<object>();
    }

    /// <summary>
    /// The XmlFixDxFieldReference class provides a representation
    /// of a field reference that is compatible with the .NET XML
    /// serializer.
    /// </summary>
    public class XmlFixDxFieldReference
    {
        /// <summary>
        /// The Name property provides access to the name of
        /// the FIX field the reference relates to.
        /// </summary>
        [XmlAttribute("name")]
        public string Name;

        /// <summary>
        /// The Required property provides access to a boolean
        /// value that indicates whether or not the field reference
        /// is a required component of the entity it belongs to.
        /// </summary>
        [XmlAttribute("required")]
        public bool Required;
    }

    /// <summary>
    /// The XmlFixDxGroupReference class provides a representation
    /// of a repeating group that is compatible with the .NET XML 
    /// serialization system.
    /// </summary>
    public class XmlFixDxGroupReference
    {
        /// <summary>
        /// The Name property provides access to the name of
        /// the FIX field that the group uses to mark the start
        /// of the repeating group (instance count).
        /// </summary>
        [XmlAttribute("name")]
        public string Name;

        /// <summary>
        /// The Required property provides access to a boolean
        /// that indicates whether or not the group is a required
        /// component of the entity it belongs to.
        /// </summary>
        [XmlAttribute("required")]
        public bool Required;


        /// <summary>
        /// The Elements property provides access to the list of
        /// all of the FIX elements the group is composed of.
        /// </summary>
        [XmlArray("elements")]
        [XmlArrayItem("field", typeof(XmlFixDxFieldReference))]
        [XmlArrayItem("group", typeof(XmlFixDxGroupReference))]
        [XmlArrayItem("block", typeof(XmlFixDxBlockReference))]
        public List<object> Elements = new List<object>();
    }

    /// <summary>
    /// The XmlFixDxBlockReference class provides a representation
    /// of a component block definition that is compatible with the
    /// .NET XML serializer.
    /// </summary>
    public class XmlFixDxBlockReference
    {
        /// <summary>
        /// The Name property provides access to the FIX name
        /// of the component block definition that the reference
        /// definition refers to.
        /// </summary>
        [XmlAttribute("name")]
        public string Name;

        /// <summary>
        /// The Required 
        /// </summary>
        [XmlAttribute("required")]
        public bool Required;

    }

    /// <summary>
    /// The XmlFixDxElements (Dx=Dictionary) class provides 
    /// a representation of the collection of elements that
    /// are contained with the header, trailer, and message
    /// elements in a dictionary in a format compatible for
    /// use with the .NET XML serializer.
    /// </summary>
    public class XmlFixDxElements
    {
        /// <summary>
        /// The Elements property provides access to the list
        /// of FIX dictionary elements that the element collection
        /// is composed of.
        /// </summary>
        [XmlArray("elements")]
        [XmlArrayItem("field", typeof(XmlFixDxFieldReference))]
        [XmlArrayItem("group", typeof(XmlFixDxGroupReference))]
        [XmlArrayItem("block", typeof(XmlFixDxBlockReference))]
        public List<object> Elements = new List<object>();
    }

    /// <summary>
    /// The XmlFixDxProperties (Dx=Dictionary) class provides
    /// a representation of the metadata properties that are in
    /// a dictionary, in a format that is compatible for use with
    /// the .NET XML serializer.
    /// </summary>
    public class XmlFixDxProperties
    {
        [XmlArray("properties")]
        [XmlArrayItem("property", typeof(XmlFixDxProperty))]
        public List<object> Elements = new List<object>();
    }

    /// <summary>
    /// The XmlFixDxProperty (Dx=Dictionary) class provides a
    /// representation of a single metadata property that is in
    /// an instance of a dictionary, in a format that is compatible
    /// for use with the .NET XML serializer.
    /// </summary>
    public class XmlFixDxProperty
    {
        [XmlAttribute("name")]
        public string Name;

        [XmlAttribute("value")]
        public string Value;
    }
}
