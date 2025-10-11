
using ModdingAPI;
using UnityEngine;

namespace Sidequel.World;

internal class Objects
{
    internal static void Setup(IModHelper helper)
    {
        helper.Events.Gameloop.GameStarted += (_, _) =>
        {
            SetupObjects();
        };
    }
    private static void SetupObjects()
    {
        if (!State.IsActive) return;
        var NPCs = GameObject.Find("NPCs").transform;
        NPCs.Find("ToughBirdNPC (1)").gameObject.SetActive(false);
    }
}

