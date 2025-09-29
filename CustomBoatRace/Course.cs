
using ModdingAPI;

namespace CustomBoatRace;

internal class BoatRaceCourse
{
    public static readonly string VanillaCourseId = "default";
    public static List<string> IDs { get => [VanillaCourseId, .. courses.Keys]; }
    private static readonly SortedDictionary<string, BoatRaceCourse> courses = [];
    public readonly string id;
    internal IReadOnlyList<Tuple<float, float, float>> data;
    internal float BestTime { get => bestTime < 0 ? initialBestTime : bestTime; }
    private readonly float initialBestTime;
    private float bestTime = -1f;
    private BoatRaceCourse(string id, IReadOnlyList<Tuple<float, float, float>> data, float initialBestTime)
    {
        this.id = id;
        this.data = data;
        this.initialBestTime = initialBestTime;
    }
    // Tuple<float, float, float> of data: x coeff, z coeff, y rotation
    public static void Register(string id, IReadOnlyList<Tuple<float, float, float>> data, float initialBestTime)
    {
        id = id.Trim();
        if (courses.ContainsKey(id)) return;
        if (id == VanillaCourseId) return;
        courses[id] = new BoatRaceCourse(id, data, initialBestTime);
    }
    public static void TryParse(IEnumerable<string> lines)
    {
        string? id = null;
        float? initialBestTime = null;
        List<Tuple<float, float, float>> parsedData = [];
        List<string> initialBestTimeKeys = ["initialbesttime", "initial best time", "best time", "time", "best"];
        foreach (var rawLine in lines)
        {
            var line = rawLine.Trim();
            if (line.Length == 0) continue;
            if (line.StartsWith('#')) continue;
            if (id == null && line.ToLower().StartsWith("id:"))
            {
                id = line[3..].Trim();
                if (id.Length == 0) id = null;
                else if (id == VanillaCourseId) return;
            }
            if (initialBestTime == null)
            {
                foreach (var key in initialBestTimeKeys)
                {
                    if (!line.ToLower().StartsWith($"{key}:")) continue;
                    var s = line[(key.Length + 1)..].Trim();
                    if (float.TryParse(s, out var v) && v > 3 && v < 10000)
                    {
                        initialBestTime = v;
                        break;
                    }
                }
            }
            var entries = line.Split(' ', StringSplitOptions.RemoveEmptyEntries).Select(c => c.Trim()).ToList();
            if (entries.Count != 3) continue;
            float[] values = new float[3];
            for (int i = 0; i < 3; i++)
            {
                if (!float.TryParse(entries[i], out values[i])) continue;
            }
            parsedData.Add(new(values[0], values[1], values[2]));
        }
        Monitor.Log($"id: {id}, initialBestTime: {initialBestTime}, num: {parsedData.Count}");
        if (id != null && initialBestTime != null && parsedData.Any())
        {
            Register(id, parsedData, (float)initialBestTime);
        }
    }
    public static bool TryToGet(string id, out BoatRaceCourse course) => courses.TryGetValue(id, out course);
    public void SendTime(float time)
    {
        if (time < BestTime)
        {
            Monitor.Log($"BEST TIME!!! ({BestTime} -> {time})", LL.Warning);
            bestTime = time;
            OnBestTimeUpdated();
        }
    }
    private static readonly string BestTimesCacheFileName = "besttimes.txt";
    private static ModdingAPI.IO.TextCache? bestTimeCacheFile = null;
    private static void OnBestTimeUpdated()
    {
        List<string> lines = [];
        foreach (var key in courses.Keys)
        {
            var course = courses[key];
            if (course.bestTime < 0) continue;
            lines.Add($"{course.id}:{CustomBoatRace.ConvertRaceTimeToDialogueString(course.bestTime)}");
        }
        bestTimeCacheFile?.WriteLines(lines);
    }

    internal static void OnGameLaunched(IMod mod)
    {
        List<string> courseFiles = [
            ..ModdingAPI.IO.Core.GetFiles(mod, "*.boatrace.txt"),
            ..ModdingAPI.IO.Core.GetFiles(mod, "boatrace/*.txt"),
        ];
        foreach (var filename in courseFiles)
        {
            var file = new ModdingAPI.IO.TextFile(mod, filename, createFileIfNotExists: false);
            if (file.Exists()) TryParse(file.ReadLines());
        }
        bestTimeCacheFile = new ModdingAPI.IO.TextCache(mod, BestTimesCacheFileName);
        foreach (var line in bestTimeCacheFile.ReadLines())
        {
            if (line.Trim().Length == 0) continue;
            try
            {
                var parts = line.Split(':', 2);
                if (parts.Length != 2) continue;
                var id = parts[0].Trim();
                var time = float.Parse(parts[1]);
                if (time <= 0) continue;
                if (TryToGet(id, out var course)) course.bestTime = time;
            }
            catch { }
        }
    }
}

