
using ModdingAPI;
using UnityEngine;

namespace Sidequel.Character;

internal static class Pose
{
    private static readonly Dictionary<Poses, RuntimeAnimatorController> controllers = [];
    private static bool setupDone = false;
    internal static void Setup(IModHelper helper)
    {
        helper.Events.Gameloop.GameStarted += (_, _) =>
        {
            if (!State.IsActive) return;
            ModdingAPI.Character.OnSetupDone(() =>
            {
                controllers[Poses.Sitting] = GetController(Characters.AuntMay);
                controllers[Poses.Walking] = GetController(Characters.HydrationDog);
                controllers[Poses.Running] = GetController(Characters.RunningGoat);
                controllers[Poses.Standing] = GetController(Characters.DadBoatDeer2);
                controllers[Poses.ReallyAnxious] = GetController(Characters.Camper);
                setupDone = true;
                onSetupDone?.Invoke();
                onSetupDone = null!;
            });
        };
        helper.Events.Gameloop.ReturnedToTitle += (_, _) =>
        {
            setupDone = false;
        };
    }
    private static event Action onSetupDone = null!;
    private static void OnSetupDone(Action action)
    {
        if (setupDone) action.Invoke();
        else onSetupDone += action;
    }
    internal static Animator GetAnimator(Transform tr) => tr.GetComponentInChildren<Animator>();
    internal static RuntimeAnimatorController GetController(Characters ch)
    {
        return ModdingAPI.Character.Get(ch).transform.GetComponentInChildren<Animator>().runtimeAnimatorController;
    }
    internal static void Set(Transform target, Poses pose) => OnSetupDone(() =>
    {
        var animator = GetAnimator(target);
        if (animator == null) return;
        animator.runtimeAnimatorController = controllers[pose];
    });
    internal static void Set(Characters target, Poses pose) => Set(ModdingAPI.Character.Get(target).transform, pose);
    internal static void Set(Characters target, Characters from)
    {
        ModdingAPI.Character.OnSetupDone(() => Set(
            ModdingAPI.Character.Get(target).transform,
            ModdingAPI.Character.Get(from).transform
        ));
    }
    internal static void Set(Transform target, Characters from)
    {
        ModdingAPI.Character.OnSetupDone(() => Set(target, ModdingAPI.Character.Get(from).transform));
    }
    internal static void Set(Characters target, Transform from)
    {
        ModdingAPI.Character.OnSetupDone(() => Set(ModdingAPI.Character.Get(target).transform, from));
    }
    internal static void Set(Transform target, Transform from)
    {
        var targetAnimator = GetAnimator(target);
        var baseAnimator = GetAnimator(from);
        if (targetAnimator == null || baseAnimator == null) return;
        targetAnimator.runtimeAnimatorController = baseAnimator.runtimeAnimatorController;
    }
}

