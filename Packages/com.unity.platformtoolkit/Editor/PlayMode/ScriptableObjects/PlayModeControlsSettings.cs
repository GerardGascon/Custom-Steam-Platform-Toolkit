using UnityEditor;
using UnityEngine;
using UnityEngine.Serialization;

namespace Unity.PlatformToolkit.PlayMode
{
    /// <summary>
    /// This class stores serialized data used for the Play Mode environment.
    /// WHen loaded or reloaded (OnEnable) this class instantiates a runtime component for PlayMode which can use this data.
    /// </summary>
    [CreateAssetMenu(menuName = "Platform Toolkit/Play Mode Controls Settings", order = -1)]
    internal class PlayModeControlsSettings : ScriptableObject
    {
        [SerializeField, HideInInspector]
        internal PlayModeCapabilityAssetDefinition m_CurrentCapability;

        [SerializeField, HideInInspector]
        internal PlayModeEnvironmentData m_Environment;

        [SerializeField, HideInInspector]
        [FormerlySerializedAs("m_UserManager")]
        [FormerlySerializedAs("m_Users")]
        internal PlayModeUserData m_Accounts;

#if INPUT_SYSTEM_AVAILABLE
        [SerializeField, HideInInspector]
        internal PlayModeInputSystemData m_InputSystem;
#endif

        [SerializeField, HideInInspector]
        internal PlayModeSaveData m_LocalSaveData;

        public PlayModeSaveData LocalSaveData => m_LocalSaveData;

        [SerializeField, HideInInspector]
        private bool m_SetupPrior;


        private CapabilitySelector m_CapabilitySelector;
        public ICapabilitySelector CapabilitySelector => m_CapabilitySelector;

        private PlayModeControlsRuntime m_Runtime;
        public PlayModeControlsRuntime Runtime => m_Runtime;


        // Used by the custom inspector / editor for this class to identify the case when an inspector is created for
        // an object before OnEnable has been called.
        //
        // This is a workaround for a flow in Unity: see more detailed comment in
        // PlayModeSettingsEditor.CreateInspectorGUI().
        internal bool HasBeenEnabled = false;

        // Used to persist writes in order to make changes visible to the asset inspector.
        public ScriptableObjectDataChangePersistor Persistor { get; set; }
        public PlayModeAccessor PlayModeAccessor { get; private set; }

        /// <summary>
        /// Once created, a ViewMode is left assigned for the duration of this object's lifetime.
        ///  Use <see cref="PlayModeControlsViewModel.AreSettingsValid"/> and <see cref="PlayModeControlsViewModel.IsValid"/> to check binding readability.
        /// </summary>
        public PlayModeControlsViewModel ViewModel { get; private set; }

        private void Awake()
        {
            m_Environment ??= new PlayModeEnvironmentData();
            m_Accounts ??= new PlayModeUserData();
            #if INPUT_SYSTEM_AVAILABLE
            m_InputSystem ??= new PlayModeInputSystemData();
            #endif
            m_LocalSaveData ??= new PlayModeSaveData();
        }

        // Note that OnEnable is called without a matching OnDisable call on ScriptableObject if data get reloaded.
        private void OnEnable()
        {
            // Give a data persistor to each object that can programmatically modify data stored in a ScriptableObject
            // (or its children) at runtime / in playmode, so changes are persisted to disk and thus visible in Inspectors.
            Persistor ??= new ScriptableObjectDataChangePersistor(this);

            if (m_CapabilitySelector == null)
            {
                m_CapabilitySelector = new CapabilitySelector(this);
                m_CapabilitySelector.CurrentCapabilityChanged += OnCapabilityChanged;
            }

            PlayModeAccessor ??= new PlayModeAccessor();
            HasBeenEnabled = true;

            // If OnEnable has been called after a data reload, the runtime recreation will trigger invalidations on this
            ViewModel ??= new PlayModeControlsViewModel(this);

            // TODO: Ideally this isn't recreated in play-mode, but this is unavoidable whilst a ScriptableObject owns this.
            // This is because object instances will change over domain reloads.
            RecreateRuntime();
        }

        private void OnDisable()
        {
            DisposeRuntime();

            // Unbind the ViewModel to flag any UI accessing data
            ViewModel?.Dispose();
        }

        public PlayModeControlsRuntime RecreateRuntime()
        {
            DisposeRuntime();

            Debug.Assert(m_Runtime == null);

            var capability = m_CapabilitySelector.CurrentCapability;
            if (capability != null)
            {
                m_Runtime = new PlayModeControlsRuntime(this, capability, Persistor);

                if (!m_SetupPrior)
                {
                    m_Runtime.UserManager.CreateInitialAccountSet();
                    m_SetupPrior = true;
                    Persistor.PersistWrites();
                }

                m_LocalSaveData.Initialize(Persistor);
            }

            ViewModel.BindRuntime(m_Runtime, this);

            return m_Runtime;
        }

        private void DisposeRuntime()
        {
            m_Runtime?.Dispose();
            m_Runtime = null;
        }

        public void Dispose()
        {
             DisposeRuntime();

            /// Unbind the ViewModel to prevent any UI accessing data
            ViewModel.Dispose();

            if (m_CapabilitySelector != null)
                m_CapabilitySelector.CurrentCapabilityChanged -= OnCapabilityChanged;
            m_CapabilitySelector = null;

            PlayModeAccessor?.Dispose();
            PlayModeAccessor = null;

            Persistor?.Dispose();
            Persistor = null;
        }

        private void OnDestroy()
        {
            Dispose();
        }

        private void OnCapabilityChanged()
        {
            RecreateRuntime();
            Save();
        }

        private void Save()
        {
            EditorUtility.SetDirty(this);
        }
    }
}
