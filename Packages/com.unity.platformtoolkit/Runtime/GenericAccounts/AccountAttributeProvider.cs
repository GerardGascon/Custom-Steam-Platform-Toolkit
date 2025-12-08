using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

namespace Unity.PlatformToolkit
{
    /// <summary>Helper for supporting <see cref="IAccount.HasAttribute{T}"/> and <see cref="IAccount.GetAttribute{T}"/></summary>
    /// <remarks>
    /// <para>To set up call <see cref="RegisterAttributeGetter{TAttribute}"/> for every supported Attribute Id, then call <see cref="FinalizeRegistration"/>.</para>
    /// </remarks>
    internal class AccountAttributeProvider<TAccount>
        where TAccount : IAccount
    {
        private IReadOnlyDictionary<string, IReadOnlyList<string>> m_AttributeIdToNames;
        private Dictionary<string, object> m_AttributeIdToGetter = new();

        private readonly Dictionary<string, object> m_AttributeNameToGetter = new();

        public AccountAttributeProvider(IReadOnlyDictionary<string, IReadOnlyList<string>> attributeIdToNames)
        {
            m_AttributeIdToNames = attributeIdToNames;
        }

        public bool HasAttribute<T>(string attributeName)
        {
            return m_AttributeNameToGetter.TryGetValue(attributeName, out var attributeGetter) && attributeGetter is Func<TAccount, Task<T>>;
        }

        public Task<T> GetAttribute<T>(TAccount account, string attributeName)
        {
            if (!m_AttributeNameToGetter.TryGetValue(attributeName, out var attributeGetter))
                throw new InvalidOperationException($"Attribute with name {attributeName} does not exist.");

            if (attributeGetter is Func<TAccount, Task<T>> getter)
                return getter(account);

            throw new InvalidOperationException($"Attribute {attributeName} type mismatch. Attribute with name {attributeName} ");
        }

        public void RegisterAttributeGetter<TAttribute>(string attributeId,  Func<TAccount, Task<TAttribute>> getter)
        {
            if (!m_AttributeIdToGetter.TryAdd(attributeId, getter))
                throw new ArgumentException($"Attribute {attributeId} is already registered, multiple registrations are not allowed.");
        }

        public void FinalizeRegistration()
        {
            foreach (var attributeId in m_AttributeIdToNames.Keys)
            {
                if (!m_AttributeIdToGetter.TryGetValue(attributeId, out var getter))
                {
                    throw new InvalidOperationException($"Attribute {attributeId} has no registered getter.");
                }

                if (m_AttributeIdToNames.TryGetValue(attributeId, out var attributeNames) && attributeNames is { Count: > 0 })
                {
                    foreach (var attributeName in attributeNames)
                    {
                        m_AttributeNameToGetter.Add(attributeName, getter);
                    }
                }
            }

            // These dictionaries are only needed during set up and can be freed
            m_AttributeIdToNames = null;
            m_AttributeIdToGetter = null;
        }
    }
}
