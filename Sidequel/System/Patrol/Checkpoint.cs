
//#define ENABLE_CHECKPOINT_DEBUGLINES

using UnityEngine;

namespace Sidequel.System.Patrol;

internal class Checkpoint : MonoBehaviour
{
    private BoxCollider collider = null!;
#if DEBUG && ENABLE_CHECKPOINT_DEBUGLINES
    private DebugLines debugLines = null!;
#endif
    internal bool Passed { get; private set; } = false;
    internal Const.PatrolCheckpoints Id { get; private set; } = default;
    private void Awake()
    {
        collider = gameObject.AddComponent<BoxCollider>();
        collider.isTrigger = true;
        gameObject.layer = 2; // ignore laycast
    }
    internal Checkpoint Set(Const.PatrolCheckpoints id, Vector3 position, Vector3 size)
    {
        transform.position = position;
        collider.size = size;
        Id = id;
        PassingStateRegistory.AddCheckpoint(this);
        Passed = PassingStateRegistory.GetPassed(Id);
#if DEBUG && ENABLE_CHECKPOINT_DEBUGLINES
        debugLines = new DebugLines(this, position, size);
#endif
        return this;
    }
    private void OnTriggerEnter(Collider other)
    {
        if (Passed) return;
        var player = other.GetComponent<Player>();
        var isPlayer = player != null && (bool)player;
        if (!isPlayer) return;
        Debug($"collided with player", LL.Warning);
        Passed = true;
        PassingStateRegistory.SetPassed(Id);
#if DEBUG && ENABLE_CHECKPOINT_DEBUGLINES
        debugLines.OnPassed();
#endif
    }

#if DEBUG && ENABLE_CHECKPOINT_DEBUGLINES
    private class DebugLines
    {
        private readonly LineRenderer[] lines;
        private Vector3 center;
        private Vector3 size;
        private Vector3 Center { get => center; set { center = value; SetPositions(); } }
        private Vector3 Size { get => size; set { size = value; SetPositions(); } }
        private readonly Checkpoint checkpoint;
        internal DebugLines(Checkpoint checkpoint, Vector3 center, Vector3 size)
        {
            this.checkpoint = checkpoint;
            this.center = center;
            this.size = size;
            var material = new Material(Shader.Find("Sprites/Default"));
            lines = [.. Enumerable.Range(0, 12).Select(CreateLine)];
            foreach (var line in lines)
            {
                line.material = material;
                line.startColor = line.endColor = checkpoint.Passed ? Color.green : Color.red;
            }
            SetPositions();
        }
        private void SetPositions()
        {
            var vertices = GetVertices();
            for (int i = 0; i < lines.Length; i++)
            {
                var line = lines[i];
                var pair = verticesIndexes[i];
                line.SetPositions([vertices[pair.Item1], vertices[pair.Item2]]);
            }
        }
        internal void OnPassed()
        {
            foreach (var line in lines)
            {
                line.startColor = line.endColor = Color.green;
            }
        }
        private LineRenderer CreateLine(int i)
        {
            var obj = new GameObject($"debugLine ({i})");
            obj.transform.parent = checkpoint.gameObject.transform;
            obj.transform.localPosition = Vector3.zero;
            var res = obj.AddComponent<LineRenderer>();
            var width = Mathf.Min([size.x, size.y, size.z]) > 5 ? 1f : 0.3f;
            res.startWidth = res.endWidth = width;
            res.useWorldSpace = true;
            return res;
        }
        private static readonly IReadOnlyList<Tuple<int, int>> verticesIndexes = [
            new(0, 1),
            new(0, 2),
            new(0, 4),
            new(1, 3),
            new(1, 5),
            new(2, 3),
            new(2, 6),
            new(3, 7),
            new(4, 5),
            new(4, 6),
            new(5, 7),
            new(6, 7),
        ];
        private List<Vector3> GetVertices()
        {
            List<Vector3> result = [];
            var x = size.x * 0.5f;
            var y = size.y * 0.5f;
            var z = size.z * 0.5f;
            Vector3[] diffs = [
                new(-x, -y, -z),
                new(-x, -y, z),
                new(-x, y, -z),
                new(-x, y, z),
                new(x, -y, -z),
                new(x, -y, z),
                new(x, y, -z),
                new(x, y, z),
            ];
            return [.. diffs.Select(p => center + p)];
        }
    }
#endif
}

