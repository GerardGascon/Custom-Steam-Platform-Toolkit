using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Serialization;

namespace Unity.PlatformToolkit
{
    [Serializable]
    internal class AttributeStore
    {
        [SerializeField]
        private List<string> attributeIds;

        [SerializeField]
        private List<string> names;

        public AttributeStore(List<string> attributeIds, List<string> attributeNames)
        {
            Assert.AreEqual(attributeIds.Count, attributeNames.Count);

            this.attributeIds = attributeIds;
            this.names = attributeNames;
        }

        public IReadOnlyDictionary<string, IReadOnlyList<string>> Attributes
        {
            get
            {
                var result = new Dictionary<string, IReadOnlyList<string>>();
                for (int i = 0; i < attributeIds.Count; i++)
                {
                    if (!result.ContainsKey(attributeIds[i]))
                        result.Add(attributeIds[i], new List<string>());
                    ((List<string>)result[attributeIds[i]]).Add(names[i]);
                }
                return result;
            }
        }
    }
}
