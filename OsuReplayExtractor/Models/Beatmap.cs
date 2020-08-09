using System;

namespace OsuReplayExtractor.Replay
{
    public class Beatmap
    {
        public Beatmap()
        {}
        public string ArtistName
        {
            get; set;
        }
        public string SongTitle
        {
            get; set;
        }
        public string Difficulty
        {
            get; set;
        }
        public string Hash
        {
            get; set;
        }
        public GameMode GameMode
        {
            get; set;
        }

        public override string ToString()
        {
            return ArtistName + ":" + SongTitle + ":" + Difficulty + ":" + Hash;
        }
    }

    public enum GameMode
    {
        Osu = 0,
        Taiko = 1,
        CTB = 2,
        Mania = 3
    }
}
