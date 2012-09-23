using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Xml;
using System.Xml.XPath;

namespace VfxEngine.Settings
{
    /// <summary>
    /// The IVfxSettings interface defines an interface that is
    /// implemented to provide access to configuration settings
    /// that are stored in an XML document. 
    /// </summary>
    public interface IVfxSettings
    {
        XmlDocument Document { get; }
    }


    /// <summary>
    /// The VfxConfiguration class encapsulates an instance
    /// of an XmlDocument that contains VfxEngine configuration
    /// settings for use by an application.
    /// </summary>
    public class VfxSettings : IVfxSettings
    {
        /// <summary>
        /// The Document property provides access to an instance
        /// of an XML document that contains all of the configuration
        /// settings for the application.
        /// </summary>
        public XmlDocument Document { get; private set; }

        /// <summary>
        /// Initializes a new instance of the class with a reference
        /// to a new XML document that can be used to construct a new
        /// representation of the application's configuration.
        /// </summary>
        public VfxSettings()
        {
            this.Document = new XmlDocument();
        }

        /// <summary>
        /// Initializes a new instance of the class with a reference
        /// to an existing XML document that contains the application
        /// configuration information.
        /// </summary>
        /// <param name="source">
        /// A reference to an existing instance of an XML document that
        /// will be assigned to the instance's 'Document' property.
        /// </param>
        public VfxSettings(XmlDocument source)
        {
            this.Document = source;
        }
    }
}
