namespace Unity.PlatformToolkit
{
    /// <summary>
    /// Interface for referring to platform devices used as part of input ownership.
    /// </summary>
    public interface IInputDevice
    {
        /// <summary>
        /// Platform can support different types of Id for input devices.
        /// </summary>
        public string IdType { get; }

        /// <summary>
        /// Platform specific and input system agnostic id.
        /// </summary>
        public string Id { get; }
    }
}
