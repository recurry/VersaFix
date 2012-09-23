using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VfxEngine.Fix
{
    /// <summary>
    /// The IFixDxImporter interface defines an interface for
    /// importing FIX data dictionaries from either a file or
    /// a stream, into a VersaFix compatible dictionary.
    /// </summary>
    public interface IFixDxImporter
    {
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
        FixDictionary Import(string path);

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
        FixDictionary Import(Stream stream);
    }
}
