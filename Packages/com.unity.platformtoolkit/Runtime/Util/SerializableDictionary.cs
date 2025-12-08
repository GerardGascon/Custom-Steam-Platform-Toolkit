using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace Unity.PlatformToolkit
{
    /// <summary>To use this you can't just add a SerializedField as Unity doesn't serialize generics well. The workaround is to make a class inherit from this.</summary>
    [Serializable]
    internal class SerializableDictionary<TKey, TValue> : Dictionary<TKey, TValue>, ISerializationCallbackReceiver
    {
        [SerializeField]
        [FormerlySerializedAs("m_KeyList")]
        [FormerlySerializedAs("Keys")]
        private List<TKey> keys = new List<TKey>();

        [SerializeField]
        [FormerlySerializedAs("m_ValueList")]
        [FormerlySerializedAs("Values")]
        private List<TValue> values = new List<TValue>();

        public void OnAfterDeserialize()
        {
            if (keys.Count != values.Count)
            {
                return;
            }

            Clear();

            for (int i = 0; i < keys.Count; ++i)
            {
                Add(keys[i], values[i]);
            }

            keys.Clear();
            values.Clear();
        }

        public void OnBeforeSerialize()
        {
            keys.Clear();
            values.Clear();

            foreach (KeyValuePair<TKey, TValue> kvp in this)
            {
                keys.Add(kvp.Key);
                values.Add(kvp.Value);
            }
        }
    }
}
