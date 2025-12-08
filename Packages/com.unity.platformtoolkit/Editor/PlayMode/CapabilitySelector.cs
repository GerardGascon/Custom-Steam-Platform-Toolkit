using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Unity.PlatformToolkit.PlayMode
{
    /// <summary>
    /// Implementation of <see cref="ICapabilitySelector"/> that tracks <see cref="PlayModeCapabilityAssetDefinition"/> and exposes its assets
    /// as <see cref="IPlayModeCapability"/>. This object serializes <see cref="CurrentCapability"/>, but is not automatically saved anywhere, so
    /// it's meant to be used as part of an object that is serialized and saved. Currently saved as part of <see cref="PlayModeControlsEditorSettings"/>.
    /// </summary>
    internal class CapabilitySelector : ICapabilitySelector, IDisposable
    {
        private PlayModeControlsSettings m_Settings;

        public IPlayModeCapability CurrentCapability
        {
            get
            {
                if (m_Settings.m_CurrentCapability == null)
                {
                    CurrentCapability = Capabilities.FirstOrDefault();
                }
                return m_Settings.m_CurrentCapability;
            }
            set
            {
                if (value is not PlayModeCapabilityAssetDefinition capabilityAsset)
                {
                    throw new ArgumentException($"Expected value of type {nameof(PlayModeCapabilityAssetDefinition)}");
                }
                else if (capabilityAsset != m_Settings.m_CurrentCapability)
                {
                    m_Settings.m_CurrentCapability = capabilityAsset;
                    CurrentCapabilityChanged?.Invoke();
                }
            }
        }

        public CapabilitySelector(PlayModeControlsSettings settings)
        {
            m_Settings = settings;
            PlayModeControlsAssetTracker.PlayModeCapabilityAssetsCreatedOrDeleted += OnCapabilitiesCreatedOrDeleted;
        }

        public void Dispose()
        {
            PlayModeControlsAssetTracker.PlayModeCapabilityAssetsCreatedOrDeleted -= OnCapabilitiesCreatedOrDeleted;
        }

        public event Action CurrentCapabilityChanged;

        public IEnumerable<IPlayModeCapability> Capabilities => PlayModeControlsAssetTracker.GetPlayModeCapabilityAssets();

        public event Action CapabilitiesCreatedOrDeleted;
        private void OnCapabilitiesCreatedOrDeleted()
        {
            CapabilitiesCreatedOrDeleted?.Invoke();
        }
    }
}
