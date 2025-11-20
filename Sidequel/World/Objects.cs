
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
        NPCs.Find("YellAtNode").GetComponent<RangedInteractable>().enabled = false;
        foreach (Transform child in NPCs.Find("ArtistQuest"))
        {
            child.gameObject.SetActive(child.name == "Artist1");
        }
        new GameObject("Sidequel_ClimbersRemover").AddComponent<ClimbersRemover>();
        RemoveItems();
        ModdingAPI.Character.OnSetupDone(() =>
        {
            var nephew = ModdingAPI.Character.Get(Characters.RunningNephew).gameObject;
            var jon = ModdingAPI.Character.Get(Characters.RangerJon).gameObject;
            GameObject.Destroy(nephew.GetComponent<PathNPCMovement>());
            nephew.transform.position = new(239.9925f, 48.3835f, 60.3349f);
            nephew.transform.localRotation = Quaternion.Euler(0, 214.6103f, 0);
            nephew.GetComponentInChildren<Animator>().runtimeAnimatorController = jon.GetComponentInChildren<Animator>().runtimeAnimatorController;
            new GameObject("Sidequel_PictureGuyWatcher").AddComponent<PictureGuyWatcher>();
        });
    }
    private static void RemoveItems()
    {
        foreach (var item in GameObject.FindObjectsOfType<CollectOnTouch>())
        {
            // remove coins and feathers
            item.gameObject.SetActive(false);
        }
        foreach (var crack in GameObject.FindObjectsOfType<BuriedCollectable>())
        {
            crack.gameObject.SetActive(false);
        }
        foreach (var crack in GameObject.FindObjectsOfType<BuriedChest>())
        {
            crack.gameObject.SetActive(false);
        }
        string[] removedItemPrefixes = [
            "ShellPickup",
            "ToyShovelPickup",
            "TrashPickup",
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
    private class PictureGuyWatcher : MonoBehaviour
    {
        private GameObject atFoot = null!;
        private GameObject atTop = null!;
        private void Awake()
        {
            atFoot = ModdingAPI.Character.Get(Characters.PictureFox1).gameObject;
            atTop = ModdingAPI.Character.Get(Characters.PictureFox2).gameObject;
        }
        private void Update()
        {
            if (!atFoot.activeSelf) atFoot.SetActive(true);
            if (atTop.activeSelf) atTop.SetActive(false);
        }
    }
}

