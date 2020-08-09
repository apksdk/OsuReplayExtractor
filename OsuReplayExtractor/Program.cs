using OsuReplayExtractor.Replay;
using System;
using System.Collections.Generic;
using System.Globalization;
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
                 File.Create(apiFileLocation).Close();
            }

            string osuFolderPath;
            if (args.Length == 0)
            {
                Console.WriteLine(Properties.Resources.PromptInstallPath);
                osuFolderPath = Console.ReadLine();
            }
            else
            {
                osuFolderPath = args[0];
            }

            ExtractReplays(replayLocation, osuFolderPath);

            Console.WriteLine(Properties.Resources.PromptExit);
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
                Console.WriteLine(Properties.Resources.InformMissingAPIKey);
                skipAPI = true;
            }

            // Get all replays' beatmap hash
            Console.WriteLine(Properties.Resources.InformReplayGathering);
            foreach (string replayPath in replays)
            {
                replayMapHashes.Add(ReplayReader.GetReplayInfo(replayPath));
            }

            Console.WriteLine(Properties.Resources.InformMapsGathering);
            Dictionary<string, Beatmap> beatmaps = DBReader.ReadDB(osuFolderPath + "\\osu!.db");
            // Match the replay to its' corresponding beatmap
            for (int i = 0; i < replayMapHashes.Count; i++)
            {
                string totalPercentage = (Convert.ToDouble(i) / (replayMapHashes.Count - 1)).ToString("P", CultureInfo.InvariantCulture);
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

                    CopyAndRenameReplay(replays[i], replayLocation, replayMapHash, beatmap, totalPercentage);
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
                        Console.WriteLine(Properties.Resources.InformUsingSearchAPI);
                        Beatmap retreivedBeatmap = OsuApi.GetMapWithApiAsync(replayMapHash.BeatmapHash, out apiErrorMsg);
                        if (retreivedBeatmap == null)
                        {
                            Console.WriteLine(Properties.Resources.InformSearchAPIFailed);
                            failedSearches.Add(replayMapHash.BeatmapHash);
                            failedMapPaths.Add(replays[i]);
                            // Add beatmap hash to skip duplicate API calls
                            beatmaps.Add(replayMapHash.BeatmapHash, null);
                        }
                        else
                        {
                            CopyAndRenameReplay(replays[i], replayLocation, replayMapHash, retreivedBeatmap, totalPercentage);
                        }
                    }
                }
            }

            failedSearches = failedSearches.Distinct().ToList();
            LogFailures(failedSearches, failedMapPaths, apiErrorMsg, skipAPI);
        }

        private static void LogFailures(List<string> failedSearches, List<string> failedMapPaths, string apiErrorMsg, bool skipAPI)
        {
            if (failedSearches.Count > 0)
            {
                Console.WriteLine(Properties.Resources.InformFailedMapHashCount, failedSearches.Count);
                File.WriteAllLines(AppDomain.CurrentDomain.BaseDirectory + "\\FailedMapMatches.txt", failedSearches.ToArray(), Encoding.UTF8);
                Console.WriteLine(Properties.Resources.InformFailedReplayCount, failedMapPaths.Count);
                File.WriteAllLines(AppDomain.CurrentDomain.BaseDirectory + "\\FailedExtractsPath.txt", failedMapPaths.ToArray(), Encoding.UTF8);
                Console.WriteLine(Properties.Resources.InformFailedResultPaths);
            }

            if(skipAPI)
            {
                Console.WriteLine(Properties.Resources.InformSearchAPISkip);
            }

            if (apiErrorMsg.Length > 0)
            {
                Console.WriteLine(Properties.Resources.InformSearchAPIFailureReponse, apiErrorMsg);
                Console.WriteLine(Properties.Resources.InformSearchAPIFailureMessage);
            }
        }

        static void CopyAndRenameReplay(string replayFilePath, string savePath, MapReplay replay, Beatmap beatmap, string totalPercentage)
        {
            string replayFileName = replay.PlayerName + " - " + beatmap.ArtistName + " - " + beatmap.SongTitle + " [" + beatmap.Difficulty + "] (" + replay.ReplayDate.ToString("dd-MM-yyyy hh-mm-ss tt", new CultureInfo("en-AU")) + ") " + replay.ReplayType.ToString();
            replayFileName = string.Join("", replayFileName.Split(Path.GetInvalidFileNameChars()));
            File.Copy(replayFilePath, savePath + "\\" + replayFileName + ".osr", true);
            Console.WriteLine(totalPercentage + ": " + replayFileName);
        }
    }
}
