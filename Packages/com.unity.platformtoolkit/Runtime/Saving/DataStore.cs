using System;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UIElements;

namespace Unity.PlatformToolkit
{
    /// <summary>DataStore stores string, float, and integer values that can be easily saved and loaded using the <see cref="ISavingSystem"/>.</summary>
    /// <remarks>Changes to <see cref="DataStore"/> objects are local and are only saved after calling <see cref="Save"/>.</remarks>
    public class DataStore
    {
        private class DataStoreSave
        {
            public int Version;
            public SerializableDictionary<string, string> Strings;
            public SerializableDictionary<string, int> Ints;
            public SerializableDictionary<string, float> Floats;
        }

        private const string k_FileName = "pt-datastore";

        /// <summary>Creates an empty <see cref="DataStore"/> object.</summary>
        /// <returns>New <see cref="DataStore"/>.</returns>
        public static DataStore Create()
        {
            return new DataStore();
        }

        /// <summary>Loads <see cref="DataStore"/> object from an existing <see cref="ISavingSystem"/> save.</summary>
        /// <param name="savingSystem">The saving system to use.</param>
        /// <param name="saveName">The name of the save.</param>
        /// <param name="createIfNotFound">If true, a new <see cref="DataStore"/> will be created if the save doesn't already exist. If false, an exception will be thrown if the save is not found.</param>
        /// <exception cref="ArgumentNullException"><see cref="ISavingSystem"/> is null.</exception>
        /// <returns>Task containing the DataStore save.</returns>
        /// <exception cref="InvalidAccountException"><see cref="IAccount"/> linked to the <see cref="ISavingSystem"/> is signed out.</exception>
        /// <exception cref="InvalidSystemException"><see cref="ISavingSystem"/> is invalid.</exception>
        /// <exception cref="IO.IOException">There was an error when accessing the file or reading the data.</exception>
        /// <exception cref="InvalidOperationException">Save is already open.</exception>
        /// <exception cref="ArgumentException">The given saving system or save name is null. The save name might contain invalid characters, is too long, or is empty.</exception>
        /// <exception cref="IO.FileNotFoundException">Save with a given name doesn't exist in the <see cref="ISavingSystem"/>.</exception>
        public static async Task<DataStore> Load(ISavingSystem savingSystem, string saveName, bool createIfNotFound = true)
        {
            if (savingSystem is null)
                throw new ArgumentNullException("Saving system is null");

            if (createIfNotFound && !await savingSystem.SaveExists(saveName))
                return Create();

            await using var saveReadable = await savingSystem.OpenSaveReadable(saveName);
            var data = await saveReadable.ReadFile(k_FileName);
            return new DataStore(data);
        }

        /// <summary>Saves the DataStore object into an <see cref="ISavingSystem"/> save.</summary>
        /// <param name="savingSystem">The saving system to use.</param>
        /// <param name="saveName">The name of the save.</param>
        /// <returns>Task that completes when the save is saved.</returns>
        /// <exception cref="ArgumentException">The given saving system or save name is null. The save name might contain invalid characters, is too long, or is empty.</exception>
        /// <exception cref="NotEnoughSpaceException">There's not enough space to write the data. This can mean that the system is out of memory, but other limits can also be imposed by platforms. For example, limits on how much storage is allocated for each account or how large a single commit can be.</exception>
        /// <exception cref="System.IO.IOException">There was an error writing or committing the data.</exception>
        /// <exception cref="InvalidAccountException"><see cref="IAccount"/> linked to the <see cref="ISavingSystem"/> is signed out.</exception>
        /// <exception cref="InvalidSystemException"><see cref="ISavingSystem"/> is invalid.</exception>
        public async Task Save(ISavingSystem savingSystem, string saveName)
        {
            if (savingSystem is null)
                throw new ArgumentNullException("Saving system is null");

            await using var saveWritable = await savingSystem.OpenSaveWritable(saveName);
            await saveWritable.WriteFile(k_FileName, ConvertToBytes());
            await saveWritable.Commit();
        }

        private SerializableDictionary<string, string> m_Strings = new();
        private SerializableDictionary<string, int> m_Ints = new();
        private SerializableDictionary<string, float> m_Floats = new();

        private DataStore()
        {
        }

        private DataStore(byte[] data)
        {
            LoadFromBytes(data);
        }

        /// <summary>Checks if the specified key exists in the DataStore save.</summary>
        /// <param name="key">The key to check.</param>
        /// <returns>True if the key exists, otherwise false.</returns>
        public bool HasKey(string key)
        {
            return m_Strings.ContainsKey(key) || m_Ints.ContainsKey(key) || m_Floats.ContainsKey(key);
        }

        /// <summary>Sets a string value for a given key.</summary>
        /// <param name="key">The key of the value to set.</param>
        /// <param name="value">The string value to set.</param>
        public void SetString(string key, string value)
        {
            m_Ints.Remove(key);
            m_Floats.Remove(key);

            m_Strings[key] = value;
        }

        /// <summary>Sets an integer value for the given key.</summary>
        /// <param name="key">The key of the value to set.</param>
        /// <param name="value">The integer value to set.</param>
        public void SetInt(string key, int value)
        {
            m_Strings.Remove(key);
            m_Floats.Remove(key);

            m_Ints[key] = value;
        }

        /// <summary>Sets a float value for the given key.</summary>
        /// <param name="key">The key of the value to set.</param>
        /// <param name="value">The float value to set.</param>
        public void SetFloat(string key, float value)
        {
            m_Strings.Remove(key);
            m_Ints.Remove(key);

            m_Floats[key] = value;
        }

        /// <summary>Retrieves a string value.</summary>
        /// <param name="key">The key of the value to retrieve.</param>
        /// <returns>The string value associated with the specified key. If it doesn't exist the default value will be returned.</returns>
        public string GetString(string key)
        {
            return GetString(key, default);
        }

        /// <inheritdoc cref="GetString(string)"/>
        /// <param name="key">The key of the value to retrieve.</param>
        /// <param name="defaultValue">The default value to return if the key doesn't exist.</param>
        public string GetString(string key, string defaultValue)
        {
            return m_Strings.TryGetValue(key, out var value) ? value : defaultValue;
        }

        /// <summary>Retrieves an int value.</summary>
        /// <param name="key">The key of the value to retrieve.</param>
        /// <returns>The integer value associated with the specified key. If it doesn't exist the default value will be returned.</returns>
        public int GetInt(string key)
        {
            return GetInt(key, default);
        }

        /// <inheritdoc cref="GetInt(string)"/>
        /// <param name="key">The key of the value to retrieve.</param>
        /// <param name="defaultValue">The default value to return if the key doesn't exist.</param>
        public int GetInt(string key, int defaultValue)
        {
            return m_Ints.TryGetValue(key, out var value) ? value : defaultValue;
        }

        /// <summary>Retrieves a float value.</summary>
        /// <param name="key">The key of the value to retrieve.</param>
        /// <returns>The float value associated with the specified key. If it doesn't exist the default value will be returned.</returns>
        public float GetFloat(string key)
        {
            return GetFloat(key, default);
        }

        /// <inheritdoc cref="GetFloat(string)"/>
        /// <param name="key">The key of the value to retrieve.</param>
        /// <param name="defaultValue">The default value to return if the key doesn't exist.</param>
        public float GetFloat(string key, float defaultValue)
        {
            return m_Floats.TryGetValue(key, out var value) ? value : defaultValue;
        }

        /// <summary>Deletes the specified key and value.</summary>
        /// <param name="key">The key to delete.</param>
        public void DeleteKey(string key)
        {
            m_Strings.Remove(key);
            m_Ints.Remove(key);
            m_Floats.Remove(key);
        }

        /// <summary>Deletes all keys and values from the DataStore save.</summary>
        /// <remarks>Does not delete the image.</remarks>
        public void DeleteAll()
        {
            m_Strings.Clear();
            m_Ints.Clear();
            m_Floats.Clear();
        }

        private byte[] ConvertToBytes()
        {
            var saveData = new DataStoreSave()
            {
                Version = 1,
                Strings = m_Strings,
                Ints = m_Ints,
                Floats = m_Floats
            };

            return Encoding.UTF8.GetBytes(JsonUtility.ToJson(saveData));
        }

        private void LoadFromBytes(byte[] data)
        {
            if (data.Length == 0)
                return;

            var saveData = JsonUtility.FromJson<DataStoreSave>(Encoding.UTF8.GetString(data));

            if (saveData.Version != 1)
                throw new InvalidOperationException("DataStore version mismatch");

            m_Strings =  saveData.Strings;
            m_Ints = saveData.Ints;
            m_Floats = saveData.Floats;
        }
    }
}
