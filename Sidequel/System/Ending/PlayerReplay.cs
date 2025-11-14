
using UnityEngine;

namespace Sidequel.System.Ending;

internal class EndingPlayerReplay : MonoBehaviour
{
    private Player player = null!;
    private PlayerEffects effects = null!;
    private Quaternion originalAnimatorRotation;
    private PlayerIKAnimator animator = null!;
    private Vector3 lastPosition = default;
    internal float time;

    internal bool isSwimming;
    internal bool isGliding;
    internal bool isClimbing;
    internal bool isRunning;
    internal bool isSliding;
    private Vector3 _velocity;
    private Vector3 angularVelocity;
    public PlayerReplayData data = null!;
    public Vector3? walkTo { get; set; }
    private PlayerReplayFrame startFrame;
    private int lastFrame;
    internal bool IsPlaying { get; private set; }
    internal bool IsPausing { get; private set; }
    private void Awake()
    {
        player = gameObject.GetComponent<Player>();
        effects = gameObject.GetComponent<PlayerEffects>();
        animator = player.ikAnimator;
    }
    private void Update()
    {
        player.transform.position = Vector3.SmoothDamp(player.transform.position, transform.position, ref _velocity, 1f);
        player.transform.rotation = SmoothDampQuaternion(player.transform.rotation, transform.rotation, ref angularVelocity, 1f);
    }
    private static Quaternion SmoothDampQuaternion(Quaternion current, Quaternion target, ref Vector3 currentVelocity, float smoothTime)
    {
        Vector3 eulerAngles = current.eulerAngles;
        Vector3 eulerAngles2 = target.eulerAngles;
        return Quaternion.Euler(
            Mathf.SmoothDampAngle(eulerAngles.x, eulerAngles2.x, ref currentVelocity.x, smoothTime),
            Mathf.SmoothDampAngle(eulerAngles.y, eulerAngles2.y, ref currentVelocity.y, smoothTime),
            Mathf.SmoothDampAngle(eulerAngles.z, eulerAngles2.z, ref currentVelocity.z, smoothTime)
        );
    }
    private void LateUpdate()
    {
        if (data == null || data.frames.Count == 0 || !IsPlaying) return;
        PlayerReplayFrame frameData;
        if (time >= 0f)
        {
            frameData = data.GetFrame(time, lastFrame);
            if (frameData.index > lastFrame)
            {
                HandleEvents(data.frames[lastFrame].eventFlags);
                lastFrame = frameData.index;
                if (lastFrame < 0) throw new Exception($"LateUpdate: lastFrame is negative!! (lastFrame: {lastFrame})");
                if (pauseFrames.Contains(lastFrame))
                {
                    // Debug($"auto-pausing (frame index: {lastFrame})");
                    Pause();
                }
            }
        }
        else
        {
            frameData = PlayerReplayFrame.Lerp(startFrame, data.frames[0], 1f - time / startFrame.time);
            lastFrame = 0;
        }
        SetFrameData(frameData);
        time += Time.deltaTime;
        if (time > data.lastFrame.time) Stop();
        lastPosition = transform.position;
    }
    public void Play(float startTime = 0f)
    {
        IsPlaying = true;
        time = startTime;
        player.body.isKinematic = false;
        startFrame = PlayerReplayFrame.FromTransform(transform, startTime, -1);
        walkTo = null;
    }
    private HashSet<int> pauseFrames = [];
    public void SetPauseFrames(IEnumerable<int> frames)
    {
        pauseFrames = [.. frames];
    }
    public void Pause()
    {
        IsPausing = true;
        IsPlaying = false;
    }
    public void Continue()
    {
        if (!IsPausing) return;
        IsPausing = false;
        IsPlaying = true;
    }
    public void Stop()
    {
        IsPlaying = false;
        player.body.isKinematic = false;
        animator.transform.localRotation = originalAnimatorRotation;
        _velocity = Vector3.zero;
        isSwimming = false;
        isGliding = false;
        isClimbing = false;
        isRunning = false;
        isSliding = false;
    }
    private void SetFrameData(PlayerReplayFrame frame)
    {
        transform.position = frame.position;
        transform.rotation = frame.rotation;
        animator.transform.localRotation = frame.animatorRotation;
        _velocity = frame.velocity;
        player.body.velocity = _velocity;
        isSwimming = frame.isSwimming;
        isGliding = frame.isGliding;
        isClimbing = frame.isClimbing;
        isRunning = frame.isRunning;
        isSliding = frame.isSliding;
    }
    private void HandleEvents(PlayerReplayFrame.Event eventFlags)
    {
        if (eventFlags.HasFlag(PlayerReplayFrame.Event.FlapWings))
        {
            effects.FlapWings();
        }
        if (eventFlags.HasFlag(PlayerReplayFrame.Event.Jump))
        {
            effects.Jump();
        }
    }
    public void CleanRecordsFromData(int count)
    {
        data.frames.RemoveRange(0, count);
        lastFrame -= count;
        if (lastFrame < 0) throw new Exception($"CleanRecordsFromData: lastFrame is negative!! (lastFrame: {lastFrame})");
        for (int i = 0; i < data.frames.Count; i++)
        {
            var value = data.frames[i];
            value.index = i;
            data.frames[i] = value;
        }
    }
}

