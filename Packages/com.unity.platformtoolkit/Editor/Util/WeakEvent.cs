using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine.Assertions;

namespace Unity.PlatformToolkit.Editor
{
    internal delegate void WeakEventHandler();
    internal delegate void WeakEventHandler<TEventArgs>(TEventArgs args);

    /// <summary>
    /// An implementation of <see cref="WeakEventBase"/> with no arguments.
    /// </summary>
    /// <typeparam name="TDelegateType">The delegate type for the event.</typeparam>
    internal class WeakEvent : WeakEventBase<WeakEventHandler>
    {
        /// <summary>
        /// Takes a copy of listeners and invokes them. This allows listeners to be added and removed during invocation.
        /// </summary>
        public void Invoke()
        {
            try
            {
                var dispatchQueue = AcquireDispatchQueue();
                foreach (var handler in dispatchQueue)
                {
                    handler.Invoke();

                }
            }
            finally
            {
                ReleaseDispatchQueue();
            }
        }
    }

    /// <summary>
    /// An implementation of <see cref="WeakEventBase"/> with a single argument.
    /// </summary>
    /// <typeparam name="TDelegateType">The delegate type for the event.</typeparam>
    internal class WeakEvent<TEventArgs> : WeakEventBase<WeakEventHandler<TEventArgs>>
    {
        /// <summary>
        /// Takes a copy of listeners and invokes them. This allows listeners to be added and removed during invocation.
        /// </summary>
        public void Invoke(TEventArgs arg)
        {
            try
            {
                var dispatchQueue = AcquireDispatchQueue();
                foreach (var handler in dispatchQueue)
                {
                    handler.Invoke(arg);
                }
            }
            finally
            {
                ReleaseDispatchQueue();
            }
        }
    }

    /// <summary>
    /// A base class for weak events that allows a listener with a poorly defined lifetime or no teardown flow to register itself without concern for leaks.
    /// </summary>
    /// <typeparam name="TDelegateType">The delegate type for the event.</typeparam>
    internal abstract class WeakEventBase<TDelegateType> where TDelegateType : Delegate
    {
        /// <summary>
        /// This is a type that can be used to attach transient event handlers and actions to a GC-able object.
        /// ConditionalWeakTable does not hold strong refs to keys, and doesn't need key entries removing.
        /// </summary>
        private readonly ConditionalWeakTable<object, List<object>> m_HandlersByGcObject = new();
        private readonly List<TDelegateType> m_DispatchQueue = new();
        private bool m_DispatchQueueInUse = false;

        protected readonly List<WeakReference<TDelegateType>> m_Listeners = new();

        /// <summary>
        /// Add a listener with a weak reference to it. This can be removed with an explicit removal call or will clean itself up at a later point after the owning object is garbage collected.
        /// This is safe to call redundantly for a handler and will not register duplicates.
        /// </summary>
        /// <param name="gcOwner">The gc-able owner of the handler. The handler may not have references to it if's a delegate from a passed-in method.</param>
        /// <param name="handler">The event handler to invoke.</param>
        public void AddWeakListener(TDelegateType handler)
        {
            Assert.IsNotNull(handler);

            lock (m_Listeners)
            {
                for (int i = m_Listeners.Count - 1; i >= 0; --i)
                {
                    bool gotValue = m_Listeners[i].TryGetTarget(out TDelegateType entryValue);
                    if (!gotValue)
                    {
                        RemoveAt(i, null);
                        continue;
                    }
                    else if (entryValue == handler)
                    {
                        // Entry already exists
                        return;
                    }
                }

                // Bind the handler's lifetime to that of the target object using a ConditionalWeakTable.
                var target = handler.Target;
                if (target != null)
                {
                    m_HandlersByGcObject.GetOrCreateValue(target).Add(handler);
                }

                m_Listeners.Add(new WeakReference<TDelegateType>(handler));
            }
        }

        public void RemoveListener(TDelegateType handler)
        {
            lock (m_Listeners)
            {
                for (int i = m_Listeners.Count - 1; i >= 0; --i)
                {
                    if (m_Listeners[i].TryGetTarget(out TDelegateType entryValue))
                    {
                        if (entryValue == handler)
                        {
                            RemoveAt(i, entryValue);
                        }
                    }
                    else
                    {
                        RemoveAt(i, null);
                    }
                }
            }
        }

        protected IReadOnlyList<TDelegateType> AcquireDispatchQueue()
        {
            lock (m_Listeners)
            {
                Assert.IsFalse(m_DispatchQueueInUse, "Reentrant event dispatch detected. This is not supported.");
                m_DispatchQueueInUse = true;

                m_DispatchQueue.Clear();
                foreach (var listener in m_Listeners)
                {
                    if (listener.TryGetTarget(out TDelegateType handler))
                    {
                        m_DispatchQueue.Add(handler);
                    }
                }
            }

            return m_DispatchQueue;
        }

        protected void ReleaseDispatchQueue()
        {
            m_DispatchQueue.Clear();
            m_DispatchQueueInUse = false;
        }

        private void RemoveAt(int index, TDelegateType resolvedEntryToRemove)
        {
            var entry = m_Listeners[index];

            // Remove the handler from the list for this owner. No need to remove the owner entry itself as ConditionalWeakTable manages this.
            if (resolvedEntryToRemove != null &&
                resolvedEntryToRemove.Target != null &&
                m_HandlersByGcObject.TryGetValue(resolvedEntryToRemove.Target, out var handlerList))
            {
                handlerList.Remove(resolvedEntryToRemove);
            }

            m_Listeners.RemoveAt(index);
        }
    }
}
