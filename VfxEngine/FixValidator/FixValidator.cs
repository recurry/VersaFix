using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Globalization;

using VfxEngine.Fix;


namespace VfxEngine.FixValidator
{
    public enum FixValidationCode
    {
        Field_Missing   = 0x01,
        Group_Missing   = 0x02,
        Field_Present   = 0x03,
        Group_Present   = 0x04,
        Field_Invalid   = 0x05,
        Group_Invalid   = 0x06,
        Field_OOS       = 0x07,
        Field_UDF       = 0x08
    }

    // REC: The FixValidationResult encapsulates the
    // validation result for a single FIX element.
    public class FixValidationResult : ICloneable
    {
        private string _fieldTag;

        public string FieldTag
        {
            get { return _fieldTag; }
            set { _fieldTag = value; }
        }

        private string _fieldName;

        public string FieldName
        {
            get { return _fieldName; }
            set { _fieldName = value; }
        }

        private string _fieldValue;

        public string FieldValue
        {
            get { return _fieldValue; }
            set { _fieldValue = value; }
        }


        private bool _fieldRequired;

        public bool FieldRequired
        {
            get { return _fieldRequired; }
            set { _fieldRequired = value; }
        }

        private List<FixValidationResult> _listNodes = new List<FixValidationResult>();

        public List<FixValidationResult> Nodes
        {
            get { return _listNodes; }
            set { _listNodes = value; }
        }

        private FixValidationCode _validationCode;

        public FixValidationCode ResultCode
        {
            get { return _validationCode; }
            set { _validationCode = value; }
        }

        private string _validationText;

        public string ResultText
        {
            get { return _validationText; }
            set { _validationText = value; }
        }


        #region ICloneable Members

        public object Clone()
        {
            FixValidationResult r = new FixValidationResult();
            r.FieldTag = _fieldTag; 
            r.FieldName = _fieldName;
            r.FieldValue = _fieldValue;
            r.FieldRequired = _fieldRequired;
            r.ResultCode = _validationCode;
            r.ResultText = _validationText;

            foreach (FixValidationResult n in _listNodes)
            {
                FixValidationResult child = n.Clone() as FixValidationResult;
                r.Nodes.Add(child); 
            }

            return r; 
        }

        #endregion
    }

    public class FixValidatorSettings
    {
        private List<FixDictionary> _dictionaries;

        public List<FixDictionary> Dictionaries
        {
            get { return _dictionaries; }
            set { _dictionaries = value; }
        }
    }

    // REC: The FixValidator class provides for validation
    // of FIX messages against a set of FIX dictionaries.
    public class FixValidator
    {
        // REC: Internal structure for maintaining the list
        // of tag/value pairs that are found in a FIX message
        // that the validator is processing:
        private struct SrcField
        {
            public string Tag;

            public string Name;

            public string Value; 
        }

        // REC: The set of lists that are used to 
        // validate the structure of a message:
        private class MsgLayout
        {
            public List<FixValidationResult> Header = new List<FixValidationResult>();

            public List<FixValidationResult> Message = new List<FixValidationResult>();
            
            public List<FixValidationResult> Trailer = new List<FixValidationResult>();
        }

        // REC: The preprocessed list of elements that
        // are in the primary dictionary's header:
        private List<FixValidationResult> _cachedHeader = new List<FixValidationResult>(); 

        // REC: The preprocessed list of elements that
        // are in the primary dictionary's trailer:
        private List<FixValidationResult> _cachedTrailer = new List<FixValidationResult>();

        private FixValidatorSettings _settings;

        public FixValidator()
        {

        }

        public void Configure(FixValidatorSettings settings)
        {
            _settings = settings;

            _cachedHeader.Clear();

            _cachedTrailer.Clear(); 

            if (_settings.Dictionaries.Count > 0)
            {
                FixDictionary primary = _settings.Dictionaries[0];

                // REC: Preprocess the list of elements in the primary
                // dictionary's header so that it doesn't have to be 
                // done every time a message is validated:
                FixDxCollection hdrElements = primary.Resolve(primary.Header);
                foreach (IFixDxElement hdrElement in hdrElements)
                {
                    _cachedHeader.Add(CreateResult(hdrElement));
                }

                // REC: Preprocess the list of elements in the primary
                // dictionary's trailer so that it doesn't have to be 
                // done every time a message is validated:
                FixDxCollection trlElements = primary.Resolve(primary.Trailer);
                foreach (IFixDxElement trlElement in trlElements)
                {
                    _cachedTrailer.Add(CreateResult(trlElement));
                }
            }
        }



        // REC: Validate an instance of the FixMessage class against
        // the header/message/trailer definition in the dictionaries:
        public List<FixValidationResult> Validate(FixMessage msg)
        {
            List<FixValidationResult> result = new List<FixValidationResult>(); 

            // REC: Build a queue of all of the FIX elements that
            // are contained in the message:
            Queue<SrcField> srcFields = new Queue<SrcField>(); 

            // REC: Add the elements that are in the message's
            // header to the queue of source fields:
            PopulateQueue(srcFields, msg.Header); 

            // REC: Add the elements that are in the message's
            // body to the queue of source fields:
            PopulateQueue(srcFields, msg.Content); 

            // REC: Add the elements that are in the message's
            // trailer to the queue of source fields:
            PopulateQueue(srcFields, msg.Trailer); 


            // REC: Create the MsgLayout structure that defines
            // the layout of a message as it is defined in the 
            // configured FIX dictionaries:
            MsgLayout msgLayout = new MsgLayout();
            
            // REC: The message layout's header entries are 
            // copied from the cached list of entries that is
            // created when the validator is configured:
            foreach (FixValidationResult hdrEntry in _cachedHeader)
            {
                msgLayout.Header.Add(hdrEntry.Clone() as FixValidationResult);
            }

            // REC: The message layout's content entries are
            // only configured if the corresponding message
            // type can be found in the dictionaries:
            FixField fldMsgType = msg.Header.GetField(35);
            if (fldMsgType != null)
            {
                string strMsgType = fldMsgType.Content;
                foreach (FixDictionary dx in _settings.Dictionaries)
                {
                    FixDxMessage dxMessage = dx.GetMessageByType(strMsgType);
                    if (dxMessage != null)
                    {
                        // REC: Expand the list of elements contained
                        // in the message's definition and populate the
                        // message layout element list with them:
                        FixDxCollection msgElements = dx.Resolve(dxMessage.Elements);
                        foreach (IFixDxElement msgElement in msgElements)
                        {
                            msgLayout.Message.Add(CreateResult(msgElement));
                        }

                        break; 
                    }
                }
            }

            // REC: the message layout's trailer entries are
            // copied from the cached list of entries that is
            // created when the validator is configured:
            foreach (FixValidationResult trlEntry in _cachedTrailer)
            {
                msgLayout.Trailer.Add(trlEntry.Clone() as FixValidationResult);
            }


            // REC: After the message layout has been defined
            // the validator can compare the content of the 
            // source message to the layout definition:
            return ProcessElements(srcFields, msgLayout); 
        }

        // REC: The ProcessElements method compares the contents
        // of a message to the corresponding message layout and
        // returns a list of validation result entries for each
        // of the fields that were encountered in the message:
        private List<FixValidationResult> ProcessElements(Queue<SrcField> queue, MsgLayout layout)
        {
            List<FixValidationResult> result = new List<FixValidationResult>();

            int hdrIndex = 0;
            int msgIndex = 0;
            int trlIndex = 0;

            while (queue.Count > 0)
            {
                bool processed = false; 

                // REC: Retrieve the next source field from
                // the sequential queue of message fields:
                SrcField current = queue.Dequeue(); 

                // REC: Scan the result set for a placeholder
                // for this field before checking the template:
                bool fieldOOS = false; 
                foreach (FixValidationResult fvr in result)
                {
                    // REC: If the current field's tag matches the
                    // tag in the validation result, and the result's
                    // validation code is set to Field_Missing, then
                    // the result entry is a placeholder for the field:
                    
                    if (fvr.ResultCode == FixValidationCode.Field_Missing)
                    {
                        if (current.Tag == fvr.FieldTag)
                        {
                            // REC: The fact that this entry is a placeholder
                            // means that it was previously passed over when
                            // a field was being matched. This means that the
                            // current field is out of sequence, or it would
                            // already be in the validation result set:
                            fvr.FieldValue = current.Value;

                            fvr.ResultCode = FixValidationCode.Field_Present;
                            fvr.ResultText = "Field Present";
                            if (fieldOOS == true)
                            {
                                fvr.ResultCode = FixValidationCode.Field_OOS;
                                fvr.ResultText = "Out of Sequence";
                            }

                            //VerifyFieldType(current, fvr);

                            if (fvr.Nodes.Count > 0)
                            {
                                fvr.Nodes = ValidateGroup(queue, fvr);
                            }

                            // REC: Remove the validation result from its 
                            // current position in the result set...
                            result.Remove(fvr);

                            // REC: ...and add it to the end of the result
                            // set since that's where it was encountered in
                            // the source message:
                            result.Add(fvr);

                            // REC: Set the processed flag to true to indicate
                            // that this field in the source message has now 
                            // been completely processed:
                            processed = true;
                            break;
                        }
                        else
                        {
                            // REC: The field is missing, but it doesn't match
                            // the current one. If the missing field is also a
                            // required field, then set the OOS flag:
                            if (fvr.FieldRequired == true)
                            {
                                fieldOOS = true;
                            }

                        }
                    }
                }

                // REC: If the current field in the message wasn't found
                // in the result set, as a placeholder, then scan the 
                // elements in the message header for a match:
                if(processed == false)
                {
                    while(hdrIndex != layout.Header.Count)
                    {
                        // REC: Create a copy of the entry in the 
                        // message header layout:
                        FixValidationResult src = layout.Header[hdrIndex++];
                        FixValidationResult dst = src.Clone() as FixValidationResult ; 

                        // REC: Add the copy of the entry to the
                        // result set in anticipation of the field
                        // being matched. If it's not matched, then
                        // the entry will act as a placeholder:
                        result.Add(dst) ; 

                        if(current.Tag == src.FieldTag)
                        {
                            dst.FieldValue = current.Value; 
                            dst.ResultCode = FixValidationCode.Field_Present;
                            dst.ResultText = "Field Present" ; 

                            // REC: If the entry in the layout is a 
                            // repeating group header, then attempt
                            // to validate as many instances of the
                            // repeating group as are indicated by
                            // the value of in the message's field:
                            if(src.Nodes.Count > 0)
                            {
                                dst.Nodes = ValidateGroup(queue, dst) ; 
                                dst.ResultCode = FixValidationCode.Group_Present;
                                dst.ResultText = "Group Present";
                            }

                            processed = true ; 
                        }
                    }

                    // REC: If the current field wasn't found in the 
                    // result set, or in the layout for the message's
                    // header elements, then attempt to locate it in
                    // the message's content elements:
                    if(processed == false)
                    {
                        while(msgIndex != layout.Message.Count)
                        {
                            FixValidationResult src = layout.Message[msgIndex++] ; 
                            FixValidationResult dst = src.Clone() as FixValidationResult ; 

                            // REC: Add the copy of the entry to the
                            // result set in anticipation of the field
                            // being matched. If it's not matched, then
                            // the entry will act as a placeholder:
                            result.Add(dst) ; 

                            if(current.Tag == src.FieldTag)
                            {
                                dst.FieldValue = current.Value ; 
                                dst.ResultCode = FixValidationCode.Field_Present;
                                dst.ResultText = "Field Present" ; 

                                 // REC: If the entry in the layout is a 
                                // repeating group header, then attempt
                                // to validate as many instances of the
                                // repeating group as are indicated by
                                // the value of in the message's field:
                                if(src.Nodes.Count > 0)
                                {
                                    dst.Nodes = ValidateGroup(queue, dst) ; 
                                    dst.ResultCode = FixValidationCode.Group_Present;
                                    dst.ResultText = "Group Present" ; 
                                }

                                processed = true ;
                                break ; 
                            }
                        }
                    }

                    // REC: If the field wasn't found in the header
                    // or in the body, then scan the message trailer:
                    if(processed == false)
                    {
                        // REC: Note that the trailer elements don't 
                        // get copied into the result set unless there
                        // is a match for one of them; these elements
                        // should always be placed at the end of the 
                        // result set if there is no match for them:
                        for(int i=trlIndex; i!=layout.Trailer.Count; i++)
                        {
                            if(current.Tag == layout.Trailer[i].FieldTag)
                            {
                                while (trlIndex != layout.Trailer.Count)
                                {
                                    FixValidationResult src = layout.Trailer[trlIndex++];
                                    FixValidationResult dst = src.Clone() as FixValidationResult;

                                    result.Add(dst);
                                    if (current.Tag == src.FieldTag)
                                    {
                                        dst.FieldValue = current.Value;
                                        dst.ResultCode = FixValidationCode.Field_Present;
                                        dst.ResultText = "Field Present";

                                        if (src.Nodes.Count > 0)
                                        {
                                            dst.Nodes = ValidateGroup(queue, dst);

                                            dst.ResultCode = FixValidationCode.Group_Present;
                                            dst.ResultText = "Group Present";
                                        }

                                        processed = true;
                                        break;
                                    }

                                }
                            }
                        }

                        // REC: If the field wasn't found in the trailer
                        // layout, then it's a UDF and can just be added
                        // to the result set where it's found:
                        if (processed == false)
                        {
                            FixValidationResult dst = new FixValidationResult();
                            dst.FieldTag = current.Tag;
                            dst.FieldName = current.Name;
                            dst.FieldValue = current.Value;
                            dst.ResultCode = FixValidationCode.Field_Present;
                            dst.ResultText = "User-defined Field";

                            result.Add(dst);
                        }
                    }
                }
            }

            return result; 
        }

        // REC: The ValidateGroup method validates a repeating group
        // that is encountered in the source message, against the 
        // group's layout as defined in the FIX dictionary:
        private List<FixValidationResult> ValidateGroup(Queue<SrcField> queue, FixValidationResult group)
        {
            List<FixValidationResult> result = new List<FixValidationResult>();

            List<FixValidationResult> template = new List<FixValidationResult>();

            // REC: Attempt to convert the group's value
            // from a string into an integer that indicates
            // the number of repeating groups that follow:
            int nCount = -1;
            if (int.TryParse(group.FieldValue, out nCount) == false)
            {
                // REC: If the group entry's value is not an 
                // integer, the repeating groups cannot be 
                // parsed from the message:
                return result;
            }
            else
            {
                for (int i = 0; i != nCount; i++)
                {
                    foreach (FixValidationResult srcFvr in group.Nodes)
                    {
                        FixValidationResult child = srcFvr.Clone() as FixValidationResult;
                        template.Add(child);
                    }
                }
            }

            // REC: Attempt to validate N instances of the
            // group, as defined by the group's template:
            while (queue.Count > 0)
            {
                bool processed = false;

                // REC: Commented this out - we need to peek
                // at the message rather than dequeue it:
                //SrcField current = queue.Dequeue();
                SrcField current = queue.Peek();

                if (template.Count > 0)
                {
                    // REC: Maintain a remove list for the template
                    // elements since lists cannot be modified during
                    // an enumeration over their elements:
                    List<FixValidationResult> listRemove = new List<FixValidationResult>();
                    foreach (FixValidationResult r in template)
                    {
                        // REC: Add the current element to the remove
                        // list so that it can be removed from the template
                        // after being added to the result set:
                        listRemove.Add(r);

                        // REC: Move the template element into the 
                        // validation result set:
                        result.Add(r);

                        if (current.Tag == r.FieldTag)
                        {
                            r.FieldValue = current.Value;
                            r.ResultCode = FixValidationCode.Field_Present;
                            r.ResultText = "Field Present";

                            // REC: Since a match has been found, the element
                            // can be removed from the queue at this point:
                            queue.Dequeue();

                            if (r.Nodes.Count > 0)
                            {
                                r.Nodes = ValidateGroup(queue, r);
                            }

                            processed = true;
                            break; 
                        }
                    }

                    foreach (FixValidationResult target in listRemove)
                    {
                        template.Remove(target);
                    }
                }

                if (processed == false)
                {
                    bool fieldOOS = false; 
                    foreach (FixValidationResult m in result)
                    {
                        if (m.ResultCode == FixValidationCode.Field_Missing)
                        {
                            if (current.Tag == m.FieldTag)
                            {
                                m.FieldValue = current.Value;
                                m.ResultCode = FixValidationCode.Field_Present;
                                m.ResultText = "Field Present";
                                if (fieldOOS == true)
                                {
                                    m.ResultCode = FixValidationCode.Field_OOS;
                                    m.ResultText = "Out of Sequence";
                                }

                                // REC: Since a match has been found, the element
                                // can be removed from the queue at this point:
                                queue.Dequeue();

                                if (m.Nodes.Count > 0)
                                {
                                    m.Nodes = ValidateGroup(queue, m);
                                }

                                processed = true;
                                result.Remove(m);
                                result.Add(m);
                                break;
                            }
                            else
                            {
                                // REC: The field is missing, but it doesn't match
                                // the current one. If the missing field is also a
                                // required field, then set the OOS flag:
                                if (m.FieldRequired == true)
                                {
                                    fieldOOS = true;
                                }
                            }
                        }
                    }
                }

                if (processed == false)
                {
                    break;
                }

            }

            return result;
        }

        // REC: The PopulateQueue method is a recursive function
        // that loads all of the elements in a FixCollection into
        // an instance of a SrcField queue so that the validator
        // can then process all the elements sequentially:
        private void PopulateQueue(Queue<SrcField> queue, FixCollection elements)
        {
            foreach (IFixElement element in elements)
            {
                if (element is FixField)
                {
                    FixField msgField = element as FixField;
                    if (msgField != null)
                    {
                        // REC: Attempt to resolve the field's name 
                        // by retrieving it using it's tag:
                        string fieldName = ResolveFieldName(msgField.Tag.ToString());
                        if (fieldName == null)
                        {
                            fieldName = "Unresolved";
                        }

                        // REC: Create a new instance of SrcField that
                        // represents the contents of this field:
                        SrcField srcField = new SrcField();
                        srcField.Tag = msgField.Tag.ToString();
                        srcField.Name = fieldName;
                        srcField.Value = msgField.Content;

                        queue.Enqueue(srcField); 
                    }
                }
                else if (element is FixGroup)
                {
                    FixGroup msgGroup = element as FixGroup;
                    if (msgGroup != null)
                    {
                        string groupName = ResolveFieldName(msgGroup.Tag.ToString());
                        if (groupName == null)
                        {
                            groupName = "Unresolved";
                        }

                        // REC: Create a new instance of SrcField that
                        // represents the contents of the group:
                        SrcField srcField = new SrcField();
                        srcField.Tag = msgGroup.Tag.ToString();
                        srcField.Name = groupName;
                        srcField.Value = msgGroup.Content;

                        queue.Enqueue(srcField);

                        foreach (FixCollection instance in msgGroup.Instances)
                        {
                            PopulateQueue(queue, instance);
                        }
                    }
                }
            }
        }

        // REC: The ResolveFieldName method attempts to
        // resolve a FIX tag to it's corresponding name:
        private string ResolveFieldName(string tag)
        {
            string result = null; 

            // REC: Although a tag may be improperly presented
            // as a string, it needs to be converted into an 
            // integer in order to resolve against a dictionary:
            int nTag = -1;
            if (int.TryParse(tag, out nTag) == true)
            {
                foreach (FixDictionary dx in _settings.Dictionaries)
                {
                    FixDxField dxField = dx.Fields.GetElement(nTag) as FixDxField;
                    if (dxField != null)
                    {
                        result = dxField.Name;
                        break;
                    }
                }
            }

            return result; 
        }


        private FixValidationResult CreateResult(IFixDxElement element)
        {
            FixValidationResult result = null; 

            if (element is FixDxResolvedField)
            {
                FixDxResolvedField dxField = element as FixDxResolvedField;

                FixValidationResult fvr = new FixValidationResult();
                fvr.FieldTag = dxField.Tag.ToString();
                fvr.FieldName = dxField.Name;
                fvr.FieldValue = null;
                fvr.FieldRequired = dxField.Required;
                fvr.ResultCode = FixValidationCode.Field_Missing;
                fvr.ResultText = "Field not present.";

                result = fvr; 
            }
            else if (element is FixDxResolvedGroup)
            {
                FixDxResolvedGroup dxGroup = element as FixDxResolvedGroup;

                FixValidationResult fvr = new FixValidationResult();
                fvr.FieldTag = dxGroup.Tag.ToString();
                fvr.FieldName = dxGroup.Name;
                fvr.FieldValue = null;
                fvr.FieldRequired = dxGroup.Required;
                fvr.ResultCode = FixValidationCode.Field_Missing;
                fvr.ResultText = "Group not present.";

                // REC: Expand all of the block references in the
                // group entry so that they can be added:
                foreach (IFixDxElement dxElement in dxGroup.Elements)
                {
                    fvr.Nodes.Add(CreateResult(dxElement));
                }

                result = fvr; 
            }

            return result; 
        }

    }
}
