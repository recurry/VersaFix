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
using VfxEngine.FixVxMatcher;
using VfxEngine.FixVxRegistry;
using VfxEngine.FixDxRegistry;
using VfxEngine.Services;

namespace VfxEngine.FixParser
{
    /// <summary>
    /// The VfxFixParser class parses a single instance of
    /// a FIX message from a stream of bytes.
    /// </summary>
    public class VfxFixParser
    {
        // REC: The version matching service that is retrieved
        // from the service container:
        private IVfxFixVxMatcher _vxMatcher;

        // REC: The version registry that is retrieved from
        // the service container:
        private IVfxFixVxRegistry _vxRegistry;

        // REC: The dictionary registry that is retrieved 
        // from the service container:
        private IVfxFixDxRegistry _dxRegistry;

        // REC: The ParseContext class is an internal class
        // that is used for maintaining context information
        // while a message is being parsed:
        private class ParseContext
        {
            // REC: The current index into the buffer:
            public int Index;
            // REC: The buffer that contains the data:
            public string Buffer;

            /// <summary>
            /// The name of the session layer version of the
            /// protocol that is associated with the message
            /// that is currently being parsed.
            /// </summary>
            public string SxVersion;

            /// <summary>
            /// The name of the application layer version of
            /// the protocol that is associated with the message
            /// that is currently being parsed.
            /// </summary>
            public string AxVersion;

            /// <summary>
            /// The name of the application layer version of
            /// the protocol that is to be used if the parser
            /// cannot determine the version of a message.
            /// </summary>
            public string AxDefault;

            /// <summary>
            /// The collection of resolved elements for the header
            /// section of a FIX message. The parser initializes this
            /// member when it determines the session layer protocol
            /// that is associated with the message being parsed.
            /// </summary>
            public FixDxCollection HdrElements;

            /// <summary>
            /// The collection of resolved message elements that
            /// are associated with the type of message that is
            /// currently being parsed.
            /// </summary>
            public FixDxCollection MsgElements;

            /// <summary>
            /// The collection of resolved elements for the trailer
            /// section of a FIX message. The parser initializes this
            /// member when it determines the session layer protocol
            /// that is associated with the message being parsed.
            /// </summary>
            public FixDxCollection TrlElements;


            public ParseContext(string buffer, int index)
            {
                Index = index;
                Buffer = buffer;
            }
        }

        /// <summary>
        /// The MsgElements class is an internal class that is used
        /// as shorthand notation for a map that contains the sets of 
        /// elements for specific message types, keyed by the message
        /// types of the messages contained in it.
        /// </summary>
        private class MsgElements
        {
            public Dictionary<string, FixDxCollection> Elements = new Dictionary<string, FixDxCollection>();
        }


        /// <summary>
        /// The internal map of all resolved elements for the header
        /// section of a message, keyed by the FIX version definition
        /// that they were resolved from. This is used by the parser
        /// to locate all of the header elements for the message that
        /// is currently being parsed.
        /// </summary>
        private Dictionary<string, FixDxCollection> _hdrElements = new Dictionary<string,FixDxCollection>();

        /// <summary>
        /// The internal map of all resolved message elements for a
        /// secific FIX version, keyed by the FIX version definition
        /// that they were resolved from. This collection is lazily 
        /// initialized as messages are encountered, and is used by
        /// the parser to process the FIX fields that are encountered
        /// in the message body of the message that is being parsed.
        /// </summary>
        private Dictionary<string, MsgElements> _msgElements = new Dictionary<string, MsgElements>();

        /// <summary>
        /// The internal map of all resolved elements for the trailer
        /// section of a message, keyed by the FIX version definition
        /// that they were resolved from. This is used by the parser
        /// to locate all of the trailer elements for the message that
        /// is currently being parsed.
        /// </summary>
        private Dictionary<string, FixDxCollection> _trlElements = new Dictionary<string,FixDxCollection>();

        /// <summary>
        /// Reserved for future use:
        /// </summary>
        private string _sxOverride = null;

        /// <summary>
        /// The client can specify an override value for the application
        /// layer version that is to be used when parsing. This is useful
        /// in terms of handling the DefApplVerID tag which was introduced
        /// in FIX 5.0 and onward. Some systems will not provide ApplVerID
        /// in any messages, so you need to use DefApplVerID for this.
        /// </summary>
        private string _axOverride = null;

        /// <summary>
        /// The ParseGroupResult class is an internal class
        /// that is used to return the result from an attempt
        /// at parsing a repeating group from a message.
        /// </summary>
        private class ParseGroupResult
        {
            /// <summary>
            /// The collection of repeating group instances
            /// that were parsed from the message.
            /// </summary>
            public Collection<FixCollection> Instances = new Collection<FixCollection>();

            /// <summary>
            /// The parser's status following the attempt
            /// to parse a group from the message.
            /// </summary>
            public VfxFixParserStatus Status;
        }

        /// <summary>
        /// The Init method intializes the parser and provides
        /// it with a reference to a service container that has
        /// references to the services that the parser requires
        /// in order to parse messages. The parser requires that
        /// the IVfxFixVxRegistry and IVfxFixDxRegistry services
        /// be available from the service container, or it won't
        /// be able to parse messages properly.
        /// </summary>
        /// <param name="context">
        /// The service container that contains the services that
        /// are needed by an instance of the parser.
        /// </param>
        public void Init(IVfxServices context)
        {
            // REC: Attempt to retrieve the reference to the
            // version registry from the container:
            _vxRegistry = context.GetService(typeof(IVfxFixVxRegistry)) as IVfxFixVxRegistry;
            if (_vxRegistry == null)
            {
                throw new ArgumentException("VfxFixParser: service IVfxFixVxRegistry must be provided.");
            }

            // REC: Create a local instance of the version matcher
            // and initialize it with the provided registry:
            _vxMatcher = new VfxFixVxMatcher();
            _vxMatcher.Init(_vxRegistry);

            // REC: Attempt to retrieve the reference to the
            // dictionary registry from the container:
            _dxRegistry = context.GetService(typeof(IVfxFixDxRegistry)) as IVfxFixDxRegistry;
            if (_dxRegistry == null)
            {
                throw new ArgumentException("VfxFixParser: service IVfxFixDxRegistry must be provided.");
            }


        }

        /// <summary>
        /// The Parse method attempts to extract a single instance
        /// of a FIX message from a supplied string.
        /// </summary>
        /// <param name="buffer">
        /// The string representation of the buffer that the parser
        /// is to extract the message instance from.
        /// </param>
        /// <param name="sxVersion">
        /// The name of the FIX session layer version definition that
        /// is to be used. If null, then the parser will determine the
        /// relevant session layer version by executing a pattern match
        /// against all of the configured version definitions.
        /// </param>
        /// <param name="axVersion">
        /// The name of the FIX application layer version definition the
        /// parser will use to parse the content of the message. If this
        /// parameter is null, then the parser will attempt to determine
        /// the application layer version by executing a pattern match
        /// against all of the configured version definitions.
        /// </param>
        /// <param name="axDefault">
        /// The name of the FIX application layer version definition the
        /// parser will revert to, if it cannot determine the version by
        /// executing a pattern match against the version definitions.
        /// </param>
        /// <returns>
        /// An instance of VfxFixParserResult that contains the
        /// result information for the parse attempt.
        /// </returns>
        public VfxFixParserResult Parse(string buffer, string sxVersion, string axVersion, string axDefault)
        {
            VfxFixParserResult result = new VfxFixParserResult();

            // REC: Don't bother if there is no data
            // in the supplied buffer:
            if (buffer.Length == 0)
            {
                result.Status = VfxFixParserStatus.MsgExhausted;
                return result;
            }

            // REC: Construct the new context that will be used
            // to track the parser's state:
            ParseContext context = new ParseContext(buffer, 0);

            // REC: If an override has been specified for either the
            // session or application layer versions, assign it to the
            // context instance:
            if (!string.IsNullOrEmpty(sxVersion))
            {
                context.SxVersion = sxVersion;
            }

            if (!string.IsNullOrEmpty(axVersion))
            {
                context.AxVersion = axVersion;
            }

            if (!string.IsNullOrEmpty(axDefault))
            {
                context.AxDefault = axDefault;
            }

            // REC: Attempt to parse the header from the message:
            ParseHeader(context, result);
            // REC: Attempt to parse the content from the message:
            ParseContent(context, result);
            // REC: Attempt to parse the trailer form the message:
            ParseTrailer(context, result);

            // REC: If a complete message was extracted from the
            // buffer, the number of characters that were consumed
            // to parse the message will be provided in the result
            // information. If no message was parsed, the consumed
            // count remains at zero so that the caller knows none
            // of the provided data was used by the parser.
            if (result.Status == VfxFixParserStatus.MsgComplete)
            {
                result.Consumed = context.Index;
            }

            return result;
        }

        /// <summary>
        /// The ParseHeader method attempts to parse the header of
        /// a FIX message from the buffer in the parser's context:
        /// </summary>
        /// <param name="ctx">
        /// The parser's current context.
        /// </param>
        /// <param name="result">
        /// The parser's result information.
        /// </param>
        private void ParseHeader(ParseContext ctx, VfxFixParserResult result)
        {
            // REC: Default to success, let the parse logic
            // override this if an error occurs:
            result.Status = VfxFixParserStatus.MsgComplete;

            // REC: Start extracting FIX fields from the buffer:
            while (ctx.Index < ctx.Buffer.Length)
            {
                // REC: Reset index value in case there
                // is an error parsing the buffer:
                int idxRst = ctx.Index;

                int idxSep = ctx.Buffer.IndexOf('=', ctx.Index);
                int idxSoh = ctx.Buffer.IndexOf('\x01', idxSep + 1);

                // REC: If the separator or SOH fields were not 
                // found, then the header is incomplete and the
                // parsing cannot continue:
                if ((idxSep == -1) || (idxSoh == -1))
                {
                    result.Consumed = 0;
                    result.Status = VfxFixParserStatus.MsgExhausted;
                    return;
                }

                // REC: Extract the field's tag from the message:
                string strTag = ctx.Buffer.Substring(ctx.Index, idxSep - ctx.Index);

                // REC: Convert the field's tag from its string
                // form into its corresponding integer value:
                int nTag = -1;
                if (int.TryParse(strTag, out nTag) == false)
                {
                    // REC: If the field cannot be converted into
                    // an integer, the message is malformed:
                    result.Consumed = 0;
                    result.Status = VfxFixParserStatus.MsgMalformed;
                    return;
                }

                // REC: If the FIX BeginString has already been parsed
                // from the buffer, and is encountered again, then the
                // message was incomplete and cannot be parsed:
                if (nTag == 8)
                {
                    if (result.Message.Header.GetField(8) != null)
                    {
                        // REC: Reset the index into the message:
                        ctx.Index = idxRst;
                        // REC: Adjust the result accordingly:
                        result.Consumed = 0;
                        result.Status = VfxFixParserStatus.MsgIncomplete;
                        return;
                    }
                }

                // REC: Determine whether or not the version of the
                // session protocol has been discovered yet:
                if (ctx.HdrElements != null)
                {
                    IFixDxElement hdrElement = ctx.HdrElements.GetElement(nTag);
                    if (hdrElement != null)
                    {
                        if (hdrElement is FixDxResolvedField)
                        {
                            // REC: Retrieve the field's value:
                            string strVal = ctx.Buffer.Substring(idxSep + 1, idxSoh - (idxSep + 1));

                            FixField field = new FixField(nTag, strVal);
                            result.Message.Header.AddField(field);

                            ctx.Index = idxSoh + 1;
                        }
                        else if (hdrElement is FixDxResolvedGroup)
                        {
                            FixDxResolvedGroup groupEntry = hdrElement as FixDxResolvedGroup;

                            // REC: Since this field is a group entry it's ok
                            // to assume that the value is the number of groups
                            // that follow the group tag:
                            string strVal = ctx.Buffer.Substring(idxSep + 1, idxSoh - (idxSep + 1));

                            // REC: Convert the value into an integer and then
                            // attempt to exract that number of repeating groups:
                            int nGroups = 0;
                            if (int.TryParse(strVal, out nGroups) == true)
                            {
                                // REC: Move the context's read index ahead
                                // to compensate for reading the repeating
                                // group's tag and associated value:
                                ctx.Index = idxSoh + 1;

                                FixGroup group = new FixGroup(nTag, strVal);

                                // REC: Try to parse N instances of the
                                // repeating group from the message:
                                ParseGroupResult parseResult = ParseGroup(ctx, groupEntry, nGroups);

                                if (parseResult.Status != VfxFixParserStatus.MsgComplete)
                                {
                                    return;
                                }

                                foreach (FixCollection instance in parseResult.Instances)
                                {
                                    group.Instances.Add(instance);
                                }

                                result.Message.Header.AddGroup(group);
                            }
                            else
                            {
                                // REC: The value in the group tag couldn't 
                                // be converted into an integer, so the 
                                // number of repeating groups is unknown.
                                FixGroup group = new FixGroup(nTag, strVal);
                                result.Message.Header.AddGroup(group);

                                // REC: Move the context's read index ahead
                                // to compensate for reading the repeating
                                // group's tag and associated value:
                                ctx.Index = idxSoh + 1;
                            }

                        }
                    }
                    else
                    {
                        // REC: The parser doesn't support UDFs in the 
                        // header section of the message, so the result
                        // is considered complete if we hit a field that
                        // isn't in the session protocol dictionary:
                        break;
                    }
                }
                else
                {
                    // REC: The session protocol isn't known yet, so the
                    // field is considered a plain FIX field and added to
                    // the message's header:
                    string strVal = ctx.Buffer.Substring(idxSep + 1, idxSoh - (idxSep + 1));

                    FixField field = new FixField(nTag, strVal);
                    result.Message.Header.AddField(field);
                    ctx.Index = idxSoh + 1;

                    // REC: Attempt to determine the session layer protocol
                    // based on the header elements that have been parsed:
                    ctx.SxVersion = _vxMatcher.GetVersion(result.Message.Header, "session");
                    if (ctx.SxVersion == null)
                    {
                        // REC: If no session layer dictionary matched the
                        // elements that have been parsed so far, check for
                        // a combined version definition in case the session
                        // is running FIX versions 4.0-4.4 which do not have
                        // separate session and application layer versions:
                        ctx.SxVersion = _vxMatcher.GetVersion(result.Message.Header, "combined");
                    }

                    // REC: The elements in the message header correspond
                    // to a specific data dictionary, so that should be
                    // used for parsing the header and trailer elements:
                    if (ctx.SxVersion != null)
                    {
                        InitCachedHdrElements(ctx.SxVersion);
                        InitCachedTrlElements(ctx.SxVersion);

                        // REC: Assign the cached sets of resolved header
                        // and trailer elements to the parse context:
                        ctx.HdrElements = _hdrElements[ctx.SxVersion];
                        ctx.TrlElements = _trlElements[ctx.SxVersion];
                    }
                }
            }


            // REC: Determine if the message belongs to the session
            // layer protocol as opposed to the application layer:
            if (ctx.SxVersion != null)
            {
                // REC: Retrieve the FIX message type:
                FixField msgType = result.Message.Header.GetField(35);
                if (!string.IsNullOrEmpty(msgType.Content))
                {
                    // REC: Check the version definition registry for the
                    // corresponding protocol version definition:
                    VfxFixVxRecord vxDefinition = this._vxRegistry.Get(ctx.SxVersion);
                    if (vxDefinition != null)
                    {
                        // REC: Retrieve the associated dictionary from the
                        // configured FIX dictionary registry:
                        FixDictionary dxInstance = this._dxRegistry.GetEntry(vxDefinition.Dictionaries[0].Name);
                        if (dxInstance != null)
                        {
                            // REC: Check if the message is defined within the
                            // dictionary for the session layer:
                            FixDxMessage sxMessage = dxInstance.GetMessageByType(msgType.Content);
                            if (sxMessage != null)
                            {
                                if ((!_msgElements.ContainsKey(ctx.SxVersion)) || (!_msgElements[ctx.SxVersion].Elements.ContainsKey(msgType.Content)))
                                {
                                    InitCachedMsgElements(ctx, msgType.Content);
                                }

                                // REC: Look in the message definition cache
                                // for this particular message type:
                                if (_msgElements[ctx.SxVersion].Elements.ContainsKey(msgType.Content))
                                {
                                    ctx.MsgElements = _msgElements[ctx.SxVersion].Elements[msgType.Content];
                                }

                                // REC: The context is now initialized to parse the
                                // session layer message's content, so we're done:
                                return;
                            }
                        }
                    }
                }
            }


            // REC: If an override version has been specified for the
            // application layer, use that instead of hunting for it:
            if (ctx.AxVersion == null)
            {
                // REC: Now that the entire header of the message has been
                // extracted from the buffer, test the header contents and
                // determine the application protocol version:
                ctx.AxVersion = _vxMatcher.GetVersion(result.Message.Header, "application");
                if (ctx.AxVersion == null)
                {
                    // REC: If the application layer version cannot be
                    // determined, it may be a FIX 4.0-4.4 message, so
                    // check the combined versions as well:
                    ctx.AxVersion = _vxMatcher.GetVersion(result.Message.Header, "combined");
                    if(ctx.AxVersion == null)
                    {
                        // REC: If the application layer could not be
                        // resolved, then switch to the default:
                        ctx.AxVersion = ctx.AxDefault;
                    }
                }
            }

            if (ctx.AxVersion != null)
            {
                // REC: Now that the application version has been
                // determined, retrieve the message elements for 
                // the type of message being parsed, initializing
                // the local message elements cache if needed:
                FixField msgType = result.Message.Header.GetField(35);
                if (msgType != null)
                {
                    if ((!_msgElements.ContainsKey(ctx.AxVersion)) || (!_msgElements[ctx.AxVersion].Elements.ContainsKey(msgType.Content)))
                    {
                        InitCachedMsgElements(ctx, msgType.Content);
                    }

                    // REC: Look in the message definition cache
                    // for this particular message type:
                    if (_msgElements[ctx.AxVersion].Elements.ContainsKey(msgType.Content))
                    {
                        ctx.MsgElements = _msgElements[ctx.AxVersion].Elements[msgType.Content];
                    }
                }
            }
        }

        /// <summary>
        /// The ParseContent method attempts to parse the message body
        /// from an instance of a FIX message.
        /// </summary>
        /// <param name="ctx">
        /// The parser's context information.
        /// </param>
        /// <param name="result">
        /// The parser's result information.
        /// </param>
        private void ParseContent(ParseContext ctx, VfxFixParserResult result)
        {
            // REC: Attempt to retrieve the FIX message type
            // from the header of the message:
            FixField msgType = result.Message.Header.GetField(35);

            while (ctx.Index < ctx.Buffer.Length)
            {
                int idxSep = ctx.Buffer.IndexOf('=', ctx.Index);
                int idxSoh = ctx.Buffer.IndexOf('\x01', idxSep + 1);

                // REC: If the separator or SOH fields were not 
                // found, then the message is incomplete and the
                // parsing cannot continue:
                if ((idxSep == -1) || (idxSoh == -1))
                {
                    result.Status = VfxFixParserStatus.MsgExhausted;
                    return;
                }

                // REC: Extract the field's tag from the message:
                string strTag = ctx.Buffer.Substring(ctx.Index, idxSep - ctx.Index);
                // REC: Convert the field's tag to an integer:
                int nTag;
                if (int.TryParse(strTag, out nTag) == false)
                {
                    result.Status = VfxFixParserStatus.MsgMalformed;
                    return;
                }

                // REC: Test for premature message termination:
                if (nTag == 8)
                {
                    result.Status = VfxFixParserStatus.MsgMalformed;
                    return;
                }

                if (ctx.MsgElements != null)
                {
                    IFixDxElement element = ctx.MsgElements.GetElement(nTag);
                    if (element != null)
                    {
                        if (element is FixDxResolvedField)
                        {
                            // REC: Retrieve the field's value:
                            string strVal = ctx.Buffer.Substring(idxSep + 1, idxSoh - (idxSep + 1));

                            // REC: Create a new field to represent
                            // the parsed field/value pair:
                            FixField field = new FixField(nTag, strVal);
                            result.Message.AddField(field);
                            ctx.Index = idxSoh + 1;
                        }
                        else if (element is FixDxResolvedGroup)
                        {
                            // REC: Since this field is a group entry it's ok
                            // to assume that the value is the number of groups
                            // that follow the group tag:
                            string strVal = ctx.Buffer.Substring(idxSep + 1, idxSoh - (idxSep + 1));

                            // REC: Convert the value into an integer and then
                            // attempt to exract that number of repeating groups:
                            int nGroups = 0;
                            if (int.TryParse(strVal, out nGroups) == true)
                            {
                                // REC: Move the context's read index ahead
                                // to compensate for reading the repeating
                                // group's tag and associated value:
                                ctx.Index = idxSoh + 1;

                                FixGroup group = new FixGroup(nTag, strVal);

                                // REC: Try to parse N instances of the
                                // repeating group from the message:
                                FixDxResolvedGroup resolvedGroup = element as FixDxResolvedGroup;
                                ParseGroupResult parseResult = ParseGroup(ctx, resolvedGroup, nGroups);

                                if (parseResult.Status != VfxFixParserStatus.MsgComplete)
                                {
                                    result.Status = parseResult.Status;
                                    return;
                                }

                                foreach (FixCollection instance in parseResult.Instances)
                                {
                                    group.Instances.Add(instance);
                                }

                                result.Message.AddGroup(group);

                            }
                            else
                            {
                                // REC: The value in the group tag couldn't 
                                // be converted into an integer, so the 
                                // number of repeating groups is unknown.
                                FixGroup group = new FixGroup(nTag, strVal);
                                result.Message.AddGroup(group);

                                // REC: Move the context's read index ahead
                                // to compensate for reading the repeating
                                // group's tag and associated value:
                                ctx.Index = idxSoh + 1;
                            }
                        }
                    }
                    else
                    {
                        // REC: The tag wasn't found in the map of elements
                        // that are known for this message type, so determine
                        // whether it is a UDF or a field from the trailer:
                        if (ctx.TrlElements.GetElement(nTag) != null)
                        {
                            // REC: The field is a trailer element
                            // so content parsing is now complete:
                            return;
                        }
                        else
                        {
                            // REC: The field is just a UDF so it can be added
                            // to the message body:
                            string strVal = ctx.Buffer.Substring(idxSep + 1, idxSoh - (idxSep + 1));

                            FixField field = new FixField(nTag, strVal);
                            result.Message.AddField(field);

                            ctx.Index = idxSoh + 1;
                        }
                    }
                }
                else
                {
                    // REC: There is no cached set of elements for this
                    // type of message, so the field is either a trailer
                    // field or it is a user-defined field:
                    if (ctx.TrlElements.GetElement(nTag) != null)
                    {
                        // REC: The field is a trailer element
                        // so content parsing is now complete:
                        return;
                    }
                    else
                    {
                        // REC: The field is just a UDF so it can be added
                        // to the message body:
                        string strVal = ctx.Buffer.Substring(idxSep + 1, idxSoh - (idxSep + 1));

                        FixField field = new FixField(nTag, strVal);
                        result.Message.AddField(field);

                        ctx.Index = idxSoh + 1;
                    }
                }
            }
        }

        private void ParseTrailer(ParseContext ctx, VfxFixParserResult result)
        {
            while (ctx.Index < ctx.Buffer.Length)
            {
                int idxSep = ctx.Buffer.IndexOf('=', ctx.Index);
                int idxSoh = ctx.Buffer.IndexOf('\x01', idxSep + 1);

                // REC: If the separator or SOH fields were not 
                // found, then the message is incomplete and the
                // parsing cannot continue:
                if ((idxSep == -1) || (idxSoh == -1))
                {
                    // result.Consumed = 0;
                    result.Status = VfxFixParserStatus.MsgExhausted;
                    return;
                }

                // REC: Extract the field's tag from the message:
                string strTag = ctx.Buffer.Substring(ctx.Index, idxSep - ctx.Index);

                // REC: Convert the field's tag to an integer:
                int nTag;
                if (int.TryParse(strTag, out nTag) == false)
                {
                    result.Status = VfxFixParserStatus.MsgMalformed;
                    return;
                }

                if (ctx.TrlElements != null)
                {
                    IFixDxElement element = ctx.TrlElements.GetElement(nTag);
                    if(element != null)
                    {
                        if (element is FixDxResolvedField)
                        {
                            // REC: Retrieve the field's value:
                            string strVal = ctx.Buffer.Substring(idxSep + 1, idxSoh - (idxSep + 1));

                            // REC: Create a new field to represent
                            // the parsed field/value pair:
                            FixField field = new FixField(nTag, strVal);
                            result.Message.Trailer.AddField(field);
                            ctx.Index = idxSoh + 1;
                        }
                        else if (element is FixDxResolvedGroup)
                        {
                            FixDxResolvedGroup groupEntry = element as FixDxResolvedGroup;
                            // REC: Since this field is a group entry it's ok
                            // to assume that the value is the number of groups
                            // that follow the group tag:
                            string strVal = ctx.Buffer.Substring(idxSep + 1, idxSoh - (idxSep + 1));

                            // REC: Convert the value into an integer and then
                            // attempt to exract that number of repeating groups:
                            int nGroups = 0;
                            if (int.TryParse(strVal, out nGroups) == true)
                            {
                                // REC: Move the context's read index ahead
                                // to compensate for reading the repeating
                                // group's tag and associated value:
                                ctx.Index = idxSoh + 1;

                                FixGroup group = new FixGroup(nTag, strVal);

                                // REC: Try to parse N instances of the
                                // repeating group from the message:
                                ParseGroupResult parseResult = ParseGroup(ctx, groupEntry, nGroups);

                                if (parseResult.Status != VfxFixParserStatus.MsgComplete)
                                {
                                    result.Status = parseResult.Status;
                                    return;
                                }

                                foreach (FixCollection instance in parseResult.Instances)
                                {
                                    group.Instances.Add(instance);
                                }

                                result.Message.Trailer.AddGroup(group);

                            }
                            else
                            {
                                // REC: The value in the group tag couldn't 
                                // be converted into an integer, so the 
                                // number of repeating groups is unknown.
                                FixGroup group = new FixGroup(nTag, strVal);
                                result.Message.Trailer.AddGroup(group);

                                // REC: Move the context's read index ahead
                                // to compensate for reading the repeating
                                // group's tag and associated value:
                                ctx.Index = idxSoh + 1;
                            }
                        }
                    }
                    else
                    {
                        // REC: If the element is not in the set of trailer
                        // elements, then the parsing is complete.
                        return;
                    }
                }
                else
                {
                    result.Status = VfxFixParserStatus.MsgUnkSxProtocol;
                    return;
                }
            }
        }

        /// <summary>
        /// The ParseGroup method attempts to parse all of the group
        /// instances from a repeating group.
        /// </summary>
        /// <param name="ctx">
        /// The parser's current parsing context.
        /// </param>
        /// <param name="group">
        /// The resolved group information for the repeating group
        /// that is being extracted from the message.
        /// </param>
        /// <param name="count">
        /// The number of instances of the repeating group that the
        /// parser should expect to encounter.
        /// </param>
        /// <returns></returns>
        private ParseGroupResult ParseGroup(ParseContext ctx, FixDxResolvedGroup group, int count)
        {
            ParseGroupResult result = new ParseGroupResult();

            // Build a temporary map of flags that indicate whether
            // or not a specific element of the repeating group has
            // been encountered during the parsing of an instance:
            Dictionary<int, bool> mapEncountered = new Dictionary<int, bool>();

            foreach (IFixDxElement element in group.Elements)
            {
                if (!mapEncountered.ContainsKey(element.Tag))
                {
                    mapEncountered.Add(element.Tag, false);
                }
                else
                {
                    mapEncountered[element.Tag] = false;
                }
            }


            // REC: Extract the FIX tag of the first field in
            // the repeating group; this is needed to determine
            // when one instance stops and another starts:
            IFixDxElement startField = group.Elements.First();
            int nStartField = startField.Tag;

            // REC: Default to the complete status when parsing
            // a group, so that zero-instance groups don't cause
            // a problem; a zero-instance group is okay in terms
            // of parsing, and should only be flagged as a fault
            // when validation is run...
            result.Status = VfxFixParserStatus.MsgComplete;
            for (int i = 0; i != count; i++)
            {
                FixCollection collection = new FixCollection();

                List<int> listEncounterMapKeys = new List<int>();
                foreach (int key in mapEncountered.Keys)
                {
                    listEncounterMapKeys.Add(key);
                } 

                // REC: Reset the encounter map for all of the 
                // tags that can be encountered in an instance
                // of the repeating group:
                foreach (int key in listEncounterMapKeys)
                {
                    mapEncountered[key] = false;
                }


                while (ctx.Index < ctx.Buffer.Length)
                {
                    // REC: Ignore leading SOH characters:
                    while (ctx.Buffer[ctx.Index] == '\x01')
                    {
                        ctx.Index++;
                        if (ctx.Index >= ctx.Buffer.Length)
                        {
                            return result;
                        }
                    }

                    // REC: Locate the next tag/value separator:
                    int idxSep = ctx.Buffer.IndexOf('=', ctx.Index);
                    if (idxSep == -1)
                    {
                        // REC: If the next separator couldn't
                        // be found, the message is incomplete:
                        result.Status = VfxFixParserStatus.MsgIncomplete;
                        return result;
                    }

                    // REC: If the field doesn't have a tag then
                    // the parsing of the groups can't continue:
                    if (idxSep == ctx.Index)
                    {
                        result.Status = VfxFixParserStatus.MsgMalformed;
                        return result;
                    }

                    // REC: Attempt to locate the end of the field:
                    int idxSoh = ctx.Buffer.IndexOf('\x01', idxSep + 1);
                    if (idxSoh == -1)
                    {
                        result.Status = VfxFixParserStatus.MsgIncomplete;
                        return result;
                    }

                    string strTag = ctx.Buffer.Substring(ctx.Index, idxSep - ctx.Index);
                    if (strTag != null)
                    {
                        int nTag;
                        if (int.TryParse(strTag, out nTag) == false)
                        {
                            result.Status = VfxFixParserStatus.MsgMalformed;
                            return result;
                        }

                        string strVal = null;

                        // REC: Determine whether or not the tag
                        // is a valid member of the current group:
                        IFixDxElement element = group.Elements.GetElement(nTag);
                        if (element == null)
                        {
                            // REC: The parsing of a repeating group
                            // should cease if a tag that is not a 
                            // member of the group is encountered:
                            result.Instances.Add(collection);
                            // REC: Set the status so that the caller knows
                            // the group was successfully parsed:
                            result.Status = VfxFixParserStatus.MsgComplete;
                            return result;
                        }

                        // REC: Determine whether or not the tag
                        // has already been encountered during 
                        // the parsing of the current instance:
                        if (mapEncountered[nTag] == true)
                        {
                            // REC: Determine whether or not the 
                            // redundant tag is the first tag in
                            // the repeating group's layout:
                            if (nTag == nStartField)
                            {
                                // REC: This field is the start tag
                                // for another instance of the group:
                                result.Instances.Add(collection);
                                break;
                            }

                            return result;
                        }

                        mapEncountered[nTag] = true;

                        // REC: Determine whether or not the element
                        // represents a length encoded field:
                        if (element is FixDxResolvedField)
                        {
                            FixDxResolvedField fieldEntry = element as FixDxResolvedField;
                            if (fieldEntry.LengthCoded == true)
                            {
                                // REC: Determine whether or not the
                                // corresponding length field exists:
                                string strLen = collection.GetField(fieldEntry.LengthField).Content;
                                if (strLen != null)
                                {
                                    int nLen = -1;
                                    if (int.TryParse(strLen, out nLen) == true)
                                    {
                                        // REC: Determine whether or not there 
                                        // are enough characters remaining in
                                        // the buffer to parse the contents:
                                        if ((idxSep + nLen) >= ctx.Buffer.Length)
                                        {
                                            result.Status = VfxFixParserStatus.MsgIncomplete;
                                            return result;
                                        }

                                        strVal = ctx.Buffer.Substring(idxSep + 1, nLen);
                                        // REC: Adjust the context's read index:
                                        ctx.Index = (idxSep + 1) + nLen;
                                    }
                                }
                                else
                                {
                                    // REC: The encoded length field couldn't 
                                    // be located, so the contents will be 
                                    // parsed as though they were any other
                                    // normal field's contents:
                                    strVal = ctx.Buffer.Substring(idxSep + 1, idxSoh - (idxSep + 1));
                                    // REC: Adjust the context's read index:
                                    ctx.Index = idxSoh + 1;
                                }
                            }
                            else
                            {
                                strVal = ctx.Buffer.Substring(idxSep + 1, idxSoh - (idxSep + 1));
                                // REC: Adjust the context's read index:
                                ctx.Index = idxSoh + 1;
                            }

                            collection.AddField(new FixField(nTag, strVal));

                        }
                        else if (element is FixDxResolvedGroup)
                        {
                            // REC: If the group field's value isn't set
                            // to a specific value, then add a null group
                            // entry to the collection and continue:
                            if (idxSoh == idxSep + 1)
                            {
                                FixGroup parsedGroup = new FixGroup(nTag, null);
                                collection.AddGroup(parsedGroup);
                                result.Instances.Add(collection);
                                ctx.Index = idxSoh + 1;
                            }
                            else
                            {
                                // REC: Attempt to convert the field's value
                                // into an integer that represents the number
                                // of repeating groups that should follow:
                                strVal = ctx.Buffer.Substring(idxSep + 1, idxSoh - (idxSep + 1));

                                // REC: This might have been the issue with nested repeating
                                // groups becoming a problem:
                                ctx.Index = idxSoh + 1;

                                int nInstances = -1;
                                if (int.TryParse(strVal, out nInstances) == true)
                                {
                                    FixDxResolvedGroup subGroup = element as FixDxResolvedGroup;
                                    ParseGroupResult subResult = ParseGroup(ctx, subGroup, nInstances);
                                    if (subResult.Status != VfxFixParserStatus.MsgComplete)
                                    {
                                        break;
                                    }
                                    else
                                    {
                                        FixGroup parsedGroup = new FixGroup(nTag, strVal);
                                        foreach (FixCollection instance in subResult.Instances)
                                        {
                                            parsedGroup.Instances.Add(instance);
                                        }

                                        collection.AddGroup(parsedGroup);

                                        // REC: Adjust the context's read index:
                                        //ctx.Index = idxSoh + 1;
                                    }
                                }
                                else
                                {
                                    // REC: The instance count couldn't be converted
                                    // to an integer, so the message is malformed:
                                    result.Status = VfxFixParserStatus.MsgMalformed;
                                    break;
                                }
                            }
                        }
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// The InitCachedHdrElements method resolves all of the
        /// elements that are defined in the message header for a
        /// specific version of FIX and then adds all the resolved
        /// elements to the local cache so that they don't have to
        /// be resolved more than once for each version.
        /// </summary>
        /// <param name="vxName">
        /// The name of the version definition being cached.
        /// </param>
        private void InitCachedHdrElements(string vxName)
        {
            if (!_hdrElements.ContainsKey(vxName))
            {
                VfxFixVxRecord vxDetails = _vxRegistry.Get(vxName);
                if (vxDetails != null)
                {
                    if (vxDetails.Dictionaries.Count > 0)
                    {
                        FixDictionary dxPrimary = _dxRegistry.GetEntry(vxDetails.Dictionaries[0].Name);
                        if (dxPrimary != null)
                        {
                            _hdrElements.Add(vxName, dxPrimary.Resolve(dxPrimary.Header));
                        }
                    }
                }
            }
        }


        /// <summary>
        /// The InitCachedTrlElements method resolves all of the
        /// elements that are defined in the message trailer for a
        /// specific version of FIX and then adds all the resolved
        /// elements to the local cache so that they don't have to
        /// be resolved more than once for each version.
        /// </summary>
        /// <param name="vxName">
        /// The name of the version definition being cached.
        /// </param>
        private void InitCachedTrlElements(string vxName)
        {
            if (!_trlElements.ContainsKey(vxName))
            {
                VfxFixVxRecord vxDetails = _vxRegistry.Get(vxName);
                if (vxDetails != null)
                {
                    if (vxDetails.Dictionaries.Count > 0)
                    {
                        FixDictionary dxPrimary = _dxRegistry.GetEntry(vxDetails.Dictionaries[0].Name);
                        if (dxPrimary != null)
                        {
                            _trlElements.Add(vxName, dxPrimary.Resolve(dxPrimary.Trailer));
                        }
                    }
                }
            }
        }

        /// <summary>
        /// The InitCachedMsgElements method resolves all of 
        /// the elements that are defined for a specific type
        /// of FIX message and adds them to the local cache.
        /// </summary>
        /// <param name="ctx">
        /// The parser's context information.
        /// </param>
        /// <param name="msgType">
        /// The specific message type that is to be added to the
        /// cache of message definitions.
        /// </param>
        private void InitCachedMsgElements(ParseContext ctx, string msgType)
        {
            // REC: Check the session layer dictionaries for a
            // message definition that matches the specified type:
            VfxFixVxRecord sxDetails = _vxRegistry.Get(ctx.SxVersion);
            if (sxDetails != null)
            {
                foreach (VfxFixVersion_Dictionary_Reference dxEntry in sxDetails.Dictionaries)
                {
                    FixDictionary dx = _dxRegistry.GetEntry(dxEntry.Name);
                    if (dx != null)
                    {
                        FixDxMessage dxMsg = dx.GetMessageByType(msgType);
                        if (dxMsg != null)
                        {
                            FixDxCollection resolved = dx.Resolve(dxMsg.Elements);
                            if (!_msgElements.ContainsKey(ctx.SxVersion))
                            {
                                _msgElements.Add(ctx.SxVersion, new MsgElements());
                            }

                            _msgElements[ctx.SxVersion].Elements.Add(msgType, resolved);
                            return;
                        }
                    }
                }
            }

            // REC: The specified message type wasn't found in the session
            // layer dictionaries, so try the application layer next:
            VfxFixVxRecord axDetails = _vxRegistry.Get(ctx.AxVersion);
            if (axDetails != null)
            {
                foreach (VfxFixVersion_Dictionary_Reference dxEntry in axDetails.Dictionaries)
                {
                    FixDictionary dx = _dxRegistry.GetEntry(dxEntry.Name);
                    if (dx != null)
                    {
                        FixDxMessage dxMsg = dx.GetMessageByType(msgType);
                        if (dxMsg != null)
                        {
                            FixDxCollection resolved = dx.Resolve(dxMsg.Elements);
                            if (!_msgElements.ContainsKey(ctx.AxVersion))
                            {
                                _msgElements.Add(ctx.AxVersion, new MsgElements());
                            }

                            _msgElements[ctx.AxVersion].Elements.Add(msgType, resolved);
                            return;
                        }
                    }
                }
            }
        }
    }

}
