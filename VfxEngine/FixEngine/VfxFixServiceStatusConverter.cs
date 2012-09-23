using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VfxEngine.FixEngine
{
    /// <summary>
    /// The VfxFixServiceStatusConverter class provides a helper method
    /// to convert an instance of the VfxFixServiceStatus enumeration to
    /// a corresponding text string that is more presentable to a user.
    /// </summary>
    public sealed class VfxFixServiceStatusConverter
    {
        /// <summary>
        /// The ToString method is invoked to convert an instance
        /// of the VfxFixServerStatus enumeration into a corresponding
        /// string representation that can be displayed to an end user
        /// or written to a log file.
        /// </summary>
        /// <param name="status"></param>
        /// <returns></returns>
        public static string ToString(VfxFixServiceStatus status)
        {
            string result = "undefined";

            switch (status)
            {
                case VfxFixServiceStatus.Service_Status_Opened:
                    result = "Started";
                    break;
                case VfxFixServiceStatus.Service_Status_Closing:
                    result = "Stopping";
                    break;
                case VfxFixServiceStatus.Service_Status_Closed:
                    result = "Stopped";
                    break;
                case VfxFixServiceStatus.Service_Status_Connecting:
                    result = "Connecting";
                    break;
                case VfxFixServiceStatus.Service_Status_Disconnected:
                    result = "Disconnected";
                    break;
                case VfxFixServiceStatus.Service_Status_Sleeping:
                    result = "Sleeping";
                    break;
            }

            return result;
        }
    }
}
