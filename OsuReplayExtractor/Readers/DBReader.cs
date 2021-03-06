﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;

namespace OsuReplayExtractor.Replay
{
    class DBReader
    {

        public static Dictionary<string, Beatmap> ReadDB(string dbPath)
        {
            BinaryReader fileReader;
            fileReader = new BinaryReader(new FileStream(dbPath, FileMode.Open, FileAccess.Read, FileShare.Read));
            fileReader.ReadBytes(17);
            fileReader.ReadNullableString();
            int numBeatmaps = fileReader.ReadInt32();
            Dictionary<string, Beatmap> beatmaps = new Dictionary<string, Beatmap>();
            for (int i = 0; i < numBeatmaps; i++)
            {
                Beatmap beatmap = readBeatmap(fileReader);
                if (beatmap.Hash == null)
                {
                    continue;
                }
                beatmaps.Add(beatmap.Hash, beatmap);
            }
            fileReader.Dispose();

            if(File.Exists(@"mapCache.txt"))
            {
                ReadCache(beatmaps);
            }
            return beatmaps;
        }

        public static Dictionary<string, Beatmap> ReadCache(Dictionary<string, Beatmap> beatmaps)
        {
            StreamReader fileReader = new StreamReader(@"mapCache.txt");
            string line;
            while((line = fileReader.ReadLine()) != null)
            {
                string[] beatmapInfo = line.Split(':');
                Beatmap beatmap = new Beatmap();
                beatmap.ArtistName = beatmapInfo[0];
                beatmap.SongTitle = beatmapInfo[1];
                beatmap.Difficulty = beatmapInfo[2];
                beatmap.Hash = beatmapInfo[3];
                beatmaps.Add(beatmap.Hash, beatmap);
            }
            fileReader.Dispose();
            return beatmaps;
        }

        private static Beatmap readBeatmap(BinaryReader fileReader)
        {
            Beatmap beatmap = new Beatmap();
            beatmap.ArtistName = fileReader.ReadNullableString();
            fileReader.ReadNullableString();
            beatmap.SongTitle = fileReader.ReadNullableString();
            fileReader.ReadNullableString();
            fileReader.ReadNullableString();
            beatmap.Difficulty = fileReader.ReadNullableString();
            fileReader.ReadNullableString();
            beatmap.Hash = fileReader.ReadNullableString();
            int n;
            fileReader.ReadNullableString();
            fileReader.ReadBytes(39);
            for (int i = 0; i < 4; i++)
            {
                n = fileReader.ReadInt32();
                for (int j = 0; j < n; j++)
                {
                    fileReader.ReadBytes(14);
                }
            }
            fileReader.ReadBytes(12);
            n = fileReader.ReadInt32();
            for (int j = 0; j < n; j++)
            {
                fileReader.ReadBytes(17);
            }
            fileReader.ReadBytes(22);
            beatmap.GameMode = (GameMode)Enum.Parse(typeof(GameMode), fileReader.ReadByte().ToString(CultureInfo.InvariantCulture));
            fileReader.ReadNullableString();
            fileReader.ReadNullableString();
            fileReader.ReadUInt16();
            fileReader.ReadNullableString();
            fileReader.ReadBytes(10);
            fileReader.ReadNullableString();
            fileReader.ReadBytes(18);
            return beatmap;
        }
    }
}
