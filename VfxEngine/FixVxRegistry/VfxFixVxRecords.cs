using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

using System.Xml;
using System.Xml.Serialization;

namespace VfxEngine.FixVxRegistry
{
    /// <summary>
    /// The VfxFixVxRecords class is a simple representation of
    /// a container for a list of FIX version definitions which
    /// is compatible with the .NET XML serializer. This allows
    /// an application to easily serializer and deserialize all
    /// of its associated version definitions from a file.
    /// </summary>
    [XmlRoot("versions")]
    public class VfxFixVxRecords
    {
        // REC: The Elements property provides access to the
        // collection of FIX version definitions that relate
        // to the application. These are typically read from
        // a configuration file, when a VersaFix application
        // is first started.
        [XmlElement("version")]
        public List<VfxFixVxRecord> Elements { get; set; }
    }
}
