
using ModdingAPI;
using ModdingAPI.KeyBind;
using QuickUnityTools.Input;
using UnityEngine;

namespace Vehicles;

internal class Tractor : Vehicle
{
    private static bool setupDone = false;
    private static readonly float[][][] wheelPos = [
        [
            [0.084f, 1.8189f, 1.7989f],
            [0, 4.0436f, 2.3855f]
        ],
        [
            [0.2f, 1.8771f, 1.9761f],
            [0, 5.0328f, 0]
        ],
        [
            [-0.1f, -1.71f, -2.3254f],
            [0, 5.6725f, 0]
        ],
        [
            [-0.2524f, -1.7412f, -2.2972f],
            [0, 4.8f, 0]
        ]
    ];
    public static void Setup(IModHelper helper)
    {
        helper.Events.Gameloop.PlayerUpdated += (s, e) => SetupTractor();
        helper.Events.Gameloop.GameQuitting += (s, e) => setupDone = false;
        helper.KeyBindingsData.SetDefault(new Dictionary<string, string>() { ["ToggleTractorGravityMode"] = "JoystickButton4" });
        KeyBind.RegisterKeyBind(helper.KeyBindingsData, "ToggleTractorGravityMode", () =>
        {
            usedNonGravity = !usedNonGravity;
            Monitor.Log($"usedNonGravity {(usedNonGravity ? "ON" : "OFF")}", LL.Warning, onlyMonitor: true);
        }, name: "Fly Tractor");
    }
    private static void SetupTractor()
    {
        if (setupDone) return;
        var tractor = GameObject.Find("Tractor");
        if (tractor == null) return;
        setupDone = true;
        List<Transform> wheels = [
            tractor.transform.Find("Wheel1"),
            tractor.transform.Find("Wheel1_001"),
            tractor.transform.Find("Wheels"),
            tractor.transform.Find("Wheels_001"),
        ];
        for (int i = 0; i < 4; i++)
        {
            wheels[i].localPosition = new(wheelPos[i][0][0], wheelPos[i][0][1], wheelPos[i][0][2]);
            wheels[i].localRotation = Quaternion.Euler(wheelPos[i][1][0], wheelPos[i][1][1], wheelPos[i][1][2]);
            wheels[i].localScale = new(1, 1, 1);
        }
        tractor.transform.position = tractor.transform.position.SetY(tractor.transform.position.y + 2f);
        var body = tractor.AddComponent<Rigidbody>();
        body.mass = 5.0f;
        body.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        body.interpolation = RigidbodyInterpolation.Extrapolate;

        Util.AddInteractable(tractor, new(-0.7828f, 0, 1.499f), new(3, 5, 4));
        var vehicle = tractor.gameObject.AddComponent<Tractor>();
        vehicle.input ??= tractor.gameObject.AddComponent<GameUserInput>();
        vehicle.input.enabled = false;
        vehicle.mountPosition = tractor.transform;
    }


    private BoxCollider[] colliders = null!;
    private GameObject playerCharacter = null!;
    private Projector playerShadow = null!;
    private Transform head = null!;
    private Transform tail = null!;
    protected override void Enter()
    {
        colliders ??= GetComponentsInChildren<BoxCollider>();
        base.Enter();
        body.mass = 10;
        player.DropOrStashHeldItem();
        player.ikAnimator.SetBoatSit(true);
        player.transform.localPosition = new(0, 3, 0);
        playerCharacter ??= player.transform.Find("Character").gameObject;
        playerCharacter.SetActive(false);
        playerShadow ??= player.transform.Find("Shadow").GetComponent<Projector>();
        playerShadow.enabled = false;
        head ??= new GameObject("TractorHead").transform;
        tail ??= new GameObject("TractorTail").transform;
        head.parent = transform;
        tail.parent = transform;
        head.position = transform.position;
        tail.position = transform.position;
        head.localPosition = new(0, 0, 2.79f);
        tail.localPosition = new(0, 0, -3f);
        //head.gameObject.AddComponent<LineRenderer>();
        //tail.gameObject.AddComponent<LineRenderer>();
        foreach (var collider in colliders)
        {
            Physics.IgnoreCollision(player.myCollider, collider, true);
            collider.gameObject.layer = 9;
        }
        //for (int i = 0; i < colliders.Length - 1; i++)
        //{
        //    for (int j = i + 1; j < colliders.Length; j++)
        //    {
        //        Physics.IgnoreCollision(colliders[i], colliders[j], true);
        //    }
        //}
    }
    protected override void Exit()
    {
        base.Exit();
        player.ikAnimator.SetBoatSit(false);
        playerCharacter.SetActive(true);
        playerShadow.enabled = true;
        playerShadow.gameObject.transform.position = Context.player.transform.position;
        foreach (var collider in colliders)
        {
            Physics.IgnoreCollision(player.myCollider, collider, false);
        }
    }
    protected override void Update()
    {
        base.Update();
        if (mounted)
        {
            if (input.button4.ConsumePress() || input.menuButton.wasPressed)
            {
                Exit();
            }
            //head.gameObject.GetComponent<LineRenderer>().SetPositions([head.position, head.position + Vector3.down * 2]);
            //tail.gameObject.GetComponent<LineRenderer>().SetPositions([tail.position, tail.position + Vector3.down * 2]);
            player.body.velocity = body.velocity;
            player.body.position = mountPosition.position;
            player.body.transform.forward = body.transform.forward;
            playerShadow.gameObject.transform.position = player.transform.position;
        }
    }
    private float speed = 0;
    private static readonly float speedDiff = 0.01f;
    private float maxSpeed = 0.4f;
    private int countDown = 0;
    internal static bool usedNonGravity = false;
    private float targetAngle = 0;
    private void FixedUpdate()
    {
        if (!mounted) return;
        var usingGravity = false;
        Vector3 targetForward = body.transform.forward;
        //var distance = 6.5f;
        //var distance = 10.5f;
        var distance = 15.5f;
        if (body.transform.position.y < -10) body.transform.position = body.transform.position.SetY(10);

        var leftStickActive = input.leftStick.vector.sqrMagnitude > 0.5f;
        var nextAngle = (input.button1.isPressed, input.button2.isPressed) switch
        {
            (true, true) => 0f,
            (true, false) => 0.5f,
            (false, true) => -0.5f,
            _ => targetAngle
        };
        targetAngle = Mathf.MoveTowards(targetAngle, nextAngle, 0.02f);
        var buttonPressed = input.button1.isPressed || input.button2.isPressed;
        if (usedNonGravity)
        {
            usingGravity = false;
            if (buttonPressed)
            {
                countDown = 10;
            }
            else if (!leftStickActive)
            {
                countDown--;
                if (countDown <= 0)
                {
                    usingGravity = true;
                    usedNonGravity = false;
                    Monitor.Log($"usedNonGravity OFF", LL.Warning, onlyMonitor: true);
                }
            }
        }
        else
        {
            usingGravity = !input.button1.isPressed && !input.button2.isPressed;
        }
        maxSpeed = input.button3.isPressed ? 0.8f : 0.4f;
        body.useGravity = usingGravity;
        if (!usingGravity)
        {
            targetForward = Vector3.MoveTowards(targetForward, body.transform.forward.SetY(targetAngle), 0.1f);
        }
        if (leftStickActive)
        {
            speed = Mathf.Clamp(speed + speedDiff, 0, maxSpeed);
            var vec = new Vector3(input.leftStick.vector.y, 0, -input.leftStick.vector.x).normalized;
            vec = Camera.main.transform.TransformDirection(new(input.leftStick.vector.x, 0, input.leftStick.vector.y));
            targetForward = Vector3.MoveTowards(targetForward, vec.SetY(body.transform.forward.y).normalized, 0.06f);

            var hray = Physics.Raycast(head.position, Vector3.down, out var headInfo, distance, ~(1 << 9));
            var tray = Physics.Raycast(tail.position, Vector3.down, out var tailInfo, distance, ~(1 << 9));
            if (hray && tray && usingGravity)
            {
                body.velocity = body.velocity.SetX(0).SetZ(0);
                var to = (headInfo.point - tailInfo.point).normalized;
                to = to.SetX(targetForward.x).SetZ(targetForward.z);
                targetForward = Vector3.MoveTowards(targetForward, to, 0.1f);
                body.MovePosition(body.transform.position + to * speed);
            }
            else if ((hray || tray) && usingGravity)
            {
                var to = body.transform.forward.normalized;
                body.MovePosition(body.transform.position + to * speed);
            }
            else if (!usingGravity)
            {
                if (body.velocity.sqrMagnitude > 500)
                {
                    body.velocity = Vector3.zero;
                }
                body.velocity = body.velocity.SetY(body.velocity.y * 0.9f);
                body.MovePosition(body.transform.position + body.transform.forward * speed);
            }
            body.transform.forward = Vector3.MoveTowards(body.transform.forward, targetForward, 0.1f);
        }
        else
        {
            if (buttonPressed)
            {
                body.velocity = body.velocity.SetY(body.velocity.y * 0.9f);
            }
            speed = 0;
        }
        var vv = body.velocity.SetY(0);
        if (vv.sqrMagnitude > 500)
        {
            var y = body.velocity.y;
            body.velocity = (vv * 0.9f).SetY(y);
        }
    }
}

