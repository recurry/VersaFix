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

using VfxEngine.Fix;
using VfxEngine.FixVxRegistry;
using VfxEngine.FixDxRegistry;
using VfxEngine.Services;

namespace VfxEngine.FixAssembler
{
    /// <summary>
    /// The VfxFixAssembler class assembles an instance of a
    /// message based on the message definition that is found
    /// in a specific FIX dictionary.
    /// </summary>
    public class VfxFixAssembler
    {
        // REC: The FIX version registry that the assembler
        // relies on for locating the details of a specific
        // version of the FIX protocol:
        private IVfxFixVxRegistry _vxRegistry;

        /// <summary>
        /// The FIX dictionary registry that the assembler
        /// relies on for locating the dictionaries that are
        /// associated with specific FIX versions:
        /// </summary>
        private IVfxFixDxRegistry _dxRegistry;

        /// <summary>
        /// The internal map of all FIX fields that have been
        /// registered with the assembler for use in constructing
        /// a new instance of a FIX message.
        /// </summary>
        private Dictionary<int, FixField> _mapFields = new Dictionary<int, FixField>();

        /// <summary>
        /// The internal map of all FIX groups that have been
        /// registered with the assembler for use in constructing
        /// a new instance of a FIX message.
        /// </summary>
        private Dictionary<int, FixGroup> _mapGroups = new Dictionary<int, FixGroup>();

        /// <summary>
        /// The Init method intialized the message assembler
        /// with the services that are needed in order to create
        /// instances of FIX messages. The assembler requires that
        /// the IVfxFixVxRegistry and IVfxFixDxRegistry services be
        /// available from the provided service container.
        /// </summary>
        /// <param name="services">
        /// The service container that provides access to the services
        /// that the assembler needs in order to construct messages.
        /// </param>
        public void Init(IVfxServices services)
        {
            _vxRegistry = services.GetService(typeof(IVfxFixVxRegistry)) as IVfxFixVxRegistry;
            if (_vxRegistry == null)
            {
                throw new ArgumentException("VfxFixAssembler requires the IVfxFixVxRegistry service.");
            }

            _dxRegistry = services.GetService(typeof(IVfxFixDxRegistry)) as IVfxFixDxRegistry;
            if (_dxRegistry == null)
            {
                throw new ArgumentException("VfxFixAssembler requires the IVfxFixDxRegistry service.");
            }

        }

        /// <summary>
        /// The SetField method registers an instance of a field with 
        /// the assembler. The assembler will use this field whenever
        /// it constructs a message that contains the field's tag.
        /// </summary>
        /// <param name="field">
        /// The FIX field to register with the assembler.
        /// </param>
        public void SetField(FixField field)
        {
            if (!_mapFields.ContainsKey(field.Tag))
            {
                _mapFields.Add(field.Tag, field);
            }
            else
            {
                _mapFields[field.Tag] = field;
            }
        }

        /// <summary>
        /// The GetField method retrieves an instance of a field that
        /// has been configured in the assembler instance.
        /// </summary>
        /// <param name="tag">
        /// The FIX tag of the field to retrieve.
        /// </param>
        /// <returns>
        /// The instance of the FIX field that corresponds to the
        /// supplied tag, or null if there is no match.
        /// </returns>
        public FixField GetField(int tag)
        {
            FixField result = null;
            if (_mapFields.ContainsKey(tag))
            {
                result = _mapFields[tag];
            }
            return result;
        }

        /// <summary>
        /// The RemoveField method is invoked to remove an instance
        /// of a field that was previously added to the assembler.
        /// </summary>
        /// <param name="tag">
        /// The FIX tag of the field that is to be removed.
        /// </param>
        public void RemoveField(int tag)
        {
            if (_mapFields.ContainsKey(tag))
            {
                _mapFields.Remove(tag);
            }
        }

        /// <summary>
        /// The RegisterGroup method registers an instance of a 
        /// repeating group with the assembler. The assembler will
        /// use this group when it construct a message that contains
        /// a repeating group with this tag.
        /// </summary>
        /// <param name="group">
        /// The repeating group being registered.
        /// </param>
        public void RegisterGroup(FixGroup group)
        {
            if (!_mapGroups.ContainsKey(group.Tag))
            {
                _mapGroups.Add(group.Tag, group);
            }
            else
            {
                _mapGroups[group.Tag] = group;
            }
        }


        /// <summary>
        /// The PopulateHeader method populates all of the header
        /// elements in the specified message with the elements that
        /// are specified in a specific FIX version, using the values
        /// that have been set for those fields in the assembler. The
        /// method will replace the sort ordering for the target with
        /// the sort ordering that is found in the dictionary.
        /// </summary>
        /// <param name="sxVersion">
        /// The name of the session layer version that the elements
        /// for the message trailer will be retrieved from.
        /// </param>
        /// <param name="msg">
        /// The FIX message instance that is to be populated.
        /// </param>
        public void PopulateHeader(string sxVersion, FixMessage msg)
        {
            PopulateHdrElements(sxVersion, msg);
        }

        /// <summary>
        /// The PopulateTrailer method populates all of the trailer
        /// elements in the specified message with the elements that
        /// are specified in a specific FIX version, using the values
        /// that have been set for those fields in the assembler. The
        /// method will replace the sort ordering for the target with
        /// the sort ordering that is found in the dictionary.
        /// </summary>
        /// <param name="sxVersion">
        /// The name of the session layer version that the elements
        /// for the message trailer will be retrieved from.
        /// </param>
        /// <param name="msg">
        /// The FIX message instance that is to be populated.
        /// </param>
        public void PopulateTrailer(string sxVersion, FixMessage msg)
        {
            PopulateTrlElements(sxVersion, msg);
        }

        /// <summary>
        /// The CreateMessage method creates a new instance of
        /// a specific FIX message in accordance with the message
        /// definition in the dictionaries associated with the
        /// specified FIX application layer version. The message
        /// that is created by this implementation of the method
        /// will not contain a populated header or trailer.
        /// </summary>
        /// <param name="axVersion">
        /// The application layer version of the FIX protocol that
        /// will be used to locate the message definition for the
        /// message that is to be created.
        /// </param>
        /// <param name="msgType">
        /// The FIX message type of the message to create.
        /// </param>
        /// <returns></returns>
        public FixMessage CreateMessage(string axVersion, string msgType)
        {
            FixMessage result = new FixMessage();
            PopulateMsgElements(axVersion, msgType, result);
            return result;
        }


        /// <summary>
        /// The CreateMessage method creates a new instance of
        /// a specific FIX message in accordance with the message
        /// definition in the dictionaries associated with the
        /// specified FIX application layer version. The message
        /// that is created by this implementation of the method
        /// will also include a populated header and trailer that
        /// conforms to the header and trailer definition found in
        /// the specified session layer FIX version.
        /// </summary>
        /// <param name="sxVersion">
        /// The session layer version of the FIX protocol that will
        /// be used to locate message definitions for the message that
        /// is to be created.
        /// </param>
        /// <param name="axVersion">
        /// The application layer version of the FIX protocol that
        /// will be used to locate the message definition for the
        /// message that is to be created.
        /// </param>
        /// <param name="msgType">
        /// The FIX message type of the message to create.
        /// </param>
        /// <returns>
        /// The resulting instance of a FIX message.
        /// </returns>
        public FixMessage CreateMessage(string sxVersion, string axVersion, string msgType)
        {
            FixMessage result = new FixMessage();

            // REC: Determine if the MsgType field has already
            // been defined and maintain a reference so that it
            // can be restored after this is done:
            FixField fldMsgType = GetField(35);

            // REC: Assign the specified MsgType to the internal
            // field map so that it gets substituted:
            SetField(new FixField(35, msgType));

            PopulateHdrElements(sxVersion, result);
            PopulateMsgElements(axVersion, msgType, result);
            PopulateTrlElements(sxVersion, result);

            // REC: Restore the previous value for the
            // message type, if one was present:
            if (fldMsgType != null)
            {
                SetField(fldMsgType);
            }
            else
            {
                // REC: The MsgType field was not present before
                // this call was made, so remove it:
                RemoveField(35);
            }


            return result;
        }

        /// <summary>
        /// The PopulateHdrElements method populates the header section
        /// of a FIX message with the header elements from the specified
        /// version of the FIX protocol.
        /// </summary>
        /// <param name="version">
        /// The version of the FIX protocol that is used to populate the
        /// header elements of the message.
        /// </param>
        /// <param name="msg">
        /// The FIX message instance that is to be populated with all of
        /// the header elements from the specific FIX version.
        /// </param>
        private void PopulateHdrElements(string version, FixMessage msg)
        {
            VfxFixVxRecord vxDetails = _vxRegistry.Get(version);
            if (vxDetails != null)
            {
                // REC: Ensure that there is at least one FIX dictionary
                // assigned to the specified version definition:
                if (vxDetails.Dictionaries.Count > 0)
                {
                    FixDictionary dxEntry = _dxRegistry.GetEntry(vxDetails.Dictionaries[0].Name);
                    if (dxEntry != null)
                    {
                        FixDxCollection hdrElements = dxEntry.Resolve(dxEntry.Header);
                        if (hdrElements != null)
                        {
                            // REC: Create the sort ordering for the elements 
                            // and populate any fields and groups that have been
                            // registered with the assembler:
                            Collection<int> ordering = new Collection<int>();
                            foreach (IFixDxElement dxElement in hdrElements)
                            {
                                ordering.Add(dxElement.Tag);

                                if (dxElement is FixDxResolvedField)
                                {
                                    if (_mapFields.ContainsKey(dxElement.Tag))
                                    {
                                        msg.Header.SetField(_mapFields[dxElement.Tag]);
                                    }
                                }
                                else
                                {
                                    if (_mapGroups.ContainsKey(dxElement.Tag))
                                    {
                                        msg.Header.AddGroup(_mapGroups[dxElement.Tag]);
                                    }
                                }
                            }

                            // REC: Assign the sort ordering for the header
                            // elements to the message's header:
                            msg.Header.SetOrdering(ordering);
                        }
                    }
                    else
                    {
                        string error = string.Format("Dictionary \"{0}\" not registered.", vxDetails.Dictionaries[0]);
                        throw new ArgumentException(error);
                    }
                }
                else
                {
                    string error = string.Format("The version \"{0}\" has no associated dictionaries.", version);
                    throw new ArgumentException(error);
                }

            }
            else
            {
                string error = string.Format("The version \"{0}\" is not registered.", version);
                throw new ArgumentException(error);
            }
        }

        private void PopulateMsgElements(string version, string msgType, FixMessage msg)
        {
            VfxFixVxRecord vxDetails = _vxRegistry.Get(version);
            if (vxDetails != null)
            {
                FixDxCollection msgElements = null;
                foreach (VfxFixVersion_Dictionary_Reference dxEntry in vxDetails.Dictionaries)
                {
                    // REC: Retrieve the dictionary associated with
                    // the version definition:
                    FixDictionary dxInstance = _dxRegistry.GetEntry(dxEntry.Name);
                    // REC: Attempt to retrieve the specified message
                    // type from the dictionary:
                    FixDxMessage dxMessage = dxInstance.GetMessageByType(msgType);
                    if (dxMessage != null)
                    {
                        msgElements = dxInstance.Resolve(dxMessage.Elements);
                        break;
                    }
                }

                if (msgElements != null)
                {
                    // REC: Create the sort ordering for the elements
                    // and populate any fields and groups that have been
                    // registered with the assembler:
                    Collection<int> ordering = new Collection<int>();
                    foreach (IFixDxElement dxElement in msgElements)
                    {
                        ordering.Add(dxElement.Tag);

                        if (dxElement is FixDxResolvedField)
                        {
                            if (_mapFields.ContainsKey(dxElement.Tag))
                            {
                                msg.Content.SetField(_mapFields[dxElement.Tag]);
                            }
                        }
                        else
                        {
                            if (_mapGroups.ContainsKey(dxElement.Tag))
                            {
                                msg.Content.AddGroup(_mapGroups[dxElement.Tag]);
                            }
                        }
                    }

                    // REC: Assign the sort ordering for the header
                    // elements to the message's header:
                    msg.Content.SetOrdering(ordering);
                }
            }
        }

        private void PopulateTrlElements(string version, FixMessage msg)
        {
            VfxFixVxRecord vxDetails = _vxRegistry.Get(version);
            if (vxDetails != null)
            {
                // REC: Ensure that there is at least one FIX dictionary
                // assigned to the specified version definition:
                if (vxDetails.Dictionaries.Count > 0)
                {
                    FixDictionary dxEntry = _dxRegistry.GetEntry(vxDetails.Dictionaries[0].Name);
                    if (dxEntry != null)
                    {
                        FixDxCollection trlElements = dxEntry.Resolve(dxEntry.Trailer);
                        if (trlElements != null)
                        {
                            // REC: Create the sort ordering for the elements
                            // and populate any fields and groups that have been
                            // registered with the assembler:
                            Collection<int> ordering = new Collection<int>();
                            foreach (IFixDxElement dxElement in trlElements)
                            {
                                ordering.Add(dxElement.Tag);

                                if (dxElement is FixDxResolvedField)
                                {
                                    if (_mapFields.ContainsKey(dxElement.Tag))
                                    {
                                        msg.Trailer.SetField(_mapFields[dxElement.Tag]);
                                    }
                                }
                                else
                                {
                                    if (_mapGroups.ContainsKey(dxElement.Tag))
                                    {
                                        msg.Trailer.AddGroup(_mapGroups[dxElement.Tag]);
                                    }
                                }
                            }

                            // REC: Assign the sort ordering for the header
                            // elements to the message's header:
                            msg.Trailer.SetOrdering(ordering);
                        }
                    }
                    else
                    {
                        string error = string.Format("Dictionary \"{0}\" not registered.", vxDetails.Dictionaries[0]);
                        throw new ArgumentException(error);
                    }
                }
                else
                {
                    string error = string.Format("The version \"{0}\" has no associated dictionaries.", version);
                    throw new ArgumentException(error);
                }

            }
            else
            {
                string error = string.Format("The version \"{0}\" is not registered.", version);
                throw new ArgumentException(error);
            }
        }

    }
}
