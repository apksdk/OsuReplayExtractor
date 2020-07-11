# OsuReplayExtractor
Extracts all osu! replays that are saved locally without manually copying and renaming individual replays. Replays are named as followed:

`Player Name - Artist Name - Song Title [Diff Name] (Replay Date) Game Mode`

e.g.

`jw9478 - Eve - GekkaMidare Botan [Nameless Stage] (03-06-2020 12-22-25 PM) Osu`

This application is reliant on maps that are stored in `osu!.db`, as replays do not save map information. If your `osu!.db` is missing or does not contain the maps that are used 
for the replays, it will have to use the osu! API to get map information.

You can get your API key [here](https://osu.ppy.sh/p/api/).

## Usage
Can be executed via commandline or shortcuts like so:
- OsuReplayExtractor.exe C:\osu!

Alternatively, you can manually input your osu! folder after opening the application without any arguments.

OsuReplayExtractor supports beatmap lookup using osu!'s API. Edit the `apikey.txt` and put your API key if you wish to do so.
