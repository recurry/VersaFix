using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Xml;
using System.Xml.Serialization;

namespace VfxEngine.FixVxRegistry
{
    /// <summary>
    /// The VfxFixVxRecord encapsulates all of the details for
    /// a single instance of a FIX version definition, which is
    /// used by VersaFix to maintain the details that relate to
    /// a specific version of the FIX protocol.
    /// </summary>
    [XmlRoot("version")]
    public class VfxFixVxRecord
    {
        /// <summary>
        /// The name of the specific FIX version that the
        /// version definition relates to.
        /// </summary>
        [XmlAttribute("name")]
        public string Name { get; set; }

        /// <summary>
        /// The type of the specific layer of the session that
        /// the version definition relates to.
        /// </summary>
        [XmlAttribute("layer")]
        public string Layer { get; set; }

        /// <summary>
        /// The FIX identifier that is used in tag 8 of a message
        /// to identity this version of the protocol.
        /// </summary>
        [XmlAttribute("bs")]
        public string FixBs { get; set; }

        /// <summary>
        /// The FIX ApplVerID enumeration value that corresponds
        /// to the protocol version the definition represents.
        /// </summary>
        [XmlAttribute("id")]
        public string FixId { get; set; }

        /// <summary>
        /// The pattern matching rules that can be used by a
        /// component of the system to determine whether or not
        /// a specific message should be evaluated in the context
        /// of this version definition.
        /// </summary>
        [XmlArray("rules")]
        [XmlArrayItem("rule", typeof(VfxFixVersion_Rule))]
        public List<VfxFixVersion_Rule> Rules { get; set; }
        
        /// <summary>
        /// The collection of FIX data dictionaries that are
        /// considered to be relevant to this version.
        /// </summary>
        [XmlArray("dictionaries")]
        [XmlArrayItem("dictionary", typeof(VfxFixVersion_Dictionary_Reference))]
        public List<VfxFixVersion_Dictionary_Reference> Dictionaries { get; set; }
    }

    [XmlRoot("rule")]
    public class VfxFixVersion_Rule
    {
        [XmlAttribute("name")]
        public string Name { get; set; }

        [XmlElement("match")]
        public List<VfxFixVersion_Field> Fields { get; set; }
    }


    [XmlRoot("match")]
    public class VfxFixVersion_Field
    {
        [XmlAttribute("tag")]
        public string Tag { get; set; }

        //[XmlAttribute("name")]
        //public string Name { get; set; }

        [XmlAttribute("value")]
        public string Value { get; set; }
    }

    [XmlRoot("dictionary")]
    public class VfxFixVersion_Dictionary_Reference
    {
        [XmlAttribute("name")]
        public string Name { get; set; }
    }
}
