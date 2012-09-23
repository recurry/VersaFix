using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace VfxEngine.Fix
{
    /// <summary>
    /// The XmlQfxDictionary class provides a representation
    /// of a QuickFIX data dictionary in a format that can be
    /// used with the .NET XML serializer.
    /// </summary>
    [XmlRoot("fix")]
    public class XmlQfxDictionary
    {
        /// <summary>
        /// The QuickFix representation of a kind of layer
        /// indication, e.g. FIXT=session FIX=application.
        /// </summary>
        [XmlAttribute("type")]
        public string Type;

        /// <summary>
        /// The QuickFix representation of the major component
        /// of the data dictionary's FIX version.
        /// </summary>
        [XmlAttribute("major")]
        public string Major;

        /// <summary>
        /// The QuickFix representation of the minor component
        /// of the data dictionary's FIX version.
        /// </summary>
        [XmlAttribute("minor")]
        public string Minor;

        /// <summary>
        /// The list of FIX element references that the header
        /// section of the data dictionary is composed of.
        /// </summary>
        [XmlArray("header")]
        [XmlArrayItem("field", typeof(XmlQfxFieldReference))]
        [XmlArrayItem("group", typeof(XmlQfxGroupReference))]
        [XmlArrayItem("component", typeof(XmlQfxBlockReference))]
        public List<object> Header = new List<object>();

        [XmlArray("messages")]
        [XmlArrayItem("message", typeof(XmlQfxMessage))]
        public List<object> Messages = new List<object>();

        /// <summary>
        /// The list of FIX component block definitions that are
        /// contained in the components section of the dictionary.
        /// </summary>
        [XmlArray("components")]
        [XmlArrayItem("component", typeof(XmlQfxBlock))]
        public List<object> Blocks = new List<object>();

        /// <summary>
        /// The list of FIX field definitions that are contained
        /// in the field definition section of the dictionary.
        /// </summary>
        [XmlArray("fields")]
        [XmlArrayItem("field", typeof(XmlQfxField))]
        public List<object> Fields = new List<object>();


        /// <summary>
        /// The list of FIX element references that the trailer
        /// section of the data dictionary is composed of.
        /// </summary>
        [XmlArray("trailer")]
        [XmlArrayItem("field", typeof(XmlQfxFieldReference))]
        [XmlArrayItem("group", typeof(XmlQfxGroupReference))]
        [XmlArrayItem("component", typeof(XmlQfxBlockReference))]
        public List<object> Trailer = new List<object>();

    }

    /// <summary>
    /// The XmlQfxField class provides a representation
    /// of a QuickFix field definition in a format that 
    /// is compatible with the .NET XML serializer.
    /// </summary>
    public class XmlQfxField
    {
        /// <summary>
        /// The field's FIX tag.
        /// </summary>
        [XmlAttribute("number")]
        public string Number;

        /// <summary>
        /// The field's FIX name.
        /// </summary>
        [XmlAttribute("name")]
        public string Name;

        /// <summary>
        /// The field's FIX data type.
        /// </summary>
        [XmlAttribute("type")]
        public string Type;

        /// <summary>
        /// The set of enumerated values that are associated
        /// with the field definition; QuickFix doesn't use a
        /// separate section for enumerations, instead defining
        /// them in-place within each field...
        /// </summary>
        [XmlElement("value", typeof(XmlQfxFieldEnumerator))]
        public List<object> Enumeration = new List<object>();

    }

    /// <summary>
    /// The XmlQfxFieldEnumerator class provides a representation
    /// of a QuickFix enumerator that is compatible for use with
    /// the .NET XML serializer.
    /// </summary>
    public class XmlQfxFieldEnumerator
    {
        /// <summary>
        /// The enumerator's value.
        /// </summary>
        [XmlAttribute("enum")]
        public string Enum;

        /// <summary>
        /// The enumerator's description.
        /// </summary>
        [XmlAttribute("description")]
        public string Description;
    }

    /// <summary>
    /// The XmlQfxFieldReference class provides a representation
    /// of a QuickFix field reference that is compatible for use
    /// with the .NET XML serializer.
    /// </summary>
    public class XmlQfxFieldReference
    {
        /// <summary>
        /// The name of the field the reference relates to.
        /// </summary>
        [XmlAttribute("name")]
        public string Name;

        /// <summary>
        /// The indication as to whether or not the referenced
        /// field is a required component of its parent element.
        /// </summary>
        [XmlAttribute("required")]
        public string Required;
    }

    /// <summary>
    /// The XmlQfxBlock class provides a representation of
    /// a QuickFix component block definition in a format that
    /// is compatible with the .NET XML serializer.
    /// </summary>
    public class XmlQfxBlock
    {
        /// <summary>
        /// The component block's name.
        /// </summary>
        [XmlAttribute("name")]
        public string Name;

        /// <summary>
        /// The list of elements that the component block
        /// is composed of.
        /// </summary>
        [XmlElement("field", typeof(XmlQfxFieldReference))]
        [XmlElement("group", typeof(XmlQfxGroupReference))]
        [XmlElement("block", typeof(XmlQfxBlockReference))]
        public List<object> Elements = new List<object>();

    }

    /// <summary>
    /// The XmlQfxBlockReference class provides a representation
    /// of a QuickFix component block reference in a format that
    /// is compatible for use with the .NET XML serializer.
    /// </summary>
    public class XmlQfxBlockReference
    {
        /// <summary>
        /// The name of the component block being referenced.
        /// </summary>
        [XmlAttribute("name")]
        public string Name;

        /// <summary>
        /// Indicates whether or not the referenced block is
        /// a required component of its parent element.
        /// </summary>
        [XmlAttribute("required")]
        public string Required;
    }

    /// <summary>
    /// The XmlQfxMessage class provides a representation of
    /// a QuickFix message definition that is compatible for
    /// use with the .NET XML serializer.
    /// </summary>
    public class XmlQfxMessage
    {
        /// <summary>
        /// The message's FIX name.
        /// </summary>
        [XmlAttribute("name")]
        public string Name;

        /// <summary>
        /// The message's FIX message type.
        /// </summary>
        [XmlAttribute("msgtype")]
        public string MsgType;

        /// <summary>
        /// The message's FIX message category.
        /// </summary>
        [XmlAttribute("msgcat")]
        public string MsgCategory;

        /// <summary>
        /// The list of FIX elements that belong to the
        /// message definition.
        /// </summary>
        [XmlElement("field", typeof(XmlQfxFieldReference))]
        [XmlElement("group", typeof(XmlQfxGroupReference))]
        [XmlElement("component", typeof(XmlQfxBlockReference))]
        public List<object> Elements = new List<object>();
    }


    /// <summary>
    /// The XmlQfxGroupReference class provides a representation
    /// of a QuickFix group definition that is compatible for use
    /// with the .NET XML serializer.
    /// </summary>
    public class XmlQfxGroupReference
    {
        /// <summary>
        /// The name of the FIX field that represents the root
        /// tag for the repeating group.
        /// </summary>
        [XmlAttribute("name")]
        public string Name;

        /// <summary>
        /// Indicates whether or not the repeating group is a
        /// required component of its parent element.
        /// </summary>
        [XmlAttribute("required")]
        public string Required;

        /// <summary>
        /// The list of all elements that are components of
        /// the repeating group definition.
        /// </summary>
        [XmlElement("field", typeof(XmlQfxFieldReference))]
        [XmlElement("group", typeof(XmlQfxGroupReference))]
        [XmlElement("component", typeof(XmlQfxBlockReference))]
        public List<object> Elements = new List<object>();
    }
}
