using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace Unity.PlatformToolkit.PlayMode
{
    internal class PlayModeSavingSystem : ISavingSystem
    {
        private readonly IEnvironment m_Environment;
        private readonly GenericSavingSystem m_GenericSystem;

        public PlayModeSavingSystem(IEnvironment environment, PlayModeSaveData saveData)
        {
            m_Environment = environment;
            m_GenericSystem = new(new PlayModeStorageSystem(environment, saveData), runInBackground: false);
        }

        public async Task<IReadOnlyList<string>> EnumerateSaveNames()
        {
            await m_Environment.WaitIfPaused();
            return await m_GenericSystem.EnumerateSaveNames();
        }

        public async Task<ISaveReadable> OpenSaveReadable(string name)
        {
            await m_Environment.WaitIfPaused();
            return await m_GenericSystem.OpenSaveReadable(name);
        }

        public async Task<ISaveWritable> OpenSaveWritable(string name)
        {
            await m_Environment.WaitIfPaused();
            return await m_GenericSystem.OpenSaveWritable(name);
        }

        public async Task<bool> SaveExists(string name)
        {
            await m_Environment.WaitIfPaused();
            return await m_GenericSystem.SaveExists(name);
        }

        public async Task DeleteSave(string name)
        {
            await m_Environment.WaitIfPaused();
            await m_GenericSystem.DeleteSave(name);
        }
    }
}
