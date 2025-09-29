
using UnityEngine;

namespace CustomConversation;

using Actions;
using Cinemachine;
using ModdingAPI;

public class PositionBase()
{
    public virtual void Place(CharacterObject obj) { }
}

public class Position3(Vector3 position, Vector3? rotation = null) : PositionBase()
{
    public Vector3 position = position;
    public Vector3? rotation = rotation;
    public override void Place(CharacterObject obj)
    {
        obj.gameObject.transform.position = position;
        if (rotation != null) obj.gameObject.transform.rotation = Quaternion.Euler((Vector3)rotation);
    }
}
public class Position2(Vector2 position, Vector3? rotation = null) : PositionBase()
{
    public Vector2 position = position;
    public Vector3? rotation = rotation;
    public override void Place(CharacterObject obj)
    {
        var defaultLayer = obj.parent.layer;
        obj.parent.layer = 2; // ignore raycast
        var mask =
            (1 << 10) | // Ground
            (1 << 12); // SoftGround
        if (Physics.Raycast(new(position.x, 10000f, position.y), Vector3.down, out var hitinfo, 30000f, mask))
        {
            Monitor.Log($"PLACE ({hitinfo.point.x}, {hitinfo.point.y}, {hitinfo.point.z})", LL.Warning, onlyMonitor: true);
            Monitor.Log($"hit {hitinfo.collider.name} {hitinfo.colliderInstanceID} {hitinfo.distance}");
            obj.parent.transform.position = hitinfo.point + Vector3.up * Info.Get(obj.character).offsetY;
        }
        obj.parent.layer = defaultLayer;
        if (rotation != null) obj.parent.transform.rotation = Quaternion.Euler((Vector3)rotation);
    }
}

public class ConversationDataSet
{
    public Dictionary<Characters, PositionBase> InitialPositions = [];
    public List<ActionCore> Actions = [];
    public string ConversationId = null!;
    public Vector3? CameraPosition = null;
    public Vector3? CameraRotation = null;
}

public class DummyLookatPoint : MonoBehaviour
{
    public GameObject camera { get; private set; } = null!;
    public GameObject player { get; private set; } = null!;
    private void Awake()
    {
        camera = Context.levelController.cinemaCamera;
        player = Context.player.gameObject;
    }
}

public abstract class ConversationData : ConversationDataCore
{
    public override bool TransitionStart => true;
    public override bool TransitionEnd => true;
    protected abstract ConversationDataSet DataSet { get; }
    private ConversationDataSet data = null!;

    private Dictionary<Characters, List<ActionCore>> actions = [];
    private Dictionary<Characters, CharacterObject> characterObjects = [];
    private HashSet<CharacterObject> characters = [];
    public override bool Finished => !characterObjects.Any();
    private float startTime = -1;
    private float time = -1;
    private Transform? prevCamLookAt = null;
    private Quaternion? prevCamRotation = null;
    public override void SetupWithinTransition()
    {
        data ??= DataSet;
        var cam = Context.levelController.cinemaCamera;
        cam.SetActive(true);
        Context.levelUI.HideUI(true);
        if (!data.InitialPositions.Any()) return;
        if (!CharacterObject.TryToGet(Characters.Claire, out var player)) return;
        if (data.InitialPositions.TryGetValue(Characters.Claire, out var pos))
        {
            pos.Place(player);
            //Camera.main.transform.position = player.gameObject.transform.position;
            player.OnPlaced();
            var camPos = data.CameraPosition;
            if (camPos != null)
            {
                var camera = cam.GetComponent<CinemachineVirtualCamera>();
                prevCamLookAt = camera.LookAt;
                var tr = new GameObject("dummyLookAtPoint").transform;
                tr.gameObject.AddComponent<DummyLookatPoint>();
                tr.position = (Vector3)camPos;
                camera.LookAt = tr;
                camera.Follow = tr;
            }
            var camRot = data.CameraRotation;
            if (camRot != null)
            {
                prevCamRotation = cam.transform.rotation;
                cam.transform.rotation = Quaternion.Euler((Vector3)camRot);
            }
        }
        foreach (var ch in data.InitialPositions.Keys.ToList())
        {
            if (ch == Characters.Claire) continue;
            if (CharacterObject.TryToGet(ch, out CharacterObject obj))
            {
                data.InitialPositions[ch].Place(obj);
                obj.ClearEmotion();
                obj.ClearPose();
                obj.OnPlaced();
            }
        }
    }
    public override void Cleanup()
    {
        foreach (var ch in characters)
        {
            ch.CleanUp();
        }
    }
    public override void CleanupWithinTransition()
    {
        var cam = Context.levelController.cinemaCamera;
        var camera = cam.GetComponent<CinemachineVirtualCamera>();
        if (prevCamLookAt != null)
        {
            var tr = camera.LookAt;
            camera.Follow = prevCamLookAt;
            camera.LookAt = prevCamLookAt;
            prevCamLookAt = null;
            GameObject.Destroy(tr.gameObject);
        }
        if (prevCamRotation != null)
        {
            cam.transform.rotation = (Quaternion)prevCamRotation;
            prevCamRotation = null;
        }
        cam.SetActive(false);
        Context.levelUI.HideUI(false);
        foreach (var ch in characters)
        {
            ch.CleanUp();
        }
    }
    public override void Setup()
    {
        data ??= DataSet;
        if (data == null) throw new Exception("data is null!!");
        foreach (var action in data.Actions)
        {
            var ch = action.character;
            if (!characterObjects.ContainsKey(ch))
            {
                if (!CharacterObject.TryToGet(ch, out var obj))
                {
                    throw new Exception($"Not fuond character object!! {ch}");
                }
                characterObjects[ch] = obj;
                characters.Add(obj);
            }
            if (!actions.ContainsKey(action.character)) actions[action.character] = [];
            actions[action.character].Add(action);
        }
        foreach (Characters ch in actions.Keys)
        {
            actions[ch].Sort();
        }
    }
    public override void Update()
    {
        if (startTime < 0) startTime = Time.time;
        time = Time.time - startTime;
        foreach (var obj in characterObjects.Values.ToList())
        {
            var ch = obj.character;
            var chActions = actions[ch];
            while (true)
            {
                if (!chActions.Any())
                {
                    characterObjects.Remove(ch);
                    break;
                }
                if (time >= chActions[0].time)
                {
                    chActions[0].Invoke(obj, characters);
                    if (chActions[0] is EndAction)
                    {
                        characterObjects.Remove(ch);
                        break;
                    }
                    else
                    {
                        chActions.RemoveAt(0);
                    }
                }
                else break;
            }
            actions[ch] = chActions;
        }
    }
}

