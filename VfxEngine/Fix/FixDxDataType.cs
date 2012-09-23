using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VfxEngine.Fix
{
    /// <summary>
    /// The FixDxDataType (Dx=Dictionary) class encapsulates the
    /// details of a data type definition from a FIX dictionary.
    /// </summary>
    public sealed class FixDxDataType : IFixDxElement
    {
        /// <summary>
        /// The name of the data type, as it is defined in
        /// the FIX data dictionary.
        /// </summary>
        private string _typeName;

        /// <summary>
        /// The name of the data type that serves as the base
        /// type for the data type definition.
        /// </summary>
        private string _typeBase;

        /// <summary>
        /// The BaseType property provides access to the name
        /// of the type's base data type.
        /// </summary>
        public string BaseType
        {
            get { return _typeBase; }
        }

        /// <summary>
        /// Initializes a new instance of the class with the
        /// specified parameters.
        /// </summary>
        /// <param name="typeName">
        /// The name of the data type.
        /// </param>
        public FixDxDataType(string typeName)
        {
            _typeName = typeName;
        }

        /// <summary>
        /// Initializes a new instance of the class with the
        /// specified parameters.
        /// </summary>
        /// <param name="typeName">
        /// The name of the data type.
        /// </param>
        /// <param name="typeBase">
        /// The name of the data type's base type.
        /// </param>
        public FixDxDataType(string typeName, string typeBase)
        {
            _typeName = typeName;
            _typeBase = typeBase;
        }

        #region IFixDxElement Members

        /// <summary>
        /// The Tag property is not implemented, since data type
        /// definitions do not have an associated FIX tag.
        /// </summary>
        public int Tag
        {
            get { throw new NotImplementedException(); }
        }

        /// <summary>
        /// The HasTag property is implemented to always return
        /// false, since data type definitions do not have a tag
        /// associated with them.
        /// </summary>
        public bool HasTag
        {
            get { return false;  }
        }

        /// <summary>
        /// The Name property provides access to the name of the
        /// data type as it is defined in the FIX dictionary.
        /// </summary>
        public string Name
        {
            get { return _typeName;  }
        }

        /// <summary>
        /// The Required property is not implemented, since data
        /// type definitions do not have a required flag.
        /// </summary>
        public bool Required
        {
            get { throw new NotImplementedException(); }
        }

        #endregion
    }
}
