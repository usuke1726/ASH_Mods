
using Cinemachine;
using ModdingAPI;
using QuickUnityTools.Audio;
using QuickUnityTools.Input;
using UnityEngine;

namespace Vehicles;

internal class Ship : Vehicle
{
    private static bool setupDone = false;
    public AudioSource? honkSource = null;
    private static AudioClip? honkSound = null!;
    private static void Reset()
    {
        setupDone = false;
        honkSound = null;
    }
    public static void Setup(IModHelper helper)
    {
        helper.Events.Gameloop.PlayerUpdated += (s, e) => SetupShip();
        helper.Events.Gameloop.GameQuitting += (s, e) => Reset();
    }
    private static GameObject cameraTarget = null!; // Prevent screen shake when the ship tilts
    private static CinemachineVirtualCamera camera = null!;
    private static float cameraTargetHeight = 10f;
    private static void SetupShip()
    {
        if (setupDone) return;
        var ship = GameObject.Find("Structures")?.transform?.Find("Boat")?.gameObject;
        if (ship == null) return;
        setupDone = true;
        var motorboat = GameObject.Find("Motorboat")?.GetComponent<Motorboat>();
        if (motorboat != null)
        {
            honkSound = motorboat.honkSound;
        }
        var body = ship.GetComponent<Rigidbody>();
        body.mass = 600.0f;
        var _ship = ship.gameObject.AddComponent<Ship>();
        Util.AddInteractable(ship, new(-5.5555f, 0f, -19.0429f), new(3, 5, 4));
        _ship.input ??= ship.gameObject.AddComponent<GameUserInput>();
        _ship.input.enabled = false;
        _ship.mountPosition = ship.transform;
        cameraTarget = new GameObject("CameraTargetShip");
        cameraTarget.transform.position = _ship.transform.position.SetY(0) + Vector3.up * cameraTargetHeight;
        camera = GameObject.Find("BoatStuff").transform.Find("MotorboatCamera").gameObject.GetComponent<CinemachineVirtualCamera>();
        _ship.vehicleCamera = camera.gameObject;
    }


    private BoxCollider[] colliders = null!;
    private Action poseDisable = null!;
    private Action? resetPlayerPos = null;
    private Transform defaultLookAt = null!;
    private Transform defaultFollow = null!;
    private Transform defaultParent = null!;
    private bool flagForWaitButtonUp = false;
    private Projector playerShadow = null!;
    protected override void Enter()
    {
        base.Enter();
        defaultLookAt = camera.LookAt;
        defaultFollow = camera.Follow;
        camera.LookAt = cameraTarget.transform;
        camera.Follow = cameraTarget.transform;
        camera.gameObject.transform.position = transform.position;
        defaultParent = camera.transform.parent;
        camera.transform.parent = cameraTarget.transform;
        body.constraints = RigidbodyConstraints.None;
        player.DropOrStashHeldItem();
        playerShadow ??= player.transform.Find("Shadow").GetComponent<Projector>();
        playerShadow.enabled = false;
        body.useGravity = false;
        flagForWaitButtonUp = true;
        var offset = GetComponent<WaterOffset>();
        offset.enabled = false;
        offset.enabled = true;
        colliders ??= GetComponentsInChildren<BoxCollider>();
        foreach (var collider in colliders)
        {
            Physics.IgnoreCollision(player.myCollider, collider, true);
        }
        player.transform.localPosition = new(-10, 0, -10);
        poseDisable = player.ikAnimator.Pose(Pose.LookAround);
        for (int i = 0; i < colliders.Length - 1; i++)
        {
            for (int j = i + 1; j < colliders.Length; j++)
            {
                Physics.IgnoreCollision(colliders[i], colliders[j], true);
            }
        }
    }
    protected override void Exit()
    {
        base.Exit();
        body.useGravity = true;
        var pos = body.transform.position + Vector3.up * 15;
        resetPlayerPos = () =>
        {
            player.transform.position = (player.body.position = pos);
            player.body.velocity = Vector3.zero;
            playerShadow.enabled = true;
            playerShadow.gameObject.transform.position = Context.player.transform.position;
            Timer timer = null!;
            timer = Timer.Register(2.0f, () =>
            {
                camera.LookAt = defaultLookAt;
                camera.Follow = defaultFollow;
                camera.transform.parent = defaultParent;
                Timer.FlagToRecycle(timer);
            });
        };
        body.constraints = RigidbodyConstraints.FreezeAll;
        poseDisable();
        poseDisable = null!;
        foreach (var collider in colliders)
        {
            Physics.IgnoreCollision(player.myCollider, collider, false);
        }
    }
    protected override void Update()
    {
        if (resetPlayerPos != null)
        {
            resetPlayerPos();
            resetPlayerPos = null;
        }
        base.Update();
        if (mounted)
        {
            if (input.button4.ConsumePress() || input.menuButton.wasPressed)
            {
                Exit();
            }
            UpdateHonkable();
            player.transform.localPosition = new(0f, 0, -8);
            player.transform.forward = -body.transform.forward;
        }
    }
    private float speed = 0f;
    private void FixedUpdate()
    {
        if (!mounted) return;
        body.transform.forward = body.transform.forward.SetY(body.transform.forward.y * 0.9f);
        body.transform.position = Vector3.Lerp(body.transform.position, body.transform.position.SetY(16), 0.1f);

        var leftStickActive = input.leftStick.vector.sqrMagnitude > 0.5f;
        var button1Pressed = input.button1.isPressed;
        if (flagForWaitButtonUp)
        {
            if (button1Pressed) button1Pressed = false;
            else flagForWaitButtonUp = false;
        }
        var buttonPressed = button1Pressed || input.button2.isPressed;
        var targetSpeed = leftStickActive && buttonPressed ? 40f : 20f;
        if (input.button2.isPressed) targetSpeed = -targetSpeed;
        speed = Mathf.Lerp(speed, targetSpeed, 0.01f);
        if (leftStickActive)
        {
            var inputvec = new Vector3(input.leftStick.vector.x, 0, input.leftStick.vector.y);
            var vec = -1 * Camera.main.transform.TransformDirection(inputvec).SetY(0);
            body.transform.forward = Vector3.MoveTowards(body.transform.forward, vec.normalized, 0.01f);
        }
        if (leftStickActive || buttonPressed)
        {
            var targetVel = -body.transform.forward * speed;
            body.velocity = targetVel;
        }
        else
        {
            body.angularVelocity = body.angularVelocity * 0.99f;
            body.velocity = body.velocity * 0.99f;
            speed = Mathf.Lerp(speed, 0f, 0.1f);
        }
        cameraTarget.transform.position = transform.position.SetY(0) + Vector3.up * cameraTargetHeight;
    }

    private NPCIKAnimator[] nearbyNPCs = new NPCIKAnimator[5];
    private void UpdateHonkable()
    {
        if (honkSource != null && (bool)honkSource)
        {
            if (!honkSource.isPlaying) { honkSource = null; }
        }
        if (input.button3.wasPressed && honkSound != null && !(honkSource != null && (bool)honkSource))
        {
            var player = Singleton<SoundPlayer>.instance;
            honkSource = player.PlayLooped(honkSound, player.transform.position);
            honkSource.volume = 1.0f;
            honkSource.loop = false;
            //honkSource.pitch = 0.35f; // note A
            honkSource.pitch = 0.424f; // note C

            //var chorusFilter = honkSource.gameObject.GetComponent<AudioChorusFilter>() ?? honkSource.gameObject.AddComponent<AudioChorusFilter>();
            //chorusFilter.depth = 0.3f;
            //chorusFilter.rate = 20.0f;
            //chorusFilter.wetMix2 = 0.5f;
            //chorusFilter.wetMix1 = 0.5f;

            var reverb = honkSource.gameObject.GetComponent<AudioReverbFilter>() ?? honkSource.gameObject.AddComponent<AudioReverbFilter>();
            reverb.reflectionsLevel = 0;
            //reverb.reflectionsLevel = -10000;
            reverb.decayTime = 0.1f;

            var echoFilter = honkSource.gameObject.GetComponent<AudioEchoFilter>() ?? honkSource.gameObject.AddComponent<AudioEchoFilter>();
            echoFilter.wetMix = 0.1f;

            var lowPass = honkSource.gameObject.GetComponent<AudioLowPassFilter>() ?? honkSource.gameObject.AddComponent<AudioLowPassFilter>();
            lowPass.cutoffFrequency = 300f;

            var distort = honkSource.gameObject.GetComponent<AudioDistortionFilter>() ?? honkSource.gameObject.AddComponent<AudioDistortionFilter>();
            distort.distortionLevel = 0.7f;

            //var highPass = honkSource.gameObject.GetComponent<AudioHighPassFilter>() ?? honkSource.gameObject.AddComponent<AudioHighPassFilter>();
            //highPass.cutoffFrequency = 500f;

            int num = NPCIKAnimator.FindNearby(transform.position, 100f, nearbyNPCs);
            for (int i = 0; i < num; i++)
            {
                var animator = nearbyNPCs[i];
                var canFace = animator.GetComponentInChildren<ICanFace>();
                if (canFace != null) canFace.TurnToFace(transform);
                var canLook = animator.GetComponent<ICanLook>();
                if (canLook != null) canLook.lookAt = transform;
                var componentInParent = animator.GetComponentInParent<NPCMovement>();
                if ((bool)componentInParent)
                {
                    componentInParent.PauseAndFace(transform, 3.5f);
                }
                Timer timer = null!;
                timer = Timer.Register(3.25f, delegate
                {
                    if (canFace != null) canFace.FaceDefault();
                    if (canLook != null) canLook.lookAt = null;
                    Timer.FlagToRecycle(timer);
                });
            }
        }
    }
}

