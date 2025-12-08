using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using UnityEngine;

namespace Unity.PlatformToolkit
{
    internal abstract class AbstractInputOwnershipSystem : IInputOwnershipSystem
    {
        private readonly Dictionary<Type, Func<object, IInputDevice>> m_Converters = new();
        private readonly Dictionary<Type, Func<object, IInputDevice>> m_ConverterCache = new();
        private readonly SemaphoreSlim m_ChangeEventPendingSemaphore = new SemaphoreSlim(0, 1);

        public event Action OnChange;
        public abstract IAccount GetOwner(IInputDevice inputDevice);

        protected AbstractInputOwnershipSystem()
        {
            PairingChangeEventInvokeLoop();
        }

        public IAccount GetOwner(object inputDevice)
        {
            var concreteType = inputDevice.GetType();

            if (m_ConverterCache.TryGetValue(concreteType, out var converter))
            {
                return GetOwner(converter.Invoke(inputDevice));
            }
            if (m_Converters.TryGetValue(concreteType, out converter))
            {
                m_ConverterCache.Add(concreteType, converter);
                return GetOwner(converter.Invoke(inputDevice));
            }

            if (!TryFindMostSpecificAssignableType(concreteType, m_Converters.Keys, out var converterKey))
                return null;

            converter = m_Converters[converterKey];
            m_ConverterCache.Add(concreteType, converter);
            return GetOwner(converter.Invoke(inputDevice));
        }

        public void RegisterInputDeviceConverter<T>(Func<T, IInputDevice> converter)
        {
            m_Converters[typeof(T)] = o =>
            {
                var inputDeviceType = (T)o;
                return converter(inputDeviceType);
            };

            m_ConverterCache.Clear();
        }

        protected void MarkPairingChanged()
        {
            m_ChangeEventPendingSemaphore.Release();
        }

        private async void PairingChangeEventInvokeLoop()
        {
            while (true)
            {
                await m_ChangeEventPendingSemaphore.WaitAsync().ConfigureAwait(false);
                await SafeInvoker.InvokeOnMainThread(OnChange);
            }
        }

        private static bool TryFindMostSpecificAssignableType(Type assignedType, IEnumerable<Type> assignableTypeCandidates, out Type mostSpecificAssignableType)
        {
            var assignableTypes = assignableTypeCandidates.Where(c => c.IsAssignableFrom(assignedType)).ToList();
            if (assignableTypes.Count == 0)
            {
                mostSpecificAssignableType = null;
                return false;
            }

            for (int i = 0; i < assignableTypes.Count; i++)
            {
                for (int j = i + 1; j < assignableTypes.Count; j++)
                {
                    if (assignableTypes[i].IsAssignableFrom(assignableTypes[j]))
                    {
                        assignableTypes.RemoveAt(i);
                        i--;
                        break;
                    }
                    if (assignableTypes[j].IsAssignableFrom(assignableTypes[i]))
                    {
                        assignableTypes.RemoveAt(j);
                        j--;
                    }
                }
            }

            mostSpecificAssignableType = assignableTypes[0];
            return true;
        }
    }
}
