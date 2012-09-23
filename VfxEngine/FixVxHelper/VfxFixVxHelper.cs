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

namespace VfxEngine.FixVxHelper
{
    /// <summary>
    /// The VfxFixVxHelper (Vx=Versions) class provides some utility
    /// methods that simply the process of retrieving information that
    /// is related to a specific version definition.
    /// </summary>
    public class VfxFixVxHelper
    {
        /// <summary>
        /// The VfxEngine FIX version definition registry the
        /// helper class uses for retrieving version details.
        /// </summary>
        private IVfxFixVxRegistry _vxRegistry = null;

        /// <summary>
        /// The VfxEngine FIX dictionary registry the helper
        /// class uses for retrieving dictionary details.
        /// </summary>
        private IVfxFixDxRegistry _dxRegistry = null;

        /// <summary>
        /// The Init method is called to initialize the class
        /// with a reference to the service container that it
        /// needs in order to retrieve and initialize its own
        /// service references.
        /// </summary>
        /// <param name="services">
        /// The service container that contains the services
        /// that the class needs to access.
        /// </param>
        public void Init(IVfxServices services)
        {
            _vxRegistry = services.GetService(typeof(IVfxFixVxRegistry)) as IVfxFixVxRegistry;
            if (_vxRegistry == null)
            {
                throw new ArgumentException("The service container doesn't include a reference to IVfxFixVxRegistry.");
            }

            _dxRegistry = services.GetService(typeof(IVfxFixDxRegistry)) as IVfxFixDxRegistry;
            if (_dxRegistry == null)
            {
                throw new ArgumentException("The service container doesn't include a reference to IVfxFixDxRegistry.");
            }
        }

        /// <summary>
        /// The GetDictionaryFromField method is invoked to attempt
        /// to determine which dictionary associated with a specific
        /// version definition contains a specific FIX field.
        /// </summary>
        /// <param name="version">
        /// The FIX version definition to search.
        /// </param>
        /// <param name="tag">
        /// The FIX tag of the element to search for.
        /// </param>
        /// <returns>
        /// The first dictionary in the version definition that has
        /// a field definition that matches the specified tag.
        /// </returns>
        public FixDictionary GetDictionaryFromField(string version, int tag)
        {
            FixDictionary result = null;

            if (_vxRegistry != null)
            {
                VfxFixVxRecord vxDetails = _vxRegistry.Get(version);
                if (vxDetails != null)
                {
                    if (_dxRegistry != null)
                    {
                        foreach (VfxFixVersion_Dictionary_Reference dxEntry in vxDetails.Dictionaries)
                        {
                            FixDictionary dxInstance = _dxRegistry.GetEntry(dxEntry.Name);
                            if (dxInstance != null)
                            {
                                if (dxInstance.Fields.GetElement(tag) != null)
                                {
                                    result = dxInstance;
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
        /// The GetUniqueFields method retrieves a collection which
        /// represents the set of all FIX field definitions found in
        /// the dictionaries associated with a specific version.
        /// </summary>
        /// <param name="version">
        /// The name of the version definition that is to be used
        /// for looking up the FIX field definitions.
        /// </param>
        /// <returns>
        /// The collection of FIX field definitions that represents
        /// the complete set of all of the field definitions in the
        /// dictionaries associated with the specified version.
        /// </returns>
        public FixDxCollection GetUniqueFields(string version)
        {
            FixDxCollection result = new FixDxCollection();
            
            if (_vxRegistry != null)
            {
                VfxFixVxRecord vxDetails = _vxRegistry.Get(version);
                if (vxDetails != null)
                {
                    if (_dxRegistry != null)
                    {
                        // REC: Temporary map that is used to ensure there
                        // is only one instance of a field for each tag.
                        Dictionary<int, FixDxField> mapFields = new Dictionary<int, FixDxField>();

                        // REC: Iterate over all of the FIX dictionaries that
                        // are associated with the version and build the list
                        // of FIX field definitions from them:
                        foreach (VfxFixVersion_Dictionary_Reference dxEntry in vxDetails.Dictionaries)
                        {
                            FixDictionary dxInstance = _dxRegistry.GetEntry(dxEntry.Name);
                            if (dxInstance != null)
                            {
                                foreach (IFixDxElement dxElement in dxInstance.Fields)
                                {
                                    FixDxField dxField = dxElement as FixDxField;
                                    if (dxField != null)
                                    {
                                        if (mapFields.ContainsKey(dxField.Tag) == false)
                                        {
                                            mapFields.Add(dxField.Tag, dxField);
                                        }
                                        else
                                        {
                                            mapFields[dxField.Tag] = dxField;
                                        }
                                    }
                                }
                            }
                        }

                        foreach (int key in mapFields.Keys)
                        {
                            result.Add(mapFields[key]);
                        }
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// The GetUniqueMessages method retrieves a collection which
        /// represents the set of all FIX message definitions found in
        /// the dictionaries associated with a specific version.
        /// </summary>
        /// <param name="version">
        /// The name of the version definition that is to be used
        /// for looking up the FIX message definitions.
        /// </param>
        /// <returns>
        /// The collection of message definitions that represents
        /// the complete set of all of the field definitions in the
        /// dictionaries associated with the specified version.
        /// </returns>
        public FixDxCollection GetUniqueMessages(string version)
        {
            FixDxCollection result = new FixDxCollection();

            if (_vxRegistry != null)
            {
                VfxFixVxRecord vxDetails = _vxRegistry.Get(version);
                if (vxDetails != null)
                {
                    if (_dxRegistry != null)
                    {
                        // REC: Temporary map that is used to ensure there
                        // is only one instance of each MsgType:
                        Dictionary<string, FixDxMessage> mapMessages = new Dictionary<string, FixDxMessage>();

                        // REC: Iterate over all of the FIX dictionaries that
                        // are associated with the version and build the list
                        // of FIX field definitions from them:
                        foreach (VfxFixVersion_Dictionary_Reference dxEntry in vxDetails.Dictionaries)
                        {
                            FixDictionary dxInstance = _dxRegistry.GetEntry(dxEntry.Name);
                            if (dxInstance != null)
                            {
                                foreach (IFixDxElement dxElement in dxInstance.Messages)
                                {
                                    FixDxMessage dxMessage = dxElement as FixDxMessage;
                                    if (dxMessage != null)
                                    {
                                        if (mapMessages.ContainsKey(dxMessage.MsgType) == false)
                                        {
                                            mapMessages.Add(dxMessage.MsgType, dxMessage);
                                        }
                                        else
                                        {
                                            mapMessages[dxMessage.MsgType] = dxMessage;
                                        }
                                    }
                                }
                            }
                        }

                        foreach (string key in mapMessages.Keys)
                        {
                            result.Add(mapMessages[key]);
                        }
                    }
                }
            }

            return result;

        }

        /// <summary>
        /// The GetDictionaries method analyzes a collection of elements
        /// and determines which FIX data dictionaries are relevant to the
        /// collection of elements. The method uses VfxFixVxMatcher to find
        /// the version definitions that match the elements, and then it goes
        /// to the version registry to retrieve the relevant dictionaries.
        /// </summary>
        /// <param name="elements">
        /// The collection of FIX elements to analyze.
        /// </param>
        /// <returns>
        /// The collection of all FIX dictionaries that are relevant to the
        /// elements, according to the corresponding version definitions.
        /// </returns>
        public Collection<string> GetDictionaries(FixCollection elements)
        {
            Collection<string> result = new Collection<string>();
            if ((_vxRegistry != null) && (_dxRegistry != null))
            {
                VfxFixVxMatcher matcher = new VfxFixVxMatcher();
                matcher.Init(_vxRegistry);

                string sxVersion = matcher.GetVersion(elements, "session");
                if (sxVersion == null)
                {
                    sxVersion = matcher.GetVersion(elements, "combined");
                }

                string axVersion = matcher.GetVersion(elements, "app");
                if (axVersion == null)
                {
                    axVersion = matcher.GetVersion(elements, "combined");
                }

                VfxFixVxRecord sxDetails = _vxRegistry.Get(sxVersion);
                if (sxDetails != null)
                {
                    foreach (VfxFixVersion_Dictionary_Reference dxEntry in sxDetails.Dictionaries)
                    {
                        result.Add(dxEntry.Name);
                    }
                }

                if ((sxVersion == null) || (sxVersion.CompareTo(axVersion) != 0))
                {
                    VfxFixVxRecord axDetails = _vxRegistry.Get(axVersion);
                    if (axDetails != null)
                    {
                        foreach (VfxFixVersion_Dictionary_Reference dxEntry in axDetails.Dictionaries)
                        {
                            result.Add(dxEntry.Name);
                        }
                    }
                }
            }

            return result;
        }
    }
}
