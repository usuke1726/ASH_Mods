
namespace ModdingAPI;

using UnityEngine;
using BInput = UnityEngine.Input;

public class Input
{
    public static bool GetKey(KeyCode key)
    {
        if (!InputInterceptor.isInPrefix && InputInterceptor.IsIntercepted(key)) return false;
        return BInput.GetKey(key);
    }
    public static bool GetKeyDown(KeyCode key)
    {
        if (!InputInterceptor.isInPrefix && InputInterceptor.IsIntercepted(key)) return false;
        return BInput.GetKeyDown(key);
    }
    public static bool GetKeyUp(KeyCode key)
    {
        if (!InputInterceptor.isInPrefix && InputInterceptor.IsIntercepted(key)) return false;
        return BInput.GetKeyUp(key);
    }
}

