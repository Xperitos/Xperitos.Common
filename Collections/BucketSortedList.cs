using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Xperitos.Common.Utils;

namespace Xperitos.Common.Collections
{
    sealed class DictionaryDebugView<K, V>
    {
        private readonly IDictionary<K, V> m_dict;

        [DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
        public KeyValuePair<K, V>[] Items
        {
            get
            {
                KeyValuePair<K, V>[] array = new KeyValuePair<K, V>[m_dict.Count];
                this.m_dict.CopyTo(array, 0);
                return array;
            }
        }

        public DictionaryDebugView(IDictionary<K, V> dictionary)
        {
            if (dictionary == null)
                throw new ArgumentNullException(nameof(dictionary));
            m_dict = dictionary;
        }
    }

    /// <summary>
    /// Bucketer interface for creating buckets.
    /// </summary>
    /// <typeparam name="TKey"></typeparam>
    /// <typeparam name="TBucket"></typeparam>
    public interface IBucketer<in TKey, out TBucket>
    {
        TBucket GetBucketKey(TKey key);
    }

    public interface IBucketable<out TBucket>
    {
        TBucket GetBucket();
    }

    /// <summary>
    /// Factory for buckets.
    /// </summary>
    public static class Bucketer
    {
        public static IBucketer<TKey, TBucket> Create<TKey, TBucket>(Func<TKey, TBucket> func) => new BucketerFunc<TKey, TBucket>(func);

        private class BucketerFunc<TKey, TBucket> : IBucketer<TKey, TBucket>
        {
            public BucketerFunc(Func<TKey, TBucket> func)
            {
                m_func = func;
            }

            private readonly Func<TKey, TBucket> m_func;
            public TBucket GetBucketKey(TKey key) => m_func(key);
        }

        private class BucketerBucketable<TKey, TBucket> : IBucketer<TKey, TBucket>
        {
            public TBucket GetBucketKey(TKey key) => ((IBucketable<TBucket>)key).GetBucket();
        }

        private class BucketerIdentity<TKey, TBucket> : IBucketer<TKey, TBucket>
        {
            public TBucket GetBucketKey(TKey key) => default(TBucket);
        }

        public static IBucketer<TKey, TBucket> Default<TKey, TBucket>()
        {
            // TODO: Might not be as efficient. Consider caching GetTypeInfo calls.
            if ( typeof(IBucketable<TBucket>).GetTypeInfo().IsAssignableFrom(typeof(TKey).GetTypeInfo()) )
                return new BucketerBucketable<TKey, TBucket>();

            return new BucketerIdentity<TKey, TBucket>();
        }
    }

    /// <summary>
    /// Similar to <see cref="SortedList{TKey,TValue}"/> but provides faster insertion and lookup by using buckets.
    /// </summary>
    /// <remarks>Bucket sort order MUST BE IDENTICAL to the key sort order (e.g. Key is full date+time and bucket is just the day) other-wise an unexpected behavior will occur</remarks>
    /// <typeparam name="TKey"></typeparam>
    /// <typeparam name="TValue"></typeparam>
    /// <typeparam name="TBucket"></typeparam>
    [DebuggerTypeProxy(typeof(DictionaryDebugView<,>))]
    [DebuggerDisplay("Count = {Count}")]
    [ComVisible(false)]
    public class BucketSortedList<TKey, TValue, TBucket> : IDictionary<TKey, TValue>, IDictionary
    {
        public BucketSortedList() : this(Bucketer.Default<TKey, TBucket>())
        {
            
        }

        public BucketSortedList(Func<TKey, TBucket> bucketKeyFunc) : this(Bucketer.Create(bucketKeyFunc))
        {
        }

        public BucketSortedList(IBucketer<TKey, TBucket> bucketer)
        {
            m_bucketer = bucketer;
            m_keys = new KeyCollection(this);
            m_values = new ValueCollection(this);
        }

        private readonly IBucketer<TKey, TBucket> m_bucketer;
        private readonly SortedList<TBucket, SortedList<TKey, TValue>> m_buckets = new SortedList<TBucket, SortedList<TKey, TValue>>();
        private long m_version;

        private readonly object m_syncRoot = new object();

        private readonly KeyCollection m_keys;
        private readonly ValueCollection m_values;

        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
        {
            return new Enumerator(this);
        }

        void IDictionary.Remove(object key)
        {
            Remove((TKey)key);
        }

        object IDictionary.this[object key]
        {
            get { return this[(TKey) key]; }
            set { this[(TKey) key] = (TValue) value; }
        }

        private class DictionaryEnumerator : IDictionaryEnumerator
        {
            public DictionaryEnumerator(BucketSortedList<TKey, TValue, TBucket> list)
            {
                m_list = list;
                m_version = list.m_version;

                Reset();
            }

            private BucketSortedList<TKey, TValue, TBucket> m_list;
            private readonly long m_version;

            private SortedList<TKey, TValue> m_bucket;
            private int m_bucketIndex;
            private int m_index;

            public void Dispose()
            {
                m_list = null;
                m_bucket = null;
            }

            public bool MoveNext()
            {
                if (m_version != m_list.m_version)
                    throw new InvalidOperationException("List changed");

                while (m_bucket != null)
                {
                    ++m_index;
                    if (m_index >= m_bucket.Count)
                    {
                        m_index = -1;
                        ++m_bucketIndex;
                        if (m_bucketIndex >= m_list.m_buckets.Count)
                            m_bucket = null;
                        else
                            m_bucket = m_list.m_buckets.Values[m_bucketIndex];
                    }
                    else
                    {
                        Current = new KeyValuePair<TKey, TValue>(m_bucket.Keys[m_index], m_bucket.Values[m_index]);
                        return true;
                    }
                }

                return false;
            }

            public void Reset()
            {
                if (m_version != m_list.m_version)
                    throw new InvalidOperationException("List changed");

                m_index = -1;
                m_bucketIndex = 0;
                if (m_list.m_buckets.Count == 0)
                    m_bucket = null;
                else
                    m_bucket = m_list.m_buckets.Values[m_bucketIndex];
            }

            public KeyValuePair<TKey, TValue> Current { get; private set; }

            object IEnumerator.Current => Current;
            public object Key => Current.Key;
            public object Value => Current.Value;
            public DictionaryEntry Entry => new DictionaryEntry(Key, Value);
        }

        private class Enumerator : IEnumerator<KeyValuePair<TKey, TValue>>
        {
            public Enumerator(BucketSortedList<TKey, TValue, TBucket> list)
            {
                m_list = list;
                m_version = list.m_version;

                Reset();
            }

            private BucketSortedList<TKey, TValue, TBucket> m_list;
            private readonly long m_version;

            private SortedList<TKey, TValue> m_bucket;
            private int m_bucketIndex;
            private int m_index;

            public void Dispose()
            {
                m_list = null;
                m_bucket = null;
            }

            public bool MoveNext()
            {
                if (m_version != m_list.m_version)
                    throw new InvalidOperationException("List changed");

                while (m_bucket != null)
                {
                    ++m_index;
                    if (m_index >= m_bucket.Count)
                    {
                        m_index = -1;
                        ++m_bucketIndex;
                        if (m_bucketIndex >= m_list.m_buckets.Count)
                            m_bucket = null;
                        else
                            m_bucket = m_list.m_buckets.Values[m_bucketIndex];
                    }
                    else
                    {
                        Current = new KeyValuePair<TKey, TValue>(m_bucket.Keys[m_index], m_bucket.Values[m_index]);
                        return true;
                    }
                }

                return false;
            }

            public void Reset()
            {
                if (m_version != m_list.m_version)
                    throw new InvalidOperationException("List changed");

                m_index = -1;
                m_bucketIndex = 0;
                if (m_list.m_buckets.Count == 0)
                    m_bucket = null;
                else
                    m_bucket = m_list.m_buckets.Values[m_bucketIndex];
            }

            public KeyValuePair<TKey, TValue> Current { get; private set; }

            object IEnumerator.Current => Current;
        }

        private class KeyCollection : ICollection<TKey>, ICollection
        {
            public KeyCollection(BucketSortedList<TKey, TValue, TBucket> list)
            {
                m_list = list;
            }

            private readonly BucketSortedList<TKey, TValue, TBucket> m_list;
            public IEnumerator<TKey> GetEnumerator()
            {
                return m_list.Select(v => v.Key).GetEnumerator();
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }

            public void Add(TKey item)
            {
                throw new NotImplementedException();
            }

            public void Clear()
            {
                throw new NotImplementedException();
            }

            public bool Contains(TKey item)
            {
                throw new NotImplementedException();
            }

            public void CopyTo(TKey[] array, int arrayIndex)
            {
                int idx = arrayIndex;
                foreach (var item in this)
                {
                    if (idx >= array.Length)
                        break;

                    array[idx] = item;
                    ++idx;
                }
            }

            public bool Remove(TKey item)
            {
                throw new NotImplementedException();
            }

            public void CopyTo(Array array, int index)
            {
                throw new NotImplementedException();
            }

            public int Count => m_list.Count;
            public object SyncRoot => m_list.m_syncRoot;
            public bool IsSynchronized { get; } = false;
            public bool IsReadOnly { get; } = true;
        }

        private class ValueCollection : ICollection<TValue>, ICollection
        {
            public ValueCollection(BucketSortedList<TKey, TValue, TBucket> list)
            {
                m_list = list;
            }

            private readonly BucketSortedList<TKey, TValue, TBucket> m_list;
            public IEnumerator<TValue> GetEnumerator()
            {
                return m_list.Select(v => v.Value).GetEnumerator();
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }

            public void Add(TValue item)
            {
                throw new NotImplementedException();
            }

            public void Clear()
            {
                throw new NotImplementedException();
            }

            public bool Contains(TValue item)
            {
                throw new NotImplementedException();
            }

            public void CopyTo(TValue[] array, int arrayIndex)
            {
                int idx = arrayIndex;
                foreach (var item in this)
                {
                    if (idx >= array.Length)
                        break;

                    array[idx] = item;
                    ++idx;
                }
            }

            public bool Remove(TValue item)
            {
                throw new NotImplementedException();
            }

            public void CopyTo(Array array, int index)
            {
                throw new NotImplementedException();
            }

            public int Count => m_list.Count;
            public object SyncRoot => m_list.m_syncRoot;
            public bool IsSynchronized { get; } = false;
            public bool IsReadOnly { get; } = true;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        private IDictionary<TKey, TValue> GetBucket(TKey key, bool create = false)
        {
            var bucketKey = m_bucketer.GetBucketKey(key);
            SortedList<TKey, TValue> bucket;
            if (m_buckets.TryGetValue(bucketKey, out bucket))
                return bucket;

            if (!create)
                return null;

            m_version++;
            bucket = new SortedList<TKey, TValue>();
            m_buckets.Add(bucketKey, bucket);

            return bucket;
        }

        private SortedList<TKey, TValue> GetBucket(TKey key, out int countBefore)
        {
            var bucketKey = m_bucketer.GetBucketKey(key);
            SortedList<TKey, TValue> bucket;
            if (m_buckets.TryGetValue(bucketKey, out bucket))
            {
                countBefore = m_buckets.Values.TakeWhile(v => v != bucket).Sum(v => v.Count);
                return bucket;
            }

            countBefore = 0;
            return null;
        }

        public void Add(TKey key, TValue value)
        {
            var bucket = GetBucket(key, true);
            bucket.Add(key, value);
            ++m_version;
            Count = Count + 1;
        }

        public void Add(KeyValuePair<TKey, TValue> item)
        {
            Add(item.Key, item.Value);
        }

        bool IDictionary.Contains(object key)
        {
            var bucket = GetBucket((TKey)key);
            if (bucket == null)
                return false;

            return bucket.ContainsKey((TKey) key);
        }

        void IDictionary.Add(object key, object value)
        {
            Add((TKey)key, (TValue)value);
        }

        public void Clear()
        {
            ++m_version;
            m_buckets.Clear();
            Count = 0;
        }

        IDictionaryEnumerator IDictionary.GetEnumerator()
        {
            return new DictionaryEnumerator(this);
        }

        public bool Contains(KeyValuePair<TKey, TValue> item)
        {
            var bucket = GetBucket(item.Key);
            if (bucket == null)
                return false;

            return bucket.Contains(item);
        }

        public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
        {
            int idx = arrayIndex;
            foreach (var item in this)
            {
                if (idx >= array.Length)
                    break;

                array[idx] = item;
                ++idx;
            }
        }

        public bool Remove(KeyValuePair<TKey, TValue> item)
        {
            var bucket = GetBucket(item.Key);
            if (bucket == null)
                return false;

            if (bucket.Remove(item))
            {
                ++m_version;
                return true;
            }

            return false;
        }

        void ICollection.CopyTo(Array array, int index)
        {
            throw new NotImplementedException();
        }

        public int Count { get; private set; }

        object ICollection.SyncRoot => m_syncRoot;

        bool ICollection.IsSynchronized { get; } = false;

        public bool IsReadOnly { get; } = false;
        bool IDictionary.IsFixedSize { get; } = false;

        public bool ContainsKey(TKey key)
        {
            var bucket = GetBucket(key);
            if (bucket == null)
                return false;

            return bucket.ContainsKey(key);
        }

        public bool Remove(TKey key)
        {
            var bucket = GetBucket(key);
            if (bucket == null)
                return false;

            if (bucket.Remove(key))
            {
                ++m_version;
                return true;
            }

            return false;
        }

        public bool TryGetValue(TKey key, out TValue value)
        {
            var bucket = GetBucket(key);
            if (bucket != null)
                return bucket.TryGetValue(key, out value);

            value = default(TValue);
            return false;
        }

        public TValue this[TKey key]
        {
            get
            {
                TValue value;
                if ( !TryGetValue(key, out value) )
                    throw new InvalidOperationException("Key not found");
                return value;
            }
            set
            {
                var bucket = GetBucket(key, true);
                var beforeCount = bucket.Count;
                bucket[key] = value;
                if (bucket.Count != beforeCount)
                    Count = Count + 1;
                ++m_version;
            }
        }

        public KeyValuePair<TKey, TValue> GetAt(int index)
        {
            if (index < 0 || index >= Count)
                throw new InvalidOperationException("Out of bounds");

            foreach (var bucket in m_buckets.Values)
            {
                if (index < bucket.Count)
                    return new KeyValuePair<TKey, TValue>(bucket.Keys[index], bucket.Values[index]);

                index -= bucket.Count;
            }

            // Should never reach here.
            throw new InvalidOperationException();
        }

        public void RemoveAt(int index)
        {
            if (index < 0 || index >= Count)
                throw new InvalidOperationException("Out of bounds");

            foreach (var bucket in m_buckets.Values)
            {
                if (index < bucket.Count)
                {
                    bucket.RemoveAt(index);
                    Count -= 1;
                    m_version++;
                    return;
                }

                index -= bucket.Count;
            }

            // Should never reach here.
            throw new InvalidOperationException();
        }

        public int BinarySearchIndexOf<TSubKey>(TSubKey key,
            Func<TSubKey, TBucket> bucketSelector,
            Func<TKey, TSubKey> subKeySelector)
        {
            var bucketKey = bucketSelector(key);
            var bucketIdx = m_buckets.Keys.BinarySearchIndexOf(bucketKey, v => v);

            // Nothing found!?
            if (bucketIdx == -1)
                return -1;

            if (bucketIdx < 0)
                bucketIdx = ~bucketIdx;

            // Overflow? Return the last element.
            if (bucketIdx == m_buckets.Count)
                return ~(m_buckets.Values.Sum(v => v.Count));

            var bucket = m_buckets.Values[bucketIdx];

            var countBefore = m_buckets.Values.TakeWhile(v => v != bucket).Sum(v => v.Count);

            var idx = bucket.Keys.BinarySearchIndexOf(key, subKeySelector);
            if (idx < 0)
            {
                idx = ~idx;
                idx += countBefore;
                return ~idx;
            }

            return idx;
        }

        public ICollection<TKey> Keys => m_keys;
        ICollection IDictionary.Keys => m_keys;
        public ICollection<TValue> Values => m_values;
        ICollection IDictionary.Values => m_values;
    }
}
