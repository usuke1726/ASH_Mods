
using HarmonyLib;
using ModdingAPI;
using ModdingAPI.KeyBind;
using UnityEngine;

namespace Misc;

[HarmonyPatch(typeof(Player))]
internal class InfinityStamina
{
    [HarmonyPrefix()]
    [HarmonyPatch("UseFeather")]
    public static bool UseFeather() => !ModConfig.config.EnableInfinityStamina;

    [HarmonyPrefix()]
    [HarmonyPatch("DrainFeatherStamina")]
    public static bool DrainFeatherStamina(float amount) => !ModConfig.config.EnableInfinityStamina;
}

internal static class SuperJump
{
    internal static void Setup(IModHelper helper)
    {
        KeyBind.RegisterKeyBind(
            helper.KeyBindingsData,
            [KeybindKey.SuperJump_Keyboard, KeybindKey.SuperJump_Pad],
            () => Jump()
            , name: "superJump");
    }
    internal static void Jump()
    {
        if (!ModConfig.config.EnableSuperJump) return;
        if (!Context.TryToGetPlayer(out var player)) return;
        if (!Context.CanPlayerMove) return;
        float num = 140f;
        var tr = Traverse.Create(player);
        var flag = (player.groundHit.HasValue || tr.Field("justLeftGround").GetValue<bool>() || tr.Field("isSwimming").GetValue<bool>()) && !(tr.Field("jumpOnCooldown").GetValue<bool>());
        player.body.velocity = player.body.velocity.SetY(Mathf.Max(player.body.velocity.y, num));
        var effects = tr.Field("effects").GetValue<PlayerEffects>();
        if (flag)
        {
            effects.Jump();
            var act = tr.Field("onGroundJumped").GetValue<Action>();
            act?.Invoke();
        }
        else
        {
            effects.FlapWings(Mathf.Clamp(1f + 0.1f * 1, 0.5f, 4f));
            tr.Field("glideStartCountdown").SetValue(player.holdGlideButtonTime);
            player.DropOrStashHeldItem();
            var act = tr.Field("onWingsFlapped").GetValue<Action>();
            act?.Invoke();
        }
    }
}

internal static class TurboClaire
{
    private static IModHelper helper = null!;
    internal static void Setup(IModHelper _helper)
    {
        helper = _helper;
    }
    internal static void OnEnabledChanged()
    {
        helper.Events.Gameloop.PlayerUpdated -= Update;
        if (ModConfig.config.EnableTurbo) helper.Events.Gameloop.PlayerUpdated += Update;
    }

    private static float? defaultMaxSpeed = null;
    private static float? currentMaxSpeed = null;
    private static Animator? animator = null;
    private static int? id = null;
    private static RaceController? raceController = null;
    private static ScriptedMusic? music = null;
    private static int count = -1;
    private static readonly float maxSpeed1 = 150;
    private static readonly float maxSpeed2 = 1000;
    private static void Update(object sender, EventArgs arg)
    {
        var player = Context.player;
        animator ??= Traverse.Create(player.ikAnimator).Field("animator").GetValue<Animator>();
        id ??= Animator.StringToHash("ArmSpeed");
        defaultMaxSpeed ??= player.maxSpeed;
        currentMaxSpeed ??= defaultMaxSpeed;
        if (player.input.hasFocus && player.input.IsRunHeld())
        {
            if (UnityEngine.Input.GetKey(KeyCode.P) || UnityEngine.Input.GetKey(KeyCode.JoystickButton1))
            {
                currentMaxSpeed = Mathf.Lerp((float)currentMaxSpeed, maxSpeed2, 0.005f);
                animator.speed = Mathf.Pow(Mathf.Min(maxSpeed1, (float)currentMaxSpeed) / (float)defaultMaxSpeed, 1.0f);
            }
            else
            {
                currentMaxSpeed = Mathf.Lerp((float)currentMaxSpeed, maxSpeed1, 0.1f);
                animator.speed = Mathf.Pow((float)currentMaxSpeed / (float)defaultMaxSpeed, 1.0f);
            }
            player.maxSpeed = (float)currentMaxSpeed;
        }
        else
        {
            currentMaxSpeed = Mathf.Lerp((float)currentMaxSpeed, (float)defaultMaxSpeed, 0.2f);
            player.maxSpeed = (float)currentMaxSpeed;
            animator.speed = 1.0f;
        }

        if (UnityEngine.Input.GetKey(KeyCode.X) || UnityEngine.Input.GetKey(KeyCode.JoystickButton1))
        {
            if (player.body.transform.position.y > 30f)
            {
                player.body.AddForce(Vector3.down * 4000, ForceMode.Acceleration);
            }
        }
    }
}

