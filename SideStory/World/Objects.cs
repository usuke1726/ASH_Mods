
using ModdingAPI;
using UnityEngine;

namespace SideStory.World;

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
        var NPCs = GameObject.Find("NPCs").transform;
        NPCs.Find("ToughBirdNPC (1)").gameObject.SetActive(false);
    }
}

