
using ModdingAPI;

namespace Sidequel.System;

internal class Setup
{
    private static bool done = false;
    public Setup(IModHelper helper)
    {
        if (done) return;
        done = true;
        SaveData.Setup(helper);
        STags.Setup(helper);
        EndGameController.Setup(helper);
    }
}

