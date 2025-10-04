
using ModdingAPI;

namespace CustomConversation;

#pragma warning disable IDE1006
internal struct Info()
{
    private static readonly float defaultOffsetY = 2.4f;
    public float offsetY = defaultOffsetY;
    public static Info Get(Characters ch) => data.TryGetValue(ch, out var info) ? info : (_data[ch] = new());
    public static IReadOnlyDictionary<Characters, Info> data { get => _data; }
    private static readonly Dictionary<Characters, Info> _data = new()
    {
        [Characters.Claire] = new() { offsetY = 2.0f },
        [Characters.ClimbingRhino1] = new() { offsetY = 3.2f },
        [Characters.Tim1] = new() { offsetY = 3.0f },
    };
}
#pragma warning restore IDE1006

