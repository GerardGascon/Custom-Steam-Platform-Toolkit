using System;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using Unity.Properties;
using UnityEngine;
using UnityEngine.UIElements;

namespace Unity.PlatformToolkit.Editor
{
    [Serializable]
    internal class ImplementationData : INotifyBindablePropertyChanged
    {
        [SerializeField, DontCreateProperty]
        private string configurationData;

        [SerializeField, DontCreateProperty]
        private bool ignore;

        [CreateProperty]
        public string ConfigurationData
        {
            get => configurationData;
            set => SetProperty(ref configurationData, value);
        }

        [CreateProperty]
        public bool Ignore { get => ignore; set => SetProperty(ref ignore, value); }

        public ImplementationData()
        {
            ConfigurationData = "";
            Ignore = false;
        }

        public event EventHandler<BindablePropertyChangedEventArgs> propertyChanged;

        protected void SetProperty<T>(ref T field, T value, [CallerMemberName] string property = "")
        {
            if (value == null && field == null || value != null && value.Equals(field))
                return;

            field = value;
            propertyChanged?.Invoke(this, new BindablePropertyChangedEventArgs(property));
        }
    }
}
