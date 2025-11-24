
using ModdingAPI;

namespace Sidequel.Item;

internal class Setup
{
    private static bool done = false;
    public Setup(IModHelper helper)
    {
        if (done) return;
        done = true;
        DataHandler.Setup(helper);
        SpecialFeather.Setup(helper);
    }
}

