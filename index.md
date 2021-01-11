<meta name="google-site-verification" content="ydk3NbOgWqjR-UswmFVZyJOILAHRrm1BoiZdz17wow8" />

# OsuReplayExtractor
Extracts all osu! replays that are saved locally without manually copying and renaming individual replays. Replays are named as followed:

`Player Name - Artist Name - Song Title [Diff Name] (Replay Date) Game Mode`

e.g.

`jw9478 - Eve - GekkaMidare Botan [Nameless Stage] (03-06-2020 12-22-25 PM) Osu`

This application is reliant on maps that are stored in `osu!.db`, as replays do not save map information. If your `osu!.db` is missing or does not contain the maps that are used 
for the replays, it will have to use the osu! API to get map information.

You can get your API key [here](https://osu.ppy.sh/p/api/).

**Keep in mind that osu! API should be called ideally no more than 60 times per minute, as mentioned in the [documentation](https://github.com/ppy/osu-api/wiki#terms-of-use). While I have not seen a use case where a search of more than 60+ maps per minute are required, please open an issue this becomes a problem and I'll look into a way for throttling the requests.**

## Usage
Can be executed via commandline or shortcuts like so:
- OsuReplayExtractor.exe C:\osu!

Alternatively, you can manually input your osu! folder after opening the application without any arguments.

OsuReplayExtractor supports beatmap lookup using osu!'s API. Edit the `apikey.txt` and put your API key if you wish to do so.
