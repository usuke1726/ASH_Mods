
//#define DEBUG_ACTIVATE_SERIALIZER

using System.Runtime.InteropServices;
using UnityEngine;
using NumberStyles = System.Globalization.NumberStyles;

namespace Sidequel.System.Race;

internal static class Deserializer
{
    internal static readonly PlayerReplayData data = new() { frames = [.. Data.data.Select(DeserializeFrame)] };

#if DEBUG && DEBUG_ACTIVATE_SERIALIZER
    private static string SerializeFrame(PlayerReplayFrame frame)
    {
        byte[] data = [
            .. MemoryMarshal.Cast<int, byte>([frame.index]),
            .. MemoryMarshal.Cast<float, byte>([frame.time]),
            .. MemoryMarshal.Cast<float, byte>([frame.position.x]),
            .. MemoryMarshal.Cast<float, byte>([frame.position.y]),
            .. MemoryMarshal.Cast<float, byte>([frame.position.z]),
            .. MemoryMarshal.Cast<float, byte>([frame.velocity.x]),
            .. MemoryMarshal.Cast<float, byte>([frame.velocity.y]),
            .. MemoryMarshal.Cast<float, byte>([frame.velocity.z]),
            .. MemoryMarshal.Cast<float, byte>([frame.rotation.x]),
            .. MemoryMarshal.Cast<float, byte>([frame.rotation.y]),
            .. MemoryMarshal.Cast<float, byte>([frame.rotation.z]),
            .. MemoryMarshal.Cast<float, byte>([frame.rotation.w]),
            .. MemoryMarshal.Cast<float, byte>([frame.animatorRotation.x]),
            .. MemoryMarshal.Cast<float, byte>([frame.animatorRotation.y]),
            .. MemoryMarshal.Cast<float, byte>([frame.animatorRotation.z]),
            .. MemoryMarshal.Cast<float, byte>([frame.animatorRotation.w]),
        ];
        Flags flags = new()
        {
            isSwimming = frame.isSwimming,
            isGliding = frame.isGliding,
            isClimbing = frame.isClimbing,
            isRunning = frame.isRunning,
            isSliding = frame.isSliding,
            eventFlags = frame.eventFlags,
        };
        return flags.Serialize() + Convert.ToBase64String(data);
    }
#endif

    private static PlayerReplayFrame DeserializeFrame(string data)
    {
        var flags = Flags.Deserialize(data[0..2]);
        var bytes = Convert.FromBase64String(data[2..]);
        var index = MemoryMarshal.Cast<byte, int>(bytes[0..4])[0];
        var floats = MemoryMarshal.Cast<byte, float>(bytes[(4 * 1)..(4 * 16)]);
        var time = floats[0];
        Vector3 position = new(floats[1], floats[2], floats[3]);
        Vector3 velocity = new(floats[4], floats[5], floats[6]);
        Quaternion rotation = new(floats[7], floats[8], floats[9], floats[10]);
        Quaternion animatorRotation = new(floats[11], floats[12], floats[13], floats[14]);
        return new()
        {
            index = index,
            time = time,
            position = position,
            velocity = velocity,
            rotation = rotation,
            animatorRotation = animatorRotation,
            isSwimming = flags.isSwimming,
            isGliding = flags.isGliding,
            isClimbing = flags.isClimbing,
            isRunning = flags.isRunning,
            isSliding = flags.isSliding,
            eventFlags = flags.eventFlags,
        };
    }
    private struct Flags
    {
        internal bool isSwimming;
        internal bool isGliding;
        internal bool isClimbing;
        internal bool isRunning;
        internal bool isSliding;
        internal PlayerReplayFrame.Event eventFlags;
        private const int maskSw = 0b10000000;
        private const int maskGl = 0b01000000;
        private const int maskCl = 0b00100000;
        private const int maskRu = 0b00010000;
        private const int maskSl = 0b00001000;
        private const int maskEv = 0b00000011;

#if DEBUG && DEBUG_ACTIVATE_SERIALIZER
        internal readonly string Serialize()
        {
            int ev = (int)eventFlags;
            if (ev < 0 || ev > 2) ev = 0;
            int val = (
                (isSwimming ? maskSw : 0) |
                (isGliding ? maskGl : 0) |
                (isClimbing ? maskCl : 0) |
                (isRunning ? maskRu : 0) |
                (isSliding ? maskSl : 0) |
                ev
            );
            var s = val.ToString("x2");
            if (s.Length != 2)
            {
                Monitor.Log($"INVALID FLAGS LENGTH: s: {s}", LL.Warning, true);
            }
            return s;
        }
#endif
        internal static Flags Deserialize(string flags)
        {
            var val = int.Parse(flags, NumberStyles.HexNumber);
            return new()
            {
                isSwimming = (val & maskSw) != 0,
                isGliding = (val & maskGl) != 0,
                isClimbing = (val & maskCl) != 0,
                isRunning = (val & maskRu) != 0,
                isSliding = (val & maskSl) != 0,
                eventFlags = (PlayerReplayFrame.Event)(val & maskEv),
            };
        }
    }
}

