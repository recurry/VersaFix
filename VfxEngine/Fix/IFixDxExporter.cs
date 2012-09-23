using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VfxEngine.Fix
{
    /// <summary>
    /// The IFixDxExporter interface defines an interface
    /// for exporting an instance of a FIX data dictionary
    /// out to a file or .NET stream.
    /// </summary>
    public interface IFixDxExporter
    {
        /// <summary>
        /// The Export method is invoked to export the contents
        /// of a FIX data dictionary out to a specific file.
        /// </summary>
        /// <param name="dictionary">
        /// The FIX data dictionary to be exported.
        /// </param>
        /// <param name="path">
        /// The file the dictionary's data is to be written to.
        /// </param>
        void Export(FixDictionary dictionary, string path);

        /// <summary>
        /// The Export method is invoked to export the contents
        /// of a FIX data dictionary out to a specific stream.
        /// </summary>
        /// <param name="dictionary">
        /// The FIX data dictionary to be exported.
        /// </param>
        /// <param name="stream">
        /// The stream the dictionary's data is to be written to.
        /// </param>
        void Export(FixDictionary dictionary, Stream stream);
    }
}
