using System;
using UnityEditor;
using UnityEngine;

namespace Unity.PlatformToolkit.PlayMode
{
    /// <summary>
    /// Persist writes made to components of ScriptableObjects (specifically fields of those objects which
    /// are Serializable classes). Writes become visible to e.g. Editor asset inspectors only after they're persisted.
    /// </summary>
    internal class ScriptableObjectDataChangePersistor : IDisposable
    {
        private bool m_InvokeDataChanged;
        private bool m_SetDirty;
        private bool m_HasUpdateQueued;

        // Scriptable objects can get enabled with no matching destroys in some cases, so a weak reference is used help remind of this as well as avoid keeping a ref when it has a weak lifetime definition.
        private WeakReference<ScriptableObject> m_Object;

        internal ScriptableObjectDataChangePersistor(ScriptableObject objectToPersist)
        {
            m_Object = new WeakReference<ScriptableObject>(objectToPersist);
        }

        private void Update()
        {
            if (m_InvokeDataChanged)
            {
                OnDataChanged?.Invoke();
            }
            if (m_SetDirty)
            {
                if (m_Object.TryGetTarget(out var targetObject) && targetObject != null)
                {
                    EditorUtility.SetDirty(targetObject);
                }
            }

            ResetUpdateState();
        }

        // Used to update any already-open UIs when data changes.
        public Action OnDataChanged;

        /// <summary>
        /// Mark the ancestor ScriptableObject as dirty, so changes will be saved to disk when the project is saved.
        /// </summary>
        internal void PersistWritesWithoutNotify()
        {
            m_SetDirty = true;
            MarkForUpdate();
        }

        /// <summary>
        /// Mark the ancestor ScriptableObject as dirty, then notify observers of data changes (so e.g. UIs can refresh).
        /// </summary>
        internal void PersistWrites()
        {
            PersistWritesWithoutNotify();
            m_InvokeDataChanged = true;
        }

        private void MarkForUpdate()
        {
            if (!m_HasUpdateQueued)
            {
                m_HasUpdateQueued = true;

                // Rate limit calls to no more than one per frame, so we don't churn UI refreshes.
                EditorApplication.update += Update;
            }
        }

        private void ResetUpdateState()
        {
            m_SetDirty = false;
            m_InvokeDataChanged = false;
            m_HasUpdateQueued = false;
            EditorApplication.update -= Update;
        }

        public void Dispose()
        {
            ResetUpdateState();
        }
    }
}
