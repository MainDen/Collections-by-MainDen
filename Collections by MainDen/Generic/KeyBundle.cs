using System;
using System.Collections;
using System.Collections.Generic;

namespace MainDen.Collections.Generic
{
    public class KeyBundle : IBundle<int>, ICollection<int>, ICollection, IReadOnlyCollection<int>
    {
        private int m_count;

        private readonly int m_minValue;

        private readonly int m_maxValue;

        private readonly List<KeyValuePair<int, int>> m_segments = new List<KeyValuePair<int, int>>();

        private readonly object m_syncRoot = new object();
        
        public KeyBundle()
        {
            m_minValue = 1;
            m_maxValue = Int32.MaxValue;
        }

        public KeyBundle(int minValue, int maxValue)
        {
            MustValidate(minValue, maxValue);
            m_minValue = minValue;
            m_maxValue = maxValue;
        }

        public KeyBundle(IEnumerable<int> keys)
        {
            m_minValue = 1;
            m_maxValue = Int32.MaxValue;
            foreach (var key in keys)
            {
                Add(key);
            }
        }

        public KeyBundle(int minValue, int maxValue, IEnumerable<int> keys)
        {
            MustValidate(minValue, maxValue);
            m_minValue = minValue;
            m_maxValue = maxValue;
            foreach (var key in keys)
            {
                Add(key);
            }
        }

        public int Count => m_count;

        public bool IsReadOnly => false;

        public bool IsSynchronized => false;

        public int MaxValue => m_maxValue;

        public int MinValue => m_minValue;

        public object SyncRoot => m_syncRoot;

        public void Add(int item)
        {
            if (!Put(item))
                throw new ArgumentException($"Value '{item}' already exists in the current bundle.");
        }

        public void Clear()
        {
            m_segments.Clear();
            m_count = 0;
        }

        public void CopyTo(int[] array, int arrayIndex)
        {
            if (array is null)
                throw new ArgumentNullException(nameof(array), "Value is null");
            if (arrayIndex < 0)
                throw new ArgumentOutOfRangeException(nameof(arrayIndex), "Value is less than zero.");
            if (array.Length - arrayIndex < m_count)
                throw new ArgumentException("The number of elements in the source is greater than the available space from index to the end of the destination array.");

            foreach (var item in this)
            {
                array[arrayIndex] = item;
                ++arrayIndex;
            }
        }

        private void CopyTo(object[] array, int arrayIndex)
        {
            if (array is null)
                throw new ArgumentNullException(nameof(array), "Value is null");
            if (arrayIndex < 0)
                throw new ArgumentOutOfRangeException(nameof(arrayIndex), "Value is less than zero.");
            if (array.Length - arrayIndex < m_count)
                throw new ArgumentException("The number of elements in the source is greater than the available space from index to the end of the destination array.");

            foreach (var item in this)
            {
                array[arrayIndex] = item;
                ++arrayIndex;
            }
        }

        public bool Put(int item)
        {
            if (!Validate(item))
                throw new ArgumentOutOfRangeException(nameof(item), $"Value must be from '{MinValue}' to '{MaxValue}'.");
            return InsertKey(GetSegmentIndex(item), item);
        }

        public bool Remove(int item)
        {
            if (!Validate(item))
                throw new ArgumentOutOfRangeException(nameof(item), $"Value must be from '{MinValue}' to '{MaxValue}'.");
            if (m_segments.Count == 0)
                return false;
            return RemoveKey(GetSegmentIndex(item), item);
        }

        public bool Take(out int item)
        {
            item = default;
            if (m_segments.Count == 0)
                return false;
            item = m_segments[0].Key;
            return RemoveKey(0, item);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public IEnumerator<int> GetEnumerator()
        {
            foreach (var segment in m_segments)
            {
                int item;
                for (item = segment.Key; item < segment.Value; ++item)
                {
                    yield return item;
                }
                yield return item;
            }
        }

        public void CopyTo(Array array, int index)
        {
            if (array is int[] intArray)
            {
                CopyTo(intArray, index);
                return;
            }
            if (array is object[] objectArray)
            {
                CopyTo(objectArray, index);
                return;
            }
            throw new ArgumentException("The type of the source cannot be cast automatically to the type of the destination array.");
        }

        public bool Contains(int item)
        {
            return Contains(GetSegmentIndex(item), item);
        }

        private bool Validate(int item)
        {
            return item >= MinValue && item <= MaxValue;
        }

        private bool InsertKey(int segmentIndex, int item)
        {
            if (Contains(segmentIndex, item))
            {
                return false;
            }
            m_segments.Insert(segmentIndex + 1, new KeyValuePair<int, int>(item, item));
            JoinSegments(segmentIndex + 1, segmentIndex + 2);
            JoinSegments(segmentIndex, segmentIndex + 1);
            ++m_count;
            return true;
        }

        private bool RemoveKey(int segmentIndex, int item)
        {
            if (!Contains(segmentIndex, item))
            {
                return false;
            }
            SplitSegments(segmentIndex, item);
            --m_count;
            return true;
        }

        private void JoinSegments(int segmentIndex1, int segmentIndex2)
        {
            if (segmentIndex1 < 0)
            {
                return;
            }
            if (segmentIndex2 >= m_segments.Count)
            {
                return;
            }
            var segment1 = m_segments[segmentIndex1];
            var segment2 = m_segments[segmentIndex2];
            if (segment1.Value + 1 >= segment2.Key)
            {
                m_segments[segmentIndex1] = new KeyValuePair<int, int>(segment1.Key, segment2.Value);
                m_segments.RemoveAt(segmentIndex2);
            }
        }

        private void SplitSegments(int segmentIndex, int item)
        {
            var segment = m_segments[segmentIndex];
            if (segment.Key == segment.Value)
            {
                m_segments.RemoveAt(segmentIndex);
                return;
            }
            if (segment.Key == item)
            {
                m_segments[segmentIndex] = new KeyValuePair<int, int>(segment.Key + 1, segment.Value);
                return;
            }
            if (segment.Value == item)
            {
                m_segments[segmentIndex] = new KeyValuePair<int, int>(segment.Key, segment.Value - 1);
                return;
            }
            m_segments[segmentIndex] = new KeyValuePair<int, int>(item + 1, segment.Value);
            m_segments.Insert(segmentIndex, new KeyValuePair<int, int>(segment.Key, item - 1));
        }

        private bool Contains(int segmentIndex, int item)
        {
            return segmentIndex >= 0 && m_segments[segmentIndex].Value >= item;
        }

        private int GetSegmentIndex(int item)
        {
            var index = Array.BinarySearch(m_segments.ToArray(), new KeyValuePair<int, int>(item, item), s_comparer);
            if (index < 0)
            {
                index = ~index - 1;
            }
            return index;
        }

        private static readonly Comparer<KeyValuePair<int, int>> s_comparer = Comparer<KeyValuePair<int, int>>.Create((x, y) => x.Key == y.Key ? 0 : x.Key > y.Key ? 1 : -1);

        private static void MustValidate(int minValue, int maxValue)
        {
            if (minValue > maxValue)
                throw new ArgumentException("minValue greater than maxValue.");
            if (maxValue >= Int32.MinValue + Int32.MaxValue && minValue <= maxValue - Int32.MaxValue)
                throw new ArgumentException($"maxValue - minValue greater than {Int32.MaxValue}.");
        }

        public static KeyBundle CreateFull(int minValue, int maxValue)
        {
            var keyBundle = new KeyBundle(minValue, maxValue);
            keyBundle.m_segments.Add(new KeyValuePair<int, int>(minValue, maxValue));
            keyBundle.m_count = maxValue - minValue + 1;
            return keyBundle;
        }
    }
}
