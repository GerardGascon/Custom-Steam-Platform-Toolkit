using System;
using System.Text.RegularExpressions;

namespace Unity.PlatformToolkit
{
    internal static class SaveNameValidator
    {
        internal const string kValidSaveNameRegexString = "^[\\-a-z0-9]+$";
        internal const string kValidFileNameRegexString = "^[\\-a-z0-9]+$";
        public static bool IsValidFileName(string filename)
        {
            Regex rx = new(kValidFileNameRegexString);
            return rx.IsMatch(filename);
        }

        public static void CheckFileName(string filename)
        {
            if (!IsValidFileName(filename))
            {
                throw new ArgumentException($"{filename} is not a valid name for a file in a save game: it must match {kValidFileNameRegexString}.");
            }
        }

        public static bool IsValidSaveName(string savename)
        {
            Regex rx = new(kValidSaveNameRegexString);
            return rx.IsMatch(savename);
        }

        public static void CheckSaveName(string savename)
        {
            if (!IsValidSaveName(savename))
            {
                throw new ArgumentException($"{savename} is not a valid name for a save game: it must match {kValidSaveNameRegexString}.");
            }
        }
    }
}
