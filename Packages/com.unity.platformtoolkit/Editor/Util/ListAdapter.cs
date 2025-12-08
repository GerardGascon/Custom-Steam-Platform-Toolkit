using System;
using System.Collections;
using System.Collections.Generic;

namespace Unity.PlatformToolkit.Editor
{
    /// <summary>Adapts IList&lt;T> to an IList.</summary>
    /// <remarks>
    /// <para>UI Toolkit ListView and similar classes operate on an IList. Made this class to be able to wrap an IList&lt;T> and get back an IList.</para>
    /// <para>One interesting quirk is that UI Toolkit will add a null to an IList when adding a new element, in that case <see cref="m_CreateNewInstance"/> method is used to initialize a new element.</para>
    /// </remarks>
    internal class ListAdapter<T> : IList
    {
        private IList<T> m_List;

        public ListAdapter(IList<T> list, Func<T> createNewInstance)
        {
            m_List = list;
            m_CreateNewInstance = createNewInstance;
        }

        private readonly Func<T> m_CreateNewInstance;

        public IEnumerator GetEnumerator()
        {
            return m_List.GetEnumerator();
        }

        public void CopyTo(Array array, int index)
        {
            throw new InvalidOperationException("CopyTo not supported. Sorry for disrespecting well established programming principles.");
        }

        public int Count => m_List.Count;
        public bool IsFixedSize => false;
        public bool IsReadOnly => false;
        public bool IsSynchronized => false;
        public object SyncRoot => m_List;

        public int Add(object value)
        {
            switch (value)
            {
                case T tValue:
                    m_List.Add(tValue);
                    break;
                case null:
                    m_List.Add(m_CreateNewInstance());
                    break;
                default:
                    throw new ArgumentException($"{nameof(value)} must be null or {typeof(T).Name})");
            }
            return m_List.Count - 1;
        }

        public void Clear()
        {
            m_List.Clear();
        }

        public bool Contains(object value)
        {
            if (value is T tValue)
                return m_List.Contains(tValue);
            return false;
        }

        public int IndexOf(object value)
        {
            if (value is T tValue)
                return m_List.IndexOf(tValue);
            return -1;
        }

        public void Insert(int index, object value)
        {
            switch (value)
            {
                case T tValue:
                    m_List.Insert(index, tValue);
                    break;
                case null:
                    m_List.Insert(index, m_CreateNewInstance());
                    break;
                default:
                    throw new ArgumentException($"{nameof(value)} must be null or {typeof(T).Name})");
            }
        }

        public void Remove(object value)
        {
            if (value is not T achievement)
                return;

            m_List.Remove(achievement);
        }

        public void RemoveAt(int index)
        {
            m_List.RemoveAt(index);
        }

        public object this[int index]
        {
            get => m_List[index];
            set
            {
                if (value is not T tValue)
                {
                    throw new ArgumentException($"{nameof(value)} must be null or {typeof(T).Name})");
                }
                m_List[index] = tValue;
            }
        }
    }
}
