using System;
using System.IO;
using System.Threading.Tasks;
using Unity.PlatformToolkit.PlayMode;
using UnityEngine;

namespace Unity.PlatformToolkit
{
    internal class PlayModeSolidZipFileArchive : SolidZipFileArchive
    {
        private PlayModeSaveDataInfo m_playModeBoundInfo;

        public PlayModeSolidZipFileArchive(Stream stream, string name, bool writable, PlayModeSaveDataInfo info) : base(stream, name, writable)
        {
            m_playModeBoundInfo = info;
            m_playModeBoundInfo.Name = name;
        }
    }
}
