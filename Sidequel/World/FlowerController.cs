
using System.Reflection;
using ModdingAPI;
using UnityEngine;

namespace Sidequel.World;

internal class FlowerController
{
    internal static void Setup(IModHelper helper)
    {
        helper.Events.Gameloop.GameStarted += (_, _) =>
        {
            if (!State.IsActive) return;
            var activate = typeof(Sapling).GetMethod("ActivateFlower", BindingFlags.NonPublic | BindingFlags.Instance);
            foreach (var flower in GameObject.FindObjectsOfType<Sapling>())
            {
                activate.Invoke(flower, []);
            }
        };
    }
}

