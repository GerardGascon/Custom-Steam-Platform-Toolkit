namespace Unity.PlatformToolkit
{
    internal class GenericInputDevice : IInputDevice
    {
        public GenericInputDevice(string idType, string id)
        {
            IdType = idType;
            Id = id;
        }

        public string IdType { get; }
        public string Id { get; }
    }
}
