namespace Unity.PlatformToolkit.Editor
{
    internal class DataColumn
    {
        public string Header { get; private set; }
        public string[] Data{ get; private set; }

        public DataColumn(string header, string[] data)
        {
            Header = header;
            Data = data;
        }
    }
}
