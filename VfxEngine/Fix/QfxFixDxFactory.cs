using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VfxEngine.Fix
{
    /// <summary>
    /// The QfxFixDxFactory class provides an implementation of
    /// the interface IFixDxFactory for creating a new instance
    /// of a VersaFix data dictionary that has been initialized
    /// with the metadata properties for use with QuickFix.
    /// </summary>
    public class QfxFixDxFactory : IFixDxFactory
    {
        #region IFixDxFactory Members

        public FixDictionary Create()
        {
            FixDictionary result = new FixDictionary();

            result.Properties.Add("Type", "Not Specified");
            result.Properties.Add("Fix.Major", "Not Specified");
            result.Properties.Add("Fix.Minor", "Not Specified");

            return result;
        }

        #endregion
    }
}
