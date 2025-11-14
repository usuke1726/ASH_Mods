
using Cinemachine;
using UnityEngine;

namespace Sidequel.System.Ending;

internal class CutsceneController : MonoBehaviour
{
    private static CutsceneController instance = null!;
    internal static Transform camera = null!;
    internal const string CutsceneName = "Sidequel_EndingCutscene";
    private static Transform player = null!;
    private static Transform lookAt = null!;
    private static CinemachineVirtualCamera vCam = null!;
    internal static void PrepareCutscene(Transform _player)
    {
        player = _player;
        lookAt = new GameObject($"{CutsceneName}_Lookat").transform;
        lookAt.transform.position = player.position;
        instance = new GameObject($"{CutsceneName}Controller").AddComponent<CutsceneController>();
    }
    private void Awake()
    {
        var cutscenes = GameObject.Find("/Cutscenes").transform;
        camera = cutscenes.transform.Find("PeakCutscene/SnowTipTopPos");
        camera.position = new(197.1342f, 328.4546f, 330.7987f);
        var cam = camera.transform.Find("SitCutsceneCam");
        vCam = cam.GetComponentInChildren<CinemachineVirtualCamera>();
        vCam.LookAt = lookAt;
    }
    private static bool isFlyingCutscene = false;
    private static readonly Vector3 moveOnFlying = new Vector3(-1, 5, -1).normalized;
    private static readonly Vector3 moveLookAtOnFlying = new Vector3(2, -2, 4).normalized;
    internal static void SetFlying()
    {
        isFlyingCutscene = true;
    }
    private int countdown = 60;
    private void FixedUpdate()
    {
        if (isFlyingCutscene)
        {
            if (countdown > 0)
            {
                var scale = (0.1f / (countdown * countdown));
                camera.position += moveOnFlying * scale;
                lookAt.position += moveLookAtOnFlying * scale;
                countdown--;
            }
            else
            {
                camera.position += moveOnFlying * 0.1f;
                lookAt.position += moveLookAtOnFlying * 0.1f;
            }
        }
        else if (spot != null && isReady)
        {
#if DEBUG
            if (canMove) return;
#endif
            spot.Update();
            camera.position = spot.Position;
            camera.localRotation = Quaternion.Euler(spot.Rotation);
        }
    }
    internal static bool canMove = false;
    internal static void Next(float time) => instance._Next(time);
    private static Spot spot = null!;
    private static bool isReady = false;
    internal void _Next(float time)
    {
        isFlyingCutscene = false;
        vCam.LookAt = null!;
        spot = Spot.Next();
        isReady = false;
        spot.Reset(time);
    }
    internal static void Ready() => isReady = true;
}

