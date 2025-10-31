
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
        NPCs.Find("PolarBearNPC").gameObject.SetActive(false);
        NPCs.Find("CampfireFriends").gameObject.SetActive(true);
        new GameObject("Sidequel_ClimbersRemover").AddComponent<ClimbersRemover>();
        RemoveItems();
    }
    private static void RemoveItems()
    {
        foreach (var item in GameObject.FindObjectsOfType<CollectOnTouch>())
        {
            // remove coins and feathers
            item.gameObject.SetActive(false);
        }
        string[] removedItemPrefixes = [
            "ShellPickup",
            "ToyShovelPickup",
        ];
        var items = GameObject.FindObjectsOfType<CollectOnInteract>()
            .Where(i => removedItemPrefixes.Any(p => i.name.StartsWith(p)));
        foreach (var item in items)
        {
            item.gameObject.SetActive(false);
        }
        var shovel = GameObject.Find("Shovel");
        if (shovel != null) shovel.SetActive(false);
    }
    private class ClimbersRemover : MonoBehaviour
    {
        private GameObject tim = null!;
        private GameObject alex = null!;
        private GameObject above = null!;
        private void Awake()
        {
            var cutscenes = GameObject.Find("Cutscenes").transform;
            tim = cutscenes.Find("RockClimberHangout/ClimbSquirrel").gameObject;
            alex = cutscenes.Find("RockClimberHangout/GroundRhinoNPC").gameObject;
            above = cutscenes.Find("RockClimbersAbove").gameObject;
        }
        private void Update()
        {
            if (tim != null && tim.activeSelf) tim.SetActive(false);
            if (alex != null && alex.activeSelf) alex.SetActive(false);
            if (above != null && above.activeSelf) above.SetActive(false);
            GameObject.Destroy(gameObject);
        }
    }
}

