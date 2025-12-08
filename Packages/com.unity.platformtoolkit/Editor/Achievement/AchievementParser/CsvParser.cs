using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;
using FormatException = System.FormatException;

namespace Unity.PlatformToolkit.Editor
{
    /// <summary>
    /// Parser for achievement data in CSV format
    /// </summary>
    internal static class CsvParser
    {
        public static string NoColumnsFound = "File format error, no columns found";
        public static string InvalidNumberOfCells = "File format error, invalid number of cells.";

        // https://regex101.com/r/8pTyuT/1
        private const string k_CsvRegexPattern = @"(?:^|,)(?:""((?:[^""]|"""")*)""|([^,\r\n]*))";

        /// <summary>
        /// Parses the CSV data (in string format) to C# objects.
        /// </summary>
        /// <param name="csv">the CSV file in string format</param>
        /// <exception cref="CsvParsingException">Thrown when an invalid CSV file is given, e.g. too many rows.</exception>
        /// <returns>A collection of achievement data</returns>
        public static IReadOnlyList<DataColumn> Parse(string csv)
        {
            var dataArray = ParseCsvFields(csv);
            CheckIsValidCsvFile(dataArray, csv);
            CreateColumns(dataArray, out var columns);

            var exceptions = CheckForIllegalCharacters(columns);
            exceptions.AddRange(CheckForCharacterLimitExceeded(columns));
            if (exceptions.Count > 0)
            {
                throw new AggregateException(exceptions);
            }
            return columns;
        }

        private static List<AggregateException> CheckForCharacterLimitExceeded(IReadOnlyList<DataColumn> columns)
        {
            List<AggregateException> exceptions = new List<AggregateException>();

            for (int columnIndex = 0; columnIndex < columns.Count; columnIndex++)
            {
                if (columns[columnIndex].Header.Length > AchievementEditor.AchievementCharacterLimit)
                {
                    exceptions.Add(new AggregateException($"Character limit exceeded at header: {AchievementEditor.AchievementHeaderId}, index: {columnIndex}. Character limit: {AchievementEditor.AchievementCharacterLimit}."));
                }

                for (int dataIndex = 0; dataIndex < columns[columnIndex].Data.Length; dataIndex++)
                {
                    if (columns[columnIndex].Data[dataIndex].Length > AchievementEditor.AchievementCharacterLimit)
                    {
                        exceptions.Add(new AggregateException($"Character limit exceeded at column header: {columns[columnIndex].Header}, row: {dataIndex}. Character limit: {AchievementEditor.AchievementCharacterLimit}."));
                    }
                }
            }

            return exceptions;
        }

        private static List<AggregateException> CheckForIllegalCharacters(IReadOnlyList<DataColumn> columns)
        {
            List<AggregateException> exceptions = new List<AggregateException>();
            var idData = columns.FirstOrDefault(c => c.Header == AchievementEditor.AchievementHeaderId);
            var restOfData = columns.Where(c => c.Header != AchievementEditor.AchievementHeaderId).ToList();
            for (int i = 0; i < idData.Data.Length; i++)
            {
                if (!Regex.IsMatch(idData.Data[i], AchievementEditor.CommonIdRegexPattern))
                {
                    exceptions.Add(new AggregateException(AchievementEditor.AchievementDataError + $" At header: {AchievementEditor.AchievementHeaderId}, index {i}. Only characters a-z, A-Z, '-' and '_' are allowed for common ids."));
                }
            }

            for (int columnIndex = 0; columnIndex < restOfData.Count; columnIndex++)
            {
                for (int dataIndex = 0; dataIndex < restOfData[columnIndex].Data.Length; dataIndex++)
                {
                    if(string.IsNullOrWhiteSpace(restOfData[columnIndex].Data[dataIndex]))
                        continue;
                    if (Regex.IsMatch(restOfData[columnIndex].Data[dataIndex], AchievementEditor.AchievementImplementationDataRegexPattern))
                    {
                        exceptions.Add(new AggregateException(AchievementEditor.AchievementDataError + $" Column header: {restOfData[columnIndex].Header}, index: {dataIndex}. No whitespace allowed for platform ids"));
                    }
                }
            }

            return exceptions;
        }

        private static string[,] ParseCsvFields(string csv)
        {
            string[] lines = csv.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None);
            string[,] result = null;
            List<Exception> exceptions = new List<Exception>();

            exceptions.AddRange(ValidateHeader(lines[0]));
            if (exceptions.Count > 0)
            {
                throw new AggregateException(exceptions);
            }
            for (var rowIndex = 0; rowIndex < lines.Length; rowIndex++)
            {
                var line = lines[rowIndex];
                exceptions.AddRange(ValidateLine(line, rowIndex));
                var matches = Regex.Matches(line, k_CsvRegexPattern);
                for (var cellIndex = 0; cellIndex < matches.Count; cellIndex++)
                {
                    if (result == null)
                    {
                        result = new string[matches.Count, lines.Length];
                    }

                    var match = matches[cellIndex];
                    string value = match.Groups[1].Success
                        ? match.Groups[1].Value.Replace("\"\"", "\"")
                        : match.Groups[2].Value;

                    result[cellIndex, rowIndex] = value.Trim();
                }
            }

            if (exceptions.Count > 0)
            {
                throw new AggregateException(exceptions);
            }

            return result;
        }

        private static List<FormatException> ValidateHeader(string header)
        {
            List<FormatException> exceptions = new List<FormatException>();
            if (string.IsNullOrEmpty(header))
            {
                exceptions.Add(new FormatException(NoColumnsFound));
                return exceptions;
            }

            return exceptions;
        }

        private static List<FormatException> ValidateLine(string line, int index)
        {
            List<FormatException> exceptions = new List<FormatException>();
            if (string.IsNullOrEmpty(line))
            {
                exceptions.Add(new FormatException(GenereateEmptyLineFoundAtIndexExceptionText(index)));
            }

            return exceptions;
        }

        private static void CreateColumns(string[,] dataArray, out List<DataColumn> columns)
        {
            columns = new List<DataColumn>();

            for (int i = 0; i < dataArray.GetLength(0); i++)
            {
                var header = GetColumnHeader(dataArray, i);
                var data = GetColumnData(dataArray, i);
                columns.Add(new DataColumn(header, data));
            }
        }

        private static string GetColumnHeader(string[,] dataArray, int columnIndex)
        {
            return dataArray[columnIndex, 0];
        }

        private static string[] GetColumnData(string[,] dataArray, int columnIndex)
        {
            var data = new List<string>();

            // We need to skip the header name, so we start at 1
            for (int i = 1; i < dataArray.GetLength(1); i++)
            {
                data.Add(dataArray[columnIndex, i]);
            }

            return data.ToArray();
        }

        private static void CheckIsValidCsvFile(string[,] data, string csv)
        {
            int cellCount = 0;
            string[] lines = csv.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None);

            foreach (string line in lines)
            {
                var matches = Regex.Matches(line, k_CsvRegexPattern);
                if (line.StartsWith(","))
                {
                    cellCount++;
                }
                cellCount += matches.Count;
            }

            var columns = data.GetLength(0);
            var cells = data.GetLength(1);

            List<Exception> exceptions = new List<Exception>();
            if (cellCount != columns * cells)
            {
                exceptions.Add(new FormatException(InvalidNumberOfCells));
            }

            var columnNames = new HashSet<string>();
            for (int i = 0; i < data.GetLength(0); i++)
            {
                var header = GetColumnHeader(data, i);
                if (string.IsNullOrEmpty(header))
                {
                    exceptions.Add(new FormatException(GenerateEmptyHeaderFoundAtIndexExceptionText(i)));
                    continue;
                }
                if (!columnNames.Add(header))
                {
                    exceptions.Add(new FormatException(GenerateDuplicateHeaderNameFoundExceptionText(header)));
                }
            }

            if(!columnNames.Contains(AchievementEditor.AchievementHeaderId))
                exceptions.Add(new FormatException(GenerateHeaderDoestContainExceptionText(AchievementEditor.AchievementHeaderId)));

            if(!columnNames.Contains(AchievementEditor.AchievementHeaderProgressTarget))
                exceptions.Add(new FormatException(GenerateHeaderDoestContainExceptionText(AchievementEditor.AchievementHeaderProgressTarget)));

            if (exceptions.Count > 0)
            {
                throw new AggregateException(exceptions);
            }
        }

        public static string GenerateEmptyHeaderFoundAtIndexExceptionText(int index)
        {
            return $"File format error, empty header found at index: {index}";
        }

        public static string GenerateDuplicateHeaderNameFoundExceptionText(string duplicateHeaderName)
        {
            return $"File format error, duplicate header name found: {duplicateHeaderName}";
        }

        public static string GenerateHeaderDoestContainExceptionText(string headerName)
        {
            return $"File format error, header doesn't contain name: {AchievementEditor.AchievementHeaderId}";
        }

        public static string GenereateEmptyLineFoundAtIndexExceptionText(int index)
        {
            return $"File format error, found an empty line at row index: {index}";
        }
    }
}
