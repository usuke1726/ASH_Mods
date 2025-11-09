
using ModdingAPI;
using UnityEngine;
using Checkpoints = Sidequel.Const.PatrolCheckpoints;

namespace Sidequel.System.Patrol;

internal static class PassingStateRegistory
{
    private static HashSet<Checkpoints> unpassedIds = [];
    private static readonly Dictionary<Checkpoints, Checkpoint> checkpoints = [];
    internal const string Tag = "PatrolPassedSpots";
    internal static void AddCheckpoint(Checkpoint checkpoint) => checkpoints[checkpoint.Id] = checkpoint;
    internal static bool GetPassed(Checkpoints id) => !unpassedIds.Contains(id);
    internal static void SetPassed(Checkpoints id)
    {
        unpassedIds.Remove(id);
        Save();
    }
    internal static int PassedCount => initialData.Count - unpassedIds.Count;
    internal static int UnpassedCount => unpassedIds.Count;
    internal static bool PassedAllCheckpoints => unpassedIds.Count == 0;
    internal static Checkpoints GetRandomUnpassedCheckpointID()
    {
        if (unpassedIds.Count == 0) return default;
        return unpassedIds.PickRandom();
    }
    internal static void OnEventDone()
    {
        foreach (var checkpoint in checkpoints.Values)
        {
            GameObject.Destroy(checkpoint.gameObject);
        }
        checkpoints.Clear();
    }
    internal static void Setup(IModHelper helper)
    {
        helper.Events.Gameloop.GameStarted += (_, _) =>
        {
            if (!State.IsActive) return;
            Load();
            if (Flags.NodeIP(Const.Events.Patrol)) Data.CreateCheckpoints();
        };
    }
    private static void Save()
    {
        var data = string.Join(",", unpassedIds.Select(id => $"{(int)id}"));
        STags.SetString(Tag, data);
    }
    private static void Load()
    {
        unpassedIds.Clear();
        var data = STags.GetString(Tag);
        if (data == null)
        {
            unpassedIds = [.. initialData];
            return;
        }
        foreach (var d in data.Split(","))
        {
            if (int.TryParse(d, out var id)) unpassedIds.Add((Checkpoints)id);
        }
    }
    private static readonly HashSet<Checkpoints> initialData = [.. Enum.GetValues(typeof(Checkpoints)).Cast<Checkpoints>().Where(c => c != default)];
}

