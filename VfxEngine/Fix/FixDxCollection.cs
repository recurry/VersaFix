using System;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VfxEngine.Fix
{
    /// <summary>
    /// The FixDxCollection class manages a collection of elements
    /// that are related to an instance of a FIX dictionary.
    /// </summary>
    public sealed class FixDxCollection : Collection<IFixDxElement> 
    {
        /// <summary>
        /// The map of all elements that are contained in an instance
        /// of the collection, keyed by their respective FIX tags.
        /// </summary>
        private Dictionary<int, List<IFixDxElement>> _mapElementsByTag = new Dictionary<int, List<IFixDxElement>>();

        /// <summary>
        /// The map of all elements that are contained in an instance
        /// of the collection, keyed by their respective FIX names.
        /// </summary>
        private Dictionary<string, List<IFixDxElement>> _mapElementsByName = new Dictionary<string, List<IFixDxElement>>();

        /// <summary>
        /// The GetElement method retrieves the first element in
        /// the collection that has the specified FIX tag.
        /// </summary>
        /// <param name="tag">
        /// The FIX tag of the element to retrieve.
        /// </param>
        /// <returns>
        /// The first element in the collection that has the
        /// specified FIX tag.
        /// </returns>
        public IFixDxElement GetElement(int tag)
        {
            IFixDxElement result = null;
            if (_mapElementsByTag.ContainsKey(tag))
            {
                result = _mapElementsByTag[tag][0];
            }
            return result;
        }

        /// <summary>
        /// The GetElement method retrieves the first element in
        /// the collection that has the specified name.
        /// </summary>
        /// <param name="name">
        /// The FIX name of the element to retrieve.
        /// </param>
        /// <returns>
        /// The first element in the collection that has the
        /// specified FIX name.
        /// </returns>
        public IFixDxElement GetElement(string name)
        {
            IFixDxElement result = null;
            if (_mapElementsByName.ContainsKey(name))
            {
                result = _mapElementsByName[name][0];
            }
            return result;
        }

        /// <summary>
        /// The InsertItem method is an override of the method that
        /// is provided by the base class. The override is done for
        /// updating the tag/name maps as items are added.
        /// </summary>
        /// <param name="index">
        /// The position in the collection at which the specified
        /// element is going to be added.
        /// </param>
        /// <param name="item">
        /// The dictionary element that is going to be added.
        /// </param>
        protected override void InsertItem(int index, IFixDxElement item)
        {
            base.InsertItem(index, item);

            // REC: If the item has a FIX tag that is
            // associated with it, it is added to the
            // map of items keyed by FIX tag:
            if (item.HasTag == true)
            {
                if (!_mapElementsByTag.ContainsKey(item.Tag))
                {
                    _mapElementsByTag.Add(item.Tag, new List<IFixDxElement>());
                }

                _mapElementsByTag[item.Tag].Add(item);
            }

            // REC: Add the item to the map of elements that
            // is keyed by FIX element name:
            if (!_mapElementsByName.ContainsKey(item.Name))
            {
                _mapElementsByName.Add(item.Name, new List<IFixDxElement>());
            }

            _mapElementsByName[item.Name].Add(item);
        }

        /// <summary>
        /// The SetItem method overrides the method that is 
        /// provided by the base collection class.
        /// </summary>
        /// <param name="index">
        /// The index in the collection that the element is
        /// to be assigned to.
        /// </param>
        /// <param name="item">
        /// The element that is being assigned to the specified
        /// index in the collection.
        /// </param>
        protected override void SetItem(int index, IFixDxElement item)
        {
            base.SetItem(index, item);
            RegenerateMaps();
        }

        /// <summary>
        /// The RemoveItem method overrides the method that is
        /// provided by the base collection class.
        /// </summary>
        /// <param name="index"></param>
        protected override void RemoveItem(int index)
        {
            base.RemoveItem(index);
            RegenerateMaps();
        }

        /// <summary>
        /// The RegenerateMaps method is invoked to regenerate 
        /// the maps the collection uses to access elements by
        /// their FIX tags or FIX element names.
        /// </summary>
        private void RegenerateMaps()
        {
            // REC: Reset the map that indexes all of the 
            // elements by their FIX tag:
            _mapElementsByTag.Clear();

            // REC: Reset the map that indexes all of the
            // elements by their FIX element name:
            _mapElementsByName.Clear();

            // REC: Iterate over all of the items that are in
            // the collection and rebuild the maps:
            foreach(IFixDxElement item in this.Items)
            {
                // REC: If the item has a FIX tag that is
                // associated with it, it is added to the
                // map of items keyed by FIX tag:
                if (item.HasTag == true)
                {
                    if (!_mapElementsByTag.ContainsKey(item.Tag))
                    {
                        _mapElementsByTag.Add(item.Tag, new List<IFixDxElement>());
                    }

                    _mapElementsByTag[item.Tag].Add(item);
                }

                // REC: Add the item to the map of elements that
                // is keyed by FIX element name:
                if (!_mapElementsByName.ContainsKey(item.Name))
                {
                    _mapElementsByName.Add(item.Name, new List<IFixDxElement>());
                }

                _mapElementsByName[item.Name].Add(item);
            }
        }
    }
}
