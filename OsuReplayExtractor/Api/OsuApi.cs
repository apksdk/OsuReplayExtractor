using Newtonsoft.Json;
using System;
using System.IO;
using System.Net.Http;

namespace OsuReplayExtractor.Replay
{
    class OsuApi
    {
        public static Beatmap GetMapWithApiAsync(string mapHash, out string apiErrorMsg)
        {
            apiErrorMsg = "";
            string apiKey = File.ReadAllText(AppDomain.CurrentDomain.BaseDirectory + "\\apikey.txt");
            Uri getBeatmapApi = new Uri($"https://osu.ppy.sh/api/get_beatmaps?k={apiKey}&h={mapHash}");

            
            HttpClient client = new HttpClient();
            HttpResponseMessage response = client.GetAsync(getBeatmapApi).Result;
            client.Dispose();
            if (response.IsSuccessStatusCode)
            {
                dynamic obj = JsonConvert.DeserializeObject<dynamic>(response.Content.ReadAsStringAsync().Result);
                if (obj.Count > 0)
                {
                    Beatmap beatmap = new Beatmap
                    {
                        ArtistName = RemoveInvalidFileChars((string)obj[0]["artist"]),
                        SongTitle = RemoveInvalidFileChars((string)obj[0]["title"]),
                        Difficulty = RemoveInvalidFileChars((string)obj[0]["version"]),
                        GameMode = (GameMode)Enum.Parse(typeof(GameMode), obj[0]["mode"].ToString()),
                        Hash = (string)obj[0]["file_md5"]
                    };
                    using (StreamWriter cacheWriter = File.AppendText("mapCache.txt"))
                    {
                        cacheWriter.WriteLine(beatmap.ToString());
                    }
                    return beatmap;
                }
            }
            else
            {
                apiErrorMsg = response.ReasonPhrase;
            }
            return null;
        }

        private static string RemoveInvalidFileChars(string text)
        {
            return string.Join("", text.Split(Path.GetInvalidFileNameChars()));
        }
    }
}
