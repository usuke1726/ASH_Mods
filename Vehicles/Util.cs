
using UnityEngine;

namespace Vehicles;

internal class Util
{
    private static GameObject? interactablePrefab = null;
    public static void AddInteractable(GameObject parent, Vector3 localPosition, Vector3 size)
    {
        if (interactablePrefab == null)
        {
            interactablePrefab = GameObject.Find("Motorboat")?.GetComponent<Motorboat>()?.interactableTrigger?.gameObject;
        }
        if (interactablePrefab == null)
        {
            Monitor.Log($"interactablePrefab is null!", LL.Warning, onlyMonitor: true);
            return;
        }
        var interactable = interactablePrefab.Clone().GetComponent<FlexibleTriggerInteractable>();
        interactable.transform.SetParent(parent.transform);
        interactable.lookAt = parent.transform;
        interactable.transform.localPosition = localPosition;
        interactable.GetComponent<BoxCollider>().size = size;
        interactable.GetComponent<InteractableComponentTransfer>().transferTo = parent;
    }
}

