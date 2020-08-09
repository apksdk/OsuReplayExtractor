using System;
using System.IO;

namespace OsuReplayExtractor.Replay
{
    //From osuDodgyMomentsFinder
    internal static class BinaryReaderExtensions
    {
        internal static string ReadNullableString(this BinaryReader br)
        {
            if (br.ReadByte() != 0x0B)
                return null;
            return br.ReadString();
        }
    }
    class ReplayReader
    {
        public static MapReplay GetReplayInfo(string replayFilePath)
        {
            BinaryReader replayReader = new BinaryReader(new FileStream(replayFilePath, FileMode.Open, FileAccess.Read, FileShare.Read));

            MapReplay replay = new MapReplay();
            replay.ReplayType = (GameMode) replayReader.ReadByte();
            replayReader.ReadBytes(4);
            replay.BeatmapHash = replayReader.ReadNullableString();
            replay.PlayerName = replayReader.ReadNullableString();
            replayReader.ReadNullableString();
            replayReader.ReadBytes(23);
            replayReader.ReadNullableString();
            replay.ReplayDate = new DateTime(BitConverter.ToInt64(replayReader.ReadBytes(8), 0)).ToLocalTime();
            replayReader.Close();
            return replay;
        }
    }
}
