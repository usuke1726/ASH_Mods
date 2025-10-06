
using ModdingAPI;
using UnityEngine;

namespace SideStory.World;

internal class PlayerPosition
{
    private static readonly string playerPosTag = "PlayerPos";
    internal static void Setup(IModHelper helper)
    {
        System.STags.BeforeSaving += SavePlayerPos;
    }
    private static void SavePlayerPos()
    {
        if (!State.IsActive) return;
        if (!Context.TryToGetPlayer(out var player)) return;
        System.STags.SetString(playerPosTag, Serialize(player));
    }
    internal static void Spawn()
    {
        if (!State.IsActive) return;
        if (!Context.TryToGetPlayer(out var player)) return;
        ResetPlayerPos(player);
    }
    private static Vector3 SafePosition(Player player, Vector3 position)
    {
        List<int> diffs = [0];
        for (int i = 1; i < 5; i++)
        {
            diffs.Add(i);
            diffs.Add(-i);
        }
        List<int> yDiffs = [0, 1, 2];
        var collider = player.GetComponent<CapsuleCollider>();
        foreach (int x in diffs)
        {
            foreach (int z in diffs)
            {
                foreach (int y in yDiffs)
                {
                    Vector3 p = position + new Vector3(x / 2f, y, z / 2f);
                    collider.ToWorldSpaceCapsule(p, out var point0, out var point1, out var radius);
                    point1.y += 0.25f;
                    if (Physics.OverlapCapsule(point0, point1, radius, -513, QueryTriggerInteraction.Ignore).Any())
                    {
                        continue;
                    }
                    return p;
                }
            }
        }
        return position;
    }
    private static void ResetPlayerPos(Player player)
    {
        var pos = GetPositions(player);
        player.transform.position = pos.Item1;
        player.transform.rotation = Quaternion.Euler(pos.Item2);
        Physics.SyncTransforms();
    }
    private static Tuple<Vector3, Vector3> GetPositions(Player player)
    {
        var pos = Deserialize(Load());
        if (pos == null) return InitialPosition();
        return new(SafePosition(player, pos.Item1), pos.Item2);
    }
    private static string Load()
    {
        return State.IsNewGame ? null! : System.STags.GetString(playerPosTag);
    }
    private static string Serialize(Player player)
    {
        var pos = player.body.position;
        var rot = player.body.rotation.eulerAngles;
        return $"{pos.x},{pos.y},{pos.z},{rot.x},{rot.y},{rot.z}";
    }
    private static Tuple<Vector3, Vector3>? Deserialize(string data)
    {
        if (data == null) return null;
        Debug($"loaded pos \"{data}\"");
        var d = data.Split(",", 6);
        if (d.Length < 6) return null;
        List<float> vals = [];
        for (int i = 0; i < 6; i++)
        {
            if (float.TryParse(d[i], out var f)) vals.Add(f);
            else return null;
        }
        return new(new(vals[0], vals[1], vals[2]), new(vals[3], vals[4], vals[5]));
    }
    private static Tuple<Vector3, Vector3> InitialPosition()
    {
        return new(
            new(256.0943f, 267.7789f, 567.0623f),
            new(0f, 184.6624f, 0f)
        );
    }
}

