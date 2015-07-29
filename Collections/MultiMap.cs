using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Xperitos.Common.Collections
{
    /// <summary>
    /// Maps a single key to multiple values.
    /// </summary>
    [Serializable]
    public class MultiMap<TK, TV> : ILookup<TK, TV>
    {
        [Serializable]
        class GroupingWrapper : List<TV>, IGrouping<TK, TV>
        {
            public GroupingWrapper(TK key)
            {
                Key = key;
            }

            public TK Key { get; private set; }
        }

        private readonly Dictionary<TK, IGrouping<TK, TV>> m_dictionary = new Dictionary<TK, IGrouping<TK, TV>>();

        public int Count { get; private set; }

        private void ResetCount()
        {
            Count = m_dictionary.Sum(v => ((GroupingWrapper) v.Value).Count);
        }

        public IEnumerable<TV> this[TK key]
        {
            get { return m_dictionary[key]; }
        }

        public bool TryGetValues(TK key, out IEnumerable<TV> values)
        {
            IGrouping<TK, TV> grouping;
            if (m_dictionary.TryGetValue(key, out grouping))
            {
                values = grouping;
                return true;
            }

            values = null;
            return false;
        }

        public IEnumerable<TV> Values
        {
            get { return m_dictionary.Values.SelectMany(v => v); }
        }

        public IEnumerable<TK> Keys
        {
            get { return m_dictionary.Keys; }
        }

        public bool Contains(TK key)
        {
            return m_dictionary.ContainsKey(key);
        }

        public bool ContainsKey(TK key)
        {
            return m_dictionary.ContainsKey(key);
        }

        public bool ContainsValue(TV value)
        {
            return m_dictionary.SelectMany(v => v.Value).Contains(value);
        }

        public void RemoveKey(TK key)
        {
            m_dictionary.Remove(key);
            ResetCount();
        }

        public void RemoveValue(TV value)
        {
            // Try to remove the value from each of the keys.
            Keys.ToList().ForEach(k => RemoveValueInternal(k, value));
            ResetCount();
        }

        public void RemoveValue(TK key, TV value)
        {
            RemoveValueInternal(key, value);
            ResetCount();
        }

        private void RemoveValueInternal(TK key, TV value)
        {
            IGrouping<TK, TV> tmpList;
            if (!m_dictionary.TryGetValue(key, out tmpList))
                return;

            GroupingWrapper list = (GroupingWrapper)tmpList;
            list.Remove(value);

            if (list.Count == 0)
                m_dictionary.Remove(key);
        }

        public void Add(TK key, TV value)
        {
            IGrouping<TK, TV> tmpList;
            if (!m_dictionary.TryGetValue(key, out tmpList))
            {
                m_dictionary[key] = tmpList = new GroupingWrapper(key);
            }

            GroupingWrapper list = (GroupingWrapper)tmpList;
            list.Add(value);
            Count += 1;
        }

        public IEnumerator<IGrouping<TK, TV>> GetEnumerator()
        {
            return m_dictionary.Values.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}