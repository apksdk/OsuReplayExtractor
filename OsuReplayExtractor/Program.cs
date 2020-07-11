using OsuReplayExtractor.Replay;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace OsuReplayExtractor
{
    class Program
    {
        static void Main(string[] args)
        {
            string replayLocation = AppDomain.CurrentDomain.BaseDirectory + "\\Replays";
            if (!Directory.Exists(replayLocation))
            {
                Directory.CreateDirectory(replayLocation);
            }

            string apiFileLocation = AppDomain.CurrentDomain.BaseDirectory + "\\apikey.txt";
            if (!File.Exists(apiFileLocation))
            {
                File.Create(apiFileLocation);
            }

            string osuFolderPath;
            if (args.Length == 0)
            {
                Console.WriteLine("Please provide your osu folder path. (e.g. C:\\osu!)");
                osuFolderPath = Console.ReadLine();
            }
            else
            {
                osuFolderPath = args[0];
            }

            ExtractReplays(replayLocation, osuFolderPath);

            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();
        }

        private static void ExtractReplays(string replayLocation, string osuFolderPath)
        {
            List<string> replays = Directory.GetFiles(osuFolderPath + "\\Data\\r", "*.osr", SearchOption.AllDirectories).ToList();
            List<MapReplay> replayMapHashes = new List<MapReplay>();
            List<string> failedSearches = new List<string>();
            List<string> failedMapPaths = new List<string>();
            string apiErrorMsg = "";
            bool skipAPI = false;

            string apiKey = File.ReadAllText(AppDomain.CurrentDomain.BaseDirectory + "\\apikey.txt");
            if (apiKey.Length == 0)
            {
                Console.WriteLine("Missing API key, will not be searching online...");
                skipAPI = true;
            }

            // Get all replays' beatmap hash
            Console.WriteLine("Gathering replays...");
            foreach (string replayPath in replays)
            {
                replayMapHashes.Add(ReplayReader.GetReplayInfo(replayPath));
            }

            Console.WriteLine("Gathering existing maps from osu!...");
            Dictionary<string, Beatmap> beatmaps = DBReader.ReadDB(osuFolderPath + "\\osu!.db");
            // Match the replay to its' corresponding beatmap
            for (int i = 0; i < replayMapHashes.Count; i++)
            {
                MapReplay replayMapHash = replayMapHashes[i];
                Beatmap beatmap;
                bool success = beatmaps.TryGetValue(replayMapHash.BeatmapHash, out beatmap);
                if (success)
                {
                    // Beatmap has been looked up via API and does not exist
                    if (beatmap == null)
                    {
                        failedSearches.Add(replayMapHash.BeatmapHash);
                        failedMapPaths.Add(replays[i]);
                        continue;
                    }

                    CopyAndRenameReplay(replays[i], replayLocation, replayMapHash, beatmap);
                }
                else
                {
                    if(skipAPI)
                    {
                        failedSearches.Add(replayMapHash.BeatmapHash);
                        failedMapPaths.Add(replays[i]);
                        beatmaps.Add(replayMapHash.BeatmapHash, null);
                    } else
                    {
                        // Try getting beatmap via API if not found locally
                        Console.WriteLine("Attempting to search for beatmap online");
                        Beatmap retreivedBeatmap = OsuApi.GetMapWithApiAsync(replayMapHash.BeatmapHash, out apiErrorMsg);
                        if (retreivedBeatmap == null)
                        {
                            Console.WriteLine("Could not find beatmap online");
                            failedSearches.Add(replayMapHash.BeatmapHash);
                            failedMapPaths.Add(replays[i]);
                            // Add beatmap hash to skip duplicate API calls
                            beatmaps.Add(replayMapHash.BeatmapHash, null);
                        }
                        else
                        {
                            CopyAndRenameReplay(replays[i], replayLocation, replayMapHash, retreivedBeatmap);
                        }
                    }
                }
            }

            LogFailures(failedSearches, failedMapPaths, apiErrorMsg, skipAPI);
        }

        private static List<string> LogFailures(List<string> failedSearches, List<string> failedMapPaths, string apiErrorMsg, bool skipAPI)
        {
            failedSearches = failedSearches.Distinct().ToList();
            if (failedSearches.Count > 0)
            {
                Console.WriteLine("Failed to find " + failedSearches.Count() + " map hashes");
                File.WriteAllLines(AppDomain.CurrentDomain.BaseDirectory + "\\FailedMapMatches.txt", failedSearches.ToArray(), Encoding.UTF8);
                Console.WriteLine("Failed to extract " + failedMapPaths.Count() + " replays");
                File.WriteAllLines(AppDomain.CurrentDomain.BaseDirectory + "\\FailedExtractsPath.txt", failedMapPaths.ToArray(), Encoding.UTF8);
                Console.WriteLine("Failures have been saved into 'FailedMapMatches.txt' or 'FailedExtractsPath.txt'");
            }

            if(skipAPI)
            {
                Console.WriteLine("Skipped online search. Please add an API key under 'apikey.txt' to enable online searching.");
            }

            if (apiErrorMsg.Length > 0)
            {
                Console.WriteLine("Online search failed because the server returned: " + apiErrorMsg);
                Console.WriteLine("Please ensure your API key is correct and have Internet access before trying again.");
            }

            return failedSearches;
        }

        static void CopyAndRenameReplay(string replayFilePath, string savePath, MapReplay replay, Beatmap beatmap)
        {
            string replayFileName = replay.PlayerName + " - " + beatmap.ArtistName + " - " + beatmap.SongTitle + " [" + beatmap.Difficulty + "] (" + replay.ReplayDate.ToString("dd-MM-yyyy hh-mm-ss tt") + ") " + beatmap.GameMode.ToString();
            replayFileName = string.Join("", replayFileName.Split(Path.GetInvalidFileNameChars()));
            File.Copy(replayFilePath, savePath + "\\" + replayFileName + ".osr", true);
            Console.WriteLine(replayFileName);
        }
    }
}
