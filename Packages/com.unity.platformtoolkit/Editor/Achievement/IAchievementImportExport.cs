namespace Unity.PlatformToolkit.Editor
{
    /// <summary>
    /// Interface to handle import and export of achievement data for a specific platform implementation.
    /// </summary>
    internal interface IAchievementImportExport
    {
        /// <summary>
        /// They export key for the CSV file (Header)
        /// </summary>
        public string ExportKey { get; }

        /// <summary>
        /// Import method to import a specific achievement
        /// </summary>
        /// <param name="exportedConfigurationData">data to import</param>
        /// <param name="achievement">Class to import the data to</param>
        public void Import(string exportedConfigurationData, IAchievement achievement);

        /// <summary>
        /// Export method to export specific achievement
        /// </summary>
        /// <param name="achievement">Achievement to be exported</param>
        /// <returns>The achievement</returns>
        public ImplementationData Export(IAchievement achievement);
    }
}
