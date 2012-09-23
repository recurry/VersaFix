//************************************************************************
// The VersaFix FIX Engine. 
// Copyright (c) 2009 Assimilate Technology, Inc.
//------------------------------------------------------------------------
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with this program. If not, see <http://www.gnu.org/licenses/>.
//************************************************************************
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace VfxEngine.Fix
{
    /// <summary>
    /// The IFixDxElement interface defines the base interface
    /// for all types of element that can exist in an instance
    /// of a FIX dictionary.
    /// </summary>
    public interface IFixDxElement
    {
        /// <summary>
        /// The Tag method retrieves the element's FIX tag
        /// or throws a NotSupportedException if the element
        /// implementation doesn't include a FIX tag.
        /// </summary>
        /// <returns>
        /// The FIX tag associated with the element, or the
        /// method throws a NotSupportedException.
        /// </returns>
        int Tag { get; }

        /// <summary>
        /// The HasTag property provides access to a boolean
        /// that indicates if the element has a FIX tag that
        /// is associated with it.
        /// </summary>
        bool HasTag { get; }

        /// <summary>
        /// The Name method retrieves the element's FIX name
        /// or throws a NotSupportedException if the element
        /// implementation doesn't include a FIX name.
        /// </summary>
        /// <returns>
        /// The FIX name associated with the element, or the
        /// method throws a NotSupportedException.
        /// </returns>
        string Name { get; }


        /// <summary>
        /// The Required property provides access to the boolean
        /// that indicates whether or not it is a required element
        /// in a FIX message. If the element implementation doesn't
        /// support this property, it throws NotSupportedException.
        /// </summary>
        bool Required { get; }
    }

    /// <summary>
    /// The FixDictionary class encapsulates all of the information
    /// that is found in a FIX data dictionary. Dictionaries can be
    /// read and written in XML format with the VfxFixDxImporter or
    /// FixFixDxExporter classes. For performance reasons, there is
    /// no support for concurrent read/write access to the contents
    /// of the dictionary. If an app needs to modify the dictionary
    /// content at runtime, the recommended way of doing this is to
    /// use an instance of the VfxFixDxRegistry class to manage the
    /// dictionary instances; create a copy of the dictionary that
    /// is to be modified, make the necessary changes, and then put
    /// the dictionary back in the registry.
    /// </summary>
    public class FixDictionary : ICloneable
    {
        // REC: The collection of metadata properties that have
        // been assigned to an instance of a dictionary:
        private Dictionary<string, string> _mapProperties = new Dictionary<string, string>();

        /// <summary>
        /// The Properties property provides access to the collection
        /// of metadata settings that are associated with an instance
        /// of a dictionary. 
        /// </summary>
        public Dictionary<string, string> Properties
        {
            get { return _mapProperties; }
        }

        // REC: The collection of elements for the message header:
        //private FixDxCollection _hdrCollection = new FixDxCollection();

        private FixDxCollection _hdrCollection = new FixDxCollection();

        /// <summary>
        /// The collection of all FIX elements that are defined
        /// as members of the header for a FIX message.
        /// </summary>
        public FixDxCollection Header
        {
            get { return _hdrCollection; }
        }

        // REC: The map of all the message definitions
        // in the dictionary, keyed by their type:
        private Dictionary<string, FixDxMessage> _mapMessages = new Dictionary<string, FixDxMessage>();

        // REC: The collection of elements for the message trailer:
        private FixDxCollection _trlCollection = new FixDxCollection();

        /// <summary>
        /// The collection of all FIX elements that are defined
        /// as members of the trailer for a FIX message.
        /// </summary>
        public FixDxCollection Trailer
        {
            get { return _trlCollection; }
        }

        // REC: The list of all FIX field definitions
        // that are contained within the dictionary:
        private Collection<FixDxField> _fields = new Collection<FixDxField>();

        /// <summary>
        /// The FIX element collection that contains all of the
        /// field definitions in the dictionary:
        /// </summary>
        private FixDxCollection _fldElements = new FixDxCollection();

        /// <summary>
        /// The Fields property provides direct access to the collection
        /// of field definitions that are contained within the dictionary.
        /// </summary>
        public FixDxCollection Fields
        {
            get { return _fldElements; }
        }


        /// <summary>
        /// The FIX element collection that contains all of the
        /// data type definitions in the dictionary.
        /// </summary>
        private FixDxCollection _typeElements = new FixDxCollection();

        /// <summary>
        /// The DataTypes property provides direct access to the collection
        /// of data type definitions that are contained within the dictionary.
        /// </summary>
        public FixDxCollection DataTypes
        {
            get { return _typeElements; }
        }

        /// <summary>
        /// The FIX element collection that contains all of the
        /// enumeration definitions in the dictionary:
        /// </summary>
        private FixDxCollection _enmElements = new FixDxCollection();

        /// <summary>
        /// The Enums property provides direct access to the collection
        /// of enumeration definitions that are contained in the dictionary.
        /// </summary>
        public FixDxCollection Enums
        {
            get { return _enmElements; }
        }

        // REC: The map of all FIX field definitions that
        // are in the collection, keyed by their FIX tag:
        private Dictionary<int, FixDxField> _mapFieldsByTag = new Dictionary<int, FixDxField>();

        // REC: The map of all FIX field definitions that
        // are in the collection, keyed by their names:
        private Dictionary<string, FixDxField> _mapFieldsByName = new Dictionary<string, FixDxField>();

        // REC: The list of all component block definitions
        // that are contained within the dictionary:
        private FixDxCollection _blkElements = new FixDxCollection();

        /// <summary>
        /// The Blocks property provides access to the list of
        /// component blocks that are defined in the dictionary.
        /// </summary>
        public FixDxCollection Blocks
        {
            get { return _blkElements; }
        }


        // REC: The map of all FIX component block definitions
        // that are in the dictionary, keyed by their names:
        private Dictionary<string, FixDxBlock> _mapBlocksByName = new Dictionary<string, FixDxBlock>();

        /// <summary>
        /// The collection of all FIX message definitions that are
        /// contained in the dictionary instance.
        /// </summary>
        private FixDxCollection _messages = new FixDxCollection();

        /// <summary>
        /// The Messages property provides direct access to the 
        /// element collection that contains all of the message
        /// definitions in the dictionary.
        /// </summary>
        public FixDxCollection Messages
        {
            get { return _messages; }
        }

        // REC: The map of all FIX message definitions that
        // are contained in the dictionary, keyed by type:
        private Dictionary<string, FixDxMessage> _mapMessagesByType = new Dictionary<string, FixDxMessage>();

        // REC: The map of all FIX message definitions that
        // are contained in the dictionary, keyed by name:
        private Dictionary<string, FixDxMessage> _mapMessagesByName = new Dictionary<string, FixDxMessage>();

        // REC: The map of all FIX enumerations that are defined
        // in the dictionary, keyed by their unique identifier:
        private Dictionary<string, FixDxEnumeration> _mapEnumerations = new Dictionary<string, FixDxEnumeration>();

        /// <summary>
        /// Initializes a new instance of the class with the
        /// appropriate default settings and parameters.
        /// </summary>
        public FixDictionary()
        {

        }

        /// <summary>
        /// The AddField method attempts to add an instance of
        /// a FIX field definition to the dictionary. If there
        /// is already an entry in the dictionary that has the
        /// same identifying criteria the ArgumentException is
        /// thrown and the method fails; It's not possible for 
        /// the dictionary to replace a redundant field, since 
        /// there may be an entry in the name-based index that
        /// corresponds to a field with a *different* tag.
        /// </summary>
        /// <param name="dxField">
        /// The FIX field definition that is to be added.
        /// </param>
        public void AddField(FixDxField dxField)
        {
            if (_mapFieldsByTag.ContainsKey(dxField.Tag))
            {
                string error = string.Format("Dictionary already contains a field with tag {0}.", dxField.Tag);
                throw new ArgumentException(error);
            }

            if (_mapFieldsByName.ContainsKey(dxField.Name))
            {
                string error = string.Format("Dictionary already contains a field named {0}.", dxField.Name);
                throw new ArgumentException(error);
            }

            _mapFieldsByTag.Add(dxField.Tag, dxField);
            _mapFieldsByName.Add(dxField.Name, dxField);

            _fields.Add(dxField);
            
            _fldElements.Add(dxField);
        }

        /// <summary>
        /// The AddBlock method adds an instance of a FIX component
        /// block to the dictionary. If there is already an instance
        /// of a component block with the same name, an operation is
        /// not allowed and an ArgumentException is thrown.
        /// </summary>
        /// <param name="dxBlock">
        /// The FIX component block definition that is to be added.
        /// </param>
        public void AddBlock(FixDxBlock dxBlock)
        {
            if (_mapBlocksByName.ContainsKey(dxBlock.Name))
            {
                string error = string.Format("Dictionary already contains a block named {0}.", dxBlock.Name);
                throw new ArgumentException(error);
            }

            _mapBlocksByName.Add(dxBlock.Name, dxBlock);
            _blkElements.Add(dxBlock);
        }

        /// <summary>
        /// The GetBlockByName method attempts to lookup a block
        /// based on the text name that is assigned to it.
        /// </summary>
        /// <param name="name">
        /// The name of the component block to retrieve.
        /// </param>
        /// <returns>
        /// The corresponding component block, or null if there is
        /// no matching entry in the dictionary.
        /// </returns>
        public FixDxBlock GetBlockByName(string name)
        {
            FixDxBlock result = null;
            if (_mapBlocksByName.ContainsKey(name))
            {
                result = _mapBlocksByName[name];
            }
            return result;
        }

        /// <summary>
        /// The AddMessage method adds a new instance of a 
        /// message definition to the dictionary.
        /// </summary>
        /// <param name="dxMessage">
        /// The FIX message definition that is to be added
        /// to the dictionary.
        /// </param>
        public void AddMessage(FixDxMessage dxMessage)
        {
            if (_mapMessagesByName.ContainsKey(dxMessage.Name))
            {
                string error = string.Format("Dictionary already contains a message named {0}.", dxMessage.Name);
                throw new ArgumentException(error);
            }

            if (_mapMessagesByType.ContainsKey(dxMessage.MsgType))
            {
                string error = string.Format("Dictionary already contains a message type {0}.", dxMessage.MsgType);
                throw new ArgumentException(error);
            }

            _messages.Add(dxMessage);
            _mapMessagesByType.Add(dxMessage.MsgType, dxMessage);
            _mapMessagesByName.Add(dxMessage.Name, dxMessage);
        }

        /// <summary>
        /// The GetMessageByName method attempts to retrieve the
        /// message definition associated with a specific name.
        /// </summary>
        /// <param name="msgName">
        /// The name of the message definition to retrieve.
        /// </param>
        /// <returns>
        /// The message definition associated with the specified
        /// name, or null if there is no matching message.
        /// </returns>
        public FixDxMessage GetMessageByName(string msgName)
        {
            FixDxMessage result = null;
            if (_mapMessagesByName.ContainsKey(msgName))
            {
                result = _mapMessagesByName[msgName];
            }
            return result;
        }

        /// <summary>
        /// The GetMessageByType method attempts to retrieve the
        /// message definition associated with a specific type.
        /// </summary>
        /// <param name="msgType">
        /// The FIX message type (MsgType) of the message that
        /// is to be retrieved.
        /// </param>
        /// <returns>
        /// The message definition associated with the specified
        /// type, or null if there is no matching message.
        /// </returns>
        public FixDxMessage GetMessageByType(string msgType)
        {
            FixDxMessage result = null;
            if (_mapMessagesByType.ContainsKey(msgType))
            {
                result = _mapMessagesByType[msgType];
            }
            return result;
        }

        /// <summary>
        /// The AddEnumeration method adds an instance of an
        /// enumeration to the dictionary. Enumerations are keyed
        /// by their unique identifier, so if the enumeration that
        /// is being added collides with one that is already in the
        /// dictionary, the operation will fail.
        /// </summary>
        /// <param name="enumeration">
        /// The enumeration to be added to the dictionary.
        /// </param>
        /// <exception cref="ArgumentException">
        /// Thrown if the enumeration's name is null or blank or
        /// if there is already an enumeration with the specified
        /// name in the dictionary.
        /// </exception>
        public void AddEnumeration(FixDxEnumeration enumeration)
        {
            if (!string.IsNullOrEmpty(enumeration.Name))
            {
                if (!_mapEnumerations.ContainsKey(enumeration.Name))
                {
                    _mapEnumerations.Add(enumeration.Name, enumeration);
                }
                else
                {
                    string error = string.Format("Dictionary already contains an enumeration named {0}.", enumeration.Name);
                    throw new ArgumentException(error);
                }

                _enmElements.Add(enumeration);
            }
            else
            {
                throw new ArgumentException("The enumeration's name is not specified.");
            }
        }

        


        /// <summary>
        /// The Resolve method attempts to resolve all of the
        /// element references in a collection to the entries
        /// that correspond to them. 
        /// </summary>
        /// <param name="elements">
        /// The collection of dictionary elements to resolve.
        /// </param>
        /// <returns>
        /// The resulting collection of resolved elements.
        /// </returns>
        /// <exception cref="ArgumentException">
        /// Thrown if any elements in the collection that is
        /// supplied to the method cannot be resolved.
        /// </exception>
        public FixDxCollection Resolve(FixDxCollection elements)
        {
            FixDxCollection result = new FixDxCollection();

            // REC: Iterate over all of the elements in the collection
            // and determine how to resolve each of them:
            foreach (IFixDxElement dxElement in elements)
            {
                if (dxElement is FixDxFieldReference)
                {
                    FixDxFieldReference fieldReference = dxElement as FixDxFieldReference;
                    result.Add(ResolveFieldReference(fieldReference));
                }
                else if (dxElement is FixDxGroupReference)
                {
                    FixDxGroupReference groupReference = dxElement as FixDxGroupReference;
                    result.Add(ResolveGroupReference(groupReference));
                }
                else if (dxElement is FixDxBlockReference)
                {
                    FixDxBlockReference blockReference = dxElement as FixDxBlockReference;
                    // REC: Determine what type of block the reference
                    // is referring to (component or repeating):
                    if (string.IsNullOrEmpty(blockReference.Name))
                    {
                        string error = "The supplied block reference's name is null or empty.";
                        throw new ArgumentException(error);
                    }
                    else if (!_mapBlocksByName.ContainsKey(blockReference.Name))
                    {
                        string error = string.Format("The block reference {0} couldn't be resolved.", blockReference.Name);
                        throw new ArgumentException(error);
                    }
                    else
                    {
                        FixDxBlock dxBlock = _mapBlocksByName[blockReference.Name];
                        if (dxBlock.Type == FixDxBlockTypes.Component)
                        {
                            foreach (IFixDxElement element in Resolve(Expand(dxBlock.Elements)))
                            {
                                result.Add(element);
                            }
                        }
                        else if (dxBlock.Type == FixDxBlockTypes.Repeating)
                        {
                            // REC: Attempt to resolve the field that the repeating
                            // block references as the start field for the group:
                            if (string.IsNullOrEmpty(dxBlock.Field))
                            {
                                string error = string.Format("Repeating Block {0}'s start field is null or empty.", dxBlock.Field);
                                throw new ArgumentException(error);
                            }
                            else if (!_mapFieldsByName.ContainsKey(dxBlock.Field))
                            {
                                string error = string.Format("Repeating block {0}'s start field can't be resolved.", dxBlock.Field);
                                throw new ArgumentException(error);
                            }
                            else
                            {
                                FixDxField dxField = _mapFieldsByName[dxBlock.Field];
                                FixDxResolvedGroup dxGroup = new FixDxResolvedGroup(dxField.Tag, dxField.Name, false);
                                foreach (IFixDxElement element in Resolve(Expand(dxBlock.Elements)))
                                {
                                    dxGroup.Elements.Add(element);
                                }

                                result.Add(dxGroup);
                            }
                        }
                    }

                }
            }

            // REC: Patch from RC - sanity check all elements in the result
            // to ensure that there are no unresolved references.
            foreach (IFixDxElement e in result)
            {
                if (e is FixDxFieldReference || e is FixDxGroupReference || e is FixDxFieldReference)
                {
                    throw new Exception("unresolved references exist in the resolved collection");
                }
            }

            return result;
        }

        /// <summary>
        /// The Expand method iterates over all of the elements in
        /// a collection of dictionary elements and expands any of
        /// them that reference collections of other elements.
        /// </summary>
        /// <param name="source">
        /// The collection of elements to expand.
        /// </param>
        /// <returns>
        /// A new instance of a collection of dictionary elements
        /// with all expandable references in the source collection
        /// having been recursively expanded.
        /// </returns>
        public FixDxCollection Expand(FixDxCollection source)
        {
            FixDxCollection result = new FixDxCollection();

            foreach (IFixDxElement dxEntry in source)
            {
                if (dxEntry is FixDxBlockReference)
                {
                    FixDxBlockReference blockReference = dxEntry as FixDxBlockReference;
                    if (blockReference != null)
                    {
                        if (_mapBlocksByName.ContainsKey(blockReference.Name))
                        {
                            FixDxBlock blockEntry = _mapBlocksByName[blockReference.Name];
                            FixDxCollection blockElements = Expand(blockEntry.Elements);
                            foreach (IFixDxElement blockElement in blockElements)
                            {
                                if (blockElement is FixDxFieldReference)
                                {
                                    FixDxFieldReference reference = blockElement as FixDxFieldReference;
                                    result.Add(reference);
                                }
                                else if (blockElement is FixDxBlockReference)
                                {
                                    FixDxBlockReference reference = blockElement as FixDxBlockReference;
                                    result.Add(reference);
                                }
                            }
                        }
                    }
                }
                else
                {
                    if (dxEntry is FixDxGroupReference)
                    {
                        FixDxGroupReference srcReference = dxEntry as FixDxGroupReference;
                        FixDxGroupReference dstReference = new FixDxGroupReference(srcReference.Name, srcReference.Required);
                        foreach (IFixDxElement srcElement in Expand(srcReference.Elements))
                        {
                            dstReference.Elements.Add(srcElement);
                        }

                        result.Add(dstReference);
                    }
                    else if (dxEntry is FixDxFieldReference)
                    {
                        FixDxFieldReference element = dxEntry as FixDxFieldReference;
                        result.Add(element);
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// The ResolveFieldName function resolves a field reference
        /// against the information in the dictionary.
        /// </summary>
        /// <param name="reference">
        /// The field reference to resolve.
        /// </param>
        /// <returns>
        /// An instance of FixDxResolvedField that contains all of
        /// the resolved information about the field.
        /// </returns>
        private FixDxResolvedField ResolveFieldReference(FixDxFieldReference reference)
        {
            if (string.IsNullOrEmpty(reference.Name))
            {
                string error = "The supplied field reference has an empty or null name.";
                throw new ArgumentException(error);
            }
            else
            {
                if (!_mapFieldsByName.ContainsKey(reference.Name))
                {
                    string error = string.Format("The field reference {0} could not be resolved!", reference.Name);
                    throw new ArgumentException(error);
                }
                else
                {
                    FixDxField dxField = _mapFieldsByName[reference.Name];
                    if (dxField.LengthCoded == false)
                    {
                        FixDxResolvedField result = new FixDxResolvedField(dxField.Tag, dxField.Name,
                            dxField.Type, reference.Required);

                        return result;
                    }
                    else
                    {
                        // REC: The field is length encoded, so the field that
                        // contains the content length for this field also has
                        // to be resolved from the dictionary:
                        if (_mapFieldsByName.ContainsKey(dxField.LengthField))
                        {
                            FixDxField dxLength = _mapFieldsByName[dxField.LengthField];
                            FixDxResolvedField result = new FixDxResolvedField(dxField.Tag, dxField.Name,
                                dxField.Type, dxLength.Tag, reference.Required);

                            return result;
                        }
                        else
                        {
                            string error = string.Format("The field reference {0}'s length field {1} couldn't be resolved!",
                                dxField.Name, dxField.LengthField);
                         
                            throw new ArgumentException(error);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// The ResolveGroupReference function resolves a group
        /// reference against the information in the dictionary.
        /// </summary>
        /// <param name="reference">
        /// The group reference to resolve.
        /// </param>
        /// <returns>
        /// An instance of FixDxResolvedGroup that contains all of
        /// the resolved information about the group.
        /// </returns>
        private FixDxResolvedGroup ResolveGroupReference(FixDxGroupReference reference)
        {
            if (string.IsNullOrEmpty(reference.Name))
            {
                string error = "The supplied group reference's name is null or empty.";
                throw new ArgumentException(error);
            }
            else
            {
                if (!_mapFieldsByName.ContainsKey(reference.Name))
                {
                    string error = string.Format("Repeating group {0}'s field couldn't be resolved.", reference.Name);
                    throw new ArgumentException(error);
                }
                else
                {
                    FixDxField dxField = _mapFieldsByName[reference.Name];
                    FixDxResolvedGroup result = new FixDxResolvedGroup(dxField.Tag, dxField.Name, reference.Required);
                    foreach (IFixDxElement element in Resolve(reference.Elements))
                    {
                        result.Elements.Add(element);
                    }

                    return result;
                }
            }

        }

        #region ICloneable Members

        /// <summary>
        /// The Clone method is invoked to construct a completely
        /// new instance/copy of an instance of a dictionary.
        /// </summary>
        /// <returns></returns>
        public object Clone()
        {
            FixDictionary result = new FixDictionary();
            
            // REC: Copy all of the metadata properties into the
            // new instance of the dictionary:
            foreach (string key in _mapProperties.Keys)
            {
                result.Properties.Add(key, _mapProperties[key]);
            }

            // REC: Clone all of the header elements in this dictionary
            // and add them to the result dictionary:
            foreach (IFixDxElement hdrElement in _hdrCollection)
            {
                result.Header.Add(CloneElement(hdrElement));
            }
            
            // REC: Clone all of the field entries in this dictionary
            // and add them to the result dictionary:
            foreach (FixDxField dxField in _fldElements)
            {
                result.AddField(CloneField(dxField));
            }

            // REC: Clone all of the block entries in this dictionary
            // and add them to the result dictionary:
            foreach (FixDxBlock dxBlock in _blkElements)
            {
                result.AddBlock(CloneBlock(dxBlock));
            }

            // REC: Clone all of the message entries in this dictionary
            // and add them to the result dictionary:
            foreach (FixDxMessage dxMessage in _messages)
            {
                result.AddMessage(CloneMessage(dxMessage));
            }

            // REC: Clone all of the enum entries in this dictionary
            // and add them to the result dictionary:
            foreach (FixDxEnumeration dxEnumeration in _enmElements)
            {
                result.AddEnumeration(CloneEnumeration(dxEnumeration));
            }

            // REC: Clone all of the trailer entries in this dictionary
            // and add them to the result dictionary:
            foreach (IFixDxElement trlElement in _trlCollection)
            {
                result.Trailer.Add(CloneElement(trlElement));
            }
            
            return result;
        }

        /// <summary>
        /// The CloneElement method creates a copy of the specified
        /// dictionary element and returns it to the caller.
        /// </summary>
        /// <param name="src">
        /// The data dictionary element to be cloned.
        /// </param>
        /// <returns>
        /// The resulting clone of the supplied element.
        /// </returns>
        private IFixDxElement CloneElement(IFixDxElement src)
        {
            IFixDxElement result = null;
            if (src is FixDxField)
            {
                result = CloneField(src as FixDxField);
            }
            else if (src is FixDxFieldReference)
            {
                result = CloneFieldReference(src as FixDxFieldReference);
            }
            else if (src is FixDxBlock)
            {
                result = CloneBlock(src as FixDxBlock);
            }
            else if (src is FixDxBlockReference)
            {
                result = CloneBlockReference(src as FixDxBlockReference);
            }
            else if (src is FixDxGroupReference)
            {
                result = CloneGroupReference(src as FixDxGroupReference);
            }


            return result;
        }

        /// <summary>
        /// The CloneMessage method is invoked to create a copy of
        /// an instance of a message definition.
        /// </summary>
        /// <param name="source">
        /// The message definition to be duplicated.
        /// </param>
        /// <returns>
        /// The resulting clone of the supplied message.
        /// </returns>
        private FixDxMessage CloneMessage(FixDxMessage source)
        {
            FixDxMessage result = new FixDxMessage(source.MsgType, source.Name, source.MsgCategory);
            foreach(IFixDxElement dxElement in source.Elements)
            {
                result.Elements.Add(CloneElement(dxElement));
            }
            return result;
        }

        private FixDxEnumeration CloneEnumeration(FixDxEnumeration source)
        {
            FixDxEnumeration result = new FixDxEnumeration(source.Name);
            foreach (FixDxEnumerator dxEnumerator in source.Enumerators)
            {
                result.Enumerators.Add(new FixDxEnumerator(dxEnumerator.Value, dxEnumerator.Description));
            }

            return result;
        }

        /// <summary>
        /// The CloneField method creates a copy of an instance
        /// of a dictionary field entry.
        /// </summary>
        /// <param name="source">
        /// The dictionary field entry to duplicate.
        /// </param>
        /// <returns>
        /// The resulting clone of the supplied element.
        /// </returns>
        private FixDxField CloneField(FixDxField source)
        {
            FixDxField result = new FixDxField(source.Tag, source.Name, source.Type);
            result.Enumeration = source.Enumeration;
            result.LengthCoded = source.LengthCoded;
            result.LengthField = source.LengthField;
            return result;
        }

        /// <summary>
        /// The CloneFieldReference method creates a copy of an
        /// instance of a dictionary field reference.
        /// </summary>
        /// <param name="source">
        /// The dictionary field reference to duplicate.
        /// </param>
        /// <returns>
        /// The resulting clone of the supplied reference.
        /// </returns>
        private FixDxFieldReference CloneFieldReference(FixDxFieldReference source)
        {
            FixDxFieldReference result = new FixDxFieldReference(source.Name, source.Required);
            return result;
        }

        /// <summary>
        /// The CloneGroupReference method creates a copy of an
        /// instance of a repeating group.
        /// </summary>
        /// <param name="source">
        /// The repeating group which is to be duplicated.
        /// </param>
        /// <returns>
        /// The resulting clone of the supplied group.
        /// </returns>
        private FixDxGroupReference CloneGroupReference(FixDxGroupReference source)
        {
            FixDxGroupReference result = new FixDxGroupReference(source.Name);
            foreach (IFixDxElement dxElement in source.Elements)
            {
                result.Elements.Add(CloneElement(dxElement));
            }
            return result;
        }

        /// <summary>
        /// The CloneBlock method is invoked to create a copy of an
        /// instance of a component block definition.
        /// </summary>
        /// <param name="source">
        /// The component block to be duplicated.
        /// </param>
        /// <returns>
        /// The resulting clone of the supplied block.
        /// </returns>
        private FixDxBlock CloneBlock(FixDxBlock source)
        {
            FixDxBlock result = new FixDxBlock(source.Name, source.Category, source.Field);
            foreach (IFixDxElement dxElement in source.Elements)
            {
                result.Elements.Add(CloneElement(dxElement));
            }
            return result;
        }

        /// <summary>
        /// The CloneBlockReference method is invoked to create a copy
        /// of an instance of a component block reference.
        /// </summary>
        /// <param name="source">
        /// The block reference that is to be duplicated.
        /// </param>
        /// <returns>
        /// The resulting clone of the supplied block reference.
        /// </returns>
        private FixDxBlockReference CloneBlockReference(FixDxBlockReference source)
        {
            FixDxBlockReference result = new FixDxBlockReference(source.Name, source.Required);
            return result;
        }

        #endregion
    }
}
