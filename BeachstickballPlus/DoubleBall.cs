
namespace BeachstickballPlus;

using System.Collections;
using System.Reflection;
using HarmonyLib;
using ModdingAPI;
using UnityEngine;

internal static class DoubleVolleyball
{
    public static bool Enabled { get => ModEntry.config.DoubleBall; }
    public static bool EnabledWhackNoise { get => ModEntry.config.DoubleBall && ModEntry.config.EnemyWhackNoise; }

    private static readonly Volleyball?[] balls = [null, null];
    private static readonly Volleyball?[] orphanedBalls = [null, null];
    private static readonly float hitTimerTime = 0.25f;
    private static readonly float[] hitTimer = [-1, -1];
    private static readonly float[] groundedTimer = [-1, -1];

    private static bool secondBallSpawned = false;
    private static bool setupDone = false;

    private static int hits = 0;
    private static Action<Holdable> action = null!;
    internal static Volleyball? whackedBall = null;

    internal static void OnDisabled()
    {
        for (int i = 0; i <= 1; i++)
        {
            if (orphanedBalls[i] != null)
            {
                orphanedBalls[i]?.Kill();
                orphanedBalls[i] = null;
            }
        }
    }

    internal static void StartGame(VolleyballGameController __instance, ref IInteractable ___interactable, ref Player ___player, ref int ___hits)
    {
        if (__instance.gameStarted) return;
        __instance.hitGroundSafeTime = 0.25f;
        hitTimer[0] = -1; hitTimer[1] = -1;
        groundedTimer[0] = -1; groundedTimer[1] = -1;
        setupDone = false;
        whackedBall = null;
        secondBallSpawned = false;
        __instance.GetType().GetProperty("gameStarted").SetValue(__instance, true);
        ___interactable.enabled = false;
        action ??= GetOnPlayerHoldableUsed(__instance);
        ___player.onHoldableUsed += action;
        hits = 0;
        Context.globalData.gameData.tags.SetFloat(__instance.hitsTag, 0f);
        SpawnBall(__instance, 0);
        PassBallToEnemy(balls[0]!, __instance.opponent.transform.position + Random.insideUnitSphere, __instance.serveTime, __instance);
        ___player.ikAnimator.lookAt = balls[0]!.transform;
        ___player.walkFacingTarget = balls[0]!.transform;
        __instance.referee.lookAt = balls[0]!.transform;
        __instance.refereeInteractable.enabled = false;
    }

    internal static void SetI18nMessages()
    {
        messagesOnFailed = I18n_.Localize("TalksOnFailed")
            .Split("\n")
            .Select(s => s.Split(";", 2))
            .Where(s => s.Length == 2)
            .ToArray();
        if (messagesOnFailed.Length == 0) messagesOnFailed = [["", ""]];
    }
    private static string[][] messagesOnFailed = null!;

    internal static bool EndGame(bool popped, VolleyballGameController __instance, ref Player ___player)
    {
        if (!setupDone) return false;
        if (__instance.gameStarted)
        {
            SetHitCountResult(__instance, hits);
            for (int i = 0; i <= 1; i++)
            {
                if (balls[i] != null)
                {
                    balls[i]?.Orphan();
                    orphanedBalls[i] = balls[i];
                }
                //balls[i] = null;
            }
            ___player.onHoldableUsed -= action;
            Traverse.Create(__instance).Field("hits").SetValue(hits);
            hits = 0;
        }
        return true;
    }
    internal static void SetHitCountResult(VolleyballGameController __instance, int hits)
    {
        if (ModEntry.config.SpecialDialogue && hits <= 15)
        {
            var mes = messagesOnFailed[Random.Range(0, messagesOnFailed.Length)];
            var playerMes = mes[0].Trim();
            var enemyMes = mes[1].Trim();
            if (playerMes.Length > 0) TemporalBox.Add(playerMes, 60);
            if (hits > 0)
            {
                var hitMes = I18n_.Localize("RefereeCounts", hits);
                TemporalBox.Add(__instance.referee.transform, hitMes, 60, Random.Range(30, 55));
            }
            if (enemyMes.Length > 0)
            {
                TemporalBox.Add(__instance.opponent.transform, enemyMes, 60, Random.Range(25, 40));
            }
            Context.globalData.gameData.tags.SetFloat(__instance.hitsTag, 0);
        }
        else if (Enabled)
        {
            Context.globalData.gameData.tags.SetFloat(__instance.hitsTag, hits / 2.0f);
        }
    }

    private static Action<Holdable> GetOnPlayerHoldableUsed(VolleyballGameController __instance) => (heldObject) =>
    {
        var fl = Traverse.Create(__instance).Field("whackingActions");
        var whackingActions = fl.GetValue<WhackingActions>();
        if ((bool)whackingActions)
        {
            whackingActions.ForceNextWhack(null);
        }
        fl.SetValue(heldObject.GetComponent<WhackingActions>());
        whackingActions = fl.GetValue<WhackingActions>();
        if ((bool)whackingActions && __instance.playerShouldCatch)
        {
            var player = Traverse.Create(__instance).Field("player").GetValue<Player>();
            foreach (var ball in balls)
            {
                if (ball == null) continue;
                Vector3 position = ball.transform.position;
                position += ball.body.velocity * __instance.armAnimationTime;
                if (IsInsideEllipse(position, player.transform.position, Camera.main.transform.right * __instance.autoAimRadiusCamX, __instance.autoAimRadiusCamZ, __instance.autoAimBallHeight))
                {
                    player.TurnToFace(ball.transform);
                    whackingActions.ForceNextWhack(ball.GetComponent<IWhackable>());
                }
            }
        }
    };

    private static IEnumerator GroundCorountine(VolleyballGameController __instance)
    {
        while (true)
        {
            yield return new WaitForFixedUpdate();
            if (!__instance.gameStarted) break;
            for (int i = 0; i <= 1; i++)
            {
                if (hitTimer[i] > 0 && Time.time - hitTimer[i] >= hitTimerTime) hitTimer[i] = -1;
                if (groundedTimer[i] > 0 && Time.time - groundedTimer[i] >= __instance.hitGroundSafeTime)
                {
                    Monitor.Log($"Now game ending...", LL.Debug);
                    __instance.GetType().GetMethod("EndGame", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(__instance, new object[] { false });
                }
            }
        }
    }

    private static bool IsInsideEllipse(Vector3 point, Vector3 center, Vector3 axisX, float axisYLength, float heightLength)
    {
        Vector3 val = point - center;
        val = Quaternion.AngleAxis(Vector3.SignedAngle(axisX, Vector3.right, Vector3.up), Vector3.up) * val;
        float sqrMagnitude = axisX.sqrMagnitude;
        return val.x.Sqr() / sqrMagnitude + val.z.Sqr() / axisYLength.Sqr() + val.y.Sqr() / heightLength.Sqr() < 1f;
    }

    private static void PassBallToPlayer(VolleyballGameController __instance, Volleyball ball)
    {
        if (GetBallIdx(ball, out var idx))
        {
            groundedTimer[idx] = -1;
            hitTimer[idx] = Time.time;
        }
        var player = Traverse.Create(__instance).Field("player").GetValue<Player>();
        Vector3 destination;
        float time;
        if (hits < __instance.simpleHits || VolleyballGameController.EASY_MODE)
        {
            Vector3 position = player.transform.position;
            Vector3 val = ball.transform.position - player.transform.position;
            destination = position + val.normalized;
            time = __instance.enemyPassTimeSimple.Random();
        }
        else if (hits < __instance.simpleHits + __instance.easyHits)
        {
            destination = __instance.playerAimRegionEasy.RandomWithin();
            time = __instance.enemyPassTimeEasy.Random();
        }
        else if (hits < __instance.simpleHits + __instance.easyHits + __instance.mediumHits)
        {
            destination = __instance.playerAimRegionMedium.RandomWithin();
            time = __instance.enemyPassTimeMedium.Random();
        }
        else
        {
            destination = __instance.playerAimRegionHard.RandomWithin();
            time = __instance.enemyPassTimeHard.Random();
        }
        ball.body.velocity = CalculateBallVelocity(ball.transform.position, destination, time);
        ball.body.AddTorque(Random.insideUnitSphere * 720f);
        //___enemyHitTime = Time.time;
        Traverse.Create(__instance).Field("enemyHitTime").SetValue(Time.time);
        __instance.GetType().GetProperty("playerShouldCatch").SetValue(__instance, true);
        if (!secondBallSpawned)
        {
            secondBallSpawned = true;
            secondSpawnDone = false;
            //ballTurnIdx = (ballTurnIdx + 1) % 2;
            spawnCR = SpawnCorountine(__instance);
            __instance.StartCoroutine(spawnCR);
        }
        if (secondSpawnDone)
        {
            __instance.StopCoroutine(spawnCR);
            __instance.StartCoroutine(GroundCorountine(__instance));
        }
    }
    private static IEnumerator spawnCR = null!;
    private static bool secondSpawnDone = false;
    private static IEnumerator SpawnCorountine(VolleyballGameController __instance)
    {
        for (int i = 0; i < 10; i++)
        {
            yield return new WaitForFixedUpdate();
        }
        if (!secondSpawnDone)
        {
            SpawnBall(__instance, 1);
            PassBallToEnemy(balls[1]!, __instance.opponent.transform.position + Random.insideUnitSphere, __instance.serveTime, __instance);
            secondSpawnDone = true;
            __instance.StopCoroutine(spawnCR);
        }
    }

    internal static void OnBallHitsGround(VolleyballGameController __instance, Volleyball ball)
    {
        var pos = ball.transform.position;
        if (GetBallIdx(ball, out var idx) && hitTimer[idx] <= 0) groundedTimer[idx] = Time.time;
    }
    private static bool GetBallIdx(Volleyball ball, out int idx)
    {
        idx = -1;
        if (balls[1] != null && balls[1]?.GetHashCode() == ball.GetHashCode()) idx = 1;
        else if (balls[0] != null && balls[0]?.GetHashCode() == ball.GetHashCode()) idx = 0;
        if (idx == -1) Monitor.Log($"idx not found ({balls[0]?.GetHashCode()}, {balls[1]?.GetHashCode()}) target hash: {ball.GetHashCode()}", LL.Error);
        return idx >= 0;
    }

    internal static void OnBallWhackedByPlayer(VolleyballGameController __instance, ref Timer ___hitGroundTimer)
    {
        Timer.Cancel(___hitGroundTimer);
        ___hitGroundTimer = null!;
        if (__instance.playerShouldCatch && __instance.gameStarted)
        {
            if (whackedBall == null)
            {
                Monitor.Log($"whackedBall is null!!", LL.Warning);
                return;
            }
            hits++;
            PassBallToEnemy(__instance, whackedBall);
        }
    }

    private static void PassBallToEnemy(Volleyball ball, Vector3 aimPos, float time, VolleyballGameController __instance)
    {
        if (GetBallIdx(ball, out var idx))
        {
            groundedTimer[idx] = -1;
            hitTimer[idx] = Time.time;
        }
        ball.body.AddTorque(Random.insideUnitSphere * 720f);
        __instance.StartCoroutine(PassCorountine(__instance, aimPos, time, ball));
        Vector3 val = aimPos - ball.transform.position;
        __instance.opponent.walkTo = aimPos + val.normalized * __instance.enemyBackUpUnits;
    }
    private static void PassBallToEnemy(VolleyballGameController __instance, Volleyball ball)
    {
        float num = __instance.playerPassTime.Random();
        var player = Traverse.Create(__instance).Field("player").GetValue<Player>();
        if (!player.isGrounded)
        {
            num *= __instance.spikeMultiplier;
        }
        Vector3 aimPos = __instance.enemyAimRegion.RandomWithin();
        PassBallToEnemy(ball, aimPos, num, __instance);
    }

    private static IEnumerator PassCorountine(VolleyballGameController __instance, Vector3 destination, float time, Volleyball ball)
    {
        Vector3 start = ball.transform.position;
        Vector3 startVelocity = CalculateBallVelocity(start, destination, time);
        float startTime = Time.fixedTime;
        bool armAnimation = false;
        while (true)
        {
            float num = Mathf.Clamp(Time.fixedTime - startTime, 0f, time);
            Vector3 position = start + startVelocity * num + 0.5f * Physics.gravity * num * num;
            ball.transform.position = position;
            if (Time.fixedTime > startTime + time)
            {
                break;
            }
            if (!armAnimation && startTime + time - Time.fixedTime < __instance.armAnimationTime)
            {
                armAnimation = true;
                __instance.opponent.SwingArms();
            }
            yield return new WaitForFixedUpdate();
        }
        PassBallToPlayer(__instance, ball);
        if (secondSpawnDone) setupDone = true;
    }
    private static Vector3 CalculateBallVelocity(Vector3 start, Vector3 destination, float time)
    {
        Vector3 val = destination - start;
        return val / time - Physics.gravity * time / 2f;
    }

    private static void SpawnBall(VolleyballGameController __instance, int idx)
    {
        var player = Traverse.Create(__instance).Field("player").GetValue<Player>();
        if (balls[idx] != null && (bool)balls[idx])
        {
            balls[idx]?.Orphan();
            orphanedBalls[idx] = balls[idx];
            balls[idx] = null;
        }
        if (orphanedBalls[idx] != null && (bool)orphanedBalls[idx])
        {
            orphanedBalls[idx]?.Kill();
            orphanedBalls[idx] = null;
        }
        Vector3 val = (player.transform.position - __instance.opponent.transform.position).SetY(0);
        Vector3 val2 = val.normalized * __instance.enemyBackUpUnits;
        GameObject val3 = __instance.ballPrefab.CloneAt(__instance.opponent.transform.position + val2);
        Volleyball ball = val3.GetComponent<Volleyball>();
        ball.controller = __instance;
        balls[idx] = ball;
        Physics.IgnoreCollision(ball.GetComponent<Collider>(), __instance.opponent.GetComponent<Collider>());
        if (idx == 1)
        {
            Physics.IgnoreCollision(ball.GetComponent<Collider>(), balls[0]!.GetComponent<Collider>());
            Physics.IgnoreCollision(balls[0]!.GetComponent<Collider>(), ball.GetComponent<Collider>());
        }
    }

    internal static bool GetBall(ref Volleyball __result)
    {
        if (Environment.StackTrace.Contains("SwingArms"))
        {
            __result = balls[0]!;
            return false;
        }
        return true;
    }
}

