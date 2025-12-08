using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace Unity.PlatformToolkit.Editor
{
    internal static class CsvExporter
    {
        public static string Export(List<DataColumn> platformAchievements)
        {
            StringBuilder fileContent = new StringBuilder();
            fileContent.Append(GenerateHeader(platformAchievements));

            fileContent.Append(Environment.NewLine);
            for (int i = 0; i < platformAchievements[0].Data.Length; i++)
            {
                fileContent.Append(GenerateRow(platformAchievements, i));
                if (i < platformAchievements[0].Data.Length - 1)
                {
                    fileContent.Append(Environment.NewLine);
                }
            }

            return fileContent.ToString();
        }

        private static string GenerateRow(List<DataColumn> platformAchievements, int dataIndex)
        {
            StringBuilder rowContent = new StringBuilder();
            for (var index = 0; index < platformAchievements.Count; index++)
            {
                rowContent.Append($"{SanitizeCsvCell(platformAchievements[index].Data[dataIndex])}");
                if (index < platformAchievements.Count - 1)
                {
                    rowContent.Append(',');
                }
            }

            return rowContent.ToString();
        }

        private static string GenerateHeader(List<DataColumn> platformAchievements)
        {
            StringBuilder header = new StringBuilder();
            for (var index = 0; index < platformAchievements.Count; index++)
            {
                header.Append($"{SanitizeCsvCell(platformAchievements[index].Header)}");
                if (index < platformAchievements.Count - 1)
                {
                    header.Append(',');
                }
            }
            return header.ToString();
        }

        private static string SanitizeCsvCell(string cellData)
        {
            bool needsQuotesAroundCell = false;
            // In a CSV file, if quotes are used in the cell, an extra quote is added to represent a single quote
            if (!string.IsNullOrEmpty(cellData) && cellData.Contains(@""""))
            {
                needsQuotesAroundCell = true;
                cellData = cellData.Replace(@"""", @"""""");
            }

            // In CSV files, when a comma is used, an extra single quote is added at the start and end to indicate
            // we can ignore the comma(s) between the single quotes
            if (!string.IsNullOrEmpty(cellData) && cellData.Contains(','))
            {
                needsQuotesAroundCell = false;
                cellData =  @"""" + cellData + @"""";
            }

            // When any of the above is used, we always need to add quotes around the cell
            if (needsQuotesAroundCell)
            {
                cellData =  @"""" + cellData + @"""";
            }

            return cellData.Trim();
        }
    }
}
