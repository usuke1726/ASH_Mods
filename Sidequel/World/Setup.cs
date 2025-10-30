
using ModdingAPI;

namespace Sidequel.World;

internal class Setup
{
    private static bool done = false;
    public Setup(IModHelper helper)
    {
        if (done) return;
        done = true;
        Objects.Setup(helper);
        PlayerPosition.Setup(helper);
        ChestController.Setup(helper);
        FlowerController.Setup(helper);
    }
}

