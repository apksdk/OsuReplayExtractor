﻿using System;

namespace OsuReplayExtractor.Replay
{
    public class MapReplay
    {
        public MapReplay() { }

        public string BeatmapHash
        {
            get; set;
        }

        public string PlayerName
        {
            get; set;
        }

        public string ReplayHash
        {
            get; set;
        }

        public DateTime ReplayDate
        {
            get; set;
        }
    }
}
