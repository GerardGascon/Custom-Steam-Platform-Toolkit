using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Unity.Properties;
using UnityEditor;
using UnityEngine;

namespace Unity.PlatformToolkit.Editor
{
    internal class AchievementEditor
    {
        public static readonly string AchievementHeaderId = "ID";
        public static readonly string AchievementHeaderProgressTarget = "Progress_Target";
        public static readonly string AchievementHeaderIdNotFound = "ID not found in CSV file";
        public static readonly string AchievementHeaderProgressTargetNotFound = "Progress_Target not found in CSV file";
        public static readonly string AchievementDataError = "Illegal character(s) found in CSV data.";
        public static readonly int AchievementCharacterLimit = 64;

        // all A-Z characters, numbers and _ -.
        public static readonly string CommonIdRegexPattern = @"^[0-9A-Za-z_-]+$";
        // No white spaces filter
        public static readonly string AchievementImplementationDataRegexPattern = @"\s";

        [CreateProperty]
        public bool ExportEnabled => !HasEmptyAchievement() && !HasDuplicateAchievement();
        public static readonly string AchievementIgnoreTag = "_IGNORE_";
        public ObservableSerializableList<StoredAchievement> Achievements { get; }

        public AchievementCommonConfiguration CommonConfiguration { get; }

        private readonly List<(IAchievementConfiguration, IAchievementConfigurationContext)>
            m_ConfigurationsAndContext = new();

        public IReadOnlyList<(IAchievementConfiguration configuration, IAchievementConfigurationContext context)>
            ConfigurationsAndContext => m_ConfigurationsAndContext;

        private readonly IReadOnlyCollection<IPlatformToolkitSupportDeclaration>
            m_PlatformSupportDeclarationsWithAchievements;

        public AchievementEditor(ObservableSerializableList<StoredAchievement> achievements,
            IReadOnlyCollection<IPlatformToolkitSupportDeclaration> platformSupportDeclarations)
        {
            Achievements = achievements;
            m_PlatformSupportDeclarationsWithAchievements = platformSupportDeclarations
                .Where(platform => platform.AchievementsSupported).ToList();
            CommonConfiguration = new AchievementCommonConfiguration(Achievements);

            foreach (var supportDeclaration in platformSupportDeclarations)
            {
                if (!supportDeclaration.AchievementsSupported)
                    continue;

                var achievementContext = new AchievementConfigurationContext(supportDeclaration.Key, Achievements);
                m_ConfigurationsAndContext.Add((supportDeclaration.CreateAchievementConfiguration(achievementContext),
                    achievementContext));
            }
        }

        private bool HasEmptyAchievement()
        {
            foreach (var achievement in Achievements)
            {
                if (string.IsNullOrEmpty(achievement.Id))
                {
                    return true;
                }
            }

            return false;
        }

        private bool HasDuplicateAchievement()
        {
            var achievementIds = new HashSet<string>();
            foreach (var achievement in Achievements)
            {
                if (!achievementIds.Add(achievement.Id))
                {
                    return true;
                }
            }

            return false;
        }

        public void ImportCsv(string csv)
        {
            try
            {
                var dataColumns = CsvParser.Parse(csv);
                Import(dataColumns);
            }
            catch (Exception e)
            {
                Debug.LogWarning($"File format error. The selected file is not a valid CSV file: {e.Message}");
            }
        }

        public List<DataColumn> ExportToDataColumns()
        {
            ValidateAchievementData(Achievements);

            var dataColumns = ConvertAchievementsToDataColumns(Achievements);
            foreach ((IAchievementConfiguration configuration, IAchievementConfigurationContext context) configurationAndContext in m_ConfigurationsAndContext)
            {
                var dataColumn = new DataColumn(configurationAndContext.configuration.ImportExportProvider.ExportKey,
                    new string[Achievements.Count]);
                dataColumns.Add(dataColumn);
                for (int i = 0; i < Achievements.Count ; i++)
                {
                    var cellData = configurationAndContext.configuration.ImportExportProvider.Export(configurationAndContext.context
                        .Achievements[i]);

                    dataColumn.Data[i] = cellData.Ignore == false ? cellData.ConfigurationData : AchievementIgnoreTag;
                }
            }

            return dataColumns;
        }

        public string Export()
        {
            try
            {
                var dataColumns = ExportToDataColumns();
                return CsvExporter.Export(dataColumns);
            }
            catch (Exception e)
            {
                Debug.LogException(e);
                throw;
            }
        }

        private List<DataColumn> ConvertAchievementsToDataColumns(ObservableSerializableList<StoredAchievement> achievements)
        {
            var result = new List<DataColumn>();
            var idData = new string[achievements.Count];
            var progressTargetData = new string[achievements.Count];
            for (int i = 0; i < achievements.Count; i++)
            {
                idData[i] = achievements[i].Id;
                progressTargetData[i] = achievements[i].ProgressTarget.ToString();
            }

            result.Add(new DataColumn(AchievementHeaderId, idData));
            result.Add(new DataColumn(AchievementHeaderProgressTarget, progressTargetData));
            return result;
        }

        private void ValidateAchievementData(IReadOnlyList<StoredAchievement> achievements)
        {
            List<Exception> exceptions = new List<Exception>();
            for (int i = 0; i < achievements.Count; i++)
            {
                if (string.IsNullOrEmpty(achievements[i].Id))
                {
                    exceptions.Add(new ArgumentException(CreateEmptyAchievementFoundExceptionText(i)));
                }
            }

            if (exceptions.Count > 0)
            {
                throw new AggregateException(exceptions);
            }
        }

        public void Import(IReadOnlyList<DataColumn> dataColumns)
        {
            ObservableSerializableList<StoredAchievement> oldAchievements =
                new ObservableSerializableList<StoredAchievement>();
            foreach (var achievement in Achievements)
            {
                oldAchievements.Add(achievement.Clone());
            }

            var toOverrideData = GetStoredAchievementDataFromColumns(dataColumns);

            foreach (var supportDeclaration in m_PlatformSupportDeclarationsWithAchievements)
            {
                var achievementContext = new AchievementConfigurationContext(supportDeclaration.Key, toOverrideData);
                var configuration = supportDeclaration.CreateAchievementConfiguration(achievementContext);
                if (configuration.ImportExportProvider == null)
                {
                    Debug.LogError("ImportExportProvider is null");
                    continue;
                }

                var column = dataColumns.FirstOrDefault(column =>
                    column.Header == configuration.ImportExportProvider.ExportKey);
                if (column == null)
                {
                    Debug.LogWarning(
                        $"Did not find {configuration.ImportExportProvider.ExportKey} in CSV file, whilst this platform is installed.");

                    // Here we copy over the old data to the new data if present for the specific platform
                    // If imported data does not contain a column for a known platform, the data for that platform will not be changed.
                    var allData = GetLocalData(oldAchievements, configuration);
                    column = new DataColumn(configuration.ImportExportProvider.ExportKey, allData.ToArray());
                    if (column.Data == null || column.Data.Length == 0 || column.Data.Length != Achievements.Count)
                    {
                        continue;
                    }
                }

                for (var index = 0; index < toOverrideData.Count; index++)
                {
                    configuration.ImportExportProvider.Import(column.Data[index],
                        achievementContext.Achievements[index]);
                }
            }

            Achievements.Clear();
            for (int i = 0; i < toOverrideData.Count; i++)
            {
                Achievements.Add(toOverrideData[i]);
            }
        }

        private static List<string> GetLocalData(ObservableSerializableList<StoredAchievement> oldAchievements,
            IAchievementConfiguration configuration)
        {
            var platformAchievement = oldAchievements.Where(old =>
                !string.IsNullOrEmpty(old.GetImplementationData(configuration.ImportExportProvider.ExportKey).ConfigurationData));

            var allData = new List<string>();
            foreach (var oldData in platformAchievement)
            {
                var implementationData = oldData.GetImplementationData(configuration.ImportExportProvider.ExportKey);
                allData.Add(implementationData.ConfigurationData);
            }

            return allData;
        }

        private ObservableSerializableList<StoredAchievement> GetStoredAchievementDataFromColumns(
            IReadOnlyList<DataColumn> dataColumns)
        {
            var errors = ValidateHeader(dataColumns);
            if (errors.Count > 0)
            {
                // We need to throw these errors here because we expect ID or Progress_Target to exist.
                // If this exception is thrown, code should stop executing before we overwrite corrupted data.
                throw new AggregateException(errors);
            }

            var idData = dataColumns.First(c => c.Header == AchievementHeaderId).Data;
            var progressTargetData = dataColumns.First(c => c.Header == AchievementHeaderProgressTarget).Data;
            errors.AddRange(ValidateHeaderIdData(idData));
            errors.AddRange(ValidateHeaderProgressData(progressTargetData));

            if (errors.Count > 0)
            {
                throw new AggregateException(errors);
            }

            var storedAchievements = new ObservableSerializableList<StoredAchievement>();
            for (int i = 0; i < idData.Length; i++)
            {
                var storedAchievement = new StoredAchievement()
                {
                    Id = idData[i],
                    ProgressTarget = int.Parse(progressTargetData[i]),
                    // We add 0 as an edge case, because else it will set it to Progressive
                    UnlockType = int.Parse(progressTargetData[i]) == 0 || int.Parse(progressTargetData[i]) == 1 ? UnlockType.Single : UnlockType.Progressive
                };
                storedAchievements.Add(storedAchievement);
            }

            return storedAchievements;
        }

        private List<FormatException> ValidateHeaderProgressData(string[] progressTargetData)
        {
            List<FormatException> result = new List<FormatException>();
            for (var index = 0; index < progressTargetData.Length; index++)
            {
                if (!int.TryParse(progressTargetData[index], out var dataResult))
                {
                    result.Add(new FormatException(GenerateProgressTargetNotValidExceptionText(index)));
                }
            }

            return result;
        }

        private List<FormatException> ValidateHeaderIdData(string[] idData)
        {
            HashSet<string> foundData = new HashSet<string>();
            List<FormatException> result = new List<FormatException>();
            for (var index = 0; index < idData.Length; index++)
            {
                if (string.IsNullOrEmpty(idData[index]))
                {
                    result.Add(new FormatException(GenerateEmptyIdFoundExceptionText(index)));
                    continue;
                }

                if (!foundData.Add(idData[index]))
                {
                    result.Add(new FormatException(GenerateDuplicateHeaderDataFoundExceptionText(idData[index])));
                }
            }

            return result;
        }

        private List<Exception> CheckDuplicateHeaderName(string[] headerNames)
        {
            List<Exception> errors = new List<Exception>();
            HashSet<string> seen = new HashSet<string>();
            foreach (string headerName in headerNames)
            {
                if (!seen.Add(headerName))
                {
                    errors.Add(new FormatException(GenerateDuplicateHeaderNameExceptionText(headerName)));
                }
            }

            return errors;
        }

        private List<Exception> ValidateHeader(IReadOnlyList<DataColumn> dataColumns)
        {
            var errors = new List<Exception>();
            errors.AddRange(CheckDuplicateHeaderName(dataColumns.Select(h => h.Header).ToArray()));
            var idData = dataColumns.FirstOrDefault(c => c.Header == AchievementHeaderId);
            var progressTargetData = dataColumns.FirstOrDefault(c => c.Header == AchievementHeaderProgressTarget);
            if (idData == null)
            {
                errors.Add(new FormatException(AchievementHeaderIdNotFound));
            }

            if (progressTargetData == null)
            {
                errors.Add(new FormatException(AchievementHeaderProgressTargetNotFound));
            }

            return errors;
        }

        public string GenerateDuplicateHeaderDataFoundExceptionText(string duplicateData)
        {
            return $"Duplicate ID found: {duplicateData}";
        }

        public string GenerateEmptyIdFoundExceptionText(int index)
        {
            return $"Empty ID found at row: {index}";
        }

        public string GenerateDuplicateHeaderNameExceptionText(string duplicateHeaderName)
        {
            return $"Duplicate header name: {duplicateHeaderName} found in CSV file.";
        }

        public string GenerateProgressTargetNotValidExceptionText(int index)
        {
            return $"{AchievementHeaderProgressTarget} at row: {index} is not a valid integer";
        }

        public string CreateEmptyAchievementFoundExceptionText(int indexEmptyAchievementFound)
        {
            return $"Found an empty achievement name at number: {indexEmptyAchievementFound}.";
        }
    }
}
