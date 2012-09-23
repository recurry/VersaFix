using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VfxEngine.Fix
{
    /// <summary>
    /// The IFixDxFactory interface defines an interface for
    /// creating new instances of a FIX data dictionary.
    /// </summary>
    public interface IFixDxFactory
    {
        /// <summary>
        /// The Create method is invoked to request that the
        /// implementation of the interface create a new instance
        /// of a FIX data dictionary.
        /// </summary>
        /// <returns>
        /// A new instance of a VersaFix data dictionary.
        /// </returns>
        FixDictionary Create();
    }
}
