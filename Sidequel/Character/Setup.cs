
using ModdingAPI;

namespace Sidequel.Character;

internal class Setup
{
    private static bool done = false;
    public Setup(IModHelper helper)
    {
        if (done) return;
        done = true;
        Core.Setup(helper);
        Pose.Setup(helper);
    }
}

